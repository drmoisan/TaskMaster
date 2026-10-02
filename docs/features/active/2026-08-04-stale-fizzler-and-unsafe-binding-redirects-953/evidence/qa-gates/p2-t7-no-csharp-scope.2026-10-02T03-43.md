# P2-T7 No-C#-toolchain scope proof

Timestamp: 2026-10-02T03-43
Command: git -C <execution-worktree-root> diff --name-only 860d67bf4fddecb929e0d6c166065fd1ee752feb -- '*.cs' '*.csproj' '*.sln' '*.props' '*.targets' '*packages.config'; git -C <execution-worktree-root> status --porcelain -- '*.cs' '*.csproj' '*.sln' '*.props' '*.targets' '*packages.config'
EXIT_CODE: 0

Observations:

- BASE_SHA-anchored diff over the six C# pathspecs: printed nothing. The BASE_SHA-anchored diff is the discriminating observation because Phases 0 and 1 are committed and pushed, so porcelain is empty for committed paths.
- Porcelain over the same six pathspecs: printed nothing.
- Pathspec discrimination control: the same BASE_SHA-anchored diff with the pathspec `'*/app.config'` printed the 11 Write Set config paths (QuickFiler.Test, QuickFiler, SVGControl.Test, Tags, TaskMaster, TaskTree, TaskVisualization.Test, TaskVisualization, ToDoModel.Test, ToDoModel, UtilitiesCS.Test), so the wildcard pathspec form reaches nested project directories and the empty result is not a pathspec artifact.
- An earlier unquoted form of the same two commands (shell-expanded pathspecs) also printed nothing; the quoted form above is the one recorded.

CSHARP-TOOLCHAIN: not applicable; no C# source, project, solution or manifest file changed (D11)

Acceptance: both captures empty; the scope statement is present.

Output Summary: No *.cs, *.csproj, *.sln, *.props, *.targets or packages.config path differs from BASE_SHA or appears in porcelain; the app.config pathspec control returned the 11 expected paths.
