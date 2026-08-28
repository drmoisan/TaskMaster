# QA Gate — Post-commit scope lock (P9-T5)

Timestamp: 2026-08-27T23-39

Command: `git diff --name-only <ref>..HEAD` and `git log --format=%s <ref>..HEAD`

EXIT_CODE: 0

Output Summary: this feature writes exactly the twelve files on its owned list and nothing else. None
of the six forbidden paths appears in any range. HEAD is `2434f07fc57e590893adfdb3a7b81070eccc31bf`, different from `BASELINE_SHA`.

## Which reference range is authoritative, and why two are reported

The plan names `BASELINE_SHA..HEAD`. During this resumed run the branch merged the integration tip
`69e83171`, which brought in merged sibling features 493 and 444. `BASELINE_SHA..HEAD` therefore now
contains those siblings' files as well as this feature's, and CANNOT satisfy the plan's condition that
every `.cs` and `.csproj` entry appear on this feature's owned list. That is a consequence of the
mandatory base merge, not of scope creep.

Both ranges are reported so the reader can verify this directly:

- **Range A**, `origin/epic/quickfiler-bug-family-integration..HEAD` — the files THIS feature writes. This is the authoritative scope-lock range.
- **Range B**, `BASELINE_SHA..HEAD` — range A plus everything the merge brought in. 23 code files.

## Range A: every code file this feature writes

```
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs
QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs
QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs
QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs
QuickFiler/QuickFiler.csproj
QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs
QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs
QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs
QuickFiler/Viewers/BreadcrumbMessengerHub.cs
```

12 entries. Owned-list reconciliation:

| Owned file | Present in range A |
| --- | --- |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | yes |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | yes |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | yes |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | yes |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` | yes |
| `QuickFiler/QuickFiler.csproj` | yes |
| `QuickFiler.Test/QuickFiler.Test.csproj` | yes |

- Entries in range A that are NOT on the owned list: **0** 
- Owned files not written by this feature: **0** 

Range A equals the owned list exactly. The scope lock holds.

## Forbidden paths

Checked against BOTH ranges, so a forbidden write hidden inside the merge would still be caught:

| Forbidden path | Status |
| --- | --- |
| `QuickFiler/Viewers/WebView2Messenger.cs` | absent |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | absent |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | absent |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | absent |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | absent |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | absent |

Total violations: **0**. AC-26 holds.

## Commit subjects in `BASELINE_SHA..HEAD`

```
fix(breadcrumb): enforce close, lifetime, broadcast and lease invariants
Merge remote-tracking branch 'origin/epic/quickfiler-bug-family-integration' into bug/breadcrumb-coordinator-hub-defects-501
wip(501): preserve in-progress breadcrumb coordinator hub work
Merge pull request #654 from drmoisan/bug/quickfiler-keyboard-action-defects-444
docs(444): record post-merge toolchain gate and close the final three criteria
Merge origin/epic/quickfiler-bug-family-integration into bug/quickfiler-keyboard-action-defects-444
docs(444): record feature-review policy, code and feature audit artifacts
docs(444): record terminal clean-tree verification
docs(444): reconcile acceptance criteria and record deferred dispositions
docs(444): check off the toolchain and coverage acceptance criteria
chore(444): record the final toolchain and coverage evidence
Merge pull request #653 from drmoisan/bug/quickfiler-test-uithread-dispatcher-493
docs(quickfiler): re-verify #493 plan tasks and ACs against ground truth
test(quickfiler): re-verify #493 toolchain against the moved epic base
Merge remote-tracking branch 'origin/epic/quickfiler-bug-family-integration' into bug/quickfiler-test-uithread-dispatcher-493
Merge origin/epic/quickfiler-bug-family-integration into bug/quickfiler-keyboard-action-defects-444
docs(444): record P4-T1 final-loop formatting gate evidence
docs(quickfiler): record #493 feature-review audit artifacts
docs(482): check off the issue 482 acceptance criteria
fix(482): route expansion registration through a single owner
docs(472): check off the issue 472 acceptance criteria
fix(472): unregister navigation keys at the width they were registered
docs(quickfiler): capture the #493 R-1 residual as issue #648
chore(agent-memory): record #493 msbuild-log gate and epic-base staleness lessons
docs(quickfiler): check off the final #493 plan task
docs(444): check off the issue 444 acceptance criteria
docs(quickfiler): record #493 acceptance criteria and Phase 4-5 evidence
fix(444): guard the KbdActions enumerable constructor against duplicate registrations
test(quickfiler): funnel UiThread dispatcher mutations through a shared fixture (#493)
docs(444): capture phase 0 baselines and verify upstream 468
docs(444): promote count-mismatch follow-up defect as issue #644
```

Subjects referencing `#501` or `(501)`: **1**, so the AC-26 requirement that at least one
commit subject reference `#501` is satisfied. The remaining subjects belong to merged siblings 493 and
444 and arrived through the merge commit.

## Files outside the code scope

This feature also writes Markdown and one Cobertura XML under `docs/features/active/breadcrumb-coordinator-hub-defects-501/`, plus two promoted
follow-up records under `docs/features/potential/promoted/`. The plan's scope lock constrains `.cs`
and `.csproj` entries only, so these are outside its gate; they are listed in the handoff index of the
same timestamp.
