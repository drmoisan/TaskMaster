---
name: commandline-rebuild-omits-vsto-manifests-addin-cannot-load
description: A command-line msbuild Rebuild of TaskMaster.sln never writes TaskMaster.vsto / TaskMaster.dll.manifest, so a manual-verification phase that reopens Outlook after the rebuild needs a separate manifest-generating (Visual Studio) build first
metadata:
  type: project
---

A plain `msbuild TaskMaster.sln /t:Rebuild` produces `TaskMaster/bin/Debug/TaskMaster.dll` but NOT `TaskMaster.vsto` or `TaskMaster.dll.manifest`; `TaskMaster/TaskMaster.csproj` imports the Office targets only when `BuildingInsideVisualStudio` is true. The registered add-in (`HKCU\...\Outlook\Addins\TaskMaster`, `Manifest` = `.../TaskMaster.vsto|vstolocal`) cannot load without the `.vsto`.

**Why:** On #792 [P8-T1] (2026-09-17) the rebuild artifact recorded the missing manifests as "not a defect, by design". It was a real gap: a VS-driven build generated both manifests 16 minutes later (mtime 21:43:18, over the unchanged 21:27 assembly), four seconds before Outlook started. Had nobody done that, the "person reopens Outlook and confirms the add-in loaded" half of the task would have failed silently.

**How to apply:** In any plan task that rebuilds and then expects Outlook to load the add-in, check `TaskMaster/bin/Debug/TaskMaster.vsto` exists and its mtime is at or after the assembly's; if absent, the human must run a manifest-generating build (F5/Build in VS) before reopening Outlook. Record the manifest mtimes alongside the assembly mtime; proof the add-in ran is the session log appearing under the worktree's own `TaskMaster/bin/Debug/logs/`. Related: [[epic-checkpoint-hooks-scan-command-text-for-worktree-tokens]].
