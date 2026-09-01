# Acceptance Criteria Status Summary (Issue #656)

Timestamp: 2026-09-01T14-58
Task: [P5-T21]

- Work Mode: full-bug
- AC source: `docs/features/active/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656/spec.md`
- Total AC items: 20
- Checked off (delivered and verified): 20
- Remaining (unchecked): 0
- Items remaining: none

Verified in the source file after check-off:
`@(Select-String -Path spec.md -Pattern '^- \[x\] AC-\d+ ').Count` = 20 and
`@(Select-String -Path spec.md -Pattern '^- \[ \] AC-\d+ ').Count` = 0.

## Per-criterion status and establishing evidence

AC-1 PASS — hoisted local before the lock, guard narrowed to `if (_closeCompleted && !hostOpen)`. Evidence: `evidence/other/lock-discipline.2026-08-31T20-40.md`; hoist at line 326, first lock after the `CloseCore` declaration at 327, guard literal count 1 and old-guard count 0.
AC-2 PASS — no `_host`/`IBreadcrumbDropDownHost` call added inside any `lock (_sync)` body. Evidence: `evidence/other/lock-discipline.2026-08-31T20-40.md`; exactly one such call remains, the pre-existing `if (_closeInFlight && _host.IsOpen)` in `RequestOpen`.
AC-3 PASS — `CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain` exists in `Part3.cs` and asserts two `Uncommitted` entries. Evidence: `evidence/qa-gates/green-run.2026-08-31T20-40.md`.
AC-4 PASS — fail-before and pass-after both recorded, with both outputs present under `evidence/qa-gates/`. Evidence: `evidence/qa-gates/red-green-comparison.2026-08-31T20-40.md`; two departures reconciled in its `AC-4 Reconciliation:` section.
AC-5 PASS — `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` passes and its file is absent from the diff. Evidence: `evidence/qa-gates/standing-guards.2026-08-31T20-40.md` and `evidence/qa-gates/footprint-test.2026-08-31T20-40.md`.
AC-6 PASS — `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` passes and its file is absent from the diff. Evidence: `evidence/qa-gates/standing-guards.2026-08-31T20-40.md` and `evidence/qa-gates/footprint-test.2026-08-31T20-40.md`.
AC-7 PASS — `RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync` passes and its file is absent from the diff. Evidence: `evidence/qa-gates/standing-guards.2026-08-31T20-40.md` and `evidence/qa-gates/footprint-test.2026-08-31T20-40.md`.
AC-8 PASS — `CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce` passes and its file is absent from the diff. Evidence: `evidence/qa-gates/standing-guards.2026-08-31T20-40.md` and `evidence/qa-gates/footprint-test.2026-08-31T20-40.md`.
AC-9 PASS — `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` passes, confirming a close while the host reports not open still reaches `_host.Close`. Evidence: `evidence/qa-gates/standing-guards.2026-08-31T20-40.md`.
AC-10 PASS — the only changed file under `QuickFiler/` is `BreadcrumbDropDownOpenCoordinator.cs`. Evidence: `evidence/qa-gates/footprint-production.2026-08-31T20-40.md`.
AC-11 PASS — no `.csproj`, `.props`, `.targets` or `packages.config` path in the change set. Evidence: `evidence/qa-gates/footprint-buildconfig.2026-08-31T20-40.md`.
AC-12 PASS — the only changed file under `QuickFiler.Test/` is `BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`. Evidence: `evidence/qa-gates/footprint-test.2026-08-31T20-40.md`.
AC-13 PASS — 395 and 213 lines, both under 500. Evidence: `evidence/qa-gates/file-size.2026-08-31T20-40.md`.
AC-14 PASS — `dotnet tool run csharpier check .` exits 0 over 1566 files with no file requiring formatting. Evidence: `evidence/qa-gates/format-check.2026-08-31T20-40.md`.
AC-15 PASS — analyzer gate reports `0 Error(s)` and no warning attributed to the changed file; post-change warning set is empty and a subset of the empty baseline set. Evidence: `evidence/qa-gates/analyzer-gate.2026-08-31T20-40.md`.
AC-16 PASS — zero `Skipping target "CoreCompile"` lines, with both assembly write times later than the recorded gate start as the positive control. Evidence: the `Non-Vacuity:` section of `evidence/qa-gates/analyzer-gate.2026-08-31T20-40.md`.
AC-17 PASS — type-check gate reports `0 Error(s)`; the command carries `/t:Rebuild` and neither `/p:Nullable=enable` nor `/t:Build`. Evidence: `evidence/qa-gates/typecheck-gate.2026-08-31T20-40.md`.
AC-18 PASS — 6926 tests, 6926 passed, 0 failed, with `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook` both confirmed at line 76 of the wrapper. Evidence: `evidence/qa-gates/test-coverage.2026-08-31T20-40.md`.
AC-19 PASS — both documentation blocks record the new suppression condition, and the `CloseCore` block records why the host read is taken outside `_sync`. Evidence: the P2-T3 and P2-T4 verification recorded in `evidence/other/lock-discipline.2026-08-31T20-40.md` and the file's two `Issue #656` documentation lines.
AC-20 PASS — no new `internal`/`public` member on the coordinator (count unchanged at 12) and `IBreadcrumbDropDownHost.cs` absent from the changed-file list. Evidence: `evidence/qa-gates/no-new-seam.2026-08-31T20-40.md`.

## Footprint-base note

AC-10, AC-11 and AC-12 are footprint criteria. They were evaluated against `origin/main`
(`5670b3cfe6a52e3b890bf80f0cd85a20d4fe4723`), which is an ancestor of HEAD, rather than against the
plan's pinned base `2b85134b42872e405602e6064e02dc9cda6c319b`. The pinned base predates this
branch's reconciliation merge with `main` and therefore conflates 299 inherited paths with this
item's change set, including nine under `QuickFiler/` and `QuickFiler.Test/` and one `.csproj`.
Both measurements are recorded verbatim in each footprint artifact so the substitution is auditable.

Output Summary: All 20 acceptance criteria are delivered, verified against named evidence, and
checked off in `spec.md`. None remain outstanding.
