# P3-T4 — Scope-Boundary Diff Check (Merge-Base Anchored)

Timestamp: 2026-09-03T11-33
Command:
$mergeBase = git merge-base HEAD origin/main   # 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1
git diff --name-only $mergeBase..HEAD
git status --porcelain
(all git commands invoked as `git -C <absolute item worktree path> ...`)
EXIT_CODE: 0

git diff --name-only 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1..HEAD:
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/p0-t10-nuget-restore.2026-09-03T11-24.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/p0-t11-baseline-build.2026-09-03T11-25.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/p0-t12-literal-presence.2026-09-03T11-25.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/p0-t13-csharpier-baseline.2026-09-03T11-26.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/p0-t14-scoped-regression-baseline.2026-09-03T11-26.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/p0-t15-coverage-baseline.2026-09-03T11-28.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/p0-t16-merge-base.2026-09-03T11-28.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/p0-t8-sdk-bootstrap.2026-09-03T11-21.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/p0-t9-tool-restore.2026-09-03T11-24.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/phase0-instructions-read.2026-09-03T11-21.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/issue.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/plan.2026-09-02T08-57.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/research/2026-09-02T08-47-twelve-hour-time-format-research.md
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/spec.md

git status --porcelain:
 M QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs
 M QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs
 M QuickFiler/Controllers/EfcHomeController.Metrics.cs
 M QuickFiler/Controllers/QfcHomeController.Metrics.cs
 M docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/plan.2026-09-02T08-57.md
?? docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/regression-testing/
?? docs/features/potential/promoted/2026-09-02-quickfiler-date-time-format-missing-invariant-culture.md

Output Summary: The union of both outputs contains only: the four in-scope files listed in the
plan's Scope Lock section (two production controllers + two test files), plus paths under
docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/ (this plan file and
its evidence, including the not-yet-committed evidence/regression-testing/ directory), plus the
single pre-existing untracked path
docs/features/potential/promoted/2026-09-02-quickfiler-date-time-format-missing-invariant-culture.md
(queued sibling-issue-#742 promotion record, untouched by every task in this plan). No path under
QuickFiler/Legacy/, no TaskVisualization/TaskViewer.Designer.cs, no path matching .claude/**,
.codex/**, .agents/**, and neither config/blast-radius.json nor config/orchestration-routing.json
appears in either output.
