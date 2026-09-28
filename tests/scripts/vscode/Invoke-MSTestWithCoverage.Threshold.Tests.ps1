Set-StrictMode -Version Latest

BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $helperScriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1'
    . $helperScriptPath
}

Describe 'Assert-CoberturaLineCoverageThreshold' {
    It 'throws when the Cobertura line-coverage summary is missing' { { Assert-CoberturaLineCoverageThreshold -CoberturaXml '<coverage />' } | Should -Throw 'Cobertura line-rate is missing.' }
    It 'throws when the Cobertura line-coverage summary is non-numeric' { { Assert-CoberturaLineCoverageThreshold -CoberturaXml '<coverage line-rate="invalid" />' } | Should -Throw 'Cobertura line-rate must be numeric.' }
    It 'throws when the Cobertura line coverage is below 80 percent' { { Assert-CoberturaLineCoverageThreshold -CoberturaXml '<coverage line-rate="0.799999" />' } | Should -Throw 'Cobertura line coverage 79.9999% is below the required 80% threshold.' }
    It 'accepts a Cobertura line coverage result at exactly 80 percent' { { Assert-CoberturaLineCoverageThreshold -CoberturaXml '<coverage line-rate="0.8" />' } | Should -Not -Throw }
    It 'accepts a Cobertura line coverage result above 80 percent' { { Assert-CoberturaLineCoverageThreshold -CoberturaXml '<coverage line-rate="0.800001" />' } | Should -Not -Throw }
    It 'rejects a Cobertura line rate outside the closed unit interval' { { Assert-CoberturaLineCoverageThreshold -CoberturaXml '<coverage line-rate="1.5" />' } | Should -Throw 'Cobertura line-rate must be between 0 and 1.' }
}

Describe 'Assert-CoberturaBranchCoverageThreshold' {
    It 'accepts a branch rate at exactly the floor' { { Assert-CoberturaBranchCoverageThreshold -CoberturaXml '<coverage branch-rate="0.75" branches-valid="1200" />' } | Should -Not -Throw }
    It 'accepts a branch rate above the floor' { { Assert-CoberturaBranchCoverageThreshold -CoberturaXml '<coverage branch-rate="0.750001" branches-valid="1200" />' } | Should -Not -Throw }
    It 'rejects a branch rate below the floor' { { Assert-CoberturaBranchCoverageThreshold -CoberturaXml '<coverage branch-rate="0.7499" branches-valid="1200" />' } | Should -Throw '*is below the required 75*' }
    It 'rejects a missing branch rate' { { Assert-CoberturaBranchCoverageThreshold -CoberturaXml '<coverage />' } | Should -Throw 'Cobertura branch-rate is missing.' }
    It 'rejects a non-numeric branch rate' { { Assert-CoberturaBranchCoverageThreshold -CoberturaXml '<coverage branch-rate="invalid" />' } | Should -Throw 'Cobertura branch-rate must be numeric.' }
    It 'rejects a branch rate outside the closed unit interval' { { Assert-CoberturaBranchCoverageThreshold -CoberturaXml '<coverage branch-rate="1.5" branches-valid="1200" />' } | Should -Throw 'Cobertura branch-rate must be between 0 and 1.' }
    It 'rejects a projection with zero valid branches' { { Assert-CoberturaBranchCoverageThreshold -CoberturaXml '<coverage branch-rate="0.8" branches-valid="0" />' } | Should -Throw 'Cobertura branch coverage has no valid branches.' }
}
