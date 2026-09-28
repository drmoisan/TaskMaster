---
name: fsharp-core-hintpath-skew-895
description: "#895: FSharp.Core 11.0.100 HintPath split (3x netstandard2.1 / 3x netstandard2.0); 15 output dirs receive the dll (6 direct, 9 transitive, 6 build-order-unspecified); Sync-PackageReferences.ps1 ranks netstandard2.1 above 2.0; ToDoModel.Test/packages.config omits FSharp.Core+Deedle; #879 harness comments assume QuickFiler.Test deploys the 2.1 flavour"
metadata:
  type: project
---

Research for issue #895 (2026-09-16), artifact at
`docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/research/2026-09-16T23-30-fsharp-core-hintpath-research.md`.

Non-obvious findings (all verified by Grep/Glob; no build ran, no Bash tool in that session):

- **Only HintPaths select the flavour.** No PackageReference, props/targets, Content/None item, manifest, or package build asset touches `FSharp.Core.dll`. `nuget restore` extracting both `lib/netstandard2.0` and `lib/netstandard2.1` is harmless.
- **`scripts/vscode/Sync-PackageReferences.ps1:13-19` is the only in-repo tool that can WRITE `lib\netstandard2.1` into a csproj.** Its `$tfmPreference` fallback ranks `netstandard2.1` before `netstandard2.0`, which is inverted for net481 (NuGet itself never selects netstandard2.1 for .NET Framework). Same-TFM is tried first (lines 85-88), so it only bites when the old TFM folder vanishes on a bump. It runs before every `Invoke-VSBuild.ps1` build (VS Code tasks), NOT in CI. No Pester file exists for it in `tests/scripts/vscode/`.
- **Fifteen `bin/Debug` directories receive `FSharp.Core.dll`**: the 6 direct + Tags, Tags.Test, VBFunctions.Test, TaskTree, TaskVisualization, TaskTree.Test, TaskVisualization.Test, TaskMaster, TaskMaster.Test. Three-way cross-check agreed (ProjectReference closure / on-disk Glob in the primary checkout / 15 app.config FSharp.Core redirects). SVGControl, SVGControl.Test, VBFunctions get none. Rows with mixed parents (TaskTree, TaskVisualization, both .Test twins, TaskMaster, TaskMaster.Test) are build-order-unspecified; the six direct dirs are deterministic (primary reference wins), so QuickFiler, QuickFiler.Test, ToDoModel bins are GUARANTEED to fail a flavour gate on the unfixed tree.
- **`ToDoModel.Test/packages.config` has no FSharp.Core and no Deedle entry** although the csproj carries HintPaths for both (:92-96). Sync-PackageReferences skips HintPaths whose package id is absent from that project's packages.config, so this project breaks (CS0006) on the next bump. Separate latent defect, recommend promotion.
- **Reading a dll's referenced-assembly table without loading it:** every test project already references `System.Reflection.Metadata 10.0.0.12`, so `PEReader`/`MetadataReader.AssemblyReferences` works from MSTest with no csproj dependency edit (first use in repo). `ReflectionOnlyLoadFrom` de-dupes same identity across paths (would need one AppDomain per dir); `AssemblyName.GetAssemblyName` shows only the dll's own version (11.0.0.0 in both flavours). pwsh 7 ships S.R.M in-box on this machine.
- **#879 harness interaction:** `NetstandardBindChildDomainTests.cs:200-208, 364-369` say QuickFiler.Test/bin/Debug "deploys the flavour that references netstandard 2.1.0.0" — false after #895. `AfterInstall_DeedleTypeInitializerSucceeds` loses discriminating power (2.0 flavour binds without the installer); the display-name negative control keeps it.
- `quality-tiers.yml` and `docs/ci.research.md` still absent (re-verified); CLAUDE.md UT2 80/90 governs.

**Why:** the #879 probe returned false green twice by rooting at `TaskMaster.Test/bin/Debug`; any #895 gate must include the three deterministic-2.1 directories and must pin the flavour (agreement-only passes an all-2.1 edit).

**How to apply:** for any future FSharp.Core/Deedle bump or HintPath question, check the Sync-PackageReferences preference list and the ToDoModel.Test packages.config gap first; reuse the 15-directory list rather than re-deriving.
