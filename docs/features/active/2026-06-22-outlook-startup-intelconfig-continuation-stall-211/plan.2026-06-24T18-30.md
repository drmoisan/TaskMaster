# Issue #211 — PostLoad / LoadInboxes attribution probe (Plan)

- Feature folder: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/`
- Work Mode: full-bug
- Plan timestamp: 2026-06-24T18-30
- Scope: diagnosis-only, behavior-preserving instrumentation of the ~121 s PostLoad STA freeze that remains AFTER the confirmed AC10 fix. NO behavior fix.
- New acceptance criterion introduced by this plan: AC19.

## Objective

Pinpoint the exact COM call that causes the ~121 s STA freeze in the PostLoad window
(full-lifetime heartbeat `gapMs=120774` after "Finished loading globals"), occurring during
`AppEvents.PerformReadinessHookup()` -> `Globals.Ol.Inboxes` -> `AppOlObjects.LoadInboxes()`.
Add two diagnosis-only probes:

1. `PerformReadinessHookup` per-step START/END markers (`AppEvents.cs:215-241`) so the last
   START with no matching END before the freeze names the blocking operation of the three
   (`ToDoFolder.Items`, `OlReminders`, `Inboxes`).
2. `LoadInboxes` per-store attribution (`AppOlObjects.cs:98-139`) so the emitted line names
   which store blocks and whether the block is in `ShouldIncludeStore` (FilePath read) or
   `store.GetDefaultFolder(olFolderInbox)`.

## Verified context (from delegation + repo read)

- AC10 fix (direct-nav for JunkCertain/JunkPotential) is confirmed working
  (`evidence/other/runtime-capture-ac10-confirmed-postload-loadinboxes-2026-06-24T20-17.md`).
- Current line counts (Phase 0 must re-confirm before any edit):
  - `TaskMaster/AppGlobals/AppEvents.cs` = 499 lines.
  - `TaskMaster/AppGlobals/AppOlObjects.cs` = 424 lines.
  - All touched files (production AND test) must remain <= 500 lines; extract if an edit would exceed.
- Coverable formatter precedent: `TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs` is NOT
  `[ExcludeFromCodeCoverage]` and holds pure line-formatting; this plan reuses that pattern.
- `WrappedMSProvider::Logon` + address-book churn occurs in the PostLoad window. `AppEvents.cs`
  and `AppOlObjects.cs` are NOT `[ExcludeFromCodeCoverage]` as written; the pure formatter/aggregator
  logic must be coverable; the Stopwatch + live COM calls stay in `AppEvents`/`AppOlObjects`.
- Legacy non-SDK csproj wiring: `TaskMaster/TaskMaster.csproj` and
  `TaskMaster.Test/TaskMaster.Test.csproj` use explicit `<Compile Include>` items (no glob). Any new
  `.cs` file MUST be wired with a `<Compile Include>` item or it will not compile.
- `StoresWrapper.ShouldIncludeStore(Outlook.Store store)` returns `bool`; the COMException handling
  and rethrow logic in `LoadInboxes` must be preserved EXACTLY.

## Evidence location

All evidence under `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/<kind>/`.
EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied; delegation used canonical `evidence/` sub-paths.
Baseline coverage -> `evidence/baseline/`; final-QC and post-change coverage -> `evidence/qa-gates/`;
maintainer runtime capture -> `evidence/other/`.

## Hard constraints (apply to every task)

- Behavior-preserving; diagnosis-only. Do NOT change the included-store set, the inbox-subscription
  behavior, the COMException rethrow, or phase semantics.
- Stopwatch only. Banned APIs: `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`,
  `Task.Delay`. Target net48.
- Do NOT touch FolderTree, the JunkFolderPathNavigator AC10 fix, `PreserveReferencesHandling`, or any
  existing instrumentation lines (`[Startup timing]`, `[ui-heartbeat]`, `[gc-delta]`, `[phase-net]`,
  `[startup-lifetime-heartbeat]`, `Hook complete`).
- C# toolchain order is mandatory per implementation task and at final QA:
  CSharpier -> .NET analyzers -> nullable/TreatWarningsAsErrors -> MSTest with coverage
  (`/TestCaseFilter:"TestCategory!=LiveOutlook"`). Restart from CSharpier if any step changes files or fails.

## Scope-lock (files this plan may create or modify)

- CREATE `TaskMaster/AppGlobals/StartupInboxAttributionProbe.cs` (new coverable formatter/aggregator;
  NOT `[ExcludeFromCodeCoverage]`).
- CREATE `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` (partial-class extraction of a cohesive,
  self-contained `AppEvents` region to create file-size headroom; behavior-preserving, no logic change).
- MODIFY `TaskMaster/TaskMaster.csproj` (add both new `<Compile Include>` items to the `AppGlobals`
  item group: `<Compile Include="AppGlobals\StartupInboxAttributionProbe.cs" />` (the probe helper) and
  `<Compile Include="AppGlobals\AppEvents.ReadinessHookup.cs" />` (the `AppEvents` partial)).
- MODIFY `TaskMaster/AppGlobals/AppEvents.cs` (make `AppEvents` `partial`; per-step START/END markers in `PerformReadinessHookup`).
- MODIFY `TaskMaster/AppGlobals/AppOlObjects.cs` (per-store attribution emission + smallest seam over store enumeration).
- CREATE `TaskMaster.Test/AppGlobals/StartupInboxAttributionProbeTests.cs` (new deterministic MSTest).
- MODIFY `TaskMaster.Test/TaskMaster.Test.csproj` (add `<Compile Include="AppGlobals\StartupInboxAttributionProbeTests.cs" />`).
- MODIFY this plan file and `docs/.../issue.md`/`spec.md` AC tables only as required.

## Acceptance criterion AC19

- [x] AC19: `AppEvents.PerformReadinessHookup` emits a START and an END (`Stopwatch` F2 ms) marker for
  each of its three COM operations (`[readiness-hookup] step=ToDoFolder.Items|OlReminders|Inboxes start|end`),
  and `AppOlObjects.LoadInboxes` emits one `[loadinboxes]` line per enumerated store with the guarded
  store DisplayName, the `ShouldIncludeStore` `Stopwatch` ms, the include/exclude result, and (only when
  included) the `GetDefaultFolder(olFolderInbox)` `Stopwatch` ms. Behavior-preserving (included-store set,
  inbox-subscription behavior, COMException rethrow, and phase semantics unchanged); the existing
  `Hook complete` line is retained. Pure formatting/aggregation lives in a coverable helper
  (`StartupInboxAttributionProbe`, NOT `[ExcludeFromCodeCoverage]`) covered by deterministic MSTest
  (MSTest + Moq + FluentAssertions; no live COM/timer/filesystem/network; no temporary files), the new
  helper meets the >= 90% new-code coverage target, and there is no repository-wide coverage regression.
  `Stopwatch` only; no banned API; net48; all touched files <= 500 lines. Full C# toolchain passes in order.

---

### Phase 0 — Baseline capture

- [x] [P0-T1] Read policy files in required order and write `evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Acceptance: the artifact exists with all three required fields populated.
- [x] [P0-T2] Capture current file-size baselines to `evidence/baseline/filesize-baseline-2026-06-24T18-30.md` with `Timestamp:`, `Command:` (line-count command for the four scope files), `EXIT_CODE:`, and `Output Summary:` recording the line counts of `TaskMaster/AppGlobals/AppEvents.cs` (expect 499), `TaskMaster/AppGlobals/AppOlObjects.cs` (expect 424), and confirming `TaskMaster/AppGlobals/StartupInboxAttributionProbe.cs` and `TaskMaster.Test/AppGlobals/StartupInboxAttributionProbeTests.cs` do not yet exist. Acceptance: artifact exists with all four schema fields and the three counts/non-existence facts recorded.
- [x] [P0-T3] Capture csproj-wiring baseline to `evidence/baseline/csproj-wiring-baseline-2026-06-24T18-30.md` with `Timestamp:`, `Command:` (grep for `<Compile Include>` in `TaskMaster/TaskMaster.csproj` and `TaskMaster.Test/TaskMaster.Test.csproj`), `EXIT_CODE:`, and `Output Summary:` confirming both projects use explicit `<Compile Include>` (no glob) and that no `StartupInboxAttributionProbe` items exist yet. Acceptance: artifact exists with all four schema fields recording the explicit-include fact.
- [x] [P0-T4] Capture CSharpier formatting baseline: run `dotnet tool run csharpier --check .` and write `evidence/baseline/csharpier-baseline-2026-06-24T18-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (clean/needs-format status). Acceptance: artifact exists with all four schema fields.
- [x] [P0-T5] Capture analyzer baseline: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `evidence/baseline/analyzers-baseline-2026-06-24T18-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, diagnostic count). Acceptance: artifact exists with all four schema fields.
- [x] [P0-T6] Capture nullable/TreatWarningsAsErrors baseline: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `evidence/baseline/nullable-baseline-2026-06-24T18-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, warning-as-error count). Acceptance: artifact exists with all four schema fields.
- [x] [P0-T7] Capture test + coverage baseline: run `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"` and write `evidence/baseline/tests-coverage-baseline-2026-06-24T18-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording numeric headline coverage values (baseline repository-wide line-coverage percent and pass/fail counts). Acceptance: artifact exists with all four schema fields and a numeric baseline coverage percent (not a placeholder).

---

### Phase 1 — Coverable attribution helper

- [x] [P1-T1] Create `TaskMaster/AppGlobals/StartupInboxAttributionProbe.cs` with a public, sealed, NON-`[ExcludeFromCodeCoverage]` class holding an injected `Action<string>` sink (mirroring `StartupDiagnosticsProbe`), and add one pure method `FormatReadinessHookupStart(string step)` returning the exact `[readiness-hookup] step=<step> start` line and `FormatReadinessHookupEnd(string step, double elapsedMs)` returning `[readiness-hookup] step=<step> end elapsedMs=<F2 invariant>`. Acceptance: file exists, compiles into `TaskMaster` (csproj wired in P1-T2), formats both line shapes with `CultureInfo.InvariantCulture`, uses no `Stopwatch`/`GC`/banned API, and file is <= 500 lines.
- [x] [P1-T2] Add `<Compile Include="AppGlobals\StartupInboxAttributionProbe.cs" />` to `TaskMaster/TaskMaster.csproj` in the existing `AppGlobals` `<Compile Include>` item group. Acceptance: the csproj contains exactly one new `<Compile Include>` item for the file and the project builds (verified by P1-T6 toolchain).
- [x] [P1-T3] Add to `StartupInboxAttributionProbe` the pure per-store formatter `FormatLoadInboxesStore(string displayName, double shouldIncludeMs, bool included, double? getDefaultFolderMs)` returning a single `[loadinboxes] store=<displayName> shouldIncludeMs=<F2> included=<bool> getDefaultFolderMs=<F2 or n/a>` line, where `getDefaultFolderMs` is rendered only when `included` is true (otherwise `getDefaultFolderMs=n/a`). DisplayName is emitted verbatim from the caller (caller is responsible for the guarded read). Acceptance: method returns the exact documented line shape for the included case and the excluded case, with `CultureInfo.InvariantCulture` F2 numeric formatting; no `Stopwatch`/banned API.
- [x] [P1-T4] Add a public `EmitReadinessHookupStart(string step)`/`EmitReadinessHookupEnd(string step, double elapsedMs)`/`EmitLoadInboxesStore(...)` set on `StartupInboxAttributionProbe` that call the corresponding `Format*` method and pass the result to the injected sink, so the call sites in `AppEvents`/`AppOlObjects` only supply measured values. Acceptance: each Emit method invokes the sink exactly once with the corresponding Format output; no other side effects.
- [x] [P1-T5] Create `TaskMaster.Test/AppGlobals/StartupInboxAttributionProbeTests.cs` with deterministic MSTest (`[TestClass]`/`[TestMethod]`, FluentAssertions, Moq where a sink/mock is useful) covering: (a) `FormatReadinessHookupStart`/`End` line shapes for all three step names; (b) `FormatLoadInboxesStore` included case (with `getDefaultFolderMs`) and excluded case (`getDefaultFolderMs=n/a`); (c) each `Emit*` method calls the sink exactly once with the expected line; (d) invariant-culture formatting (e.g., a fractional ms value renders with a dot). No live COM, no live timer, no network/filesystem, no temporary files. Acceptance: tests exist, are independent and deterministic, and assert exact emitted strings.
- [x] [P1-T6] Add `<Compile Include="AppGlobals\StartupInboxAttributionProbeTests.cs" />` to `TaskMaster.Test/TaskMaster.Test.csproj` adjacent to the existing `AppGlobals\StartupDiagnosticsProbeTests.cs` item, then run the C# toolchain in order (CSharpier -> analyzers -> nullable/TWAE -> MSTest with coverage `/TestCaseFilter:"TestCategory!=LiveOutlook"`) and confirm the new tests pass. Acceptance: csproj wired, all four toolchain steps pass in a single clean pass, and the new `StartupInboxAttributionProbe`/tests build and execute.

---

### Phase 2 — PerformReadinessHookup per-step markers

- [x] [P2-T1] Make `AppEvents` a partial class to create file-size headroom (AppEvents.cs is 499 lines; the PerformReadinessHookup markers added in P2-T2..P2-T4 cannot fit under 500). Change `public class AppEvents : IAppEvents` to `public partial class AppEvents : IAppEvents` in `TaskMaster/AppGlobals/AppEvents.cs`, then relocate a cohesive, self-contained region that does NOT touch `PerformReadinessHookup`, the existing instrumentation lines, or the readiness/Hook logic (e.g., the `Unhook`/`LogAsync`/`OlInboxItems_ItemAdd` helper block, executor's choice) into a new partial file `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` (mirroring the `AppOlObjects` + `AppOlObjects.JunkFolders.cs` partial precedent), carrying the required `using` directives. Add `<Compile Include="AppGlobals\AppEvents.ReadinessHookup.cs" />` to `TaskMaster/TaskMaster.csproj`'s `AppGlobals` `<Compile Include>` item group. HARD GATE: after extraction and BEFORE any P2-T2..P2-T4 edit, `AppEvents.cs` and the new partial file must each be <= 500 lines, with >= ~12 lines headroom in the file that will receive the `PerformReadinessHookup` markers. Behavior byte-equivalent (pure relocation; no logic change). Acceptance: `AppEvents` is `partial`; the extracted region compiles via the wired partial file; both files <= 500 lines; the file that will receive the markers has documented >= ~12 lines headroom.
- [x] [P2-T2] In `TaskMaster/AppGlobals/AppEvents.cs` `PerformReadinessHookup` (lines 215-241), construct one `StartupInboxAttributionProbe` instance with sink `s => logger.Debug(s)`, and emit a START marker (`step=ToDoFolder.Items`) immediately before line 220 (`OlToDoItems = Globals.Ol.ToDoFolder.Items;`) and an END marker with the existing `toDoItemsStopwatch` elapsed ms immediately after line 221. Acceptance: START emitted before the COM read and END (with F2 ms) emitted after `toDoItemsStopwatch.Stop()`; the existing assignment and Stopwatch are unchanged.
- [x] [P2-T3] In the same method, emit a START marker (`step=OlReminders`) before line 224 (`OlReminders = Globals.Ol.OlReminders;`) and an END marker with `remindersStopwatch` elapsed ms after line 225. Acceptance: START before the read, END (F2 ms) after `remindersStopwatch.Stop()`; existing assignment and Stopwatch unchanged.
- [x] [P2-T4] In the same method, emit a START marker (`step=Inboxes`) before line 228 (`Globals.Ol.Inboxes.ForEach(...)`) and an END marker with `inboxSubscribeStopwatch` elapsed ms after line 231. Retain the existing `Hook complete` log line at lines 233-240 unchanged. Acceptance: START before the inbox enumeration/subscription, END (F2 ms) after `inboxSubscribeStopwatch.Stop()`, `Hook complete` line byte-identical to current; the included-store set and subscription behavior are unchanged.
- [x] [P2-T5] Run the C# toolchain in order (CSharpier -> analyzers -> nullable/TWAE -> MSTest with coverage `/TestCaseFilter:"TestCategory!=LiveOutlook"`) and confirm `AppEvents.cs` remains <= 500 lines after the edits (file-size headroom established in [P2-T1]; this task asserts the post-edit count). Acceptance: all four toolchain steps pass in a single clean pass and `AppEvents.cs` line count <= 500.

---

### Phase 3 — LoadInboxes per-store attribution

- [x] [P3-T1] In `TaskMaster/AppGlobals/AppOlObjects.cs` `LoadInboxes` (lines 98-139), introduce the smallest seam needed for deterministic testing of the per-store attribution: extract the per-store attribution emission (guarded DisplayName read, `ShouldIncludeStore` timing, include/exclude result, and included-only `GetDefaultFolder` timing) into an `internal` method that accepts injectable delegates (`Func<bool> shouldInclude`, a `getDefaultFolder` delegate typed to the `MAPIFolder` return of `store.GetDefaultFolder(olFolderInbox)` — i.e. `Func<MAPIFolder>` or keep the existing `(Folder)` cast inside the delegate — and `Func<string> readDisplayName`) and the `StartupInboxAttributionProbe` so a fake store can drive it without live COM. Do NOT change which stores are included, the inbox list result, or the COMException rethrow; preserve the existing `(Folder)inbox` cast and inbox-list result exactly. Acceptance: the extracted method computes and emits one `[loadinboxes]` line per store using `Stopwatch` ms around `shouldInclude()` and (only when included) `getDefaultFolder()`, the delegate type compiles against the `MAPIFolder` return of `GetDefaultFolder`, and `LoadInboxes` still returns the same inbox set with the same COMException handling/rethrow.
- [x] [P3-T2] Wire `LoadInboxes` to call the extracted method for each enumerated store, passing `() => storesWrapper.ShouldIncludeStore(store)`, a `getDefaultFolder` delegate over `store.GetDefaultFolder(OlDefaultFolders.olFolderInbox)` typed to accommodate its `MAPIFolder` return (matching the P3-T1 delegate shape), and a guarded `() => { try { return store.DisplayName; } catch { return "<unavailable>"; } }`, preserving the existing `continue` on exclude, the `inboxes.Add((Folder)inbox)` cast on include (byte-equivalent to the current cast at `AppOlObjects.cs:116`), and the existing `catch (COMException)` transient-vs-permanent branch EXACTLY (lines 119-136). Acceptance: enumeration order, included-store set, inbox list, the `(Folder)inbox` cast, and COMException rethrow logic are byte-equivalent in effect; one `[loadinboxes]` line is emitted per store.
- [x] [P3-T3] Add deterministic MSTest coverage in `TaskMaster.Test/AppGlobals/StartupInboxAttributionProbeTests.cs` (or a sibling test file wired into the csproj if the file would exceed 500 lines) for the extracted per-store attribution method using a fake store via the injectable delegates: (a) an included store emits a line with `included=true` and a `getDefaultFolderMs` value; (b) an excluded store emits `included=false` and `getDefaultFolderMs=n/a` and does NOT invoke `getDefaultFolder`; (c) a DisplayName read that throws yields `store=<unavailable>`; (d) the per-store method does not swallow or alter a thrown COMException from `getDefaultFolder` (rethrow path preserved). No live COM/timer/filesystem/network; no temporary files. Acceptance: tests exist, are deterministic, and assert the emitted line and the no-call/rethrow behaviors.
- [x] [P3-T4] Run the C# toolchain in order (CSharpier -> analyzers -> nullable/TWAE -> MSTest with coverage `/TestCaseFilter:"TestCategory!=LiveOutlook"`) and confirm `AppOlObjects.cs` and the test file remain <= 500 lines (extract if needed). Acceptance: all four toolchain steps pass in a single clean pass; both touched files <= 500 lines.

---

### Phase 4 — Maintainer cold-start capture (runtime evidence, not CI-automatable)

- [x] [P4-T1] Write capture instructions and an evidence placeholder to `evidence/other/runtime-capture-instructions-postload-loadinboxes-2026-06-24T18-30.md` containing: `Timestamp:`, the exact build/run steps for a non-debugger cold start with DebugView/OutputDebugString capture, the expected `[readiness-hookup] step=... start|end` and `[loadinboxes] store=... shouldIncludeMs=... included=... getDefaultFolderMs=...` line patterns to collect during a slow startup, the interpretation rule (the last `[readiness-hookup] ... start` with no matching `end` before the freeze names the blocking operation; the `[loadinboxes]` line whose `shouldIncludeMs` or `getDefaultFolderMs` is multi-second names the blocking store and whether the block is in `ShouldIncludeStore` (FilePath) or `GetDefaultFolder`), and a clearly-marked `PENDING MAINTAINER CAPTURE` placeholder section for the pasted runtime lines. Acceptance: artifact exists with instructions, expected line patterns, the interpretation rule, and a pending placeholder; no claim of captured evidence is recorded until the maintainer fills it.

---

### Phase 5 — Final QA loop and coverage delta

- [x] [P5-T1] Final CSharpier: run `dotnet tool run csharpier --check .` and write `evidence/qa-gates/final-csharpier-2026-06-24T18-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If it changes files, restart the loop from this step. Acceptance: artifact exists with all four schema fields and a clean formatting result.
- [x] [P5-T2] Final analyzers: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `evidence/qa-gates/final-analyzers-2026-06-24T18-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact exists with all four schema fields and a clean build.
- [x] [P5-T3] Final nullable/TreatWarningsAsErrors: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `evidence/qa-gates/final-nullable-2026-06-24T18-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact exists with all four schema fields and zero warnings-as-errors.
- [x] [P5-T4] Final tests + coverage: run `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"` and write `evidence/qa-gates/final-tests-coverage-2026-06-24T18-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording numeric post-change repository-wide line-coverage percent and pass/fail counts. Acceptance: artifact exists with all four schema fields and a numeric post-change coverage percent (not a placeholder); all tests pass.
- [x] [P5-T5] Coverage-delta verification: write `evidence/qa-gates/coverage-delta-2026-06-24T18-30.md` recording baseline coverage (from P0-T7), post-change coverage (from P5-T4), and new/changed-code coverage for `StartupInboxAttributionProbe` and the extracted `AppOlObjects` per-store method. Confirm repository-wide coverage did not regress and new-code coverage meets the >= 90% target. Acceptance: artifact reports all three numeric values; if any required value is unavailable or new-code coverage is below target or repository coverage regressed, the outcome is remediation-required (NOT PASS).
- [x] [P5-T6] AC19 verification: write `evidence/qa-gates/ac19-verification-2026-06-24T18-30.md` mapping AC19's clauses (per-step markers, per-store attribution, behavior preservation, coverable helper, deterministic tests, file-size <= 500, toolchain) to the concrete evidence artifacts and code locations from Phases 1-5. Acceptance: every AC19 clause maps to a passing artifact/location, or the unmet clause is listed as remediation-required.

---

## Preflight / validator note

- Preflight: return this plan for `DIRECTIVE: PREFLIGHT VALIDATION ONLY` through `atomic-executor`; reuse this exact file path across revision iterations.
- Validator gate: run `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"`, `artifact_path: <this file>` before treating the plan as approved. All `### Phase N — <Title>` headings are canonical (em-dash, no parenthetical between `Phase N` and the em-dash); the H1 title line is exempt.
