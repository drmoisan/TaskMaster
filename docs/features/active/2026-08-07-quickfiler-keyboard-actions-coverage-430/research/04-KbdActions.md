# Research: `QuickFiler/Controllers/KbdActions.cs`

Timestamp: 2026-08-07T22-05
Feature: `quickfiler-keyboard-actions-coverage` (issue #430, epic child F3 of #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`
Scope: read-only research. No production or test file was modified.

---

## 1. File Under Research

| Property | Value |
| --- | --- |
| Path | `QuickFiler/Controllers/KbdActions.cs` |
| Line count | 146 (file ends at line 147 with the trailing newline) |
| Compiled by | `QuickFiler/QuickFiler.csproj` line 310 (`<Compile Include="Controllers\KbdActions.cs" />`) |
| Target framework | `v4.8.1`, `LangVersion=preview` (`QuickFiler.csproj` lines 13-14) |
| `[ExcludeFromCodeCoverage]` present | **No.** The file contains no `System.Diagnostics.CodeAnalysis` using directive and no attribute. The only QuickFiler keyboard-cluster file carrying the attribute is `Controllers/KeyboardHandler.cs` line 22. |
| Existing tests | `QuickFiler.Test/Controllers/KbdActionsTests.cs` (3 methods), `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` (10 methods). Both are registered in `QuickFiler.Test/QuickFiler.Test.csproj` lines 92-93. |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.** That ledger is the sole authority on whether this file is `testable` or `ratified-exempt`. This artifact records the evidence that it must be classified `testable`; it does not classify it. |

### 1.1 The binding non-exemption clause

`CLAUDE.md` § UT2, line 303, names this exact type as a non-exempt testable seam:

> These classes are formally exempted from the 80% floor. Exemption is applied via `[ExcludeFromCodeCoverage]` attributes in source code (reviewable in PRs) or via `coverage.config` assembly-level excludes for near-wholly-untestable assemblies. **Authority**: This exemption must be ratified by the project maintainer and is tracked in `feature/csharp-coverage-uplift`. Testable seams within otherwise-COM-bound assemblies (e.g., `ToDoLoader`, `IDList` arithmetic, `KbdActions<>`, path/settings helpers) are explicitly NOT exempt and must meet the `>= 80%` floor.

**Correction to the delegation brief and to `issue.md`.** The delegation prompt and `issue.md` lines 31-33 both state that `.claude/rules/csharp.md` *and* `CLAUDE.md` name `KbdActions<>` explicitly. Verified by `rg KbdActions` across all `*.md`: only `CLAUDE.md` line 303 names it. `.claude/rules/csharp.md` contains no occurrence of `KbdActions`; it supplies the general `>= 80%` floor (line 39), the `>= 90%` new-code floor (line 40), and the seam hierarchy (lines 49-53). The obligation is unchanged — it is simply sourced from one document, not two. The planner should cite `CLAUDE.md` line 303 and not attribute the clause to `.claude/rules/csharp.md`.

**Consequence for this child:** `KbdActions.cs` cannot be placed on the exemption ledger. It must reach `>= 80%` line coverage. Any newly added member would additionally have to reach `>= 90%`.

### 1.2 How coverage will be measured

Numeric per-file line coverage is **not** established in this artifact. It will be measured at execution time with **F1's per-file coverage report harness**, derived from the Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and committed as numeric evidence under `<FEATURE>/evidence/qa-gates/`. Neither the ledger nor the harness exists on disk yet; this research is written to consume them.

The current-state analysis below is **static**: every member and branch is enumerated and mapped to the existing test method that reaches it, by name.

**Historical data point (stale, do not treat as current).** `docs/features/archive/2026-03-27-quickfiler-navigation-key-collision-111/policy-audit.2026-03-27T13-28.md` lines 72-73 record a measured `KbdActions.cs` line coverage of **46.43%** and branch coverage of **31.25%** on 2026-03-27. That measurement predates `KbdActionsRemainingBranchesTests.cs`, which covers `Find`, `FindIndex`, `Remove`, the indexer, enumeration, and `Keys`. The static mapping in section 3 indicates the current figure is far higher. The 46.43% number must not be quoted as the current baseline.

---

## 2. Structural Inventory

`public class KbdActions<TKey, UClass, VDelegate> : IEnumerable<UClass> where UClass : IKbdAction<TKey, VDelegate>, new()` (line 14-15).

| # | Member | Lines | Dependencies / notes |
| --- | --- | --- | --- |
| M1 | `static readonly log4net.ILog logger` | 17-19 | `log4net.LogManager.GetLogger` — ambient static. Runs once per **closed generic instantiation** (each closed type gets its own static field). No testability barrier. |
| M2 | `KbdActions()` | 21-24 | Assigns `_list = new List<UClass>()`. |
| M3 | `KbdActions(IEnumerable<UClass> list)` | 26-29 | `_list = new List<UClass>(list)` — a **copy**, so the caller's list is snapshotted. Throws `ArgumentNullException` on `null` (from `List<T>(IEnumerable<T>)`). **Bypasses the duplicate guard enforced by `Add`.** |
| M4 | `private List<UClass> _list = new()` | 31 | Field initializer; both ctors overwrite it. |
| M5 | `private static bool StoredKeyEquals(TKey, TKey)` | 33-34 | `EqualityComparer<TKey>.Default.Equals`. **Storage identity** — deliberately distinct from `UClass.KeyEquals` (the #111 fix). |
| M6 | `VDelegate this[TKey key]` — getter | 36-38 | `this.Find(key).Delegate`. **No null guard**: unregistered key -> `Find` returns `default(UClass)` -> `NullReferenceException`. |
| M7 | `VDelegate this[TKey key]` — setter | 39-46 | Branch at line 41 `element is not null`: true -> assign; **false -> silent no-op**. |
| M8 | `bool ContainsKey(TKey key)` | 49 | `_list.Any(x => x.KeyEquals(key))` — **matching identity** (element-defined, substring for `KaStringAsync`). |
| M9 | `UClass[] FilterKeys(TKey key)` | 51 | `_list.Where(x => x.KeyEquals(key)).ToArray()`. |
| M10 | `UClass Find(TKey key)` | 53-69 | 3-way switch on match count: `0` -> `default(UClass)` (line 60); `1` -> `matches.First()` (line 62); `default` -> builds a message using `SentenceJoin` (`UtilitiesCS/Extensions/ArrayExtensions.cs` line 401+) and throws `InvalidOperationException` (line 67). Note: no `logger.Error` on this path, unlike `FindIndex`. |
| M11 | `int FindIndex(TKey key)` | 71-88 | Same 3-way switch: `0` -> `-1`; `1` -> `_list.FindIndex(...)`; `default` -> `logger.Error` (line 85) then throw (line 86). |
| M12 | `void Add(string sourceId, TKey key, VDelegate @delegate)` | 90-104 | Duplicate guard uses `x.SourceId == sourceId && StoredKeyEquals(x.Key, key)`. True -> `logger.Error` + `ArgumentException`. False -> `UClass instance = new()` (exercises `UClass`'s parameterless ctor via the `new()` constraint), then three property setters, then `_list.Add`. |
| M13 | `void Add(UClass instance)` | 106-121 | Same guard against `instance.SourceId` / `instance.Key`. Throws `ArgumentException(message, nameof(instance))`. **No null guard on `instance`**. |
| M14 | `bool Remove(string sourceId, TKey key)` | 123-135 | `_list.FindIndex` on `SourceId` + `StoredKeyEquals`. `-1` -> `false`; else `RemoveAt` + `true`. |
| M15 | `IEnumerator<UClass> GetEnumerator()` | 137 | Public generic enumerator. |
| M16 | `IEnumerator IEnumerable.GetEnumerator()` | 139 | **Explicit interface implementation.** Only reachable through a non-generic `IEnumerable` reference. |
| M17 | `ICollection<TKey> Keys` — getter | 141-144 | `_list.Select(x => x.Key).ToList()`; preserves insertion order. |

### 2.1 Closed generic instantiations in production

Verified with `rg 'KbdActions\s*<' --glob '**/*.cs'`. Five distinct closed types exist:

| Closed type | Production declaration sites |
| --- | --- |
| `KbdActions<char, KaChar, Action<char>>` | `Interfaces/IQfcKeyboardHandler.cs:21`; `Controllers/KeyboardHandler.cs:44-49`; `Controllers/QfcCollectionController.cs:583,743`; `Controllers/EfcFormController.cs:625-631` |
| `KbdActions<char, KaCharAsync, Func<char, Task>>` | `IQfcKeyboardHandler.cs:22`; `KeyboardHandler.cs:51-56`; `QfcCollectionController.cs:584,744`; `EfcFormController.cs:568-574` |
| `KbdActions<Keys, KaKey, Action<Keys>>` | `IQfcKeyboardHandler.cs:23`; `KeyboardHandler.cs:58-63`; `QfcCollectionController.cs:1265` |
| `KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>` | `IQfcKeyboardHandler.cs:24,25`; `KeyboardHandler.cs:65-77`; `QfcCollectionController.cs:1284,1295`; `EfcFormController.cs:358` |
| `KbdActions<string, KaStringAsync, Func<string, Task>>` | `IQfcKeyboardHandler.cs:26`; `KeyboardHandler.cs:83-88`; consumed by `QfcCollectionController.cs:1349,1353,1360` |

### 2.2 Type-parameter paths exercised by existing tests

| Closed type | Exercised by an existing `KbdActions` test? |
| --- | --- |
| `<string, KaStringAsync, Func<string, Task>>` | Yes — `KbdActionsTests.cs:17,35,53` |
| `<Keys, KaKey, Action<Keys>>` | Yes — `KbdActionsRemainingBranchesTests.cs:21-22` |
| `<char, KaChar, Action<char>>` | **No** |
| `<char, KaCharAsync, Func<char, Task>>` | **No** (constructed in `QfcItemController*Tests.cs` but never populated — verified: `rg '(charActionsAsync\|keyActionsAsync\|charActions\|keyActions)\.Add\(' QuickFiler.Test` returns no matches) |
| `<Keys, KaKeyAsync, Func<Keys, Task>>` | **No** (same) |

**Coverage-mechanics caveat, stated plainly.** `dotnet-coverage` Cobertura output attributes hits to *source lines*, not to closed generic instantiations. Covering one instantiation already marks the generic method's source lines as covered. Adding tests for the other three instantiations therefore **does not raise the per-file line-coverage number for `KbdActions.cs`**. Their value is behavioral: `ContainsKey`/`FilterKeys`/`Find` dispatch through `UClass.KeyEquals`, whose semantics differ per element type (substring matching in `KaStringAsync.KeyEquals`, `==` equality in `KaChar`/`KaKey`), while `Add`/`Remove` dispatch through `StoredKeyEquals`. That asymmetry is the entire subject of the #111 fix and is currently proven for exactly one element type. The recommendation below adds a small, targeted set — not three full duplicate suites.

---

## 3. Existing Test Coverage (static analysis)

`KbdActionsTests.cs` = **A**; `KbdActionsRemainingBranchesTests.cs` = **B**.

| Member / branch | Lines | Covered by (test method name) |
| --- | --- | --- |
| M1 static logger initializer | 17-19 | A + B (implicit, any instantiation) |
| M2 `KbdActions()` | 21-24 | A: all three methods; B: `NewRegistry` helper used by all ten |
| M3 `KbdActions(IEnumerable<UClass>)` | 26-29 | **none** |
| M5 `StoredKeyEquals` | 33-34 | A: `Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate`, `Add_WhenSourceAndStoredKeyAreExactDuplicate_ThrowsArgumentException`; B: `AddInstance_ExactDuplicate_ThrowsArgumentException`, `Remove_PresentKey_RemovesAndReturnsTrue` |
| M6 indexer get — match found | 36-38 | A: `FilterKeys_WhenDistinctStoredKeysCoexist_PreservesKeyboardMatchingSemantics` (`actions["10"]`); B: `Indexer_Get_ReturnsRegisteredDelegate_Set_ReplacesIt` |
| M6 indexer get — **no match (NRE)** | 36-38 | **none** |
| M7 indexer set — `element is not null` == true | 39-45 | B: `Indexer_Get_ReturnsRegisteredDelegate_Set_ReplacesIt` |
| M7 indexer set — `element is not null` == **false (no-op)** | 41, 46 | **none** |
| M8 `ContainsKey` — true | 49 | A: `FilterKeys_WhenDistinctStoredKeysCoexist_...` |
| M8 `ContainsKey` — false | 49 | B: `EmptyRegistry_HasNoKeysAndFindReturnsDefault`, `Remove_PresentKey_RemovesAndReturnsTrue` |
| M9 `FilterKeys` — non-empty result | 51 | A: `FilterKeys_WhenDistinctStoredKeysCoexist_...`; B: `FilterKeys_ReturnsOnlyMatchingInstances` |
| M9 `FilterKeys` — **empty result** | 51 | **none** |
| M10 `Find` case 0 | 59-60 | B: `EmptyRegistry_HasNoKeysAndFindReturnsDefault` |
| M10 `Find` case 1 | 61-62 | A: `FilterKeys_WhenDistinctStoredKeysCoexist_...` (via indexer); B: `AddInstance_ThenFind_ReturnsTheRegisteredInstance` |
| M10 `Find` default (throw) | 63-67 | B: `Find_WhenMultipleSourcesShareKey_ThrowsInvalidOperationException` |
| M11 `FindIndex` case 0 | 79-80 | B: `EmptyRegistry_HasNoKeysAndFindReturnsDefault` |
| M11 `FindIndex` case 1 | 81 | B: `AddInstance_ThenFind_ReturnsTheRegisteredInstance` |
| M11 `FindIndex` default (`logger.Error` + throw) | 82-86 | B: `FindIndex_WhenMultipleSourcesShareKey_ThrowsInvalidOperationException` |
| M12 `Add(string,TKey,VDelegate)` — non-duplicate | 99-103 | A: `Add_WhenSourceAndStoredKeysAreDistinct_...`, `FilterKeys_WhenDistinctStoredKeysCoexist_...`; B: `Find_WhenMultipleSourcesShareKey_...`, `Remove_*`, `Enumeration_*`, `FilterKeys_*` |
| M12 `Add(string,...)` — duplicate throw | 92-97 | A: `Add_WhenSourceAndStoredKeyAreExactDuplicate_ThrowsArgumentException` |
| M13 `Add(UClass)` — non-duplicate | 120 | B: `AddInstance_ThenFind_ReturnsTheRegisteredInstance`, `Indexer_Get_..._Set_ReplacesIt` |
| M13 `Add(UClass)` — duplicate throw | 108-118 | B: `AddInstance_ExactDuplicate_ThrowsArgumentException` |
| M13 `Add(UClass)` — **null instance** | 106-121 | **none** |
| M14 `Remove` — found | 131-134 | B: `Remove_PresentKey_RemovesAndReturnsTrue` |
| M14 `Remove` — not found (`-1`) | 126-129 | B: `Remove_AbsentKey_ReturnsFalse` |
| M14 `Remove` — **key present under a different `SourceId`** | 125-129 | **none** |
| M15 `GetEnumerator()` (generic) | 137 | B: `Enumeration_YieldsAllRegisteredInstancesAndKeysProjection` (via `ToArray`), `FilterKeys_*` |
| M16 **`IEnumerable.GetEnumerator()` (explicit)** | 139 | **none** — `registry.ToArray()` binds to `IEnumerable<UClass>` |
| M17 `Keys` — populated | 141-144 | A: `Add_WhenSourceAndStoredKeysAreDistinct_...`; B: `Enumeration_YieldsAllRegisteredInstancesAndKeysProjection` |
| M17 `Keys` — empty | 141-144 | B: `EmptyRegistry_HasNoKeysAndFindReturnsDefault` |

---

## 4. Coverage Gaps

Ordered by value. Gaps G1-G4 are genuine unexecuted **lines**; G5-G9 are unexecuted **branches or untested contracts** on already-executed lines.

### G1 — `KbdActions(IEnumerable<UClass> list)` constructor (lines 26-29) is entirely unexecuted

No test in the repository calls this overload. It is used in production at six sites (`QfcCollectionController.cs:1265,1284,1295`; `EfcFormController.cs:358,574,631`). This is the largest single unexecuted region of the file and the highest-priority gap.

Three distinct contracts are unproven:
- **Copy semantics.** `new List<UClass>(list)` snapshots the source; later mutation of the caller's list does not affect the registry.
- **Null argument.** `List<T>(IEnumerable<T>)` throws `ArgumentNullException`; `KbdActions` adds no guard and no context.
- **Duplicate bypass.** See G2.

### G2 — Latent production defect: the `IEnumerable` constructor bypasses the duplicate guard, and a production call site relies on that bypass

`Add(string, TKey, VDelegate)` (line 92) and `Add(UClass)` (line 109) both reject a duplicate `SourceId` + stored-key pair with `ArgumentException`. The `IEnumerable` constructor performs **no such check**.

`QuickFiler/Controllers/QfcCollectionController.cs` lines 1265-1272 registers, through that constructor:

```
new KaKey("Collection", Keys.Up,   (k) => SelectPreviousItem()),
new KaKey("Collection", Keys.Down, (k) => SelectNextItem()),
new KaKey("Collection", Keys.Down, (k) => _parent.ActionOkAsync()),
```

Two entries share `SourceId = "Collection"` and `Key = Keys.Down`. The same pair passed to `Add` would throw. Because it arrives through the constructor it is accepted, after which `Find(Keys.Down)` (line 63-67) and therefore `KeyboardHandler.KeyActions[e.KeyCode]` (`KeyboardHandler.cs` line 122) throw `InvalidOperationException`.

Reachability qualifier: `KeyboardHandler.cs` line 118 gates that call on `KeyActions.ContainsKey(e.KeyCode)`, which is `true` here, so the throw is reachable whenever the synchronous `KeyActions` path is active with `Keys.Down`. The synchronous registration at line 1265 is one of two registration paths (`WireUpAsyncKeyboardHandler`, line 1275, installs the async equivalents without the duplicate), so whether it fires in practice depends on which path the collection controller takes at runtime — that determination is `QfcCollectionController.cs` behavior and belongs to **F11**, not to this child.

**Disposition: report only.** Adding a duplicate guard to the constructor would change observable behavior (`QfcCollectionController` would begin throwing at registration time) and would edit a sibling child's production file to remediate. Per `feedback: promote latent defects to issues`, this must be promoted to a real GitHub issue through the MCP promotion lifecycle rather than left as prose in this folder. The test proposed in section 7 (TC-4) **characterizes the current bypass** and cites the issue; it does not assert the bypass is correct.

### G3 — Explicit `IEnumerable.GetEnumerator()` (line 139) is unexecuted

`registry.ToArray()` in `KbdActionsRemainingBranchesTests.Enumeration_YieldsAllRegisteredInstancesAndKeysProjection` binds to the generic `IEnumerable<UClass>` overload. The non-generic explicit implementation needs a cast: `((System.Collections.IEnumerable)registry).GetEnumerator()`.

### G4 — Indexer getter on an unregistered key (lines 36-38) is unexecuted

`this[key]` calls `Find(key)`, which returns `default(UClass)` (`null` for all five production element types, all of which are classes), and then dereferences `.Delegate`. The result is a `NullReferenceException` with no diagnostic context. This is the *only* member of the type that fails without an actionable message; every other failure path throws `ArgumentException` or `InvalidOperationException` with a built message. `KeyboardHandler.cs` guards every indexer read with a preceding `ContainsKey` (lines 118-128, 155-176), so the path is defensive-only in the current drivers — but it is untested and undocumented.

### G5 — Indexer setter against an unregistered key (line 41 false branch, line 46) is unexecuted

`this[key] = value` on a key that was never registered is a **silent no-op**. No exception, no log, no return value. This is a surprising contract and is currently unproven either way.

### G6 — `Remove` where the key exists under a different `SourceId` (lines 125-129) is unexecuted

`Remove` matches on `SourceId` **and** stored key. `Remove_AbsentKey_ReturnsFalse` uses an empty registry, so the "key present, wrong owner" case — the case that actually protects one source from unregistering another source's binding — is not proven. `QfcCollectionController.UnregisterNavigation` (lines 1343-1356) depends on exactly this semantic.

### G7 — `FilterKeys` returning an empty array (line 51) is unexecuted

`KeyboardHandler.KeyDownTaskAsync` line 189 branches on `actions.Length == 0`. The empty-result contract of `FilterKeys` (empty array, not `null`) is the precondition for that branch and is untested.

### G8 — `Add(UClass instance)` with `instance == null` (lines 106-121) is unexecuted

Against a non-empty registry, `_list.Any(x => x.SourceId == instance.SourceId ...)` dereferences `instance` and throws `NullReferenceException`. Against an empty registry, `Any` short-circuits and a `null` element is stored, after which `Keys` (line 143) and `ContainsKey` (line 49) throw. There is no `ArgumentNullException` guard despite the `nameof(instance)` usage at line 118 showing the author was parameter-aware.

### G9 — Only 2 of 5 closed generic instantiations are exercised

See section 2.2 including the coverage-mechanics caveat: this gap is behavioral, not line-numeric. The specific unproven behavior is that `ContainsKey`/`FilterKeys`/`Find` use element-defined `KeyEquals` while `Add`/`Remove` use `EqualityComparer<TKey>.Default` — proven today only for `TKey = string` with `KaStringAsync`'s substring `KeyEquals`.

### Not gaps (recorded so the planner does not re-open them)

- `logger.Error` at lines 85, 96, 117 — all three are executed by existing tests; no logging seam is needed to observe them, because each is immediately followed by a throw that the tests assert on.
- `SentenceJoin` (`UtilitiesCS/Extensions/ArrayExtensions.cs:401`) — separately covered by `UtilitiesCS.Test/Extensions/ArrayExtensions_Tests.cs`. Not in this file's denominator.
- `Find` case-1 vs `FindIndex` case-1 divergence — both covered.

---

## 5. Seam Requirements

**None required. Recommendation: make zero production changes to `KbdActions.cs`.**

Justification against the seam hierarchy in `.claude/rules/csharp.md` lines 49-53:

| Candidate dependency | Assessment |
| --- | --- |
| `log4net.ILog logger` (static, line 17-19) | The only ambient static in the file. Every `logger.Error` call is immediately followed by a `throw` that existing tests already assert on, so no observability seam is needed to reach or verify those lines. Introducing an `ILogger` interface seam (hierarchy level 1) would add a constructor parameter to a type consumed by four production files across three sibling children (F3, F9, F11) — a non-additive change with **zero** coverage benefit. Rejected. |
| `EqualityComparer<TKey>.Default` (line 34) | BCL, deterministic, no external state. No seam. |
| `List<UClass>` (line 31) | In-memory. No seam. |
| COM / Outlook | **Absent.** No `Microsoft.Office.Interop.Outlook` using directive, no `MailItem`, `Store`, `MAPIFolder`, or `Application` reference anywhere in the file. |
| WinForms / UI thread | **Absent.** No `System.Windows.Forms` using directive. `Keys` enters only as a type argument at call sites, and `Keys` is a plain enum requiring no message loop. |
| Clock / timers / randomness | **Absent.** No `DateTime`, `TimeProvider`, `Task.Delay`, `Thread.Sleep`, or `Random`. |

**STA last-resort clause (epic.md Shared Design section 3): not applicable.** No WinForms control is constructed, so no `*.StaTests.cs` file is warranted. All proposed tests run on the default MSTest apartment.

Every gap in section 4 is reachable by direct construction and public-API invocation. This file is the archetype of the "testable seam within an otherwise COM-bound assembly" that `CLAUDE.md` line 303 refuses to exempt.

---

## 6. Cross-Child Contract Impact

**Recommended production change set for this file: empty. Cross-child impact: none.**

Call sites of `KbdActions<>` outside this child's file set (`rg 'KbdActions\s*<' --glob '**/*.cs'`):

| Consumer | Lines | Owning child |
| --- | --- | --- |
| `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` | 21-26 (six properties) | **F3 (this child)** |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 44-88 (six backing fields + properties), 98, 108, 118-128, 155-194 | **F3 (this child)** |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 583-584, 743-744, 1259-1272, 1284-1305, 1349-1360 | **F11** `quickfiler-collection-controller-coverage` |
| `QuickFiler/Controllers/EfcFormController.cs` | 358-372, 568-596, 625-669 | **F9** `quickfiler-efc-form-item-controller-coverage` |
| `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` | 226-233 | test-side (F10 territory) |
| `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs` | 209-210 | test-side |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 59-74 | test-side |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 43-49, 129-135 | test-side |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | 340-393 | test-side |

**Additive-vs-breaking determination:** since no production edit is proposed, the determination is trivially *no change*. If a future planner is tempted to close G2 by adding a duplicate guard to the `IEnumerable` constructor, that is a **breaking** runtime change: `QfcCollectionController.cs:1265-1272` would begin throwing `ArgumentException` during registration. It must not be done in this child. It belongs in the separate defect issue described in G2.

The only file this child modifies for `KbdActions` coverage is a **new test file** plus its `<Compile Include>` entry in `QuickFiler.Test/QuickFiler.Test.csproj`. `QuickFiler.Test.csproj` is a per-project test file, not a shared build property file, so this does not breach the `issue.md` line 76-77 shared-file constraint (which names `coverage.config` and shared property files). It is nonetheless a **merge-conflict hot spot**: every wave-1 sibling adds `<Compile Include>` lines to its own test csproj, and F9/F10/F11 add lines to this same `QuickFiler.Test.csproj`. Append new entries adjacent to the existing block at lines 92-96 to keep the conflict hunk small and mechanically resolvable.

---

## 7. Proposed Test Cases

**Target file (new):** `QuickFiler.Test/Controllers/KbdActionsConstructionAndEdgeTests.cs`
**Required companion edit:** add `<Compile Include="Controllers\KbdActionsConstructionAndEdgeTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` adjacent to line 93.

Rationale for a third file rather than extending the existing two: `KbdActionsTests.cs` is the #111 regression suite (its three methods are the archived regression contract) and `KbdActionsRemainingBranchesTests.cs` documents itself in its class comment as covering a specific enumerated branch set. A separate file keeps both intact and keeps every file well under 500 lines.

Framework: MSTest `[TestClass]`/`[TestMethod]`, FluentAssertions. **No Moq needed** — `IKbdAction<,>` implementers are concrete value objects and mocking them would add indirection without isolation benefit. Arrange-Act-Assert throughout. No STA, no timers, no files.

Every case below was cross-referenced against section 3; none duplicates an existing test.

| ID | Method name | Gap | Arrange / Act / Assert | Seam or mock |
| --- | --- | --- | --- | --- |
| TC-1 | `Ctor_FromEnumerable_CopiesAllElementsInOrder` | G1 | **A:** `var seed = new List<KaKey> { new("src", Keys.Up, _ => {}), new("src", Keys.Down, _ => {}) }`. **Act:** `var registry = new KbdActions<Keys, KaKey, Action<Keys>>(seed)`. **Assert:** `registry.Keys.Should().Equal(Keys.Up, Keys.Down)`; `registry.Should().HaveCount(2)`. | none |
| TC-2 | `Ctor_FromEnumerable_SnapshotsSource_LaterMutationDoesNotAffectRegistry` | G1 | **A:** seed list with one `KaKey`; construct registry. **Act:** `seed.Add(new KaKey("src", Keys.Escape, _ => {}))`. **Assert:** `registry.ContainsKey(Keys.Escape).Should().BeFalse(...)`; `registry.Keys.Should().HaveCount(1)`. | none |
| TC-3 | `Ctor_FromEnumerable_WithNullList_ThrowsArgumentNullException` | G1 | **A:** `IEnumerable<KaKey> source = null`. **Act:** `Action act = () => new KbdActions<Keys, KaKey, Action<Keys>>(source)`. **Assert:** `act.Should().Throw<ArgumentNullException>()`. | none |
| TC-4 | `Ctor_FromEnumerable_AcceptsDuplicateSourceAndKey_UnlikeAdd` | G1, G2 | **A:** seed list containing two `KaKey("Collection", Keys.Down, ...)` entries — mirrors `QfcCollectionController.cs:1268-1270`. **Act:** construct the registry; then `Action find = () => registry.Find(Keys.Down)`. **Assert:** construction `.Should().NotThrow()`; `registry.Keys.Should().HaveCount(2)`; `find.Should().Throw<InvalidOperationException>()`. XML comment must state this is a **characterization** test for the G2 latent defect and cite the promoted issue number, not an endorsement. | none |
| TC-5 | `Indexer_Set_WhenKeyNotRegistered_IsSilentNoOp` | G5 | **A:** empty registry; `Action<Keys> replacement = _ => {}`. **Act:** `Action act = () => registry[Keys.Enter] = replacement`. **Assert:** `act.Should().NotThrow()`; `registry.Keys.Should().BeEmpty()`; `registry.ContainsKey(Keys.Enter).Should().BeFalse()`. | none |
| TC-6 | `Indexer_Get_WhenKeyNotRegistered_ThrowsNullReferenceException` | G4 | **A:** empty registry. **Act:** `Action act = () => _ = registry[Keys.Enter]`. **Assert:** `act.Should().Throw<NullReferenceException>(because: "the indexer getter dereferences Find's default(UClass) with no guard; callers must gate on ContainsKey as KeyboardHandler does")`. | none |
| TC-7 | `NonGenericEnumerator_YieldsTheSameInstancesAsGenericEnumerator` | G3 | **A:** registry with two `KaKey` entries. **Act:** enumerate `((System.Collections.IEnumerable)registry).GetEnumerator()` into an `object[]`; also `registry.ToArray()`. **Assert:** the non-generic sequence `.Should().Equal(generic.Cast<object>())`. | none |
| TC-8 | `Remove_WhenKeyPresentUnderDifferentSourceId_ReturnsFalseAndRetainsEntry` | G6 | **A:** registry with `Add("ownerA", Keys.Enter, _ => {})`. **Act:** `var removed = registry.Remove("ownerB", Keys.Enter)`. **Assert:** `removed.Should().BeFalse()`; `registry.ContainsKey(Keys.Enter).Should().BeTrue(because: "removal is scoped to the registering SourceId")`. | none |
| TC-9 | `FilterKeys_WhenNoElementMatches_ReturnsEmptyArrayNotNull` | G7 | **A:** registry with `Keys.Enter` registered. **Act:** `var matches = registry.FilterKeys(Keys.F12)`. **Assert:** `matches.Should().NotBeNull().And.BeEmpty()`. | none |
| TC-10 | `AddInstance_WhenInstanceIsNull_AgainstPopulatedRegistry_ThrowsNullReferenceException` | G8 | **A:** registry with one entry. **Act:** `Action act = () => registry.Add((KaKey)null)`. **Assert:** `act.Should().Throw<NullReferenceException>(because: "Add(UClass) has no null guard; the duplicate scan dereferences the argument")`. Characterization test; comment must note the missing `ArgumentNullException` guard. | none |
| TC-11 | `CharInstantiation_MatchingAndStorageIdentityAgree_WhenKeyEqualsIsPlainEquality` | G9 | **A:** `new KbdActions<char, KaChar, Action<char>>()`; `Add("src", '1', _ => {})`; `Add("src", '2', _ => {})`. **Act:** `ContainsKey('1')`, `FilterKeys('1')`, `Find('1')`, `Remove("src", '1')`. **Assert:** `ContainsKey` true; `FilterKeys` yields exactly `'1'` (contrast with the `KaStringAsync` substring case proven in `KbdActionsTests.FilterKeys_WhenDistinctStoredKeysCoexist_...`); `Find` returns the single entry; `Remove` true and `ContainsKey('1')` then false. | none |
| TC-12 | `KeyAsyncInstantiation_AddFindRemoveRoundTrip_PreservesAwaitableDelegate` | G9 | **A:** `new KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>()`. **Act:** `Add("src", Keys.Return, _ => Task.CompletedTask)`; read `registry[Keys.Return]`; `await` it; `Remove`. **Assert:** the awaited task completes (`Task.CompletedTask`, no delay); `Remove` returns true. Method is `async Task`. | none |
| TC-13 | `CharAsyncInstantiation_AddThroughNewConstraint_ConstructsElementAndStoresDelegate` | G9 | **A:** `new KbdActions<char, KaCharAsync, Func<char, Task>>()`. **Act:** `Add("src", 'S', _ => Task.CompletedTask)` — exercises `UClass instance = new()` (line 99) against `KaCharAsync`'s parameterless ctor. **Assert:** `registry.Find('S').SourceId.Should().Be("src")`; `.Key.Should().Be('S')`; `.Delegate.Should().NotBeNull()`. | none |

**Count: 13 discrete test cases.** Each is individually nameable and becomes its own atomic plan task per the epic's per-file mandate.

Sequencing note for the planner: TC-4 depends on the G2 defect issue existing (it cites the issue number in its XML comment). Promote the issue before authoring TC-4, or author TC-4 last.

---

## 8. Risks and Open Questions

1. **F1's ledger could contradict section 1.1.** It cannot exempt this file without contradicting `CLAUDE.md` line 303, but the ledger is the stated authority. If F1 classifies `KbdActions.cs` as anything other than `testable`, halt and escalate rather than proceeding.
2. **The G2 defect issue must be promoted before it evaporates.** Per the recorded feedback `promote-latent-defects-to-issues`, prose in a feature folder disappears at merge. Both G2 (duplicate bypass) and G8 (missing null guard) need real GitHub issues.
3. **`QuickFiler.Test.csproj` merge conflicts.** F9, F10, and F11 all add `<Compile Include>` entries to the same file in the same wave. Mitigation is placement discipline (section 6), not avoidance.
4. **Line-coverage headroom is small; do not mistake activity for progress.** Static analysis indicates only lines 28 and 139 are currently unexecuted. The measured figure may already exceed 80%. The value of this work is branch and contract coverage (G4-G9) plus documenting three undocumented failure modes — not a large percentage delta. The planner should set expectations accordingly and let F1's harness supply the actual number.
5. **Open question: is the synchronous `KeyActions` path live?** Whether `QfcCollectionController.RegisterKeyActions` (the line 1265 path) executes in the shipped flow, versus `WireUpAsyncKeyboardHandler` (line 1275), determines whether G2 is a live user-facing crash or a dormant one. That determination requires reading `QfcCollectionController.cs` control flow, which is **F11's** file. Record the question in the promoted issue; do not resolve it here.
6. **Characterization tests lock in current behavior.** TC-4, TC-6, and TC-10 assert behavior that is arguably wrong. Each must carry an XML comment naming it a characterization test and citing the defect issue, so a future fix updates the test deliberately rather than being blocked by it.

---

## 9. Sources

| File | Lines read | Used for |
| --- | --- | --- |
| `QuickFiler/Controllers/KbdActions.cs` | 1-147 (whole file) | Structural inventory, branch enumeration |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 1-415 (whole file) | Consumer behavior; `ContainsKey` gating of indexer reads (118-128, 155-194); `[ExcludeFromCodeCoverage]` at 22 |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 1260-1399 | G2 duplicate registration (1265-1272); `UnregisterNavigation` (1343-1356); `GenerateStringKbdAction` (1363-1385) |
| `QuickFiler/Interfaces/IKbdAction.cs` | 1-18 (whole file) | Generic constraint surface |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | 1-88 (whole file) | Existing coverage map (suite A) |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | 1-181 (whole file) | Existing coverage map (suite B) |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 92-96 | Compile-include registration point |
| `QuickFiler/QuickFiler.csproj` | 13-14, 307-310, 359 | Target framework, LangVersion, compiled-surface confirmation |
| `CLAUDE.md` | 288-309 (§ UT2) | Non-exemption clause at line 303 |
| `.claude/rules/csharp.md` | 1-97 (whole file) | Seam hierarchy (49-53); coverage floors (39-41); verified absence of any `KbdActions` mention |
| `.claude/rules/general-unit-test.md` | provided in session context | Coverage Exclusion Policy; Determinism Infrastructure |
| `docs/features/epics/quickfiler-per-file-coverage/epic.md` | 1-418 (whole file) | Shared Design 1-6; F3 assignment (267-274); Known Conflict Risks (405-418) |
| `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/issue.md` | 1-95 (whole file) | Acceptance criteria, constraints |
| `docs/features/archive/2026-03-27-quickfiler-navigation-key-collision-111/policy-audit.2026-03-27T13-28.md` | 72-75 | Stale 46.43% / 31.25% measurement |
| `coverage.config` | 1-24 (whole file) | Confirmed no module-path exclusion touches QuickFiler |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | path existence only | Named as F1 harness source |

**Search commands run:** `rg 'KbdActions\s*<' --glob '**/*.cs'`; `rg 'IKbdAction' --glob '**/*.cs'`; `rg 'DelegateType|\.Activated|ToggleControl' --glob '**/*.cs'`; `rg 'SentenceJoin' --glob '**/*.cs'`; `rg 'new Ka(Char|Key|StringAsync|CharAsync|KeyAsync)\s*\(' --glob 'QuickFiler/**/*.cs'`; `rg '(charActionsAsync|keyActionsAsync|charAsync|charActions|keyActions)\.Add\(' QuickFiler.Test`; `rg 'KbdActions' --glob '**/*.md'`.
