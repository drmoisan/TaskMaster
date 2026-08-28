# Final QA — Step 4, Full-Suite Test Run and Coverage (P7-T5, AC-32 first half)

Timestamp: 2026-08-27T21-02

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\postchange.cobertura.xml`

Launched with `Start-Process -PassThru -RedirectStandardOutput -RedirectStandardError -WindowStyle Hidden`
and `-WorkingDirectory WS`, polled at 10-second intervals on `HasExited` plus stdout-log growth, then
`WaitForExit()`.

- PID: 92904
- EXIT_CODE: **0** (taken from the process object's `ExitCode`, not inferred from log text)
- Retries used: **0**. The run completed on the first attempt; no `PumpTimeoutMs` expiry occurred, so the
  at-most-twice re-run allowance was not drawn on and no process tree needed killing.
- stdout log: `FF/evidence/qa-gates/p7-t5-coverage-stdout.log` (498107 bytes, sanitized: workspace
  rendered as `WS`, host as `<host>`, account as `<account>`)
- stderr log: `FF/evidence/qa-gates/p7-t5-coverage-stderr.log` (0 bytes — empty)

## Test counts

```
Test Run Successful.
Total tests: 6711
     Passed: 6711
```

| Metric | Baseline (P0-T14) | Post-change | Delta |
| --- | ---: | ---: | ---: |
| Total | 6701 | **6711** | +10 |
| Passed | 6701 | **6711** | +10 |
| Failed | 0 | **0** | 0 |
| Skipped | 0 | **0** | 0 |

The +10 is exactly the ten new test methods this feature authored (P1-T1, P1-T3, P3-T1, P3-T3, P4-T1,
P4-T7, P4-T8, P5-T1, P5-T3, P5-T4). No test was removed, renamed, or disabled.

Mechanical confirmation of the zero counts: the log contains **0** lines matching
`^\s*(Failed|Skipped)\b` and **0** occurrences of `Test Run Failed`. `vstest.console.exe` prints a
per-test `Failed`/`Skipped` line and a summary `Failed:`/`Skipped:` line only when the count is non-zero.

## Per-assembly result

Nine test assemblies were discovered and executed:

```
QuickFiler.Test.dll   SVGControl.Test.dll   Tags.Test.dll
TaskMaster.Test.dll   TaskTree.Test.dll     TaskVisualization.Test.dll
ToDoModel.Test.dll    UtilitiesCS.Test.dll  VBFunctions.Test.dll
```

- **Within `QuickFiler.Test` specifically: 0 failed and 0 skipped.** SATISFIED.
- **For every OTHER `*.Test.dll`: the failing-test set is empty.** `BASELINE_FAILURE_SET` recorded by
  P0-T14 is also explicitly empty, and the empty set is a subset of the empty set, so **no NEW failure was
  introduced outside `QuickFiler.Test`**. SATISFIED.

## Skipped-test note — plan prose corrected

The task text anticipates that "a repo-wide run always reports skipped >= 5" because of five active
`[Ignore]` attributes in `UtilitiesCS.Test`. The observed skipped count is **0**, both at baseline and
post-change.

The five `[Ignore]` attributes do exist and were verified present at
`UtilitiesCS.Test/InputBox_Test.cs:11`, `UtilitiesCS.Test/ResourceTests.cs:17`, `:25`, `:108`, and
`UtilitiesCS.Test/YesNoToAll_Test.cs:10`. The runner nonetheless reports zero skipped, so those tests are
not counted as skipped in this harness's summary. This is a discrepancy in the plan's prose, not a
regression: the figure is identical to baseline, it is explicitly non-gating ("Skipped tests outside
`QuickFiler.Test` are recorded but do not gate"), and no `[Ignore]` attribute was added or removed by this
feature. Those files were not edited, as the P9-T5 scope lock requires.

## Coverage (numeric, from the Cobertura root element)

Artifact copied to `FF/evidence/qa-gates/postchange.cobertura.2026-08-27T21-00.xml` (10709157 bytes;
verified to contain no absolute host path, no account name and no machine name — filenames are relative
and `<source>` is `.`).

Root element read verbatim:

```xml
<coverage line-rate="0.851369" branch-rate="0.792075" complexity="25252" version="1.9"
          timestamp="1787864469" lines-covered="54411" lines-valid="63910"
          branches-covered="12933" branches-valid="16328">
```

| Metric | Raw rate | Percentage (2 dp) |
| --- | ---: | ---: |
| Repository `line-rate` | 0.851369 | **85.14%** |
| Repository `branch-rate` | 0.792075 | **79.21%** |

Supporting absolute figures: 54411 of 63910 lines covered; 12933 of 16328 branches covered.

No value in this artifact is the placeholder `UNVERIFIED`. The delta comparison against baseline is
P7-T6's task.

Acceptance: observed `EXIT_CODE: 0`; 0 failed and 0 skipped within `QuickFiler.Test`; numeric repository
line-rate 85.14% and branch-rate 79.21%; and for every other `*.Test.dll` an empty failing set, which is a
subset of the empty `BASELINE_FAILURE_SET`. PASS (AC-32, first half).
