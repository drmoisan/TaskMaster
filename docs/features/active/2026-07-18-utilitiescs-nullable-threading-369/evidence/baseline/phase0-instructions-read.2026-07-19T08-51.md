# Phase 0 — Policy and Requirements Read Evidence

- Timestamp: 2026-07-19T08-51
- Task: [P0-T1]
- Feature: utilitiescs-nullable-threading (Issue #369)
- Branch: feature/utilitiescs-nullable-threading-369

## Policy Order

Files were read in the mandated order defined by `policy-compliance-order` and the plan's Phase 0 read list:

1. `CLAUDE.md` (standing instructions; loaded in session context)
2. `.claude/rules/general-code-change.md` (cross-language code change policy; loaded in session context)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy; loaded in session context)
4. `.claude/rules/csharp.md` (C#-specific toolchain and coding standards)

Then the requirements sources:

5. `docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/spec.md` (Definition of Done — AC source)
6. `docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/user-story.md` (Acceptance Criteria — AC source)
7. `docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/issue.md`
8. `docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/research/research-findings.2026-07-18T22-45.md`

## Files Read (explicit list)

- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a01d6eefe1f9bff5a\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a01d6eefe1f9bff5a\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a01d6eefe1f9bff5a\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a01d6eefe1f9bff5a\.claude\rules\csharp.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a01d6eefe1f9bff5a\docs\features\active\utilitiescs-nullable-threading\spec.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a01d6eefe1f9bff5a\docs\features\active\utilitiescs-nullable-threading\user-story.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a01d6eefe1f9bff5a\docs\features\active\utilitiescs-nullable-threading\issue.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a01d6eefe1f9bff5a\docs\features\active\utilitiescs-nullable-threading\research\research-findings.2026-07-18T22-45.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a01d6eefe1f9bff5a\docs\features\active\utilitiescs-nullable-threading\plan.2026-07-18T22-04.md

## Key Constraints Confirmed

- Per-file `#nullable enable` pragma only; NO project/solution `<Nullable>` element.
- Nullable/type-check verification uses the PRAGMA-ONLY build: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` with NO `/p:Nullable=enable`.
- Annotation and null-safety only; no behavior/concurrency-semantics change.
- No `System.Diagnostics.CodeAnalysis` post-condition attributes (unavailable on net481).
- Leave `*.Designer.cs` and `.resx` non-opted-in and byte-unchanged.
- FLAG (do not fix) the `TimeOutTask.cs` 500-line breach and any annotation-induced breach of `ApplicationIdleTimer.cs` / `AsyncMultiTasker.cs`.
- Do NOT edit any `.claude/rules/*` file.

## Output Summary

All eight required policy/requirements files plus the plan were read in the mandated order. Constraints, hard limits, and the critical pragma-only toolchain deviation are understood and recorded. Ready to proceed to baseline capture (P0-T2..T5).
