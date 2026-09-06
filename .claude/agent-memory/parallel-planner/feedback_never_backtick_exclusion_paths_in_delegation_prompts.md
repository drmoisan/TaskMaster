---
name: never-backtick-exclusion-paths-in-delegation-prompts
description: Never write a backticked path token in the NEGATIVE scope constraints of a preparation-delegation prompt — children transcribe it into plan/spec prose and the blast-radius extractor harvests it as a write claim, serializing the whole run
metadata:
  type: feedback
---

**Rule: in a preparation-mode delegation prompt, never put a repository path inside backticks when
the sentence is telling the child NOT to touch it.** Name it in plain prose instead ("the .claude
tree", "the two published files under config/"), or omit the path entirely and state the rule by
category.

**Why.** Verified 2026-09-02 on the `bugs-2026-09-02` run, item #564. My delegation prompt carried a
SCOPE CONSTRAINTS block reading "Do NOT edit anything under `.claude/**`, `.codex/**`, `.agents/**`,
`config/blast-radius.json`, or `config/orchestration-routing.json`." A conscientious child
transcribed that list verbatim into `spec.md` (four times: non-goals, constraints, acceptance
criteria) and into `plan.md` (four times: a scope-boundary section, an AC row, and two task bodies),
always as a NEGATIVE claim — "no task in this plan touches them", "confirm the name-only output has
no entry under ...".

`Get-PlanPaths` harvests backtick-delimited whitespace-free tokens and has no notion of polarity. It
cannot distinguish "this plan writes X" from "this plan does not write X". So the derived radius for
a plan whose entire diff is one Markdown file came back as:

```
shared_surfaces: config/blast-radius.json, config/orchestration-routing.json
modules:         config
paths:           .agents/**, .claude/**, .codex/**, .github/workflows/*.yml, ...
```

**The failure is silent and total.** Both excluded files are members of `shared_surfaces` in
`config/blast-radius.json`, so every item that received the same prompt boilerplate acquires the
same two shared surfaces, and every pair then conflicts on `shared_surface_overlap`. A 13-item run
collapses to 13 singleton cohorts — fully serial — and nothing reports an error. V1 and V2 both
return ZERO findings, because the radius was derived from the same plan text the validator extracts
from, so it is perfectly self-consistent. The validators cannot catch this class of defect; only
reading the derived radius against the plan's actual diff can.

**The same prompt also produced an under-report in the other direction.** `CLAUDE.md` is backticked
19 times in that plan and is the ONLY file the diff writes, yet it is absent from the radius: it is
separator-free, and derivation admits a separator-free token only as an exact ordinal member of the
configured `shared_surfaces` list. TaskMaster's root files are not in that list (open issue #576),
so the real write target failed OPEN while the fake ones failed CLOSED. Hand-append it, per the
sanctioned remedy.

**How to apply.**

- Write the positive requirement with backticks (it drives correct derivation, and the child needs
  it): "write every file your diff will touch as a concrete backticked path."
- Write the negative requirement without them.
- ALWAYS derive and eyeball each item's radius against what the plan actually writes, before cohort
  seeding. Do not rely on V1/V2 clearing — they cannot see this.
- If contamination already landed, the fix is a targeted revision delegation asking the child to
  de-backtick the negative-scope tokens in `spec.md` and the plan and re-run preflight. Deleting the
  tokens from the radius yourself is NOT sanctioned: the skill permits appending a genuinely-written
  excluded path but grants no symmetric power to remove, and "narrow a radius to suppress a conflict
  edge" is explicitly prohibited. Fix the source text, then re-derive.

See [[blast-radius-extractor-mechanics]] for the backtick harvesting rule this exploits and
[[parallel-surface-partial-port]] for the neighbouring spurious-contention defects.
