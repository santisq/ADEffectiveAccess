using System;
using System.DirectoryServices;
using System.Management.Automation;

namespace ADEffectiveAccess;

internal sealed class DirectoryEntryBuilder : IDisposable
{
    private readonly string? _username;

    private readonly string? _password;

    private readonly AuthenticationTypes _authenticationTypes;

    internal DirectoryEntry RootEntry { get; }

    internal DirectoryEntry SearchBase { get; }

    internal string? Root { get; }

    internal DirectoryEntryBuilder(
        PSCredential? credential,
        AuthenticationTypes authenticationTypes,
        string? server,
        string? searchBase)
    {
        _username = credential?.UserName;
        _password = credential?.GetNetworkCredential().Password;
        _authenticationTypes = authenticationTypes;
        RootEntry = Create(server: server);
        SearchBase = Create(searchBase: searchBase);
    }

    internal DirectoryEntry Create(string? server = null, string? searchBase = null)
    {
        string? path = (server, searchBase) switch
        {
            (null, null) => null,
            (not null, null) => $"LDAP://{server}",
            (null, not null) => $"LDAP://{searchBase}",
            _ => $"LDAP://{server}/{searchBase}"
        };

        return path is null
            ? RootEntry
            : new DirectoryEntry(path, _username, _password, _authenticationTypes);
    }

    public void Dispose()
    {
        RootEntry.Dispose();
        SearchBase.Dispose();
        GC.SuppressFinalize(this);
    }
}
