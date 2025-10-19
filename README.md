<h1 align="center">ADEffectiveAccess</h1>

<div align="center">
<sub>AD ACLs with readable rights, flexible LDAP and no AD module needed</sub>
<br /><br />

[![build](https://github.com/santisq/ADEffectiveAccess/actions/workflows/ci.yml/badge.svg)](https://github.com/santisq/ADEffectiveAccess/actions/workflows/ci.yml)
[![PowerShell Gallery](https://img.shields.io/powershellgallery/v/ADEffectiveAccess?label=gallery)](https://www.powershellgallery.com/packages/ADEffectiveAccess)
[![LICENSE](https://img.shields.io/github/license/santisq/ADEffectiveAccess)](https://github.com/santisq/ADEffectiveAccess/blob/main/LICENSE)

</div>

> [!NOTE]
> This module has been rewritten in C# for improved performance and maintainability. The original PowerShell version is available in [this Gist](https://gist.github.com/santisq/a84af707780b1168f1fa390632096a5a).

ADEffectiveAccess is a PowerShell module that provides the `Get-ADEffectiveAccess` cmdlet, an enhanced alternative to `Get-Acl` for Active Directory. This cmdlet retrieves access control lists (ACLs) for AD objects, returning effective access and audit rules. It translates `ObjectType` and `InheritedObjectType` GUIDs into human-readable names using a per-session, per-domain map for improved performance and readability.

Unlike `Get-Acl`, there is no dependency on the Active Directory module and includes built-in LDAP search functionality to locate objects.

## Documentation

Check out [__the documentation__](./docs/en-US/Get-ADEffectiveAccess.md) for cmdlet usage and more examples.

## Installation

### Gallery

The module is available through the [PowerShell Gallery](https://www.powershellgallery.com/packages/ADEffectiveAccess):

```powershell
Install-Module ADEffectiveAccess -Scope CurrentUser
```

### Source

```powershell
git clone 'https://github.com/santisq/ADEffectiveAccess.git'
Set-Location ./ADEffectiveAccess
./build.ps1
```

## Requirements

This module requires __Windows OS__ and is compatible with __Windows PowerShell v5.1__ and [__PowerShell 7+__](https://github.com/PowerShell/PowerShell). No Active Directory module is required. Appropriate permissions are needed to read security descriptors.

## Usage

Below are examples demonstrating how to use `Get-ADEffectiveAccess` to retrieve Active Directory ACLs:

### Get ACL for a specific user by sAMAccountName

Retrieves effective access rules for the user `john.galt` in the current domain.

```powershell
PS /> $acl = Get-ADEffectiveAccess john.galt
PS /> $acl

   Path: LDAP://CN=John Galt,CN=Users,DC=mylab,DC=local

IdentityReference                 ObjectType                      InheritedObjectType  ActiveDirectoryRights
-----------------                 ----------                      -------------------  ---------------------
NT AUTHORITY\SELF                 All Objects (Full Control)      Any Inherited Object GenericRead
NT AUTHORITY\Authenticated Users  All Objects (Full Control)      Any Inherited Object ReadControl
NT AUTHORITY\SYSTEM               All Objects (Full Control)      Any Inherited Object GenericAll
BUILTIN\Account Operators         All Objects (Full Control)      Any Inherited Object GenericAll
mylab\Domain Admins               All Objects (Full Control)      Any Inherited Object GenericAll
Everyone                          User-Change-Password            Any Inherited Object ExtendedRight
NT AUTHORITY\SELF                 Email-Information               Any Inherited Object ReadProperty, WriteProperty
....

PS /> $acl[30] | Format-List

ActiveDirectoryRights       : ReadProperty
InheritanceType             : Descendents
ObjectType                  : 59ba2f42-79a2-11d0-9020-00c04fc2d3cf
InheritedObjectType         : 4828cc14-1437-45bc-9b07-ad6f015e5f28
ObjectFlags                 : ObjectAceTypePresent, InheritedObjectAceTypePresent
AccessControlType           : Allow
Type                        : Access
Owner                       : mylab\Domain Admins
Group                       : mylab\Domain Admins
Path                        : LDAP://CN=John Galt,CN=Users,DC=mylab,DC=local
IdentityReference           : BUILTIN\Pre-Windows 2000 Compatible Access
InheritanceFlags            : ContainerInherit
IsInherited                 : True
PropagationFlags            : InheritOnly
ObjectTypeToString          : General-Information
InheritedObjectTypeToString : inetOrgPerson
```

### Get ACLs for all users in an OU with audit rules

Fetches access and audit rules for all users in the `Users` OU, including SACL rules.

```powershell
PS /> Get-ADEffectiveAccess -LdapFilter "(objectCategory=person)" -SearchBase "OU=Users,DC=mylab,DC=local" -Audit
```

### Pipe AD user object to retrieve ACL

Uses pipeline input from `Get-ADUser` to get effective access rules for the user `jdoe`.

```powershell
PS /> Get-ADUser -Identity "jdoe" | Get-ADEffectiveAccess
```

### Get ACLs for deleted groups with a limit

Retrieves access rules for up to 10 deleted group objects.

```powershell
PS /> Get-ADEffectiveAccess -LdapFilter "(&(isDeleted=TRUE)(objectClass=group))" -IncludeDeletedObjects -Top 10
```

### Query ACLs with specific credentials

Retrieves access rules for a user using specified credentials.

```powershell
PS /> Get-ADEffectiveAccess -Identity "john.galt" -Credential (Get-Credential)
```

## Changelog

- [CHANGELOG.md](./CHANGELOG.md)
- [Releases](https://github.com/santisq/ADEffectiveAccess/releases)

## Contributing

Contributions are welcome, if you wish to contribute, fork this repository and submit a pull request with the changes.
