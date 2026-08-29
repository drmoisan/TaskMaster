# [P7-T2] Changed-line coverage (Issue 638)

Timestamp: 2026-08-29T12-43

Command:

```
git diff -U0 ecdb1c84ba8541ab67042985919cfed4df768c01 -- QuickFiler/Controllers/EfcDataModel.cs
# then, over coverage/coverage.cobertura.xml, aggregate every <class> whose filename
# resolves to QuickFiler/Controllers/EfcDataModel.cs and intersect its <line number=...>
# entries with the post-image line numbers named by the + hunks above
```

EXIT_CODE: 0

Output Summary:

COVERAGE_XML_MODE: koverage-processed (copied from [P7-T1])

In this mode `Merge-CoberturaClassesByFilename`
(`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:428`) has already merged the async
state machine's generated class into the file-level entry, and `filename` is repo-relative.
The aggregation confirmed this: exactly **1** `<class>` element resolved to
`QuickFiler/Controllers/EfcDataModel.cs`, carrying 284 `<line>` entries. Aggregation is by
`filename` rather than by `class`, because a C# async state machine emits its lines under a
separate generated class and a per-class figure would understate the method.

## Changed-line derivation

`git diff -U0` hunk headers, verbatim:

```
@@ -147,0 +148,8 @@ namespace QuickFiler.Controllers
@@ -254,0 +263,36 @@ namespace QuickFiler.Controllers
@@ -281,0 +326,6 @@ namespace QuickFiler.Controllers
@@ -289 +339 @@ namespace QuickFiler.Controllers
@@ -305,0 +356,6 @@ namespace QuickFiler.Controllers
@@ -310 +366 @@ namespace QuickFiler.Controllers
@@ -323,0 +380,6 @@ namespace QuickFiler.Controllers
@@ -328 +390 @@ namespace QuickFiler.Controllers
```

The post-image lines those `+` hunks name are 148-155, 263-298, 326-331, 339, 356-361, 366,
380-385 and 390: **65** lines in total. Intersecting that set with the line numbers that
appear as `<line number=...>` entries under the aggregated filename leaves **29** lines.
The 36 excluded lines are non-executable — XML doc comments, brace-only lines that emit no
sequence point, blank lines, and the two-part string constant — and are correctly outside
the denominator.

CHANGED_LINES_VALID: 29

CHANGED_LINES_COVERED: 27

CHANGED_LINE_COVERAGE_PERCENT: 93.10

93.10 is at or above the required 90.0.

## Per-line detail, so a third party can re-derive the figure

| Line | Hits | Marker | Source |
|---|---|---|---|
| 154 | 1 | COVERED | `internal Action<string> UserDiagnosticAction { get; set; } = text => MessageBox.Show(text);` |
| 281 | 1 | COVERED | `{` (helper body open) |
| 283 | 1 | COVERED | `{` (try block open) |
| 284 | 1 | COVERED | `archiveRoot = Globals.Ol.ArchiveRootPath;` |
| 285 | 1 | COVERED | `return true;` |
| 287 | 1 | COVERED | `catch (InvalidOperationException ex)` |
| 288 | 1 | COVERED | `{` |
| 289 | 1 | COVERED | `archiveRoot = null;` |
| 290 | 1 | COVERED | `logger.Warn(` |
| 291 | 1 | COVERED | `"Cannot resolve the Outlook archive root. Details are withheld from this "` |
| 292 | 1 | COVERED | `+ "message because they contain a mailbox address.",` |
| 293 | 1 | COVERED | `ex` |
| 294 | 1 | COVERED | `);` |
| 295 | 1 | COVERED | `return false;` |
| 297 | 1 | COVERED | `}` |
| 327 | 1 | COVERED | `if (!TryGetArchiveRoot(out var olAncestor))` (move path) |
| 328 | 1 | COVERED | `{` |
| 329 | 1 | COVERED | `return false;` |
| 339 | 1 | COVERED | `OlAncestor = olAncestor,` (move path) |
| 356 | 1 | COVERED | `if (!TryGetArchiveRoot(out var olAncestor))` (Outlook open path) |
| 357 | 1 | COVERED | `{` |
| 358 | 1 | COVERED | `UserDiagnosticAction(ArchiveRootUnavailableMessage);` |
| 359 | 1 | COVERED | `return;` |
| 366 | 0 | UNCOVERED | `OlAncestor = olAncestor,` (Outlook open path) |
| 380 | 1 | COVERED | `if (!TryGetArchiveRoot(out var olAncestor))` (file-system open path) |
| 381 | 1 | COVERED | `{` |
| 382 | 1 | COVERED | `UserDiagnosticAction(ArchiveRootUnavailableMessage);` |
| 383 | 1 | COVERED | `return;` |
| 390 | 0 | UNCOVERED | `OlAncestor = olAncestor,` (file-system open path) |

## The two uncovered lines

Lines 366 and 390 are the `OlAncestor = olAncestor,` initializer members on the **success**
branch of `OpenOlFolderAsync` and `OpenFsFolderAsync`. No test in this change reaches them:
each folder-open test either fails the archive-root guard (lines 356-359 and 380-383) or
fails the OneDrive guard above it, and driving the success branch would require constructing
a real `EmailFiler` against a live Outlook folder, which is a COM dependency the test policy
forbids. The move path's equivalent line, 339, is covered because
`MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` reaches it before
`EmailFiler.SortAsync` raises its null reference.

## Note on the seam's default lambda

The plan anticipated that the lambda body of [P2-T1]'s default seam value would be
uncovered, because every test replaces the seam before invoking the paths that use it. The
observed figure is more favourable than that: line 154 reports `hits=1`. The property's
initializer runs on every construction, and CSharpier keeps the declaration and its lambda
body on the same source line, so the emitted sequence point covers the line even though no
test executes `MessageBox.Show`. The line is therefore reported COVERED above; had the
formatter split it, the lambda body would have appeared as a third uncovered line and the
figure would be 27 of 30, or 90.00, still at or above the floor.
