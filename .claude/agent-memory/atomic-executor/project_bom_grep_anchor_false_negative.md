---
name: bom-grep-anchor-false-negative
description: GNU grep's ^ anchor fails to match immediately after a UTF-8 BOM, silently under-counting/misclassifying files; ripgrep (Grep tool) handles BOM correctly
metadata:
  type: project
---

Most `UtilitiesCS/**/*.cs` files in TaskMaster carry a UTF-8 BOM before their first line. Bash/GNU
`grep -rl "^#nullable"` (or any `^`-anchored pattern) does NOT match immediately after a BOM, so it
silently undercounts opted-in files and can misclassify a BOM-prefixed opted-in file as
non-opted-in. Concretely: `grep -l "^#nullable" ActionButton.cs` returned nothing even though line
11 is `#nullable enable`, because the file starts with an `efbb bf` BOM. Ripgrep (the `Grep` tool)
correctly skips the BOM and gave the accurate answer (400 opted-in files across
UtilitiesCS+SVGControl, not 156 as bash grep reported).

**Why this matters:** during #376 (utilitiescs-nullable-ci-capstone) Phase 4's genuine-enforcement
verification (AC2), the plan's illustrative non-opted-in candidate (`UtilitiesCS/Dialogs/
ActionButton.cs`) was actually opted-in; using bash grep to "confirm" it as non-opted-in would
have produced a false verification result (the deliberate defect would have failed the gate,
contradicting the P4-T5 acceptance criterion). Caught before the defect was introduced by
re-checking with the Grep tool.

**How to apply:** always use the `Grep` tool (ripgrep-based), never bash/GNU `grep`, for any
`^`-anchored line-start pattern search across this repo's `.cs` files — especially when the result
selects a candidate file for a verification step whose correctness depends on the classification
being accurate. If bash grep must be used for some reason, strip the BOM first or accept that
counts/candidate selections from it are unreliable and re-verify with ripgrep before acting.
