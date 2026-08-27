# Final commit evidence

- Timestamp: `2026-08-27T01:59:37Z`
- Remediation commit: `98b7a5e14afcf5580bf9351be3ca18e9e306dca9`
- Commit message: `fix(quickfiler): reject rooted selections at filing boundary`
- Commit summary: 27 files changed, 738 insertions, 254 deletions.
- `git diff --cached --check` before commit: exit 0.
- `git status --porcelain` immediately after commit: empty.
- Temporary runtime profile `.codex/agents/commit-steward-c3-elevated.toml`: removed after the exact steward handoff and absent before the commit.
- Coverage force-add check: no committed path is under `coverage/`.

## Committed paths

- `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`
- `QuickFiler/Controllers/EfcFormController.cs`
- `QuickFiler/Controllers/EfcSelectionGuard.cs`
- `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/change-description.2026-08-26.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/other/resolver-consumer-check.2026-08-26T22-19.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/coverage-delta.2026-08-26T22-28.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/final-analyzer-build.2026-08-26T22-25.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/final-csharpier.2026-08-26T22-24.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/final-nullable-build.2026-08-26T22-26.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/final-size-scope.2026-08-26T22-29.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/final-test-coverage.2026-08-26T22-27.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/p4-t2-scope-lock.2026-08-26T22-23.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/redaction-sweep.2026-08-26T22-31.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/toolchain-clean-pass.2026-08-26T22-30.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/regression-testing/p4-t1-integration.2026-08-26T22-22.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/regression-testing/rc4-getstem.2026-08-26T22-21.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/regression-testing/revert-expect-fail.2026-08-26T22-18.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/regression-testing/revert-pass-after.2026-08-26T22-20.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/analyzer-build.2026-08-26T22-14.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/format-check.2026-08-26T22-13.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/full-suite-coverage.2026-08-26T22-16.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/nullable-build.2026-08-26T22-15.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/phase0-instructions-read.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/pre-change-facts.2026-08-26T22-17.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/remediation-plan.2026-08-26T22-12.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/spec.md`
