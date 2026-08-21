---
name: blast-radius-extractor-mechanics
description: Get-PlanPaths only extracts backtick-delimited (code-span) paths from plan text, so plan authoring style silently decides whether blast-radius contention fires at all
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
