---
name: backticked-paths-are-the-change-footprint
description: A downstream tool harvests backtick-delimited repository paths from spec.md/plan to derive the change footprint — backtick every in-scope file, leave out-of-scope files unbackticked
metadata:
  type: feedback
---

In feature documents, every repository file the fix will create or modify must appear at least once
as a Markdown inline code span with its full repository-relative path. Conversely, do not backtick a
path the fix will not touch — write out-of-scope paths as bare prose under a clearly labelled
non-goals section. Never use placeholder forms such as `<FEATURE>/...` or `${VAR}/...`.

**Why:** A downstream tool derives the change footprint by harvesting backticked path tokens from
`spec.md` and the plan. A file named only in bare prose is invisible to it, so an in-scope file that
is not backticked silently drops out of the footprint. The same mechanism makes a backticked
out-of-scope path a false positive that widens the apparent blast radius.

**How to apply:** After drafting, sweep the document twice — once to confirm every in-scope file has
at least one backticked occurrence, once to confirm no out-of-scope file is backticked. This bites
hardest in the Root Cause Analysis section, where it is natural to cite comparison files that will
not be modified: keep those citations unbackticked. Add a one-line note in the non-goals section
explaining that the paths there are deliberately unbackticked, so a later editor does not "fix" the
formatting and corrupt the footprint. Related: [[full-bug-spec-only]],
[[ac-gates-verify-satisfiability]].
