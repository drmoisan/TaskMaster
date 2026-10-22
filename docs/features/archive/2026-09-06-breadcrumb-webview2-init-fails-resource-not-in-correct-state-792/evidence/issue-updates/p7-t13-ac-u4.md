# [P7-T13] AC-U4 check-off

- Issue: #792
- Timestamp: 2026-09-17T21-18
- Command: edit `spec.md` line 309 from `- [ ] AC-U4:` to `- [x] AC-U4:` (checkbox only); then `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U4:'` and `'^- \[ \] AC-U4:'`; byte comparison of the text after the checkbox against the same line of `git show "${SpecRefSha}:$FEATURE/spec.md"` with `$SpecRefSha` bound by CMD-BASE (run from `coverage/plan792-helper.ps1` with the item worktree as the working directory, console encoding UTF-8; the helper's opening branch assertion passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`)
- EXIT_CODE: 0
- Output Summary: `AC-U4: line 309 | checked=1 open=0 | ref-state '- [ ]' | text-byte-identical-to-ref=True`; `SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4`.
- PostedAs: none (local `spec.md` check-off only; nothing was posted to GitHub)

## Check-off line (verbatim, `spec.md:309`)

```
- [x] AC-U4: `PopulateFolderCombobox` and `InitializeBreadcrumbHostAsync` report failures through `TryReportBoundaryFault` to the user, not log-only.
```

## Evidence the check-off rests on

- [P0-T15] `evidence/regression-testing/fail-before-exception.p0-t15.md` — the `PopulateFolderCombobox` half was already satisfied at baseline (`EfcFormController.cs:1270` called `TryReportBoundaryFault`), so a failing run was structurally impossible; the dossier records `Passed PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault`, `Total tests: 1`, `Passed: 1` on the unfixed tree as the absence-of-defect proof for that half.
- [P1-T4] `evidence/regression-testing/p1-t4-fail-before.md` — the `InitializeBreadcrumbHostAsync` half observed failing: `InitializeBreadcrumbHostAsync_WhenHostIsNull_ReportsThroughTheBoundarySinkToTheUser` and the strengthened `PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink` were among the 4 of 5 failing on their pre-predicted assertions.
- [P4-T11] `evidence/regression-testing/p4-t11-pass-after.md` — pass-after: `Total tests: 234`, `Passed: 234`.
- [P5-T1] `evidence/regression-testing/p5-t1-ac-u4-mutation.md` — non-vacuity: with the notifier call dropped inside `DefaultBoundaryErrorSink`, the new default-sink test failed on the pre-predicted `ContainSingle` assertion while the pre-existing sink-substituting test still passed (showing the strengthened test is the discriminating one); restoration returned `Passed: 2`.
