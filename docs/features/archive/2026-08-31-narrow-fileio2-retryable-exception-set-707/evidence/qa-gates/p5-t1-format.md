Timestamp: 2026-09-03T13-30
Iteration: 1

Command 1 (before hash): Get-FileHash -Algorithm SHA256 -LiteralPath "UtilitiesCS/To Depricate/FileIO2.cs"
Command 2 (before hash): Get-FileHash -Algorithm SHA256 -LiteralPath "UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs"
Command 3 (format): dotnet tool run csharpier format "UtilitiesCS/To Depricate/FileIO2.cs" "UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs"
Command 4 (after hash): Get-FileHash -Algorithm SHA256 -LiteralPath "UtilitiesCS/To Depricate/FileIO2.cs"
Command 5 (after hash): Get-FileHash -Algorithm SHA256 -LiteralPath "UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs"
EXIT_CODE: 0

FileIO2.cs SHA-256 before: C47EE8EFDDD2FDB0A39088491BCA8FD8AA326263525181456B115A05040C839A
FileIO2.cs SHA-256 after:  C47EE8EFDDD2FDB0A39088491BCA8FD8AA326263525181456B115A05040C839A (unchanged)
FileIO2_Tests.cs SHA-256 before: 9B8547FEA7D466467A7A0ADA4E6EFAA0F2207F82F9B7D417F0B615C0AB8D90CD
FileIO2_Tests.cs SHA-256 after:  9B8547FEA7D466467A7A0ADA4E6EFAA0F2207F82F9B7D417F0B615C0AB8D90CD (unchanged)

Console output: "Formatted 2 files in 1613ms."

Output Summary: `EXIT_CODE: 0`. Literal console line "Formatted 2 files in 1613ms." recorded (processed-file count, not a rewrite indicator per CSharpier 1.2.6 behavior — both files were already correctly formatted, hashes identical before/after).
