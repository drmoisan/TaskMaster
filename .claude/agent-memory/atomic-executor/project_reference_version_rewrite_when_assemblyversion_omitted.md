---
name: reference-version-rewrite-when-assemblyversion-omitted
description: dependencies/ProjectConsistency Invoke-VersionReconciliation rewrites a Reference Include assembly version to the PACKAGE version when -AssemblyVersion is omitted; confirm the declared version against the restored lib assemblies instead
metadata:
  type: project
---

`Invoke-VersionReconciliation` (in `scripts/dependencies/ProjectConsistency.psm1`) falls back to the
manifest **package** version for a `<Reference Include="X, Version=...">` whose simple name equals
the package id when `-AssemblyVersion` is not supplied. An assembly version is not required to track
its package version, so the fallback corrupts `.csproj` files wholesale:
`Apache.Arrow, Version=23.0.0.0` becomes `23.0.0`, `Microsoft.Data.Analysis, Version=1.0.0.0`
becomes `0.23.0`. Measured on issue #911: 51 such rewrites in `QuickFiler.csproj` alone.
`Invoke-ProjectConsistencyRepair` in `ConsistencyVerifier.psm1` calls it **without**
`-AssemblyVersion`, so any caller that uses that entry point over the real tree inherits the defect.

**Why:** resolving the assembly version is filesystem work, so the module leaves it to the caller
and documents the fallback as a convenience; over a real tree the convenience is destructive, and it
breaks any gate asserting an empty `.csproj` porcelain after a repair run.

**How to apply:** a composition root should wire `Invoke-AnalyzerItemRepair` and
`Invoke-VersionReconciliation` itself rather than calling `Invoke-ProjectConsistencyRepair`, and
resolve the assembly version by **confirming** rather than selecting: enumerate
`packages/<id>.<version>/lib/**/<id>.dll`, read `[System.Reflection.AssemblyName]::GetAssemblyName`
on each, preserve the version the project already declares when the package ships it anywhere, and
rewrite only a version the package ships nowhere. Measured on this tree: 796 of 796 declared
versions confirmed under that rule (so it is a no-op and still falsifiable), versus 9 disagreements
if the compatible-folder assembly is selected outright. Restrict the search to `lib` and memoise it
per id|version; the unrestricted search also matches analyzer and tooling copies of a same-named
assembly. A full-tree run still takes ~110 s because the modules re-parse the project text once per
package.
