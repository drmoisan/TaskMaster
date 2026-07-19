# Final QC — Solution-Wide Per-File Nullable Pragma Gate

- Timestamp: 2026-07-19T12-35
- Task: [P7-T3]

## Command 1 (plan-mandated, solution-wide)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`)
- EXIT_CODE: 1
- CS86xx count: 0

Output Summary: Aborts on 2 pre-existing vendored SVGControl CS0649 errors (a non-nullable warning
promoted by `TreatWarningsAsErrors`), which short-circuit the solution build before `UtilitiesCS`
compiles. Zero CS86xx emitted anywhere. `/p:Nullable=enable` was NOT passed. This is the documented,
pre-existing, out-of-scope blocker (unrelated to issue #374).

## Command 2 (authoritative CS86xx detector, scoped isolated build)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- CS86xx count: 0

Output Summary: The scoped isolated build compiles the entire `UtilitiesCS` cluster (all 14 opted-in
files) past the SVGControl short-circuit, excluding only the pre-existing non-nullable codes
CS0649/CS0618/CS0168. Result: `0 Error(s)`, **zero CS86xx** across all 14 remediated files under the
per-file pragma with `TreatWarningsAsErrors`. AC1 satisfied for the full cluster.

## Pragma presence verification

All 14 in-scope files carry exactly one `#nullable enable` pragma (grep `-c` = 1 each):
DelegateButtonTemplate, FolderNotFoundViewer, MyBoxViewer, InputBoxViewer, ActionButton,
DelegateButton, FunctionButton, InputBox, NotImplementedDialog, MyBox, MyBoxModeless, YesNoToAll,
ExtraDeclarations, AssemblyInfo.

## Note

The solution-wide command's inability to reach the cluster (Command 1 EXIT 1) is a pre-existing,
out-of-scope vendored blocker, not an AC1 failure. AC1 is judged on the CS86xx count (zero),
established authoritatively by Command 2. Flagged for the maintainer/epic-planner; no `.claude/rules/*`
file was edited.
