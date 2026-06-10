---
name: csharp-analyzer-packages-config-quirks
description: Non-obvious legacy-MSBuild quirks when adding Roslyn analyzers to this repo's non-SDK packages.config projects (issue #181)
metadata:
  type: project
---

Adding third-party Roslyn analyzers to TaskMaster's legacy NON-SDK / packages.config C# projects (issue #181) surfaced quirks that are invisible to SDK-style/dotnet tooling.

**Why:** This repo's 16 packages.config projects restore via `nuget restore` and build via msbuild + a Roslyn 5.6 (VS18) compiler. Analyzer packages designed for `<PackageReference>` auto-selection do not behave the same way here.

**How to apply:** When wiring analyzer packages into this repo, expect and handle these:

1. **Manual Roslyn-version subfolder selection.** NuGet does NOT auto-select the right `analyzers/dotnet/cs/...` subfolder for packages.config projects. `<Analyzer Include>` items must point at the correct Roslyn-version subfolder explicitly: Meziantou.Analyzer → `roslyn5.0`, Roslynator.Analyzers → `roslyn4.7` for this repo's Roslyn 5.6.

2. **SecurityCodeScan.VS2019 5.6.7 is incompatible with Roslyn 5.6 here.** Its analyzer types fail to load (TypeInitializationException → FileNotFoundException for `YamlDotNet, Version=11.0.0.0`), emitting **CS8032**. CS8032 is a compiler warning, NOT an analyzer rule, so it CANNOT be set to `suggestion` via `.editorconfig`. Under the CI `/p:TreatWarningsAsErrors=true` nullable build it becomes an error and breaks the gate. Wiring the co-located `YamlDotNet.dll` as a sibling `<Analyzer>` does not resolve it in the Roslyn 5.6 load context. Resolution for #181: SecurityCodeScan was dropped from the rollout (the other 5 analyzers load cleanly); revisit only with a Roslyn-5.x-compatible security analyzer.

3. **AsyncFixer rule IDs are `AsyncFixer01`–`AsyncFixer06`, not `AF*`.**

4. **Verified compatible versions (Roslyn 5.6 / packages.config):** Meziantou.Analyzer 3.0.101, SonarAnalyzer.CSharp 10.27.0.140913, Roslynator.Analyzers 4.15.0, AsyncFixer 2.1.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4.

5. **Baseline nullable-as-errors build already has 84 errors, all confined to the two vendored projects** (UtilitiesSwordfish 50, SVGControl 34). First-party no-regression is measured against that 84 baseline. See [[evidence-and-lifecycle-for-every-change]].
