using System;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ADEffectiveAccess;

public sealed class EffectiveAuditRule : EffectiveRule<ActiveDirectoryAuditRule>
{
    public ActiveDirectoryRights ActiveDirectoryRights { get => Rule.ActiveDirectoryRights; }

    public ActiveDirectorySecurityInheritance InheritanceType { get => Rule.InheritanceType; }

    public Guid ObjectType { get => Rule.ObjectType; }

    public Guid InheritedObjectType { get => Rule.InheritedObjectType; }

    public ObjectAceFlags ObjectFlags { get => Rule.ObjectFlags; }

    public AuditFlags AuditFlags { get => Rule.AuditFlags; }

    internal EffectiveAuditRule(
        ActiveDirectoryAuditRule auditRule,
        IdentityReference? owner,
        IdentityReference? group,
        string? path)
        : base(auditRule, owner, group, path)
    {
        Type = RuleType.Audit;
    }
}
