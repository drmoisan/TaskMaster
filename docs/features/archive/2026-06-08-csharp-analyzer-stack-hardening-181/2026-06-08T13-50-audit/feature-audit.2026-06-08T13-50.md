# Feature Audit: csharp-analyzer-stack-hardening (Issue #181)

**Audit Date:** 2026-06-08
**Feature Folder:** `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
**Base Branch:** `main`
**Head Branch:** `feature/csharp-analyzer-stack-181`
**Work Mode:** `full-feature`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `main` (commit `2a522ed831865c2918ab02df153ef2929b0617dc`)
- **Head branch/commit:** `feature/csharp-analyzer-stack-181` (commit `71e0777ada475c408d85d3b6c68e6192b4bc070b`)
- **Merge base:** `2a522ed831865c2918ab02df153ef2929b0617dc`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/**`
  - Additional evidence: `artifacts/csharp/coverage.xml` (canonical Cobertura), live `git diff` against the merge base, `gh pr/run` status queries
- **Feature folder used:** `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
- **Requirements source:** `user-story.md` (checkbox AC1–AC8) and `spec.md` (Definition of Done, prose/checkbox) per `full-feature` work mode.
- **Work mode resolution note:** `issue.md` carries `- Work Mode: full-feature`, so the authoritative AC sources are `spec.md` and `user-story.md`. `user-story.md` contains the checkbox AC1–AC8 (the authoritative checkbox source); `issue.md` carries the same AC text under "(early draft)" and is not the authoritative checkbox source for `full-feature`.
- **Scope note:** Audit scope is the full branch diff vs `main`. The PR-context summary misclassifies the C# build-config changes as "Docs/templates/agents/tooling" and reports "Core logic changes: 0 files"; this was rejected for scope (see policy-audit `## Rejected Scope Narrowing`). The actual diff contains 31 C# build-config files, a new `BannedSymbols.txt`, and a new `.editorconfig`; no `.cs` source files changed.

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
| 1 | AC1 — Analyzer packages referenced by first-party projects; clean `nuget restore` | PASS | 15 first-party `*.csproj` each carry 9 `<Analyzer Include>` items (135 total); 15 `packages.config` carry 5 analyzer packages as developmentDependency. Restore EXIT 0. `evidence/qa-gates/final-restore.2026-06-08T12-12.md`, `p3-restore.2026-06-08T12-12.md` | `git diff <base>..<head> -- "*.csproj" \| grep -c '<Analyzer Include'`; `nuget.exe restore TaskMaster.sln` | 135 = 15 projects x 9 DLLs. Vendored excluded. |
| 2 | AC2 — BannedApiAnalyzers + BannedSymbols.txt active; 5 banned symbols flagged | PASS | `BannedSymbols.txt` (new) with 5 logical symbols (7 lines incl. overloads); `<AdditionalFiles ..\BannedSymbols.txt>` in all 15 projects; RS0030 fires (60 diagnostics recorded at warning during verification). `evidence/qa-gates/p4-build-no-regression.2026-06-08T12-12.md` | `git diff <base>..<head> -- BannedSymbols.txt`; build with RS0030 at warning | RS0030 held at suggestion for rollout; promotion is documented follow-up. |
| 3 | AC3 — TimeProvider/FakeTimeProvider seam + guidance in rules/csharp.md; no runtime change | PASS | `.claude/rules/csharp.md` "Time seam (TimeProvider) — guidance only" section added; guidance-only, no `.cs` edits. | `git diff <base>..<head> -- .claude/rules/csharp.md`; `git diff <base>..<head> -- "*.cs"` (empty) | Microsoft.Bcl.TimeProvider already present; no new production dependency. |
| 4 | AC4 — `.editorconfig` severities + file-scoped-namespace pref + naming rules, scoped to avoid build-breaking errors | PASS | New `.editorconfig` (+567): global default + per-rule severities all at suggestion; naming rules; file-scoped-namespace preference. Analyzer build EXIT 0. `evidence/other/editorconfig-severity-map.2026-06-08T12-12.md`, `p2-severities-toolchain.2026-06-08T12-12.md` | `git diff <base>..<head> -- .editorconfig` | `.globalconfig` not used (CLAUDE.md retains policy via `.editorconfig`); the single warning line preserves baseline MSTEST0032. |
| 5 | AC5 — Four toolchain stages pass to environment extent; nullable step does NOT regress | PARTIAL | Restore EXIT 0; analyzer build EXIT 0; nullable build at 84-error vendored baseline (no regression, 0 first-party, 0 CS8032); format and test at Phase 0 baseline (pre-existing CSharpier `.cs` finding + flaky timer tests). `evidence/qa-gates/final-*.2026-06-08T12-12.md` | `msbuild ... -p:Nullable=enable -p:TreatWarningsAsErrors=true`; `csharpier check .`; `vstest.console.exe ... /EnableCodeCoverage` | No-regression criterion met. PARTIAL because format/nullable/test do not reach a fully-green single pass locally (baseline is non-green); this is the documented environment limit. |
| 6 | AC6 — PR CI GREEN (nullable-as-errors + MSTest-with-coverage) | UNVERIFIED | No GitHub PR exists for the branch and no CI run is recorded against the branch head; PR-context CI status is "(not available)". | `gh pr list --head feature/csharp-analyzer-stack-181 --state all` (empty); `gh run list --branch feature/csharp-analyzer-stack-181` (empty) | Local parity shown but cannot substitute for an actual green CI run. Blocking for merge. |
| 7 | AC7 — No do_not_change invariant violated; rules/csharp.md retains MSTest/Moq, 80/90, msbuild+vstest | PASS | All 7 hard invariants PASS, incl. only `csharp.md` changed in `.claude/rules/`, no CPM/quality-tiers/globalconfig, MSTest/Moq/FluentAssertions and 80/90 retained, no CS8032 suppression. `evidence/other/invariant-check.2026-06-08T12-12.md` | `git status --porcelain -- .claude/rules/`; grep CS8032 across config | SecurityCodeScan deferral recorded as authorized adaptation. |
| 8 | AC8 — Change scoped to build-config + rules/csharp.md + .editorconfig + per-project analyzer refs + BannedSymbols.txt; no app logic changes | PASS | Diff is 15 `.csproj`, 15 `packages.config`, `.editorconfig`, `BannedSymbols.txt`, `.claude/rules/csharp.md`, plus feature docs/evidence and agent-memory notes. No production/test `.cs` modified; no compile-required seam needed. `evidence/other/invariant-check.2026-06-08T12-12.md` Invariant 7 | `git diff <base>..<head> -- "*.cs" --stat` (empty) | Directory.Build.props not used. Feature docs and `.claude/agent-memory` notes are audit-trail, not application logic. |

---

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**
- **PASS:** 6 criteria (AC1, AC2, AC3, AC4, AC7, AC8)
- **PARTIAL:** 1 criterion (AC5)
- **UNVERIFIED:** 1 criterion (AC6)
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. AC6 is UNVERIFIED: no PR and no CI run exist for the branch head, so the authoritative repo-wide 80% coverage gate and the nullable-as-errors / MSTest-with-coverage CI steps are not confirmed green. This is the single blocking item for merge.
2. AC5 is PARTIAL: the local toolchain does not reach a fully-green single pass (format/nullable/test are at the documented non-green Phase 0 baseline), though the protected nullable gate does not regress. This is an environment limitation, resolved by the same green CI run that satisfies AC6.

**Recommended follow-up verification steps:**

1. Open the PR for `feature/csharp-analyzer-stack-181` and confirm a GREEN GitHub Actions CI run (nullable-as-errors at the vendored-only baseline; MSTest-with-coverage passing the scoped 80% repo-wide / 90% new-code gates). On success, AC6 becomes PASS and AC5 is corroborated by the authoritative environment.
2. Confirm the 7 flaky wall-clock-timer tests pass or are retried green on the CI run, since they are nondeterministic and unrelated to this change.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as PASS may be checked off in the authoritative source file(s) when represented as markdown checkboxes and not already checked.
- Criteria evaluated as PARTIAL, FAIL, or UNVERIFIED remain unchecked.

AC1, AC2, AC3, AC4, AC7, AC8 (PASS) are checked off in `user-story.md`. AC5 (PARTIAL) and AC6 (UNVERIFIED) remain unchecked. `spec.md` Definition of Done items are prose/checkbox secondary items and are not modified by this review (the AC1–AC8 checkbox source is `user-story.md`). `issue.md` already shows AC1–AC8 as `[x]` from the executor run; this review does not alter `issue.md` because it is not the authoritative `full-feature` checkbox source, and AC5/AC6 are not PASS by this audit.

### AC Status Summary

- Source: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/user-story.md`
- Total AC items: 8
- Checked off (delivered): 6 (AC1, AC2, AC3, AC4, AC7, AC8)
- Remaining (unchecked): 2 (AC5, AC6)
- Items remaining: AC5 (toolchain stages / nullable no-regression — PARTIAL, full green is CI-only); AC6 (PR CI GREEN — UNVERIFIED, no PR/CI run)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `user-story.md` | 8 | 6 | 2 | Checkbox-backed; authoritative AC source for full-feature |
| `spec.md` | 0 (no AC1–AC8 checkboxes; Definition of Done only) | 0 | n/a | Secondary; prose/Definition-of-Done, not the AC checkbox source — not modified |
| `issue.md` | 8 (early-draft duplicate) | already [x] by executor | n/a | Not the authoritative full-feature checkbox source; not modified by this review |
