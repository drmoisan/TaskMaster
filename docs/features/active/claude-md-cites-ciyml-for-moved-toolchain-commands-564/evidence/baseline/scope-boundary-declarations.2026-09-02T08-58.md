---
timestamp: 2026-09-02T08-58
plan: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/plan.2026-09-02T08-58.md
task: P1-T1
---

# Scope Boundary Declarations

Timestamp: 2026-09-02T08-58

## Declaration (a): No Edits Under .claude/**, .codex/**, .agents/**, config/blast-radius.json, config/orchestration-routing.json

This plan performs zero edits under:
- `.claude/**` (any path under .claude directory)
- `.codex/**` (any path under .codex directory)
- `.agents/**` (any path under .agents directory)
- `config/blast-radius.json`
- `config/orchestration-routing.json`

These paths are published from the upstream `drm-copilot` repository with zero templating and are silently overwritten on the next push-down. No task in this plan touches them. The final-QA phase (P3-T6) verifies this boundary via `git diff origin/main...HEAD --name-only`.

## Declaration (b): .claude/rules/csharp.md Unchanged

The baseline search recorded in P0-T5 confirmed:
- Command: `Select-String -Path .claude/rules/csharp.md -Pattern 'ci\.yml|workflows'`
- Result: 0 matches
- Conclusion: `.claude/rules/csharp.md` contains zero citations to ci.yml or workflow files and requires no edit.

## Declaration (c): No C# Toolchain Gate Applies

The general-code-change.md policy mandates a 7-stage toolchain loop:
1. Formatting
2. Linting
3. Type checking
4. Architecture-boundary tests
5. Unit tests
6. Contract / schema compatibility checks
7. Integration tests

This plan substitutes a targeted text/diff verification loop (Phase 3) in place of the full toolchain because:
- The only file this plan edits is `CLAUDE.md`, a Markdown documentation file.
- The plan does not modify any *.cs, *.csproj, *.props, *.targets files.
- The plan does not modify any test files.
- The plan does not modify any Python, PowerShell, or TypeScript files.

No C# formatter (CSharpier), linter (msbuild analyzers), type-checker (msbuild nullable), or test runner (vstest) is invoked.

Verification: P3-T6 confirms via `git diff origin/main...HEAD --name-only` that the diff contains only `CLAUDE.md`.

## Declaration (d): No Failing-Regression-Test Task Included

The general-code-change.md policy's Bugfix Workflow (§ "Bugfix Workflow (all languages, defects only)") requires:
1. Create a failing regression test first
2. Implement the minimal, targeted fix
3. Verify locally before review

This plan omits step 1 (failing-regression-test task) because:
- Per spec.md's Test Strategy section: "a Markdown-text change has no unit-test surface, so no MSTest/Pester/pytest additions apply"
- Documentation corrections to Markdown files cannot have unit-test regressions.
- Phase 0's baseline citation capture (P0-T5) stands in as the false-before evidence for AC1–AC4, proving the citations were stale before the fix.

No executable behavior is changed by this plan.

---

All scope boundaries are honored. This plan is narrowly scoped to the three citation token replacements in CLAUDE.md.
