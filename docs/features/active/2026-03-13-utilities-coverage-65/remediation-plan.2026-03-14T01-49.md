# 2026-03-13-utilities-coverage — Remediation Plan

- **Issue:** #65
- **Owner:** drmoisan
- **Last Updated:** 2026-03-14T01-49
- **Status:** Draft
- **Version:** 0.1
- **Source of truth:** `docs/features/active/2026-03-13-utilities-coverage-65/remediation-inputs.2026-03-14T01-49.md`
- **Generation note:** Repo plan template was not present at `docs/features/templates/feature/plan.yyyy-MM-ddTHH-mm.md`, and no automatic `atomic_planner` delegation mechanism was available in this tool environment. This plan was therefore created manually as a best-effort remediation scaffold.

## Objective

Bring the `utilities-coverage` feature into compliance with its documented acceptance criteria by closing the per-file coverage gap, fixing policy violations in the test corpus, and reconciling the Phase 8 QA record with the actual final state.

## Atomic remediation tasks

### Phase 0 — Re-baseline the remediation scope

- [ ] [P0-T1] Re-open `remediation-inputs.2026-03-14T01-49.md`, `spec.md`, and `user-story.md` and derive the exact list of non-excluded below-threshold files from `evidence/qa-gates/final-per-file-coverage.2026-03-14T01-42.md`
- [ ] [P0-T2] Group the remaining failing files by directory / behavior area so remediation can be batched without widening scope unnecessarily

### Phase 1 — Restore coverage compliance

- [ ] [P1-T1] Add or expand tests for the highest-signal low-coverage files in `UtilitiesCS/Extensions/` and `UtilitiesCS/HelperClasses/`
- [ ] [P1-T2] Add or expand tests for the remaining low-coverage files in `UtilitiesCS/Threading/` and `UtilitiesCS/ReusableTypeClasses/`
- [ ] [P1-T3] Add or expand tests for the remaining low-coverage files in `UtilitiesCS/NewtonsoftHelpers/`, `EmailIntelligence/`, and other in-scope directories
- [ ] [P1-T4] Re-run coverage collection and regenerate per-file coverage evidence until all non-excluded files are at or above 80%

### Phase 2 — Fix structural test-policy violations

- [ ] [P2-T1] Split `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` into focused files/classes under 500 lines each
- [ ] [P2-T2] Split `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs`, `BayesianClassifierTests.cs`, and `EmailParsingSorting/MailItemHelperTests.cs` under the 500-line limit
- [ ] [P2-T3] Replace repo-filesystem discovery, wall-clock timestamps, and sleep-based waiting patterns with mocks, fixed values, or synchronization primitives
- [ ] [P2-T4] Normalize changed tests to the preferred FluentAssertions style where practical

### Phase 3 — Re-validate and reconcile docs

- [ ] [P3-T1] Run the full C# toolchain in order: format → analyzers → nullable → tests
- [ ] [P3-T2] Run the coverage pass and regenerate `final-test-coverage`, `final-per-file-coverage`, and `final-coverage-delta` evidence artifacts
- [ ] [P3-T3] Update `plan.2026-03-13T22-21.md` so Phase 8 reflects the true final state after remediation
- [ ] [P3-T4] Confirm the updated feature satisfies all acceptance criteria in `spec.md` and `user-story.md`

## Acceptance gate

This remediation is complete only when all of the following are true in one final reviewable state:

1. Every non-excluded `UtilitiesCS` file in feature scope is at **≥80%** line coverage.
2. The oversized `UtilitiesCS.Test` files are split below the 500-line limit.
3. The affected tests are isolated and deterministic, without repo-filesystem or sleep-based coupling.
4. Format, analyzer build, nullable build, MSTest, and coverage verification all pass.
5. The feature plan and QA evidence accurately describe the final outcome.
