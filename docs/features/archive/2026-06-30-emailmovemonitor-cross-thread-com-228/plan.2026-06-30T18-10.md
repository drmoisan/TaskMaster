# Atomic Implementation Plan — EmailMoveMonitor Cross-Thread COM (Issue #228)

- Issue: #228
- Feature folder: `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228`
- Work Mode: full-bug (from `issue.md` metadata)
- Authoritative requirements: `spec.md` (AC1–AC9), `issue.md`
- Research: `artifacts/research/2026-06-30T00-00-00Z-emailmovemonitor-cross-thread-com-research.md`
- Plan created: 2026-06-30T18-10
- Last revised: 2026-06-30T18-10 — incorporated atomic-executor preflight delta (P2-T1 `UiThread` namespace correction from `UtilitiesCS.Threading` to `UtilitiesCS`; citation fixes for P6-T1 `TryUnhookOrReplace` `:18-53` and P5-T3 commented-out `UnhookItemAsync` block `:78-101`)
- Supersedes seeded `plan.2026-06-30T17-52.md`

## Evidence Location Invariant

All evidence artifacts produced by this plan MUST be written under
`docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/<kind>/`
per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Writing to
`artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any other
non-canonical path is a policy violation. No task in this plan may redirect evidence
to a non-canonical location.

## Acceptance Criteria Mapping (from spec.md)

- AC1 — All Outlook COM access in `EmailMoveMonitor` marshaled to captured STA thread → P2, P3, P4, P5
- AC2 — Redundant `Task.Run` unhook wrapper removed; returned-node behavior unchanged → P6
- AC3 — `IEmailMoveMonitor` interface + injectable marshal-to-STA delegate; tests substitute pass-through → P2, P3, P7
- AC4 — Regression/unit tests added and passing (hook/unhook bookkeeping, `UnhookItem(null)`, shared-folder counting, COM-only-via-delegate) → P7, P8
- AC5 — Changed `EmailMoveMonitor` bookkeeping >=90%; repo-wide >=80% (testable denominator); exemption documented/scoped → P9, P10
- AC6 — No banned-API regressions; `TimeProvider.Delay` preserved → P3, P5, P6, P9
- AC7 — No out-of-scope behavior changes; log4net logging in `TryUnhookOrReplace` preserved → P6, P9
- AC8 — Full toolchain pass in order (csharpier → analyzers → nullable → MSTest w/ coverage), clean final pass → P9
- AC9 — Spec/issue references updated to reflect implemented behavior → P10

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in required order and record the read evidence to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/baseline/phase0-instructions-read.2026-06-30T18-10.md`. The artifact MUST include `Timestamp:`, `Policy Order:`, and an explicit list of files read: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/ci-workflows.md`, `.claude/rules/tonality.md`. Acceptance: artifact exists with all three required fields populated.
- [x] [P0-T2] Capture baseline format state by running `dotnet tool run csharpier --check .` from repo root and record to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/baseline/baseline-csharpier.2026-06-30T18-10.md`. Artifact MUST include `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact exists with all four fields.
- [x] [P0-T3] Capture baseline analyzer state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/baseline/baseline-analyzers.2026-06-30T18-10.md`. Artifact MUST include `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail plus warning/error counts). Acceptance: artifact exists with all four fields.
- [x] [P0-T4] Capture baseline nullable type-check state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/baseline/baseline-nullable.2026-06-30T18-10.md`. Artifact MUST include `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact exists with all four fields.
- [x] [P0-T5] Capture baseline test + coverage state by running `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` and record to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/baseline/baseline-tests-coverage.2026-06-30T18-10.md`. Artifact MUST include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with numeric headline values: total tests passed/failed and repo-wide line coverage percent. Acceptance: artifact exists with all four fields and a numeric coverage percent (not a placeholder).
- [x] [P0-T6] Record the pre-change `EmailMoveMonitor` coverage denominator to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/baseline/baseline-emailmovemonitor-coverage.2026-06-30T18-10.md`. Artifact MUST capture the current line-coverage percent for `QuickFiler\Helper Classes\EmailMoveMonitor.cs` from the P0-T5 coverage report (research §6 confirms zero existing tests; expected near 0%). Acceptance: artifact exists with `Timestamp:`, source path, and a numeric pre-change coverage percent for the file.

---

### Phase 1 — Define IEmailMoveMonitor Interface

- [x] [P1-T1] Create `QuickFiler\Interfaces\IEmailMoveMonitor.cs` declaring `internal interface IEmailMoveMonitor` with exactly the three production members `void HookItem(MailItem mail, Action<MailItem> moveAction)`, `void UnhookItem(MailItem mail)`, and `void UnhookAll()`, using `Microsoft.Office.Interop.Outlook` types and matching the existing `EmailMoveMonitor` signatures. Include XML doc comments stating each method marshals Outlook COM access to the captured STA thread. Acceptance: file exists, compiles when wired (P1-T2), and the three signatures match `EmailMoveMonitor` exactly.
- [x] [P1-T2] Add `<Compile Include="Interfaces\IEmailMoveMonitor.cs" />` to `QuickFiler\QuickFiler.csproj` in the Interfaces `<ItemGroup>` (legacy packages.config project uses explicit Compile Include; no glob). Acceptance: the csproj contains the new Compile Include entry in the Interfaces item group.

---

### Phase 2 — Add Injectable Marshal-to-STA Delegate Seam

- [x] [P2-T1] In `QuickFiler\Helper Classes\EmailMoveMonitor.cs`, add a `private readonly Action<Action> _marshalToSta;` field and a constructor parameter `Action<Action> marshalToSta = null` to the existing `EmailMoveMonitor()` constructor; default a null argument to a delegate that invokes `UiThread.Dispatcher.Invoke(action)` (mirror the `TimeProvider` default-to-real-implementation style at `QfcDatamodel.cs:109`). Add `using UtilitiesCS;` (the namespace that declares `public static class UiThread`; do NOT use `using UtilitiesCS.Threading;`, which does not contain `UiThread`), or fully-qualify as `UtilitiesCS.UiThread`. Acceptance: constructor compiles; default path resolves to the `UiThread` STA dispatcher; tests can pass a synchronous pass-through `a => a()`.
- [x] [P2-T2] Declare `EmailMoveMonitor : IEmailMoveMonitor` on the class declaration in `QuickFiler\Helper Classes\EmailMoveMonitor.cs`. Acceptance: class compiles implementing the interface; no member signature mismatch.

---

### Phase 3 — Marshal HookItem COM Access

- [x] [P3-T1] In `EmailMoveMonitor.HookItem` (`QuickFiler\Helper Classes\EmailMoveMonitor.cs:29-38`), route the Outlook COM access (`mail.Parent`, `folder.EntryID`, `folder.BeforeItemMove +=`) through `_marshalToSta(...)` so the COM reads and the event subscribe execute on the captured STA thread, while preserving the `lock (_hookedItems)` bookkeeping invariant (subscribe `BeforeItemMove` only when no existing hooked item shares the folder). Do not introduce `Thread.Sleep`/`Task.Delay`/`DateTime.Now`/`DateTime.UtcNow`/`Random.Shared`. Acceptance: all COM member access in `HookItem` occurs inside the marshal delegate body; the first-item-per-folder subscribe rule is preserved.

---

### Phase 4 — Cache Stable EntryID Strings in EmailMoveAction

- [x] [P4-T1] In `EmailMoveAction` (`QuickFiler\Helper Classes\EmailMoveMonitor.cs:171-188`), add `string MailEntryId` and `string FolderEntryId` properties populated at construction from `mail.EntryID` and `folder.EntryID` (captured on the STA thread, since `HookItem` now marshals construction). Keep the existing `Mail`/`Folder`/`MoveAction` members. Acceptance: `EmailMoveAction` exposes the two cached string IDs; values are read once at construction.
- [x] [P4-T2] Update `HookItem` (`QuickFiler\Helper Classes\EmailMoveMonitor.cs`) so the `EmailMoveAction` is constructed inside the STA-marshaled body where `folder.EntryID` and `mail.EntryID` are already being read, ensuring the cached IDs are captured on the STA thread. Acceptance: `EmailMoveAction` construction (and its EntryID reads) occurs inside the marshal delegate body.

---

### Phase 5 — Marshal UnhookItem and UnhookAll; Prefer Cached IDs

- [x] [P5-T1] In `EmailMoveMonitor.UnhookItem` (`QuickFiler\Helper Classes\EmailMoveMonitor.cs:40-59`), preserve the `mail is null` no-op guard, then route the COM-dependent comparison and the `BeforeItemMove -=` unsubscribe through `_marshalToSta(...)`. Compute the live `mail.EntryID`/`(mail.Parent as Folder)?.EntryID` reads inside the marshaled body and compare against the cached `FolderEntryId`/`MailEntryId` from P4-T1. Preserve the count-based rule (unsubscribe only when the removed item is the last for its folder). Acceptance: `UnhookItem(null)` returns without COM access; all remaining COM access occurs inside the marshal delegate; last-item-per-folder unsubscribe rule preserved.
- [x] [P5-T2] In `EmailMoveMonitor.UnhookAll` (`QuickFiler\Helper Classes\EmailMoveMonitor.cs:135-145`), route the per-item `BeforeItemMove -=` unsubscribe loop through `_marshalToSta(...)` while preserving the `lock (_hookedItems)` scope and the `_hookedItems.Clear()` semantics. Acceptance: all `BeforeItemMove -=` calls occur inside the marshal delegate; the list is cleared exactly once.
- [x] [P5-T3] In the dormant `EmailMoveMonitor.UnhookItemAsync` (`:61-87`) and `GetParentFolderAsync` (`:89-133`), apply the same marshal seam to any retained Outlook COM access to the extent these members remain in the file, WITHOUT re-wiring them into the active call path (the commented-out `UnhookItemAsync` block in `QfcDatamodel.QueueProcessing.cs:78-101` stays commented out). If a member's COM access cannot be marshaled without re-activating it, leave the member unchanged and record the decision in the P10 issue-update mirror. Acceptance: no new active caller of `UnhookItemAsync`/`GetParentFolderAsync` is introduced; any retained COM access in them is either marshaled or explicitly left unchanged with a recorded rationale.

---

### Phase 6 — Remove Redundant Task.Run in DequeueNextItemGroupAsync; Migrate Consumers to Interface

- [x] [P6-T1] In `QfcDatamodel.DequeueNextItemGroupAsync` (`QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs:55-114`), remove the `await Task.Run(() => { ... }, _token)` wrapper (lines 70-105) so the `for` loop calling `TryUnhookOrReplace(ref nodes, i)` runs directly; keep the surrounding `try/catch` that logs `"Error unhooking items from move monitor"` via log4net and rethrows, and keep the `return nodes;` behavior. Do not alter `TryUnhookOrReplace` (`:18-53`) per-item retry/replace bookkeeping or its log4net logging. Acceptance: method no longer contains `Task.Run`; returned-node list semantics unchanged; `TryUnhookOrReplace` body unchanged.
- [x] [P6-T2] Change the `_moveMonitor` field type from `EmailMoveMonitor` to `IEmailMoveMonitor` in `QuickFiler\Controllers\QfcDatamodel.cs:100`, keeping the initializer `new EmailMoveMonitor()` (default marshal delegate). Acceptance: field is typed `IEmailMoveMonitor`; construction unchanged; file compiles.
- [x] [P6-T3] Change the `_moveMonitor` field type from `EmailMoveMonitor` to `IEmailMoveMonitor` in `QuickFiler\Controllers\QfcQueue.cs:40`, keeping the initializer `new EmailMoveMonitor()`. Acceptance: field is typed `IEmailMoveMonitor`; construction unchanged; file compiles.
- [x] [P6-T4] Change the `_moveMonitor` field type from `EmailMoveMonitor` to `IEmailMoveMonitor` in `QuickFiler\Controllers\QfcCollectionController.cs:77`, keeping the initializer `new EmailMoveMonitor()`. Acceptance: field is typed `IEmailMoveMonitor`; construction unchanged; file compiles.

---

### Phase 7 — Add Unit Tests for EmailMoveMonitor Bookkeeping

- [x] [P7-T1] Create `QuickFiler.Test\Helper Classes\EmailMoveMonitorTests.cs` as a `[TestClass]` using MSTest, Moq, and FluentAssertions, mocking `Microsoft.Office.Interop.Outlook.MailItem`/`Folder` directly with Moq (precedent: `QfcHomeControllerIterationTests.cs:29-30`). Construct `EmailMoveMonitor` with a synchronous pass-through marshal delegate `a => a()` and a captured-call counter so each test can assert COM access flows only through the delegate. Include an `[TestInitialize]`/`[TestCleanup]` pair that establishes and restores `UiThread.Dispatcher` static state explicitly (research §6; issue #199 precedent) so tests are order-independent. Acceptance: file exists, compiles, and the test class uses the pass-through delegate plus explicit static-state setup/teardown.
- [x] [P7-T2] Add `<Compile Include="Helper Classes\EmailMoveMonitorTests.cs" />` to `QuickFiler.Test\QuickFiler.Test.csproj` (legacy packages.config; explicit Compile Include required). Acceptance: csproj contains the new Compile Include entry.
- [x] [P7-T3] Add a test asserting `HookItem` subscribes `BeforeItemMove` exactly once for the first item of a folder and does NOT re-subscribe for a second item sharing the same folder EntryID (Moq `Verify` on the folder event add, called once). Acceptance: test passes and asserts single subscribe for shared folder.
- [x] [P7-T4] Add a test asserting `UnhookItem` unsubscribes `BeforeItemMove` only when removing the last item for a folder (two items same folder → first `UnhookItem` does not unsubscribe; second does). Acceptance: test passes and asserts last-item-only unsubscribe.
- [x] [P7-T5] Add a test asserting `UnhookItem(null)` is a no-op: no COM access, no marshal-delegate invocation, no change to hooked-item count. Acceptance: test passes; marshal-delegate invocation count is zero for the null call.
- [x] [P7-T6] Add a test asserting the cached-EntryID comparison path: after `HookItem`, `UnhookItem` matches the hooked item using the cached `MailEntryId`/`FolderEntryId` and removes exactly the matching entry. Acceptance: test passes and verifies removal of the correct entry via cached IDs.
- [x] [P7-T7] Add a test asserting all Outlook COM access during `HookItem`/`UnhookItem`/`UnhookAll` occurs only through the injected marshal delegate (use a delegate that records the managed thread id / invocation and assert COM member access count outside the delegate is zero). Acceptance: test passes; proves COM access is delegate-gated. Maps AC1/AC3.
- [x] [P7-T8] Add a test asserting `UnhookAll` unsubscribes every hooked folder's `BeforeItemMove` and clears all bookkeeping state (subsequent `UnhookItem` of a previously hooked item is a no-op). Acceptance: test passes; state fully cleared.
- [x] [P7-T9] Add an edge-case test for duplicate hook of the same item and for unhooking an item that was never hooked (no exception, no spurious unsubscribe). Acceptance: test passes for both negative scenarios.

---

### Phase 8 — Regression Test for Threading Defect

- [x] [P8-T1] Add a test in `QuickFiler.Test\Helper Classes\EmailMoveMonitorTests.cs` that invokes `UnhookItem` from a ThreadPool thread (e.g., inside `Task.Run`) using a marshal delegate that records the thread id on which the COM-access body executes, and asserts the recorded execution thread is the marshal-target thread rather than the calling ThreadPool thread. This proves the self-marshaling contract that fixes the cross-thread COM defect (AC1). Acceptance: test passes; the COM-access body runs on the marshal-target thread, not the invoking background thread.

---

### Phase 9 — Final QA Loop (Full Toolchain)

- [x] [P9-T1] Run format step `dotnet tool run csharpier .` from repo root and record to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/qa-gates/qa-csharpier.2026-06-30T18-10.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If files change, restart the loop from this task. Acceptance: artifact exists with all four fields and EXIT_CODE 0 in the final pass.
- [x] [P9-T2] Run analyzer step `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/qa-gates/qa-analyzers.2026-06-30T18-10.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (warning/error counts; confirm no banned-API regressions per AC6). If it changes files or fails, restart from P9-T1. Acceptance: artifact exists with all four fields and a clean build.
- [x] [P9-T3] Run nullable type-check step `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/qa-gates/qa-nullable.2026-06-30T18-10.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If it changes files or fails, restart from P9-T1. Acceptance: artifact exists with all four fields and a clean build.
- [x] [P9-T4] Run test + coverage step `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` and record to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/qa-gates/qa-tests-coverage.2026-06-30T18-10.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including numeric post-change values: total tests passed/failed and repo-wide line coverage percent. If it changes files or fails, restart from P9-T1. Acceptance: artifact exists with all four fields and numeric post-change coverage; all tests pass in the final pass.

---

### Phase 10 — Coverage Verification and Document Updates

- [x] [P10-T1] Compute and record coverage deltas to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/qa-gates/coverage-delta.2026-06-30T18-10.md`, reporting: baseline repo-wide coverage (from P0-T5), post-change repo-wide coverage (from P9-T4), and new/changed `EmailMoveMonitor` bookkeeping line coverage. Verify repo-wide >=80% (testable denominator) and changed/new `EmailMoveMonitor` bookkeeping >=90%. If either threshold is unmet, the plan outcome is remediation-required (not PASS). Acceptance: artifact records all three numeric values and an explicit PASS/REMEDIATION verdict against the two thresholds. Maps AC5.
- [x] [P10-T2] Document the COM-host-bound exemption boundary in the coverage-delta artifact and (if `[ExcludeFromCodeCoverage]` is applied to any genuinely COM-bound member) note that the marshaled bookkeeping logic is NOT exempt per CLAUDE.md exemption clause (c); only live event subscription / live STA dispatcher behavior reachable without the seam is exemption-eligible, and any applied exemption requires maintainer ratification. Acceptance: artifact contains an explicit exempt-vs-non-exempt boundary statement scoped to `EmailMoveMonitor`. Maps AC5.
- [x] [P10-T3] Update `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/spec.md` to check off AC1–AC9 with brief evidence references (test file path, test names, coverage numbers, QA artifact paths) and set Status to reflect implementation completion. Acceptance: spec AC checkboxes reflect verified state with evidence references. Maps AC9.
- [x] [P10-T4] Mirror the issue update to `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/issue-updates/issue-228.2026-06-30T18-10.md` with `Timestamp:`, the exact update text, and `PostedAs:` field per `evidence-and-timestamp-conventions`. Acceptance: issue-update mirror exists with required fields. Maps AC9.

---

## Notes on Invariants and Hazards

- No-deadlock invariant (spec): a synchronous marshaled call must not deadlock against the STA thread's own `BeforeItemMove` event-dispatch reentrancy. The `BeforeItemMove` handler body stays STA-bound by Outlook contract and is NOT re-marshaled (P5 does not touch the handler body in `SetupBeforeItemMove`).
- `UiThread.Dispatcher` is process-global, set-once static state. P7-T1 mandates explicit Arrange/teardown of this state; tests must not rely on execution order (research §6; issue #199).
- Banned-API guard (AC6): touched files must not introduce `DateTime.Now`/`DateTime.UtcNow`/`Random.Shared`/`Thread.Sleep`/`Task.Delay`; the existing `TimeProvider.Delay` usage at `QfcDatamodel.QueueProcessing.cs:142` is preserved unchanged.
- File-size limit (500 lines): `EmailMoveMonitor.cs` is currently 189 lines; the new interface and tests are separate files. No file is expected to exceed the limit.
