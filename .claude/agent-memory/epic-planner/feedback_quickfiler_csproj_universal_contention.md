---
name: quickfiler-csproj-universal-contention
description: Legacy non-SDK csproj files make every child that adds or removes a file contend on the same project file; partition the file by region in the manifest instead of adding dependency edges
metadata:
  type: feedback
---

Every C# project in TaskMaster is a legacy non-SDK project with explicit `<Compile Include>` items
(`QuickFiler.csproj` 125 entries, `QuickFiler.Test.csproj` 116, measured 2026-08-21). Any child that
adds a regression-test file, or deletes a production file, must edit the project file. Since the
Bugfix Workflow in `CLAUDE.md` requires a failing regression test first, that is almost every child.

This is the universal contention surface that made a parallel-surface attempt over the same corpus
produce 51 cohorts for 72 items at maximum width 6. It cannot be configured away.

**Do not model it as `depends_on`.** A dependency edge costs a whole wave and still leaves the
conflict possible. Instead:

1. **Prefer an existing test file.** Check for one first — a test added to a file that already
   carries a compile entry needs no project-file edit at all. In the 2026-08-21 foundation epic this
   removed the conflict for 2 of 4 children outright (`WinFormsPumpHostTests.cs`,
   `QfcItemController.InitializationTests.Part3.cs`, `KaCharTests.cs`, `KbdActionsTests.cs` all
   already existed).
2. **Partition the file by region in the epic manifest**, naming the exact line range each child
   owns and stating that the other regions belong to siblings. This is the same technique that let
   two Lane A children edit `CLAUDE.md` in the same epic without an edge.
3. **Only if two children must both insert** into the same item group, put them in different waves.

**Why:** a child told only "you may need to edit the csproj" will insert at whatever line its tool
picks, and two siblings inserting into the same `ItemGroup` conflict at fan-in — where `epic-plan`
requires you to halt rather than resolve ad hoc, because a conflict is supposed to mean a
decomposition defect. Region partition makes the disjointness explicit and auditable.

**How to apply:** during decomposition, for each candidate child, run
`ls <TestProject>/<Area>/*Tests.cs` and `grep -n "<TargetFile>" <Project>.csproj`. Record per child
in the manifest under a "Shared-Surface Coordination" heading whether it edits the project file and
which region it owns.

Related: [[quickfiler-potential-docs-stranded-on-stale-epic-branch]],
[[governance-doc-edits-need-execution-authorization]].
