---
name: no-coverage-exemption-when-purpose-is-testability
description: Maintainer rejects [ExcludeFromCodeCoverage] as a substitute for real testability seams when the feature's purpose is testability
metadata:
  type: feedback
---

When a feature's stated purpose is to make code testable, the maintainer (Dan Moisan)
will NOT ratify `[ExcludeFromCodeCoverage]` exemptions over COM/WinForms/Outlook-bound
members as the way to satisfy the coverage AC. He expects genuine interface/adapter seams
that make the members unit-testable instead.

**Why:** On issue #227 the delivered refactor exempted 103 members (101 methods + 2
properties) as COM/WinForms/host-bound and asked the maintainer to ratify the boundary
(the same pattern he accepted for #223). He denied it: "None of these should be untestable.
The purpose of creating an interface for the viewer was to make all of this testable."
He explicitly pointed to retyping concrete control collections (e.g. `IList<Button>` →
`IList<IButton>`) and building seams. Note this reverses the #223 precedent where an
authority-scoped exemption WAS ratified — the distinguishing factor is that #227's whole
objective was testability, so exemption defeats the purpose.

**How to apply:** For testability-purpose C# work, do not plan around
`[ExcludeFromCodeCoverage]` for control/dispatch/WebView/COM boundaries. Plan real seams
first: extend `UtilitiesCS/Interfaces/IWinForm/` with leaf-control interfaces + thin
adapters, add a mockable UI-dispatch seam, and adapters for WebView2 core-init and Outlook
COM. When presenting a coverage AC for maintainer ratification, expect denial if the
denominator is defined by exemptions rather than by seams; surface the seam plan instead.
Reserve exemptions for members that are genuinely irreducible after seams, justified
individually (not by broad category). Related: [[remediation-loop-strict-handoff]].
