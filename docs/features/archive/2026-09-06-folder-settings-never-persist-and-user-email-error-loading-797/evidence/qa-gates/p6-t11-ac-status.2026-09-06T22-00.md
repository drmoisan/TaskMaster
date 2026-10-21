# P6-T11 — Acceptance-criteria Status Summary (Issue #797)

Timestamp: 2026-09-07T10-16

## Acceptance Criteria Status

- Source: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md (authoritative under full-bug work mode), mirrored in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md
- Total AC items: 8
- Checked off (delivered): 7
- Remaining (unchecked): 1
- Items remaining: AC3 — "A value saved in Folder Settings is present after an Outlook restart (manual verification)."

## Per-criterion detail

| AC | Status | Implementing task | Verifying test or procedure | Evidence artifact |
|---|---|---|---|---|
| AC1 | PASS | P2-T1 | `LoadStoresAsync_WhenConfigDeserializesToNull_FreshWrapperAdoptsLoaderDiskConfiguration`, with `LoadStoresAsync_WhenConfigKeyIsAbsent_FreshWrapperKeepsEmptyDiskPath` as the negative case | evidence/regression-testing/p2-t5-root-cause-1-green.2026-09-06T22-00.md |
| AC2 | PASS | P2-T2 | `Serialize_WithEmptyDiskPath_LogsErrorAndArmsNoTimer`, `Serialize_WithNullDiskPath_LogsErrorAndArmsNoTimer` | evidence/regression-testing/p2-t5-root-cause-1-green.2026-09-06T22-00.md |
| AC3 | NOT MET — BLOCKED-MANUAL | none; not automatable | nine-step manual procedure requiring a live Outlook restart | evidence/other/p6-t1-ac3-manual-verification.2026-09-06T22-00.md, with the fail-before exception dossier at evidence/regression-testing/fail-before-exception.2026-09-06T22-00.md |
| AC4 | PASS | P2-T3 and P2-T4 | `SerializeNow_WithConfiguredPath_WritesWithoutFiringTimer`, with `Serialize_WithConfiguredPath_StillRequiresTimerFireToWrite` pinning the unchanged deferred path | evidence/regression-testing/p2-t5-root-cause-1-green.2026-09-06T22-00.md |
| AC5 | PASS | P4-T1 and P4-T2 | `PersistJunkFolderSelections_WhenGlobalsAreNotTheTypedSink_LogsErrorAndDoesNotInvoke`, `PersistJunkFolderSelections_PassesJunkCertainPathFirst`, and the retargeted `PersistJunkFolderSelections_WhenApplyMethodIsMissing_DoesNotThrow` | evidence/regression-testing/p4-t7-remaining-criteria-green.2026-09-06T22-00.md |
| AC6 | PASS | P3-T1, P3-T2 and P3-T3 | the four `GetSmtpAddressFromStore_*` fallback cases, `RefreshUserEmailAddress_WhenRootFolderIsNull_ReturnsNullAndDoesNotThrow`, and the three `PopulateWithCurrent_When*` retry cases | evidence/regression-testing/p3-t4-root-cause-2-green.2026-09-06T22-00.md |
| AC7 | PASS | P4-T3 | the six `TrimStorePrefix_*` pure-function cases and `PopulateWithCurrent_RendersInboxAndRootFolderWithoutStorePrefix` | evidence/regression-testing/p4-t7-remaining-criteria-green.2026-09-06T22-00.md |
| AC8 | PASS | P4-T4 and P4-T5 | `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText` (inverted under D6), `PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow`, `GetRelativeFsPath_WithNullCurrent_ReturnsPlaceholderAndDoesNotThrow` | evidence/regression-testing/p4-t7-remaining-criteria-green.2026-09-06T22-00.md |

## The AC3 branch, recorded explicitly

P6-T4 conditions the AC3 check-off on the P6-T1 artifact recording `AC3-RESULT: PASS`. That artifact
records `AC3-RESULT: BLOCKED-MANUAL`: this execution environment has no live Outlook host and the
executing agent is directed not to start one, load the add-in, or drive any user interface, so all
nine procedure steps are recorded as NOT PERFORMED with their reasons and no result is fabricated.
The AC3 checkbox is therefore left unmarked in both spec.md and issue.md, and the plan outcome for
that one criterion is remediation-required rather than complete: the criterion is handed to the
maintainer for manual verification. Every other criterion is genuinely verified by automated test, and
AC3 being blocked was not used to soften any of them.

## Final coverage figures, restated from the P5-T7 artifact

- BASELINE_LINE_PERCENT=53.23
- POSTCHANGE_LINE_PERCENT=53.26
- CHANGED_LINE_PERCENT=91.09

Rule R9 selected the comparable branch: the baseline `lines-valid` of 83466 and the post-change
`lines-valid` of 83537 differ by 71 lines, 0.085 percent of the baseline, inside the 5 percent
tolerance. Post-change line coverage is not below the baseline, so the no-regression rule is
satisfied. The changed-line figure of 91.09 percent, computed over the executable changed lines of
every measurable file with the relocated-unmodified lines excluded from the denominator, is at or
above the 90 percent figure CLAUDE.md requires of new and changed code. Both percentages sit below
CLAUDE.md's 80 percent repository-wide floor; that is a pre-existing condition under the narrower
two-assembly scope this plan measures, which this change neither creates nor resolves.

## Excluded test classes

Four shell-icon test classes are excluded from every local run in this plan for environmental reasons
unrelated to this change: `HelperClasses.ShellUtilities_Tests`,
`HelperClasses.ShellUtilitiesStatic_Tests`, `HelperClasses.SysImageListHelperTests` and
`EmailIntelligence.OSBrowser_Tests`. They stall vstest on this workstation. CI covers them.

## Declared expectation changes

1. **The D6 inversion.** `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText`, in
   UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs, previously
   asserted that a null current store throws a `NullReferenceException`, contradicting its own name.
   AC8 changes that behaviour, so the assertion is inverted to require that the act does not throw and
   that the archive and junk labels carry their existing placeholder literals. The test name is
   unchanged and the new assertion is stricter than the original: it pins specific rendered values
   rather than merely an exception type. This is a declared expectation change, not a weakened test.
2. **The P3-T1 arrangement correction: none was required.** P3-T1 required the two pre-existing tests
   `GetSmtpAddressFromStore_WhenExchangeUserIsUnavailable_ReturnsNull` and
   `GetSmtpAddressFromStore_WhenExchangeLookupThrowsComException_ReturnsNull` to be re-derived against
   the new fallback ordering. Both supply neither an at-sign-bearing address-entry address nor an
   at-sign-bearing display name, so both still return null and both passed unchanged. No arrangement
   was corrected and no second expectation change arises.

A second, non-weakening retarget accompanies AC5: the reflection-era globals doubles in the store
controller tests are retargeted to the typed sink. The negative test asserting the
missing-implementation path does not throw is retargeted rather than deleted, so the loud-failure
branch retains coverage.

## Session helper

The session-scoped helper at coverage/plan797-helpers.ps1 was deleted before the commit, per rule R2.
It was created in Phase 0, rewritten in place as later tasks required, never committed, and lived in a
git-ignored directory throughout, so it satisfies the general code change policy's exemption for a
script created and deleted within an agent session and was never a production PowerShell file.

## TERMINAL-PORCELAIN

Verbatim output of `git status --porcelain --untracked-files=all`, observed immediately after the
P6-T11 commit:

```text
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/prd-feature/feedback_backticked_paths_are_the_change_footprint.md
 M .claude/agent-memory/task-researcher/MEMORY.md
?? .claude/agent-memory/atomic-planner/project_797_folder_settings_persistence_plan_seams.md
?? .claude/agent-memory/task-researcher/project_folder_settings_persistence_797.md
```

Residual classification, one entry per line above:

- All five lines belong to the agent-memory residual class, the second of the two classes the gate
  admits. They are pre-existing modifications and additions left by the preparation subagents before
  execution began, they are outside this item's Write Set, and this execution neither edited, reverted
  nor committed them. Every path staged for the commit was named explicitly; no `git add -A` and no
  `git add .` was used.
- No other path is present, so the gate passes. In particular the session helper is absent, because it
  was deleted before the commit and lived in a git-ignored directory throughout.

This section was written as an explicit placeholder before the commit and replaced with the observed
output afterwards; no value was predicted. The replacement is folded into the same commit by an
amend, which does not change the residual set, so the listing above remains accurate for the
post-commit state. No commit hash is quoted here, because an amend rewrites it and a quoted hash
would immediately become stale.

The first admitted residual class, this plan file, is absent from the listing above because the plan
file was committed with its P6-T1 through P6-T10 check-offs already applied. Its P6-T11 check-off is
written after the commit, at which point it becomes the second residual, exactly as the gate
anticipates.

Output Summary: Seven of eight acceptance criteria are delivered and checked off in both spec.md and
issue.md. AC3 is BLOCKED-MANUAL and left unmarked, handed to the maintainer with a nine-step
procedure. Changed-line coverage is 91.09 percent and document-level coverage did not regress. The
terminal porcelain listing holds only agent-memory residuals.
