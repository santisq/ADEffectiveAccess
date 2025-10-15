using System;
using System.DirectoryServices;
using System.Management.Automation;
using System.Security.Principal;

namespace ADEffectiveAccess;

[Cmdlet(VerbsCommon.Get, "ADEffectiveAccess", DefaultParameterSetName = IdentitySet)]
[OutputType(typeof(EffectiveAccessRule), typeof(EffectiveAuditRule))]
[Alias("gea", "gacl")]
public sealed class GetADEffectiveAccessComand : PSCmdlet, IDisposable
{
    private const string SecurityDescriptor = "nTSecurityDescriptor";

    private const string FilterSet = "Filter";

    private const string IdentitySet = "Identity";

    private DirectoryEntryBuilder? _entryBuilder;

    // [ThreadStatic]
    // private static GuidResolver? _map;

    private static GuidResolver? _map;

    [Parameter(
        Position = 0,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ParameterSetName = IdentitySet)]
    public string Identity { get; set; } = null!;

    [Parameter(Position = 0, ParameterSetName = FilterSet)]
    [ValidateNotNullOrEmpty]
    public string? LdapFilter { get; set; }

    [Parameter]
    public SwitchParameter Audit { get; set; }

    [Parameter(ParameterSetName = FilterSet)]
    [ValidateRange(0, int.MaxValue)]
    public int Top { get; set; } = 0;

    [Parameter]
    public SwitchParameter IncludeDeletedObjects { get; set; }

    [Parameter(ParameterSetName = FilterSet)]
    public SearchScope SearchScope { get; set; } = SearchScope.Subtree;

    [Parameter(ParameterSetName = FilterSet)]
    [ValidateNotNullOrEmpty]
    public string? SearchBase { get; set; }

    [Parameter]
    [Credential]
    public PSCredential? Credential { get; set; }

    [Parameter]
    public string? Server { get; set; }

    [Parameter(ParameterSetName = FilterSet)]
    [ValidateRange(0, int.MaxValue)]
    public int PageSize { get; set; } = 1000;

    [Parameter]
    public AuthenticationTypes AuthenticationTypes { get; set; } = AuthenticationTypes.Secure;

    protected override void BeginProcessing()
    {
        try
        {
            _map ??= GuidResolver.GetFromTLS();
            _entryBuilder = new DirectoryEntryBuilder(Credential, AuthenticationTypes);
            _map.SetCurrentContext(Server, _entryBuilder);
        }
        catch (Exception exception)
        {
            exception.ThrowGuidResolverError(this);
        }
    }

    protected override void ProcessRecord()
    {
        if (_entryBuilder is null) return;
        if (_map is null) return;

        using DirectoryEntry root = GetRootEntry(_entryBuilder);
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
                obj.WriteInvalidSecurityDescriptorError(this);
                continue;
            }

            AclBuilder builder = new(obj.Path, descriptor);
            WriteObject(
                builder.EnumerateAccessRules(_map),
                enumerateCollection: true);

            if (Audit)
            {
                WriteObject(
                    builder.EnumerateAuditRules(_map),
                    enumerateCollection: true);
            }
        }
    }

    private string? TryGetIdentityPath(string? identity) => identity switch
    {
        _ when identity is null => null,
        _ when Guid.TryParse(identity, out Guid guid) => $"<GUID={guid:D}>",
        _ when LanguagePrimitives.TryConvertTo(identity, out SecurityIdentifier sid) => $"<SID={sid}>",
        _ => null
    };

    private DirectoryEntry GetRootEntry(DirectoryEntryBuilder builder)
    {
        const string dn = "distinguishedName";
        if (ParameterSetName == FilterSet)
        {
            return builder.Create(SearchBase);
        }

        string? path = TryGetIdentityPath(Identity);
        if (path is not null)
        {
            return builder.Create(path);
        }

        using DirectorySearcher searcher = new(
            searchRoot: builder.RootEntry,
            filter: $"(|({dn}={Identity})(samAccountName={Identity}))",
            propertiesToLoad: [dn]);

        SearchResult? result = searcher.FindOne()
            ?? throw Identity.ToIdentityNotFoundException(builder.Root);

        return builder.Create(result.Properties[dn][0].ToString());
    }

    public void Dispose()
    {
        _entryBuilder?.Dispose();
        GC.SuppressFinalize(this);
    }
}
