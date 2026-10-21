# Research — Issue #637: `BreadcrumbBridgeRouter.SelectRow` emits a rooted path, leaving #614 D1 half-closed

- Timestamp: 2026-08-29T12-30
- Branch: `bug/breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
- Base: `origin/main` at `ecdb1c84ba8541ab67042985919cfed4df768c01`
- All paths below are relative to the isolated worktree root `<repo-root>/.claude/worktrees/<worktree-id>`.
- Every line number was read against the current tree on this branch. No citation is carried forward from any other document.

---

## 0. Executive summary of verified findings

1. The defect is real and reproducible by inspection. `SelectRow` rejects only an out-of-root rooted target; a rooted target **at or under** the bound archive root is committed verbatim (`QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs:96-106`).
2. There are **two unrelated `SelectRow` families** in this repository. Only the `BreadcrumbBridgeRouter` family (private, `BreadcrumbRow` parameter) is in scope. The `SelectRow(int index)` family on `BreadcrumbStateModel` / `BreadcrumbSelectionSession` / `FolderBreadcrumbBridgeRouter` / `BreadcrumbBridgeCoordinator` is a different surface and is not touched by this fix. A single-pass grep on the bare identifier conflates the two and yields a count roughly ten times too large.
3. **A blanket normalization in `SelectRow` would be a regression.** `SelectRow` also carries the trash pseudo-row text and ordinary *relative* suggestion targets. `TryMakeArchiveRelative` returns `false` for both, so an unconditional "commit only when `TryMakeArchiveRelative` succeeds" rewrite would reject every normal selection. The change must remain scoped inside the existing `ArchiveStemContract.IsFullOutlookPath(selection)` arm.
4. **The issue's third finding is materially inaccurate against the current tree.** `ButtonOK_Click` is `async void`, but it delegates to `ButtonOkClickAsync`, which wraps the whole chain in `try { ... } catch (System.Exception ex) { BoundaryErrorSink(ex.Message, ex); }` (`QuickFiler/Controllers/EfcFormController.cs:460-475`). An `InvalidOperationException` from `Globals.Ol.ArchiveRootPath` is therefore **logged, not unhandled**, on the OK-button path. The genuine defect on that path is different and is described in §10.
5. The keyboard entry points to the same chain (`KbdExecuteAsync(ActionOkAsync)` and the always-on `Keys.Return` action) have **no** try/catch, so they are a separate and narrower exposure than the button path.

---

## 1. `BreadcrumbBridgeRouter.SelectRow` — current body and the surviving branch

File: `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`

```csharp
 83        private void SelectRow(BreadcrumbRow row)
 84        {
 85            if (row.Kind == BreadcrumbRowKind.Banner)
 86            {
 87                return; // Banner rows are never selectable.
 88            }
 89
 90            string selection =
 91                row.Kind == BreadcrumbRowKind.TrashPseudoRow
 92                    ? BreadcrumbRowBuilder.TrashRowText
 93                    : row.FilingTarget;
 94            // #614 D2: reject only an out-of-root FULL Outlook target; a rooted target at or
 95            // under the root passes verbatim (#439) and no bound root leaves the row unguarded.
 96            if (
 97                _boundRoot.Length != 0
 98                && ArchiveStemContract.IsFullOutlookPath(selection)
 99                && !ArchiveStemContract.TryMakeArchiveRelative(selection, _boundRoot, out _)
100            )
101            {
102                log.Error("Breadcrumb row rejected: target is outside the archive root.");
103                return;
104            }
105
106            CommitSelection(row, selection);
107        }
```

`CommitSelection` is the sole non-clearing write site:

```csharp
131        private void CommitSelection(BreadcrumbRow row, string selection)
132        {
133            _selectedRowId = row.RowId;
134            SelectedFolderPath = selection;
135            PostOutbound(
136                new BreadcrumbRenderMessage(_renderer.RenderRows(_rows, _selectedRowId), null)
137            );
138            SelectedFolderPathChanged?.Invoke(this, SelectedFolderPath);
139        }
```

### How a rooted at-or-under-root value survives

The guard at lines 96-100 is a three-term conjunction whose **third term is negated**. `TryMakeArchiveRelative` returns `true` for a path at or under the root and discards the stem into `out _` (line 99). Therefore:

| `selection` shape | `_boundRoot.Length != 0` | `IsFullOutlookPath` | `TryMakeArchiveRelative` | `!Try...` | Guard fires? | Committed value |
|---|---|---|---|---|---|---|
| Rooted, strictly under root | true | true | **true** | false | **no** | rooted value, verbatim |
| Rooted, exactly equal to root | true | true | **true** (stem empty) | false | **no** | rooted root, verbatim |
| Rooted, out of root / cross-store | true | true | false | true | yes | nothing (rejected) |
| Relative stem (`Clients\North`) | true | **false** | n/a (short-circuited) | n/a | no | relative value, verbatim (correct) |
| `Trash to Delete` pseudo-row | true | **false** | n/a | n/a | no | `Trash to Delete` (correct) |
| Any value, no bound root | **false** | n/a | n/a | n/a | no | verbatim (deliberate #439 pass-through mode) |

The branch that must change is the **fall-through at line 106 for rows 1 and 2 of that table**: rows where `IsFullOutlookPath(selection)` is true and `TryMakeArchiveRelative` succeeds. The out-parameter is currently discarded (`out _`, line 99); binding it and committing the stem is the mechanically minimal change, plus a deterministic non-selection when that stem is empty.

**Critical constraint for the planner:** the guard's `IsFullOutlookPath` short-circuit is load-bearing. `TryMakeArchiveRelative("Clients\\North", @"\\mailbox@example.com\Archive", out _)` returns **false** (the prefix test at `ArchiveStemContract.cs:129-135` fails), and `TryMakeArchiveRelative("Trash to Delete", root, out _)` also returns **false**. A rewrite of the form "commit only when `TryMakeArchiveRelative` succeeds with a non-empty stem" applied to the *whole* method would therefore reject every ordinary suggestion row and the trash pseudo-row. The new behavior must be nested inside the `IsFullOutlookPath` arm.

---

## 2. `SelectHierarchyPath` — the concrete target semantics

File: `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`

```csharp
109        private void SelectHierarchyPath(BreadcrumbRow row, string fullPath)
110        {
111            if (_boundRoot.Length == 0)
112            {
113                CommitSelection(row, fullPath); // Preserved no-archive-root binding mode.
114                return;
115            }
116
117            // #614 D1/D9: a path outside the archive root, and the root itself, are deterministic
118            // non-selections; the prior selection stays unchanged and is never nulled (#499).
119            if (
120                !ArchiveStemContract.TryMakeArchiveRelative(fullPath, _boundRoot, out string stem)
121                || stem.Length == 0
122            )
123            {
124                log.Error("Breadcrumb selection rejected: not a folder inside the archive root.");
125                return;
126            }
127
128            CommitSelection(row, stem);
129        }
```

Stated concretely rather than by reference, the four behaviors the planner should mirror in `SelectRow`'s rooted arm are:

- **B1 (no bound root):** `_boundRoot.Length == 0` short-circuits to a verbatim commit (line 111-115). `SelectRow` already has the equivalent short-circuit as the first conjunct at line 97.
- **B2 (empty stem is a non-selection):** the disjunct `|| stem.Length == 0` at line 121 is what converts the archive-root-exact case — for which `TryMakeArchiveRelative` returns `true` — into a rejection. This is the specific clause `SelectRow` lacks.
- **B3 (rejection is an early `return`, not a null-out):** the method returns without touching `SelectedFolderPath`, so a prior valid selection survives. This is the #499 interaction; `CommitSelection` is never reached, so no `SelectedFolderPathChanged` event is raised.
- **B4 (rejection is diagnosed value-free):** `log.Error` at line 124 with a fixed message that embeds neither the path nor the root. The message text is `"Breadcrumb selection rejected: not a folder inside the archive root."`. `SelectRow`'s existing rejection message at line 102 is `"Breadcrumb row rejected: target is outside the archive root."` — also value-free. Any new rejection message must stay value-free; `BreadcrumbBridgeRouterIssue614Tests.AssertRejectionDiagnosticWithoutIdentifiers` (`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs:310-326`) asserts that no message containing the fragment contains `@`.
- **B5 (commit the stem):** `CommitSelection(row, stem)` at line 128 — the stem, not the input.

---

## 3. `TryMakeArchiveRelative` — signature, out-parameter semantics, truth table

File: `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs`

Signature (`:106-110`):

```csharp
public static bool TryMakeArchiveRelative(
    string fullPath,
    string archiveRoot,
    out string stem
)
```

Body (`:112-145`), abbreviated with line numbers:

- `:112` `stem = string.Empty;` — the out-parameter is **unconditionally initialized to empty first**, so `stem` is never the input on any exit path.
- `:113-116` returns `false` when `fullPath` is null/empty or `archiveRoot` is null/empty/whitespace.
- `:118` `root = archiveRoot.TrimEnd('\\', '/')`; `:119-122` returns `false` when the trimmed root is empty (separator-only root).
- `:124-127` `if (string.Equals(fullPath, root, OrdinalIgnoreCase)) return true;` — **returns `true` with `stem` still `string.Empty`**.
- `:129-135` returns `false` when `fullPath.Length <= root.Length` or the ordinal-case-insensitive `StartsWith` fails.
- `:137-141` returns `false` when the character at `root.Length` is not a separator (the `Archive2` near-miss guard).
- `:143-144` `stem = fullPath.Substring(root.Length).TrimStart('\\', '/'); return true;`

### Full truth table

| # | Case | Example (`fullPath`, `archiveRoot`) | Returns | `stem` | Evidence |
|---|---|---|---|---|---|
| a | Rooted strictly under root | `\Archive\Clients\North`, `\Archive` | `true` | `Clients\North` | `:143-144` |
| a' | Rooted under root, differing case | `\aRcHiVe\Clients\North`, `\Archive` | `true` | `Clients\North` | `:131` `OrdinalIgnoreCase` |
| a'' | Root has trailing separator | `\Archive\Clients`, `\Archive\` | `true` | `Clients` | `:118` `TrimEnd` |
| a''' | Forward-slash boundary | `\Archive/Clients`, `\Archive` | `true` | `Clients` | `:138` accepts `/` |
| b | Rooted exactly equal to root | `\Archive`, `\Archive` | **`true`** | **`string.Empty`** | `:124-127` |
| c | Rooted outside root / different store | `\\other@example.org\Archive\Clients`, `\\mailbox@example.com\Archive` | `false` | `string.Empty` | `:131` |
| c' | Separator-boundary near miss | `\Archive2\Clients`, `\Archive` | `false` | `string.Empty` | `:137-141` |
| d | Already-relative value | `Clients\North`, `\\mailbox@example.com\Archive` | **`false`** | `string.Empty` | `:130-131` (`StartsWith` fails) |
| e | Null or empty `fullPath` | `null` or `""`, any root | `false` | `string.Empty` | `:113-116` |
| f | Empty / whitespace / separator-only `archiveRoot` | any, `""` / `"   "` / `"\"` | `false` | `string.Empty` | `:113-116`, `:118-122` |

Case (b) returning `true` is the exact mechanism by which `SelectRow`'s negated third conjunct lets the archive-root-exact value through. Case (d) returning `false` is the exact reason a blanket rewrite is unsafe (§1).

Behaviors (a), (a'), (a''), (a'''), (b), (c), (c'), (e), (f) are each pinned by a named test in `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs` (test methods at `:164, :179, :194, :209, :224, :239, :254, :269, :284, :299, :315`). Case (d) is not separately named there but is the same code path as (c).

---

## 4. `ArchiveStemContract` — complete public surface

File: `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs`. The type is `public static class ArchiveStemContract` (`:18`) in namespace `UtilitiesCS.OutlookObjects.Folder`, carrying `#nullable enable` at `:1`. It has exactly **three** public members and two private constants (`BackslashSeparator` `:20`, `ForwardSeparator` `:21`).

1. `public static bool IsFullOutlookPath(string value)` — `:41`. Returns `false` for null/empty (`:43-46`); `true` when the first character is `\` or `/` (`:48-53`); `true` when `value.Length > 1 && value[1] == ':'` (`:55`); otherwise `false`.

2. `public static void RequireArchiveRelativeStem(string value, string paramName)` — `:68`. Two validations, both throwing `ArgumentException` with `paramName` as the second constructor argument:
   - `:70-77` when `string.IsNullOrWhiteSpace(value)`, message is `paramName` concatenated with:
     `" must be a non-empty archive-relative path (relative to the Outlook archive root); it was null, empty, or whitespace."`
   - `:79-86` when `IsFullOutlookPath(value)`, message is `paramName` concatenated with:
     `" must be an archive-relative path (relative to the Outlook archive root), but a full (rooted) Outlook or filesystem path was supplied. The value is withheld from this message because it can contain a mailbox address or user-profile path."`
   - Neither message embeds `value`. Both are exercised by `ArchiveStemContractTests` at `:80, :95, :106, :117, :133, :150`.

3. `public static bool TryMakeArchiveRelative(string fullPath, string archiveRoot, out string stem)` — `:106`. See §3.

`RequireArchiveRelativeStem` is invoked at exactly three production sites:
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs:189-192` (`ResolvePaths(Folder)`)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs:210-213` (`ResolvePaths()`)
- `QuickFiler/Controllers/EfcDataModel.cs:384` (inside `ToArchiveRelativeStem`)

---

## 5. Call-site census — the selection family (EXHAUSTIVE, two independent searches)

### The two-family disambiguation

`SelectRow` is an overloaded name across two unrelated surfaces:

- **Family A (in scope):** `BreadcrumbBridgeRouter.SelectRow(BreadcrumbRow row)` and `BreadcrumbBridgeRouter.SelectHierarchyPath(BreadcrumbRow row, string fullPath)` — both `private`, both declared in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`.
- **Family B (NOT in scope):** `SelectRow(int index)` on `UtilitiesCS.OutlookObjects.Folder.BreadcrumbStateModel` (`BreadcrumbStateModel.cs:120`), `BreadcrumbSelectionSession` (`BreadcrumbSelectionSession.cs:176`), `FolderBreadcrumbBridgeRouter` (`FolderBreadcrumbBridgeRouter.cs:178`), and `QuickFiler.Viewers.BreadcrumbBridgeCoordinator` (`BreadcrumbBridgeCoordinator.cs:125`). Family B has no `SelectedFolderPath`, no `_boundRoot`, and no `ArchiveStemContract` reference; it is the ItemViewer drop-down selector surface.

Any count that mixes the families is wrong. A single-pass grep on the bare token `SelectRow` returns **106 matching lines across 34 files** repository-wide; only **6** of those lines belong to Family A.

### Search 1

Tool: `Grep`, pattern `SelectRow|SelectHierarchyPath`, glob `*.cs`, repository root, `output_mode: content`.
Result: 106 matching lines across 34 files. Family-A lines isolated from that output: 12 (6 in `BreadcrumbBridgeRouter*.cs`, plus 6 lines in test/doc text that merely *name* the methods without calling them — `EfcSelectionGuard.cs:30`, `BreadcrumbBridgeRouterQueueTests.Part2.cs:252/265/275/314`, `BreadcrumbBridgeCoordinatorTests.cs:432`).

### Search 2 (independently constructed)

Tool: `Grep`, pattern `Select(Row|HierarchyPath)\s*\(` (call/declaration syntax only, excluding prose and identifier-substring matches), path scoped to the `QuickFiler` production project.
Result: **12 matching lines across 4 files**, of which 3 lines (`BreadcrumbBridgeCoordinator.cs:125`, `:127`, `ItemViewer.FolderSearch.cs:27`) are Family B and 9 lines are Family A.

### Agreement

Both searches independently produce the **same 9 Family-A syntax lines**: 2 declarations + 7 call sites. Counts agree.

### The census

**Declarations — 2. There is no interface declaration and no overload of either method anywhere in the repository.** Both are `private` instance methods on the `sealed partial class BreadcrumbBridgeRouter`; `QuickFiler/Interfaces/` contains no member of either name.

| # | Kind | File:line | Signature |
|---|---|---|---|
| D1 | declaration | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs:83` | `private void SelectRow(BreadcrumbRow row)` |
| D2 | declaration | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs:109` | `private void SelectHierarchyPath(BreadcrumbRow row, string fullPath)` |

**Call sites — 7 (4 to `SelectRow`, 3 to `SelectHierarchyPath`). All 7 are in production; zero test call sites, because both members are private and every test drives them through `ProcessInboundAsync` or the public `SelectFirstRow`.**

| # | Target | File:line | Calling context | Behavior change from this fix? |
|---|---|---|---|---|
| C1 | `SelectRow` | `BreadcrumbBridgeRouter.cs:201` | `SelectFirstRow()` (public; called from `EfcFormController.cs:438`) | **Yes**, when row 0's `FilingTarget` is rooted at-or-under the bound root |
| C2 | `SelectRow` | `BreadcrumbBridgeRouter.cs:286` | `ProcessInboundAsync`, `BreadcrumbMessageTypes.RowSelected` arm | **Yes**, same condition |
| C3 | `SelectRow` | `BreadcrumbBridgeRouter.Arrows.cs:153` | `HandleUpArrow` (Up arrow onto a previous row) | **Yes**, same condition |
| C4 | `SelectRow` | `BreadcrumbBridgeRouter.Arrows.cs:161` | `MoveSelection(row, step)` (Down arrow) | **Yes**, same condition |
| C5 | `SelectHierarchyPath` | `BreadcrumbBridgeRouter.Selection.cs:33` | `ActivateSegment` (segmentActivate message) | No — method not modified |
| C6 | `SelectHierarchyPath` | `BreadcrumbBridgeRouter.Selection.cs:47` | `ActivateChild` (renderedChildActivate message) | No |
| C7 | `SelectHierarchyPath` | `BreadcrumbBridgeRouter.Arrows.cs:138` | `TryRightTreeTransitionAsync` (#440 Right descent) | No |

**Delegate / event wiring reaching Family A — 2 indirect entry points, neither a direct call site:**
- `BreadcrumbBridgeRouter.cs:55` `_host.MessageReceived += OnHostMessageReceived;` → `OnHostMessageReceived` (`:291`) → `ProcessInboundAsync` → C2 / C5 / C6 / C7.
- `EfcFormController.cs:438` `_router?.SelectFirstRow();` → C1.

All four `SelectRow` call sites (C1-C4) share one implementation, so the behavior change is uniform across them; there is no per-call-site divergence to reason about.

---

## 6. `MoveToFolderAsync` overload census (EXHAUSTIVE, two independent searches)

### Search 1

Tool: `Grep`, pattern `MoveToFolderAsync`, glob `*.cs`, repository root.
Result: **16 matching lines across 6 files.**

### Search 2 (independently constructed — broader stem, to catch any non-`Async` sibling or partially-renamed member the first pattern would miss)

Tool: `Grep`, pattern `MoveToFolder`, glob `*.cs`, repository root.
Result: **16 matching lines across 6 files** — byte-identical line set.

### Agreement

Counts agree at 16. The broader stem finds **no** `MoveToFolder` member without the `Async` suffix, confirming there is no non-async sibling and no partially-renamed overload.

### Declarations — 3 across 2 declaring types. There is no interface declaring any member of this family.

| # | Declaring type | File:line | Full signature |
|---|---|---|---|
| M1 | `QuickFiler.Controllers.EfcDataModel` | `QuickFiler/Controllers/EfcDataModel.cs:259-265` | `async public Task<bool> MoveToFolderAsync(string folderpath, bool saveAttachments, bool saveEmail, bool savePictures, bool moveConversation)` |
| M2 | `QuickFiler.Controllers.EfcDataModel` | `QuickFiler/Controllers/EfcDataModel.cs:336-343` | `public async Task MoveToFolderAsync(MAPIFolder folder, string olAncestor, bool saveAttachments, bool saveEmail, bool savePictures, bool moveConversation)` |
| M3 | `QuickFiler.EfcHomeController` | `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:89-95` | `internal Task<bool> MoveToFolderAsync(string selectedFolder, bool saveAttachments, bool saveEmail, bool savePictures, bool moveConversation)` |

M3 is a **separate type's same-named forwarder**, not an overload of M1/M2. It is a test seam: it forwards to `_dataModel.MoveToFolderAsync` (M1) unless the injectable `MoveToFolderAsyncAction` delegate property (`EfcHomeController.ExecuteMoves.cs:14-21`) is set. A census that counted only "overloads of `EfcDataModel.MoveToFolderAsync`" would report 2 and miss M3 entirely.

### Call sites — 5 production + 1 test = 6

| # | File:line | Resolves to | Notes |
|---|---|---|---|
| K1 | `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:78-84` | M3 | inside `ExecuteMovesCoreAsync` |
| K2 | `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:98-104` | M1 | the `MoveToFolderAsyncAction is null` branch of M3 |
| K3 | `QuickFiler/Controllers/EfcDataModel.cs:346-352` | M1 | inside M2; the only caller of `ToArchiveRelativeStem` |
| K4 | `QuickFiler/Controllers/EfcFormController.cs:537-544` | M2 | `ButtonCreateClickAsync`, after `CreateFolderAsync` returns a `MAPIFolder` |
| K5 | `QuickFiler/Controllers/EfcFormController.cs:843-852` | M2 | `CreateFolderAsync` (the keyboard 'N' path) |
| K6 | `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:87-93` | M3 | `MoveToFolderAsync_WithInjectedAction_UsesCapturedMoveOptions` |

Plus 3 non-call textual references: a delegate-property declaration (`ExecuteMoves.cs:21`), a null test on that property (`ExecuteMoves.cs:97`), a delegate invocation (`ExecuteMoves.cs:105`), two test assignments to the property (`EfcHomeControllerExecuteMovesTests.cs:69, :125`), one test method name (`:65`), and one comment (`EfcHomeControllerTests.cs:55`). 3 declarations + 6 call sites + 7 other = 16 lines. The line accounting closes.

### The `string` overload's `DestinationOlStem` assignment

`QuickFiler/Controllers/EfcDataModel.cs`:

```csharp
282            var config = new EmailFilerConfig()
283            {
284                SaveMsg = saveEmail,
285                SaveAttachments = attachments,
286                SavePictures = savePictures,
287                DestinationOlStem = folderpath,
288                Globals = Globals,
289                OlAncestor = Globals.Ol.ArchiveRootPath,
290                FsAncestorEquivalent = folderRoot,
291            };
```

Line 287 assigns `folderpath` verbatim; there is no `ToArchiveRelativeStem` call and no `ArchiveStemContract` call anywhere in M1. The same verbatim-assignment shape recurs in two sibling methods on the same type that are **not** part of the `MoveToFolderAsync` family but share the identical exposure: `OpenOlFolderAsync` (`:308`) and `OpenFsFolderAsync` (`:326`), both of which also read `Globals.Ol.ArchiveRootPath` (`:310`, `:328`).

### The `MAPIFolder` overload's normalization

```csharp
336        public async Task MoveToFolderAsync(
337            MAPIFolder folder,
338            string olAncestor,
...
345            var folderpath = ToArchiveRelativeStem(folder.FolderPath, olAncestor);
346            var result = await MoveToFolderAsync(
347                folderpath,
...
353            if (!result)
354            {
355                MessageBox.Show($"Cannot move to folderpath {folderpath}");
356            }
357        }
```

`ToArchiveRelativeStem` is declared at `:372-386` and is called from **exactly one** site (`:345`), confirming the issue's statement.

### Can the two overloads converge on one normalization path?

They are already partially converged: M2 normalizes and then delegates to M1 (`:345-352`), so M1 is the single funnel. Two facts block moving normalization *into* M1 as a straight lift:

1. **Parameter asymmetry.** M2 receives `olAncestor` as an explicit parameter (`:338`) supplied by the caller (`EfcFormController.cs:539` and `:846`, both `_globals.Ol.ArchiveRootPath`). M1 has no ancestor parameter; it reads `Globals.Ol.ArchiveRootPath` internally at `:289`. Normalizing inside M1 would require reading that property *before* deciding whether normalization applies, which widens rather than narrows the `InvalidOperationException` exposure described in §10.
2. **Input-domain asymmetry.** M2's input is always a full Outlook `MAPIFolder.FolderPath`, so `ToArchiveRelativeStem`'s unconditional throw is correct there. M1's input is a *presented selection* that is normally already relative and may legitimately be the `"Trash to Delete"` sentinel (M1 branches on it at `:272`). An unconditional `ToArchiveRelativeStem` in M1 would throw on every ordinary relative stem and on the trash sentinel. Convergence would require the same `IsFullOutlookPath`-gated shape as §1, i.e. normalize-if-rooted rather than always-normalize.

A third, non-blocking observation: M1's `folderpath` is also compared by value against the literal `"Trash to Delete"` at `:272` (`bool attachments = (folderpath != "Trash to Delete") ? saveAttachments : false;`). Any normalization inserted upstream of that comparison must leave the trash sentinel untouched or the attachment-save behavior silently flips.

---

## 7. `SelectedFolderPath` consumer census (EXHAUSTIVE, two independent searches)

### Search 1

Tool: `Grep`, pattern `SelectedFolderPath`, glob `*.cs`, repository root, `output_mode: content`.
Result: **74 matching lines across 9 files.**

### Search 2 (independently constructed — differencing two counts to separate the property from the same-prefixed event, which a single content grep visually conflates)

Tool: `Grep`, pattern `SelectedFolderPath`, `output_mode: count` → 74 lines / 9 files.
Tool: `Grep`, pattern `SelectedFolderPathChanged`, `output_mode: count` → 13 lines / 5 files.
Derived: 74 − 13 = **61 lines that reference the property without the event**, plus 2 lines (`BreadcrumbBridgeRouter.Selection.cs:138`, `BreadcrumbBridgeRouter.cs:146`) that carry both tokens on one line.

### Agreement

Both searches place the entire population in **9 files: 2 production, 7 test**, and both place **all** production occurrences in exactly **3 files**. Counts agree.

### Production surface — complete

| # | File:line | Kind | Detail |
|---|---|---|---|
| P1 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:59` | declaration | `public string? SelectedFolderPath { get; private set; }` — the setter is `private`, so no external write is possible |
| P2 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:61` | doc reference | `<see cref="SelectedFolderPath"/>` on the event |
| P3 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:143` | **read** | `if (SelectedFolderPath != null)` — the #499 change-detection guard in `BindRowsAsync` |
| P4 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:145` | **write** | `SelectedFolderPath = null;` — the #499 clear-on-rebind |
| P5 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs:134` | **write** | `SelectedFolderPath = selection;` in `CommitSelection` |
| P6 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs:138` | **read** | event payload argument |
| P7 | `QuickFiler/Controllers/EfcFormController.cs:321` | **read** | `get => _router?.SelectedFolderPath;` — the sole cross-type read |

**Total: 2 writes (P4, P5), 3 reads (P3, P6, P7), 1 declaration, 1 doc reference.** P5 is the only write reached by `SelectRow`.

### Downstream blast radius through P7

`EfcFormController.SelectedFolder` (`:316-322`) is the single external consumer. Its own readers, exhaustively:

| Reader | File:line | Observes a change when the producer emits a stem instead of a rooted path? |
|---|---|---|
| `EfcFormController.IsValidSelection` | `:1155` → `IsSelectableFolder` `:1151` → `EfcSelectionGuard.IsValidCreationSelection` | **Yes — this is the corrective effect.** `IsValidCreationSelection` returns `false` for any `IsFullOutlookPath` value (`EfcSelectionGuard.cs:76`). A rooted selection currently makes the New-Folder gesture report "Please select a valid folder" (`EfcFormController.cs:819`) even for an in-archive folder. A stem passes. |
| `EfcFormController.ActionOkAsync` | `:743` → `EfcSelectionGuard.IsValidFilingSelection` `:748` | **Yes — this is the corrective effect.** `IsValidFilingSelection` also rejects rooted values (`EfcSelectionGuard.cs:50`). Today, selecting a breadcrumb row whose target is rooted-under-root reaches OK and is refused with "Please select a valid folder." (`:751`). A stem passes the guard and files. |
| `EfcFormController.ButtonCreateClickAsync` | `:505` (guard), `:513`, `:528` | Yes — same guard, plus the value is passed to `FolderHelper.CreateFolderAsync(SelectedFolder, ArchiveRootPath, ...)` (`:527-532`), which concatenates beneath the archive root. A stem is the correct input there; a rooted value was never valid. |
| `EfcFormController.CreateFolderAsync` | `:817` (guard), `:823`, `:835` | Yes — same as above, via `FolderHelper.CreateFolder` (`:834-838`). |
| `EfcFormController.ActionOkAsync` → `_homeController.OpenOlFolderAsync(SelectedFolder)` | `:763` → `EfcHomeController.cs:427-430` → `EfcDataModel.OpenOlFolderAsync` `:299-316` | Yes. `DestinationOlStem = folderpath` verbatim at `EfcDataModel.cs:308`, then `EmailFilerConfig.ResolvePaths()` (`EmailFiler.cs`) enforces `RequireArchiveRelativeStem`. A rooted value would throw there; a stem does not. |
| `_homeController.OpenFsFolderAsync(SelectedFolder)` | `:513`, `:823` → `EfcHomeController.cs:432-435` → `EfcDataModel.OpenFsFolderAsync` `:318-334` | Yes, same shape (`DestinationOlStem` at `:326`). |
| `EfcHomeController.ExecuteMovesCoreAsync` | `EfcHomeController.ExecuteMoves.cs:69` | Yes. The value flows to M3 → M1 → `DestinationOlStem` (`EfcDataModel.cs:287`) → `ResolvePaths` → `RequireArchiveRelativeStem`. This is the #614 D1 leak the fix closes. |
| `EfcHomeController.HandleMoveResult` failure text | `EfcHomeController.ExecuteMoves.cs:134` | Cosmetic only — the message text `"Cannot move to folderpath {selectedFolder}"` would name the stem rather than the rooted path. Note this message **does** embed the value; a stem is strictly less identifying than a rooted store path, so this is a small improvement, not a regression. |
| `EfcHomeController.QuickFileMetrics_WRITE` | `EfcHomeController.Metrics.cs:56` | Yes, cosmetically: the metrics CSV column would carry the stem. Per prior research there are **zero in-repo readers** of that CSV, so no code observes it. |
| `EfcDataModel.MoveToFolderAsync` trash-sentinel test | `EfcDataModel.cs:272` | **No.** The trash pseudo-row is not `IsFullOutlookPath`, so the fix leaves `"Trash to Delete"` byte-identical. |

**Consumers that do NOT observe any change:**
- Every consumer reached when `_boundRoot` is empty (`BindRowsAsync(rows, scores, ct)`, the 3-argument public overload at `BreadcrumbBridgeRouter.cs:75-82`). The internal 4-argument overload (`:92`) is called from exactly one production site, `EfcFormController.cs:987`.
- P3/P4, the #499 clear-on-rebind pair — they write `null` and read for null-ness only.
- `EfcItemController.SelectedFolder` (`EfcItemController.cs:589-593`) — reads `_itemViewer.GetSelectedFolder()`, a different source entirely; it does not touch `BreadcrumbBridgeRouter`.
- The whole Family-B breadcrumb surface (§5) — it has no `SelectedFolderPath` member.

**Net assessment:** every behavioral consumer of a stem-valued `SelectedFolderPath` either improves (a previously-refused in-archive selection now files) or is unchanged. No consumer requires a rooted value. The one direction that could regress is the archive-root-exact case becoming a non-selection: today it produces a rooted value that `IsValidFilingSelection` rejects with a dialog; after the fix it produces no selection at all and the prior selection survives. Both outcomes refuse to file the archive root; the fix's outcome is quieter and matches `SelectHierarchyPath`.

---

## 8. The test that pins the current behavior (EXHAUSTIVE, two independent searches)

### The pinning test

- File: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
- Test method: `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch`, declared at `:118-119`
- Fixture: `archiveRoot = @"\Archive"` (`:123`), `fullTarget = @"\aRcHiVe\Clients\North"` (`:124`), bound through the internal 4-argument overload (`:146-154`), then `rowSelected` on `row-0` (`:155-158`)
- Assertions:

```csharp
161            provider.Verify(
162                p => p.ResolveLeafKeyAsync(fullTarget, It.IsAny<CancellationToken>()),
163                Times.Once
164            );
165            router.SelectedFolderPath.Should().Be(fullTarget);
```

Line **165** is the assertion that pins the defect. Line 161-164 pins a *different and still-correct* property — that the provider lookup uses the original rooted path — and must be preserved: `ToHierarchyPath` (`BreadcrumbBridgeRouter.cs:152-167`) is not changed by this fix.

**Corrected assertion.** With `archiveRoot = @"\Archive"` and `fullTarget = @"\aRcHiVe\Clients\North"`, `TryMakeArchiveRelative` takes the `StartsWith(OrdinalIgnoreCase)` path (`ArchiveStemContract.cs:131`), the boundary character at index 8 is `\` (`:137-141`), and `stem = fullTarget.Substring(8).TrimStart('\\','/')` = `Clients\North`. The corrected assertion is therefore:

```csharp
router.SelectedFolderPath.Should().Be(@"Clients\North");
```

The test **name** also asserts the old spec ("RemainsUnchanged") and should be renamed; the XML/inline comment at `:120-122` ("so the provider must receive the original full path unchanged") remains accurate for the `provider.Verify` half and should be kept, narrowed to the provider claim. This is a **deliberate spec correction**: the #439 acceptance criterion that a rooted target survives selection is superseded by #614's archive-relative-stem invariant, which #614 already enforced on the `SelectHierarchyPath` half and at the filing boundary but not on the `SelectRow` half.

### Exhaustive enumeration of every OTHER test whose assertions depend on `SelectRow` emitting a rooted value, or on the archive-root-exact case being a selection

#### Search 1 — assertion-side

Tool: `Grep`, pattern `SelectedFolderPath`, glob `*.cs`, repository root, `output_mode: content`.
Result: 74 lines across 9 files; **7 test files**: `BreadcrumbBridgeRouterTests.Selection.cs` (12), `BreadcrumbBridgeRouterTests.cs` (2), `BreadcrumbBridgeRouterQueueTests.Part2.cs` (24), `BreadcrumbBridgeRouterQueueTests.cs` (4), `BreadcrumbBridgeRouterIssue614Tests.cs` (11), `BreadcrumbBridgeRouterIssue439Tests.cs` (12), and the production files. All 7 test files are in `QuickFiler.Test`; **no test in `UtilitiesCS.Test` or any other test project references the property.**

#### Search 2 — trigger-side, independently constructed

Rather than looking at assertions, this search enumerates the **complete set of test-visible entry points that reach `SelectRow`** (per §5 those are exactly: the `rowSelected` inbound message, the public `SelectFirstRow`, and the `Up`/`Down` arrow keys).

Tool: `Grep`, pattern `rowSelected|SelectFirstRow|\\"key\\":\\"(Up|Down)\\"`, glob `*.cs`, repository root.
Result: **32 matching lines across 10 files.** Removing 4 production lines (`EfcFormController.cs:438`, `BreadcrumbBridgeRouter.cs:196`, `BreadcrumbMessages.cs:28`, `BreadcrumbDocumentAssets.cs:98`) and 4 `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs` lines (`:57, :72, :205, :215` — codec parse tests that construct no router) leaves **24 test lines in 6 files**, all in `QuickFiler.Test`.

#### Agreement

Both searches converge on the same 6 `QuickFiler.Test` files. Search 2 additionally proves that no test outside `QuickFiler.Test` can reach `SelectRow` at all.

#### Classification of all 24 SelectRow-reaching test invocations

The fix changes behavior only when **`_boundRoot` is non-empty AND the selected row's `FilingTarget` is `IsFullOutlookPath` AND that target is at or under the bound root**. Classifying every invocation against those three conditions:

| Test file:line | Bound root | Selected row's `FilingTarget` | Affected? |
|---|---|---|---|
| `EfcHomeControllerExecuteMovesTests.cs:274` (`SelectFirstRow`) | **empty** (3-arg bind at `:267-271`) | caller-supplied | No — no bound root |
| `BreadcrumbBridgeRouterTests.Selection.cs:32, 48, 64, 78, 137, 150, 255` | **empty** (`Bind()` at `BreadcrumbBridgeRouterTests.cs:113-118`; `BindThreeRows()` at `.Selection.cs:120-127`; local bind at `:245-252`) | `Inbox\Projects\Alpha`, `Inbox\Beta`, banner, `Trash to Delete` | No — no bound root, and none is rooted |
| `BreadcrumbBridgeRouterQueueTests.cs:111, 112, 189, 317, 448` | **empty** (`Bind()` at `:86-96`) | `LeafPath` (relative) | No |
| `BreadcrumbBridgeRouterQueueTests.Part2.cs:208, 233, 260` | **empty** (`Bind()`) | `LeafPath` (relative) | No |
| `BreadcrumbBridgeRouterIssue614Tests.cs:65, 89, 105, 157, 180` (via the `RowSelected()` helper at `:288-291`) | `\\mailbox@example.com\Archive` | `Clients\North` (relative) at `:65,:89,:105,:157`; `\\other@example.org\Archive\Clients` (rooted, **out of root**) at `:180` | No — relative values are untouched; the out-of-root value is still rejected, so `RowSelected_OutOfRootFilingTarget_DoesNotStoreAFullOutlookPath` (`:169`) keeps passing unchanged |
| `BreadcrumbBridgeRouterIssue439Tests.cs:86` | `\Archive` | `Clients\North` (relative) | No |
| `BreadcrumbBridgeRouterIssue439Tests.cs:156` | `\Archive` | `\aRcHiVe\Clients\North` (**rooted, under root**) | **YES** — the pinning test |
| `BreadcrumbBridgeRouterIssue439Tests.cs:233` | `\Archive` | `Clients\Canceled` (relative) | No |
| `BreadcrumbBridgeRouterIssue439Tests.cs:295` | `\\mailbox@example.com\Archive` | `Clients\North` (relative) | No |

#### Result — the countable population

- **Tests whose assertions must change: exactly 1** — `BreadcrumbBridgeRouterIssue439Tests.Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch`, one assertion line (`:165`), plus a name/comment correction.
- **Tests that depend on the archive-root-exact case being a `SelectRow` selection: exactly 0.** No test binds a presented row whose `FilingTarget` equals the bound archive root. The two tests that assert a root-valued `SelectedFolderPath` — `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection` (`:619`, assertion `:665` `Be(@"\Archive")`) and `Issue614Tests.SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode` (`:188`, assertion `:221` `Be(@"\Archive")`) — both go through **`SelectHierarchyPath` under an empty `_boundRoot`**, not through `SelectRow`. `Issue439SlashOnlyArchiveRoot...` binds with `@"\"` (`:645`), which `BindRowsAsync` trims to the empty string at `BreadcrumbBridgeRouter.cs:107-109`, so `_boundRoot.Length == 0`. Neither is affected.
- **Tests asserting no-bound-root pass-through: 2** (the two named immediately above). Both must be preserved unchanged, since the fix does not touch the `_boundRoot.Length == 0` short-circuit.

---

## 9. The composition test from the #614 remediation

- File: `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`
- Test method: `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary`, declared at `:167-168`
- Shape: a 10-element candidate matrix (`:172-184`) is filtered through `EfcSelectionGuard.IsValidFilingSelection` (`:189`); every value that passes is placed into an `EmailFilerConfig` with `OlAncestor = @"\\mailbox@example.com\Archive"` (`:197`) and `DestinationOlStem = candidate` (`:198`), and `config.ResolvePaths()` is asserted not to throw (`:204-208`). A final assertion (`:211-213`) requires at least one candidate to have been evaluated.

**Confirmation the proposed fix keeps it passing.** The test exercises no router at all — it composes `EfcSelectionGuard` directly against `EmailFilerConfig.ResolvePaths`. The fix modifies neither `EfcSelectionGuard` nor `EmailFilerConfig`, so the test is structurally untouched. Semantically it also stays green and becomes *more* meaningful: the fix guarantees the producer now emits only values in the accepted class (relative stems, `Trash to Delete`) or nothing at all, which is precisely the precondition this composition test asserts about the guard's accepted set.

Two adjacent tests in the same file explicitly record the deferral this issue closes and will need their rationale comments revisited (they remain factually correct as *guard-surface* claims, since the guard still rejects rooted values — the producer simply stops producing them):

- `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsRejected` (`:143-154`), whose comment at `:146` reads "normalization is deferred to issue #637" and whose `because` string at `:152` repeats it.
- The same deferral is recorded in production at `QuickFiler/Controllers/EfcSelectionGuard.cs:30`: `"Producer-side normalization in BreadcrumbBridgeRouter.SelectRow is deferred to issue #637."` This comment becomes stale on merge and should be updated.

---

## 10. The OK chain and the `ArchiveRootPath` throw

### The hops, with file and line

| Hop | File:line | Code |
|---|---|---|
| H1 | `QuickFiler/Controllers/EfcFormController.cs:418` | `_formViewer.Ok.Click += ButtonOK_Click;` |
| H2 | `QuickFiler/Controllers/EfcFormController.cs:460` | `public async void ButtonOK_Click(object sender, EventArgs e) => await ButtonOkClickAsync();` |
| H3 | `QuickFiler/Controllers/EfcFormController.cs:462-475` | `ButtonOkClickAsync` — `try { ... await ActionOkAsync(); } catch (System.Exception ex) { BoundaryErrorSink(ex.Message, ex); }` |
| H4 | `QuickFiler/Controllers/EfcFormController.cs:738-772` | `ActionOkAsync` — guard at `:745-753`, `_formViewer.Hide()` at `:756`, `await _homeController.ExecuteMovesAsync()` at `:759`, `_formViewer.Dispose(); Cleanup();` at `:769-770` |
| H5 | `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:32-47` | `ExecuteMovesAsync` |
| H6 | `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:67-87` | `ExecuteMovesCoreAsync` → `MoveToFolderAsync` at `:78` |
| H7 | `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:89-112` | M3 forwarder → `_dataModel.MoveToFolderAsync(...)` at `:98` |
| H8 | `QuickFiler/Controllers/EfcDataModel.cs:259-297` | M1; `OlAncestor = Globals.Ol.ArchiveRootPath` at `:289` |
| H9 | `TaskMaster/AppGlobals/AppOlObjects.cs:253-267` | `ArchiveRootPath` getter |
| H10 | `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:32-60` | `RequireResolvedArchiveRoot` |

### (a) Is `ButtonOK_Click` `async void`, and does it rethrow?

**`async void`: confirmed. Rethrows: NOT confirmed — the opposite is true on the current tree.**

```csharp
460        public async void ButtonOK_Click(object sender, EventArgs e) => await ButtonOkClickAsync();
461
462        internal async Task ButtonOkClickAsync()
463        {
464            try
465            {
466                if (SynchronizationContext.Current is null)
467                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);
468
469                await ActionOkAsync();
470            }
471            catch (System.Exception ex)
472            {
473                BoundaryErrorSink(ex.Message, ex);
474            }
475        }
```

`BoundaryErrorSink` is an injectable seam defaulting to the log4net logger (`EfcFormController.cs:127-129`). It swallows the exception. Therefore an `InvalidOperationException` from `ArchiveRootPath` reached through the **OK button** is logged and does not crash the message pump. The issue's claim that it "becomes an unhandled UI-thread exception" is incorrect for that entry point.

**The real OK-button defect is a silent half-completed teardown.** `ActionOkAsync` calls `_formViewer.Hide()` at `:756` *before* `await _homeController.ExecuteMovesAsync()` at `:759`, and `_formViewer.Dispose(); Cleanup();` at `:769-770` *after*. When the await throws, lines 769-770 never run: the EFC form is hidden, undisposed, and uncleaned, and the user sees no message — only a log entry. The item is not filed and nothing says so.

**Two entry points do lack the catch and remain genuinely unhandled:**
- `EfcFormController.cs:392` — `new KaKeyAsync("Collection", Keys.Return, (k) => ActionOkAsync())` in `RegisterAlwaysOnAsyncKeyActions` (`:383-395`). This registers `ActionOkAsync` directly, bypassing `ButtonOkClickAsync`.
- `EfcFormController.cs:623` and `:683` — `KbdExecuteAsync(ActionOkAsync)` for the `'K'` character action. `KbdExecuteAsync` is declared at `:894-898` and `:900-904` and contains **no** try/catch.

### (b) Does `ExecuteMovesAsync` use try/finally with no catch?

**Confirmed.** `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:32-47`:

```csharp
 32        public async Task ExecuteMovesAsync()
 33        {
 34            if (!TryBeginExecuteMoves())
 35            {
 36                return;
 37            }
 38
 39            try
 40            {
 41                await ExecuteMovesCoreAsync();
 42            }
 43            finally
 44            {
 45                ResetExecuteMovesState();
 46            }
 47        }
```

The `finally` releases the `Interlocked` re-entrancy guard (`:54-65`), so a throw does **not** wedge the guard. It does not observe or translate the exception.

### (c) Does `ArchiveRootPath` throw `InvalidOperationException` on unresolvable / cross-store?

**Confirmed.** `TaskMaster/AppGlobals/AppOlObjects.cs:253-267`:

```csharp
253        public string ArchiveRootPath
254        {
255            get
256            {
257                if (_archiveRootPath is null)
258                {
259                    _archiveRootPath = ArchiveRootPathGuard.RequireResolvedArchiveRoot(
260                        Path.Combine(Root.FolderPath, "Archive"),
261                        ArchiveRoot?.FolderPath,
262                        message => logger.Error(message)
263                    );
264                }
265                return _archiveRootPath;
266            }
267        }
```

`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs`:

```csharp
 38            if (
 39                string.IsNullOrWhiteSpace(composedArchiveRootPath)
 40                || string.IsNullOrWhiteSpace(resolvedArchiveFolderPath)
 41            )
 42            {
 43                logDiagnostic?.Invoke(UnresolvableRule);
 44                throw new InvalidOperationException(UnresolvableRule);
 45            }
 46
 47            if (
 48                !string.Equals(
 49                    composedArchiveRootPath,
 50                    resolvedArchiveFolderPath,
 51                    StringComparison.OrdinalIgnoreCase
 52                )
 53            )
 54            {
 55                logDiagnostic?.Invoke(CrossStoreRule);
 56                throw new InvalidOperationException(CrossStoreRule);
 57            }
```

Message constants at `:13-17`. The value is cached only on success (`AppOlObjects.cs:259`, assignment happens after the call returns), so the throw **recurs on every read** rather than being cached.

Note the throw is not confined to `MoveToFolderAsync`: within the EFC chain, `EfcFormController.cs:987` reads `ArchiveRootPath` for `BindRowsAsync` (inside a `try/catch` at `:989-995`), and `:529`, `:539`, `:836`, `:846` read it inside the create paths.

### The narrowest seam for a benign degrade

The narrowest production seam is **`EfcHomeController.ExecuteMovesAsync` (`ExecuteMoves.cs:32-47`)**, because:
- It is the single funnel for the OK-filing path, reached from all three OK entry points (button, `Keys.Return`, `'K'`).
- It already owns a `try` block, so adding a `catch (InvalidOperationException)` is a one-clause change with no restructuring and no new file.
- It sits *below* `ActionOkAsync`'s `Hide`/`Dispose`/`Cleanup` sequence, so catching there lets `ActionOkAsync` continue to line 769-770 and complete the teardown deterministically instead of leaving the form hidden-and-undisposed.
- It is already unit-testable headlessly: `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs` constructs `EfcHomeController` and drives `ExecuteMovesAsync`/`MoveToFolderAsync` through the injected `MoveToFolderAsyncAction` seam without Outlook.

A narrower alternative — catching inside `EfcDataModel.MoveToFolderAsync` (M1) at the `ArchiveRootPath` read — is worse: `EfcDataModel.cs` carries no `#nullable enable`, has no injectable notification seam, and M1's `return false` contract would silently route to the "Cannot move to folderpath X" message, conflating an archive-root configuration failure with an ordinary move failure.

### Existing UI-notification pattern for aborting a filing operation

The repository already has a first-class, injectable pattern for exactly this. Cited example, `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs`:

```csharp
 23        internal Action<string> MoveFailureMessageAction { get; set; } =
 24            text => MessageBox.Show(text);
```

used at `:132-136`:

```csharp
132            if (!result)
133            {
134                MoveFailureMessageAction($"Cannot move to folderpath {selectedFolder}");
135                return;
136            }
```

This is the pattern to reuse: an `internal Action<...>` property on the controller with a `MessageBox.Show` default, so production shows a dialog and tests assert on a captured string without a UI. A second instance of the same pattern with a richer signature exists on the same type at `QuickFiler/Controllers/EfcHomeController.cs:299-305`:

```csharp
299        internal Action<
300            string,
301            string,
302            MessageBoxButtons,
303            MessageBoxIcon
304        > MessageBoxShowAction { get; set; } =
305            (text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon);
```

A third instance of the same idiom exists in `QuickFiler/Controllers/QfcItemController.MailActions.cs:31`. The planner should not invent a new notification shape; one of these two `EfcHomeController` seams is the right vehicle.

**Redaction constraint:** any new user-facing message must not embed the archive root path. `ArchiveRootPathGuard.UnresolvableRule` and `CrossStoreRule` (`ArchiveRootPathGuard.cs:13-17`) are already redacted, value-free strings and are the appropriate text to surface.

---

## 11. Test project and framework facts

### Ownership

| Affected production file | Owning test project | Test files |
|---|---|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | **`QuickFiler.Test`** | `Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`, `Controllers/BreadcrumbBridgeRouterIssue614Tests.cs`, `Controllers/BreadcrumbBridgeRouterTests.cs` + `.Selection.cs`, `Controllers/BreadcrumbBridgeRouterQueueTests.cs` + `.Part2.cs` |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` (comment only) | `QuickFiler.Test` | `Controllers/EfcSelectionGuardTests.cs` |
| `QuickFiler/Controllers/EfcDataModel.cs` (if §6 convergence is in scope) | `QuickFiler.Test` | no dedicated `EfcDataModelTests.cs` exists — this is a coverage gap, not an existing suite |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` (if §10 degrade is in scope) | `QuickFiler.Test` | `Controllers/EfcHomeControllerExecuteMovesTests.cs`, `Controllers/EfcHomeControllerTests.cs` |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` (no change expected) | `UtilitiesCS.Test` | `OutlookObjects/Folder/ArchiveStemContractTests.cs` |

`UtilitiesCS.Test` owns **no** test that reaches `BreadcrumbBridgeRouter` (§8, Search 2). No other test project is involved.

### Framework confirmation for the specific files that will change

`QuickFiler.Test/packages.config`: `FluentAssertions` 8.10.0 (`:8`), `Moq` 4.20.72 (`:112`), `MSTest.Analyzers` (`:114`), `MSTest.TestAdapter` 4.3.3 (`:119`), `MSTest.TestFramework` 4.3.3 (`:120`), all `targetFramework="net481"`.
`UtilitiesCS.Test/packages.config`: identical versions at `:9`, `:139`, `:141`, `:146`, `:147`.

In `BreadcrumbBridgeRouterIssue439Tests.cs` specifically (the file that changes), all three are in use at `:4` (`using FluentAssertions;`), `:5` (`using Microsoft.VisualStudio.TestTools.UnitTesting;`), `:6` (`using Moq;`), with `[TestClass]` at `:17` and `[TestMethod]` at `:20, :118, :168, :257, :306, :353, :420, :496, :541, :618`.

### The existing archive-root binding pattern in the router tests — do not invent a new fixture

There are **two** established shapes; both are legitimate and neither requires any new infrastructure.

**Shape 1 — per-test local construction (used by `BreadcrumbBridgeRouterIssue439Tests.cs`, the file that changes).** Strict mocks, constructed inline, bound through the **internal 4-argument** `BindRowsAsync` overload whose fourth positional argument is the archive root. Concrete cited example, `BreadcrumbBridgeRouterIssue439Tests.cs:123-158`:

```csharp
123            const string archiveRoot = @"\Archive";
124            const string fullTarget = @"\aRcHiVe\Clients\North";
125            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
126            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
127            host.SetupGet(h => h.IsCoreInitialized).Returns(true);
128            host.Setup(h => h.NavigateToString(It.IsAny<string>()));
129            host.Setup(h => h.PostMessageJson(It.IsAny<string>()));
130            FolderTreeNodeKey key = Key(fullTarget);
131            provider
132                .Setup(p => p.ResolveLeafKeyAsync(fullTarget, It.IsAny<CancellationToken>()))
133                .ReturnsAsync(key);
134            provider
135                .Setup(p => p.GetAncestorChainAsync(key, It.IsAny<CancellationToken>()))
136                .ReturnsAsync(Chain(fullTarget, "Clients", "North"));
137            var router = new BreadcrumbBridgeRouter(
138                provider.Object,
139                host.Object,
140                new BreadcrumbMessageCodec(),
141                new BreadcrumbHtmlRenderer(),
142                new BreadcrumbOutboundQueue(host.Object)
143            );
144
145            // Act
146            router
147                .BindRowsAsync(
148                    new[] { fullTarget },
149                    new[] { new FolderScore(fullTarget, 730, 0.73) },
150                    archiveRoot,
151                    CancellationToken.None
152                )
153                .GetAwaiter()
154                .GetResult();
155            router
156                .ProcessInboundAsync("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}")
157                .GetAwaiter()
158                .GetResult();
```

Local helpers in that file: `Key(string)` at `:668-671`, `Chain(string, string, string)` at `:673-687`, `Segment(string, string, bool)` at `:689-692`.

**Shape 2 — `[TestInitialize]` fixture with a `BindChain` helper and a log4net `MemoryAppender` (used by `BreadcrumbBridgeRouterIssue614Tests.cs`).** Preferable when the test must assert the *rejection diagnostic* rather than only the selection value. Cited: `Setup()` at `:38-52`, `Cleanup()` at `:54-58`, `BindChain(...)` at `:236-262` (which passes `ArchiveRoot` as the fourth `BindRowsAsync` argument at `:257`), `BindStandardChain()` at `:224-234`, `Inbound(json)` at `:264-267`, JSON builders at `:269-291`, `AssertRejectionDiagnosticWithoutIdentifiers` at `:310-326`, and appender attach/detach at `:338-356`.

The `internal` 4-argument overload is visible to `QuickFiler.Test` (it is used from 14 sites there), so no `InternalsVisibleTo` change is required.

**Empty-root note the planner must respect:** to produce `_boundRoot.Length == 0` in a test, pass `string.Empty`, `null`, whitespace, **or a separator-only value** such as `@"\"` — `BindRowsAsync` at `BreadcrumbBridgeRouter.cs:107-109` applies `TrimEnd('\\','/')`, so `@"\"` becomes empty. `BreadcrumbBridgeRouterIssue439Tests.cs:645` relies on exactly this.

---

## 12. Nullable posture of each production file that may change

`/p:TreatWarningsAsErrors=true` promotes `CS86xx` only in files carrying a `#nullable enable` directive (this repository has no `Directory.Build.props` and no `<Nullable>` element in any project).

| Production file | `#nullable enable`? | Consequence for edited lines |
|---|---|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | **YES** — line 1 | `CS86xx` become build errors. The new `out string stem` local is non-nullable and definitely assigned by `TryMakeArchiveRelative` on both paths (`ArchiveStemContract.cs:112`), so no warning is expected; but `SelectedFolderPath` is `string?` (`BreadcrumbBridgeRouter.cs:59`) and `CommitSelection` takes a non-nullable `string` (`:131`) — do not introduce a nullable temporary. |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | **YES** — line 1 | same |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | **YES** — line 1 | same |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | **YES** — line 1 | comment-only change expected; no diagnostic risk |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` | **YES** — line 1 | no change expected |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` | **YES** — line 1 | no change expected |
| `QuickFiler/Controllers/EfcDataModel.cs` | **NO** | file does not participate in nullable analysis; `CS86xx` will not be promoted on edited lines |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | **NO** | as above |
| `QuickFiler/Controllers/EfcHomeController.cs` | **NO** | as above |
| `QuickFiler/Controllers/EfcFormController.cs` | **NO** | as above |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | **NO** | as above |
| `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` | **NO** | as above |

Derivation: `Grep` for `^#nullable` across `QuickFiler/` returns 26 files, all of which are breadcrumb-related plus `EfcSelectionGuard.cs`; none of `EfcDataModel.cs`, `EfcFormController.cs`, `EfcHomeController*.cs` appears. Separate targeted greps confirmed `ArchiveStemContract.cs:1` and `EmailFilerConfig.cs:1`.

**File-size headroom (500-line limit).** `BreadcrumbBridgeRouter.Selection.cs` is 209 lines; `BreadcrumbBridgeRouter.cs` is 304; `BreadcrumbBridgeRouter.Arrows.cs` is 211; `EfcHomeController.ExecuteMoves.cs` is 147; `BreadcrumbBridgeRouterIssue439Tests.cs` is 694 lines — **already over the 500-line limit**, so no material additions should be made to that test file; a corrected assertion is a substitution and does not worsen it, but any *new* regression tests belong in a new file or in `BreadcrumbBridgeRouterIssue614Tests.cs` (358 lines).

---

## 13. Open questions the planner must decide (report-only; no plan proposed here)

1. **Scope of the third finding.** §10 shows the issue's premise (unhandled UI-thread exception via `ButtonOK_Click`) is inaccurate for the button path but accurate for the two keyboard paths, and that the real button-path defect is a half-completed teardown. Whether to address that in this issue or promote it separately is a scoping decision.
2. **Whether the `string`/`MAPIFolder` convergence in §6 is in scope.** It is a genuinely separate change to a non-nullable file with no existing test class, and it is not required to close D1.
3. **Stale comment `EfcSelectionGuard.cs:30`** and the two `EfcSelectionGuardTests.cs` rationale strings (`:146`, `:152`) reference "#637 deferred" and become inaccurate on merge.
4. **`OpenOlFolderAsync` / `OpenFsFolderAsync`** (`EfcDataModel.cs:299-334`) assign `DestinationOlStem` verbatim exactly as M1 does and read `ArchiveRootPath` in the same way. They are reached from `ActionOkAsync:763` and `ButtonCreateClickAsync:513` / `CreateFolderAsync:823`. They are outside the literal wording of #637 but share its defect class.
