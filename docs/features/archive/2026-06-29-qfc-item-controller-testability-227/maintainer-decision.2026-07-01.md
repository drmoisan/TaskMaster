# Maintainer Decision — Issue #227 (R2 Exemption-Boundary Ratification)

- **Date:** 2026-07-01
- **Decision owner:** Dan Moisan (project maintainer)
- **Decision:** Option 2 — REJECT. Ratification of the 103-member `[ExcludeFromCodeCoverage]`
  boundary is **DENIED**.
- **Status:** Ratification denied; new work directed.

## Decision

The maintainer denies ratification of the exemption boundary recorded in
`evidence/other/exemption-boundary.2026-06-29T12-40.md` (101 methods + 2 properties across the
nine `QfcItemController` partials).

Rationale (maintainer, verbatim intent): none of these members should be untestable. The purpose
of introducing an interface for the viewer was to make all of this code testable. Exempting the
members with `[ExcludeFromCodeCoverage]` defeats that purpose. The correct remedy is to introduce
seams and interface abstractions so the currently-exempted paths become testable — for example,
control collections currently typed as `IList<Button>` (concrete WinForms controls) should be
evaluated for retyping to an interface such as `IList<IButton>`.

## Directive

Redesign the seam so the exempted members become unit-testable, rather than exempting them:

1. **Control abstraction interfaces.** Extend the existing `UtilitiesCS/Interfaces/IWinForm/`
   layer (currently `IControl`, `IContainerControl`, `IControlCollection`, `IForm`,
   `IScrollableControl`, `IUserControl`) with leaf-control interfaces as needed (candidates:
   `IButton`, `ILabel`, `ICheckBox`, `IComboBox`, `ITextBox`, and any others required), plus
   thin adapters over the concrete WinForms controls. Retype control collections and members on
   `IItemViewer`/`ItemViewer` to the interface types where that enables the controller logic to
   be exercised without a live control tree.
2. **UI-dispatch seam.** Replace direct `Control.Invoke` / `BeginInvoke` / `Dispatcher` marshaling
   with an injectable dispatch seam so UI-thread routing is mockable (aligns with the DI-seam rule
   ordering: interface > delegate > adapter).
3. **WebView2 core-init adapter.** Wrap `EnsureCoreWebView2Async` and the init-completed handler
   behind an adapter seam.
4. **Outlook COM adapters.** Wrap the `MailItem` / `MailItemHelper` / `ConversationResolver.LoadAsync`
   boundaries so the controller logic is testable without a live Outlook host.
5. **`async void` event handlers.** Keep handlers as thin delegators to testable async methods so
   the substantive logic is covered.

The objective is to reduce the exemption set toward zero. Any member that remains genuinely
irreducible after seams are introduced must be individually justified in a follow-up boundary
artifact and re-submitted for ratification; a broad category-level exemption is not accepted.

## Effect on acceptance criteria

- **AC5** is NOT satisfied. The "ratify the exemption boundary" path is closed. AC5 must be met by
  making the members testable (≥80% testable floor and ≥90% new/changed-code target through real
  coverage, not exemption).

## Process effect

- Remediation cycle 1 exits **without** meeting its exit gate (R2 not ratified). R1 (canonical
  `artifacts/csharp/coverage.xml`) remains resolved and is unaffected.
- The work re-enters the large-path design pipeline: research → spec update → atomic plan →
  atomic execution → feature review. This is a cross-cutting seam redesign, not a targeted
  remediation touch-up.

## References

- `evidence/other/exemption-boundary.2026-06-29T12-40.md` (denied boundary)
- `2026-06-29T13-15-audit/remediation-inputs.2026-06-29T13-15.md`, `2026-06-29T13-20-remediation/remediation-inputs.2026-06-29T13-20.md` (R2 escalation)
- `UtilitiesCS/Interfaces/IWinForm/` (existing control-interface layer to extend)
