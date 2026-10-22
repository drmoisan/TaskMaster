# [P7-T17] AC-U9 check-off

- Issue: #792
- Timestamp: 2026-09-17T21-18
- Command: edit `spec.md` line 314 from `- [ ] AC-U9:` to `- [x] AC-U9:` (checkbox only); then `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U9:'` and `'^- \[ \] AC-U9:'`; byte comparison of the text after the checkbox against the same line of `git show "${SpecRefSha}:$FEATURE/spec.md"` with `$SpecRefSha` bound by CMD-BASE; then `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U[1-9]:'` and `'^- \[ \] AC-U[1-9]:'` (run from `coverage/plan792-helper.ps1` with the item worktree as the working directory, console encoding UTF-8; the helper's opening branch assertion passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`)
- EXIT_CODE: 0
- Output Summary: `AC-U9: line 314 | checked=1 open=0 | ref-state '- [ ]' | text-byte-identical-to-ref=True`; `CHECKED-TOTAL: 8`; `OPEN-TOTAL: 1` (AC-U5, line 310, remains `- [ ]`); `SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4`; `git diff --numstat -- spec.md` prints `8 8`, that is exactly the eight checkbox lines changed and nothing else.
- PostedAs: none (local `spec.md` check-off only; nothing was posted to GitHub)

## Check-off line (verbatim, `spec.md:314`)

```
- [x] AC-U9: The pre-existing over-ceiling size of the two files that are not fully split is recorded explicitly in the change description as pre-existing debt, with the line counts before and after.
```

## Evidence the check-off rests on

- [P7-T3] `evidence/qa-gates/p7-t3-file-size-audit.md`, the `PRE-EXISTING-DEBT:` line, verbatim:

```
PRE-EXISTING-DEBT: EfcItemController.cs before 1122 after 1076; QfcCollectionController.cs before 2333 after 2306
```

  with the accompanying statement that both files were over the ceiling at the [P0-T8] baseline, both shrank because members moved to new partials, neither is brought under the ceiling, and no new file exceeds it. ([P6-T2] carries the same figures as the advisory measurement.)

## Whole-set verification (the [P7-T17] acceptance)

| AC | line | state | text identical to reference |
|---|---|---|---|
| AC-U1 | 306 | `[x]` | true |
| AC-U2 | 307 | `[x]` | true |
| AC-U3 | 308 | `[x]` | true |
| AC-U4 | 309 | `[x]` | true |
| AC-U5 | 310 | `[ ]` (open; Phase 8, human-executed) | true |
| AC-U6 | 311 | `[x]` | true |
| AC-U7 | 312 | `[x]` | true |
| AC-U8 | 313 | `[x]` | true |
| AC-U9 | 314 | `[x]` | true |

`'^- \[x\] AC-U[1-9]:'` returns 8; `'^- \[ \] AC-U[1-9]:'` returns 1. The reference (`SPEC-REF-SHA`) and the current `spec.md` both have 401 lines, and the `git diff -U0` shows 16 changed lines: the 8 removed `- [ ]` forms and the 8 added `- [x]` forms of AC-U1 through AC-U4 and AC-U6 through AC-U9.

## Positive control on the comparator

Run separately after the verification (same worktree, UTF-8 console): for AC-U1, `CONTROL-SAME-TEXT: True` (current text after the checkbox `-ceq` reference text after the checkbox), `CONTROL-MUTATED-TEXT: False` (the same with one character appended), `CONTROL-WHOLE-LINE-WITH-CHECKBOX: False` (the whole line compares unequal because only the checkbox differs). The comparator therefore discriminates a one-character change, and a `True` for the criterion text is a real byte-identity.
