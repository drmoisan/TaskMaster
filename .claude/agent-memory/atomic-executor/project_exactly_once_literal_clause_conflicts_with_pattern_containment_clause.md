---
name: exactly-once-literal-clause-conflicts-with-pattern-containment
description: A plan clause demanding a marker literal "exactly once" is jointly unsatisfiable with a sibling clause demanding a regex literal that contains that marker be present verbatim — derive the emitted markers from the pattern
metadata:
  type: project
---

When one task requires a strip pattern such as
`(?s)<!-- x:begin -->.*?<!-- x:end -->` to be present verbatim (so a test can bind to it by
containment), and the same task requires each marker literal to appear **exactly once** in the
file, the obvious implementation fails: the block-emitting code writes both markers a second
time, so each counts 2.

**Why:** the two clauses are not independently satisfiable in the obvious shape. Recognising
that early avoids either gaming the count or halting a phase over a plan defect. The resolution
also happens to be better engineering than the obvious shape, which is what makes it the right
call rather than a workaround.

**How to apply:** derive the emitted markers from the pattern instead of retyping them:

```powershell
$blockPattern = '(?s)<!-- x:begin -->.*?<!-- x:end -->'
$marker = $blockPattern.Substring(4) -split '\.\*\?'   # drop (?s), split on .*?
$stripped = [regex]::Replace($existing, $blockPattern, '').TrimEnd()
$block = $marker[0] + "`n" + $report + "`n" + $marker[1]
```

Each marker is now written once and used twice, the counts pass, and the block the step
**emits** and the block it **strips** are structurally incapable of drifting apart. Record the
first failing count and the reason in the evidence artifact — the near-miss is the evidence that
the clause is a live gate.

Related, same run: two other plan clauses were measured-but-unmet and were **reported rather
than accommodated** — a numstat "at least 6 deletions" floor that the delivered edit shape made
5, and a union-count clause that grew because the task's own artifacts land in the counted
scope. See [[project_scope_gate_cannot_list_artifacts_written_after_it]].

Confirmed 2026-09-20 on issue #911 remediation cycle 1, tasks P3-T4 and P3-T10.
