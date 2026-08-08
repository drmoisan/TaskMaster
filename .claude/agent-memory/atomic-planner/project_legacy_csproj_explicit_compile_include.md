---
name: legacy-csproj-explicit-compile-include
description: Legacy non-SDK / packages.config C# projects in this repo list every source via explicit <Compile Include> (no glob) AND do not receive transitive assembly references through ProjectReference; both need plan scope-lock + task AC
metadata:
  type: project
---

Legacy non-SDK / packages.config C# projects in this repo enumerate every source file via explicit `<Compile Include="..." />` items with NO wildcard glob. A new `.cs` file added to such a project will NOT compile into the assembly unless a matching `<Compile Include>` item is added. Confirmed legacy (as of #263 planning, 2026-07-07): `UtilitiesCS/UtilitiesCS.csproj` (436 Compile Include items), `TaskMaster/TaskMaster.csproj` (35), `TaskMaster.Test/TaskMaster.Test.csproj` (33), `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (396). Treat all four TaskMaster-solution first-party projects as explicit-include.

**Why:** Caught during #207 planning. Executor passed preflight and completed Phase 0, then correctly STOPPED at P1-T1 because the plan created two new `UtilitiesCS/OutlookObjects/*.cs` files but `UtilitiesCS.csproj` was not in the scope-lock list and no `<Compile Include>` wiring was specified — the files could not build into `UtilitiesCS.dll`. The plan already did the equivalent for `TaskMaster.Test.csproj`, so it was a consistency gap.

**How to apply:** When a plan creates ANY new `.cs` file in a legacy/packages.config project, the plan MUST (a) list a `MODIFY <project>.csproj` entry in the scope-lock with the exact `<Compile Include>` item(s), and (b) fold the csproj wiring into the file-creation task's single binary outcome (file exists AND is wired into the csproj so it compiles). Verify a project is legacy by checking for `packages.config` and explicit `<Compile Include>` items rather than SDK-style globbing.

## Second failure mode: transitive assembly references do not flow

In a legacy non-SDK project, a `<ProjectReference>` does NOT flow the referenced project's assembly references to the compiler — they land in `ReferenceDependencyPaths` (copy-local at runtime) rather than `ReferencePath` (compile-time). A test that merely names a type from a transitively-referenced package fails to compile with `CS0012`.

**Why:** Caught during #418 preflight. `SVGControl.Test.csproj` had a `ProjectReference` to `SVGControl` but no `<Reference Include="Svg" ...>` and no `Svg` entry in its own `packages.config`; every planned test named `SvgDocument`, so the whole test file would have failed with `CS0012` for `Svg, Version=3.4.0.0`.

**How to apply:** Before planning tests in a legacy test project, grep that project's own `.csproj` `<Reference>` block and `packages.config` for every third-party type the tests will name. If a type comes only from the production project's package set, add an explicit task that adds BOTH the `packages.config` `<package id=... />` entry and the `<Reference Include=... ><HintPath>..\packages\<id>.<ver>\lib\<tfm>\<dll>.dll</HintPath></Reference>` item, copying the exact `Version=`/`PublicKeyToken=` from the production project's csproj. Widen the scope-lock entry for that csproj to permit `<Reference>` items, and add the `packages.config` unconditionally.

### Corollary: assert by `ParameterType.FullName`, not `typeof(...)`, when the scope lock forbids adding the reference

`TaskMaster.Test.csproj` carries only `Microsoft.Office.Interop.Outlook`; it has NO `<Reference Include="Office, Version=15.0.0.0 ...">` (the Office Core PIA that supplies `Microsoft.Office.Core.IRibbonControl`), even though `TaskMaster.csproj` does. So a reflection test that writes `typeof(Microsoft.Office.Core.IRibbonControl)` fails to compile in the test project.

**Why:** Caught in #503 preflight (delta B3). The plan's scope lock deliberately restricted `TaskMaster.Test.csproj` to four `<Compile Include>` entries, so adding a `<Reference>` mid-execution would have breached the scope lock; the fix was to weaken the assertion, not widen the scope.

**How to apply:** When a signature-shape assertion needs a type the test project cannot reference, and adding the reference would breach the plan's own scope lock, pin the assertion to `method.GetParameters()[0].ParameterType.FullName == "<literal namespace-qualified name>"` and state in the task text WHY `typeof` is prohibited. This works because the assembly (e.g. `office.dll`) is present in `bin\Debug\` and the GAC at runtime even though it is absent from the compile-time reference set. Choose deliberately between the two remedies — widening the scope lock is the alternative, and the plan must say which one it took.
