# Phase 0 — Policy documents read (P0-T1)

Timestamp: 2026-09-01T21-22

Policy Order: the order defined by `.claude/skills/policy-compliance-order/SKILL.md` —
standing instructions first (`CLAUDE.md`), then the cross-language code-change policy, then the
cross-language unit-test policy, then the language- and domain-specific rules that the files in
scope select. The files in scope for this change are `*.cs` and `*.csproj` under `QuickFiler/` and
`QuickFiler.Test/`, so the C# rule file applies. The tier, tonality and plan-acceptance-gate rules
are read in addition because this plan's gates cite them directly.

## Files read, in order

1. `CLAUDE.md` (repository root) — standing instructions: policy compliance order, General Code
   Change Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy, tone policy,
   and the four-command C# toolchain.
2. `.claude/rules/general-code-change.md` — design principles, module rigor tiers, the mandatory
   toolchain loop, the 500-line file-size limit, error handling, naming, dependencies, I/O
   boundaries.
3. `.claude/rules/general-unit-test.md` — the five core unit-test principles, coverage
   requirements and the coverage exclusion policy, scenario completeness, Arrange-Act-Assert,
   external-dependency prohibitions, test file location, determinism infrastructure.
4. `.claude/rules/csharp.md` — CSharpier / analyzer / nullable / MSTest toolchain commands, coding
   standards, testing standards, deterministic test rules, the DI seam preference order (the
   injectable-delegate seam is item 2 at line 52), the five-package analyzer stack, prohibited
   behaviors.
5. `.claude/rules/quality-tiers.md` — the T1 through T4 tier system, the uniform-versus-tier
   dependent gate matrix, and the uniform coverage thresholds.
6. `.claude/rules/tonality.md` — required professional tone, prohibitions on humor, hyperbole and
   decorative metaphor, evidence-first wording.
7. `.claude/rules/plan-acceptance-gates.md` — acceptance-gate rules G1 through G9, the write-mode
   register, the checkable-literal definition and placeholder guard, and the deliberately uncovered
   sub-classes (the general unobservable-success-output class and the task-ordering class).

All seven files are present in this worktree and were read in full in this session.

## Conflicts observed between policy documents

`CLAUDE.md` states a repository-wide line-coverage floor of 80 percent and 90 percent for new
modules, classes and methods. `.claude/rules/general-unit-test.md` and
`.claude/rules/quality-tiers.md` state 85 percent line and 75 percent branch uniformly across
T1 through T4. The plan's "Coverage threshold reconciliation (AC20)" section governs the treatment:
both repository-wide figures are recorded numerically and reported, the blocking gates are
change-scoped, and a repository-wide figure below a policy floor at baseline is recorded as a
pre-existing condition rather than silently accepted. No floor is superseded and no waiver is
granted by this plan or by this artifact.

`.claude/rules/general-unit-test.md` states that no production file may be excluded from coverage
measurement, while `CLAUDE.md` ratifies a COM/VSTO/WinForms coverage exemption applied through
`[ExcludeFromCodeCoverage]`. This change adds and removes no `[ExcludeFromCodeCoverage]` attribute
(AC20), so the conflict is not reached by any edit in this plan. It is recorded here rather than
resolved.

## Orchestrator-supplied preconditions (recorded, not re-performed)

The plan's Phase 0 assumes a bootstrapped tree. The orchestrator performed the following before
delegating; they are recorded here as preconditions and were not repeated by the executor. Their
absence from the plan is not logged as an executor-discovered plan defect.

1. `.dotnet-sdk` installed via `scripts/vscode/Install-RepoDotNetSdk.ps1`; `dotnet --version`
   reports `8.0.205`. The directory is git-ignored.
2. `packages/` restored via `scripts/vscode/Invoke-Restore.ps1` (172 packages). Analyzer versions
   verified in agreement between the csproj `<Analyzer Include>` items and the restored package
   folders: Meziantou.Analyzer 3.0.194 and Roslynator.Analyzers 5.0.0. There is no analyzer-path
   skew.
3. The `dotnet-coverage` global tool is present at version 18.10.0.

P0-T4 runs `dotnet tool restore` independently and records its own evidence, as the plan requires.
