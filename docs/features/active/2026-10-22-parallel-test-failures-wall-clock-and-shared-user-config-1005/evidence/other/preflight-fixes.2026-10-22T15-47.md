# Preflight Fixes (Issue #1005)

Timestamp: 2026-10-22T15-47
Plan: docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/plan.2026-10-22T11-02.md
Validator: mcp__drm-copilot__validate_orchestration_artifacts (plan) passed before and after the fixes.

Each fix edits task text only. No task was added, removed, or reordered.

## Fixes

1. [P0-T7] Unrecorded baseline count. P5-T2 compares the number of `DfDeedle_COM_Tests` tests "as at P0-T7", but the P0-T7 acceptance did not require that count. P0-T7 now also records the number of `DfDeedle_COM_Tests` tests executed, derived from the trx.
2. [P1-T2] Conflict between tasks. P1-T2 required the probe assertion message to name the settings file token, and the P6-T2 added-line scan requires a count of zero for that token. The message now describes the shared per-host settings file without spelling the file name. The probe assertion is unchanged.
3. [P1-T6] Evidence field order. The artifact declares `ExpectedExitCode: 1` and records two commands (ANALYZE, exit 0, and the vstest run, exit 1). The first `EXIT_CODE:` forms the machine-read record, so the task now states that the vstest command and its exit code are recorded first.
4. [P3-T2] Test input aligned with the AC2 wording. AC2 requires that "a shorter ID does not save", and the task described a second ID of the same length. The test now generates "0100" from seed "ZZ" (one save) and then the shorter "02" from seed "0001" (no further save). The register name `GetNextToDoID_IdNotLongerThanCachedLength_DoesNotSaveAgain` already covers this case and is unchanged.
5. [P4-T7] Contradiction inside the task. The task keeps the P1-T4 probe, which passes `Settings.Default` to `SettingsSaveProbe.CountSavesOnCurrentThread`, while its acceptance required no `Settings.Default` reference. The acceptance now exempts that single probe argument.
6. [P4-T8] The same contradiction for the P1-T5 probe argument (`Properties.Settings.Default`). The acceptance now exempts that single probe argument. The "no `Save()` call" clause is unchanged.
7. [P6-T3] Scope fence did not cover existing paths. The merge-base diff already lists `docs/features/potential/promoted/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config.md`, and the working tree holds the untracked issue-1006 promotion record under the same folder. Neither path was in the permitted set. The permitted set now includes any path already listed in the P0-T3 `merge-base-scope` artifact (those promotion records and the `.claude/agent-memory/` changes). "No other `.claude/` path" and the `UtilitiesCS/Extensions/` exclusion are unchanged.
8. [P6-T4] Wording. "In one process" now reads "in one `vstest.console.exe` invocation", because vstest starts one test host per assembly. The elapsed-time field is now required only for runs that include `DfDeedle_COM_Tests`, because the other nine solo runs cannot report that test.

## Observations (not defects; recorded for the executor)

- During the P1-T6 fail-before run, `AppAutoFileObjectsCoverageExpansionTests.TestCleanup` still calls `defaults.Save()` outside the probe. That call is not cancelled and writes the settings file. This is existing behavior that the P0-T7 baseline run also shows, and P4-T8 removes it. The probes themselves cancel every save they record.
- `ApplicationSettingsBase.OnSettingsSaving` reads the delegate field twice. A non-probe save on another thread can therefore race with the last probe unsubscribing. After the fix, no test saves, so this exposure is limited to the P1-T6 run and its window is very small.
- The final line of the planner internal review record in the plan file is `PREFLIGHT: REVISIONS REQUIRED`. That line was left by the planner and does not reflect this preflight result.
