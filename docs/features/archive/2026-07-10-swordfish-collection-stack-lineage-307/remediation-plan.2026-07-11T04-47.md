# Remediation Plan — Cycle 1 (#307 swordfish-collection-stack-lineage)

- Timestamp: 2026-07-11T04:47:55Z
- Entry-ts: 2026-07-11T04-47
- Inputs: remediation-inputs.2026-07-11T04-47.md
- Scope: resolve the two integration-time merge conflicts against origin/epic/swordfish-removal-integration @ 618954b8 and reverify the C# toolchain. No feature-behavior change; the resolution is the deterministic union of two already-reviewed sibling edits.
- Executor edits all production files (strict-handoff: orchestrator does not edit production files directly).

### Phase 1 — Merge and resolve

- [x] [P1-T1] From the feature worktree, run `git merge origin/epic/swordfish-removal-integration` (expect the two documented conflicts).
- [x] [P1-T2] Resolve `UtilitiesCS/Interfaces/IGlobals/IToDoObjects.cs` to the union: `PrefixList`/`LoadPrefixList` typed `ConcurrentObservableCollection<IPrefix>` (F2) AND `FilteredFolderScraping`/`FolderRemap` typed `ScoDictionaryNew<...>` (F1). Remove all conflict markers.
- [x] [P1-T3] Resolve `UtilitiesCS/UtilitiesCS.csproj` to remove BOTH the `ScoSortedDictionary.cs` and `ScoStack.cs` `<Compile Include>` entries (both source files are deleted by their features). Remove all conflict markers.
- [x] [P1-T4] Confirm no other conflicts remain (`git diff --name-only --diff-filter=U` empty) and no leftover conflict markers exist anywhere.

### Phase 2 — Reverify toolchain (CLAUDE.md order)

- [x] [P2-T1] `csharpier .` then `csharpier --check .` (EXIT 0, 0 files need formatting).
- [x] [P2-T2] `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (0 first-party errors).
- [x] [P2-T3] `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` (reproduces vendored-only baseline set; 0 new first-party nullable diagnostics).
- [x] [P2-T4] `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /EnableCodeCoverage` (0 failures beyond the documented pre-existing baseline set; all F2 tests pass). Restart the loop from P2-T1 if any step changes files or fails.
- [x] [P2-T5] Write the merge reverify evidence to `evidence/remediation-baseline/merge-reverify.2026-07-11T04-47.md` with Timestamp/Command/EXIT_CODE/Output Summary for each toolchain step.

### Phase 3 — Commit

- [x] [P3-T1] Commit the merge with both resolutions and the evidence artifact on branch feature/swordfish-collection-stack-lineage.
