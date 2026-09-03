# Finding 1 — CustomUI Document Format Pass (P1-T5)

Timestamp: 2026-09-03T01-40
Task: [P1-T5]
Command: `Get-FileHash -Algorithm SHA256 -LiteralPath TaskMaster/Ribbon/RibbonExplorer.xml`, then `dotnet tool run csharpier format TaskMaster/Ribbon/RibbonExplorer.xml`, then the same hash again.
EXIT_CODE: 0

The format pass is mandatory after the CustomUI edit: `.csharpierignore` excludes project, props and
targets files, not XML, so this document is formatter-visible and CSharpier owns its layout.

## SHA-256, before and after

| Point | SHA-256 |
|---|---|
| Before the format run | `6C2673485DCBC716E1DAD38803A8A1AAC91F918F943DD0C417796388732326C9` |
| After the format run | `6C2673485DCBC716E1DAD38803A8A1AAC91F918F943DD0C417796388732326C9` |

The two hashes are identical, so CSharpier rewrote nothing. The four attribute-value renames of
P1-T3 preserved the existing attribute-per-line layout, and the whole-element deletion of P1-T4
removed a line that was already formatted. Line count after the run: 544, one fewer than the
baseline 545 recorded in P0-T10, which is the single deleted element line.

## Why the console line is not used as the rewrite signal

CSharpier printed `Formatted 1 files in 686ms.` That figure is the number of files CSharpier
PROCESSED, not the number it CHANGED, so a one-file run always prints 1 whether or not it rewrote
anything. Keying a rewrite decision on it would have reported a rewrite here, which the identical
hashes disprove. The before-and-after hash pair is the observation of record.

Any reflow the formatter produces is accepted by the plan's design decision 1; none occurred. The
semantic check that the edit is exactly four renames plus one element removal is P1-T9, which
compares element and attribute sets rather than lines and is therefore reflow-independent either
way.

Output Summary: The formatter ran with EXIT_CODE 0 over the CustomUI document and rewrote nothing —
the SHA-256 is identical before and after. The file is 544 lines, one fewer than the 545-line
baseline, accounting for the deleted button element.
