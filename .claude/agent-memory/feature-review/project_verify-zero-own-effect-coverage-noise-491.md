---
name: verify-zero-own-effect-coverage-noise-491
description: technique for independently verifying a "coverage shortfall has zero own-effect" claim from dual committed Cobertura XMLs, used on #491
metadata:
  type: project
---

When an executor claims a post-change coverage shortfall is environmental noise unrelated to the
change (rather than a real regression), verify it directly from the two committed Cobertura XML
files rather than accepting the narrative:

1. Confirm root `<coverage lines-covered=... lines-valid=...>` attributes match the claimed
   baseline/post-change numbers exactly, and that `lines-valid` (denominator) is identical between
   the two files (proves same instrumented surface).
2. For "zero own-effect" claims (e.g. the changed files are in an excluded/uninstrumented
   assembly), grep both XML files for the changed assembly's `name="..."` and `filename="..."`
   patterns — zero matches in both files is direct proof of zero own-effect, not just an assertion
   about the harness's documented exclusion policy.
3. For "attributed to unrelated file X" claims, extract that file's `<class line-rate=...>` node
   from both XML files and confirm a real numeric drop; then `git diff --name-only <base> <head>`
   to confirm X is not in the branch's diff.
4. If the executor ran multiple diagnostic capture attempts but deleted the raw XML for all but the
   canonical one, treat only the canonical (officially-cited) capture as verified; disclose the
   others as narrative-only corroboration in the audit rather than silently accepting "reproducible
   across N runs" as proven.

This four-step check let a #491 AC10 shortfall (85.5627% vs 85.5788% baseline, -10 lines) be
dispositioned non-blocking on independently verified evidence rather than on trust: zero
`QuickFiler.Test` occurrences in either Cobertura file, and the entire shortfall traced to
`SegmentStopWatch.cs` + `OlTableExtensions.Etl.cs`, both confirmed absent from the branch diff.

Related: [csharp-coverage-constants-nondeterministic], [jacoco-summary-substitution-is-valid-coverage-evidence].
