---
name: mcp-plan-validator-requires-lf
description: CRLF once broke the MCP plan validator, but as of 2026-08-07 it ACCEPTS CRLF; treat CRLF as a suspect only when headings/tasks fail, and verify by running the validator rather than pre-normalizing
metadata:
  type: feedback
---

> **SUPERSEDING FINDING (2026-08-07, epic #136 child F8 / issue #437).** The current validator
> build **ACCEPTS CRLF**. Verified empirically: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/plan.2026-08-06T21-17.md`
> is CRLF in the working tree (`file` reports "with CRLF line terminators", and the repo has
> `* text=auto` in `.gitattributes` with `core.autocrlf=true`, so EVERY committed plan is CRLF once
> checked out) and the plan validator returned `ok: true` on it. So the rule below is
> version-dependent, not permanent.
>
> **Practical consequence:** do NOT treat "this plan will be CRLF after checkout in the execution
> worktree" as a blocking risk, and do not spend effort pre-normalizing or adding `.gitattributes`
> overrides for plan files. Run the validator and observe. Keep the detection recipes below — they
> are still the correct way to identify CRLF *if* a future validator build regresses and starts
> failing canonical-looking headings. Compare [[mcp-plan-validator-defective-em-dash]], which
> records the same lesson: validator behavior on this axis shifts between versions, so observe
> rather than assume.

Historical finding (validator build as of 2026-07-08, no longer reproducing): the plan validator (`artifact_type: "plan"`) required LF line endings. A plan committed with CRLF (Windows default) fails with every `### Phase N — <Title>` heading and every `- [ ] [P#-T#]` task line flagged plus "Plan does not contain any canonical phase headings", even though the em-dash and task format are correct.

**Why:** The executor's textual PREFLIGHT (`PREFLIGHT: ALL CLEAR`) tolerates CRLF, so a plan can be genuinely "preflight-cleared" and committed yet still fail the separate MCP validator gate that `atomic-plan-contract` requires before treating a plan as approved. Confirmed empirically 2026-07-08 (#262): identical minimal plan passed with LF, failed with CRLF; hyphen-vs-em-dash was ruled out (em-dash is correct and required — see [[remediation-plan-em-dash-required]]).

**How to apply:** Before running the MCP plan validator, if the plan fails on headings/tasks that look canonical, check line endings. Normalize CRLF->LF with `tr -d '\r'` — this is content-preserving and is NOT re-planning or regenerating, so it is allowed even under an implementation-only mandate. Note git may re-apply CRLF on checkout via autocrlf, but the working-tree file the validator reads is what matters, and the committed blob content is unaffected. Related: [[orchestrator-state-validator-divergence]].

**Detection pitfall (cost cycles 2026-07-16, #327):** in git-bash, `grep -c $'\r' <file>` reports `0` even on a genuine CRLF file (grep strips the trailing CR before counting), so it is USELESS for detecting CRLF. Use `cmp -l committed_blob working_file` (git-bash cmp DID surface the 0x0D at the line terminator) or `perl -ne 'print "CR\n" if /\r/'`, or `git show HEAD:<path> | cmp - <path>`. The committed blob was LF but the checked-out working file was CRLF (+1 byte per line: 21495 -> 21656). A red-herring hunt (a `` `[P3-T4]` `` bracketed task-ID in prose on a `- ` bullet line) looked like the cause because `sed -i` on git-bash silently normalized CRLF->LF as a side effect, making the "bracket fix" copy pass for the wrong reason.
