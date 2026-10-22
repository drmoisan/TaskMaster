# P2-T12 — Staging

Timestamp: 2026-09-01T14-49

Command:
```
git add -A -- QuickFiler.Test docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648
git status --porcelain -- QuickFiler.Test
```
(both run from the checkout root)

EXIT_CODE: 0

Output Summary:

The pathspec is deliberately narrow. A bare `git add -A` would also stage unrelated queued work
elsewhere in the tree, including files another agent is writing beneath `.claude/agent-memory/` while
this plan executes, and those files would then be billed to this issue.

`git status --porcelain -- QuickFiler.Test` output, verbatim:

```
M  QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
```

`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` is the only entry beneath `QuickFiler.Test`. Its
status code is `M ` — modified, staged, with a clean worktree relative to the index.

The `git add` command emitted 37 informational `warning: ... LF will be replaced by CRLF the next time
Git touches it` lines, one per Markdown evidence artifact this plan authored. Those are line-ending
normalisation notices for newly staged files, not errors; the command exited 0. They do not apply to
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, which P2-T1's formatting pass already converted
to CRLF.

Staging is what lets the name-listing diff in P2-T13 observe the evidence files this plan created; an
anchored `git diff --name-only` enumerates tracked changes only and would otherwise be blind to them.
