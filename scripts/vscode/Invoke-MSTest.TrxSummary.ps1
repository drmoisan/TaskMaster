Set-StrictMode -Version Latest

# This part file holds the test-result summary projection. It is a separate file rather than an
# addition to either entry point because both entry points dot-source it: the plain run and the
# coverage run each write a summary beside their own test-result document, and duplicating the
# reader in two entry points would give one behaviour two implementations.
#
# Both functions are pure. Neither reads, writes nor deletes a file, and neither starts a process.
# The caller supplies the test-result document as a string and persists the rendered summary, so the
# parsing and the rendering are unit-testable without any file existing.

function Get-TrxRunSummary {
    <#
        .SYNOPSIS
        Reduces a test-result document to a run verdict, its counters and the failed test names.

        .DESCRIPTION
        The test-result document's root declares a default namespace, so every node is in that
        namespace and an unprefixed XPath selects nothing at all. This function resolves every node
        through a local-name predicate instead, which matches regardless of the namespace the
        document declares. A companion test asserts that an unprefixed path over the same fixture
        selects zero nodes, so the namespace handling is proven rather than assumed.

        Skipped is not a figure the test platform reports. It is derived as total minus executed, and
        the derivation is stated in the rendered output so a reader is never left guessing which of
        the reported figures it came from. Every figure the platform does report is carried through
        unchanged, including the not-executed and inconclusive counts, so committing the summary in
        place of the document loses no reported figure.

        The function is pure: it performs no I/O and mutates nothing.

        .PARAMETER TrxContent
        The test-result document as a string. A document carrying no result-summary node is rejected.

        .OUTPUTS
        A pscustomobject carrying Outcome, Total, Executed, Passed, Failed, Error, Timeout, Aborted,
        NotExecuted, Inconclusive, the derived Skipped, and FailedTestName as a collection that is
        empty rather than null when no result failed.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$TrxContent
    )

    [xml]$resultDocument = $TrxContent

    $resultSummaryNode = $resultDocument.SelectSingleNode("//*[local-name()='ResultSummary']")
    if (-not $resultSummaryNode) {
        throw 'Test-result XML has no <ResultSummary> node.'
    }

    $countersNode = $resultSummaryNode.SelectSingleNode("./*[local-name()='Counters']")
    if (-not $countersNode) {
        throw 'Test-result XML has no <Counters> node beneath <ResultSummary>.'
    }

    # A counter the platform omitted is read as zero. GetAttribute is guarded by HasAttribute
    # because Set-StrictMode -Version Latest makes a missing XML attribute throw on bare property
    # access, and an omitted counter is a legitimate document rather than an error.
    $counterFigure = @{}
    foreach ($counterName in @(
            'total', 'executed', 'passed', 'failed',
            'error', 'timeout', 'aborted', 'notExecuted', 'inconclusive')) {
        if ($countersNode.HasAttribute($counterName)) {
            $counterFigure[$counterName] = [int]$countersNode.GetAttribute($counterName)
        }
        else {
            $counterFigure[$counterName] = 0
        }
    }

    $failedTestName = [System.Collections.Generic.List[string]]::new()
    foreach ($resultNode in @($resultDocument.SelectNodes("//*[local-name()='Results']/*"))) {
        if (-not $resultNode.HasAttribute('outcome')) {
            continue
        }

        if ($resultNode.GetAttribute('outcome') -ne 'Failed') {
            continue
        }

        $failedTestName.Add($resultNode.GetAttribute('testName'))
    }

    [pscustomobject]@{
        Outcome        = $resultSummaryNode.GetAttribute('outcome')
        Total          = $counterFigure['total']
        Executed       = $counterFigure['executed']
        Passed         = $counterFigure['passed']
        Failed         = $counterFigure['failed']
        Error          = $counterFigure['error']
        Timeout        = $counterFigure['timeout']
        Aborted        = $counterFigure['aborted']
        NotExecuted    = $counterFigure['notExecuted']
        Inconclusive   = $counterFigure['inconclusive']
        Skipped        = $counterFigure['total'] - $counterFigure['executed']
        FailedTestName = [string[]]$failedTestName.ToArray()
    }
}

function Format-TrxRunSummary {
    <#
        .SYNOPSIS
        Renders a test-run summary as the committed-evidence text.

        .DESCRIPTION
        Pure formatting, separated from the parsing and from the run itself so the committed text is
        assertable in a unit test without invoking a test run. The rendered text states the skipped
        derivation in words rather than leaving a reader to infer it, and it reports the not-executed
        and inconclusive figures verbatim so the summary carries every figure the test platform
        reported.

        The function is pure: it performs no I/O.

        .PARAMETER Summary
        The object Get-TrxRunSummary returns.

        .OUTPUTS
        One string holding the rendered summary. The string carries no trailing newline; the caller
        that persists it supplies the file's final newline.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject]$Summary
    )

    $failedTestText = if (@($Summary.FailedTestName).Count -gt 0) {
        (@($Summary.FailedTestName) -join ', ')
    }
    else {
        'none'
    }

    $summaryLine = [System.Collections.Generic.List[string]]::new()
    $summaryLine.Add('Test run outcome: ' + $Summary.Outcome)
    $summaryLine.Add('Total ' + $Summary.Total + ', executed ' + $Summary.Executed +
        ', passed ' + $Summary.Passed + ', failed ' + $Summary.Failed + '.')
    $summaryLine.Add('Skipped ' + $Summary.Skipped +
        ', derived as total minus executed rather than reported by the test platform.')
    $summaryLine.Add('Figures reported verbatim by the test platform: error ' + $Summary.Error +
        ', timeout ' + $Summary.Timeout + ', aborted ' + $Summary.Aborted +
        ', notExecuted ' + $Summary.NotExecuted + ', inconclusive ' + $Summary.Inconclusive + '.')
    $summaryLine.Add('Failed tests: ' + $failedTestText)

    return ($summaryLine -join [System.Environment]::NewLine)
}
