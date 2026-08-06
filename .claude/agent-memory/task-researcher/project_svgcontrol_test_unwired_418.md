---
name: svgcontrol-test-unwired-418
description: SVGControl.Test is absent from TaskMaster.sln and its pinned test packages are missing from packages/, so it cannot build; plus the ExCSS/Fizzler binding-redirect topology behind issue #418
metadata:
  type: project
---

`SVGControl.Test` is **not** listed in `TaskMaster.sln` (only `SVGControl` is), its `packages.config` pins
`Castle.Core 5.1.1`, `FluentAssertions 6.12.0`, `Moq 4.20.69`, `MSTest.TestAdapter 3.1.1`,
`MSTest.TestFramework 3.1.1` — none of which exist under `packages/` — and its
`EnsureNuGetPackageBuildImports` target emits a hard MSBuild `<Error>` for the missing
`MSTest.TestAdapter.3.1.1` props. Result: the project does not build and has no `bin/`.
`scripts/vscode/Sync-PackageReferences.ps1` only rewrites `<HintPath>` values, so it cannot repair this.
`InternalsVisibleTo("SVGControl.Test")` already exists at `SVGControl/RelativePath.cs:19` and `SVGControl`
is not strong-named.

**Why:** Discovered while researching issue #418 (2026-08-04). Any AC that says "add an MSTest test in
`SVGControl.Test`" is silently blocked until the project is retargeted and added to the solution — this is
real scope that AC wording tends to hide.

**How to apply:** Before accepting any plan that puts new tests in `SVGControl.Test`, verify the project
still fails to build and surface the repair (retarget packages, fix the `<Error>` guard + `<Import>` paths,
add to `.sln`) as an explicit prerequisite task. Alternative fallback: tests in `UtilitiesCS.Test`, which
needs a new `ProjectReference` to `SVGControl` plus `InternalsVisibleTo("UtilitiesCS.Test")`.

Related #418 binding facts (verify before reusing — versions drift):
- `Svg 3.4.7` (identity `Svg, Version=3.4.0.0`) references `ExCSS, Version=4.2.3.0`; only `ExCSS 4.3.1` is
  deployed. 16 `app.config` files redirect ExCSS to `4.3.1.0`; `SVGControl.Test/app.config:23` is the lone
  outlier redirecting to `4.2.4.0`, a version that exists nowhere on disk.
- The ExCSS reference lives in `SvgDocument.Create<T>(XmlReader, string)` inside an `if (styles.Any())`
  branch, so the bind happens at JIT time of that method — removing `<style>` from an SVG payload does
  **not** avoid it.
- `SvgDocument.Open<T>` returns `null` (no exception) for element-free input; it is not exception-only.
- 13 `app.config` files carry a Fizzler redirect to `1.3.0.0` against a deployed `1.3.1.0`, but nothing in
  the graph references Fizzler at all (verified: the string is absent from every DLL in
  `SVGControl/bin/Debug/`). Latent, not active.

See [[qfc-item-controller-227-r2-denial]] for the repo's precedent on not accepting blanket scope
exemptions without per-item analysis.
