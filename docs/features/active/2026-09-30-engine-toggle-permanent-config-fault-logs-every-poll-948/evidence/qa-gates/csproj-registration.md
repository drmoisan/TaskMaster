# csproj Registration (P1-T2)

Timestamp: 2026-10-01T23-41
Command: pwsh census of TaskMaster.Test\TaskMaster.Test.csproj (fixture entries, new entry line and exact text); git diff --numstat MERGE-BASE -- TaskMaster.Test/TaskMaster.Test.csproj (MERGE-BASE 59cbab04f1c854baa2a03b6cbf755c1df4f961b4); git status --porcelain -- TaskMaster.Test/TaskMaster.Test.csproj
EXIT_CODE: 0
Output Summary: one compile entry inserted directly after the ThrowingSink entry (line 362); NEW_COUNT=1, NEW_ENTRY_EXACT=1, NEW_LINE=363, EXISTING_ENTRIES=5; numstat 1 insertion, 0 deletions; porcelain shows the project file modified.

```
LAST_EXISTING_LINE=362 NEW_LINE=363 EXISTING_ENTRIES=5 NEW_COUNT=1
NEW_ENTRY_EXACT=1
1	0	TaskMaster.Test/TaskMaster.Test.csproj
```

Porcelain:

```
 M TaskMaster.Test/TaskMaster.Test.csproj
```

- LAST_EXISTING_LINE (362) equals LAST-FIXTURE-ENTRY-LINE (362) from P0-T7.
- NEW_LINE (363) equals LAST_EXISTING_LINE plus 1.
- EXISTING_ENTRIES (5) equals ANCHOR-PARTIAL-COUNT (5).
- The project file is outside the formatter (.csharpierignore), so no format pass follows this edit.
