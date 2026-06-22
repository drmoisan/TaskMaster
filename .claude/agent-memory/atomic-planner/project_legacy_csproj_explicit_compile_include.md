---
name: legacy-csproj-explicit-compile-include
description: Legacy non-SDK / packages.config C# projects in this repo list every source via explicit <Compile Include> (no glob); new .cs files need csproj wiring in plan scope-lock + task AC
metadata:
  type: project
---

Legacy non-SDK / packages.config C# projects in this repo (confirmed: `UtilitiesCS/UtilitiesCS.csproj`, `TaskMaster.Test/TaskMaster.Test.csproj`) enumerate every source file via explicit `<Compile Include="..." />` items with NO wildcard glob. A new `.cs` file added to such a project will NOT compile into the assembly unless a matching `<Compile Include>` item is added.

**Why:** Caught during #207 planning. Executor passed preflight and completed Phase 0, then correctly STOPPED at P1-T1 because the plan created two new `UtilitiesCS/OutlookObjects/*.cs` files but `UtilitiesCS.csproj` was not in the scope-lock list and no `<Compile Include>` wiring was specified — the files could not build into `UtilitiesCS.dll`. The plan already did the equivalent for `TaskMaster.Test.csproj`, so it was a consistency gap.

**How to apply:** When a plan creates ANY new `.cs` file in a legacy/packages.config project, the plan MUST (a) list a `MODIFY <project>.csproj` entry in the scope-lock with the exact `<Compile Include>` item(s), and (b) fold the csproj wiring into the file-creation task's single binary outcome (file exists AND is wired into the csproj so it compiles). Verify a project is legacy by checking for `packages.config` and explicit `<Compile Include>` items rather than SDK-style globbing.
