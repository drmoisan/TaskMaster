---
name: preflight-fullname-claude-exclusion-and-displayname-census
description: Two census defects found in the #959 plan preflight (2026-10-02) - a FullName "*\.claude\*" skip empties every repo-wide count inside an item worktree, and DataRow DisplayName strings inflate method-name token counts
metadata:
  type: project
---

1. **FullName exclusion inside an item worktree.** Item worktrees live under
`<repo>\.claude\worktrees\agent-*`, so a payload filter such as
`$f.FullName -like "*\.claude\*"` matches EVERY file and the repo-wide counts it feeds read 0.
In the #959 plan this made `DEAD-MEMBERS-CS` and `TODOMODEL-CSPROJ-MATCHES` unsatisfiable at base
and vacuous at final. **How to apply:** in preflight, check every Get-ChildItem exclusion in a
payload; the exclusion must test the root-relative path (`$f.FullName.Substring($root.Length)`),
as the coverage discovery payload already does. Require a positive-control count (for example
`CS-FILES:` at least 1). Related: [[glob-tool-blind-under-claude-worktrees]].

2. **DisplayName strings in census tokens.** A whitespace-stripped token census over a test file
counts the method name inside every `DisplayName = "<method> [row]"` string, so a four-row
data test reports the name 5 times, not 1. **How to apply:** for each test-name token in a
census table, add one per DataRow DisplayName that embeds it.

Also seen in the same pass: recount table BASE values against comments and field initializers
(a field initializer `_x = Enum.Empty;` matches a reset-statement token), and count every
`Times.Exactly(n)` occurrence instead of trusting the planner's line list.
