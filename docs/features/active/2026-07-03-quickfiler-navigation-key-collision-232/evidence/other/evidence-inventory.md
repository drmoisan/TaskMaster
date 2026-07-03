# Evidence Inventory (Issue #232)

Timestamp: 2026-07-03T13-50

All evidence artifacts referenced across Phases 0-5 of `plan.2026-07-03T10-36.md` are present on disk under
`docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/`.

## Phase 0 — Baseline
- `evidence/baseline/phase0-instructions-read.md` — present
- `evidence/baseline/git-baseline.md` — present
- `evidence/baseline/csharpier-baseline.md` — present
- `evidence/baseline/msbuild-analyzers-baseline.md` — present
- `evidence/baseline/msbuild-nullable-baseline.md` — present
- `evidence/baseline/vstest-baseline.md` — present
- `evidence/baseline/pre-fix-source-confirmation.md` — present

## Phase 1-3 — Part A regression testing / scope
- `evidence/regression-testing/reported-repro.expect-fail.md` (P1-T2) — present
- `evidence/regression-testing/reported-repro.pass-after-fix.md` (P2-T3) — present
- `evidence/regression-testing/swap-register-unregister-order.pass.md` (P3-T2) — present
- `evidence/regression-testing/double-registration-guard.pass.md` (P3-T5) — present
- `evidence/other/ac8-scope-confirmation.md` (P3-T6) — present

## Phase 4 — Part B additive logging
- `evidence/regression-testing/part-b-logging-no-regression.md` (P4-T6) — present

## Phase 5 — Final QA gates
- `evidence/qa-gates/csharpier-final.md` (P5-T1) — present
- `evidence/qa-gates/msbuild-analyzers-final.md` (P5-T2) — present
- `evidence/qa-gates/msbuild-nullable-final.md` (P5-T3) — present
- `evidence/qa-gates/vstest-final.md` (P5-T4) — present
- `evidence/qa-gates/coverage-delta.md` (P5-T5) — present

## Phase 6 — Closeout
- `evidence/other/follow-up-candidates.md` (P6-T4) — present
- `evidence/other/evidence-inventory.md` (P6-T3, this file) — present

## Working tree state (`git status --porcelain`)

```
 M .claude/agent-memory/task-researcher/MEMORY.md
 M QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs
 M QuickFiler/Controllers/QfcCollectionController.cs
 M QuickFiler/Controllers/QfcDatamodel.cs
 M QuickFiler/Controllers/QfcHighConfidencePreFilter.cs
 M QuickFiler/Controllers/QfcItemController.FolderHandling.cs
?? .claude/agent-memory/task-researcher/project_qfc_high_confidence_dual_pipeline.md
?? docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/
```

Confirmation: the tracked modifications are exactly the four production files and one test file this change
targets:
- Part A: `QuickFiler/Controllers/QfcCollectionController.cs` + `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
- Part B: `QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`,
  `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`

The two `.claude/agent-memory/task-researcher/*` entries (one modified index, one new memory file) and the
untracked feature folder `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/` were
present at the Phase 0 baseline (`evidence/baseline/git-baseline.md`) and are unrelated to the production/
test code touched by this change. No unintended files outside the feature folder and the five
production/test files were created or modified.
