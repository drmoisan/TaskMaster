# Reconciliation of Two Independent Phase 6 Evidence Sets

Timestamp: 2026-08-27T14-27
Task: not a plan task; recorded because the audit trail would otherwise be ambiguous
Command: comparison of the two Phase 6 artifact sets present in `evidence/qa-gates/`
EXIT_CODE: 0

## What happened

Two independent Phase 6 toolchain runs against this worktree produced two parallel sets of gate
artifacts. Both are retained. Neither is deleted, because neither is wrong and each was written by a
different actor.

| Set | Filename suffix | Written by | Actual write time (UTC) |
| --- | --- | --- | --- |
| A | `2026-08-27T10-23.md` | the parent epic-orchestrator session's own Phase 6 re-run | 14:23:41Z to 14:24:20Z |
| B | `2026-08-27T14-18.md` / `2026-08-27T14-19.md` | this resumed child orchestrator session | 14:19:06Z to 14:25:42Z |

Set A's filename label is **local time, not UTC**: `10-23` local corresponds to `14-23` UTC on this
host, which runs at UTC-4. Set A also attributes the analyzer gate to `[P6-T2]` and the nullable gate
to `[P6-T3]`, whereas the plan places the repository-wide CSharpier check at [P6-T2], the analyzer
gate at [P6-T3] and the nullable gate at [P6-T4]. Both are labelling defects in set A, not
measurement defects. Set B uses UTC labels and the plan's task numbering.

Set B is the set of record for plan reconciliation, because its labels and task attributions match
the plan. Set A is retained as independent corroboration.

## Where the two sets agree

| Measurement | Set A | Set B | Agreement |
| --- | --- | --- | --- |
| Total tests | 6701 | 6701 | identical |
| Passed | 6701 | 6701 | identical |
| Failed | 0 | 0 | identical |
| CSharpier check | exit 0, 1540 files | exit 0, 1540 files | identical |
| Analyzer gate | exit 0, 0 errors, 5 warnings | exit 0, 0 errors, 5 warnings | identical |
| Analyzer non-vacuity | 0 `Skipping target "CoreCompile"` | 0 `Skipping target "CoreCompile"`, 51 `CoreCompile:` | identical, set B adds the positive count |
| Nullable gate | exit 0, 0 errors, 0 `CS86xx` | exit 0, 0 errors, 0 `CS86xx` | identical |
| Nullable non-vacuity | 0 `Skipping target "CoreCompile"` | 0 `Skipping target "CoreCompile"`, 54 `CoreCompile:` | identical, set B adds the positive count |
| `line-rate` | 85.1255% | 85.1255% | identical |
| `lines-covered` / `lines-valid` | 54379 / 63881 | 54379 / 63881 | identical |
| The formerly failing re-entrancy test | passes | passes | identical |
| The 5 warnings | System.Reactive `packages.config`, 5 projects | same | identical |

## The one figure that differs, and why

| Measurement | Set A | Set B | Difference |
| --- | --- | --- | --- |
| `branches-covered` | 12924 | 12927 | +3 |
| `branches-valid` | 16320 | 16320 | 0 |
| `branch-rate` | 79.1912% | 79.2096% | +0.0184 |

Three branches out of 16320. This is the known covered-count nondeterminism of this repository's
coverage collector: repeated runs over an identical tree drift by a few counted units while the
`*-valid` denominators stay fixed. It is not a source difference — the two runs measured the same
commit — and it does not move either figure across any threshold. Both readings clear the 75 percent
branch floor in `.claude/rules/quality-tiers.md` by more than four percentage points.

The line figures did not drift at all between the two runs, which is why the line-rate is quoted
identically in both sets.

## Set A's additional finding, retained

Set A records two earlier attempts of its own that this session did not observe and does not claim:

- an attempt that aborted with `MSTest with coverage failed with exit code -1` after 1064 passing
  tests, writing no coverage file and failing no test;
- an attempt that completed 6701 tests with 3 failures, all in
  `UtilitiesCS.Test.HelperClasses.FileIO2_Tests`, all with
  `System.IO.IOException: The process cannot access the file ...\TestData\FileIO2\sample.csv`,
  attributed to a residual handle from the aborted attempt and cleared by re-running that assembly
  alone under `/InIsolation`.

Set A also records that its CSharpier check initially reported
`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` as unformatted on line endings only, because
the one-line `_isExecuting` edit was applied with `sed -i`, which rewrote the file with LF endings.
That finding was resolved before commit `889fa298`: set B's repository-wide check at
2026-08-27T14:17:23Z reports 1540 files with zero unformatted, so no line-ending defect survives in
the tree being merged.

Both of set A's failure modes are consistent with this session's own experience of test-host
contention in this worktree (see
`evidence/qa-gates/toolchain-loop.2026-08-27T14-18.md`), and none of them is a defect in the change
under test.

## Concurrency note

The two sets were written within four minutes of each other, so two actors were writing this feature
folder concurrently. No file was written by both: the two sets occupy disjoint filenames, and a
byte-comparison of every shared measurement is tabulated above. This is recorded rather than
smoothed over, because an auditor reading `evidence/qa-gates/` will otherwise find two Phase 6 gate
sets with no explanation of which is authoritative.
