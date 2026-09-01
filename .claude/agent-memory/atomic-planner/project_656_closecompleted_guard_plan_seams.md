---
name: project-656-closecompleted-guard-plan-seams
description: Planning seams found authoring the #656 CloseCore completed-close guard plan — wrapper scripts accept no TestCaseFilter, the coverage runner prints no percentage, and MSBuild non-vacuity is better proved by assembly mtimes than by log text
metadata:
  type: project
---

Seams found while authoring the atomic plan for issue #656 (`BreadcrumbDropDownOpenCoordinator.CloseCore` completed-close guard).

**Why:** each of these would have produced an unsatisfiable or vacuous acceptance condition, and none is visible from the plan text alone.

**How to apply:** re-check these before authoring any TaskMaster C# plan that runs a scoped test, asserts a coverage number, or asserts MSBuild non-vacuity.

- **Neither test wrapper accepts a `TestCaseFilter` override.** `scripts/vscode/Invoke-MSTest.ps1:54` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1:76` both hardcode `'/InIsolation', '/TestCaseFilter:TestCategory!=LiveOutlook'` with no parameter to extend it. A `[expect-fail]` red run scoped to one test therefore requires a **direct** `vstest.console.exe` invocation that reproduces both protections explicitly — `/InIsolation` plus a `TestCategory!=LiveOutlook&FullyQualifiedName~<name>` conjunction. That is not the prohibited "bare" call; editing the wrapper would breach a footprint boundary.
- **`Invoke-MSTestWithCoverage.ps1` prints no coverage percentage on a successful run.** `Assert-CoberturaLineCoverageThreshold` (`Invoke-MSTestWithCoverage.Helpers.ps1:459-491`) only *throws* below 80%. Never assert a printed percentage. Read `line-rate` from `coverage/coverage.cobertura.xml` instead. That path is git-ignored (`.gitignore:144`, `coverage/*`).
- **Its `EXIT_CODE: 0` strictly implies zero failed tests**, because `Invoke-MSTestWithCoverage.ps1:235-237` throws when the inner vstest exit code is non-zero. That fact is what lets a plan assert zero failures without asserting a console literal.
- **Cobertura `class/@filename` is repo-relative with backslashes**, and classes are merged one node per file (`ConvertTo-KoverageRelativePath` at `Helpers.ps1:50-97`, `Merge-CoberturaClassesByFilename`). Per-file coverage lookups must use `QuickFiler\Viewers\X.cs`, not a forward-slash or absolute form.
- **Prove MSBuild non-vacuity with output-assembly `LastWriteTime`, not log text.** Asserting the absence of `Skipping target "CoreCompile"` needs a positive control, and whether that string appears at `Verbosity=normal` is an unobserved assumption. Record a `Gate Start:` wall-clock stamp and assert `bin\Debug\<asm>.dll` `LastWriteTime` is later — that proves recompilation regardless of verbosity.
- **`Select-String` has no `-Recurse` parameter.** Pipe `Get-ChildItem -Recurse -Filter *.cs` into it. And use `@(Select-String ...).Count`, never `(Select-String ...).Count`, so a zero-match result is a number.
- **`Invoke-VSBuild.ps1` is unusable under a footprint boundary** — it runs `Sync-PackageReferences.ps1` over every `.csproj` and rewrites `HintPath` values. Resolve MSBuild through vswhere directly, as `Invoke-Restore.ps1:22-30` does.
- **Bootstrap facts for a fresh agent worktree:** `.dotnet-sdk/` and `packages/` are both absent and both git-ignored (`.gitignore:350` `.dotnet*/`; `.gitignore:191` `**/[Pp]ackages/*`), and `dotnet-tools.json` sits at the worktree ROOT, not under `.config/`. `dotnet-coverage` must also resolve or `Invoke-MSTestWithCoverage.ps1:292-294` throws.
- **Guard-literal replacement was safely zero-hit checkable:** `if (_closeCompleted)` is NOT a substring of `if (_closeCompleted && !hostOpen)` because the searched literal carries the closing parenthesis. Pair it with a rule that the new doc comments must not contain the old literal.
- **Count `_host.` on non-comment lines only** (`^\s*[^/\s].*_host\.`, baseline 5 lines). The AC-19 doc comments would otherwise inflate the count that AC-2's lock-discipline gate reads.

**Round-2 preflight delta (2026-08-31) added four more:**

- **A Cobertura `class` node carries no `lines-covered`/`lines-valid`.** `Invoke-MSTestWithCoverage.Helpers.ps1:374-380` sets exactly `line-rate`, `branch-rate`, `complexity` on a class node; `:442-445` sets `lines-covered`/`lines-valid` on the ROOT `coverage` node only. `GetAttribute` returns an empty string for the missing pair, so a per-file covered/valid gate reading them is unsatisfiable. Derive per-file counts from the class-relative `./lines/line` set (count, and count with `hits > 0`) — the descendant axis double-counts.
- **The coverage wrapper produces NO TRX.** `Invoke-MSTestWithCoverage.ps1:70-76` passes no `/Logger:trx` and `scripts/vscode/TaskMaster.cli.runsettings` declares no logger. Test counts and failing names for a wrapper run must come from the tee'd console log, never from a TRX. Only a direct `vstest.console.exe` call that passes `/Logger:trx` writes one.
- **`TestResults\` does not exist in a fresh worktree**, and neither the msbuild file logger nor `Tee-Object -FilePath` creates a missing parent directory. Add a Phase 0 task that `New-Item -Force`s every subdirectory the plan writes into, before the first task that writes one.
- **A wrapper failure anywhere kills the coverage baseline, not just the test gate.** The throw at `:235-237` precedes the post-processing at `:339-343`, so on a non-zero exit the emitted `coverage\coverage.cobertura.xml` is the RAW document: absolute `class/@filename`, no root `lines-covered`/`lines-valid`. A "record the failure and continue" tolerance branch must say how the baseline coverage numbers are still obtained (dot-source the helpers and run `ConvertTo-KoverageCoberturaXml` in memory) or declare BLOCKED.
- **Anchor a per-file coverage no-regression gate on `lines-valid`, not the rate.** Per-file `lines-covered` drifts by up to four lines between two runs of the same tree (`.claude/agent-memory/orchestrator/coverage-lines-covered-is-nondeterministic.md`), so a near-1.0 baseline rate fails on correct work. Require a reproduction run before failing.

See [[declaration-only-seam-task-for-fail-before]] (not needed here — the test compiles against unmodified production code, so the red is runtime), [[trx-needs-resultsdirectory]], [[absolute-counts-in-shared-files-go-stale]].
