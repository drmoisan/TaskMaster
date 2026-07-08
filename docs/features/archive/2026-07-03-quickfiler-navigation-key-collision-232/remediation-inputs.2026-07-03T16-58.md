# Remediation Inputs — Cycle 1 (Issue #232)

**Entry timestamp:** 2026-07-03T16-58
**Cycle:** 1
**Author:** orchestrator
**Base branch:** `main` (merge-base `00507b595297c3e6970634a1855f1144c987dbdf`)
**Head under remediation:** `90e75ec1`
**Trigger:** feature-review at 2026-07-03T16-51 produced 1 blocking finding.
**Upstream detail (authoritative finding text):** `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/remediation-inputs.2026-07-03T16-51.md` and the three audits `policy-audit.2026-07-03T16-51.md`, `code-review.2026-07-03T16-51.md`, `feature-audit.2026-07-03T16-51.md`.

Canonical issue number for this feature is 232. All artifact content, file paths, and cross-references must use this number.

---

## Blocking Finding 1 (BLOCKING) — Machine-readable C# coverage artifact absent

The fail-closed coverage-verification gate requires a machine-readable Cobertura coverage artifact for every language with changed source files. C# is the only changed-source language. No `artifacts/csharp/coverage.xml` exists, and no Cobertura `coverage.xml` is persisted under the feature `evidence/**` tree. Coverage exists only as transcribed prose in `evidence/qa-gates/vstest-final.md`, `evidence/qa-gates/coverage-delta.md`, and `evidence/baseline/vstest-baseline.md`. This drives AC10 to PARTIAL.

**This is an evidence-verifiability gap, not evidence of unmet coverage.** No change to the Part A crash fix (`QfcCollectionController.cs`) or the Part B logging is required by this finding.

### Required remediation actions
1. Re-run C# coverage collection over the touched test assemblies (`UtilitiesCS.Test`, `QuickFiler.Test`, `TaskMaster.Test`) using the Cobertura runsettings already referenced in the evidence (`/InIsolation`, first-party + Swordfish module set, `[ExcludeFromCodeCoverage]`/`GeneratedCode` attribute excludes — identical to the ratified configuration).
2. Persist the resulting Cobertura `coverage.xml` to a durable, reviewable path: the canonical `artifacts/csharp/coverage.xml` AND a committable copy under the feature evidence tree at `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/coverage/2026-07-03T16-58/coverage.xml` (evidence-location invariant: use the feature `evidence/` tree, never `artifacts/coverage/`).
3. Re-verify directly from the persisted XML: (a) every `<class>` mapped to `QfcHighConfidencePreFilter.cs` has line-rate 1.0 (changed-line coverage >= 90%); (b) repo-wide line-rate shows no regression versus the recorded baseline (~76.57%, exemption-governed testable denominator).
4. Update `evidence/qa-gates/coverage-delta.md` to cite the persisted XML path and the artifact-derived figures.

### Acceptance criterion affected
- AC10 (coverage) — currently PARTIAL; target PASS once the artifact is persisted and verified.

---

## Bundled non-blocking correction (author decision — included because it is a correctness nit in the Part B logging feature this branch introduced)

- **Caller-context string correction (Minor).** In `QuickFiler/Controllers/QfcDatamodel.cs`, the new `logger.Debug(...)` caller-context string names `LoadRemainingEmailsToQueueAsync` while the log call is physically inside `ScoreRemainingQueueMailItemAsync`. Correct the caller-context string so it accurately identifies the method it is emitted from (retain the master-queue-admission context descriptor). This is a one-line string change with no control-flow effect. It is bundled here because the coverage regeneration re-runs the full toolchain regardless, so no additional QA cost is incurred, and it improves the accuracy of the very logging feature under review.

## Explicitly out of scope for this cycle
- No change to the Part A fix logic or the register/unregister/guard behavior.
- No change to high-confidence filtering / dequeue behavior (tracked separately as feature #233).
- The pre-existing `QfcCollectionController.cs` >500-line overage and the `QfcCollectionControllerTests.cs` at-cap (500-line) condition are pre-existing/at-limit, not introduced-regressions, and are not remediation items for this cycle.

## Exit condition for this cycle
Re-audit (feature-review) reports 0 blocking findings, with the C# coverage gate verified from the persisted machine-readable artifact and AC10 flipped to PASS.
