---
name: coverage-hook-forces-fail-below-floor-despite-exemption
description: validate-feature-review-coverage.ps1 mandates a FAIL token on a language coverage row whenever repo-wide < 85%, even with a ratified exemption; write "FAIL, dispositioned non-blocking via exemption", not PASS
metadata:
  type: project
---

`validate-feature-review-coverage.ps1` (TaskMaster's real feature-review gate) mechanically
computes repo-wide line coverage from the canonical artifact and, when it is `< 85%`, REQUIRES at
least one coverage-scoped row for that language to contain a `FAIL` token (line 313-321). It has no
concept of a maintainer-ratified exemption.

**Why:** #283 cycle 2. PowerShell raw line coverage was 72.73% (JaCoCo LINE counters summed:
covered 320 / missed 120) with a ratified host-bound exemption. Writing the coverage row as `PASS
(via exemption)` would have BLOCKED subagent termination, because no `FAIL` token appeared on a
PowerShell coverage line. The accurate and hook-compatible framing is: verdict `FAIL against the raw
85% floor`, then a `Disposition:` line stating the FAIL is NON-BLOCKING via the ratified exemption
(R3 acceptance path) with changed-line no-regression. Both a PASS row (e.g., the test-run row) and a
FAIL row (coverage) can coexist for the same language; the gate needs >=1 FAIL row when below floor.

**How to apply:**
- Compute each language's repo-wide figure the way the hook does before writing the verdict:
  LCOV `LF/LH` for TS/Python; JaCoCo `//counter[@type="LINE"]` (summed across ALL levels — same
  ratio) for PowerShell/C#. Note C# `artifacts/csharp/coverage.xml` is Cobertura, has no `<counter>`
  elements, so the hook returns `$null` repo-wide and skips the numeric gate — a `PASS` C# row is
  fine (see [[csharp-coverage-artifact-is-cobertura]]).
- Branch: hook checks `//counter[@type="BRANCH"]`; Pester JaCoCo often has none → `$null` → skipped.
- Never put a narrowing token (`N/A`, `not applicable`, `out of scope`, `UNVERIFIED`,
  `informational only`, `context only`) on ANY coverage-scoped row for a changed language.
- `Get-ChangedLanguageSet` only detects a language from summary lines matching EXACTLY
  `^\s*-\s+<path>\s+\(\+\d+/-\d+\)$`. When correcting the C# misclassification in the summary
  overview (see [[coverage-hook-trusts-misclassified-summary]]), the re-added C# lines MUST use the
  `(+N/-N)` numstat format or the hook silently skips C# enforcement.
- Simulate before finalizing: dot-source the hook and call
  `Invoke-FeatureReviewCoverageValidation -RawPayload (@{output=$tokens}|ConvertTo-Json)`.
</content>
