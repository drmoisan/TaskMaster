# Policy Audit — collection-lock-recursion-coverage-317 (Issue #317)

- Timestamp: 2026-07-11T22-30
- Branch: `test/collection-lock-recursion-coverage-317`
- Base branch: `main`
- Resolved merge-base: `5ecbc4c61bd87ac09b75d52a8913d7e53b410343` (verified via `git merge-base HEAD main` in this session; matches the caller-supplied `5ecbc4c6` and equals `main`'s current tip, so branch and base have not diverged further since the branch was cut)
- Head SHA reviewed: `532d080f097c8da50a28f6867296569bdb886fa2`
- Work mode: `full-bug` (per `plan.2026-07-11T19-27.md`'s persisted marker; `spec.md` is the sole AC source, no `user-story.md` exists for this feature — consistent with the caller's instruction)
- Reviewer tool set: Read/Grep/Glob/Bash/Write/Edit; no `mcp__drm-copilot__*` tools were available in this session.

## Note on MCP Template/Collector Unavailability

- `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__collect_pr_context` are not present in this session's tool list. Per `policy-audit-template-usage`'s fallback instruction and the established manual-fallback pattern for this repo, this audit is authored against the full canonical heading set from that skill (Executive Summary + sections 1–10 + Appendix A/B) without the MCP-resolved template file, and `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt` were hand-authored in this session from verified `git diff --numstat main HEAD` / `git diff main HEAD` output rather than the real collector. This is a substitute, not the tool output, and is recorded here for auditability.

## Rejected Scope Narrowing

None detected. The calling prompt explicitly instructed full-branch-diff scope and did not attempt to narrow to a plan/task/phase subset. No caller text triggered the Scope Invariant's rejection clause.

## Evidence Location Compliance

- No `validate_evidence_locations.py` script exists in this repository (this is a cross-repo artifact referenced in prior memory notes, not a real TaskMaster tool); the check below is a manual scan.
- Manual scan of `git diff --name-only main HEAD` for `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`: **zero matches**.
- All 21 feature-folder evidence files added by this branch resolve under the canonical `docs/features/active/2026-07-11-collection-lock-recursion-coverage-317/evidence/{baseline,regression-testing,qa-gates,other}/` sub-paths, consistent with `evidence-and-timestamp-conventions`.
- The plan's `artifacts/csharp/coverage.xml` output is the canonical, skill-documented tool-consumed coverage artifact path for C# (see the Coverage Artifact Paths table), not a forbidden evidence path. No violation.
- **Verdict: PASS.**

## Executive Summary

This is a minimal, test-only restoration: one new MSTest test file (88 lines, 2 `[TestMethod]`s) and a single `<Compile Include>` line added to `UtilitiesCS.Test.csproj`. No production `.cs` file is touched. The change restores regression coverage for a lock-recursion hazard test that a prior epic child (F5, #308) deleted in error (independently corroborated by four separate sources cited in `research.2026-07-11T21-15.md` and re-verified directly by this review — see §3). All 5 acceptance criteria in `spec.md` are evidenced; 4 are fully earned as worded, and AC-5 ("Full C# toolchain passes in a single final pass") is **PARTIAL** as literally worded because the nullable/`TreatWarningsAsErrors` build step returns exit code 1 — this is an identical, pre-existing, unrelated 34-error failure in the vendored `SVGControl.csproj` project, confirmed present at baseline before this change and unchanged by it. Repo-wide C# coverage, read from the available Cobertura artifact, is well under the 80%/85% floors in both raw and first-party-only readings; this is also a pre-existing condition not introduced or worsened by this change (the touched `UtilitiesCS` package coverage held at 88.34%→88.35%). Overall disposition: **PASS on substance**, with two pre-existing, non-blocking findings recorded below and no remediation required from this feature's own delivered work.

## 1. General Unit Test Policy Compliance

- **Independence/Isolation/Determinism**: PASS. Both tests construct a fresh `ConcurrentObservableCollection<int>`, no shared state, no I/O, no timers/sleeps, deterministic FluentAssertions assertions.
- **Fast execution**: PASS. Two synchronous, in-memory tests.
- **Readability/AAA structure**: PASS. Both methods use `// Arrange`, `// Act`/`// Act & Assert`, `// Assert` comments and descriptive XML doc summaries explaining the historical Swordfish hazard and why it no longer applies.
- **Scenario completeness**: The spec explicitly scopes this as guard-rail regression coverage for a hazard that cannot occur today "by construction" (both tests assert the safe, non-throwing path only). No negative-path test is claimed or required by spec.md; this is consistent with the restoration's stated scope (no new behavior, only restored coverage).
- **External dependencies**: PASS. No mocks needed (concrete type under test), no network/filesystem/temp files.
- **Coverage requirements** (`.claude/rules/general-unit-test.md` / `quality-tiers.md`: line ≥85%, branch ≥75% uniformly; CLAUDE.md: repo-wide ≥80%, new code ≥90%, no changed-line regression) — see §5 for full detail:
  - New file (`ConcurrentObservableCollectionLockRecursionTests.cs`): line-rate 100% (1.0) across all 4 generated class entries. **PASS** against both the 90% (CLAUDE.md) and 85% (quality-tiers) new-code floors.
  - Changed-line regression on touched production surface: none — production `ConcurrentObservableCollection<T>` is untouched; the `UtilitiesCS` package line-rate moved from 88.34% to 88.35% (marginal increase). **PASS.**
  - Repo-wide C# coverage (from `artifacts/csharp/coverage.xml`): **FAIL** against both the 80% and 85% floors — see §5 for the numeric breakdown and disposition (pre-existing, non-blocking).
- **Test file location**: The test lives in `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/`, mirroring its two living siblings (`ConcurrentObservableCollection_Tests.cs`, `ConcurrentObservableCollectionSerialization_Tests.cs`) in the same folder — this is the repo's pre-existing, established C#-project convention (a sibling `*.Test` project mirroring production folder structure, not a literal `tests/` directory), consistent with every other test in this project. **PASS** (no colocation in production source tree).

## 2. General Code Change Policy Compliance

- **Simplicity/reusability/extensibility/separation of concerns**: PASS. No production code touched; test-only restoration with a documented, minimal one-line namespace normalization from the recovered pre-deletion content.
- **File size limit (500 lines)**: PASS. New file is 88 lines. `UtilitiesCS.Test.csproj` is a pre-existing 901-line project file receiving a single added line; project/build manifest files are not "production code, test code, or reusable script" files under this rule's plain meaning, and the file's size was not created or materially changed by this PR (net +1 line).
- **Error handling/logging/contracts**: N/A — no executable production logic added; tests only assert on `Should().NotThrow()` / value equality.
- **Naming**: PASS. `PascalCase` type/method names, descriptive test names (`Add_When...HandlerReadsCountFromCollection_DoesNotThrow`) that state scenario and expected outcome per repo convention.
- **Public API compatibility**: PASS. No public API changed.
- **Dependencies**: PASS. Only already-approved libraries used (MSTest, FluentAssertions); no new dependency added.
- **I/O boundaries / temp files**: PASS. No I/O, no temp files.

## 3. Language-Specific Code Change Policy Compliance (C#)

- **CSharpier formatting**: PASS on final pass. First `csharpier check` attempt failed on line-ending mismatch (exit code 1); `csharpier format` was run and the phase restarted per its own loop-restart rule; second `check` passed (exit code 0, "Checked 1 files"). Independently re-verifiable: `git diff --stat main` continues to show exactly the two planned files after the fix (`evidence/qa-gates/csharpier-check.2026-07-11T20-15.md`).
- **.NET analyzer diagnostics** (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild`): PASS. Post-change build succeeded, 0 errors, 20 pre-existing warnings unrelated to the touched file (verified via evidence's explicit grep-confirmed "zero diagnostics reference `ConcurrentObservableCollectionLockRecursionTests.cs`").
- **Nullable/`TreatWarningsAsErrors`**: **FAIL** as a literal pass/fail reading of this individual toolchain step (exit code 1, 34 errors) — **disposition: pre-existing, non-blocking**. All 34 errors are in the vendored `SVGControl\SVGControl.csproj` project (CS8618/CS8600/CS8601/CS8602/CS8603/CS8625/CS0649), identical in count, project, and diagnostic codes to the baseline run captured before any restoration edit (`evidence/baseline/baseline-nullable-build.2026-07-11T19-52.md`, also exit code 1, same 34 errors). Zero errors reference `UtilitiesCS.Test` or either of the two files this change touches (confirmed via the evidence's own grep, and independently plausible given the change is additive-only in an unrelated project). This finding is orthogonal to the change under review, not introduced or worsened by it.
- **MSBuild/Build (plain, no special properties)**: PASS. Post-restore build (`P2-T1`) succeeded with zero compile errors on the restored file and csproj change.

## 4. Language-Specific Unit Test Policy Compliance (C#)

- **Framework**: PASS. `[TestClass]`/`[TestMethod]` (MSTest), no xUnit/NUnit introduced.
- **Assertion library**: PASS. FluentAssertions (`Invoking(...).Should().NotThrow(...)`, `.Should().Be(...)`) used throughout; no bare MSTest `Assert` calls.
- **Mocking**: N/A — no external dependency to mock; concrete `ConcurrentObservableCollection<int>` under test, consistent with CUT2's "use Moq" guidance only applying when mocking is needed.
- **Toolchain commands used**: match the repo-approved commands in `CUT3`/`csharp.md` (csharpier, msbuild analyzer build, msbuild nullable build, vstest with `/EnableCodeCoverage`).

## 5. Test Coverage Detail

### Coverage Evidence Checklist (mandatory per-language rows)

- **TypeScript**: N/A — zero `.ts`/`.tsx` files changed on this branch (`git diff --name-only main HEAD | grep -E '\.tsx?$'` returns no matches). Zero-changed-files is a permitted basis for N/A per the coverage-verification procedure.
- **Python**: N/A — zero `.py` files changed on this branch.
- **PowerShell**: N/A — zero `.ps1`/`.psm1` files changed on this branch.
- **C#**: coverage-scoped verdict required (`.cs`/`.csproj` files changed). **FAIL** on the repo-wide gate; **PASS** on the new-code and changed-line-regression gates. See numeric breakdown below. This verdict is not a scope-narrowing statement — it is an explicit FAIL on the repo-wide component, dispositioned as pre-existing and non-blocking for this specific PR.

### 5.1 Numeric Coverage — C# (artifact: `artifacts/csharp/coverage.xml`, Cobertura format)

This artifact was produced by `vstest.console.exe "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /EnableCodeCoverage /InIsolation` — a **single-test-assembly-scoped run**, not a full-solution multi-project test run. This matters: first-party assemblies not exercised by `UtilitiesCS.Test.dll`'s own suite (`ToDoModel`, `Tags`, `QuickFiler`) read 0% in this artifact not because they are untested in the repository, but because their own dedicated test projects were not part of this run.

| Metric | Baseline (P0-T9) | Post-change (P3-T4) | Change | Disposition |
|---|---|---|---|---|
| `UtilitiesCS` package line-rate (touched production package) | 88.34% | 88.35% | +0.01pp | PASS — no regression |
| Repo-wide, all Cobertura packages (raw, vendor-inflated denominator) | 60.68% (97230/160234) | 60.69% (97272/160270) | +0.01pp | FAIL vs. 80%/85% floor — pre-existing, single-assembly-scoped artifact, not this PR's regression |
| Repo-wide, first-party packages only, production-only (this review's independent recomputation: `ToDoModel`+`Tags`+`TaskMaster`+`SVGControl`+`UtilitiesCS`+`QuickFiler`, vendor packages `System.Linq.Async`/`log4net`/`FluentAssertions`/`Mono.Reflection`/`System.Interactive`/`Deedle`/`FSharp.Core` excluded by name) | not separately computed at baseline | 71969/108775 = 66.16% | n/a (single reading) | FAIL vs. 80%/85% floor — same pre-existing, single-assembly-scoped-run caveat; `ToDoModel`/`Tags`/`QuickFiler` at literal 0% in this run because this run did not execute their own test projects |
| New/changed-code coverage: restored test file (`ConcurrentObservableCollectionLockRecursionTests.cs`) | n/a (did not exist) | 100% (line-rate 1.0, all 4 generated class entries) | new | PASS — exceeds both the 90% (CLAUDE.md) and 85% (quality-tiers) new-code floors |

**Independent verification performed by this review**: this reviewer parsed `artifacts/csharp/coverage.xml` directly (Python `re`-based per-package `<line hits>` aggregation) rather than relying solely on the feature's own evidence narrative, and independently reproduced the 60.69% raw root `line-rate` figure and the per-package breakdown (`UtilitiesCS` 88.37% ≈ evidence's claimed 88.35%; small rounding-method difference between whole-package-line-rate-attribute vs. line-by-line recount, immaterial to the PASS/FAIL call).

**Disposition — repo-wide C# coverage (both readings)**: FAIL against the 80%/85% floor is the correct literal verdict for this artifact. It is dispositioned **non-blocking** for this PR because: (a) this two-file, test-only, additive-only change cannot itself move repo-wide coverage in the direction of the floor — the only production package it touches (`UtilitiesCS`) held steady or improved; (b) the low reading is an artifact of a single-assembly-scoped `vstest` invocation that does not exercise `ToDoModel`/`Tags`/`QuickFiler`'s own dedicated test suites, not evidence that those suites do not exist or do not pass; (c) prior reviews of this repository (documented in persistent review memory for issues #171, #253, #269, #278, #283, #309) have consistently found that a genuine full-solution run, or a by-name vendor-package exclusion, reads meaningfully higher (historically ~79–91% depending on methodology) — still a pre-existing, marginal-to-moderate shortfall against the 80%/85% floor, but categorically different from, and not attributable to, this specific change. **Recommendation** (non-blocking, tracked as a backlog/tooling item, not a code defect in this PR): produce the canonical coverage artifact from a full-solution `vstest` run (all `*.Test.dll` assemblies together) so the repo-wide row reflects genuine first-party coverage rather than a single-project-scoped run.

### 5.2 Branch Coverage

The Cobertura artifact reports `branch-rate="1"` at the root and for every `<package>` element. This is very likely an artifact of the conversion tool (`dotnet-coverage merge -f cobertura`) not populating branch-level data from the underlying `.coverage` binary rather than genuine 100% branch coverage across ~150k+ lines of first-party and vendored code. This reviewer treats the reported branch-rate figure as unconfirmed rather than a true 100% PASS, and notes it for future tooling improvement; it does not change the PASS/FAIL C# coverage-row requirement above, which is driven by line coverage.

## 6. Test Execution Metrics

- Baseline: 4211 tests, 4211 passed, 0 failed (`evidence/baseline/baseline-test-coverage.2026-07-11T19-50.md`).
- Post-change: 4213 tests (4211 + 2 newly-restored), 4213 passed, 0 failed (`evidence/qa-gates/post-change-test-coverage.2026-07-11T20-25.md`).
- Targeted run of the two new tests: 2/2 passed (`evidence/regression-testing/restored-tests-pass.2026-07-11T20-07.md`).
- Zero new failures; zero pre-existing failures reappeared (baseline and post-change both report 0 failures — an exact match).
- **Verdict: PASS.**

## 7. Code Quality Checks

- Format (CSharpier): PASS (final pass, after one loop restart — see §3).
- Lint/analyzers: PASS (0 errors post-change).
- Type-check/nullable: FAIL on this specific gate, pre-existing and unrelated — see §3 disposition.
- Architecture-boundary tests: N/A — no architecture-boundary tooling (e.g., NetArchTest.Rules) is configured for this project in this repo; not applicable to a test-only change.
- Unit tests: PASS (see §6).
- Contract/schema compatibility: N/A — no public contract changed.
- Integration tests: N/A — no adapter/external-system code touched.

## 8. Gaps and Exceptions

1. **Nullable/`TreatWarningsAsErrors` build step fails** (34 pre-existing `SVGControl.csproj` errors, identical at baseline and post-change, zero references to files touched by this PR). Non-blocking; recommend as a separate repo-hygiene backlog item, not attributable to this change.
2. **Repo-wide C# coverage reads below the 80%/85% floor** from the available single-assembly-scoped Cobertura artifact (60.69% raw / 66.16% first-party-production-only by this reviewer's independent recomputation). Non-blocking for this PR for the reasons in §5.1; recommend producing a genuine full-solution coverage run as a separate tooling backlog item.
3. **Reported branch-rate of 100% in the Cobertura artifact is not trusted** as genuine — likely a tooling/conversion limitation, not a real signal. Recorded as unconfirmed, not used to fail or pass any gate.
4. **Coverage-threshold policy-document conflict** (pre-existing, not introduced by this PR): CLAUDE.md states an 80% repo-wide / 90% new-code floor with a documented COM/VSTO/WinForms exemption; `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state a uniform 85% line / 75% branch floor with "tier-specific lower thresholds are not used." This audit applied the stricter 85%/75% reading from the currently-loaded project rules (consistent with this review task's own explicit coverage-threshold instructions) while also reporting the CLAUDE.md 80%/90% figures for completeness. Neither reading changes the verdict here (both new-code and no-regression gates pass under either policy; the repo-wide gate fails, non-blocking, under either policy). Flagging for the user rather than halting, since the conflict predates this PR and blocking on it would prevent delivering the required audit artifacts for a review that did not create the conflict.
5. **No remediation-inputs artifact is produced** for this feature. All findings above are pre-existing, evidenced-identical-at-baseline conditions unrelated to the two files this PR touches; none constitute a blocking finding introduced by this change.

## 9. Summary of Changes

- Added: `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs` (88 lines, 2 `[TestMethod]`s, MSTest + FluentAssertions, restored verbatim from pre-deletion content at `0ec111b2~1` except for one normalized namespace line, independently re-verified by this reviewer via `git show 0ec111b2~1:<path>`).
- Modified: `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (+1 line, one `<Compile Include>` entry, positioned immediately after its sibling entry as planned).
- No production `.cs` file changed. No other test file changed. `git diff --stat main HEAD` (code files only) confirms exactly these two files.

## 10. Compliance Verdict

**Overall: PASS**, with two pre-existing, non-blocking findings (§8.1, §8.2) that are unrelated to and unworsened by this change, and one tooling-trust caveat (§8.3) that does not affect the verdict. No blocking findings were identified in the diff under review.

## Appendix A: Test Inventory

| Test Method | File | Framework | Assertion Library | Mocking | Result |
|---|---|---|---|---|---|
| `Add_WhenCollectionChangedHandlerReadsCountFromCollection_DoesNotThrow` | `ConcurrentObservableCollectionLockRecursionTests.cs` | MSTest | FluentAssertions | none (concrete type) | PASS (targeted + full-suite runs) |
| `Add_WhenCollectionChangedHandlerUsesNewItemsFromEventArgs_DoesNotThrow` | `ConcurrentObservableCollectionLockRecursionTests.cs` | MSTest | FluentAssertions | none (concrete type) | PASS (targeted + full-suite runs) |

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier check "UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs"` (then `format` on first-attempt failure)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` (plus a follow-up plain `/t:Build` to restore Debug binaries wiped by `/t:Rebuild`)
4. `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
5. `dotnet-coverage merge -o artifacts/csharp/coverage.xml -f cobertura <path-to>.coverage`
