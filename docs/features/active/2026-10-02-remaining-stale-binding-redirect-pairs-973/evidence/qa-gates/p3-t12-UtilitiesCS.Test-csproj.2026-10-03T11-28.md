# P3-T12 EDIT-CSPROJ UtilitiesCS.Test/UtilitiesCS.Test.csproj (issue #973)

Timestamp: 2026-10-03T11-28
Command: Edit tool on UtilitiesCS.Test/UtilitiesCS.Test.csproj (old_string the three-line System.Linq.Async Reference element at 896-898; new_string those lines plus the nine spec Part C lines with VERSION 10.0.0.12); Greps for the Include, Aliases, CS0121 and CS0433 (per file and over glob */*.csproj); CMD-LINECOUNT; CMD-CRCOUNT; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- '*.csproj'; git -C <execution-worktree-root> status --porcelain -- '*.csproj'
EXIT_CODE: 0
Output Summary: aliased Reference element inserted after line 898; every EDIT-CSPROJ gate holds; LINECOUNT 1023 to 1032, CRCOUNT 1022 to 1031; numstat 9/0; the Include and Aliases Greps over */*.csproj return exactly the five Write Set projects, count 1 each; the name-listing diff and the porcelain both list exactly those five.

Before: LINECOUNT 1023, CRCOUNT 1022
After: LINECOUNT 1032, CRCOUNT 1031
Include Version=10.0.0.12: 1; Aliases: 1; CS0121: 1; CS0433: 1

Glob Grep `Include="System.Linq.AsyncEnumerable, Version=` over */*.csproj: QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, UtilitiesCS.Test (count 1 each)
Glob Grep `<Aliases>SystemLinqAsyncEnumerable</Aliases>` over */*.csproj: the same five (count 1 each)

git diff --numstat (name list) '*.csproj' at plan start:
9	0	QuickFiler/QuickFiler.csproj
9	0	TaskMaster/TaskMaster.csproj
9	0	ToDoModel/ToDoModel.csproj
9	0	UtilitiesCS.Test/UtilitiesCS.Test.csproj
9	0	UtilitiesCS/UtilitiesCS.csproj

git status --porcelain -- '*.csproj':
 M QuickFiler/QuickFiler.csproj
 M TaskMaster/TaskMaster.csproj
 M ToDoModel/ToDoModel.csproj
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
 M UtilitiesCS/UtilitiesCS.csproj
