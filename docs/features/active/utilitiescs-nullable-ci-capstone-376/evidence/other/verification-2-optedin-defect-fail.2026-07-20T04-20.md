# P4-T3 [expect-fail] — Verification 2: Opted-In Defect Fails the Gate

Timestamp: 2026-07-20T04-20

## Defect introduced

`UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` (opted-in candidate, `#nullable
enable` at line 1), inside `FormatPercent`:

```csharp
string? maybeNull = null;
int len = maybeNull.Length;
```

Command: `MSBuild.exe TaskMaster.sln -t:Rebuild -p:Configuration=Debug "-p:Platform=Any CPU" -p:TreatWarningsAsErrors=true`

EXIT_CODE: 1 (non-zero, expected outcome for this task)

Output Summary: Build failed with the following diagnostic:

```
UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs(34,23): error CS8602: Dereference of a possibly null reference. [UtilitiesCS/UtilitiesCS.csproj]
```

This confirms the gate genuinely enforces nullable warnings-as-errors for an opted-in file: the
deliberately-introduced null-dereference on a local declared `string?` fails the build with
CS8602, exactly as expected.
