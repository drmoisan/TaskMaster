Timestamp: 2026-07-20T14-15

## AC check-off (Phase 1) for issue #392

In `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/issue.md`,
`## Acceptance Criteria` section, changed from `- [ ]` to `- [x]`:

- AC-1: backed by
  `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md` (fail-before) and
  `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md` (pass-after). No temporary files
  or external dependencies used (real `System.Windows.Forms.ComboBox` and `Mock<IItemViewer>` only).
- AC-2: backed by `evidence/other/root-cause-392.2026-07-20T13-50.md` (diagnosis),
  `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md`,
  `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md`. Production fix at
  `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` line ~200-204 (P1-T5).
- AC-3: backed by
  `evidence/regression-testing/targeted-no-regression-392.2026-07-20T14-13.md` (6/6 pre-existing
  tests still pass unchanged).
- AC-4: backed by `evidence/other/root-cause-392.2026-07-20T13-50.md`,
  `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md`,
  `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md`. Production fix at
  `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` line ~227-230 (P1-T6).

AC-5 remains unchecked (`- [ ]`) pending Phase 2 final QC.
