---
name: cobertura-per-file-rates-corrupted-441
description: Open issue #441 makes Merge-CoberturaClassesByFilename rewrite @line-rate/@branch-rate with a defective .// selector, corrupting PER-FILE rates too — derive rates from class/lines/line deduped by max(hits), never from the rate attributes
metadata:
  type: project
---

Issue #441's title says the defect inflates the repo total. It also corrupts **per-file** rates, which
makes it a direct threat to any numeric coverage acceptance criterion.

`Merge-CoberturaClassesByFilename` (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:167-292`)
rebuilds a merged class's `<lines>` correctly, then rewrites its `@line-rate`/`@branch-rate` using a
`.//lines/line` descendant selector that double-counts.

**Why:** Proven by exact arithmetic on `FilerQueue.cs` in the committed #424 Cobertura report —
recorded `line-rate="0.405797"` = 28/69, true rate 18/49 = 0.367347; recorded
`branch-rate="0.428571"` = 6/14, true rate 5/10 = 0.5. Both match the defect formula to six decimals.
A consequence worth remembering: the `quickfiler-per-file-coverage` epic manifest's own measured
baseline table cites these corrupted figures, so the epic's stated starting numbers are unreliable.

**How to apply:** When a plan or AC depends on per-file coverage numbers, derive them from the
direct-child axis `class/lines/line`, grouped by `@filename`, deduplicated by `@number` taking
`max(@hits)`. Never read `@line-rate`/`@branch-rate`, and never use a `.//` descendant selector.

Two related requirements: one source file can emit multiple `<class>` elements sharing a `filename`
(async state machines, `<>c` closure classes), so union them; and decide whether a file has a
denominator from its `<line>` child count, never from `line-rate`, because a declaration-only file
reports `line-rate="0"` for having no lines rather than for being uncovered.

Detection tell: `Get-CoberturaCoverageSummary` rounds to 6 decimals while dotnet-coverage emits full
precision, so a 16-digit rate was never merged and is trustworthy; a 6-decimal rate went through the
defective path.

Absence of a file from the report is NOT 0% and NOT coverage — see
[[excludefromcodecoverage-partial-type-trap]].
