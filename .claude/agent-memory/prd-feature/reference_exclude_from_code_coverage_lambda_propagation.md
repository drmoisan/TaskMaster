---
name: exclude-from-code-coverage-lambda-propagation
description: Method-level [ExcludeFromCodeCoverage] does not suppress nested lambda bodies but class-level does; a partial-class attribute exempts the whole type
metadata:
  type: reference
---

Two measured facts about `[ExcludeFromCodeCoverage]` under the repo's
`dotnet-coverage --output-format cobertura` writer. Both were verified numerically against
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
during epic #136 child F13 (issue #455); tracked upstream as issue #457.

1. **Method-level does NOT suppress nested lambdas.** A lambda is lifted into a compiler-generated
   closure whose method does not inherit the attribute, so the collector still instruments it and
   the lines are permanently uncovered. Measured: `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`
   has method-level attributes at `:394` and `:457`, yet source lines 406, 409 and 471-490 stay in
   the denominator — 23 of that file's 24 uncovered lines.
2. **Class-level DOES suppress nested lambdas.** `QuickFiler/Viewers/WebView2Messenger.cs` has four
   dispatcher lambdas under a class-level attribute and produces no `filename=` entry in the report
   at all.

**Trap:** an attribute on one part of a `partial` type applies to the whole type. Extracting exempt
production forwarders into a `partial` of the measured class exempts every covered line of that
class. The extraction must be a separate type.

**How to apply:** when a spec or plan proposes relocating host-bound forwarders behind an exemption,
require a dedicated non-partial adapter type with a single class-level attribute, not scattered
method-level attributes. Related: [[interface-files-zero-coverage-denominator]],
[[quickfiler-perfile-coverage-baseline]].
