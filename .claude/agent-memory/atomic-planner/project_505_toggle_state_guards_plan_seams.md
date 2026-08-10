---
name: project-505-toggle-state-guards-plan-seams
description: "#505/#506/#518 bundled full-bug plan facts: runtime red (not compile red), R4 red captured between catalog and XML tasks, coordinator+EngineToggleCatalog seam, raw cobertura to gitignored coverage/, manual-verification evidence kind accepted"
metadata:
  type: project
---

Plan seams for the #505/#506/#518 bundled `full-bug` delivery (plan.2026-08-08T19-22.md in feature 2026-08-08-ribbon-engine-toggle-state-guards-505).

**Why:** three causally coupled defects in `RibbonViewer.EngineCommands.cs`; the spec pins a two-guard-shape split (4 toggle sites -> new `EngineToggleStateCoordinator`, 6 command sites -> existing `RunEngineCommandAsync` gate + 6 new `EngineCommandCatalog` entries + XML `getEnabled`).

**How to apply:**
- Unlike #503 (compile-time red + exception dossier), #505's red is a RUNTIME red: reflection shape-pin tests compile against pre-fix code and fail on execution. No fail-before exception dossier needed when a genuine failing run exists. Reflection-invoke keeps R2 compiling across the signature change.
- R4 (existing catalog-derived XML set-equality tests) has no test-code change; its red is captured as a dedicated `[expect-fail]` run task BETWEEN the catalog-extension task and the XML-edit task. "Land atomically" means same commit, not same task — a captured red between the two edits satisfies AC-15 and the atomicity risk.
- `EngineCommandCatalogTests.ControlIds_ContainsExactlyTheEightEngineBackedControlIds` hard-codes the 8-entry set; extending the catalog requires renaming/extending it (research §8 predicted this).
- The `RibbonViewer(RibbonController)` public ctor is field-assignment-only (`Controller.Try` is inside an uninvoked lambda) — safe to construct in a unit test; `RibbonController()` parameterless ctor exists and #507 tests already use it.
- Raw Cobertura goes to the gitignored `coverage\` dir (`.gitignore` `coverage/*`), never under docs/features (81 MB incident) and never `artifacts/csharp/coverage.xml` (SubagentStop hook hard-codes an 85% floor there vs the real 80% policy).
- `evidence/manual-verification/` is an accepted evidence kind (spec AC-22 requires it verbatim; #503 executed with it; not on the forbidden list).
- Type-check gate is CI's `msbuild /t:Rebuild /m ... /p:TreatWarningsAsErrors=true` WITHOUT `/p:Nullable=enable` (issue #522, known-defective); cite #522 in every type-check task so the executor does not "correct" it back to the CLAUDE.md form.
- `Invoke-MSTestWithCoverage.ps1` applies no `\.claude\` filter and derives repoRoot internally, so it is safe in an agent worktree rooted under `.claude\worktrees\`; expected assembly count 9, and 0 discovered = filter bug, never an empty suite.

Related: [[project-503-ribbon-readiness-plan-seams]], [[async-state-machine-coverage-aggregation]], [[csharpier-format-not-pipe-files-gate]].
