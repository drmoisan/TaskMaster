# triage-multiselect-only-first (Issue #183)

- Date captured: 2026-06-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/triage-multiselect-only-first/ (Issue #183)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #183
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/183
- Last Updated: 2026-06-10
- Work Mode: minor-audit

## Summary

When multiple emails are highlighted in Outlook and a Triage button (Set A/B/C) is clicked, the triage level is applied to only the first selected item instead of all selected items.

## Environment

- OS/version: Windows / Outlook desktop (VSTO add-in)
- Python version: N/A (C# / .NET Framework VSTO add-in)
- Command/flags used: Ribbon buttons `TriageSetA_Click` / `TriageSetB_Click` / `TriageSetC_Click`
- Data source or fixture: Live Outlook Explorer selection

## Steps to Reproduce

1. In Outlook, highlight a group of emails that belong to the same conversation/thread.
2. Click a Triage ribbon button (for example, "Set A").
3. Inspect the `Triage` user-defined field (UDF) on each highlighted email.

## Expected Behavior

The selected triage level is written to the `Triage` UDF of every highlighted email.

## Actual Behavior

Only the first item in the selection receives the `Triage` UDF value. Remaining items that share a `ConversationID` are skipped.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: N/A

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Root cause is in `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs`, method `TrainSelectionAsync`. The selection pipeline deduplicates by `ConversationID`:

```csharp
.GroupBy(m => m.ConversationID)
.Select(g => g.First())
```

This dedup was introduced for issue #137 so the Bayesian classifier is trained only once per conversation when Outlook conversation view auto-selects an entire thread. However, the same loop also performs the user-visible action — writing the `Triage` UDF via `TestActionAsync` -> `MailItem.SetUdf("Triage", …)`. As a result, the UDF write is incorrectly suppressed for every item after the first in a conversation.

The two concerns must be decoupled:
- Writing the `Triage` UDF must apply to every selected `MailItem`.
- Training the classifier should continue to dedup by `ConversationID` (preserve #137 behavior).

## Acceptance Criteria

- [x] AC1: When `TrainSelectionAsync` is invoked with a selection containing multiple `MailItem` objects that share the same `ConversationID`, the `Triage` user-defined field is written (`SetUdf("Triage", triageId)`) to every selected `MailItem`, not only the first.
- [x] AC2: Training deduplication from issue #137 is preserved: the Bayesian classifier is trained at most once per distinct `ConversationID`, so `TotalEmailCount` and `MatchEmailCount` increment exactly once for a multi-item single-conversation selection.
- [x] AC3: A deterministic MSTest regression test in `Triage_OlLogicTests` proves AC1 (UDF written to all same-conversation items) and the existing #137 training-dedup tests continue to pass unchanged.
- [x] AC4: The fix is confined to the triage selection path (`Triage_OlLogic.cs` and its test file); no unrelated production behavior changes.
- [x] AC5: The full C# toolchain (CSharpier format, .NET analyzer build, nullable/TreatWarningsAsErrors build, MSTest with coverage) passes in a single clean pass; changed-line coverage does not regress.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `Triage_OlLogicTests` — add a regression test asserting the `Triage` UDF is written to all selected items sharing a `ConversationID`, while training increments `TotalEmailCount`/`MatchEmailCount` only once (preserving the existing #137 tests).
- [ ] Integration scenario to retest: Manual multi-select triage in Outlook.
- [ ] Manual verification notes: Confirm every highlighted email shows the chosen triage value.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch