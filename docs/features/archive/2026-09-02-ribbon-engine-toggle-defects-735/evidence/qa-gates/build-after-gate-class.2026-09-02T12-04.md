# Finding 2 — Build After the Gate Class (P2-T3)

Timestamp: 2026-09-03T02-02
Task: [P2-T3]
Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

## Trailing counts

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.34
```

Five warnings, unchanged from the P0-T6 and P0-T7 baselines (the System.Reactive `packages.config`
advisory, one per consuming project). Zero errors.

## The compile-item registration took effect

`TaskMaster/TaskMaster.csproj` is a legacy non-SDK project that lists every source file explicitly,
so a file absent from the item group is silently not compiled and a green build would prove nothing
about it. The build log was searched for the registered item path.

Search: `Ribbon\SpamManagerResetGate.cs` (literal, case-sensitive) over the normal-verbosity
MSBuild file log.
Hits: **2**.

Both hits are inside the `CoreCompile` step for `TaskMaster.csproj`: the file appears in the source
list passed on the `csc.exe` command line the log records at that step. That is direct evidence the
compiler received the file, rather than an inference from the build having succeeded.

## Registration edit

`git diff --numstat -- TaskMaster/TaskMaster.csproj` reports `1  0` — exactly one line added and
none removed, which also confirms the file's CRLF line endings were preserved (a line-ending change
would have re-written every line and shown the whole file as modified).

The added line is:

```
    <Compile Include="Ribbon\SpamManagerResetGate.cs" />
```

placed in alphabetical position inside the existing ribbon item group, immediately after the
`Ribbon\RibbonViewer.EngineCommands.cs` entry and immediately before the
`Ribbon\TryFunctionalityInConstruction.cs` entry. The project file is excluded from the formatter by
`.csharpierignore`, so no format pass is required for it.

Output Summary: Build succeeded with EXIT_CODE 0, 5 warnings and 0 errors. The new gate file appears
twice in the build log as an input on the `csc.exe` command line for `TaskMaster.csproj`, proving
the compile-item registration took effect. The project-file edit is a single added line.
