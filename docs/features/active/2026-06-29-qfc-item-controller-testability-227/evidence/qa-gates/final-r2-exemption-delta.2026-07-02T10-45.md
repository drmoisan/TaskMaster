# Final QA — Exemption-Count Delta (P8-T6, AC8, AC10)

Timestamp: 2026-07-02T10-45

## Delta

| Milestone | Exemption count |
|---|---:|
| P0-T7 starting (cycle-1, denied) | 103 |
| After Phase 5 | 57 |
| After Phase 6 | 42 |
| After Phase 7 (final) | **41** (38 `QfcItemController` members + 3 DI-adapter shims) |

Net reduction: **103 -> 41**.

## De-exempted members map to >= 1 passing test

Every member removed from the exemption set is covered by at least one passing test:

- Phase 5 (~46 members): covered by the per-cluster `QfcItemController.*Tests` files (Initialization,
  ViewerSetup, Conversation, EventWiring, EventHandlers, Navigation, FocusAndTheme, MailActions,
  Properties) — see `evidence/qa-gates/p5r-tests-coverage.2026-07-02T09-16.md`.
- Phase 6 (15 members + extracted cores): covered by `SeamDispatcherTests` (9 dispatcher members),
  `SeamCoreTests` (Reply/ReplyAll/Forward, CollapseConversation EntryID fallback, the five `*Core`
  methods, `HandleWebViewInitializedAsync`), and `SeamFactoryTests` (`PopulateConversation()`,
  `FlagAsTask`, `FlagAsTaskAsync`, `MoveMailAsync`, `WireIntentEvents`).

## No blanket/category exemption remains (AC8, AC10)

- The 41 residuals are enumerated and individually justified in
  `evidence/qa-gates/p7r-residual-verification.2026-07-02T10-30.md` and
  `evidence/other/exemption-boundary.2026-07-02T10-30.md`; each carries an inline per-member
  justification comment (verified programmatically).
- No member exercisable through the narrowed `IItemViewer` or a mockable collaborator retains an
  exemption. The residuals are exclusively: concrete control-tree orchestration/traversal, out-of-scope
  COM collaborators (`MailItem` construction, `FolderPredictor`), the out-of-scope `Theme` and
  `TlpCellSnapShot` collaborators, deliberate virtual test seams, thin WinForms-event shells, and the
  three DI-adapter forwarding shims.

Output Summary: Exemption count reduced 103 -> 41; every de-exempted member maps to >= 1 passing test;
no blanket/category exemption remains (AC8, AC10 satisfied).
