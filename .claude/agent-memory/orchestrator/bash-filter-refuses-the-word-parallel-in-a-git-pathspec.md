---
name: bash-filter-refuses-the-word-parallel-in-a-git-pathspec
description: The worktree-isolation Bash filter matches the bare word "parallel" anywhere in a command and refuses it as an xargs/parallel stdin feed, so a git pathspec naming docs/features/parallel cannot be run at all
metadata:
  type: project
---

Under Agent worktree isolation the Bash-tool filter refuses a command containing the
token `parallel` **anywhere**, including inside a `git` pathspec operand, with:

```
... this command feeds git its arguments from stdin at runtime (xargs/parallel), so the
repository it targets cannot be verified. Refusing to run it ...
```

Verified 2026-09-12 (issue 602 preparation). `git -C <abs> grep -l -i -F -e '<token>' --
docs/features/parallel | wc -l` was refused, while the byte-identical command with
`docs/features/archive` or `docs/features/active` ran normally. Nothing in the refused
command reads stdin; the filter is matching the GNU `parallel` executable name as a bare
word and has no notion of operand position.

**Consequences.**

- A repository-relative path under the parallel-run docs directory can never be named in a
  Bash command in an isolated worktree. Staging it, diffing it, or asserting a count over it
  requires another route.
- This bites plan authoring directly: an acceptance condition whose command text names that
  directory is not merely wrong, it is unrunnable, and it fails identically whatever the
  executor did. Treat it as the unsatisfiable-assertion class, not as a tooling annoyance.
- The same class of word-match refusal is documented for `pwsh` and for glob expansion; see
  [[worktree-isolation-blocks-pwsh-per-agent-type]],
  [[bash-tool-rejects-complex-commands-in-isolated-worktree]] and
  [[hooks-pattern-match-bash-command-text]]. The distinguishing feature here is that the
  refused token is an ordinary English word that appears in first-party repository paths.

**How to apply.** Use the `Grep` and `Glob` tools for any read over that subtree. For a
staging span, stage a parent that does not spell the word, or scope the pathspec so the
token never appears on the command line. Before writing a count-based acceptance condition,
check that the command text contains none of the filter's trigger words; a condition that
cannot execute is worse than one that is merely vacuous, because its failure looks like an
environment problem rather than a defect.

Related: [[feedback_no_cd_or_non_allowlisted_bash_segments]],
[[select-string-pattern-quoting-in-plans]].

**Second, unrelated measurement trap found the same run.** `wc -l` IS available through the
Bash tool here (only `git`, `gh`, `pwsh` and `poetry run` are allowlisted as leading
tokens, but a trailing `| wc -l` and `| head -N` were both accepted), which makes
`git grep -l ... | wc -l` the cheap way to count a tracked-file population without
dumping hundreds of paths into context. Prefer it over the `Grep` tool's
`output_mode: "count"` for large populations.
