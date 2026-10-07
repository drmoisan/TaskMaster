---
name: backtick-span-grep-gap-matches
description: Enumerating slash-bearing inline code spans with Grep -o over `[^`]*/[^`]*` returns prose gaps between spans and can skip a span that follows a gap; pwsh classifier is refused in isolated worktrees
metadata:
  type: project
---

When a preflight must re-check blast-radius extraction (Get-PathTokenKind in .claude/lib/blast-radius/BlastRadiusExtraction.psm1) by hand, the Grep `-o` pattern for backtick spans containing a slash also matches the prose GAP between two spans whenever the gap has a slash (the match starts at a closing backtick). The gap match consumes the next span's opening backtick, so a slash-bearing span directly after a slash-bearing gap is not listed.

**Why:** the isolation guard refuses `pwsh` from Bash, so the real classifier cannot be run; the manual Grep is the only enumeration, and its gap artifacts look like spans (issue #928 round-5 preflight, 2026-09-28).

**How to apply:** treat any match that begins with a backtick followed by prose, or ends with an opening backtick, as a gap; for every gap, check that the span immediately after it has no slash. Also check config/blast-radius.json: separator-free `shared_surfaces` entries are accepted as root surfaces, and `mandate_reads` (which includes scripts/vscode/** on this repo) drops matching tokens from the harvest. See [[preflight-evidence-field-token-scan]].
