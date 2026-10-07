---
name: 968-r5-multi-pass-format-exemption-and-no-bash-cr-check-seams
description: Issue #968 preflight round 5 — a post-format census exemption keyed to the current formatter pass fails on any D-13 restart; the fix is a union-of-prior-passes label plus a correction-set label, and a third-pass residual remains; also, Bash can be disabled for the planner session so the pwsh CR check needs a ripgrep substitute
metadata:
  type: project
---

Round 5 of #968 (2026-10-03) found one defect: P6-T2 allowed a LINES value or `SPAN:` range to differ from its Phase 1-5 record "only if `REWRITTEN:` named the file", and P6-T1 defined `REWRITTEN:` as the paths whose hash changed in *this* formatter pass. On a second pass (Phase 6 restart or Phase 8 restart to P6-T1) the files the first pass re-laid are already formatter-stable, so the new pass does not name them, their values still differ from the record, and the gate fails with nothing wrong in the tree. The correction that triggered the restart has the same problem.

**Why:** a restart re-runs the scoped format, and a formatter rewrite is idempotent, so "rewritten this pass" is a one-shot observation. Any exemption keyed to it is satisfiable exactly once per run.

**How to apply:**
- When a census compares post-format values against a pre-format record and the plan has a restart path that re-enters the format task, the exemption must name three sets: the current pass's `REWRITTEN:`, a `PRIOR-PASS-REWRITTEN:` union of every earlier pass in the run, and a `RESTART-CORRECTED:` set (the Write Set paths the triggering correction edited). The restarted artifact carries the union forward because the artifact path is overwritten in place.
- Residual the round-5 delta did not close (applied verbatim on instruction, reported as advisory): `RESTART-CORRECTED:` as worded names only the correction that triggered *this* restart. On a third pass, a file edited by the first correction and not re-laid by the formatter is named by none of the three labels and fails again. The complete wording is "the union of the Write Set paths edited by every correction that triggered a D-13 restart in this run". Author it that way the first time. (The orchestrator accepted the advisory as a knock-on and a second planner pass in the same round applied the union wording to P6-T1, the round-5 revision-record bullet, the Status line and the self-review clause; no acceptance value other than the label definition changed.)
- Line citations drift by one when a round inserts a revision-record bullet near the top of the plan: the round-5 bullet at line 25 moved D-13 from 156 to 157, and both the caller's knock-on instruction and the in-plan round-5 clause still said 156. Re-derive every "line N" a caller supplies against the current file before writing it into a self-review clause, and label which numbering a clause uses when it cites a prior round's numbers.
- A `REWRITTEN:` reference that gates whether a *build* is fresh (P6-T3's `PROD_DLL_ADVANCED` when a production file was rewritten) correctly stays current-pass; do not sweep it into the union.
- Related: [[968-r3-last-recorded-value-rule-and-merged-payload-hook-seams]], [[968-r4-restart-path-reanchor-and-recorded-not-gated-exemption-seams]].

Session mechanics learned in the same round:
- The planner session had no Bash tool at all ("Bash is disabled for this session, in subagents as well as here"), so the caller-specified `pwsh ... ReadAllBytes` CR count could not run. Grep (ripgrep) for `\r` and `\x0D` in count mode over the plan is the available substitute; report the substitution rather than claiming the byte count ran. A `\r$` match is not enough on its own, because ripgrep's `$` handling of CR varies; use the unanchored form.
- The plan's self-review preamble claimed the records were "repeated verbatim" in `planner-review.<ts>.md`. A round whose instruction confines edits to the plan file breaks that claim as soon as the in-plan record is extended; amend the preamble to say the new round's form is carried in the plan only, rather than leaving a false verbatim-copy statement or touching the evidence file.
