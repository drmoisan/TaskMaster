---
name: project-670-capture-time-sanitisation-seams
description: Issue #670 preflight rounds 2-5 — a script that echoes a vswhere-resolved path leaks a host literal into an intermediate commit; gate sanitisation at capture time, not in a later sweep
metadata:
  type: project
---

Issue #670 (`QfcItemController.InitializeWebViewAsync` fault unobserved) took five preflight rounds.
Three of them were the same defect class, found one artifact at a time.

**The class:** a Phase 0 baseline task whose `Output Summary:` reproduces tool output that names an
absolute host path. If a Phase 3 commit task stages that artifact and the only sweep that reaches it
runs in Phase 4, the literal lands in an intermediate commit — and in-place sanitisation cannot
recover a literal already committed. The fix is always the same shape: a capture-time rewrite
instruction plus a capture-time zero-sweep gate in the task's own **Acceptance**, never a later
backstop.

**The non-obvious instance:** `scripts/vscode/Invoke-Restore.ps1` resolves MSBuild through `vswhere`
at `:27` and echoes it at `:32` (`Write-Host "Using MSBuild: $msbuildPath"`). A task that merely
*invokes* that script therefore leaks a resolved Program Files path even though the task text names
no `vswhere` call of its own. I initially flagged this as a judgment call and declined to act;
round 4 verified the mechanism and rejected the judgment.

**How to apply:** When auditing host-path obligations, enumerate tasks that resolve a tool
*indirectly* through a repo script, not just tasks whose command line contains `vswhere`. Grep the
invoked script for `Write-Host`/`Write-Output` of a resolved path. Also check what the invoked tool
*relays*: `Invoke-Restore.ps1:36` passes an absolute `$resolvedSolutionPath` to MSBuild `/t:Restore`,
whose own output then names project and package directories absolutely.

**Corollary that held:** gating the two artifacts at capture time left `P3-T14`'s four-file
in-scope list correct and unchanged — once they carry no host literal when P3-T14 runs, they are
not "artifacts produced so far that carry host identifiers". Fixing upstream avoided touching a
count downstream. Related: [[observation-scope-must-match-blast-radius]],
[[../_shared_no_absolute_host_paths]].
