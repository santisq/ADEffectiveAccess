---
external help file: ADEffectiveAccess.dll-Help.xml
Module Name: ADEffectiveAccess
online version: https://github.com/santisq/ADEffectiveAccess/blob/main/docs/en-US/Get-ADEffectiveAccess.md
schema: 2.0.0
---

# Get-ADEffectiveAccess

## SYNOPSIS

Retrieves effective access and audit rules for Active Directory objects, translating `ObjectType` and `InheritedObjectType` GUIDs into human-readable names.

## SYNTAX

### Identity (Default)

```powershell
Get-ADEffectiveAccess
    -Identity <String>
    [-Audit]
    [-IncludeDeletedObjects]
    [-Credential <PSCredential>]
    [-Server <String>]
    [-AuthenticationTypes <AuthenticationTypes>]
    [<CommonParameters>]
```

### Filter

```powershell
Get-ADEffectiveAccess
    [[-LdapFilter] <String>]
    [-Audit]
    [-Top <Int32>]
    [-IncludeDeletedObjects]
    [-SearchScope <SearchScope>]
    [-SearchBase <String>]
    [-Credential <PSCredential>]
    [-Server <String>]
    [-PageSize <Int32>]
    [-AuthenticationTypes <AuthenticationTypes>]
    [<CommonParameters>]
```

## DESCRIPTION

An enhanced alternative to `Get-Acl` for Active Directory, this cmdlet retrieves access control lists (ACLs) for AD objects, returning effective access and audit rules. It translates `ObjectType` and `InheritedObjectType` GUIDs into human-readable names using a per-session, per-domain map for improved performance and readability.

Unlike `Get-Acl`, there is no dependency on the Active Directory module and includes built-in LDAP search functionality to locate objects.

## EXAMPLES

### Example 1

```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -Audit

Use this switch to include audit rules from the System Access Control List (SACL).

> [!NOTE]
>
> Usage of this switch may impact performance in large directories.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Credential

Specifies a user account with permission to perform this action. Default is the current user. Accepts a username (e.g., `User01`, `myDomain\User01`) or a [`PSCredential`](https://learn.microsoft.com/en-us/dotnet/api/system.management.automation.pscredential) object from [`Get-Credential`](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.security/get-credential). Prompts for a password if a username is provided.

```yaml
Type: PSCredential
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeDeletedObjects

Includes deleted objects in the search. Required when retrieving ACLs for deleted objects. See [`DirectorySearcher.Tombstone`](https://learn.microsoft.com/en-us/dotnet/api/system.directoryservices.directorysearcher.tombstone#system-directoryservices-directorysearcher-tombstone) for details.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -LdapFilter

Specifies an LDAP query to filter Active Directory objects (e.g., `(objectClass=user)`).

For more details, see the [__Remarks__ section from `DirectorySearcher.Filter`](https://learn.microsoft.com/en-us/dotnet/api/system.directoryservices.directorysearcher.filter#remarks).

```yaml
Type: String
Parameter Sets: Filter
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SearchScope

Specifies the Active Directory search scope:

- `Base` (`0`): Searches only the current path.
- `OneLevel` (`1`): Searches immediate children.
- `Subtree` (`2`): Searches the current path and all children.

```yaml
Type: SearchScope
Parameter Sets: Filter
Aliases:
Accepted values: Base, OneLevel, Subtree

Required: False
Position: Named
Default value: Subtree
Accept pipeline input: False
Accept wildcard characters: False
```

### -Server

Specifies the AD DS instance to connect to. Accepts:

- Fully qualified domain name
- NetBIOS name
- Directory server name (with optional port, e.g. `myDC01:636`)
- Global Catalog (e.g. `GC://myChildDomain`)

Defaults to the current domain if not specified.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Top

Limits the number of objects to retrieve ACLs for. Default is `0` (no limit, determined by LDAP filter). See [`DirectorySearcher.SizeLimit`](https://learn.microsoft.com/en-us/dotnet/api/system.directoryservices.directorysearcher.sizelimit#system-directoryservices-directorysearcher-sizelimit) for details.

```yaml
Type: Int32
Parameter Sets: Filter
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -AuthenticationTypes

Specifies the authentication method. Default is `Secure`.

> [!TIP]
>
> [`AuthenticationTypes`](https://learn.microsoft.com/en-us/dotnet/api/system.directoryservices.authenticationtypes) is a `Flags` Enum, you can combine values as needed, e.g.: `-AuthenticationTypes 'Secure, FastBind'`.

```yaml
Type: AuthenticationTypes
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: Secure
Accept pipeline input: False
Accept wildcard characters: False
```

### -Identity

Specifies an Active Directory object by:

- A DistinguishedName
- A GUID (`objectGuid`)
- A SID (`objectSid`)
- A sAMAccountName

> [!TIP]
>
> Accepts pipeline input from [ActiveDirectory cmdlets](https://learn.microsoft.com/en-us/powershell/module/activedirectory) with `objectGuid` or `DistinguishedName` properties.

```yaml
Type: String
Parameter Sets: Identity
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -PageSize

Sets the maximum number of objects returned per page in a paged search. Default is `1000`.

See [`DirectorySearcher.PageSize`](https://learn.microsoft.com/en-us/dotnet/api/system.directoryservices.directorysearcher.pagesize) for more details.

```yaml
Type: Int32
Parameter Sets: Filter
Aliases:

Required: False
Position: Named
Default value: 1000
Accept pipeline input: False
Accept wildcard characters: False
```

### -SearchBase

Specifies the `DistinguishedName` of an Organizational Unit or Container as the search base. Defaults to the domain root if not specified.

```yaml
Type: String
Parameter Sets: Filter
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

Accepts a string representing a `DistinguishedName`, `objectGuid`, `objectSid`, or `sAMAccountName` via pipeline for the [`-Identity` parameter](#-identity). You can also pipe objects from Active Directory cmdlets having `DistinguishedName` or `objectGuid` properties.

## OUTPUTS

### ADEffectiveAccess.EffectiveAccessRule

Represents effective access rules with resolved `ObjectType` and `InheritedObjectType` GUIDs.

### ADEffectiveAccess.EffectiveAuditRule

Represents effective audit rules with resolved `ObjectType` and `InheritedObjectType` GUIDs (when `-Audit` is specified).

## NOTES

- This cmdlet maintains a per-session, per-domain map to translate `ObjectType` and `InheritedObjectType` into human-readable names, improving usability and performance.
- Querying audit rules (`-Audit`) or deleted objects (`-IncludeDeletedObjects`) may impact performance on large directories.
- Ensure the account used has sufficient permissions to read security descriptors.

## RELATED LINKS

[__ActiveDirectoryAccessRule__](https://learn.microsoft.com/en-us/dotnet/api/system.directoryservices.activedirectoryaccessrule)

[__ActiveDirectoryAuditRule__](https://learn.microsoft.com/en-us/dotnet/api/system.directoryservices.activedirectoryauditrule)

[__Active Directory Module__](https://learn.microsoft.com/en-us/powershell/module/activedirectory)

[__DirectorySearcher__](https://learn.microsoft.com/en-us/dotnet/api/system.directoryservices.directorysearcher)
