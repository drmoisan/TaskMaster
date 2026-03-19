# Remediation Plan — 2026-03-19-outlook-folder-wrapper-tests-82 (2026-03-19T17-56)

- **Issue:** #82
- **Owner:** drmoisan
- **Primary requirements source:** `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/remediation-inputs.2026-03-19T17-56.md`
- **Secondary scoping docs:** `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/spec.md`, `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/user-story.md`
- **Status:** Draft
- **Last Updated:** 2026-03-19T17-56
- **Note:** The usual automatic `atomic_planner` handoff is not available in this tool surface, so this remediation plan was created directly as a best-effort stand-in to avoid leaving the feature folder incomplete.

## Overview

This remediation narrows the current branch back to the documented `#82` folder-wrapper feature scope and removes or formally resolves the remaining `MSB3277` build-warning signal in `UtilitiesCS.Test`. The scoped feature implementation itself already passes its acceptance criteria; the work here is branch packaging and final PR-readiness cleanup.

### Phase 0 — Baseline refresh

- [ ] [P0-T1] Read `remediation-inputs.2026-03-19T17-56.md`, `policy-audit.2026-03-19T17-56.md`, `code-review.2026-03-19T17-56.md`, `feature-audit.2026-03-19T17-56.md`, and `plan.2026-03-19T09-43.md`.
  - **Acceptance:** All remediation requirements and original feature boundaries are restated before branch cleanup begins.

- [ ] [P0-T2] Capture the current branch scope against `origin/development`.
  - **Acceptance:** Save refreshed branch-scope evidence showing the current oversized diff before cleanup work starts.

### Phase 1 — Branch isolation

- [ ] [P1-T1] Create a clean branch from `development` for the `#82` folder-wrapper feature.
  - **Acceptance:** The new branch starts from `origin/development` (or an updated local `development` aligned to it).

- [ ] [P1-T2] Cherry-pick or reconstruct only the intended `#82` files onto the clean branch.
  - **Acceptance:** The branch contains only the documented folder-wrapper production/test/doc files and no unrelated carryover from older feature ancestry.

- [ ] [P1-T3] Refresh `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` for the cleaned branch.
  - **Acceptance:** The refreshed PR context now matches the active feature folder’s documented scope.

### Phase 2 — Warning cleanup

- [ ] [P2-T1] Investigate the `MSB3277` `System.Reflection.Metadata` warning reported by `UtilitiesCS.Test`.
  - **Acceptance:** Root cause is documented and tied to the exact package/project references involved.

- [ ] [P2-T2] Either resolve the conflicting reference versions or document an explicit approved exception.
  - **Acceptance:** Review-time analyzer and nullable validation no longer produce an unexplained `MSB3277` warning.

### Phase 3 — Final validation

- [ ] [P3-T1] Run the repo validation loop on the cleaned branch.
  - **Acceptance:** Formatter evidence, analyzer build, nullable build, MSTest, and coverage evidence all reflect the cleaned branch state.

- [ ] [P3-T2] Refresh the review artifacts after branch cleanup.
  - **Acceptance:** `policy-audit`, `code-review`, and `feature-audit` no longer report branch-scope mismatch relative to `development`.

### Phase 4 — Status synchronization

- [ ] [P4-T1] Sync the original feature plan and supporting docs to the cleaned-branch outcome.
  - **Acceptance:** Any now-completed status items are checked off accurately and the feature docs reflect the final PR-ready state.

- [ ] [P4-T2] Confirm final branch readiness for PR creation.
  - **Acceptance:** Refreshed `pr_context`, validation evidence, and audits all agree on the same scoped change set.
