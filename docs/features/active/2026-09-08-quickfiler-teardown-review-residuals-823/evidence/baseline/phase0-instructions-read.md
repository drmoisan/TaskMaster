# Phase 0 — Policy instructions read

Timestamp: 2026-09-09T13-44

Task: [P0-T1]

Policy Order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/csharp.md`, `.claude/rules/tonality.md`, `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/spec.md`

READ: CLAUDE.md (298 lines) — standing project instructions; policy compliance order; C# toolchain (format, analyzers, nullable, vstest); general code-change policy; general unit-test policy; C# code-change and unit-test policies; tone policy.
READ: .claude/rules/general-code-change.md (54 lines) — design principles, module rigor tiers, seven-stage toolchain loop, 500-line file ceiling, error handling, naming, I/O boundaries.
READ: .claude/rules/general-unit-test.md (72 lines) — five core unit-test principles, coverage requirements (line >= 85%, branch >= 75%), coverage exclusion policy, scenario completeness, AAA structure, test file location, determinism infrastructure.
READ: .claude/rules/quality-tiers.md (37 lines) — T1-T4 module rigor tiers; uniform-versus-tier-dependent gate matrix; uniform line coverage >= 85% and branch coverage >= 75%.
READ: .claude/rules/csharp.md (67 lines) — CSharpier formatting via `dotnet tool run`, .NET analyzer command with `/t:Rebuild`, nullable analysis with `/p:TreatWarningsAsErrors=true` and no `/p:Nullable=enable`, MSTest + Moq + FluentAssertions, DI seams, analyzer stack, prohibited behaviors.
READ: .claude/rules/tonality.md (54 lines) — professional tone required; humor, hyperbole and metaphor restricted; evidence-first wording.
READ: docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/spec.md (671 lines) — the sole acceptance-criteria source for this `full-bug` feature; Write Set; scope and non-goals; R1 through R6; AC1 through AC29.

Reading only. Per D21 none of these files was modified, and none is modified by any task of this plan.

CONFLICTS: The two coverage-threshold sources differ and neither is edited or reconciled by this plan.
- `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state a uniform line floor of 85% and a branch floor of 75%.
- `CLAUDE.md` section UT2 states a repository-wide line floor of 80% against a testable denominator, and a 90% floor for new modules, classes and methods.
Per [P6-T9] both are reported as separate dispositions with the measured value beside each; neither is lowered, weakened or deleted, and neither is silently picked over the other. No other conflict was found among the seven files.

Output Summary: All seven files read in the stated order. One recorded conflict between the coverage floors stated by the two rule files and by CLAUDE.md section UT2; it is carried to [P6-T9] as two separate dispositions rather than resolved here. No policy file was modified.
