using System;
using System.DirectoryServices;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ADReadableAcl;

public class MyCustomAcl
{
    private readonly static Type _targetType = typeof(NTAccount);

    private readonly ActiveDirectorySecurity _acl = new();

    public string? Path { get; }

    public IdentityReference? Owner { get => field ??= _acl.GetOwner(_targetType); }

    public IdentityReference? Group { get => field ??= _acl.GetGroup(_targetType); }

    public AuthorizationRuleCollection? Access { get => field ??= _acl.GetAccessRules(true, true, _targetType); }

    internal MyCustomAcl(string path, byte[] descriptor)
    {
        Path = path;
        _acl.SetSecurityDescriptorBinaryForm(descriptor, AccessControlSections.All);
    }
}

[Cmdlet(VerbsDiagnostic.Test, "Command")]
public class Class1 : PSCmdlet
{
    private const string SecurityDescriptor = "nTSecurityDescriptor";

    [Parameter(Position = 0)]
    public string? LdapFilter { get; set; }

    protected override void EndProcessing()
    {
        DirectorySearcher searcher = new(LdapFilter);
        searcher.PropertiesToLoad.Add(SecurityDescriptor);
        searcher.SecurityMasks =
            SecurityMasks.Group | SecurityMasks.Dacl |
            SecurityMasks.Owner | SecurityMasks.Sacl;

        foreach (SearchResult obj in searcher.FindAll())
        {
            byte[]? descriptor = obj.Properties[SecurityDescriptor][0] as byte[];
            if (descriptor is not null)
            {
                WriteObject(new MyCustomAcl(obj.Path, descriptor));
            }
        }
    }
}
