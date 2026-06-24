# Atomic Plan — AC10: direct-path navigation for JunkCertain / JunkPotential (issue #211)

- Feature: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/`
- Issue: #211
- Work Mode: full-bug
- Plan timestamp: 2026-06-24T17-30
- Scope: FIRST behavior-changing (non-diagnostic) increment for #211. Bugfix workflow: a failing (red-before-green) regression test FIRST, then the minimal targeted direct-navigation fix in BOTH `LoadJunkCertain` and `LoadJunkPotential`. NO opportunistic refactors. Satisfies spec AC10 (the prior "fix target RETRACTED" note is superseded by the confirmed `LoadJunk*` full-`FolderTree`-enumeration root cause documented in the delegation).

## Confirmed Root Cause (do not re-investigate; re-cite exact lines in Phase 0)

- `Globals.Ol.JunkCertain` -> `AppOlObjects.LoadJunkCertain()` builds `new FolderTree(Root).Roots.FirstOrDefault()` (`TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs:103`); `LoadJunkPotential()` has the identical pattern (`AppOlObjects.JunkFolders.cs:56`).
- `Root = App.Session.DefaultStore.GetRootFolder()` (`AppOlObjects.cs:183`).
- `FolderTree(MAPIFolder olRoot)` ctor (`UtilitiesCS/OutlookObjects/Folder/FolderTree.cs:33-38`) calls `RootFromFolder` (`:150-156`) -> `InitializeChildren` (`:185-194`), which RECURSIVELY enumerates the ENTIRE default-store folder hierarchy on the STA (recursion at `:191-192`) BEFORE any search.
- Proven cost (per delegation): JunkCertain = 50,172 ms cold vs direct `DefaultStore.GetDefaultFolder(Inbox)` = 4.4 ms on the same store; model deserialize 0.9 ms.

## Matching Semantics Studied (REPRODUCE EXACTLY; cited for the executor)

The direct-navigation replacement MUST resolve the IDENTICAL folder for valid configured paths. A wrong folder would misroute spam/junk email. The matching semantics of the current code are:

1. `sequence = folderPath.Split('\\')` then `new Queue<string>(...)` (`AppOlObjects.JunkFolders.cs:62,109`). `folderPath` is the stored `RelativePath` (root prefix stripped — see point 5).
2. Comparator passed to `FindSequentialNode` is `(current, other) => current.Name == other` (`AppOlObjects.JunkFolders.cs:64,111`) — ordinal `string ==` on `FolderWrapper.Name`. Case-SENSITIVE; NO trimming; NO culture.
3. `FolderWrapper.Name` = `OlFolder.Name` (`FolderWrapper .cs:176-182`, `LoadName() => OlFolder?.Name`).
4. `TreeNode<T>.FindSequentialNode` (`UtilitiesCS/ReusableTypeClasses/Other/TreeNodeOfT.cs:149-160`):
   - FIRST segment: `FindNode(comparator, descendByLevel: true)` (`:153`) — a BREADTH-FIRST level-by-level search starting AT THE ROOT NODE ITSELF (`FindNode` `:162-185`, BFS via `GetNextLevel` `:194-204`). The root node's `Value` is `FolderWrapper(olRoot, olRoot)` whose `.Name` is the root folder's own name, so the BFS will match the root first if the first segment equals the root's name; otherwise it descends level by level and returns the FIRST node (shallowest, breadth-first order) whose `.Name` equals the first segment.
   - SUBSEQUENT segments: `node = node.Children?.Where(x => comparator(x.Value, next))?.FirstOrDefault()` (`:157`) — matched ONLY against the matched node's DIRECT children, first match wins.
   - Returns `null` if any segment fails to match.
5. `FolderWrapper.RelativePath` (the stored setting) is composed by `LoadRelativePath()` (`FolderWrapper .cs:193-223`): when `OlFolder.FolderPath` contains `OlRoot.FolderPath`, it is `OlFolder.FolderPath.Replace(OlRoot.FolderPath + "\\", "")` (`:221`) — i.e. the path BELOW the root, root prefix removed. Therefore the FIRST stored segment is normally a DIRECT CHILD of root; `FindSequentialNode`'s BFS-from-root-for-the-first-segment will (for a normal relative path) match that direct child at level 1.

### Direct-navigation equivalence contract (binding)

Reproduce semantics 1-5 EXACTLY using the pure helper (Design point 1):
- Split the configured path on `'\\'` into segments verbatim (do not trim, do not drop empty segments beyond what `Split` produces).
- For the FIRST segment: reproduce `FindNode(descendByLevel:true)` BFS-from-root. Match the root node first (compare first segment to root `Name` ordinally); if no match, BFS its descendants level by level and take the first ordinal `Name ==` match. (For the normal stored-relative-path case this resolves the level-1 child; the BFS-from-root is preserved so resolution is byte-identical to the current code for ALL inputs, including degenerate ones.)
- For each SUBSEQUENT segment: match ONLY direct children of the current node by ordinal `Name ==`, first match wins.
- Any unmatched segment -> not found (`null`).
- Comparison is ordinal `string ==`, case-sensitive, no trimming — identical to the existing comparator.

This is the single correctness-critical invariant of this increment; it is encoded by the regression tests in Phase 2.

## Design (binding decisions)

1. **Pure path-navigation helper** — `TaskMaster/AppGlobals/JunkFolderPathNavigator.cs` (new; NOT `[ExcludeFromCodeCoverage]`). A pure, COM-free static class operating over a narrow abstraction `IFolderNode` (a name + its direct child folders by name) so it is deterministically unit-testable with MSTest + Moq + FluentAssertions, no live COM/timer/network/filesystem, no temporary files.
   - `internal interface IFolderNode { string Name { get; } IReadOnlyList<IFolderNode> ChildFolders { get; } }`
   - `internal static IFolderNode ResolvePath(IFolderNode root, string relativePath)` — returns the matched node or `null`, reproducing semantics 1-5 above (BFS-from-root for the first segment, direct-child match for subsequent segments). Touches ONLY the folders along the resolution path plus the breadth-first frontier required for the first-segment match — NOT every node in the tree.
   - The helper exposes `Split`/first-segment-BFS/child-walk as small internal methods so the per-segment matching is directly testable.
2. **COM adapter (thin, kept in the COM-bound seam, `[ExcludeFromCodeCoverage]`)** — a private adapter in `AppOlObjects.JunkFolders.cs` (or a small co-located private nested type) that wraps a live `MAPIFolder` as `IFolderNode`, exposing `Name => _olFolder.Name` and lazily enumerating ONLY `_olFolder.Folders` on demand (no recursion, no eager full-tree walk). The adapter is `[ExcludeFromCodeCoverage]` because it is a direct COM wrapper with no testable logic; the navigation logic it feeds is fully covered in the pure helper.
3. **`LoadJunkCertain` / `LoadJunkPotential` rewrite (minimal)** — replace `new FolderTree(Root).Roots.FirstOrDefault()` + `FindSequentialNode(...)` with: wrap `Root` in the COM adapter, call `JunkFolderPathNavigator.ResolvePath(adapter, folderPath)`, and project the matched node back to its live `Folder`. PRESERVE VERBATIM: the `folderPath.IsNullOrEmpty()` early `return null`; the `MyBox.ShowDialog(...)` prompt; `NamespaceMAPI.PickFolder()`; the `FolderWrapper` + `WriteJunk*Setting(wrapper.RelativePath)` + `Properties.Settings.Default.Save()` fallback. Keep ALL existing diagnostic instrumentation (`[spam-init]`, `[phase-net]`, etc.) intact.
4. **Do NOT change `FolderTree`** — the cached/cooperative-yield/refreshable `FolderTree` is a separate issue. Only the two `LoadJunk*` resolution sites are changed to bypass it.
5. **Legacy csproj wiring** — `TaskMaster.csproj` and `TaskMaster.Test.csproj` are legacy `packages.config` (non-SDK) projects with explicit `<Compile Include>` items and NO glob (see memory: legacy-csproj-explicit-compile-include). Every new `.cs` file requires an explicit `<Compile Include>` entry or it will not compile.
6. net48. Banned APIs (BannedApiAnalyzers, RS0030): `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay` — none introduced. All touched/new files (production AND test) <= 500 lines. MSTest + Moq + FluentAssertions.

## Evidence Location Invariant

All evidence MUST be written under `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/<kind>/` per `evidence-and-timestamp-conventions`. Non-canonical `artifacts/...` evidence paths are forbidden.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in required order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and write `evidence/baseline/phase0-instructions-read-2026-06-24T17-30.md` with `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [x] [P0-T2] Re-cite confirmed root cause and matching semantics from source: record the exact line numbers verified — `AppOlObjects.JunkFolders.cs:56,62,64,103,109,111`; `AppOlObjects.cs:183`; `FolderTree.cs:33-38,150-156,185-194`; `TreeNodeOfT.cs:149-160,162-185,194-204`; `FolderWrapper .cs:176-182,193-223`. Write `evidence/baseline/baseline-rootcause-citations-2026-06-24T17-30.md` with `Timestamp:` and the verified file:line list.
- [x] [P0-T3] Capture file-size baseline for `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` (expected 133 lines), the new `JunkFolderPathNavigator.cs` (expected absent), and the new test file (expected absent). Write `evidence/baseline/baseline-file-size-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (per-file line counts).
- [x] [P0-T4] Capture csproj-wiring baseline: confirm `TaskMaster.csproj` uses explicit `<Compile Include>` (cite `:407-408` for `AppOlObjects.cs` / `AppOlObjects.JunkFolders.cs`) and `TaskMaster.Test.csproj` uses explicit `<Compile Include>` (27 occurrences), both no-glob. Write `evidence/baseline/baseline-csproj-wiring-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T5] Capture CSharpier baseline: run `dotnet tool run csharpier --check .`. Write `evidence/baseline/baseline-csharpier-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T6] Capture analyzer baseline: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `evidence/baseline/baseline-analyzers-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T7] Capture nullable/TWAE baseline: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `evidence/baseline/baseline-nullable-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T8] Capture MSTest + coverage baseline: run `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`. Write `evidence/baseline/baseline-tests-coverage-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including numeric headline values: total tests passed, repo-wide line coverage percent, and `TaskMaster.dll` coverage percent (the assembly receiving the new helper).

---

### Phase 1 — New Coverable Navigation Helper (no behavior change yet)

- [x] [P1-T1] Create `TaskMaster/AppGlobals/JunkFolderPathNavigator.cs` defining `internal interface IFolderNode { string Name { get; } System.Collections.Generic.IReadOnlyList<IFolderNode> ChildFolders { get; } }` and an empty (to be filled in P1-T2) `internal static class JunkFolderPathNavigator`. Acceptance: file exists, compiles, NOT decorated `[ExcludeFromCodeCoverage]`, <= 500 lines.
- [x] [P1-T2] Implement `JunkFolderPathNavigator.ResolvePath(IFolderNode root, string relativePath)` and its internal sub-methods reproducing matching semantics 1-5 exactly: split on `'\\'` verbatim; FIRST segment via BFS-from-root (root matched first, then level-by-level descendants, first ordinal `Name ==` match); SUBSEQUENT segments via direct-child ordinal `Name ==` first-match; unmatched segment -> `null`. Implementation touches only the resolution path plus the first-segment BFS frontier, never an eager full-tree walk. Acceptance: method present with the exact signature; no banned APIs; no COM/IO; XML doc comment states the equivalence contract.
- [x] [P1-T3] Add an explicit `<Compile Include="AppGlobals\JunkFolderPathNavigator.cs" />` entry to `TaskMaster/TaskMaster.csproj` in the existing source `<ItemGroup>` adjacent to the `AppOlObjects` entries (`:407-408`). Acceptance: entry present; project compiles with the new file.

---

### Phase 2 — Red-Before-Green Regression Test (failing first)

- [x] [P2-T1] Create `TaskMaster.Test/AppGlobals/JunkFolderPathNavigatorTests.cs` with a deterministic fake `IFolderNode` hierarchy (in-memory, no COM) that exposes a COUNTER incremented each time a node's `ChildFolders` is enumerated. Acceptance: file exists, `[TestClass]`, uses MSTest + FluentAssertions (+ Moq where useful), no live COM/timer/network/filesystem, no temporary files, <= 500 lines.
- [x] [P2-T2] Add an explicit `<Compile Include="AppGlobals\JunkFolderPathNavigatorTests.cs" />` entry to `TaskMaster.Test/TaskMaster.Test.csproj`. Acceptance: entry present; test project compiles.
- [x] [P2-T3] `[expect-fail]` Author the defect-encoding regression test `ResolvePath_AccessesOnlyFoldersAlongThePath_NotEntireTree`: build a wide+deep fake tree, resolve a valid path, and assert the child-enumeration counter is O(path depth + first-segment BFS frontier) and strictly LESS than the count required to enumerate every node in the tree. NOTE: this test targets `JunkFolderPathNavigator.ResolvePath`, which does not exist with navigation behavior until Phase 1 is complete; to capture the "red" against the ORIGINAL full-enumeration behavior, P2-T4 runs a temporary equivalent harness over the current `FolderTree`/`FindSequentialNode` path. Acceptance: test method present, tagged `[expect-fail]` in the plan, asserting the enumeration-bound invariant.
- [x] [P2-T4] `[expect-fail]` Capture the RED run: execute the enumeration-bound assertion against the CURRENT full-`FolderTree`-enumeration behavior (run the assertion harness over `new FolderTree(fakeRoot)`-equivalent eager enumeration, or temporarily point the test at the legacy path) and confirm it FAILS because the legacy path enumerates the entire tree. Write `evidence/regression-testing/red-run-enumeration-bound-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), and `Output Summary:` showing the failing assertion (eager enumeration count >> path-bound count). This is the fail-before artifact required by the bugfix workflow.
- [x] [P2-T5] Add correctness tests to `JunkFolderPathNavigatorTests.cs`: (a) valid single-segment path resolves the correct direct child; (b) valid nested multi-segment path resolves the correct deep folder; (c) case-sensitivity — a path differing only in case does NOT match (ordinal `==`); (d) first-segment-equals-root-name resolves the root (BFS-from-root parity); (e) unmatched segment returns `null`. Acceptance: five tests present, each Arrange-Act-Assert with clear FluentAssertions messages.

---

### Phase 3 — Minimal Direct-Navigation Fix (the green)

- [x] [P3-T1] In `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs`, add a private `[ExcludeFromCodeCoverage]` COM adapter that wraps a live `MAPIFolder` as `IFolderNode` (`Name => _olFolder.Name`; `ChildFolders` lazily enumerates ONLY `_olFolder.Folders` on demand, no recursion). Acceptance: adapter present; decorated `[ExcludeFromCodeCoverage]`; no eager full-tree walk; file <= 500 lines.
- [x] [P3-T2] Rewrite `LoadJunkCertain` (`AppOlObjects.JunkFolders.cs:101-131`): replace `new FolderTree(Root).Roots.FirstOrDefault()` + `FindSequentialNode(...)` with `JunkFolderPathNavigator.ResolvePath(<adapter over Root>, folderPath)` projecting the matched node to its live `Folder`. PRESERVE VERBATIM the `IsNullOrEmpty()` early return, `MyBox.ShowDialog(...)`, `NamespaceMAPI.PickFolder()`, `FolderWrapper` + `WriteJunkCertainSetting(wrapper.RelativePath)` + `Save()` fallback, and all diagnostic instrumentation. Acceptance: no `FolderTree` reference remains in `LoadJunkCertain`; fallback block byte-identical to original; null/empty early return unchanged.
- [x] [P3-T3] Rewrite `LoadJunkPotential` (`AppOlObjects.JunkFolders.cs:54-84`) with the identical direct-navigation substitution and the identical verbatim fallback preservation (`WriteJunkPotentialSetting`). Acceptance: no `FolderTree` reference remains in `LoadJunkPotential`; fallback block byte-identical to original; null/empty early return unchanged.
- [x] [P3-T4] Point the regression test `ResolvePath_AccessesOnlyFoldersAlongThePath_NotEntireTree` at the production `JunkFolderPathNavigator.ResolvePath` (remove the temporary legacy harness used for the red capture). Acceptance: the test now exercises the production helper.
- [x] [P3-T5] Capture the GREEN run: execute the full `JunkFolderPathNavigatorTests` suite (including the enumeration-bound test and the five correctness tests). Write `evidence/regression-testing/green-run-enumeration-bound-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:` (zero), and `Output Summary:` showing the enumeration-bound test now PASSES (path-bound count) and all correctness tests pass. Acceptance: all listed tests pass; the previously-failing assertion is green.

---

### Phase 4 — Maintainer Cold-Start Re-Capture (runtime, evidence-gated)

- [x] [P4-T1] Write AC9-style non-debugger cold-start re-capture INSTRUCTIONS at `evidence/other/ac10-coldstart-junk-navigation-recapture-instructions-2026-06-24T17-30.md`: steps to perform a non-debugger cold start, locate the `[spam-init] ValidatePathsSet.JunkCertain` / `ValidatePathsSet.JunkPotential` `Stopwatch` ms lines, and confirm JunkCertain/JunkPotential resolution no longer blocks (expected single-digit-to-low-double-digit ms, comparable to the 4.4 ms direct `GetDefaultFolder` reference, versus the proven ~50,172 ms). Include `Timestamp:` and the exact log tags to inspect.
- [x] [P4-T2] Create the maintainer-gated placeholder `evidence/other/runtime-capture-ac10-junk-navigation-PLACEHOLDER.md` with `Timestamp:`, a `MAINTAINER-GATED (runtime, not CI-automatable)` header, the expected pass condition (JunkCertain + JunkPotential resolution each well under the 5000 ms threshold and no full-tree enumeration), and a pointer to the instructions file. Acceptance: placeholder present; clearly marked pending maintainer capture.

---

### Phase 5 — Final QA Loop (full toolchain, in order; restart from CSharpier on any change)

- [x] [P5-T1] CSharpier: run `dotnet tool run csharpier .`. If it changes files, restart the loop from this task. Write `evidence/qa-gates/final-qc-csharpier-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P5-T2] Analyzers: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `evidence/qa-gates/final-qc-analyzers-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P5-T3] Nullable/TWAE: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `evidence/qa-gates/final-qc-nullable-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P5-T4] MSTest + coverage: run `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`. Write `evidence/qa-gates/final-qc-tests-coverage-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including numeric post-change repo-wide line coverage percent, `TaskMaster.dll` percent, and total tests passed.
- [x] [P5-T5] Coverage delta + new-code threshold: compute and record baseline coverage (from P0-T8), post-change coverage (from P5-T4), and NEW/CHANGED-code coverage for `JunkFolderPathNavigator.cs` (target `>= 90%`) and the changed lines in `AppOlObjects.JunkFolders.cs` (no regression on changed lines; the `[ExcludeFromCodeCoverage]` adapter is excluded). Write `evidence/qa-gates/final-qc-coverage-delta-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` listing baseline %, post-change %, new-code %; FAIL the plan (remediation-required) if new-code < 90% or repo-wide regresses.
- [x] [P5-T6] File-size gate: confirm `AppOlObjects.JunkFolders.cs`, `JunkFolderPathNavigator.cs`, and `JunkFolderPathNavigatorTests.cs` are each <= 500 lines. Write `evidence/qa-gates/final-qc-filesize-2026-06-24T17-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (per-file line counts).
- [x] [P5-T7] AC10 check-off: per `acceptance-criteria-tracking`, record the automated-portion verification of spec AC10 (direct-navigation fix in both `LoadJunk*` sites + coverable nav helper + red-before-green regression evidence + full toolchain pass in order) and mark the runtime portion maintainer-gated (P4-T2 placeholder). Write `evidence/qa-gates/p5-acceptance-criteria-checkoff-2026-06-24T17-30.md` with `Timestamp:` and the AC10 status table.
