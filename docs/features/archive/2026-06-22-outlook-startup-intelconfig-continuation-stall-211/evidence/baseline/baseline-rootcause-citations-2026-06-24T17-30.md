# Baseline — Root Cause and Matching Semantics Citations (AC10, issue #211)

Timestamp: 2026-06-24T19-06

Verified file:line list (read and confirmed against current source):

## Defect site — TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs
- :56  `var root = new FolderTree(Root).Roots.FirstOrDefault();` (LoadJunkPotential full-tree build)
- :62  `var sequence = new Queue<string>(folderPath.Split('\\'));` (LoadJunkPotential)
- :64  `var node = root.FindSequentialNode((current, other) => current.Name == other, sequence);` (ordinal `==` comparator, case-sensitive)
- :103 `var root = new FolderTree(Root).Roots.FirstOrDefault();` (LoadJunkCertain full-tree build)
- :109 `var sequence = new Queue<string>(folderPath.Split('\\'));` (LoadJunkCertain)
- :111 `var node = root.FindSequentialNode((current, other) => current.Name == other, sequence);` (ordinal `==` comparator, case-sensitive)

## Root folder source — TaskMaster/AppGlobals/AppOlObjects.cs
- :183 `_root = (Folder)App.Session.DefaultStore.GetRootFolder();` (Root = default-store root folder)

## Full-tree builder — UtilitiesCS/OutlookObjects/Folder/FolderTree.cs
- :33-38  `FolderTree(MAPIFolder olRoot)` ctor -> `RootFromFolder(olRoot)`
- :150-156 `RootFromFolder(MAPIFolder olRoot)` -> `InitializeChildren(root, olRoot)`
- :185-194 `InitializeChildren` RECURSIVELY enumerates entire hierarchy (recursion at :191-192 `if (child.Folders.Count > 0) InitializeChildren(childNode, olRoot);`)

## Sequential matcher — UtilitiesCS/ReusableTypeClasses/Other/TreeNodeOfT.cs
- :149-160 `FindSequentialNode<U>` — FIRST segment via `FindNode(comparator, descendByLevel:true)` (:153); SUBSEQUENT segments via `node.Children?.Where(x => comparator(x.Value, next))?.FirstOrDefault()` (:157)
- :162-185 `FindNode(comparator, descendByLevel)` — when descendByLevel true: BFS from `{ this }` (root node), first ordinal match per level (:170-184)
- :194-204 `GetNextLevel` — BFS frontier expansion (children of current level)

## Relative path composition — UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs
- :176-182 `Name` property and `LoadName() => OlFolder?.Name`
- :193-223 `LoadRelativePath()` — when `OlFolder.FolderPath` contains `OlRoot.FolderPath`, returns `OlFolder.FolderPath.Replace(OlRoot.FolderPath + "\\", "")` (:221) — path below root, root prefix removed

## Matching semantics confirmed (semantics 1-5 from plan):
1. `folderPath.Split('\\')` -> `Queue<string>` (verbatim, no trim).
2. Comparator `(current, other) => current.Name == other` — ordinal `string ==`, case-SENSITIVE, no trim, no culture.
3. `FolderWrapper.Name == OlFolder.Name`.
4. FindSequentialNode: first segment = BFS-from-root (root matched first, then level-by-level, first match wins); subsequent segments = direct-children first-match only; null on any unmatched segment.
5. Stored RelativePath is root-prefix-stripped, so first stored segment is normally a direct child of root (matched at BFS level 1).

EXIT_CODE: 0 (verification by source read; no command failure)
