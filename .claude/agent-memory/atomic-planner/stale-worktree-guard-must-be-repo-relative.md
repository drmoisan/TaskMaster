---
name: stale-worktree-guard-must-be-repo-relative
description: a stale-.claude/worktrees test-DLL guard must match `.claude` RELATIVE to the executing repo root; an absolute-path match flags the executing agent worktree itself and can never pass after a build
metadata:
  type: project
---

Any plan task that guards against stale `*.Test.dll` files left in `.claude/worktrees` MUST evaluate the
`.claude` segment relative to the executing repository root, not against the absolute path:

```powershell
$root = (Resolve-Path .).Path.TrimEnd('\')
Get-ChildItem -Path . -Recurse -Filter '*.Test.dll' |
    Where-Object { $_.FullName.Substring($root.Length) -match '^\\\.claude\\' }
```

The absolute form `$_.FullName -match '\\\.claude\\'` is always wrong here.

**Why:** agents in this repo execute inside a worktree whose own root is
`...\TaskMaster\.claude\worktrees\agent-<id>`. Every one of that worktree's own freshly built
`bin\Debug\*.Test.dll` therefore contains a literal `\.claude\` segment, so the absolute predicate throws
on the first measurement run after any build, with no authorized path past it. Verified empirically by
atomic-executor during #454 preflight; the same predicate gated three separate tasks (baseline coverage,
final coverage, final QC test step), so the whole measurement spine was unreachable.

**How to apply:** when writing or reviewing a plan that carries a "stale worktree assemblies" preflight
(this repo's `CMD-PREFLIGHT` idiom, sourced from the local-vstest recursive-search hazard), check the
predicate's anchoring before shipping. Also fix the prose acceptance clauses — "no `*.Test.dll` under any
`.claude` path" must become "under any `.claude` path BELOW the executing repository root", or a literal
reader re-introduces the defect. Related: [[reference-vstest-scoped-run-command]].
