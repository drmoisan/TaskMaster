# P4-T26 — AC14 evidence-path override record

Timestamp: 2026-09-01T20-21
Command: `Test-Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/coverage'`, plus verification that the substituted artifacts exist at their canonical paths
EXIT_CODE: 0

## The override

    EVIDENCE_LOCATION_OVERRIDE_REJECTED: docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/coverage/ replaced with docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/baseline/ (Phase 0 coverage) and docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/ (Phase 4 coverage)

AC14's criterion text states that the Phase 0 and post-change coverage artifacts are both stored under `evidence/coverage/`. That is **not a canonical evidence kind**. The canonical kinds defined by `evidence-and-timestamp-conventions` are `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other` and `remediation-baseline`, and the non-overridable evidence-path clause of `atomic-plan-contract` states that no upstream instruction may introduce another. The instruction is therefore rejected and the canonical kinds are substituted.

## Substituted locations, and the artifacts at them

**Phase 0 coverage → `evidence/baseline/`**

- `evidence/baseline/baseline.cobertura.xml` — the Cobertura document captured before any edit in this delivery run
- `evidence/baseline/p0-t12-vstest-coverage.md` — the run record, carrying the exit code, the discovery line, the test counts and the `POSTPROCESSED` flag
- `evidence/baseline/p0-t13-coverage-counters.md` — the derived counters `BASELINE_LINES_COVERED`, `BASELINE_LINES_VALID` and `BASELINE_LINE_PERCENT`

**Phase 4 coverage → `evidence/qa-gates/`**

- `evidence/qa-gates/postchange.cobertura.xml` — the post-change Cobertura document
- `evidence/qa-gates/p4-t5-vstest-coverage.md` — the run record with the same field set
- `evidence/qa-gates/p4-t6-coverage-counters.md` — the derived counters `POSTCHANGE_LINES_COVERED`, `POSTCHANGE_LINES_VALID` and `POSTCHANGE_LINE_PERCENT`
- `evidence/qa-gates/p4-t8-coverage-delta.md` — the comparison that discharges AC14's substantive requirement

Both substituted kinds are named above, as this task requires.

## No `evidence/coverage/` directory was created

    Test-Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/coverage'
    False

The non-canonical directory does not exist. The override was applied by writing to the canonical locations from the outset rather than by creating the directory and moving files afterwards.

## AC14's criterion text was not edited

`acceptance-criteria-tracking` rule 3 permits changing only `- [ ]` to `- [x]` and explicitly prohibits an executor from modifying criterion text. The superseded `evidence/coverage/` spelling therefore remains in `spec.md` exactly as authored. It is discharged by this record together with the artifacts enumerated above, which is the handling the non-overridable evidence-path clause prescribes — the substance of AC14 is the no-regression comparison, and the storage location named in its text is a detail that a governance rule overrides.

## AC14's substantive requirement is satisfied

AC14 requires that repository-wide line coverage not regress relative to the Phase 0 baseline captured before any edit. From `evidence/qa-gates/p4-t8-coverage-delta.md`:

    BASELINE_LINES_COVERED   = 54983      POSTCHANGE_LINES_COVERED = 54988   (+5)
    BASELINE_LINE_PERCENT    = 85.3866    POSTCHANGE_LINE_PERCENT  = 85.3771  (-0.0095 pp)
    BASELINE_POSTPROCESSED   = yes        POSTCHANGE_POSTPROCESSED = yes

Both stated gates pass: the covered-line count rose, and the ratio is inside the 0.10-percentage-point band. The two documents are in the same post-processing state, so their denominators are comparable. The three changed lines in `QfcItemController.Initialization.cs` are covered before and after, so no changed line regressed.

Output Summary: AC14's stated storage location `evidence/coverage/` is non-canonical and is superseded by `evidence/baseline/` for the Phase 0 coverage artifacts and `evidence/qa-gates/` for the Phase 4 coverage artifacts. No `evidence/coverage/` directory exists. The criterion text is unedited. AC14's substantive no-regression requirement is satisfied and AC14 is checked off.
