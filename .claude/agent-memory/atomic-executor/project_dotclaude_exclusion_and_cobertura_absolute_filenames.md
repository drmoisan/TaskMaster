---
name: dotclaude-exclusion-and-cobertura-absolute-filenames
description: Two coupled facts for agent worktrees - the dot-claude assembly-discovery exclusion must be applied to the worktree-RELATIVE path, and dotnet-coverage writes absolute filename attributes that need sanitising to repo-relative
metadata:
  type: project
---

Two facts that bite together whenever a plan runs coverage from an agent worktree under
`.claude/worktrees/<id>/`.

**1. The dot-claude assembly-discovery exclusion must be evaluated against the worktree-RELATIVE
path.** Plans say "discover `*.Test.dll` under `\bin\Debug\`, excluding any path containing a
dot-claude directory segment", to reject stale build outputs in leftover agent worktrees. But the
agent worktree *is itself* under a `.claude` segment, so evaluating the rule against `FullName`
matches every candidate and discovery returns an **empty** assembly list. vstest then runs nothing
and the test gate passes vacuously. Strip the worktree root first
(`$_.FullName.Substring($root.Length)`) and apply the four filters to the remainder. On issue #798
this was the difference between 0 and 9 assemblies / 7,023 tests.

**2. `dotnet-coverage --output-format cobertura` writes ABSOLUTE `filename` attributes.** Plans
routinely assert "the `filename` attribute is repository-relative and uses backslash separators."
It is not repository-relative; it is the full host path including the account name. Two consequences:
suffix-based matching (`EndsWith("Extensions\DfDeedle.cs")`) still works either way, so measurement
is unaffected — but committing the document verbatim as evidence embeds absolute host paths, which
the artifact-hygiene rule forbids. Sanitise by removing the worktree-root prefix, which both clears
the hygiene violation and produces exactly the repository-relative form the plan assumed. Re-read
the root `line-rate` / `lines-covered` / `lines-valid` attributes from the sanitised copy to prove
the edit changed nothing measurable, and count residual occurrences of the account and machine name
to prove the sweep was complete.

**How to apply:** Do both before writing the Cobertura evidence copy, and record the relative-path
detail in the baseline artifact — the final-QC task repeats the same discovery and will hit the same
trap. Related: [[no-absolute-host-paths]], [[processed-cobertura-filenames-use-backslash]].
