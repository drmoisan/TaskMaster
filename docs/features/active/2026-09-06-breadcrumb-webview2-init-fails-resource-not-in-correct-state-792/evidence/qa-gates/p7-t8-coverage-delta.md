# [P7-T8] Coverage delta (baseline `p0-baseline` versus final `p7-final`, QuickFiler.Test scope)

- Issue: #792
- Timestamp: 2026-09-17T21-15
- PASS-NUMBER: 1
- Command: CMD-BASE (binds `$BaseSha` from `p0-t7-git-base.md`); both processed documents loaded from the gitignored `coverage/p0-baseline.cobertura.xml` and `coverage/p7-final.cobertura.xml`; per-file maps by the [P0-T12] max-hits merge; `git show $BaseSha:QuickFiler/Controllers/EfcFormController.cs` and `git show $BaseSha:QuickFiler/Controllers/EfcDataModel.cs` for the gate (2) text sets; `git diff --unified=0 $BaseSha HEAD -- <file>` for the gate (3) and (5) hunks; `Get-CoberturaPackageLineSummary` for gate (6); all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory (the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`; console output encoding forced to UTF-8 before the `git show` reads)
- EXIT_CODE: 0
- Output Summary: gates (1) through (5) PASS; gate (6) `BRANCH: A`, PASS; gate (7) reported (0.242471 baseline, 0.244154 final); no `BLOCKED: coverage` condition.

BASE-SHA: e7cbb57229c63a228e7fe0bcbcdbfbc06db8bcd3 (the `git merge-base HEAD origin/main` after `git fetch origin`, per [P0-T7]; bare local `main` was not used)

## Gate (1) — `WebView2EnvironmentContract.cs` per-file line rate

- COVERED/VALID: 9/9; rate 1.0000 (>= 0.90)
- GATE-1: PASS

## Gate (2) — new executable lines of the relocated parts (>= 90 percent excluding the pre-declared misses)

New executable line = a Cobertura row of the file whose trimmed source text does not occur (trimmed) anywhere in the BASE-SHA text of the originating file.

### `Controllers/EfcFormController.Breadcrumb.cs` (against `git show $BaseSha:QuickFiler/Controllers/EfcFormController.cs`, 1321 lines read)

- ROWS-TOTAL: 97; NEW-EXECUTABLE-LINES: 23 (line numbers 71 72 76 81 86 87 88 89 90 95 96 97 98 99 106 107 108 109 116 117 122 124 128)
- PRE-DECLARED-MISSES: 1 — line 128 `label.BeginInvoke(new MethodInvoker(() => label.Text = message));` (the `BeginInvoke` branch of `ShowFolderAreaError`), hits=0
- COUNTED-NEW-LINES: 22; COUNTED-NEW-COVERED: 22; RATIO: 1.0000
- Every uncovered new line, by number and text: 128 `label.BeginInvoke(new MethodInvoker(() => label.Text = message));` (the pre-declared miss; no other)
- GATE-2-FILE: PASS

### `Controllers/EfcDataModel.Carry.cs` (against `git show $BaseSha:QuickFiler/Controllers/EfcDataModel.cs`, 499 lines read)

- ROWS-TOTAL: 56; NEW-EXECUTABLE-LINES: 15 (line numbers 31 33 37 44 46 47 51 52 59 60 70 72 77 96 97)
- PRE-DECLARED-MISSES: 2 — line 70 `scoringInput,` and line 72 `).InitAsync(scoringInput, FolderPredictor.InitOptions.FromField),` (the lines of the `FromField` scoring lambda in the null-list branch that read `scoringInput`; reachable only with a live `MailItemHelper`), both hits=0
- COUNTED-NEW-LINES: 13; COUNTED-NEW-COVERED: 13; RATIO: 1.0000
- Every uncovered new line, by number and text: 70 `scoringInput,`; 72 `).InitAsync(scoringInput, FolderPredictor.InitOptions.FromField),` (both pre-declared; no other)
- GATE-2-FILE: PASS

GATE-2: PASS

Positive controls on the classifier: the known-new line `private Task InitializeBreadcrumbHostOnceAsync()` is present once in `Breadcrumb.cs` and classified new (true); the known-moved line `private void BindFolderRows(string[] rows)` is present once and classified not-new (false). The 1321- and 499-line reads equal the [P0-T8] totals of the two originating files, so the base text sets are complete.

## Gate (3) — changed-line no-regression for the seven instrumented edited files

Every added line of `git diff --unified=0 $BaseSha HEAD -- <file>` is listed below as `hits=<n>` (a Cobertura row) or `hits=non-executable` (no row); `n/a` marks the three cited clauses. The ratio is over the added executable lines other than the `n/a` lines.

### `Viewers/WebView2BreadcrumbHost.cs` — 7 hunks, 27 added, 13 deleted

- Added lines: 16-19 non-executable (doc comment); 154-157 non-executable (remarks); 161 non-executable (comment); 162 non-executable (`void NavigateCore()`); 163 hits=1; 164 hits=1 (`CoreWebView2? core = _control.CoreWebView2;`); 165 hits=1; 166 hits=1; 167-169 hits=1 (the `log.Error(` call); 170 hits=1 (`return;`); 171-172 non-executable; **173 hits=0 n/a** (`ForwardNavigateToString(html);` inside `NavigateCore`, by citation of the forwarder's exemption remark — re-derived in the post-change file at `WebView2BreadcrumbHost.cs:187-192` with `[ExcludeFromCodeCoverage]` at `:193`; the plan's citation `:170-172` referred to the pre-change file, where the remark sat at `:171-172`); 174 hits=1; 175 non-executable; 179 hits=1 (`NavigateCore();`, the null-dispatcher inline path); 183 hits=1 (`_ = dispatcher.Dispatch(NavigateCore);`); 263 hits=1 (`string cacheFolder = WebView2EnvironmentContract.ResolveUserDataFolder();`); 264 hits=1 (`CoreWebView2EnvironmentOptions options = WebView2EnvironmentContract.CreateOptions();`)
- ADDED-EXECUTABLE (after n/a): 13; COVERED: 13; N/A: 1; NON-EXECUTABLE: 13; RATIO: 1.0000 (plan expectation: 100 percent) — PASS

### `Controllers/BreadcrumbBridgeRouter.cs` — 1 hunk, 46 added, 0 deleted

- Added lines: 331-346 non-executable (const declaration text, doc comment, signature); 347-350 hits=1 (null guard and throw); 351-352 non-executable; 353-366 hits=1 (`hadPendingDocument`, `_pendingDocument = null;`, `DiscardPending()`, `BuildRows(...)` four lines, `_selectedRowId = null;`, the selection-clearing `if` block with `SelectedFolderPathChanged?.Invoke(this, null);`); 367 non-executable; 368 hits=1 (`_host.NavigateToString(...)`); 369-374 hits=1 (the `log.Error(` call); 375 hits=1; 376 non-executable
- ADDED-EXECUTABLE (after n/a): 26; COVERED: 26; N/A: 0; NON-EXECUTABLE: 20; RATIO: 1.0000 (plan expectation: 100 percent, selection-clearing branch included) — PASS

### `Controllers/BreadcrumbOutboundQueue.cs` — 1 hunk, 13 added, 0 deleted

- Added lines: 66-73 non-executable (doc comment, signature); 74 hits=1; 75 hits=1 (`int discarded = _pending.Count;`); 76 hits=1 (`_pending.Clear();`); 77 hits=1 (`return discarded;`); 78 hits=1
- ADDED-EXECUTABLE (after n/a): 5; COVERED: 5; N/A: 0; NON-EXECUTABLE: 8; RATIO: 1.0000 (plan expectation: 100 percent, the three `DiscardPending` statements) — PASS

### `Controllers/EfcHomeController.cs` — 4 hunks, 20 added, 3 deleted

- Added lines: 50-52 non-executable (public constructor parameters); **54-61 hits=0 n/a** (the public constructor's forwarding initializer `: this(` through `) { }`, by citation of `EfcHomeController.cs:40`, re-derived: `private static EfcHomeControllerDependencies CreateDefaultDependencies()` is at line 40 and binds the production factories, and every QuickFiler.Test call site passes a dependencies instance); 67-69 hits=1 (internal constructor parameters); 84-86 non-executable (comment); 87 hits=1 (`DataModel.CarriedFolderHandler = carriedFolderHandler;`); 88 hits=1 (`DataModel.CarriedMailHelper = carriedMailHelper;`); 89 non-executable
- The two deposit statements (87, 88) are covered, as required.
- ADDED-EXECUTABLE (after n/a): 5; COVERED: 5; N/A: 8; NON-EXECUTABLE: 7; RATIO: 1.0000 (plan expectation: 100 percent) — PASS

### `Controllers/QfcItemController.cs` — 1 hunk, 6 added, 0 deleted

- Added lines: 267-270 non-executable (doc comment); 271 hits=1 (`internal IFolderSearchHandler FolderHandler => _folderHandler;`); 272 non-executable
- ADDED-EXECUTABLE (after n/a): 1; COVERED: 1; N/A: 0; NON-EXECUTABLE: 5; RATIO: 1.0000 (plan expectation: 100 percent) — PASS

### `Helper Classes/EfcViewerQueue.cs` — 3 hunks, 9 added, 2 deleted

- Added lines: 25 hits=1 (`> ProductionBlockingPriorityScheduler { get; set; } = InvokeOnUiDispatcher;`, the initializer); 68 hits=1 (`ProductionBlockingPriorityScheduler = InvokeOnUiDispatcher;`, the reset); 71-75 non-executable (doc comment and signature); **76 hits=0 n/a** (`UiThread.Dispatcher.Invoke(action, priority);`, the single body line of `InvokeOnUiDispatcher`, by citation of D8: `UiThread.Dispatcher` throws `InvalidOperationException` outside an initialized UI thread, re-derived at `UtilitiesCS/Threading/UiThread.cs:264-282`, throw at `:277`); 77 non-executable
- Its remaining added executable lines (25, 68) are covered, as required.
- ADDED-EXECUTABLE (after n/a): 2; COVERED: 2; N/A: 1; NON-EXECUTABLE: 6; RATIO: 1.0000 (plan expectation: 100 percent) — PASS

### `Controllers/QfcItemController.ViewerSetup.cs` — 1 hunk, 3 added, 8 deleted

- Added lines: 55 non-executable (comment); 56 non-executable (`string cacheFolder = WebView2EnvironmentContract.ResolveUserDataFolder();`); 57 non-executable (`CoreWebView2EnvironmentOptions options = WebView2EnvironmentContract.CreateOptions();`) — no Cobertura row exists for any of them because they sit inside the method-level-exempt `InitializeWebViewAsync` (`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` re-derived at `QfcItemController.ViewerSetup.cs:48`, the method signature at `:49`)
- Recorded `n/a` by citation of `ViewerSetup.cs:48` — PASS (not ratio-gated)

GATE-3: PASS (no file carries an added executable line that no test reached other than the three `n/a` lines; no `BLOCKED: coverage`)

## Gate (4) — type-level no-regression for the split

- `EfcFormController` parts, final `lines-covered`: `EfcFormController.cs` 74 + `Breadcrumb.cs` 62 + `SetupAndProperties.cs` 47 + `EventHandlers.cs` 44 + `Actions.cs` 23 + `Helpers.cs` 45 = 295; baseline `EfcFormController.cs` 251; 295 >= 251 — PASS
- `EfcDataModel`: `EfcDataModel.cs` 193 + `EfcDataModel.Carry.cs` 34 = 227; baseline `EfcDataModel.cs` 189; 227 >= 189 — PASS

GATE-4: PASS

## Gate (5) — per-file no-regression for the seven files (post >= base minus deleted covered lines)

| File | post `lines-covered` | base `lines-covered` | deleted lines with base hits >= 1 | floor | result |
|---|---|---|---|---|---|
| `Viewers/WebView2BreadcrumbHost.cs` | 100 | 91 | 6 (old lines 166, 246-250) | 85 | PASS |
| `Controllers/BreadcrumbBridgeRouter.cs` | 237 | 211 | 0 | 211 | PASS |
| `Controllers/BreadcrumbOutboundQueue.cs` | 29 | 23 | 0 | 23 | PASS |
| `Controllers/EfcHomeController.cs` | 230 | 226 | 1 (old line 58) | 225 | PASS |
| `Controllers/QfcItemController.cs` | 74 | 73 | 0 | 73 | PASS |
| `Helper Classes/EfcViewerQueue.cs` | 46 | 46 | 2 (old lines 25, 68) | 44 | PASS |
| `Controllers/QfcItemController.ViewerSetup.cs` | 194 | 194 | 0 (the 8 deleted lines had no baseline row; method-level exempt) | 194 | PASS |

GATE-5: PASS

## Gate (6) — package comparability

- `QuickFiler` package `lines-valid`: baseline 12633, final 12754; absolute difference 121; 1 percent of the baseline figure 126.33; 121 <= 126.33
- BRANCH: A
- `QuickFiler` package `line-rate`: baseline 0.817304, final 0.820056; baseline minus 0.005 = 0.812304; 0.820056 >= 0.812304
- GATE-6: PASS

(Branch B not taken.)

## Gate (7) — scoped document line-rate, reported, not gated (D10)

- QUICKFILER-SCOPED-DOCUMENT-LINE-RATE: baseline 0.242471 | final 0.244154
- REPO-WIDE-FLOOR: NOT MEASURED (single test assembly; see D10)

## Summary against the Phase 0 baseline

- `QuickFiler` package: 10325/12633 lines (0.817304), 2487/3187 branches (0.780358) at baseline; 10459/12754 lines (0.820056), 2517/3217 branches (0.782406) final; +134 covered lines, +121 valid lines.
- Tests: 1436 passed at baseline; 1468 passed final (+32).

## Acceptance

Gates (1) through (5) are PASS; exactly one branch of (6) is named (A). No `BLOCKED: coverage`.
