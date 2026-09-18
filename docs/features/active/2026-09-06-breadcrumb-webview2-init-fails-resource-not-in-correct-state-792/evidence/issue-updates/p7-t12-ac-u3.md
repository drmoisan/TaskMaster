# [P7-T12] AC-U3 check-off

- Issue: #792
- Timestamp: 2026-09-17T21-18
- Command: edit `spec.md` line 308 from `- [ ] AC-U3:` to `- [x] AC-U3:` (checkbox only); then `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U3:'` and `'^- \[ \] AC-U3:'`; byte comparison of the text after the checkbox against the same line of `git show "${SpecRefSha}:$FEATURE/spec.md"` with `$SpecRefSha` bound by CMD-BASE (run from `coverage/plan792-helper.ps1` with the item worktree as the working directory, console encoding UTF-8; the helper's opening branch assertion passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`)
- EXIT_CODE: 0
- Output Summary: `AC-U3: line 308 | checked=1 open=0 | ref-state '- [ ]' | text-byte-identical-to-ref=True`; `SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4`.
- PostedAs: none (local `spec.md` check-off only; nothing was posted to GitHub)

## Check-off line (verbatim, `spec.md:308`)

```
- [x] AC-U3: The pop-out path carries the already-initialized folder predictor and loaded `MailItemHelper` from the QfcItem, following the #678 carry pattern, and constructs the `EfcViewer` on the UI thread.
```

## Evidence the check-off rests on

- [P1-T4] `evidence/regression-testing/p1-t4-fail-before.md` — the UI-thread half observed failing: `ProductionBlockingPriorityScheduler_DefaultIsTheNamedUiDispatcherInvoke` (scheduler delegate identity) was among the 4 of 5 tests failing on the unfixed tree on its pre-predicted assertion.
- [P3-T7] `evidence/regression-testing/p3-t7-fail-before.md` — the carry half observed failing: the adoption tests (`TryAdoptCarriedFolderHandler_*`, `InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry`), the carry-read tests (`ReadPopOutCarry_*`) and `EfcHomeController_DepositsTheCarryOnTheDataModelBeforeConstructingTheFormController` failed on their pre-predicted assertions.
- [P4-T11] `evidence/regression-testing/p4-t11-pass-after.md` — pass-after: `Total tests: 234`, `Passed: 234`.
- [P5-T6] `evidence/regression-testing/p5-t6-ac-u3-deposit-mutation.md` — non-vacuity: deposit removed, the pre-predicted captured-handler reference assertion failed (`but found <null>`); restoration returned `Passed: 1`.
- [P5-T8] `evidence/regression-testing/p5-t8-ac-u3-adoption-mutation.md` — non-vacuity: adoption disabled, the pure adoption test failed on the pre-predicted boolean and the `InitFolderHandlerAsync` test on the pre-predicted unguarded construction path; restoration returned `Passed: 2`.
- [P6-T4] `evidence/qa-gates/p6-t4-popout-ordering.md` — `ORDERING: PASS`: in both `PopOutControlGroup` and `PopOutControlGroupAsync` the `ReadPopOutCarry(group)` read precedes `RemoveSpecificControlGroup(Async)(selection)`, so the carry is read before `Cleanup` nulls the handler and helper.
- Final pass: [P7-T8] gate 3 records the two deposit statements in `EfcHomeController.cs` (lines 87, 88), the `QfcItemController.FolderHandler` accessor (line 271) and the `EfcViewerQueue` initializer and reset (lines 25, 68) covered; the single body line of `InvokeOnUiDispatcher` (line 76) is `n/a` by D8, as the spec anticipates ("verifiable only as a scheduler-delegate assertion").
