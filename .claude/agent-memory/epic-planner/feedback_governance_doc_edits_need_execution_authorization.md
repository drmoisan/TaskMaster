---
name: governance-doc-edits-need-execution-authorization
description: An epic whose issues require editing CLAUDE.md or .claude/rules/** collides with the policy-compliance-order hard constraint; plan it, scope it, and make the user's kickoff the authorization point
metadata:
  type: feedback
---

Some bug issues are defects *in* the governance documents themselves (wrong toolchain commands,
contradictory coverage thresholds). Fixing them requires editing `CLAUDE.md` and
`.claude/rules/**`, which the `policy-compliance-order` skill hard constraint prohibits: "Do NOT
modify policy documents under `.claude/rules/` or `.github/instructions/`."

Do not treat this as a planning blocker and do not silently ignore it. Resolve it structurally:

1. **Planning is unaffected.** Preparation produces specs and atomic plans that *propose* the
   edits; no governance file is modified. Say this explicitly in the checkpoint.
2. **Record a `governance_edit_authorization` block** in `epic-planner-state.json` naming the
   exact files, the issues that require them, and `status: required_at_execution`.
3. **Put an "Execution Authorization Required" section in `epic.md`** and repeat it in the kickoff
   artifact, so running `/epic-run <slug>` is the visible authorization act.
4. **Scope the suspension per child, in the delegation prompt.** State that the constraint is
   suspended for that feature only, only at the sites its issues enumerate, and add the hard
   limit: no policy requirement may be relaxed, weakened, or deleted in order to make a gate pass.
5. **Partition the shared file between siblings.** Two children editing `CLAUDE.md` in different
   sections merge cleanly only if each is told which region it owns and which regions belong to a
   sibling. In the Lane A epic, feature 512 owned the C# toolchain command block and feature 494
   owned § UT2 coverage; each prompt named the other's region as off-limits. That let both stay
   in different waves without a fake `depends_on` edge.

**Why:** An agent cannot grant itself or a subagent permission to edit configuration or policy.
The user's own `/epic-run` command is the only thing that can, so the plan's job is to make that
authorization explicit, bounded, and reviewable rather than to smuggle it into a delegation prompt.

Related: [[check-inflight-branches-before-decomposition]],
[[preexisting-issues-skip-potential-to-issue]].
