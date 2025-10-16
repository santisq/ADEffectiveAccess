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
        string? searchBase)
    {
        _username = credential?.UserName;
        _password = credential?.GetNetworkCredential().Password;
        _authenticationTypes = authenticationTypes;
        RootEntry = Create();
        Root = RootEntry.Properties["distinguishedName"][0]?.ToString();
        SearchBase = searchBase is null ? RootEntry : Create(searchBase);
    }

    internal DirectoryEntry Create(string? path = null) =>
        new(path is null ? null : $"LDAP://{path}", _username, _password, _authenticationTypes);

    public void Dispose()
    {
        RootEntry.Dispose();
        SearchBase.Dispose();
        GC.SuppressFinalize(this);
    }
}
