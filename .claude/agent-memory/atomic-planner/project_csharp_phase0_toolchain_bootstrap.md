---
name: csharp-phase0-toolchain-bootstrap
description: C# plans need a Phase 0 bootstrap task (Install-RepoDotNetSdk.ps1 + dotnet tool restore + dotnet-coverage) or every csharpier and coverage task fails on a fresh checkout
metadata:
  type: project
---

Every C# atomic plan in this repo must open Phase 0 with a toolchain-bootstrap task before any csharpier or coverage command task. Three separate prerequisites are not satisfied by a fresh checkout:

1. `global.json` pins SDK `8.0.205` with `"paths": [".dotnet-sdk", "$host$"]`, and `.dotnet-sdk/` is gitignored. Until `scripts/vscode/Install-RepoDotNetSdk.ps1` runs, `dotnet tool run csharpier --version` fails with an instruction to run that script.
2. `Install-RepoDotNetSdk.ps1` does NOT run `dotnet tool restore`, so csharpier (manifest at repo-root `dotnet-tools.json`) needs a separate `dotnet tool restore`.
3. `dotnet-coverage` is a global tool that is not installed by either of the above. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws without it (guard near line 129).

Package restore itself is fine: `packages/` is gitignored and restored by `scripts/vscode/Invoke-Restore.ps1` (`msbuild /t:Restore /p:RestorePackagesConfig=true`); the `EnsureNuGetPackageBuildImports` target is `BeforeTargets="PrepareForBuild"` so it does not fire during restore.

**Why:** #418 preflight pass 1 returned two blocking findings (B1, B2) because the plan's csharpier baseline, csharpier final-QC, coverage baseline, and coverage final-QC tasks were all unrunnable — and the two coverage tasks carry the mandatory numeric coverage evidence that a minor-audit plan cannot report PASS without.

**How to apply:** Make it `[P0-T1]`, ahead of the policy reads, with acceptance requiring an `evidence/baseline/toolchain-bootstrap.<ts>.md` artifact that records `EXIT_CODE: 0` for all three commands plus a verified `csharpier --version` and a resolving `dotnet-coverage --version`. Related: [[evidence-path-normalization]], [[csharp-coverage-gate-jacoco-format]].
