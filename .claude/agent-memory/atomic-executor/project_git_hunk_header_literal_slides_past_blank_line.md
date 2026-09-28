---
name: git-hunk-header-literal-slides-past-blank-line
description: git compacts an insertion group downward past identical context, so a gate asserting "the hunk header begins @@ -N,0 +M," fails for a correct edit when the inserted block starts with a blank line and line N+1 is already blank
metadata:
  type: project
---

A plan gate of the form "`git diff --unified=0 <base> -- <file>` prints exactly one `@@ ` line and
that line begins `@@ -163,0 +164,`" is not satisfied by an edit that inserts "one blank line, then
the method" after line 163 when line 164 of the original is already blank. Git reports
`@@ -164,0 +165,<count>` instead, and attributes the added block as "method text first, blank line
last".

**Mechanism.** xdiff's change-compaction slides an insertion group as far as the identical
surrounding context allows, and the indent heuristic then picks the boundary. A leading blank in the
inserted text is interchangeable with the pre-existing blank that follows the insertion point, so the
two spellings of the same edit are equally minimal and git chooses the later one. Measured directly
(git 2.x, default config, `git diff --no-index --unified=0` on a 16-line reproduction of the real
boundary): intended `@@ -6,0 +7,10 @@`, reported `@@ -7,0 +8,10 @@`.

**How to apply.** Never assert a hunk-header position literal. Assert the properties the criterion
actually needs and that survive both spellings:

- exactly one line beginning `@@ ` (a single contiguous insertion),
- zero lines beginning with `-` other than the `---` header (nothing pre-existing was deleted),
- `git diff --numstat <base> -- <file>` whose deleted column is exactly `0` and whose added column is
  inside a stated range.

The numstat deleted-column `0` is the load-bearing assertion for an "existing test untouched"
acceptance criterion; the hunk position adds nothing to it and only introduces a false failure. Use a
range rather than an exact added-line count whenever a formatter runs between the edit and the gate.

Related: [[project_csharpier_requires_blank_line_before_comment_breaking_numstat_bounds]],
[[project_preflight_recurring_csharp_plan_defect_classes]].
