---
name: coverage-delta-reproduce-baseline-counting-method
description: A coverage-delta task must reproduce whatever per-<line> counting method the baseline artifact used (deduped vs all-descendant), or package/class before-after numbers differ ~2x and look like a catastrophic regression
metadata:
  type: project
---

When a plan task says "compare the post-change `SVGControl` package figure against the baseline's
`1412 / 3266`", read *how* the baseline artifact arrived at its denominator before computing yours.
Cobertura repeats every statement line twice — once under `<class><methods><method><lines><line>` and
once under the class-level `<class><lines><line>` — so two defensible methods give roughly 2x-different
package and class denominators:

- **Deduped / class-level only** (`$class.lines.line`): `SVGControl` reads 853/1838 = 46.41%.
- **All `<line>` descendants** (`$node.SelectNodes('.//line')`): `SVGControl` reads 1648/3500 = 47.09%.

The #418 baseline used the **all-descendant** method (its per-class rows sum exactly to 3266, and
`SvgRenderer` 264/422 is 211 statement lines doubled). Computing the post-change figure the deduped way
would have reported 1838 total against a 3266 baseline — a denominator that appears to have *halved*,
which is nonsense and would have triggered a false `COVERAGE_DENOMINATOR_CHANGE` escalation.

Per-`<method>` figures are unaffected: a `<method>` node has exactly one `<lines>` child, so
`line-rate` and `.//line` counts inside a method are honest either way. Only package- and class-level
rollups diverge.

**Why:** #418 `[P2-T9]` gated a `>= 90%` newly-added-member rule on `<method>` `line-rate` (safe) while
also requiring package/class before-after deltas (method-sensitive). Verifying the baseline's method
first turned an apparent package regression into the true result: 1412/3266 = 43.23% -> 1648/3500 =
47.09%, an improvement.

**How to apply:** Before writing any coverage-delta artifact, sanity-check the baseline by recomputing
one of its own recorded rows from the current XML structure and confirming your script reproduces the
shape (e.g. that the baseline's per-class denominators sum to its package denominator). State the
counting method explicitly in the delta artifact so the next reader can reproduce it.

**Correction (2026-09-02, issue #532):** the claim that repo-wide root `<coverage>` attributes are
already deduped and need no adjustment holds only for **raw `dotnet-coverage` generator output**. It
does NOT hold for a post-processed `ConvertTo-KoverageCoberturaXml` artifact — before issue #441 was
fixed, that post-processing step's root attributes *were* the (undeduped) all-descendant sum, which
is the exact defect #441 corrects. Check which kind of artifact you're reading before applying the
"no adjustment needed" shortcut; do not assume it transfers from raw output to a post-processed file.
See also [[csharp-canonical-coverage-artifact-conversion]] and
[[dotnet-coverage-denominator-nondeterminism]].
