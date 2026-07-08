# AC Reconciliation (Option A AC1-AC8)

Timestamp: 2026-06-12T19-22

Command: Mapping each acceptance criterion to its supporting evidence artifact(s) on disk.

EXIT_CODE: 0

Output Summary: AC1-AC7 PASS (each maps to a present, schema-valid evidence artifact). AC8 PENDING user action.

| AC | Status | Plan task(s) | Supporting evidence |
|----|--------|--------------|---------------------|
| AC1 | PASS | P1-T1 | New file `scripts/vscode/TaskMaster.cli.runsettings` (parallelization only, no DataCollectors). Validated in P1-T1 (well-formed XML, Workers=0/ClassLevel, no `<DataCollectionRunSettings>`). |
| AC2 (content) | PASS | P1-T2 | `TaskMaster.runsettings` edit: `<DataCollector friendlyName="Code Coverage">` Exclude block with exactly 7 mirrored `<ModulePath>` entries; MSTest Parallelize preserved; no `enabled="true"`. Validated in P1-T2. |
| AC2 (effect) | PENDING (via AC8) | P2-T5, P2-T6, P2-T7 | `evidence/regression-testing/exclusion-effect-not-cli-verifiable.2026-06-12T19-22.md`; `evidence/issue-updates/vs-verification-checklist.2026-06-12T19-22.md`; `evidence/issue-updates/ac8-vs-confirmation-pending.2026-06-12T19-22.md`. |
| AC3 | PASS | P1-T3 | Both scripts' `Resolve-RunSettingsPath` now resolve `scripts\vscode\TaskMaster.cli.runsettings` via `$PSScriptRoot` (deterministic); missing-file throw names the CLI path; `Invoke-MSTestWithCoverage.Helpers.ps1` unchanged. Revises #188 AC1-AC3. |
| AC4 | PASS | P1-T4, P2-T1 (Pester) | Test asserts `/Settings:` -> CLI runsettings for both scripts; missing-file throw -> CLI path; only wrapper seams mocked. 9/9 in-scope Pester tests pass (`evidence/qa-gates/powershell-toolchain-final.2026-06-12T19-22.md`). |
| AC5 | PASS | P2-T2, P2-T3 | `evidence/regression-testing/cli-no-collect-run.2026-06-12T19-22.md` (42 Deedle tests pass, NO `.coverage` attachment); `evidence/regression-testing/koverage-no-double-collect.2026-06-12T19-22.md` (inner vstest omits `/collect`). |
| AC6 | PASS | P2-T4 | `evidence/regression-testing/cli-parallelization-parity.2026-06-12T19-22.md` (CLI runsettings retains Workers=0/ClassLevel; parity TRUE). |
| AC7 | PASS | P0-T5 (baseline) + P2-T1 (final) | `evidence/baseline/powershell-toolchain-baseline.2026-06-12T19-22.md` + `evidence/qa-gates/powershell-toolchain-final.2026-06-12T19-22.md`: format clean; no net-new analyzer debt (2 in-scope / 16 folder, unchanged); Pester in-scope tests pass; coverage 77.06% (no regression). |
| AC8 | PENDING user action | P2-T6, P2-T7 | `evidence/issue-updates/vs-verification-checklist.2026-06-12T19-22.md`; `evidence/issue-updates/ac8-vs-confirmation-pending.2026-06-12T19-22.md`. Authoritative VS confirmation pending; CLI cannot reproduce the VS static-coverage failure. |

## Verdict

- AC1, AC2 (content), AC3, AC4, AC5, AC6, AC7: PASS — each maps to at least one present, schema-valid evidence artifact.
- AC2 (effect) and AC8: PENDING user action in Visual Studio (CLI not capable of reproduction; not a blocker).
- No AC1-AC7 mapping is missing. Overall: all CLI-verifiable criteria satisfied; AC8 recorded as pending.
