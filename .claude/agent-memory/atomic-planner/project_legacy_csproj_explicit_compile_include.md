---
name: legacy-csproj-explicit-compile-include
description: Legacy non-SDK / packages.config C# projects in this repo list every source via explicit <Compile Include> (no glob) AND do not receive transitive assembly references through ProjectReference; both need plan scope-lock + task AC
metadata:
  type: project
---

Legacy non-SDK / packages.config C# projects in this repo enumerate every source file via explicit `<Compile Include="..." />` items with NO wildcard glob. A new `.cs` file added to such a project will NOT compile into the assembly unless a matching `<Compile Include>` item is added. Confirmed legacy (as of #263 planning, 2026-07-07): `UtilitiesCS/UtilitiesCS.csproj` (436 Compile Include items), `TaskMaster/TaskMaster.csproj` (35), `TaskMaster.Test/TaskMaster.Test.csproj` (33), `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (396). Treat all four TaskMaster-solution first-party projects as explicit-include.

**Why:** Caught during #207 planning. Executor passed preflight and completed Phase 0, then correctly STOPPED at P1-T1 because the plan created two new `UtilitiesCS/OutlookObjects/*.cs` files but `UtilitiesCS.csproj` was not in the scope-lock list and no `<Compile Include>` wiring was specified — the files could not build into `UtilitiesCS.dll`. The plan already did the equivalent for `TaskMaster.Test.csproj`, so it was a consistency gap.

**How to apply:** When a plan creates ANY new `.cs` file in a legacy/packages.config project, the plan MUST (a) list a `MODIFY <project>.csproj` entry in the scope-lock with the exact `<Compile Include>` item(s), and (b) fold the csproj wiring into the file-creation task's single binary outcome (file exists AND is wired into the csproj so it compiles). Verify a project is legacy by checking for `packages.config` and explicit `<Compile Include>` items rather than SDK-style globbing.

**Anti-pattern — the batched csproj task.** Do NOT collect a phase's `<Compile Include>` additions into one later "add all entries" task. Every task between the creating task and the batch task that asserts "the file compiles" is then unsatisfiable, because the file is not in the compilation until its entry lands. Caught in #455 F13 preflight: five tasks across four phases (`[P1-T1]`, `[P1-T3]`, `[P2-T1]`, `[P3-T1]`, `[P4-T2]`) each asserted compilation before the batch task ran. The fix is to fold each entry into its own creating task and demote the former batch task to a **verification** task (entry count, CRLF line-count delta, no unrelated entry moved). A file *move* is the worst case: it breaks the project build outright until the old path's entry is removed.

## Third failure mode: "the file compiles" mid-refactor

A "compiles" acceptance is unsatisfiable for any task in the middle of a multi-task extraction, even with correct csproj wiring, whenever an earlier task deleted a member whose call site a later task rebinds. In #455 Phase 1, `[P1-T2]` deleted `BreadcrumbPopupUiOperations.ShowOwnedPopup` but `[P1-T4]` rebinds its only call site, so nothing between them compiles.

**How to apply:** Reserve build-bearing acceptances for the phase's dedicated msbuild task. For intermediate refactor steps use content-based acceptance (symbol absent, symbol present, attribute count) and add an explicit clause naming the task that first exercises a compiling build.

## Second failure mode: transitive assembly references do not flow

In a legacy non-SDK project, a `<ProjectReference>` does NOT flow the referenced project's assembly references to the compiler — they land in `ReferenceDependencyPaths` (copy-local at runtime) rather than `ReferencePath` (compile-time). A test that merely names a type from a transitively-referenced package fails to compile with `CS0012`.

**Why:** Caught during #418 preflight. `SVGControl.Test.csproj` had a `ProjectReference` to `SVGControl` but no `<Reference Include="Svg" ...>` and no `Svg` entry in its own `packages.config`; every planned test named `SvgDocument`, so the whole test file would have failed with `CS0012` for `Svg, Version=3.4.0.0`.

**How to apply:** Before planning tests in a legacy test project, grep that project's own `.csproj` `<Reference>` block and `packages.config` for every third-party type the tests will name. If a type comes only from the production project's package set, add an explicit task that adds BOTH the `packages.config` `<package id=... />` entry and the `<Reference Include=... ><HintPath>..\packages\<id>.<ver>\lib\<tfm>\<dll>.dll</HintPath></Reference>` item, copying the exact `Version=`/`PublicKeyToken=` from the production project's csproj. Widen the scope-lock entry for that csproj to permit `<Reference>` items, and add the `packages.config` unconditionally.
