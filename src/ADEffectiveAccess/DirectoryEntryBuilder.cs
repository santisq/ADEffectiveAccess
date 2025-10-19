using System;
using System.DirectoryServices;
using System.Management.Automation;

namespace ADEffectiveAccess;

internal sealed class DirectoryEntryBuilder : IDisposable
{
    private readonly string? _username;

    private readonly string? _password;

    private readonly AuthenticationTypes _authenticationTypes;

    internal DirectoryEntry DomainEntry { get; }

    internal DirectoryEntry SearchBase { get; }

    internal string? DomainDistinguishedName
    {
        get => DomainEntry.Properties["distinguishedName"][0]?.ToString();
    }

    internal DirectoryEntryBuilder(
        PSCredential? credential,
        AuthenticationTypes authenticationTypes,
        string? server,
        string? searchBase)
    {
        _username = credential?.UserName;
        _password = credential?.GetNetworkCredential().Password;
        _authenticationTypes = authenticationTypes;
        DomainEntry = Create(server: server);
        SearchBase = ResolveSearchBase(searchBase);
    }

    internal DirectoryEntry Create(string? server = null, string? searchBase = null)
    {
        string? path = (server, searchBase) switch
        {
            (null, null) => null,
            (not null, null) => server,
            (null, not null) => searchBase,
            _ => $"{server}/{searchBase}"
        };

        if (path is not null && !path.Contains("://"))
            path = $"LDAP://{path}";

        DirectoryEntry entry = new(path, _username, _password, _authenticationTypes);
        _ = entry.NativeObject; // force bind
        return entry;
    }

    private DirectoryEntry ResolveSearchBase(string? searchBase)
    {
        if (searchBase is null) return DomainEntry;

        if (!searchBase.Contains("="))
            throw new ArgumentException(
                $"SearchBase '{searchBase}' is not a valid DistinguishedName. " +
                "It must follow the format 'OU=Name,DC=domain,DC=com' for an Organizational Unit or Container.",
                nameof(searchBase));

        try
        {
            return Create(searchBase: searchBase);
        }
        catch (Exception exception)
        {
            throw new ArgumentException(
                $"SearchBase '{searchBase}' could not be found in '{DomainDistinguishedName}'.",
                nameof(searchBase), innerException: exception);
        }
    }

    public void Dispose()
    {
        DomainEntry.Dispose();
        SearchBase.Dispose();
        GC.SuppressFinalize(this);
    }
}
