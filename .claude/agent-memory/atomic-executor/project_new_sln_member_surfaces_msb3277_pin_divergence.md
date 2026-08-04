---
name: new-sln-member-surfaces-msb3277-pin-divergence
description: Adding a previously-unbuilt legacy test project to TaskMaster.sln surfaces a brand-new MSB3277 warning whenever its packages.config pins differ from the project it ProjectReferences; plan for it as an in-scope csproj/packages.config edit
metadata:
  type: project
---

Adding a long-unbuilt legacy (`packages.config`, non-SDK) test project to `TaskMaster.sln`
reliably produces **one new `MSB3277` reference-conflict warning** if that test project pins a
different version of any transitive package than the production project it `ProjectReference`s.
Observed on #418: `SVGControl.Test` pinned `System.Runtime.CompilerServices.Unsafe 6.0.0`
(assembly `6.0.0.0`) while `SVGControl` pins `6.1.2` (assembly `6.0.3.0`) and copies that DLL
into its `bin\Debug`. RAR reports an unresolvable conflict, attributed solely to the test
project's `.csproj`.

**Why:** The warning is invisible until the project actually builds. A Phase 0 baseline taken
before the `.sln` entry is added cannot contain it, so it reads as a brand-new diagnostic
against the baseline and can trip a "zero new diagnostics" gate or a `SCOPE_EXCEEDED` clause.
It cannot be cleared from any `.cs` file — only from `packages.config` + the `<Reference>`
`Version=`/`<HintPath>` pair.

**How to apply:** When a plan repairs an unwired legacy test project, expect this and make sure
the Scope Lock authorizes editing that project's `packages.config` and `.csproj` reference
versions for diagnostic remediation, not just under a package-restore contingency. Before
running the solution gate, diff the test project's pins against the referenced production
project's pins (`grep -nE '<package id=' <proj>/packages.config`) and align them up front.
Check the app.config `bindingRedirect` for the same assembly when you retarget, since the
redirect's `newVersion` must match the new assembly version.

Related: [[legacy-csproj-no-transitive-compile-refs]], [[project-build-test-env]].
