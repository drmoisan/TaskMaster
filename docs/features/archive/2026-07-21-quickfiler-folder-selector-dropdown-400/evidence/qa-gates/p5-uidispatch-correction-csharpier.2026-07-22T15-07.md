# P5-T178 — CSharpier format + scoped check for the Branch B UI-dispatch correction

Timestamp: 2026-07-22T15-07Z

Command: `cd "C:/Users/DanMoisan/repos/TaskMaster-wt/2026-07-21T10-25" && TOOL="/c/Users/DanMoisan/.dotnet/tools/csharpier.exe"; "$TOOL" --version; F=QuickFiler/Viewers/BreadcrumbUiDispatcher.cs; "$TOOL" format "$F"; echo "FORMAT_EXIT=$?"; "$TOOL" format "$F"; echo "FORMAT2_EXIT=$?"; "$TOOL" check "$F"; echo "CHECK_EXIT=$?"; wc -l "$F"; sha256sum "$F"; git diff --stat`

EXIT_CODE: 0

## Edited file set for the selected branch

`BRANCH: B` edited exactly one file: `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`. No test source and no project,
runsettings, coverage-config, threshold, filter, exclusion, or designer file was edited, so the format/check scope is
that single file.

## Result

- CSharpier version: `1.3.0`.
- `csharpier format QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` — `Formatted 1 files in 478ms.`, exit `0`.
- `csharpier format` repeated — `Formatted 1 files in 359ms.`, exit `0`, no further on-disk change (line count and
  content identical between passes).
- Authoritative `csharpier check QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` — `Checked 1 files in 317ms.`, exit `0`.
- `csharpier pipe-files` was **not** used at any point as a formatting or verification gate.

## Post-format physical line counts

| File | Lines | Limit | Post-format SHA-256 |
|---|---:|---:|---|
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | 285 | 480 | `0764d49c8747276722853bf30fe32aca133cb19a3d634a9cda351217fd49017e` |

Every touched file is at most 500 lines, and the dispatcher is at most 480. No test file was touched, so the 480-line
test cap on `BreadcrumbUiThreadDispatchTests.cs` (unchanged at 480 lines, SHA-256
`e4bd60150636a83ce977681249e03c63a2fc7ca96c32c5f8ef5bbb760926e62e`) is preserved by construction.

Output Summary: `csharpier format` (mutating, on-disk) then a second `format` pass produced no further change, and the
authoritative scoped `csharpier check` returned `EXIT_CODE: 0` for the single edited file
`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`. Post-format physical line count 285 (limit 480), SHA-256
`0764d49c...49017e`. `csharpier pipe-files` was not used. EXIT_CODE: 0.
