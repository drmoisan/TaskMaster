# P0-T8 no-reader search on origin/main

Timestamp: 2026-10-01T12-11
Command: git grep -n -I -F ".bak" origin/main -- . ":(exclude)docs/features" ":(exclude).claude/agent-memory"
EXIT_CODE: 0
Output Summary: Exactly two hits, one in `.gitignore` (the `*.rptproj.bak` rule) and one in `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` (the unrelated string literal `file-backup.bak`).

```
origin/main:.gitignore:257:*.rptproj.bak
origin/main:UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs:412:            var backupTarget = MissingOwnedPath("file-backup.bak");
```
