# P0-T17 — Baseline-Relative Reading Of The C# Gates

Timestamp: 2026-09-13T05-06
Task: [P0-T17]

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; <read spec.md, select the AC15 and AC22 lines, test each for the phrase "no worse than the Phase 0 baseline", and count the phrase across the whole file>'
EXIT_CODE: 0

Outcome: no edit was needed. The amendment the task describes is a contingency and was not the
observed case.

PHRASE_PRESENT_IN_AC15: True
PHRASE_PRESENT_IN_AC22: True
TOTAL_PHRASE_OCCURRENCES_IN_SPEC: 2

The phrase `no worse than the Phase 0 baseline` occurs exactly twice in `spec.md`, once in AC15 and
once in AC22, which is what the two per-criterion tests above establish. Line positions at the time of
this reading are AC15 at line 288 and AC22 at line 295; both shifted by two lines relative to the
pre-P0-T16 state because P0-T16 added one sentence and one blank line earlier in the file.

## AC15, verbatim

```
- [ ] **AC15 — Project-file correction, absence paired with a parse check.** `TaskMaster/TaskMaster.csproj` line 37 carries the publish-destination element with the repository-relative value used by the other project file in this repository for the same property; a case-insensitive search of that file for the account, host and employer organization tokens and for a drive-letter-rooted user-profile path returns zero matches; and the file loads without error as an XML document. All three observations hold, and each of the two msbuild passes named in `CLAUDE.md` records an exit code and an error count that are no worse than the Phase 0 baseline recorded for that same command. The comparison is baseline-relative by construction: "no new diagnostics" has no meaning without a recorded prior count, so Phase 0 must capture one per pass as an integer rather than as a prose adjective. An unqualified absence of error from a solution-wide rebuild is deliberately not demanded, because the pre-change state of that rebuild is not this delivery's to repair.
```

## AC22, verbatim

```
- [ ] **AC22 — Full toolchain pass and no temporary files.** A single consecutive pass of `Invoke-Formatter`, `Invoke-ScriptAnalyzer`, and `Invoke-Pester` over the changed script and test files completes with zero new findings and zero failed tests, and the C# format check completes without error. The two msbuild passes named in `CLAUDE.md` are judged against the Phase 0 baseline rather than against absolute zero: each must record an exit code and an error count that are no worse than the Phase 0 baseline recorded for that same command. Absolute success is deliberately not demanded, because the pre-change state of a whole-solution rebuild is not this delivery's to fix and a red baseline would make the clause unsatisfiable for reasons this change does not cause. A review of the seven test files in the Write Set confirms no test creates, writes or deletes a file on disk and no fixture is loaded from a path.
```

## Output Summary

Both criterion texts are recorded above verbatim and the phrase is present in both. Neither criterion
demands an unqualified absence of error from a solution-wide rebuild: AC15 states that such a demand
is deliberately not made, and AC22 states the same in its own wording. No amendment was made to either
criterion.

This reading is load-bearing for this worktree, because the P0-T8 and P0-T9 baselines are both red for
a pre-existing analyzer HintPath skew that originates at the base commit. Under the recorded reading,
the Phase 7 C# gates are satisfied when each msbuild pass records an exit code of at most 1 and an
error count of at most 2, matching the baseline artifacts for the same commands.

EXIT_CODE: 0
