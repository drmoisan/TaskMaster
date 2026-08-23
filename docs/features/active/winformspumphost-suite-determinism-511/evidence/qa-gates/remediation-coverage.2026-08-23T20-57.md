# Remediation QA Gate — Post-Change Numeric Coverage

Timestamp: 2026-08-23T19-25

Command:
```
pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\remediation.cobertura.xml
```

Run from the worktree root and launched per the Phase 3 long-running command mechanic: a detached
`pwsh -NoProfile` runner invoked `Start-Process -PassThru` with stdout redirected to
`coverage\coverage-remediation.log` and stderr to `coverage\coverage-remediation.err.log`, recorded
the child PID, then polled to completion. The recorded exit code is taken from the returned process
object's `ExitCode` property. The script wraps the same nine assemblies with `dotnet-coverage` and
emits Cobertura XML; `vstest.console.exe /EnableCodeCoverage` alone emits a binary `.coverage` file,
not a percentage, which is why this script is the source of numeric coverage.

EXIT_CODE: 0

Output Summary:

| Measure | Value |
| --- | --- |
| Launched PID | **12564** |
| Exit code (from the process object's `ExitCode`) | **0** |
| Discovered test assemblies | **9** |
| vstest result | `Test Run Successful.` — total 6459, passed 6459, failed 0 |
| vstest exit code, recorded verbatim | **0** (the script exits non-zero and throws at line 236 when `dotnet-coverage` returns non-zero; it returned 0) |
| Stderr log size | 0 bytes |
| Post-processed Cobertura XML | `coverage\remediation.cobertura.xml` (10,465,460 bytes) |
| Wall time | 47.0414 s test time; run ended 2026-08-23T19-24-41 |

Script output tail, verbatim:

```
Discovered 9 test assemblies.
Test Run Successful.
Total tests: 6459
     Passed: 6459
 Total time: 47.0414 Seconds
Code coverage results: ...\coverage\remediation.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: ...\coverage\remediation.cobertura.xml
```

### The four required figures, as numeric percentages to two decimal places

| Figure | Cobertura attribute | Raw | Percent |
| --- | --- | --- | --- |
| Repository headline line rate | root `line-rate` | `0.855916` | **85.59%** |
| Repository headline branch rate | root `branch-rate` | `0.790598` | **79.06%** |
| `QuickFiler` package line rate | `QuickFiler` package `line-rate` | `0.81084081028582` | **81.08%** |
| Changed-module rate (`QuickFiler\Controllers\QfcItemController*` classes, aggregated) | per-`<line>` count across the 10 matched classes | 1410 / 1633 | **86.34%** |

Supporting root counters: `lines-covered="53505"`, `lines-valid="62512"`,
`branches-covered="12580"`, `branches-valid="15912"`. The `QuickFiler` package branch rate is
`0.7502908103916247` (**75.03%**).

No coverage field above is empty and none carries the token `UNVERIFIED`.

### All nine packages

| Package | line-rate | Percent |
| --- | --- | --- |
| QuickFiler | 0.81084081028582 | 81.08% |
| UtilitiesCS | 0.8958155619596542 | 89.58% |
| TaskVisualization | 0.8984326018808777 | 89.84% |
| SVGControl | 0.47303128371089537 | 47.30% |
| ToDoModel | 0.5731056563500534 | 57.31% |
| Tags | 0.9268929503916449 | 92.69% |
| TaskMaster | 0.7335945151811949 | 73.36% |
| TaskTree | 0.9548387096774194 | 95.48% |
| VBFunctions | 1 | 100.00% |

### Per-class figures for the changed module

Ten Cobertura classes have a `filename` beginning `QuickFiler\Controllers\QfcItemController`
(`MATCH_COUNT=10`), matching the baseline's match count exactly. Filenames use backslashes because
the script's Koverage post-processing rewrites them; a forward-slash query matches nothing.

| Filename | line-rate | Percent | branch-rate |
| --- | --- | --- | --- |
| `QuickFiler\Controllers\QfcItemController.cs` | 1 | 100.00% | 0.7857142857142857 |
| `QuickFiler\Controllers\QfcItemController.Initialization.cs` | 0.949612 | 94.96% | 0.90625 |
| `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs` | 0.850829 | 85.08% | 0.677419 |
| `QuickFiler\Controllers\QfcItemController.Conversation.cs` | 0.882353 | 88.24% | 0.944444 |
| `QuickFiler\Controllers\QfcItemController.FolderHandling.cs` | 0.952381 | 95.24% | 0.7 |
| `QuickFiler\Controllers\QfcItemController.EventWiring.cs` | 0.815182 | 81.52% | 0.65 |
| `QuickFiler\Controllers\QfcItemController.EventHandlers.cs` | 0.7865168539325843 | 78.65% | 0.6111111111111112 |
| `QuickFiler\Controllers\QfcItemController.Navigation.cs` | 0.90678 | 90.68% | 0.818182 |
| `QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs` | 0.793249 | 79.32% | 0.691176 |
| `QuickFiler\Controllers\QfcItemController.MailActions.cs` | 0.768 | 76.80% | 0.727273 |

### Counting method (reproduced from the baseline exactly)

The aggregate changed-module figure was computed by counting `<line>` elements inside each matched
`<class>`, deduplicated by line `number` **within** each class, and summing across the ten classes:
1,633 total lines, 1,410 covered, **86.34%**. This is byte-for-byte the method the baseline artifact
mandates. Cobertura repeats line entries under `<method>` as well as under the class-level `<lines>`
element, so an all-descendant count without deduplication roughly doubles the denominator and would
fabricate a coverage delta. The measurement was taken against the **post-processed** XML produced by
the same script that produced the baseline.

### Attempts

| Attempt | Timestamp | Outcome | Machine-load state at launch |
| --- | --- | --- | --- |
| 1 (only) | 2026-08-23T19-23 launch, 19-24-41 end | Success. `Test Run Successful`, 6459/6459, `dotnet-coverage` exit 0, post-processing completed. | Idle. Average processor load sampled at 15% immediately before launch; zero `testhost`, `vstest.console`, and `dotnet-coverage` processes resident; the 17 idle MSBuild node-reuse processes left by this cycle's own P3-T4 analyzer build (all with StartTime 19:16:12, i.e. started by this run) were stopped first, per the load lesson recorded in the baseline coverage artifact. |

The bounded re-run authorization in P3-T7 (up to two additional attempts for a load-induced
60,000 ms `PumpTimeoutMs` expiry in `QuickFiler.Test`, the out-of-scope #592 defect) was **not
exercised**: the first attempt succeeded. Total attempts: 1 of a permitted maximum of 3.

### Sibling-assembly failures during the coverage run

**None.** The run reported 6459 total and 6459 passed with zero failures, so no sibling-assembly
failure needed to be listed or attributed to issue #594.

### Acceptance conditions

1. The script reported exactly 9 discovered test assemblies — met (`Discovered 9 test assemblies.`).
2. No coverage field is empty or contains the token `UNVERIFIED` — met; all four figures are numeric.
3. The vstest exit code is recorded verbatim — met (0).
4. Every attempt is recorded — met; a single attempt, recorded above.
5. Sibling-assembly failures listed and attributed — vacuously met, there were none.

The raw Cobertura XML stays in the gitignored `coverage\` directory and is not copied into the
evidence tree. P3-T9 copies it to the gitignored `artifacts/csharp/` producer path for the
downstream review-gate hook.
