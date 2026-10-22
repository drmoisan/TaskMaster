# [P15-T2] Final QA loop, step 1 verification — repository-wide CSharpier check

Timestamp: 2026-08-26T16-44

Command:

```
pwsh -NoProfile -Command "Set-Location <repo-root>; dotnet tool run csharpier check ."
```

`dotnet tool run` is used so the manifest-pinned CSharpier 1.2.6 from `dotnet-tools.json` is the
version that runs, matching `.github/workflows/ci.yml`. A globally installed CSharpier of a different
version would produce diffs that disagree with CI.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

```
Checked 1530 files in 5853ms.
```

**Zero files reported as needing formatting.** CSharpier `check` prints one line per unformatted file
and exits non-zero when any exist; the output contains no such line and the exit code is `0`.

This is a repository-wide check, not a scoped one. Unlike P15-T1 — which formats only the owned file
set, because a bare `.` would rewrite the 39 sibling-derived files the integration merges brought in
— the read-only `check` can safely cover the whole tree, and covering the whole tree is what gives the
result its value: it confirms that this branch, including the merged sibling work, is formatter-clean
as a whole and will pass CI's format gate.

## Scope reconciliation, 1,520 to 1,530

The P0-T11 baseline reported `Checked 1520 files`. This run reports `Checked 1530 files`. The
ten-file increase is exactly the ten `.cs` files added across the whole `61edc19b..HEAD` range, and no
file left the check scope:

| Added `.cs` file | Source |
|---|---|
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` | this feature |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | this feature |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | this feature |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs` | this feature |
| `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs` | this feature |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs` | sibling merge |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs` | sibling merge |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | sibling merge |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | sibling merge |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` | sibling merge |

1,520 + 10 = 1,530. The five `.cobertura.xml` files also added in the range do not enter the count:
`.csharpierignore` excludes `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`,
`*.trx`, `*.csproj`, `*.props`, and `*.targets`. The arithmetic closing exactly on the `.cs` additions
alone is the confirmation that those exclusions are in force.

## Acceptance verification

| Clause | Status |
|---|---|
| `EXIT_CODE: 0` | met |
| zero files reported as needing formatting | met — the output is a single summary line with no per-file report |
