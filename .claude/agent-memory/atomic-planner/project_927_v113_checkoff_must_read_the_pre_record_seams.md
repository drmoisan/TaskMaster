---
name: project-927-v113-checkoff-must-read-the-pre-record-seams
description: "#927 revision 1.13 (confirming-preflight round after the two-point census was introduced): the four sibling invalidations a two-point census leaves behind (the AC check-off task still reading bare post zeros; 'reads files only' contradicted by the payload's own Tee-Object -Append; an 'expected 1' parenthetical citing the superseded baseline equality; a spec Assumptions bullet still carrying the brief's zero) and how each was corrected without touching anything else"
metadata:
  type: project
---

Seams from the revision-1.13 round of plan 927 (coordinator-approved F1 to F4 from the confirming preflight over 1.12). Root cause of all four: revision 1.12 rebuilt the P4-T8 census as a two-point (PRE-CLEANUP / POST-CLEANUP) record but the sibling text that consumed or described the census was not swept.

**F1: the check-off task must read the pre record, not only the post zeros.** When a gate task is rebuilt so its post-cleanup zero is only meaningful beside a pre-cleanup 1, the later AC check-off task that reads the same artifact must require the `PRE-CLEANUP:` record (`RX-LENGTH=40`, `UTF16-PROFILE-FILES=1`, no `PRE-CLEANUP-CENSUS: NOT OBSERVED` line) and the `POST-CLEANUP:` record separately, and must state that the pre lines reading 1 are expected and not a residual. Before writing, check every label in the supplied text against the literal labels the producing task writes; declare "match, no adjustment" explicitly.

**F2: "reads files only" is false for a Tee-Object -Append payload.** Say "reads repository files only, and its only write is the append to SCRATCH\<file>". The sibling words "read-only" in the preceding task's cross-reference sentence and in the earlier round record describe the repository and were reported, not edited (exact-four-corrections mandate).

**F3: an "expected 1" parenthetical must not cite a superseded baseline equality.** After a baseline row is declared an undercount, grep every task whose expectation was justified by that row ("P0-T17 expects X equal to Y") and re-point it at the re-measurement rows (P1-T12 `-NOW=1`, the PRE-CLEANUP census) while naming the old row as the undercount.

**F4: spec Assumptions bullets carry the brief's figures too.** A measurement correction that amended an AC also invalidated an Assumptions bullet ("no UTF-16 tracked file carries an identifier (the brief recorded zero binary-only hits)"). Correct it to the verified fact, name the archive path in plain prose (never the profile path), add a dated supersession note, do not attribute the brief's zero to a cause that was not verified, and re-derive the anchored AC count (20).

**Mechanics.** The plan's working copy was CRLF on every line and spec.md LF; the Edit tool preserved both, verified by `\r$` count equal to the total line count (plan) and zero (spec). Glob still does not enumerate under `.claude/worktrees/`; a case-insensitive Grep over the UTF-16 file (100 hits) is the no-channel corroboration. The validator MCP tool was absent from the tool surface; say so and hand off.

**How to apply:** after any round that changes the shape of an evidence record (one figure to a two-point record, a bare line to a labelled record), sweep every consumer of that artifact (check-off tasks, "expected N" parentheticals, spec assumptions, closing-status prose) in the same round, not the next.
