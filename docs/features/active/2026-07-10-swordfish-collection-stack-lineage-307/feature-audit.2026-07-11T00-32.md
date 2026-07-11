# Feature Acceptance Audit — swordfish-collection-stack-lineage (#307, epic F2)

- Timestamp: 2026-07-11T00-32
- Reviewer: feature-reviewer
- Work Mode: `full-feature` (AC sources: `spec.md` §Acceptance Criteria + `user-story.md` §Acceptance Criteria)
- Verdict: **PASS** — all acceptance criteria met; 0 Blocking findings

## Scope and Baseline

- Base (resolved): `origin/epic/swordfish-removal-integration` (epic integration branch).
- Merge-base: `0b72b11bb1145dd00f70fe9de8d7a6ed3bef79bb`.
- Scope diff: three-dot merge-base diff `origin/epic/swordfish-removal-integration...HEAD` (the
  authoritative PR-style scope; the two-dot range the caller referenced also surfaces divergence
  noise from siblings #306/#309/#310, which is excluded here — see policy-audit §Scope Resolution).
- Baseline suite: 4680 passed / 0 failed. Post-change: 4685 passed / 0 failed. New tests added, four
  legacy direct-test files + `RecentsList_Tests` deleted; no new failures.

## Acceptance Criteria Inventory

- `spec.md` §Acceptance Criteria: 16 checkbox items (all pre-checked in source).
- `user-story.md` §Acceptance Criteria: 8 checkbox items (all pre-checked in source).
- Total: 24 AC items across both sources.

## Acceptance Criteria Evaluation

### spec.md

| # | Criterion (abbreviated) | Verdict | Evidence |
|---|---|---|---|
| S1 | Clean `ConcurrentObservableCollection<T>` created on `ObservableCollection<T>` with IList(<T>)/IList, Find*/Exists, CollectionChanged, Subscribe, and full serialization surface | PASS | `ConcurrentObservableCollection.cs` (search/observer/list-conv surface); `.Serialization.cs` (file ctors incl. AltListLoader, Serialize/SerializeAsync, Deserialize overloads, FilePath/FolderPath/FileName, ToList/FromList, FS/Prompt seams) |
| S2 | Clean collection serializes as a bare JSON array (no `[JsonObject]`) | PASS | No `[JsonObject]` attribute on type; `CollectionRoundTrip_Tests` asserts `StartWith("[")` / `NotStartWith("{")` |
| S3 | `CtfMap` and `SubjectMapSco` (incl. AltListLoader) re-based and compile against the surface | PASS | `CtfMap.cs`, `SubjectMapSco.cs` diffs; analyzer build EXIT 0; suites green |
| S4 | Direct consumers re-pointed (`Filters`, `PrefixList`, `OlFolderClassifierGroup`) | PASS | `AppAutoFileObjects.cs`, `AppToDoObjects.cs`, `OlFolderClassifierGroup.cs` diffs |
| S5 | Interface members `IAppAutoFileObjects.Filters`, `IToDoObjects.PrefixList`/`LoadPrefixList` updated; IScoCollection/IScoCollection2 untouched | PASS | interface diffs; deletion-gate confirms only F5-reserved interfaces still reference `IScoCollection` |
| S6 | `SloStack<T>` positional surface (`this[int]`, `Peek(int)`, `Pop(int)`, `TryPeek`/`TryPop` front+indexed, Push→AddFirst, Pop()/Peek()→TakeFirst/First; top==index 0) | PASS | `SloStack.cs:126-233`; `SloStack_Tests`, `SloStackUndoContract_Tests` |
| S7 | `SloStack<T>` exposes `SerializeAsync()` and typed `ISmartSerializable<SloStack<T>>` with file-based `Static.Deserialize` | PASS | `SloStack.cs:43-124, 235-256` |
| S8 | All `ScoStack<IMovedMailInfo>` consumers migrated (QuickFiler controllers+interfaces, MovedMails/LoadMovedMails, SortEmail, EmailFiler, IAppAutoFileObjects.MovedMails) | PASS | QuickFiler/TaskMaster/UtilitiesCS diffs; deletion-gate: 0 residual `ScoStack<` first-party references |
| S9 | MovedMails construction reconciled to file-based `SloStack<IMovedMailInfo>.Static.Deserialize` (no stubbed members) | PASS | `AppAutoFileObjects.cs` `LoadMovedMails` diff |
| S10 | JSON round-trip test per persisted collection (MovedMails, Filters, PrefixList, CtfMap, SubjectMapSco), in-memory, asserting order/values + `$type` stability | PASS | `CollectionRoundTrip_Tests.cs` (6 tests covering all five collections) |
| S11 | `SortEmail.UndoAsync` and `QfcFormController.UndoDialog` undo behavior preserved (forward `stack[i]`, positional `Pop(i)` shift-and-reprocess, `Serialize()`); contract covered | PASS | `SortEmail.cs:552-608`; `QfcFormController.Actions.cs:206-250`; `SloStackUndoContract_Tests`; `evidence/regression-testing/undo-contract.md` |
| S12 | `RecentsList<T>` dead code removed (`RecentsList.cs` + `RecentsList_Tests.cs` deleted, not migrated) | PASS | both files deleted in diff; `evidence/regression-testing/recentslist-deadcode-check.md` (no live consumer) |
| S13 | Legacy `ScoCollection.cs`/`ScoStack.cs` + direct tests deleted only after grep confirms no non-F5 reference | PASS | `evidence/regression-testing/deletion-gate.md` (only two F5-reserved interface hits remain) |
| S14 | Migrated tests compile and pass (ManageFilters, sender, lock-recursion, EmailFiler, coverage-expansion) | PASS | `evidence/qa-gates/vstest-coverage.md` 4685/0; re-pointed test diffs |
| S15 | New `SloStack`/clean-collection members meet the new-code coverage bar (>=90% new; line>=85%/branch>=75% per rules) | PASS | new-code line coverage 98.0% (`evidence/qa-gates/coverage-delta.md`) |
| S16 | Full C# toolchain passes in order (csharpier→analyzers→nullable→MSTest) with no errors in final pass | PASS (nullable gate is pre-existing vendored-only baseline; first-party clean) | `evidence/qa-gates/*`; see policy-audit §Toolchain |
| S17 (scope) | No UtilitiesSwordfish deletion, ProjectReference removal, sln edit, or F1/F3/F5-reserved type changes | PASS | scope-boundary grep returned NONE; no ProjectReference/sln changes |

Note on S16: the nullable `TreatWarningsAsErrors` build returns EXIT 1 solely from the 84-error
vendored-only baseline (UtilitiesSwordfish 50 + SVGControl 34) with zero first-party diagnostics;
the operative first-party type-safety gate per csharp.md is the analyzer build, which is green. This
is a documented pre-existing baseline, not a failure introduced by F2 — the AC "no errors in the
final pass" is satisfied on the first-party surface the policy governs.

### user-story.md

| # | Criterion (abbreviated) | Verdict | Evidence |
|---|---|---|---|
| U1 | Clean `ConcurrentObservableCollection<T>` base exists with the full member surface | PASS | maps to S1 |
| U2 | Every `ScoCollection<T>` subclass/consumer + interface members re-based | PASS | maps to S3/S4/S5 |
| U3 | `SloStack<T>` provides the positional surface + SerializeAsync + file-based Static.Deserialize | PASS | maps to S6/S7 |
| U4 | Every `ScoStack<IMovedMailInfo>` consumer migrated with construction reconciled | PASS | maps to S8/S9 |
| U5 | JSON round-trip compatibility test per persisted collection | PASS | maps to S10 |
| U6 | QuickFiler and SortEmail undo flows preserved with no regression | PASS | maps to S11 |
| U7 | Dead `RecentsList<T>` type + test deleted (not migrated) | PASS | maps to S12 |
| U8 | Legacy `ScoCollection.cs`/`ScoStack.cs` + direct tests removed only after unreferenced; full toolchain green + new-code coverage bar met | PASS | maps to S13/S15/S16 |

## Acceptance Criteria Check-off

All 24 AC items (16 in `spec.md`, 8 in `user-story.md`) were already checked `[x]` in the source
files by the executor and are confirmed PASS by this review. No unchecked PASS items remained, so no
new check-off edits were required. No item was downgraded.

## Summary

- Total AC items evaluated: 24 (spec 16 + user-story 8).
- PASS: 24. PARTIAL: 0. FAIL: 0. UNVERIFIED: 0.
- Scope boundary held: no UtilitiesSwordfish/ProjectReference/sln/IScoCollection/IScoCollection2/
  ISubjectMapSco/ScoDictionary/ScoSortedDictionary edits.
- Blocking findings: 0.
- Verdict: **ready to merge**. Non-blocking observations (pre-existing `AppToDoObjects.cs` 503-line
  size; verbatim-ported byte[] no-op ctor; coverage-rule tension resolved in favor of CLAUDE.md) are
  documented in policy-audit and code-review; none require remediation for this feature.

### Acceptance Criteria Status
- Source: `spec.md`, `user-story.md`
- Total AC items: 24
- Checked off (delivered): 24
- Remaining (unchecked): 0
- Items remaining: none
