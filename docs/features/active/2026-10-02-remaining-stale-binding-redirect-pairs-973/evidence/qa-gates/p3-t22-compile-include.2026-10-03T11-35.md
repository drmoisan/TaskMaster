# P3-T22 Compile Include for the new partial file (issue #973; Part H)

Timestamp: 2026-10-03T11-35
Command: EDIT-COMPILE-INCLUDE (Edit tool on UtilitiesCS/UtilitiesCS.csproj; old_string `    <Compile Include="EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs" />`; new_string that line, a line break and `    <Compile Include="EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.ConditionalEngine.cs" />`); Grep of the two Compile items -n; Grep `Compile Include=` count; CMD-LINECOUNT; CMD-CRCOUNT; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- UtilitiesCS/UtilitiesCS.csproj; git -C <execution-worktree-root> diff -U0 a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- UtilitiesCS/UtilitiesCS.csproj (CMD-HUNKS)
EXIT_CODE: 0
Output Summary: the new Compile item sits immediately after the existing CategoryClassifierGroup.cs item (619 and 620); Compile item count 498 to 499; LINECOUNT 1344 to 1345 and CRCOUNT 1343 to 1344; numstat 10/0 with exactly two hunks (the Part C element and the Compile Include).

Before: LINECOUNT 1344, CRCOUNT 1343, `Compile Include=` 498 (equals P0-T20 COMPILE-ITEMS-BASELINE 498), existing item at 619
After: LINECOUNT 1345, CRCOUNT 1344, `Compile Include=` 499
619 `    <Compile Include="EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs" />`
620 `    <Compile Include="EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.ConditionalEngine.cs" />` (count 1)
NUMSTAT: 10	0	UtilitiesCS/UtilitiesCS.csproj
HUNK @@ -400,0 +401,9 @@
HUNK @@ -610,0 +620 @@
