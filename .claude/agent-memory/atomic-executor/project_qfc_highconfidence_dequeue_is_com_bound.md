---
name: qfc-highconfidence-dequeue-is-com-bound
description: A QfcDatamodel dequeue test with HighConfidenceModeEnabled=true reaches live Outlook COM unless a scoring seam is injected first; order the seam task before the test task.
metadata:
  type: project
---

`QfcDatamodel.DequeueNextItemGroupAsync` with `_globals.QfSettings.HighConfidenceModeEnabled == true`
routes through `DequeueWithHighConfidenceGateAsync`, which hard-wires `ScoreRemainingQueueMailItemAsync`
as the gate's score loader. That method constructs `new FolderScoringService()`, whose `ScoreAsync`
calls `MailItemHelper.FromMailItemAsync` plus `FolderPredictor` — live Outlook COM, prohibited by
`.claude/rules/general-unit-test.md` UT4.

**Why:** the only pre-existing datamodel test in `QfcQueuePurePathsTests.cs` deliberately pins
`HighConfidenceModeEnabled = false` for exactly this reason. A plan that adds a high-confidence
datamodel regression test without first introducing an injectable scoring seam
(`internal Func<IFolderScoringService> ScoringServiceFactory`) produces a test that cannot run
deterministically.

**How to apply:** when reviewing or executing a QuickFiler queue/datamodel plan, check that any task
exercising the high-confidence dequeue path is ordered AFTER the task that introduces the scoring
seam. `IFolderScoringService.ScoreAsync` already returns `(long Score, string TopFolder)`, and
`FolderScoringService` is `[ExcludeFromCodeCoverage]` at `QfcHighConfidencePreFilter.cs:166`, so the
seam is a mock target, not new surface. Note also that `CreateUninitializedDatamodel`
(`FormatterServices.GetUninitializedObject`) skips field initializers, so an auto-property seam is
null until the test assigns it. Related: [[project_qfc_backgroundworker_async_void_race]].
