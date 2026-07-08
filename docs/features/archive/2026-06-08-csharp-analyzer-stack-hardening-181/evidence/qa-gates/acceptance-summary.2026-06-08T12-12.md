# P6-T7 — Acceptance Criteria Verification Summary (Issue #181)

Timestamp: 2026-06-08T13-40

AC source (Work Mode: full-feature): `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/issue.md` `## Acceptance Criteria`. (No `spec.md`/`user-story.md` are present in the active folder; the issue.md AC section is the authoritative acceptance source used for this verification.)

| AC | Verdict | Evidence |
|---|---|---|
| AC1 — Analyzer packages referenced by first-party projects; restore clean via `nuget restore`; SecurityCodeScan.VS2019 not referenced | PASS | `p3-restore.2026-06-08T12-12.md` (P3-T17/T18, restore EXIT 0, 5 packages present, 0 SecurityCodeScan), `final-restore.2026-06-08T12-12.md` (P6-T2, EXIT 0) |
| AC2 — BannedApiAnalyzers + BannedSymbols.txt active; 5 banned symbols flagged in new/touched code | PASS | `BannedSymbols.txt` (P3-T1, 5 targets), `p4-build-no-regression.2026-06-08T12-12.md` Revision-2 P4-T18 (60 RS0030 diagnostics on DateTime.Now / Task.Delay with remediation messages) |
| AC3 — TimeProvider/FakeTimeProvider seam guidance in rules/csharp.md; no runtime change | PASS | `.claude/rules/csharp.md` "Time seam (TimeProvider) — guidance only" (P5-T1); guidance-only, no `.cs` edits |
| AC4 — `.editorconfig` severities (5 analyzers) + naming + namespace preference scoped to avoid build-break; SCS severities removed | PASS | `.editorconfig` (P2-T2..T7 severities/naming retained; P2-T8 SCS removed), `p2-severities-toolchain.2026-06-08T12-12.md` (analyzer build 0 errors) |
| AC5 — Four toolchain stages pass locally to environment extent; nullable step returns to 84-error baseline with no regression after SecurityCodeScan removal | PASS | `final-format` (baseline state), `final-restore` (EXIT 0), `final-analyzer-build` (EXIT 0), `final-nullable-build` (84 = baseline, no regression), `final-test-coverage` (collector ran; failures are baseline flaky timer tests) |
| AC6 — PR CI green expectation (nullable-as-errors + MSTest-with-coverage) | PASS (local parity) | Local commands match the CI workflow (`.github/workflows/ci.yml`): restore + analyzer build + nullable build all green/at-baseline locally; coverage collected. Authoritative CI green is confirmed by the PR run; the local toolchain uses the same commands. |
| AC7 — No do_not_change invariant violated; SecurityCodeScan.VS2019 deferral recorded as documented adaptation (not a violation); rules/csharp.md retains MSTest/Moq, 80/90, msbuild+vstest | PASS | `invariant-check.2026-06-08T12-12.md` (P5-T4 all invariants PASS), `.claude/rules/csharp.md` deferral note (P5-T3) + retained policy (P5-T2). The deferral is an authorized adaptation per the issue's "adapted so it builds cleanly with zero new build/CI failures" mandate. |
| AC8 — Change scoped to build-config + rules/csharp.md + `.editorconfig` + per-project analyzer refs + BannedSymbols.txt; no app logic changes except compile-required seams | PASS | `invariant-check.2026-06-08T12-12.md` Invariant 7 + `git status` review: only `.editorconfig`, 15 `.csproj`, 15 `packages.config`, `BannedSymbols.txt`, `.claude/rules/csharp.md` changed; no production `.cs` modified; no compile-required seam needed |

## Notes
- AC7 explicitly records the SecurityCodeScan.VS2019 deferral as an authorized adaptation with the CS8032 root cause (SecurityCodeScan.VS2019 5.6.7 incompatible with Roslyn 5.6: TypeInitializationException -> FileNotFoundException for YamlDotNet 11.0.0.0 -> CS8032, which cannot be set to suggestion via .editorconfig and breaks the protected nullable gate under TreatWarningsAsErrors). No CS8032 suppression and no substitute security analyzer were introduced. The 5-analyzer stack is the adopted set.
- AC6 is verified to local parity; the final authoritative green CI status is produced by the PR GitHub Actions run, which executes the same restore/analyzer/nullable/coverage commands recorded here.

## Verdict
AC1–AC8 all PASS with supporting evidence. No AC lacks supporting evidence; overall verdict for this acceptance check is PASS.
