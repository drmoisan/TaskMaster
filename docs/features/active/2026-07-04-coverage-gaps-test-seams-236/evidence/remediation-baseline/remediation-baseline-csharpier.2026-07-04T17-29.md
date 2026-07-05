Timestamp: 2026-07-04T17:36:47-04:00
Command: dotnet tool restore; dotnet tool run csharpier --check .; dotnet tool run csharpier check .
EXIT_CODE: 0
Output Summary:
- `dotnet tool restore` passed with exit code 0.
- The exact planned command `dotnet tool run csharpier --check .` was executed and failed with exit code 1 because CSharpier 1.2.6 expects the `check` subcommand.
- Compatibility check `dotnet tool run csharpier check .` passed with exit code 0.
- Formatter baseline signal: clean. CSharpier checked 1252 files and reported no formatting changes required.

Exact planned command output:
```text
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
'--check' was not matched. Did you mean one of the following?
check
'.' was not matched. Did you mean one of the following?
-h
Description:

Usage:
  CSharpier [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  format <directoryOrFile>  Format files.
  check <directoryOrFile>   Check that files are formatted. Will not write any changes.
  pipe-files                Keep csharpier running so that multiples files can be piped to it via stdin.
  server                    Run CSharpier as a server so that multiple files may be formatted.

RESTORE_EXIT_CODE: 0
CSHARPIER_EXIT_CODE: 1
START: 2026-07-04T17:36:34.2593809-04:00
END: 2026-07-04T17:36:34.5974498-04:00
Required command was not provided.
Unrecognized command or argument '--check'.
Unrecognized command or argument '.'.
```

Compatibility check output:
```text
Checked 1252 files in 3417ms.
CSHARPIER_COMPAT_CHECK_EXIT_CODE: 0
START: 2026-07-04T17:36:43.4976848-04:00
END: 2026-07-04T17:36:47.1090541-04:00
```
