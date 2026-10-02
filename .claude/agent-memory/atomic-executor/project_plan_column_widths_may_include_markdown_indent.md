---
name: project-plan-column-widths-may-include-markdown-indent
description: Plan column-width claims for delivered C# source may count the Markdown code-block indent; re-measure in-file and use repo CSharpier precedents for chain layout
metadata:
  type: project
---

In the #942 plan (round 3 preflight, 2026-09-30), the planner's self-review stated 100-column fit figures for delivered new-partial lines that were 4 columns too high on most lines: it measured with the 4-space Markdown indent that the plan strips when the file is written (for example 99 claimed, 95 in-file). The error was conservative, so no gate was affected.

**Why:** a width claim decides whether CSharpier keeps a reason string on the same line as its call, and single-line token gates depend on that. A claim measured in the wrong frame can be wrong in either direction.

**How to apply:** when a plan asserts that a delivered line fits in 100 columns, re-measure it at the in-file indentation, not the plan's. For FluentAssertions chain layout, cite repository precedents instead of reasoning from memory of CSharpier's algorithm. Examples: `harness` / `.Errors[0]` / `.Exception.Should()` / `.Call(...)` in EngineToggleStateCoordinatorTests.Race.cs 224-229; `x` / `.Should()` / `.NotBeSameAs(` with arguments on separate lines at Race.cs 240-245; and `+ "..."` concatenation operands indented 4 inside broken argument lists. The default width is 100 because the repository has no .csharpierrc and no max_line_length. See [[project-csharpier-chain-wrap-defeats-singleline-search-gates]].
