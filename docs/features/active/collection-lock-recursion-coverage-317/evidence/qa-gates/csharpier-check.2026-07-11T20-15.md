# CSharpier Format-Check (#317) — Phase 3, P3-T1

Timestamp: 2026-07-11T20-15

Command: `dotnet tool run csharpier check "UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs"`

## First attempt (loop restart trigger)

EXIT_CODE: 1

Output: `Error ... Was not formatted. The file contained different line endings than formatting it
would result in. Checked 1 files in 329ms.`

Remediation: ran `dotnet tool run csharpier format` on the same single-file path (508ms, 1 file
formatted), per the phase's own restart-on-file-change rule. The `.csproj` was untouched by
CSharpier (file-based formatter, does not touch project files).

## Second attempt (post-format, final)

Command: `dotnet tool run csharpier check "UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs"`

EXIT_CODE: 0

Output Summary: `Checked 1 files in 339ms.` Zero formatting diffs. Confirmed via `git diff --stat main`
that the diff scope remains exactly the two planned files after the CSharpier line-ending fix.
