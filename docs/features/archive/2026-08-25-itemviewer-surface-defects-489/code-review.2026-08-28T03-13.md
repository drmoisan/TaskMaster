# Code Review — itemviewer-surface-defects (Issue #489)

- Timestamp: 2026-08-28T03-13 (UTC)
- Branch: `bug/itemviewer-surface-defects-489` at `74d02ad2` vs merge base `69e83171` (`epic/quickfiler-bug-family-integration`)
- Reviewed: all 25 code/project paths in the branch diff, read in full or in diff form; four new test files read end to end.

## Findings Summary

| ID | Severity | Blocking | File / location | Finding |
|---|---|---|---|---|
| RC-1 | Major | **Yes** | `QuickFiler/Controllers/QfcItemController.EventWiring.cs` (`WireIntentEvents` line 94; `UnwireIntentEvents` line 446 ff.) | 17 subscriptions vs 16 detachments: `PicturesChanged` is never unwired, so `Cleanup()` leaks one live subscription per controller lifetime; regression introduced by this branch, deferral target (484) already merged. Detail below and in the policy audit § 8. |
| CR-1 | Minor | No | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` (`HtmlDarkConverter`) | The guarded and unguarded branches duplicate the seven-line navigate-and-toggle body verbatim. Extracting a local function would remove the duplication; the shape does mirror the existing guarded pair in `EventWiring.cs`, which the spec mandated, so this is style debt, not an error. |
| CR-2 | Minor | No | `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs` (`BuildInertFlagTasks`) | `FormatterServices.GetUninitializedObject(typeof(FlagTasks))` depends on `FlagTasks.Run(modal: true)` tolerating a fully-uninitialized instance. It does today (returns `DialogResult.None`), and the helper's doc comment records exactly why, but any future constructor-established invariant in `FlagTasks` will surface here as an opaque NRE rather than a compile error. Acceptable, clearly documented; a seam interface on `FlagTasks` would be sturdier if the type is touched again. |
| CR-3 | Info | No | `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:80` / `QfcItemController.Navigation.cs:54` | The #490 D2 bare forward converts an off-UI-thread `JumpToSearchTextbox` from a deterministic throw into a silent no-op. Known, spec-adopted, recorded in the D2 dossier and reframed finding O3 (444-owned caller guard). Must be promoted to an issue after merge or the residual disappears. |
| CR-4 | Info | No | `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | Sits at exactly 500/500 lines after receiving only line-neutral rename edits. Zero headroom for any future edit; the next feature touching it must extract first. Same pressure: `EventWiringTests.cs` 499, `MailActionsTests.cs` 498, `FolderHandlingTests.cs` 498. |
| CR-5 | Info | No | `docs/.../evidence/baseline/phase0-baseline-index.2026-08-27T23-36.md` | The index cures its own staleness via a dated amendment section rather than a superseding index file. Acceptable and honest; a reader must read to the end to get the true state. |

## RC-1 — the shipped subscription leak (Blocking)

**What ships:** `WireIntentEvents()` now performs 17 `+=` subscriptions (the 17th, `_itemViewer.PicturesChanged += this.CbxPictures_CheckedChanged;`, is the #486 D3 fix and is correct); `UnwireIntentEvents()` still performs 16 `-=` detachments with no `PicturesChanged` line (both counts re-measured on HEAD by this review). A controller that is wired and later passes through `Cleanup()` therefore keeps one live delegate on its viewer: the torn-down controller stays reachable from the pooled viewer (object-lifetime leak accumulating one controller graph per recycle), and every subsequent picture toggle on the reused viewer still invokes the stale controller's handler.

**Why the shipped deferral does not hold:** the executor's handoff record (`evidence/other/wireintentevents-16-to-17-handoff.2026-08-28T01-55.md`) is accurate and satisfies AC11's letter, but it hands the 17th detachment to "upstream 484" while itself recording `Upstream484Landed: true`. 484 is merged and closed; there is no in-flight branch, no issue, and no live sibling that owns the fix. The obligation dangles.

**Why the fix belongs in this branch (each point verified on disk):**
1. The 484-owned regression test `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` (`EventWiringTests.cs:377-419`) asserts sixteen individual `VerifyRemove(..., Times.Once())` calls and never pins a total — adding a 17th detachment keeps it green unmodified.
2. `EventWiring.cs` is already inside this feature's permitted diff set and measures 483 lines (17 spare under the ceiling).
3. `EventWiringTests.Part2.cs` (81 lines, this feature's own new file) already holds the wiring-side fixture; a mirrored `UnwireIntentEvents_DetachesPicturesChanged` test is roughly 15 lines.
4. `-=` on a never-attached event is a no-op, and `UnwireIntentEvents()` already begins with the `_itemViewer is null` teardown guard, so the added line is safe on every path including aborted initialization.
5. The confinement wording in the spec ("the `EventWiring.cs` diff is confined to `WireIntentEvents`") is spec text amendable through the same dated in-place amendment mechanism the feature already used for the csproj-entry count; the amendment would strengthen, not weaken, the delivered behavior.

Directives are in `remediation-inputs.2026-08-28T03-13.md`.

## Production changes reviewed

- **`ToolStripMenuItemCb` ownership consolidation (#486 D1/D2)** — pure deletions on both twins plus the four constructor calls and five designer wirings; the setter (`ToolStripMenuItemCb.cs`, untouched, verified absent from diff) becomes the sole image owner. Correct: the deleted consumer-side handler read `ToolStripMenuItem.Checked` (always false under the `new`-shadowed property) and cleared the image on the same turn. The rejected `base.Checked = value` alternative is rightly rejected (double indicator). Designer edits are surgical: 0 additions, 1 and 5 deletions respectively.
- **`CbxPictures_CheckedChanged` + wire (#486 D3)** — matches the three sibling handlers exactly in shape and `[ExcludeFromCodeCoverage]`-free placement; `_optionsPictures` becomes a live projection. Covered 3/3 lines (100 percent). Correct fix; its unwire counterpart is RC-1.
- **`HtmlDarkConverter` guard (#489 D2)** — `InvokeRequired`-guarded routing through the mockable `IItemViewer.Invoke(Delegate)` seam; the whole navigate-plus-expanded-toggle body is marshalled as one turn, which is the right granularity (marshalling only `NavigateToString` would have left `item.ToggleDark` off-thread). See CR-1 for the duplication note.
- **`FocusSubject` `void` -> `bool` (#490 D3) and caller discard** — makes the discarded `Control.Focus()` result explicit at the only caller; interface XML doc states the contract and return meaning.
- **`SetFolderItems` -> `AddFolderItems` rename (#490 D1)** — the name now matches append semantics (`BreadcrumbCoordinator?.AddItems`). All production and test call sites updated (independent solution grep for residual `.SetFolderItems(`: zero); the two spec-protected test method names survive verbatim; 501-owned comment left deliberately stale as mandated.
- **`FlagAsTask`/`FlagAsTaskAsync` local-hold (#490 D4)** — branch on the local, property written once for presentation; `VerifyGet ... Times.Never()` tests pin the read-back's absence, and the `ViewerSetupTests` setter assertions were left intact.
- **`UiScheduler` seam deletion (#489 D4 carve-out)** — interface member, capture, and property removed; the two consumed seams (`UiDispatcher`, `UiSyncContext`) pinned by tests; the six unrelated same-named members on other types verified untouched via diff absence.
- **`IItemViewer` XML documentation (#489 D3, #490 D2)** — ordering contract on the set/sort pair and one threading contract ("the viewer forwards; the controller marshals") on both focus members. Accurate and placed on the contract, where mocks are built from.

## Test additions reviewed (22 tests, four new files + one grown file)

All 22 use MSTest attributes, Moq where mocking occurs, FluentAssertions (or Moq interaction verification with justification strings) for assertions, Arrange–Act–Assert with comments, and per-test doc comments naming the defect. No banned timing API, no temp files, no live `Form`, no mutable global state left dirty. Notable strengths: the `ToolStripMenuItemCbTests` image assertions compare rendered byte sequences rather than reference equality; `ThemeMarshallingTests` deliberately leaves `Invoke` unconfigured so "marshalled" and "executed inline" are distinguishable, and seeds the `ConversationResolver.Count` sentinel so a failure is attributable to the guard rather than an incidental NRE (both decisions documented in-file); the metadata-absence tests are backed by the compiler argument (leftover designer `+=` to a deleted method is CS0103), which makes them complete REDs for deletion defects.

Scenario coverage is adequate for the defect set: positive (pins), negative/absence (metadata REDs), both guard branches for #489 D2, event-raise state transition for #486 D3, and async variant for #490 D4. No concurrency-sensitive behavior was added that would demand a scheduler harness.

## Scope and sibling discipline

- Scope-lock diff re-derived independently: exactly 25 code/project paths, one-to-one with the plan's P10-T2 permitted list.
- Forbidden files verified absent from the diff: `QfcItemController.Navigation.cs` (444), `ItemViewer.Breadcrumb.cs` (488), `BreadcrumbBridgeCoordinator.cs` and `.Search.cs` (501), `QfcCollectionControllerTests.cs` (pinned), `QfcItemController.TestSupport.cs` (493), any `UtilitiesCS/` path.
- `QuickFiler.Test.csproj`: exactly 4 additions, 0 deletions, appended at the recorded block tails, no pre-existing entry moved (diff read directly).
- `SeamDispatcherTests.cs`: single one-token hunk at line 193 (the compiler-forced rename); the spec-protected test at line 99 is untouched (the file's only hunk is the rename).
- Cross-child edits confined to the members named in § Sibling-collision resolution (verified per-file against the diffs).

## Verdict

One Blocking finding (RC-1). Everything else in the diff is correct, minimal, well-tested, and disciplined; after RC-1 remediation this branch is merge-ready from a code-quality standpoint.
