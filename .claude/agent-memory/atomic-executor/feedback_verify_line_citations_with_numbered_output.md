---
name: verify-line-citations-with-numbered-output
description: Never assert a file:line citation from an unnumbered sed/head window — always use grep -n "" or Read; a hand-counted preflight finding got applied and corrupted three correct citations
metadata:
  type: feedback
---

When asserting any `file:line` or `file:start-end` citation in a preflight finding, audit, or
review, derive it from **line-numbered output** (`grep -n "" <file> | sed -n 'A,Bp'`, the Grep
tool with `-n`, or Read). Never hand-count offsets from a `sed -n 'A,Bp'` / `head` window.

**Why:** During #438 preflight cycle 1, I hand-counted line offsets from a `sed -n '300,395p'`
window and reported three citations as off-by-one (advisory "A3"). All three were already
correct. The planner faithfully applied my delta in revision 1, which *corrupted* previously
correct citations in both the plan and `spec.md` — `EventHandlers.cs:177` became `:178` (a
closing brace), the sanctioned test span `313-350` became `313-349` (dropping the method's
closing brace), and `355-388` became `353-387`. Cycle 2 caught it only because I re-verified
with `grep -n ""`. The coordinator's course correction was explicit: "this time verify any
line citation against line-numbered output before asserting it."

**How to apply:**
- Applies to every preflight/audit/review finding, not just blocking ones. A downstream planner
  treats even an advisory as authoritative and will apply it verbatim.
- Blank lines and attribute lines are the usual miscount source: a `[TestMethod]` attribute, the
  `/// <summary>` opener, and the trailing `}` are each easy to slide by one.
- A method-span citation convention in this repo includes the attribute line through the closing
  brace (e.g. `313-350`), not the signature through the last statement.
- If a citation looks wrong, re-read with numbers before writing the delta; the cost of one extra
  numbered read is far below the cost of a corrupted plan revision cycle.
- Related: [[project_418_plan_rationale_clauses_are_evidence]] — unmeasured world-state claims in
  plan prose are the other recurring source of preflight churn.

## Corollary: never assert a file LOCATION you have not listed (#498 preflight cycle 2)

The same rule governs *where* a file lives, not only which line. In #498 preflight cycle 2 I
reported that the CSharpier tool manifest is at `.config/dotnet-tools.json` and asked the planner
to "correct" a plan sentence that was already right. `.config/` does not exist in this repository
at all; the manifest is at the repo root as `dotnet-tools.json` with `"isRoot": true`, which
CLAUDE.md also states. The planner independently verified and correctly REJECTED the observation.

**How to apply:** before writing "the file is at X", run `ls X` or `find . -maxdepth 2 -name <f>`.
Ecosystem defaults are the trap — `.config/dotnet-tools.json` is the .NET convention and this repo
deviates from it. A plausible-by-convention location is a hypothesis, not a fact.
