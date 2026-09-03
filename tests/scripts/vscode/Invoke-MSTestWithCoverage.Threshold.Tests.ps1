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
}
