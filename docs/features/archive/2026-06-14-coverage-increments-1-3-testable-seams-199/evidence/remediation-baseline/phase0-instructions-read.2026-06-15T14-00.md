# Phase 0 — Instructions Read (Remediation Cycle 2026-06-15T14-00)

Timestamp: 2026-06-15T14-00

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific code/test standards)

Files Read:
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md
- docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/remediation-inputs.2026-06-15T14-00.md (cycle directive)
- docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/remediation-plan.2026-06-15T14-00.md (plan of record)

Scope Constraint Acknowledged:
- Only file authorized for change: UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
- No production code change (IdleAsyncQueue.cs, UiThread.cs) permitted; flag-and-stop if required.
- No assertion weakening, no [DoNotParallelize]-only substitute, no sleeps/retries/polling/timing tolerances.
- Toolchain order: csharpier -> msbuild analyzers -> msbuild nullable (TreatWarningsAsErrors) -> MSTest with coverage.
- Raw Cobertura XML to artifacts/csharp/ only; coverage headline summary to canonical feature evidence path.
