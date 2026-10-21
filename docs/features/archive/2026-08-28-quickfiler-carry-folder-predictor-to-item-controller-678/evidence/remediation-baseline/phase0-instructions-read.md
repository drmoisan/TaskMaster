# Phase 0 — Policy Documents Read (Remediation Cycle 1)

- Timestamp: 2026-09-02T01-02
- Issue: #678
- Cycle: remediation cycle 1
- Task: [P0-T1]

## Policy Order

The reading order is the one defined by `.claude/skills/policy-compliance-order/SKILL.md`:
standing instructions first, then the cross-language code-change policy, then the
cross-language unit-test policy, then the language-specific rules for the files in scope
(C# only for this cycle), then the supporting rule files this plan's acceptance conditions
are written against.

## Files read, in order

1. `CLAUDE.md` — standing repository instructions, including the C# toolchain command set,
   the four-step toolchain loop, the coverage floors (>= 80 percent repository-wide,
   >= 90 percent for new modules/classes/methods) and the COM/VSTO/WinForms coverage
   exemption.
2. `.claude/rules/general-code-change.md` — design principles, module rigor tiers, the
   mandatory toolchain loop, the 500-line file-size limit, error handling and logging,
   naming, public-API compatibility, dependencies and I/O boundaries.
3. `.claude/rules/general-unit-test.md` — the five core unit-test principles, coverage
   requirements and the coverage exclusion policy, scenario completeness,
   Arrange-Act-Assert structure, external-dependency prohibitions, test file location,
   test categories and determinism infrastructure (banned APIs in test code, controllable
   clock, seeded RNG).
4. `.claude/rules/csharp.md` — the C#-specific toolchain (CSharpier, analyzer build,
   nullable build, MSTest with coverage), coding standards, deterministic test rules, DI
   seams including the `TimeProvider` time seam, the five-package analyzer stack, the
   severity-first ordering invariant, the deferred SecurityCodeScan decision, and the
   prohibited behaviors list.
5. `.claude/rules/quality-tiers.md` — the T1 through T4 tier definitions, the
   `quality-tiers.yml` source of truth, and the uniform-versus-tier-dependent gate matrix.
6. `.claude/rules/tonality.md` — required professional tone, the prohibitions on humor and
   hyperbole, the restriction on metaphor, evidence-first wording, and the handling of
   difficult messages.
7. `.claude/rules/plan-acceptance-gates.md` — acceptance-gate rules G1 through G9, the
   attribution window, the write-mode register and its membership criterion, the
   checkable-literal definition and placeholder guard, the deliberately uncovered
   sub-classes (the general unobservable-success-output class, the task-ordering class,
   and the rejected executor-choice heuristic), and the authoring guidance for plan
   authors.

All seven files were read in full before any task in Phase 1 or Phase 2 of this plan was
started.

## Conflicts observed

No conflicting instruction was found between the seven documents and the remediation plan
`remediation-plan.2026-09-01T23-44.md`. The plan's toolchain command set is
character-for-character the set `CLAUDE.md` and `.claude/rules/csharp.md` both prescribe,
including `/t:Rebuild` for the two gate builds and the prohibition on `/p:Nullable=enable`.

## Output Summary

Seven policy documents read in the prescribed order. No conflict detected. Toolchain
command set confirmed against `CLAUDE.md` and `.claude/rules/csharp.md`.
