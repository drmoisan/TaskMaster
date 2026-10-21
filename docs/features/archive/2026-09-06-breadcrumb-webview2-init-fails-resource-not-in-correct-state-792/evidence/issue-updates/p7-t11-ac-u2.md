# [P7-T11] AC-U2 check-off

- Issue: #792
- Timestamp: 2026-09-17T21-18
- Command: edit `spec.md` line 307 from `- [ ] AC-U2:` to `- [x] AC-U2:` (checkbox only); then `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U2:'` and `'^- \[ \] AC-U2:'`; byte comparison of the text after the checkbox against the same line of `git show "${SpecRefSha}:$FEATURE/spec.md"` with `$SpecRefSha` bound by CMD-BASE (run from `coverage/plan792-helper.ps1` with the item worktree as the working directory, console encoding UTF-8; the helper's opening branch assertion passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`)
- EXIT_CODE: 0
- Output Summary: `AC-U2: line 307 | checked=1 open=0 | ref-state '- [ ]' | text-byte-identical-to-ref=True`; `SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4`.
- PostedAs: none (local `spec.md` check-off only; nothing was posted to GitHub)

## Check-off line (verbatim, `spec.md:307`)

```
- [x] AC-U2: `_pendingDocument` is never silently dropped: it is delivered when initialization later succeeds or an error is surfaced.
```

## Evidence the check-off rests on

- [P3-T7] `evidence/regression-testing/p3-t7-fail-before.md` — observed failing: the pending-cleared tests (`NotifyInitializationFailed_ClearsThePendingDocumentAndNavigatesTheErrorBanner`, `NotifyInitializationFailed_LeavesNoStashForALaterInitialization`) failed on their pre-predicted assertions on the unfixed tree; the control `NotifyCoreInitialized_AfterAnEarlierStash_StillNavigatesIt` passed.
- [P4-T11] `evidence/regression-testing/p4-t11-pass-after.md` — pass-after: `Total tests: 234`, `Passed: 234`.
- [P5-T7] `evidence/regression-testing/p5-t7-ac-u2-mutation.md` — non-vacuity: with the pending-document clear removed, the pre-predicted `_navigated` count assertion failed (expected 1, found 2: banner then the replayed stale stash); restoration returned `Passed: 1`.
- Final pass: [P7-T8] gate 3 records every added executable line of `BreadcrumbBridgeRouter.NotifyInitializationFailed` (including `_pendingDocument = null;` at line 354) covered.
