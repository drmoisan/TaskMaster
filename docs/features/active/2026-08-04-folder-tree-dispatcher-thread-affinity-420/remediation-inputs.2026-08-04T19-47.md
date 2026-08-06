# Remediation Inputs: Folder-tree dispatcher thread affinity

## Authoritative Requirements

This document is the primary requirements source for remediation of the feature-review findings in `policy-audit.2026-08-04T19-47.md`, `code-review.2026-08-04T19-47.md`, and `feature-audit.2026-08-04T19-47.md`.

## Required Fixes

1. In `TaskMaster/AppGlobals/AppOlObjects.cs`, remove the worker-to-UI synchronous invocation while `_folderTreeServiceGate` is held. Preserve one session-scoped service and no worker fallback. Add deterministic tests for worker-first composition concurrent with UI-side disposal or equivalent shutdown interleaving.
2. In `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs`, make disposal and publication mutually safe. A completed build must not set state, publish a snapshot, schedule refresh, or raise `SnapshotChanged` after disposal.
3. Marshal notification-sink unsubscribe/disposal to the captured STA dispatcher. Add deterministic tests proving construction, refresh, notification cleanup, and disposal happen on that dispatcher.
4. In `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs`, replace the public fire-and-forget initialization behavior with an observable readiness/failure contract or remove that path. Subscribe close/cancellation handling before awaiting a cold snapshot so a close-during-load cannot wire a closed viewer or retain service handlers. Test successful, faulted, and close-before-snapshot completion cases.
5. In the ribbon path, establish and test a defined failure policy for the awaited initialization task. The legacy `TryLoadFolderFilter` wrapper must not silently lose a fault.
6. Add dedicated STA-hosted coverage for `WpfUiDispatcher.InvokeAsync(Func<Task<TResult>>)` including successful result, fault propagation, and any relevant cancellation behavior.
7. Re-run final coverage with a baseline and final report from equivalent scope. Enforce the repository policy of `>=80%` repository-wide and `>=90%` new methods/classes/modules. Do not claim an exception unless an approved exception artifact exists.
8. Update `spec.md`, QA evidence, and acceptance-criteria checkboxes only after each criterion is independently verified. Correct the coverage assertion and the final inventory if evidence differs from the actual files.

## Verification Commands

```powershell
dotnet tool run csharpier .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/qa-gates/coverage-final.cobertura.xml
git diff --check origin/main
```

## Do Not Do

- Do not introduce `Task.Yield`, a worker-local dispatcher, or a caller-selected traversal fallback.
- Do not weaken coverage requirements, alter policy files, or record an unapproved coverage exception.
- Do not remove tests or replace deterministic race tests with sleeps, timers, polling, live Outlook, network, or temporary files.
- Do not expand the public `IOutlookFolderTreeService` snapshot contract.
- Do not claim acceptance criteria or QA completion before evidence is regenerated and reviewed.
