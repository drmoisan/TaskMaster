# [P0-T15] `QuickFiler.Test/QuickFiler.Test.csproj` insertion anchors re-derived by element text

Timestamp: 2026-08-27T09-45
Command: `Select-String -SimpleMatch` for each `<Compile Include>` element text against `QuickFiler.Test\QuickFiler.Test.csproj`
EXIT_CODE: 0

File: `QuickFiler.Test/QuickFiler.Test.csproj`
Total lines at branch head: **490**

| Element text | Occurrences | Observed line |
| --- | --- | --- |
| `<Compile Include="Controllers\QfcCollectionControllerTests.cs" />` | 1 | 122 |
| `<Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />` | 1 | 123 |
| `<Compile Include="Controllers\QfcCollectionController.TestSupport.cs" />` | 1 | 124 |

**The two insertion anchors are consecutive** (122 and 123), so the owned slot is exactly one line
wide and is unambiguous.

## Observed neighbourhood (verbatim)

```xml
    <Compile Include="Controllers\EfcHomeControllerSeamTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
    <Compile Include="Controllers\QfcCollectionController.TestSupport.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDefects468Tests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDefects468MoveTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDefects468ConversationTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerLayout.StaTests.cs" />
    <Compile Include="Controllers\QfcDatamodelTests.cs" />
```

#468's contiguous block is visible here: it begins **after** the dark-mode entry (line 123) with
`QfcCollectionController.TestSupport.cs` at 124 and ends **before**
`Controllers\QfcDatamodelTests.cs`. `[P1-T13]` inserts one line at 123, immediately after
`QfcCollectionControllerTests.cs` and immediately before the dark-mode entry, so it never writes
inside #468's block.

The item group is confirmed **not** alphabetically ordered: `…ControllerTests.cs` precedes
`…ControllerDarkModeTests.cs`. The slot is therefore identified only by the element-text anchor pair,
never by alphabetical position and never by a line number carried in the plan.

## TestSupport observation (informational only)

`QuickFiler/Controllers/…` — the file `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs`
added by upstream #468 `[P2-T1]` **is present** on this branch, registered at line 124.

Per decision D-P1 this observation changes nothing: the new test file
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` carries its own
`private static` reflection field-setter and item-group builder and depends on no other test file.
Recording the presence keeps the D-P1 rationale auditable — the coupling was removed deliberately, not
because the helper was absent.

## Acceptance evaluation

- `<Compile Include="Controllers\QfcCollectionControllerTests.cs" />` occurs exactly once. PASS.
- `<Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />` occurs exactly once. PASS.

Output Summary: both insertion anchors occur exactly once and are consecutive at lines 122 and 123;
#468's `TestSupport.cs` entry is present at 124 (informational); acceptance conditions met.
