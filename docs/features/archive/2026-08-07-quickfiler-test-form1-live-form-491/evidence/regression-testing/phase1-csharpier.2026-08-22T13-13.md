Timestamp: 2026-08-22T13-13
Command: dotnet tool run csharpier format .; dotnet tool run csharpier check .
EXIT_CODE: 0
Output Summary: `format`: "Formatted 1518 files in 6864ms." `check`: "Checked 1518 files in 6215ms." (EXIT_CODE: 0). `git status --porcelain -- QuickFiler.Test` after the format pass shows only the two expected paths (`M QuickFiler.Test/QuickFiler.Test.csproj`, `?? QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs`); CSharpier did not need to rewrite the new guard test file's content beyond what was already written (its own formatting already matched CSharpier's output), and `.csharpierignore` excludes `*.csproj` so the csproj edit is invisible to the check.
