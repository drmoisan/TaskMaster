# P4-T2 — Plain Entry-Point Wiring

Timestamp: 2026-09-13T06-18
Task: [P4-T2], with the builder signature change from [P4-T1]

Command: pwsh -NoProfile -Command '<dot-source scripts/vscode/Invoke-MSTest.ps1, then parse it and enumerate Get-VsTestArgumentList parameters with their Mandatory attribute state, then invoke the builder once and print the returned array element by element>'
EXIT_CODE: 0

## [P4-T1] — builder parameters and returned array

Parameter block of `Get-VsTestArgumentList`, read from the parsed tree:

```
TestAssembly mandatory=True
RunSettingsPath mandatory=True
ResultsDirectory mandatory=True
LogFileName mandatory=True
```

Both new parameters are declared `[Parameter(Mandatory = $true)]`.

Direct invocation with a one-element assembly array:

```
ELEMENTS=6
  [0] C:\a.Test.dll
  [1] /Settings:C:\rs
  [2] /InIsolation
  [3] /TestCaseFilter:TestCategory!=LiveOutlook
  [4] /ResultsDirectory:C:\rd
  [5] /Logger:trx;LogFileName=mstest-run.trx
```

The two appended elements are the results-directory switch carrying the supplied directory and the trx
logger switch carrying the supplied explicit log file name. The values quoted above are the synthetic
arguments of this observation, not host paths. The plain builder has no argument separator, so no
index-against-separator clause applies to it; the ordering assertion for this member is the
index comparison between the two new switches in
`tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1`.

Note on the plan's verification clause: P4-T1's acceptance names P4-T5 as the verifier for both the
mandatory declaration and the returned array. Of P4-T5's three specified tests only the array-membership
clause is covered there, so the mandatory declaration is recorded here by direct observation of the
parsed parameter block rather than by a test. No test was added beyond the three P4-T5 names.

## [P4-T2] — entry-point wiring

`Invoke-MSTestMain` now declares `[string]$ResultsDirectory = 'coverage\test-results'` and
`[string]$LogFileName = 'mstest-run.trx'`, resolves both against the repository root as
`$resolvedResultsDirectory` and `$resolvedLogFilePath`, dot-sources
`scripts/vscode/Invoke-MSTest.TrxSummary.ps1` from the script directory as the first statement of its
body, and passes `-ResultsDirectory $resolvedResultsDirectory` and `-LogFileName $LogFileName` to the
builder by name.

DOT_SOURCES_SUMMARY_PART_FILE: true. The dot-source is mandatory rather than incidental: this entry
point dot-sources nothing else, so without it `Get-TrxRunSummary` is unresolvable and every summary
attempt would take the non-fatal warning branch.

After the existing exit-code check, in this order: the test-result document is read and reduced to a
run summary inside a try block; on success the summary is written to the results directory joined with
the log file name without its extension plus the suffix `.summary.txt`, the summary path is reported,
and the raw test-result document is discarded. Any failure to obtain a run summary — a missing
document, a read that throws, text that does not parse as XML, or `Get-TrxRunSummary` throwing for any
reason including a document that parses but carries no result-summary node — is caught and reported as
a non-fatal warning, and that single suppression covers both the summary write and the discard. The
existing exit-code throw is untouched, so a genuine test failure is still surfaced, and a test that
mocks no content writer is never made to write a real file.

The summary path is reported with `Write-Output` rather than `Write-Host`, because the repository
analyzer reports `PSAvoidUsingWriteHost` and a third occurrence in this file would be a diagnostic
absent from the P0-T11 baseline set. The two pre-existing `Write-Host` calls are left unchanged. This
is recorded in full in `evidence/qa-gates/p4-t6-phase4-toolchain.md`.

No directory-creation call was added. The test console creates the results directory itself when it is
given the switch, and if the directory does not exist the summary read fails and the non-fatal branch
suppresses the write, so a guard would add an unreachable line rather than protect anything.

Output Summary: Both new parameters are mandatory on the builder; the builder returns a six-element
array for a one-element assembly array, the last two elements being the two new switches; the entry
point declares both defaults, resolves both against the repository root, dot-sources the summary part
file, forwards both to the builder, and performs the summary write and the discard only on the path
where the document was read successfully. Verified against the tests in P4-T5, which report three
passed and zero failed.
