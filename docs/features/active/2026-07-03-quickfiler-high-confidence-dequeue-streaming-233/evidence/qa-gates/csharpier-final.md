Timestamp: 2026-07-03T17:48:00.4361061-04:00
Command: dotnet tool run csharpier .
EXIT_CODE: 1
Output Summary:
- CSharpier did not run formatting.
- The installed CSharpier command shape requires a subcommand.
- Error excerpt:
  - `'.' was not matched. Did you mean one of the following?`
  - `Commands:`
  - `format <directoryOrFile>  Format files.`
  - `check <directoryOrFile>   Check that files are formatted. Will not write any changes.`
  - `Required command was not provided.`
  - `Unrecognized command or argument '.'.`
- Result: QA formatter gate failed for the exact command required by the plan.
