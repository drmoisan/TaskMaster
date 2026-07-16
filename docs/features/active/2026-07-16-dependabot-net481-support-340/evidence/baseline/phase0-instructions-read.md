# Phase 0 — Instructions Read

- Timestamp: 2026-07-16T15-56
- Issue: #340

## Policy Order

1. `CLAUDE.md` (repo-root standing instructions; policy compliance order)
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `spec.md`, `user-story.md`, `research/2026-07-16T16-10-dependabot-net481-support-research.md` (this feature folder)

## Files Read

- `CLAUDE.md` — repo-root standing instructions (already resident in session context; reconfirmed applicable policy order).
- `.claude/rules/general-code-change.md` — read in full. This is a config/documentation-only change (no `.cs`, `.csproj`, or test file is touched). Its design-principles sections (simplicity, reusability, separation of concerns) and naming/error-handling sections apply only loosely to a declarative YAML artifact and a documentation section; the file-size limit (500 lines, with an explicit Markdown-documentation exception) and the "avoid breaking public APIs" guidance are the concretely applicable clauses. The seven-stage "Mandatory Toolchain Loop" (format/lint/type-check/architecture/unit/contract/integration) does not apply to this change: there is no C# source, so no formatter/linter/type-checker/test runner has a target to run against. Per `spec.md`'s Definition of Done, this is superseded by the plan's own Phase 7 substitution (YAML-validity check, AC-4 enumeration, AC-10 diff review).
- `.claude/rules/general-unit-test.md` — read in full. No unit-test policy applies to this feature: no test code is added or modified, no production source file is added or modified, and there is no executable behavior to cover with MSTest/Moq/FluentAssertions. Coverage requirements, scenario-completeness requirements, and determinism-infrastructure requirements in this policy have no applicable target in this change.
- `spec.md` (this feature folder, `docs/features/active/2026-07-16-dependabot-net481-support-340/spec.md`) — read in full.
- `user-story.md` (this feature folder, `docs/features/active/2026-07-16-dependabot-net481-support-340/user-story.md`) — read in full.
- `research/2026-07-16T16-10-dependabot-net481-support-research.md` (this feature folder, `docs/features/active/2026-07-16-dependabot-net481-support-340/research/2026-07-16T16-10-dependabot-net481-support-research.md`) — read in full.

## Conclusion

No C# toolchain (CSharpier/analyzer/nullable/`vstest.console.exe`) applies to this change. Phase 7 of the plan substitutes a YAML-validity check plus AC-4/AC-10 verification steps, consistent with `spec.md`'s Definition of Done.
