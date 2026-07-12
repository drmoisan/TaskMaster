# collection-lock-recursion-coverage-317 (Plan)

- **Issue:** #317
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-11T19-27
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug (spec.md present, defect/restoration; no user-story.md; enforces spec-driven expectations and the full QA loop per `atomic-plan-contract`)
- **Feature folder (`<FEATURE>`):** `docs/features/active/collection-lock-recursion-coverage-317`
- **Worktree:** `C:/Users/DanMoisan/repos/TaskMaster-wt/collection-lock-recursion-coverage-317`, branch cut from `main` at `5ecbc4c6`
- **Timestamp token:** every `<TS>` placeholder below MUST be substituted with the real ISO-8601
  timestamp (`yyyy-MM-ddTHH-mm`) at the moment the artifact is written, per
  `evidence-and-timestamp-conventions`.

## Deviation from standard Bugfix Workflow (documented, not a policy violation)

CLAUDE.md's Bugfix Workflow normally requires a failing regression test first. Per spec.md's Root
Cause Analysis and Assumptions sections, this issue has no reproducible failing behavior today: the
hazard the deleted test guarded against (`LockRecursionException` on re-entrant reads inside a
synchronous `CollectionChanged` handler) cannot occur on the current lock-free
`ObservableCollection<T>`-based `ConcurrentObservableCollection<T>`. Both restored `[TestMethod]`s
pass by construction. This plan therefore treats "restore the missing regression guard" as the
deliverable itself (per spec.md and the calling brief) and does not tag any task `[expect-fail]`.

## Evidence location note

All evidence artifacts in this plan resolve to `<FEATURE>/evidence/<kind>/` per the
Non-Overridable Evidence Path Clause in `evidence-and-timestamp-conventions`. No non-canonical path
(e.g. `artifacts/baselines/`, `artifacts/qa/`) is used anywhere in this plan. One additional
non-evidence, tool-consumed artifact is produced at `artifacts/csharp/coverage.xml` (Cobertura XML)
for the repo's canonical coverage-gate consumption; this path is separate from, and in addition to,
the canonical evidence artifacts recorded under `<FEATURE>/evidence/qa-gates/`.

**Fail-closed evidence rule:** If any required baseline artifact, verification artifact, or QA
artifact is missing or incomplete, the plan's overall outcome MUST be treated as remediation-required,
never PASS.

---

### Phase 0 — Baseline Capture & Policy Read

- [x] [P0-T1] Read `CLAUDE.md` in full (policy reading order position 1) in the feature worktree
      `C:/Users/DanMoisan/repos/TaskMaster-wt/collection-lock-recursion-coverage-317/CLAUDE.md`.
      Acceptance: the file has been read in this execution session (confirmed by quoting its
      Policy Compliance Order section verbatim in the Phase 0 evidence artifact from P0-T5).
- [x] [P0-T2] Read `.claude/rules/general-code-change.md` (policy reading order position 2) in the
      feature worktree. Acceptance: file read; its Mandatory Toolchain Loop section quoted in the
      P0-T5 evidence artifact.
- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` (policy reading order position 3) in the
      feature worktree. Acceptance: file read; its Coverage Requirements section quoted in the
      P0-T5 evidence artifact.
- [x] [P0-T4] Read `.claude/rules/csharp.md` (policy reading order position 4, C#-specific) in the
      feature worktree. Acceptance: file read; its Toolchain section quoted in the P0-T5 evidence
      artifact.
- [x] [P0-T5] Write the Phase 0 policy-read evidence artifact to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/baseline/phase0-instructions-read.md`
      containing at minimum `Timestamp:`, `Policy Order:` (the 4-item list from P0-T1..P0-T4 in
      order), and an explicit list of the four file paths read. Acceptance: the file exists at the
      exact path above and contains all three required fields.
- [x] [P0-T6] Record baseline git state (current branch name and `HEAD` short SHA, obtained via
      `git rev-parse --abbrev-ref HEAD` and `git rev-parse --short HEAD` in the feature worktree) to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/baseline/git-baseline-state.<TS>.md`.
      Acceptance: artifact exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output
      Summary:` line stating the branch name and SHA.
- [x] [P0-T7] Run the baseline analyzer build:
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      in the feature worktree, before any restoration edit. Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/baseline/baseline-analyzer-build.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`
      (build succeeded/failed, warning/error counts).
- [x] [P0-T8] Run the baseline nullable build:
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
      in the feature worktree, before any restoration edit. Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/baseline/baseline-nullable-build.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T9] Run the baseline full test pass with coverage:
      `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
      in the feature worktree, before any restoration edit. Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/baseline/baseline-test-coverage.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:`
      line with the numeric total test pass/fail counts and the numeric baseline line-coverage
      percentage for `UtilitiesCS.dll`.
- [x] [P0-T10] Grep `UtilitiesCS.Test/**/*.cs` in the feature worktree for `LockRecursion` and for
      the combination of `CollectionChanged` with `DoesNotThrow`, confirming zero matches (the
      coverage-gap premise from spec.md and research.md). Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/baseline/baseline-coverage-gap-confirmation.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:` (the grep patterns used), and
      `Output Summary:` stating "0 matches" for both patterns.

---

### Phase 1 — Restore Test File & csproj Wiring

- [x] [P1-T1] Run `git show 0ec111b29923cfadd63c26908e41e069924d4ea5~1:UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`
      in the feature worktree (read-only) and record the exact recovered file content, including its
      literal pre-deletion namespace declaration line, to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/other/pre-deletion-file-recovery.<TS>.md`.
      Acceptance: artifact exists, contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and the full
      recovered file content verbatim, with the literal namespace line called out explicitly.
- [x] [P1-T2] Write
      `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`
      using the content recovered in P1-T1, unconditionally setting the namespace declaration to
      `UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection` (this is a no-op if the
      recovered content already declares this namespace; it is an explicit one-line edit if the
      recovered content declares `ConcurrentObservableCollection.Tests` or any other value), so the
      file matches the folder-mirroring convention already used by its two living siblings
      `ConcurrentObservableCollection_Tests.cs` and `ConcurrentObservableCollectionSerialization_Tests.cs`.
      The file must contain both `[TestMethod]`s
      `Add_WhenCollectionChangedHandlerReadsCountFromCollection_DoesNotThrow` and
      `Add_WhenCollectionChangedHandlerUsesNewItemsFromEventArgs_DoesNotThrow`, using MSTest
      attributes and FluentAssertions (`Should().NotThrow()`), with no mocks. Acceptance: the file
      exists at the exact path above, contains exactly one `namespace` declaration equal to
      `UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection`, and contains both
      named `[TestMethod]`s verbatim.
- [x] [P1-T3] Edit `UtilitiesCS.Test/UtilitiesCS.Test.csproj` to insert
      `<Compile Include="ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionLockRecursionTests.cs" />`
      immediately after the existing line
      `<Compile Include="ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollection_Tests.cs" />`
      (at or near line 391). Acceptance: the csproj contains the new `<Compile Include>` line exactly
      once, immediately following the sibling entry, with no other line in the file changed.

---

### Phase 2 — Targeted Verification (maps to AC-1 through AC-4)

- [x] [P2-T1] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` in
      the feature worktree and confirm the restored file and csproj change compile with zero errors.
      Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/regression-testing/post-restore-build.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      stating zero compile errors.
- [x] [P2-T2] Run
      `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:Add_WhenCollectionChangedHandlerReadsCountFromCollection_DoesNotThrow,Add_WhenCollectionChangedHandlerUsesNewItemsFromEventArgs_DoesNotThrow`
      and confirm both restored tests pass. Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/regression-testing/restored-tests-pass.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      stating `2/2 passed, 0 failed`. This satisfies AC-1.
- [x] [P2-T3] Grep the restored file for the `namespace` declaration and confirm it equals
      `UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection` exactly once. Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/regression-testing/namespace-verification.<TS>.md`.
      Acceptance: artifact contains the grep command, its output, and confirms exactly one matching
      namespace line. This satisfies AC-2.
- [x] [P2-T4] Grep `UtilitiesCS.Test/UtilitiesCS.Test.csproj` for
      `ConcurrentObservableCollectionLockRecursionTests.cs` and confirm exactly one `<Compile
      Include>` line references it, positioned immediately after the
      `ConcurrentObservableCollection_Tests.cs` entry. Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/regression-testing/csproj-wiring-verification.<TS>.md`.
      Acceptance: artifact confirms exactly one match at the expected position. This satisfies AC-3.
- [x] [P2-T5] Run `git diff --stat main` (repo root, no path filter) in the feature worktree and
      confirm the output lists exactly two changed files:
      `UtilitiesCS.Test/UtilitiesCS.Test.csproj` and
      `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`.
      Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/regression-testing/repo-wide-diff-scope.<TS>.md`.
      Acceptance: artifact contains the full `git diff --stat main` output and confirms no other
      file (production or test) appears in it. This satisfies AC-4.

---

### Phase 3 — Final QA Loop (Full C# Toolchain; maps to AC-5)

Loop behavior: if any task in this phase fails, or if any command changes files (e.g. CSharpier
reformats a file), restart this phase from P3-T1. Do not proceed to Phase 4 until all six tasks in
this phase complete without errors in a single pass.

- [x] [P3-T1] Run CSharpier format-check scoped to the single touched `.cs` file:
      `dotnet tool run csharpier check UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`
      (scoped to this one file, not repo-wide, to avoid unrelated `.csproj`/`.cs` reformatting churn
      that would violate AC-4's two-file-only diff scope). If the check reports a diff, run
      `dotnet tool run csharpier format` on the same path and restart the phase. Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/qa-gates/csharpier-check.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      confirming zero formatting diffs.
- [x] [P3-T2] Run the post-change analyzer build:
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
      Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/qa-gates/post-change-analyzer-build.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      confirming zero analyzer errors/warnings-as-errors on the touched file.
- [x] [P3-T3] Run the post-change nullable/TreatWarningsAsErrors build:
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
      Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/qa-gates/post-change-nullable-build.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      confirming zero nullable warnings/errors.
- [x] [P3-T4] Run the full post-change test pass with coverage:
      `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`.
      Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/qa-gates/post-change-test-coverage.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`
      with the numeric total test pass/fail counts (including the two newly-restored tests) and the
      numeric post-change line-coverage percentage for `UtilitiesCS.dll`. Any pre-existing failure
      must match the baseline's known pre-existing failure set exactly (zero new failures).
- [x] [P3-T5] Convert the `.coverage` binary produced by P3-T4 to Cobertura XML at the canonical
      tool-consumed path `artifacts/csharp/coverage.xml` (e.g. via
      `dotnet-coverage merge -o artifacts/csharp/coverage.xml -f cobertura <path-to>.coverage`).
      Acceptance: `artifacts/csharp/coverage.xml` exists, is well-formed XML, and parses with a
      `<coverage>` root element carrying `line-rate`/`lines-covered`/`lines-valid` attributes.
- [x] [P3-T6] Compare the baseline coverage percentage from P0-T9 against the post-change coverage
      percentage from P3-T4 and confirm no regression on the two changed files' lines (the restored
      test file's own lines are new and expected at or above 90% coverage by execution since both
      `[TestMethod]`s must pass; the touched production surface exercised — `Add`,
      `OnCollectionChanged`, `Count`, `CollectionChanged` add/remove on
      `ConcurrentObservableCollection<T>` — is unchanged and already covered by the surviving sibling
      test file, so no production coverage regression is possible). Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/qa-gates/coverage-delta-verification.<TS>.md`.
      Acceptance: artifact contains baseline coverage %, post-change coverage %, changed-file
      coverage %, and an explicit PASS/FAIL statement on "no regression on changed lines." This
      satisfies AC-5 together with P3-T1 through P3-T4.

---

### Phase 4 — Acceptance Criteria Closure & Evidence Commit

- [x] [P4-T1] Edit `docs/features/active/collection-lock-recursion-coverage-317/spec.md` to check off
      AC-1 through AC-5 under `## Acceptance Criteria`, appending an inline evidence-artifact
      reference (relative path) to each checked item. Acceptance: all five AC checkboxes in
      `spec.md` are `- [x]` and each line cites the specific evidence artifact path(s) that satisfy
      it (from Phase 2 and Phase 3 above).
- [x] [P4-T2] Write a closure-summary evidence artifact to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/other/ac-closure-summary.<TS>.md`
      listing AC-1 through AC-5, each mapped to its exact backing evidence artifact path(s) from
      Phases 2 and 3. Acceptance: the artifact exists and every AC has at least one mapped, existing
      evidence-artifact path.
- [x] [P4-T3] Run `git status --porcelain` in the feature worktree and confirm it returns empty
      (all code changes and evidence artifacts staged/committed). Record to
      `docs/features/active/collection-lock-recursion-coverage-317/evidence/other/clean-worktree-confirmation.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`
      confirming empty `git status --porcelain` output.

---

## Acceptance Criteria Coverage Map (for preflight cross-check)

- AC-1 (both `[TestMethod]`s exist and pass) → P1-T2 (creation), P2-T2 (targeted pass verification),
  P3-T4 (full-suite pass verification).
- AC-2 (namespace matches living siblings) → P1-T2 (creation), P2-T3 (verification).
- AC-3 (csproj `<Compile Include>` entry present) → P1-T3 (creation), P2-T4 (verification).
- AC-4 (only two files touched, repo-wide) → P2-T5 (verification), P4-T3 (clean-worktree
  confirmation).
- AC-5 (full toolchain passes, zero regressions, no coverage regression) → P3-T1 through P3-T6.
