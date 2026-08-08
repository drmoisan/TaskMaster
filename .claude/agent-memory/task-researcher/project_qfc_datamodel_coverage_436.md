---
name: qfc-datamodel-coverage-436
description: "Issue #436 (epic #136 child F5) QfcDatamodel.cs research: [ExcludeFromCodeCoverage] is type-scoped across all 3 partials, so removal is a 3-file event needing sequencing"
metadata:
  type: project
---

Research completed 2026-08-08 for `QuickFiler/Controllers/QfcDatamodel.cs` (issue #436, child F5 of
epic `quickfiler-per-file-coverage` #136). Artifact:
`docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-qfcdatamodel.md`.

Load-bearing findings that are not obvious from reading any single file:

1. **`[ExcludeFromCodeCoverage]` on one partial declaration excludes the whole type.** The attribute at
   `QfcDatamodel.cs:25` currently keeps `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs` AND
   `QfcDatamodel.FrameBuilding.cs` out of the coverage denominator. Verified against the committed
   Cobertura report from issue #424 (`.../2026-08-06-...-424/evidence/qa-gates/coverage-final.cobertura.xml`
   contains no `QfcDatamodel` class entry) plus that feature's `coverage-delta` note. Consequence:
   attribute removal must be the LAST production task, after FrameBuilding's COM-bound members have
   seams or member-level attributes.
2. **Conclusion reached: no irreducible remainder for `QfcDatamodel.cs`.** The only untestable members
   (two `MessageBox.Show` methods and a dead RunWorkerCompleted handler) are verified dead code; the
   recommendation is delete, not exempt.
3. **UtilitiesCS grants `InternalsVisibleTo` only to `UtilitiesCS.Test` and `ToDoModel.Test`** — not
   `QuickFiler.Test` (there is a deliberately commented-out grant). So `DfDeedle.TableEtlInvoker` and
   friends are unreachable from QuickFiler tests; frame-building needs a QuickFiler-side seam.

**Why:** epic #136 mandates per-file 80% coverage with refactor-first, exempt-only-the-irreducible.
Getting the exclusion scope wrong would make a sibling phase look like a large regression.

**How to apply:** when planning or reviewing any QuickFiler partial-class coverage child, check whether
a sibling partial carries the exemption attribute before assuming a file is measured, and sequence
attribute removal last. See [[qfc-item-controller-227-r2-denial]] and
[[feedback-exemption-audit-check-proven-techniques]] for the exemption-boundary precedent.
