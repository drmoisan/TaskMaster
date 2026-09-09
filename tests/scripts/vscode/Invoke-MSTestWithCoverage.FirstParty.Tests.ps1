Set-StrictMode -Version Latest

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    . (Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1')

    # Defined inside BeforeAll rather than at file scope because Pester 5 runs each It block in a
    # child scope of the containing block, so only a function defined here is resolvable from one.
    function Get-DescendantAxisCoverageTally {
        <#
            .SYNOPSIS
            Reproduces the descendant-axis Cobertura tally that issue #815 exists to remove.

            .DESCRIPTION
            A private, test-scoped reproduction of the aggregation snippet pinned in the atomic
            plan for issue 809. It exists so the regression test can assert, over one identical
            fixture, that the delivered function returns strictly smaller totals than the
            defective selection does. It is never called by production code.

            One deliberate change is made to the pinned snippet: its hard-coded nine-name
            first-party literal is replaced by the ProjectNames parameter. The pinned literal does
            not contain the fixture package name, so retaining it would make this helper return
            all zeros over the fixture and the differential assertion would compare zero against
            zero, which would demonstrate nothing. Parameterising it also keeps the production
            assembly names out of this file.

            .PARAMETER XmlDocument
            A Cobertura document.

            .PARAMETER ProjectNames
            The package names to retain. Every caller in this file supplies it explicitly.

            .OUTPUTS
            A pscustomobject carrying LinesCovered, LinesValid, BranchesCovered and BranchesValid
            as integers.
        #>
        [CmdletBinding()]
        [OutputType([pscustomobject])]
        param(
            [Parameter(Mandatory = $true)]
            [xml]$XmlDocument,

            [Parameter(Mandatory = $true)]
            [string[]]$ProjectNames
        )

        $coveredLines = 0
        $validLines = 0
        $coveredBranches = 0
        $validBranches = 0

        foreach ($packageNode in @($XmlDocument.SelectNodes('/coverage/packages/package'))) {
            if ($ProjectNames -notcontains $packageNode.GetAttribute('name')) {
                continue
            }

            foreach ($lineNode in @($packageNode.SelectNodes('.//line'))) {
                $validLines++
                $hits = $lineNode.GetAttribute('hits')
                if ($hits -and [int]$hits -gt 0) {
                    $coveredLines++
                }

                $conditionParts = $lineNode.GetAttribute('condition-coverage')
                if ($conditionParts -and $conditionParts -match '\(([0-9]+)/([0-9]+)\)') {
                    $coveredBranches += [int]$Matches[1]
                    $validBranches += [int]$Matches[2]
                }
            }
        }

        [pscustomobject]@{
            LinesCovered    = $coveredLines
            LinesValid      = $validLines
            BranchesCovered = $coveredBranches
            BranchesValid   = $validBranches
        }
    }

    # The field-initializer-across-constructors shape confirmed at issue #670. Line 20 appears in
    # the class rollup and again in three constructors, line 30 in the rollup and in one method,
    # line 40 only in the rollup, and line 50 only in a method. The multiplicities differ
    # deliberately: a uniformly duplicated fixture leaves every ratio unchanged and so cannot
    # discriminate a correct aggregation from the defective one.
    $script:duplicateRowFixture = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages>
    <package name="Ns" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Ns.Foo" filename="Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods>
            <method name=".ctor" signature="()" line-rate="0" branch-rate="0">
              <lines><line number="20" hits="1" branch="True" condition-coverage="100% (2/2)" /></lines>
            </method>
            <method name=".ctor" signature="(int)" line-rate="0" branch-rate="0">
              <lines><line number="20" hits="1" branch="True" condition-coverage="100% (2/2)" /></lines>
            </method>
            <method name=".ctor" signature="(string)" line-rate="0" branch-rate="0">
              <lines><line number="20" hits="1" branch="True" condition-coverage="100% (2/2)" /></lines>
            </method>
            <method name="M" signature="()" line-rate="0" branch-rate="0">
              <lines>
                <line number="30" hits="0" branch="True" condition-coverage="0% (0/2)" />
                <line number="50" hits="1" branch="True" condition-coverage="50% (1/2)" />
              </lines>
            </method>
          </methods>
          <lines>
            <line number="20" hits="1" branch="True" condition-coverage="100% (2/2)" />
            <line number="30" hits="0" branch="True" condition-coverage="0% (0/2)" />
            <line number="40" hits="1" branch="True" condition-coverage="50% (1/2)" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

    # The same package, plus a second package whose name is outside the supplied allowlist. Its
    # three covered lines must enter neither the numerator nor the denominator.
    $script:allowlistFixture = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages>
    <package name="Ns" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Ns.Foo" filename="Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods>
            <method name=".ctor" signature="()" line-rate="0" branch-rate="0">
              <lines><line number="20" hits="1" branch="True" condition-coverage="100% (2/2)" /></lines>
            </method>
            <method name=".ctor" signature="(int)" line-rate="0" branch-rate="0">
              <lines><line number="20" hits="1" branch="True" condition-coverage="100% (2/2)" /></lines>
            </method>
            <method name=".ctor" signature="(string)" line-rate="0" branch-rate="0">
              <lines><line number="20" hits="1" branch="True" condition-coverage="100% (2/2)" /></lines>
            </method>
            <method name="M" signature="()" line-rate="0" branch-rate="0">
              <lines>
                <line number="30" hits="0" branch="True" condition-coverage="0% (0/2)" />
                <line number="50" hits="1" branch="True" condition-coverage="50% (1/2)" />
              </lines>
            </method>
          </methods>
          <lines>
            <line number="20" hits="1" branch="True" condition-coverage="100% (2/2)" />
            <line number="30" hits="0" branch="True" condition-coverage="0% (0/2)" />
            <line number="40" hits="1" branch="True" condition-coverage="50% (1/2)" />
          </lines>
        </class>
      </classes>
    </package>
    <package name="Ns.Test" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Ns.Test.FooTests" filename="Ns.Test\FooTests.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="1" branch="False" />
            <line number="12" hits="1" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@
}

Describe 'Get-CoberturaFirstPartyCoverageSummary' {
    It 'counts a line repeated across constructor rows once, in both the line and the branch totals' {
        # AC5 and AC6. The four counts are the discriminating assertion; no rate is asserted,
        # because the line rate is 0.75 under both computations over this fixture and a rate
        # assertion would therefore pass whether or not the fix is correct.
        [xml]$document = $script:duplicateRowFixture

        $summary = Get-CoberturaFirstPartyCoverageSummary -XmlDocument $document -ProjectNames @('Ns')
        $descendantAxis = Get-DescendantAxisCoverageTally -XmlDocument $document -ProjectNames @('Ns')

        $summary.LinesValid | Should -Be '4'
        $summary.LinesCovered | Should -Be '3'
        $summary.BranchesValid | Should -Be '8'
        $summary.BranchesCovered | Should -Be '4'

        $descendantAxis.LinesValid | Should -Be 8
        $descendantAxis.BranchesValid | Should -Be 16
        $descendantAxis.LinesValid | Should -BeGreaterThan ([int]$summary.LinesValid)
        $descendantAxis.BranchesValid | Should -BeGreaterThan ([int]$summary.BranchesValid)
    }

    It 'excludes a package outside the supplied first-party allowlist from both totals' {
        # AC4. The second package carries three covered non-branch lines. Retaining it would raise
        # both the numerator and the denominator, so an unchanged pair proves it was skipped.
        [xml]$document = $script:allowlistFixture

        $summary = Get-CoberturaFirstPartyCoverageSummary -XmlDocument $document -ProjectNames @('Ns')

        $summary.LinesValid | Should -Be '4'
        $summary.LinesCovered | Should -Be '3'
    }

    It 'defaults the ProjectNames parameter to Get-KoverageProjectAllowlist' {
        # AC4. Read from the function AST rather than by invoking the function: the default is
        # evaluated at parameter binding and derives its names from the tracked project files, so
        # it can never contain the fixture package name, and invoking it would additionally
        # perform a recursive repository scan that a unit test must not do.
        $commandInfo = Get-Command -Name 'Get-CoberturaFirstPartyCoverageSummary' -ErrorAction Stop
        $parameterAst = $commandInfo.ScriptBlock.Ast.FindAll(
            { $args[0] -is [System.Management.Automation.Language.ParameterAst] }, $true) |
            Where-Object { $_.Name.VariablePath.UserPath -eq 'ProjectNames' }

        $parameterAst.DefaultValue.Extent.Text | Should -Be '(Get-KoverageProjectAllowlist)'
    }

    It 'throws when the document carries no packages node' {
        # Negative scenario. The message matches Get-CoberturaCoverageSummary exactly, so a caller
        # cannot be surprised by a second wording for the same rejection.
        [xml]$document = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" />
'@

        { Get-CoberturaFirstPartyCoverageSummary -XmlDocument $document -ProjectNames @('Ns') } |
            Should -Throw 'Cobertura XML does not contain a <packages> node.'
    }

    It 'returns zero counts and a zero rate when a retained package carries no classes' {
        # Boundary. The fixture's own stale line-rate and branch-rate attributes are deliberately
        # non-zero so a returned '0' cannot come from copying the input.
        [xml]$document = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0.5" branch-rate="0.25" lines-covered="1" lines-valid="2" branches-covered="1" branches-valid="4">
  <packages>
    <package name="Ns" line-rate="0.5" branch-rate="0.25" complexity="1">
      <classes />
    </package>
  </packages>
</coverage>
'@

        $summary = Get-CoberturaFirstPartyCoverageSummary -XmlDocument $document -ProjectNames @('Ns')

        $summary.LinesValid | Should -Be '0'
        $summary.LineRate | Should -Be '0'
        $summary.BranchRate | Should -Be '0'
    }
}

Describe 'Format-CoberturaFirstPartyCoverageSummary' {
    It 'renders the four counts and both percentages on one line' {
        # AC8. The rendered text is asserted without invoking a coverage run, which is why the
        # formatting function is separate from the entry point that prints it.
        [xml]$document = $script:duplicateRowFixture
        $summary = Get-CoberturaFirstPartyCoverageSummary -XmlDocument $document -ProjectNames @('Ns')

        $rendered = Format-CoberturaFirstPartyCoverageSummary -Summary $summary

        $rendered | Should -Be 'First-party coverage: lines 3/4 (75.00%), branches 4/8 (50.00%)'
    }
}

Describe 'Get-CoberturaFirstPartyCoverageReport' {
    It 'renders the report line from a Cobertura string without touching the filesystem' {
        # AC1 and AC8. This composition function is the only production path the entry-point
        # wiring calls, so it is covered here rather than left to the uncovered wiring line.
        $rendered = Get-CoberturaFirstPartyCoverageReport -CoberturaXml $script:duplicateRowFixture -ProjectNames @('Ns')

        $rendered | Should -Be 'First-party coverage: lines 3/4 (75.00%), branches 4/8 (50.00%)'
    }
}
