---
name: quickfiler-duplicate-iqfcformcontroller-435
description: Issue #435/F6 — QuickFiler.Interfaces.IQfcFormController is compiled dead code; its only consumer file (Interfaces/IQfcHomeController.cs) is not in the csproj; namespace resolution turns on compilation-unit-vs-namespace-body using placement
metadata:
  type: project
---

Two files named `IQfcFormController.cs` are both compiled by `QuickFiler.csproj`
(`Controllers\...` and `Interfaces\...`), declaring different interfaces with the same simple name.
The `QuickFiler.Interfaces` one has **zero implementers and zero compiled consumers** — its only
consumer, `QuickFiler/Interfaces/IQfcHomeController.cs`, is itself absent from the csproj (only
`Controllers/IQfcHomeController.cs` is compiled, and it never mentions `IQfcFormController`).
Two of its members (`MaximizeQfcFormViewer`, `MinimizeQfcFormViewer`) have no implementation
anywhere in the repo — that grep is the fastest one-shot proof of deadness.

**Why:** F6 research (2026-08-07) had to determine which variant was authoritative before the plan
could touch either. The maintainer-facing risk was recommending a deletion that would break
`QfcHomeController` (sibling child F7); it would not.

**How to apply:**
- The decisive mechanism is `using`-directive **placement**, not the using list. In this repo the
  QuickFiler/QuickFiler.Test files put usings at *compilation-unit* scope (before `namespace`), so
  C# lookup resolves a bare name through the enclosing-namespace chain FIRST and only reaches the
  usings at the global step. That is why `namespace QuickFiler.Controllers.Tests` files with
  `using QuickFiler.Interfaces;` still bind to `QuickFiler.Controllers.IQfcFormController`. Do not
  reason from the using list alone — check whether the usings are inside or outside the namespace body.
- Fast corroboration trick: find an assignment that requires an inheritance relationship. Here
  `QfcHomeController.cs` backs `public IFilerFormController FormController` with a field typed
  `IQfcFormController`; only the `Controllers` variant derives from `IFilerFormController`.
- `QuickFiler/Notes/**`, `QuickFiler/Legacy/**`, `Viewers/QfcFormViewerExpanded.cs`,
  `Viewers/QfcFormViewerDark.cs`, and `Interfaces/IQfcHomeController.cs` are all in the working tree
  but NOT in the csproj. Always verify `<Compile Include=...>` before counting a consumer.
- `Viewers/QfcFormViewerExpanded.cs` + `QfcFormViewerDark.cs` are in `namespace QuickFiler` with
  BOTH `using QuickFiler.Controllers;` and `using QuickFiler.Interfaces;` and a bare
  `IQfcFormController` reference — they would fail CS0104 if ever added to the csproj. Useful as the
  concrete demonstration of the latent hazard.

See also [[qfc-item-controller-227-r2-denial]] for the precedent that coverage-exemption boundaries
need per-member barrier analysis before any exemption is accepted.
