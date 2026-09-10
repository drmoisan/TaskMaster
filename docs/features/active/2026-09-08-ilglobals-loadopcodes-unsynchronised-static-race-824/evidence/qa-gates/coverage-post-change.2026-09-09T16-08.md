# QC loop step 5 — post-change coverage run (Issue #824, task P5-T8)

Timestamp: 2026-09-09T16-08

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; pwsh -NoProfile -File ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/post-change.cobertura.xml 2>&1 | Tee-Object -FilePath coverage/coverage-post-change.log | Select-Object -Last 12'`

EXIT_CODE: 0

## Command identity with the baseline

This is the Coverage Command Of Record recorded in
`evidence/baseline/coverage-baseline.2026-09-09T15-14.md`, re-run byte-for-byte, varying only the
two output paths the plan authorises:

| Argument | Baseline (P0-T11) | Post-change (P5-T8) |
|---|---|---|
| `-CoverageOutput` | `coverage/baseline.cobertura.xml` | `coverage/post-change.cobertura.xml` |
| `Tee-Object -FilePath` | `coverage/coverage-baseline.log` | `coverage/coverage-post-change.log` |

Every other argument is identical, so the two runs measure the same population and the P5-T11
comparison is between comparable documents. The `Select-Object -Last` bound differs (30 against 12)
and affects only how much of the console transcript was echoed; `Tee-Object` writes the complete
stream to the log before that stage, and every derivation below reads the log.

## Output Summary

`coverage/post-change.cobertura.xml` exists.

Document-level coverage:

| Attribute | Value | As a percentage |
|---|---|---|
| `/coverage/@line-rate` | 0.856241 | 85.6241 % |
| `/coverage/@branch-rate` | 0.798129 | 79.8129 % |

The runner additionally printed
`First-party coverage: lines 56035/65443 (85.62%), branches 13482/16892 (79.81%)`.

Test counts, derived by the rule stated in P0-T11:

| Count | Value | Source |
|---|---|---|
| Total | 7212 | read from the line `Total tests: 7212`; count of lines matching `^\s*Total tests:` is 1 |
| Passed | 7212 | read from the line `Passed: 7212`; count of lines matching `^\s*Passed:` is 1 |
| Failed | **0** | no line matching `^\s*Failed:` is present (count 0) and the log contains `Test Run Successful.` |
| Skipped | 0 | no line matching `^\s*Skipped:` is present (count 0) |

Both observations the plan requires for the zero-failure conclusion on an all-green run are recorded
explicitly:

- the log contains `Test Run Successful.` — count of matching lines: **1**;
- the log contains no line matching `^\s*Failed:` — count: **0**.

For completeness the negative case was measured as well: the count of lines matching
`Test Run Failed\.` is **0**.

The failed count is 0, so the REMEDIATION-REQUIRED branch of this task is not taken and no failing
test needs to be enumerated or compared against the baseline failing set, which
`evidence/baseline/coverage-baseline.2026-09-09T15-14.md` records as empty.

Terminal console lines:

```
Test Run Successful.
Total tests: 7212
     Passed: 7212
 Total time: 30.0303 Seconds
Post-processing coverage XML for Koverage compatibility...
First-party coverage: lines 56035/65443 (85.62%), branches 13482/16892 (79.81%)
```

## Test-count delta against the baseline

| Run | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| Baseline (P0-T11) | 7210 | 7210 | 0 | 0 |
| Post-change (P5-T8) | 7212 | 7212 | 0 | 0 |

The suite grew by exactly 2 tests, which is the net change this feature makes to the discovered test
population: four added (`LoadOpCodes_DoesNotRepublishPublishedTables`,
`SingleByteOpCodes_FieldIsInitOnly`, `MultiByteOpCodes_FieldIsInitOnly`,
`OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes`) minus two deleted
(`LoadOpCodes_PopulatesKnownSingleByteOpCodes`, `LoadOpCodes_PopulatesKnownOpCode_Ret`). The two
renames are net-zero. This matches the scoped-run control of 14 recorded by P3-T5 against a baseline
class size of 12.

The raw Cobertura document is not committed: `coverage/*` is gitignored and the document is on the
order of 10 MB. Per plan D22 the committed coverage evidence is the compact markdown extract written
by P5-T10.
