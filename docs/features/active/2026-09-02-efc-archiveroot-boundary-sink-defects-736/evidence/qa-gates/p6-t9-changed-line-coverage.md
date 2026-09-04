# P6-T9 — Changed-line coverage across the four production files

Timestamp: 2026-09-04T02-12

Command:

```
git add -A
git status --porcelain
git diff --cached -U0 origin/main -- TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs TaskMaster/AppGlobals/AppOlObjects.cs QuickFiler/Controllers/EfcFormController.cs QuickFiler/Controllers/EfcDataModel.cs
```

EXIT_CODE: 0

The changed-line set is derived mechanically as the added and modified line numbers reported by the
anchored diff above, restricted to those four paths and intersected with the coverable line numbers
present in the P6-T6 Cobertura document. The diff pathspec is spelled with `/` because git requires
it; the four Cobertura keys it is normalized to before matching are spelled with the Windows
separator, per the paragraph in P6-T8 that fixes them:
`TaskMaster\AppGlobals\AppOlObjects.ArchiveRoot.cs`, `TaskMaster\AppGlobals\AppOlObjects.cs`,
`QuickFiler\Controllers\EfcFormController.cs`, and `QuickFiler\Controllers\EfcDataModel.cs`.

**The Cobertura document read here is the one produced by the P6-T6 execution that followed
P6-T13**, whose SHA-256 the P6-T6 artifact records as
`A462D34E34BCA57A8AFC77A861562C1CBD5674B27EAC062BFE3DBC729044A777`. P6-T13 executed before this task
despite its higher number; no figure is carried over from the pre-P6-T13 run.

## Clause 6, observed first — did `ShowModelessFaultNotice`'s exclusion take effect?

**No line of `ShowModelessFaultNotice` in `QuickFiler/Controllers/EfcFormController.cs` appears in
the P6-T6 Cobertura document at all.** The member is declared at line 201 and its closing brace is at
line 229; the set of line numbers the document carries for that file within the span 201 through 229
is empty, and no `<method>` element under that file's class nodes carries a `name` containing
`ShowModelessFaultNotice`. The `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` attribute
P4-T1 applied therefore took effect completely, including on the `FormClosed` lambda at line 227,
which closes over a local rather than over `this` and is consequently lifted into a display type that
the attribute does reach — the contrasting case P6-T8 measures for the AppOlObjects wrapper, whose
three lambdas capture `this` and are lifted into members of the declaring class instead.

Because no such line is in the strict denominator uncounted, no line is added to the unreachable set
by this clause.

| | Value | Lenient figure computed from it |
|---|---|---|
| Original `U` | **7** | 100.00% |
| Recomputed `U` | **7** | 100.00% |

The two are equal, so clauses 1 through 5 below are evaluated against `U` = 7.

## Clause 1 — the unreachable set `U`

Seven members, in three groups, matching D2's three-group enumeration row for row.

**(a) One line in `TaskMaster/AppGlobals/AppOlObjects.cs`** — group size 1.

| File | Line | Enclosing member | Why unreachable |
|---|---|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 266 | the `ArchiveRootPath` property getter (property declared at line 260) | P1-T5 forbids any test in this plan from constructing an `AppOlObjects`, and the assignment `_archiveRootPath = ResolveValidatedArchiveRootPath();` sits in the calling getter rather than inside the `[ExcludeFromCodeCoverage]` wrapper it calls, so the attribute does not remove it. |

**(b) Three lines in `QuickFiler/Controllers/EfcDataModel.cs`** — group size 3. Every coverable line
of the production body of `InvokeFilerAsync`, declared at line 355.

| File | Line | Enclosing member | Why unreachable |
|---|---|---|---|
| `QuickFiler/Controllers/EfcDataModel.cs` | 359 | `InvokeFilerAsync` | P5-T4's `TestableEfcDataModel` override replaces this body, so the base body never executes under test. |
| `QuickFiler/Controllers/EfcDataModel.cs` | 360 | `InvokeFilerAsync` | Same override; this is the `return new EmailFiler(config).SortAsync(mailHelpers);` statement, whose execution would require a live Outlook filer. |
| `QuickFiler/Controllers/EfcDataModel.cs` | 361 | `InvokeFilerAsync` | Same override; the member's closing brace. |

**(c) Three lines in `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs`** — group size 3. This group
is **not enumerated independently here**: it is exactly the set `L` that P6-T8 clause 3 derives
mechanically from the `<ResolveValidatedArchiveRootPath>b__` member-name prefix and the wrapper's
source span, and this task takes that set as given so both tasks speak about one set derived once.
P6-T8 recorded `L` = {89, 90, 91}, size 3, which is the size this clause assumed, so `U` is unchanged
by it.

| File | Line | Enclosing member | Why unreachable |
|---|---|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | 89 | lambda `<ResolveValidatedArchiveRootPath>b__74_0`, lifted out of the `[ExcludeFromCodeCoverage]` wrapper `internal string ResolveValidatedArchiveRootPath()` (declared at line 86) | A live Outlook COM crossing: `() => Path.Combine(Root.FolderPath, "Archive")`. |
| `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | 90 | lambda `<ResolveValidatedArchiveRootPath>b__74_1`, same wrapper | A live Outlook COM crossing: `() => ArchiveRoot?.FolderPath`. |
| `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | 91 | lambda `<ResolveValidatedArchiveRootPath>b__74_2`, same wrapper | The logger sink `message => logger.Error(message)`, inside a member excluded by inspection with the written justification that file carries. |

**Group sizes: 1, 3, 3. Sum: `U` = 7.**

## Clause 2 — per-file and aggregate figures

| File (Cobertura key) | Changed lines from the diff | Changed coverable | Changed covered | Quotient |
|---|---|---|---|---|
| `TaskMaster\AppGlobals\AppOlObjects.ArchiveRoot.cs` | 95 | 21 | 18 | 85.71% |
| `TaskMaster\AppGlobals\AppOlObjects.cs` | 4 | 1 | 0 | 0.00% |
| `QuickFiler\Controllers\EfcFormController.cs` | 111 | 33 | 33 | **100.00%** |
| `QuickFiler\Controllers\EfcDataModel.cs` | 16 | 4 | 1 | 25.00% |
| **Aggregate** | 226 | **59** | **52** | **88.14%** |

The controller file's 33 changed coverable lines are now all covered. Five of them — 1005, 1024,
1025, 1033 and 1034 — were uncovered before P6-T13, and closing them is what that task was authored
for.

## Clause 3 — the aggregate recorded twice

| Figure | Numerator | Denominator | Percentage |
|---|---|---|---|
| **Strict** (all changed coverable lines, including every member of `U`) | 52 | **59** | **88.14%** |
| **Lenient** (exactly the 7 lines of `U` excluded, and no others) | 52 | 52 | 100.00% |

Strict denominator as a numeral: **59**. `U` as a numeral: **7**.

## Clause 4 — the floor, and the `10U` escape

The `>= 90.00%` floor is evaluated against the **strict** figure, per D2. The lenient figure is
recorded for the reader and is not the gate.

**Strict figure 88.14% is below the 90.00% floor.** The condition D2 defines is then examined:

| Quantity | Value |
|---|---|
| Strict denominator `N` | 59 |
| `U` | 7 |
| `10U` | **70** |
| Is `N` < `10U`? | **yes** (59 < 70) |

This is the single arithmetic under which D2's reachability argument does not hold: the strict
quotient `(N − U) / N` reaches 90.00% only at `N = 10U`, and the four production files contribute 59
changed coverable lines against a required 70, so the strict figure cannot reach 90.00% whatever the
tests do. D2 records this as the branch this item was expected to take, and the measurement confirms
it rather than diverging from it.

**Precondition on the escape, evaluated and satisfied.** The escape may fire only when the count of
uncovered changed coverable lines lying **outside** `U` is exactly 0.

| Quantity | Value |
|---|---|
| Uncovered changed coverable lines (all) | 7: `AppOlObjects.ArchiveRoot.cs` 89, 90, 91; `AppOlObjects.cs` 266; `EfcDataModel.cs` 359, 360, 361 |
| Clause 1's set `U` | the same 7 lines |
| **Set difference (uncovered outside `U`)** | **0** |
| Members of that difference, named individually | `none` |

The difference is empty, so **the escape fires with its precondition satisfied**. Every uncovered
changed line is a member of the unreachable set enumerated in advance; not one reachable changed line
is uncovered. Had any reachable changed line been uncovered, the escape would have been denied and
the floor recorded as failed.

**Reported to the caller**: the strict changed-line figure is 88.14% against a 90.00% floor, on a
strict denominator of 59 with `U` = 7 and `10U` = 70. This is the arithmetic condition D2 requires be
reported rather than resolved by excluding further lines, and no further line is excluded here.

## Clause 5 — every uncovered changed line, by file, line number, and enclosing member

| File | Line | Enclosing member | In `U`? |
|---|---|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | 89 | lifted lambda of the `[ExcludeFromCodeCoverage]` wrapper `ResolveValidatedArchiveRootPath()` | yes, group (c) |
| `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | 90 | lifted lambda of the same wrapper | yes, group (c) |
| `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | 91 | lifted lambda of the same wrapper | yes, group (c) |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 266 | `ArchiveRootPath` getter | yes, group (a) |
| `QuickFiler/Controllers/EfcDataModel.cs` | 359 | `InvokeFilerAsync` | yes, group (b) |
| `QuickFiler/Controllers/EfcDataModel.cs` | 360 | `InvokeFilerAsync` | yes, group (b) |
| `QuickFiler/Controllers/EfcDataModel.cs` | 361 | `InvokeFilerAsync` | yes, group (b) |

The `git status --porcelain` span was captured alongside the diff so a newly created path is visible
to the derivation. Every one of its 68 lines names a path under this feature folder or one of the
eleven ratified Write Set paths; the three newly created source files
(`TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs`,
`QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs`, and
`TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs`) appear with status `A`, which
is what makes them visible to the anchored `--cached` diff at all.

Output Summary: 59 changed coverable lines across the four production files, 52 covered, strict
aggregate **88.14%**. `U` = 7 (groups of 1, 3 and 3), `10U` = 70, which exceeds the strict
denominator of 59, so the strict figure cannot reach the 90.00% floor by arithmetic. The escape's
precondition is satisfied: the count of uncovered changed coverable lines lying outside `U` is
**0**, with no member to name. The lenient figure with exactly those 7 lines excluded is 100.00%.
Clause 6 found no `ShowModelessFaultNotice` line in the document, so `U` was not recomputed upward.
The arithmetic condition is reported to the caller per D2 rather than resolved by excluding further
lines.
