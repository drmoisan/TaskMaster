# Feature Inputs Read Confirmation — Issue #171

- Timestamp: 2026-06-02
- Task: [P0-T2]

## Authoritative inputs read

| Input | Path |
|-------|------|
| Issue (context via spec/research) | `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/spec.md` (Context/Repro sections reproduce issue #171) |
| Spec (Definition of Done) | `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/spec.md` |
| User story (AC1–AC8) | `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/user-story.md` |
| Research artifact | `artifacts/research/quickfiler-high-confidence-prefilter-171.2026-06-02T13-45.md` |

## Acceptance criteria (user-story numbering)

1. AC1 — Pre-UI scoring and filtering in `RunAsync` between `InitEmailQueueAsync` and `LoadItemsAsync`, before any UI item controller.
2. AC2 — Below-threshold (`TopScore() < cutoff`) excluded before UI load.
3. AC3 — No-suggestion (`TopScore() == 0`) excluded before UI load.
4. AC4 — Predetermined folder carried and preselected (not index 1).
5. AC5 — Inclusive boundary: top score == cutoff retained.
6. AC6 — No transient render; post-UI removal pass not invoked in HC sequence.
7. AC7 — Mode disabled => standard `IList<MailItem>` path unchanged, no pre-pass.
8. AC8 — New logic in new file; scoring reused; oversized files not worsened; off-UI scoring; DI-seam testable; toolchain pass.

## Seam line anchors (verified against current source)

| Anchor | Plan ref | Verified current |
|--------|----------|------------------|
| `RunAsync` InitEmailQueueAsync `Task.Run` end | line 257 | line 257 (confirmed) |
| `RunAsync` `LoadItemsAsync(listEmail)` | line 262 | line 262 (confirmed) |
| `QfcFormController.LoadItemsAsync(IList<MailItem>, ProgressTracker)` | line 898 | line 898 (confirmed) |
| `QfcFormController.ApplyHighConfidenceFilterAsync` call | ~941 | line 941 (confirmed) |
| `QfcCollectionController.LoadControlsAndHandlers_01Async(IList<MailItem>,...)` | line 335 | line 335 (confirmed) |
| `QfcCollectionController.EncapsulateItemGroup` | line 512 | line 512 (confirmed) |
| `QfcItemController.AssignFolderComboBox` | ~863 | line 863 (confirmed) |
| `QfcItemController.SelectedFolder` getter | 1082-1085 | lines 1082-1086 (confirmed) |
| `QfcItemController.TopFolderScore` | ~1092 | line 1092 (confirmed) |
| `FolderScorer.TopScore()` | line 234 | line 234 (confirmed) |
| `FolderScorer.ToArray(int)` | line 240 | line 240 (confirmed) |

All anchors match current source. No preflight block required.
