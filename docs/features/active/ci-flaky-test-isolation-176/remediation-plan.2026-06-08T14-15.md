# ci-flaky-test-isolation — Remediation Plan (Issue #176)

- **Issue:** #176
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-08T14-15
- **Status:** Draft
- **Version:** 0.1
- **Authoritative inputs:** `docs/features/active/ci-flaky-test-isolation-176/remediation-inputs.2026-06-08T14-15.md`

**Fail-closed evidence rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

**Scope note:** This plan contains no code-change phases. The audits found zero blocking code findings; AC1-AC6 are verified PASS. The only open acceptance criterion (AC7) is an external-CI confirmation gate that cannot be remediated by source change.

**Phase 0 — Context & Inputs**
- [ ] [P0-T1] Link source audits: `policy-audit.2026-06-08T14-15.md`, `code-review.2026-06-08T14-15.md`, `feature-audit.2026-06-08T14-15.md`, `remediation-inputs.2026-06-08T14-15.md`.
- [ ] [P0-T2] Record current head: `bug/ci-flaky-test-isolation-176` @ `92e35bcd`; PR #177 into `main`.

**Phase 1 — PR CI Confirmation (AC7, part 1)**
- [ ] [P1-T1] Run `gh pr checks 177 --watch` (or inspect the PR #177 Actions run). Confirm all steps pass, including "Run MSTest suite with coverage". Record the run databaseId and conclusion.
- [ ] [P1-T2] Capture the green-run evidence under `evidence/qa-gates/<timestamp>/` (run id + conclusion).

**Phase 2 — Post-merge main CI Confirmation (AC7, part 2)**
- [ ] [P2-T1] After merge, confirm the post-merge `main` Actions run is green (the workflow that failed as run 27138963879). Record run id and conclusion.
- [ ] [P2-T2] On confirmation, check off AC7 (`[x]`) in `spec.md` per `acceptance-criteria-tracking`.

**Phase 3 — Follow-up (tracked separately)**
- [ ] [P3-T1] Port the two test-isolation fixes to `development` to prevent reintroduction on the next `development` -> `main` merge (per `spec.md` Rollout & Follow-up). Track as a separate change, not part of PR #177.
