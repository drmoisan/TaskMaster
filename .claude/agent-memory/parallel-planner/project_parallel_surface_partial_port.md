---
name: parallel-surface-partial-port

description: Parallel surface works in TaskMaster and issue #545 is fixed, but three spurious-contention defects in config/blast-radius.json and the path extractor make any large run effectively serial (measured 83.3% graph density, mean cohort width 1.45)
metadata:
  type: project
---

**2026-08-29 UPDATE — defects 1 and 2 below are FIXED.** Re-read `config/blast-radius.json` on
`main @ b56400ab`: the `claude-runtime` umbrella module is GONE (the module map is now exactly the
18 real C# projects plus `config`), and `mandate_reads` has been extended to the ten entries
`.claude/rules/**`, `.claude/skills/atomic-plan-contract/SKILL.md`,
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.github/instructions/**`,
`artifacts/**`, `quality-tiers.yml`, `.claude/skills/acceptance-criteria-tracking/SKILL.md`,
`.claude/skills/policy-compliance-order/SKILL.md`, `.claude/agent-memory/**`, and
`.agents/skills/**`. The 83.3%-density measurement below predates both fixes and should be
re-measured before it is cited again. Defect 3 (placeholder tokens) and issue #576 (root build
files absent from `shared_surfaces`) were NOT re-checked on 2026-08-29 and may still stand.

A two-item run planned on 2026-08-29 (`bugs-635-440`) produced **zero** conflict edges between a
`UtilitiesCS`-scoped bug and a docs-only audit, so the surface now discriminates rather than
collapsing. Note the corollary: `.claude/agent-memory/**` being a `mandate_reads` exclusion is
load-bearing and correct — every agent-driven branch writes those append-only index files, so
counting them as contention would serialize every run. The cost is a possible trivial append-order
merge conflict on the second PR, which is a merge-time cost and not a scheduling constraint.

Status as of 2026-08-21, measured empirically on `main @ a01bdbb0`. Supersedes the 2026-08-11
assessment, which is now wrong on its headline blocker. Re-verify before relying on this.

**Issue #545 is CLOSED and its blocker is gone.** `config/blast-radius.json` was re-authored for
TaskMaster: it now enumerates the real 18 C# project modules (`QuickFiler`, `UtilitiesCS`,
`ToDoModel`, ... plus `.Test` siblings) and the location-bucket modules `docs`/`tests` that used to
collapse the graph are gone. The feature-folder glob no longer causes universal contention.
`/parallel-plan` is structurally usable.

**The remaining problem is the opposite of the old one: spurious contention, not zero parallelism.**
Measured over the 16 real committed plans under `docs/features/active/`, deriving each radius with
`Get-BlastRadius` and testing all 120 pairs with `Test-BlastRadiusConflict`:

- conflict graph density **83.3%** (100 of 120 pairs conflict)
- `compute-cohorts.sh` yields **11 cohorts for 16 items**, max parallel width **2**, mean width **1.45**
- per-edge reasons: `path_overlap` 83, `module_overlap` 80

Three distinct, independently fixable defects drive most of that, each creating a clique:

1. **`claude-runtime` umbrella module is still present** (`.claude/** -> claude-runtime`), matching
   **10/16 = 62%** of plans. `.claude/rules/parallel-orchestration.md` "Module-map granularity
   criterion" states this module was REMOVED upstream (alongside `python-dev-tools`,
   `vscode-extension`, `copilot-surface`, `agents-surface`) precisely because an umbrella keyed on a
   top-level directory carries no information and only suppresses concurrency. TaskMaster's copy
   never got that removal. Single largest clique driver.
2. **Mandate-read leakage.** `mandate_reads` lists only two skill files by exact path
   (`atomic-plan-contract`, `evidence-and-timestamp-conventions`, both under `.claude/skills/`).
   Real plans also cite, and therefore contend on: `.claude/skills/acceptance-criteria-tracking/SKILL.md`
   (6/16), `.claude/skills/policy-compliance-order/SKILL.md` (3/16), `.claude/agent-memory/**`
   (4/16), and the **entire `.agents/skills/**` tree** (3/16 each for `atomic-plan-contract`,
   `evidence-and-timestamp-conventions`, `csharp`). `.agents/` is a live parallel skill tree in this
   repo and appears nowhere in `mandate_reads`.
3. **Placeholder tokens are extracted as real paths.** `Get-PlanPaths` does NOT reject
   placeholder-bearing tokens, unlike the plan-acceptance gates. Verified: `<FEATURE>/spec.md`,
   `<FEATURE>/evidence/baseline/x.md`, and `${VAR}/y.cs` all extract verbatim. `<FEATURE>/spec.md`
   appears in 5/16 plans and `<FEATURE>/issue.md` in 4/16, so two otherwise-disjoint items
   (`QuickFiler/A.cs` vs `ToDoModel/B.cs`) conflict with
   `detail=<FEATURE>/spec.md ~ <FEATURE>/spec.md`. Relative per-feature evidence paths
   (`evidence/qa-gates/coverage-final.cobertura.xml`, 3/16) do the same.

**Genuine contention that must NOT be "fixed away":** `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
(9/16 = 56%), `.github/workflows/ci.yml` (3/16), and the `*.Test.csproj` compile-entry files. Those
are real shared surfaces and their serialization is correct.

**Still open and unported:** issue #576 (`shared_surfaces` omits TaskMaster's root build files, so
`TaskMaster.sln`, `Directory.Build.targets`, `coverage.config`, `.editorconfig` are dropped entirely
from derived radii and such pairs report `conflict=False` — fail-OPEN). Until it lands, hand-append
those exact paths per the rule file's sanctioned remedy.

**How to apply:** do not fan out a large parallel run before defects 1-3 are fixed. At 83.3% density
an 80-item run is roughly 60+ cohorts deep — effectively serial, so `max_concurrency` is inert — and
most of that depth is spurious and would be recomputed away once the config is corrected. Fix the
truth table first (small, contained), then plan. Note `max_concurrency` is bounded 1..32 since
PR #575; do not clamp to 8. See [[parallel-surface-cannot-express-ordering]] for why the
coverage/CI cluster needs `/epic-plan` instead, and [[blast-radius-extractor-mechanics]] for the
backtick rule that governs whether any of this fires at all.
