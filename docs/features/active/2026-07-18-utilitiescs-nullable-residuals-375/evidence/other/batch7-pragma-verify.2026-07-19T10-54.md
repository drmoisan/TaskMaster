# Batch 7 Pragma Verification (P8-T6)

Timestamp: 2026-07-19T10-54

Batch 7 opted-in hand-written files (4, FolderRemap):
1. FolderSelector.cs — `OlFolderRemap? _selection = null`; `Selection` and `SelectFolder` returns → `OlFolderRemap?`.
2. FolderRemapViewer.cs — own field `_controller = null!` (set via SetController).
3. FolderRemapTree.cs (two classes) — `_roots = null!` (parameterless ctor leaves it null); `PropertyChanged`
   events `?` (both classes); nested `OlFolderRemap` ctor-unset fields `_olRoot/_olFolder/_name/_relativePath = null!`;
   `_mappedTo`/`MappedTo` → `OlFolderRemap?`; `_batchNotifier` initializer kept. Justified `!` at two
   GetRemapList-filtered sites (`mapping.MappedTo!.RelativePath`, `new TreeNode<OlFolderRemap>(mapping.MappedTo!)`).
4. FolderRemapController.cs (284 lines) — `_mappings2 = null!` (set via setter in ctor); aligned
   `SelectFolder` consumption with `OlFolderRemap?` (existing `is null` checks preserved); justified `!`
   at Mappings2-filtered sites (`mapping.MappedTo!.RelativePath` x2) and at the `as`-cast `target!.Value`
   / `target.Value.MappedTo!.Name` sites (original code dereferenced these directly, assuming non-null).

P8-T5: The two `*.Designer.cs` files (FolderRemapViewer, FolderSelector) carry NO `#nullable` pragma
and are unmodified (`git status` clean) — AC3.

## Trustworthy isolated CS86xx gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx, 15 pre-existing out-of-scope warnings. First
pass surfaced 6 CS8602/CS8604 (all consequences of `OlFolderRemap.MappedTo` becoming nullable plus
`e.TargetModel as TreeNode<...>` / `e.ListView as TreeListView` producing nullable results); all
resolved with justified `!` at filtered/assumed-non-null sites, and by annotating the two UNUSED
`MoveObjectsToChildren` parameters (`targetTree`/`sourceTree`) as `TreeListView?` to accept the
nullable `as`-cast arguments. No new runtime guard; no behavior change.
