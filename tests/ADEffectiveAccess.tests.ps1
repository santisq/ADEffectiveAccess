# ain't nobody got time for that
$ErrorActionPreference = 'Stop'

Describe 'ADEffectiveAccess Module' {
    It 'Should not throw on import' {
        $moduleName = (Get-Item ([IO.Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
        $manifestPath = [IO.Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)

        { Import-Module $manifestPath } | Should -Not -Throw
    }
}
