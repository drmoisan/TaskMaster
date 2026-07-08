# Feature Audit: csharp-analyzer-stack-hardening (Issue #181)

**Audit Date:** 2026-06-08
**Feature Folder:** `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
**Base Branch:** `main`
**Head Branch:** `feature/csharp-analyzer-stack-181`
**Work Mode:** `full-feature`
**Audit Type:** Cycle-2 exit reaudit (post-remediation, post-CI-green)

---

## Scope and Baseline

- **Base branch:** `main` (commit `2a522ed831865c2918ab02df153ef2929b0617dc`)
- **Head branch/commit:** `feature/csharp-analyzer-stack-181` (commit `cdf9a45f961597e4a699e2f59933967fdf7236ff`)
- **Merge base:** `2a522ed831865c2918ab02df153ef2929b0617dc` (verified `git merge-base main HEAD`)
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/**`
  - Additional evidence: `artifacts/csharp/coverage.xml` (canonical Cobertura), live `git diff` against the merge base, `gh pr checks 182` and `gh run view 27158840914`
- **Feature folder used:** `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
- **Requirements source:** `user-story.md` (checkbox AC1–AC8) and `spec.md` (Definition of Done, prose/checkbox) per `full-feature` work mode.
- **Work mode resolution note:** `issue.md` carries `- Work Mode: full-feature`, so the authoritative AC sources are `spec.md` and `user-story.md`. `user-story.md` contains the checkbox AC1–AC8 (the authoritative checkbox source); `issue.md` carries the same AC text under "(early draft)" and is not the authoritative checkbox source for `full-feature`.
- **Scope note:** Audit scope is the full branch diff vs `main`. The PR-context summary misclassifies the C# build-config changes as non-core-logic; this was rejected for scope (see policy-audit `## Rejected Scope Narrowing`). The actual diff contains 31 C# build-config files, a new `BannedSymbols.txt`, a new `.editorconfig`, and one formatting-only production `.cs` file (`UtilitiesCS/Extensions/IEnumerableExtensions.cs`, cycle-2 remediation).

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/user-story.md` — primary (checkbox-backed AC1–AC8)
- `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/spec.md` — secondary (Definition of Done; prose/checkbox, not the AC1–AC8 checkbox source)

### Acceptance criteria (from user-story.md)

1. AC1: Analyzer packages referenced by first-party projects; restore cleanly via `nuget restore`.
2. AC2: BannedApiAnalyzers + BannedSymbols.txt active; 5 banned symbols flagged in new/touched code.
3. AC3: TimeProvider/FakeTimeProvider seam + guidance added to rules/csharp.md; no runtime behavior changed.
4. AC4: .editorconfig/.globalconfig carries new severities, file-scoped-namespace pref, naming rules, scoped to avoid build-breaking errors.
5. AC5: All four toolchain stages pass locally to the extent the environment allows; nullable TreatWarningsAsErrors step does NOT regress.
6. AC6: PR CI is GREEN, including nullable-as-errors and MSTest-with-coverage steps.
7. AC7: No do_not_change invariant violated; rules/csharp.md updated retaining MSTest/Moq, 80/90 coverage, msbuild+vstest.
8. AC8: Change scoped to C# build-config + rules/csharp.md (+ .editorconfig/.globalconfig + Directory.Build.props if used + per-project analyzer refs). No application logic changes except seam introductions required to compile.

### From spec.md (Definition of Done — secondary, prose/checkbox)

- Acceptance criteria documented and mapped to tests or demos; behavior matches AC; tests updated as applicable; edge cases covered; docs updated; telemetry if applicable; toolchain pass completed. Seeded test conditions: `nuget restore` succeeds; both msbuild stages green; vstest run unaffected; PR CI green.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | AC1 — Analyzer packages referenced by first-party projects; clean `nuget restore` | PASS | 15 first-party `*.csproj` each carry 9 `<Analyzer Include>` items (135 total); 15 `packages.config` carry 5 analyzer packages as developmentDependency. Restore EXIT 0 locally and GREEN on CI. `evidence/qa-gates/final-restore.2026-06-08T18-06.md` | `git diff <base>..<head> -- "*.csproj" \| grep -c '<Analyzer Include'`; `nuget.exe restore TaskMaster.sln`; CI run 27158840914 | 135 = 15 projects x 9 DLLs. Vendored excluded. |
| 2 | AC2 — BannedApiAnalyzers + BannedSymbols.txt active; 5 banned symbols flagged | PASS | `BannedSymbols.txt` (new) with 5 logical symbols; `<AdditionalFiles ..\BannedSymbols.txt>` in all 15 projects; RS0030 fires (verified at warning during P4). `evidence/qa-gates/p4-build-no-regression.2026-06-08T12-12.md` | `git diff <base>..<head> -- BannedSymbols.txt`; build with RS0030 at warning | RS0030 held at suggestion for rollout; promotion is documented follow-up. |
| 3 | AC3 — TimeProvider/FakeTimeProvider seam + guidance in rules/csharp.md; no runtime change | PASS | `.claude/rules/csharp.md` "Time seam (TimeProvider) — guidance only" section added; guidance-only, no `.cs` logic edits. | `git diff <base>..<head> -- .claude/rules/csharp.md`; `git diff <base>..<head> -- "*.cs"` (only a formatting-only file) | Microsoft.Bcl.TimeProvider already present; no new production dependency. |
| 4 | AC4 — `.editorconfig` severities + file-scoped-namespace pref + naming rules, scoped to avoid build-breaking errors | PASS | New `.editorconfig` (+567): global default + per-rule severities all at suggestion; naming rules; file-scoped-namespace preference. Analyzer build EXIT 0 / GREEN on CI. `evidence/other/editorconfig-severity-map.2026-06-08T12-12.md` | `git diff <base>..<head> -- .editorconfig` | `.globalconfig` not used (policy retained via `.editorconfig`); the single warning line preserves baseline MSTEST0032. |
| 5 | AC5 — Four toolchain stages pass to environment extent; nullable step does NOT regress | PASS | All four stages (CSharpier format, analyzer/code-style build, nullable `TreatWarningsAsErrors` build, MSTest-with-coverage) pass GREEN in a single authoritative CI pass (run 27158840914, conclusion success). 0 first-party nullable errors, 0 CS8032, no regression. `evidence/qa-gates/final-*.2026-06-08T18-06.md`, `evidence/qa-gates/ci-green.2026-06-08T18-06.md` | `gh run view 27158840914 --json conclusion,headSha`; `msbuild ... -p:Nullable=enable -p:TreatWarningsAsErrors=true` | Cycle-1 PARTIAL upgraded to PASS: the authoritative CI now demonstrates a fully-green single pass; the cycle-2 formatting fix cleared the last local non-green item. |
| 6 | AC6 — PR CI GREEN (nullable-as-errors + MSTest-with-coverage) | PASS | PR #182 required checks GREEN at branch head `cdf9a45f`: run 27158840914 "Format, build, analyze, and test" = pass (6m1s), actionlint = pass, conclusion success. The job comprises CSharpier, nuget restore, analyzer/code-style build, nullable-as-errors build, and MSTest-with-coverage. `evidence/qa-gates/ci-green.2026-06-08T18-06.md` | `gh pr checks 182` (both pass); `gh run view 27158840914 --json conclusion,headSha` (success, cdf9a45f) | Cycle-1 UNVERIFIED upgraded to PASS: PR exists and CI is GREEN at the branch head, verified independently. |
| 7 | AC7 — No do_not_change invariant violated; rules/csharp.md retains MSTest/Moq, 80/90, msbuild+vstest | PASS | All 7 hard invariants PASS, incl. only `csharp.md` changed in `.claude/rules/`, no CPM/quality-tiers/globalconfig, MSTest/Moq/FluentAssertions and 80/90 retained, no CS8032 suppression. `evidence/other/invariant-check.2026-06-08T12-12.md` | `git status --porcelain -- .claude/rules/`; grep CS8032 across config | SecurityCodeScan deferral recorded as authorized adaptation. |
| 8 | AC8 — Change scoped to build-config + rules/csharp.md + .editorconfig + per-project analyzer refs + BannedSymbols.txt; no app logic changes | PASS | Diff is 15 `.csproj`, 15 `packages.config`, `.editorconfig`, `BannedSymbols.txt`, `.claude/rules/csharp.md`, one formatting-only `.cs` file, plus feature docs/evidence and agent-memory notes. No production `.cs` logic and no test `.cs` modified. `evidence/regression-testing/diff-scope-after-fix.2026-06-08T18-06.md` | `git diff <base>..<head> -- "*.cs"` (single formatting-only file); `git diff <base>..<head> -- UtilitiesCS/Extensions/IEnumerableExtensions.cs` (1 insert / 5 deletes, lambda collapse) | The single `.cs` edit is a CSharpier formatting fix required to pass the CI formatting gate (a seam/compile-equivalent build necessity), not application logic. Directory.Build.props not used. |

---

## Summary

**Overall Feature Readiness:** READY

**Criteria summary:**
- **PASS:** 8 criteria (AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8)
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Blocking findings (FAIL + blocking PARTIAL): 0.**

**Cycle-2 changes vs the 2026-06-08T13-50 audit:**
- AC5: PARTIAL -> PASS. The cycle-2 formatting fix cleared the last local non-green item; all four toolchain stages now pass in a single GREEN authoritative CI pass.
- AC6: UNVERIFIED -> PASS. PR #182 exists and its required checks are GREEN at the branch head (run 27158840914, conclusion success), verified independently via `gh pr checks 182` and `gh run view 27158840914`.
- AC1–AC4, AC7, AC8 remain PASS; AC8 now accounts for the single formatting-only `.cs` edit (confirmed no logic change).

**Recommended follow-up verification steps:**

1. None blocking for merge. The documented post-merge follow-ups remain optional: promote RS0030 from suggestion to warning after legacy banned-symbol call-site cleanup, and re-evaluate SecurityCodeScan when a Roslyn-5.x-compatible security analyzer is available.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as PASS may be checked off in the authoritative source file(s) when represented as markdown checkboxes and not already checked.
- Criteria evaluated as PARTIAL, FAIL, or UNVERIFIED remain unchecked.

All eight criteria (AC1–AC8) are evaluated PASS in this cycle-2 exit reaudit and are checked off `[x]` in `user-story.md`. AC5 and AC6 were the two remaining unchecked items from the cycle-1 audit; both are now PASS (AC5 corroborated by the GREEN CI pass; AC6 confirmed GREEN at the branch head) and are checked off in this review. `spec.md` Definition of Done items are prose/checkbox secondary items and are not modified by this review. `issue.md` already shows AC1–AC8 as `[x]` from the executor run; it is not the authoritative `full-feature` checkbox source.

### AC Status Summary

- Source: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/user-story.md`
- Total AC items: 8
- Checked off (delivered): 8 (AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8)
- Remaining (unchecked): 0
- Items remaining: none

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `user-story.md` | 8 | 8 | 0 | Checkbox-backed; authoritative AC source for full-feature |
| `spec.md` | 0 (no AC1–AC8 checkboxes; Definition of Done only) | 0 | n/a | Secondary; prose/Definition-of-Done, not the AC checkbox source — not modified |
| `issue.md` | 8 (early-draft duplicate) | already [x] by executor | n/a | Not the authoritative full-feature checkbox source; not modified by this review |
