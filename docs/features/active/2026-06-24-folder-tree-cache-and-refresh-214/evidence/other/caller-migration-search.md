# Caller Migration Search Evidence

Issue: 214

## Command

```powershell
rg -n "new FolderTree|FolderTree\.CreateAsync|Task\.Run\(\(\) => new FolderTree" TaskMaster/Ribbon/RibbonController.cs UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs
```

## Result

- Exit code: `1`
- Output: no matches

`rg` exit code `1` is the expected passing result for this negative search.
