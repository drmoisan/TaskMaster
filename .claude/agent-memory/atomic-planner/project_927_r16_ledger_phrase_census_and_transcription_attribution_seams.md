---
name: project-927-r16-ledger-phrase-census-and-transcription-attribution-seams
description: "#927 round 16 (v1.19): count a ledger phrase on EVERY line including the summary UNMET parenthetical, not just the list entry and the per-criterion line; a figure an acceptance requires in an artifact must either be read by a step or be marked as a transcription with its source; when a prior round named files 'by count' because it had no command channel, expect preflight to run the diff and demand the names; a deferred check-off task's own acceptance must carry the guard reading (MAIN-MOVED) the producing task states"
metadata:
  type: project
---

Four seams from confirming preflight round 15 over #927 revision 1.18 (the four deltas were all
wording; convergence was declared).

1. **Phrase census must include the summary line.** The ledger's `UNMET:` summary line carries
   the same `AC4: PENDING P6-T38` parenthetical as the `Items remaining` entry and the
   per-criterion line, so the phrase occurs on THREE lines (9, 17, 25), not two. Round 15 counted
   only the two list-style lines. When a task says "match by content", count with a search over
   the whole file and cite every line number, and make the tick branch assert the summary line too
   ("its `UNMET:` line names no AC4").
2. **A required-in-artifact figure needs a reader or an attribution.** P6-T39's acceptance
   required run 36484682458's figures in the artifact, but no step read that run. The fix is not a
   new gh step: mark the figures "(transcribed from this plan, not re-read by this task; read from
   that run's log by preflight round N on <date>: head <40-char>, branch, event, conclusion,
   the verbatim lines)". Anything an acceptance names must be either observed by a step or
   explicitly labelled as a transcription with its provenance.
3. **"Named by count" is a placeholder preflight will fill.** Round 15 (no command channel) wrote
   "two QuickFiler Search part files"; preflight ran `git diff --name-status` and returned the two
   names. When the session cannot run a diff, say so AND expect the confirming pass to supply the
   names; then name them in plain prose everywhere the count appeared (D-decision, task prose,
   artifact wording line, CITATION line) and state which sibling was NOT changed.
4. **Deferred check-off tasks carry the guard themselves.** P6-T20 executes inside P6-T38, and
   P6-T38 stated the `MAIN-MOVED=0` tick condition and the `BASELINE-SCOPE: MAIN MOVED` reason,
   but P6-T20's own acceptance did not. A sibling-region claim "P6-T20 checked" was false because
   only the producing task was read. Put the guard reading in both tasks' acceptance text.

**Why:** each was a cross-reference the prior round's self-review claimed to have checked; each
surfaced as a preflight delta. Round count for this plan reached 16.

**How to apply:** on a check-off/ledger revision, grep the ledger for the full phrase and cite all
hits; on any "artifact records X" clause, find the step that reads X or add the transcription
label; on any count-only file reference, plan to replace it with names on the next pass; on any
deferred task, edit the deferred task's acceptance, not only its host.

Related: [[project-927-r12-three-dot-anchor-and-control-ref-ancestry-seams]],
[[project-927-v113-checkoff-must-read-the-pre-record-seams]].
