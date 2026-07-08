# Baseline — CSharpier Formatting State (Issue #181)

Timestamp: 2026-06-08T12-27
Command: dotnet tool restore ; dotnet csharpier check .
EXIT_CODE: 1

Output Summary:
- dotnet tool restore: csharpier 1.2.6 restored, EXIT_CODE 0.
- dotnet csharpier check .: EXIT_CODE 1 (dirty). Checked 1056 files in ~2.1s.
- One pre-existing unformatted file at baseline: .\UtilitiesCS\Extensions\IEnumerableExtensions.cs (around line 132, a System.Threading.Timer lambda formatting difference).
- This is a PRE-EXISTING baseline condition not introduced by this plan. It is recorded as the format baseline reference. This plan does not modify .cs source files, so csharpier check is expected to remain at this same baseline state unless the unrelated file is reformatted by an explicit `dotnet csharpier .` run.
- NOTE: The plan's format toolchain step (`dotnet tool run csharpier .`) writes formatting in place. To preserve baseline scope (no .cs source changes) this plan uses the CI verification form `dotnet csharpier check .` for evidence and does not rewrite the unrelated pre-existing file. If a final-QA step requires a clean format pass, the single pre-existing file is the only delta and is out of this plan's scope.
