# Feature Audit — Acceptance Criteria Verification, Issue #797

- Date: 2026-09-07
- Timestamp label: 2026-09-07T22-40
- Work Mode: `full-bug` (marker at `issue.md` line 12)
- Authoritative AC source: `spec.md` in this feature folder, `## Acceptance Criteria` section, lines 455-462. Under `full-bug` the acceptance-criteria-tracking skill resolves the AC source to `spec.md` only. `user-story.md` is correctly absent and its absence is not a finding. `issue.md` lines 96-103 carry a verbatim mirror of the same eight criteria; both files were checked and agree.
- Baseline: `origin/main` at `c431dc3297e864041d829e8d79b348960b8d8019`
- Branch: `bug/folder-settings-never-persist-797`
- Evidence base: `artifacts/797-source-review.patch`, the working-tree post-image of every production file cited, the committed evidence tree under `evidence/`, and the two session Cobertura documents read directly.

## Verdict

**PASS.** Seven of eight acceptance criteria are delivered and verified. One (AC3) is UNVERIFIED because it requires a live Outlook restart; it is correctly unchecked in both requirement files and handed to the maintainer with a nine-step procedure. **Zero blocking findings.**

| Result | Count |
|---|---|
| PASS | 7 |
| PARTIAL | 0 |
| FAIL | 0 |
| UNVERIFIED | 1 |
| **Total** | **8** |

---

## Acceptance criteria evaluation

| AC | Criterion (abbreviated) | Verdict | Implementing code | Verifying test |
|---|---|---|---|---|
| AC1 | Fresh-build path adopts the resource-defined disk configuration; first Save creates the file | **PASS** | `AppOlObjects.StoreLoading.cs` 39-42, 76-79 | `LoadStoresAsync_WhenConfigDeserializesToNull_FreshWrapperAdoptsLoaderDiskConfiguration`; negative `LoadStoresAsync_WhenConfigKeyIsAbsent_FreshWrapperKeepsEmptyDiskPath` |
| AC2 | `Serialize()` logs an error on empty or null `Config.Disk.FilePath` | **PASS** | `SmartSerializable.cs` 451-472 | `Serialize_WithEmptyDiskPath_LogsErrorAndArmsNoTimer`; `Serialize_WithNullDiskPath_LogsErrorAndArmsNoTimer` |
| AC3 | Saved value present after an Outlook restart (manual verification) | **UNVERIFIED** | AC1, AC2 and AC4 supply the mechanism | Not automatable; manual handoff at `evidence/other/p6-t1-ac3-manual-verification.2026-09-06T22-00.md` |
| AC4 | An explicit Save is not lost inside the 3-second deferred window | **PASS** | `SmartSerializable.cs` 485-499; `StoreWrapperController.cs` 285-290 | `SerializeNow_WithConfiguredPath_WritesWithoutFiringTimer`; unchanged-path pin `Serialize_WithConfiguredPath_StillRequiresTimerFireToWrite` |
| AC5 | Double-persistence removed or made loud; reflection replaced by a typed seam | **PASS** | `IJunkFolderSelectionSink.cs`; `AppOlObjects.JunkFolders.cs` 19, 54-57; `StoreWrapperController.cs` 333-346 | `PersistJunkFolderSelections_PassesJunkCertainPathFirst`; `PersistJunkFolderSelections_WhenGlobalsAreNotTheTypedSink_LogsErrorAndDoesNotInvoke`; retargeted `PersistJunkFolderSelections_WhenApplyMethodIsMissing_DoesNotThrow` |
| AC6 | SMTP address shown; specific message with reason on failure; fallback source; retry on dialog open | **PASS** | `StoreWrapper.cs` 199-297; `StoreWrapperController.Display.cs` 41-51, 78-86 | four `GetSmtpAddressFromStore_*` cases; `RefreshUserEmailAddress_WhenRootFolderIsNull_ReturnsNullAndDoesNotThrow`; three `PopulateWithCurrent_When*` cases |
| AC7 | Inbox and Root Folder displayed without the leading `\\` | **PASS** | `StoreWrapperController.Display.cs` 48-50, 155-177 | six `TrimStorePrefix_*` cases; `PopulateWithCurrent_RendersInboxAndRootFolderWithoutStorePrefix` |
| AC8 | A null `Current` renders the placeholder instead of throwing | **PASS** | `StoreWrapperController.Display.cs` 30-33, 121-129 | `PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow`; `GetRelativeFsPath_WithNullCurrent_ReturnsPlaceholderAndDoesNotThrow`; inverted `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText` |

---

## Per-criterion verification detail

Each criterion below was traced to concrete code and to a named test rather than accepted from the executor's own mapping. Where the verdict depended on a chain of facts, the chain is set out so it can be re-checked.

### AC1 — PASS

> When `StoresWrapper.json` is absent, the fresh-build path adopts the resource-defined disk configuration so `Config.Disk.FilePath` resolves to `%LocalAppData%\TaskMaster\StoresWrapper.json`, and the first Save creates the file.

**Implementation.** `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs` hoists the `TryGetValue` result into a `configFound` local (lines 39-42) so the branch fact survives past the `if/else`, then applies the loader's configuration after the fresh build (lines 76-79): `StoresWrapper.Config.CopyFrom(config.Config, true);`.

**Why the adoption reaches the field the serializer reads.** The chain was walked in full and holds at every link:

1. `StoresWrapper : SmartSerializable<StoresWrapper>` (`StoresWrapper.cs` line 17), so `StoresWrapper.Config` **is** `SmartSerializable<T>.Config` at `SmartSerializable.cs` line 69 — the same property `TryGetSerializationPath` reads at line 453. There is no second configuration object.
2. `NewSmartSerializableConfig.CopyFrom(other, deep: true)` (`NewSmartSerializableConfig.cs` lines 197-214) deep-copies and calls `Disk.CopyFrom(other.Disk)`.
3. `FilePathHelper.CopyFrom` (`FilePathHelper.cs` lines 449-458) assigns `_filePath = other._filePath` directly.

**Consistency check.** The statement uses the identical idiom already present on the successful-deserialize paths at `SmartSerializable.cs` lines 224, 246 and 302, so the fresh-build branch is now brought into line with the branch that already worked. This corroborates the specification's claim that the shared overload needed no change.

**Scope correctness.** The `configFound` guard correctly excludes the key-absent branch, which the criterion places out of scope and which AC2's error log makes visible rather than silent.

**Test.** The positive test injects a loader whose disk path is the fixed non-existent value `X:\FakeAppData\TaskMaster\StoresWrapper.json` and a deserialize stub returning null, then asserts the fresh wrapper's `Config.Disk.FilePath` equals that value. The negative test asserts the empty path is retained when the key is absent. Neither touches the filesystem.

**Bounded residual.** "The first Save creates the file" is proven up to the seam, not to disk: the tests prove the path is populated and AC4's tests prove the write goes through the stream-writer seam, but no automated test writes a real file, correctly, because the unit test policy forbids it. The disk residual is exactly what the AC3 manual procedure covers.

### AC2 — PASS

> `SmartSerializable<T>.Serialize()` logs an error (not a silent return) when invoked with an empty or null `Config.Disk.FilePath`.

**Implementation.** `TryGetSerializationPath(out string filePath)` at `SmartSerializable.cs` lines 451-464 replaces the previous `if (Config.Disk.FilePath != "")` guard. It uses `string.IsNullOrEmpty`, which closes the null hole the criterion names, and on rejection calls `logger.Error` with a message naming the type and the offending value. `Serialize()` (lines 466-472) routes through it.

**Both cases are genuinely covered.** The empty-string case and the null case are separate tests. The null case is not hypothetical: the pre-change guard compared only against `""`, and `FilePathHelper` can assign a null `_filePath`, so a null path previously passed the guard and reached the write path.

**Factoring.** The guard is shared by `Serialize()` and `SerializeNow()`, so the deferred and explicit entry points cannot diverge in their diagnostic. This is the reason AC4's fix does not reintroduce a silent failure.

**Test.** Both tests attach an in-memory log4net appender to the root logger of the repository owning the serializer, filter captured events by the test's own probe type name, assert at least one error-level event, and additionally assert `timerFactoryCallCount == 0` and `timerStub.Started == false` — proving the rejecting path arms no timer as well as logging. Both restore the logger state in a `finally`.

**Coverage.** `TryGetSerializationPath` reads `line-rate="1" branch-rate="1"` in the post-change Cobertura document, verified directly by this reviewer.

### AC3 — UNVERIFIED

> A value saved in Folder Settings is present after an Outlook restart (manual verification).

**Verdict basis.** The criterion requires a live Outlook process with this build of the VSTO add-in loaded, a real user profile directory, and a full process teardown and restart. None is available in an agent environment, and the executing agent was directed not to start one, load the add-in, or drive any user interface.

**The handoff is honest and complete.** `evidence/other/p6-t1-ac3-manual-verification.2026-09-06T22-00.md` records `AC3-RESULT: BLOCKED-MANUAL`, reproduces all nine procedure steps verbatim from the plan, marks every one NOT PERFORMED with an individual reason, fabricates no observation, and explains why no annotation was added beside the criterion line (the AC-tracking skill permits exactly one edit to a criterion line — `- [ ]` to `- [x]` — and the specification forbids rewording). The checkbox is left unmarked and byte-identical to its authored text in both `spec.md` line 457 and `issue.md` line 98. Plan task P6-T4 is correspondingly left unchecked, which is the plan's own conditional branch behaving as designed rather than an incomplete execution.

**What the automated evidence does establish.** The three mechanisms that produce the AC3 symptom are each independently proven: the fresh-build path now adopts a real disk path (AC1), the serializer no longer returns silently on an empty or null path (AC2), and an explicit Save writes inline rather than through the deferred timer (AC4). What is not established is that the file appears on disk in a live VSTO host.

**Assessment.** Honest non-verification, correctly recorded, correctly not checked off. It is not evaluated as PASS and it is not treated as a blocking defect. It is handed to the maintainer.

### AC4 — PASS

> An explicit Save is not lost if Outlook closes within the 3-second deferred-write window (flush on save or on shutdown).

**Implementation.** `SerializeNow()` at `SmartSerializable.cs` lines 485-499 evaluates the AC2 guard and then calls `SerializeThreadSafe(filePath)` inline. `StoreWrapperController.SaveChanges` line 290 switches from `Model.Serialize()` to `Model.SerializeNow()`. Every other caller of `Serialize()` keeps the deferred behaviour unchanged.

**No lost-write window exists.** `SerializeNow` never consults or consumes the single-shot guard before writing — it writes unconditionally once the path guard passes — so no interleaving of a pending deferred timer with an explicit save can drop the explicit save. Both interleavings were traced: a deferred request followed by an explicit save produces two writes of current state, and an explicit save followed by a deferred request produces an inline write plus a later deferred one. Neither loses data.

**Preconditions honoured.** `SerializeThreadSafe` requires a non-null `_parent`; `StoresWrapper` sets it in both constructors, so the guard is satisfied on both the deserialized and the fresh-built model. The AC2 guard is evaluated before the synchronous write, so the fix does not substitute one silent failure for another — an explicit D3 requirement.

**The unchanged-behaviour claim is directly evidenced.** `Serialize_WithConfiguredPath_StillRequiresTimerFireToWrite` asserts the deferred path arms the timer, writes nothing until `FireElapsed()`, and then writes exactly once. Placing it in the same file as the explicit-save test means the "deferred path is unchanged" claim rests on an assertion rather than on prose.

**Two secondary hazards, neither defeating the criterion.** The explicit save now performs file I/O and an infinite-timeout write-lock acquisition on the UI thread, and it re-arms the single-shot guard up to three seconds early, permitting a redundant second timer. Both are recorded as CR-2 and CR-3 in `code-review.2026-09-07T22-40.md`. Neither loses a write, which is what the criterion requires.

**Coverage.** `SerializeNow` reads `line-rate="1" branch-rate="1"`, verified directly.

### AC5 — PASS

> The junk-folder double-persistence path is either removed or made to fail loudly; the reflection lookup is replaced by a typed seam.

**The seam is genuinely typed.** `UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs` declares a single member whose XML documentation states the parameter order is part of the contract. `AppOlObjects` gains `: IJunkFolderSelectionSink` and an **explicit** implementation at lines 54-57, so the type's public surface does not widen. Project reference direction is preserved: declared in `UtilitiesCS`, implemented in `TaskMaster`.

**No reflection fallback survives.** Three checks, all passed: the `GetMethod` call with its `BindingFlags` and parameter-type array is deleted outright; the `using System.Reflection;` directive is removed from `StoreWrapperController.cs`, which the file could not compile without if any other reflection use remained, and both the analyzer and nullable rebuilds exited 0; and a pattern search over the patch finds no replacement reflection API.

**The double-persistence path fails loudly at the seam.** `if (olObjects is not IJunkFolderSelectionSink sink)` is followed by `logger.Error` — upgraded from the previous `logger.Warn` — naming the interface, then `return`. The criterion's disjunction is satisfied by the fail-loudly branch, which is the reading design decision D2 selects in the authoritative specification.

**No infinite recursion in the forwarder.** The explicit implementation's body calls `ApplyJunkFolderSelections(a, b)` unqualified. This is safe only because an explicit interface implementation is excluded from the type's own member lookup, so the call binds to the `internal` method at lines 36-45. Verified that the internal method exists with the matching signature, and that argument order is preserved: the first parameter routes to `WriteJunkCertainSetting` and the second to `WriteJunkPotentialSetting`.

**Argument order is pinned by test, as the criterion detail requires.** `PersistJunkFolderSelections_PassesJunkCertainPathFirst` uses distinguishable values (`Inbox\Certain Folder`, `Inbox\Potential Folder`) and asserts both positions plus an invocation count of 1.

**The loud-failure branch retains coverage.** `NonSinkOlObjects` declares a `public void ApplyJunkFolderSelections(string, string)` with the historic name and signature but does not implement the interface — precisely the case that discriminates the old binding from the new one. The pre-existing negative test is retargeted onto it rather than deleted.

**Recorded residual.** The second persistence mechanism itself is retained by design; a divergence between the JSON model and the .NET user settings is not made loud, only the seam-absence case is. `spec.md` risk 4 records this as an accepted rollout consequence and step 9 of the AC3 procedure is written to check it. Recorded as CR-5, advisory.

### AC6 — PASS

> User Email shows the SMTP address; on lookup failure it shows a specific message including the reason, falls back to an alternative source (the account SMTP address or the store display name when it is an SMTP address), and the lookup is retried when the dialog opens.

Each of the four obligations was checked separately.

**Shows the SMTP address.** `PopulateWithCurrent` renders `Current?.UserEmailAddress` when non-null. `GetSmtpAddressFromStore_WhenPrimarySmtpAddressIsPresent_ReturnsIt` pins the success path and additionally asserts `LastSmtpLookupError` is cleared to null, so a stale reason cannot accompany a resolved address.

**Falls back to an alternative source, in the specified order.** `StoreWrapper.GetSmtpAddressFromStore` implements exactly: Exchange primary SMTP; then `addressEntry?.Address` when it contains an at-sign; then `DisplayName` when it contains an at-sign; then null. The substantive repair is that each of the first two steps carries its **own** `catch (COMException)` that captures the reason and continues, replacing a single outer catch that converted any failure anywhere in the chain into `return null`. One implementation detail is load-bearing and correct: the `addressEntry` local is hoisted above the first `try` so the second step can still use it after the first throws — had it stayed inside the first block, the address-entry fallback would have been unreachable on exactly the path that needs it. Three tests cover cases 2, 3 and 4 of the order.

**Specific message including the reason.** `BuildUserEmailUnavailableText` returns `"Email address unavailable"` when no reason was captured and `"Email address unavailable: {reason}"` when one was, replacing the generic `"Error Loading"` for this label only. `PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason` asserts both that the reason text appears and that the value is not `"Error Loading"` — the correct pair, since asserting only the former would not prove the generic placeholder was displaced.

**Retried when the dialog opens.** The retry is gated on `Current is not null && Current.UserEmailAddress is null` at `StoreWrapperController.Display.cs` lines 48-51. `PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress` proves the retry runs, and `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup` proves it does not run when the address is already populated — the second test is constructed so the mocked chain would yield a *different* address, so an unchanged value is genuine proof that no lookup occurred rather than a coincidence.

**The accepted COM limitation, and whether the bound holds.** The criterion reintroduces a synchronous Outlook COM property read on the UI thread. The condition `Current.UserEmailAddress is null` does bound the retry to the failing case only. However, the documented bound of "at most once per dialog open" does **not** hold: `PopulateWithCurrent`'s only production call site is `DisplayName_SelectedValueChanged` (`StoreWrapperController.cs` line 169), which fires on every store selection change, and a failed retry leaves `UserEmailAddress` null so the gate stays open. The true bound is one lookup per populate invocation on a store whose address is still null. This is recorded as CR-1 (Medium, advisory) in the code review. It does not defeat AC6, whose authoritative text requires only that the lookup be retried when the dialog opens — which it is.

**Safety.** `RefreshUserEmailAddress` is safe when `RootFolder` is null because the chain's first read is null-conditional; `RefreshUserEmailAddress_WhenRootFolderIsNull_ReturnsNullAndDoesNotThrow` pins it. `LastSmtpLookupError` carries `[JsonIgnore]`, correctly, since it describes one runtime lookup rather than stored state.

**Coverage.** `GetSmtpAddressFromStore` reads `line-rate="0.8710" branch-rate="0.9444"`; `RefreshUserEmailAddress` and `BuildUserEmailUnavailableText` both read 1.00 line and 1.00 branch. All verified directly against the post-change Cobertura document.

### AC7 — PASS

> Inbox and Root Folder are displayed without the leading `\\` (cosmetic).

**Implementation.** `TrimStorePrefix(string?)` is a pure private-surface static helper: it returns the input unchanged unless it starts with exactly `\\` (ordinal comparison), in which case it returns `Substring(2)`. Both label assignments route through it.

**No rendering regression on any input.** The call site is `TrimStorePrefix(Current?.Inbox?.FolderPath) ?? "Error Loading"`. Because the helper returns null for a null input, the placeholder still fires exactly as before; because it returns the empty string unchanged, an empty `FolderPath` still renders as an empty label, matching pre-change behaviour. The trim therefore changes only the case it is meant to change.

**Boundary coverage.** Six pure-function cases: leading `\\`, no leading backslash, single leading backslash, empty string, null, and prefix-only. Plus `PopulateWithCurrent_RendersInboxAndRootFolderWithoutStorePrefix`, which asserts the rendered label text end to end.

**Accessibility choice.** `internal static` rather than private, with an in-code rationale: the pure cases are reachable from `UtilitiesCS.Test`, to which the assembly already grants `InternalsVisibleTo`, and `internal` does not widen the controller's public surface. Reasonable and documented.

**Coverage.** `TrimStorePrefix` reads `line-rate="1" branch-rate="1"`.

### AC8 — PASS

> A null `Current` store selection renders the placeholder text instead of throwing.

**Implementation.** The four previously unguarded dereferences at the top of `PopulateWithCurrent` are now null-conditional (`Current?.ArchiveRoot`, `Current?.ArchiveFsRoot`, `Current?.JunkCertain`, `Current?.JunkPotential`), matching the form the immediately following block already used — the inconsistency inside a single method that the specification identified. `GetRelativeFsPath`'s dereference is guarded so it returns the same `"Please select an archive"` placeholder it already returned for an unset archive root.

**All reachable dereferences are covered.** The AC6 retry block, which sits between the mirror assignments and the label assignments, is itself guarded by `Current is not null`, so it cannot reintroduce the throw the criterion removes.

**Tests.** `PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow` asserts no throw and all six rendered placeholder values. `GetRelativeFsPath_WithNullCurrent_ReturnsPlaceholderAndDoesNotThrow` covers the helper path.

**The declared inversion.** `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText` previously asserted `act.Should().Throw<NullReferenceException>()` — codifying the defect while its name described the fix. It now asserts no throw plus four exact rendered values. This is a strengthening, not a weakening: the original pinned only an exception type, the replacement pins four exact strings. It was declared in advance as design decision D6 (`spec.md` lines 420-432), which is the correct treatment under the General Code Change Policy's rule that existing tests are part of the specification.

---

## Regression surface

The specification's D1 claim is that, because the shared deserialize overload is not modified, its behavioural regression surface is empty. This reviewer confirms the overload is untouched: the only hunks against `SmartSerializable.cs` in the patch are the two additions in the Serialization region (lines 1657-1718 of the patch), and neither touches `Deserialize<T,U>` or `DeserializeJson`. The documented fail-soft null contract relied on by the folder-predictor load path is therefore preserved exactly.

`spec.md` lines 611-615 states that any failure among the existing test callers that pin the overload's behaviour would indicate it had been modified contrary to D1. The final run was fully green at 5262 of 5262 with zero failures, so no such indication exists.

The out-of-scope items the specification enumerates were each confirmed untouched: `SmartSerializableBase.cs` receives no hunk; `IOlObjects` is not extended; no VSTO add-in lifecycle file appears in the working set; no resource file appears; the QuickFiler recipient-resolution blocking hazard is not addressed; and the dead-branch observation at the former `StoreWrapperController.cs` line 466 is left as found.

---

## Test execution and coverage summary

- Final scoped run: 5262 total, 5262 passed, 0 failed, 0 skipped, exit code 0.
- Tests added: 25 (baseline 5237 to 5262, which reconciles exactly).
- Red-first: 16 tests recorded as failing before their fixes; all 16 enumerated as passing after, in full rather than sampled.
- Changed-line coverage: 91.09 percent over 101 executable changed lines, against the 90 percent requirement CLAUDE.md sets for new and changed code.
- No-regression: post-change 53.26 percent is not below the baseline 53.23 percent, on a comparable denominator (`lines-valid` moved 0.085 percent, inside rule R9's 5 percent tolerance).
- Both figures independently re-verified by this reviewer against the raw Cobertura root elements, and every new production member's own line and branch rate read directly. Details in `policy-audit.2026-09-07T22-40.md` section 5.
- No coverage-exclusion attribute is introduced and no production file is excluded from measurement.

The absolute repository line coverage of 53.26 percent sits below both documented floors. This is pre-existing under the two-assembly measurement scope, was already 53.23 percent at baseline, and is recorded as an observation under plan rule R8 rather than raised as a finding. See the policy audit for the full authority analysis.

---

## Findings affecting acceptance criteria

None. Zero blocking findings. Six advisory findings are recorded in `code-review.2026-09-07T22-40.md`; the only one that touches a criterion's supporting prose is CR-1, which corrects an overstated bound in the AC6 documentation without defeating the criterion itself.

No remediation-inputs artifact is produced.

---

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md
  (authoritative under full-bug work mode), mirrored verbatim in the same folder's issue.md
- Total AC items: 8
- Checked off (delivered): 7
- Remaining (unchecked): 1
- Items remaining: AC3 — "A value saved in Folder Settings is present after an Outlook restart (manual verification)."
```

**Check-off action taken by this reviewer: none required.** All seven criteria evaluated PASS were already checked `- [x]` in both `spec.md` and `issue.md` by the executor, and both files agree. AC3, evaluated UNVERIFIED, is correctly left `- [ ]` in both files and its criterion text is byte-identical to the authored wording. No criterion text was altered and no criterion was added.

## Outstanding work

1. **AC3 manual verification.** Owner: the project maintainer. Procedure: the nine steps in `evidence/other/p6-t1-ac3-manual-verification.2026-09-06T22-00.md`. On success, change `- [ ]` to `- [x]` on the AC3 line in `spec.md` and mirror the same single-character change in `issue.md`, and nothing else. On failure, do not check it off; report against issue #797. Step 9 additionally checks the known junk-folder rollout consequence recorded as `spec.md` risk 4.
2. **CR-1**, the AC6 retry bound, is worth addressing in a follow-up: either add a per-store or per-controller attempted flag so the retry is genuinely once per dialog open, or correct the three code comments and the specification prose to state the actual bound.
3. **Pre-existing, out of scope by design.** The `SmartSerializable.cs` 500-line cap violation (D5), the removal of the second junk-folder persistence mechanism (D2), the non-blocking Outlook COM read (Non-Goals item 8), and the QuickFiler recipient-resolution blocking hazard (Non-Goals item 7) each remain open and each should be raised as its own work item rather than folded into a bug fix.
