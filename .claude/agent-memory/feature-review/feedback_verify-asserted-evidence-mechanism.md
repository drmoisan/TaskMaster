---
name: verify-asserted-evidence-mechanism
description: When an evidence capture or AC note claims a behavior is "proven by unit tests", grep the test project for the asserting mechanism before accepting it; on #418 the AC-11 capture's dual-channel claim was false because no test touched Trace or log4net.
metadata:
  type: feedback
---

An evidence artifact that names its own evidentiary basis — "proven by unit tests", "covered by the
regression tests", "asserted in `<Project>.Test`" — is making a checkable claim. **Grep the test
project for the asserting mechanism before repeating the claim in a review artifact.** Coverage of a
line is not assertion of its behavior.

**Why:** On issue #418 cycle 4, the maintainer's AC-11 designer-load capture disclaimed an unexercised
observation with: "The dual-channel behavior is proven by unit tests in `SVGControl.Test`." Verified
false — `grep -rn "Trace\|log4net\|Listener\|Appender\|DescribeFailure" SVGControl.Test/*.cs` returns
**zero matches**. No test installs a `TraceListener`, captures `log4net` output, or asserts
`DescribeFailure`. The `Trace.TraceError` lines *are* executed by the parse-failure constructor tests,
which is exactly why `DescribeFailure` measures 100% line coverage — and that 100% figure is what made
the claim feel corroborated. Execution is not assertion.

The verdict did not change (AC-3's requirement was an implementation shape, statically checkable by
inspection), but the clause was load-bearing: it was the fallback offered for the limitation the
capture *had* disclosed, so the reader was told the basis was one notch stronger than it was.

**How to apply:**
- Treat "proven by X" in any capture, AC evidence note, or plan rationale as a claim to verify, not
  context to inherit. The cheap check is one `grep` for the mechanism (`Trace`, `Listener`, `Mock<`,
  the member name) in the test tree.
- A 100%-line-coverage figure on a logging or diagnostic member is weak evidence that anything asserts
  it. Ask whether a test would still fail if the emission were deleted.
- When the claim is false but the verdict survives, say both plainly: record the corrected basis
  (usually "verified by code inspection, executed but not asserted by the tests") and keep the PASS.
  Downgrading a criterion for an overstated citation is as wrong as accepting the citation.
- Watch for this specifically in *human-authored* captures. A maintainer reporting a GUI observation is
  not in a position to audit which tests exist, so the citation is usually inherited from a prior
  artifact rather than checked.

Related: [[verify-parity-claims-in-remediation-inputs]] — the same discipline applied to claims this
agent writes rather than claims it reads.
