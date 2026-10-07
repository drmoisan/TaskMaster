---
name: scope-amendment-narrow-exclusions-and-log
description: When a maintainer folds related defects into a running item, amend each forbidding statement narrowly ("other than the exact named files, each constrained to ..."), keep a Scope Amendment Log quoting the old wording, and state the file-size-split rule (both files under the limit, member set identical, verbatim move, csproj Compile Include) in the spec
metadata:
  type: feedback
---

Rule: a mid-item scope amendment (maintainer folds related defects in rather than filing them) is applied to spec.md by narrowing, never by deleting, every statement that previously forbade the new work, and every narrowing is recorded with its before/after wording in a dated `## Scope Amendment Log` section placed before `## Acceptance Criteria`.

**Why:** On #973 (2026-10-03) the maintainer folded in six unused `using Microsoft.Graph.*` deletions, a CLAUDE.md bullet rewording, and (orchestrator ruling under the related-defect directive) a partial-class split of a 539-line file. The spec had five places that said "no .cs file" (Scope bullet, AC16, AC14 parenthetical, Constraints, Rollout follow-up list) and a dated Planner Amendment that reasoned from "edits no .cs file". The caller required that each exclusion stay exactly as strong for every previously excluded file; a reviewer must be able to see that from the spec alone.

**How to apply:**
- Phrase each narrowed exclusion as "no X other than the exact files named in AC-N, each of whose diff consists only of <permitted edits>", so the admission is itself a constraint.
- Add new ACs after the last existing one; never renumber or reword AC1..AC-last except the minimal parenthetical that is now false (quote it in the log).
- Dated planner/log entries whose reasoning is superseded get a bracketed dated note appended, not a rewrite.
- A file-size split folded in under the directive needs the rule in the spec even when the concern name is left to the planner: both files under 500 total lines, `partial` on both declarations, base list on exactly one, verbatim move of one contiguous region (ordered line comparison with zero differences), member-declaration census equal before/after, `#nullable enable` on line 1 of the new file, CRLF, explicit `<Compile Include>` in the non-SDK csproj, CSharpier checked-file count becomes baseline plus one, coverage denominator unchanged (no sequence points added or removed).
- Counts for the folded scope backed by a second research record go in AC prose and body freely, but report that record on a `supplemental-research-path:` line so the hook keeps validating the original `research-path` file (see [[ac-gates-verify-satisfiability]] item 10).
- Record the amendment in issue.md as a `## Scope Amendment (<date>, maintainer direction)` section appended after the existing content, leaving the `- Work Mode:` marker untouched.
