---
name: 959-r5-bash-dedoubles-backslashes-in-pwsh-command-payloads
description: Preflight round 5 seam on #959 - a doubled backslash inside a `pwsh -NoProfile -Command '...'` payload issued from Bash arrives de-doubled, so a regex class like `[\\/]` matches only `/` and a zero-count gate over backslash paths can never reach 0; normalize with `[char]92` and a forward-slash pattern and add two discriminating controls
metadata:
  type: project
---

A doubled backslash in a `pwsh -Command` payload run through the Bash tool arrives de-doubled. The #959 plan's `CMD-EVIDENCE-FIELDS` used `-notmatch "[\\/]evidence[\\/](baseline|regression-testing|qa-gates)[\\/]"`; through the channel it became `[\/]`, matched only `/`, and every backslash repository-relative path counted as noncanonical, so `NONCANONICAL-SUBFOLDER-FILES: 0` was unsatisfiable. Reading the regex (revision 1.3 self-review said it "admits exactly the three subfolders") did not catch it; the preflight reviewer caught it by running the payload through the plan's own channel.

**Why:** Git Bash / MSYS argument conversion collapses `\\` to `\` before pwsh sees the command string (same mechanism as the #927 R9 note, now observed inside a regex character class rather than a path). A single backslash passes through unchanged, which is why `\w`, `\d`, `\r\n` and `scripts\vscode\...` path literals in the same plan were fine.

**How to apply:**
- Never write `\\` inside a pwsh payload string or regex. Build paths/regexes backslash-free: `.Replace([string][char]92, "/")` on the path, then a forward-slash pattern (`"/evidence/(baseline|regression-testing|qa-gates)/"`); supply a literal backslash with `[char]92`.
- Pair every such filter with two printed controls built from the same pattern variable, one that must print `True` (a path that should be flagged) and one that must print `False` (a canonical path), each mapped to a `... NOT DISCRIMINATING` stop in every task that gates the count. Same pattern as the `$anchor` field-check controls of R4.
- State the rule once in the plan's Command channel convention (next to the "no double-quoted literal ends in a backslash" rule) so the sweep for `\\` over payload lines has a rule to cite.
- The sweep is a whole-plan Grep for `\\\\`; classify each hit as payload (fix), Listing written by the Write tool (C# verbatim strings never pass through pwsh: leave), or prose quoting a script regex (leave).
- A planner without a shell cannot evaluate a payload; say so in the self-review and record the reviewer's channel observation as the reviewer's, not as the planner's.
