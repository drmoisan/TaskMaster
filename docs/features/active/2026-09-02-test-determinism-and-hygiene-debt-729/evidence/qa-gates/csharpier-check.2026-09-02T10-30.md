# Post-change CSharpier check (P6-T2)

Timestamp: 2026-09-02T23-34

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

## Which of the two acceptance outcomes held

The first outcome held: `EXIT_CODE: 0`. The repo-wide read-only formatter gate reports no
unformatted file at all, so the subset-derivation branch of this task's acceptance was not needed
and was not exercised.

## Reported unformatted set (verbatim)

```
(empty — CSharpier reported no unformatted file)
```

## Full console output

```
Checked 1571 files in 5757ms.
```

## Subset derivation

Not performed, and not required, because the reported set is empty. An empty set is trivially a
subset of the `Baseline unformatted set:` recorded by P0-T8, which is itself empty, and it
contains none of the seven plan-owned formattable paths and does not contain
`UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs`.

## Authorized mechanical branch

Not taken. The reported set does not contain `TaskMaster.Test/packages.config`, so the
directory-form `dotnet tool run csharpier format TaskMaster.Test` invocation — which is broader
than the D4 scope lock — was not run, and no `git checkout HEAD --` restoration was required.

Output Summary:

- `Checked 1571 files in 5757ms.` with exit code 0 and an empty unformatted set.
- The checked-file count fell from the P0-T8 baseline of 1576 to 1571, a difference of 5. That
  difference reconciles exactly against the file inventory of Phases 3 through 5. The seventeen
  deletions comprise twelve `.cs` files and five `.resx` files. CSharpier does not process
  `.resx`, so the five `.resx` deletions do not move the count. Five of the twelve `.cs`
  deletions are `*.Designer.cs` files, which CSharpier skips by filename, leaving seven counted
  `.cs` deletions. Two guard `.cs` files were added. 1576 - 7 + 2 = 1571, which is the observed
  count.
- This satisfies the `dotnet tool run csharpier check .` clause of spec.md AC21 in its literal
  form: no unformatted files are reported.
