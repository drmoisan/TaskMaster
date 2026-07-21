# Phase 0 — Policy Reads (Remediation, Issue #328)

Timestamp: 2026-07-16T02-30

Policy Order: per `.claude/skills/policy-compliance-order/SKILL.md` — (1) CLAUDE.md,
(2) `.claude/rules/general-code-change.md`, (3) `.claude/rules/general-unit-test.md`,
(4) language/domain-specific rules (C#: `.claude/rules/csharp.md`), then the contract and
convention skills required by this remediation plan.

Files read (explicit list):

- `CLAUDE.md` (standing project instructions; C# Code/Unit Test policy sections)
- `.claude/rules/general-code-change.md` (cross-language code change policy; file-size limit; toolchain loop)
- `.claude/rules/general-unit-test.md` (cross-language unit test policy; coverage floors — line >= 85%, branch >= 75%; coverage-exclusion prohibition)
- `.claude/rules/csharp.md` (C# toolchain order csharpier -> analyzers -> nullable/TWAE -> vstest; analyzer stack; coverage floors)
- `.claude/skills/atomic-plan-contract/SKILL.md` (plan format; Phase 0 requirements; evidence-path clause; final QA loop; no-SKIPPED rule)
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` (canonical `<FEATURE>/evidence/<kind>/` scheme; ISO-8601 `yyyy-MM-ddTHH-mm`; artifact schema)
- `.claude/skills/policy-compliance-order/SKILL.md` (mandatory policy reading order; hard constraints)
- `.claude/rules/powershell.md` (auto-loaded; PoshQC toolchain; used for the R1 hook-parse verification of the JaCoCo artifact)

Note: this remediation modifies only Markdown documents and one JaCoCo coverage-gate input file
(`artifacts/csharp/coverage.xml`); no `.cs`/`.csproj`/`.props`/`.targets` file is changed, so the
four-stage C# toolchain loop is carried over from the delivered plan's Phase 4 evidence (see P0-T2).
