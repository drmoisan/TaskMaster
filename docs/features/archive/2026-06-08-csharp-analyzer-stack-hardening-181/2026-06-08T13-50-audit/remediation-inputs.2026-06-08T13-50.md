# Remediation Inputs: csharp-analyzer-stack-hardening (Issue #181)

- Cycle entry timestamp: 2026-06-08T13-50
- Feature folder: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
- Base branch: `main` @ `2a522ed831865c2918ab02df153ef2929b0617dc`
- Head branch: `feature/csharp-analyzer-stack-181` @ `71e0777ada475c408d85d3b6c68e6192b4bc070b`
- Work mode: `full-feature`

## Source Audit Artifacts (findings origin)

- `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/policy-audit.2026-06-08T13-50.md`
- `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/code-review.2026-06-08T13-50.md`
- `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/feature-audit.2026-06-08T13-50.md`

## Findings Requiring Remediation

The branch has no FAIL findings and no `.cs` code defects. The two open items are verification gates, not code fixes:

1. **AC6 — PR CI GREEN (UNVERIFIED).** No GitHub PR exists for `feature/csharp-analyzer-stack-181` and no CI run is recorded against the branch head. The authoritative repo-wide 80% coverage gate and the nullable-as-errors / MSTest-with-coverage CI steps are unverified.
   - Evidence: `gh pr list --head feature/csharp-analyzer-stack-181 --state all` (empty); `gh run list --branch feature/csharp-analyzer-stack-181` (empty); `artifacts/pr_context.summary.txt` CI status "(not available)".
   - Expected behavior: a GitHub Actions CI run against the branch head completes GREEN, including the nullable `/p:TreatWarningsAsErrors=true` build at the vendored-only 84-error baseline (0 first-party errors, 0 CS8032) and the MSTest-with-coverage step passing the repository's scoped 80% repo-wide / 90% new-code gates.
   - Verification commands: `gh pr create ...` (via the `pr-author` skill flow; do not bypass with a hand-written body), then `gh pr checks <pr>` / `gh run list --branch feature/csharp-analyzer-stack-181` showing all required checks GREEN.

2. **AC5 — Local toolchain stages / nullable no-regression (PARTIAL).** The local toolchain does not reach a fully-green single pass; format, nullable, and test sit at the documented non-green Phase 0 baseline (1 pre-existing CSharpier `.cs` finding in `UtilitiesCS\Extensions\IEnumerableExtensions.cs`; 84 vendored nullable errors; 7 flaky wall-clock-timer tests). The protected nullable gate does NOT regress (84 = baseline, 0 first-party errors, 0 CS8032).
   - Evidence: `evidence/qa-gates/final-format.2026-06-08T12-12.md`, `evidence/qa-gates/final-nullable-build.2026-06-08T12-12.md`, `evidence/qa-gates/final-test-coverage.2026-06-08T12-12.md`.
   - Expected behavior: the no-regression condition is corroborated by the authoritative CI environment when AC6's CI run completes. No local code change is required; the baseline non-green conditions are pre-existing and out of scope for this build-config-only feature.
   - Verification commands: same CI run as AC6; specifically the nullable build error count equals the Phase 0 baseline and the MSTest coverage step passes.

## Fix List (file paths, expected behavior, verification)

This cycle requires no source-code edits. The remediation actions are:

- Action R1 (process): Open the PR for `feature/csharp-analyzer-stack-181` against `main` using the `pr-author` skill flow (canonical body file + SHA-256 provenance receipt; the `enforce-pr-author-skill.ps1` hook will block a hand-written `--body`). File touched: PR body artifact only (no repo source). Verification: PR exists (`gh pr view`).
- Action R2 (process): Confirm a GREEN GitHub Actions CI run against the branch head. Files touched: none. Verification: `gh pr checks` / `gh run list` shows all required checks GREEN; capture the run URL and result into `evidence/qa-gates/`.
- Action R3 (evidence): Record the CI green result as a feature-review-consumable evidence artifact under `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/ci-green.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (the run URL and per-check status).

## Do Not Do (scope guard)

- Do NOT introduce any CS8032 suppression (`dotnet_diagnostic.CS8032`, `<WarningsNotAsErrors>` containing CS8032) to force the nullable gate green; the 84-error vendored baseline is the accepted state.
- Do NOT touch the two vendored projects (SVGControl, UtilitiesSwordfish.NET.General) or attempt to fix their pre-existing nullable errors.
- Do NOT reformat or edit `UtilitiesCS\Extensions\IEnumerableExtensions.cs` to silence the pre-existing CSharpier baseline finding; it is out of scope.
- Do NOT promote RS0030 or any new analyzer rule from suggestion to warning/error in this cycle (would risk breaking the protected nullable gate).
- Do NOT add SecurityCodeScan or any substitute security analyzer; the deferral stands.
- Do NOT modify `.claude/rules/` policy documents other than the already-delivered `.claude/rules/csharp.md` content.
- Do NOT bypass the `pr-author` skill to create the PR.
- Do NOT modify application `.cs` logic; this is a build-config/documentation feature.

## Cycle Artifacts (this remediation cycle)

1. `remediation-inputs.2026-06-08T13-50.md` (this file) — authored at cycle entry.
2. `remediation-plan.2026-06-08T13-50.md` — atomic-planner authors / refines at cycle entry (target plan file created below).
3. `code-review.<exit-ts>.md`, `feature-audit.<exit-ts>.md`, `policy-audit.<exit-ts>.md` — feature-review authors at cycle exit after CI is green.

## Handoff

Per `remediation-handoff-atomic-planner`: hand off to `atomic-planner` to finalize `remediation-plan.2026-06-08T13-50.md`, then `atomic-executor` preflight (`DIRECTIVE: PREFLIGHT VALIDATION ONLY`) and execution, then `feature-review` reaudit at the exit timestamp. The exit gate is `blocking_count == 0` (AC6 PASS, AC5 corroborated by CI).
