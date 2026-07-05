Timestamp: 2026-07-04T18-52
Command: dotnet tool restore; dotnet tool run csharpier --check .; supplemental: dotnet tool run csharpier check .
EXIT_CODE: 1
Output Summary:
- dotnet tool restore completed successfully and restored CSharpier 1.2.6.
- Planned command `dotnet tool run csharpier --check .` exited 1 because this CSharpier version expects the `check` subcommand instead of `--check`.
- Supplemental formatter check `dotnet tool run csharpier check .` exited 1.
- Formatter clean/dirty signal: dirty.
- CSharpier checked 1268 files and reported `testResults.xml` was not formatted because the file did not end with a single newline.
