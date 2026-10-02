# P4-T1 Repository-wide CSharpier format

Timestamp: 2026-10-01T21-18
ITERATION: 1
Command: dotnet tool run csharpier format . (CMD-FORMAT-REPO payload, TASKID p4-t1; Write Set hashes and `git status --porcelain --untracked-files=all` taken before and after; console stream teed to coverage\logs\p4-t1.csharpier-format.log, git-ignored)
EXIT_CODE: 0
Output Summary:
Formatter summary line (observation): Formatted 1636 files in 6101ms.
FORMAT_EXIT_CODE: 0
WRITESET-CHANGED: (none printed)
WRITESET-CHANGED-COUNT: 0
PORCELAIN-BEFORE-COUNT: 0
PORCELAIN-AFTER-COUNT: 0
PORCELAIN-SAME: True
SORTEMAIL-BOM-PRESENT: True (added observation, one read-only statement appended to the payload: UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs still begins with the UTF-8 BOM bytes EF BB BF after the formatter ran)
Acceptance: FORMAT_EXIT_CODE 0; WRITESET-CHANGED-COUNT 0 (the ten Write Set and SortEmail_Tests .cs hashes are identical before and after, so the loop rule does not trigger); PORCELAIN-SAME True (no file outside the Write Set changed). All three hold.
