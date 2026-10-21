# QA Gate — Project-File Line Budget (P6-T4)

Timestamp: 2026-08-27T20-52

## The gating command

Command: `git diff --numstat -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj`

EXIT_CODE: 0

Output, verbatim:

```
1	0	QuickFiler.Test/QuickFiler.Test.csproj
1	0	QuickFiler/QuickFiler.csproj
```

| Project file | Added | Deleted | Required | Verdict |
| --- | ---: | ---: | --- | --- |
| `QuickFiler/QuickFiler.csproj` | 1 | 0 | exactly 1 added, 0 deleted | SATISFIED |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 1 | 0 | exactly 1 added, 0 deleted | SATISFIED |

Exactly **two rows**, each with 1 added line and 0 deleted lines. This is the whole authorized
project-file budget for this feature.

## No other project file appears

Command: `git status --porcelain -- '*.csproj'`
Output:

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler/QuickFiler.csproj
```

Only the two owned project files are modified. No other `.csproj` anywhere in the repository is touched,
and no `.sln`, `.props` or `.targets` file is touched.

## The two added lines

`QuickFiler/QuickFiler.csproj` (AC-23), inserted immediately after the
`Viewers\BreadcrumbBridgeCoordinator.Search.cs` sibling entry:

```xml
    <Compile Include="Viewers\BreadcrumbBridgeCoordinator.Suggestions.cs" />
```

`QuickFiler.Test/QuickFiler.Test.csproj` (AC-24), inserted immediately after the
`Viewers\BreadcrumbBridgeCoordinatorTests.cs` sibling entry for the same production type:

```xml
    <Compile Include="Viewers\BreadcrumbBridgeCoordinatorSupersessionTests.cs" />
```

## Line-number drift recorded

The plan named line 392 for the production insertion and line 60 for the test insertion. Both numbers had
drifted since the plan was written:

- In `QuickFiler/QuickFiler.csproj` the quoted `Search.cs` anchor is at line **395**, not 392 (line 392 is
  a `</Compile>` closing tag).
- In `QuickFiler.Test/QuickFiler.Test.csproj` the `Viewers\BreadcrumbBridgeCoordinatorTests.cs` anchor is
  at line **65**, not 60 (line 60 is a `Controllers\...` entry).

In both cases the insertion was made relative to the CONTENT the plan quotes verbatim rather than to the
stale line number. That is the plan's own operative rule for the test project — "adjacency, not
alphabetical position" — and applying the same rule to the production project keeps the new entry beside
its sibling partial-part entry, which is what minimises merge surface with the concurrent sibling epic
children.

Both files retain their original encoding and line endings: UTF-8 with BOM, CRLF. Verified with `file`
after each edit. `git diff` shows a single `+` line in each with no whitespace or encoding churn, which is
what the 1-added / 0-deleted numstat confirms.
