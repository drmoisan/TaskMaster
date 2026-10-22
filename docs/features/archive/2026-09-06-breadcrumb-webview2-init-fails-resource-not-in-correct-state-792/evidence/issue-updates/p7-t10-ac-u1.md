# [P7-T10] AC-U1 check-off

- Issue: #792
- Timestamp: 2026-09-17T21-18
- Command: edit `spec.md` line 306 from `- [ ] AC-U1:` to `- [x] AC-U1:` (checkbox only); then `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U1:'` and `'^- \[ \] AC-U1:'`; byte comparison of the text after the checkbox against the same line of `git show "${SpecRefSha}:$FEATURE/spec.md"` with `$SpecRefSha` bound by CMD-BASE (run from `coverage/plan792-helper.ps1` with the item worktree as the working directory, console encoding UTF-8; the helper's opening branch assertion passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`)
- EXIT_CODE: 0
- Output Summary: `AC-U1: line 306 | checked=1 open=0 | ref-state '- [ ]' | text-byte-identical-to-ref=True`; `SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4`; the reference and current `spec.md` both have 401 lines.
- PostedAs: none (local `spec.md` check-off only; nothing was posted to GitHub)

## Check-off line (verbatim, `spec.md:306`)

```
- [x] AC-U1: A failed `CoreWebView2` initialization is retried, and on final failure the Efc view shows a visible error state in the folder area instead of a blank list.
```

## Evidence the check-off rests on

- [P3-T7] `evidence/regression-testing/p3-t7-fail-before.md` — observed failing on the unfixed tree: 18 of 31 tests failed on their pre-predicted assertions, including the retry tests (`InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce`, `InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing`), the label test (`InitializeBreadcrumbHostAsync_OnFinalFailure_ShowsTheErrorTextInTheFolderAreaLabel`) and the router notification test (`InitializeBreadcrumbHostAsync_OnFinalFailure_NotifiesTheRouter`).
- [P4-T11] `evidence/regression-testing/p4-t11-pass-after.md` — pass-after: `Total tests: 234`, `Passed: 234`; all 32 Issue792/contract tests pass.
- [P5-T5] `evidence/regression-testing/p5-t5-ac-u1-mutation.md` — non-vacuity: mutation A (attempt limit 3 to 1) and mutation B (per-attempt report) each failed on the pre-predicted assertions (`Failed: 2` each), and each restoration returned `Passed: 2`.
- D4 label carrier — `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs:114-129` `ShowFolderAreaError` writes `FolderAreaInitializationFailedText` to the existing folder-area label `EfcViewer.label2`; the final-pass coverage ([P7-T8] gate 2) records every new executable line of that file covered except the pre-declared `BeginInvoke` branch at line 128.

## Positive control on the verification

The comparator is `-ceq` on the substring after the checkbox: the current line with a single character appended compares false against the reference (see the control run recorded in [P7-T17]), and the whole line including the checkbox compares false (`- [x]` versus `- [ ]`), so `text-byte-identical-to-ref=True` is a discriminating result.
