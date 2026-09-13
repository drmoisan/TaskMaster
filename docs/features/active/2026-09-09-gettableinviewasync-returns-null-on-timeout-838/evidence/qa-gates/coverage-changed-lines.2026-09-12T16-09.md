# P4-T8 — Denominator-C coverage for the changed and added production lines

Timestamp: 2026-09-13T03-17

Command: a single pwsh payload that first runs `git -C . add -N UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.Failures.cs` (idempotent after P4-T1), then runs `git -C . diff --unified=0 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.Failures.cs`, derives the added-line numbers per file from the hunk headers, reads the Cobertura document at `Join-Path $env:TEMP "taskmaster-838\final-tests\coverage.cobertura.xml"`, applies the fixed per-file aggregation rule to each file, and intersects the added-line numbers with the line numbers the document carries.

The two-dot form against the working tree is used deliberately: the Cobertura document was generated from the working tree after P4-T1's format pass, so line numbers must come from the working tree and not from a committed revision.

EXIT_CODE: 0

## Added lines per file

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, 29 added lines:

```
36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,103,107,124,128,147,151,152,153,154,155,156,157,158,159
```

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.Failures.cs`, 33 added lines, which is the whole file:

```
1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33
```

## Intersection with the Cobertura line elements

| File | Class nodes matched | Executable added lines | Covered | Uncovered |
|---|---|---|---|---|
| `OlTableExtensions.TableAccess.cs` | 9 | 10 | 10 | none |
| `OlTableExtensions.TableAccess.Failures.cs` | 1 | 6 | 6 | none |

The executable added lines in the file under fix are 103, 107, 124, 128, 147, 153, 154, 155, 156 and 159: the two bound catch clauses, the two retry-ceiling throws of the wrapper, the guard's null test, its cancellation check, its timeout throw and the unsuppressed return. The executable added lines in the new file are 26 through 31, the helper's body. Every one of the sixteen is covered.

The lines the document does not carry are non-executable — the fifteen XML documentation lines added above the method declaration, the corrected comment lines, the using directive, the namespace and class declarations, braces, and the helper's own documentation and signature lines — and are excluded from both numerator and denominator, as the plan's denominator-C definition requires.

```
DENOMINATOR_C_ADDED_LINES=62
DENOMINATOR_C_EXECUTABLE=16
DENOMINATOR_C_COVERED=16
DENOMINATOR_C_PERCENT=100
```

Output Summary: denominator C, the blocking figure, is 100 percent. Both acceptance clauses hold: the executable count is 16, which is at least 1, and the percentage is at least 90. The figure is labelled denominator C and is not comparable to the repository-wide denominator-R figure recorded in P4-T9. The per-file aggregation rule is mandatory here rather than a refinement: the file under fix contributes nine class nodes to the document because it is a partial class whose members include six async state machines, one of them the state machine of the method under fix, so reading any single class node would have under-counted the member the change touches.
