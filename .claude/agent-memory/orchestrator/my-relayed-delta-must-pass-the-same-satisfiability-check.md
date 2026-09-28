---
name: my-relayed-delta-must-pass-the-same-satisfiability-check
description: A preflight delta I relay is plan text and can itself contain an assertion that cannot fail; run every command I author through the satisfiability rules before sending, and treat a worker's evidenced refusal as probably correct
metadata:
  type: feedback
---

Before relaying a preflight delta, run every command **I** author in it through the same
satisfiability rules the plan is judged by. My delta is plan text, not commentary on plan text.

**Why:** on #815 (2026-09-09) I relayed a delta instructing the plan to assert that
`git grep -c -F -e 'aggregate-compare-815' -- scripts tests docs` "prints nothing and exits 1". The
plan document itself lives under `docs/` and names that token three times, so the search matches the
plan's own prose and the assertion could never pass. `atomic-planner` refused it with evidence; I
re-ran the command and it printed the plan file at count 3. I had written an unsatisfiable gate into
the correction whose entire purpose was removing unsatisfiable gates. The same delta's companion
clause was also inert: `git status --porcelain -- coverage` cannot report a file under a
`.gitignore`d path because plain porcelain omits ignored entries — it needs `--ignored`.

The general trap: a content search scoped to `docs/`, `.` or the repo root will match the plan, the
spec, the issue and the evidence artifacts, because those documents quote the very literals the gate
is about. Self-matching is the default, not the exception.

**How to apply:**

- For any `git grep` / `Select-String` I author, ask what ELSE in the tree contains that literal —
  specifically the plan, spec, issue and evidence files that discuss it. Scope the path list to the
  code surface, or assert a count against a recorded baseline instead of against zero.
- A path-scoped `git status --porcelain` over a gitignored directory needs `--ignored` or it is inert.
- Prefer "equals the count recorded in `<baseline artifact>`" over "equals <literal>" whenever the
  literal is something I measured once and could have measured wrong. On this same run my delta also
  pinned a `VBFunctions` positive control at 2 when the tree had 3.
- **When a worker refuses an instruction of mine with specific evidence, assume it is right and
  verify, rather than restating the instruction.** Both refusals on this run were correct. A worker
  that pushes back with a measurement is doing the job; overriding it without checking converts my
  error into committed plan text. See [[subagent-self-reported-correction-can-be-false]] for the
  converse case — verify either way, but do not treat pushback as non-compliance.

## A relayed delta can create a BLOCKING defect, not just an inert one (verified 2026-09-13, #879)

The #815 case above produced gates that could not pass. The #879 case is worse and is the one to
expect: **two of the nine round-2 defects were created by the round-1 delta I relayed**, and both
were blocking.

- The round-1 delta added a porcelain companion to `[P0-T15]` (a correct G8b fix) whose pathspec
  included the feature folder, and an acceptance demanding the output name only `plan.md` and
  `spec.md`. By the time that task runs, the fourteen tasks before it have written a dozen untracked
  evidence artifacts into that folder and checked off their own plan lines. The gate could not pass.
  The companion was right; its **scope** and its acceptance were authored without asking what the
  tree looks like at that task's position.
- The same delta's `HOST=ToDoModel.Test` amendment scoped the path substitution to two task IDs.
  Nine tasks name `TaskMaster.Test/Bootstrap/...` by literal path. On the fallback branch seven of
  them read or stage a path that does not exist.

**How to apply.** When a delta changes a path SCOPE or adds a CONTINGENCY branch, the check is not
"is this command correct" — it is "at this task's position in the plan, what does this command
actually emit, and does every OTHER task that names the same paths get the same treatment". Two
concrete forms:

- For any span whose output is asserted, walk the earlier tasks and list what they will have created
  by then. Evidence artifacts and plan check-offs are the usual surprise.
- For any conditional substitution, grep the plan for the literal prefix being substituted and list
  every task ID that names it. Scope the amendment to that full list, not to the tasks that happened
  to be under discussion.

Related: [[preflight-catches-vacuous-gates]], [[my-own-negative-claims-need-a-scoped-search]],
[[apply-every-part-of-a-multipart-delta]], [[preflight-sibling-invalidation-cascade]],
[[preflight-rounds-exceed-target-legitimately]].
