---
name: explicit-compile-items-decide-membership-not-file-presence
description: In legacy csproj projects with explicit Compile items and no glob, finding a .cs file proves nothing about whether it compiles; verify membership by locating a Compile item naming it.
metadata:
  type: project
---

In this repository's legacy `.csproj` projects (for example `UtilitiesCS.Test/UtilitiesCS.Test.csproj`),
every source file is listed by an explicit `<Compile Include="..." />` item and there is no wildcard
glob. File presence on disk is therefore not evidence of compilation. Two consequences show up
repeatedly in preflight:

1. **A search hit can be dead code.** `UtilitiesCS.Test` carries two root-level files with a
   method-level `[Ignore]` attribute (`InputBox_Test.cs`, `YesNoToAll_Test.cs`). Neither is named by a
   Compile item; only the `Dialogs\` copies are (project lines 430 and 436), and those carry no
   `[Ignore]`. A reviewer who greps for `[Ignore]` and stops there will wrongly conclude that a
   zero-skipped acceptance condition is unsatisfiable. This was raised and rejected as factually false
   on issue #872 preflight round 3.
2. **A type leaves the assembly when its Compile item is removed, not when the file is deleted.** In a
   delete sequence of "remove Compile item, then delete file", the downstream compile break begins at
   the item removal. Plan prose that dates a non-compiling span from the delete task under-reports
   where the span starts.

**Why:** explicit Compile items decouple on-disk presence from compilation membership, so the two
ordinary verification reflexes — grep the tree, check the file exists — both return answers about the
wrong thing.

**How to apply:** before asserting that any `.cs` file is compiled (or that a symbol in it is live),
grep the owning `.csproj` for a Compile item naming that exact relative path. See
[[project_analyzer_hintpath_skew_breaks_all_four_gates]] and
[[project_preflight_recurring_csharp_plan_defect_classes]].
