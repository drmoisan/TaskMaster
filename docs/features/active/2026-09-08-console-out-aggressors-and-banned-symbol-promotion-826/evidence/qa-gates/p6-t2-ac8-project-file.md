# AC8 and the no-other-project-file constraint (issue #826, [P6-T2])

Timestamp: 2026-09-09T19-46

Command, run as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
git diff --numstat $Base -- "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
git diff $Base -- "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
git diff --name-only $Base -- "*.csproj" "*.props" "*.targets"
git status --porcelain --untracked-files=all -- "*.csproj" "*.props" "*.targets"
```

EXIT_CODE: 0

## Output 1 — anchored numstat

```
1	0	UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

Exactly 1 added and 0 removed lines.

## Output 2 — anchored diff

```
diff --git a/UtilitiesCS.Test/UtilitiesCS.Test.csproj b/UtilitiesCS.Test/UtilitiesCS.Test.csproj
index 4119128c..dc599c52 100644
--- a/UtilitiesCS.Test/UtilitiesCS.Test.csproj
+++ b/UtilitiesCS.Test/UtilitiesCS.Test.csproj
@@ -547,6 +547,7 @@
     <Compile Include="OutlookObjects\Table\OlTableExtensions_Tests.cs" />
     <Compile Include="OutlookObjects\Table\OlTableExtensionsEtlClockTests.cs" />
     <Compile Include="OutlookObjects\Table\GetTableInViewAsyncClockTests.cs" />
+    <Compile Include="OutlookObjects\Table\OlTableExtensionsTimeoutDiagnosticsTests.cs" />
     <Compile Include="OutlookObjects\Table\OlToDoTable_Tests.cs" />
     <Compile Include="ReusableTypeClasses\NewSmartSerializable\ConfigController_Tests.cs" />
   </ItemGroup>
```

A single added line whose text contains the token `OlTableExtensionsTimeoutDiagnosticsTests.cs`, with no
removed line anywhere. Every surrounding entry appears as unchanged context, which is what proves no
existing entry was reordered or reformatted. The indentation and element shape match the neighbours
exactly.

## Output 3 — name-listing diff over project, props and targets files

```
UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

Exactly that one project file and nothing else. No `.props` or `.targets` file appears.

## Output 4 — porcelain companion span

```
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

The companion lists no untracked or modified project, props or targets file other than that one. The two
spans are complementary: the anchored diff enumerates tracked changes and is blind to a newly created
file, while the porcelain status would surface an untracked one. Neither reveals a second project file.

Per plan decision D17, no `:(exclude).claude` pathspec is needed on these spans, because they are already
scoped to `*.csproj`, `*.props` and `*.targets` and no file with one of those extensions is tracked under
`.claude`.

Output Summary: the anchored numstat reads exactly 1 added and 0 removed lines for
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`, the anchored diff shows a single added `<Compile Include>`
line naming the new test file with no reordering or reformatting, and neither the name-listing diff nor
the porcelain companion lists any other project, props or targets file. AC8 is satisfied.
