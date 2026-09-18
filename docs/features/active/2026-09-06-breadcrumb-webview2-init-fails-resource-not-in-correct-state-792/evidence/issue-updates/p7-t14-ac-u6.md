# [P7-T14] AC-U6 check-off

- Issue: #792
- Timestamp: 2026-09-17T21-18
- Command: edit `spec.md` line 311 from `- [ ] AC-U6:` to `- [x] AC-U6:` (checkbox only); then `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U6:'` and `'^- \[ \] AC-U6:'`; byte comparison of the text after the checkbox against the same line of `git show "${SpecRefSha}:$FEATURE/spec.md"` with `$SpecRefSha` bound by CMD-BASE (run from `coverage/plan792-helper.ps1` with the item worktree as the working directory, console encoding UTF-8; the helper's opening branch assertion passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`)
- EXIT_CODE: 0
- Output Summary: `AC-U6: line 311 | checked=1 open=0 | ref-state '- [ ]' | text-byte-identical-to-ref=True`; `SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4`.
- PostedAs: none (local `spec.md` check-off only; nothing was posted to GitHub)

## Check-off line (verbatim, `spec.md:311`)

```
- [x] AC-U6: All three production WebView2 environment creations resolve their user-data folder and their additional browser arguments from one shared owner, and a test asserts the three agree.
```

## Evidence the check-off rests on

- [P0-T14] `evidence/regression-testing/p0-t14-ac-u6-structural-fail-before.md` — the structural gate observed FAIL on the unfixed tree: `PRIMARY-CONSTRUCTION-COUNT: 3`, `CREATEASYNC-OUTSIDE-ADAPTER: 1`, `SEAM-CALLER-COUNT: 2`, `CONTRACT-READER-COUNT: 0`, `AC-U6-STRUCTURAL: FAIL`.
- [P6-T1] `evidence/qa-gates/p6-t1-ac-u6-structural-pass.md` — the same gate, unchanged, on the fixed tree: 1 (the contract file) / 0 / 3 / 3, `AC-U6-STRUCTURAL: PASS`.
- [P1-T4] `evidence/regression-testing/p1-t4-fail-before.md` — the site-1 seam test `InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam` observed failing on the unfixed tree.
- [P4-T4] `evidence/regression-testing/p4-t4-site3-mutation.md` — site 3 (`EfcItemController.InitializeWebViewAsync`, uninstrumented) proven through the seam: `EfcItemController_InitializeWebViewAsync_PassesTheContractValuesThroughTheSeam` passes unmutated and fails on the pre-predicted assertion when mutated; file restored byte-identical.
- [P4-T11] `evidence/regression-testing/p4-t11-pass-after.md` — pass-after: `Total tests: 234`, `Passed: 234`, including the `WebView2EnvironmentContractTests` class that asserts the shared values.
- [P5-T2] `evidence/regression-testing/p5-t2-ac-u6-site1-mutation.md` — non-vacuity: site 1 regressed to an inline construction fails the pre-predicted `AdditionalBrowserArguments` assertion and the structural gate (`PRIMARY-CONSTRUCTION-COUNT: 2`, FAIL); in its second half, site 2 (`QfcItemController.ViewerSetup.cs`, method-level exempt) regressed is caught structurally (`PRIMARY-CONSTRUCTION-COUNT: 2`, `CONTRACT-READER-COUNT: 2`, FAIL); both restored to 1/0/3/3 PASS.
- [P5-T3] `evidence/regression-testing/p5-t3-ac-u6-constant-mutation.md` — non-vacuity: mutating the shared constant fails the three pre-predicted string-equality tests while the three contract-relative tests still pass; restoration returned `Passed: 6`.
- Final pass: [P7-T8] gate 1 records `WebView2EnvironmentContract.cs` at 9/9 lines covered.
