# Baseline — Banned-API Inventory for Touched Production Files (Issue #207, AC10 worklist)

Timestamp: 2026-06-22T16-51

Command: `grep -rnE 'DateTime\.Now|DateTime\.UtcNow|Random\.Shared|Thread\.Sleep|Task\.Delay' TaskMaster/AppGlobals/AppEvents.cs TaskMaster/AppGlobals/AppOlObjects.cs`

EXIT_CODE: 0

Output Summary (matches enumerated):
| File:Line | Text | Type | Action |
|---|---|---|---|
| AppEvents.cs:184 | `// via a message-pumping DispatcherTimer (no Thread.Sleep/Task.Delay) and performed` | COMMENT (not a call site) | none — descriptive comment |
| AppEvents.cs:225 | `/// <c>Thread.Sleep</c>/<c>Task.Delay</c>.` | XML-DOC (not a call site) | none — doc text |
| AppEvents.cs:456 | `await Task.Delay(100);` | **CALL SITE** (ProcessNewInboxItemsAsync unprocessed-queue retry path) | **REMEDIATE (AC10/P3-T2)**: replace with `await DispatcherDelay.WaitAsync(TimeSpan.FromMilliseconds(100))` |

Authoritative AC10 remediation worklist: exactly ONE banned-API call site exists in the touched production files — `Task.Delay(100)` at AppEvents.cs:456. No DateTime.Now/UtcNow, Random.Shared, or Thread.Sleep call sites. AppOlObjects.cs contains zero banned-API call sites. The line 184/225 matches are comment/doc text, not calls, and require no remediation (they will be removed/retained as part of the surrounding code edits, not as banned-API fixes).
