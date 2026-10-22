# Two Compile Include Entries Added (P1-T2)

Timestamp: 2026-08-27T10-35
Task: [P1-T2]
Command: `Select-String -SimpleMatch -Pattern 'QfcItemController.TestSupport.cs' -Path 'QuickFiler.Test/QuickFiler.Test.csproj'` to locate `L`, then a line read of `L`, `L+1`, and `L+2`
EXIT_CODE: 0
Output Summary: The anchor search returns exactly one match, at line `L = 157`. Line 158 contains the
simple string `QfcItemController.UiThreadDispatcherFixture.cs` and line 159 contains
`QfcItemController.UiThreadDispatcherFixtureTests.cs`, in that order, immediately after the anchor.
`git diff --stat` reports `2 insertions(+)` and zero deletions, so nothing else in the file changed.

## Line numbers and matched lines

| Line | Text (leading whitespace trimmed) |
| --- | --- |
| 157 (`L`) | `<Compile Include="Controllers\QfcItemController.TestSupport.cs" />` |
| 158 (`L+1`) | `<Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixture.cs" />` |
| 159 (`L+2`) | `<Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />` |

Simple-string containment results: line 158 contains
`QfcItemController.UiThreadDispatcherFixture.cs` (`True`); line 159 contains
`QfcItemController.UiThreadDispatcherFixtureTests.cs` (`True`).

The anchor match count is 1, so `L` is unambiguous. The two inserted lines do not contain the anchor
string, so inserting them did not create a second anchor match.

## Neighbourhood and diff

The insertion point sits inside the grouped `QfcItemController.*` block of the project's `<Compile>`
item group, which is the `Qfc*` neighbourhood spec AC-8 requires. The plan and spec cite the anchor
at line 146; the actual anchor line at `BASE_SHA` is 157. The offset is a consequence of the epic
integration branch carrying `<Compile Include>` entries that `main` at `988e819b` did not, and the
anchor is identified by its literal text rather than by its line number, so the offset changes
nothing.

```
diff --git a/QuickFiler.Test/QuickFiler.Test.csproj b/QuickFiler.Test/QuickFiler.Test.csproj
@@ -155,6 +155,8 @@
     <Compile Include="Controllers\QfcItemController.MailActionsTests.cs" />
     <Compile Include="Controllers\QfcItemController.PropertiesTests.cs" />
     <Compile Include="Controllers\QfcItemController.TestSupport.cs" />
+    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixture.cs" />
+    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />
     <Compile Include="Controllers\QfcItemController.InitializationTests.cs" />
     <Compile Include="Controllers\QfcItemController.InitializationTests.Part2.cs" />
     <Compile Include="Controllers\QfcItemController.InitializationTests.Part3.cs" />
```

`git diff --stat` for this path: `QuickFiler.Test/QuickFiler.Test.csproj | 2 ++`, `1 file changed,
2 insertions(+)`.

## Encoding preserved

The file was UTF-8 **with BOM** and CRLF line terminators before the edit and remains UTF-8 with BOM
and CRLF after it, verified with `file`. Losing either would produce formatter or build churn
unrelated to this change.
