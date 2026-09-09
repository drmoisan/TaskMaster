# AC20 — Documentation Boundary

Timestamp: 2026-09-09T17-26

Command: git add --intent-to-add -- . ":(exclude).claude"; git diff --name-only $b -- docs/features/
EXIT_CODE: 0

Command: git status --porcelain --untracked-files=all -- docs/features/
EXIT_CODE: 0

DiffPaths: 42
StatusPaths: 42
PathsOutsideFeatureFolder: 0
PathsUnderEpics: 0
PathsUnderPotential: 0

Output Summary: Every path either command reports begins with
docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/. No reported path begins with
docs/features/epics/ or docs/features/potential/, and none names any other active feature folder.
The two commands agree on 42 paths.

The intent-to-add span is required because a name-listing diff enumerates tracked changes only and
would otherwise report none of the evidence files this plan creates; all 40 of them are new. It is
run with the D4 exclusion pathspec so the tracked .claude/agent-memory tree, which this executor
writes to during the run, does not enter the staging set.

## Reported paths

The 42 paths are: 40 newly created files under this feature's evidence/ tree, together with two
modifications to documents this feature owns, plan.2026-09-08T23-51.md and spec.md. The plan
modification is the task check-off progression; the spec modification is the acceptance-criteria
check-off progression, in which only the `- [ ]` to `- [x]` box state changed and no criterion text
was altered.

The 40 evidence files are, under evidence/baseline/: base-commit.md, build-analyzers.md,
build-analyzers.txt, build-nullable.md, build-nullable.txt, coverage-baseline-by-file.md,
coverage-baseline.cobertura.xml, coverage-baseline.md, csharpier-check.md, dotnet-tool-restore.md,
phase0-instructions-read.md, pinned-source-facts.md and restore.md; under evidence/other/:
ac21-justification.md, ac7-mechanism-note.md, ac8-createcancellationtokensource-proof.md,
ac8-createcancellationtokensource-proof.txt, file-size-accounting.md and plan-deviations.md; under
evidence/qa-gates/: ac11-etlasync-tuple.md, ac19-historical-records.md, ac23-attributes-retained.md,
ac26-ac27-boundary.md, ac32-non-vacuity.md, ac33-changed-line-coverage.md,
ac33-coverage-comparison.md, ac6-ac20-amended-spec-verification.md, coverage-postchange.cobertura.xml,
qc-build-analyzers.md, qc-build-analyzers.txt, qc-build-nullable.md, qc-build-nullable.txt,
qc-coverage-postchange.md, qc-csharpier-check.md and qc-csharpier-format.md; and under
evidence/regression-testing/: ac7-fail-before.md, phase3-green.md, phase4-green.md, phase5-green.md
and phase7-green.md.

## What the zero counts establish

The binding clause of AC20 is that no promotion record, no potential entry, no epic manifest edit and
no sibling feature folder appears in this feature's diff. PathsUnderPotential is 0, so no promotion
record was written on this branch, which is also what AC35 requires and what
evidence/other/ac35-reachability-observation.md records as
`PromotionWrittenOnThisBranch: false`. PathsUnderEpics is 0, so no epic manifest was edited.
PathsOutsideFeatureFolder is 0, so no sibling feature folder was touched.
