# Code Review — efc-controller-surface-defects-464

- **Timestamp:** 2026-08-28T02-29 (UTC)
- **Branch:** `bug/efc-controller-surface-defects-464` @ `7c9b02ee` vs `origin/epic/quickfiler-bug-family-integration` @ `38f09789`
- **Files reviewed:** all 12 non-doc paths in the branch diff, in full

## Findings

| ID | Severity | File:Line | Finding |
|---|---|---|---|
| CR-1 | Minor | `QuickFiler/Controllers/EfcFormController.cs:1142` | Truncated comment: `// #465 D (RC7): single classification owner. StartsWith, never Substring, over the` — ends mid-clause (intended completion is presumably "over the producer's prefix"). Cosmetic. |
| CR-2 | Minor | `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:453` | `IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` reproduces `ActionOkAsync`'s guard expression rather than driving the member (the member shows a `MessageBox`, so driving it is prohibited). The reproduction is faithful today and AC 976 pins the structural routing, but if a later change reshapes the in-method guard the test will keep passing against the stale reproduction. A comment in the test acknowledges this. Acceptable under the headless policy; note for future maintainers. |
| CR-3 | Minor | `QuickFiler/Viewers/EfcViewer.cs:117` | `ProcessCmdKey` invokes `_keyboardHandler.ToggleKeyboardDialogAsync(sender, e)` without awaiting or observing the returned task. Pre-existing shape; this feature changed only the claim condition on the line above. Out of scope, recorded for completeness (a fault in the dialog toggle escapes unobserved). |
| CR-4 | Observation | `QuickFiler/Controllers/EfcFormController.cs:959-977` | The `BindFolderRows` (presentation-only) / `BindSourceFolderRows` (retain + present) split is the load-bearing fix for #465 C delete-gesture accumulation. Naming is close enough to invite mis-selection by a future caller; the comments at both members mitigate this. No action required. |

No Blocking or Major findings.

## Design and Correctness Assessment

- **RC1 (post-teardown null-state contract).** The detach-then-null ordering in both `Cleanup()` methods matches the 484 convention. The `_parentCleanup` clear-before-invoke makes single invocation structural rather than flag-guarded — simpler than a re-entrancy flag and verified by `FormCleanup_InvokesParentCleanupExactlyOnce` (`Times.Once()` across two calls). Call-site null tests on `DarkMode`/`ActiveTheme` are the correct remedy for the `params object[]` eager-materialisation defect: the dependency array is built before `GetOrLoad` is entered, so no callee-side fix could work.
- **RC2 (timer).** `_timer?.Dispose(); _timer = null;` with the disposal-precedes-null comment; test observes disposal as `ObjectDisposedException` state on a never-firing timer. Deterministic, race-free.
- **RC3 (`async void` boundaries).** The five handlers are now one-line adapters over `internal async Task` members; faults route to `BoundaryErrorSink`, an injectable seam whose default is exactly one `logger.Error(message, exception)` call — the 444 rim pattern (catch-and-log, no rethrow), and the seam-with-default shape mirrors 484's `MoveFailureNotifier`. `throw;` is absent from the file (grep-verified). `PopulateFolderCombobox` gained its own boundary because both call sites discard the task. `ThrowInitializationFailure` uses `ExceptionDispatchInfo.Capture(...).Throw()` and the test proves same-instance rethrow with the originating frame preserved.
- **RC4/RC6/RC11 (dead surface removal).** `RegisterActions`, the sync `ToggleExpansion` pair, `ConversationResolverPropertyChanged`, `InitializeWebView`, the 7-parameter constructor, `_selectorsCtrls`, the `EfcViewer` dead members, and the three `EfcViewer3.*` orphans are removed rather than repaired — correct for dead code, and each removal is pinned by a reflection-absence test so re-introduction fails a named test. The `_selectorsCtrls` argument is replaced by an explicit commented `null`, which is behaviour-identical (the field was declared null and never assigned).
- **RC5 (incognito literal).** Hoisted to `internal const string IncognitoArgument`, asserted byte-wise (ASCII check plus explicit U+002D assertions on the first two characters); the QFC site fixed in place with a one-line diff. The EN DASH is gone from the repository's live sites (byte-level grep returned zero).
- **RC7 (banner classification).** `IsBannerRow` classifies by `BreadcrumbRowBuilder.BannerPrefix` (`"===="`) via `StartsWith`, never `Substring`; `IsValidSelection` routes through `IsSelectableFolder`, and `ActionOkAsync`'s guard composes `IsBannerRow` with the retained `EfcSelectionGuard.IsValidFilingSelection`, so #614's rooted-path rejection survives and the change is strictly narrowing at the filing boundary. See the feature audit for the RC7 non-edit judgment.
- **RC8 (`RefreshSuggestionsAsync`).** The UI-thread control read happens before `Task.Run`; the worker lambda touches only `_dataModel` and the captured string. `MatchesForSearchText` is a pure, null-safe helper tested without any viewer.
- **RC9 (`BindFolderRows` write-back).** The retention responsibility moved to `BindSourceFolderRows` and `ApplyDeleteGesture`; `WithTrashRow` is idempotent (asserted by applying it twice, and end-to-end by driving `ActionDeleteAsync` twice).
- **RC10 (`ClaimsAltChord`).** Pure static predicate; masks with `Keys.KeyCode` and claims only bare Alt (`Keys.Menu`/`Keys.None`), so Alt+F/Alt+M mnemonics reach `base.ProcessCmdKey`. Scoped to `EfcViewer` only; `CharActions` reachability through `KeyboardHandler` (owned by #498, constrained by 444) is untouched.

## Test Quality

- All new tests follow Arrange–Act–Assert with descriptive `Member_Condition_Expectation` names and reasoned FluentAssertions messages.
- Positive, negative, boundary (null, empty, short-row), idempotency, and post-teardown state-transition scenarios are covered for each remedy; the `[DataTestMethod]` over the five boundary members gives five named results.
- Fixtures use `FormatterServices.GetUninitializedObject` plus reflection seams; Outlook-interop-importing files fully qualify `System.Action`/`System.Exception` (the namespace trap from the upstream briefing is avoided).
- No test constructs, shows, or derives from a `Form`; no message pump; no timing dependence anywhere.

## Compliance Notes

- CSharpier clean (re-verified on all 8 files), analyzer and nullable gates clean and non-vacuous per evidence, 1169/1169 tests green (TRX independently recounted).
- No public API surface removed that any live caller uses: removed members were dead (zero references outside the removed blocks; reflection tests pin absence), and `IQfcItemController` fixed points (`ToggleNavigation(bool)`, `MoveMailAsync`) are implemented and untouched — no path under `QuickFiler/Interfaces/` appears in the diff.
