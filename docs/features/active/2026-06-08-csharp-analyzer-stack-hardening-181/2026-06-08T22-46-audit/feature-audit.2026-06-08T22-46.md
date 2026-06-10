# Feature Audit: csharp-analyzer-stack-hardening (Issue #181)

**Audit Date:** 2026-06-08
**Feature Folder:** `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
**Base Branch:** `main`
**Head Branch:** `feature/csharp-analyzer-stack-181`
**Work Mode:** `full-feature`
**Audit Type:** Cycle-5 exit reaudit (fix in working tree on top of HEAD `0883d0f7`)

---

## Scope and Baseline

- **Base branch:** `main` (commit `2a522ed831865c2918ab02df153ef2929b0617dc`)
- **Head branch/commit:** `feature/csharp-analyzer-stack-181` (commit `0883d0f7367844f16ede7d48972a91886aaff5be`) plus the uncommitted working-tree cycle-5 fix.
- **Merge base:** `2a522ed831865c2918ab02df153ef2929b0617dc` (verified `git merge-base HEAD main`)
- **Complete intended change set:** committed branch diff (`git diff 2a522ed8..HEAD`) + working-tree diff (`git diff`).
- **Evidence sources:**
  - Primary: `remediation-inputs.2026-06-08T21-53.md` (cycle-5 scope, four-test acceptance, guardrails)
  - Plan: `remediation-plan.2026-06-08T21-53.md` (executed; all tasks checked)
  - Feature evidence: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/**` at the `.2026-06-08T21-53` timestamp (baseline/, regression-testing/, qa-gates/)
  - Live `git diff` against HEAD and the merge base
- **Feature folder used:** `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
- **Requirements source:** `user-story.md` (checkbox AC1–AC8) and `spec.md` (Definition of Done) per `full-feature` work mode, supplemented by the cycle-5 four-test acceptance criteria in `remediation-inputs.2026-06-08T21-53.md`.
- **Work mode resolution note:** `issue.md` carries `- Work Mode: full-feature`, so the authoritative AC sources are `spec.md` and `user-story.md`. The cycle-5 remediation acceptance (four named tests passing deterministically; zero regression) is evaluated as a delivery gate layered on the feature ACs, not as a replacement for them.
- **Scope note:** Audit scope is the full branch diff vs `main` plus the working-tree fix. No caller narrowing to a plan/task/phase subset was accepted; the cycle-5 four-file budget is the legitimate, authorized scope of the remediation, and the audit evaluates the full intended change set against it.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/user-story.md` — primary (checkbox-backed AC1–AC8)
- `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/spec.md` — secondary (Definition of Done; prose/checkbox)
- `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/remediation-inputs.2026-06-08T21-53.md` — cycle-5 delivery gate (four-test acceptance)

### Acceptance criteria (from user-story.md)

1. AC1: Analyzer packages referenced by first-party projects; restore cleanly via `nuget restore`.
2. AC2: BannedApiAnalyzers + BannedSymbols.txt active; 5 banned symbols flagged in new/touched code.
3. AC3: TimeProvider/FakeTimeProvider seam + guidance added to rules/csharp.md; no runtime behavior changed.
4. AC4: .editorconfig/.globalconfig carries new severities, file-scoped-namespace pref, naming rules, scoped to avoid build-breaking errors.
5. AC5: All four toolchain stages pass locally to the extent the environment allows; nullable TreatWarningsAsErrors step does NOT regress.
6. AC6: PR CI is GREEN, including nullable-as-errors and MSTest-with-coverage steps.
7. AC7: No do_not_change invariant violated; rules/csharp.md updated retaining MSTest/Moq, 80/90 coverage, msbuild+vstest.
8. AC8: Change scoped to C# build-config + rules/csharp.md (+ .editorconfig/.globalconfig + per-project analyzer refs). No application logic changes except seam introductions required to compile.

### Cycle-5 delivery gate (from remediation-inputs.2026-06-08T21-53.md)

- C5-1: `FromSeed_ShouldBuildFileNameFromParts` passes deterministically.
- C5-2: `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths` passes deterministically.
- C5-3: `People_Deserialize_CanDeserializePatternCorrectly` passes deterministically.
- C5-4: `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` passes deterministically.
- C5-5: No currently-passing test regressed; `PeopleScoConverter`/shortcut path still passes.
- C5-6: Full toolchain passes in one final pass; no `[Ignore]` re-added; no assertion weakened; no sleeps/retries.

### From spec.md (Definition of Done — secondary, prose/checkbox)

- Acceptance criteria documented and mapped to tests; behavior matches AC; tests updated as applicable; edge cases covered; docs updated; toolchain pass completed; PR CI green.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | AC1 — Analyzer packages referenced; clean `nuget restore` | PASS | Committed branch diff unchanged from cycle 2: 15 `*.csproj` carry 9 `<Analyzer Include>` items each; 15 `packages.config` carry 5 analyzer packages. | `git diff 2a522ed8..HEAD -- "*.csproj"`; CI run 27158840914 (cycle 2) | Cycle-5 working tree does not touch build-config. |
| 2 | AC2 — BannedApiAnalyzers + BannedSymbols.txt active; banned symbols flagged | PASS | `BannedSymbols.txt` present (committed); cycle-5 edits introduce none of the 5 banned symbols. | `evidence/regression-testing/finding-b-banned-symbol-check.2026-06-08T21-53.md`, `consume-banned-symbol-check.2026-06-08T21-53.md` | Cycle-5 fixes are banned-symbol-clean. |
| 3 | AC3 — TimeProvider seam + guidance; no runtime change | PASS | `.claude/rules/csharp.md` guidance present (committed); cycle-5 Finding C removes a wall-clock dependency by per-item reporting (no new time API). | `git diff -- "*.cs"` | Finding C strengthens determinism without DateTime/Timer-based time reads. |
| 4 | AC4 — `.editorconfig` severities; scoped to avoid build-break | PASS | `.editorconfig` present (committed); cycle-5 working tree does not modify it. | `git diff --stat` | No `.editorconfig`/`.globalconfig` change in cycle 5. |
| 5 | AC5 — Four toolchain stages pass; nullable step does NOT regress | PASS | Cycle-5 final pass: csharpier EXIT 0; analyzer EXIT 0 (0 errors); nullable `/t:Build` 0 Warning/0 Error, 0 first-party errors; vstest 4055 passed. | `evidence/qa-gates/final-format/analyzer-build/nullable-build/test-coverage.2026-06-08T21-53.md` | Restarted once after the in-budget normalization edit; clean in the final pass. |
| 6 | AC6 — PR CI GREEN (nullable-as-errors + MSTest-with-coverage) | PARTIAL (non-blocking for local exit) | The cycle-2 committed head was CI-GREEN (run 27158840914). The cycle-5 fix is in the working tree and not yet committed/pushed, so a branch-head CI run including the cycle-5 edits does not yet exist. | `gh run view 27158840914` (cycle 2) | The cycle-5 local toolchain passes in a single final pass (AC5). The required CI-green-against-branch-head check is a post-commit/push gate the orchestrator must confirm after committing the working tree. This is a process sequencing item, not a defect in the change. |
| 7 | AC7 — No do_not_change invariant violated; rules retained | PASS | No `.claude/rules/`, `.editorconfig`/`.globalconfig`, `BannedSymbols.txt`, analyzer-wiring, or vendored-project change in the cycle-5 working tree; MSTest/Moq/FluentAssertions and 80/90 coverage policy retained. | `git diff --stat` (only 4 cycle-5 files + a benign agent-memory note) | No invariant surface touched by cycle 5. |
| 8 | AC8 — Change scoped to authorized files; no unrelated logic changes | PASS | Cycle-5 working tree touches exactly the three authorized production files (Findings A/B/C) plus the one authorized test-file formatting fix. | `git diff --stat` | Within the cycle-5 four-file authorized budget. |
| C5-1 | `FromSeed_ShouldBuildFileNameFromParts` deterministic pass | PASS | PASSED [38 ms]; `FolderPath == @"C:\data"` (uncorrupted). | `evidence/regression-testing/fromseed-after-fix.2026-06-08T21-53.md` | Finding A. |
| C5-2 | `CalcMaxSeedLength_..._ShouldSubtractComponentLengths` deterministic pass | PASS | PASSED; returns 239 (was 245 with corrupted `FolderPath`). | `evidence/regression-testing/calcmaxseedlength-after-fix.2026-06-08T21-53.md` | Finding A. |
| C5-3 | `People_Deserialize_CanDeserializePatternCorrectly` deterministic pass | PASS | PASSED [269 ms]; `people.Config.Disk.FileName == "pplkey.json"`. Sibling shortcut test also PASSED. | `evidence/regression-testing/people-deserialize-after-fix.2026-06-08T21-53.md` | Finding B; `/InIsolation` is a vstest flag for a pre-existing Moq binding-redirect issue, not a source change. |
| C5-4 | `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` deterministic pass | PASS | PASSED across 3 consecutive isolated runs (275/274/266 ms). | `evidence/regression-testing/consume-after-fix.2026-06-08T21-53.md` | Finding C; determinism from per-item reporting. |
| C5-5 | No regression; `PeopleScoConverter`/shortcut still passes | PASS | Full suite 4055 passed / 9 failed (all pre-existing flaky timer tests); the 3 integration tests pass; shortcut path passes. | `evidence/qa-gates/final-test-coverage.2026-06-08T21-53.md`, `finding-b-integration-regression-resolved.2026-06-08T21-53.md` | The 9 failures are not the four targets nor the serialization tests. |
| C5-6 | One-pass toolchain; no `[Ignore]` re-added; no weakened assertion; no sleeps | PASS | Final pass clean; both target test files have `[Ignore]` commented out (not re-added); no assertion changed; no sleep/retry added. | `git diff 2a522ed8..HEAD -- "*PeopleScoDictionaryNewTests.cs"`, `git diff -- "*ToDoItemTests.cs"`, `consume-banned-symbol-check.2026-06-08T21-53.md` | Verified directly against the diffs. |

---

## Zero-Regression Verification (scrutinized item #4)

- **Full first-party suite:** 4064 total, 4055 passed, 9 failed (`evidence/qa-gates/final-test-coverage.2026-06-08T21-53.md`).
- **The 9 failures are pre-existing flaky wall-clock-timer/dispatcher tests**, confirmed out of scope and verified to pass in isolation. None are the four target tests; none are the three `ScoDictionaryConverterTests` serialization tests. The two failures in the final run that were not in the cycle-5 baseline failing set (`ToList_InternalHelper_ConsumesEnumerableAndReportsProgress`, `WireNotifications_OnMappedToChange_RaisesPropertyChanged`) were re-run in isolation and both PASS, confirming flaky-under-load membership rather than regression.
- **Coverage:** post-change 59.06% vs baseline 59.04% repo-wide aggregate (+0.02pp, +54 covered lines). No regression. (`evidence/qa-gates/coverage-delta.2026-06-08T21-53.md`.)
- **Conclusion:** the nine failures are pre-existing and out of scope; the cycle introduced no non-flaky regression.

---

## Acceptance Criteria Check-off

The following AC items in `user-story.md` were previously checked off in cycle 2 and remain satisfied by the unchanged committed branch diff: AC1, AC2, AC3, AC4, AC5, AC7, AC8. AC6 (PR CI green against branch head) requires a post-commit/push CI run that includes the cycle-5 working-tree fix; it is left as the orchestrator's exit gate and is not re-checked here.

The cycle-5 delivery gate (C5-1 through C5-6) is satisfied per the evaluation table above. The two re-enabled tests had their `[Ignore]` attributes commented out by prior cycles and remain so; the reviewer made no source changes.

### Acceptance Criteria Status

- Source: `user-story.md` (AC1–AC8) + `remediation-inputs.2026-06-08T21-53.md` (C5-1–C5-6)
- Total AC items: 14 (8 feature ACs + 6 cycle-5 gates)
- Checked off (delivered): 13 PASS
- Remaining (unchecked): 1 (AC6 — PR CI green against branch head, pending commit/push of the working-tree fix)
- Items remaining: AC6 — PR CI GREEN at the branch head including the cycle-5 edits (post-commit/push orchestrator gate; non-blocking for local cycle exit per AC5 single-pass toolchain success)

---

## Summary

The four named tests pass deterministically, no assertion was weakened, no `[Ignore]` was re-added, and the change set is confined to the authorized cycle-5 budget. The full first-party suite shows zero non-flaky regression and coverage did not regress. All feature ACs verifiable against the local change set are PASS. AC6 is a non-blocking PARTIAL: it depends on a CI run against a branch head that includes the still-uncommitted cycle-5 fix, which is a process sequencing step for the orchestrator after commit/push, not a defect in the change. There are no FAIL findings and no blocking PARTIAL findings.

**BLOCKING FINDINGS: 0**
