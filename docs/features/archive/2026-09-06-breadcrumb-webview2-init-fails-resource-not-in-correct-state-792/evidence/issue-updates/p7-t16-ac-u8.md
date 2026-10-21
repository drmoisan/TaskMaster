# [P7-T16] AC-U8 check-off

- Issue: #792
- Timestamp: 2026-09-17T21-18
- Command: edit `spec.md` line 313 from `- [ ] AC-U8:` to `- [x] AC-U8:` (checkbox only); then `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U8:'` and `'^- \[ \] AC-U8:'`; byte comparison of the text after the checkbox against the same line of `git show "${SpecRefSha}:$FEATURE/spec.md"` with `$SpecRefSha` bound by CMD-BASE (run from `coverage/plan792-helper.ps1` with the item worktree as the working directory, console encoding UTF-8; the helper's opening branch assertion passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`)
- EXIT_CODE: 0
- Output Summary: `AC-U8: line 313 | checked=1 open=0 | ref-state '- [ ]' | text-byte-identical-to-ref=True`; `SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4`.
- PostedAs: none (local `spec.md` check-off only; nothing was posted to GitHub)

## Check-off line (verbatim, `spec.md:313`)

```
- [x] AC-U8: No file created or modified by this change exceeds 500 lines, and every added or removed .cs file has a matching Compile item edit in its owning project file.
```

## Evidence the check-off rests on

- [P7-T3] `evidence/qa-gates/p7-t3-file-size-audit.md` — authoritative post-format sizes: all 29 write-set `.cs` files measured after the final-pass format step; every file created by this change is at most 500 lines (largest new file `EfcFormController.EventHandlers.cs` at 383; largest new test file `EfcFormControllerIssue792Tests.cs` at 348); `UNEXPECTED-OVER-CEILING: 0`. The two modified files still over the ceiling (`EfcItemController.cs` 1076, `QfcCollectionController.cs` 2306) were over it before this change (1122 and 2333 at [P0-T8]) and are the pre-existing debt AC-U9 records; both shrank.
- [P6-T3] `evidence/qa-gates/p6-t3-compile-item-parity.md` — Compile-item parity: `git diff --name-status $BaseSha HEAD -- '*.cs'` lists `A-ROWS: 17`, `D-ROWS: 0`; the two csproj diffs add exactly 17 bare `<Compile Include="...cs" />` elements and remove 0; `INCLUDE-SET-EQUALS-A-SET: True`; `UNTRACKED-SOURCE: none`.
- [P0-T8] `evidence/baseline/p0-t8-line-counts.md` — the gate's FAIL state: the seventeen new paths recorded absent and `OVER-CEILING-BEFORE: 3`.
