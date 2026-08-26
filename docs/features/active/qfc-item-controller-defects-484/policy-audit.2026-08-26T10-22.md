# Policy Audit — qfc-item-controller-defects (Issue #484)

- Reviewer: feature-review agent
- Timestamp: 2026-08-26T10-22
- Branch: `bug/qfc-item-controller-defects-484`
- Head: `4f2b55f17c65b9dbce3a2cb75e453d12616ce1d4`
- Base (merge-base with `epic/quickfiler-bug-family-integration`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`
- Work mode (from `issue.md`): `full-bug` — `spec.md` is the sole acceptance-criteria source; `user-story.md` is intentionally absent and its absence is not a defect.

## Scope

Full branch diff `61edc19b..4f2b55f1`: 74 files, +4570/-200. Changed code files are exactly four
production partials (`QuickFiler/Controllers/QfcItemController.{FocusAndTheme,EventWiring,ViewerSetup,MailActions}.cs`)
and five test files (`QuickFiler.Test/Controllers/QfcItemController.{FocusAndThemeTests,EventWiringTests,MailActionsTests,ViewerSetupTests,TestSupport}.cs`);
the remaining 65 files are feature-folder evidence markdown, `plan.2026-08-24T09-36.md` (checkbox
flips), and `spec.md` (verified: exactly 50 checkbox flips, zero criterion-text edits). Verified
independently with `git diff --name-status` and a filtered spec diff.

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` were absent in the review
worktree and were regenerated manually from `git diff --numstat` / `git diff` against the verified
merge-base before this audit (documented assumption; the merge-base supplied by the caller was
re-verified with `git merge-base` and matches).

## Rejected Scope Narrowing

None. The caller supplied the full feature-vs-base diff as the review scope and no instruction
attempted to narrow language coverage, file subsets, or toolchain checks. The caller's instruction
not to create `artifacts/csharp/coverage.xml` is a canonical-artifact production decision for a
pre-existing repository-wide condition, not a scope narrowing; coverage was verified from the
committed evidence of record as required by the evidence-verification review model.

## 1. General Code Change Policy

| Check | Verdict | Evidence |
|---|---|---|
| Bugfix workflow (failing regression test first, per defect) | PASS | Fail-before dossiers for all five issues: `evidence/regression-testing/480-sync-tightened-fail.md`, `480-async-fail.md`, `481-empty-bodies-fail.md`, `481-unguarded-fail.md`, `483-fail.md` (6 targeted failures verbatim), `484-fail.md`, `485-fail.md`; pass-after artifacts for each. The one untestable subscription carries the inspection dossier `fail-before-exception.webresourcerequested-detach.md`. |
| Minimal targeted fix, no opportunistic refactor | PASS | Production diffs are confined to the five defect sites; the only extraction (`TryResolveCidResource`) is the #485 fix itself. |
| File-size limit (500 lines) | PASS | All nine owned files measured directly by this reviewer (`awk` line count): 338, 482, 499, 257, 497, 499, 498, 489, 498. Maximum 499 (`ViewerSetup.cs`, `EventWiringTests.cs`). |
| Fail fast / no silent swallow | PASS | The #483 catch now logs at error level, notifies through the seam, and rethrows `InvalidOperationException` with the original fault as `InnerException` (`MailActions.cs:140-155`). Verified in source. |
| Separation of concerns / testable seams | PASS | `TryResolveCidResource` is a pure static decision function; `MoveFailureNotifier` isolates the modal dialog; the lambda adapter retains only SDK glue. |
| Naming, comments (why not what) | PASS | New members PascalCase; every non-obvious guard carries an issue-tagged rationale comment. |
| No breaking public API change | PASS | No public member added or removed on the four partials (all additions are `internal`/`private`); `IQfcItemController.cs` and `IItemViewer.cs` absent from the diff, hence byte-identical. |
| Dependencies | PASS | No new package or library reference; `QuickFiler.Test.csproj` and `QuickFiler.csproj` unmodified. |

## 2. C# Code Change Policy (toolchain)

Final consecutive pass evidence (`evidence/qa-gates/toolchain-consecutive-pass.md`), stages ordered
13-41 through 13-56 on 2026-08-26 with SHA-256 proof that no owned file changed mid-pass, zero
restarts:

| Stage | Result | Evidence |
|---|---|---|
| `dotnet tool run csharpier format .` | EXIT 0, 0 rewritten | `evidence/qa-gates/csharpier-format.md` |
| `dotnet tool run csharpier check .` | EXIT 0, 1520 checked, 0 unformatted | `evidence/qa-gates/csharpier-check.md`; independently re-run read-only by this reviewer on the head commit: `Checked 1520 files`, EXIT 0 |
| `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0, 0 errors (5 pre-existing System.Reactive packages.config warnings) | `evidence/qa-gates/msbuild-analyzers.md` |
| `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` | EXIT 0, 0 errors | `evidence/qa-gates/msbuild-nullable.md` |
| `vstest.console.exe QuickFiler.Test.dll /EnableCodeCoverage /InIsolation` | EXIT 0, 959/959 passed (baseline 938 + 21 new results from 19 new methods; arithmetic re-verified against the test diff) | `evidence/qa-gates/quickfiler-test-final.md` |

One invocation-level note is disclosed in the evidence: the first P7-T5 vstest invocation stalled and
was terminated by a wall-clock limit before producing any outcome; the identical command succeeded on
re-run in 12.3 s with SHA-256 proof of no intervening file change. This is a disclosed environment
stall, not a hidden restart; the consecutive-pass claim survives it.

The msbuild and vstest stages are accepted from the committed evidence plus the orchestrator's
independent confirmation; this reviewer did not rebuild the solution (evidence-verification model).

## 3. Unit Test Policy

| Check | Verdict | Evidence |
|---|---|---|
| MSTest + Moq + FluentAssertions only | PASS | All 19 new test methods inspected in the diff; no xUnit/NUnit. |
| No `Thread.Sleep` / `Task.Delay` / wall-clock waits / `DateTime.Now` in new test code | PASS | Regex scan of all added test lines: zero matches. The #484 timer test uses `Timeout.Infinite` on both arguments so no wait exists. |
| No temporary files / external dependencies | PASS | Regex scan for temp/file APIs in added test lines: zero matches. The literal `C:\OneDrive` string in `InjectFilingCollaborators` is a mock config value; no filesystem I/O occurs. |
| Determinism / independence / isolation | PASS | All collaborators are Moq mocks or injected delegates; the single real-`ItemViewer` test starts no message pump, calls no `Show()`, and restores `SynchronizationContext` in `finally`. |
| Exactly one new real `ItemViewer` construction | PASS | Grep of added lines for `new ItemViewer()` / `new QuickFiler.ItemViewer()`: exactly 1 (the #481 control-tree unwire test). |
| AAA structure and intent docs | PASS | Every new test carries Arrange/Act/Assert comments and an issue-tagged `<summary>`. |
| Tests not colocated with production code | PASS | All test additions are under `QuickFiler.Test/`. |
| No production path excluded from measurement | PASS | Zero `[ExcludeFromCodeCoverage]` occurrences anywhere in the branch diff (added or removed); no `coverage.config` or exclude edits. |

## 4. Coverage (C# — the only language with changed files on this branch)

No TypeScript, Python, or PowerShell files changed on this branch; coverage rows for those languages
are therefore not required. All figures below were read from the committed evidence of record
(`evidence/qa-gates/coverage-final.md`, `evidence/qa-gates/coverage-delta.md`, baseline
`evidence/baseline/coverage.md`), produced by `Invoke-MSTestWithCoverage.ps1` over the same 9 test
assemblies as the Phase 0 baseline (6503/6503 tests passing inside the collection run). The raw
Cobertura files are intentionally uncommitted per the feature `evidence/.gitignore` convention; the
root-element attributes are recorded verbatim in the evidence and are internally consistent
(53905/63543 = 0.848323, re-computed by this reviewer).

- C# repo-wide line coverage 84.8323% (up from the 84.775% baseline): PASS against the 80% repository floor stated in CLAUDE.md UT2, which this feature's spec and plan were authored against.
- C# repo-wide line coverage measured against the separate 85% floor in `.claude/rules/general-unit-test.md`: FAIL (84.8323% < 85%) — a pre-existing, repository-wide shortfall that predates this branch, which this branch improved by +0.0573 pp; dispositioned non-blocking for this child (see the policy-conflict exception below).
- C# changed-line coverage: PASS — 126 of 132 added executable lines covered (95.5%); the six uncovered lines are exactly the two authorized WebView2-runtime carve-outs (lambda adapter lines 94, 95, 101 inside the pre-existing `[ExcludeFromCodeCoverage]` `InitializeWebViewAsync`, and the guarded detach block lines 489-491), and no line covered at the baseline is uncovered now.
- C# new-member coverage: PASS — all five named new production members (`TryResolveCidResource`, `NotifyMoveFailure`, `UnwireEvents`, `UnwireControlTreeEvents`, `UnwireIntentEvents`) measure 100% line rate; `DetachWebResourceRequestedHandler` measures 62.5% under the spec's explicit, evidence-backed carve-out (guarded `-=` unreachable without a live WebView2 runtime), and the default `MoveFailureNotifier` initializer measures 100%.
- C# branch coverage 78.8057% repo-wide (up from 78.6876%): PASS against the 75% branch floor.
- Per-file line rate direction for all four touched production files: PASS — every one moved up (EventWiring 81.52 to 84.99, ViewerSetup 85.08 to 90.43, MailActions 76.80 to 84.40, FocusAndTheme 79.32 to 81.15).

### Surfaced policy-conflict exception (coverage floor)

The repository publishes two conflicting repo-wide line-coverage floors: CLAUDE.md (embedded General
Unit Test Policy UT2) states `>= 80%`, while `.claude/rules/general-unit-test.md` and
`.claude/rules/quality-tiers.md` state `>= 85%`. The measured 84.8323% clears the first and misses
the second. The shortfall against the 85% reading is pre-existing and repository-wide (baseline
84.775% at the merge-base predates this branch), and this branch moved the figure up. A single bug
child cannot close a repository-wide gap; every change-scope coverage gate (changed-line, new-member,
per-file no-regression) passes. The FAIL row above is therefore dispositioned non-blocking for this
feature, and the floor contradiction is surfaced here for maintainer-level resolution. This mirrors
the disposition pattern accepted on prior reviews of pre-existing repo-wide conditions.

## 5. Evidence Location Compliance

Scanned the full branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/`: zero occurrences. Every evidence artifact added by
this branch lives under the canonical
`docs/features/active/qfc-item-controller-defects-484/evidence/<kind>/` tree (baseline, qa-gates,
regression-testing, other). No `validate_evidence_locations.py` scanner exists in this repository;
the manual diff scan documented here is the working substitute. No raw `.trx` or `.cobertura.xml` is
tracked by this branch's diff (the TRX and Cobertura outputs were directed to gitignored paths and
their numeric content recorded in markdown evidence). EVIDENCE_LOCATION_OVERRIDE_REJECTED: none
required — no caller instruction specified a non-canonical evidence path.

## 6. Hygiene

- Host-path and account-name leak scan of all tracked files in the feature folder and the code diff (`DanMoisan`, `C:\Users`, `C:/Users`): zero matches.
- Working tree at review time: clean (`git status --porcelain` empty).
- Plan: 132/132 tasks checked. Spec: 50/50 criteria checked, zero text edits (verified).
- Tonality of committed artifacts: professional, evidence-first; no violations observed.

## 7. Verdict

PASS with zero blocking findings. One surfaced (non-blocking) policy-conflict exception on the
repository-wide coverage floor, recorded in section 4. Non-blocking code-review findings are recorded
in `code-review.2026-08-26T10-22.md`.
