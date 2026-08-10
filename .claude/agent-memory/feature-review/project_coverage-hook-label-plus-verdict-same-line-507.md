---
name: coverage-hook-label-plus-verdict-same-line-507
description: validate-feature-review-coverage.ps1 requires the language label token, a coverage keyword, AND PASS/FAIL all on the SAME physical line, with no banned narrowing word anywhere on any line satisfying label+coverage
metadata:
  type: project
---

#507 remediation-cycle-exit review (2026-08-08): `Test-LanguageCoverageRow` in
`.claude/hooks/validate-feature-review-coverage.ps1` is stricter than prose-level summarization
suggests. It filters `policy-audit` text to lines matching a language label (`C#`, `CSharp`,
`csharp`, `.NET`, `dotnet` for CSharp — note `csharp` matches case-insensitively as a substring, so
a bare artifact path like `` `artifacts/csharp/coverage.xml` `` counts as a label line), then
further filters those to lines also containing a coverage keyword
(`coverage|lcov|line[s]?\s+hit|pester`), then requires at least one of those lines to also contain
literal `PASS` or `FAIL`. Prose spread across multiple wrapped markdown lines (e.g. cycle-1's
`#507` policy-audit, which had "C#" and "coverage" on one line and "FAIL"/"non-blocking" several
lines later in the same paragraph) does NOT satisfy this — I confirmed by dot-sourcing the hook and
calling `Test-LanguageCoverageRow` directly against that exact file, which returned
`"CSharp coverage rows contain neither a PASS nor a FAIL verdict."` even though the paragraph read
as compliant to a human. Separately, ANY line matching label+coverage (not just the verdict line)
that also contains a banned narrowing phrase (`informational only|context only|out of plan
scope|out of scope|not applicable|N/A|UNVERIFIED`, case-insensitive) unconditionally fails the
check, even if a different line later gives a clean PASS/FAIL.

**Why:** Confirmed cycle-1's own `policy-audit.2026-08-08T17-45.md` would NOT have passed this
hook's coverage check as literally worded (verified via direct dot-source simulation), despite the
review having proceeded to remediation. Writing a short, single-line, unambiguous verdict like
`` "C# coverage verdict: FAIL (repo-wide raw coverage below floor, pre-existing, non-blocking
disposition)." `` reliably passes; relying on a longer narrative paragraph does not.

**How to apply:** Before finalizing any policy-audit with a coverage section, dot-source
`.claude/hooks/validate-feature-review-coverage.ps1` and call `Test-LanguageCoverageRow` directly
against the drafted text for each changed language (pass `$null`/`$null` for RepoWidePct/BranchPct
when the canonical artifact is intentionally absent, matching the reviewed feature's actual state).
Also simulate `Invoke-FeatureReviewCoverageValidation` end-to-end with a synthetic
`policy-audit-path`/`code-review-path`/`feature-audit-path` payload before reporting the final
tokens, to catch path-regex or cross-artifact-timestamp mismatches too. See
[[taskmaster-validator-memories-are-cross-repo]] for why the cross-repo heading-template memories
do not apply here — this hook, not a heading validator, is the real gate.
