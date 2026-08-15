Set-StrictMode -Version Latest

BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $scriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-VSBuild.ps1'
    . $scriptPath -NoExecute
}

Describe 'ConvertTo-MSBuildPropertyArgument' {
    It 'adds the /p: prefix for bare property assignments' {
        $argument = ConvertTo-MSBuildPropertyArgument -Property 'EnableNETAnalyzers=true'

        $argument | Should -Be '/p:EnableNETAnalyzers=true'
    }

    It 'preserves an existing /p: prefix' {
        $argument = ConvertTo-MSBuildPropertyArgument -Property '/p:TreatWarningsAsErrors=true'

        $argument | Should -Be '/p:TreatWarningsAsErrors=true'
    }
}

Describe 'Get-MSBuildBuildArguments' {
    It 'returns each additional MSBuild property as a separate argument' {
        $arguments = Get-MSBuildBuildArguments `
            -ResolvedSolutionPath 'C:\repo\TaskMaster.sln' `
            -Configuration 'Debug' `
            -Platform 'Any CPU' `
            -MSBuildProperty @(
            'EnableNETAnalyzers=true',
            'EnforceCodeStyleInBuild=true'
        )

        $arguments | Should -Be @(
            'C:\repo\TaskMaster.sln',
            '/t:Build',
            '/p:Configuration=Debug',
            '/p:Platform=Any CPU',
            '/p:EnableNETAnalyzers=true',
            '/p:EnforceCodeStyleInBuild=true',
            '/m'
        )
    }

    It 'emits /t:Rebuild in the target position when -Target Rebuild is supplied' {
        $arguments = Get-MSBuildBuildArguments `
            -ResolvedSolutionPath 'C:\repo\TaskMaster.sln' `
            -Configuration 'Debug' `
            -Platform 'Any CPU' `
            -Target 'Rebuild' `
            -MSBuildProperty @(
            'EnableNETAnalyzers=true',
            'EnforceCodeStyleInBuild=true'
        )

        $arguments | Should -Be @(
            'C:\repo\TaskMaster.sln',
            '/t:Rebuild',
            '/p:Configuration=Debug',
            '/p:Platform=Any CPU',
            '/p:EnableNETAnalyzers=true',
            '/p:EnforceCodeStyleInBuild=true',
            '/m'
        )
    }
}

Describe 'Get-RequestedMSBuildProperties' {
    It 'maps analyzer switches to the expected MSBuild properties' {
        $properties = Get-RequestedMSBuildProperties -EnableNETAnalyzers -EnforceCodeStyleInBuild

        $properties | Should -Be @(
            'EnableNETAnalyzers=true',
            'EnforceCodeStyleInBuild=true'
        )
    }

    It 'emits no MSBuild property for the deprecated -EnableNullable switch' {
        $properties = Get-RequestedMSBuildProperties -EnableNullable -TreatWarningsAsErrors

        $properties | Should -Be @(
            'TreatWarningsAsErrors=true'
        )
    }
}

