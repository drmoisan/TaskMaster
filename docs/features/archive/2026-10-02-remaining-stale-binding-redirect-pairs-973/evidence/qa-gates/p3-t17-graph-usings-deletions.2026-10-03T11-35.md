# P3-T17 Part F Graph using deletions (issue #973)

Timestamp: 2026-10-03T11-35
Command: EDIT-USING on the five Part F files (Edit tool; old_string the directive line(s) plus the following line read by Grep `^using Microsoft\.Graph` -A 1 -n in P0-T20; new_string the following line alone); Grep `^using Microsoft\.Graph` count; Grep of the following-line texts -n; Grep `^using ` count; CMD-LINECOUNT; CMD-CRCOUNT; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- '*.cs'; git -C <execution-worktree-root> diff -U0 a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- '*.cs' (CMD-HUNKS)
EXIT_CODE: 0
Output Summary: the six Microsoft.Graph using directives deleted from the five files, nothing else changed; every EDIT-USING gate holds; one hunk per file at old-file starts 7, 10, 11, 18 and 6; CRLF preserved (line count equals CR count in every file).

| File | USINGS-BEFORE | USINGS-AFTER | Lines/CR before | Lines/CR after | Numstat | Hunk |
|---|---|---|---|---|---|---|
| UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs | 11 | 10 | 306/306 | 305/305 | 0	1 | @@ -7 +6,0 @@ |
| UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs | 15 | 14 | 270/270 | 269/269 | 0	1 | @@ -10 +9,0 @@ |
| UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs | 20 | 18 | 539/539 | 537/537 | 0	2 | @@ -11,2 +10,0 @@ |
| UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs | 23 | 22 | 356/356 | 355/355 | 0	1 | @@ -18 +17,0 @@ |
| UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs | 8 | 7 | 188/188 | 187/187 | 0	1 | @@ -6 +5,0 @@ |

`^using Microsoft\.Graph` over the five files: 0
Following lines now in the deleted directives' positions (count 1 each in its file): StoreWrapper.cs:7 `using Microsoft.Office.Interop.Outlook;`; Triage_OlLogic.cs:10 `using Microsoft.Office.Interop.Outlook;`; CategoryClassifierGroup.cs:11 `using Microsoft.Office.Interop.Outlook;`; ManagerAsyncLazy.cs:18 `using Newtonsoft.Json;`; FolderMinimalWrapper.cs:6 `using Newtonsoft.Json;`
EOL-NORMALISED: no (not needed)
