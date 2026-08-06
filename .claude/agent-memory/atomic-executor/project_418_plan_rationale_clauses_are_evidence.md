---
name: plan-rationale-clauses-are-evidence
description: "#418 cycle 2 took 3 preflight passes; all 3 blockers were unmeasured world-state claims in plan prose, never in the fix — measure any rationale an artifact must reproduce"
metadata:
  type: project
---

In #418 remediation cycle 2 (a two-line `.csproj`/`packages.config` change) preflight took three passes.
Every blocker was a claim the plan made *about the world*, not a defect in the fix, the commands, or the
acceptance clauses:

1. Header asserted "working tree clean" when two tracked `.claude/agent-memory/feature-review/` files were
   modified — which made `[P0-T5]`'s halt clause fire at task 5 and `[P1-T7]`'s "exactly two tracked files"
   unsatisfiable.
2. `[P1-T2]` justified a single-line `packages.config` entry with "csharpier formats only `*.cs`". False:
   `.csharpierignore` excludes `*.csproj`/`*.props`/`*.targets` but **not** `packages.config`, and that file
   is visibly csharpier-reflowed. Conclusion (stay single-line) was right for a different reason — width.
3. `[P0-T9]` required the executor to *record* that `UtilitiesSwordfish.Test`'s project file is
   `UtilitiesSwordfish.NET.Test.csproj`. That directory holds only `bin/` and `obj/`; `git ls-files` returns
   zero. The planner had read `obj/…csproj.AssemblyReference.cache` and inferred a live project from build
   residue that outlived the tear-down commit (#308).

**Why:** a rationale clause that a task orders an artifact to reproduce is not commentary — it becomes
evidence in the audit trail, and a reaudit that checks it reopens the cycle over prose. Pattern 2 and 3
share a shape: wrong supporting fact, right conclusion, so nothing fails at runtime and only a disk check
catches it.

**How to apply:** during preflight, verify every factual assertion a task requires an artifact to state, not
just the commands and paths. During execution, re-measure such clauses at the point of writing rather than
transcribing plan prose. Treat `obj/` and `bin/` contents as residue, never as proof a project exists —
`git ls-files <dir>` is the authoritative check. Ten directories match `*.Test` in this repo but only nine
test assemblies exist; `UtilitiesSwordfish.Test` is stale untracked output.

Related: [[project_bom_grep_anchor_false_negative]] (another measure-don't-assume trap),
[[project_cobertura_runsettings_attributes_override]] (csharpier v1 formats packages.config XML).
