# Remediation Inputs: Issue #211 startup-latency attribution instrumentation

**Generated:** 2026-06-23T15-31
**Base branch:** `main` (`9385bf607aca6c5722f2da7961a895c685710942`)
**Head:** `bug/outlook-startup-intelconfig-continuation-stall-211` (`e3a84b5dc4544aaf8b498dfed4e7b45708c9c12a`)
**Work mode:** `full-bug` (AC source: `spec.md`)

## Source Audit Artifacts

- Policy audit: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/policy-audit.2026-06-23T15-31.md`
- Code review: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/code-review.2026-06-23T15-31.md`
- Feature audit: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/feature-audit.2026-06-23T15-31.md`

## Why this file exists

The feature-review workflow requires `remediation-inputs.<timestamp>.md` when the policy audit contains PARTIAL results or the feature audit contains FAIL/PARTIAL criteria. Two ACs are FAIL (AC9, AC10) and two are PARTIAL (AC4, AC8). This file records each remediation-required finding. Note: none of these findings is a defect in the delivered code — the code review records zero Blocker/Major findings and the C# toolchain is green. The findings are unmet acceptance criteria that are either maintainer-runtime tasks, evidence-gated, or PR-CI-gated, and are therefore not all addressable by an automated atomic-planner handoff.

## Remediation-required findings

### RF-1 (FAIL, AC9) — maintainer non-debugger per-engine attribution capture is missing

- **Finding:** Only `evidence/other/runtime-capture-engines-nondebugger-PLACEHOLDER.md` exists; no real non-debugger cold-start capture of the new `[engine-init]`/`[engine-init-config]` lines identifies the dominant Engines-phase engine(s)/resource(s).
- **Owner/type:** Maintainer-run runtime task (not CI-automatable, not addressable by atomic-planner).
- **Required action:** Maintainer performs a non-debugger cold start with the Phase 3 instrumentation, captures the per-engine attribution lines via DebugView/OutputDebugString, and records the artifact under `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/other/`, replacing the placeholder.
- **Artifact path on completion:** `evidence/other/runtime-capture-engines-nondebugger-<timestamp>.md`.

### RF-2 (FAIL, AC10) — evidence-gated Phase 4 fix not implemented

- **Finding:** No Phase 4 TaskMaster-side fix, no fix-invariant unit test, and no reduction re-capture exist.
- **Owner/type:** Evidence-gated on RF-1 (AC9). Cannot start until AC9 attributes the dominant cost. If AC9 attributes the cost to a non-TaskMaster external cause, the required output is documentation of that finding rather than a code change.
- **Required action (after RF-1):** Hand off to atomic-planner to implement the minimal indicated fix (for example deferring non-critical engine init off the startup critical path via `IdleAsyncQueue`, parallelizing independent model loads, or caching deserialized models) with the required invariant unit test, then maintainer re-captures to confirm the latency reduction.
- **Blocking dependency:** RF-1 must complete first.

### RF-3 (PARTIAL, AC4/AC8) — repo-wide coverage floor confirmation is PR-CI-gated

- **Finding:** The deterministic full-suite aggregate is 64.05% (baseline 64.04%, no regression), below the 80% raw floor. The 80% floor applies to the post-exemption testable denominator; the authoritative repo-wide determination is the PR CI run, not available locally.
- **Owner/type:** PR-CI verification (not a code defect; new-code coverage is 100% and there is no regression).
- **Required action:** Confirm repo-wide coverage against the post-exemption testable denominator via the PR CI run before closing the coverage gate. No source change is indicated unless CI reveals a genuine post-exemption shortfall attributable to this change.

## Atomic-planner handoff disposition

- RF-1: NOT handed off (maintainer runtime task).
- RF-2: NOT handed off yet (blocked on RF-1 evidence; hand off after AC9 attribution exists).
- RF-3: NOT handed off (PR-CI verification, not a code change).

No atomic remediation plan is created at this time because no remediation-required finding is currently a code-implementable task: RF-1 and RF-3 are runtime/CI verification tasks, and RF-2 is strictly evidence-gated on RF-1. When the AC9 capture exists and attributes the cost to a TaskMaster-side cause, RF-2 should be handed to the atomic-planner per `remediation-handoff-atomic-planner`.

## Merge disposition

The delivered Phase 1 + Phase 3 instrumentation is mergeable as a diagnostic increment (Conditional Go): toolchain green, new-code coverage 100%, no regression, no Blocker/Major code findings. Issue #211 must remain OPEN because its objective (eliminate the multi-minute startup latency) is unmet pending RF-1 and RF-2.
