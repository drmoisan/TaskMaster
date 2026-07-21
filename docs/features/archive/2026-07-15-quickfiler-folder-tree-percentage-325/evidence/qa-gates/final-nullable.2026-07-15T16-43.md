# Final QC — Nullable / TreatWarningsAsErrors (P6-T3)

Timestamp: 2026-07-16T11-20
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
Output Summary: Build SUCCEEDED. 0 Warning(s), 0 Error(s). The plan's nullable/type-check gate passes
green with warnings treated as errors — identical to the P0-T4 baseline (0/0).

Supplementary observation (not the plan gate): a genuine `/t:Rebuild /p:Nullable=enable` (full
recompile) surfaces latent, pre-existing repo-wide nullable debt that the incremental `/t:Build`
gate does not force. The bulk is in vendored `SVGControl` (many CS8618/CS8625/CS8600). Within the
#325 files it surfaces a small number of nullable-enable-only diagnostics
(FolderTreeStateModel `_highlighted` CS8618; FolderHierarchyBuilder `cumulative` CS8600/CS8625; and
the pre-existing `IFolderSearchHandler.FindFolder` `= null` defaults inherited from #324). These:
- appear ONLY under a full nullable recompile (never under the default analyzer/type-check builds
  that actually compile the new files — those are clean, see P6-T2);
- are consistent with the repository's nullable-disabled convention (new files carry no `#nullable`
  directive and no reference-type `?` annotations, matching the #324 Folder types);
- are NOT fixable by adding reference-type `?` annotations without introducing CS8632
  ("annotation only valid in a #nullable context") into the DEFAULT build, which WOULD break the
  analyzer and incremental nullable gates. Introducing `#nullable enable` file scopes is beyond the
  plan's scope.
The plan's specified gate is green; the new code introduces zero warnings under the builds that
compile it.
