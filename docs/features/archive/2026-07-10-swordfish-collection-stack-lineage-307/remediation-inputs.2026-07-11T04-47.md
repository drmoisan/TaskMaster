# Remediation Inputs — Cycle 1 (#307 swordfish-collection-stack-lineage)

- Timestamp: 2026-07-11T04:47:55Z
- Entry-ts: 2026-07-11T04-47
- Trigger: integration-time merge conflict (new finding discovered after feature-review; feature-review itself returned 0 Blocking findings)
- Base under integration: origin/epic/swordfish-removal-integration @ 618954b855a09235ed8d698eda3ac1720d2f3ddb
- Feature branch tip: 78684e65bcda53292f3e3dc5958d784f98322fd9
- Merge-base: 0b72b11bb1145dd00f70fe9de8d7a6ed3bef79bb (branch is behind the integration tip; parallel sibling child merges #306/#309/#310 landed while F2 executed)

## Finding: two content conflicts merging integration tip into feature branch

The `git merge origin/epic/swordfish-removal-integration` produces exactly two content conflicts. Both are disjoint cross-feature edits (each feature changed a different concern in a shared file); the correct resolution is the union of both sides.

### Conflict 1 — UtilitiesCS/Interfaces/IGlobals/IToDoObjects.cs

- HEAD (#307) retyped `PrefixList` / `LoadPrefixList` to `ConcurrentObservableCollection<IPrefix>` and left `FilteredFolderScraping` / `FolderRemap` as `ScoDictionary<...>`.
- Integration (#306 dictionary-lineage) left `PrefixList` / `LoadPrefixList` as `ScoCollection<IPrefix>` and retyped `FilteredFolderScraping` / `FolderRemap` to `ScoDictionaryNew<...>`.

Deterministic resolution (union — take each feature's own change):

```
        ConcurrentObservableCollection<IPrefix> PrefixList { get; }
        ConcurrentObservableCollection<IPrefix> LoadPrefixList();
        ScoDictionaryNew<string, int> FilteredFolderScraping { get; }
        ScoDictionaryNew<string, string> FolderRemap { get; }
```

### Conflict 2 — UtilitiesCS/UtilitiesCS.csproj

- HEAD (#307) removed the `ScoStack.cs` `<Compile Include>` entry (this feature deletes ScoStack.cs). At this hunk position HEAD shows `ScoSortedDictionary.cs` remaining.
- Integration (#309 scosorteddictionary-removal) removed the `ScoSortedDictionary.cs` entry. At this hunk position it shows `ScoStack.cs` remaining.

Both files are deleted by their respective features, so both compile entries must be removed. Deterministic resolution (union of both deletions):

```
    <Compile Include="ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs" />
    <Compile Include="ReusableTypeClasses\Serializable\SerializableList.cs" />
```

(neither `ScoSortedDictionary.cs` nor `ScoStack.cs` entries remain)

## Exit criteria for this cycle

- Merge committed with both conflicts resolved as above.
- Full C# toolchain green against the no-regression baseline: csharpier check EXIT 0; analyzer build 0 first-party errors; nullable build reproduces the vendored-only baseline set with 0 new first-party diagnostics; vstest 0 failures (baseline set only).
- feature-review reaudit produces code-review / feature-audit / policy-audit with blocking_count == 0.
