# Policy Audit — ci-flaky-test-isolation (Issue #176)

- Component: Test-isolation fix for two flaky `UtilitiesCS.Test` cases; one narrow production seam on `PhysicalFileInfoAdapter`.
- Date: 2026-06-08T14-15
- Reviewer: feature-reviewer agent
- Review type: Feature-branch review of PR #177 into `main`.
- Work Mode: `issue.md` is absent in the feature folder, so the persisted Work-Mode marker is missing. Fail-closed default per the workflow is `full-feature`. The authoritative workflow input names `spec.md` (AC1-AC7) as the acceptance-criteria source, which is the `full-bug` AC source and is appropriate for this defect fix. AC evaluation therefore uses `spec.md` AC1-AC7. The marker absence is recorded under Gaps and Exceptions.
- Base branch (resolved): `main`
- Merge base: `3b379f600a91d415d1efaaee4a4188c88ef54b4c` (committed 2026-06-08T08:59:45-04:00)
- Head: `bug/ci-flaky-test-isolation-176` @ `92e35bcd`
- Diff range: `3b379f600a91d415d1efaaee4a4188c88ef54b4c..92e35bcd`
- AC source: `docs/features/active/ci-flaky-test-isolation-176/spec.md` AC1-AC7.

## Executive Summary

The branch removes two test-isolation defects that fail intermittently under parallel CI execution on `main` (CI run 27138963879).

- Defect 1 (`OlFolderClassifierGroup_Tests.cs`): the test double's key-tracking store was a plain `List<string>` written from a concurrent callback (`AsyncMultiTasker.AsyncMultiTaskChunker`). Concurrent `List<T>.Add` corrupts the backing array, which produced the observed `{<null>, "Inbox"}`. Fix: the store is now a `ConcurrentBag<string>` exposed as `IEnumerable<string>`; the FluentAssertions `Contain` assertion is unchanged.
- Defect 2 (`PhysicalFileSystemAdapters_Tests.cs`): the test opened write/append/read-write handles on the real shared `TaskMaster.sln`, which throws `IOException` when another process holds the file under parallel CI. Fix: the three write-mode members are exercised through a new injectable-delegate seam on `PhysicalFileInfoAdapter` using test-owned sentinel streams (read-only DLL opens plus an in-memory append stream), asserted with `BeSameAs`. No scratch file and no real write/append handle.

The production change is a narrow, behavior-preserving seam: the public `PhysicalFileInfoAdapter(FileInfo)` constructor binds three new delegate fields to the wrapped `FileInfo` (`AppendText`, `Open`, `OpenWrite`), and a new `internal` constructor accepts those delegates for injection. The three affected members now invoke the delegates; for the public constructor the delegates resolve to the original `FileInfo` methods, so production runtime behavior is unchanged.

Coverage was verified by inspecting the committed Cobertura evidence artifacts (no re-run). `PhysicalFileInfoAdapter.cs` per-file line-rate rose from a baseline 0.8909 to 0.9155, with the new internal constructor fully covered; no per-file regression.

One acceptance criterion (AC7: PR CI green on `main` and post-merge `main` CI green) is not locally verifiable and remains pending the GitHub Actions run on PR #177. This is the only non-PASS criterion and it is environmental/external, not a defect in the change. It is recorded as PARTIAL (pending external CI), which is a remediation trigger under the workflow. See Section 10 and the remediation inputs artifact.

Overall verdict: PARTIAL. Blocking findings on the change itself: 0. One pending external-CI criterion (AC7) drives a remediation handoff for the CI-green confirmation step only.

## Rejected Scope Narrowing

The workflow input directed self-determined scope per the scope invariant ("Determine scope yourself per the skill's scope invariant"). It included one context note: that the changed production file is `PhysicalFileInfoAdapter.cs`, an additive seam, and that the authoritative full-suite coverage gate is the PR #177 CI run because local full-assembly MSTest-with-coverage is environmentally unreliable (a pre-existing Moq binding-redirect failure). The note explicitly stated it is "not scope narrowing."

Assessment: this note is not a scope narrowing. It does not exclude any changed file, it does not exempt the C# language from review, and it does not instruct skipping a toolchain check. The C# coverage verdict below is an explicit PASS based on the committed Cobertura artifacts. The full branch diff (3 C# files, plus docs/evidence) was audited. No narrowing was attempted; none rejected.

## Evidence Location Compliance

All evidence the executor produced is under the canonical `<FEATURE>/evidence/<kind>/` subtree:
- `evidence/baseline/2026-06-08T13-21-38Z/` (BASELINE.md, baseline.cobertura.xml, fullrun/baseline-full.trx)
- `evidence/qa-gates/2026-06-08T13-29-05Z/` (QA-GATE.md, postchange.cobertura.xml, olstress1/post.cobertura.xml, postfix/postfix.cobertura.xml, full2.trx, fullrun1/run1.trx)
- `evidence/qa-gates/2026-06-08T13-58-59Z/` (QA-GATE.md, postchange.cobertura.xml, raw .coverage)

Branch-diff scan for prohibited non-canonical evidence paths (`artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`, `artifacts/post-change/`): none present. Verified via `git diff --name-only 3b379f6..92e35bcd | grep -E "^artifacts/(baselines|baseline|qa|qa-gates|evidence|coverage|regression-testing|post-change)/"`, which returned no matches.

Note: `validate_evidence_locations.py` is not present in this repository. The equivalent scan was run against `git diff --name-only` for the resolved range. The repository PreToolUse hook `.claude/hooks/enforce-evidence-locations.ps1` enforces this at write time.

Note on uncommitted on-disk evidence: the large `.trx` run logs and the raw `.coverage` binary are present on disk under the canonical evidence folders but were intentionally left uncommitted (per the workflow input). The committed Cobertura XML and QA-GATE/BASELINE markdown are sufficient for coverage verification. This is recorded, not flagged as a violation.

Verdict: PASS.

## 1. General Unit Test Policy Compliance

- Independence / isolation: PASS. Both fixes target test isolation. Defect 1 replaces a non-thread-safe shared list with `ConcurrentBag<string>`; Defect 2 removes a shared-file write dependency so the test no longer contends with another process for `TaskMaster.sln`.
- Determinism: PASS. The QA-GATE evidence reports 14/14 over 5 consecutive coverage runs for the affected-class set and 10/10 in isolation for the adapter test. Both target tests pass deterministically.
- Fast execution: PASS. No sleeps, retries, or timing hacks added (confirmed by reading the diff).
- No temp files / external deps: PASS. Defect 2 explicitly avoids any scratch/temporary file. Sentinel streams are read-only opens of the test assembly DLL plus an in-memory `MemoryStream` for the append `StreamWriter`; all disposed via `using`. This removes the prohibited write handle on the shared `.sln`.
- Scenario completeness: PASS for the change scope. The write-mode delegation (`AppendText`, `Open(FileMode.Open)`, `OpenWrite`) remains covered through the seam; read-only real-file delegation is retained.
- AAA + intent documentation: PASS. Both diffs add explanatory comments stating the concurrency and file-sharing rationale.

Verdict: PASS.

## 2. General Code Change Policy Compliance

- Simplicity first: PASS. The production change is the smallest seam that enables deterministic coverage of the three write-mode members: three delegate fields and one additional constructor. No broad refactor.
- Separation of concerns: PASS. The seam isolates the testable delegation surface without changing the I/O semantics of the public constructor.
- Error handling: PASS. Both constructors retain `ArgumentNullException` guards; the new internal constructor additionally guards each injected delegate against null.
- Naming: PASS. `_appendText`, `_openByMode`, `_openWrite` are descriptive; `seamAdapter`/`sentinel*` names in the test are clear.
- File size limit (500 lines): PASS. `PhysicalFileInfoAdapter.cs` remains well under 500 lines after the +35/-6 edit; the two test files remain under the limit.
- Comment "why not what": PASS. The added comments explain the concurrency hazard and the file-sharing/CI rationale.

Verdict: PASS.

## 3. Language-Specific Code Change Policy Compliance (C#)

- Strong contracts / explicit APIs: PASS. The seam constructor is `internal` (test-visible), keeping the public surface minimal. Delegate types are explicit (`Func<StreamWriter>`, `Func<FileMode, FileStream>`, `Func<FileStream>`).
- Null-safety: PASS. All four constructor parameters/fields are null-guarded.
- Composition / focused types: PASS. The injectable-delegate seam follows the repo's minimal-DI preference (narrow injectable delegates rather than a generic runner framework).
- Resource safety: PASS. The test disposes all sentinel streams/writers via `using`; `leaveOpen: true` on the append writer is intentional because the `MemoryStream` is separately disposed.
- Formatting (CSharpier): PASS per `evidence/qa-gates/2026-06-08T13-58-59Z/QA-GATE.md` (`dotnet tool run csharpier format .`, clean; the pinned local tool requires the `format` subcommand).

Verdict: PASS.

## 4. Language-Specific Unit Test Policy Compliance (C#)

- Framework: PASS. MSTest (`[TestClass]`/`[TestMethod]`) retained; no xUnit/NUnit introduced.
- Mocking: PASS. The change uses a production injectable seam plus test-owned sentinel streams rather than mocking a concrete `FileInfo` (which the public constructor cannot accept as a mock). This is the documented deviation in `spec.md` and is consistent with Moq usage elsewhere; no executable or framework boundary is mocked improperly.
- Assertions: PASS. FluentAssertions `BeSameAs` for delegation identity; `Contain` retained for Defect 1.
- New-unit coverage target (>= 90%): PASS. The new internal seam constructor shows method line-rate 1.0 (100%) in `evidence/qa-gates/2026-06-08T13-58-59Z/postchange.cobertura.xml`.

Verdict: PASS.

## 5. Test Coverage Detail

Languages with changed files in the branch diff: **C# only**. The source/test diff is 3 `.cs` files (1 production, 2 test). No `.ts/.tsx`, `.py`, or `.ps1/.psm1` source files changed. Remaining changed paths are Markdown docs and Cobertura XML evidence.

| Language | Changed files | Canonical artifact | Artifact present | Coverage verdict |
|---|---|---|---|---|
| C# (csharp / .NET) | yes (1 production: PhysicalFileInfoAdapter.cs; 2 test) | `artifacts/csharp/coverage.xml` + committed per-feature Cobertura evidence | YES (feature evidence Cobertura; `artifacts/csharp/coverage.xml` present but stale from a prior feature) | **PASS** |
| TypeScript | none | `coverage/lcov.info` | n/a | N/A (zero changed files) |
| Python | none | `artifacts/python/lcov.info` | n/a | N/A (zero changed files) |
| PowerShell | none | `artifacts/pester/powershell-coverage.xml` | n/a | N/A (zero changed files) |

C# coverage verdict: **PASS**. Coverage was verified by inspecting committed Cobertura evidence artifacts (review model: verify existing artifacts, do not re-run). The reviewer parsed the artifacts directly.

Reviewer-parsed per-file line coverage (Cobertura `line-rate` on the class node for `PhysicalFileInfoAdapter`):

| File | Baseline line-rate | Post-change line-rate | Gate | Verdict |
|---|---|---|---|---|
| `PhysicalFileInfoAdapter.cs` (modified production) | 0.8909 (89.09%) | 0.9155 (91.55%) | modified: no regression and >= 80% | PASS (rose +0.0246, no regression) |
| New internal seam constructor (new unit) | n/a | 1.0 (100%) | new unit: >= 90% | PASS |

Write-mode delegation members remain hit post-change: `AppendText` hits=1, `Open(FileMode)` hits=1, `OpenWrite` hits=1 (per `evidence/qa-gates/2026-06-08T13-58-59Z/QA-GATE.md`, corroborated by the postchange Cobertura class node).

Repo-wide C# coverage: the committed Cobertura evidence files are intentionally trimmed to the affected classes (`<!-- trimmed to affected classes for issue #176 -->`), so their root `<coverage>` element does not carry a repo-wide aggregate line-rate. The repo-wide full-assembly coverage gate is the GitHub Actions CI run on PR #177; local full-assembly MSTest-with-coverage is environmentally unreliable due to a pre-existing `System.Threading.Tasks.Extensions 4.2.0.1` Moq binding-redirect `TypeInitializationException` in the local vstest host (documented identically on baseline and post-change in both QA-GATE files). The change is test-isolation plus a behavior-preserving seam; it adds no untested production code path (the seam's new constructor is fully covered, and the public constructor's behavior is identical to the prior parameterized-constructor form). There is no plausible repo-wide regression mechanism from this change.

Note on `artifacts/csharp/coverage.xml`: a 29 MB file dated 2026-06-02 exists at this path from a prior feature; it is stale relative to issue #176 and is not the authoritative evidence for this review. The authoritative coverage evidence for #176 is the committed per-feature Cobertura under `evidence/`. (The repo coverage hook parses `artifacts/csharp/coverage.xml` as JaCoCo; that file is Cobertura, so the hook's repo-wide parse yields null and does not enforce a threshold. The reviewer parsed the per-feature Cobertura manually instead.)

Verdict: **PASS** (modified-file no-regression gate met at 91.55%; new-unit >= 90% gate met at 100%; repo-wide full-assembly confirmation deferred to PR #177 CI per the documented local environment constraint, with no untested new production path introduced).

## 6. Test Execution Metrics

From `evidence/baseline/2026-06-08T13-21-38Z/BASELINE.md`, `evidence/qa-gates/2026-06-08T13-29-05Z/QA-GATE.md`, and `evidence/qa-gates/2026-06-08T13-58-59Z/QA-GATE.md` (review is evidence verification, not re-execution):

- Affected-class set (OlFolder + adapter + wrapper) with coverage: 14/14 passed, 5 consecutive deterministic runs.
- `PhysicalFileSystemAdapters_Tests` in isolation: 4/4 passed across 10 consecutive runs; the seam-rework gate reports 12/12 for the filtered adapter+wrapper set, including the target test.
- Both target tests pass deterministically after the fix.
- Full-assembly local run is blocked by the pre-existing Moq binding-redirect issue (863 of the failures), identically on baseline; the only post-change delta in the full-assembly noise is one unrelated out-of-scope flaky Win32 shell-icon test (`ShellUtilities_Tests.GetFileIcon_...`), which flips under load and is not introduced by this branch.

Verdict: PASS for the change scope (target tests deterministic; no in-scope regression). Full-suite green confirmation deferred to PR #177 CI (AC7).

## 7. Code Quality Checks

- CSharpier: PASS — `dotnet tool run csharpier format .` clean per the 13-58-59Z QA-GATE; scoped files needed no rewrite beyond the authored edits.
- Analyzers (msbuild `EnableNETAnalyzers`/`EnforceCodeStyleInBuild`): PASS — Build succeeded, 0 Error(s). 19 pre-existing project-wide warnings outside the scoped files; zero warnings in either scoped file; zero analyzer delta versus baseline.
- Nullable (msbuild `Nullable=enable`/`TreatWarningsAsErrors`): PASS for scoped files — the command-line override forces nullable on projects that do not opt in, surfacing the pre-existing CS86xx baseline (905 errors, identical to baseline; delta 0). `UtilitiesCS` and `PhysicalFileInfoAdapter.cs` are nullable-clean. The single scoped-file diagnostic (`OlFolderClassifierGroup_Tests.cs(29,49)` `_originalDialogInvoker`) is pre-existing on untouched code.

Verdict: PASS (no analyzer/nullable/format delta introduced by the change).

## 8. Gaps and Exceptions

1. `issue.md` is absent from the feature folder, so the persisted Work-Mode marker is missing. Per the workflow this fails closed to `full-feature`; the workflow input authoritatively names `spec.md` AC1-AC7 (the `full-bug` AC source), which is appropriate for this defect. AC evaluation used `spec.md` AC1-AC7. This is a documentation gap, not a code defect; no `user-story.md` exists either. Recommend adding `issue.md` with an explicit `- Work Mode: full-bug` marker for future audits.
2. AC7 (PR CI green on `main` and post-merge `main` CI green) is not locally verifiable and is pending the GitHub Actions run on PR #177. Recorded as PARTIAL (pending external CI). This is the only non-PASS criterion and is environmental, not a defect in the change. It drives the remediation handoff for the CI-green confirmation step.
3. The QA-GATE at `2026-06-08T13-29-05Z` states "No production files changed," which was accurate for that batch (Defect 1 only) but predates the Defect 2 seam rework recorded at `2026-06-08T13-58-59Z`. The later gate correctly records the production seam. This is a timeline artifact across two gate snapshots, not a misstatement of the final state. No action required.
4. Repo-wide full-assembly C# coverage was not produced locally due to the pre-existing Moq binding-redirect failure; the committed Cobertura evidence is trimmed to affected classes. The authoritative repo-wide gate is PR #177 CI. Documented above; no untested new production path is introduced.
5. The spec's "Scope & Non-Goals" line "Out of scope: production code changes" is superseded within the same spec by AC3 and the "Verification deviation" section, which document the accepted narrow production seam and the reason a mocked `IFileInfo` was infeasible. This is an internally reconciled, documented deviation, not a hidden scope breach.

## 9. Summary of Changes

C# production (1): `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs` (+35/-6) — converted the primary-constructor form to two explicit constructors; added three private delegate fields (`_appendText`, `_openByMode`, `_openWrite`); public constructor binds them to the wrapped `FileInfo`; new `internal` constructor accepts injected delegates with null guards; `AppendText`, `Open(FileMode)`, and `OpenWrite` now invoke the delegates.

C# tests (2): `UtilitiesCS.Test/EmailIntelligence/OlFolderClassifierGroup_Tests.cs` (+11/-2) — `BuiltGroupingKeys` changed from `List<string>` to `ConcurrentBag<string>` exposed as `IEnumerable<string>`; concurrent callback writes to the bag. `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` (+43/-23) — removed the write/append/read-write opens on the real `.sln`; write-mode delegation now exercised through the seam with sentinel streams and `BeSameAs`; read-only real-file delegation retained.

Docs/evidence: `spec.md`, `plan.2026-06-08T09-16.md`, and the `evidence/baseline/` and `evidence/qa-gates/` subtrees (BASELINE.md, two QA-GATE.md, trimmed Cobertura XML; large `.trx`/`.coverage` left on disk uncommitted).

## 10. Compliance Verdict

**PARTIAL.**

Blocking findings on the change itself: 0.

The code change, tests, toolchain results, and per-file coverage all PASS. The only non-PASS criterion is AC7 (external CI green on `main`), which is not locally verifiable and is pending the PR #177 GitHub Actions run. Per the workflow, a PARTIAL/pending required acceptance criterion is a remediation trigger; a remediation-inputs artifact is produced to track the single remaining action (confirm PR #177 CI green and post-merge `main` CI green). No code remediation is required.

Recommendation: conditional go for PR. The change is ready to merge from a policy and code-quality standpoint; AC7 must be confirmed green on the PR #177 CI run before final close, per the spec's own rollout plan.

## Appendix A: Test Inventory

Tests affected by this branch (verified by reading the diff and sources):
- `UtilitiesCS.Test.EmailIntelligence.OlFolderClassifierGroup_AdditionalTests.BuildClassifiersAsync_WithFixtureAndFolderConfig_StoresBuiltFolderClassifier` — now deterministic under parallel execution via the `ConcurrentBag` tracking store.
- `UtilitiesCS.Test.HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` — write-mode delegation now covered via the injectable seam and `BeSameAs`; read-only real-file delegation retained.

## Appendix B: Toolchain Commands Reference

Commands recorded in feature evidence (review is evidence verification; the reviewer additionally parsed the committed Cobertura artifacts directly):
1. Format: `dotnet tool run csharpier format .` (pinned local tool; bare `csharpier .` errors).
2. Analyzers: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. Nullable: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. Test + coverage (filtered, per the documented local binding-redirect workaround): `vstest.console.exe UtilitiesCS.Test.dll /TestCaseFilter:'FullyQualifiedName~PhysicalFileSystemAdapters_Tests|FullyQualifiedName~FileInfoWrapper_Tests' /EnableCodeCoverage`
5. Coverage verification (reviewer): parsed committed Cobertura `evidence/baseline/2026-06-08T13-21-38Z/baseline.cobertura.xml` and `evidence/qa-gates/2026-06-08T13-58-59Z/postchange.cobertura.xml` for the `PhysicalFileInfoAdapter` class `line-rate` and method line-rates.
6. Authoritative repo-wide full-suite C# coverage gate: GitHub Actions CI on PR #177 (deferred; local full-assembly run blocked by pre-existing Moq binding redirect).
