# Phase 7 — AC13 compile-entry audit for the six new source files

Timestamp: 2026-09-07T03-21
Task: [P7-T3]
Issue: #798

Host-specific absolute paths are redacted to a `<worktree>` token. Each search was a literal,
case-sensitive, single-file search rooted at `<worktree>`.

## Result

Each of the six new `.cs` files — two production and four test — has exactly one `<Compile Include>`
entry, and each entry sits in the project file that owns the source file.

| # | Literal searched | Project file searched | Matches | Line |
|---|---|---|---|---|
| 1 | `<Compile Include="Extensions\DfDeedle.QfcColumns.cs" />` | `UtilitiesCS/UtilitiesCS.csproj` | 1 | 994 |
| 2 | `<Compile Include="Ribbon\RibbonCommandBoundary.cs" />` | `TaskMaster/TaskMaster.csproj` | 1 | 462 |
| 3 | `<Compile Include="Extensions\DfDeedleQfcColumnTimeoutTests.cs" />` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 1 | 190 |
| 4 | `<Compile Include="Extensions\DfDeedleRequiredColumnValidationTests.cs" />` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 1 | 191 |
| 5 | `<Compile Include="Ribbon\RibbonCommandBoundaryTests.cs" />` | `TaskMaster.Test/TaskMaster.Test.csproj` | 1 | 324 |
| 6 | `<Compile Include="Controllers\QfcDatamodelRethrowTests.cs" />` | `QuickFiler.Test/QuickFiler.Test.csproj` | 1 | 147 |

Matched lines, verbatim:

```
UtilitiesCS/UtilitiesCS.csproj:994:    <Compile Include="Extensions\DfDeedle.QfcColumns.cs" />
TaskMaster/TaskMaster.csproj:462:    <Compile Include="Ribbon\RibbonCommandBoundary.cs" />
UtilitiesCS.Test/UtilitiesCS.Test.csproj:190:    <Compile Include="Extensions\DfDeedleQfcColumnTimeoutTests.cs" />
UtilitiesCS.Test/UtilitiesCS.Test.csproj:191:    <Compile Include="Extensions\DfDeedleRequiredColumnValidationTests.cs" />
TaskMaster.Test/TaskMaster.Test.csproj:324:    <Compile Include="Ribbon\RibbonCommandBoundaryTests.cs" />
QuickFiler.Test/QuickFiler.Test.csproj:147:    <Compile Include="Controllers\QfcDatamodelRethrowTests.cs" />
```

Note on separators: these projects are legacy non-SDK-style `.csproj` files whose `Include`
attributes use backslash separators. A forward-slash spelling matches no entry in any of these five
project files, so the backslash form recorded above is the only form that can be asserted.

Output Summary: All six required `<Compile Include>` literals were found, one match each, each in the
project file that owns its source file. Two entries sit in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
because that project owns two of the four new test files; the remaining four entries are distributed
one per project across `UtilitiesCS/UtilitiesCS.csproj`, `TaskMaster/TaskMaster.csproj`,
`TaskMaster.Test/TaskMaster.Test.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj`. No new source
file is missing a compile entry and no entry is duplicated.
