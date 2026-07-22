# P5 Open Coordinator CSharpier Restart

Timestamp: 2026-07-22T09:47:18.4677296Z

Command: `$files=@(<the resolved J1 three-production/two-test tuple>); @($files) | & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' pipe-files`

EXIT_CODE: `0` on both passes

Output Summary: PASS. The P5-T118 C# 7.3 test-project compilation failure required an ordered restart. After removing only unsupported nullable syntax from the new test fixture, CSharpier ran twice on the exact J1 tuple and made no further change. This artifact supersedes the 09-46 formatter evidence.

The four unchanged tuple hashes remain as recorded at 09-46. `BreadcrumbDropDownOpenCoordinatorTests.cs` is now 395 lines with stable SHA-256 `7BA72D6CBCBC462136DF6C6D5072182CCBBF4BD09EDC8BC79CFF1008E0F6D98A`.
