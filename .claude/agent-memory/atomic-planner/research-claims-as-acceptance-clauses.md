---
name: research-claims-as-acceptance-clauses
description: Never encode an unmeasured third-party-library behavior claim from a research artifact as a literal acceptance clause; require the measurement or phrase the clause behavior-agnostically
metadata:
  type: feedback
---

Do not lift an unverified claim about third-party library behavior out of a research artifact and write it into a task's acceptance clause as a literal assertion. Either require the executor to measure it first, or phrase the clause so it holds under either outcome.

**Why:** #418. Research §1.4 asserted that `Svg.SvgDocument.Open` "returns `null` without throwing" for element-free input such as `Array.Empty<byte>()`. That claim propagated into three plan task bodies and into AC-5's own note in `issue.md`. Execution disproved it — an empty payload raises `System.Xml.XmlException: Root element is missing` from `SvgDocument.Create<T>(XmlReader, ...)` at the `XmlReader` level, before any SVG element handling. Two tests failed on their own premise rather than on the production fix, and a whole revision pass was spent correcting the plan and amending the AC before the criterion could be checked off (checking off an AC endorses its text, so a false claim must not survive into the check-off).

**How to apply:**
- When a research artifact states a null-vs-throw, empty-input, or default-value behavior of a package the repo consumes, treat it as unverified unless the artifact records a measurement.
- Prefer clauses that assert the *contract under test* (`returns false`, `does not throw`, `leaves Document null`) over clauses that assert the *library's internal failure shape* (`with a null error for Array.Empty<byte>()`). The former survived this correction unchanged; the latter did not.
- When a branch exists in first-party code that only a specific library behavior can reach, plan the DI seam as the coverage route from the start rather than relying on a real input shape.
- When correcting a disproved claim, scope the correction to what was actually measured. An "empty payload throws" measurement does not license the broader "no payload can reach the null path" claim; record the remainder as an open question. Related: [[coverage-gate-clr-invoked-private-members]].
