---
name: project-927-v112-two-point-census-and-ac-amendment-seams
description: "#927 revision 1.12 (root-cause correction after the Phase 0 backslash-collapse undercount): how to make a post-cleanup zero-count gate able to fail with ONE byte-identical payload run at two points (& { census } 2>&1 | Tee-Object -Append to SCRATCH; first four lines = pre, last four = post), why the pre-run needs a cross-reference sentence in the preceding read-only task, the $b collision when adopting the [char]92 construction, ripgrep transcoding UTF-16 BOM files as a no-channel corroboration, the AC-amendment note placement, and keeping a coordinator-supplied literal while rewording around it when the literal alone would be stale"
metadata:
  type: project
---

Seams from the revision-1.12 round of plan 927 (coordinator-approved R8-01 to R8-06, one root cause: the P0-T17 UTF-16 census's `[\\/]` class collapsed between Bash and pwsh, so the baseline undercounted by one UTF-16 profile-path file, 1213 versus 1214).

**Two-point census with one payload (R8-02).** A post-cleanup `UTF16-PROFILE-FILES=0` gate is vacuous unless the same census printed 1 before the cleanup. To keep the two runs byte-identical, wrap the reviewer-verified statements as `& { ... } 2>&1 | Tee-Object -Append -FilePath (Join-Path (Join-Path $env:TEMP "hygiene-927") "utf16-census.txt")`; the pre run appends lines 1 to 4, the post run lines 5 to 8; the task reads the file back (`CENSUS-FILE-LINES=`, `CENSUS| n | line`) and records the first four under `PRE-CLEANUP:` and the last four under `POST-CLEANUP:`; a re-run appends four more, so "last four" is always the latest post record. State in the task exactly when the pre run executes (after the last read-only task's command, immediately before the writing task's first command) and where the lines are held (SCRATCH, which the helper never touches). Because an executor works in task order and reaches the writing task before reading the later task, add ONE cross-reference sentence to the preceding read-only task (declare it in the return). A missing or non-1 pre record is `PRE-CLEANUP-CENSUS: NOT OBSERVED` and "acceptance not met, no stop" when the stop list (C12) may not change.

**`$b` collision.** The `[char]92` construction binds `$b`; a census that already used `$b` for the byte array must rename it (`$bytes`) or the BOM test reads the backslash character.

**No-channel corroboration of a UTF-16 census.** ripgrep transcodes a byte-order-marked UTF-16 file, so `Grep -i users` over the file returns line counts (100 here) and corroborates the reviewer's `UTF16-PROFILE-FILES=1` without a command channel. The Glob tool still does not enumerate under `.claude/worktrees/`.

**AC amendment mechanics (R8-01).** Replace only the reference-figure clause (equality stays exact); put the dated note as a paragraph between the `## Acceptance Criteria` heading and the first `- [ ] AC1` line (adjacent, not a checkbox, not a nested bullet), name the defect and the evidence path in the sibling backticked style, and re-derive the anchored checkbox count (`^- \[[ x]\] AC` = 20) before and after. Record the coordinator's authorisation in the self-review paragraph.

**Coordinator literal that would be stale.** R8-03 supplied `this round-8 revision of the file` for a paragraph that, after the round, must describe revision 1.12. Keep the supplied literal verbatim (the reviewer greps for it) and reword the surrounding clause so the sentence is true ("the preflight pass over this round-8 revision of the file returned R8-01 to R8-06"), then append the round history; declare the rewording in the return.

**R8-06 sweep report.** After replacing the last `.NET` operand, `git grep -E` operands legitimately remain (POSIX bracket expressions are collapse-proof); enumerate every remaining hit by task and class (git grep, deliberate `RX-CANONICAL=` literal, completed-task Select-String probes, the defective completed census, prose) rather than claiming "none".

**How to apply:** any plan whose zero-count gate follows a rewrite should carry the identical-payload two-point form above; any plan adopting the `[char]92` construction must sweep the payload for a pre-existing `$b`; any coordinator-authorised AC amendment follows the note placement and count re-derivation here.
