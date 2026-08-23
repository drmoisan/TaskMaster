# Baseline — CI status of the three toolchain steps on `main` (settles research open question 7)

Timestamp: 2026-08-10T15-05

Command: `gh run list --branch main --limit 6 --json name,status,conclusion,headSha,createdAt`
EXIT_CODE: 0

| Conclusion | Workflow | Head SHA | Created |
|---|---|---|---|
| failure | CI | a682c7a2 | 2026-08-10T16:33:37Z |
| success | CI | cee6a1ca | 2026-08-10T10:29:25Z |
| success | CI | 7eee27e0 | 2026-08-09T11:37:25Z |
| failure | CI | d169363a | 2026-08-09T03:50:58Z |
| success | CI | f910ff2f | 2026-08-08T23:12:26Z |
| success | CI | b112f5ed | 2026-08-08T22:59:24Z |

Command: `gh api repos/drmoisan/TaskMaster/actions/runs/31409582674/jobs` (the failing run at main's tip)
EXIT_CODE: 0

Job `Format, build, analyze, and test` step conclusions:

| Conclusion | Step |
|---|---|
| success | Setup CSharpier |
| success | **Verify formatting** (`dotnet csharpier check .`) |
| success | **Build with analyzers and code style enforcement** |
| success | **Build with nullable warnings treated as errors** (`/t:Rebuild /m`, no `/p:Nullable=enable`) |
| **failure** | Run MSTest suite with coverage |
| success | Upload test results |

Job `actionlint`: success.

## Output Summary

`main` is intermittently red, but **not at any of the three steps this feature corrects.** All three
gates that this feature adopts as the corrected documented commands passed on main's tip
(`a682c7a2`). The sole failing step is `Run MSTest suite with coverage`, which is a test and coverage
concern outside this feature's scope and plausibly belongs to the sibling coverage features (issues
441, 457, 494) or to the known-flaky `PhysicalFileInfoAdapter` test.

## Consequence for the design decision

This is direct confirmation that adopting CI's commands verbatim yields gates that are **passable on
a clean checkout**, satisfying AC3 and AC5. It independently corroborates local run M3 in
`baseline-nullable-gate-vacuity.2026-08-10T14-25.md` (EXIT 0, 0 errors, genuine recompile) and the
`dotnet csharpier check .` result in `baseline-csharpier-replacement-forms.2026-08-10T14-45.md`
(EXIT 0, 1517 files).

It also means the corrected documented toolchain will not inherit main's current redness: the
red step is not one of the three being documented.

## Additional consequence for the "should `ci.yml` change?" decision

`.github/workflows/ci.yml` lines 2-8 confirm the workflow declares `workflow_dispatch:` alongside
`push` and `pull_request` triggers on `main` and `development`. The policy rule
`modified-workflow-needs-green-run` at `.claude/skills/feature-review-workflow/SKILL.md:74` states
that "a green `workflow_dispatch` run against the branch head also satisfies the rule, not only a
PR-context run". So a green run **is** in principle obtainable on an epic-child branch, and the
research document is correct to withdraw the "unobtainable green run" argument.

However, a further practical obstacle exists that the research document did not have the data to
identify. **The `Run MSTest suite with coverage` step is currently failing on `main`.** Any
`workflow_dispatch` run against this branch head would execute that same step and inherit the same
failure, because this feature changes nothing that affects it. A workflow change made here would
therefore be gated behind a green run that this feature has no means to obtain, and would be blocked
by a defect owned by a different feature.

This is an additional, independent reason to leave `ci.yml` unchanged, on top of the primary reason
that its commands are already correct and are precisely what this feature adopts.
