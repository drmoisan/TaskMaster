# [P0-T2] Phase 0 Instructions Read

- **Issue:** #438
- **Task:** [P0-T2]
- **Timestamp:** 2026-08-08T11-41

## Policy Order

Policies were read in the mandatory order defined by `.claude/skills/policy-compliance-order/SKILL.md`:

1. `CLAUDE.md` (repo root) — standing instructions: policy compliance order, General Code Change Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy, Tone Policy, C# toolchain order.
2. `.claude/rules/general-code-change.md` — cross-language code-change policy: design principles, module rigor tiers, mandatory toolchain loop, 500-line file limit, error handling/logging, naming, public API compatibility, dependencies, I/O boundaries.
3. `.claude/rules/general-unit-test.md` — cross-language unit test policy: five core principles, coverage requirements, coverage exclusion policy, scenario completeness, AAA structure, external-dependency prohibition, test file location, test categories, determinism infrastructure.
4. `.claude/rules/csharp.md` — C#-specific standards: CSharpier formatting, .NET analyzers, nullable type-check, MSTest + Moq + FluentAssertions, coding standards, deterministic test rules, DI seams, analyzer stack, prohibited behaviors.

Additional standing rules present in session context and applied: `.claude/rules/tonality.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/orchestrator-state.md`, `.claude/rules/benchmark-baselines.md`, `.claude/rules/ci-workflows.md`.

Skill contracts read and applied: `policy-compliance-order`, `atomic-plan-contract`, `evidence-and-timestamp-conventions`, `acceptance-criteria-tracking`.

## Files Read (explicit list)

| # | Path | Purpose |
|---|---|---|
| 1 | `CLAUDE.md` | Repo standing instructions and policy compliance order |
| 2 | `.claude/rules/general-code-change.md` | Cross-language code change policy |
| 3 | `.claude/rules/general-unit-test.md` | Cross-language unit test policy |
| 4 | `.claude/rules/csharp.md` | C# toolchain and coding standards |
| 5 | `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/spec.md` | Acceptance-criteria source (AC-1…AC-14 gating; HV-1 non-gating) |
| 6 | `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/research/2026-08-08T10-30-quickfiler-search-keystroke-focus-steal-research.md` | Approved design (Option 3), §1–§9 read in full |
| 7 | `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/plan.2026-08-08T09-57.md` | Plan of record, version 1.2 |

## Key constraints carried into execution

- Toolchain order: format (CSharpier 1.2.6, `format`/`check` subcommands) → analyzers → nullable/TreatWarningsAsErrors → coverage-enabled vstest. Restart from format on any failure or file change.
- .NET Framework 4.8.1: no `init` setters, no `record`, no `record struct`.
- MSTest + Moq + FluentAssertions; Arrange–Act–Assert; no temporary files; no `Thread.Sleep`/`Task.Delay`/wall-clock waits in tests.
- 500-line ceiling on every production, test, and reusable script file.
- Every new `.cs` file requires an explicit `<Compile Include>` entry in its legacy non-SDK `.csproj`.
- Weakening assertions or relaxing test expectations to make a build pass is a prohibited behavior (`.claude/rules/csharp.md` § Prohibited Behaviors).
- Evidence path scheme is `<FEATURE>/evidence/<kind>/` (non-overridable).

## Result

- **Command:** N/A (document reads)
- **EXIT_CODE:** 0
- **Output Summary:** All four policy documents were read in the required order, plus the AC source (`spec.md`), the approved research (§1–§9), and the plan of record. No conflicting instruction was encountered; no policy document was modified.
