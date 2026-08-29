---
name: derive-counts-exhaustively-before-approving
description: Any count/enumeration written into an approved spec.md acceptance criterion must come from a symbol-family search cross-checked by a second independent search, never a single-pass grep; counting tools must be section-scoped
metadata:
  type: feedback
---

Two rules that both govern numbers landing in an approved artifact.

**Rule A — never commit a count to an approved acceptance criterion from a single-pass
grep.** When deriving any count, enumeration, or population that will be written into a
`spec.md` acceptance criterion ("N call sites", "N identifiers removed", "N
variable-argument sites"), search by the full symbol/method FAMILY — every overload of a
reflection call, not one named pattern — and cross-check the resulting number with a
second, independently constructed search before it is written.

**Rule B — a tool that counts checkboxes or list items inside a generated document must
scope its match to the specific named section** (e.g. `## Acceptance Criteria`), not the
whole file, and must be exercised against a fixture containing unrelated checkboxes
OUTSIDE that section before it ships.

**Why:** An acceptance criterion is a contract that the executor is judged against. A
number derived from one narrow pattern reads authoritative but silently under-counts the
real population, so the AC certifies completion over a subset while the remainder stays
untouched — and because the number is already approved, no later gate re-derives it. Rule
B is the same failure in tooling form: a whole-file checkbox scan absorbs unrelated
checkboxes from other sections, so the count it reports is not the count of the thing it
claims to measure.

**How to apply:** Carry both rules into every preparation delegation whose deliverable is
a `spec.md` with an `## Acceptance Criteria` section, and into any plan that asks the
executor to assert over a count. Pairs with
[[self-review-before-preflight-round-one]] — an unverified count is exactly the class of
gap the internal self-review pass is meant to catch before preflight sees it.
