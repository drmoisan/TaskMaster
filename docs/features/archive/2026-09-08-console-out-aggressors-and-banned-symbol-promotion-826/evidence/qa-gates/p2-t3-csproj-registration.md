# Project-file registration of the new test file (issue #826, [P2-T3])

Timestamp: 2026-09-09T19-16

Command: the single line was inserted with the Edit tool, then the following ran as one
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
@(Select-String -LiteralPath "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -CaseSensitive -SimpleMatch "OlTableExtensionsTimeoutDiagnosticsTests.cs").Count
git diff --numstat $Base -- "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
git diff --numstat $Base -- "*.csproj"
```

EXIT_CODE: 0

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `-SimpleMatch` `OlTableExtensionsTimeoutDiagnosticsTests.cs` count in the project file | 1 | 1 |
| anchored numstat for `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 1 added, 0 removed | exactly 1 added, 0 removed |
| anchored numstat over `*.csproj` | one row only, that same file | exactly that one project file and no other |

Anchored numstat over `*.csproj` against base `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`:

```
1	0	UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

## Anchored diff

```
@@ -547,6 +547,7 @@
     <Compile Include="OutlookObjects\Table\OlTableExtensions_Tests.cs" />
     <Compile Include="OutlookObjects\Table\OlTableExtensionsEtlClockTests.cs" />
     <Compile Include="OutlookObjects\Table\GetTableInViewAsyncClockTests.cs" />
+    <Compile Include="OutlookObjects\Table\OlTableExtensionsTimeoutDiagnosticsTests.cs" />
     <Compile Include="OutlookObjects\Table\OlToDoTable_Tests.cs" />
     <Compile Include="ReusableTypeClasses\NewSmartSerializable\ConfigController_Tests.cs" />
   </ItemGroup>
```

The insertion point was located by the `OutlookObjects\Table\` token rather than by a line number. The
new entry sits adjacent to the existing `OutlookObjects\Table\` compile items, with the same four-space
indentation and the same element shape as its neighbours. Nothing was removed, reordered or reformatted:
the diff shows a single `+` line and no `-` line at all, and the surrounding context lines are unchanged,
which also confirms the file's CRLF line endings were preserved.

Output Summary: `UtilitiesCS.Test.csproj` gains exactly one `<Compile Include>` entry naming the new test
file. No other project, props or targets file is touched. All three acceptance conditions for [P2-T3]
hold.
