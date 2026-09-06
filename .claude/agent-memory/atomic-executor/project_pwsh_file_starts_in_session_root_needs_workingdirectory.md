---
name: pwsh-file-starts-in-session-root-needs-workingdirectory
description: pwsh -File from the Bash tool starts in the session-root worktree, so a script's own `git rev-parse --show-toplevel` silently resolves to the WRONG worktree; pass -WorkingDirectory
metadata:
  type: project
---

`pwsh -NoProfile -File <abs-path-to-script>` launched through the Bash tool inherits the Bash tool's default
working directory, which in a parallel/item run is the **session root worktree**, not the item worktree.

**Why:** The plan convention "every shell invocation begins with `Set-Location (git rev-parse --show-toplevel)`"
is written assuming the process already sits in the item worktree. Under `-File` it does not, so
`git rev-parse --show-toplevel` returns the session root and the script `Set-Location`s to the wrong tree.
This fails **silently and plausibly**: in item #751 the plan's test-assembly discovery returned
`ASSEMBLY_COUNT=0` because the session root had no built `bin\Debug` output. A zero result reads like a
genuine finding (and the delegation prompt even named a known tooling defect that predicts zero), so it can
be mis-reported as a blocker instead of a harness error.

**How to apply:** Always launch script files as
`pwsh -NoProfile -WorkingDirectory "<abs item worktree>" -File "<abs script>"`.
`-WorkingDirectory` is a real pwsh parameter and is the only anchor available for `-File` (unlike
`-Command`, where you can prepend `Set-Location "<abs>"; ...`). Before treating any zero/empty discovery
result as a finding, print `(Get-Location).Path` from inside the script and confirm it is the item worktree.

Related: [[project_bash_cwd_resets_use_env_dash_c]],
[[project_relative_path_in_pwsh_dotnet_io_hits_wrong_worktree]],
[[project_doubled_backslash_dedoubles_bash_to_native_exe]].
