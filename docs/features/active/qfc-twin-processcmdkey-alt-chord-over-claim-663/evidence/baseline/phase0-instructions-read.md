# Phase 0 — Instructions read ([P0-T1])

Timestamp: 2026-09-01T21-44

Policy Order:

1. `CLAUDE.md` — standing instructions, all sections
2. `.claude/rules/general-code-change.md` — cross-language code change policy
3. `.claude/rules/general-unit-test.md` — cross-language unit test policy
4. `.claude/rules/csharp.md` — C#-specific code change and unit test policy

Supplementary rule files read after the four core policies:

5. `.claude/rules/quality-tiers.md`
6. `.claude/rules/tonality.md`
7. `.claude/rules/plan-acceptance-gates.md`

## Explicit list of all seven file paths read

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `.claude/rules/quality-tiers.md`
- `.claude/rules/tonality.md`
- `.claude/rules/plan-acceptance-gates.md`

All seven paths are repository-relative to `<repo-root>`.

Command: Read tool invocations against the seven paths listed above.

EXIT_CODE: 0

Output Summary: All seven files were read in the stated order. The four core policies were read
first, in the order `CLAUDE.md`, `general-code-change.md`, `general-unit-test.md`, `csharp.md`, then
the three supplementary rule files. Constraints carried forward into execution: CSharpier is invoked
only through `dotnet tool run` at the manifest-pinned 1.2.6; the analyzer and type-check MSBuild gates
use `/t:Rebuild`; `/p:Nullable=enable` is never added to the type-check gate; MSTest, Moq and
FluentAssertions are the mandated test stack; no temporary files may be created by tests; no
production file may be excluded from coverage measurement; no policy document under `.claude/rules/`
may be modified.
