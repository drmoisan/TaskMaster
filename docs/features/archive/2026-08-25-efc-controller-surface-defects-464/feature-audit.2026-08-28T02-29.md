# Feature Audit — efc-controller-surface-defects-464

- **Timestamp:** 2026-08-28T02-29 (UTC)
- **Branch:** `bug/efc-controller-surface-defects-464` @ `7c9b02ee` vs `origin/epic/quickfiler-bug-family-integration` @ `38f09789` (16 commits; merge base equals base tip)
- **Work mode:** `full-bug` (`issue.md:6`) → AC source is **`spec.md` only**; `user-story.md` correctly does not exist
- **AC census:** 74 checkboxes in `spec.md`, all 74 checked at review time; plan `plan.2026-08-25T07-01.md` has 200/200 tasks checked

## Audit Method

The review mandate was to verify check-offs, not assume them. Every criterion was classified by how it was substantiated:

- **Direct re-verification (72 of 74):** the reviewer independently confirmed the criterion against the branch diff, the delivered source (greps, byte-level checks, line counts), the delivered test bodies (approximately 15 read in full; every named test confirmed present), the committed final TRX (outcome attributes recounted: 1169 `Passed`, 0 any other outcome), or the file system (deletions, absence of `user-story.md`, evidence-tree existence).
- **Committed-evidence verification (2 of 74):** criteria over the analyzer gate (no new diagnostics vs baseline) and the nullable gate (no new errors vs baseline) rest on `evidence/qa-gates/msbuild-analyzers.md` / `msbuild-nullable.md`, which carry exit codes, diagnostic-id set comparisons, and non-vacuity proof (0 `Skipping target "CoreCompile"` against 36 `csc.exe` invocations). Re-executing these rebuilds in the executor's worktree was deliberately avoided as a mutating operation; the evidence is internally consistent and accepted.

**No criterion was found checked without supporting evidence. Zero check-offs failed verification.**

## Verification by Issue Group

### #459 — keyboard registration defects (4/4 PASS)
`RegisterActions` and both sync `ToggleExpansion` overloads are deleted from source (diff-verified) and pinned absent by reflection tests. The On→Off→On test (body read in full) drives `ToggleExpansionOn`/`ToggleExpansionOff` against a strict `Mock<IQfcKeyboardHandler>` backed by a real `KbdActions<char, KaChar, Action<char>>`, asserts no throw and an untouched registry, and does not await the unpumped-dispatcher marshal — exactly as the criterion specifies. The `KbdActions<>` contract and `overwriteDuplicates` truth table are documented at `spec.md:320-323`, and the diff contains no path matching `KbdActions` (verified).

### #460 — Cleanup NRE and timer leak (7/7 PASS)
All seven tests exist and pass; bodies of the timer, `ApplyReadEmailFormat`, and `Subject`/`Sender`/`To` tests read in full. `Cleanup()` now contains exactly one `_itemViewer = null` assignment (diff-verified; the duplicate is gone), nulls `_buttons`, and disposes `_timer` before nulling it. The timer test uses the 484 deterministic technique verbatim.

### #461 — dead conversation-expanded handler (4/4 PASS)
Handler deleted (diff-verified, reflection-pinned); `PopulateConversation` assigns `SetTopicThread` to `ConversationResolver.UpdateUI` (source `:301` + named test); zero `PropertyChanged +=` subscriptions to a conversation resolver remain and the `nameof(...Expanded)` guard token is absent (both grep-verified).

### #463 — incognito EN DASH (4/4 PASS)
`internal const string IncognitoArgument = "--incognito "` declared and passed by `InitializeWebViewAsync`; the test (body read) asserts value equality, all-ASCII, and U+002D at positions 0 and 1. `ViewerSetup.cs:61` byte-verified as two U+002D (`cat -A`); a repository-file byte grep for U+2013 over all four production files returns zero. The `EfcItemController.cs` EN DASH site was removed with its containing method (`InitializeWebView` deleted wholesale), as required.

### #464 — null guards and async-void boundaries (12/12 PASS)
The five theme/dark-mode guard tests exist (three bodies read); the five-`[DataRow]` boundary test (body read) faults each extracted member and verifies exactly one sink invocation; the sink's default delegate is exactly one `logger.Error(message, exception)` call (source-inspected, as the criterion requires). `throw;` is absent from `EfcFormController.cs` and `throw (e.InitializationException)` absent from `EfcItemController.cs` (both grep-verified). `ThrowInitializationFailure` is `internal static void` taking `System.Exception`; its stack-trace-preservation test (body read) proves same-instance rethrow with the originating frame retained. #464 D is closed by the #459 deletions: the only remaining `CharActions` use in `EfcItemController.cs` is a `Remove` call (`:719`); no async lambda is registered into it.

### #465 — form-controller lifecycle and selection (11/11 PASS)
Idempotent `Cleanup` and `Times.Once()` parent-cleanup tests read in full; the clear-before-invoke structure makes single invocation structural. `RefreshSuggestionsAsync` reads `SearchText.Text` on the UI thread before `Task.Run`, and no `_formViewer` member access appears inside either lambda (diff-verified). `WithTrashRow` declared and idempotency-tested both directly and via double `ActionDeleteAsync`. `BindFolderRows` no longer writes `_folderRows` (the write-back moved to `BindSourceFolderRows`, which is the fix, not a violation — the criterion bans the write-back *in `BindFolderRows`*, and the accumulation defect is resolved because presentation-path rebinds no longer re-retain). `IsBannerRow` declared; both classification sites route through it (source-verified); the three-vs-four-`=` identical-classification and null/short-row tests read in full; `IsBannerRow`'s prefix is literally `BreadcrumbRowBuilder.BannerPrefix` (`"===="`, `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19`) and no `BreadcrumbRowBuilder` path is in the diff.

### #466 — dead code and latent NRE traps (8/8 PASS)
`SetController`, `_formController`, `EfcViewer.EditFiltersMenuItem_Click`, `InitializeWebView`, the 7-parameter constructor, and `_selectorsCtrls` all deleted (diff-verified, reflection-pinned). The live Edit Filters route (`EfcFormController.EditFiltersMenuItem_Click`, subscription at `:398` region) is unchanged and pinned by a named test. `EfcViewer3.cs`/`.Designer.cs`/`.resx` are absent from the working tree (verified); they were uncompiled orphans, so `QuickFiler/QuickFiler.csproj` correctly has a zero-line diff (verified). **The four pure-deletion paths in the diff are all spec-required deletions, not accidental losses.**

### #467 — ProcessCmdKey Alt-mnemonic swallowing (7/7 PASS)
`ClaimsAltChord` declared `internal static`; `ProcessCmdKey` returns `true` only inside the `ClaimsAltChord` branch (source read in full). All five predicate tests present (bare-Alt true; Alt+F false; Alt+M false; non-Alt false; null handler false). No test in `EfcViewerTests.cs` constructs, shows, or derives from a `Form` (grep-verified). QFC twin untouched: zero diff entries for `QfcFormViewer.cs` and `QfcFormKeyHandler.cs` (verified). Per the 444 constraint, the guard narrows only what `EfcViewer` claims; `KeyboardHandler.cs` (#498-owned) is not in the diff.

### Cross-cutting (17/17 PASS)
Baseline tree exists; CSharpier clean (independently re-run on the 8 owned files); analyzer and nullable gates clean vs baseline (committed-evidence verification, non-vacuous); final test run 1169/1169 with the passing count exceeding baseline by 70 >= 44 (TRX independently recounted); no pre-existing `[TestMethod]` deleted or renamed (the only touched pre-existing test file has a 317-added/0-deleted diff); created test files 260/470/164 lines, all < 500; `EfcFormController.cs` 1189 <= 1193 (the stricter addendum gate, which also satisfies the spec's literal 1204) with the evidence itemising per-remedy deltas against the true 1073 base; `EfcItemController.cs` 1117 < 1170; `ViewerSetup.cs` diff exactly one changed line at the incognito literal; sibling-owned path intersection empty; `WebView2BreadcrumbHost` construction byte-identical (21-line window diffs empty; moved only by uniform line offset from insertions above — the #476 dependency is on the construction's shape and enclosing member, both untouched); zero `[ExcludeFromCodeCoverage]` additions in any `.cs` diff (the ~12 whole-diff grep hits are markdown prose, confirmed); no `QuickFiler/Interfaces/` path in the diff; no `Thread.Sleep`/`Task.Delay` or other banned tokens in any test this feature wrote; no temp files, live Outlook, `BackgroundWorker`, or shown forms; `user-story.md` does not exist.

## Judgment: the RC7 non-edit decision was correct

The base-drift addendum directed delivering #465 D without editing `QuickFiler/Controllers/EfcSelectionGuard.cs` (merged sibling #614's file), and the executor complied. This reviewer independently confirmed the underlying facts: `EfcSelectionGuard.BannerPrefix` is `"==="` (`EfcSelectionGuard.cs:15`), both row producers use `"===="` (`BreadcrumbRowBuilder.cs:19`, `FolderSuggestionTree.cs:16`), and the pre-existing comment (now `EfcFormController.cs:318-320`, zero-diff region) describes a four-`=` rejection.

The decision is endorsed for three reasons:
1. **Widening `BannerPrefix` to `"===="` would relax a merged filing guard** — a three-`=` row would begin passing `IsValidFilingSelection` — in a file this feature does not own, while `EfcSelectionGuardTests.cs` asserts only on a four-`=` banner, so the relaxation would pass every existing test silently. That is precisely the widen-the-strict-guard failure mode recorded against #614's own remediation history.
2. **The delivered composition is strictly narrowing:** `ActionOkAsync` now rejects `null`, rejects `IsBannerRow` (four-`=`) rows, *and* retains `EfcSelectionGuard.IsValidFilingSelection` (which also rejects three-`=` prefixes and rooted paths), so no previously rejected input becomes accepted and both classification sites agree on both arities (test-verified: both rejected at both sites).
3. **The residual is preserved, not lost:** the third arity variant and the stale comment are recorded as follow-up item 7 in `evidence/other/followup-promotions.md` with a duplicate check and an explicit promotion path. It is a latent inconsistency with no user-observable effect today (no producer emits a three-`=` row), which is the correct severity for a promoted follow-up rather than an in-scope fix.

## Honest-Disclosure Endorsements (not defects)

- **Coverage delta:** the executor declined to claim the +14.93-point line-rate difference because the two runs' `lines-valid` denominators differ by 17,946 lines against a ~150-line production diff. This reviewer confirms the arithmetic and the reasoning; the required no-regression assertion holds and the delivered floors pass on the delivered denominator (see policy audit §6-§7).
- **Unmeasured new members:** `EfcItemController` and `EfcViewer` carry pre-existing class-level `[ExcludeFromCodeCoverage]`, so their new members are behaviour-tested but unmeasured. Disclosed in evidence, counts unchanged vs baseline, zero attribute additions in the diff.
- **Load-driven sibling failures:** the Phase 0 aggregate instrumented run recorded 15 `QfcItemController.*` timeouts (sibling #489's files); the isolated baseline and every final run are green. Correctly attributed as pre-existing environment flakiness under instrumentation load.

### Acceptance Criteria Status

- Source: `docs/features/active/efc-controller-surface-defects-464/spec.md`
- Total AC items: 74
- Checked off (delivered): 74
- Remaining (unchecked): 0
- Items remaining: none

Reviewer substantiation: 74 of 74 substantiated — 72 by direct independent re-verification against the diff, source, test bodies, TRX, and file system; 2 (the analyzer and nullable no-new-diagnostics gates) by internally consistent committed execution evidence carrying non-vacuity proof. Zero criteria found checked without supporting evidence; zero newly checked off by this review; zero unchecked.

## Outstanding Obligations at Fan-In (owed by the orchestrator, not this feature)

1. Promote the seven recorded follow-up items (including the RC7 residual) through the promotion lifecycle — they exist only as feature-folder prose until then.
2. Regenerate and retain a canonical C# coverage artifact before the integration→main PR (policy audit NB-1).
3. Carry the one-line `ViewerSetup.cs` edit through any fan-in conflict by keeping both edits, per `spec.md` §RC5 — never drop it.
