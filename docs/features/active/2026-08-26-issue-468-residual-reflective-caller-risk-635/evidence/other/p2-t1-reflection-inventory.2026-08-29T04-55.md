# Reflection Entry-Point Inventory (P2-T1) — discharges AC-8

- **Issue:** #635
- **Plan task:** [P2-T1]

Timestamp: 2026-08-29T06-31

## Output Summary

A seventeen-pattern reflection entry-point inventory was taken over the QuickFiler production tree
(228 tracked files) and the QuickFiler test tree (151 tracked files), with the two counts reported
separately for every pattern. All sixteen name-resolving patterns print a production count of zero.
The seventeenth pattern, the namespace token `System.Reflection`, prints a production count of 39,
which is recorded verbatim and is not asserted to be zero; [P2-T2] classifies that whole population and
shows that none of its occurrences takes a member-name argument.

INVENTORY_PATTERNS: 17
QF_PROD_SCOPE_FILES: 228
QF_TEST_SCOPE_FILES: 151

## Command

Command:

```
pwsh -NoProfile -Command 'Write-Output ("QF_PROD_SCOPE_FILES=" + (git ls-files -- "QuickFiler/*").Count); Write-Output ("QF_TEST_SCOPE_FILES=" + (git ls-files -- "QuickFiler.Test/*").Count); @("GetMethod(","GetMethods(","GetMember(","GetMembers(","GetProperty(","GetProperties(","GetField(","GetFields(","GetEvent(","InvokeMember(","Type.GetType(","Activator.CreateInstance","Assembly.CreateInstance","Assembly.Load","Delegate.CreateDelegate","CallByName","System.Reflection") | ForEach-Object { $p = $_; $prod = @(git grep -n -I -F -e $p -- "QuickFiler/*").Count; $test = @(git grep -n -I -F -e $p -- "QuickFiler.Test/*").Count; Write-Output ($p + " prod=" + $prod + " test=" + $test) }'
```

Output, verbatim:

```
QF_PROD_SCOPE_FILES=228
QF_TEST_SCOPE_FILES=151
GetMethod( prod=0 test=69
GetMethods( prod=0 test=4
GetMember( prod=0 test=6
GetMembers( prod=0 test=0
GetProperty( prod=0 test=24
GetProperties( prod=0 test=0
GetField( prod=0 test=172
GetFields( prod=0 test=2
GetEvent( prod=0 test=10
InvokeMember( prod=0 test=0
Type.GetType( prod=0 test=0
Activator.CreateInstance prod=0 test=4
Assembly.CreateInstance prod=0 test=0
Assembly.Load prod=0 test=0
Delegate.CreateDelegate prod=0 test=0
CallByName prod=0 test=0
System.Reflection prod=39 test=121
```

EXIT_CODE: 0

The `pwsh -NoProfile -Command` wrapper exits `0` regardless of the exit code of any command inside it.
Only the printed values are asserted.

## The seventeen pattern rows

The two scope lines that the command prints before the pattern rows are recorded under the measured
scope section below and are not pattern rows. The seventeen pattern rows, in the listed order, are:

| # | Pattern | Production count | Test count | Name-resolving |
|---|---|---|---|---|
| 1 | `GetMethod(` | 0 | 69 | yes |
| 2 | `GetMethods(` | 0 | 4 | yes |
| 3 | `GetMember(` | 0 | 6 | yes |
| 4 | `GetMembers(` | 0 | 0 | yes |
| 5 | `GetProperty(` | 0 | 24 | yes |
| 6 | `GetProperties(` | 0 | 0 | yes |
| 7 | `GetField(` | 0 | 172 | yes |
| 8 | `GetFields(` | 0 | 2 | yes |
| 9 | `GetEvent(` | 0 | 10 | yes |
| 10 | `InvokeMember(` | 0 | 0 | yes |
| 11 | `Type.GetType(` | 0 | 0 | yes |
| 12 | `Activator.CreateInstance` | 0 | 4 | yes |
| 13 | `Assembly.CreateInstance` | 0 | 0 | yes |
| 14 | `Assembly.Load` | 0 | 0 | yes |
| 15 | `Delegate.CreateDelegate` | 0 | 0 | yes |
| 16 | `CallByName` | 0 | 0 | yes |
| 17 | `System.Reflection` | 39 | 121 | no — a namespace token, not a call |

The row count is 17, which equals the recorded `INVENTORY_PATTERNS` value.

## The sixteen name-resolving patterns all print prod=0

Rows 1 through 16 are the name-resolving patterns: every pattern in the list except `System.Reflection`.
Each prints `prod=0`. The QuickFiler production assembly therefore contains no reflective member-lookup
call site, no late-bound instance creation from a type name, no assembly load by name, no delegate
creation from a named method, and no `CallByName` late-binding call.

That set includes the `GetField(` and `GetFields(` family, rows 7 and 8. **The earlier AC-16 search
omitted the `GetField(` family entirely.** Its reflection inventory covered only the `GetMethod(` and
`InvokeMember(` patterns, which are rows 1 and 10 here. The omission matters because `GetField(` is the
family actually used against the affected type: it prints a test-tree count of 172, the largest of any
row, and [P2-T3] enumerates the sites among them whose receiver is `typeof(QfcCollectionController)`.
The removed thirteenth identifier `_templateTlp` is a private field, so field reflection is precisely
the mechanism that could have reached it. Recording that omission explicitly is a required part of this
inventory, and [P3-T1] records the corresponding correction to the AC-16 record.

## The System.Reflection row is recorded, not asserted to be zero

The `System.Reflection` row prints a production value of 39. That value is at least 32, is recorded
verbatim, and is not asserted to be zero.

The production pathspec `QuickFiler/*` reaches every tracked file under the QuickFiler production tree,
including tracked non-source files such as the project file, its tracked backup, and package manifests,
so the printed production value is expected to exceed the count of first-party source-file occurrences.
[P2-T2] enumerates and classifies the whole printed population of 39 and shows that its five classes
sum to 39 with the "call site taking a member-name argument" class empty.

## Base-commit reference values for the test-tree column

These are recorded for comparison and are not asserted:

| Pattern | Plan reference | Printed here | Difference |
|---|---|---|---|
| `GetMethod(` | 69 | 69 | 0 |
| `GetMethods(` | 4 | 4 | 0 |
| `GetMember(` | 6 | 6 | 0 |
| `GetMembers(` | 0 | 0 | 0 |
| `GetProperty(` | 24 | 24 | 0 |
| `GetProperties(` | 0 | 0 | 0 |
| `GetField(` | 172 | 172 | 0 |
| `GetFields(` | 2 | 2 | 0 |
| `GetEvent(` | 10 | 10 | 0 |
| `InvokeMember(` | 0 | 0 | 0 |
| `Type.GetType(` | 0 | 0 | 0 |
| `Activator.CreateInstance` | 4 | 4 | 0 |
| `Assembly.CreateInstance` | 0 | 0 | 0 |
| `Assembly.Load` | 0 | 0 | 0 |
| `Delegate.CreateDelegate` | 0 | 0 | 0 |
| `CallByName` | 0 | 0 | 0 |
| `System.Reflection` | 121 | 121 | 0 |

Every test-column reference value reproduces exactly. This is consistent with the [P0-T5] finding that
`TRACKED_CS` is unchanged on this branch and with the [P1-T4] finding that no `.cs` file is written by
this item.

## Measured scope for every zero recorded here

QF_PROD_SCOPE_FILES: 228 — the count of tracked files matching the pathspec `QuickFiler/*`, which is
the search set for every production-column value in the table. It is greater than zero, so each
`prod=0` is a measurement over a non-empty search set.

QF_TEST_SCOPE_FILES: 151 — the count of tracked files matching the pathspec `QuickFiler.Test/*`, which
is the search set for every test-column value. It is likewise greater than zero, so each `test=0` in
the table is a measurement over a non-empty search set.

The plan's base-commit reference values for these two scope sizes are 228 and 151. Both reproduce
exactly.

These are the measured scope sizes that [P2-T2], [P2-T4] and [P3-T4] cite for every zero result taken
over either QuickFiler tree.

The pathspec `QuickFiler/*` requires the literal path prefix `QuickFiler/`, so it does not reach the
QuickFiler test tree, whose paths begin `QuickFiler.Test/`. The two scopes are therefore disjoint and
the production and test columns are independent measurements.
