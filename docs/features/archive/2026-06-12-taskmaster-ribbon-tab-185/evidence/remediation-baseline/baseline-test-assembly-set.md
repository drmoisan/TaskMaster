# Phase 0 — Repo-Wide Test Assembly Set (Issue #185)

Timestamp: 2026-06-12T11-16

Command: `find . -type f -ipath '*/bin/Debug/*' -name '*.Test.dll' | grep -viE '/(obj|ref)/' | sort`

EXIT_CODE: 0

Output Summary: Seven first-party `*.Test.dll` assemblies resolved under `bin/Debug` (excluding `obj` and `ref` paths). These are the instrumentation targets for the repository-wide coverage run (P1-T1 / P2-T4).

Resolved assembly paths (relative to repo root):
1. QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
2. Tags.Test/bin/Debug/Tags.Test.dll
3. TaskMaster.Test/bin/Debug/TaskMaster.Test.dll
4. TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll
5. ToDoModel.Test/bin/Debug/ToDoModel.Test.dll
6. UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll
7. VBFunctions.Test/bin/Debug/VBFunctions.Test.dll

Vendored test projects note: `SVGControl.Test` and `UtilitiesSwordfish.Test` directories exist but do not produce a `*.Test.dll` matching the enumeration pattern (SVGControl.Test has no built bin/Debug DLL output; UtilitiesSwordfish.Test's bin/Debug contains `Swordfish.NET.General.dll` and `Newtonsoft.Json.dll`, not a `*.Test.dll`). They are excluded both by the file-pattern enumeration and by the vendored-exclusion policy in .claude/rules/csharp.md.
