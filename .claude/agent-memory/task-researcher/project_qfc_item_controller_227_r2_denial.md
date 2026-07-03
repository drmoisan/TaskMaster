---
name: qfc-item-controller-227-r2-denial
description: Issue #227 — maintainer denied ratification of the 103-member exemption boundary (2026-07-01); directed seam redesign instead of exemption.
metadata:
  type: project
---

On 2026-07-01 the maintainer (Dan Moisan) DENIED ratification of the `QfcItemController`
103-member `[ExcludeFromCodeCoverage]` boundary recorded in
`docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-06-29T12-40.md`.

**Why:** the maintainer's stated intent for introducing `IItemViewer` was to make the controller
testable; blanket per-method/per-partial exemption defeats that purpose. Directive: redesign seams
(control-abstraction interfaces, injectable UI-dispatch, WebView2 core-init adapter, Outlook COM
adapters, thin-delegator `async void` handlers) so the exemption count trends toward zero, with any
true residual individually justified — not a broad category-level exemption.

**How to apply:** Do not accept blanket `[ExcludeFromCodeCoverage]` on a method/partial-file just
because the class as a whole is COM/WinForms-bound in this codebase — the maintainer expects a
per-member barrier analysis before exemption, and will reject boundaries that exempt members whose
bodies are actually already reachable through an existing mockable seam (this happened for ~38 of
the 103 members in #227 — see `artifacts/research/2026-07-01T00-00-qfc-item-controller-seam-redesign-research.md`).
This is a general precedent for any future `[ExcludeFromCodeCoverage]` boundary submitted for
ratification in this repo, not specific to QuickFiler.

Process effect: work re-enters research → spec update → atomic plan → atomic execution → feature
review (not a remediation patch to the existing plan).
