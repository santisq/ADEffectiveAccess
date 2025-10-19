# CHANGELOG

## [0.0.1] - 2025-10-19

- Rewritten in C# for improved performance and uploaded to the PowerShell Gallery.
- Added enhanced functionality compared to the [original PowerShell version](https://gist.github.com/santisq/a84af707780b1168f1fa390632096a5a), including LDAP search (`-LdapFilter`), audit rules (`-Audit`), deleted object support (`-IncludeDeletedObjects`), and pipeline input from AD cmdlets. See the [Parameters section](./docs/en-US/Get-ADEffectiveAccess.md#parameters) for details.
- Implemented per-session, per-domain caching for GUID translation (`ObjectType` and `InheritedObjectType`), improving efficiency and reducing LDAP queries.

    ```powershell
    PS ..\ADEffectiveAccess> Measure-Command { Get-ADEffectiveAccess john.galt }

    Days              : 0
    Hours             : 0
    Minutes           : 0
    Seconds           : 0
    Milliseconds      : 691
    Ticks             : 6916496
    TotalDays         : 8.0052037037037E-06
    TotalHours        : 0.000192124888888889
    TotalMinutes      : 0.0115274933333333
    TotalSeconds      : 0.6916496
    TotalMilliseconds : 691.6496

    PS ..\ADEffectiveAccess> Measure-Command { Get-ADEffectiveAccess john.galt }

    Days              : 0
    Hours             : 0
    Minutes           : 0
    Seconds           : 0
    Milliseconds      : 8
    Ticks             : 85416
    TotalDays         : 9.88611111111111E-08
    TotalHours        : 2.37266666666667E-06
    TotalMinutes      : 0.00014236
    TotalSeconds      : 0.0085416
    TotalMilliseconds : 8.5416

    PS ..\ADEffectiveAccess>
    ```

- Enhanced error handling for invalid search bases and identity resolution, ensuring robust validation.
