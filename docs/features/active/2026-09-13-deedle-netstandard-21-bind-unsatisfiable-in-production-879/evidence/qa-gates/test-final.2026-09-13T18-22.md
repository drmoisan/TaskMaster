# Phase 5 Step 4 — Tests With Coverage, Final Toolchain Loop

Recorded by `[P5-T7]`.

Timestamp: 2026-09-14T11-52

Build lock: ACQUIRED 879 at 2026-09-14T11:52:31, RELEASED by 879 at 2026-09-14T11:54:08.

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

Run from `<worktree-root>` via `Set-Location -LiteralPath <worktree-root>`. `-CoverageOutput`
was left at its default, `coverage\coverage.cobertura.xml`. Issue #873's
`Test-RawCoverageDocumentRetained` deletes the raw Cobertura document unless its parent
directory is exactly the repository `coverage` directory, by equality and not containment, so
the default is load-bearing and was not changed. `-SearchRoot .` is the solution-wide scope
that `[P0-T8]` used, and it was not narrowed: narrowing it would change the coverage
denominator `[P5-T10]` compares.

EXIT_CODE: 0

ExpectedExitCode: 0

Cobertura Document State: POSTPROCESSED

Output Summary:

```
Discovered 9 test assemblies.
line-rate      = 0.858327
lines-valid    = 65616
lines-covered  = 56320
branch-rate    = 0.79973
test total     = 7283
test failed    = 0
test passed    = 7283
```

Failing Test Names: NONE

## Acceptance

All six numeric values are present as numbers rather than placeholders: `line-rate`,
`lines-valid` and `lines-covered` read from `coverage/coverage.cobertura.xml`, and the total,
failed and passed counts read from `coverage/test-results/mstest-coverage-run.trx`.

The `Failing Test Names:` field records the literal `NONE` because the failed count is zero.
The empty set is a subset of the two failures recorded at `[P0-T8]`, namely
`ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` and
`InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic`, so the subset condition
is satisfied and this task does not block. The subset formulation rather than an equality on
the count is what makes that so: those two are intermittent, and this run is the case the plan
anticipated in which the intermittent pair happens to pass. No third name appeared, and no
name outside those two appeared, so the loop was not restarted.

The failing-name list was derived mechanically from the TRX rather than from the console
tail: every `UnitTestResult` whose `outcome` is not `Passed` was enumerated and the count was
zero, which agrees with the `ResultSummary` counters above.

## Exit-Code Path Observed

The run exited 0, which is a different path from the one `[P0-T8]` observed and the difference
is attributable and recorded rather than assumed.

`Invoke-MSTestWithCoverage.ps1` line 262 throws on a non-zero collection exit code, which is
the path `[P0-T8]` took because two tests failed there; that throw happens before
post-processing at lines 383-384, which is why the baseline document is raw collector output.
Here no test failed, so the collection exit code was zero, the throw did not occur, and
post-processing ran. `Cobertura Document State: POSTPROCESSED` is confirmed independently of
the exit path by a fixed-string search of the document for the literal `<sources>`, which
post-processing injects and the raw document does not carry: that search returned 1 hit,
against 0 hits at baseline.

Issue #891's `Assert-CoberturaLineCoverageThreshold` at line 386 therefore did run in this
invocation, and it did not throw: it asserts against the DOCUMENT-LEVEL line-rate of the
post-processed document, which is the first-party rate of `0.858327`, and that clears its
hard-coded threshold. The runner reported
`First-party coverage: lines 56320/65616 (85.83%), branches 13613/17022 (79.97%)`. Issue #891
is named here rather than worked around; no runner parameter was added and
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` was not edited, it being outside the
authorised write set.

## Denominator Note for `[P5-T10]`

The figures above are POST-PROCESSED, first-party only. `[P0-T8]`'s figures are RAW collector
output over all modules including third-party. The two document-level rates are therefore not
directly comparable, and `[P5-T10]` records that rather than asserting a comparison.

## Evidence Hygiene

Neither the raw Cobertura document nor the TRX is committed. Both live under `coverage/`,
which `.gitignore` line 144 excludes via `coverage/*`. Only the projected figures above are
retained in this artifact.
