# Policy Audit — storewrapper-dialog-imprecise-for-genuine-failure (#287)

- Reviewed: 2026-09-01
- Branch: `bug/storewrapper-dialog-imprecise-for-genuine-failure-287`
- Base anchor (independently recomputed): `git merge-base HEAD origin/main` = `09eae2e85cd586c092fb1977a76cd9e895ec0a3b` (matches the caller-supplied SHA; the plan's own D12 literal, `2b85134b42872e405602e6064e02dc9cda6c319b`, is stale and was correctly superseded by the executor's own P0-T2 divergence investigation — verified in `evidence/baseline/base-anchor.md`).
- HEAD verified: `git rev-parse HEAD` = `564792e57aa2a6f0088d0b4f727bdf86a115c92a`, matches the task description.
- Work mode: `full-bug` (from `issue.md:12`). AC source per `acceptance-criteria-tracking`: `spec.md` v2.0 only.

## Overall verdict: PASS

## Scope Invariant Compliance

No caller instruction in this task attempted to narrow scope to a plan/task/phase subset, mark a language "out of scope," or skip a toolchain/coverage check. The audit was performed against the full `git diff 09eae2e85cd586c092fb1977a76cd9e895ec0a3b..HEAD`, independently computed, not the plan's task list.

## Rejected Scope Narrowing

None found. No section required.

## Evidence Location Compliance

- `git diff --name-only 09eae2e85cd586c092fb1977a76cd9e895ec0a3b..HEAD | grep -E "^artifacts/"` returned zero matches — no file was written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or `artifacts/evidence/`.
- All evidence lives under the canonical `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/evidence/{baseline,regression-testing,qa-gates,other}/` tree, matching the Evidence Location Invariant.
- `validate_evidence_locations.py` does not exist anywhere in this repository (`find . -iname validate_evidence_locations.py` returned nothing), consistent with prior review findings that several named validator scripts referenced by the reviewer-agent skill were never ported into TaskMaster. Compliance was instead verified manually via the git-diff grep above. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events were needed — the plan (D11/D13) already targeted the canonical path.

## Footprint Compliance (AC2, AC16)

`git diff --name-only 09eae2e85cd586c092fb1977a76cd9e895ec0a3b..HEAD` lists exactly:
- 5 source/test files (the D1 set): `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs`, `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`, `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs`, `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs`, `UtilitiesCS.Test/OutlookObjects/Store/DisabledStoresControllerTests.cs`.
- Everything else changed is under `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/` (issue.md, spec.md, plan.md, research/, evidence/).

Independently confirmed: no `.csproj`/`.props`/`.targets`/`packages.config` file appears in the diff; `git diff --stat ... -- .claude/rules .github/instructions` is empty; `git diff --stat ... -- .claude/agent-memory` is empty (no agent-memory writes occurred on this branch, though they would have been tolerated per D13). PASS.

## Uncommitted residual

`git status --porcelain` shows one modified, uncommitted file: `plan.2026-08-31T20-56.md`. This is the documented "check-off fixpoint" residual described in the task brief and the plan's own P5-T19 text (checking off a task necessarily leaves the plan file dirty at the moment of the final read). Not a blocking finding, per instruction. It does not affect the footprint compliance check above, which is anchored to `HEAD`, not the working tree.

## C# Toolchain Verification (independent)

All four claims were independently corroborated from the raw evidence logs and by re-running read-only checks, not merely trusted from the self-report prose:

1. **CSharpier format** — `dotnet tool run csharpier check` re-run directly by this review against the five changed files: `Checked 5 files in 1651ms.` (exit 0). The full-repo claim (`evidence/qa-gates/final-csharpier-check.md`, exit 0, "Checked 1565 files") is consistent and not independently re-run in full (would duplicate a multi-second no-op check); the scoped re-run is sufficient corroboration for the changed files. PASS.
2. **Analyzer build** (`msbuild /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) — verified against the raw log `coverage/p3-analyzer-build.log`: `grep -cE ": error " ` = 0; tail shows "5 Warning(s) 0 Error(s)". The 5 warnings are confirmed by grep to be the pre-existing `System.Reactive.PackagesConfigCheck` `packages.config` advisories on `QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS.Test`, `UtilitiesCS` — unrelated to this change and present on `main`. PASS.
3. **Nullable build** (`msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true`) — verified against `coverage/p3-nullable-build.log`: `grep -cE ": error "` = 0; the only `: warning ` lines are the same 5 `System.Reactive.PackagesConfigCheck` advisories. `StoreLaunchReadinessEvaluator.cs` carries `#nullable enable` (line 1, unchanged) and no CS86xx diagnostic appears. Correct use of `/t:Rebuild`, not `/t:Build`, and no `/p:Nullable=enable` — matching CLAUDE.md's C#1 guidance exactly. PASS.
4. **MSTest with coverage** — verified against the raw `coverage/p3-testrun.log` tail: "Total tests: 6912 / Passed: 6912 / Total time: 28.0643 Seconds", with the coverage artifact written to `coverage/post-change.cobertura.xml`. Independently re-parsed with a Python `xml.etree` script (not the executor's PowerShell one-liner): root `line-rate=0.85297`, `branch-rate=0.79293`, `lines-covered=54869`, `lines-valid=64327` — exact match to the self-report. PASS.

**RED-first bugfix workflow** (General Code Change Policy, Bugfix Workflow) was followed and independently corroborated: `evidence/regression-testing/fail-before-wiring-tests.md` shows a P1 run (6912 discovered, 6907 passed, 5 failed) where the 5 failures are exactly the new copy-assertion tests, before the two `Launch()` call sites were rewired; `pass-after-wiring.md` shows the identical suite green (6912/6912) once Phase 2 rewired the call sites. This is genuine test-first evidence, not merely an executor's narrative claim.

## Coverage Verification (mandatory, all languages with changed files)

Only C# has changed files on this branch (no TypeScript, Python, or PowerShell files in the diff). PowerShell/TypeScript/Python coverage rows are correctly out of scope because zero files of those languages changed — this is a genuine "zero changed files" case, not a narrowing of an in-scope language.

**C# — PASS.**

- Coverage artifact: `artifacts/csharp/coverage.xml` is not present in this repo layout; the repository's actual coverage artifact convention (confirmed by prior review memory and by this branch's own plan D9-D11) is `coverage/*.cobertura.xml`, gitignored, regenerated per run. The raw Cobertura XML for this change (`coverage/post-change.cobertura.xml`, 10.78 MB, independently parsed above) and its baseline counterpart (`coverage/baseline.cobertura.xml`) are both present in the worktree and were independently re-parsed by this review, not merely quoted from the executor's evidence prose.
- Repo-wide: baseline line-rate 85.3035% -> post-change line-rate 85.297% (delta -0.0065 pp, within the plan's own -0.05pp tolerance and still >= both the CLAUDE.md 80% floor and the `.claude/rules/general-unit-test.md` 85% uniform floor). Branch-rate: baseline 79.289% -> post-change 79.293% (improved, clears the 75% uniform branch floor). **Repo-wide clears both candidate floors: PASS under either reconciliation of the open coverage-floor question below.**
- New code (`StoreLaunchReadinessEvaluator.BuildUnavailableMessage`, `BuildUnavailableTitle`): independently re-parsed from `coverage/post-change.cobertura.xml` — `class-line-rate=1`, `method line-rate=1` and `branch-rate=1` for both methods, `uncovered-count=0`. Clears the >=90% new-code floor (CLAUDE.md UT2) and the >=85%/>=75% uniform floor (quality-tiers.md) with margin.
- Modified files with production changes: `StoreWrapperController.cs` and `DisabledStoresController.cs` each had two literal-argument lines replaced by two method-call lines inside an already-`[ExcludeFromCodeCoverage]` `Launch()` method — no change to the coverage denominator inside that method. `DisabledStoresController`'s class-level `line-rate`/`branch-rate` in the Cobertura XML is bit-for-bit identical between baseline and post-change (0.929577 / 0.777778), confirming zero regression on the modified lines (the changed lines are inside the exempted method and were not newly measured).
- No production file was added to a coverage exclusion (`git diff --name-only ... -- coverage.config TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings` is empty).

## Coverage floor reconciliation (open item, not blocking for this PR)

CLAUDE.md states an 80% repo-wide / 90% new-code floor with a COM/VSTO exemption pathway. `.claude/rules/quality-tiers.md` and `.claude/rules/general-unit-test.md` state a uniform 85% line / 75% branch floor across all tiers with no tier-dependent relief, and state explicitly that "tier-specific lower coverage floors are not used in this repository." These two documents are in direct numeric conflict and neither one is marked superseded. Per CLAUDE.md's own Policy Compliance Order, `.claude/rules/general-unit-test.md` (item 3) is read after CLAUDE.md (item 1) in the same document, so the rules-file's 85%/75% figure is the more specific, more recently-stated instruction — but CLAUDE.md's own text still states 80%/90% verbatim and has not been edited to match. This review does not resolve the conflict (out of scope: "do not modify policy documents"), and it does not matter for this PR's verdict because the observed repo-wide and new-code coverage figures (85.297% line / 79.293% branch repo-wide; 100%/100% new code) clear both candidate floors. Flagging for maintainer attention: a future PR whose coverage lands between 80% and 85% repo-wide would receive a different verdict depending on which document governs.

## CUT1–CUT3 (C# test framework/tooling) Compliance

- MSTest: all new tests use `[TestMethod]` inside existing `[TestClass]` files (`StoreWrapperController_Tests`, `DisabledStoresControllerTests`) — confirmed by direct diff read.
- Moq: `Mock<IApplicationGlobals>`, `Mock<IOlObjects>` used consistently with the file's existing pattern; the spec's own "Moq caveat" (only `Ol` and `StoresWrapper` are set up, `LoadAsync` left unmocked) is followed in every new Arrange block — confirmed by direct diff read.
- FluentAssertions: every new assertion uses `.Should().Be(...)`, `.Should().NotBe(...)`, `.Should().Throw<ArgumentOutOfRangeException>()`, `.Should().BeNull()` — no raw MSTest `Assert.*` introduced.

## UT1–UT4 (General Unit Test Policy) Compliance

- Independence/isolation: each new test constructs its own mocks and controller instance; no shared mutable state between tests.
- Determinism: no clock, RNG, `Thread.Sleep`, `Task.Delay`, or filesystem I/O in any new test.
- AAA structure: every new test carries `// Arrange`, `// Act`, `// Assert` comments (or `// Arrange & Act` for the pure-function tests) — confirmed by direct diff read.
- No temporary files: none created.
- `MyBox.DialogInvoker` `AsyncLocal` seam is saved/restored in a `try`/`finally` in every new end-to-end test — confirmed by direct diff read (matches the pre-existing pattern in the same files).
- Test file location: both modified test files remain under `UtilitiesCS.Test/OutlookObjects/Store/`, mirroring the production `UtilitiesCS/OutlookObjects/Store/` tree — no colocation violation.

## File Size Limit (500 lines)

Independently re-measured with `awk 'END{print NR}'` on each of the five changed files (not merely quoted from evidence): `StoreLaunchReadinessEvaluator.cs`=96, `StoreWrapperController.cs`=478, `DisabledStoresController.cs`=181, `StoreWrapperController_Tests.Launch.cs`=480, `DisabledStoresControllerTests.cs`=373. All five are under 500. `StoreWrapperController.cs` is unchanged at 478 (2 literal-argument lines swapped for 2 method-call lines, net 0), matching the spec's stated 22-line headroom concern and confirming AC15 with margin intact for future work.

## Design Principles / Error Handling / Naming (General Code Change Policy §1, §3, §5)

- Simplicity: two pure `internal static` `string`-returning switch-expression methods, no new class, no new abstraction layer beyond what the spec justifies (net48/LangVersion 12.0 has no `IsExternalInit`, so `record`/`record struct` are unavailable — verified true, matching `UtilitiesCS.csproj:10`).
- Fail-fast: both methods throw `ArgumentOutOfRangeException` for `Ready` rather than silently returning a wrong string — matches General Code Change Policy §3.1.
- Naming: `BuildUnavailableMessage`/`BuildUnavailableTitle` match the repository's precedented `Build<X>Message` convention, independently confirmed present at `UtilitiesCS/Dialogs/MyBoxModeless.cs:123` (`BuildMessage`) and `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs:342` (`BuildUnavailableMessage`, `BuildToggleFailedMessage`, etc.) — the spec's precedent citation is accurate, not fabricated.
- XML docs: both new methods carry `<summary>`, `<param>`, `<returns>`, and `<exception>` doc comments.

## Verdict summary

PASS. No blocking findings. See `code-review.2026-09-01T14-01.md` for design/quality detail and `feature-audit.2026-09-01T14-01.md` for the AC-by-AC verification table.
