---
name: worktree-root-breaks-dotclaude-exclusion
description: Never plan a "discovered assembly path contains \.claude\" assertion — the agent worktree root is itself under .claude\worktrees\, so the gate is unsatisfiable; use a workspace-root prefix test, but normalize separators and supply the path data yourself (the runner prints only a count)
metadata:
  type: project
---

The standard guidance "when globbing for `*.Test.dll`, exclude any path containing `\.claude\`" is **unsatisfiable when the executing workspace is an agent worktree**, because the worktree root is itself `...\TaskMaster\.claude\worktrees\agent-<id>\`. Every discovered assembly path then contains `\.claude\` and the assertion fails 100% of the time.

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` resolves its search root from `$PSScriptRoot\..\..` (line ~271), i.e. the worktree, and its discovery filter (lines ~296-302) excludes only `\obj\` and `\ref\`.

Correct assertion to write into a plan task: every discovered assembly path **begins with the workspace-root prefix**, and no discovered path contains a `\.claude\worktrees\` segment **after** that prefix (which would be a stale sibling worktree build).

**Why:** #508 preflight pass 1 returned this as a blocking finding on two tasks (baseline coverage capture and final-QC coverage capture). The substring rule is correct in the main checkout and wrong in every agent worktree, which is where plans actually execute.

**How to apply:** Whenever a plan task asserts something about discovered test-assembly paths, state the assertion as a prefix test against the literal workspace root supplied by the caller. Related: [[invoke-mstest-with-coverage-script]], [[invoke-mstest-single-searchroot-defect]].

## Two further traps the prefix-test replacement itself walks into (#736 round-2 preflight)

Writing the prefix test is not the end of it. Round 2 of #736 returned both of these as BLOCKING against the very clause this memory told me to write.

**A. `git rev-parse --show-toplevel` prints FORWARD slashes on Windows; `FullName` prints backslashes.** `C:/Users/.../agent-abc` versus `C:\Users\...\agent-abc\bin\Debug\X.Test.dll`. An unnormalized "begins with" comparison is false for **every** path, so the gate fails 100% of the time — the same failure rate as the `\.claude\` rule it replaced, just for a different reason. Normalize in the plan's own command block and say so in the acceptance text:

```
$root = ((git rev-parse --show-toplevel) -replace '/', '\').TrimEnd('\')
```

**B. The runner prints NO path data at all, so the prefix clause has no data source.** `Invoke-MSTestWithCoverage.ps1` line ~315 emits only `Write-Output "Discovered $($testAssemblies.Count) test assemblies."` — a bare count. It never prints a `FullName` and never prints a file name. A clause asserting "every discovered assembly path begins with..." or "the discovered file-name set is exactly {...}" is asserting over output that does not exist. The fix is to make the plan produce the data itself: re-run the script's own discovery pipeline in the task's command block, character-for-character (`-Filter '*.Test.dll'`, `-match '\\bin\\Debug\\'`, `-notmatch '\\obj\\'`, `-notmatch '\\ref\\'`), and add a count-equality clause against the `Discovered N test assemblies.` line as the cross-check that the two enumerations agreed.

**Why both:** this is the contract's "observe a command's success-case output before asserting over that output" rule biting twice on one clause. I inferred the runner's output from its behavior rather than reading its `Write-Output` calls, and I inferred that two absolute Windows paths would compare as strings.

**How to apply:** before writing any clause over a script's output, grep that script for `Write-Output`/`Write-Host` and confirm the value is actually printed on a SUCCESSFUL run. If it is not, supply the data from the plan's own command block rather than deleting the clause. And normalize path separators on any comparison whose two sides come from git and from .NET respectively.
