---
name: nuget-flatcontainer-nuspec-webfetch
description: How to read a package's declared dependency ranges when the repo's packages/ tree has no extracted .nuspec — WebFetch the NuGet flat-container nuspec URL; mark findings [V-web]
metadata:
  type: reference
---

The restored `packages/` tree in the main checkout contains the `.nupkg` but no extracted `.nuspec`, and the item worktrees have no `packages/` at all, so dependency ranges cannot be read from disk without a zip tool.

Working route (used 2026-10-02 for #973): `WebFetch` on `https://api.nuget.org/v3-flatcontainer/<id-lowercase>/<version>/<id-lowercase>.nuspec` and ask for the `<dependencies>` group of the relevant target framework verbatim. This returns the primary document. The nuget.org package page (`https://www.nuget.org/packages/<Id>/<version>`) also works but is a secondary summary; one summary wrongly said Microsoft.Bcl.Memory was absent "because System.Memory serves that purpose" — prefer the nuspec.

Tag such findings `[V-web]` in research notes. For the assembly-version-from-package-version mapping, use the csproj `Reference Include` strings in this repo (Azure.Core 1.63.0 -> 1.63.0.0; Microsoft.Extensions.* and Microsoft.Bcl.* 10.0.12 -> 10.0.0.12; IdentityModel 8.23.0 -> 8.23.0.0; MSAL 4.90.1 -> 4.90.1.0) and mark any extrapolation to an uninstalled version as `[I]`.
