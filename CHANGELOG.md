# CHANGELOG

## [0.0.1] - 2025-10-19

- Rewritten in C# for improved performance and uploaded to the PowerShell Gallery.
- Added enhanced functionality compared to the [original PowerShell version](https://gist.github.com/santisq/a84af707780b1168f1fa390632096a5a), including LDAP search (`-LdapFilter`), audit rules (`-Audit`), deleted object support (`-IncludeDeletedObjects`), and pipeline input from AD cmdlets. See the [Parameters section](./docs/en-US/Get-ADEffectiveAccess.md#parameters) for details.
- Implemented per-session, per-domain caching for GUID translation (`ObjectType` and `InheritedObjectType`), improving efficiency and reducing LDAP queries.
- Enhanced error handling for invalid search bases and identity resolution, ensuring robust validation.
