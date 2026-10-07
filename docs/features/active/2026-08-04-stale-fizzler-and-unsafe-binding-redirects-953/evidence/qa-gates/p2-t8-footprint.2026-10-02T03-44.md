# P2-T8 Footprint assertion (CMD-FOOTPRINT)

Timestamp: 2026-10-02T03-44
Command: git -C <execution-worktree-root> diff --name-only 860d67bf4fddecb929e0d6c166065fd1ee752feb -- . ; git -C <execution-worktree-root> status --porcelain --untracked-files=all
EXIT_CODE: 0

Capture 1, `git diff --name-only <BASE_SHA> -- .` (verbatim; the three git advisory lines about LF to CRLF on the plan and the two new files are omitted here and are not errors). Because Phases 0 and 1 are committed, this BASE_SHA-anchored diff is the discriminating observation and lists the whole item:

```text
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/project_953_fizzler_redirect_sweep_and_ratchet_plan_seams.md
.claude/agent-memory/atomic-planner/project_953_r1_pwsh_refused_and_count_recheck_seams.md
.claude/agent-memory/atomic-planner/project_953_r2_grep_long_line_omission_and_cr_anchor_seams.md
.claude/agent-memory/atomic-planner/project_953_r3_grep_gitignore_directory_path_and_measured_line_lengths.md
QuickFiler.Test/app.config
QuickFiler/app.config
SVGControl.Test/app.config
Tags/app.config
TaskMaster/app.config
TaskTree/app.config
TaskVisualization.Test/app.config
TaskVisualization/app.config
ToDoModel.Test/app.config
ToDoModel/app.config
UtilitiesCS.Test/app.config
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t10-poshqc-format.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t11-poshqc-analyze.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t12-poshqc-test.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t13-budget-baseline.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t2-base-anchor.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t3-ac-precondition.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t4-fizzler-census.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t5-shared-string-census.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t6-unsafe-census.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t7-encoding-baseline.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t8-known-debt-remeasure.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/p0-t9-linecount-baseline.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/phase0-instructions-read.2026-10-02T03-08.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/other/preflight-clearance.2026-10-02T03-45.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t1-module-authored.2026-10-02T03-18.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t10-taskvisualization-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t11-taskvisualization-test-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t12-todomodel-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t13-todomodel-test-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t14-utilitiescs-test-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t16-sweep-verification.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t17-unsafe-unchanged.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t4-quickfiler-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t5-quickfiler-test-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t6-svgcontrol-test-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t7-tags-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t8-taskmaster-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p1-t9-tasktree-redirect.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/regression-testing/p1-t15-pass-after.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/regression-testing/p1-t2-tests-authored.2026-10-02T03-18.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/regression-testing/p1-t3-fail-before.2026-10-02T03-21.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/plan.2026-10-02T00-16.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/research/2026-10-02T00-35-fizzler-redirect-remedy-and-redirect-gate-research.md
docs/features/potential/promoted/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md
scripts/dependencies/BindingRedirectVerification.psm1
tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1
```

Capture 2, `git status --porcelain --untracked-files=all` (verbatim):

```text
 M docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/plan.2026-10-02T00-16.md
 M scripts/dependencies/BindingRedirectVerification.psm1
 M tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1
?? docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p2-t1-poshqc-format.iter1.2026-10-02T03-29.md
?? docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p2-t1-poshqc-format.iter2.2026-10-02T03-33.md
?? docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p2-t2-poshqc-analyze.iter2.2026-10-02T03-35.md
?? docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p2-t3-poshqc-test.iter2.2026-10-02T03-38.md
?? docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p2-t4-loop-closure.2026-10-02T03-40.md
?? docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p2-t5-function-test-map.2026-10-02T03-41.md
?? docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p2-t6-file-size-audit.2026-10-02T03-42.md
?? docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/qa-gates/p2-t7-no-csharp-scope.2026-10-02T03-43.md
```

(The porcelain capture was taken before this artifact was written, so this artifact and its predecessors written afterwards are not in it; all of them lie under the feature folder, which is inside the Write Set.)

Evaluation (C6): the evaluated set is the union of the two captures' paths minus the five `.claude/agent-memory/atomic-planner/` paths (removed by prefix) minus the P0-T2 INHERITED list (the preflight-clearance artifact, issue.md, plan.2026-10-02T00-16.md, the research artifact and `docs/features/potential/promoted/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`, each listed in INHERITED). The remainder was compared against the Write Set union: the 11 Write Set configs, `scripts/dependencies/BindingRedirectVerification.psm1`, `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`, and paths under `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/`. Every remaining path is in that union; no other path appears (no STOP: FOOTPRINT). The promoted potential entry is outside the Write Set but is an INHERITED path present before this item started (P0-T2), so it is subtracted, not an item change.

Positive control: all 13 named source paths are present in the evaluated set: QuickFiler/app.config, QuickFiler.Test/app.config, SVGControl.Test/app.config, Tags/app.config, TaskMaster/app.config, TaskTree/app.config, TaskVisualization/app.config, TaskVisualization.Test/app.config, ToDoModel/app.config, ToDoModel.Test/app.config, UtilitiesCS.Test/app.config, scripts/dependencies/BindingRedirectVerification.psm1 and tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 (13 of 13).

Acceptance: every evaluated path is inside the Write Set union; all 13 named source paths are present.

Output Summary: Footprint equals the Write Set. 13 of 13 named source paths present; no path outside the Write Set after the agent-memory and INHERITED subtractions.
