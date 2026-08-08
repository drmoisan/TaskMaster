# Research: `QuickFiler/Controllers/KaChar.cs`

Timestamp: 2026-08-07T22-05
Feature: `quickfiler-keyboard-actions-coverage` (issue #430, epic child F3 of #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`
Scope: read-only research. No production or test file was modified.

---

## 1. File Under Research

| Property | Value |
| --- | --- |
| Path | `QuickFiler/Controllers/KaChar.cs` |
| Line count | 99 (file ends at line 100 with the trailing newline) |
| Types declared | **Two** public classes in one file: `KaChar` (lines 11-56) and `KaCharAsync` (lines 58-98) |
| Compiled by | `QuickFiler/QuickFiler.csproj` line 307 |
| Target framework | `v4.8.1`, `LangVersion=preview` |
| `[ExcludeFromCodeCoverage]` present | **No.** No `System.Diagnostics.CodeAnalysis` using directive and no attribute on either class. |
| Existing tests | `QuickFiler.Test/Controllers/KaCharTests.cs` — 10 test methods (6 for `KaChar`, 4 for `KaCharAsync`). Registered at `QuickFiler.Test/QuickFiler.Test.csproj` line 94. |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.** This artifact records evidence supporting a `testable` classification; it does not classify the file. |

### 1.1 Exemption posture

Neither class references `Microsoft.Office.Interop.Outlook`, derives from a WinForms type, or is Designer-generated, so none of the three `CLAUDE.md` § UT2 exemption categories (a), (b), or (c) applies. Both are pure value objects. Under the epic's ratified reconciliation (`epic.md` Shared Design section 1 — "refactor first, exempt only the irreducible remainder"), there is no irreducible remainder here and no exemption is available.

The single `using System.Windows.Forms;` at line 6 exists only so `KaChar.DelegateType` (line 45) can name `Keys`; `Keys` is a plain enum and requires no message loop, no STA apartment, and no form.

### 1.2 How coverage will be measured

Numeric per-file line coverage is not established here. It will be measured at execution time with **F1's per-file coverage report harness**, derived from the Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and committed under `<FEATURE>/evidence/qa-gates/`. The analysis below is static: members and branches enumerated, then mapped to existing test methods by name.

---

## 2. Structural Inventory

### 2.1 `KaChar : IKbdAction<char, Action<char>>` (lines 11-56)

| # | Member | Lines | Notes |
| --- | --- | --- | --- |
| C1 | `KaChar()` | 13 | Empty body. Required by the `new()` constraint on `KbdActions<TKey, UClass, VDelegate>` (`KbdActions.cs` line 15) and invoked at `KbdActions.cs` line 99. |
| C2 | `KaChar(string sourceId, char key, Action<char> action)` | 15-20 | Assigns through the **properties**, so the three setters execute. No null or range guard. |
| C3 | `SourceId` get / set | 22-27 | Backing field `_sourceId` (line 22). |
| C4 | `Key` get / set | 29-34 | Backing field `_key` (line 29). |
| C5 | `Delegate` get / set | 36-41 | Backing field `_action` (line 36). |
| C6 | `DelegateType` get | 43-46 | Returns `typeof(Action<Keys>)`. **Mismatched** — see gap G1. Getter only; no setter. **Not a member of `IKbdAction<,>`** (it is commented out at `IKbdAction.cs` line 16). |
| C7 | `KeyEquals(char other)` | 48 | `Key == other`. Expression-bodied, single line, no branch. |
| C8 | `Update` get / set | 50-55 | `Action<string>`, backing field `_update` (line 50). **Not a member of `IKbdAction<,>`** (commented out at `IKbdAction.cs` line 15). |

### 2.2 `KaCharAsync : IKbdAction<char, Func<char, Task>>` (lines 58-98)

| # | Member | Lines | Notes |
| --- | --- | --- | --- |
| A1 | `KaCharAsync()` | 60 | Empty body. Invoked through the `new()` constraint at `KbdActions.cs` line 99. |
| A2 | `KaCharAsync(string sourceId, char key, Func<char, Task> function)` | 62-67 | Assigns through properties. No guards. |
| A3 | `SourceId` get / set | 69-74 | |
| A4 | `Key` get / set | 76-81 | |
| A5 | `Delegate` get / set | 83-88 | `Func<char, Task>`. |
| A6 | `KeyEquals(char other)` | 90 | `Key == other`. |
| A7 | `Update` get / set | 92-97 | Unused. |
| — | *no `DelegateType`* | — | Structural asymmetry with `KaChar`. Recorded, not a defect in itself. |

### 2.3 Dependencies

- **COM / Outlook Interop:** none. No `Microsoft.Office.Interop.Outlook` using directive.
- **WinForms:** `using System.Windows.Forms;` (line 6) used solely for the `Keys` type name at line 45. No control, no form, no handle, no message loop.
- **Clock / timers / randomness:** none. No `DateTime`, `TimeProvider`, `Task.Delay`, `Thread.Sleep`, or `Random` anywhere in the file.
- **Asynchrony:** `KaCharAsync` has **no `async` method and no `await`**. Its "Async" suffix describes only the *shape of the stored delegate* (`Func<char, Task>`). The type itself is fully synchronous. Awaiting is done by the caller, `KeyboardHandler.KeyDownTaskAsync` (`KeyboardHandler.cs` line 176).
- **Unused members verified by search:** `rg 'DelegateType|\.Activated|ToggleControl' --glob '**/*.cs'` returns, for `DelegateType`, only the two declarations (`KaChar.cs:43`, `KaKey.cs:43`) and the commented-out interface line (`IKbdAction.cs:16`). `rg 'Update\s*=|\.Update\(' --glob 'QuickFiler/**/*.cs'` returns only `KaStringAsync.cs:25`. **No production code reads or writes `KaChar.Update`, `KaCharAsync.Update`, or `KaChar.DelegateType`.**

### 2.4 Production construction sites

`rg 'new Ka(Char|CharAsync)\s*\(' --glob 'QuickFiler/**/*.cs'`:

- `KaChar`: `EfcFormController.cs` lines 634, 639, 644, 649, 654, 659, 664, 669 (eight registrations inside `GetKbdActions()`); `QfcCollectionController.cs` lines 1259-1263 registers `char` actions via `KbdActions.Add(string, TKey, VDelegate)`, which constructs `KaChar` through the `new()` constraint rather than a literal `new KaChar(...)`.
- `KaCharAsync`: `EfcFormController.cs` lines 577, 578, 587, 588, 589, 594, 595, 596 (eight registrations inside `GetAsyncCharacterActions()`).

Both are also declared as type arguments in `IQfcKeyboardHandler.cs` lines 21-22 and `KeyboardHandler.cs` lines 44-56.

---

## 3. Existing Test Coverage (static analysis)

Source: `QuickFiler.Test/Controllers/KaCharTests.cs` (lines 1-155).

| Member / branch | Lines | Covered by (test method name) |
| --- | --- | --- |
| C1 `KaChar()` | 13 | `KaChar_ParameterlessConstructor_LeavesNullDelegate` |
| C2 3-arg ctor | 15-20 | `KaChar_Constructor_StoresSourceIdKeyAndDelegate`, `KaChar_Delegate_DispatchesToSuppliedAction`, `KaChar_KeyEquals_MatchesSameCharAndRejectsOther`, `KaChar_Constructor_NullDelegate_IsStoredNotRejected`, `KaChar_DefaultCharKey_IsSupported` |
| C3 `SourceId` getter | 24 | `KaChar_Constructor_StoresSourceIdKeyAndDelegate`, `KaChar_ParameterlessConstructor_LeavesNullDelegate` |
| C3 `SourceId` setter | 25 | `KaChar_Constructor_StoresSourceIdKeyAndDelegate` (via ctor line 17) |
| C3 `SourceId` setter **after construction** | 25 | **none** |
| C4 `Key` getter | 31 | `KaChar_Constructor_StoresSourceIdKeyAndDelegate` |
| C4 `Key` setter | 32 | via ctor line 18 |
| C4 `Key` setter **after construction** | 32 | **none** |
| C5 `Delegate` getter | 38 | `KaChar_Constructor_StoresSourceIdKeyAndDelegate`, `KaChar_Delegate_DispatchesToSuppliedAction`, `KaChar_ParameterlessConstructor_LeavesNullDelegate`, `KaChar_Constructor_NullDelegate_IsStoredNotRejected` |
| C5 `Delegate` setter | 39 | via ctor line 19 |
| C5 `Delegate` setter **after construction** | 39 | **none** |
| C6 **`DelegateType` getter** | 45 | **none** |
| C7 `KeyEquals` — true | 48 | `KaChar_KeyEquals_MatchesSameCharAndRejectsOther`, `KaChar_DefaultCharKey_IsSupported` |
| C7 `KeyEquals` — false | 48 | `KaChar_KeyEquals_MatchesSameCharAndRejectsOther` |
| C8 **`Update` getter** | 52 | **none** |
| C8 **`Update` setter** | 53 | **none** |
| A1 **`KaCharAsync()`** | 60 | **none** |
| A2 3-arg ctor | 62-67 | `KaCharAsync_Constructor_StoresSourceIdKeyAndDelegate`, `KaCharAsync_Delegate_AwaitsAndCompletesSynchronously`, `KaCharAsync_KeyEquals_MatchesSameCharAndRejectsOther`, `KaCharAsync_Constructor_NullDelegate_IsStoredNotRejected` |
| A3 `SourceId` get / set | 71-72 | `KaCharAsync_Constructor_StoresSourceIdKeyAndDelegate` (getter; setter via ctor) |
| A3 `SourceId` setter **after construction** | 72 | **none** |
| A4 `Key` get / set | 78-79 | `KaCharAsync_Constructor_StoresSourceIdKeyAndDelegate` (getter; setter via ctor) |
| A4 `Key` setter **after construction** | 79 | **none** |
| A5 `Delegate` get / set | 85-86 | `KaCharAsync_Constructor_StoresSourceIdKeyAndDelegate`, `KaCharAsync_Delegate_AwaitsAndCompletesSynchronously`, `KaCharAsync_Constructor_NullDelegate_IsStoredNotRejected` |
| A5 `Delegate` setter **after construction** | 86 | **none** |
| A6 `KeyEquals` — true / false | 90 | `KaCharAsync_KeyEquals_MatchesSameCharAndRejectsOther` |
| A7 **`Update` getter** | 94 | **none** |
| A7 **`Update` setter** | 95 | **none** |
| Delegate invocation **throwing** (either class) | — | **none** |

**Corroborating search:** `rg '(charActionsAsync|charActions)\.Add\(' QuickFiler.Test` returns no matches, confirming that no other test in the repository reaches `KaCharAsync()` indirectly through `KbdActions`'s `new()` constraint.

---

## 4. Coverage Gaps

Six unexecuted line regions plus three untested contracts. Gaps G1-G4 are genuine unexecuted lines and are the priority.

### G1 — `KaChar.DelegateType` (lines 43-46) is unexecuted, and returns the wrong type

```csharp
public Type DelegateType
{
    get => typeof(Action<Keys>);
}
```

`KaChar` implements `IKbdAction<char, Action<char>>`. Its delegate is `Action<char>` (line 37). `DelegateType` reports `typeof(Action<Keys>)` — the value that is correct for `KaKey` (`KaKey.cs` line 45), not for `KaChar`. This is a copy-paste defect.

Impact assessment, evidence-based:
- **No consumer exists.** `rg 'DelegateType'` across all `*.cs` yields only the two declarations and the commented-out interface member. Production impact today is **nil**.
- The member is public API on a type consumed by `KeyboardHandler.cs`, `QfcCollectionController.cs` (F11), and `EfcFormController.cs` (F9).

**Disposition: cover, do not correct, and promote a defect issue.** Correcting the return value is a behavior change to a public member of a type consumed outside this child's file set, which the child's additive-only constraint forbids (`issue.md` lines 65-70). Deleting the member is likewise non-additive. TC-1 below is a **characterization** test that records the current value and cites the promoted issue. Per the recorded `promote-latent-defects-to-issues` feedback, prose in this folder disappears at merge; a real GitHub issue is required.

### G2 — `KaChar.Update` (lines 50-55) is unexecuted and unused

`Action<string> Update` has no reader or writer anywhere in the repository. It is not on `IKbdAction<,>` — line 15 of `IKbdAction.cs` shows it commented out:

```csharp
//Action<string> Update { get; set; }
//Type DelegateType { get; }
```

The commented-out interface lines explain the orphan: `Update` and `DelegateType` were once contract members, were withdrawn from the interface, and were left behind on the implementers. Only `KaStringAsync` still uses its `Update` (`KaStringAsync.cs` lines 62, 73).

**Disposition: cover, do not delete.** Same additive-only reasoning as G1. Two trivial round-trip tests execute both accessors.

### G3 — `KaCharAsync.Update` (lines 92-97) is unexecuted and unused

Identical to G2 for the async sibling.

### G4 — `KaCharAsync()` parameterless constructor (line 60) is unexecuted

`KaCharTests.cs` tests `KaChar`'s parameterless constructor (`KaChar_ParameterlessConstructor_LeavesNullDelegate`) but has no equivalent for `KaCharAsync`. This constructor is **not dead code**: `KbdActions<char, KaCharAsync, Func<char, Task>>.Add(sourceId, key, delegate)` invokes it through the `new()` constraint at `KbdActions.cs` line 99. Confirmed that no existing test reaches it that way (section 3, corroborating search).

### G5 — Post-construction setters are unexecuted on both classes

Every setter (`SourceId`, `Key`, `Delegate` on both classes — lines 25, 32, 39, 72, 79, 86) executes only through the constructors. The setters are exercised in production by `KbdActions.Add` (`KbdActions.cs` lines 100-102), which sets all three on a freshly `new()`-ed instance. Reassignment after construction — which is what the indexer setter path does via `element.Delegate = value` (`KbdActions.cs` line 44) — is never proven for these two types. Note: `KbdActions.cs` line 44 *is* covered, but only for `KaKey`.

**Coverage-mechanics note:** these setter lines are already marked covered by the constructor tests, so closing G5 does not move the line-coverage number. Its value is contract proof for the mutable-property surface that `KbdActions.Add` and the `KbdActions` indexer setter depend on.

### G6 — `char` boundary values are only half-covered

`KaChar_DefaultCharKey_IsSupported` covers `'\0'` for `KaChar`. Not covered:
- `char.MaxValue` (`'￿'`) for either class.
- `'\0'` for `KaCharAsync`.

Relevance: `KeyboardHandler.cs` lines 124, 128, 171, 176 cast `e.KeyValue` (an `int`) to `char` with an unchecked cast, so the full `char` range is reachable at the boundary.

### G7 — Error handling on delegate invocation is untested for both classes

Neither class guards a null delegate (proven by `KaChar_Constructor_NullDelegate_IsStoredNotRejected` and `KaCharAsync_Constructor_NullDelegate_IsStoredNotRejected`), and neither test invokes the null delegate. Nor does any test cover a delegate that **throws**:
- `KaChar.Delegate` is `Action<char>` — an exception propagates synchronously to the caller. `KeyboardHandler.KeyboardHandler_KeyDown` (line 122) has **no try/catch**, so the exception escapes to the WinForms message pump.
- `KaCharAsync.Delegate` is `Func<char, Task>` — a fault surfaces on `await`. `KeyboardHandler.KeyboardHandler_KeyDownAsync` (lines 137-147) does catch and log.

This asymmetry in the surrounding error handling is worth pinning down at the value-object level, deterministically, via `Task.FromException`.

### Not gaps (recorded so the planner does not re-open them)

- `KaCharAsync` has no `DelegateType` member, so there is nothing to cover there.
- `KeyEquals` on both classes is a single expression with both outcomes already covered.
- No timing, COM, or UI dependency exists in this file, so there is no untestable region.

---

## 5. Seam Requirements

**None required. Recommendation: make zero production changes to `KaChar.cs`.**

Assessment against the `.claude/rules/csharp.md` seam hierarchy (lines 49-53):

| Candidate dependency | Assessment |
| --- | --- |
| Interface seam (level 1) | Nothing to extract. Both classes are data holders with one comparison expression each. `IKbdAction<char, ...>` already exists as the contract. |
| Injectable delegate seam (level 2) | The delegate **is** the injected collaborator. `Action<char>` and `Func<char, Task>` are supplied by the caller and can be any test lambda. No further seam is meaningful. |
| Adapter seam (level 3) | No static or third-party API is called. `typeof(Action<Keys>)` at line 45 is a compile-time metadata token, not a call. |
| COM / Outlook | Absent. |
| WinForms control / handle / message loop | Absent. `Keys` (line 45) is an enum. |
| Clock / timer / RNG | Absent. |

**STA last-resort clause (epic.md Shared Design section 3): not applicable.** No WinForms control is constructed. No `*.StaTests.cs` file is warranted; all proposed tests run on the default MSTest apartment.

**Determinism (`.claude/rules/general-unit-test.md` § Determinism Infrastructure):** satisfied without infrastructure. No wall-clock read exists in production, so no `TimeProvider` or `FakeTimeProvider` is needed. All async assertions use `Task.CompletedTask` / `Task.FromException`, which complete synchronously — no `Task.Delay`, no `Thread.Sleep`, no fake timers required. The existing `KaCharTests.cs` already follows this pattern (its class comment at lines 14-15 states "No timing dependency is introduced") and **contains no wall-clock wait** — verified across all 155 lines. No policy defect in the existing suite.

---

## 6. Cross-Child Contract Impact

**Recommended production change set for this file: empty. Cross-child impact: none.**

Call sites of `KaChar` / `KaCharAsync` outside this child's file set:

| Consumer | Lines | Owning child | Members used |
| --- | --- | --- | --- |
| `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` | 21-22 | **F3 (this child)** | type arguments only |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 44-56, 124-128, 171-176 | **F3 (this child)** | type arguments; `Delegate` via `KbdActions` indexer |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 583-584, 743-744, 1259-1263 | **F11** | `KbdActions.Add(...)` -> `new()` + `SourceId`/`Key`/`Delegate` setters |
| `QuickFiler/Controllers/EfcFormController.cs` | 568-596, 625-669 | **F9** | 3-arg constructors (16 registrations) |
| `QuickFiler.Test/Controllers/QfcItemController*.cs` | see `04-KbdActions.md` §6 | test-side | type arguments only |

**Additive-vs-breaking determination:** no production edit is proposed, so the determination is *no change*.

Two changes a future planner might be tempted to make, both **breaking** and both out of scope for this child:
1. Correcting `DelegateType` to `typeof(Action<char>)` — changes a public member's observable return value.
2. Deleting the unused `Update` and `DelegateType` members — removes public API surface from a type consumed by F9 and F11.

Both belong in the defect issue promoted per G1, not here.

The only files this child modifies for `KaChar` coverage: `QuickFiler.Test/Controllers/KaCharTests.cs` (**append only** — no `.csproj` edit needed, since line 94 already registers it). Appending to an existing test file rather than adding a new one keeps `QuickFiler.Test.csproj` untouched for this file, reducing the merge-conflict surface shared with F9, F10, and F11.

---

## 7. Proposed Test Cases

**Target file:** `QuickFiler.Test/Controllers/KaCharTests.cs` (existing, 155 lines — append; the file stays far below the 500-line limit).
**Companion edits:** none. `QuickFiler.Test.csproj` line 94 already registers the file.

Framework: MSTest `[TestClass]`/`[TestMethod]`, FluentAssertions. **No Moq** — both types under test are the concrete collaborators; a mock would isolate nothing. Arrange-Act-Assert. No STA, no timers, no temporary files, no external services.

Each case was cross-referenced against section 3; none duplicates an existing test.

| ID | Method name | Gap | Arrange / Act / Assert | Seam or mock |
| --- | --- | --- | --- | --- |
| TC-1 | `KaChar_DelegateType_ReturnsActionOfKeys_CharacterizingKnownMismatch` | G1 | **A:** `var ka = new KaChar("src", 'a', _ => { })`. **Act:** `var t = ka.DelegateType`. **Assert:** `t.Should().Be(typeof(Action<Keys>), because: "characterization only — KaChar stores Action<char>, so this value is a known copy-paste defect tracked by issue <N>; it is recorded, not endorsed")`. XML comment must name it a characterization test and cite the promoted issue. | none |
| TC-2 | `KaChar_Update_DefaultsToNullAndRoundTripsAssignedAction` | G2 | **A:** `var ka = new KaChar("src", 'a', _ => { })`; `Action<string> probe = _ => { }`. **Act:** read `ka.Update` (expect null), assign `ka.Update = probe`, read again. **Assert:** first read `.Should().BeNull()`; second `.Should().BeSameAs(probe)`. Comment notes `Update` is not on `IKbdAction<,>` (`IKbdAction.cs` line 15, commented out) and has no production consumer. | none |
| TC-3 | `KaChar_Update_InvokesAssignedActionWithSuppliedString` | G2 | **A:** `string received = null; var ka = new KaChar(); ka.Update = s => received = s`. **Act:** `ka.Update("x")`. **Assert:** `received.Should().Be("x")`. | none |
| TC-4 | `KaChar_SourceIdKeyAndDelegateSetters_ReplaceConstructedValues` | G5 | **A:** `var ka = new KaChar("first", 'a', _ => { })`; `Action<char> replacement = _ => { }`. **Act:** set `SourceId = "second"`, `Key = 'b'`, `Delegate = replacement`. **Assert:** all three read back the new values; `Delegate.Should().BeSameAs(replacement)`. Mirrors the mutation `KbdActions.Add` (lines 100-102) and the `KbdActions` indexer setter (line 44) perform. | none |
| TC-5 | `KaChar_KeyEquals_AtCharMaxValueBoundary_MatchesAndRejects` | G6 | **A:** `var ka = new KaChar("src", char.MaxValue, _ => { })`. **Act/Assert:** `ka.KeyEquals(char.MaxValue).Should().BeTrue()`; `ka.KeyEquals((char)(char.MaxValue - 1)).Should().BeFalse()`. Comment cites the unchecked `(char)e.KeyValue` casts at `KeyboardHandler.cs` lines 124, 128. | none |
| TC-6 | `KaChar_Delegate_WhenActionThrows_PropagatesToCaller` | G7 | **A:** `var ka = new KaChar("src", 'a', _ => throw new InvalidOperationException("boom"))`. **Act:** `Action act = () => ka.Delegate('a')`. **Assert:** `act.Should().Throw<InvalidOperationException>().WithMessage("boom")`, `because: "KaChar performs no exception shielding; KeyboardHandler_KeyDown (KeyboardHandler.cs:114-131) has no try/catch, so the exception reaches the message pump"`. | none |
| TC-7 | `KaChar_Delegate_WhenNull_InvocationThrowsNullReferenceException` | G7 | **A:** `var ka = new KaChar("src", 'a', null)`. **Act:** `Action act = () => ka.Delegate('a')`. **Assert:** `act.Should().Throw<NullReferenceException>()`. Characterization: the constructor stores null without guarding (already proven by `KaChar_Constructor_NullDelegate_IsStoredNotRejected`); this case proves the **consequence**, which that test does not. | none |
| TC-8 | `KaCharAsync_ParameterlessConstructor_LeavesNullDelegateAndDefaultKey` | G4 | **A/Act:** `var ka = new KaCharAsync()`. **Assert:** `ka.Delegate.Should().BeNull()`; `ka.SourceId.Should().BeNull()`; `ka.Key.Should().Be('\0')`. Comment states this ctor is reached in production through the `new()` constraint at `KbdActions.cs` line 99. | none |
| TC-9 | `KaCharAsync_Update_DefaultsToNullAndRoundTripsAssignedAction` | G3 | **A:** `var ka = new KaCharAsync("src", 'a', _ => Task.CompletedTask)`; `Action<string> probe = _ => { }`. **Act:** read, assign, read. **Assert:** null then `BeSameAs(probe)`. | none |
| TC-10 | `KaCharAsync_SourceIdKeyAndDelegateSetters_ReplaceConstructedValues` | G5 | **A:** `var ka = new KaCharAsync("first", 'a', _ => Task.CompletedTask)`; `Func<char, Task> replacement = _ => Task.CompletedTask`. **Act:** reassign all three. **Assert:** all three read back the new values. | none |
| TC-11 | `KaCharAsync_KeyEquals_AtDefaultAndMaxCharBoundaries_MatchesAndRejects` | G6 | **A:** one instance with `Key = '\0'`, one with `Key = char.MaxValue`. **Act/Assert:** each matches its own key and rejects the other. | none |
| TC-12 | `KaCharAsync_Delegate_WhenFunctionReturnsFaultedTask_AwaitObservesTheFault` | G7 | **A:** `var ka = new KaCharAsync("src", 'a', _ => Task.FromException(new InvalidOperationException("boom")))`. **Act:** `Func<Task> act = async () => await ka.Delegate('a')`. **Assert:** `await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom")`. Deterministic: `Task.FromException` completes synchronously, no delay, no timer. Method is `async Task`. | none |
| TC-13 | `KaCharAsync_Delegate_WhenFunctionThrowsSynchronously_ThrowsBeforeTaskIsReturned` | G7 | **A:** `var ka = new KaCharAsync("src", 'a', _ => throw new InvalidOperationException("boom"))`. **Act:** `Action act = () => ka.Delegate('a')` (note: **not** awaited — the throw happens before a Task exists). **Assert:** `act.Should().Throw<InvalidOperationException>()`. Distinguishes the synchronous-throw path from the faulted-task path in TC-12; `KeyboardHandler.KeyDownTaskAsync` line 176 is inside the caller's try/catch either way, but the two paths differ for any caller that stores the Task before awaiting. | none |

**Count: 13 discrete test cases** (7 for `KaChar`, 6 for `KaCharAsync`). Each is individually nameable and becomes its own atomic plan task per the epic's per-file mandate.

Sequencing note: TC-1 cites the G1 defect issue number in its XML comment. Promote that issue before authoring TC-1, or author TC-1 last.

---

## 8. Risks and Open Questions

1. **TC-1 encodes a known-wrong value.** If a later change corrects `DelegateType`, TC-1 must be updated deliberately. Mitigation: the XML comment names it a characterization test and cites the issue. Reviewers must not read it as an endorsement of the current value.
2. **Three defect issues need promotion from this file.** (a) `KaChar.DelegateType` returns `typeof(Action<Keys>)` instead of `typeof(Action<char>)`; (b) `Update` is orphaned public API on four types (`KaChar`, `KaCharAsync`, `KaKey`, `KaKeyAsync`) with no consumer, a residue of the withdrawn interface members at `IKbdAction.cs` lines 15-16; (c) `KaCharAsync` lacks the `DelegateType` member that `KaChar` has. Per `promote-latent-defects-to-issues`, these must become real GitHub issues. Items (b) and (c) are shared with `06-KaKey.md` — file **one** issue covering the orphan-member cleanup across both files, not four.
3. **Two public classes in one file.** `KaChar.cs` declares both `KaChar` and `KaCharAsync`. This does not breach the 500-line rule (99 lines) and `.claude/rules/general-code-change.md` § Module & File Structure asks for cohesion, which two variants of one concept satisfy. No split is recommended. Recorded so a reviewer does not flag it.
4. **Line-coverage headroom is modest.** Static analysis puts the unexecuted lines at 45, 52-53, 60, 94-95 — roughly six of the file's executable lines. The measured figure is likely already near or above 80%. F1's harness supplies the actual number; the value of this work is the unused-member coverage plus the error-handling contracts (G7), not a large percentage delta.
5. **Coverage attribution across two classes in one file.** Cobertura reports per-file line coverage but per-class entries. If F1's harness aggregates by `filename`, `KaChar` and `KaCharAsync` roll into one `KaChar.cs` figure; if it aggregates by class, they report separately. Either satisfies the epic's per-**file** mandate, but the planner should confirm which the harness produces before writing the evidence artifact, so the recorded number is unambiguous.

---

## 9. Sources

| File | Lines read | Used for |
| --- | --- | --- |
| `QuickFiler/Controllers/KaChar.cs` | 1-100 (whole file) | Structural inventory, both classes |
| `QuickFiler/Controllers/KaKey.cs` | 1-100 (whole file) | `DelegateType` comparison (line 45), orphan-`Update` pattern |
| `QuickFiler/Interfaces/IKbdAction.cs` | 1-18 (whole file) | Contract surface; commented-out `Update` / `DelegateType` at lines 15-16 |
| `QuickFiler/Controllers/KbdActions.cs` | 1-147 (whole file) | `new()` constraint (line 15), instance construction and setter calls (lines 99-102), indexer setter (line 44) |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 1-415 (whole file) | Consumer behavior; `(char)e.KeyValue` casts (124, 128, 171, 176); try/catch asymmetry (114-131 vs 133-148) |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 1260-1399 | `KaChar` registration context |
| `QuickFiler.Test/Controllers/KaCharTests.cs` | 1-155 (whole file) | Existing coverage map; confirmed no wall-clock wait |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 92-96 | Confirmed `KaCharTests.cs` already registered (line 94) |
| `QuickFiler/QuickFiler.csproj` | 13-14, 307-310, 359 | Target framework, compiled-surface confirmation |
| `CLAUDE.md` | 288-309 (§ UT2) | Exemption categories; testable-seam clause at line 303 |
| `.claude/rules/csharp.md` | 1-97 (whole file) | Seam hierarchy (49-53); coverage floors (39-41) |
| `.claude/rules/general-unit-test.md` | provided in session context | Coverage Exclusion Policy; Determinism Infrastructure |
| `docs/features/epics/quickfiler-per-file-coverage/epic.md` | 1-418 (whole file) | Shared Design 1-6; F3 assignment (267-274) |
| `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/issue.md` | 1-95 (whole file) | Acceptance criteria; additive-only constraint (65-70) |
| `coverage.config` | 1-24 (whole file) | Confirmed no module-path exclusion touches QuickFiler |

**Search commands run:** `rg 'IKbdAction' --glob '**/*.cs'`; `rg 'DelegateType|\.Activated|ToggleControl' --glob '**/*.cs'`; `rg 'Update\s*=|\.Update\(' --glob 'QuickFiler/**/*.cs'`; `rg 'new Ka(Char|Key|StringAsync|CharAsync|KeyAsync)\s*\(' --glob 'QuickFiler/**/*.cs'`; `rg '(charActionsAsync|keyActionsAsync|charAsync|charActions|keyActions)\.Add\(' QuickFiler.Test`.
