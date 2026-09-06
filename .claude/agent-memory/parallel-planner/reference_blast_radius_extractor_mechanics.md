---
name: blast-radius-extractor-mechanics
description: Get-PlanPaths only extracts backtick-delimited (code-span) paths, splits on spaces, ignores polarity, and drops any extension outside a closed 23-member allow-list (no resx/config/props/targets) — four independent silent fail-opens
metadata:
  type: reference
---

Verified 2026-08-21 against `.claude/lib/blast-radius/BlastRadius.psm1` on `main @ a01bdbb0`.

**`Get-PlanPaths` extracts a path only when it is wrapped in backticks (a Markdown code span).**
Measured on single-line inputs:

| plan text form | extracted |
| --- | --- |
| `` Edit `QuickFiler/Controllers/X.cs` `` | yes |
| `Edit QuickFiler/Controllers/X.cs to fix it.` (bare prose) | **no** |
| `- [ ] [P1-T1] Modify QuickFiler/Controllers/X.cs` (bare in a task line) | **no** |
| `Touches QuickFiler/**` (bare subtree glob) | **no** |

This is the highest-leverage fact about the whole surface, because it means **plan authoring style
decides whether contention is detected at all.** A child orchestrator whose atomic plan names files
in bare prose produces a radius containing nothing but the feature-folder glob
`docs/features/active/<name>/**`, so it reports `conflict=False` against every other item and gets
co-scheduled with items it genuinely collides with. That is a silent fail-OPEN with no error and no
warning. Real committed plans in this repo do use backticks (86 and 47 paths extracted from two
sampled plans), so the convention holds in practice — but it is a convention, not an enforced
invariant.

**A path containing a SPACE is silently split and lost (verified 2026-09-02, item #707).** Extraction
harvests whitespace-free tokens, so the backticked token `UtilitiesCS/To Depricate/FileIO2.cs` was
split at the space and emitted as the fragment `Depricate/FileIO2.cs`, which names no tracked file.
The production file the plan actually rewrites was therefore absent from the radius entirely — a
silent fail-OPEN identical in effect to the bare-prose case, but invisible because the plan author
did use backticks. This repository has at least one such path (`UtilitiesCS/To Depricate/`), so
check for it whenever a radius omits a file you know the plan edits. The rule file records the
whitespace split as a known residual for PLACEHOLDER forms; this is the same mechanism biting a real
path, and it is not benign in that direction. Hand-append the exact space-bearing path.

**The recognized-extension allow-list is CLOSED and omits four extensions this repository uses
heavily (verified 2026-09-02, item #729).** `$script:RecognizedPathExtension` at
`.claude/lib/blast-radius/BlastRadiusExtraction.psm1:87-93` is a 23-member `HashSet`:

```text
cfg cs csproj ini js json jsx lock md ps1 psd1 psm1 py sh sln toml ts tsx txt xml yaml yml
```

A wildcard-free token whose final component's extension is outside that set is classified as a
DIRECTORY reference and dropped, however well-formed the path is. Measured: backticked
`UtilitiesCS.Test/Form1.resx` and `TaskMaster.Test/packages.config` both harvest to nothing while
`UtilitiesCS.Test/Form1.cs` on the same input harvests fine. The set is hardcoded, not a
`config/blast-radius.json` key, so it cannot be corrected from the destination workspace.

Missing extensions that matter here: `resx` (every WinForms resource file), `config`
(`packages.config`, carried by every non-SDK-style .NET Framework project in the repo), and `props`
/ `targets` (`Directory.Build.props`, `Directory.Build.targets`). This is a third fail-OPEN in the
same family as the bare-prose and space-split cases: the plan author did everything right and the
write is still invisible to V1, V2, and V3, because the validator extracts from the same text with
the same allow-list and is therefore self-consistent.

**The extractor has no notion of POLARITY.** A backticked path in a sentence saying the plan will
NOT touch it is harvested exactly like one saying it will. See
[[never-backtick-exclusion-paths-in-delegation-prompts]] for the delegation-prompt rule this forces
and for the measured blast radius it produced.

Two related gotchas:

- **Exported parameter names are `-RadiusA`/`-RadiusB`**, not `-A`/`-B`, on
  `Test-BlastRadiusConflict`. The facade exports `Get-PlanPaths`, `Get-BlastRadius`,
  `Get-BlastRadiusFromObservedPaths`, `Get-NormalizedDeclaredRadius`, `Test-BlastRadius`,
  `Test-BlastRadiusConflict`. Note the normalizer is `Get-NormalizedDeclaredRadius`, not the
  `normalize_declared_radius` name the skill prose uses.
- **`$c.reasons` holds hashtables**, so `$c.reasons -join ','` prints
  `System.Collections.Hashtable`. Expand with `$_.kind` and `$_.detail`.

**How to apply:** when auditing a derived radius that looks suspiciously empty, check the plan for
bare-prose paths before concluding the item is genuinely independent. When writing probe scripts in
PowerShell, remember the backtick is also the PowerShell escape character — build test strings with
`[char]0x60` rather than inline backticks, and avoid `-notmatch '*\evidence\*'` style patterns whose
trailing backslash is an illegal regex. See [[parallel-surface-partial-port]] for the contention
defects this interacts with.
