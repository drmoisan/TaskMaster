---
name: small-to-large-reroute-flips-work-mode
description: When a seeded small/minor-audit item turns out to need 4+ production files, flip issue.md to full-bug before prd-feature; also restamp executor evidence whose filename time is in the future
metadata:
  type: project
---

On #1005 (2026-10-22) a seeded `small` + `minor-audit` checkpoint had to become `large` once AC4
("fix every test that reaches Settings.Default.Save()") pulled in 7 production files. The large route
runs prd-feature, and a spec.md in a minor-audit folder is an integrity failure, so I edited the
issue.md marker to `- Work Mode: full-bug`, set checkpoint `work-mode`/`path_selected`/`route_id`, and
recorded a `route_change` block. spec.md then became the AC source (issue AC1-AC7 carried forward plus
AC8). The whole run (research, spec, plan, 2 preflights, execution, 1 remediation cycle, PR, CI) finished
in about 2.5 hours and `require_complete` + `require_model_routing` passed with
`required_mcp_tools` naming `new_potential_bug_entry`.

**Why:** the user's seed said "switch to large if >3 files"; doing the work-mode flip up front avoided
the prd-feature/minor-audit conflict. Give the researcher the 11-label `## Numeric Derivation
Evidence` spec in its prompt (see [[prd-feature-stop-hooks-are-workmode-blind]]); even so it named two
labels `Primary Search Strategy` instead of `... or Query Expression`, so prd-feature wrote count-free ACs.

**How to apply:** after every executor return, compare evidence filename stamps with file
LastWriteTimeUtc; a sonnet remediation executor stamped 11 files 10-20 minutes in the future. Rename to
the write time and fix the `Timestamp:` line before committing. See [[evidence-timestamps-can-be-synthesized]].
