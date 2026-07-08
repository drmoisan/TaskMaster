# Remediation Plan: csharp-analyzer-stack-hardening (Issue #181)

- Cycle entry timestamp: 2026-06-08T13-50
- Feature folder: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
- Base branch: `main` @ `2a522ed831865c2918ab02df153ef2929b0617dc`
- Head branch: `feature/csharp-analyzer-stack-181` @ `71e0777ada475c408d85d3b6c68e6192b4bc070b`
- Work mode: `full-feature`
- Requirements source: `user-story.md` (AC1–AC8) and `spec.md` (Definition of Done)
- Inputs: `remediation-inputs.2026-06-08T13-50.md`

## Objective

Close the two open acceptance gates identified by the cycle-1 audit without any source-code change: AC6 (PR CI GREEN, currently UNVERIFIED) and AC5 (local toolchain / nullable no-regression, currently PARTIAL — to be corroborated by the authoritative CI run). No `.cs` edits, no policy weakening, no CS8032 suppression. This is a verification-and-evidence remediation cycle.

## Scope Guard (do-not list)

- No CS8032 suppression of any kind.
- No edits to vendored projects (SVGControl, UtilitiesSwordfish.NET.General).
- No reformat/edit of the pre-existing `UtilitiesCS\Extensions\IEnumerableExtensions.cs` CSharpier baseline finding.
- No severity promotion of RS0030 or any new analyzer rule.
- No SecurityCodeScan or substitute security analyzer.
- No `.claude/rules/` policy edits beyond the already-delivered `csharp.md`.
- PR must be created through the `pr-author` skill flow (no hand-written `--body`).

### Phase 0 — Policy reads and baseline state capture

- [ ] [P0-T1] Read policy files in the required order per `policy-compliance-order` (CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read list. Evidence: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/remediation-baseline/phase0-instructions-read.2026-06-08T13-50.md` with `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [ ] [P0-T2] Capture current PR/CI baseline state for the branch head. Command: `gh pr list --head feature/csharp-analyzer-stack-181 --state all` and `gh run list --branch feature/csharp-analyzer-stack-181`. Evidence: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/remediation-baseline/baseline-ci-state.2026-06-08T13-50.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (expected: no PR, no CI run).
- [ ] [P0-T3] Confirm the working tree is clean and the head commit matches the audited SHA. Command: `git status --porcelain` and `git rev-parse HEAD`. Evidence: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/remediation-baseline/baseline-worktree.2026-06-08T13-50.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

### Phase 1 — Open PR and trigger CI

- [ ] [P1-T1] Author the PR body and SHA-256 provenance receipt from the PR context artifacts using the `pr-author` skill (required before `gh pr create`; enforced by `enforce-pr-author-skill.ps1`). Acceptance: canonical body file and receipt exist; no hand-written `--body` used.
- [ ] [P1-T2] Create the PR for `feature/csharp-analyzer-stack-181` against `main` via `gh pr create --body-file <canonical-body>`. Acceptance: `gh pr view` returns an open PR; record the PR number/URL. Evidence: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/issue-updates/pr-181.2026-06-08T13-50.md` with `Timestamp:`, the PR URL, and `PostedAs: body`.

### Phase 2 — Confirm green CI and record evidence

- [ ] [P2-T1] Wait for and confirm a GREEN GitHub Actions CI run against the branch head. Command: `gh pr checks <pr>` / `gh run list --branch feature/csharp-analyzer-stack-181 --limit 1`. Acceptance: all required checks GREEN, including the nullable-as-errors build (error count equal to the Phase 0 vendored-only baseline = 84, 0 first-party errors, 0 CS8032) and the MSTest-with-coverage step (scoped repo-wide >= 80%, new-code >= 90% where applicable). Evidence: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/ci-green.2026-06-08T13-50.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (run URL, per-check status, coverage headline).
- [ ] [P2-T2] If any required check is RED, stop and open a new remediation cycle with a fresh `remediation-inputs.<new-ts>.md` describing the specific failing check; do not weaken any gate to force green. Acceptance: either CI is GREEN (proceed) or a new cycle is opened (do not self-approve).

### Phase 3 — Final QA verification (no code change; evidence verification only)

- [ ] [P3-T1] Verify no source files changed during this cycle. Command: `git diff 2a522ed831865c2918ab02df153ef2929b0617dc..HEAD -- "*.cs"` (must be empty) and `git status --porcelain` (only new evidence/PR artifacts). Acceptance: zero `.cs` changes; working tree contains only this cycle's evidence/PR artifacts. Evidence: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-nochange-verification.2026-06-08T13-50.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P3-T2] Confirm the canonical Cobertura coverage artifact remains consistent (`artifacts/csharp/coverage.xml`, line-rate 0.5899) and that the CI coverage step (P2-T1) supplies the authoritative scoped repo-wide and new-code figures. Acceptance: post-change coverage recorded numerically from the CI run; no-regression confirmed (post-change >= baseline 58.89% raw; scoped CI figures meet 80/90). Evidence: reference P2-T1 `ci-green` artifact plus `evidence/qa-gates/coverage-delta.2026-06-08T12-12.md`.
- [ ] [P3-T3] Hand off to `feature-review` for cycle-exit reaudit. Acceptance: `feature-review` produces `code-review.<exit-ts>.md`, `feature-audit.<exit-ts>.md`, `policy-audit.<exit-ts>.md` at the exit timestamp; AC6 evaluates PASS and AC5 evaluates PASS (corroborated by CI); `blocking_count == 0`.

## Coverage Evidence Contract

- Baseline coverage: 58.89% lines raw (`evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`); scoped repo-wide baseline is established by CI.
- Post-change coverage: recorded from the CI MSTest-with-coverage run (P2-T1) and the canonical Cobertura (`artifacts/csharp/coverage.xml`, 58.99% raw).
- New/changed-code coverage: N/A — zero production `.cs` lines added or modified in the feature (verified `git diff <base>..<head> -- "*.cs"` empty); the >= 90% new-code obligation is not triggered.
- No-regression: post-change raw 58.99% >= baseline 58.89%; scoped CI figures are the authoritative 80/90 gate.

## Exit Gate

`blocking_count == 0` when AC6 is PASS (green CI run recorded) and AC5 is PASS (no-regression corroborated by CI). On any RED required check, open cycle N+1; do not self-approve and do not weaken any gate.
