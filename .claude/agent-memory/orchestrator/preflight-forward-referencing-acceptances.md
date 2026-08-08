---
name: preflight-forward-referencing-acceptances
description: Atomic-plan preflight finds a forward-referencing acceptance clause in nearly every round; budget 3 rounds and name the check explicitly in the preflight prompt
metadata:
  type: feedback
---

Budget **three** `atomic-executor` preflight rounds for a large atomic plan, and explicitly name
this check in the preflight prompt: **"No acceptance may reference state that a later task
establishes."**

**Why:** On issue #455 (epic #136 F13, 14 phases / 220 tasks) every preflight round found at least
one instance of this single defect class, and nothing else blocked:

- Round 1 — B1 `[P2-T3]` asserted an attribute count that `[P2-T7]` later creates; B2 a Phase 4
  ordering let the `DECLINED` branch assert a source state `[P4-T10]` had not yet applied; B3 five
  tasks required "the file compiles" before their `<Compile Include>` entry existed in the non-SDK
  csproj; B4 an AC's numeric clause had no task recording the figure.
- Round 2 — B1-B4 verified fixed, and an exhaustive sweep found B5, the same class again
  (`[P3-T12]` required host test classes created at `[P3-T13]`..`[P3-T36]`).
- Round 3 — clear, after the sweep audited all 35 intra-task `[P#-T#]` references and confirmed the
  4 forward ones were explicit *non*-requirements.

The defect is invisible to the MCP plan validator, which passed all three times: the validator
checks heading/task/ID shape, not whether an acceptance is satisfiable at its own position.

**How to apply:**

- In the preflight prompt, state the rule verbatim and ask for an exhaustive sweep across *all*
  tasks, not just revised ones. Round 2 found B5 only because the sweep was exhaustive.
- Two structural repo facts make this defect frequent here: `QuickFiler.csproj` and
  `QuickFiler.Test.csproj` are non-SDK explicit-`<Compile Include>` projects, so a created file is
  not in the build until its entry lands — always fold the csproj entry into the *creating* task and
  convert any batch entry task into a verification task; and multi-task refactors create genuine
  non-compiling windows, so document each window in the plan preamble and name the task that records
  the first compiling build.
- Do not "fix" a forward reference by deleting it. The accepted pattern is an explicit disclaimer:
  the task states what it deliberately does **not** assert and names the later task that records it.
- Expect the planner to push back on parts of a relayed delta. On this run it correctly rejected two
  of my instructions (one proposed acceptance was false because another task also edited the test
  csproj; another was unsatisfiable for a reason the preflight had not spotted). Relay the delta, but
  let the planner overrule it with evidence — see [[planner-executor-lack-mcp-validator]].

Related: [[planner-executor-lack-mcp-validator]], [[preparation-mode-plans-need-repo-relative-paths]],
[[prep-child-upstream-dependency-must-be-nonhalting]].
