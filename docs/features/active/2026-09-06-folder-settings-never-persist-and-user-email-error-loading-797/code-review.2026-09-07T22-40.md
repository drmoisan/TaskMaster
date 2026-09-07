# Code Quality Review — Issue #797 (Folder Settings never persist; User Email "Error Loading")

- Date: 2026-09-07
- Timestamp label: 2026-09-07T22-40
- Base: `origin/main` at `c431dc3297e864041d829e8d79b348960b8d8019`
- Branch: `bug/folder-settings-never-persist-797`
- Source of record: `artifacts/797-source-review.patch` (`git diff origin/main HEAD`, `*.cs` and `*.csproj`), cross-read against the working-tree post-image of every production file discussed below.

## Verdict

**PASS.** Zero blocking findings. Six advisory findings (CR-1 through CR-6), none of which alters an acceptance-criteria verdict or requires remediation before merge.

The change is well-constructed. Each of the two root causes is fixed at the narrowest correct site, the two silent-failure paths that made the defect undiagnosable are both converted to error-level diagnostics, the reflection binding is removed entirely rather than merely wrapped, and every new member is documented with a `// why:` comment that names the mechanism it repairs. The highest-risk edit — the deferred-write ordering change — is correct and does not lose writes, though it carries two secondary hazards recorded below.

| Finding | Severity | Blocking |
|---|---|---|
| CR-1 — AC6 retry bound is stated more tightly than the code enforces | Medium | No |
| CR-2 — explicit save performs file I/O and an unbounded lock wait on the UI thread | Low | No |
| CR-3 — `SerializeNow` re-arms the single-shot guard early, permitting a redundant second timer | Low | No |
| CR-4 — `SmartSerializable.cs` grew 45 lines while already 113 lines over the cap | Low | No |
| CR-5 — the AC5 double-persistence path is retained, and divergence between the two stores is not itself loud | Low | No |
| CR-6 — the production seam forwarder has no test coverage | Low | No |

---

## What was verified, and how

This review traced each acceptance criterion to concrete code and to a named test, rather than accepting the executor's mapping. Four verifications required following a chain rather than reading a single line, and each is set out below because the conclusion depends on the chain holding at every link.

### AC1 — the fresh-build path genuinely adopts the loader's disk configuration

The fix is one statement at `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs` lines 76-79:

```csharp
if (configFound && StoresWrapper is not null)
{
    StoresWrapper.Config.CopyFrom(config.Config, true);
}
```

The question this raises is whether `StoresWrapper.Config` is the same object that `Serialize()` later reads, or a different configuration that happens to share a name. It is the same object, and the chain was walked in full:

1. `StoresWrapper` is declared `public partial class StoresWrapper : SmartSerializable<StoresWrapper>` (`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` line 17), so `StoresWrapper.Config` resolves to `SmartSerializable<T>.Config` at `SmartSerializable.cs` line 69 — the very property `TryGetSerializationPath` reads at line 453.
2. `NewSmartSerializableConfig.CopyFrom(other, deep: true)` at `NewSmartSerializableConfig.cs` lines 197-214 deep-copies `other` and then calls `Disk.CopyFrom(other.Disk)`.
3. `FilePathHelper.CopyFrom` at `FilePathHelper.cs` lines 449-458 assigns `_filePath = other._filePath` directly, alongside the folder path, file name, stem and extension.

So the loader's materialised path reaches the exact field the serializer guard inspects. The fix also uses the identical idiom the successful-deserialize path already uses at `SmartSerializable.cs` lines 224, 246 and 302 (`instance.Config.CopyFrom(loader.Config, true)`), which is the right consistency choice and the reason design decision D1's claim that the shared overload needed no change holds.

The refactor from `if (_globals.IntelRes.Config.TryGetValue(...))` to a hoisted `configFound` local is necessary rather than cosmetic: the fresh-build statement sits below the `if/else`, so the branch fact must survive to that point. The key-absent branch is correctly excluded by `configFound`, matching the criterion's stated scope, and made visible by AC2's new error log rather than left silent.

Tests: `LoadStoresAsync_WhenConfigDeserializesToNull_FreshWrapperAdoptsLoaderDiskConfiguration` asserts the adopted path, and `LoadStoresAsync_WhenConfigKeyIsAbsent_FreshWrapperKeepsEmptyDiskPath` asserts the negative case. Neither touches the filesystem; both assert on the configuration value.

### AC5 — the seam is genuinely typed and no reflection fallback survives

Three separate checks, all of which had to pass:

1. **The reflection is gone, not wrapped.** The `GetMethod` call with its `BindingFlags` and parameter-type array is deleted outright (patch lines 1594-1602), and the `using System.Reflection;` directive is removed from `StoreWrapperController.cs` (patch line 1488). A directive removal is meaningful evidence here, because the file would not compile if any other reflection use survived, and the analyzer and nullable rebuilds both exited 0. A pattern search over the patch for reflection APIs finds no replacement.
2. **The cast is compile-checked and the failure is loud.** `if (olObjects is not IJunkFolderSelectionSink sink)` followed by `logger.Error(...)` and a `return`, then `sink.ApplyJunkFolderSelections(JunkEmail?.RelativePath, JunkPotential?.RelativePath)`. The former `logger.Warn` is now `logger.Error`, which is what "fail loudly" requires. The message names the interface, so it is actionable.
3. **The explicit implementation does not recurse.** `AppOlObjects.JunkFolders.cs` lines 54-57 declare `void IJunkFolderSelectionSink.ApplyJunkFolderSelections(a, b) => ApplyJunkFolderSelections(a, b);`. This is only safe because an explicit interface implementation is excluded from the type's own member lookup, so the unqualified call inside the body binds to the `internal` method at lines 36-45 rather than to itself. It does, and that internal method exists with the matching signature. Had the implementation been implicit, the same body would have been infinite recursion. The choice of an explicit implementation is also the correct one for the stated goal of not widening the type's public surface.

Module boundary. The interface is declared in `UtilitiesCS` and implemented in `TaskMaster`, so the one-way `TaskMaster` to `UtilitiesCS` project reference direction is preserved and `UtilitiesCS` gains no reference to `TaskMaster`. This matches the shape of the existing `IStoreDisableService` and `IStoreRehookService` interfaces in the same folder.

Test-double design is notably good here. `NonSinkOlObjects` declares a `public void ApplyJunkFolderSelections(string, string)` with the historic name and signature but does **not** implement the interface. That is precisely the discriminating case: the old reflection lookup would have bound to it, the typed cast does not. The paired assertions — an error event exists, and `ApplyCallCount` is 0 — attribute the event to this test without relying on an exact global event count.

### AC4 — deferred-write ordering: no lost-write window

This was examined for the three hazards the caller flagged. The conclusion is that no write is lost, but two secondary hazards exist and are recorded as CR-2 and CR-3.

The mechanism. `RequestSerialization` (`SmartSerializable.cs` lines 595-604) arms a single-shot three-second timer only when `_serializationRequested.CheckAndSetFirstCall` is true; the guard is reset only in `SerializeThreadSafe`'s `finally` block (line 543). `SerializeNow` (lines 485-499) bypasses the timer and calls `SerializeThreadSafe(filePath)` inline, after evaluating the AC2 guard.

**Lost-write analysis.** Two interleavings were considered.

- A deferred `Serialize()` at t=0 arms a timer that will fire at t=3; the user clicks Save at t=1. `SerializeNow` writes the current state inline. The pending timer still fires at t=3 and writes again. Two writes of the same state, no loss.
- `SerializeNow` at t=0 writes inline and re-arms the guard; a later `Serialize()` arms a fresh timer. No loss.

There is no interleaving in which the explicit save is dropped, because `SerializeNow` never consults or consumes the single-shot guard before writing — it writes unconditionally once the path guard passes. That is the correct design for this requirement.

**Re-entrancy.** `SerializeThreadSafe` acquires `_readWriteLock` (a `ReaderWriterLockSlim` constructed with the default non-recursive policy) via `TryEnterWriteLock(-1)`. Same-thread re-entry would throw `LockRecursionException`, but no re-entrant path exists: `SerializeToStream` serializes `_parent` through Newtonsoft and no property getter on `StoresWrapper` calls back into serialization. The realistic contention is cross-thread and is recorded as CR-2.

**Disposal ordering.** `_parent.ThrowIfNull(...)` guards the write, and `StoresWrapper` sets the parent reference in both constructors, so the guard is satisfied on both the deserialized and the fresh-built model — the precondition D3 identified holds. The `StreamWriter` is created inside a `using` and explicitly closed; the `finally` releases the lock before resetting the guard, in the correct order. No disposal-ordering defect was found. The one disposal gap, `_timer` never being disposed, is pre-existing and is discussed under CR-3.

**AC2 evaluated before any synchronous write.** `SerializeNow` calls `TryGetSerializationPath` first, so the fix does not substitute one silent failure for another. This was an explicit D3 requirement and it is honoured.

Tests pin both halves: `SerializeNow_WithConfiguredPath_WritesWithoutFiringTimer` asserts one writer creation and `timerStub.Started == false`, while `Serialize_WithConfiguredPath_StillRequiresTimerFireToWrite` asserts the deferred path arms the timer, writes nothing until `FireElapsed()`, and then writes exactly once. Having both in one file makes the unchanged-behaviour claim directly evidenced rather than asserted, which is the right structure.

### AC6 — the synchronous COM read, and whether the bound holds

The retry is at `StoreWrapperController.Display.cs` lines 48-51:

```csharp
if (Current is not null && Current.UserEmailAddress is null)
{
    Current.RefreshUserEmailAddress();
}
```

The condition is correct as far as it goes — the lookup runs only when the address is null, and the null-check on `Current` means a null store selection cannot throw here. But the claimed bound of "at most once per dialog open" does not hold. See CR-1.

The fallback chain in `StoreWrapper.GetSmtpAddressFromStore` was read in full and implements exactly the order the specification fixes: Exchange primary SMTP; then the address entry's own `Address` when it contains an at-sign; then `DisplayName` when it contains an at-sign; then null with `LastSmtpLookupError` set. Each of the first two steps carries its own `catch (COMException)` that captures the reason and continues rather than aborting, which is the substantive repair — the pre-change code had a single outer catch that converted any failure anywhere in the chain into `return null`.

Two details are correct and easy to get wrong. First, the `addressEntry` local is hoisted above the first `try` so the second step can still use it after the first step throws; had it stayed inside the first block, the address-entry fallback would have been unreachable on exactly the path that needs it. Second, `LastSmtpLookupError` is cleared to null on every success path and set only on total failure, so the controller cannot render a stale reason alongside a successfully resolved address.

`BuildUserEmailUnavailableText` renders `"Email address unavailable"` when no reason was captured and `"Email address unavailable: {reason}"` when one was. This satisfies the "specific message including the reason" requirement and correctly stops sharing the generic `"Error Loading"` literal with the Inbox and Root Folder labels. `PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason` asserts both that the reason text appears and that the value is not `"Error Loading"`, which is the right pair of assertions.

`RefreshUserEmailAddress` is safe when `RootFolder` is null because the chain's first read is null-conditional; `RefreshUserEmailAddress_WhenRootFolderIsNull_ReturnsNullAndDoesNotThrow` pins this.

### AC7 and AC8

`TrimStorePrefix` is a pure static helper: it returns the input unchanged unless the input starts with exactly `\\`, in which case it returns `Substring(2)`. Null and empty both pass through unchanged. Six boundary cases cover null, empty, prefix-only, single backslash, no backslash and the ordinary case.

One interaction worth confirming: the call site is `TrimStorePrefix(Current?.Inbox?.FolderPath) ?? "Error Loading"`. Because the helper returns null for a null input, the null-coalescing placeholder still fires exactly as it did before, and because it returns the empty string unchanged, an empty `FolderPath` still renders as an empty label — the same as the pre-change behaviour. The helper therefore introduces no rendering regression on any input.

For AC8, all four previously unguarded dereferences at the top of `PopulateWithCurrent` are now null-conditional, matching the form the very next block already used, and `GetRelativeFsPath`'s dereference is guarded so it returns the same placeholder it already returned for an unset archive root. The two new tests plus the inverted existing test cover the populate path and the helper path.

The in-scope cleanup at `GetRelativeFsPath` — `&` changed to `&&` — is behaviourally inert as claimed: both operands call the null-tolerant `IsNullOrEmpty` string extension and neither has a side effect. The in-code comment correctly declines to claim it repairs a fault.

### The controller partial split

The relocation of `PopulateWithCurrent`, `BindExcludeStoreCheckbox` and `GetRelativeFsPath` into `StoreWrapperController.Display.cs` is verbatim, verified by diffing the removed block in `StoreWrapperController.cs` against the added block in the new file: outside the deliberate AC6, AC7 and AC8 edits, the relocated text including the commented-out block is byte-identical. Both parts carry the `partial` keyword, the new file opens with `#nullable enable` matching the original, and both new production files have hand-added compile entries, which is essential in a non-SDK-style project because a missing entry produces a silently absent file rather than a build error. The result is 388 and 173 lines, both within the cap, which is the outcome D4 was written to produce.

---

## Findings

### CR-1 — The AC6 retry bound is stated more tightly than the code enforces (Medium, advisory)

**Location.** `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` lines 41-51, and the corresponding comment at `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` lines 214-219.

**Rule.** General Code Change Policy section 5.3 — "Comment **why**, not what. Keep comments synchronized with behavior." Also the evidence-first wording requirement in `.claude/rules/tonality.md`: match the strength of the wording to the strength of the evidence.

**The claim.** Both in-code comments state the retry is attempted "at most once per dialog open and only when the address is null, which bounds the added UI-thread latency to the single lookup startup already performs." `spec.md` lines 491-493, Non-Goals item 8 and risk 1 all repeat the same bound, and it is the stated basis on which the reintroduced synchronous COM read was accepted.

**Verification basis.** `PopulateWithCurrent` has exactly one production call site, at `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` line 169, inside `DisplayName_SelectedValueChanged`. That handler fires on **every** store selection change, not only at dialog open — a fact `spec.md` line 491 itself acknowledges when it says the method "runs both when the dialog opens and on every store re-selection." The retry gate is `Current.UserEmailAddress is null`, and when the lookup fails, `RefreshUserEmailAddress` assigns null back to `UserEmailAddress`, so the gate remains open. Consequently, for a store whose Exchange lookup keeps failing, every re-selection of that store runs the full synchronous COM chain again.

**Impact.** The true bound is one blocking COM chain per `PopulateWithCurrent` invocation on a store whose address is still null — unbounded in the number of user selection changes, not one per dialog open. On the chain that `spec.md` documents as independently demonstrated capable of long UI-thread blocks, a user cycling the store combo box on a mailbox with a persistent Exchange failure incurs one such block per cycle. The mitigating facts are that a successful lookup populates the address and permanently closes the gate for that store within the session, and that the failing case is exactly the case the user is trying to diagnose.

**Why this is not blocking.** The authoritative AC6 checkbox text requires only that "the lookup is retried when the dialog opens," which the code satisfies. The overstated bound is in the plan, the specification's supporting prose and the code comments, not in the criterion.

**Recommendation.** Either add a per-store or per-controller attempted flag so the retry is genuinely once per dialog open, or correct the three comments and the specification prose to state the actual bound. The first is a small change: a `bool` field on the controller set on the first retry and reset in `Launch`.

### CR-2 — The explicit save performs file I/O and an unbounded lock wait on the UI thread (Low, advisory)

**Location.** `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` line 290 (`Model.SerializeNow();` inside `SaveChanges`), reaching `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs` lines 519-546.

**Rule.** General Code Change Policy section 6.2 — isolate I/O; and section 3 — document non-obvious failure modes.

**Verification basis.** `SaveChanges` is called from `ButtonSave_Click` and from `DisplayName_SelectedValueChanged`, both UI-thread paths. `SerializeThreadSafe` acquires the write lock with `TryEnterWriteLock(-1)`, an infinite timeout, and then performs `CreateStreamWriter(filePath)` — `File.CreateText` in production — plus a full Newtonsoft serialization of the whole stores wrapper, all inline on the calling thread. Before this change, `SaveChanges` called `Serialize()`, which returned immediately after arming a timer and performed no I/O on the caller's thread.

**Impact.** The change deliberately trades a lost-write risk for a UI-thread stall risk, which is the correct trade for this requirement. The residual is that if the deferred timer callback is already inside the write lock on a ThreadPool thread, the UI thread waits with no timeout and no diagnostic. The window is short for a settings JSON of this size, and no evidence of an actual stall exists, so the severity is low. What is missing is acknowledgement: `SerializeNow`'s `// why:` comment describes the lock as a correctness mechanism but does not note that the caller now blocks on it.

**Recommendation.** Record the accepted latency in the comment, or use a bounded `TryEnterWriteLock(timeout)` with an error log when acquisition fails, so a stall is diagnosable rather than silent. Note that `SerializeThreadSafe`'s existing `if (TryEnterWriteLock(-1))` already has a false branch that silently does nothing, which a bounded timeout would make reachable — that branch would need an error log to avoid reintroducing exactly the silent-failure class this issue exists to remove.

### CR-3 — `SerializeNow` re-arms the single-shot guard early, permitting a redundant second timer (Low, advisory)

**Location.** `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs` line 543 (the `finally` reset), reached from `SerializeNow` at line 497; interacting with `RequestSerialization` at lines 595-604.

**Rule.** General Code Change Policy section 1 — simplicity and avoidance of non-obvious state interactions; General Unit Test Policy — concurrency behaviour must be covered when relevant.

**Verification basis.** `SerializeThreadSafe`'s `finally` replaces `_serializationRequested` with a fresh `ThreadSafeSingleShotGuard`. Before this change that reset occurred only from the timer callback, after the deferred write had completed. `SerializeNow` now performs the same reset up to three seconds early, while a timer armed by an earlier `Serialize()` may still be pending. In that window a subsequent `Serialize()` passes `CheckAndSetFirstCall` and arms a second timer, overwriting the `_timer` field at line 599 without disposing the first. Both timers then fire `SerializeThreadSafe` with their separately captured file paths.

**Impact.** The consequence is a redundant write of current state, not a lost write and not a corrupted file, because each write serializes the live `_parent`. The `_timer` overwrite-without-dispose pattern is pre-existing: `_timer` is declared at line 584 and assigned at line 599, and a search of the file finds no `Dispose` call on it on any path. This change widens the window in which the overwrite can occur but does not create the pattern.

**Why this is not blocking.** No lost or corrupted write results, and the redundant write is idempotent in effect.

**Recommendation.** Consider stopping or disposing any pending `_timer` inside `SerializeNow` before writing, which would make the explicit save fully supersede the deferred one. If the interleaving is left as is, an interleaving test — deferred request, then explicit save, then fire the pending timer, asserting the write count — would pin the accepted behaviour. No such test exists today; both AC4 tests exercise one path at a time.

### CR-4 — `SmartSerializable.cs` grew 45 lines while already 113 lines over the cap (Low, advisory)

**Location.** `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs`.

**Rule.** General Code Change Policy section 4.1 and `.claude/rules/general-code-change.md` — "No production code, test code, or reusable script file may exceed 500 lines."

**Verification basis.** 613 lines at the base commit, 658 after the change, a delta of +45. Independently corroborated by the patch's own two hunk headers for the file (`@@ -439,11 +439,35 @@` = +24, `@@ -453,6 +477,27 @@` = +21), which reconcile 613 to 658 exactly.

**Disposition.** The overage is pre-existing and is deliberately not resolved here by design decision D5, Non-Goals item 1 and risk 5, on the stated ground that splitting a shared reusable-type-classes file during a parallel run would create merge contention with concurrently running sibling work items. That rationale is sound and the file is not made worse in any structural sense — the two additions are cohesive with the existing Serialization region and both are documented.

**One wording correction.** `spec.md` line 195 states this change "adds a small number of lines to it." A 45-line addition, 7.3 percent growth on an already over-cap file, is more than that phrasing conveys. The evidence artifact `p5-t8` reports both counts accurately, so the record is not misleading; only the specification's prose understates the delta.

**Recommendation.** None under this issue. The split should be raised as its own work item, as D5 states.

### CR-5 — The AC5 double-persistence path is retained; divergence between the two stores is not itself loud (Low, advisory)

**Location.** `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` lines 36-45, and `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` lines 285-290.

**Verification basis.** `SaveChanges` writes the junk selections into the JSON model (`Current.JunkCertain = JunkEmail; Current.JunkPotential = JunkPotential;`) and then calls `PersistJunkFolderSelections()`, which routes through the new seam into `ApplyJunkFolderSelections`, which writes `Properties.Settings.Default.OlJunkCertain` and `.JunkPotential` and calls `Save()`. Both mechanisms still run on every save.

**Assessment.** AC5 is a disjunction — "either removed or made to fail loudly" — and design decision D2, which is recorded in the authoritative specification, selects the fail-loudly reading with an explicit rationale (removal would break the globals junk-folder accessors that read the settings values back). What the change makes loud is the seam-absence case. What is not made loud is a disagreement between the two stores once both are writing, which `spec.md` risk 4 records as a known and accepted rollout consequence and which step 9 of the AC3 manual procedure is written to check.

**Why this is not a finding against the change.** The criterion is satisfied under the reading the authoritative AC source itself selects, and the residual divergence risk is disclosed rather than concealed. It is recorded here so the reviewer of the follow-up work item has the full picture.

**Recommendation.** None under this issue. The removal of the second mechanism, which requires rehoming the globals junk-folder accessors onto the JSON model, belongs in a separate work item.

### CR-6 — The production seam forwarder has no test coverage (Low, advisory)

**Location.** `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` lines 54-57.

**Verification basis.** The post-change Cobertura document measures this file at 21 of 69 lines and the file carries no class-level coverage-exclusion attribute, so the zero is real rather than an artefact of exclusion. The single executable changed line in this file — the explicit implementation's forwarding expression — is uncovered, because driving it would call `Properties.Settings.Default.Save()` and write to the .NET user settings store, which the unit test policy forbids.

**Impact.** The forwarder's argument order is verified by code read only. This reviewer performed that read: the explicit implementation passes `(junkCertainRelativePath, junkPotentialRelativePath)` positionally to the internal method, whose body routes the first to `WriteJunkCertainSetting` and the second to `WriteJunkPotentialSetting`. The order is correct and the parameter names match the interface, so a transposition is not present. The seam contract itself is separately pinned on the UtilitiesCS side by `PersistJunkFolderSelections_PassesJunkCertainPathFirst`, which asserts distinguishable values in both positions against a recording double.

**Why this is not blocking.** The uncovered line is a one-line positional pass-through, the contract is pinned on the calling side, and the aggregate changed-line figure of 91.09 percent clears the applicable gate.

**Recommendation.** None under this issue. Introducing a settings seam on the TaskMaster side would make the forwarder testable and is a reasonable separate improvement.

---

## Test quality assessment

Measured against `.claude/rules/general-unit-test.md` and the C# Unit Test Policy. The result is strong.

**What is done well.**

- The log-capture helpers are the most delicate part of the new tests and are handled carefully. `AttachRootMemoryAppender` documents *why* it attaches to the root logger rather than a named one: the serializer's logger is initialised from the declaring type reported by reflection over a member of a generic type, which resolves to the generic type definition, so an appender attached to a closed constructed type's full name would capture nothing. That is a real trap, correctly identified and correctly avoided. Both helpers restore the previous logger level and the repository's `Configured` flag through a `restore` delegate invoked in a `finally`.
- Both assertions on captured log events filter by a name that occurs nowhere else in the test project — `SerializeGuardProbeItem` for the serializer and `IJunkFolderSelectionSink` for the controller — and assert existence rather than an exact count, with the stated reason that a concurrently running class could only add events. Both host classes additionally carry `[DoNotParallelize]`. The AC5 test pairs the event assertion with `ApplyCallCount.Should().Be(0)`, which is what actually attributes the event to that test rather than to a neighbour.
- `NonSinkOlObjects` is a genuinely discriminating double, as set out above. Retargeting the pre-existing negative test rather than deleting it preserves coverage of the loud-failure branch, which is the right call and avoids the common mistake of removing a test whose subject changed.
- The four `GetSmtpAddressFromStore_*` cases are properly table-shaped over the fallback order, each isolating one step, and case 4 asserts on `LastSmtpLookupError` rather than only on the null return, which pins the mechanism the controller actually consumes.
- No test can reach `MyBox.ShowDialog` or any live Outlook worker. The AC5 tests operate on the UtilitiesCS side against doubles and never enter `LoadJunkPotential` or `LoadJunkCertain`, the only dialog-raising members in the touched files.
- Every FluentAssertions call on a non-obvious expectation supplies a `because` reason, so a failure message states the criterion rather than only the mismatch.

**Minor points, none rising to a finding.**

- `SmartSerializableSerializeGuardTests` declares its own harness and probe type with a comment explaining why the established harness could not be reused (it is a private nested class in another file). That is the right justification to record, though it means the probe type must implement thirteen `ISmartSerializable<T>` members that the tests never exercise. The `#pragma warning disable CS0067` on the probe's `PropertyChanged` event is unavoidable and correctly scoped to the single member.
- `SmartSerializableSerializeGuardTests` sits one directory level shallower than a strict mirror of the production file's path, matching the established local convention for this class's sibling test files. The "match the existing style" rule governs and is satisfied.
- The two fake paths use a `X:\` drive that does not exist, and neither is ever opened because all writes go through the injected seam. Using a non-existent root is a good defensive choice: if the seam injection were ever broken, the test would fail loudly rather than silently writing somewhere real.

**Determinism.** No `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, real wall-clock wait, or temporary file appears anywhere in the patch, verified by pattern search. The three-second deferred timer is advanced by an explicit `FireElapsed()` call on a manual-fire double injected through the existing `TimerFactory` seam.

**Coverage exclusion.** Zero occurrences of `ExcludeFromCodeCoverage` in the patch. No production file was excluded from measurement, and both new production files are accounted for in the post-change Cobertura document — the display partial as a measured class at 100 percent line coverage, and the interface file legitimately absent because an interface declaration emits no IL.

---

## Design and maintainability

The design decisions hold up under review.

- **D1 (fix at the call site, not the shared serializer)** is the right call. The shared overload's null return is a documented fail-soft contract relied on by a second production caller, and the mechanical argument in the specification is correct: on the file-absent path there is no instance to copy onto, so "adopt the loader's configuration" is not expressible inside that overload without changing its contract.
- **D2 (a dedicated interface rather than extending `IOlObjects`)** avoids forcing changes on four existing test stubs and keeps the public surface narrow through explicit implementation. Correct.
- **D3 (flush on the explicit save path, not a shutdown handler)** is well grounded: the VSTO shutdown event is documented in-repo as no longer raised, so a flush placed there would never run. Verified as a design rationale rather than accepted on assertion.
- **D4 (partial split)** produces two files comfortably within the cap and follows an established in-repo precedent.
- **D6 (declared test inversion)** is correctly framed as a strengthening and is disclosed in advance, which is exactly the treatment the General Code Change Policy's "existing tests are part of the spec" rule requires.

The naming throughout is descriptive (`TryGetSerializationPath`, `RefreshUserEmailAddress`, `BuildUserEmailUnavailableText`, `LastSmtpLookupError`), the accessibility choices are deliberate and justified in comments (`internal static TrimStorePrefix` rather than private, with the `InternalsVisibleTo` rationale stated), and every non-obvious edit carries a `// why:` comment naming the mechanism it repairs rather than restating what the code does.

One small note on `TryGetSerializationPath`: its `out string filePath` parameter is declared non-nullable but carries null on the failure path. Neither caller uses the value after a false return, so no defect arises, and the nullable rebuild passed. Declaring it `out string? filePath` would express the contract more precisely.

---

## Conclusion

**PASS. Zero blocking findings.** Six advisory findings recorded, of which CR-1 is the only one worth acting on in the near term and none of which requires remediation before merge. No remediation-inputs artifact is produced.
