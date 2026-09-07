# [P5-T1] Branch file list

Timestamp: 2026-08-27T20-06
Command: `$mb = git merge-base HEAD origin/epic/quickfiler-bug-family-integration` then `git diff --name-only "$mb..HEAD"`
EXIT_CODE: 0
Output Summary: 82 changed paths. The production `.cs` partition holds exactly the three expected
paths; the project-file partition holds exactly `QuickFiler.Test/QuickFiler.Test.csproj` and
`QuickFiler/QuickFiler.csproj` is absent.

The merge base is bound to a variable and interpolated inside a quoted string. PowerShell parses a
subexpression followed by adjacent bare text as two tokens rather than concatenating them, so the
inline `$(git merge-base ...)..HEAD` spelling would not have produced a single revision-range
argument.

## Re-derived merge base

| Item | Value |
| --- | --- |
| `git merge-base HEAD origin/epic/quickfiler-bug-family-integration` (re-derived now) | `4f238289090e4c97ca505511a5a73e8092dce0f9` |
| Value captured at `[P0-T6]` | `125c36b0669d9dd6095f156901bba138e2272f56` |
| Do they agree? | **No** |

The two do **not** agree, and that is expected rather than a defect. After Phase 3 completed, the
epic orchestrator merged the integration branch tip `4f238289` into this feature branch, which
advanced the merge base from the original branch point to that tip. The merge commit is
`3f373f5b Merge origin/epic/quickfiler-bug-family-integration into bug/quickfiler-keyboard-action-defects-444`.
Using the re-derived base is the correct choice for a scope gate: it excludes the sibling feature's
own changes (feature 442's `EfcHomeController*` and `QfcHomeController*` work), which arrived
through that merge and are not this feature's diff. Had the stale `[P0-T6]` base been used, those
sibling paths would have appeared in this file list and the scope gates would have reported
violations this feature did not commit.

## Partition 1 — production `.cs` paths (3)

| Path |
| --- |
| `QuickFiler/Controllers/KbdActions.cs` |
| `QuickFiler/Controllers/QfcCollectionController.cs` |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` |

Exactly the three paths the acceptance condition names:
`QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/Controllers/QfcCollectionController.cs`, and
`QuickFiler/Controllers/QfcItemController.Navigation.cs`.

## Partition 2 — test `.cs` paths (4)

| Path |
| --- |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` |
| `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` |

`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` is the one file this
feature creates; the other three are pre-existing files it appends tests to.
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is absent, as `[P2-T11]` requires.

## Partition 3 — project files (1)

| Path |
| --- |
| `QuickFiler.Test/QuickFiler.Test.csproj` |

Exactly the single path `QuickFiler.Test/QuickFiler.Test.csproj`. **`QuickFiler/QuickFiler.csproj`
is absent**, as the epic-wide project-file discipline requires: that file is shared with three
concurrently-live sibling features and this feature holds only the `Controllers\Qfc*` region of the
test project.

## Partition 4 — documentation and evidence paths (74)

| Path |
| --- |
| `.claude/agent-memory/orchestrator/MEMORY.md` |
| `.claude/agent-memory/orchestrator/potential-to-issue-keeps-only-summary-section.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t10-roslynator.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t11-tool-restore.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t12-upstream-468-verification.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t13-controller-anchors.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t14-digits-read-baseline.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t15-csproj-anchors.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t16-csharpier-check.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t17-analyzer-baseline.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t18-nullable-baseline.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t19-build.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t20-coverage-baseline.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t21-file-metrics.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t22-nav-tests-baseline.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t6-environment.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t7-dotnet-sdk.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t8-nuget-restore.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/p0-t9-meziantou.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/phase0-instructions-read.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/other/p1-t30-logger-review.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/other/p3-t28-unread-mechanism.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/fail-before-444.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/fail-before-472.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/fail-before-482.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p1-t19-format.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p1-t20-size.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p1-t3-444-red.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p1-t6-444-green.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t11-frozen-test-file.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t12-interface-untouched.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t13-format.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t14-size.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t3-472-red.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t7-472-green.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t8-digits-zero-read.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t9-format-selection.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p3-t14-single-owner.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p3-t15-signature-retention.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p3-t16-format.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p3-t17-size.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p3-t27-ac482-08.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p3-t3-482-red.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p3-t8-482-green.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t1-format.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t10-kbdactions-coverage.2026-08-27T19-59.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t11-coverage-delta.2026-08-27T20-00.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t12-clean-pass.2026-08-27T20-00.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t2-format-check.2026-08-27T19-48.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t3-size-audit.2026-08-27T19-49.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t4-analyzers.2026-08-27T19-50.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t5-typecheck.2026-08-27T19-51.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t6-final-tests.2026-08-27T19-53.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t6/p4-t6-final.trx` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t7-trx-hygiene.2026-08-27T19-56.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t8-coverage-final.2026-08-27T19-58.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t9-syncexpanded-coverage.2026-08-27T19-58.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/fail-before-exception.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p1-t10-build.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p1-t11-kbdactions-suite.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p1-t14-build.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p1-t15-keysdown-pin.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p1-t16-keysdown-binding.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p1-t2-build.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p1-t5-build.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p2-t10-nav-tests.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p2-t6-build.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p3-t12-build.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p3-t13-482-suite.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p3-t2-build.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p3-t7-build.2026-08-27T09-45.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/plan.2026-08-24T20-33.md` |
| `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md` |
| `docs/features/potential/promoted/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan.md` |

Two of these sit outside the feature folder and are called out explicitly:

- `.claude/agent-memory/orchestrator/MEMORY.md` and
  `.claude/agent-memory/orchestrator/potential-to-issue-keeps-only-summary-section.md` are
  orchestrator-authored memory records that entered the range through commit
  `12256da4 docs(444): promote count-mismatch follow-up defect as issue #644` and the merge commit.
  They are not source, test, or project files and carry no product behaviour.
- `docs/features/potential/promoted/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan.md`
  is the promoted potential entry for the `UnregisterNavigation` count-mismatch follow-up defect,
  which became GitHub issue #644.

## Acceptance

- The artifact records the re-derived merge base — met (`4f238289090e4c97ca505511a5a73e8092dce0f9`).
- It states whether that equals the `[P0-T6]` value — met (it does not; the reason is recorded above).
- The production `.cs` partition holds exactly the three named paths — met.
- The project-file partition holds exactly `QuickFiler.Test/QuickFiler.Test.csproj`, with
  `QuickFiler/QuickFiler.csproj` absent — met.
