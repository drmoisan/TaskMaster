---
name: modified-file-subfloor-nonblocking-disposition-230
description: "#230 precedent: a modified file below the 85/75 per-file floor can carry a FAIL row dispositioned non-blocking (no remediation-inputs) when it is >= 80%, has zero changed-line regression, improved vs baseline, and the residue is pre-existing #197 debt"
metadata:
  type: project
---

On #230 (2026-08-07), `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` read 81.88% line / 62.96% branch at head — below the uniform 85/75 per-file floor — while the feature itself improved the file from 74.37%/56.00%, covered 100% of its changed executable lines (`ResolveControlGroupsAsync` 38/38; every other edit comment/attribute-only), and the uncovered residue sat entirely in members the diff never touched.

Disposition used: an explicit `FAIL` on the C# per-file coverage row, marked "dispositioned non-blocking," with four-part rationale: (a) above the workflow's 80% modified-file remediation-trigger floor, (b) zero regression on changed lines, (c) both metrics improved vs baseline, (d) residue is pre-existing QuickFiler debt tracked by #197. No `remediation-inputs` artifact was produced because none of the enumerated remediation triggers in `feature-review-workflow` step 8 fires (< 80% modified-file, regression, < 90% new-file, repo-wide < 80%, artifact absence, toolchain failure, AC FAIL). Hook-wise a FAIL verdict on a coverage row is fully acceptable — the hook only requires PASS-or-FAIL plus no narrowing phrases.

**Why:** The skill's threshold section (85/75 per-file) is stricter than its own remediation-trigger enumeration (80/regression). Treating an improvement to pre-existing debt as blocking would penalize incremental drawdown; #283/#392 established the "FAIL, dispositioned non-blocking" row shape and #230 extends it to the no-remediation-inputs case when no trigger fires.

**How to apply:** When a modified file is sub-floor, compute baseline-vs-head per-file figures from both Cobertura XMLs before deciding. If all four rationale legs hold, write the FAIL row with the disposition inline and skip remediation-inputs; if any leg fails (regression, < 80%, or the residue is in lines the feature touched), remediation is required. See [[rescoping-to-instrumented-package-does-not-always-clear-floor]].
