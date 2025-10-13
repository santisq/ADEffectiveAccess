using System;
using System.DirectoryServices;
using System.Management.Automation;

namespace ADEffectiveAccess;

[Cmdlet(VerbsCommon.Get, "ADEffectiveAccess")]
[Alias("gea", "gacl")]
public sealed class GetADEffectiveAccessComand : PSCmdlet
{
    private const string SecurityDescriptor = "nTSecurityDescriptor";

    private SchemaMap? _map;

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

    [Parameter]
    [Credential]
    public PSCredential? Credential { get; set; }

    [Parameter]
    public string? Server { get; set; }

    protected override void BeginProcessing()
    {
        try
        {
            _map = new SchemaMap(Server);
        }
        catch (Exception exception)
        {
            ErrorRecord error = new(
                exception, "SchemaMapCreationFailure",
                ErrorCategory.ConnectionError, null);

            ThrowTerminatingError(error);
        }
    }

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
            WriteObject(
                builder.EnumerateAccessRules(_map!),
                enumerateCollection: true);

            if (Audit)
            {
                WriteObject(
                    builder.EnumerateAuditRules(_map!),
                    enumerateCollection: true);
            }
        }
    }
}
