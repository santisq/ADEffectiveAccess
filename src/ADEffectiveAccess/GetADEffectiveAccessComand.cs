using System;
using System.DirectoryServices;
using System.Management.Automation;

namespace ADEffectiveAccess;

[Cmdlet(VerbsCommon.Get, "ADEffectiveAccess", DefaultParameterSetName = FilterSet)]
[OutputType(typeof(EffectiveAccessRule), typeof(EffectiveAuditRule))]
[Alias("gea", "gacl")]
public sealed class GetADEffectiveAccessComand : PSCmdlet
{
    private const string SecurityDescriptor = "nTSecurityDescriptor";

    private const string FilterSet = "Filter";

    private const string IdentitySet = "Identity";

    [ThreadStatic]
    private static GuidResolver? _map;

    [Parameter(Position = 0, ParameterSetName = FilterSet)]
    [ValidateNotNullOrEmpty]
    public string? LdapFilter { get; set; }

    [Parameter(Position = 0, Mandatory = true, ParameterSetName = IdentitySet)]
    public string? Identity { get; set; }

    [Parameter]
    public SwitchParameter Audit { get; set; }

    [Parameter(ParameterSetName = FilterSet)]
    [ValidateRange(0, int.MaxValue)]
    public int Top { get; set; } = 0;

    [Parameter]
    public SwitchParameter IncludeDeletedObjects { get; set; }

    [Parameter]
    public SearchScope SearchScope { get; set; } = SearchScope.Subtree;

    [Parameter]
    [ValidateNotNullOrEmpty]
    public string? SearchBase { get; set; } = string.Empty;

    [Parameter]
    [Credential]
    public PSCredential? Credential { get; set; }

    [Parameter]
    public string? Server { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 1000;

    protected override void BeginProcessing()
    {
        try
        {
            _map ??= new GuidResolver();
            _map.SetCurrentContext(Server);
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
        using DirectoryEntry root = new(SearchBase);
        using DirectorySearcher searcher = new(root, LdapFilter, [SecurityDescriptor])
        {
            SizeLimit = Top,
            Tombstone = IncludeDeletedObjects,
            SearchScope = SearchScope,
            PageSize = PageSize,
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
