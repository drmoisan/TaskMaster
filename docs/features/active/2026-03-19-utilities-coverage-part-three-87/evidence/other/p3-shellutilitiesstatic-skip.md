# P3-T9: Skip Re-Validation — ShellUtilitiesStatic.cs

## File
`UtilitiesCS\HelperClasses\FileSystem\ShellUtilitiesStatic.cs`

## Current Coverage
`line-rate="0.3333"` (~33.3%) — companion shell tests already exist in `UtilitiesCS.Test\HelperClasses\ShellUtilities_Tests.cs`.

## Source Analysis
`ShellUtilitiesStatic` mirrors the instance-based shell wrapper and exposes additional testable branches in `GetFileType`, `GetFileIcon`, and `GetSysImageIndex`.

## Revalidation Result
Because the Windows shell P/Invoke surface is already being exercised successfully by existing tests, the remaining uncovered logic should be revisited instead of skipped. The file is not blocked by unavailable infrastructure.

## Decision: Return To Implementation
