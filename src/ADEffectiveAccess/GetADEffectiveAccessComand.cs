using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

    private SecurityMasks _masks = SecurityMasks.Group | SecurityMasks.Dacl | SecurityMasks.Owner;

    private DirectoryEntryBuilder? _builder;

    private GuidResolver? _map;

    [Parameter(
        Position = 0,
        Mandatory = true,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        ParameterSetName = IdentitySet)]
    [ADObjectTransform]
    public string? Identity { get; set; }

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
        if (Audit)
        {
            _masks |= SecurityMasks.Sacl;
        }

        try
        {
            _builder = new DirectoryEntryBuilder(
                credential: Credential,
                authenticationTypes: AuthenticationTypes,
                server: Server,
                searchBase: SearchBase);

            _map = GuidResolver.GetFromTLS();
            _map.SetContext(Server, _builder);
        }
        catch (Exception exception)
        {
            GuidResolver.ClearFromTLS();
            exception.ThrowGuidResolverError(this);
        }
    }

    protected override void ProcessRecord()
    {
        Assert(_builder is not null);
        Assert(_map is not null);

        try
        {
            if (Identity is not null)
            {
                GetByIdentity(_builder, Identity);
                return;
            }

            using DirectorySearcher searcher = new(
                searchRoot: _builder.SearchBase,
                filter: LdapFilter,
                propertiesToLoad: [SecurityDescriptor])
            {
                SizeLimit = Top,
                Tombstone = IncludeDeletedObjects,
                SearchScope = SearchScope,
                PageSize = PageSize,
                SecurityMasks = _masks
            };

            foreach (SearchResult result in searcher.FindAll())
            {
                WriteRules(result);
            }
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (IdentityNotMappedException exception)
        {
            exception.WriteIdentityNotFoundError(this);
        }
        catch (Exception exception)
        {
            exception.WriteUnderterminedError(this);
        }
    }

    private void WriteRules(SearchResult searchResult)
    {
        Assert(_map is not null);

        if (!searchResult.TryGetProperty(SecurityDescriptor, out byte[]? descriptor))
        {
            searchResult.WriteInvalidSecurityDescriptorError(this);
            return;
        }

        AclBuilder builder = new(searchResult.Path, descriptor);
        WriteObject(
            builder.EnumerateRules(_map, includeAudit: Audit),
            enumerateCollection: true);
    }

    private void GetByIdentity(DirectoryEntryBuilder builder, string identity)
    {
        string ldapFilter = identity switch
        {
            _ when Guid.TryParse(identity, out Guid guid) => guid.ToFilter(),
            _ when LanguagePrimitives.TryConvertTo(identity, out SecurityIdentifier sid) => sid.ToFilter(),
            _ => identity.ToFilter()
        };

        using DirectorySearcher searcher = new(
            searchRoot: builder.DomainEntry,
            filter: ldapFilter,
            propertiesToLoad: [SecurityDescriptor])
        {
            SecurityMasks = _masks,
            Tombstone = IncludeDeletedObjects
        };

        SearchResult result = searcher.FindOne()
            ?? throw identity.ToIdentityNotFoundException(builder.DomainDistinguishedName);

        WriteRules(result);
    }

    [Conditional("DEBUG")]
    private static void Assert([DoesNotReturnIf(false)] bool condition, string? message = null)
        => Debug.Assert(condition, message);

    public void Dispose()
    {
        _builder?.Dispose();
        GC.SuppressFinalize(this);
    }
}
