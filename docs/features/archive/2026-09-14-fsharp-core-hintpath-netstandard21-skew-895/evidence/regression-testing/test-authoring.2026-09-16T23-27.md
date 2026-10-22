# Phase 1 — Static Wiring and Shape Check (Issue #895)

Timestamp: 2026-09-17T01-19
Task: [P1-T4]
WORKTREE-LEAF: agent-a8bc4dc5978785885

Covers `[P1-T1]` (Shape A), `[P1-T2]` (Shape B) and `[P1-T3]` (project-file registration).

Command: the `[P1-T4]` payload, run inside a WT-PREAMBLE `pwsh -NoProfile -Command` payload. The
`git status` span is the companion the tracked-only numstat needs, because the two new files are
untracked until `[P1-T8]`.

EXIT_CODE: 0
ExpectedExitCode: 0

## Output Summary:

```
A [TestMethod] COUNT=2
A SolutionHasExactlySixFSharpCoreHintPaths COUNT=1
A EveryFSharpCoreHintPath_SelectsNetstandard20 COUNT=1
A internal static COUNT=2
A #nullable COUNT=0
A DoNotParallelize COUNT=0
A [DataRow( COUNT=0
B [DataTestMethod] COUNT=1
B [DataRow( COUNT=15
B DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [ COUNT=15
B [TestMethod] COUNT=1
B Detector_OnPackageNetstandard21Binary_Reports21 COUNT=1
B PEReader COUNT=1
B #nullable COUNT=0
B DoNotParallelize COUNT=0
P Compile Include="Bootstrap\FSharpCoreHintPathAlignmentTests.cs" COUNT=1
P Compile Include="Bootstrap\FSharpCoreDeployedIdentityTests.cs" COUNT=1
A LINES=211
B LINES=244
BOOTSTRAP_CS_FILES=5
2	0	TaskMaster.Test/TaskMaster.Test.csproj
 M TaskMaster.Test/TaskMaster.Test.csproj
?? TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs
?? TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs
```

## Acceptance

Every clause holds:

- `A [TestMethod] COUNT=2`, `A SolutionHasExactlySixFSharpCoreHintPaths COUNT=1`,
  `A EveryFSharpCoreHintPath_SelectsNetstandard20 COUNT=1`, `A internal static COUNT=2`,
  `A #nullable COUNT=0`, `A DoNotParallelize COUNT=0`, `A [DataRow( COUNT=0`.
- `B [DataTestMethod] COUNT=1`, `B [DataRow( COUNT=15`, the `B DisplayName` count 15,
  `B [TestMethod] COUNT=1`, `B Detector_OnPackageNetstandard21Binary_Reports21 COUNT=1`,
  `B PEReader COUNT=1` (at least 1), `B #nullable COUNT=0`, `B DoNotParallelize COUNT=0`.
- Both `P` counts are 1.
- `A LINES=211` and `B LINES=244`, each at most 500.
- `BOOTSTRAP_CS_FILES=5`, up from the `[P0-T9]` baseline of 3.
- The numstat line reads `2	0	TaskMaster.Test/TaskMaster.Test.csproj`: two insertions, zero
  deletions, so no existing item in that project file was altered.
- The porcelain span lists exactly three paths: the two new files as `??` and the project file as
  ` M`.

## Note on formatting

Both new files were formatted with the manifest-pinned CSharpier before this measurement, so the
shape recorded above is the formatted shape. `dotnet tool run csharpier format` reported
`Formatted 2 files in 2168ms.` and the read-only follow-up `dotnet tool run csharpier check`
reported `Checked 2 files in 749ms.` with exit 0. The formatter split the longer `[DataRow]`
attributes across lines; the `[DataRow(` and `DisplayName` counts are unchanged at 15 each because
the split occurs at an argument boundary and never inside a string literal. The authoritative
repository-wide format gate remains `[P4-T5]`, which measures the three files by content hash.

## Note on the Shape A failure message

`[P1-T6]` asserts that the failure message for `EveryFSharpCoreHintPath_SelectsNetstandard20`
carries all three offending project-file names. The assertion library renders a non-empty
collection failure by naming one representative element only, so the offending entries are
projected into the reason argument by the test itself rather than left to the renderer. The
resulting message is recorded verbatim in the `[P1-T6]` artifact, which is the observation that
decides the gate.
