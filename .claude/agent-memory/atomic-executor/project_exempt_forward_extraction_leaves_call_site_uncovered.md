---
name: exempt-forward-extraction-leaves-call-site-uncovered
description: Extracting an SDK body into an [ExcludeFromCodeCoverage] forward leaves the CALL statement inside the measured member, so a per-member >=90% coverage gate on host-bound code is structurally unsatisfiable
metadata:
  type: project
---

The "extract the host-bound SDK body into a small `[ExcludeFromCodeCoverage]` private forward so the
testable decision stays measured" pattern does NOT make the enclosing member reach 90%. The `return
ForwardX(...)` / `ForwardX(html);` call statement and its closing brace stay in the measured member
and are only reachable on the SDK-bound happy path, so they read as uncovered forever.

Measured on #476 (`WebView2BreadcrumbHost` / `WebView2CoreInitializer`), 2026-08-27:
`NavigateToString` 5/8 = 62.50% (uncovered = the inline `ForwardNavigateToString` fallback),
`DetachCore` 6/9 = 66.67%, `CreateEnvironmentAsync` 10/12 = 83.33%, `EnsureCoreWebView2Async`
4/6 = 66.67%. Aggregate over the eleven newly measured members 86/99 = 86.87%. Removing the two
class-level exemptions moved the repo line rate only +0.0133 pp (85.1302% -> 85.1435%), so the
denominator cost is small but the per-member gate still fails.

**Why:** the extraction moves the *body* out, not the *call*. A one-line `return ForwardX(...)`
member is 2 measured lines (statement + brace) of which 0 can be covered without the real host.

**How to apply:** when planning a coverage gate over members that carry this pattern, gate on the
*decision* lines (guards, null checks, dispatcher routing) or on the file rate, not on a per-member
90% floor. If a plan already pins a per-member floor, do not invent tests that lean on unverified SDK
throw behaviour to scrape the call line — record the shortfall with member and figure (most plans'
acceptance explicitly allows "recorded as met or, where not met, named with the specific member and
figure") and escalate at completion. Relates to [[csharp-canonical-coverage-artifact-conversion]] and
[[coverage-delta-reproduce-baseline-counting-method]].
