# Preflight Fixes, Round 2 (Issue #1005)

Timestamp: 2026-10-22T16-01
Plan: docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/plan.2026-10-22T11-02.md
Validator: mcp__drm-copilot__validate_orchestration_artifacts (plan) passed before and after the fixes.

Each fix edits task text only. No task was added, removed, or reordered.

## Fixes

1. [P4-T7] Missing instruction behind an acceptance condition. The acceptance requires no `Settings.Default` reference other than the probe argument, but the existing class summary of `AppStagingFilenamesTests` mentions `Settings.Default` several times, and the task, unlike P4-T5 and P4-T6, did not say to update the summary. The task now says to update the class summary. The acceptance is unchanged.
2. [P6-T2] Ambiguous reference. "Both runsettings files" did not name the files, and the repository holds several `.runsettings` files. The task now names `TaskMaster.runsettings` and `scripts/vscode/TaskMaster.cli.runsettings`. The diff scan over `*.runsettings` is unchanged.
3. [P6-T3] Scope fence did not cover agent memory written during execution. The permitted set covered only `.claude/agent-memory/` paths already listed by P0-T3, so an agent-memory file written by any agent after P0-T3 would fail the fence although no plan task edits it. The permitted set now includes any path under `.claude/agent-memory/`. "No other `.claude/` path" and the `UtilitiesCS/Extensions/` exclusion are unchanged.
4. [P7-T7] Count literal that a restarted QA loop breaks. The acceptance allowed exactly "the two `.jacoco.xml` projection copies", but the P7 loop rule writes a new P7-T4 artifact on each pass, so a restart leaves more than one final projection copy. The acceptance now allows only `.jacoco.xml` files named `baseline-coverage-projection.*.jacoco.xml` or `final-coverage-projection.*.jacoco.xml`. The prohibition on raw trx, raw coverage, and Cobertura files is unchanged.

## Observations (not defects; recorded for the executor)

- P1-T1: the revised clause about copying a delegate field to a local has no effect on the probe, because the probe holds its handler in a local variable and reads no delegate field. The non-atomic double read is inside `ApplicationSettingsBase.OnSettingsSaving`, which the probe cannot change. Only a save that no probe records is exposed: such a save can run on another thread while the last probe unsubscribes. After the fixes no test saves, so the exposure is limited to the P1-T6 fail-before run (the `AppAutoFileObjectsCoverageExpansionTests.TestCleanup` save).
- `ToDoModel.Test` and `TaskTree.Test` declare no `LangVersion` and therefore compile as C# 7.3 on .NET Framework 4.8.1. `TaskMaster.Test` declares `latest`.
- The post-processed Cobertura document keeps `<methods>` elements (the merge and closure-filter helpers under `scripts/vscode/` retain method-level lines), so the per-member figures that P7-T5 and AC7 require can be read from it.
