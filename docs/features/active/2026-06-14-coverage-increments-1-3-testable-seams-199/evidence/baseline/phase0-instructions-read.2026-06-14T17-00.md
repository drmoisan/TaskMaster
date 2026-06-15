---
Timestamp: 2026-06-14T17-00
Policy Order:
  1. CLAUDE.md
  2. .claude/rules/general-code-change.md
  3. .claude/rules/general-unit-test.md
  4. .claude/rules/csharp.md
  5. .claude/skills/atomic-plan-contract/SKILL.md
  6. .claude/skills/evidence-and-timestamp-conventions/SKILL.md
---

## Files Read

1. `CLAUDE.md` — project instructions, C# toolchain, policy compliance order, tone policy
2. `.claude/rules/general-code-change.md` — cross-language code change policy, design principles, toolchain loop
3. `.claude/rules/general-unit-test.md` — cross-language unit test policy, coverage requirements, AAA structure
4. `.claude/rules/csharp.md` — C# coding standards, analyzer stack, DI seams, prohibited behaviors
5. `.claude/skills/atomic-plan-contract/SKILL.md` — atomic plan format, Phase 0 requirements, final QA loop rules
6. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` — canonical evidence paths, ISO-8601 timestamps

## Phase 6 Scope

Executing Phase 6 of plan `plan.2026-06-14T17-00.md` to close AC1 sub-branch: replace the
three raw `MessageBox.Show(...)` calls in the `ProjectEntry.ProjectID` property setter with
`MyBox.ShowDialog(...)` calls, then add four new MSTest change-confirmation branch tests to
`ProjectEntryDialogBranchesTests.cs`.

## Hard Constraints Acknowledged

- Production change limited to: `MessageBox.Show` → `MyBox.ShowDialog` in the `ProjectID` setter only.
- No other logic, API, or behavior changes permitted.
- Flag-and-Stop if unexpected `System.Windows.Forms` usage found beyond the three documented call sites.
- Full C# toolchain must pass green: csharpier → analyzers → nullable → MSTest.
- Cobertura XML goes to `artifacts/csharp/` only; feature evidence folder gets summaries only.
