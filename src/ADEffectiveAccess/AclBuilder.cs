using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ADEffectiveAccess;

public sealed class AclBuilder : ActiveDirectorySecurity
{
    private readonly static Type _targetType = typeof(NTAccount);

    private readonly string? _path;

    private readonly IdentityReference? _owner;

    private readonly IdentityReference? _group;

    internal AclBuilder(string path, byte[] descriptor) : base()
    {
        SetSecurityDescriptorBinaryForm(descriptor, AccessControlSections.All);
        _path = path;
        _owner = GetOwner(_targetType);
        _group = GetGroup(_targetType);
    }

    internal IEnumerable<EffectiveAccessRule> EnumerateAccessRules(GuidResolver map)
    {
        foreach (ActiveDirectoryAccessRule rule in GetAccessRules(true, true, _targetType))
        {
            yield return new EffectiveAccessRule(rule, _owner, _group, _path)
            {
                ObjectTypeToString = map.Translate(rule.ObjectType, "All Objects (Full Control)"),
                InheritedObjectTypeToString = map.Translate(rule.InheritedObjectType, "Any Inherited Object")
            };
        }
    }

    internal IEnumerable<EffectiveAuditRule> EnumerateAuditRules(GuidResolver map)
    {
        foreach (ActiveDirectoryAuditRule rule in GetAuditRules(true, true, _targetType))
        {
            yield return new EffectiveAuditRule(rule, _owner, _group, _path)
            {
                ObjectTypeToString = map.Translate(rule.ObjectType, "All Objects (Full Control)"),
                InheritedObjectTypeToString = map.Translate(rule.InheritedObjectType, "Any Inherited Object")
            };
        }
    }
}
