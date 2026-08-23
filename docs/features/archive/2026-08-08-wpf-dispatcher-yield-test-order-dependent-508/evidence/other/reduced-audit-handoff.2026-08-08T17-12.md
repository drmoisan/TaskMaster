# Reduced-Audit Handoff (small path)

Timestamp: 2026-08-08T17-12

Task: [P2-T29]

Handoff to the small-path reduced `feature-review` audit. Work mode: `minor-audit`.

## Reduced artifact set

| Artifact | Path |
|---|---|
| Requirements source (sole) | `<FEATURE>/issue.md` — `## Acceptance Criteria`, AC1..AC9 |
| Plan of record | `<FEATURE>/plan.2026-08-08T15-23.md` (Version 1.2), all 60 tasks `[x]` |
| Baseline evidence | `<FEATURE>/evidence/baseline/` (14 artifacts) |
| Regression-testing evidence | `<FEATURE>/evidence/regression-testing/` (3 artifacts) |
| QA-gate evidence | `<FEATURE>/evidence/qa-gates/` (19 artifacts) |

Supporting (not part of the mandated reduced set, but relevant): `<FEATURE>/evidence/other/`
(4 artifacts) and `<FEATURE>/evidence/issue-updates/ac-reconciliation.2026-08-08T17-11.md`.

## Reduced-audit scope (from the plan's `## Reduced Audit Block`)

1. **Requirements traceability** limited to the `## Acceptance Criteria` section of
   `<FEATURE>/issue.md`. `spec.md` and `user-story.md` are **not** required and their absence is
   **not** a finding (confirmed absent at P0-T2).
2. **Policy compliance** for `.claude/rules/csharp.md` (DI seams, prohibited behaviors) and
   `.claude/rules/general-unit-test.md` (determinism, coverage exclusion policy).
3. **Evidence completeness** against `baseline/`, `regression-testing/`, and `qa-gates/`.
4. **Coverage gate**: repository line-rate non-regression plus changed-class line coverage >= 90%.
5. **Scope boundary**: diff confined to the two in-scope files.

## Result summary for the auditor

| Check | Result |
|---|---|
| AC1..AC9 | 9 of 9 `[x]`, each citing an on-disk artifact (P2-T25) |
| Evidence Checklist | 3 of 3 `[x]` (baseline, targeted verification, end-state) |
| Plan tasks | 60 of 60 `[x]` (P0 15, P1 16, P2 29) |
| Scope boundary | 2 files, both in scope; no `.csproj`/`.sln`; nothing under `TaskMaster/Ribbon/`; `UiThread.cs` untouched |
| Repo line-rate | 0.858162 -> 0.858328 (delta +0.000166, non-negative) |
| Changed-class coverage | 96.43% deduped / 97.37% tool-reported line, 100% branch (>= 90% gate met) |
| Toolchain | pass 4 clean: 0/0/0/0/0 exit codes, 6295/6295 tests |
| AC7 repeat runs | 3 of 3 identical and fully green (4667/4667/0 each) |
| Prohibited fixes | 0 hits on all 7 patterns; assertion unchanged |

## Five items the auditor should read before forming findings

These are recorded honestly in the evidence and are **not** concealed defects. Each has a
substantiating artifact.

1. **The toolchain required four loop passes, not one.** Passes 1 and 2 failed on two
   `QuickFiler.Test` tests (`QfcItemController_InitializationTests`, WinForms window-handle race);
   pass 3 was abandoned after a stale-build condition; pass 4 is clean and is what P2-T6 attests. A
   controlled attribution experiment proved the failures reproduce at merge-base with the change
   fully reverted (`6293 / 6291 / 2`, matching the "Run 1" figures at `issue.md:53`). No test was
   ignored, filtered, or retried. See `evidence/regression-testing/preexisting-failure-attribution.2026-08-08T16-52.md`
   and `evidence/qa-gates/toolchain-clean-pass.2026-08-08T16-56.md`. **Escalated as a separate
   defect deserving its own issue.**
2. **The nullable toolchain step is an incremental no-op** in this repository — at the P0-T9
   baseline and at the P2-T4 gate identically, so the comparison is like-for-like but the step
   enumerates nothing. The effective nullable check on the changed code is the analyzer build, which
   recompiled both projects and reported zero CS86xx. A forced `/t:Rebuild` exposes **195
   pre-existing repository-wide nullable errors**, none attributed to `WpfDispatcherYield.cs`.
   **Escalated as pre-existing repository-wide debt, out of scope.** See
   `evidence/baseline/msbuild-nullable.2026-08-08T16-19.md`.
3. **P0-T11 contradicted the plan's stated expectation about `[ExcludeFromCodeCoverage]`.** The plan
   predicted the attribute would not be honored; measurement showed it **is** honored (class
   entirely absent from the baseline report). The task text directed recording whichever state was
   observed, so this is a measurement, not a deviation. Consequence: P1-T7's removal is a real policy
   correction that admitted 38 lines to the denominator for the first time. See
   `evidence/baseline/wpfdispatcheryield-coverage.2026-08-08T16-24.md`.
4. **The CSharpier command spelling differs from the CLAUDE.md canonical string.** CSharpier 1.3.0
   requires `format`/`check` subcommands; bare `csharpier .` is not a valid 1.3.0 invocation, and
   `dotnet tool run csharpier` is unavailable in this checkout (no `.config/dotnet-tools.json`, no
   repo-local SDK). The same 1.3.0 binary produced both the baseline and the gate. **Must not be
   read as a toolchain deviation.** See `evidence/qa-gates/csharpier-format.2026-08-08T16-48.md`.
5. **All git diff/status/grep gates are scoped** with `-- '*.cs' '*.csproj' '*.sln'` or explicit file
   paths, because `.claude/agent-memory/**` is tracked, was already dirty at branch head, and its
   prose contains the literal token `DoNotParallelize`. Unscoped assertions would be both
   unsatisfiable and false-positive. Scoping loses nothing: P1-T15 proves the two in-scope files are
   the entire source diff. See `evidence/other/scope-boundary.2026-08-08T16-33.md`.

## Two pre-existing build warnings (out of scope, unchanged from baseline)

- 5x `System.Reactive` packages.config packaging warning (needs a PackageReference migration).
- 1x `CS2002` duplicate `<Compile Include>` for `PercentageFormatterTests.cs` in
  `UtilitiesCS.Test.csproj` (needs a `.csproj` edit, forbidden by the scope boundary).

Warning count is 6 at baseline and 6 post-change — unchanged.

## Deliberately out of scope

`TaskMaster/Ribbon/**` (concurrent work on #503 and #507), `UtilitiesCS/Threading/UiThread.cs`, all
other test files, all `.csproj`/`.sln` files, the QuickFiler WinForms handle race, and the
repository-wide nullable debt.

## Not done by this executor

No commit was made and no branch operation was performed; all changes are left in the working tree
for the orchestrator. No GitHub issue was updated (the `ac-reconciliation` artifact is a local
mirror with `PostedAs: unknown`). No PR was authored — AC4's "is justified in the PR body" clause
depends on PR authoring, and the technical substance for that body is in
`evidence/qa-gates/no-behavior-change.2026-08-08T17-08.md` and the plan's
`## Design Decision — Seam Shape` section.

Output Summary: Handoff to the reduced `feature-review` audit recorded, with the reduced artifact
set (issue.md, plan, and the baseline / regression-testing / qa-gates evidence folders) and the
five-point reduced-audit scope. All 60 plan tasks, all 9 ACs, and all 3 evidence-checklist items are
complete. Five items are flagged for the auditor to read as recorded qualifications rather than
concealed defects, of which two (the pre-existing QuickFiler handle race and the 195-error
repository-wide nullable debt) are escalated as out-of-scope defects needing their own issues.
