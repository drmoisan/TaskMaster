# [P13-T7] Full `QuickFiler.Test` suite after the issue #474 defect 2 readiness tests

Timestamp: 2026-08-26T16-20

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /Logger:"trx;LogFileName=p13-t7.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p13-t7
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 1024  Passed: 1024`. Total time 10.09 s, first attempt, no
retry, no test-host crash.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p13-t7/p13-t7.trx`:

```
total="1024" executed="1024" passed="1024" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `0`, as the task's acceptance requires.

## Reconciliation of the total against the P13-T2 run

The P13-T2 run at the seam reported 964 tests. This run reports 1024. The 60-test increase is fully
accounted for and no test disappeared.

The two TRX test-name sets were compared directly:

```
grep -o 'testName="[^"]*"' evidence/qa-gates/p13-t2/p13-t2.trx | sort -u   # 964 distinct names
grep -o 'testName="[^"]*"' evidence/qa-gates/p13-t7/p13-t7.trx | sort -u   # 1024 distinct names
comm -13 <p13-t2 set> <p13-t7 set> | wc -l                                 # 60 added
comm -23 <p13-t2 set> <p13-t7 set> | wc -l                                 # 0 removed
```

| Source of the delta | Count |
|---|---|
| `TryGetMoveReadiness_*` — the two tests this phase verifies (P13-T4, P13-T5) | 2 |
| Tests brought in by the merge of the sibling branch `bug/breadcrumb-router-navigation-defects-498` at merge commit `ef907908` | 58 |
| Tests removed | 0 |

The 58 sibling-derived tests are not owned by this feature. They are present because this branch was
brought up to date with `origin/epic/quickfiler-bug-family-integration` before the final QA loop, so
that the QA loop runs against the tree that will actually be reviewed and merged. All 58 pass.

## Relationship to the seam-neutrality claim in P13-T2

P13-T2 established that the seam itself added no test and changed no outcome: 964 passed before and
964 passed at the seam. This run does not re-establish that; it establishes that the two behavioural
tests added on top of the seam pass, and that nothing in the merged tree regressed.

| Run | Total | Passed | Failed | Tree |
|---|---|---|---|---|
| P12-T4 (end of Phase 12) | 964 | 964 | 0 | pre-seam |
| P13-T2 (at the seam) | 964 | 964 | 0 | seam only |
| P13-T7 (this run) | 1024 | 1024 | 0 | seam + 2 readiness tests + sibling 498 merge |

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 3,079 substitutions across the four token
classes recorded in `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`
(workspace-root prefix 2,049, machine name 1,027, account name 3, user-profile prefix 0, 8.3
short-name form 0). A post-sanitisation residual scan over the same four patterns returned 0 hits.
No `<Counters>` element, no `outcome` attribute, and no test name was modified by the substitution.
