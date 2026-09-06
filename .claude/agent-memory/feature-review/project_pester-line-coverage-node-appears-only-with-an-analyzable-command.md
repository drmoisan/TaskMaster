---
name: pester-line-coverage-node-appears-only-with-an-analyzable-command
description: A PowerShell line can be absent from Pester's JaCoCo XML entirely and then appear covered after an edit, because Pester counts analyzable *commands*, not source lines; verify with the analyzed-command total delta
metadata:
  type: project
---

Pester's code coverage instruments **analyzable commands**, not source lines. A source line that
holds only an operand of a multi-line boolean chain — for example
`$_.FullName -notmatch '\\\.claude\\'` as one term of an `-and` chain inside a `Where-Object` — carries
no command of its own and therefore gets **no `<line nr="N">` node at all** in the JaCoCo XML. It is
neither missed nor covered; it is absent.

**Why this matters for a changed-line coverage gate:** a plan that asks "was the changed line covered
before and after?" can get `LINE301 NODE COUNT=0` at baseline and `count=1, ci=1` after the fix, which
matches neither the "covered before, covered after" branch nor the "not reported, still not reported"
branch. At #752 the plan enumerated exactly those two branches and classified everything else as
stop-and-report; the executor measured the third combination, recorded it, and left the task's
checkbox unchecked rather than forcing it. That was the right call — the third combination is a
coverage *gain*, which neither failure condition describes.

**How to apply / how to verify the mechanism rather than trust it:**

1. Locate the `<sourcefile name="X.ps1">` node boundaries in *both* XMLs (`grep -n 'sourcefile name='`)
   before believing any `nr="N"` hit — a bare grep for `nr="301"` matches other sourcefiles' line 301.
   At #752 the baseline's only `nr="301"` belonged to a different file entirely.
2. Corroborate with Pester's own analyzed-command total, printed in the detailed console summary and
   usually quoted in the evidence (`802 analyzed Commands in 11 Files` -> `803 ...`). A delta of exactly
   the number of new invocation expressions confirms the mechanism instead of asserting it.
3. Report it as a coverage gain, not a regression, and say so explicitly — an absent baseline node is
   not the same as an uncovered baseline line.

Related: [[cobertura-class-line-double-count-trap]], [[verify-the-asserted-evidence-mechanism]].
