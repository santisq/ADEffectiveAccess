using System.Security.AccessControl;
using System.Security.Principal;

interface IEffectiveRule
{
    public IdentityReference? Owner { get; }

    public IdentityReference? Group { get; }

    public string? Path { get; }

    public IdentityReference IdentityReference { get; }

    public InheritanceFlags InheritanceFlags { get; }

    public bool IsInherited { get; }

    public PropagationFlags PropagationFlags { get; }
}
