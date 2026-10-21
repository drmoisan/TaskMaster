# efc-controller-surface-defects (Issue #464)

- Issue: #464
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/464
- Type: bug
- Work Mode: full-bug
- Epic: quickfiler-bug-family
- Integration Branch: epic/quickfiler-bug-family-integration
- Wave: 2
- Upstream Dependencies: #484, #444
- Owner: drmoisan
- Last Updated: 2026-08-25
- Status: Active

## Summary

This feature closes eight pre-existing defect issues on the Email Filer (EFC) controller surface.
All eight were filed on 2026-08-07 during preparation research for epic #136 and were deferred out
of that work because its non-functional requirement prohibited behavior change to observable
QuickFiler flows. Every defect alters observable behavior on a teardown, notification, input-routing,
or failure path, so each requires its own regression test.

The eight issues are grouped into one feature because they are confined to three files of the same
subsystem (`EfcFormController.cs`, `EfcItemController.cs`, `EfcViewer.cs`) plus one shared line in
`QfcItemController.ViewerSetup.cs`, and because three of them share a single root cause. Splitting
them would produce eight concurrent branches editing the same three files.

## Issues Closed by This Feature

| Issue | Title | Primary file | Severity |
|---|---|---|---|
| #459 | `efc-item-controller-keyboard-registration-defects` | `EfcItemController.cs` | High |
| #460 | `efc-item-controller-cleanup-nre-and-timer-leak` | `EfcItemController.cs` | High |
| #461 | `efc-item-controller-dead-conversation-expanded-handler` | `EfcItemController.cs` | High |
| #463 | `quickfiler-webview2-incognito-arg-en-dash` | `EfcItemController.cs`, `QfcItemController.ViewerSetup.cs` | Medium |
| #464 | `efc-controllers-null-guard-and-async-void-boundary-defects` | `EfcFormController.cs`, `EfcItemController.cs` | High |
| #465 | `efc-form-controller-lifecycle-and-selection-defects` | `EfcFormController.cs` | High |
| #466 | `efc-dead-code-and-latent-nre-traps` | `EfcViewer.cs`, `EfcItemController.cs` | Medium |
| #467 | `efc-viewer-processcmdkey-swallows-alt-mnemonics` | `EfcViewer.cs` | Medium |

Issue #464 is the primary issue for this feature and supplies the folder name and branch suffix.

## Root-Cause Grouping

The eight issues reduce to eight distinct root causes, not eight independent defects. Three issues
share one cause; two issues share one edit site under different causes. The grouping below is
authoritative for planning and is restated in `spec.md`.

### RC1 — No post-teardown null-state contract (shared by #460, #464, #465)

`Cleanup()` on both controllers nulls its fields, and nothing downstream guards the resulting state.
Property getters, re-entrant action paths, and dependency-passing helpers all assume live fields.
The already-merged QFC twins carry exactly the guards the EFC side lacks, which fixes the intended
behavior without design work.

- #460 A: `EfcItemController.Cleanup()` unconditionally dereferences `Buttons` (backing `_buttons`),
  which the 5-argument constructor never assigns; `Cleanup` also never nulls `_buttons` while nulling
  15 siblings, and writes `_itemViewer = null` twice (`EfcItemController.cs:264`, `:276`).
- #460 C: `Subject` reads `_itemViewer.LblSubject.Text` (`:610`) while `Sender` (`:595`) and `To`
  (`:621`) read `_itemInfo`, so `Subject` throws post-`Cleanup` while its siblings still work.
- #464 A: theme and dark-mode accessors pass dependencies eagerly, so the dependency check cannot run
  before the dereference. `EfcFormController.DarkMode` (`:272-282`) passes `_globals.Ol` as a
  `params object[]` element; `ActiveTheme` (`:255`) uses `strict: true` with `_themes` as sole
  dependency; `LoadTheme` (`:267`) dereferences a null `_themes`; `EfcItemController.DarkMode`
  (`:439`) repeats the eager-argument shape.
- #465 A: `EfcFormController.Cleanup()` (`:187-194`) is not idempotent and has no re-entrancy guard.
  Two paths can invoke the OK action for one user gesture (the always-on `Keys.Return` binding and
  the OK button `Click` subscription), and the second `ActionOkAsync` dereferences a nulled field.

RC1 is one cause with one remedy shape: an explicit post-cleanup contract (idempotent `Cleanup`,
lazily-evaluated dependency checks, and consistent accessor backing fields). Fixing #464 A without
fixing #460 A/C and #465 A would leave the same class of defect live on adjacent members.

### RC2 — Dereference-instead-of-dispose on teardown (#460 B)

`EfcItemController.cs:277` assigns `_timer = null` without disposing the `System.Threading.Timer`
declared at `:377`, leaking an armed OS timer per item cleaned up while expanded and unread. This
lives in the same method as RC1 but is a distinct cause: the field is correctly nulled, the resource
is not released.

### RC3 — `async void` and unobserved-Task fault escape (#464 B, C, D, E)

Faults on fire-and-forget and `async void` paths escape to the synchronization context or the thread
pool instead of reaching a logged boundary.

- B: `logger.Error(...); throw;` inside `async void` at `EfcFormController.cs` (five handler sites).
- C: `_ = PopulateFolderCombobox()` with no `try`/`catch` in the callee, so the folder list silently
  stays empty on fault. The sibling fire-and-forget in `InitializeBreadcrumbHostAsync` does carry a
  logged boundary and is the in-repo remedy pattern.
- D: `async` lambdas registered into `CharActions`, which is
  `KbdActions<char, KaChar, Action<char>>`, so they compile as `async void`
  (`EfcItemController.cs:704`, `:711`, `:716`, `:741`, `:882`, `:887`).
- E: `throw (e.InitializationException)` at `EfcItemController.cs:777` rethrows a captured exception
  and resets its stack trace from inside a WebView2 UI-thread event handler.

### RC4 — `KbdActions<>` contract misuse (#459 A, B, C)

The `KbdActions<>` indexer setter performs a `Find(key)` and assigns only when the element is
non-null, so a missing key is a silent no-op rather than an insert; `Add` throws `ArgumentException`
on a duplicate `(sourceId, key)` pair. The sync and async expansion paths use the API inconsistently.

- A: `RegisterActions` (`EfcItemController.cs:680`) assigns through the indexer after filtering out
  exactly the keys that are present, so the `overwriteDuplicates: false` path registers nothing.
- B: `ToggleExpansion(ToggleState)` (`:862`) registers and removes `'B'`/`'D'`; the async bodies
  `ToggleExpansionOn` (`:944`) and `ToggleExpansionOff` (`:931`), dispatched by
  `ToggleExpansionAsync` at `:913` and `:922`, do neither.
- C: because of B, a sync-On / async-Off / sync-On sequence leaves the entries in place and the
  second sync-On throws on a UI-thread call path.

**Shared edit site with RC3-D.** The `'B'`/`'D'` registration block that #459 B must change contains
the `async void` lambdas that #464 D must change (`EfcItemController.cs:882`, `:887`). These are two
causes at one edit site and must be sequenced in one phase, not two.

### RC5 — Non-ASCII character in a machine-parsed literal (#463)

The WebView2 additional-browser-arguments string is `"–incognito "`, whose first character is
U+2013 EN DASH rather than two ASCII hyphen-minus characters, so Chromium silently ignores the
switch and browsing data persists. Three call sites, one cause:
`EfcItemController.cs:184`, `EfcItemController.cs:217`, `QfcItemController.ViewerSetup.cs:55`.

### RC6 — `nameof` bound to a name the publisher never raises (#461)

`ConversationResolverPropertyChanged` (`EfcItemController.cs:741`) guards on
`nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded)`, which resolves at compile time
to the literal `"Expanded"`. `ConversationResolver` raises only `"ConversationInfo"`,
`"ConversationItems"`, `"Df"`, and `"UpdateUI"`. The subscription at `:668` fires; the handler body
never executes, so background-loaded conversation rows never reach the topic thread.

### RC7 — Duplicated magic constant with divergent arity (#465 D)

The banner prefix is tested three ways: `Substring(0, 3) == "==="` in `IsValidSelection`
(`EfcFormController.cs:1047`), `StartsWith("====")` in `ActionOkAsync` (`:706`), and
`BreadcrumbRowBuilder.BannerPrefix` (`"===="`). A row beginning with exactly three `=` is classified
inconsistently across the three sites.

### RC8 — Illegal cross-thread WinForms control read (#465 B)

`RefreshSuggestionsAsync` (`EfcFormController.cs:795-804`) evaluates `_formViewer.SearchText.Text`
inside the `Task.Run` lambda at `:799`. `SearchText_TextChanged` reads the same property correctly on
the UI thread.

### RC9 — Read-modify-write through a rebind that writes back (#465 C)

`ActionDeleteAsync` (`EfcFormController.cs:740-748`) reads `_folderRows`, inserts `"Trash to Delete"`
at index 0, and calls `BindFolderRows`, which at `:871` stores the *result* — now containing the
trash row — back into `_folderRows`. A second invocation inserts a second trash row.

### RC10 — Input-routing over-claim (#467)

`EfcViewer.ProcessCmdKey` (`EfcViewer.cs:94-105`) returns `true` for every Alt-modified key whenever
a keyboard handler is attached, without asking the handler whether it claims the key, so
`base.ProcessCmdKey` never runs and both menu strips lose their mnemonic path.

### RC11 — Dead code carrying a latent trap (#466)

- A: `EfcViewer.SetController` (`EfcViewer.cs:50-53`) has no call site from `EfcFormController`,
  unlike its QFC twin, so `_formController` (`:48`) is permanently null and
  `EditFiltersMenuItem_Click` (`:157-160`) would throw. The handler is currently unreachable because
  `EfcViewer.Designer.cs` never wires `EditFiltersMenuItem.Click`; a routine Designer regeneration
  arms it.
- B: `InitializeWebView()` (`EfcItemController.cs:174`) and `RegisterActions` (`:680`) have zero call
  sites; `_selectorsCtrls` (`:381`) is initialized to `null`, never assigned, and passed to
  `SetupThemes` at `:97` and `:144`.
- C: the 7-argument `EfcItemController` constructor overload has zero call sites.
- D: `QuickFiler/Viewers/EfcViewer3.cs` and siblings are present in the tree with no
  `<Compile Include>` entry, yet `EfcViewer3.cs` carries a misleading `[ExcludeFromCodeCoverage]`.

RC11-A and RC10 are both in `EfcViewer.cs` but are distinct causes with distinct remedies.

## Authoritative Requirement Sources

The promoted potential documents are the authoritative requirement source. Each carries file:line,
the offending code block, root cause, suggested fix, and severity, and is richer than the GitHub
issue body:

- `docs/features/potential/promoted/2026-08-07-efc-item-controller-keyboard-registration-defects.md` (#459)
- `docs/features/potential/promoted/2026-08-07-efc-item-controller-cleanup-nre-and-timer-leak.md` (#460)
- `docs/features/potential/promoted/2026-08-07-efc-item-controller-dead-conversation-expanded-handler.md` (#461)
- `docs/features/potential/promoted/2026-08-07-quickfiler-webview2-incognito-arg-en-dash.md` (#463)
- `docs/features/potential/promoted/2026-08-07-efc-controllers-null-guard-and-async-void-boundary-defects.md` (#464)
- `docs/features/potential/promoted/2026-08-07-efc-form-controller-lifecycle-and-selection-defects.md` (#465)
- `docs/features/potential/promoted/2026-08-07-efc-dead-code-and-latent-nre-traps.md` (#466)
- `docs/features/potential/promoted/2026-08-07-efc-viewer-processcmdkey-swallows-alt-mnemonics.md` (#467)

## Line-Citation Currency (verified 2026-08-25 against merge base `2300becf`)

Pull request #605 landed an independent fix for issue #439 touching `EfcFormController.cs`,
`BreadcrumbBridgeRouter.cs`, and several `UtilitiesCS/OutlookObjects/Folder/` breadcrumb files. That
change is already merged into this branch, so every citation into `EfcFormController.cs` taken from a
2026-08-07 issue body is stale by a small offset. The verified current anchors are:

| Issue | Issue-body citation | Verified current line | File |
|---|---|---|---|
| #465 A | `:189-196` | `:187-194` (`Cleanup`), `:189` (`_globals.Ol` deref) | `EfcFormController.cs` |
| #465 B | `:800-803` | `:795-804` (`RefreshSuggestionsAsync`), `:799` (read inside `Task.Run`) | `EfcFormController.cs` |
| #465 C | `:742-750`, `:881` | `:740-748` (`ActionDeleteAsync`), `:871` (`BindFolderRows` write-back) | `EfcFormController.cs` |
| #465 D | `:708`, `:1049` | `:706` (`StartsWith("====")`), `:1047` (`Substring(0, 3)`) | `EfcFormController.cs` |
| #464 A | `:257`, `:269`, `:276-283` | `:255` (`strict: true`), `:267` (`LoadTheme`), `:272-282` (`DarkMode`) | `EfcFormController.cs` |
| #464 A (item) | `:441-448` | `:439` (`DarkMode`) | `EfcItemController.cs` |

Citations into `EfcItemController.cs`, `EfcViewer.cs`, and `QfcItemController.ViewerSetup.cs` were
re-verified individually and are current as written, except `QfcItemController.ViewerSetup.cs:52`
which is now `:55`.

## Files This Feature Owns

Production files this feature may write:

- `QuickFiler/Controllers/EfcFormController.cs` (1084 lines at merge base)
- `QuickFiler/Controllers/EfcItemController.cs` (1170 lines at merge base)
- `QuickFiler/Viewers/EfcViewer.cs` (162 lines at merge base)
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (430 lines at merge base) — RC5 only, the
  single `"–incognito "` literal at `:55`. No other change to this file is in scope; it is owned by
  upstream feature #484.

Project files:

- `QuickFiler.Test/QuickFiler.Test.csproj` — new `Compile Include` entries only, inserted adjacent to
  the existing `Controllers\Efc*Tests.cs` cluster.
- `QuickFiler/QuickFiler.csproj` — only if RC11-D removes orphaned files.

## Acceptance Criteria

Acceptance criteria for this `full-bug` feature are authored in `spec.md`, which is the authoritative
AC source per the `acceptance-criteria-tracking` skill. This section exists to name that source, not
to duplicate it.

## Out of Scope

- Splitting `EfcFormController.cs` or `EfcItemController.cs` to satisfy the 500-line file ceiling.
  Both files exceed the ceiling at the merge base (1084 and 1170 lines) and predate this feature.
  Reducing them is a refactor, not a bug fix, and would collide with every other epic child touching
  these files. No acceptance criterion in this feature may assert a line count under 500 for either
  file; the ceiling is asserted only over files this feature creates.
- Any change to `QfcItemController.*` beyond the single RC5 literal.
- Any change to `EfcHomeController.*` (owned by feature #442) or the breadcrumb surface (owned by
  features #498 and #501).
- Wiring `EfcViewer.SetController` into a working Edit Filters command as a new feature. RC11-A is
  resolved by removing the trap or by wiring the existing member, whichever the spec selects; adding
  new Edit Filters functionality is not in scope.
