using System.Security.AccessControl;
using System.Security.Principal;

namespace ADEffectiveAccess;

public abstract class EffectiveRule<TRule>(
    TRule rule,
    IdentityReference? owner,
    IdentityReference? group,
    string? path)
    : IEffectiveRule where TRule : AuthorizationRule
{
    protected TRule Rule { get; } = rule;

    public RuleType Type { get; protected set; }

    public IdentityReference? Owner { get; } = owner;

    public IdentityReference? Group { get; } = group;

    public string? Path { get; } = path;

    public IdentityReference IdentityReference { get => Rule.IdentityReference; }

    public InheritanceFlags InheritanceFlags { get => Rule.InheritanceFlags; }

    public bool IsInherited { get => Rule.IsInherited; }

    public PropagationFlags PropagationFlags { get => Rule.PropagationFlags; }

    public string? ObjectTypeToString { get; internal set; }

    public string? InheritedObjectTypeToString { get; internal set; }
}
