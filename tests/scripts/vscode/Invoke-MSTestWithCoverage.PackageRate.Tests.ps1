Set-StrictMode -Version Latest

BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $helperScriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1'
    . $helperScriptPath
}

Describe 'Get-CoberturaPackageLineSummary' {
    It 'accumulates line and branch totals across every class in the package' {
        # Issue #733 finding 1: a package rate must be computed from the package's own classes.
        # Class A contributes 2 lines, 1 covered. Class B contributes 2 lines, both covered, one
        # of them a branch line carrying 1 of 2 conditions. Hand-computed package totals are
        # therefore lines 3 of 4 (rate '0.75') and branches 1 of 2 (rate '0.5').
        [xml]$doc = @'
<package name="Ns" line-rate="0" branch-rate="0" complexity="1">
  <classes>
    <class name="Ns.A" filename="Ns\A.cs" line-rate="0" branch-rate="0" complexity="1">
      <lines>
        <line number="10" hits="1" branch="False" />
        <line number="11" hits="0" branch="False" />
      </lines>
    </class>
    <class name="Ns.B" filename="Ns\B.cs" line-rate="0" branch-rate="0" complexity="1">
      <lines>
        <line number="20" hits="1" branch="False" />
        <line number="21" hits="1" branch="True" condition-coverage="50% (1/2)">
          <conditions>
            <condition number="0" type="jump" coverage="50%" />
          </conditions>
        </line>
      </lines>
    </class>
  </classes>
</package>
'@

        $summary = Get-CoberturaPackageLineSummary -PackageNode $doc.SelectSingleNode('//package')

        $summary.LinesValid | Should -Be '4'
        $summary.LinesCovered | Should -Be '3'
        $summary.LineRate | Should -Be '0.75'
        $summary.BranchesValid | Should -Be '2'
        $summary.BranchesCovered | Should -Be '1'
        $summary.BranchRate | Should -Be '0.5'
    }

    It 'falls back to a zero rate when no class in the package carries any lines' {
        # Boundary: a class with neither a <lines> nor a <methods> element is valid input per the
        # Get-CoberturaClassLineSummary contract, so the package denominator is zero. The fallback
        # must be the string '0', matching Get-CoberturaCoverageSummary's existing zero-denominator
        # convention exactly. The fixture's own stale line-rate and branch-rate attributes are
        # deliberately non-zero so a returned '0' cannot come from copying the input.
        [xml]$doc = @'
<package name="Ns" line-rate="0.5" branch-rate="0.25" complexity="1">
  <classes>
    <class name="Ns.A" filename="Ns\A.cs" line-rate="0.5" branch-rate="0.25" complexity="1" />
    <class name="Ns.B" filename="Ns\B.cs" line-rate="0.5" branch-rate="0.25" complexity="1" />
  </classes>
</package>
'@

        $summary = Get-CoberturaPackageLineSummary -PackageNode $doc.SelectSingleNode('//package')

        $summary.LineRate | Should -Be '0'
        $summary.BranchRate | Should -Be '0'
        $summary.LinesValid | Should -Be '0'
        $summary.BranchesValid | Should -Be '0'
    }
}
