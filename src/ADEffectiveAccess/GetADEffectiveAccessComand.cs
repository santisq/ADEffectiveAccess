using System;
using System.DirectoryServices;
using System.Management.Automation;

namespace ADEffectiveAccess;

[Cmdlet(VerbsCommon.Get, "ADEffectiveAccess")]
public sealed class GetADEffectiveAccessComand : PSCmdlet
{
    private const string SecurityDescriptor = "nTSecurityDescriptor";

    [Parameter(Position = 0)]
    public string? LdapFilter { get; set; }

    [Parameter]
    public SwitchParameter Audit { get; set; }

    [Parameter]
    [ValidateRange(0, int.MaxValue)]
    public int Top { get; set; } = 0;

    [Parameter]
    public SwitchParameter IncludeDeletedObjects { get; set; }

    [Parameter]
    public SearchScope SearchScope { get; set; } = SearchScope.Subtree;

    protected override void EndProcessing()
    {
        using DirectorySearcher searcher = new(LdapFilter, [SecurityDescriptor])
        {
            SizeLimit = Top,
            Tombstone = IncludeDeletedObjects,
            SearchScope = SearchScope,
            SecurityMasks = SecurityMasks.Group |
                SecurityMasks.Dacl |
                SecurityMasks.Owner
        };

        if (Audit)
        {
            searcher.SecurityMasks |= SecurityMasks.Sacl;
        }

        foreach (SearchResult obj in searcher.FindAll())
        {
            if (obj.Properties[SecurityDescriptor][0] is not byte[] descriptor)
            {
                ErrorRecord error = new(
                    new InvalidOperationException($"No Security Descriptor found for '{obj.Path}'."),
                    "InvalidSecurityDescriptorType", ErrorCategory.InvalidResult, obj);

                WriteError(error);
                continue;
            }

            AclBuilder builder = new(obj.Path, descriptor);
            WriteObject(builder.EnumerateAccessRules(), enumerateCollection: true);

            if (Audit)
            {
                WriteObject(builder.EnumerateAuditRules(), enumerateCollection: true);
            }
        }
    }
}
