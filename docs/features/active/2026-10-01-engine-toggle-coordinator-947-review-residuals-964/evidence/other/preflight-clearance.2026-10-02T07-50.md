# Preflight Clearance — Issue #964

- Timestamp: 2026-10-02T07-50
- Issue: #964 (engine-toggle-coordinator-947-review-residuals)
- Branch: bug/engine-toggle-coordinator-947-review-residuals-964
- Base: origin/main at 94287369908cc920b21b0e3256314f988ad7d2f5
- Work Mode: minor-audit
- Plan: docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/plan.2026-10-02T05-20.md (version 1.3)
- Plan blob SHA cleared: 7f46d91cb534de5b0abade1af87f0b3825a85433
- Plan validator: mcp__drm-copilot__validate_orchestration_artifacts (artifact_type plan) returned ok with no warnings on the cleared blob.

## Signal

PREFLIGHT: ALL CLEAR

CONVERGENCE: NO FURTHER ROUNDS EXPECTED

## Rounds

- Round count: 4
- Round 1 (plan version 1.0, blob 9ca203e423940ed37cf386b1f453f13eb5fe11d6): PREFLIGHT: REVISIONS REQUIRED, 3 defects.
  - The pre-existing-failure comparison for AC8 read short trx test names; replaced by a fully qualified name set (`FAILED-FQN` rows) compared verbatim against the Phase 0 baseline.
  - P1-T5, P1-T11 and P1-T21 ran commands without naming an evidence artifact; each now creates its evidence file and the sibling tasks append to it.
  - The host-path hygiene sweep ran before the last artifacts were written; P2-T20 adds a final sweep.
- Round 2 (plan version 1.1, blob eadf8998db4e040fe3d09891c120fcee65d7ca9e): PREFLIGHT: ALL CLEAR, 0 defects (full-plan re-check).
- Orchestrator delta after round 2 (plan version 1.2, blob 50a7ab21390138005989f654c51d33cb645adabf): P2-T19 asserted that no file outside the Write Set evidence list exists under the evidence folder, which this committed record would contradict. The plan now names this record as a preparation-phase file, pins its blob at P0-T2 and checks it unchanged at P2-T19.
- Round 3 (plan version 1.2): PREFLIGHT: REVISIONS REQUIRED, 3 defects.
  - `CLEARANCE-PATH:` must be recorded in forward-slash form, because `git rev-parse HEAD:<path>` does not resolve a backslash-separated path.
  - The hygiene sweep printed totals only, so a hit could not be attributed to a file; it now prints one `HIT-FILE` row per file with a non-zero count.
  - The P2-T9 and P2-T20 lower bounds now count this record (33 and 36), so a single missing evidence file fails them.
- Round 4 (plan version 1.3, blob 7f46d91cb534de5b0abade1af87f0b3825a85433): PREFLIGHT: ALL CLEAR, 0 defects.

## Execution Notes

- This record is committed by the preparation run before execution and must not be modified afterwards: P0-T2 records its blob and P2-T19 requires the working-tree hash to match.
- The executor must be dispatched without worktree isolation, because the Bash-tool isolation guard refuses every `pwsh` invocation in an isolated agent and every toolchain step in the plan is a pwsh payload.
