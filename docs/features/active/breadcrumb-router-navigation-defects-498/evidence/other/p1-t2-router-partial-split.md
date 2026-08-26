# P1-T2 — Mandatory Decision-D8 Partial-Class Split of BreadcrumbBridgeRouter.cs

Timestamp: 2026-08-26T08-54

Command: `pwsh -NoProfile -Command '$vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m "/p:Configuration=Debug" "/p:Platform=Any CPU" "/p:EnableNETAnalyzers=true" "/p:EnforceCodeStyleInBuild=true"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

All four acceptance conditions hold.

### 1. Type declaration made partial

`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:19` changed from
`    public sealed class BreadcrumbBridgeRouter` to
`    public sealed partial class BreadcrumbBridgeRouter`. That is the ONLY edit made at or above line
409 in the primary file; every other line from 1 to 408 is byte-identical to its pre-split form, which
is what `P1-T4` re-verifies.

### 2. Twelve private members relocated, unchanged, to the new file

`QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` **exists**. It declares the same sealed
partial class in namespace `QuickFiler.Controllers` and carries `#nullable enable` on line 1.

The relocated range was the contiguous tail `:410-594` of the pre-split file — verified before the move
by reading index boundaries: pre-split line 408 was `        }` (the close of `ExpandLeafAsync`), line
409 was blank, line 410 opened `private void ActivateSegment(BreadcrumbRow row, int segmentIndex)`,
line 594 was the close of `FindSelectable`, and lines 595 and 596 were the class and namespace closing
braces. Exactly **185 lines** were moved.

All twelve members named by the task were found in the moved block, in the task's stated order, and no
thirteenth member was moved:

| Order | Member | Line within the moved block |
|---:|---|---:|
| 1 | `ActivateSegment(BreadcrumbRow, int)` | 1 |
| 2 | `ActivateChild` | 20 |
| 3 | `FetchChainAsync` | 34 |
| 4 | `SelectRow` | 67 |
| 5 | `SelectHierarchyPath` | 85 |
| 6 | `ToArchiveRelativePath` | 95 |
| 7 | `PostRowRender` | 119 |
| 8 | `PostOutbound` | 129 |
| 9 | `DeliverDocument` | 134 |
| 10 | `FindRow` | 148 |
| 11 | `IndexOf` | 161 |
| 12 | `FindSelectable` | 174 |

**Pure mechanical relocation.** The 185 moved lines were transplanted verbatim, as an unmodified block:
no member body, signature, accessibility, or ordering within the moved set changed, and no member
outside the moved set moved. The only content added to the new file is its file header — the
`#nullable enable` pragma, five `using` directives, the namespace and class declaration, one XML doc
comment, and the two closing braces. The only content removed from the primary file beyond the moved
185 lines is the single blank separator line that had preceded `ActivateSegment`, which would otherwise
have been left dangling before the class-closing brace.

**Using directives, narrowed to what the moved members need.** The new file declares `System`
(`Exception`, `OperationCanceledException`, `StringComparison`), `System.Collections.Generic`
(`IReadOnlyList<>`), `System.Threading` (`CancellationToken`), `System.Threading.Tasks` (`Task<>`), and
`UtilitiesCS.OutlookObjects.Folder` (`BreadcrumbRow`, `BreadcrumbSegment`, `BreadcrumbRowKind`,
`BreadcrumbRowBuilder`, `FolderBreadcrumbSegment`, `FolderTreeNodeKey`, `BreadcrumbRenderMessage`,
`BreadcrumbOutboundMessage` — each confirmed to live in that namespace). The primary file's
`QuickFiler.Viewers` and `UtilitiesCS` directives were deliberately NOT copied because no moved member
names a type from either; the analyzer gate below confirms the narrower set is both sufficient and free
of unused-directive diagnostics.

### 3. Project-file registration, immediately adjacent

`<Compile Include="Controllers\BreadcrumbBridgeRouter.Selection.cs" />` was inserted into
`QuickFiler/QuickFiler.csproj`. Measured with `Get-Content` and `Select-String -SimpleMatch`:

- the literal `Controllers\BreadcrumbBridgeRouter.cs` is on line **290**
- the literal `Controllers\BreadcrumbBridgeRouter.Selection.cs` is on line **291**
- adjacency `291 == 290 + 1` evaluates **True**

The item group was NOT re-sorted. `git diff --stat` over the project file reports `1 file changed, 1
insertion(+)` and no deletion, so no other line was disturbed. CRLF terminators were preserved: the
file went from 594 CR-bearing lines to 595, with every LF still part of a CRLF pair and the final line
still unterminated. The insertion was made with an explicit CRLF-preserving rewrite; `sed -i` was not
used, as the plan's binding CRLF rule requires.

### 4. Line counts, measured with `(Get-Content -LiteralPath $path).Count`

| File | Lines | Limit for this task | Result |
|---|---:|---:|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | **410** | at or under 460 | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | **204** | at or under 220 | PASS |

The primary file fell from **596** lines (its `P0-T16` baseline, 96 over the 500-line limit) to **410**,
resolving the pre-existing violation this plan is obliged to fix. Both parts are now well under 500,
which is the headroom Phases 2, 3 and 6 will consume.

### 5. Analyzer gate

The analyzer Rebuild recipe returned **`EXIT_CODE: 0`** — `Build succeeded`, **5 Warning(s), 0
Error(s)**, elapsed 00:00:15.99. The warning count and composition are unchanged from the `P0-T13`
baseline: the same five `System.Reactive` packages.config advisories on `QuickFiler`, `TaskMaster`,
`ToDoModel`, `UtilitiesCS` and `UtilitiesCS.Test`. No diagnostic names either split file.

Additionally, and read-only, `dotnet tool run csharpier check` over exactly the two split files
returned `EXIT_CODE: 0` (`Checked 2 files`), so the relocation introduced no formatting debt for the
`P8-T1` and `P8-T2` gates to absorb.

### 6. Scope

`git status --porcelain --untracked-files=all` restricted to the four source projects reports exactly
three entries, all owned:

```
 M QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
 M QuickFiler/QuickFiler.csproj
?? QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs
```

No MUST-NOT-WRITE file was touched.
