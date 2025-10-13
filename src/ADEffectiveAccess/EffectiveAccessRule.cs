using System;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ADEffectiveAccess;

public sealed class EffectiveAccessRule : EffectiveRule<ActiveDirectoryAccessRule>
{
    public ActiveDirectoryRights ActiveDirectoryRights { get => Rule.ActiveDirectoryRights; }

    public ActiveDirectorySecurityInheritance InheritanceType { get => Rule.InheritanceType; }

    public Guid ObjectType { get => Rule.ObjectType; }

    public Guid InheritedObjectType { get => Rule.InheritedObjectType; }

    public ObjectAceFlags ObjectFlags { get => Rule.ObjectFlags; }

    public AccessControlType AccessControlType { get => Rule.AccessControlType; }

    internal EffectiveAccessRule(
        ActiveDirectoryAccessRule accessRule,
        IdentityReference? owner,
        IdentityReference? group,
        string? path)
        : base(accessRule, owner, group, path)
    {
        Type = RuleType.Access;
    }
}
