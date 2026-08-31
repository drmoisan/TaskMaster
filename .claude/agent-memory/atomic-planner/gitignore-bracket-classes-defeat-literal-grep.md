---
name: gitignore-bracket-classes-defeat-literal-grep
description: Never conclude a path is untracked from a literal grep of .gitignore — this repo writes bracketed character classes ([Tt]est[Rr]esult*/, [Bb]in/, [Oo]bj/) that a literal search misses
metadata:
  type: feedback
---

Before asserting in a plan that some path is or is not gitignored, search `.gitignore` with a
character-class-tolerant pattern (or read the file), never with the literal directory name.

**Why:** This repository's `.gitignore` uses the Visual Studio template's bracketed-case form. A grep
for the literal `TestResults` returns nothing even though `.gitignore:39` is `[Tt]est[Rr]esult*/`,
which does match `TestResults/`. On the #469 plan this produced a false factual claim in the plan
prose ("`TestResults` and `*.trx` are NOT ignored") that survived into a self-review enumeration and
was only caught by [[trx-needs-resultsdirectory]], which had recorded the real line. A plan that
states a wrong tree fact is a defect even when the command it justifies happens to be harmless.

Known bracketed entries at the time of writing: `:26` `[Bb]in/`, `:27` `[Oo]bj/`, `:39`
`[Tt]est[Rr]esult*/`, `:40` `[Bb]uild[Ll]og.*`. Plain entries include `:144` `coverage/*`.

**How to apply:** Any plan task whose acceptance depends on a clean tree, or whose prose explains
where a tool's output lands, must cite the `.gitignore` line number and quote the entry verbatim in
its bracketed form. Grep with a pattern like `[Tt]est|[Bb]in|[Oo]bj` or just read the first ~60 lines
of `.gitignore`. Relatedly, do not route tool output somewhere merely because a literal grep
suggested the default location was tracked. See [[agent-memory-is-tracked-scope-git-gates]] for the
converse trap, where a path that looks like tooling scratch actually IS tracked.
