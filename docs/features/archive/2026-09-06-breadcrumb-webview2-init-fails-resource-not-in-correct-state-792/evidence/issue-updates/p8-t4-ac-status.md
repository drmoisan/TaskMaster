# [P8-T4] Acceptance reconciliation over spec.md

- Issue: #792
- Timestamp: 2026-09-18T06-32
- Command: `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U[1-9]:'` and `'^- \[ \] AC-U[1-9]:'`; CMD-BASE (binds `$BaseSha` and `$SpecRefSha` from `evidence/baseline/p0-t7-git-base.md`); `git diff --numstat $SpecRefSha -- $FEATURE/spec.md`; `git diff --stat $SpecRefSha -- $FEATURE/spec.md`; `git diff -U0 $SpecRefSha -- $FEATURE/spec.md` with every `-`/`+` line paired in order and compared: prefix `- [ ] ` on the removed line, prefix `- [x] ` on the added line, and the text after the checkbox `-ceq` (run under `pwsh -NoProfile -WorkingDirectory <item worktree> -Command`, console encoding UTF-8, on branch `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792`, HEAD `986ce5aafb5cae63fb9a01ce1d904491ea2b3b95`)
- EXIT_CODE: 0
- Output Summary: `CHECKED: 9`; `OPEN: 0`; `SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4`; `BASE-SHA: e7cbb57229c63a228e7fe0bcbcdbfbc06db8bcd3`; `git diff --numstat` prints `9 9` for `spec.md` (`1 file changed, 9 insertions(+), 9 deletions(-)`), that is exactly nine changed lines; the `-U0` diff has 9 removed and 9 added lines, and each of the nine pairs (AC-U1 through AC-U9) reports `only-checkbox-differs=True`; `NON-AC-CHANGED-LINES: 0`; `EVERY-CHANGED-LINE-IS-A-CHECKBOX-FLIP: True`.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/spec.md
- Total AC items: 9
- Checked off (delivered): 9
- Remaining (unchecked): 0
- Items remaining: none

## Per-criterion state (spec.md lines 306-314)

| AC | line | state | text byte-identical to `SPEC-REF-SHA` | check-off artifact |
|---|---|---|---|---|
| AC-U1 | 306 | `[x]` | true | `evidence/issue-updates/p7-t10-ac-u1.md` |
| AC-U2 | 307 | `[x]` | true | `evidence/issue-updates/p7-t11-ac-u2.md` |
| AC-U3 | 308 | `[x]` | true | `evidence/issue-updates/p7-t12-ac-u3.md` |
| AC-U4 | 309 | `[x]` | true | `evidence/issue-updates/p7-t13-ac-u4.md` |
| AC-U5 | 310 | `[x]` | true | `evidence/issue-updates/p8-t3-ac-u5.md` (human-executed runbook, [P8-T2]) |
| AC-U6 | 311 | `[x]` | true | `evidence/issue-updates/p7-t14-ac-u6.md` |
| AC-U7 | 312 | `[x]` | true | `evidence/issue-updates/p7-t15-ac-u7.md` |
| AC-U8 | 313 | `[x]` | true | `evidence/issue-updates/p7-t16-ac-u8.md` |
| AC-U9 | 314 | `[x]` | true | `evidence/issue-updates/p7-t17-ac-u9.md` |

The per-line byte-identity column is the `text-byte-identical-to-ref=True` result of the [P8-T3] helper run over all nine criteria (reference and current `spec.md` both 401 lines).

## Positive controls on the counting patterns

- `'^- \[[ x]\] AC-U[1-9]:'` (either checkbox state) returns 9, equal to checked + open, so the two counting patterns partition the nine criterion lines with none unaccounted for.
- `'^- \[x\] AC-U0:'` returns 0: a pattern for a criterion that does not exist returns zero from the same cmdlet, so the 9 is not an artefact of a permissive pattern.
- The pairwise comparison uses `-ceq` (case-sensitive ordinal); the [P8-T3] control run showed it returns `False` for a one-character append and for a case change of the same text.

## Diff shape (`git diff -U0 $SpecRefSha -- spec.md`, changed lines truncated to 22 characters)

```
-- [ ] AC-U1: A failed
-- [ ] AC-U2: `_pendin
-- [ ] AC-U3: The pop-
-- [ ] AC-U4: `Populat
-- [ ] AC-U5: Manual v
-- [ ] AC-U6: All thre
-- [ ] AC-U7: The brea
-- [ ] AC-U8: No file 
-- [ ] AC-U9: The pre-
+- [x] AC-U1: A failed
+- [x] AC-U2: `_pendin
+- [x] AC-U3: The pop-
+- [x] AC-U4: `Populat
+- [x] AC-U5: Manual v
+- [x] AC-U6: All thre
+- [x] AC-U7: The brea
+- [x] AC-U8: No file 
+- [x] AC-U9: The pre-
```

No task in this plan edited any criterion text; every change to `spec.md` since `SPEC-REF-SHA` is a checkbox flip.
