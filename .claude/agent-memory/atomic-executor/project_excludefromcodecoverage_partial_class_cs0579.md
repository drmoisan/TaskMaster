---
name: excludefromcodecoverage-partial-class-cs0579
description: Applying [ExcludeFromCodeCoverage] to a partial class on more than one part (code-behind + Designer) is a CS0579 duplicate-attribute build break; annotate the partial type once
metadata:
  type: project
---

`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` is not `AllowMultiple`. Many WinForms/VSTO types in this repo are partial classes split across a hand-maintained `*.cs` code-behind and an auto-generated `*.Designer.cs` (e.g., `ThisAddIn`, `ProjectViewer`, the QuickFiler viewers `EfcViewer`/`QfcFormViewer`/`QfcItemViewer*`/`ItemViewer`). Applying the attribute to the same partial type in BOTH files produces `error CS0579: Duplicate '...ExcludeFromCodeCoverage' attribute`, which breaks the analyzer/nullable msbuild gate.

**Why:** the attribute targets the type, not the file; both partial declarations resolve to one type, so two attributes = duplicate.

**How to apply:** when exempting a partial type for coverage, add the attribute to exactly ONE part (prefer the hand-maintained code-behind `*.cs` for PR review visibility) and add the `using System.Diagnostics.CodeAnalysis;` there. A single annotation excludes the whole type (all parts' members). Do not annotate the Designer part as well. Coverage tooling confirms the whole type leaves the denominator with one annotation. See [[project_build_test_env]] for the dash-switch msbuild invocation that surfaces this.
