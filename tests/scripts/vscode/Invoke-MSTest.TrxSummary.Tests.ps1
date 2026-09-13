Set-StrictMode -Version Latest

BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $summaryScriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-MSTest.TrxSummary.ps1'
    . $summaryScriptPath

    # The namespaced fixture. Its root declares the default TeamTest namespace, which is what a real
    # test-result document does, so a reader that relies on the namespace being absent selects
    # nothing from it. Total 10 against executed 9 leaves one skipped, and the platform's own
    # notExecuted figure is 1.
    $script:namespacedTrxFixture = @'
<TestRun id="8d3f1b84-0000-4000-8000-000000000001" name="local run" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
    <UnitTestResult testName="Contoso.Alpha.PassesCleanly" outcome="Passed" duration="00:00:00.0100000" />
    <UnitTestResult testName="Contoso.Alpha.FailsOnAssertion" outcome="Failed" duration="00:00:00.0200000" />
  </Results>
  <ResultSummary outcome="Failed">
    <Counters total="10" executed="9" passed="8" failed="1" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="1" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
  </ResultSummary>
</TestRun>
'@

    # The derivation fixture. Total 12 against executed 7 leaves five skipped, which deliberately
    # differs from the platform's own notExecuted figure of 4, so an implementation that copied
    # notExecuted instead of subtracting would fail rather than coincide.
    $script:derivationTrxFixture = @'
<TestRun id="8d3f1b84-0000-4000-8000-000000000002" name="local run" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
    <UnitTestResult testName="Contoso.Beta.PassesCleanly" outcome="Passed" duration="00:00:00.0100000" />
  </Results>
  <ResultSummary outcome="Completed">
    <Counters total="12" executed="7" passed="5" failed="2" error="1" timeout="0" aborted="0" inconclusive="2" passedButRunAborted="0" notRunnable="0" notExecuted="4" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
  </ResultSummary>
</TestRun>
'@

    # The verdict fixture. Four results of four different outcomes, two of them Failed, so an
    # implementation that returned every result name, or the first failure only, would fail the
    # exactness assertion rather than satisfy it.
    $script:failingTrxFixture = @'
<TestRun id="8d3f1b84-0000-4000-8000-000000000003" name="local run" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
    <UnitTestResult testName="Contoso.Gamma.PassesCleanly" outcome="Passed" duration="00:00:00.0100000" />
    <UnitTestResult testName="Contoso.Gamma.FailsFirst" outcome="Failed" duration="00:00:00.0200000" />
    <UnitTestResult testName="Contoso.Gamma.NeverRan" outcome="NotExecuted" />
    <UnitTestResult testName="Contoso.Gamma.FailsSecond" outcome="Failed" duration="00:00:00.0300000" />
  </Results>
  <ResultSummary outcome="Failed">
    <Counters total="4" executed="3" passed="1" failed="2" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="1" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
  </ResultSummary>
</TestRun>
'@

    # A run that reported counters but carries no result element at all.
    $script:noResultElementTrxFixture = @'
<TestRun id="8d3f1b84-0000-4000-8000-000000000004" name="local run" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results />
  <ResultSummary outcome="Completed">
    <Counters total="0" executed="0" passed="0" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
  </ResultSummary>
</TestRun>
'@

    # A document that parses as XML but carries no result-summary node, which is the negative path.
    $script:noResultSummaryTrxFixture = @'
<TestRun id="8d3f1b84-0000-4000-8000-000000000005" name="local run" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
    <UnitTestResult testName="Contoso.Delta.PassesCleanly" outcome="Passed" duration="00:00:00.0100000" />
  </Results>
</TestRun>
'@

    function Get-UnprefixedResultSummaryNodeCount {
        <#
            .SYNOPSIS
            Counts the nodes an unprefixed XPath selects from a test-result document.

            .DESCRIPTION
            A test-only probe. It exists so the companion namespace test measures the very thing the
            production reader had to avoid: an XPath that names the element without a namespace
            prefix and without a local-name predicate. On a namespaced document the count is zero,
            which is why a reader written that way would report an empty run as a false green.
        #>
        [CmdletBinding()]
        [OutputType([int])]
        param(
            [Parameter(Mandatory = $true)]
            [string]$TrxContent
        )

        [xml]$resultDocument = $TrxContent

        return @($resultDocument.SelectNodes('//ResultSummary')).Count
    }
}

Describe 'Get-TrxRunSummary namespace handling' {
    It 'reads the counters correctly from a namespaced test-result document' {
        # AC9, first half. The reader must resolve nodes through a local-name predicate so the
        # default namespace the fixture declares does not hide them.
        $summary = Get-TrxRunSummary -TrxContent $script:namespacedTrxFixture

        $summary.Outcome | Should -BeExactly 'Failed'
        $summary.Total | Should -Be 10
        $summary.Executed | Should -Be 9
        $summary.Passed | Should -Be 8
        $summary.Failed | Should -Be 1
    }

    It 'an unprefixed XPath over the same fixture selects zero nodes' {
        # AC9, second half, and mandatory rather than optional. Without it the first test would pass
        # for a reader that happened to work on a fixture with no namespace, and a namespace
        # regression would then surface as a run reporting zero of everything rather than as a
        # failure. The asserted count is exactly zero over the identical fixture the first test uses.
        Get-UnprefixedResultSummaryNodeCount -TrxContent $script:namespacedTrxFixture |
            Should -Be 0
    }
}

Describe 'Get-TrxRunSummary skipped derivation' {
    It 'reports skipped as total minus executed and preserves the verbatim platform figures' {
        # AC10, first half. Skipped is not a figure the test platform reports, so it is derived. The
        # fixture's derived skipped figure of 5 differs from its notExecuted figure of 4, so the
        # assertion distinguishes a subtraction from a copy. Every figure the platform did report is
        # asserted to be carried through unchanged.
        $summary = Get-TrxRunSummary -TrxContent $script:derivationTrxFixture

        $summary.Total | Should -Be 12
        $summary.Executed | Should -Be 7
        $summary.Skipped | Should -Be 5
        $summary.Skipped | Should -Be ($summary.Total - $summary.Executed)

        $summary.NotExecuted | Should -Be 4
        $summary.Inconclusive | Should -Be 2
        $summary.Error | Should -Be 1
        $summary.Timeout | Should -Be 0
        $summary.Aborted | Should -Be 0
        $summary.Passed | Should -Be 5
        $summary.Failed | Should -Be 2
    }

    It 'states the skipped derivation in the formatted output' {
        # AC10, second half. The rendered text must say where the skipped figure came from rather
        # than leaving a reader to infer it from the other counters, and it must also carry the
        # not-executed and inconclusive figures so committing the summary loses no reported figure.
        $summary = Get-TrxRunSummary -TrxContent $script:derivationTrxFixture

        $formatted = Format-TrxRunSummary -Summary $summary

        $formatted | Should -Match 'derived as total minus executed'
        $formatted | Should -Match 'Skipped 5'
        $formatted | Should -Match 'notExecuted 4'
        $formatted | Should -Match 'inconclusive 2'
    }
}

Describe 'Get-TrxRunSummary verdict and failed names' {
    It 'reports the run verdict and the names of the failed results' {
        # AC11, first half. The verdict comes from the result-summary outcome attribute rather than
        # from any counter, and the returned names are exactly the two results whose own outcome
        # attribute is Failed, in document order. The fixture also holds a Passed and a NotExecuted
        # result, so returning every result name would fail this assertion.
        $summary = Get-TrxRunSummary -TrxContent $script:failingTrxFixture

        $summary.Outcome | Should -BeExactly 'Failed'

        $failedName = @($summary.FailedTestName)
        $failedName.Count | Should -Be 2
        $failedName[0] | Should -BeExactly 'Contoso.Gamma.FailsFirst'
        $failedName[1] | Should -BeExactly 'Contoso.Gamma.FailsSecond'
    }

    It 'returns an empty failed-name collection for a fixture with no result elements' {
        # AC11, second half. The distinction between an empty collection and a null reference matters
        # to the caller, which enumerates the names. The assertions are written without piping the
        # value into Should, because piping an empty collection sends no input at all and would make
        # an empty result indistinguishable from a null one.
        $summary = Get-TrxRunSummary -TrxContent $script:noResultElementTrxFixture

        $failedName = $summary.FailedTestName
        ($null -eq $failedName) | Should -BeFalse
        ($failedName -is [System.Array]) | Should -BeTrue
        @($failedName).Count | Should -Be 0
    }

    It 'throws a specific message for a document with no result-summary node' {
        # AC11 negative path. The fixture parses as XML and carries a result element, so the only
        # thing missing is the result-summary node, which is what the thrown message must name.
        { Get-TrxRunSummary -TrxContent $script:noResultSummaryTrxFixture } |
            Should -Throw -ExpectedMessage '*has no <ResultSummary> node.*'
    }
}
