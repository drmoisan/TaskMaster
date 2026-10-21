# Upstream Cited Files (P0-T5)

Timestamp: 2026-10-01T22-57
Command: git diff --name-only BRANCH-BASE MERGE-BASE -- scripts/vscode scripts/hygiene TaskMaster.runsettings .gitignore .csharpierignore .editorconfig coverage.config global.json dotnet-tools.json BannedSymbols.txt (with BRANCH-BASE 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f and MERGE-BASE 59cbab04f1c854baa2a03b6cbf755c1df4f961b4)
EXIT_CODE: 0
Output Summary: UPSTREAM-TOOLING is exactly .gitignore with GITIGNORE-CITED-RULES=4 (one added line `*.csproj.bak`); UPSTREAM-CITED-CODE lists the expected 947 landing (production file, test project file, new ThrowingSink partial); code-tree porcelain empty; PROD_LINES=476; SIBLING-947-PRESENT=True.

UPSTREAM-TOOLING:

```
.gitignore
```

GITIGNORE-CITED-RULES=4 (the four rules `*.trx`, `*cobertura*.xml`, `coverage/*`, `!coverage/.gitkeep` are each present as a whole line at MERGE-BASE)

.gitignore hunk (`git diff -U0 BRANCH-BASE MERGE-BASE -- .gitignore`):

```
@@ -257,0 +258 @@ ServiceFabricBackup/
+*.csproj.bak
```

The upstream change adds one ignore rule after line 257 and does not touch the four cited rules, so the tooling condition admits it.

UPSTREAM-CITED-CODE (`git diff --name-status BRANCH-BASE MERGE-BASE -- TaskMaster/Ribbon TaskMaster.Test/Ribbon TaskMaster.Test/TaskMaster.Test.csproj TaskMaster/TaskMaster.csproj TaskMaster.Test/packages.config TaskMaster/packages.config`):

```
A	TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs
M	TaskMaster.Test/TaskMaster.Test.csproj
M	TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
```

Changes to the production file, the test project file and the fixture partials are the expected issue 947 landing and are reconciled by P0-T6 and P0-T7.

CODE-TREE-PORCELAIN (`git status --porcelain -- TaskMaster TaskMaster.Test`): no line printed.

MERGE-BASE probe of the production file:

- PROD_LINES=476
- SINK_GUARD_TOKEN=1
- SIBLING-947-PRESENT: True (confirms the P0-T4 pre-merge probe; shape S applies)
