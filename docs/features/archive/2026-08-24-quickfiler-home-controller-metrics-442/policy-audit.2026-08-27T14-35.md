# Policy Audit — quickfiler-home-controller-metrics-442

- Timestamp: 2026-08-27T14-35 (UTC)
- Feature: `quickfiler-home-controller-metrics-442` (GitHub issues #442, #443, #451)
- Branch: `bug/quickfiler-home-controller-metrics-442`, HEAD `6a1b9ca481fbfb760fbc34f5ad1145895a209de5`
- Base: `origin/epic/quickfiler-bug-family-integration` @ `0ddab4107b3b147e706a6c15856888b3b5d6404b`
- Merge base: `0ddab4107b3b147e706a6c15856888b3b5d6404b` (recomputed this session; equals the base tip, three-dot range isolates exactly this feature's contribution)
- Work mode: `full-bug` (`spec.md` is the sole acceptance-criteria source; `user-story.md` correctly absent)
- Scope: full branch diff against the resolved base — 74 files, +5364/-555, verified with `git diff --stat origin/epic/quickfiler-bug-family-integration...HEAD`
- Reviewer: feature-review agent

## Rejected Scope Narrowing

None detected. The delegation prompt supplied the full three-dot branch diff as scope and asked for evaluation (not exclusion) of two pre-adjudicated items; neither instruction narrows the audit scope, and both items are evaluated below. All commands in this audit were run by this reviewer against the branch worktree unless explicitly attributed to committed evidence artifacts.

## 1. Verdict Summary

| # | Policy area | Verdict | Severity of any finding |
|---|---|---|---|
| 1 | CLAUDE.md — C# toolchain order (format, analyzers, nullable, test) | PASS | — |
| 2 | CLAUDE.md — MSTest / Moq / FluentAssertions | PASS | — |
| 3 | CLAUDE.md § UT2 — coverage floors and changed-line no-regression | PASS with two dispositioned FAIL rows (section 7) | Minor, non-blocking |
| 4 | general-code-change — 500-line file cap | PASS | — |
| 5 | general-code-change — design, error handling, naming, I/O boundaries | PASS | — |
| 6 | general-unit-test — determinism ban list | PASS | — |
| 7 | general-unit-test — temp-file prohibition | PASS | — |
| 8 | general-unit-test — coverage-exclusion policy | PASS | — |
| 9 | general-unit-test — AAA structure, documentation, scenario completeness | PASS | — |
| 10 | quality-tiers — uniform 85/75 thresholds | PASS repo-wide; two per-file FAIL rows dispositioned (section 7) | Minor, non-blocking |
| 11 | tonality — committed artifacts | PASS | — |
| 12 | Evidence location invariant | PASS | — |
| 13 | Plan ownership boundary (AC-19 documented deviation) | PARTIAL — disclosed and correctly not claimed | Minor, non-blocking |

**Blocking findings: 0.**

## 2. Toolchain Compliance (CLAUDE.md, C# Code Change Policy)

The pass of record is `evidence/qa-gates/toolchain-loop.2026-08-27T14-18.md` with per-step artifacts at the same timestamp. Independently corroborated facts supplied by the launching session (measured 2026-08-27):

1. Format: `dotnet tool run csharpier format .` exit 0; 0 of 7 owned files rewritten (SHA-256 before/after). Check: exit 0 over 1540 files.
2. Analyzers: `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exit 0, 0 errors, 5 pre-existing `System.Reactive` `packages.config` warnings. Non-vacuity: 51 `CoreCompile:` executions, zero `Skipping target "CoreCompile"` lines.
3. Nullable: `msbuild ... /p:TreatWarningsAsErrors=true` exit 0, 0 errors, 0 `CS86xx`, 54 `CoreCompile:` executions, zero skips. Command matches CLAUDE.md character-for-character (no `/p:Nullable=enable`, `/t:Rebuild` not `/t:Build`).
4. Test: coverage-enabled vstest run exit 0 — 6701 tests, 6701 passed, 0 failed, 0 skipped.

The restart rule was exercised: attempt A (2026-08-27T13:54) aborted at the analyzer step on `MSB3021`/`MSB3027` file-lock contention from a hung pre-existing test run (zero compiler diagnostics), the contending process was terminated with reasons recorded, and the phase restarted from step 1. The loop-closure record satisfies the "restart from step 1" and "single clean final pass" requirements. Verdict: **PASS**.

## 3. Test Framework and Library Policy (CUT1/CUT2)

Verified by reading both owned test files in full:

- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (453 lines, 11 `[TestMethod]`): MSTest attributes, Moq mocks (`Mock<IQfcCollectionController>`, `It.Ref<AppointmentItem>.IsAny`), FluentAssertions throughout.
- `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` (490 lines, 15 `[TestMethod]`): MSTest, Moq (`Mock<MailItem>`), FluentAssertions; headless fakes (`FakeApplicationGlobals`, `FakeFileSystemFolderPaths`) where interface stubs are simpler than mocks, which is consistent with policy.

No xUnit/NUnit usage. Verdict: **PASS**.

## 4. File Size Cap (general-code-change § 4)

Measured this session with `awk 'END{print NR}'` on every code file in the branch diff:

| File | Lines | Cap |
|---|---|---|
| `QuickFiler/Controllers/QfcHomeController.cs` | 449 | 500 |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 215 | 500 |
| `QuickFiler/Controllers/EfcHomeController.cs` | 445 | 500 |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 124 | 500 |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | 147 | 500 |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | 453 | 500 |
| `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` | 490 | 500 |
| `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` | 219 | 500 |

`QfcHomeController.cs` moved from 487 to 449 lines (net deletion of the defective metrics machinery). All files under the cap. Verdict: **PASS**.

## 5. Unit Test Determinism and Environment (general-unit-test)

- Ban list: `git grep -nE "Thread\.Sleep|Task\.Delay|DateTime\.Now|Path\.GetTempPath|GetTempFileName"` over both owned test files run by this reviewer — zero hits (exit 1).
- Clock: every clock read goes through `FakeTimeProvider` (`Microsoft.Extensions.Time.Testing`) or the injected `MetricsNowFactory`. Stopwatch elapsed values are injected by reflection into the internal tick field rather than started/stopped against the wall clock, which is deterministic by construction.
- Temp files: no filesystem writes occur in any test; all writes are captured through the injected `MetricsFileWriter` (QFC) and `MetricsLineWriter` (EFC) seams. The fixture comment in `QfcHomeControllerMetricsTests.cs` correctly documents why the default production writer must be replaced (it probes a real path with a bounded retry loop).
- Culture manipulation (`de-DE` tests, both files) saves and restores `CultureInfo.CurrentCulture` in `try`/`finally`, preserving test independence.
- Async tests await real completed/yielded tasks; no fake-timer facility is needed because no timer remains in the code under test (the `System.Timers.Timer` machinery was deleted).

Verdict: **PASS**.

## 6. Coverage-Exclusion Policy

`git diff origin/epic/quickfiler-bug-family-integration...HEAD | grep -c "ExcludeFromCodeCoverage"` returns 0 — the change adds no coverage-exclusion attribute and no `coverage.config` exclusion. The AC-22 exception (section 7) is argued from the ratified CLAUDE.md § UT2 COM/VSTO testable-denominator exemption without removing any file from the denominator. Verdict: **PASS**.

## 7. Coverage Verification (mandatory per changed language)

Changed languages in the branch diff, determined by this reviewer from `git diff --name-only` extensions: **C# only**. Zero `.ps1`/`.psm1`, zero `.py`, zero `.ts`/`.tsx` files changed on this branch.

- C# canonical coverage artifact `artifacts/csharp/coverage.xml`: **FAIL — artifact absent** at the canonical path in this worktree; coverage verification is mandatory for all languages with changed files. Disposition: **non-blocking, procedural**. The raw Cobertura outputs are gitignored by repository convention and deliberately not committed; the executor committed post-processed figures with full counter detail in `evidence/baseline/mstest-coverage.2026-08-26T10-42.md` and `evidence/qa-gates/coverage-delta.2026-08-27T14-19.md`, and this reviewer corroborated the arithmetic (54379/63881 = 85.1255%; 12927/16320 = 79.2096%; 53912/63543 = 84.8433% baseline). No remediation is directed for this row; the substantive verification below is performed from the committed evidence of record.
- C# repo-wide line coverage: 85.1255% post-change vs 84.8433% baseline (>= 85% floor, and up +0.2822) — **PASS**.
- C# repo-wide branch coverage: 79.2096% post-change vs 78.8181% baseline (>= 75% floor, and up +0.3915) — **PASS**.
- C# changed-line coverage: 39 of 39 changed coverable lines covered, 100.00%, zero regression — **PASS**.
- C# per-file line coverage, modified file `QuickFiler/Controllers/QfcHomeController.cs`: 76.23% is below the 85% uniform floor — **FAIL**, dispositioned **non-blocking**: the file's only change is pure deletion (0 changed coverable lines), its figure improved from 68.40% baseline (+7.83), and the residual uncovered lines are pre-existing Outlook-Interop-bound code inside the CLAUDE.md § UT2 ratified testable-denominator exemption.
- C# per-file line coverage, modified file `QuickFiler/Controllers/QfcHomeController.Metrics.cs`: 80.00% is below the 85% uniform floor — **FAIL**, dispositioned **non-blocking**: improved from 63.31% baseline (+16.69), changed-line coverage 10/10 = 100%, and the ten residual uncovered lines are the pre-existing inline Outlook `AppointmentItem` block that reads 39/49 in both the baseline and post-change documents (identical on both sides; this feature neither caused nor worsened it).
- C# per-file line coverage, remaining owned files: `EfcHomeController.cs` 98.25% (+0.44), `EfcHomeController.Metrics.cs` 100.00% (+2.27), `EfcHomeController.ExecuteMoves.cs` 89.86% with an identical uncovered-line set on both sides (7 lines, all COM-bound; the ratio moved down only because the denominator shrank when a nine-line guard became three) — **PASS** on the no-regression requirement that governs modified files.
- New-code rows: this feature adds no new file. The six members named in the spec's Test Strategy: five at 100.00%; `QuickFileMetrics_WRITE` aggregates 88.37% (76/86) against the spec's 90% member target — evaluated as an adequately justified stated exception in section 8.
- PowerShell: zero changed `.ps1`/`.psm1` files on this branch; no PowerShell coverage row is owed for this diff.
- Python: zero changed `.py` files on this branch; no Python coverage row is owed for this diff.
- TypeScript: zero changed `.ts`/`.tsx` files on this branch; no TypeScript coverage row is owed for this diff.

The two per-file FAIL rows are recorded honestly against the uniform floor and dispositioned non-blocking on the evidence above (improvement or parity on every file, 100% changed-line coverage in aggregate, pre-existing COM-bound residue under a ratified exemption, repo-wide figures above both floors). They do not trigger remediation.

## 8. Adjudicated Item 2 — AC-22 exception (QuickFileMetrics_WRITE at 88.37%)

Judgement: **the exception is adequately justified.** Grounds, verified against `evidence/qa-gates/coverage-delta.2026-08-27T14-19.md` and the source:

1. The shortfall is wholly attributable to ten lines of inline Outlook `AppointmentItem` creation in the QFC overload (`QfcHomeController.Metrics.cs:81-90` post-change), guarded by `UtilitiesCS.Calendar.GetCalendar(...)`, which returns null in every unit fixture. This block is a direct Interop call sequence, exactly the category the ratified CLAUDE.md § UT2 exemption addresses, and it is not behind an injectable seam.
2. The block reads 39/49 in both the baseline and post-change documents — byte-for-byte unchanged — so the feature neither caused nor worsened the residual.
3. Excluding only that block, the member measures 76/76 = 100.00%; every EFC overload, including the newly implemented one-argument overload, is at 100.00%.
4. The exception is called out per CLAUDE.md § UT5 in three places (the coverage delta, the acceptance-criteria status record, and the planned PR body) rather than absorbed silently.

## 9. Adjudicated Item 1 — AC-19 / [P7-T6] / [P7-T27] documented deviation

Judgement: **the deviation is adequately disclosed and correctly not claimed as passing.** Verified:

- The three-dot diff of `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` is exactly one line: `SetField(controller, "_isExecuting", true)` becomes `SetField(controller, "_isExecuting", 1)` (commit `889fa298`).
- The technical necessity claim holds: AC-14 makes `_isExecuting` a `private int` (verified at `EfcHomeController.cs:389-393`), and `FieldInfo.SetValue` rejects a boxed `bool` for an `int` field, so no production-side change could make the pinned test pass. The change is the minimal in-repo caller update the General Code Change Policy itself requires for a necessary breaking change.
- The gate artifact `evidence/qa-gates/ownership-gate.2026-08-27T14-03.md` reports the gate **unclean** at both comparison points, records the parent ratification with the fan-in-safety verification (four historical commits on that file, none owned by a concurrent sibling), and scopes the ratification to this one file on this one feature.
- AC-19 is left unchecked in `spec.md`, [P7-T6] and [P7-T27] are left unchecked in the plan, and no artifact claims the gate clean. This is the recorded, intended state; it is not raised as Blocking.

Residual observation (Minor, PA-2 below): the branch diff also contains two paths outside the seven owned files and the feature folder that the deviation record does not mention — `.claude/agent-memory/orchestrator/MEMORY.md` (modified) and `.claude/agent-memory/orchestrator/agent-tool-cannot-course-correct-running-subagent.md` (added), committed by the parent orchestrator (`2b3760eb`). Orchestrator agent-memory is a repository-governance mechanism whose writes are routine and non-code, but the changed-file inventory (`changed-file-inventory.2026-08-27T14-32.md`) reached its "every other listed path is owned or in-folder" statement by excluding `.claude/agent-memory` via pathspec rather than by listing and classifying the two paths.

## 10. Evidence Location Compliance

- `git diff --name-only origin/epic/quickfiler-bug-family-integration...HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"` — zero matches. All 59 evidence artifacts live under `docs/features/active/quickfiler-home-controller-metrics-442/evidence/<kind>/`, the canonical location.
- `validate_evidence_locations.py` does not exist in this repository (searched the tree); the manual diff scan above substitutes for it.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none — no caller instruction supplied a non-canonical evidence path.

Verdict: **PASS**.

## 11. Tonality and Artifact Hygiene

- Scanned every committed file under the feature folder for absolute host paths, the user account name, and machine-name patterns — zero matches. The one TRX filename cited in `qfc-flush-red.2026-08-26T11-19.md` has its account and host components redacted to `<account>_<HOST>` per convention.
- Emoji scan over the feature folder — zero matches.
- Prose in the evidence artifacts is factual and measured; claims are consistently tied to commands, exit codes, and counters ("recorded rather than omitted", signed differences, both aborted toolchain attempts documented). No hyperbole, humor, or metaphor observed.

Verdict: **PASS**.

## 12. Findings Register

| ID | Severity | Finding | Disposition |
|---|---|---|---|
| PA-1 | Minor, non-blocking | AC-19 ownership boundary not met: one ratified, disclosed, unclaimed one-line write to plan-forbidden `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs:64` | Accepted as documented deviation (section 9); AC-19, [P7-T6], [P7-T27] correctly left unchecked |
| PA-2 | Minor, non-blocking | Two `.claude/agent-memory/orchestrator/**` paths in the branch diff are outside the AC-19 boundary and are absent from the changed-file inventory, which excluded them by pathspec | Recommend the PR body enumerate them alongside the EfcHomeControllerTests.cs deviation so the inventory is complete without carve-outs |
| PA-3 | Minor, non-blocking | Canonical C# coverage XML absent at `artifacts/csharp/coverage.xml`; verification performed from committed, arithmetically corroborated evidence documents | Procedural; figures verified in section 7 |
| PA-4 | Minor, non-blocking | Two modified files below the 85% uniform per-file line floor (76.23%, 80.00%) | Dispositioned in section 7: improvement on both, 100% changed-line coverage, pre-existing COM-bound residue under ratified exemption |

**Blocking findings: 0. No remediation-inputs artifact is produced.**
