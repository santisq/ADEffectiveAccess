using System;
using System.DirectoryServices;
using System.Management.Automation;

namespace ADEffectiveAccess;

internal sealed class DirectoryEntryBuilder : IDisposable
{
    private readonly string? _username;

    private readonly string? _password;

    private readonly AuthenticationTypes _authenticationTypes;

    internal DirectoryEntry RootEntry { get => field ??= Create(); }

    internal string? Root { get => field ??= RootEntry.Properties["distinguishedName"][0]?.ToString(); }

    internal DirectoryEntryBuilder(
        PSCredential? credential,
        AuthenticationTypes authenticationTypes)
    {
        _username = credential?.UserName;
        _password = credential?.GetNetworkCredential().Password;
        _authenticationTypes = authenticationTypes;
    }

    internal DirectoryEntry Create(string? path = null) =>
        new(path is null ? null : $"LDAP://{path}", _username, _password, _authenticationTypes);

    public void Dispose()
    {
        RootEntry.Dispose();
        GC.SuppressFinalize(this);
    }
}
