---
name: agent-worktree-discovery-and-evidence-hygiene
description: In a .claude/worktrees agent checkout the standard "exclude \.claude\" test-discovery rule discards every assembly; and never commit raw Cobertura dumps as evidence
metadata:
  type: project
---

Two mechanics that cost real cycles on #507.

## 1. The `\.claude\` test-discovery exclusion inverts in an agent worktree

The standing rule is: when globbing for `*.Test.dll`, exclude any path containing `\.claude\`,
because ~20 stale `.claude/worktrees/agent-*` checkouts hold old builds that produce bogus
`AssemblyInitialize` signature failures.

That rule assumes you are running from the main checkout. An isolated agent worktree is itself
rooted at `...\.claude\worktrees\agent-<id>\`, so **every** absolute path under it contains
`\.claude\`. Applying the filter to the absolute path discovers zero assemblies and vstest exits 1
with no useful message.

Correct form: scope `Get-ChildItem` to the worktree root, then filter on the path **relative** to
that root, excluding nested `.claude` trees, `\obj\`, and `\ref\`:

```powershell
$rel = $_.FullName.Substring($root.Length)
if ($rel -match '\\bin\\Debug\\' -and $rel -notmatch '\\obj\\' -and
    $rel -notmatch '\\ref\\' -and $rel -notmatch '\.claude') { $_.FullName }
```

Correct discovery yields 9 assemblies (one per `*.Test` project). **A discovery count of 0 is a
filter bug, never a real failure** — say so explicitly in the delegation prompt, because an executor
that trusts a 0-count will report a false blocker.

## 2. Never commit raw Cobertura dumps as evidence

An executor committed `phase0-baseline-coverage.cobertura.xml` (37 MB) and
`phase2-final-coverage.cobertura.xml` (44 MB) as evidence — about 1.42 million inserted lines, for a
one-line production bugfix. Commit `d0955dc4` ("docs(#503): replace raw cobertura coverage evidence
with jacoco summaries") had already established the opposite convention.

The evidence conventions require **numeric coverage headlines** in the markdown artifacts, not the
raw dumps. The dumps are regenerable from the `dotnet-coverage merge ... -f cobertura` command
recorded in the vstest artifact.

**How to apply:** check `git diff --stat` before opening a PR. A six- or seven-figure insertion count
on a small change means an agent committed generated output. Removing it in a follow-up commit is not
enough — the blob stays in history; rewrite the branch (`reset --soft` to the merge base, restage
without the files, recommit, `push --force-with-lease`) before the PR exists.

Related: [[feedback_commit_before_ci_gate]], [[feedback_commit_all_evidence_clean_worktree]].
