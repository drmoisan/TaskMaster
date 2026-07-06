Timestamp: 2026-07-06T11-18-04:00
Issue: #243
Command: csharpier .
EXIT_CODE: 1
Output Summary: FAIL. The installed CSharpier CLI rejected `csharpier .` because it requires a command such as `format <directoryOrFile>` or `check <directoryOrFile>`. No formatting changes were observed in `git status --short --untracked-files=all` after the command; only existing Phase 0 feature-folder artifacts were listed as untracked.

Primary Output:
```
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

Required command was not provided.
Unrecognized command or argument '.'.
```
