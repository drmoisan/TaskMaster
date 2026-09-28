# Phase 2 independent confirmation: [P2-T14] re-run and em-dash conservation re-check

- Issue: #792
- Timestamp: 2026-09-17T19-29
- Command: the [P2-T14] command sequence exactly as recorded in `evidence/regression-testing/p2-t14-pure-move-proof.md` (CMD-OUTLOOK, CMD-VSTEST, CMD-BUILD-PLAIN, CMD-SCOPED-RUN with `<task>` = `p2-t14`), run synchronously a second time from `coverage/plan792-helper.ps1` with the item worktree as the working directory, plus a byte-level non-ASCII scan over the six `QuickFiler/Controllers/EfcFormController*.cs` parts against `git show 0d0275e99:QuickFiler/Controllers/EfcFormController.cs`
- EXIT_CODE: 0
- Output Summary: second run reproduced the recorded result exactly: `Test Run Successful.`; `Total tests: 187`; `Passed: 187`; `Failed: 0 (omitted category)`; `Skipped: 0 (omitted category)`; `Total time: 2.3218 Seconds`; CMD-BUILD-PLAIN `EXIT_CODE: 0`, `Build succeeded.`, `    0 Warning(s)`, `    0 Error(s)` (exact-line match true). Em-dash re-check: the six parts contain 2 non-ASCII lines, the original contains 2, and no mojibake was found; no edit was needed.

## Why this artifact exists

This executor was relaunched with the instruction to execute [P2-T13] through [P2-T15] on the belief that the earlier executor had stopped. The earlier executor was still live in this worktree: it wrote its [P2-T14] artifact at 19:26, committed [P2-T15] as `11f5aa598` at 19:28:54 and checked off [P2-T14] and [P2-T15] in the plan at 19:29:36, while this executor's own [P2-T14] run was in progress. This executor's write of `p2-t14-pure-move-proof.md` briefly overwrote the committed artifact in the working copy; it was restored from `HEAD` (`git checkout HEAD -- <that path>`) and this executor's observations are recorded here instead, in `evidence/other/`, so the committed [P2-T14] evidence is unchanged. No source file was written by this executor. Detection signals: plan-file check-off count advanced during a read-only interval, and HEAD advanced from `0d0275e99` to `11f5aa598` between two status samples.

## [P2-T14] second-run observations (TRX and console)

- `COUNTERS: total=187 executed=187 passed=187 failed=0`; `OUTCOME: 187 Passed`; 187 `Passed <name>` console lines; zero `Failed ` and zero `Skipped ` lines; `TRX-PRESENT: true` under the gitignored `coverage/test-results/p2-t14/`.
- Positive controls on the filter: each of the eight `FullyQualifiedName~` alternatives matched discovered tests (`EfcFormControllerTests` 32, `EfcItemControllerTests` 10, `EfcDataModel` 33, `QfcCollectionControllerTests` 13, `ViewerQueueStaticWrapperTests` 8, `BreadcrumbBridgeRouterQueueTests` 26, `WebView2BreadcrumbHostTests` 8, `EfcHomeController` 57; 17 classes, 187 total). `ISSUE792-CLASSES-IN-RUN: 0`, so the three Phase 1 `*Issue792Tests` classes with their four `[expect-fail]` tests were not selected, as intended. `Select-String -SimpleMatch 'LiveOutlook'` over `QuickFiler.Test/` returns 0 files, so the absence of a `TestCategory` clause selects no live-Outlook test.
- Build non-vacuity: `BUILD-CSC-INVOCATIONS: 0`, `BUILD-CORECOMPILE-SKIPPED: 18` (incremental). `QuickFiler/bin/Debug/QuickFiler.dll` 19:24:31 and `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` 19:24:34 were produced by the [P2-T13] `/t:Rebuild`; the newest source under `QuickFiler/` and `QuickFiler.Test/` (`*.cs`, `*.csproj`, excluding `bin/` and `obj/`) is 19:23:59, so the assemblies under test embody the committed Phase 2 tree.

## Em-dash conservation re-check (`QuickFiler/Controllers/EfcFormController.cs:195`)

The relaunch instruction reported one non-ASCII line across the six parts against two in the original, attributing the loss to a hyphen substituted for the em dash in the doc comment now at line 195. Re-derived byte-level (`[System.IO.File]::ReadAllBytes`, UTF-8 decode, per-line scan for any code point above U+007F, plus a scan for the mojibake sequences `Ã` and `â€`):

| File | Lines | BOM | Non-ASCII lines |
|---|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | 266 | yes | 2: line 1 (U+FEFF, BOM) and line 195 (U+2014, em dash) |
| `QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs` | 243 | no | 0 |
| `QuickFiler/Controllers/EfcFormController.EventHandlers.cs` | 383 | no | 0 |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | 184 | no | 0 |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 125 | no | 0 |
| `QuickFiler/Controllers/EfcFormController.Helpers.cs` | 270 | no | 0 |

- `NON-ASCII-LINE-COUNT: 2`; `MOJIBAKE-LINE-COUNT: 0`; every part is CRLF-only (LF count equals CRLF count in each).
- Original at `0d0275e99`: `ORIGINAL-NON-ASCII-LINE-COUNT: 2`, the same two lines (the BOM-prefixed `using System;` and the `load-bearing rather than defensive —` doc-comment line).
- Conclusion: line 195 already holds U+2014 on disk; the parts are byte-faithful to the original for this line. No file was edited. The reported count of one is consistent with the instrument effect recorded in `evidence/qa-gates/p2-t13-compile-gate.md` (a `git show` stream decoded as code page 437 turns U+2014 into three characters and the BOM line into a differently filtered line), not with any loss in the working tree.
