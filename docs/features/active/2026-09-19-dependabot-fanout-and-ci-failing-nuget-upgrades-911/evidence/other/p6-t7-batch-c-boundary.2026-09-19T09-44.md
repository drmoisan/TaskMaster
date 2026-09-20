# P6-T7 — Batch C budget boundary

Timestamp: 2026-09-19T09-44

Command:

```
git -C "<execution-worktree-root>" show --name-only --format= 6b2426689eaece9bdd79998d9b9b9880fb0f9991
```

EXIT_CODE: 0

The measurement is taken from the batch's **own commit**, using the same two counts and the
same derivation P2-T9 defines:

- production = paths matching `scripts/**` with extension `.ps1`, `.psm1` or `.psd1` and
  not under `tests/`;
- test = paths matching `tests/**` with extension `.ps1`.

## Output Summary

```
TOTAL_PATHS=37
PROD_COUNT=3
TEST_COUNT=3
```

## Production members, exactly 3

```
scripts/dependencies/AnalyzerItemRepair.psm1
scripts/dependencies/ConsistencyVerifier.psm1
scripts/dependencies/ProjectConsistency.psm1
```

These are the three the plan names for Batch C, in a different order, which the acceptance
permits.

## Test members, exactly 3

```
tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1
tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1
tests/scripts/dependencies/ProjectConsistency.Tests.ps1
```

These are the three the plan names for Batch C, in a different order.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| Production count | exactly 3 | 3 |
| Test count | exactly 3 | 3 |
| Production members enumerated and asserted | the three named | all three present, no others |
| Test members enumerated and asserted | the three named | all three present, no others |
| No enumerated path is `scripts/dependencies/PackageGraph.psm1` | absent | 0 matches for `PackageGraph` anywhere in the commit |

The counts are **exact rather than bounded above**, and the members are asserted rather than
expected, for the reason P2-T9 sets out: an at-most-3 bound is satisfied by 0 and 0, so it
catches an overrun and misses an omission, and P6-T6's pathspec assertion does not close
that gap because a subset test over a pathspec set is satisfied by an empty commit.

Batch C sits at 3 of 3 production and 3 of 3 test slots with **no headroom**. That is why
the exactness matters here more than in any other batch: a fourth production file would
show in this count and nowhere else the hook would catch it. `PackageGraph.psm1` is the
specific file the Phase 5 prohibition names, and this is the commit-level form of that
prohibition.

## Hook state, recorded as an observation and asserted over by nothing

`.claude/state/powershell-batch-budget.default.json` exists and contains:

```json
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

The three entries are scratchpad scripts belonging to worktree `2026-08-23T22-51`, which
this session never touched, and `testFiles` is empty. **None of the six files this batch
wrote appears.** That is the behaviour the Measured Tree Facts row predicts:
`.claude/hooks/enforce-powershell-batch-budget.ps1` computes its root as
`Split-Path (Split-Path $PSScriptRoot -Parent) -Parent` and `settings.json:144` registers it
by a relative path that resolves against the **session** worktree, so every file this plan
writes lands out-of-root and is discarded with `permissionDecision = 'allow'`, no slot
consumed and `shouldWriteState = $false`.

An assertion over these arrays would therefore read the same whatever the batch did. Nothing
in this artifact asserts over them. The commit measurement above asserts the same per-batch
budget the hook nominally enforces, fails when a batch genuinely overruns, and does not
depend on a hook that cannot observe this worktree.

No task in this plan deleted or reset that state file, and neither
`CLAUDE_POWERSHELL_BUDGET_PROD` nor `CLAUDE_POWERSHELL_BUDGET_TEST` was set or raised.

## Preconditions the artifact records as satisfied

| Task | Requirement | Result |
|---|---|---|
| P6-T3 | `EXIT_CODE: 0` | 0 — `Passed=268 Failed=0`, aggregate line 93.85, the three new modules at 100.00, 100.00 and 98.74 |
| P6-T4 | `EXIT_CODE: 0` | 0 — union 35 paths, 0 C# compilation inputs |
| P6-T5 | `EXIT_CODE: 0` | 0 — `Checked 1623 files in 4380ms.`, no findings |
| P6-T2 | its own acceptance **as written**, not an exit code | satisfied — exactly 13 findings, every one a member of the P0-T17 baseline, 0 in files this change owns |
| P6-T6 | produced a commit | yes — `6b2426689eaece9bdd79998d9b9b9880fb0f9991`, 37 files |

P6-T2's stated expectation is `ok:false` and a non-zero exit while the residual baseline
findings stand, so its exit code is deliberately not the precondition; the exact-13
finding-set condition is.

P6-T1 is not listed above because it is a write-mode step rather than a gate: its result is
the rewrite count, which closed at 0 on its fourth pass with `REVERT-SET: empty`.

## Batch C is closed

| Batch | Production | Test | Commit |
|---|---|---|---|
| A | 1 | 1 | `48f0c710a` |
| B | 2 | 3 | `596e7a70c` |
| C | 3 | 3 | `6b2426689` |

Batch D remains, carrying `scripts/dependencies/Repair-PackageManifestConsistency.ps1` and
its suite plus the `DependabotConfig.Tests.ps1` extension, and begins at P7-T1.
