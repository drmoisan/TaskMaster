# [P8-T3] AC-U5 check-off

- Issue: #792
- Timestamp: 2026-09-18T06-31
- Command: edit `spec.md` line 310 from `- [ ] AC-U5:` to `- [x] AC-U5:` (checkbox only); then `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U5:'` and `'^- \[ \] AC-U5:'`; byte comparison of the text after the checkbox against the same line of `git show "${SpecRefSha}:$FEATURE/spec.md"` with `$SpecRefSha` bound by CMD-BASE (run from the gitignored `coverage/plan792-helper.ps1` under `pwsh -NoProfile -WorkingDirectory <item worktree> -File`, console encoding UTF-8; the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` passed; HEAD `986ce5aafb5cae63fb9a01ce1d904491ea2b3b95`); then a separate `pwsh -NoProfile -WorkingDirectory <item worktree> -Command` control run for the AC-U5 line specifically
- EXIT_CODE: 0
- Output Summary: `AC-U5: line 310 | checked=1 open=0 | ref-state '- [ ]' | text-byte-identical-to-ref=True`; `SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4`; `REF-SPEC-LINES: 401`, `CUR-SPEC-LINES: 401`; `SPEC-NUMSTAT-VS-HEAD: 1 1` (the AC-U5 line is the only change to `spec.md` since the [P7-T18] commit); `SPEC-CHANGED-CHECKBOX-LINES: 2` (`-- [ ] AC-U5:` removed, `+- [x] AC-U5:` added).
- PostedAs: none (local `spec.md` check-off only; nothing was posted to GitHub)

## Check-off line (verbatim, `spec.md:310`)

```
- [x] AC-U5: Manual verification on both entry points: pop-out from QuickFiler and ribbon Sort Email each show suggestion rows and respond to typed search.
```

Reference line at `SPEC-REF-SHA` (verbatim, line 310 of `git show 11b107a55fc32078f97e0cd48f893c175be5b6f4:$FEATURE/spec.md`):

```
- [ ] AC-U5: Manual verification on both entry points: pop-out from QuickFiler and ribbon Sort Email each show suggestion rows and respond to typed search.
```

## Evidence the check-off rests on

- [P8-T2] `evidence/other/p8-t2-ac-u5-manual-verification.md` — the runbook's four observations all `PASS` and the artifact states `AC-U5: PASS`: pop-out from QuickFiler showed suggestion rows (maintainer-confirmed), typed search in the popped-out view changed the rows (maintainer-confirmed), ribbon Sort Email showed suggestion rows under "Matched Folders:" (maintainer-confirmed), and the session log (21:43:22 to 23:48:15) contains zero occurrences of `Breadcrumb CoreWebView2 initialization failed`, `0x8007139F` and `resource not in correct state` with the positive controls recorded there (instrument-verified).
- [P8-T1] `evidence/other/p8-t1-addin-rebuild.md` — the session under test ran the add-in rebuilt from HEAD `986ce5aaf` (assembly mtime 21:27:02, manifests generated 21:43:18, add-in startup 21:43:22).

## Positive control on the comparator (separate run, AC-U5 line)

- `CONTROL-SAME-TEXT: True` — current text after the checkbox `-ceq` reference text after the checkbox.
- `CONTROL-MUTATED-TEXT: False` — the same with one character appended.
- `CONTROL-CASE-FLIP: False` — the same with the current text upper-cased (`-ceq` is case-sensitive).
- `CONTROL-WHOLE-LINE-WITH-CHECKBOX: False` — the whole line compares unequal because only the checkbox differs (`- [x]` versus `- [ ]`).

The comparator discriminates a one-character change and a case change, so `text-byte-identical-to-ref=True` is a real byte-identity.

Note on the helper: the helper script's own final control line (`CONTROL-MUTATED-TEXT-DETECTED`) raised a non-terminating `InvalidOperation` (`[System.Char] does not contain a method named 'Substring'`) and printed an empty value; this is a defect in that gitignored helper line, not in the verification above, which is why the control was run separately (as [P7-T17] also did). The helper's exit code was 0 and every verification line before the control printed as recorded.
