# AC22 CI Evidence (PR #976)

Timestamp: 2026-10-03T07-39
Command: gh run view 37120059960 --json status,conclusion,headSha,jobs
EXIT_CODE: 0
Output Summary: CI run 37120059960 (workflow CI, event pull_request, PR #976) on head 78e24a68ce523d18fba6b68c0905876b46f62ceb concluded success; all seven jobs concluded success, and all six required checks report bucket pass.

## Run

- Run: https://github.com/drmoisan/TaskMaster/actions/runs/37120059960
- Head SHA: 78e24a68ce523d18fba6b68c0905876b46f62ceb
- Run conclusion: success

## Job Results (in toolchain order)

| Step | Job | Job ID | Conclusion | Key output |
|---|---|---|---|---|
| 1 Format | format-check / Verify formatting | 111194270132 | success | `Checked 1640 files` |
| 2 Analyzers | build-analyzers / Build with analyzers and code style enforcement | 111194270164 | success | `Build succeeded.` 0 Warning(s), 0 Error(s) |
| 3 Type-check | build-nullable / Build with nullable warnings treated as errors | 111194270148 | success | `Build succeeded.` 0 Warning(s), 0 Error(s) |
| 4 Test + coverage | mstest-coverage / Run MSTest suite with coverage | 111194270196 | success | Total tests 7388, Passed 7388; first-party lines 56630/65855 (85.99%), branches 13683/17078 (80.12%) |
| other | hygiene / Repository hygiene guard | 111194270142 | success | n/a |
| other | pester / Run Pester suite with coverage | 111194270166 | success | n/a |
| other | actionlint / actionlint | 111194270190 | success | n/a |

Key output lines were extracted from `gh run view 37120059960 --job <id> --log`.

## Notes

- Skipped compile target: the CI runner performs a cold checkout, so its `/t:Build` analyzer and nullable steps compile every project; no incremental skip is possible on a fresh runner (see CLAUDE.md, C#1 item 2).
- CI runs the shell-icon test classes that fail locally on this workstation for environmental reasons; the CI MSTest job reports 7388 of 7388 passed, so no test failed.
- Local DIRECT-route result (recorded in `toolchain-final.md` and `coverage-comparison.md`): 7365 of 7365 passed; first-party lines 85.35% to 85.36%, branches 79.73% to 79.75%.
- The CI first-party figures are from the CI coverage runner and are not directly comparable to the local DIRECT-route figures; both are at or above the repository floors.
