# Research: `QuickFiler/Controllers/KaStringAsync.cs`

Timestamp: 2026-08-07T22-05
Feature: `quickfiler-keyboard-actions-coverage` (issue #430, epic child F3 of #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`
Scope: read-only research. No production or test file was modified.

---

## 1. File Under Research

| Property | Value |
| --- | --- |
| Path | `QuickFiler/Controllers/KaStringAsync.cs` |
| Line count | 95 (file ends at line 96 with the trailing newline) |
| Types declared | One: `KaStringAsync : IKbdAction<string, Func<string, Task>>` (lines 10-94) |
| Compiled by | `QuickFiler/QuickFiler.csproj` line 309 |
| Target framework | `v4.8.1`, `LangVersion=preview` |
| `[ExcludeFromCodeCoverage]` present | **No.** No `System.Diagnostics.CodeAnalysis` using directive and no attribute. |
| Existing tests | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` — 8 test methods. Registered at `QuickFiler.Test/QuickFiler.Test.csproj` line 96. |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.** This artifact records evidence supporting a `testable` classification; it does not classify the file. |

### 1.1 Headline finding: there is no asynchrony inside this type, and no timer seam is required

The delegation brief and `issue.md` lines 73-74 both anticipate that this file needs a fake-timer or injected-clock approach:

> **Determinism.** `Thread.Sleep`, `Task.Delay`, and real wall-clock waits are prohibited in tests; `KaStringAsync` requires a fake-timer or injected-clock approach.

**That expectation is not supported by the code.** Verified across all 95 lines:

| Construct searched | Occurrences in `KaStringAsync.cs` |
| --- | --- |
| `async` keyword | **0** |
| `await` | **0** |
| `Task.Delay` | **0** |
| `Thread.Sleep` | **0** |
| `Timer`, `System.Threading.Timer`, `DispatcherTimer` | **0** |
| `DateTime` / `DateTimeOffset` / `TimeProvider` / `Stopwatch` | **0** |
| `Random` | **0** |
| `SynchronizationContext` / `ConfigureAwait` / `Task.Run` | **0** |
| `Microsoft.Office.Interop.Outlook` (COM) | **0** |
| `System.Windows.Forms` | **0** (the file does not even import it) |

The `Async` suffix describes only the **shape of the stored delegate** (`Func<string, Task>`, line 44) — a value the type holds and hands back. It never invokes, awaits, schedules, or times anything. `KeyEquals` (lines 57-79), the only method with logic, is entirely synchronous.

**The asynchrony lives in the caller.** `KeyboardHandler.KeyDownTaskAsync` (`KeyboardHandler.cs` lines 150-204) is the driver: it owns the `_filterBuilder` accumulation (lines 79, 180, 190, 195, 200-201), sets `Activated` across the registry (line 187), and performs the single `await StringActionsAsync[keyName](keyName)` (line 194). That method is in `KeyboardHandler.cs`, which is a **different file in the same F3 cluster** and is not covered by this artifact.

**Conclusion: no `TimeProvider`, no `FakeTimeProvider`, no fake-timer facility, and no injected clock is needed for this file.** Section 5 records the full seam determination. The `issue.md` constraint at lines 73-74 should be corrected in `spec.md` to scope the fake-timer concern to `KeyboardHandler.cs` (where the `await` and the filter-buffer state machine actually live), or dropped if that file likewise has no timing dependency.

### 1.2 Audit of the existing test file for prohibited wall-clock waits

`QuickFiler.Test/Controllers/KaStringAsyncTests.cs` was read in full (lines 1-168) and checked against `.claude/rules/general-unit-test.md` § Determinism Infrastructure ("Banned APIs in test code — `setTimeout`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits, and `Date.now()` outside the clock interface are prohibited in tests").

| Check | Result |
| --- | --- |
| `Thread.Sleep` | Absent |
| `Task.Delay` | Absent |
| `.Wait()` / `.Result` / `GetAwaiter().GetResult()` | Absent |
| `DateTime.Now` / `DateTime.UtcNow` / `Stopwatch` | Absent |
| Real wall-clock wait of any form | Absent |
| Async pattern used | `Task.CompletedTask` (lines 25, 31, 66) and a single `async Task` test method awaiting a synchronously-completing delegate (lines 56-74) |

**Finding: no policy defect.** The existing suite is already deterministic and its class comment at line 15 states "No timing dependency is introduced," which the code confirms. Nothing needs remediation on the determinism axis. This closes the specific attention point raised in the delegation brief.

### 1.3 How coverage will be measured

Numeric per-file line coverage is not established here. It will be measured at execution time with **F1's per-file coverage report harness**, derived from the Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and committed under `<FEATURE>/evidence/qa-gates/`. The analysis below is static.

---

## 2. Structural Inventory

| # | Member | Lines | Notes |
| --- | --- | --- | --- |
| S1 | `KaStringAsync()` | 12 | Empty body. Required by the `new()` constraint at `KbdActions.cs` line 15; invoked at `KbdActions.cs` line 99. |
| S2 | `KaStringAsync(string sourceId, string key, Func<string, Task> function, Action<string> update, System.Action toggleControl)` | 14-27 | **Five** parameters — the only element type in the cluster with more than three. Line 23 assigns `Key = key.ToLower()`, which then passes through the `Key` **setter** (line 40) that lower-cases again. Double normalization is harmless but redundant. `key == null` throws `NullReferenceException` at line 23 before reaching the setter. No other guards. |
| S3 | `SourceId` get / set | 29-34 | Backing field `_sourceId` (line 29). |
| S4 | `Key` get / set | 36-41 | Backing field `_key` (line 36). **Setter normalizes**: `_key = value.ToLower()` (line 40). `value == null` throws `NullReferenceException`. Uses culture-sensitive `ToLower()`, not `ToLowerInvariant()`. |
| S5 | `Delegate` get / set | 43-48 | `Func<string, Task>`. Stored and returned; **never invoked by this type**. |
| S6 | `Activated` get / set | 50-55 | `bool`, initialized `false` (line 50). Set externally by `KeyboardHandler.cs` line 187 (`StringActionsAsync.ForEach(x => x.Activated = true)`) and reset internally at line 77. |
| S7 | `KeyEquals(string other)` | 57-79 | The only logic in the file. Four-way structure, detailed in section 2.1. |
| S8 | `Update` get / set | 81-86 | `Action<string>`. **Not a member of `IKbdAction<,>`** (commented out at `IKbdAction.cs` line 15) but, unlike its `KaChar`/`KaKey` counterparts, it **is** consumed — by this class's own `KeyEquals` at lines 62 and 73. |
| S9 | `ToggleControl` get / set | 88-93 | `System.Action`. Consumed by `KeyEquals` at lines 68 and 75. Not on `IKbdAction<,>` and not present on any sibling element type. |

### 2.1 `KeyEquals` branch structure (lines 57-79)

```
57  public bool KeyEquals(string other)
58  {
59      if (Key.Contains(other))              // B1  ordinal substring test
60      {
61          if (Activated && Update is not null)   // B1a
62              Update(Key.Substring(other.Length - 1, 1));
63          return true;                       // early return — Activated is NOT reset
64      }
65      else if (other.Length == 1)           // B2
66      {
67          if (Activated && ToggleControl is not null)  // B2a
68              ToggleControl();
69      }
70      else if (other.Length > 1)            // B3
71      {
72          if (Update is not null)            // B3a  NOTE: not gated on Activated
73              Update(Key.Substring(0, 1));
74          if (Activated && ToggleControl is not null)  // B3b
75              ToggleControl();
76      }
77      Activated = false;                     // reached only via B2, B3, or neither
78      return false;
79  }
```

Behavioral properties that matter for testing:

1. **Two distinct identity notions coexist.** `KeyEquals` is *substring* matching (`Key.Contains(other)`), whereas `KbdActions.StoredKeyEquals` (`KbdActions.cs` lines 33-34) is exact `EqualityComparer<string>.Default` equality. That deliberate split is the #111 fix; `KbdActions.ContainsKey`/`FilterKeys`/`Find` route through the substring form while `Add`/`Remove` route through the exact form.
2. **`Update` is gated on `Activated` in B1 but NOT in B3.** Line 61 requires `Activated && Update is not null`; line 72 requires only `Update is not null`. This asymmetry is real and untested.
3. **B1 returns early (line 63), skipping the `Activated = false` reset at line 77.** The existing test `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` asserts this for the true path; the reset is never asserted on the false paths.
4. **There is no fourth branch.** `other.Length == 0` cannot reach B2 or B3, because `Key.Contains("")` is `true` for every non-null `Key`, so B1 always claims it. See gap G1.
5. **`String.Contains(String)` on .NET Framework is ordinal**, so case matters. Both the constructor (line 23) and the setter (line 40) lower-case `Key`, and `KeyboardHandler.cs` line 180 lower-cases the probe (`char.ToLower((char)e.KeyValue)`), so the two sides agree in the production path. A mixed-case `other` passed directly does **not** match.

### 2.2 Production wiring — `Update` and `ToggleControl` are always null today

`rg 'new KaStringAsync\s*\(' --glob 'QuickFiler/**/*.cs'` returns exactly **one** production construction site: `QfcCollectionController.GenerateStringKbdAction` (`QfcCollectionController.cs` lines 1363-1385):

```csharp
var stringAsyncAction = new KaStringAsync(
    "Collection",
    key,
    (s) => ChangeByIndexAsync(int.Parse(s) - 1),
    //(s) => grp.ItemViewer.LblItemNumber.Text = s,
    null,
    null
);
```

Line 1380 shows the real `Update` implementation commented out. Both `update` and `toggleControl` are passed as `null`, and nothing assigns them afterwards (`rg 'Update\s*=|\.Update\(' --glob 'QuickFiler/**/*.cs'` returns only `KaStringAsync.cs:25`).

**Consequence:** in the current production wiring, branch bodies at lines 62, 68, 73, and 75 are **unreachable at runtime**. They remain live, public, tested API. This is not a reason to exempt or delete them — it is context the planner should have when weighing how much additional investment those branches justify. The gaps below prioritize accordingly.

### 2.3 Dependencies

- **COM / Outlook Interop:** none.
- **WinForms:** none — the file does not import `System.Windows.Forms`.
- **Clock / timers / randomness / async machinery:** none (section 1.1).
- **External I/O:** none.
- Only BCL string operations (`Contains`, `Substring`, `ToLower`) and delegate storage.

---

## 3. Existing Test Coverage (static analysis)

Source: `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` (lines 1-168). Helper `NewKa` at lines 20-25 constructs with `sourceId = "src"` and defaults the delegate to `_ => Task.CompletedTask`.

| Member / branch | Lines | Covered by (test method name) |
| --- | --- | --- |
| S1 `KaStringAsync()` | 12 | `KeySetter_LowercasesValue` (line 46) |
| S2 5-arg ctor | 14-27 | `Constructor_LowercasesKeyAndStoresMembers`; and via `NewKa` in `Delegate_AwaitsAndCompletesSynchronously`, all four `KeyEquals_*` tests, `KeyEquals_NullDelegatesAreToleratedInNonMatchBranches` |
| S2 ctor with **null `key`** | 23 | **none** |
| S3 `SourceId` getter | 31 | `Constructor_LowercasesKeyAndStoresMembers` |
| S3 `SourceId` setter | 33 | via ctor line 22 |
| S3 `SourceId` setter **after construction** | 33 | **none** |
| S4 `Key` getter | 39 | `Constructor_LowercasesKeyAndStoresMembers`, `KeySetter_LowercasesValue` |
| S4 `Key` setter (normalizing) | 40 | `KeySetter_LowercasesValue`; also via ctor line 23 |
| S4 `Key` setter with **null** | 40 | **none** |
| S5 `Delegate` getter | 46 | `Constructor_LowercasesKeyAndStoresMembers`, `Delegate_AwaitsAndCompletesSynchronously` |
| S5 `Delegate` setter | 47 | via ctor line 24 |
| S5 `Delegate` setter **after construction** | 47 | **none** |
| S5 `Delegate` invocation — **faulted / throwing** | — | **none** |
| S6 `Activated` getter | 53 | `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` (line 92) |
| S6 `Activated` setter | 54 | all four `KeyEquals_*` tests (lines 82, 104, 123, 141, 159) |
| S6 `Activated` **reset to false at line 77** | 77 | **executed** but **never asserted** |
| S7 / **B1** contains-match, `Activated && Update != null` | 59-63 | `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` |
| **B1a false via `Activated == false`** | 61 | `KeyEquals_ContainsMatchWhileNotActivated_ReturnsTrueWithoutUpdate` |
| **B1a false via `Update == null`** (Activated true) | 61 | **none** |
| **B1 with `other == Key` (exact match)** | 62 | **none** |
| **B1 with `other.Length == 0`** | 62 | **none** |
| **B2** single-char non-match, `Activated && ToggleControl != null` | 65-68 | `KeyEquals_SingleCharNonMatchWhileActivated_InvokesToggleControlAndReturnsFalse` |
| **B2a false via `Activated == false`** | 67 | **none** |
| **B2a false via `ToggleControl == null`** (Activated true) | 67 | **none** |
| **B3** multi-char non-match, `Update != null`, `Activated && ToggleControl != null` | 70-75 | `KeyEquals_MultiCharNonMatch_InvokesUpdateWithFirstCharAndReturnsFalse` |
| **B3a false via `Update == null`** | 72 | `KeyEquals_NullDelegatesAreToleratedInNonMatchBranches` |
| **B3b false via `ToggleControl == null`** | 74 | `KeyEquals_NullDelegatesAreToleratedInNonMatchBranches` |
| **B3b false via `Activated == false`** (Update still fires) | 72-74 | **none** |
| **`KeyEquals(null)`** | 59 | **none** |
| S8 `Update` getter | 84 | via `KeyEquals` lines 61-62, 72-73 |
| S8 `Update` setter | 85 | via ctor line 25 |
| S8 `Update` setter **after construction** | 85 | **none** |
| S9 `ToggleControl` getter | 91 | via `KeyEquals` lines 67, 74 |
| S9 `ToggleControl` setter | 92 | via ctor line 26 |
| S9 `ToggleControl` setter **after construction** | 92 | **none** |

**Line-coverage assessment:** every executable line in the file (12, 22-26, 31, 33, 39, 40, 46, 47, 53, 54, 59, 61-63, 65, 67, 68, 70, 72-75, 77, 78, 84, 85, 91, 92) appears to be executed by the existing suite. Unlike the other four files in this cluster, `KaStringAsync.cs` has **no unexecuted line region**. Its measured line coverage is likely at or near 100% already, and F1's harness will confirm.

**The gaps here are therefore branch gaps and untested contracts, not missing lines.** `.claude/rules/general-unit-test.md` § Coverage Requirements sets a branch floor of >= 75% alongside the line floor, and `CLAUDE.md` § UT2 requires "positive flows, negative flows, edge cases and boundary conditions, error-handling behavior" per unit — several of which are absent here.

---

## 4. Coverage Gaps

Ordered by value. G1 and G2 are boundary defects; G3-G5 are unproven branch conditions; G6-G8 are contract gaps.

### G1 — `KeyEquals("")` throws `ArgumentOutOfRangeException` (line 62) and is untested

`Key.Contains("")` is `true` for every non-null `Key`, so an empty `other` always takes branch B1. If `Activated` is `true` and `Update` is non-null, line 62 evaluates:

```csharp
Update(Key.Substring(other.Length - 1, 1));   // other.Length == 0  =>  Substring(-1, 1)
```

`String.Substring` with a negative `startIndex` throws `ArgumentOutOfRangeException`. There is no guard.

**Reachability, stated precisely:**
- Not reachable through the current `KeyboardHandler` driver. `KeyboardHandler.KeyDownTaskAsync` appends a character (line 180) before calling `ContainsKey(_filterBuilder.ToString())` (line 181) and `FilterKeys(...)` (line 188), so `other` always has length >= 1.
- Additionally not reachable today because `Update` is always null in production wiring (section 2.2), which makes the guard at line 61 short-circuit before line 62.

**Disposition:** a genuine latent robustness defect, currently double-shielded. Cover it with a characterization test (TC-1) and promote a defect issue per `promote-latent-defects-to-issues`. Do **not** add a guard in this child — `KeyEquals` is invoked from `KbdActions` lines 49, 51, 55, 73, 80, which are consumed by F9, F10, and F11; changing its throw behavior is not additive.

### G2 — `KeyEquals(null)` throws `ArgumentNullException` (line 59) and is untested

`Key.Contains(null)` throws `ArgumentNullException` from the BCL. `KeyEquals` adds no guard and no context. The exception type differs from G1's, and the failure occurs one line earlier, so the two are distinct contracts. Also unreachable through the current driver (`_filterBuilder.ToString()` is never null), but reachable by any direct `KbdActions.ContainsKey(null)` call.

### G3 — B1a false via `Update == null` while `Activated == true` (line 61) is untested

`KeyEquals_NullDelegatesAreToleratedInNonMatchBranches` uses `"zz"`, which lands in **B3**, not B1. The contains-branch null-`Update` tolerance is unproven. Given section 2.2 (production `Update` is always null) and that `KeyboardHandler` line 187 sets `Activated = true` on every registered action when the filter buffer reaches length 1, **this is the exact combination the shipped code executes most often**. It should not be the untested one.

### G4 — B2a false conditions (line 67) are untested — both of them

`KeyEquals_SingleCharNonMatchWhileActivated_InvokesToggleControlAndReturnsFalse` covers only `Activated == true && ToggleControl != null`. Neither of the two false paths is covered:
- `Activated == false` with a non-null `ToggleControl` (toggle must **not** fire).
- `Activated == true` with `ToggleControl == null` (must not throw).

The second is the shipped configuration (section 2.2).

### G5 — B3b false via `Activated == false` (lines 72-74) is untested, and it hides a real asymmetry

With a multi-character non-matching `other` and `Activated == false`:
- Line 72 `Update is not null` is **not** gated on `Activated`, so `Update(Key.Substring(0, 1))` **does** fire.
- Line 74 is gated, so `ToggleControl` does **not** fire.

`KeyEquals_MultiCharNonMatch_InvokesUpdateWithFirstCharAndReturnsFalse` sets `Activated = true`, so it cannot distinguish the two gates. This is the single most consequential untested behavior in the file: it is the only place where `Update` and `ToggleControl` diverge, and the divergence looks unintentional.

### G6 — The `Activated = false` reset (line 77) is never asserted on any non-match path

Line 77 executes in the existing tests but no assertion observes it. The reset is a real state transition — `KeyboardHandler` line 187 sets `Activated = true` across the whole registry when the filter buffer reaches length 1, and relies on `KeyEquals` clearing it. `CLAUDE.md` § UT2 requires "state transitions for stateful components"; `Activated` is that component and its clearing edge is unverified.

Complementarily, `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` line 92-95 **does** assert that B1's early return skips the reset. So one half of the transition contract is proven and the other half is not.

### G7 — Boundary cases of the `Substring` arithmetic in B1 (line 62) are untested

Only `other = "ab"` against `Key = "abc"` is covered (`Substring(1, 1)` -> `"b"`). Untested:
- **Exact match**: `other == Key` -> `Substring(Key.Length - 1, 1)` -> the **last** character. This is the upper boundary of the valid index range and the case that occurs when the user finishes typing a two-digit index.
- **Single-character prefix**: `other = "a"` -> `Substring(0, 1)` -> `"a"`. Lower valid boundary.
- **Single-character non-prefix substring**: `Key = "abc"`, `other = "c"` -> contains, `Substring(0, 1)` -> `"a"`. Demonstrates that the reported character is derived from `other.Length`, not from *where* the match occurred — surprising and worth documenting.

The production key space is `"1".."9"` and `"01".."99"` (`QfcCollectionController.GenerateStringKbdAction` lines 1367-1374), so both the one-digit and two-digit boundaries are live.

### G8 — Case sensitivity and post-construction mutation are untested

- `Key.Contains(other)` is **ordinal**. `Key = "abc"`, `KeyEquals("AB")` returns `false` and lands in B3. Callers must lower-case; `KeyboardHandler.cs` line 180 does. Undocumented by test.
- The `Key` setter's normalization is proven only for a value assigned to a default-constructed instance (`KeySetter_LowercasesValue`). Reassigning `Key`, `SourceId`, `Delegate`, `Update`, or `ToggleControl` **after** construction is untested, though `KbdActions.Add` (lines 100-102) and the `KbdActions` indexer setter (line 44) do exactly that.
- Constructing or assigning a **null** `Key` throws `NullReferenceException` (line 23 or line 40) — untested, and a `NullReferenceException` rather than an `ArgumentNullException` is a poor diagnostic for a public setter.

### G9 — The stored `Delegate` is never tested for error propagation

`Delegate_AwaitsAndCompletesSynchronously` covers the success path with `Task.CompletedTask`. A faulted or synchronously-throwing `Func<string, Task>` is untested. `KeyboardHandler.KeyDownTaskAsync` line 194 awaits this delegate inside the caller's try/catch (lines 137-147), so the fault path is live.

### Not gaps (recorded so the planner does not re-open them)

- There is no fourth `KeyEquals` branch. `other.Length == 0` is claimed by B1 (section 2.1 item 4); it is not a missing `else`.
- No timing, COM, WinForms, or UI-thread dependency exists, so there is no untestable region and no STA requirement.
- The existing test suite has no determinism violation (section 1.2).

---

## 5. Seam Requirements

**None required. Recommendation: make zero production changes to `KaStringAsync.cs`.**

Assessment against the `.claude/rules/csharp.md` seam hierarchy (lines 49-53):

| Candidate dependency | Level considered | Determination |
| --- | --- | --- |
| Clock / time source | 1 (interface) then `TimeProvider` per `.claude/rules/csharp.md` lines 55-63 | **Not applicable — no time is read.** Verified: zero occurrences of `DateTime`, `DateTimeOffset`, `TimeProvider`, `Stopwatch`, `Task.Delay`, `Thread.Sleep`, or any timer type in the file (section 1.1). Introducing a `TimeProvider` constructor parameter would add a sixth parameter to a five-parameter constructor consumed by F11, for no testability benefit. Rejected. |
| Fake-timer / virtual scheduler | — | **Not applicable.** No async continuation, no scheduling, no `SynchronizationContext` use. The one `Task`-typed member is stored, not awaited. Rejected. |
| `Update` callback | 2 (injectable delegate) | **Already present.** `Action<string> Update` is a constructor-injected delegate (line 18) with a settable property (line 85). Tests supply a capture lambda; the existing suite already does (`KaStringAsyncTests.cs` line 81). No new seam. |
| `ToggleControl` callback | 2 (injectable delegate) | **Already present.** `System.Action ToggleControl` (lines 19, 92). This is the seam that keeps the UI-toggle side effect out of this type; without it, `KeyEquals` would need a control reference. Its existence is why this file needs no STA test. No new seam. |
| `Delegate` (`Func<string, Task>`) | 2 (injectable delegate) | **Already present.** Constructor-injected (line 17). Tests supply `Task.CompletedTask` / `Task.FromException`, which complete synchronously. No new seam. |
| String operations (`Contains`, `Substring`, `ToLower`) | 3 (adapter) | BCL, deterministic, no external state. No seam. |
| COM / Outlook | — | Absent. |
| WinForms control / handle / message loop | — | Absent; the file does not import `System.Windows.Forms`. |

**Why the higher-priority level was not used:** the seam hierarchy prefers an interface seam (level 1), but level 1 exists to isolate a *boundary* — a process, filesystem, clock, or third-party API. `KaStringAsync` touches no boundary. Its three collaborators are already level-2 injectable delegates supplied through the constructor, which is the correct and minimal seam for single-call-path callbacks per `.claude/rules/csharp.md` line 52. Adding an interface over them would be indirection without isolation benefit and would break the constructor signature that `QfcCollectionController` (F11) calls.

**STA last-resort clause (epic.md Shared Design section 3): not applicable.** No WinForms control is constructed. No `*.StaTests.cs` file is warranted; all proposed tests run on the default MSTest apartment.

**Determinism (`.claude/rules/general-unit-test.md` § Determinism Infrastructure):** satisfied without infrastructure. No production wall-clock read exists, so no `FakeTimeProvider` is needed. Every proposed async assertion uses `Task.CompletedTask` or `Task.FromException`, both of which complete synchronously.

---

## 6. Cross-Child Contract Impact

**Recommended production change set for this file: empty. Cross-child impact: none.**

Call sites of `KaStringAsync` outside this child's file set:

| Consumer | Lines | Owning child | Members used |
| --- | --- | --- | --- |
| `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` | 26 | **F3 (this child)** | type argument (`StringActionsAsync`) |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 83-88, 178-201 | **F3 (this child)** | `Activated` setter (187); `KeyEquals` via `ContainsKey` (181) and `FilterKeys` (188); `Key` getter (193); `Delegate` via indexer + `await` (194) |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 1349, 1353, 1360, 1363-1385 | **F11** | 5-arg constructor (1376-1383); `KbdActions.Add`/`Remove` with string keys |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | 17, 35, 53 | test-side (**F3**) | element type for the #111 regression suite |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | 340-393 | test-side (F11 territory) | type argument |

**Additive-vs-breaking determination:** no production edit is proposed, so the determination is *no change*.

Three changes a future planner might be tempted to make, all **breaking** and all out of scope:
1. Guarding `KeyEquals` against `other == ""` (G1) or `other == null` (G2) — changes the exception contract observed by `KbdActions.ContainsKey`/`FilterKeys`/`Find`, which F9, F10, and F11 consume.
2. Gating line 72's `Update` call on `Activated` to match line 61 (G5) — changes observable side-effect behavior.
3. Replacing `ToLower()` with `ToLowerInvariant()` on lines 23 and 40 — a correctness improvement under a Turkish locale, but a behavior change to a normalization that `QfcCollectionController` depends on.

All three belong in promoted defect issues, not in this child.

The only file this child modifies for `KaStringAsync` coverage: `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` (**append only** — no `.csproj` edit needed, since line 96 already registers it). Appending rather than adding a new file keeps `QuickFiler.Test.csproj` untouched for this file, reducing the merge-conflict surface shared with F9, F10, and F11.

---

## 7. Proposed Test Cases

**Target file:** `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` (existing, 168 lines — append; with ~13 added cases the file lands near 380-420 lines, still under the 500-line limit. If it approaches the limit during authoring, split the boundary/error cases into `QuickFiler.Test/Controllers/KaStringAsyncBoundaryTests.cs` and add the corresponding `<Compile Include>` to `QuickFiler.Test.csproj` adjacent to line 96.)
**Companion edits:** none expected.

Framework: MSTest `[TestClass]`/`[TestMethod]`, FluentAssertions. **No Moq** — the three collaborators are already injectable delegates, and capture lambdas are a more direct and readable stand-in than a mock. Arrange-Act-Assert. No STA, no timers, no temporary files, no external services. The existing private helper `NewKa` (lines 20-25) is reused where the signature fits.

Each case was cross-referenced against section 3; none duplicates an existing test.

| ID | Method name | Gap | Arrange / Act / Assert | Seam or mock |
| --- | --- | --- | --- | --- |
| TC-1 | `KeyEquals_WithEmptyString_WhileActivatedWithUpdate_ThrowsArgumentOutOfRangeException` | G1 | **A:** `var ka = NewKa("abc", update: _ => { }); ka.Activated = true`. **Act:** `Action act = () => ka.KeyEquals("")`. **Assert:** `act.Should().Throw<ArgumentOutOfRangeException>(because: "Key.Contains(\"\") is always true, so an empty probe enters the contains branch and evaluates Substring(-1, 1)")`. XML comment: characterization test for the G1 latent defect, cites the promoted issue, and records that the path is unreachable through KeyboardHandler (which always appends at least one char before probing, KeyboardHandler.cs:180-181). | injectable delegate (already present) |
| TC-2 | `KeyEquals_WithEmptyString_WhileNotActivated_ReturnsTrueWithoutThrowing` | G1 | **A:** `var ka = NewKa("abc", update: _ => { }); ka.Activated = false`. **Act:** `var result = ka.KeyEquals("")`. **Assert:** `result.Should().BeTrue()`. Proves the guard at line 61 is what shields the defect, and that the empty probe is otherwise treated as a match. | injectable delegate |
| TC-3 | `KeyEquals_WithNull_ThrowsArgumentNullException` | G2 | **A:** `var ka = NewKa("abc")`. **Act:** `Action act = () => ka.KeyEquals(null)`. **Assert:** `act.Should().Throw<ArgumentNullException>(because: "Key.Contains(null) throws from the BCL; KeyEquals adds no guard and no parameter context")`. Characterization. | none |
| TC-4 | `KeyEquals_ContainsMatchWhileActivatedWithNullUpdate_ReturnsTrueWithoutThrowing` | G3 | **A:** `var ka = NewKa("abc", update: null); ka.Activated = true`. **Act:** `var result = ka.KeyEquals("ab")`. **Assert:** `result.Should().BeTrue()`; `ka.Activated.Should().BeTrue(because: "the contains branch returns at line 63 before the line 77 reset")`. Comment notes this is the shipped configuration: `QfcCollectionController.cs:1376-1383` passes a null `update`. | injectable delegate |
| TC-5 | `KeyEquals_SingleCharNonMatchWhileNotActivated_DoesNotInvokeToggleControl` | G4 | **A:** `bool toggled = false; var ka = NewKa("abc", toggle: () => toggled = true); ka.Activated = false`. **Act:** `var result = ka.KeyEquals("z")`. **Assert:** `result.Should().BeFalse()`; `toggled.Should().BeFalse(because: "ToggleControl is gated on Activated at line 67")`. | injectable delegate |
| TC-6 | `KeyEquals_SingleCharNonMatchWhileActivatedWithNullToggle_DoesNotThrow` | G4 | **A:** `var ka = NewKa("abc", toggle: null); ka.Activated = true`. **Act:** `Action act = () => ka.KeyEquals("z")`. **Assert:** `act.Should().NotThrow()`. Distinct from `KeyEquals_NullDelegatesAreToleratedInNonMatchBranches`, which probes with `"zz"` and therefore exercises branch B3, not B2. | injectable delegate |
| TC-7 | `KeyEquals_MultiCharNonMatchWhileNotActivated_InvokesUpdateButNotToggleControl` | G5 | **A:** `string updateArg = null; bool toggled = false; var ka = NewKa("abc", update: s => updateArg = s, toggle: () => toggled = true); ka.Activated = false`. **Act:** `var result = ka.KeyEquals("zz")`. **Assert:** `result.Should().BeFalse()`; `updateArg.Should().Be("a", because: "line 72 gates only on Update != null, unlike line 61 and line 74 which also require Activated")`; `toggled.Should().BeFalse()`. **Highest-value case in this artifact** — the only test that separates the two gates. | injectable delegates |
| TC-8 | `KeyEquals_OnNonMatchBranches_ResetsActivatedToFalse` | G6 | **A:** three instances of `NewKa("abc")` each with `Activated = true`. **Act:** call `KeyEquals("z")` (B2), `KeyEquals("zz")` (B3), and — for the neither-branch case, which requires a non-contains `other` of length 0, unreachable — restrict to B2 and B3. **Assert:** `Activated.Should().BeFalse()` after each, `because: "line 77 clears the flag on every path that does not return early at line 63"`. Complements the existing B1 assertion at `KaStringAsyncTests.cs:92-95`. | none |
| TC-9 | `KeyEquals_WithExactKeyMatch_InvokesUpdateWithLastCharacter` | G7 | **A:** `string updateArg = null; var ka = NewKa("10", update: s => updateArg = s); ka.Activated = true`. **Act:** `var result = ka.KeyEquals("10")`. **Assert:** `result.Should().BeTrue()`; `updateArg.Should().Be("0", because: "Substring(other.Length - 1, 1) at the upper boundary yields the final character")`. Uses a live production key shape (two-digit index, `QfcCollectionController.cs:1373`). | injectable delegate |
| TC-10 | `KeyEquals_WithSingleCharSubstringNotAtStart_ReportsCharacterByProbeLengthNotMatchPosition` | G7 | **A:** `string updateArg = null; var ka = NewKa("abc", update: s => updateArg = s); ka.Activated = true`. **Act:** `ka.KeyEquals("c")`. **Assert:** result `BeTrue()`; `updateArg.Should().Be("a", because: "the reported character is derived from other.Length, not from the index at which the match occurred")`. Documents surprising-but-current behavior. | injectable delegate |
| TC-11 | `KeyEquals_IsOrdinalAndCaseSensitive_UppercaseProbeDoesNotMatchLowercasedKey` | G8 | **A:** `string updateArg = null; var ka = NewKa("abc", update: s => updateArg = s); ka.Activated = true`. **Act:** `var result = ka.KeyEquals("AB")`. **Assert:** `result.Should().BeFalse(because: "String.Contains is ordinal; the constructor lower-cases Key, so callers must lower-case the probe as KeyboardHandler.cs:180 does")`; `updateArg.Should().Be("a")` (the multi-char branch B3 fired). | injectable delegate |
| TC-12 | `KeySetter_WithNull_ThrowsNullReferenceException` | G8 | **A:** `var ka = new KaStringAsync()`. **Act:** `Action act = () => ka.Key = null`. **Assert:** `act.Should().Throw<NullReferenceException>(because: "the setter calls value.ToLower() with no guard; an ArgumentNullException would be the appropriate contract")`. Characterization; cite the promoted issue. | none |
| TC-13 | `Constructor_WithNullKey_ThrowsNullReferenceException` | G8 | **A/Act:** `Action act = () => new KaStringAsync("src", null, _ => Task.CompletedTask, null, null)`. **Assert:** `act.Should().Throw<NullReferenceException>()`. Distinct from TC-12: the throw originates at line 23 (`key.ToLower()`) before the setter is reached. | none |
| TC-14 | `Setters_AfterConstruction_ReplaceSourceIdKeyDelegateUpdateAndToggleControl` | G8 | **A:** `var ka = new KaStringAsync("first", "AB", _ => Task.CompletedTask, null, null)`; prepare a replacement `Func<string, Task>`, `Action<string>`, and `Action`. **Act:** assign all five properties. **Assert:** `SourceId == "second"`; `Key == "cd"` (setter re-normalizes); `Delegate`, `Update`, `ToggleControl` each `BeSameAs` their replacement. Mirrors the mutation `KbdActions.Add` (lines 100-102) and the `KbdActions` indexer setter (line 44) perform. | injectable delegates |
| TC-15 | `Delegate_WhenFunctionReturnsFaultedTask_AwaitObservesTheFault` | G9 | **A:** `var ka = NewKa("abc", func: _ => Task.FromException(new InvalidOperationException("boom")))`. **Act:** `Func<Task> act = async () => await ka.Delegate("abc")`. **Assert:** `await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom")`. Deterministic: `Task.FromException` completes synchronously — no delay, no timer, no wall-clock wait. Comment notes `KeyboardHandler.KeyDownTaskAsync` awaits this at line 194 inside the caller's try/catch (lines 137-147). Method is `async Task`. | injectable delegate |

**Count: 15 discrete test cases.** Each is individually nameable and becomes its own atomic plan task per the epic's per-file mandate.

Sequencing note: TC-1, TC-3, TC-12, and TC-13 are characterization tests that cite defect issue numbers in their XML comments. Promote the issues described in section 8 before authoring those four, or author them last.

---

## 8. Risks and Open Questions

1. **The `issue.md` fake-timer constraint is factually wrong for this file and should be corrected.** `issue.md` lines 73-74 assert that `KaStringAsync` requires a fake-timer or injected-clock approach. Section 1.1 shows it contains no time or async machinery at all. Carry the correction into `spec.md` so the atomic plan does not budget a task for a seam that has nothing to isolate. If the constraint is meant for `KeyboardHandler.cs`, restate it against that file.
2. **Defect issues to promote (per `promote-latent-defects-to-issues`):**
   - `KeyEquals("")` -> `ArgumentOutOfRangeException` via `Substring(-1, 1)` at line 62 (G1).
   - `KeyEquals(null)` -> unguarded `ArgumentNullException` at line 59 (G2).
   - The `Update` gate asymmetry between line 61 (`Activated &&`) and line 72 (no `Activated`) (G5) — flag as "intent unclear", not "confirmed bug", since no evidence establishes which behavior was intended.
   - `Key` setter / constructor throw `NullReferenceException` rather than `ArgumentNullException` (G8).
   - `ToLower()` vs `ToLowerInvariant()` on lines 23 and 40 — locale-dependent normalization on a key space that is entirely ASCII digits today, so low severity.

   These can reasonably be **one** issue titled around `KaStringAsync.KeyEquals` input-validation hardening rather than five.
3. **Line coverage will not move; branch coverage will.** Section 3 concludes every executable line is already reached. The planner and the reviewer must both understand that the per-file *line* number recorded as evidence may be unchanged by this work, while branch coverage and the § UT2 scenario-completeness requirement improve materially. Setting the expectation up front avoids a mid-execution scope argument.
4. **Four `KeyEquals` branch bodies are unreachable in current production wiring.** Lines 62, 68, 73, 75 depend on `Update` / `ToggleControl` being non-null, and the sole production construction site passes `null, null` with the real implementation commented out (`QfcCollectionController.cs` lines 1376-1383). They remain public API and are worth covering, but the planner should not treat them as high-risk production paths. If F11 later restores the commented-out `Update` (line 1380), these branches go live and TC-4, TC-7, TC-9, TC-10, and TC-11 become the regression net for that change.
5. **Test file size.** `KaStringAsyncTests.cs` is 168 lines and 15 cases will roughly double it. Monitor against the 500-line limit; the split plan is stated in section 7.
6. **Open question for F1:** does the harness aggregate Cobertura by `filename` or by class? This file declares one class so the answer does not change its number, but the evidence artifact should state which basis was used, for consistency with `KaChar.cs` and `KaKey.cs`, which each declare two.

---

## 9. Sources

| File | Lines read | Used for |
| --- | --- | --- |
| `QuickFiler/Controllers/KaStringAsync.cs` | 1-96 (whole file) | Structural inventory; `KeyEquals` branch structure (57-79); absence of all timing/async/COM constructs |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 1-415 (whole file) | The actual async driver (150-204); `_filterBuilder` state (79, 180, 190, 195, 200-201); `Activated` broadcast (187); `await` of the stored delegate (194); try/catch boundary (137-147) |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 1260-1399 | Sole production construction site (1363-1385); null `update`/`toggleControl` (1381-1382); commented-out `Update` (1380); key space `"1".."99"` (1367-1374); `Remove` usage (1349, 1353) |
| `QuickFiler/Controllers/KbdActions.cs` | 1-147 (whole file) | `KeyEquals` dispatch sites (49, 51, 55, 73, 80); `StoredKeyEquals` contrast (33-34); setter mutation (44, 100-102); `new()` constraint (15, 99) |
| `QuickFiler/Interfaces/IKbdAction.cs` | 1-18 (whole file) | Contract surface; commented-out `Update` at line 15 explaining the orphan-elsewhere / used-here split |
| `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 1-168 (whole file) | Existing coverage map; determinism audit (section 1.2) |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | 1-88 (whole file) | `KaStringAsync` as the #111 regression element type |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 92-96 | Confirmed `KaStringAsyncTests.cs` already registered (line 96) |
| `QuickFiler/QuickFiler.csproj` | 13-14, 307-310, 359 | Target framework, compiled-surface confirmation |
| `CLAUDE.md` | 288-309 (§ UT2) | Exemption categories; scenario-completeness requirement; testable-seam clause at line 303 |
| `.claude/rules/csharp.md` | 1-97 (whole file) | Seam hierarchy (49-53); `TimeProvider` guidance (55-63); coverage floors (39-41) |
| `.claude/rules/general-unit-test.md` | provided in session context | Determinism Infrastructure (banned APIs, fake timers); Coverage Requirements (branch floor) |
| `docs/features/epics/quickfiler-per-file-coverage/epic.md` | 1-418 (whole file) | Shared Design 1-6; F3 assignment (267-274) |
| `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/issue.md` | 1-95 (whole file) | The fake-timer constraint at lines 73-74 corrected by section 1.1; additive-only constraint (65-70) |
| `coverage.config` | 1-24 (whole file) | Confirmed no module-path exclusion touches QuickFiler |

**Search commands run:** `rg 'IKbdAction' --glob '**/*.cs'`; `rg 'KbdActions\s*<' --glob '**/*.cs'`; `rg 'DelegateType|\.Activated|ToggleControl' --glob '**/*.cs'`; `rg 'Update\s*=|\.Update\(' --glob 'QuickFiler/**/*.cs'`; `rg 'new Ka(Char|Key|StringAsync|CharAsync|KeyAsync)\s*\(' --glob 'QuickFiler/**/*.cs'`.
