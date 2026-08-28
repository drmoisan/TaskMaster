---
name: csharpier-skips-designer-cs-by-filename
description: CSharpier 1.2.6 skips *.Designer.cs via generated-file detection, not .csharpierignore; prove it with a single-file check reporting "Checked 0 files"
metadata:
  type: project
---

CSharpier 1.2.6 does **not** format `*.Designer.cs`. The skip comes from its built-in generated-file
detection on the filename, NOT from `.csharpierignore` (which lists only `**/evidence/**`,
`*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`, `*.targets`) and
not because the files happen to be formatted.

**Why:** A repo-wide `csharpier check .` reporting zero unformatted files is ambiguous — it cannot
distinguish "processed and already clean" from "not processed at all". The distinction matters
whenever a plan gates a `.Designer.cs` edit on the risk of a whole-file reformat of a 6000-line
generated file destroying a one-line diff.

**How to apply:** Run `dotnet tool run csharpier check <the-single-file>`. It prints
**`Checked 0 files`** for a Designer file and `Checked 1 files` for a normal one. That is the
falsifiable proof. Strengthen it by measuring a line that exceeds the 100-column default print width
(there is no `.csharpierrc` in this repo): `QuickFiler/Viewers/ItemViewer.Designer.cs:256` is 111
columns and `ItemViewerExpanded.Designer.cs:274` is 110, so either would be re-wrapped if processed.

Related: [[project_csharpier_pipefiles_nonenforcing_gate]],
[[project_count_idiom_pitfalls_csharpier_and_measureobject]],
[[project_csharpier_formats_xml_print_width]]
