# Phase 0 — Policy Documents Read ([P0-T1])

Timestamp: 2026-08-10T22-30
Command: (none — analysis artifact)
EXIT_CODE: (none — analysis artifact)

## Policy Order

The `policy-compliance-order` skill defines the required reading order. It was applied as written:

1. `CLAUDE.md` (standing instructions, always loaded)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. Language- or domain-specific rules for the files in scope:
   - C#: `.claude/rules/csharp.md`
   - PowerShell: `.claude/rules/powershell.md`

## Files read, in order

| # | Path | Status |
|---|---|---|
| 1 | `CLAUDE.md` | Read (full policy body; §§ C#1, C#5, CUT3, "C# Toolchain (run in this exact order)", UT2 inspected verbatim) |
| 2 | `.claude/rules/general-code-change.md` | Read |
| 3 | `.claude/rules/general-unit-test.md` | Read (PROTECTED — not edited by this feature) |
| 4 | `.claude/rules/csharp.md` | Read (in scope for edits at rows 12-15 of the spec replacement table) |
| 5 | `.claude/rules/powershell.md` | Read (governs the executable-carrier change to `scripts/vscode/Invoke-VSBuild.ps1`) |

## Skills read

| Skill | Path |
|---|---|
| `policy-compliance-order` | `.claude/skills/policy-compliance-order/SKILL.md` |
| `atomic-plan-contract` | `.claude/skills/atomic-plan-contract/SKILL.md` |
| `evidence-and-timestamp-conventions` | `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` |
| `acceptance-criteria-tracking` | `.claude/skills/acceptance-criteria-tracking/SKILL.md` |

Additionally read for site location: `.claude/skills/csharp-qa-gate/SKILL.md` (in scope for edits at rows 16-19).

## Recorded interaction with the `policy-compliance-order` hard constraint

`policy-compliance-order` states: "Do NOT modify policy documents under `.claude/rules/` or
`.github/instructions/`." That constraint is suspended for this feature for
`.claude/rules/csharp.md` only, by the epic's "Execution Authorization Required" section
(`docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`). `.github/instructions/` remains
under the unsuspended constraint and is excluded (SD1). `.claude/rules/general-unit-test.md` and
`.claude/rules/quality-tiers.md` are protected by this feature's own scope and are not edited here.

## Output Summary

All five policy documents and all four required skills were read in the prescribed order before any
edit was made. C# and PowerShell language rules both apply: the feature changes `*.ps1`, `*.json`
and Markdown governance documents, and executes (but does not change) C# source. `.claude/rules/csharp.md`
line-coverage/test obligations for C# are not engaged because no `*.cs` file is modified;
`.claude/rules/powershell.md` coverage obligations (line >= 85%, branch >= 75%) are engaged and are
discharged by [P0-T16], [P6-T3] and [P6-T4].
