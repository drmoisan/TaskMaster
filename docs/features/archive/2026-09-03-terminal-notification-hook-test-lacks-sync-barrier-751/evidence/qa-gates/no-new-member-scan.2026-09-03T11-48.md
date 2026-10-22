# P4-T10 — No New Type, Field, Primitive, Helper, or Using Directive (Issue #751)

Timestamp: 2026-09-03T14-46

EXIT_CODE: 0 (all three commands)

## Command 1 — net line bounds

Command:

```powershell
git diff --numstat f8414ee9..HEAD -- TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs
```

Verbatim output:

```
1	1	TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs
2	1	TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs
```

| File | Added | Deleted | Added minus deleted | Required | Result |
|---|---|---|---|---|---|
| `AppOlObjectsFolderTreeServiceTests.cs` | 2 | 1 | **1** | between 1 and 2 inclusive | PASS |
| `AppOlObjectsFolderTreeServiceLifecycleTests.cs` | 1 | 1 | **0** | equal to 0 | PASS |

## Command 2 — new type / synchronization primitive scan

Command:

```powershell
git diff f8414ee9..HEAD -- "TaskMaster.Test/AppGlobals/*.cs" | Select-String -Pattern '^\+.*(TaskCompletionSource|ManualResetEvent|SemaphoreSlim|\bclass\b|\bstruct\b)'
```

Verbatim output:

```
(no match)
```

Match count: **0**. PASS.

## Command 3 — added-declaration scan

Command:

```powershell
git diff f8414ee9..HEAD -- "TaskMaster.Test/AppGlobals/*.cs" | Select-String -Pattern '^\+\s*(using\s|private\s|internal\s|protected\s|public\s|static\s)'
```

Verbatim output:

```
(no match)
```

Match count: **0**. PASS.

## Acceptance

| Required | Observed | Result |
|---|---|---|
| Numstat row for `AppOlObjectsFolderTreeServiceTests.cs` shows added minus deleted between 1 and 2 inclusive | 1 | PASS |
| Numstat row for `AppOlObjectsFolderTreeServiceLifecycleTests.cs` shows added minus deleted equal to 0 | 0 | PASS |
| Command 2 produced no match | 0 | PASS |
| Command 3 produced no match | 0 | PASS |

## Why command 3 is required and not redundant with the numstat bound

The net-line band of 1 to 2 lines exists to tolerate a CSharpier reflow of the inserted statement across two
physical lines. That same band would equally admit a single added `using` directive or a single added
one-line field, so the numstat bound alone cannot exclude either. The added-declaration scan is what does.

The three added lines this plan authorizes each begin with a token outside command 3's alternation:
`(await ...`, `Volatile.Read(...`, and `Interlocked.Increment(...`. The empty match set is therefore
reachable for the intended change, which is what makes this gate falsifiable rather than vacuous. A new gate
field, a new `TaskCompletionSource`, a new helper method, a new fixture type, or a new `using` directive
could not satisfy the net-line bounds and both empty match sets simultaneously.

## Recorded limit of command 3

Command 3 is keyed on a line beginning with `using` or with an access or `static` modifier. A continuation
declarator on a multi-declarator field list carries no modifier and would therefore not match it. That shape
exists in the fixture today: `AppOlObjectsFolderTreeServiceLifecycleTests.cs:158-159` declares
`InvokedTerminalHookCount` and `LoadThreadId` across two lines, of which only the first carries `internal`.
Adding a third declarator to it would be net `+1`, which the numstat bound of exactly `0` for that file
rejects.

For `AppOlObjectsFolderTreeServiceTests.cs` the numstat band is 1 to 2, so the exclusion there rests on the
net-line bound together with the fact that every declaration form spec AC7 names — `TaskCompletionSource`,
gate field, helper method, fixture type — is caught by command 2 or by a modifier-bearing first line. No
additional command is added for this residual shape; the limit is recorded here so a reviewer is not misled
about the scan's reach.
