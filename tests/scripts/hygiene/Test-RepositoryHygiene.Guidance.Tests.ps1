Set-StrictMode -Version Latest

# Pester tests for the human-readable guidance of the repository hygiene guard. Every function
# under test is pure: the input is a set of finding lines and the output is text, so no test
# touches git, the environment or the file system.

BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath '../../../scripts/hygiene/Test-RepositoryHygiene.ps1')
}

Describe 'Get-HygieneRuleGuidance' {
    It 'returns a title, problem and remediation for rule <Rule>' -ForEach @(
        @{ Rule = 'profile-path' }
        @{ Rule = 'raw-document' }
        @{ Rule = 'backup-file' }
        @{ Rule = 'unreadable' }
    ) {
        $guidance = Get-HygieneRuleGuidance -Rule $Rule

        $guidance.Title | Should -Not -BeNullOrEmpty -Because "rule $Rule needs a title"
        $guidance.Problem | Should -Not -BeNullOrEmpty -Because "rule $Rule must explain why it fails"
        @($guidance.Remediation).Count | Should -BeGreaterThan 0 -Because "rule $Rule must say how to fix it"
    }

    It 'names the csproj.user remedy in the profile-path guidance' {
        $guidance = Get-HygieneRuleGuidance -Rule 'profile-path'

        ($guidance.Remediation -join ' ') | Should -Match 'csproj\.user' -Because 'the most common source is a Visual Studio PublishUrl written into a project file'
    }

    It 'throws for an unknown rule so a new rule cannot ship without guidance' {
        { Get-HygieneRuleGuidance -Rule 'no-such-rule' } | Should -Throw '*no-such-rule*' -Because 'every rule must have guidance'
    }
}

Describe 'ConvertTo-HygieneFinding' {
    It 'parses a profile-path line into rule, path and line number' {
        $finding = ConvertTo-HygieneFinding -Line 'HYGIENE profile-path TaskMaster/TaskMaster.csproj:37'

        $finding.Rule | Should -BeExactly 'profile-path' -Because 'the rule name follows the HYGIENE prefix'
        $finding.Path | Should -BeExactly 'TaskMaster/TaskMaster.csproj' -Because 'the line-number suffix is not part of the path'
        $finding.LineNumber | Should -Be 37 -Because 'the suffix carries the line number'
    }

    It 'parses a path-only finding with line number zero' {
        $finding = ConvertTo-HygieneFinding -Line 'HYGIENE backup-file TaskMaster.sln.bak'

        $finding.Path | Should -BeExactly 'TaskMaster.sln.bak' -Because 'a backup-file finding carries only a path'
        $finding.LineNumber | Should -Be 0 -Because 'zero means no line number'
    }

    It 'returns null for <Line>' -ForEach @(
        @{ Line = 'HYGIENE Findings=1' }
        @{ Line = '' }
        @{ Line = 'unrelated output' }
    ) {
        ConvertTo-HygieneFinding -Line $Line | Should -BeNullOrEmpty -Because 'only finding lines are parsed'
    }
}

Describe 'ConvertTo-GitHubAnnotationValue' {
    It 'escapes percent and newlines in a message' {
        ConvertTo-GitHubAnnotationValue -Value "50%`r`nnext" | Should -BeExactly '50%25%0D%0Anext' -Because 'workflow commands are single-line and percent-encoded'
    }

    It 'additionally escapes colon and comma in a property' {
        ConvertTo-GitHubAnnotationValue -Value 'a:b,c' -IsProperty | Should -BeExactly 'a%3Ab%2Cc' -Because 'colon and comma delimit workflow-command properties'
    }
}

Describe 'Get-HygieneFindingGuidance' {
    It 'returns nothing when there are no findings' {
        $output = @(Get-HygieneFindingGuidance -Lines @('HYGIENE Findings=0'))

        $output.Count | Should -Be 0 -Because 'a clean run needs no explanation'
    }

    It 'explains each finding and prints one remediation section per rule' {
        $lines = @(
            'HYGIENE profile-path a/one.csproj:37'
            'HYGIENE profile-path b/two.md:2'
            'HYGIENE backup-file c/old.bak'
            'HYGIENE Findings=3'
        )

        $output = @(Get-HygieneFindingGuidance -Lines $lines)

        $output | Should -Contain 'Repository hygiene guard failed with 3 finding(s):' -Because 'the header states the total'
        $output | Should -Contain '  - a/one.csproj line 37: Absolute user-profile path committed [profile-path]' -Because 'each finding is explained with its file and line'
        $output | Should -Contain '  - c/old.bak: Backup file committed [backup-file]' -Because 'a path-only finding omits the line'
        @($output | Where-Object { $_ -like '`[profile-path`]*' }).Count | Should -Be 1 -Because 'the remediation section appears once per rule'
        @($output | Where-Object { $_ -like '  How to fix:*' }).Count | Should -BeGreaterThan 0 -Because 'remediation steps are printed'
        @($output | Where-Object { $_ -like '::error*' }).Count | Should -Be 0 -Because 'annotations are emitted only on request'
    }

    It 'emits one GitHub annotation per finding when requested' {
        $lines = @('HYGIENE profile-path a/one.csproj:37', 'HYGIENE raw-document d/run.trx', 'HYGIENE Findings=2')

        $output = @(Get-HygieneFindingGuidance -Lines $lines -EmitAnnotations)

        $annotations = @($output | Where-Object { $_ -like '::error *' })
        $annotations.Count | Should -Be 2 -Because 'each finding yields one annotation'
        $annotations[0] | Should -BeLike '::error file=a/one.csproj,line=37,title=Hygiene%3A Absolute user-profile path committed::*' -Because 'the annotation targets the reported file and line'
        $annotations[1] | Should -BeLike '::error file=d/run.trx,title=*' -Because 'a path-only finding has no line property'
    }
}
