---
name: splatting-is-the-line-budget-lever-for-ceiling-bound-test-files
description: When a plan adds parameters to a function whose test file sits near the 500-line ceiling, converting multi-line continuation call sites to splatted hashtables frees five lines per site — enough to absorb the change without deleting a test
metadata:
  type: project
---

A signature change ripples into every call site in the covering test file. In this repository test
files use the backtick-continuation style, so one call costs six lines. Converting it to
`Function @script:argumentSet` costs one, freeing five lines per site.

Observed on 2026-09-13 (item 873): `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` had
four lines of headroom against the 500-line ceiling and had to absorb two extra arguments at ten call
sites plus a new mock, two fixture replacements and a mock-body extension. A naive inline edit landed
at 517. Splatting the ten sites and compressing only the newly authored comments brought it to 491
with all 28 tests intact.

**Why:** the ceiling is a hard repo rule and deleting a test to fit it is never the answer. The lever
has to come from the call-site shape, not from coverage.

**How to apply:**

- One hashtable per *distinct argument set*, or one base plus `.Clone()` per site with only the
  differing keys overridden. Chained clones are fine and cheapest: each derived set costs one clone
  line plus one line per differing key.
- Hashtable `+` merge is **not** a substitute for `.Clone()` when a key is present in both operands —
  `@{a=1} + @{a=2}` throws. Clone then assign.
- Never supply a parameter both inside the splat and explicitly on the same invocation; PowerShell
  treats that as a binding error, not an override.
- If a Pester `BeforeEach` declares fixture scalars the call sites also need, invert the dependency:
  declare the hashtable in `BeforeAll` and have the `BeforeEach` read the scalars *out of it*. That
  keeps existing assertions that reference those scalars working, removes the duplication, and is line
  -negative because the long fixture array moves rather than being copied.
- Measure with `(Get-Content -LiteralPath $abs).Count` after the format step, since the formatter can
  change the count.

Related: [[project_appglobalstests_at_500_line_ceiling]],
[[project_poshqc_format_strips_bom_and_crlf_only_when_it_rewrites]].
