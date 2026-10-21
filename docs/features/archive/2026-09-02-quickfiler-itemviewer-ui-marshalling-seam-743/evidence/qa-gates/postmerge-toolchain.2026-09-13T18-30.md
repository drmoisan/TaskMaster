# Post-merge toolchain re-run after merging `main` at e6d86049e

Timestamp: 2026-09-13T18-30
Toolchain pass: 1 (single clean pass; no step failed and no step changed a file, so no restart was required)

Merge commit: `f8f4a15d3`, merging `e6d86049e31096914eaf6dcbc7222e5b9f435258` into `bug/quickfiler-itemviewer-ui-marshalling-seam-743`. The merge was automatic with no conflicts.

## Why these gates were re-run

The merge changed four files inside this item's two projects, measured with `git diff --name-only 5f506559 HEAD -- QuickFiler QuickFiler.Test`:

- `QuickFiler.Test/QuickFiler.Test.csproj` (auto-merged; both sides had edits)
- `QuickFiler.Test/SetupAssemblyInitializer.cs`
- `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` (added by `main`)
- `QuickFiler/Controllers/QfcHomeController.cs`

Two of those are build inputs to the assembly this item's acceptance evidence is measured against, so the whole C# toolchain was re-run rather than a subset.

## Step 1 — Format

Timestamp: 2026-09-13T17-55
Command: `dotnet tool run csharpier check .` Run from the item worktree root via `Set-Location` inside one `pwsh` invocation, while holding the shared machine build lock for item 743 (acquired 17:55:58, released 17:56:11).
EXIT_CODE: 0
Output Summary: `Checked 1628 files in 5156ms.` No file was reformatted, so the loop did not restart.

## Step 2 — Analyze

Timestamp: 2026-09-13T17-56
Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, console output redirected to the ignored path `coverage/p7-analyzer.log`. Build lock acquired 17:56:32, released 17:57:09.
EXIT_CODE: 0
Output Summary: `Build succeeded.` / `0 Warning(s)` / `0 Error(s)` / `Time Elapsed 00:00:21.11`. Non-vacuity check: a regex count of diagnostics of the form `(error|warning) XXnnnn` over the whole log returns **0**, and `/t:Rebuild` was used rather than `/t:Build`, so `CoreCompile` ran on every project and the analyzers actually executed.

## Step 3 — Type-check (nullable)

Timestamp: 2026-09-13T17-57
Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`, console output redirected to the ignored path `coverage/p7-nullable.log`. Build lock acquired 17:57:13, released 17:57:43.
EXIT_CODE: 0
Output Summary: `Build succeeded.` / `0 Warning(s)` / `0 Error(s)` / `Time Elapsed 00:00:17.99`. Diagnostic regex count over the log: **0**. `/p:Nullable=enable` was deliberately NOT added, per the CLAUDE.md instruction that the property is a solution-wide opt-in absent from the CI command.

## Step 4 — Test (trx-derived summary)

Timestamp: 2026-09-13T17-58
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p7-postmerge-serial.trx" /ResultsDirectory:coverage\trx\p7 "/TestCaseFilter:TestCategory!=LiveOutlook"`, console output redirected to the ignored path `coverage/p7-vstest.log`. Build lock acquired 17:57:58, released 17:58:26.
EXIT_CODE: 0
Output Summary: `Test Run Successful.` / `Total tests: 1401` / `Passed: 1401` / `Total time: 15.4476 Seconds`. REGIME: SERIAL (no `/Settings:` argument).

Count reconciliation against the pre-merge final gate: P6-T5 recorded `total=1400 passed=1400 failed=0 timeout=0`. The post-merge total is 1401. The single added test is accounted for by `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, which `main` added and the merge brought in. 1400 + 1 = 1401, and the delta is fully explained.

The `.trx` document was written under the gitignored repository-root `coverage` directory and is not committed. This summary is the committed projection of it.

## Coverage: deliberately not re-measured, with the reason stated

AC4 requires a pre-change and a post-change coverage measurement taken **in the same session with the same command**. That pair was taken on 2026-09-13 at 02:26 (P0-T9) and 03:49 (P6-T6) and is recorded in `evidence/qa-gates/ac4-coverage-comparison.2026-09-12T19-30.md`.

Re-running only the post-change half now would break the pairing that AC4's own wording requires, substituting a figure from a different session against a baseline from the earlier one. That would weaken the AC4 evidence, not strengthen it.

The merge does not disturb the measurement. Neither subject file — `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and `QuickFiler/Controllers/QfcItemController.Initialization.cs` — appears in the merge's change set for these projects, verified by the `git diff --name-only` above. The one test class the merge added, `QfcHomeControllerTests`, exercises `QfcHomeController`, which is neither subject file. The recorded AC4 figures therefore stand: ViewerSetup.cs 0.904762 to 0.906103, Initialization.cs 0.950382 unchanged.

## Citation re-derivation after the merge

Every source citation this item's spec and evidence rely on was re-derived against the merged tree. The merge changed none of the cited files, so no citation moved as a result of the merge. One pre-existing stale citation was found and is recorded in `evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md` under `CITATION NOTE`: the P0-T11 declaration's line numbers for `ReleaseTransactionGate` (cited 88-91, actually declared at 107) and `BeginTransactionAsync` (cited 122-126, actually declared at 142) were shifted by this item's **own** P1 instrumentation, not by the merge. The citation that carries correction C2 is line 32, which is unchanged and was re-verified to read `private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1);`, with the unbounded `await TransactionGate.WaitAsync()` at line 149.

## Raw-artifact hygiene after the merge

No `.trx` and no `.cobertura.xml` was added to the tree by this pass. All tool output was directed to the gitignored repository-root `coverage` directory.
