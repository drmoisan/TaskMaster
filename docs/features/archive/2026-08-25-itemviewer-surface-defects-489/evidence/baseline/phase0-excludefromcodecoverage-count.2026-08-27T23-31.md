# Phase 0 — Repository-wide [ExcludeFromCodeCoverage] Count (P0-T16)

Timestamp: 2026-08-27T23-31
Command: git grep -n -E "\[(System\.Diagnostics\.CodeAnalysis\.)?ExcludeFromCodeCoverage\]" -- "*.cs" | Measure-Object | Select-Object -ExpandProperty Count
EXIT_CODE: 0

BaselineExcludeAttributeCount: 261

## Both spellings are present among the hits

The acceptance condition requires the summary to name a hit in each spelling, which is what proves the
pattern is blind to neither form:

- **Unqualified** — `QuickFiler/Viewers/ItemViewer.cs:20` reads `[ExcludeFromCodeCoverage]`. This is
  the attribute spec AC55 requires to remain unchanged, and it is the reason no fix inside any
  `ItemViewer*.cs` partial can be proved by a coverage delta.
- **Fully qualified** — `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:60` reads
  `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`. Four further fully-qualified
  applications sit in the same file at `:83`, `:97`, `:111` and `:125`.

## Why the alternation is load-bearing, measured

The two counts were taken side by side on this branch:

| Pattern | Count |
|---|---:|
| `git grep -n -E "\[(System\.Diagnostics\.CodeAnalysis\.)?ExcludeFromCodeCoverage\]" -- "*.cs"` | **261** |
| `git grep -n -F "[ExcludeFromCodeCoverage]" -- "*.cs"` | 249 |
| difference | **12** |

The measured difference is **12**, exactly the figure the plan states. All twelve are real attribute
applications written in the fully-qualified form, and five of the twelve sit in
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs` — the very file P2-T5 adds
`CbxPictures_CheckedChanged` to, and the form every neighbouring member of that file already uses.

A fixed-string count of the unqualified spelling alone would therefore return `249` whatever the
executor wrote into that file, so spec AC55 and AC56, which rest on this count and on P11-T10's
recount, would be unfalsifiable. The alternation is what makes them capable of failing.

## Count idiom

The `git grep` is wrapped in `(... | Measure-Object).Count`, which makes the pipeline's own exit code
`0` regardless of whether `git grep` found matches. That is why this artifact declares no
`ExpectedExitCode:` field and records `EXIT_CODE: 0`.

Output Summary: The repository-wide occurrence count of the coverage-exclusion attribute, counted in
**both** spellings across `*.cs`, is **261**. A fixed-string count of the unqualified spelling alone
returns **249**, a difference of **12**, confirming the alternation is load-bearing exactly as the
plan states. Both spellings are named among the hits: the unqualified form at
`QuickFiler/Viewers/ItemViewer.cs:20` and the fully-qualified form at
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs:60`, with four more at `:83`, `:97`, `:111`
and `:125` in that same file. P11-T10 recounts with the identical pattern and spec AC56 requires the
post-change count to be **not greater than** 261.
