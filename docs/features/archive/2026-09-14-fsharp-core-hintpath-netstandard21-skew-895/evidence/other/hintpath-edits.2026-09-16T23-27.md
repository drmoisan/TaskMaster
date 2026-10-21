# Phase 2 — HintPath Edits Verified (Issue #895)

Timestamp: 2026-09-17T01-22
Task: [P2-T4]
WORKTREE-LEAF: agent-a8bc4dc5978785885

Covers `[P2-T1]` (`QuickFiler/QuickFiler.csproj` line 52), `[P2-T2]`
(`QuickFiler.Test/QuickFiler.Test.csproj` line 259) and `[P2-T3]` (`ToDoModel/ToDoModel.csproj`
line 42). This is the pre-commit measurement of AC3's edited-file clause; `[P4-T14]` is the
post-commit confirming run.

`origin/main` was re-fetched at the Phase 1 to Phase 2 boundary and resolves to
`91746d2e4776a59ee1db1856c5c490a009c4958b`, unchanged from the `[P0-T8]` capture.

Commands (inside a WT-PREAMBLE payload):

```
git diff --numstat origin/main -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj ToDoModel/ToDoModel.csproj UtilitiesCS/UtilitiesCS.csproj UtilitiesCS.Test/UtilitiesCS.Test.csproj ToDoModel.Test/ToDoModel.Test.csproj
git diff -U0 origin/main -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj ToDoModel/ToDoModel.csproj
```

EXIT_CODE: 0
ExpectedExitCode: 0

## Output Summary:

Six-file census, re-run from `[P0-T9]`:

```
QuickFiler/QuickFiler.csproj NS21=0 NS20=1
QuickFiler.Test/QuickFiler.Test.csproj NS21=0 NS20=1
ToDoModel/ToDoModel.csproj NS21=0 NS20=1
UtilitiesCS/UtilitiesCS.csproj NS21=0 NS20=1
UtilitiesCS.Test/UtilitiesCS.Test.csproj NS21=0 NS20=1
ToDoModel.Test/ToDoModel.Test.csproj NS21=0 NS20=1
```

Numstat, verbatim:

```
1	1	QuickFiler.Test/QuickFiler.Test.csproj
1	1	QuickFiler/QuickFiler.csproj
1	1	ToDoModel/ToDoModel.csproj
```

`-U0` hunks, verbatim:

```
diff --git a/QuickFiler.Test/QuickFiler.Test.csproj b/QuickFiler.Test/QuickFiler.Test.csproj
index be567fa20..cc2abb610 100644
--- a/QuickFiler.Test/QuickFiler.Test.csproj
+++ b/QuickFiler.Test/QuickFiler.Test.csproj
@@ -259 +259 @@
-      <HintPath>..\packages\FSharp.Core.11.0.100\lib\netstandard2.1\FSharp.Core.dll</HintPath>
+      <HintPath>..\packages\FSharp.Core.11.0.100\lib\netstandard2.0\FSharp.Core.dll</HintPath>
diff --git a/QuickFiler/QuickFiler.csproj b/QuickFiler/QuickFiler.csproj
index 4bc114f7e..e9bbea7f5 100644
--- a/QuickFiler/QuickFiler.csproj
+++ b/QuickFiler/QuickFiler.csproj
@@ -52 +52 @@
-      <HintPath>..\packages\FSharp.Core.11.0.100\lib\netstandard2.1\FSharp.Core.dll</HintPath>
+      <HintPath>..\packages\FSharp.Core.11.0.100\lib\netstandard2.0\FSharp.Core.dll</HintPath>
diff --git a/ToDoModel/ToDoModel.csproj b/ToDoModel/ToDoModel.csproj
index 3ae250ccc..ad6d6fcce 100644
--- a/ToDoModel/ToDoModel.csproj
+++ b/ToDoModel/ToDoModel.csproj
@@ -42 +42 @@
-      <HintPath>..\packages\FSharp.Core.11.0.100\lib\netstandard2.1\FSharp.Core.dll</HintPath>
+      <HintPath>..\packages\FSharp.Core.11.0.100\lib\netstandard2.0\FSharp.Core.dll</HintPath>
```

## Acceptance

- All six census lines read `NS21=0 NS20=1`: yes. Every `FSharp.Core` HintPath in the solution now
  selects the netstandard2.0 flavour, and each of the six files still declares exactly one.
- The numstat output is exactly three lines, one insertion and one deletion each, for
  `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj` and
  `ToDoModel/ToDoModel.csproj`: yes. The order git prints them in (`QuickFiler.Test` before
  `QuickFiler`, because `.` sorts before `/`) is git's path ordering, not a plan expectation.
- No line appears for `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  or `ToDoModel.Test/ToDoModel.Test.csproj`: yes, all three are byte-identical to `origin/main`.
- In the `-U0` output every `-` content line contains `lib\netstandard2.1\FSharp.Core.dll` and every
  `+` content line contains `lib\netstandard2.0\FSharp.Core.dll`, three of each: yes.
- The hunk headers `@@ -52 +52 @@`, `@@ -259 +259 @@` and `@@ -42 +42 @@` confirm the edits landed on
  the lines the plan names and that each file changed on exactly one line, with no reordering,
  renumbering or reformatting of any other item.
- The recorded diff carries repository-relative paths only; no absolute host path appears.
