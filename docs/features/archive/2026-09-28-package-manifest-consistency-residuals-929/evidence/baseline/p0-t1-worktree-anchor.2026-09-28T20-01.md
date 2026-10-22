# P0-T1 — Worktree anchor

Timestamp: 2026-09-30T09-09
Command: git rev-parse --show-toplevel; git rev-parse --abbrev-ref HEAD; git fetch origin main; git rev-parse origin/main; git merge-base origin/main HEAD; git rev-parse HEAD; git cat-file -t <BASE-SHA>; git cat-file -t <P0-START>; git rev-list --count <BASE-SHA>..HEAD; git merge-base --is-ancestor 231e1c0b55105aeb626bf5a6e8d0266a567cacad HEAD; git status --porcelain --untracked-files=all; git ls-files -- "*.csproj.bak"; git diff --name-only HEAD -- <seven Write Set source paths> (all run from <execution-worktree-root>)
EXIT_CODE: 0
Output Summary:
- show-toplevel: <execution-worktree-root>
- Branch: bug/package-manifest-consistency-residuals-929
- origin/main (after fetch): b305903e275b8abf58e8e65831c189f517568fe4
- BASE-SHA (merge-base origin/main HEAD): 231e1c0b55105aeb626bf5a6e8d0266a567cacad (git cat-file -t: commit)
- P0-START (HEAD): 481b33c594d8412cb64e604ff53295db215ac2f1 (git cat-file -t: commit)
- git rev-list --count <BASE-SHA>..HEAD: 8 (recorded as measured)
- MERGED-MAIN-ANCESTOR: 0 (git merge-base --is-ancestor 231e1c0b55105aeb626bf5a6e8d0266a567cacad HEAD exited 0; the worktree carries the 2026-09-30 merge of origin/main)
- Seven-path diff against HEAD: empty (no Write Set source file is already modified)
- Note: origin/main has advanced to b305903e2 since the branch merged 231e1c0b5; the merge-base remains 231e1c0b5, so <BASE-SHA> is the merged tip as the plan states.

BASE-UNTRACKED:
```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
?? .claude/agent-memory/atomic-executor/index_csharp_nullable_and_component_gotchas.md
?? .claude/agent-memory/atomic-executor/index_pwsh_git_and_gate_mechanics_misc.md
?? .claude/agent-memory/atomic-executor/index_test_isolation_and_coverage.md
?? .claude/agent-memory/atomic-executor/project_hygiene_pattern_array_comma_precedence_and_regex_token_hits.md
?? .claude/agent-memory/atomic-planner/project_929_manifest_residuals_plan_seams.md
```
No entry matches *.cs, *.csproj, packages.config or app.config.

BAK-TRACKED:
```
QuickFiler.Test/QuickFiler.Test.csproj.bak
QuickFiler/QuickFiler.csproj.bak
Tags/Tags.csproj.bak
TaskTree/TaskTree.csproj.bak
TaskVisualization.Test/TaskVisualization.Test.csproj.bak
TaskVisualization/TaskVisualization.csproj.bak
ToDoModel.Test/ToDoModel.Test.csproj.bak
ToDoModel/ToDoModel.csproj.bak
```
Eight tracked .csproj.bak copies (the plan names two carrying altcover tokens; all eight are recorded). They are not project files and are not edited (decision D12).
