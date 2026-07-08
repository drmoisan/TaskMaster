# Remediation Inputs: Issue #211 outlook-startup-latency diagnostics + AC10 fix

**Entry Timestamp:** 2026-06-24T15-35
**Feature Folder:** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211`
**Base Branch:** `main` (merge-base `9385bf607aca6c5722f2da7961a895c685710942`)
**Head:** `bug/outlook-startup-latency-211` (`6d6209f0`)
**Work Mode:** `full-bug`

## Source Audit Artifacts (findings that produced this remediation)

- `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/policy-audit.2026-06-24T15-35.md`
- `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/code-review.2026-06-24T15-35.md`
- `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/feature-audit.2026-06-24T15-35.md`

## Trigger Classification

Remediation is triggered by:
- One policy-audit FAIL: repo-wide C# line coverage 61.90% < 80% gate (pre-existing, non-regressing).
- Two feature-audit PARTIAL acceptance criteria: AC9 (attribution reopened/superseded) and AC10 (runtime latency-reduction re-capture maintainer-gated).

No toolchain failures and no code-review Blocker/Major findings exist. All four C# toolchain steps pass (csharpier, analyzers, nullable/TWAE, 4109/4109 tests). The remediation items below are NOT automated code defects in the delivered diagnostics/fix; they are (a) a pre-existing repo-wide coverage condition and (b) maintainer-gated runtime verification needed to close the bug's stated goal.

## Remediation-Required Findings

### Finding 1 — Repo-wide C# coverage below the 80% gate (FAIL, pre-existing)
- **Severity:** Blocking against the literal policy gate; non-regressing relative to baseline.
- **Files/scope:** Repo-wide C# (unexempted aggregate). Baseline 61.84% -> post-change 61.90% (+0.06pp).
- **Expected behavior:** Repo-wide C# line coverage ≥ 80% per the General/C# Unit Test Policy, OR an explicit, maintainer-ratified COM/VSTO/WinForms exemption-adjusted "testable denominator" figure computed and recorded that meets ≥ 80%.
- **Disposition guidance:** This branch does not introduce the deficit. Options: (a) maintainer accepts the pre-existing condition as out-of-scope-for-this-bug and merges on judgment (the diagnostics/fix do not regress coverage); or (b) route uplift to the tracked `feature/csharp-coverage-uplift` effort. Do NOT weaken the gate or add blanket `[ExcludeFromCodeCoverage]` to skirt it.
- **Verification command:** `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"TestCategory!=LiveOutlook" /EnableCodeCoverage` then parse repo-wide line-rate from the Cobertura output.
- **Evidence:** `evidence/baseline/baseline-coverage-2026-06-24T17-30.cobertura.xml`, `evidence/qa-gates/postchange-coverage-2026-06-24T17-30.cobertura.xml`.

### Finding 2 — AC9 attribution reopened/superseded (PARTIAL)
- **Severity:** Blocking for issue closure; not blocking for merging the diagnostics.
- **Scope:** `spec.md` AC9; the per-engine attribution did not converge (cost is a cross-cutting, intermittent STA stall, not phase/engine-specific).
- **Expected behavior:** A non-debugger cold-start capture set that settles the dominant blocking sub-step/folder/store using the now-deployed `[store-filter]`/`[spam-init]`/`[store-wrapper-init]`/`[phase-net]` probes, then an updated AC9 disposition in `spec.md`.
- **Verification:** Maintainer runtime capture per `evidence/other/coldstart-*-capture-instructions-*.md`; replace the corresponding `*PLACEHOLDER.md` with the real capture; update `spec.md` AC9.
- **Do NOT:** Mark AC9 PASS from inference; do not delete the diagnostic probes before the attribution concludes.

### Finding 3 — AC10 runtime latency-reduction re-capture is a maintainer-gated placeholder (PARTIAL)
- **Severity:** Blocking for issue closure; the automated fix + invariant test are complete.
- **Scope:** `spec.md` AC10; the JunkFolderPathNavigator fix is implemented, equivalence-documented, and covered (95%) with red-before-green regression evidence, but the "re-capture confirming the startup-latency reduction" clause is unmet.
- **Expected behavior:** A maintainer non-debugger cold-start re-capture showing the JunkCertain/JunkPotential navigation no longer incurs the ~50s enumeration cost; recorded in place of `evidence/other/runtime-capture-ac10-junk-navigation-PLACEHOLDER.md`; AC10 disposition updated.
- **Verification:** Capture per `evidence/other/ac10-coldstart-junk-navigation-recapture-instructions-2026-06-24T17-30.md`.
- **Do NOT:** Close issue #211 or mark AC10 PASS until the runtime re-capture confirms the reduction.

## Do-Not List (applies to all findings)

- No scope creep beyond the three findings above.
- No policy weakening (do not lower the 80% gate, do not relax `TreatWarningsAsErrors`, do not weaken assertions).
- No silent skips of toolchain or coverage checks for any changed language.
- No blanket `[ExcludeFromCodeCoverage]` to inflate the coverage figure.
- No removal of the diagnostic probes until runtime attribution (AC9) and AC10 latency confirmation are complete.
- Preserve the AC10 not-found fallback (MyBox -> PickFolder -> WriteSetting -> Save) and `FolderTree.cs` exactly as-is.

## Notes for the Orchestrator

Findings 2 and 3 are inherently maintainer-gated (live Outlook VSTO host required; not CI-automatable). An `atomic-planner`/`atomic-executor` code-remediation cycle can address Finding 1's disposition decision and any documentation/spec updates, but the runtime captures themselves require maintainer action. If the maintainer elects to merge the diagnostics + AC10 automated portion now and track issue closure separately, that is consistent with the Conditional Go recommendation in the code review.
