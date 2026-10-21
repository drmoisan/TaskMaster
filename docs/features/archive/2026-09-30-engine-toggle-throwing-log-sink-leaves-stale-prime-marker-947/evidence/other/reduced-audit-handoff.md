# Reduced-Audit Handoff (issue 947, P2-T23)

Timestamp: 2026-10-01T18-16
Work Mode: minor-audit (acceptance criteria source: issue.md `## Acceptance Criteria` only)

## Reduced-audit artifact list

- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/phase0-instructions-read.md
- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/coverage-baseline.md
- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-fail-before.md
- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-pass-after.md
- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/toolchain-final-pass.md
- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/coverage-summary.md
- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/footprint-scope.md
- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/other/ac-status-summary.md

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

## Footprint (from P2-T13)

DIFF-FOOTPRINT code paths:
- M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
- M TaskMaster.Test/TaskMaster.Test.csproj
- A TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs

PORCELAIN-FOOTPRINT code paths: none (all three code paths are committed at caeb82c40 and unmodified since; the porcelain span lists only feature-folder paths).

## Anchors

- BASE-SHA: 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85
- COVERAGE-ROUTE: DIRECT (STALL-PROBE: REPRODUCES at P0-T12)
- HANDOFF-HEAD: caeb82c4037fc52b9c2a95ef4ffa690a744f4baa (observation). This differs from BASE-SHA, which the plan expected to equal HEAD, because the orchestrator committed Phase 0 (433d5c2e2) and Phase 1 (caeb82c40) before this phase. Phase 2 itself committed nothing.

## Scope statements

- Research section 7 item 1 (the click-boundary sink call in `HandleToggleClickAsync`) was brought into scope by the maintainer consolidation of 2026-10-01 and is fixed by edits E5 and E6, so this plan requests no promotion for it.
- Research section 7 item 2 is the open #944 follow-up on log volume; this plan neither changes it nor re-files it.
- The committed test evidence consists of projections and summaries only: the `First-party coverage:` line, the JaCoCo package projection, the trx-derived summary and the per-method and per-arm figures, all transcribed into Markdown. No raw trx, raw coverage document or msbuild log is in the feature folder.

## Literal-clause divergences for the reviewer

Each divergence below follows from the mid-plan commits. Each is recorded in its artifact together with the paired BASE-SHA diff observation:
- prime-fault-ordering-identity.md (P2-T11): the TaskMaster.Test/Ribbon anchored diff exits 1, not 0, and the Ribbon porcelain is empty rather than one `??` line. The paired name-status diff lists exactly `A ...ThrowingSink.cs`.
- footprint-scope.md (P2-T13): the diff lists the partial as `A`; no `??` line for it; FEATURE-STATUS-LINES 14 rather than at least 34.

## Coordinator ruling on the P2-T11 and P2-T13 divergence

Timestamp: 2026-10-01T19-00
Ruling: ACCEPTED as a re-anchoring under the coordinator's standing authority. The literal untracked-file and porcelain-count clauses of P2-T11 and P2-T13 assumed nothing would be committed before the end of the plan; Phases 0 and 1 were committed at 433d5c2e2 and caeb82c40, so those clauses cannot hold as written. The anchored BASE-SHA diff that each task pairs with them shows exactly one added test partial (`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs`), no existing test file changed, and no path outside the Write Set. Each check stays able to fail: a modified existing test file or a path outside the Write Set would appear in the anchored diff. No acceptance criterion changes intent.
