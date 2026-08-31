# Research — Issue #469 residual-scope verification (`qfc-collection-move-diagnostics-defects`)

- **Date:** 2026-08-29T12-31
- **Issue:** #469
- **Branch:** `bug/qfc-collection-move-diagnostics-defects-469`, cut from `origin/main` = `ecdb1c84ba8541ab67042985919cfed4df768c01`
- **Scope:** research only. No production or test source file was modified. No C# toolchain command was run.
- **Tool limitation affecting this artifact:** the Bash tool is disabled in this session, so no `git` command could be executed. Every item below that would normally be confirmed with `git show`/`git log` is marked **unverified** and the reason is stated inline. All other findings are read directly from the current working tree and cited `file:line`.

---

## 0. Executive summary

| Claim | Verdict |
|---|---|
| C1 — Defect "unreachable null guard" is fixed | **HELD** |
| C2 — Defect "trailing null element" is fixed | **HELD** |
| C3 — Defect "positional access into a `ConcurrentDictionary`" is fixed | **HELD** |
| C4 — Defect 4 not satisfied against the issue's literal Expected Behavior | **HELD** |
| C5 — the doc comment's "same instance" claim is true end to end | **PARTIALLY HELD** — true in the steady-state production configuration, not unconditionally. The premise stated in the delegation prompt (static `Globals` vs injected `_globals`) is **refuted**; a different and real divergence mechanism was found. |
| C6 — stale comment in `QfcHomeController.Metrics.cs` | **HELD**, plus a second stale copy of the same false statement in a test file that the prompt did not name |

Additional material finding: **the residual scope of defect 4 is already tracked as GitHub issue #629**, promoted on 2026-08-26 (`docs/features/potential/promoted/2026-08-26-qfc-remove-stackmoveditems-parameter.md:11-12`). Issue #469 does not need to re-open that work.

---

## 1. Numbering discrepancy that must be resolved before acceptance criteria are written

The issue text and the landed code use **inverted numbering for defects 1 and 2**. This is not a semantic disagreement — both defects are fixed — but any acceptance criterion that says "#469 defect 1" is ambiguous today.

| Source | "defect 1" means | "defect 2" means |
|---|---|---|
| `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/issue.md:30` and `:41` | unreachable null guard | trailing null element |
| `docs/features/active/qfc-collection-controller-defects-468/spec.md:92-93` | unreachable null guard | trailing null element |
| `QuickFiler/Controllers/QfcCollectionController.cs:2362` and `:2372` | trailing null element (array length) | unreachable null guard |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:275` and `:351-352` | trailing null element (array length) | unreachable null guard |

The issue and the #468 spec agree with each other. The shipped source comments and the shipped test doc comments agree with each other and disagree with both. **Recommendation:** treat the issue's numbering as authoritative (it is the published GitHub text) and, if any work lands on this branch, correct the two source comments and the two test doc comments rather than renumbering the issue.

Below, this document uses the **issue's** numbering and cross-references the code's label where relevant.

---

## 2. Claim-by-claim verification

### C1 — Defect 1 (unreachable null guard in `GetMoveDiagnostics`): **HELD**

Current `GetMoveDiagnostics` body, `QuickFiler/Controllers/QfcCollectionController.cs:2350-2416`.

- `QuickFiler/Controllers/QfcCollectionController.cs:2370` — `var qf = TryGetItemGroupByIndex(k)?.ItemController;` is the only producer of `qf`.
- `QuickFiler/Controllers/QfcCollectionController.cs:2377-2383` — the guard:

  ```csharp
  if (qf is null)
  {
      strOutput[k] =
          $"{dataLineBeg} ,QuickFiled,{durationText},{durationMinutesText},"
          + "To Unknown,Sender Unknown,Email,Folder Unknown,Sent Date Unknown,Sent Time Unknown";
      continue;
  }
  ```

- The **first** dereference of `qf` after the guard is `QuickFiler/Controllers/QfcCollectionController.cs:2385` (`var helper = qf.ItemHelper;`). The other dereferences are at `:2408` and `:2410`.
- **Verification that nothing dereferences `qf` above the guard:** between the assignment at `:2370` and the guard at `:2377` the only lines are the comment block `:2372-2376`. There is no statement of any kind. The guard therefore dominates every dereference. The code labels this "Issue #469 defect 2" at `:2372`.

The regression test that pins this is `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:369` (`GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing`), which asserts both non-throw and the presence of the `"To Unknown,Sender Unknown,Email,Folder Unknown"` text produced only by the guard branch.

### C2 — Defect 2 (trailing null element): **HELD**

- `QuickFiler/Controllers/QfcCollectionController.cs:2366` — `string[] strOutput = new string[_itemGroupsToMove.Count];` — no `+ 1`.
- `QuickFiler/Controllers/QfcCollectionController.cs:2367` — `var loopTo = _itemGroupsToMove.Count;`
- `QuickFiler/Controllers/QfcCollectionController.cs:2368` — `for (k = 0; k < loopTo; k++)`.

Allocation length and loop bound are read from the same expression, so length and iteration count cannot diverge.

**Every index in `0..Count-1` is assigned on both branches:**
- null branch — assigned at `:2379`, then `continue` at `:2382`;
- non-null branch — no `continue`, `break`, `return`, or `goto` exists between `:2385` and the assignment at `:2412` (`strOutput[k] = dataLine;`), which is the last statement of the loop body before `:2413`.

The only way an index could be skipped is an exception escaping the loop, which would abandon the whole array rather than return a partially filled one. The COM interaction at `:2393-2405` (`olAppointment.Body` / `olAppointment.Save()`) is the sole throw candidate on the non-null branch; it is on the exception path, not on a return path.

Pinned by `QfcCollectionControllerDefects468MoveTests.cs:290` (`GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine`) and `:327` (`GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls`, which asserts `NotContainNulls`).

**Residual note (not a defect claim, but relevant to any new work):** `QuickFiler/Controllers/QfcCollectionController.cs:2366` dereferences `_itemGroupsToMove.Count` with no null check, while `TryGetItemGroupByIndex` at `:2342` does check for null. `GetMoveDiagnostics` therefore throws `NullReferenceException` if invoked before `CacheItemGroupsForMove()` or after `Cleanup()`. This is pre-existing and outside issue #469's four defects.

### C3 — Defect 3 (positional access into a `ConcurrentDictionary`): **HELD**

- Field declaration: `QuickFiler/Controllers/QfcCollectionController.cs:76` — `private IReadOnlyList<QfcItemGroup> _itemGroupsToMove;` (rationale comment at `:71-75`).
- Bounds-checked indexer read: `QuickFiler/Controllers/QfcCollectionController.cs:2340-2348`:

  ```csharp
  private QfcItemGroup TryGetItemGroupByIndex(int index)
  {
      if (_itemGroupsToMove is null || index < 0 || index >= _itemGroupsToMove.Count)
      {
          return null;
      }

      return _itemGroupsToMove[index];
  }
  ```

- **No `ConcurrentDictionary` remains on this path.** A search of the whole `QuickFiler/` tree for `ConcurrentDictionary|ElementAt` returns exactly two hits: `QuickFiler/Controllers/QfcCollectionController.cs:73` (the historical explanation inside the new comment) and `QuickFiler/Controllers/EfcFormController.cs:178` (`itemTlpRows.ElementAt(4)`, an unrelated EFC row lookup). `System.Collections.Concurrent` is still imported at `QuickFiler/Controllers/QfcCollectionController.cs:2` but is used for `ConcurrentBag<Task> BackgroundLoadingTasks` at `:85`.

- **Every assignment to `_itemGroupsToMove` preserves the order of `_itemGroups`.** There are exactly two assignments:
  1. `QuickFiler/Controllers/QfcCollectionController.cs:729` — `_itemGroupsToMove = _itemGroups.ToList();` inside `CacheItemGroupsForMove()`. `_itemGroups` is declared `private List<QfcItemGroup> _itemGroups;` at `:297`, and `List<T>.ToList()` is an ordered copy.
  2. `QuickFiler/Controllers/QfcCollectionController.cs:875` — `_itemGroupsToMove = Array.Empty<QfcItemGroup>();` inside `CleanupBackground()`; an empty collection is order-trivial and preserves the previous non-null, zero-length post-clear semantics (comment at `:871-874`).

  No other write site exists. `CacheItemGroupsForMove()` is called from `SwapItemGroups` at `:741`.

Pinned by `QfcCollectionControllerDefects468MoveTests.cs:45` (structural: declared type is assignable to `IReadOnlyList<QfcItemGroup>`) and `:83` (`TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation`, behavioural).

### C4 — Defect 4 (`MoveEmailsAsync` ignores `stackMovedItems`): **HELD — not satisfied against the issue's literal Expected Behavior**

The issue's Expected Behavior item 4, verbatim from `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/issue.md:70-71`:

> 4. Either `MoveEmailsAsync` populates the undo stack it is handed, or the parameter is removed from
>    the contract.

Current state — the parameter is **retained**, **discarded**, and **documented as a deferral**.

Operative lines, `QuickFiler/Controllers/QfcCollectionController.cs:2241-2260`:

```csharp
/// <summary>
/// Moves every cached item group's message to its assigned destination folder.
/// </summary>
/// <param name="stackMovedItems">
/// The undo stack. This parameter does not carry the undo records: the stack is populated
/// by the email filer's push-to-undo-stack path, which pushes onto
/// <c>Globals.AF.MovedMails</c>. That is the same instance the caller passes here, because
/// the caller reads it from the same globals object. Passing a different instance would not
/// redirect the undo records, and passing <c>null</c> does not suppress them. The parameter
/// is retained only for source compatibility with existing callers; removing it is a
/// follow-up candidate, not part of this change.
/// </param>
public async Task MoveEmailsAsync(SloStack<IMovedMailInfo> stackMovedItems)
{
    //TraceUtility.LogMethodCall(stackMovedItems);

    // The parameter is deliberately discarded rather than left untouched. The undo records
    // reach the stack through the email filer, not through this argument, and the discard
    // states that at the point of use so the parameter cannot be read as an oversight.
    _ = stackMovedItems;
```

The same doc block is duplicated verbatim on the interface at `QuickFiler/Interfaces/IQfcCollectionController.cs:51-63` (parameter named `StackMovedItems` there).

Neither disjunct of the issue's Expected Behavior is met: the method does not populate the stack, and the parameter is not removed. The chosen route is a third one — document the true mechanism and defer removal.

**Already-tracked follow-up.** `docs/features/potential/promoted/2026-08-26-qfc-remove-stackmoveditems-parameter.md` records the removal as **GitHub issue #629** (`:11-12`), promoted 2026-08-26 from `[P14-T5]` of the #468 branch, with the deferral rationale at `:35-41`: `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` was outside the #468 owned file set and named must-not-touch by decision D11 of `docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md`.

### C5 — "undo records reach the caller's stack instance": **PARTIALLY HELD (see verdict and confidence below)**

#### C5.a The prompt's static-vs-injected premise is refuted

`EmailFiler`'s `Globals` is **not** a static class. It is an instance property:

- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:71-76` — `private IApplicationGlobals _globals = default!;` with `internal IApplicationGlobals Globals { get => _globals; set => _globals = value; }`.
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:373` — `Globals ??= Config.Globals!;` inside `ValidateParameters()`.
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:185-189` — `PushToUndoStack` reads `Globals.Ol.Root.FolderPath` then `Globals.AF.MovedMails.Push(info);`.

The one genuinely static `Globals` class in the repository is the VSTO-generated `TaskMaster/ThisAddIn.Designer.cs:171` (`internal sealed partial class Globals`), which is in the `TaskMaster` assembly and is not referenced by any member of the QuickFiler move path. Two other identifiers that read like a static `Globals` are also instance properties: `QuickFiler/Controllers/QfcHomeController.cs:152` (`internal IApplicationGlobals Globals { get; set; }`, the symbol used in `QfcHomeController.Metrics.cs:77`, `:101`, `:131`) and `TaskMaster/Ribbon/RibbonController.cs:40` (`protected internal ApplicationGlobals Globals { get; set; }`).

**There is therefore no static/injected split on this path.** The doc comment's phrase `Globals.AF.MovedMails` refers to `EmailFiler`'s own injected property, not to a static.

#### C5.b The injected chain is single-instance end to end

Verified link by link:

1. `QuickFiler/Controllers/QfcFormController.cs:40` — `_globals = appGlobals;` (constructor).
2. `QuickFiler/Controllers/QfcFormController.cs:49` — `_movedItems = _globals.AF.MovedMails;` — the caller's captured stack reference.
3. `QuickFiler/Controllers/QfcFormController.Actions.cs:49-58`, `:83-92`, `:139-148` — all three `new QfcCollectionController(...)` sites pass `AppGlobals: _globals`.
4. `QuickFiler/Controllers/QfcCollectionController.cs:47` — `_globals = AppGlobals;`.
5. `QuickFiler/Controllers/QfcCollectionController.cs:695-704` — `new QfcItemController(appGlobals: _globals, ...)`.
6. `QuickFiler/Controllers/QfcItemController.MailActions.cs:125-134` — `new EmailFilerConfig() { ... Globals = _globals, ... }`.
7. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:373` — `Globals ??= Config.Globals!;`.
8. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:188` — `Globals.AF.MovedMails.Push(info);`.

Both the caller's read (step 2) and the filer's push (step 8) go through the **same `IApplicationGlobals` instance**. That part of the doc comment is correct.

#### C5.c The real divergence mechanism: `AppAutoFileObjects.Initialized<T>` does not memoize

The doc comment's inference "same globals object, therefore same stack instance" is only valid if `IApplicationGlobals.AF.MovedMails` is a stable reference. It is not, unconditionally.

- `TaskMaster/AppGlobals/AppAutoFileObjects.cs:177-181`:

  ```csharp
  private SloStack<IMovedMailInfo> _movedMails;
  public SloStack<IMovedMailInfo> MovedMails
  {
      get => Initialized(_movedMails, LoadMovedMails);
  }
  ```

- `TaskMaster/AppGlobals/AppAutoFileObjects.cs:43-50`:

  ```csharp
  private T Initialized<T>(T obj, Func<T> initializer)
  {
      if (obj is null)
      {
          obj = initializer.Invoke();
      }
      return obj;
  }
  ```

  `obj` is a **by-value parameter**. The helper never writes back to the field. While `_movedMails` is null, every read of `MovedMails` invokes `LoadMovedMails()` again.

- `TaskMaster/AppGlobals/AppAutoFileObjects.cs:183-201` — `LoadMovedMails()` returns `SloStack<IMovedMailInfo>.Static.Deserialize(...)` when `PythonStaging` resolves, and `null` otherwise.
- `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStack.cs:243-262` — every `Static.Deserialize` overload calls `GetInstance()`, which is `new SmartSerializable<SloStack<T>>()`, so each call produces a **new** object.
- `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs:312-362` — with `askUserOnError: false` the method returns a non-null instance on every path (`CreateEmpty` on `FileNotFoundException` and on any other exception, `:338` and `:349`).

The only write to the backing field is `TaskMaster/AppGlobals/AppAutoFileObjects.cs:208` — `await Task.Run(() => _movedMails = LoadMovedMails());` inside `LoadMovedMailsAsync()`, which runs once per startup from `LoadParallelAsync` (`:76`) or `LoadSequentialAsync` (`:93`).

**Consequences:**

1. **Steady-state production (`PythonStaging` resolved, `AF.LoadAsync` completed):** `_movedMails` is a single cached non-null instance, so `_movedItems` and the filer's push target are the same object. The doc comment is true here. Startup ordering supports this: `SpecialFolders` is populated synchronously in the `AppFileSystemFolderPaths` constructor (`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs:22-26`, `:213`, `PythonStaging` added at `:292`), which runs inside `LoadBasicMethod` (`TaskMaster/AppGlobals/ApplicationGlobals.cs:111-114`) before `_autoFileObjects.LoadAsync()` at `:137`. `TryAddSpecialFolder` inserts the key at `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs:143` before `CreateMissingPaths`, so a missing directory does not remove the key.
2. **`PythonStaging` unresolvable:** `LoadMovedMails()` returns `null` (`AppAutoFileObjects.cs:199`), `_movedMails` stays null, and every read returns `null`. `_movedItems` is then `null` **and** `EmailFiler.cs:188` raises `NullReferenceException`. This configuration is already codified by two existing tests: `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs:94` and `:113` both assert `movedMails.Should().BeNull()`, and `:193` asserts it stays null after `LoadMovedMailsAsync` runs against missing configuration.
3. **Any read taken while `_movedMails` is still null but `LoadMovedMails()` can succeed** (for example, a read racing `LoadMovedMailsAsync` during `LoadParallelAsync`) returns a fresh, distinct `SloStack` that is not the instance eventually cached. In that window the doc comment's identity claim is false. I found no production read of `AF.MovedMails` inside the startup window — the QuickFiler form is constructed long after startup — so this window is not demonstrably reachable in the shipped flow.

#### C5 verdict and confidence

**Verdict:** the doc comment's factual claim is **true in the steady-state production configuration and false as an unconditional statement**. It omits a load-bearing precondition: that `AppAutoFileObjects._movedMails` has already been assigned by `LoadMovedMailsAsync`. In the degraded `PythonStaging`-missing configuration the undo record is not merely redirected — the push throws.

**Confidence:**
- That `EmailFiler` and `QfcFormController` resolve the same `IApplicationGlobals` instance in the QuickFiler flow: **high** (eight-link chain read directly from source, cited above).
- That `Initialized<T>` does not memoize and each `LoadMovedMails()` call constructs a new object: **high** (both the helper and `SloStack.Static.Deserialize`/`SmartSerializable.Deserialize` were read).
- That the divergence is reachable in a real, shipped configuration: **low to moderate**. The `PythonStaging`-missing case is reachable and is already test-documented, but it produces a null on both sides rather than two different stacks; the two-distinct-instances case requires a read inside the startup window that I could not find a production caller for. **This does not raise issue #469 to High severity.** It is a defect of `AppAutoFileObjects`, in a different assembly and a different file, and it is not one of #469's four defects.

**Recommendation for C5:** do not widen #469. If this is to be pursued, promote it as its own defect against `TaskMaster/AppGlobals/AppAutoFileObjects.cs:43-50` ("`Initialized<T>` takes the backing field by value and never memoizes, so a failed first load re-runs the loader on every property read"), noting that the same helper also backs `Encoder` (`:441`) and `SubjectMap` (`:462`).

### C6 — stale comment in `QfcHomeController.Metrics.cs`: **HELD**

Exact current text, `QuickFiler/Controllers/QfcHomeController.Metrics.cs:171-174`:

```csharp
// GetMoveDiagnostics returns an array one element longer than it fills, so its trailing
// element is null; dropping null and whitespace-only entries keeps blank rows out of
// the CSV.
var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
```

**The comment is now false.** Per C2, the array length equals `_itemGroupsToMove.Count` and every index is assigned.

**Is the filter vacuous?** It depends on which contract you measure against.

- **Against the production implementation: yes, vacuous.** Both branches of `GetMoveDiagnostics` assign a string that begins with `dataLineBeg` and contains literal commas (`QfcCollectionController.cs:2379-2381` and `:2407-2412`), so no element can be `null`, empty, or whitespace-only. `dataLineBeg` itself is always non-blank at both call sites (`QfcHomeController.Metrics.cs:48` and `:129`, both formatted as `"MM/dd/yyyy,hh:mm,"`).
- **Against the interface contract: no, still load-bearing.** The call is made through `IQfcCollectionController.GetMoveDiagnostics` (`QuickFiler/Interfaces/IQfcCollectionController.cs:122-129`), which carries **no** XML documentation and therefore no non-null guarantee. A test double supplying nulls exists today: `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:403` (`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`) feeds `new[] { "line-one", "   ", null, "line-two" }` through a `Mock<IQfcCollectionController>` and asserts only `"line-one"` and `"line-two"` reach the writer. **Deleting the filter would fail that test.**

**Second stale copy the prompt did not name.** `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:397-401` repeats the same false statement as the test's own doc comment:

```csharp
/// <summary>
/// GetMoveDiagnostics returns an array one element longer than it fills, so its trailing
/// element is null. Null and whitespace-only entries must be dropped before the write rather
/// than producing a blank CSV line.
/// </summary>
```

The test's behaviour is still correct and worth keeping; only its stated justification is stale.

**Asymmetry between the two `GetMoveDiagnostics` call sites in that file:**

| Call site | Enclosing method | Filters? | Writes via |
|---|---|---|---|
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs:92-99` | `QuickFileMetrics_WRITE(string filename)` (`:36`) | **No** — `strOutput` passed straight through | `FileIO2.WriteTextFile(filename, strOutput, myDocuments)` at `:103`, guarded by the `MyDocuments` lookup at `:101` |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs:162-169` | `WriteMetricsAsync(string filename)` (`:107`) | **Yes** — `:174` | the injectable `MetricsFileWriter` seam (`:28-34`, default `FileIO2.WriteTextFileAsync`) at `:179`, with `CancellationToken.None` |

The **async** path is the one production uses: `QuickFiler/Controllers/QfcFormController.cs:47` binds `WriteMetrics = parent.WriteMetricsAsync`, and `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:229` invokes that delegate. `QfcHomeController.QuickFileMetrics_WRITE` is declared on `IFilerHomeController` (`QuickFiler/Interfaces/IFilerHomeController.cs:41`) but I found no production call site reaching the QFC implementation; the only direct callers are tests (`QfcHomeControllerMetricsTests.cs:36`, `:51`, `:281`) and the non-compiled legacy file (`QuickFiler/Legacy/QuickFileController.cs:694`, see §4). The `EfcHomeController` overloads (`QuickFiler/Controllers/EfcHomeController.Metrics.cs:13`, `:43`, `:59`) are a separate implementation.

The asymmetry is therefore harmless today but is an inconsistency: the filtered path is live, and the unfiltered path appears to be effectively dead in production.

---

## 3. E1 — every site that must change if `stackMovedItems` is removed

### Searches used and cross-check

- **Search A (method-name family):** pattern `MoveEmails` across `*.cs`. This is deliberately broader than `MoveEmailsAsync` so that any differently-named overload or legacy sibling surfaces.
- **Search B (parameter type):** pattern `SloStack<IMovedMailInfo>` across the whole repository, unfiltered by extension.
- **Search C (reflection by string name):** pattern `"MoveEmailsAsync"` (with quotes) across the whole repository, plus a separate sweep of `GetMethod(` / `InvokeMember(` across `*.cs`.
- **Search D (file-level confirmation):** pattern `MoveEmailsAsync`, `files_with_matches`, unfiltered — 92 files, of which exactly 4 are `.cs`; the remaining 88 are `.md` documents, `.trx` test result files, and Cobertura `.xml` coverage artifacts under `docs/features/`.

**Agreement:** Searches A, B, C and D agree on the same four `.cs` files and the same eight code lines. There is **no disagreement to resolve**. Search A additionally surfaced two non-compiled legacy members and one non-compiled interface member with the shorter name `MoveEmails`; these are reported separately below and are **not** part of the totals, because they are different members and are not compiled (§4).

### Enumerated sites

**Declaration sites — TOTAL 2**

| # | Site | Text |
|---|---|---|
| 1 | `QuickFiler/Interfaces/IQfcCollectionController.cs:63` | `Task MoveEmailsAsync(SloStack<IMovedMailInfo> StackMovedItems);` (preceded by the XML doc block at `:51-62`, which also mentions the parameter and would need its `<param>` element removed) |
| 2 | `QuickFiler/Controllers/QfcCollectionController.cs:2253` | `public async Task MoveEmailsAsync(SloStack<IMovedMailInfo> stackMovedItems)` (preceded by the XML doc block at `:2241-2252`; the discard at `:2260` and its explanatory comment at `:2257-2259` must also be removed) |

**Production invocation sites — TOTAL 1**

| # | Site | Text |
|---|---|---|
| 1 | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225` | `await _groups.MoveEmailsAsync(_movedItems);` |

Note: removing the argument does not orphan `_movedItems`. It is still declared at `QuickFiler/Controllers/QfcFormController.cs:86`, assigned at `:49`, and nulled at disposal; other consumers must be re-checked before any further cleanup (out of scope for this enumeration).

**Test invocation sites — TOTAL 5**, all in `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`

| # | Line | Text | Enclosing test method |
|---|---|---|---|
| 1 | `:165` | `Func<Task> act = () => controller.MoveEmailsAsync(null);` | `MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException` (`:144`) |
| 2 | `:216` | `Func<Task> act = () => controller.MoveEmailsAsync(null);` | `MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime` (`:195`) |
| 3 | `:263` | `Func<Task> act = () => controller.MoveEmailsAsync(null);` | `MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow` (`:251`) |
| 4 | `:484` | `Func<Task> withNullStack = () => controller.MoveEmailsAsync(null);` | `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` (`:471`) |
| 5 | `:485` | `Func<Task> withSuppliedStack = () => controller.MoveEmailsAsync(stack);` | same as #4 |

**Moq `Setup` / `Verify` / `It.IsAny` expressions naming the method or its parameter type — TOTAL 0**

Verified two ways. (a) No occurrence of `MoveEmailsAsync` appears in any file containing `Mock<IQfcCollectionController>`; the 16 `Mock<IQfcCollectionController>` construction sites are in `QfcHomeControllerMetricsTests.cs`, `QfcHomeControllerIterationTests.cs`, `QfcFormControllerTests.cs`, `QfcFormControllerDeactivateTests.cs`, `QfcItemController.*Tests.cs`, and `QfcItemController.TestSupport.cs:389`, and none of them names this member. (b) The only `SloStack<IMovedMailInfo>` occurrence anywhere in `QuickFiler.Test` is a direct construction at `QfcCollectionControllerDefects468MoveTests.cs:481`, not a Moq matcher.

One test is worth naming because it drives the caller and could be mistaken for a dependency: `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:445` (`BackGroundMoveAsync_ShouldMoveEmails`) calls `_controller.BackGroundMoveAsync()` at `:451` with no assertions and no `MoveEmailsAsync` setup. It requires **no** change.

**Reflection-based invocation by string name — TOTAL 0**

The literal `"MoveEmailsAsync"` appears **nowhere** in the repository (zero matches, unfiltered by extension). The repository-wide sweep of `GetMethod(` / `InvokeMember(` returned 60+ hits, none of which names this member or `IQfcCollectionController`. The reflection helpers used against this class (`QfcCollectionControllerTestSupport.InvokeNonPublic`, e.g. `QfcCollectionControllerDefects468MoveTests.cs:409`) target `"TryGetItemGroupByIndex"`, and the field helpers target `"_itemGroupsToMove"` and `"_itemGroups"`.

### E1 totals

| Category | Exact total |
|---|---|
| Declaration sites (interface + implementations) | **2** |
| Production invocation sites | **1** |
| Test invocation sites | **5** |
| Moq `Setup`/`Verify`/`It.IsAny` naming the method or its parameter type | **0** |
| Reflection-based invocation by string name | **0** |
| **All code sites** | **8 lines across 4 files** |

Non-code artifacts mentioning `MoveEmailsAsync` (88 files: `.md` specs, plans, audits, research; `.trx` test result files; Cobertura `.xml` coverage snapshots) are historical evidence and are excluded from every total above.

---

## 4. E2 — implementers of `IQfcCollectionController`

**Exactly one, excluding Moq-generated mocks.**

| # | Site | Text |
|---|---|---|
| 1 | `QuickFiler/Controllers/QfcCollectionController.cs:22` | `public class QfcCollectionController : IQfcCollectionController` (carries `[ExcludeFromCodeCoverage]` at `:21`) |

Two near-misses that are **not** implementers:

- `QuickFiler/Notes/notes_interfaces.cs:62` declares a **second, distinct** `public interface IQfcCollectionController` (with a `bool MoveEmails(ref cStackObject MovedMails);` member at `:31` on a neighbouring interface). This file is **not compiled**: `QuickFiler/QuickFiler.csproj` contains no `Notes` entry at all, while the real interface is wired at `QuickFiler/QuickFiler.csproj:362` (`<Compile Include="Interfaces\IQfcCollectionController.cs" />`). The project is a legacy non-SDK csproj with explicit `Compile Include` items, so absence means exclusion.
- `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:353` declares `public IQfcCollectionController Parent { get; private set; }`. That is a hand-written `IQfcItemController` double exposing the parent, not an implementation of the collection interface.

Also **not compiled**, and therefore not implementers or call sites: `QuickFiler/Legacy/QfcGroupOperationsLegacy.cs` and `QuickFiler/Legacy/QuickFileController.cs`. `QuickFiler/QuickFiler.csproj` contains no `Legacy`, `QuickFileController`, or `QfcGroupOperations` entry.

---

## 5. E3 — sweep for other stale prose describing the pre-fix behaviour

**Method deviation and why.** The delegation prompt asked me to start from the changed-file sets of commits `d512fcfe`, `137ee307` and `613e88c3`. **This is unverified: the Bash tool is disabled in this session, so no `git show --stat` could be run and I could not read those commits' file lists.** I substituted a content-driven sweep that is independent of commit boundaries: repository-wide searches (all file types, `docs/` and feature folders included) for `one element longer`, `trailing element`, `trailing null`, `Count + 1`, `EmailsLoaded + 1`, `ConcurrentDictionary`, `ElementAt`, and the guard-placement phrasing, followed by a targeted read of every consumer of the changed APIs (`GetMoveDiagnostics`, `TryGetItemGroupByIndex`, `_itemGroupsToMove`, `MoveEmailsAsync`). This is a superset of the consumers of the three commits' changed APIs, but I cannot certify it covers every file those commits touched.

### 5.1 Stale statements in compiled source — 2 found

| # | Site | Stale statement | Why false now |
|---|---|---|---|
| 1 | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:171-173` | "GetMoveDiagnostics returns an array one element longer than it fills, so its trailing element is null" | `QfcCollectionController.cs:2366` allocates exactly `Count`; every index is assigned (§C2) |
| 2 | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:397-400` | same sentence, as the doc comment of `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` | same |

**No third stale statement exists in compiled source.** Specifically, these four look similar but are correctly framed as history and are **not** stale:

- `QuickFiler/Controllers/QfcCollectionController.cs:2362-2365` — "The array **was** allocated as Count + 1..." (past tense).
- `QuickFiler/Controllers/QfcCollectionController.cs:2372-2376` — "It **previously sat** below this ItemHelper read..." (past tense).
- `QuickFiler/Controllers/QfcCollectionController.cs:71-75` — the `ConcurrentDictionary` sentence is the stated rationale for the ordered field, not a description of the current field.
- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:36-42`, `:73-80`, `:186-190`, `:282-287`, `:360-366` — all explicitly prefixed "Before the fix..." or "This test has no deterministic pre-fix red state...".

### 5.2 Pre-fix code shape surviving in non-compiled files — 1 found

`QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:1272` still contains `string[] strOutput = new string[EmailsLoaded + 1];` with a `for (k = 1; k <= loopTo; k++)` loop at `:1274` — the same off-by-one shape, with index `0` also left unassigned. This is a **different class in a file that is not in the csproj** (§4) and is unreachable from the shipped assembly. It should not be "fixed" under issue #469; if it is a concern, it belongs to whatever issue owns legacy-file deletion.

### 5.3 Documents describing the pre-fix state

These are historical or issue-mirroring artifacts. None is a live instruction to a future reader that the defect is still open, with the exception noted for the first two rows.

| Category | Sites |
|---|---|
| **Issue text mirrored into this feature folder** (present tense, describes pre-fix source with stale line numbers) | `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/issue.md:16-19`, `:30-61`, `:73-79`; `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/spec.md:12`, `:46-48` |
| Origin promoted document (superseded) | `docs/features/potential/promoted/2026-08-07-qfc-collection-move-diagnostics-defects.md:15`, `:39-41` |
| Feature #468 spec / research / plan / evidence (historical record of the work that fixed these) | `docs/features/active/qfc-collection-controller-defects-468/spec.md:93`, `:226`, `:848`; `.../research/test-harness-feasibility.md:388`, `:393`, `:767`; `.../research/qfc-collection-controller-defects.md:99-101`; `.../evidence/regression-testing/p6-t1-fail-before.2026-08-26T10-17.md:65`, `:70`; `.../evidence/qa-gates/p6-t7-commit.2026-08-26T10-29.md:55` |
| **Cross-feature note in an unrelated active feature that is now resolved** | `docs/features/active/quickfiler-home-controller-metrics-442/spec.md:869-872` (CFN-2), `.../research/quickfiler-home-controller-metrics.research.2026-08-24T10-00.md:257`, `:935`, `.../plan.2026-08-24T09-40.md:556`, `.../evidence/issue-updates/cross-feature-notes-handoff.2026-08-26T11-32.md:43-46` |

The two rows in bold are the ones a reader could act on incorrectly. The `#442` cross-feature note CFN-2 says the trailing-null "becomes a blank CSV line the moment #442 lands"; that hazard no longer exists.

---

## 6. E4 — existing regression tests for issue #469 and the effect of removing the parameter

All #469 regression tests live in one file: `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` (498 lines). Shared helpers are in `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs`.

| Test method | Line | Defect covered (issue numbering) | Breaks if `stackMovedItems` is removed? | Required change |
|---|---|---|---|---|
| `ItemGroupsToMoveFieldDeclaresAnOrderedContract` | `:45` | 3 (structural) | No | None. Optionally update the `because` string at `:57-59`, which names `MoveEmailsAsync` in prose only. |
| `TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation` | `:83` | 3 (behavioural) | No | None |
| `MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException` | `:144` | #473 defect 2 (not #469), but exercises `MoveEmailsAsync` | **Yes — compile error** | Change `:165` from `controller.MoveEmailsAsync(null)` to `controller.MoveEmailsAsync()` |
| `MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime` | `:195` | #473 defect 2 | **Yes — compile error** | Change `:216` the same way |
| `MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow` | `:251` | #473 defect 2 | **Yes — compile error** | Change `:263` the same way |
| `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine` | `:290` | 2 (array length) | No | None |
| `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls` | `:327` | 2 (array length) | No | None |
| `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing` | `:369` | 1 (null guard) | No | None |
| `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` | `:471` | 4 | **Yes — the test's entire premise disappears** | The two argument shapes it distinguishes (`:484` null vs `:485` supplied) cease to exist. **Retire the method**, and with it the `NoStackEffect` constant at `:493-495` and the `SloStack<IMovedMailInfo>` construction at `:481` and the `using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;` at `:12` if nothing else in the file uses it. Retiring it removes the only test asserting the retained parameter is inert, which is exactly what removal makes unnecessary. |

Related tests outside the #469 set that touch the same surface and would **not** break: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:83` and `:111` (issue #97 `GetMoveDiagnostics` null-appointment guards), `QfcCollectionControllerTests.cs:66-71` (injects `_itemGroupsToMove` as an ordered list), and the eight `QfcHomeControllerMetricsTests.cs` tests listed in §C6.

---

## 7. E5 — file size and the 500-line limit

| File | Lines | Status |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | **2,437** (last content line `:2437`) | **Exceeds** the 500-line limit by roughly 4.9x |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 232 (measurement is from a read of `:182-189` plus surrounding structure; treat as approximate — **unverified**, no shell available for `wc -l`) | Under the limit |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 498 (last content line `:497`) | Under the limit, **1 line of headroom** |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | 454 (last content line `:453`) | Under the limit |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | 500 (per `docs/features/active/qfc-collection-controller-defects-468/policy-audit.2026-08-26T17-15.md:88`, independently re-measured at that review) | **Exactly at** the limit — cannot receive new test methods |

**The repository's applicable rule** is `.claude/rules/general-code-change.md`: "No production code, test code, or reusable script file may exceed **500 lines**", with exceptions only for throwaway agent-session scripts, raw text fixtures, and Markdown.

**Documented exception status for `QfcCollectionController.cs`:** there is **no** exemption to the rule. There is an adjudicated, tracked, non-blocking finding:

- `docs/features/active/qfc-collection-controller-defects-468/policy-audit.2026-08-26T17-15.md:144` — "PA-2 | `QfcCollectionController.cs` exceeds the 500-line cap (2,437 lines; pre-existing at 2,349, +88 by this feature under an AC-25 no-split constraint); tracked by #623 | Major (pre-existing) | **NON-BLOCKING**".
- `docs/features/active/qfc-collection-controller-defects-468/code-review.2026-08-26T17-15.md:16` — CR-2 records the same, noting "the split remedy is prohibited by AC-25 and assigned to #623".
- `docs/features/active/qfc-collection-controller-defects-468/spec.md:1059-1063` states the excess is "**a pre-existing condition**" and not created by that feature.

**Constraint this imposes on any #469 fix:** the file is already over the cap and has an active no-split constraint delegating decomposition to issue #623. Any change on this branch should be **net-negative or net-neutral in lines** for `QfcCollectionController.cs`. Removing the parameter (issue #629's approach) is net-negative there. Adding new prose or new guards is not advisable without a corresponding deletion.

---

## 8. Recommendation on residual scope

**Issue #469 has no genuine residual defect that justifies a fix branch.**

Rationale, in order of weight:

1. **Three of four defects are demonstrably fixed on `main`** (C1, C2, C3), each with at least one deterministic regression test in `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`.
2. **The fourth defect's only remaining action — parameter removal — is already an open, separately tracked issue (#629)**, promoted 2026-08-26 with a written approach, a complete site enumeration, and a stated reason for the deferral. Re-opening it under #469 would duplicate #629 and would touch `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, the exact file the #468 scope lock deliberately protected.
3. **The C5 triage that the issue itself demanded is now complete and does not raise severity.** `issue.md:96-98` says "Defect 4 needs triage before a final severity can be assigned: if the undo record is genuinely dropped, undo-after-move is broken, which would be High." The verified chain (§C5.b) shows the record is not dropped in the shipped configuration. The `Initialized<T>` non-memoization finding (§C5.c) is a real but separate defect in `TaskMaster/AppGlobals/AppAutoFileObjects.cs`, in a different assembly, and is not one of #469's four defects.

**What is genuinely actionable, and it is small and documentation-only:**

| Item | Site | Size |
|---|---|---|
| A. Correct the stale comment | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:171-173` — rewrite to state the real reason the filter is retained: the call is made through `IQfcCollectionController`, which does not guarantee non-null elements. Do **not** delete the filter; that would fail `QfcHomeControllerMetricsTests.cs:403`. | 3 lines |
| B. Correct the second stale comment | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:397-400` — same correction to the test's doc comment. The test body is correct as written. | 4 lines |
| C. Fix the defect-numbering inversion | `QuickFiler/Controllers/QfcCollectionController.cs:2362` and `:2372`; `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:275`, `:306`, `:313`, `:340`, `:351`, `:387` — swap "defect 1" and "defect 2" to match `issue.md` and `docs/features/active/qfc-collection-controller-defects-468/spec.md:92-93`. | comment-only |
| D. Close the resolved cross-feature note | `docs/features/active/quickfiler-home-controller-metrics-442/spec.md:869-872` (CFN-2) — mark resolved. | docs-only |

**Suggested disposition.** Close issue #469 as fixed, with a closing comment that (i) records the C1/C2/C3 evidence cited above, (ii) states the defect-4 triage conclusion and points to #629 as the sole remaining action, and (iii) links items A–D. If the maintainer prefers to land A–D first, they are a documentation-only change: no production behaviour changes, item A and B are comment edits, and nothing added to `QfcCollectionController.cs` (§7 constraint respected — items C touch only existing comment lines). Under those conditions a fix branch is defensible, but it should be scoped explicitly as "comment and documentation accuracy", not as a defect fix, and its acceptance criteria should not restate #469's Expected Behavior item 4.

**Item to promote separately if desired:** `TaskMaster/AppGlobals/AppAutoFileObjects.cs:43-50` — `Initialized<T>` accepts the backing field by value and never assigns it, so a property whose loader returns null re-invokes the loader on every read and can hand out distinct instances. Affects `MovedMails` (`:180`), `Encoder` (`:441`), and `SubjectMap` (`:462`). Severity depends on whether any consumer captures one of these references, which `QuickFiler/Controllers/QfcFormController.cs:49` does.

---

## 9. Testing implications (no test code written)

If items A–D are taken:

- **No new tests are required.** Items A, B and C are comment-only; item D is documentation-only. The repository's Bugfix Workflow ("create a failing regression test first") applies to defects; a comment correction has no observable behaviour to regress.
- **The existing suite is the guard.** `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:403` already pins that the filter must not be deleted; `QfcCollectionControllerDefects468MoveTests.cs:290`, `:327` and `:369` already pin the three landed fixes. Re-running `QuickFiler.Test` in full is sufficient verification.
- **Do not add tests to `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`** — it is exactly at the 500-line cap. `QfcCollectionControllerDefects468MoveTests.cs` has one line of headroom. Any new test method needs a new file with a `Compile Include` entry in `QuickFiler.Test`'s csproj.
- **If issue #629 is executed instead** (parameter removal), the test work is subtractive, not additive: three one-token call edits (§6 rows 3–5) plus retirement of `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` and its `NoStackEffect` constant. That reduces `QfcCollectionControllerDefects468MoveTests.cs` well below the cap and requires no new file.

---

## 10. Items explicitly not verified

| Item | Reason |
|---|---|
| Changed-file sets of commits `d512fcfe`, `137ee307`, `613e88c3` | The Bash tool is disabled in this session; no `git show`/`git log` could be run. §5 substitutes a content-driven repository sweep, which is a superset of those commits' API consumers but cannot be certified to cover every file they touched. |
| Exact `wc -l` for `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | Same reason. All other line counts in §7 were derived from end-of-file reads or from an independently re-measured figure recorded in `policy-audit.2026-08-26T17-15.md:88`. |
| Whether `SmartSerializable.CreateEmpty` can return null under an "abort" dialog response | Not read to completion. It does not change the §C5 conclusion, which rests on `Initialized<T>` not writing back to the field — a property verified directly at `TaskMaster/AppGlobals/AppAutoFileObjects.cs:43-50`. |
| Whether the `AF.MovedMails` startup-window race (§C5.c case 3) is reachable from any production caller | No production read of `AF.MovedMails` inside the `LoadParallelAsync` window was found, but absence of a found caller is not proof of absence. Stated as low-to-moderate confidence rather than as fact. |
| GitHub state of issues #469, #623 and #629 | No network or `gh` access was used. Issue numbers and the #629 promotion are cited from the in-repo document `docs/features/potential/promoted/2026-08-26-qfc-remove-stackmoveditems-parameter.md:11-12`, which records them; their current open/closed state on GitHub is unknown. |
