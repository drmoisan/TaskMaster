# Code Review — itemviewer-surface-defects (Issue #489) — remediation cycle 1 exit-gate reaudit

- Timestamp: 2026-08-28T04-35 (UTC)
- Branch: `bug/itemviewer-surface-defects-489` at `923cd3ce`; remediation delta reviewed line by line (`d77ac212..923cd3ce`); full-branch findings from `code-review.2026-08-28T03-13.md` re-dispositioned below.

## Findings Summary

| ID | Severity | Blocking | File / location | Finding |
|---|---|---|---|---|
| RC-1 | — | No | `QuickFiler/Controllers/QfcItemController.EventWiring.cs:481` | **CLOSED.** The `PicturesChanged` detachment now exists, mirroring the attach at `:94`; wire and unwire both perform 17 operations. Verified at source, at commit granularity (RED TRX committed with the test, fix in the following commit), at runtime (live rerun 2/2 passed), and in coverage (line 481 `hits="1"`). |
| RCV-1 | Info | No | `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:377` | `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` is deliberately left unrenamed although the member now performs 17 detachments. Adjudicated **acceptable**: the test pins sixteen individual `VerifyRemove(..., Times.Once())` calls and no total (no `VerifyNoOtherCalls` in the file), so every assertion it makes remains true; it passes unmodified (confirmed in this reviewer's live rerun and in the committed full-suite TRX); renaming a merged sibling's stable test node ID is churn with no behavioural gain; and the staleness is documented in the spec amendment and the handoff addendum. The 17th detachment has its own dedicated test, so the union of the two tests covers all 17. |
| CR-1 | Minor | No | `QfcItemController.FocusAndTheme.cs` (`HtmlDarkConverter`) | Carried forward unchanged (untouched by the remediation): guarded/unguarded branches duplicate the navigate-and-toggle body; style debt, spec-mandated shape. |
| CR-2 | Minor | No | `QfcItemController.MailActionsTests.Part2.cs` (`BuildInertFlagTasks`) | Carried forward unchanged: `GetUninitializedObject(typeof(FlagTasks))` fragility, documented in-code. |
| CR-3 | Info | No | `ItemViewer.FolderSearch.cs:80` / `QfcItemController.Navigation.cs:54` | Carried forward unchanged: #490 D2 bare forward converts an off-UI-thread throw into a silent no-op; spec-adopted; promotion to an issue owed after merge (reframed O3). |
| CR-4 | Info | No | `BreadcrumbDropDownIntegrationTests.cs` | Carried forward unchanged: exactly 500/500 lines, zero headroom; `EventWiringTests.cs` 499, `MailActionsTests.cs` 498, `FolderHandlingTests.cs` 498. The remediation correctly avoided both full files and grew only `Part2.cs` (105/500). |
| CR-5 | Info | No | `evidence/baseline/phase0-baseline-index.2026-08-27T23-36.md` | Carried forward unchanged: index cures its own staleness via a dated in-file amendment; read to the end. |

Blocking findings this cycle: **0**.

## The remediation delta, reviewed line by line

**Production (`QfcItemController.EventWiring.cs`, +1).** The single added line `_itemViewer.PicturesChanged -= this.CbxPictures_CheckedChanged;` is placed after the `AttachmentsChanged` detachment, mirroring the wire order, inside the member whose `_itemViewer is null` guard runs first. `-=` against a never-attached handler is a no-op, so the line is safe on every teardown path, including a controller that is cleaned up without ever being wired. No other production text changed. File at 484/500.

**Test (`QfcItemController.EventWiringTests.Part2.cs`, +24).** `UnwireIntentEvents_DetachesPicturesChanged` follows the file's existing harness pattern (mock viewer and keyboard handler, reflection field injection via the shared test-support helper, wire then unwire), asserts with `VerifyRemove(v => v.PicturesChanged -= It.IsAny<EventHandler>(), Times.Once())`, carries a doc comment stating the RC-1 invariant, and uses explicit Arrange/Act/Assert sections. MSTest and Moq per policy; deterministic; no external dependencies; no temporary files. It is the correct minimal shape: it pins exactly the one behavior RC-1 lacked, leaving the sibling test to pin the other sixteen.

**Docs (`spec.md` +45/-2; handoff record +82/-0; remediation plan checkbox flips only).** The spec amendment is a pure strengthening: the disposition-table row now covers the matching detachment, the #486 handoff criterion additionally requires the discharge addendum, the superseded risk row is marked superseded but retained as history, and the non-rename is documented with its reason. The handoff addendum is a pure append (original 115 lines byte-identical per the committed check, corroborated by the +82/-0 numstat) and flips the record from a live obligation to a discharged one via `ObligationDischargedInBranch: true`. The remediation plan diff is 24/-24, exclusively `[ ]` to `[x]` flips — verified by filtering the diff for any non-checkbox change (none).

## New defects introduced by the remediation

None found. Specifically checked: no behavioural change other than the added detachment; no formatting drift (csharpier check exits 0 on both files); no analyzer or nullable regression (5/0 warnings/errors both rebuilds, 0 CS86xx); no test-count regression (1121 -> 1122 scoped, 6741 -> 6742 repo-wide, 0 failed/skipped both); no scope creep (25-path set identical, no `.csproj` change); no evidence-hygiene violation (zero host tokens across all committed rem1 evidence, TRX sanitised in entity form and strict-parsed).

## Verdict

**Approve.** RC-1 is cured with the minimal correct fix and a genuine RED-first regression test. The delta introduces no new findings above Info level; the only new record, RCV-1, is a documented and acceptable naming staleness on a merged sibling's test.
