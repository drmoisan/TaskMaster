# Research: `QuickFiler/Controllers/KaKey.cs`

Timestamp: 2026-08-07T22-05
Feature: `quickfiler-keyboard-actions-coverage` (issue #430, epic child F3 of #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`
Scope: read-only research. No production or test file was modified.

---

## 1. File Under Research

| Property | Value |
| --- | --- |
| Path | `QuickFiler/Controllers/KaKey.cs` |
| Line count | 99 (file ends at line 100 with the trailing newline) |
| Types declared | **Two** public classes in one file: `KaKey` (lines 11-56) and `KaKeyAsync` (lines 58-98) |
| Compiled by | `QuickFiler/QuickFiler.csproj` line 308 |
| Target framework | `v4.8.1`, `LangVersion=preview` |
| `[ExcludeFromCodeCoverage]` present | **No.** No `System.Diagnostics.CodeAnalysis` using directive and no attribute on either class. |
| Existing tests | `QuickFiler.Test/Controllers/KaKeyTests.cs` — 9 test methods (5 for `KaKey`, 4 for `KaKeyAsync`). Registered at `QuickFiler.Test/QuickFiler.Test.csproj` line 95. |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.** This artifact records evidence supporting a `testable` classification; it does not classify the file. |

### 1.1 Exemption posture

Neither class references `Microsoft.Office.Interop.Outlook`, derives from a WinForms type, or is Designer-generated, so none of the three `CLAUDE.md` § UT2 exemption categories applies. Both are pure value objects.

`using System.Windows.Forms;` (line 6) supplies the `System.Windows.Forms.Keys` **enum**, which is the key type for both classes. An enum requires no message loop, no STA apartment, no window handle, and no form. This file is WinForms-*referencing*, not WinForms-*bound*, and the `epic.md` Shared Design section 3 STA last-resort clause does not engage.

### 1.2 How coverage will be measured

Numeric per-file line coverage is not established here. It will be measured at execution time with **F1's per-file coverage report harness**, derived from the Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and committed under `<FEATURE>/evidence/qa-gates/`. The analysis below is static.

---

## 2. Structural Inventory

`KaKey.cs` is structurally parallel to `KaChar.cs`, substituting `Keys` for `char`.

### 2.1 `KaKey : IKbdAction<Keys, Action<Keys>>` (lines 11-56)

| # | Member | Lines | Notes |
| --- | --- | --- | --- |
| K1 | `KaKey()` | 13 | Empty body. Required by the `new()` constraint at `KbdActions.cs` line 15; invoked at `KbdActions.cs` line 99. |
| K2 | `KaKey(string sourceId, Keys key, Action<Keys> action)` | 15-20 | Assigns through the properties, so the three setters execute. No guards. |
| K3 | `SourceId` get / set | 22-27 | Backing field `_sourceId` (line 22). |
| K4 | `Key` get / set | 29-34 | Backing field `_key` (line 29), type `Keys`. |
| K5 | `Delegate` get / set | 36-41 | Backing field `_action` (line 36), type `Action<Keys>`. |
| K6 | `DelegateType` get | 43-46 | Returns `typeof(Action<Keys>)`. **Correct here** (contrast `KaChar.cs` line 45, which returns the same value for an `Action<char>` delegate — see `05-KaChar.md` gap G1). Not a member of `IKbdAction<,>` (commented out at `IKbdAction.cs` line 16). |
| K7 | `KeyEquals(Keys other)` | 48 | `Key == other`. Expression-bodied, no branch. **Reference-equality-free enum comparison — see gap G6 for the `[Flags]` consequence.** |
| K8 | `Update` get / set | 50-55 | `Action<string>`, backing field `_update` (line 50). Not on `IKbdAction<,>` (commented out at `IKbdAction.cs` line 15). |

### 2.2 `KaKeyAsync : IKbdAction<Keys, Func<Keys, Task>>` (lines 58-98)

| # | Member | Lines | Notes |
| --- | --- | --- | --- |
| Y1 | `KaKeyAsync()` | 60 | Empty body. Invoked through the `new()` constraint at `KbdActions.cs` line 99. |
| Y2 | `KaKeyAsync(string sourceId, Keys key, Func<Keys, Task> function)` | 62-67 | Assigns through properties. No guards. |
| Y3 | `SourceId` get / set | 69-74 | |
| Y4 | `Key` get / set | 76-81 | |
| Y5 | `Delegate` get / set | 83-88 | `Func<Keys, Task>`. |
| Y6 | `KeyEquals(Keys other)` | 90 | `Key == other`. |
| Y7 | `Update` get / set | 92-97 | Unused. |
| — | *no `DelegateType`* | — | Same asymmetry as `KaCharAsync`. |

### 2.3 Dependencies

- **COM / Outlook Interop:** none.
- **WinForms:** the `Keys` enum only (lines 6, 11, 15, 29-33, 36, 45, 48, 58, 62, 76-80, 83, 90). No control, form, handle, or message loop.
- **Clock / timers / randomness:** none. No `DateTime`, `TimeProvider`, `Task.Delay`, `Thread.Sleep`, or `Random`.
- **Asynchrony:** `KaKeyAsync` has **no `async` method and no `await`**. The "Async" suffix describes only the stored delegate's shape (`Func<Keys, Task>`). Awaiting is performed by the caller, `KeyboardHandler.KeyDownTaskAsync` (`KeyboardHandler.cs` lines 159, 168).
- **Unused members verified by search:** `rg 'DelegateType'` yields only the two declarations (`KaChar.cs:43`, `KaKey.cs:43`) and the commented-out interface line. `rg 'Update\s*=|\.Update\(' --glob 'QuickFiler/**/*.cs'` yields only `KaStringAsync.cs:25`. **No production code reads or writes `KaKey.Update`, `KaKeyAsync.Update`, or `KaKey.DelegateType`.**

### 2.4 Production construction sites

`rg 'new Ka(Key|KeyAsync)\s*\(' --glob 'QuickFiler/**/*.cs'`:

- `KaKey`: `QfcCollectionController.cs` lines 1268, 1269, 1270 (three registrations, passed to the `KbdActions(IEnumerable<UClass>)` constructor at line 1265).
- `KaKeyAsync`: `QfcCollectionController.cs` lines 1287, 1288 (via the `IEnumerable` ctor at line 1284), line 1302 (via the `IEnumerable` ctor at line 1295); `EfcFormController.cs` line 365 (via the `IEnumerable` ctor at line 358).

Both are declared as type arguments in `IQfcKeyboardHandler.cs` lines 23-25 and `KeyboardHandler.cs` lines 58-77.

**Note carried from `04-KbdActions.md` gap G2:** `QfcCollectionController.cs` lines 1268-1270 register two `KaKey` instances that share `SourceId = "Collection"` and `Key = Keys.Down`. That is a latent defect in the `KbdActions(IEnumerable)` constructor's missing duplicate guard, not a defect in `KaKey`. It is recorded here only because `KaKey` is the element type involved.

---

## 3. Existing Test Coverage (static analysis)

Source: `QuickFiler.Test/Controllers/KaKeyTests.cs` (lines 1-144).

| Member / branch | Lines | Covered by (test method name) |
| --- | --- | --- |
| K1 `KaKey()` | 13 | `KaKey_ParameterlessConstructor_LeavesNullDelegateAndNoneKey` |
| K2 3-arg ctor | 15-20 | `KaKey_Constructor_StoresSourceIdKeyAndDelegate`, `KaKey_Delegate_DispatchesToSuppliedAction`, `KaKey_KeyEquals_MatchesSameKeyAndRejectsOther`, `KaKey_Constructor_NullDelegate_IsStoredNotRejected` |
| K3 `SourceId` getter | 24 | `KaKey_Constructor_StoresSourceIdKeyAndDelegate` |
| K3 `SourceId` setter | 25 | via ctor line 17 |
| K3 `SourceId` setter **after construction** | 25 | **none** |
| K4 `Key` getter | 31 | `KaKey_Constructor_StoresSourceIdKeyAndDelegate`, `KaKey_ParameterlessConstructor_LeavesNullDelegateAndNoneKey` |
| K4 `Key` setter | 32 | via ctor line 18 |
| K4 `Key` setter **after construction** | 32 | **none** |
| K5 `Delegate` getter | 38 | `KaKey_Constructor_StoresSourceIdKeyAndDelegate`, `KaKey_Delegate_DispatchesToSuppliedAction`, `KaKey_ParameterlessConstructor_...`, `KaKey_Constructor_NullDelegate_IsStoredNotRejected` |
| K5 `Delegate` setter | 39 | via ctor line 19 |
| K5 `Delegate` setter **after construction** | 39 | **none** |
| K6 **`DelegateType` getter** | 45 | **none** |
| K7 `KeyEquals` — true | 48 | `KaKey_KeyEquals_MatchesSameKeyAndRejectsOther` |
| K7 `KeyEquals` — false | 48 | `KaKey_KeyEquals_MatchesSameKeyAndRejectsOther` |
| K8 **`Update` getter** | 52 | **none** |
| K8 **`Update` setter** | 53 | **none** |
| Y1 **`KaKeyAsync()`** | 60 | **none** |
| Y2 3-arg ctor | 62-67 | `KaKeyAsync_Constructor_StoresSourceIdKeyAndDelegate`, `KaKeyAsync_Delegate_AwaitsAndCompletesSynchronously`, `KaKeyAsync_KeyEquals_MatchesSameKeyAndRejectsOther`, `KaKeyAsync_Constructor_NullDelegate_IsStoredNotRejected` |
| Y3 `SourceId` get / set | 71-72 | `KaKeyAsync_Constructor_StoresSourceIdKeyAndDelegate` (getter; setter via ctor) |
| Y3 `SourceId` setter **after construction** | 72 | **none** |
| Y4 `Key` get / set | 78-79 | `KaKeyAsync_Constructor_StoresSourceIdKeyAndDelegate` (getter; setter via ctor) |
| Y4 `Key` setter **after construction** | 79 | **none** |
| Y5 `Delegate` get / set | 85-86 | `KaKeyAsync_Constructor_StoresSourceIdKeyAndDelegate`, `KaKeyAsync_Delegate_AwaitsAndCompletesSynchronously`, `KaKeyAsync_Constructor_NullDelegate_IsStoredNotRejected` |
| Y5 `Delegate` setter **after construction** | 86 | **none** |
| Y6 `KeyEquals` — true / false | 90 | `KaKeyAsync_KeyEquals_MatchesSameKeyAndRejectsOther` |
| Y7 **`Update` getter** | 94 | **none** |
| Y7 **`Update` setter** | 95 | **none** |
| Delegate invocation **throwing** (either class) | — | **none** |
| Modifier-combined `Keys` values | 48, 90 | **none** |

**Indirect-coverage note.** `KaKey` is additionally exercised as the element type of `KbdActionsRemainingBranchesTests.cs` (all ten methods use `KbdActions<Keys, KaKey, Action<Keys>>` — line 21-22). That suite reaches `KaKey`'s 3-arg constructor (line 42), its parameterless constructor via the `new()` constraint (`KbdActions.cs` line 99, from `registry.Add("src", Keys.Enter, ...)`), all three setters (`KbdActions.cs` lines 100-102), the `Delegate` setter through the `KbdActions` indexer (`KbdActions.cs` line 44, in `Indexer_Get_ReturnsRegisteredDelegate_Set_ReplacesIt`), the `Key` getter (`KbdActions.cs` line 143), and `KeyEquals` (`KbdActions.cs` lines 49, 51, 55, 73, 80). It does **not** reach `DelegateType` or `Update`. `KaKeyAsync` receives no such indirect coverage: `rg '(keyActionsAsync|keyActions)\.Add\(' QuickFiler.Test` returns no matches.

---

## 4. Coverage Gaps

Four unexecuted line regions plus three untested contracts.

### G1 — `KaKey.DelegateType` (lines 43-46) is unexecuted

Unlike its `KaChar` counterpart, this member returns the **correct** type: `KaKey` implements `IKbdAction<Keys, Action<Keys>>` and `DelegateType` reports `typeof(Action<Keys>)`. There is no defect to characterize here — only an unexecuted getter.

The member has no production consumer (`rg 'DelegateType'` yields only declarations and the commented-out interface line at `IKbdAction.cs:16`). It is orphaned public API left behind when the member was withdrawn from `IKbdAction<,>`.

**Disposition: cover, do not delete.** Deleting a public member of a type consumed by `QfcCollectionController.cs` (F11) is non-additive and breaches this child's constraint (`issue.md` lines 65-70). A single assertion executes the getter.

### G2 — `KaKey.Update` (lines 50-55) is unexecuted and unused

`Action<string> Update` has no reader or writer anywhere in the repository. `IKbdAction.cs` line 15 shows the withdrawn contract member:

```csharp
//Action<string> Update { get; set; }
//Type DelegateType { get; }
```

Only `KaStringAsync` still uses its `Update` (`KaStringAsync.cs` lines 62, 73). **Disposition: cover, do not delete** — same additive-only reasoning as G1.

### G3 — `KaKeyAsync.Update` (lines 92-97) is unexecuted and unused

Identical to G2 for the async sibling.

### G4 — `KaKeyAsync()` parameterless constructor (line 60) is unexecuted

`KaKeyTests.cs` covers `KaKey`'s parameterless constructor (`KaKey_ParameterlessConstructor_LeavesNullDelegateAndNoneKey`) but has no equivalent for `KaKeyAsync`. This constructor is **not dead code**: `KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>.Add(sourceId, key, delegate)` invokes it through the `new()` constraint at `KbdActions.cs` line 99. Verified that no existing test reaches it that way.

### G5 — Post-construction setters are unexecuted on `KaKeyAsync`

All six setter lines (25, 32, 39, 72, 79, 86) execute only through constructors in the direct tests. For `KaKey` the `KbdActionsRemainingBranchesTests` suite reaches them indirectly via `KbdActions.Add` (lines 100-102) and via the indexer setter (line 44). For `KaKeyAsync` there is **no** such indirect path, so lines 72, 79, and 86 are reached only by the 3-arg constructor and reassignment is entirely unproven.

**Coverage-mechanics note:** these lines are already marked covered by the constructor tests, so closing G5 does not move the line-coverage number. Its value is contract proof for the mutable surface that `KbdActions.Add` and the `KbdActions` indexer setter rely on.

### G6 — `Keys` is a `[Flags]` enum and modifier-combined values are untested

`System.Windows.Forms.Keys` carries `[Flags]` and defines modifier bits (`Keys.Shift = 0x10000`, `Keys.Control = 0x20000`, `Keys.Alt = 0x40000`) plus the `Keys.KeyCode = 0xFFFF` mask. `KeyEquals` (lines 48, 90) uses plain `==`, so:

- `new KaKey("src", Keys.Control | Keys.C, ...).KeyEquals(Keys.C)` is **`false`**.
- `KeyboardHandler` looks keys up with `e.KeyCode` (`KeyboardHandler.cs` lines 98, 108, 118, 155, 164), and `KeyEventArgs.KeyCode` returns `KeyData & Keys.KeyCode` — modifiers **stripped**.

Consequence: a modifier-combined registration can never match at runtime through the `KeyboardHandler` path. All current production registrations use bare keys (`Keys.Up`, `Keys.Down`, `Keys.Return`, `Keys.Enter`, `Keys.Escape` — `QfcCollectionController.cs` lines 1268-1302, `EfcFormController.cs` line 365), so nothing is broken today. This is the "out-of-range modifiers" scenario named in `issue.md` line 87 and is currently untested. It is a **documentation-of-contract** gap, not a defect.

Also untested: `Keys.None` as an explicit registered key (only observed as the *default* value in `KaKey_ParameterlessConstructor_...`), and an undefined numeric cast such as `(Keys)0x7FFF`.

### G7 — Error handling on delegate invocation is untested for both classes

Neither class guards a null delegate (proven by `KaKey_Constructor_NullDelegate_IsStoredNotRejected` / `KaKeyAsync_Constructor_NullDelegate_IsStoredNotRejected`), and neither test invokes the null delegate or a throwing delegate.

- `KaKey.Delegate` is `Action<Keys>` — an exception propagates synchronously. `KeyboardHandler.KeyboardHandler_KeyDown` (lines 114-131) invokes it via `DynamicInvoke` (line 122) with **no try/catch**; `DynamicInvoke` additionally wraps the original exception in `TargetInvocationException`. Worth pinning down the un-wrapped, direct-invocation behavior at the value-object level.
- `KaKeyAsync.Delegate` is `Func<Keys, Task>` — a fault surfaces on `await`. `KeyboardHandler.KeyboardHandler_KeyDownAsync` (lines 137-147) catches and logs.

### Not gaps (recorded so the planner does not re-open them)

- `KaKeyAsync` has no `DelegateType` member; nothing to cover.
- `KeyEquals` true/false outcomes are covered on both classes.
- No timing, COM, or UI-thread dependency exists in this file.

---

## 5. Seam Requirements

**None required. Recommendation: make zero production changes to `KaKey.cs`.**

Assessment against the `.claude/rules/csharp.md` seam hierarchy (lines 49-53):

| Candidate dependency | Assessment |
| --- | --- |
| Interface seam (level 1) | Nothing to extract. Both classes are data holders with one comparison expression each. `IKbdAction<Keys, ...>` already is the contract. |
| Injectable delegate seam (level 2) | The delegate **is** the injected collaborator; `Action<Keys>` / `Func<Keys, Task>` are supplied by the caller and can be any test lambda. |
| Adapter seam (level 3) | No static or third-party API is called. `typeof(Action<Keys>)` (line 45) is a compile-time metadata token. |
| COM / Outlook | Absent. |
| WinForms control / handle / message loop | Absent. `Keys` is an enum — see section 1.1. |
| Clock / timer / RNG | Absent. |

**STA last-resort clause (epic.md Shared Design section 3): not applicable.** No WinForms control is constructed. The existing `KaKeyTests.cs` class comment (lines 12-13) already records the reasoning: "Keys is an enum, so no WinForms message loop is required." No `*.StaTests.cs` file is warranted; all proposed tests run on the default MSTest apartment.

**Determinism (`.claude/rules/general-unit-test.md` § Determinism Infrastructure):** satisfied without infrastructure. No wall-clock read exists in production, so no `TimeProvider` / `FakeTimeProvider` is needed. Async assertions use `Task.CompletedTask` / `Task.FromException`, which complete synchronously. The existing `KaKeyTests.cs` **contains no wall-clock wait** — verified across all 144 lines; its class comment at line 16 states "No timing dependency is introduced." No policy defect in the existing suite.

---

## 6. Cross-Child Contract Impact

**Recommended production change set for this file: empty. Cross-child impact: none.**

Call sites of `KaKey` / `KaKeyAsync` outside this child's file set:

| Consumer | Lines | Owning child | Members used |
| --- | --- | --- | --- |
| `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` | 23-25 | **F3 (this child)** | type arguments only |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 58-77, 118-122, 155-168 | **F3 (this child)** | type arguments; `Delegate` via `KbdActions` indexer + `DynamicInvoke` |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 1265-1272, 1284-1305 | **F11** | 3-arg constructors (six registrations) |
| `QuickFiler/Controllers/EfcFormController.cs` | 358-372 | **F9** | 3-arg constructor (one registration) |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | 21-179 | test-side (**F3**) | element type for the registry suite |
| `QuickFiler.Test/Controllers/QfcItemController*.cs`, `QfcCollectionControllerTests.cs` | see `04-KbdActions.md` §6 | test-side | type arguments only |

**Additive-vs-breaking determination:** no production edit is proposed, so the determination is *no change*. Deleting the orphaned `Update` / `DelegateType` members would remove public API from a type consumed by F9 and F11 and is therefore **breaking**; it belongs in the promoted cleanup issue, not in this child.

The only file this child modifies for `KaKey` coverage: `QuickFiler.Test/Controllers/KaKeyTests.cs` (**append only** — no `.csproj` edit needed, since line 95 already registers it). Appending rather than adding a new file keeps `QuickFiler.Test.csproj` untouched for this file, reducing the merge-conflict surface shared with F9, F10, and F11.

---

## 7. Proposed Test Cases

**Target file:** `QuickFiler.Test/Controllers/KaKeyTests.cs` (existing, 144 lines — append; stays far below the 500-line limit).
**Companion edits:** none. `QuickFiler.Test.csproj` line 95 already registers the file.

Framework: MSTest `[TestClass]`/`[TestMethod]`, FluentAssertions. **No Moq** — both types under test are the concrete collaborators. Arrange-Act-Assert. No STA, no timers, no temporary files, no external services.

Each case was cross-referenced against section 3; none duplicates an existing test.

| ID | Method name | Gap | Arrange / Act / Assert | Seam or mock |
| --- | --- | --- | --- | --- |
| TC-1 | `KaKey_DelegateType_ReturnsActionOfKeys_MatchingItsDeclaredDelegate` | G1 | **A:** `var ka = new KaKey("src", Keys.Enter, _ => { })`. **Act:** `var t = ka.DelegateType`. **Assert:** `t.Should().Be(typeof(Action<Keys>), because: "KaKey stores Action<Keys>, so DelegateType agrees with its IKbdAction<Keys, Action<Keys>> contract")`. Comment may contrast `KaChar.cs` line 45, which returns the same value for an `Action<char>` delegate. | none |
| TC-2 | `KaKey_Update_DefaultsToNullAndRoundTripsAssignedAction` | G2 | **A:** `var ka = new KaKey("src", Keys.A, _ => { })`; `Action<string> probe = _ => { }`. **Act:** read `ka.Update` (expect null), assign `probe`, read again. **Assert:** first `BeNull()`, second `BeSameAs(probe)`. Comment notes `Update` is not on `IKbdAction<,>` (`IKbdAction.cs` line 15) and has no production consumer. | none |
| TC-3 | `KaKey_Update_InvokesAssignedActionWithSuppliedString` | G2 | **A:** `string received = null; var ka = new KaKey(); ka.Update = s => received = s`. **Act:** `ka.Update("x")`. **Assert:** `received.Should().Be("x")`. | none |
| TC-4 | `KaKey_SourceIdKeyAndDelegateSetters_ReplaceConstructedValues` | G5 | **A:** `var ka = new KaKey("first", Keys.A, _ => { })`; `Action<Keys> replacement = _ => { }`. **Act:** set `SourceId = "second"`, `Key = Keys.B`, `Delegate = replacement`. **Assert:** all three read back the new values; `Delegate.Should().BeSameAs(replacement)`. Mirrors `KbdActions.Add` lines 100-102 and the indexer setter at line 44. | none |
| TC-5 | `KaKey_KeyEquals_WithModifierCombinedKey_DoesNotMatchBareKeyCode` | G6 | **A:** `var ka = new KaKey("src", Keys.Control \| Keys.C, _ => { })`. **Act/Assert:** `ka.KeyEquals(Keys.Control \| Keys.C).Should().BeTrue()`; `ka.KeyEquals(Keys.C).Should().BeFalse(because: "KeyEquals is plain enum equality; KeyEventArgs.KeyCode strips modifiers, so a modifier-combined registration can never match through KeyboardHandler (KeyboardHandler.cs lines 98, 108, 118, 155, 164)")`. Documents the `issue.md` line 87 "out-of-range modifiers" scenario. | none |
| TC-6 | `KaKey_KeyEquals_WithExplicitNoneAndUndefinedValue_BehavesAsPlainEquality` | G6 | **A:** `var none = new KaKey("src", Keys.None, _ => { })`; `var undefined = new KaKey("src", (Keys)0x7FFF, _ => { })`. **Act/Assert:** `none.KeyEquals(Keys.None).Should().BeTrue()`; `none.KeyEquals(Keys.A).Should().BeFalse()`; `undefined.KeyEquals((Keys)0x7FFF).Should().BeTrue()`. Boundary case: an undefined enum value is stored and compared without validation. | none |
| TC-7 | `KaKey_Delegate_WhenActionThrows_PropagatesToCaller` | G7 | **A:** `var ka = new KaKey("src", Keys.Enter, _ => throw new InvalidOperationException("boom"))`. **Act:** `Action act = () => ka.Delegate(Keys.Enter)`. **Assert:** `act.Should().Throw<InvalidOperationException>().WithMessage("boom")`, `because: "KaKey performs no exception shielding; KeyboardHandler_KeyDown (KeyboardHandler.cs:114-131) has no try/catch, and its DynamicInvoke call at line 122 would additionally wrap this in TargetInvocationException"`. | none |
| TC-8 | `KaKey_Delegate_WhenNull_InvocationThrowsNullReferenceException` | G7 | **A:** `var ka = new KaKey("src", Keys.A, null)`. **Act:** `Action act = () => ka.Delegate(Keys.A)`. **Assert:** `act.Should().Throw<NullReferenceException>()`. Characterization: the constructor stores null without guarding (already proven by `KaKey_Constructor_NullDelegate_IsStoredNotRejected`); this proves the **consequence**, which that test does not. | none |
| TC-9 | `KaKeyAsync_ParameterlessConstructor_LeavesNullDelegateAndNoneKey` | G4 | **A/Act:** `var ka = new KaKeyAsync()`. **Assert:** `ka.Delegate.Should().BeNull()`; `ka.SourceId.Should().BeNull()`; `ka.Key.Should().Be(Keys.None)`. Comment states this ctor is reached in production through the `new()` constraint at `KbdActions.cs` line 99. | none |
| TC-10 | `KaKeyAsync_Update_DefaultsToNullAndRoundTripsAssignedAction` | G3 | **A:** `var ka = new KaKeyAsync("src", Keys.A, _ => Task.CompletedTask)`; `Action<string> probe = _ => { }`. **Act:** read, assign, read. **Assert:** null then `BeSameAs(probe)`. | none |
| TC-11 | `KaKeyAsync_SourceIdKeyAndDelegateSetters_ReplaceConstructedValues` | G5 | **A:** `var ka = new KaKeyAsync("first", Keys.A, _ => Task.CompletedTask)`; `Func<Keys, Task> replacement = _ => Task.CompletedTask`. **Act:** reassign all three. **Assert:** all three read back the new values. | none |
| TC-12 | `KaKeyAsync_KeyEquals_WithModifierCombinedKey_DoesNotMatchBareKeyCode` | G6 | **A:** `var ka = new KaKeyAsync("src", Keys.Shift \| Keys.Tab, _ => Task.CompletedTask)`. **Act/Assert:** matches the combined value, rejects `Keys.Tab`. Same rationale as TC-5, applied to the async registry path (`KeyboardHandler.cs` lines 155, 164). | none |
| TC-13 | `KaKeyAsync_Delegate_WhenFunctionReturnsFaultedTask_AwaitObservesTheFault` | G7 | **A:** `var ka = new KaKeyAsync("src", Keys.Return, _ => Task.FromException(new InvalidOperationException("boom")))`. **Act:** `Func<Task> act = async () => await ka.Delegate(Keys.Return)`. **Assert:** `await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom")`. Deterministic: `Task.FromException` completes synchronously. Method is `async Task`. | none |
| TC-14 | `KaKeyAsync_Delegate_WhenFunctionThrowsSynchronously_ThrowsBeforeTaskIsReturned` | G7 | **A:** `var ka = new KaKeyAsync("src", Keys.Return, _ => throw new InvalidOperationException("boom"))`. **Act:** `Action act = () => ka.Delegate(Keys.Return)` (not awaited — the throw precedes Task creation). **Assert:** `act.Should().Throw<InvalidOperationException>()`. Distinguishes the synchronous-throw path from TC-13's faulted-task path; the two differ for any caller that stores the Task before awaiting, as `KeyboardHandler.cs` line 159 does not but a future caller might. | none |

**Count: 14 discrete test cases** (8 for `KaKey`, 6 for `KaKeyAsync`). Each is individually nameable and becomes its own atomic plan task per the epic's per-file mandate.

No sequencing constraint: unlike `05-KaChar.md` TC-1, none of these cases cites a defect issue number, because `KaKey.DelegateType` is correct.

---

## 8. Risks and Open Questions

1. **Orphan-member cleanup issue is shared with `KaChar.cs`.** `Update` is unused public API on four types (`KaChar`, `KaCharAsync`, `KaKey`, `KaKeyAsync`), and `DelegateType` exists on `KaChar` and `KaKey` but not on their async siblings — residue of the withdrawn interface members at `IKbdAction.cs` lines 15-16. Per `promote-latent-defects-to-issues`, file **one** GitHub issue covering the cleanup across both files; do not file four.
2. **TC-5, TC-6, and TC-12 document a contract, not a bug.** Plain enum equality against a `[Flags]` type is a legitimate design given that all lookups go through `e.KeyCode`. The tests must be worded as contract documentation so a reviewer does not read them as regression tests for a defect.
3. **Two public classes in one file.** `KaKey.cs` declares both `KaKey` and `KaKeyAsync`. This does not breach the 500-line rule (99 lines) and satisfies `.claude/rules/general-code-change.md` § Module & File Structure cohesion, since the two are variants of one concept. No split recommended. Recorded so a reviewer does not flag it.
4. **Line-coverage headroom is modest.** Static analysis puts the unexecuted lines at 45, 52-53, 60, 94-95 — roughly six of the file's executable lines, the same profile as `KaChar.cs`. The measured figure is likely already near or above 80%. F1's harness supplies the actual number; the value of this work is the unused-member coverage plus the `[Flags]` and error-handling contracts, not a large percentage delta.
5. **Coverage attribution across two classes in one file.** As with `KaChar.cs`: confirm whether F1's harness aggregates Cobertura entries by `filename` or by class before writing the evidence artifact, so the recorded number is unambiguous.
6. **`KaKey` benefits from indirect coverage that `KaKeyAsync` does not.** `KbdActionsRemainingBranchesTests.cs` exercises `KaKey` through the registry (section 3, indirect-coverage note). If a future refactor of that suite changes its element type, `KaKey`'s indirect coverage disappears silently. The direct tests proposed above reduce that fragility.

---

## 9. Sources

| File | Lines read | Used for |
| --- | --- | --- |
| `QuickFiler/Controllers/KaKey.cs` | 1-100 (whole file) | Structural inventory, both classes |
| `QuickFiler/Controllers/KaChar.cs` | 1-100 (whole file) | `DelegateType` comparison (line 45); parallel structure |
| `QuickFiler/Interfaces/IKbdAction.cs` | 1-18 (whole file) | Contract surface; commented-out `Update` / `DelegateType` at lines 15-16 |
| `QuickFiler/Controllers/KbdActions.cs` | 1-147 (whole file) | `new()` constraint (line 15); construction and setter calls (99-102); indexer setter (44); `KeyEquals` dispatch (49, 51, 55, 73, 80); `Key` getter (143) |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 1-415 (whole file) | Consumer behavior; `e.KeyCode` lookups (98, 108, 118, 155, 164); `DynamicInvoke` (122); try/catch asymmetry (114-131 vs 133-148) |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 1260-1399 | `KaKey` / `KaKeyAsync` registration sites (1265-1305) |
| `QuickFiler.Test/Controllers/KaKeyTests.cs` | 1-144 (whole file) | Existing coverage map; confirmed no wall-clock wait |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | 1-181 (whole file) | Indirect `KaKey` coverage via the registry suite |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 92-96 | Confirmed `KaKeyTests.cs` already registered (line 95) |
| `QuickFiler/QuickFiler.csproj` | 13-14, 307-310, 359 | Target framework, compiled-surface confirmation |
| `CLAUDE.md` | 288-309 (§ UT2) | Exemption categories; testable-seam clause at line 303 |
| `.claude/rules/csharp.md` | 1-97 (whole file) | Seam hierarchy (49-53); coverage floors (39-41) |
| `.claude/rules/general-unit-test.md` | provided in session context | Coverage Exclusion Policy; Determinism Infrastructure |
| `docs/features/epics/quickfiler-per-file-coverage/epic.md` | 1-418 (whole file) | Shared Design 1-6; F3 assignment (267-274) |
| `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/issue.md` | 1-95 (whole file) | Acceptance criteria; additive-only constraint (65-70); modifier scenario (87) |
| `coverage.config` | 1-24 (whole file) | Confirmed no module-path exclusion touches QuickFiler |

**Search commands run:** `rg 'IKbdAction' --glob '**/*.cs'`; `rg 'KbdActions\s*<' --glob '**/*.cs'`; `rg 'DelegateType|\.Activated|ToggleControl' --glob '**/*.cs'`; `rg 'Update\s*=|\.Update\(' --glob 'QuickFiler/**/*.cs'`; `rg 'new Ka(Char|Key|StringAsync|CharAsync|KeyAsync)\s*\(' --glob 'QuickFiler/**/*.cs'`; `rg '(charActionsAsync|keyActionsAsync|charAsync|charActions|keyActions)\.Add\(' QuickFiler.Test`.
