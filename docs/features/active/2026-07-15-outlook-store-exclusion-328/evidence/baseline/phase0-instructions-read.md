# Phase 0 — Policy Instructions Read (Issue #328)

Timestamp: 2026-07-15T18-45

Policy Order: Per `.claude/skills/policy-compliance-order`:
1. CLAUDE.md (standing instructions)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. Language-specific: .claude/rules/csharp.md (C# toolchain and standards)

Plus atomic-execution contract skills.

Files read (explicit list):
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md
- .claude/skills/atomic-plan-contract/SKILL.md
- .claude/skills/evidence-and-timestamp-conventions/SKILL.md
- .claude/skills/policy-compliance-order/SKILL.md

Notes:
- C# toolchain order: CSharpier format -> analyzer msbuild -> nullable/TreatWarningsAsErrors msbuild -> vstest.console.exe with coverage.
- Evidence path authority: all artifacts resolve under
  docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/<kind>/.
- File-size limit 500 lines (documented exceptions per plan: AppToDoObjects.cs pre-existing 503 lines).
- Coverage floor: repo-wide line >= 80%; new modules/classes/methods >= 90%; no regression on changed lines.
