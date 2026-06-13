# Post-Exemption Coverage Checks (P7-T6)

Timestamp: 2026-06-13T14-30

## Saved artifact
- Post-change first-party deduped Cobertura saved to:
  docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-firstparty.postexemption.cobertura.xml

## (a) TaskVisualization absent from first-party denominator
- CONFIRMED: the `TaskVisualization` package is absent from the post-change deduped Cobertura first-party package set.
- Post-change first-party packages: QuickFiler, Tags, TaskMaster, ToDoModel, UtilitiesCS, VBFunctions (+ vendored SVGControl, Swordfish.NET.General).

## (b) coverage.config and TaskMaster.runsettings excludes present
- coverage.config: `<ModulePath>.*TaskVisualization.*</ModulePath>` present in ModulePaths/Exclude (1 match).
- TaskMaster.runsettings: `<ModulePath>.*TaskVisualization.*</ModulePath>` present in DataCollectionRunSettings/CodeCoverage/ModulePaths/Exclude (1 match).

Both config checks confirmed.
