# P4-T8 — Batch B budget boundary, measured from the batch's own commit

Timestamp: 2026-09-20T01-30

Command:

```
git -C <W> show --name-only --format= 596e7a70c78443861576f21a572bd2a919f02c66
```

with the two counts derived from the returned path list by the same rule P2-T9 defines: the
production count is the number of paths matching `scripts/**` with extension `.ps1`, `.psm1` or
`.psd1` and not under `tests/`; the test count is the number matching `tests/**` with extension
`.ps1` or ending `.Tests.ps1`.

EXIT_CODE: 0

## Measurement

```
COMMIT_PATH_COUNT=26
PRODUCTION_COUNT=2
  PROD: scripts/dependencies/PackageCompatibility.psm1
  PROD: scripts/vscode/Sync-PackageReferences.ps1
TEST_COUNT=3
  TEST: tests/scripts/dependencies/DependabotConfig.Tests.ps1
  TEST: tests/scripts/dependencies/PackageCompatibility.Tests.ps1
  TEST: tests/scripts/vscode/Sync-PackageReferences.Tests.ps1
```

## Enumerated members against the expected members

| Kind | Expected member | Present |
|---|---|---|
| Production | `scripts/dependencies/PackageCompatibility.psm1` | yes |
| Production | `scripts/vscode/Sync-PackageReferences.ps1` | yes |
| Test | `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` | yes |
| Test | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | yes |
| Test | `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | yes |

Two production and three test members, matching the Batch B row of the plan's batch table exactly
and with no additional PowerShell path of either kind in the commit.

Batch B is at 2 of 3 production slots and **3 of 3 test slots**. The test side has no headroom,
which is the same condition Batch C will be in on both sides.

## Why the counts are exact rather than bounded above

An at-most-3 bound is satisfied by 0 and 0, so it catches an overrun and misses an omission, and
P4-T7's assertion does not close that gap: it asserts only that `git show --name-only` lists paths
drawn from its pathspec set, which an empty commit also satisfies. Exact counts catch both
directions. This is the reasoning P2-T9 sets out, applied unchanged.

## Hook state, recorded as an observation and asserted over by nothing

```
HOOK_STATE_FILES=1
  powershell-batch-budget.default.json ::
  {
    "prodCap": 3,
    "testCap": 3,
    "prodFiles": [
      "<temp>/C--Users-DanMoisan-repos-TaskMaster-wt-2026-08-23T22-51/.../scratchpad/run-vstest.ps1",
      "<temp>/C--Users-DanMoisan-repos-TaskMaster-wt-2026-08-23T22-51/.../scratchpad/postrebase_verify.ps1",
      "<temp>/C--Users-DanMoisan-repos-TaskMaster-wt-2026-08-23T22-51/.../scratchpad/run-toolchain-442.ps1"
    ],
    "testFiles": []
  }
```

The one state file present names three scratchpad scripts belonging to a **different worktree**
session, `2026-08-23T22-51`, and carries an empty `testFiles` array. It records nothing this batch
did: neither of the two production files nor any of the three test files this commit carries
appears in it, and the file was not updated by any write this phase performed.

That is exactly the behaviour the Measured Tree Facts row predicts.
`.claude/hooks/enforce-powershell-batch-budget.ps1` computes its root as
`Split-Path (Split-Path $PSScriptRoot -Parent) -Parent`, and `settings.json:144` registers it by a
relative path resolving against the **session** worktree, so every file this plan writes into the
**execution** worktree is out-of-root and is discarded at lines 277-282 with
`permissionDecision = 'allow'`, no slot consumed and `shouldWriteState = $false`. An assertion
over those arrays would read stale foreign content whatever this batch did, which is why the
commit measurement supersedes it. The arrays are observed here and asserted over by nothing.

Raising `CLAUDE_POWERSHELL_BUDGET_PROD` or `CLAUDE_POWERSHELL_BUDGET_TEST` is not authorised
anywhere in this plan and was not done. No task deleted or reset the state file.

## Preconditions, recorded as satisfied

| Task | Requirement | Observed |
|---|---|---|
| P4-T3 | `EXIT_CODE: 0` | 0, `Passed=227 Failed=0`, aggregate LINE 92.08 |
| P4-T4 | `EXIT_CODE: 0` | 0, `Checked 1623 files in 4506ms.`, 0 findings |
| P4-T5 | `EXIT_CODE: 0` | 0, stdout 0 bytes, 8 workflow files enumerated independently |
| P4-T6 | `EXIT_CODE: 0` | 0, union 25 paths, 0 C# compilation inputs |
| P4-T2 | its own acceptance **as written**, which is the exact-13 finding-set condition and **not** an exit code | total exactly 13, all 13 members of the P0-T17 baseline, 0 findings in the seven owned files; the task's stated expectation is `ok:false` and a non-zero exit while the residual baseline findings stand, and EXIT_CODE 1 was observed |
| P4-T7 | produced a commit | `596e7a70c78443861576f21a572bd2a919f02c66`, 26 files |

P4-T1 is not in the precondition list the task names, and is recorded here for completeness: it
ran twice, rewrote 2 of 38 files in round 1 and 0 of 38 in round 2, and derived an empty revert
set in both rounds.

## Working-tree state at this task

`git status --porcelain --untracked-files=all` was empty immediately after the P4-T7 commit. It is
non-empty now and expected to be: this phase's two closing tasks write the P4-T7 artifact, this
artifact, and their two plan check-offs, none of which any pathspec authorises a commit for until
the Phase 6 boundary.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| Production count from the batch commit | exactly 2 | 2 | PASS |
| Production members | `PackageCompatibility.psm1`, `Sync-PackageReferences.ps1` | both, and no other | PASS |
| Test count from the batch commit | exactly 3 | 3 | PASS |
| Test members | `PackageCompatibility.Tests.ps1`, `DependabotConfig.Tests.ps1`, `Sync-PackageReferences.Tests.ps1` | all three, and no other | PASS |
| Hook-state arrays recorded as observation only | not asserted over | recorded; nothing asserted against them | PASS |
| P4-T3 through P4-T6 returned `EXIT_CODE: 0` | all four | all four | PASS |
| P4-T2 satisfied its own acceptance as written | exact-13 finding set | satisfied | PASS |
| P4-T7 produced a commit | yes | `596e7a70…` | PASS |

Output Summary: measured from the Batch B commit `596e7a70c78443861576f21a572bd2a919f02c66`, which
carries 26 paths, the production PowerShell count is **exactly 2** —
`scripts/dependencies/PackageCompatibility.psm1` and `scripts/vscode/Sync-PackageReferences.ps1` —
and the test PowerShell count is **exactly 3** —
`tests/scripts/dependencies/PackageCompatibility.Tests.ps1`,
`tests/scripts/dependencies/DependabotConfig.Tests.ps1` and
`tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`. Both match the plan's Batch B row with no
additional PowerShell path of either kind. The batch is at 2 of 3 production and 3 of 3 test
slots. The single hook state file names three scratchpad scripts from an unrelated worktree
session and an empty `testFiles` array, confirming the hook observed nothing this batch did; it is
recorded as an observation and asserted over by nothing. P4-T3 through P4-T6 all returned
EXIT_CODE 0, P4-T2 satisfied its exact-13 finding-set condition with the expected `ok:false`, and
P4-T7 produced the commit.
