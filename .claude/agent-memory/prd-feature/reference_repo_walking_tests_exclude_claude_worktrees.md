---
name: repo-walking-tests-exclude-claude-worktrees
description: Any AC or test that enumerates project files or source files from the repository root must exclude the .claude directory, because agent worktrees are nested under .claude/worktrees and hold full copies of every csproj — a count-based assertion (e.g. "exactly six HintPaths") silently inflates otherwise
metadata:
  type: reference
---

When a spec proposes a static "count N members solution-wide" test that walks the tree from the
directory containing TaskMaster.sln (the precedent in RibbonControllerTests / SortEmail_Tests), the
enumeration must skip directories named .git, .claude, packages, bin, obj and node_modules. The
.claude exclusion is the non-obvious one: agent worktrees live at .claude/worktrees/agent-* inside the
primary checkout and contain a complete copy of every project file, so a recursive csproj glob from the
primary root finds 18 x (1 + number of live worktrees) files and a count assertion of "exactly six"
fails or, worse, an agreement-only assertion passes against stale copies.

**Why:** Seen while writing the #895 spec (2026-09-16): the Shape-A HintPath-alignment AC asserts
count == 6 across all csproj files. The same nesting already breaks vstest discovery (user-level memory
project_local_vstest_exclude_claude_worktrees), so it is a standing repository property, not a one-off.

**How to apply:** Put the exclusion list in the AC text itself, not only in the design prose, so the
executor cannot satisfy the criterion with a bare Directory.EnumerateFiles. Pair every count assertion
with a flavour/content assertion and vice versa (count alone passes vacuously on deletion; content alone
passes vacuously on an empty set). Related: [[ac-gates-verify-satisfiability]],
[[backticked-paths-are-the-change-footprint]].
