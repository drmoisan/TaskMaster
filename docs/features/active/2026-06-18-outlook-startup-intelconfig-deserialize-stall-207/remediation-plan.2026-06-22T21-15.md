# Outlook Startup IntelConfig Deserialize Stall (#207) — Remediation Plan (PR #210 CI failure, Cycle 1)

Work Mode: full-bug (remediation cycle)
Target plan path: `docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/remediation-plan.2026-06-22T21-15.md`
Feature root (FEATURE): `docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207`
Authoritative inputs: `remediation-inputs.2026-06-22T21-15.md`

## Cycle summary

PR #210 failed the required CI check "Format, build, analyze, and test" (run 27984128719). The CI MSTest
step runs the whole solution without the local `/TestCaseFilter:"TestCategory!=LiveOutlook"` exclusion, so
the opt-in `LiveOutlook` harness `LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold` executed on a
headless agent where `new Outlook.Application()` threw `COMException 0x80040154 (REGDB_E_CLASSNOTREG)`. The
harness asserts the hookup "must not throw," so it failed.

Fix (test-only, single file): guard the harness so a `COMException` raised during `Outlook.Application`
creation/startup whose HRESULT indicates Outlook is unavailable (at minimum `0x80040154`
REGDB_E_CLASSNOTREG; also the class-not-available family such as `0x80040112` CLASS_E_NOTLICENSED and
`0x80080005` CO_E_SERVER_EXEC_FAILURE / `0x800401F0` CO_E_NOTINITIALIZED-style class-not-available HRESULTs
that mean "no Outlook here") is converted to `Assert.Inconclusive(...)`. When Outlook IS available, the
harness runs exactly as before; the real assertion path is unchanged.

## Scope lock

- Touch only: `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs`.
- No production code change. No `.github/workflows/**` change. No global `.runsettings` `<TestCaseFilter>`.
- Harness retains `[TestCategory("LiveOutlook")]`.
- net48; MSTest + Moq + FluentAssertions; deterministic; no live COM/timer/filesystem/temp files introduced.
- Banned APIs: none introduced (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`).
- File must remain `<= 500` lines.

## Acceptance criteria (from remediation-inputs.2026-06-22T21-15.md)

- AC-R1: The `LiveOutlook` harness, when run without Outlook available, reports Inconclusive (skipped), not Failed; when Outlook is available it runs as before.
- AC-R2: The full local toolchain passes (CSharpier -> analyzers -> nullable/TWAE -> MSTest gated), and the whole-suite behavior no longer fails on the harness.
- AC-R3: Change confined to `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs`.
- AC-R4: After push, the required CI check on PR #210 is green. (Verified post-merge/push; out of band of local execution but tracked here.)
- AC-R5: Exit gate — code-review, feature-audit, and policy-audit reaudits show 0 blocking findings.

## Evidence locations (canonical, non-overridable)

- Baseline: `docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/evidence/remediation-baseline/`
- Regression: `docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/evidence/regression-testing/`
- Final QC: `docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/evidence/qa-gates/`

Coverage note: the touched file is the `LiveOutlook` integration harness, explicitly excluded from the
coverage denominator per its XML doc and the CLAUDE.md COM/VSTO/Interop coverage exemption. No production
lines change in this cycle, so the no-regression requirement is satisfied by capturing the unchanged
repository coverage headline in baseline and final-QC artifacts; there is no new production code requiring
the 90% new-code floor.

---

### Phase 0 — Baseline capture

- [x] [P0-T1] Read policy files in required order and record `evidence/remediation-baseline/phase0-instructions-read.2026-06-22T21-15.md` with `Timestamp:`, `Policy Order:`, and the explicit list of files read: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `remediation-inputs.2026-06-22T21-15.md`.
- [x] [P0-T2] Record branch/commit baseline to `evidence/remediation-baseline/branch-commit-2026-06-22T21-15.md` with `Timestamp:`, `Command: git rev-parse --abbrev-ref HEAD && git rev-parse HEAD`, `EXIT_CODE:`, and `Output Summary:` (branch name and HEAD SHA).
- [x] [P0-T3] Run CSharpier in check mode and write `evidence/remediation-baseline/csharpier-2026-06-22T21-15.md` with `Timestamp:`, `Command: dotnet tool run csharpier --check .`, `EXIT_CODE:`, `Output Summary:` (clean / files needing formatting).
- [x] [P0-T4] Run the analyzer build and write `evidence/remediation-baseline/analyzers-2026-06-22T21-15.md` with `Timestamp:`, `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `EXIT_CODE:`, `Output Summary:` (build result, warning/error counts).
- [x] [P0-T5] Run the nullable/TWAE build and write `evidence/remediation-baseline/nullable-2026-06-22T21-15.md` with `Timestamp:`, `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`, `EXIT_CODE:`, `Output Summary:` (build result).
- [x] [P0-T6] Run the gated MSTest baseline with coverage and write `evidence/remediation-baseline/mstest-coverage-2026-06-22T21-15.md` with `Timestamp:`, `Command: vstest.console.exe <TaskMaster.Test assembly path> /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`, `EXIT_CODE:`, `Output Summary:` including numeric passed/failed counts and the repository line-coverage headline percent.
- [x] [P0-T7] Capture the current line count of `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs` to `evidence/remediation-baseline/linecounts-2026-06-22T21-15.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (current 147 lines, used to confirm the post-change file stays `<= 500`).

### Phase 1 — Guard harness COM unavailability as Inconclusive

- [x] [P1-T1] In `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs`, add a private static readonly set (or `IsOutlookUnavailableHResult(int hr)` helper method) enumerating the class-not-registered/not-available HRESULTs that mean "no Outlook here" — at minimum `0x80040154` (REGDB_E_CLASSNOTREG), plus `0x80040112` (CLASS_E_NOTLICENSED) and `0x80080005` (CO_E_SERVER_EXEC_FAILURE) — with an inline `// why` comment; acceptance: helper exists, returns true only for the enumerated HRESULTs and false otherwise, and the file still compiles.
- [x] [P1-T2] In the same file, wrap the `new Outlook.Application()` (and gate/coordinator construction startup) inside the STA worker so a thrown `System.Runtime.InteropServices.COMException` whose `ErrorCode` satisfies the P1-T1 helper is captured into a dedicated "Outlook unavailable" signal (e.g. a `bool outlookUnavailable` / `string skipReason` set on the worker), distinct from the existing `captured` general-exception path; acceptance: a class-not-available `COMException` during Application creation sets the skip signal and does NOT populate `captured`, while any other exception still populates `captured` exactly as before.
- [x] [P1-T3] After `thread.Join()` and before the existing `captured.Should().BeNull(...)` assertion in the test body, add a guard: when the skip signal is set, call `Assert.Inconclusive(...)` with a clear message naming the HRESULT and that Outlook is not registered/available in this environment; acceptance: in a no-Outlook environment the test reports Inconclusive (skipped) and the downstream `captured`/`completed`/`maxTickBlockMs` assertions are not reached; when Outlook is available the skip signal is unset and the original assertion path runs unchanged.
- [x] [P1-T4] Update the class/method XML doc remark in the same file to state the new behavior: the harness skips via `Assert.Inconclusive` when Outlook is unavailable (class-not-registered HRESULTs) and otherwise runs the real assertion path; acceptance: the XML doc references the skip-on-unavailable behavior and the file remains valid C#.
- [x] [P1-T5] Confirm scope lock and constraints: only `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs` is modified, `[TestCategory("LiveOutlook")]` is retained, no banned API is introduced, and the file is `<= 500` lines; record `evidence/regression-testing/scope-lock-2026-06-22T21-15.md` with `Timestamp:`, `Command: git diff --stat` and a `wc -l` of the file, `EXIT_CODE:`, and `Output Summary:` (single file changed, retained category, line count).

### Phase 2 — Final QC loop

- [x] [P2-T1] Run CSharpier formatting and write `evidence/qa-gates/csharpier-2026-06-22T21-15.md` with `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, `Output Summary:` (clean pass; if files were reformatted, restart the loop from this step).
- [x] [P2-T2] Run the analyzer build and write `evidence/qa-gates/analyzers-2026-06-22T21-15.md` with `Timestamp:`, `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `EXIT_CODE:`, `Output Summary:` (build result, 0 new analyzer findings).
- [x] [P2-T3] Run the nullable/TWAE build and write `evidence/qa-gates/nullable-2026-06-22T21-15.md` with `Timestamp:`, `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`, `EXIT_CODE:`, `Output Summary:` (clean build, no nullable warnings-as-errors).
- [x] [P2-T4] Run the gated MSTest run with coverage and write `evidence/qa-gates/mstest-coverage-2026-06-22T21-15.md` with `Timestamp:`, `Command: vstest.console.exe <TaskMaster.Test assembly path> /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`, `EXIT_CODE:`, `Output Summary:` including numeric passed/failed counts and the repository line-coverage headline percent (this is the CI-equivalent gated path and must pass).
- [x] [P2-T5] Run the `LiveOutlook` harness WITHOUT the exclusion to confirm the new guard: `vstest.console.exe <TaskMaster.Test assembly path> /TestCaseFilter:"TestCategory=LiveOutlook"`, and write `evidence/regression-testing/liveoutlook-skip-verification-2026-06-22T21-15.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. If this dev machine has no Outlook registered, the harness must report Inconclusive (skipped), not Failed. If Outlook IS registered here, the harness runs for real; in that case record that the skip path is verified by inspection of the guard (cite P1-T1/P1-T2/P1-T3) and that the COMException-to-Inconclusive branch cannot be exercised on this machine.
- [x] [P2-T6] Record the coverage delta to `evidence/qa-gates/coverage-delta-2026-06-22T21-15.md` with `Timestamp:`, baseline coverage percent (from P0-T6), post-change coverage percent (from P2-T4), changed/new-code coverage statement, and `Output Summary:` noting that the only touched file is the coverage-exempt `LiveOutlook` harness so no production line coverage changed and the `>= 80%` floor is unaffected.
- [x] [P2-T7] Write the AC verification summary to `evidence/qa-gates/ac-verification-2026-06-22T21-15.md` mapping AC-R1..AC-R5 to evidence artifacts (AC-R1 -> P2-T5; AC-R2 -> P2-T1..P2-T4; AC-R3 -> P1-T5 scope-lock; AC-R4 -> deferred to post-push CI check on PR #210; AC-R5 -> exit-gate reaudit), with `Timestamp:` and a per-AC PASS / DEFERRED status with the supporting artifact path.

---

## Structural self-check

- Phase headings are canonical `### Phase N — <Title>` with em-dash, no parenthetical qualifiers (per [[plan-validator-phase-heading-constraint]]).
- Task IDs are sequential per phase: P0-T1..T7, P1-T1..T5, P2-T1..T7.
- Every task is atomic with one binary acceptance criterion and explicit file paths.
- Phase 0 includes policy reads + per-command baseline artifacts (CSharpier, analyzers, nullable/TWAE, gated MSTest with coverage headline).
- Phase 2 runs the full ordered QA loop (format -> lint -> type-check -> test) with coverage, plus the un-gated LiveOutlook skip verification and coverage-delta.
- All evidence paths resolve under `<FEATURE>/evidence/{remediation-baseline,regression-testing,qa-gates}/`; no forbidden `artifacts/` evidence paths used.
