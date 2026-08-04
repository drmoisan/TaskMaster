---
name: legacy-csproj-no-transitive-compile-refs
description: In this repo's non-SDK csproj projects, a ProjectReference does NOT give the referencing project compile-time access to the referenced project's third-party types — plan for an explicit <Reference> + packages.config entry (CS0012)
metadata:
  type: project
---

Every `*.Test.csproj` in TaskMaster is a legacy non-SDK project using `packages.config`.
A `<ProjectReference>` to a production project does **not** flow that project's
third-party assembly references to the compiler: MSBuild's `ResolveAssemblyReference`
puts transitive dependencies in `ReferenceDependencyPaths` (copy-local only), while
`csc` is fed `@(ReferencePath)` (primary references + ProjectReference outputs only).

Symptom: `CS0012: The type 'X' is defined in an assembly that is not referenced.`
The DLL is sitting in `bin\Debug\` — that is a runtime artifact, not a compile reference.

**Why:** discovered on #418 preflight. `SVGControl.Test` has a `ProjectReference` to
`SVGControl` but no `<Reference Include="Svg" ...>`. Every planned test named
`SvgDocument` (as the type of `SvgRenderer.Document`, and in a
`Mock<Func<byte[], SvgDocument>>` seam), so the whole assembly would have failed to
compile despite `Svg.dll` being copy-local.

**How to apply:** when a plan adds tests that name a type owned by a *package* the
production project references (not a type the production project itself declares),
budget a task that adds BOTH:
- `<package id="<Id>" version="<Ver>" targetFramework="net481" />` to the test project's
  `packages.config`, and
- a matching `<Reference Include="<Name>, Version=..., PublicKeyToken=...">` with a
  `<HintPath>..\packages\<Id>.<Ver>\lib\<tfm>\<Name>.dll</HintPath>`,
copying the exact shape from the production `.csproj`.

Also widen the plan's Scope Lock to permit `<Reference>` items and unconditional
`packages.config` edits, or the executor hits a plan that both requires and forbids
the edit. Related: [[project_timeprovider_seam_gotchas]] (same CS0012 class, caused by
an optional parameter type instead of a test-authored type reference).
