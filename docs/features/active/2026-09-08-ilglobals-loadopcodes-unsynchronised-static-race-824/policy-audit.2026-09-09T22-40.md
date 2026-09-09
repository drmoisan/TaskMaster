# Policy Audit — Issue #824 (ILGlobals.LoadOpCodes unsynchronised static race)

- **Feature:** `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824`
- **Issue:** #824
- **Work Mode:** `full-bug` (marker read at `issue.md:12`) — `spec.md` is the sole AC source
- **Reviewer:** feature-review agent
- **Timestamp:** 2026-09-09T22-40
- **Base branch:** `epic/review-residuals-2026-09-08-integration`
- **Branch base commit:** `553f874a287261af0dd42e4f9270d31ac475308a`
- **Overall verdict:** **PASS** — 0 blocking findings

---

## Template Provenance

The `policy-audit-template-usage` skill requires the template be resolved through
`mcp__drm-copilot__resolve_policy_audit_template_asset`. No `mcp__drm-copilot__*` tool is exposed in
this session, so the asset could not be resolved and
`mcp__drm-copilot__validate_orchestration_artifacts` could not be run. Per the recorded precedent for
this condition, this artifact is hand-authored preserving all twelve canonical major headings rather
than being marked BLOCKED. The unavailability is recorded here rather than left implicit.

## Verification Constraints (tool restriction, recorded rather than concealed)

The delegating prompt banned the Bash tool for this review, citing a recorded hang risk for
`git -C` invocations from a feature-review agent in this repository. Consequences, stated precisely:

- No `git diff`, `git merge-base`, `git log`, or `git status` was executed by this reviewer.
- The change footprint used here is the caller-supplied measurement against `553f874a`, corroborated
  where possible by direct on-disk inspection (file contents, line counts, Cobertura documents,
  msbuild and vstest logs).
- Every claim below is labelled with its verification basis: **re-derived** (this reviewer measured
  it directly from the working tree or a coverage/log artifact) or **evidence-attested** (read from a
  committed evidence artifact and not independently re-derivable without git).

This restriction materially reduced only the byte-for-byte unchanged-file proofs (AC5, AC8, AC9,
AC10). All four are corroborated by independent secondary observations recorded per criterion in
`feature-audit.2026-09-09T22-40.md`.

## Rejected Scope Narrowing

**None detected.** The caller supplied the full branch footprint against the correct base branch and
did not attempt to limit the audit to a plan, task, phase, or file subset. Two caller statements were
checked and found to be correct rather than narrowing:

1. "There is no `user-story.md` and its absence is correct, not a gap." Verified: `issue.md:12`
   records `- Work Mode: full-bug`, and the `acceptance-criteria-tracking` skill maps `full-bug` to
   `spec.md` only. Not a narrowing.
2. "The three unchecked boxes in spec.md's Impact / Severity block are severity selectors, not
   acceptance criteria." Verified by reading `spec.md:29-33`: they are Blocker / High / Low with
   `Medium` selected. Not a narrowing.

## Evidence Location Compliance

`validate_evidence_locations.py` was not run (Bash banned). A `Glob` over
`C:/Users/DanMoisan/repos/TaskMaster-wt/rr0908-824/artifacts/**/*` returned six paths, all
pre-existing PR-body artifacts from unrelated issues (`pr_body_564`, `pr_body_565`, `pr_body_735`
and their receipts). **Zero** files exist under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/`.

All 51 evidence artifacts this feature produced are under
`<FEATURE>/evidence/{baseline,regression-testing,qa-gates,other}/`, which are canonical kinds per
`evidence-and-timestamp-conventions`. **PASS.** (Re-derived by `Glob`.)

---

## Executive Summary

Issue #824 is a publication race on two `public static` opcode-table fields. The delivered fix
converts both to `public static readonly` with no initializer, moves the reflection fill loop into an
explicit static constructor that builds each table in a local and assigns the field exactly once
after the loop completes, and reduces `LoadOpCodes()` to
`RuntimeHelpers.RunClassConstructor(typeof(ILGlobals).TypeHandle);`.

The change footprint is two source files plus documentation and evidence. No project file, policy
document, analyzer configuration, coverage threshold, or coverage exclusion list was touched.

All twelve acceptance criteria are verified PASS. Both behavioural gates (AC2, AC3) carry
deterministic fail-before / pass-after pairs with discovery-count controls. The full C# toolchain
passed with one loop restart, and every gate figure the executor reported was independently
re-derived by this reviewer from the on-disk logs and Cobertura documents.

Blocking findings: **0**. Four non-blocking observations are recorded in section 8.

---

## 1. General Unit Test Policy Compliance

Source: `.claude/rules/general-unit-test.md`.

| Requirement | Verdict | Evidence and verification basis |
|---|---|---|
| Independence — any order | **PASS** | Re-derived. No test in `ILGlobals_Tests.cs` writes shared state. The AC2 test at :43-69 reads two static fields and calls a method that is a no-op after type initialization, so it cannot perturb a sibling. AC2's own criterion text at `spec.md:460-467` establishes order-independence on the unfixed tree by case analysis over the only two possible orderings. |
| Isolation — one unit per test | **PASS** | Re-derived. Each of the six new/reworked tests asserts a single property of a single member. |
| Fast execution | **PASS** | Evidence-attested and corroborated. Scoped class run: 14 tests in 1.3307 s (`evidence/qa-gates/ac11-named-tests.2026-09-09T16-09.md`). Full suite 7212 tests in 30.0303 s, re-derived from `coverage/coverage-post-change.log:7233-7239`. |
| Determinism — no flakiness | **PASS** | Re-derived. A `Grep` over the whole `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/` directory for `Thread\.Sleep\|Task\.Delay\|Stopwatch\|DateTime\.Now\|DateTime\.UtcNow\|Random\|await \|Parallel\.\|new Thread\|ThreadPool\|Sleep\(\|TimeSpan\|Retry\|\[DoNotParallelize\]` returned **zero matches**. The delivered gates use only reference identity (`BeSameAs`) and `FieldInfo.IsInitOnly` reflection. No test asserts a probabilistic property, samples a race, counts consecutive green runs, or applies a timing tolerance. |
| Readability and maintainability | **PASS** | Re-derived. Every new test carries an XML documentation comment stating its role; all assertions carry `because` reason strings. |
| Line coverage >= 85 % | **PASS** | Re-derived. `/coverage/@line-rate` = `0.856241` read directly from `coverage/post-change.cobertura.xml:2` (85.6241 %). |
| Branch coverage >= 75 % | **PASS** | Re-derived. `/coverage/@branch-rate` = `0.798129` from the same element (79.8129 %). |
| No regression on changed lines | **PASS** | Re-derived. See section 5. |
| Coverage exclusion policy — no production file excluded | **PASS** | Re-derived. `Grep` for `ExcludeFromCodeCoverage` over `ILGlobals.cs` returns 0. `coverage.config` and `.editorconfig` are absent from the change footprint. No `exclude` entry was added anywhere. |
| Scenario completeness — positive flows | **PASS** | Re-derived. Publication, length, reference identity, init-only encoding, and exhaustive population are all asserted. |
| Scenario completeness — edge/boundary | **PASS** | Re-derived. `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` (`ILGlobals_Tests.cs:127-188`) enumerates every `public static` `OpCode` field on `typeof(OpCodes)`, which exercises boundary indices `0x00` and `0xFF` of both tables, and asserts the `(value & 0xff00) == 0xfe00` classification for every multi-byte opcode. |
| Scenario completeness — error handling | **PASS with a called-out exception** | Re-derived. The `throw new Exception("Invalid OpCode.")` path at `ILGlobals.cs:161` is untested and is the only uncovered code in the class. It is unreachable over the fixed `System.Reflection.Emit.OpCodes` set, is pre-existing (the identical two lines were uncovered at baseline), and is explicitly called out at `spec.md:417-420`, which satisfies the UT5 requirement to state the exception rather than leave it implicit. |
| Scenario completeness — concurrency | **PASS with a called-out exception** | Re-derived and assessed. No concurrency test exists. `spec.md:386-391` argues that a test keyed to the exit code of a known-intermittent failure cannot fail in one direction and is therefore unusable as a gate. That reasoning is correct, and the chosen substitute — testing the publication property structurally — is the right one under the determinism ban. Recorded as a justified exception, not a gap. |
| Arrange–Act–Assert structure | **PASS** | Re-derived. Every test carries explicit `// Arrange`, `// Act`, `// Assert` comments; the two publication tests carry `// Assert` only with an XML comment explaining that reading the field *is* the property under test, which is correct rather than a missing Act. |
| Clear failure messages | **PASS** | Re-derived. All six new/reworked assertions supply `because` strings; the AC4 test formats the opcode field name and index into its reasons. |
| No external dependencies | **PASS** | Re-derived. Reflection over `System.Reflection.Emit.OpCodes` only. No filesystem, network, database, or process dependency. |
| No temporary files | **PASS** | Re-derived. No `Path.GetTempFileName`, `Path.GetTempPath`, or file I/O of any kind in either changed file. |
| Test file location mirrors source | **PASS (pre-existing repo convention)** | Re-derived. `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` mirrors `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`. The rule's literal `tests/` tree requirement is not how any C# test in this repository is laid out; the file pre-existed this change and was not moved. Not attributable to #824. |

## 2. General Code Change Policy Compliance

Source: `.claude/rules/general-code-change.md` and CLAUDE.md.

| Requirement | Verdict | Evidence and verification basis |
|---|---|---|
| Bugfix Workflow — failing regression test first | **PASS** | Evidence-attested. `evidence/regression-testing/ac2-fail-before.2026-09-09T15-25.md` records `ExpectedExitCode: 1`, `EXIT_CODE: 1`, `TOTAL=13 / PASSED=12 / FAILED=1` with a full per-test outcome list; the fix lands in Phase 2 and `evidence/regression-testing/ac2-ac3-pass-after.2026-09-09T15-38.md` records `TOTAL=15 / PASSED=15 / FAILED=0`. |
| Bugfix Workflow — minimal, targeted fix | **PASS** | Re-derived. Two source files, one production type. No opportunistic refactor. |
| Bugfix Workflow — open a new issue rather than widen scope | **PARTIAL, non-blocking** | Re-derived. Two latent members (`Cache`, `modules`) are recorded as non-goals and documented at `evidence/other/follow-up-latent-statics.2026-09-09T16-21.md`, which states filing is owned by the epic layer. The follow-up issue is **not yet filed**. See finding N-3. |
| Simplicity first | **PASS** | Re-derived. The fix is the simplest construct that provides the CLR's type-initialization guarantee. No `Lazy<T>`, no lock, no double-checked locking, no indirection. |
| Separation of concerns | **PASS** | Re-derived. `ILGlobals` remains a pure lookup table with no I/O. |
| Fail fast and explicitly | **PASS** | Re-derived. The `throw new Exception("Invalid OpCode.")` guard is preserved verbatim at `ILGlobals.cs:159-162`. No `try`/`catch` was added, widened, or removed. |
| No broad catch-all added | **PASS** | Re-derived. Zero `catch` clauses in either changed file. |
| Established logging pattern | **PASS** | Re-derived. `ILGlobals` performs no logging and none was added, which matches the pre-change state. |
| Invariants enforced at initialization | **PASS** | Re-derived. Both tables are assigned only in the static constructor, after the fill completes. `readonly` makes any assignment elsewhere CS0198, a compile error. |
| Naming conventions | **PASS** | Re-derived. `singleTable` / `multiTable` are camelCase locals; the pre-existing field names `singleByteOpCodes` / `multiByteOpCodes` were deliberately preserved for source compatibility. |
| No public API break | **PASS** | Re-derived. Member names, types, and accessibility are unchanged; only `readonly` was added. Adding `readonly` breaks writers only, and a repo-wide `Grep` for `(singleByteOpCodes\|multiByteOpCodes)\s*=` over `*.cs` returns exactly two matches, both inside the static constructor. Zero caller edits were required. |
| No new dependencies | **PASS** | Re-derived. One `using System.Runtime.CompilerServices;` on a BCL namespace. No NuGet package, project reference, or analyzer added. |
| **File size limit — 500 lines** | **PASS** | Re-derived by reading each file to its last line. `ILGlobals.cs` = **228** (272 headroom); `ILGlobals_Tests.cs` = **270** (230 headroom); `MethodBodyReader.cs` = **299**; `MethodBodyReader_Tests.cs` = **489** (11 headroom, unchanged). Both changed files, production and test, are under the cap. |
| Toolchain loop run in order, restarting on any change | **PASS** | Evidence-attested and corroborated. One restart recorded (`FORMAT_CHANGED_TREE=True` on the first P5-T1 pass, `False` on the second). Corroborating figures re-derived — see section 7. |

## 3. Language-Specific Code Change Policy Compliance (C#)

Source: CLAUDE.md §C#1–C#7.

| Requirement | Verdict | Evidence and verification basis |
|---|---|---|
| CSharpier formatting via `dotnet tool run` | **PASS** | Re-derived. `coverage/csharpier-check.log` line 1 reads `Checked 1622 files in 4670ms.` — a single-line clean result. `check` exits non-zero when any file needs formatting, so exit 0 is a real discriminator. |
| `dotnet format` not used | **PASS** | Re-derived. No `dotnet format` invocation appears in any command string in any evidence artifact. |
| Analyzer gate `/t:Rebuild`, not `/t:Build` | **PASS** | Evidence-attested. Command recorded at `evidence/qa-gates/msbuild-analyzers-final.2026-09-09T16-02.md` uses `/t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. |
| Analyzer gate non-vacuous | **PASS** | **Re-derived.** `Grep` over `coverage/msbuild-analyzers-final.log`: `Skipping target "CoreCompile"` = **0**; `^\s*CoreCompile:` = **12**; `: error [A-Z]+[0-9]+:` combined with `: warning [A-Z]+[0-9]+:` = **0**. The gate compiled and produced zero diagnostics. |
| Nullable gate command form, no `/p:Nullable=enable` | **PASS** | Evidence-attested. `evidence/qa-gates/msbuild-nullable-final.2026-09-09T16-03.md` records the CLAUDE.md form character-for-character and states explicitly that `/p:Nullable=enable` was not added and `/t:Build` was not substituted. |
| Nullable gate non-vacuous, zero CS86xx | **PASS** | **Re-derived.** `Grep` over `coverage/msbuild-nullable-final.log`: `Skipping target "CoreCompile"` = **0**; `^\s*CoreCompile:` = **13**; `: error [A-Z]+[0-9]+:` combined with `: warning [A-Z]+[0-9]+:` = **0**. This discharges the CS8618 risk `spec.md:581-583` records as unverified: no CS8618 is raised on either `readonly` field. |
| `#nullable enable` retained | **PASS** | Re-derived. `ILGlobals.cs:1`. |
| Strong contracts, explicit types at boundaries | **PASS** | Re-derived. `OpCode[] singleTable` and `OpCode[] multiTable` are explicitly typed; no `var` was introduced in production code. |
| XML documentation on non-obvious public API | **PASS** | Re-derived. Both fields (`ILGlobals.cs:115-121`, `:124-129`), the static constructor (`:133-138`), and `LoadOpCodes()` (`:171-176`) carry XML doc. The field docs state the residual honesty — `readonly` prevents reassignment of the reference but not element mutation — explicitly. |
| Comment *why*, not *what*; comments synchronized with behavior | **PARTIAL, non-blocking** | Re-derived. See finding N-1: the `LoadOpCodes()` doc comment at `ILGlobals.cs:172-173` says "Retained for the five existing call sites"; at head there are two. |
| No suppression added | **PASS** | Re-derived. `Grep` for `null!` over `ILGlobals.cs` returns **0**; the two pre-existing `!` null-forgiving operators at `:151` are unchanged in intent and are on `info1.GetValue(null)!`, guarded by the `FieldType == typeof(OpCode)` check above. No `#pragma warning disable` and no `SuppressMessage` was added. |
| No analyzer severity, `.editorconfig`, or `.globalconfig` change | **PASS** | Evidence-attested plus footprint. None of these paths appears in the caller-measured footprint or in the executor's 49-path scope listing. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

Source: CLAUDE.md §CUT1–CUT3.

| Requirement | Verdict | Evidence and verification basis |
|---|---|---|
| MSTest framework | **PASS** | Re-derived. `using Microsoft.VisualStudio.TestTools.UnitTesting;` at `ILGlobals_Tests.cs:4`; `[TestClass]` at :9; fourteen `[TestMethod]` attributes. No xUnit or NUnit reference. |
| Moq for mocking | **PASS (not required)** | Re-derived. No mock is needed: the unit under test is a static lookup table with no injectable collaborator. Introducing Moq here would add indirection without isolation benefit. |
| FluentAssertions for assertions | **PASS** | Re-derived. Every assertion in the file uses `.Should()`. No MSTest `Assert.*` API is used. |
| MSTest attribute style | **PASS** | Re-derived. `[TestClass]` / `[TestMethod]` only. |
| Toolchain command selection | **PASS** | See section 3 and section 7. |

## 5. Test Coverage Detail

Coverage artifact used: `coverage/post-change.cobertura.xml` (Cobertura, produced by the Coverage
Command Of Record `scripts/vscode/Invoke-MSTestWithCoverage.ps1`). The canonical
`artifacts/csharp/coverage.xml` path does not exist in this worktree; the raw Cobertura documents for
both the baseline and the post-change run are present on disk and were **read directly by this
reviewer**, which is stronger evidence than a committed extract. The committed evidence of record is
the pair of markdown extracts under `evidence/baseline/` and `evidence/qa-gates/`, whose figures this
reviewer confirmed match the raw documents exactly.

### Repo-wide, per language

- **C#** — repo-wide line coverage **85.6241 %** (56035 / 65443), branch coverage **79.8129 %**
  (13482 / 16892), read directly from `coverage/post-change.cobertura.xml:2`. Line >= 85 % and
  branch >= 75 % thresholds from `.claude/rules/quality-tiers.md` are both met, as is the CLAUDE.md
  80 % figure. Verdict: **PASS**.
- **PowerShell** — N/A, zero changed `.ps1` files on the branch.
- **Python** — N/A, zero changed `.py` files on the branch.
- **TypeScript** — N/A, zero changed `.ts`/`.tsx` files on the branch.

### Modified files (both files existed before this change; zero new files)

| File | Cobertura class | Baseline line-rate | Post line-rate | Baseline branch-rate | Post branch-rate | Verdict |
|---|---|---|---|---|---|---|
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | `SDILReader.ILGlobals` | 0.9459459459459459 | **0.95** | 0.875 | **0.875** | **PASS** |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | n/a — test file, correctly outside the coverage denominator per the exclusion policy's permitted test-file entry | — | — | — | — | **PASS** |

Both figures re-derived by this reviewer:
`coverage/baseline.cobertura.xml:44010` and `coverage/post-change.cobertura.xml:44010`.
`SDILReader.MethodBodyReader` is byte-identical on both metrics across the two documents
(`0.9732620320855615` / `0.8947368421052632`, complexity 38, at baseline line 44420 and post line
44426), which independently corroborates that `MethodBodyReader.cs` was not modified.

Both files clear line >= 85 % and branch >= 75 %. Coverage improved rather than regressed.

### Changed-line coverage — independently re-derived

The executor reported changed-code coverage of 9 / 9 = 100 %. This reviewer confirmed the underlying
premise directly from the Cobertura document rather than accepting the computation:

- Within the `SDILReader.ILGlobals` class element, the **only** instrumented lines with `hits="0"`
  are **160** and **161** (`coverage/post-change.cobertura.xml:44049-44050`). Those are the `{` and
  the `throw new Exception("Invalid OpCode.");` of the invalid-opcode guard at `ILGlobals.cs:160-161`.
- Neither 160 nor 161 is in the executor's added-line set, and both were uncovered at baseline
  (recorded there as lines 142, 143 before the edit shifted them). The residual is the same
  unreachable path on both sides.
- Every instrumented line in the intersection of the added-line set with the class's instrumented
  lines — 141, 142, 155, 163, 167, 168, 169, 178, 179 — carries `hits="1"`
  (`coverage/post-change.cobertura.xml:44051-44057, 44062-44064, 44085-44086, 44109`). That is
  exactly 9 lines, all covered. **9 / 9 = 100 % confirmed.**
- Line 159 carries `condition-coverage="50% (1/2)"`, the untaken false branch of
  `(num2 & 0xff00) != 0xfe00`. This is the sole source of the 0.875 class branch-rate and is
  identical at baseline. No branch regression.

**Coverage verdict: PASS. No coverage remediation trigger.**

## 6. Test Execution Metrics

All four figures below were **re-derived** by this reviewer from `coverage/coverage-post-change.log`
rather than taken from the committed extract.

| Metric | Value | Source |
|---|---|---|
| Total tests | **7212** | `coverage/coverage-post-change.log:7234` |
| Passed | **7212** | `coverage/coverage-post-change.log:7235` |
| Failed | **0** | no line matching `^\s*Failed:`; `Test Run Successful.` at :7233 |
| Skipped | **0** | no line matching `^\s*Skipped:` |
| First-party coverage banner | `lines 56035/65443 (85.62%), branches 13482/16892 (79.81%)` | `coverage/coverage-post-change.log:7239` |

Suite growth: baseline 7210 → post-change 7212, a net +2. This decomposes exactly as four tests added
(`LoadOpCodes_DoesNotRepublishPublishedTables`, `SingleByteOpCodes_FieldIsInitOnly`,
`MultiByteOpCodes_FieldIsInitOnly`, `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes`) minus two
deleted (`LoadOpCodes_PopulatesKnownSingleByteOpCodes`, `LoadOpCodes_PopulatesKnownOpCode_Ret`), with
the two renames net-zero. Re-derived by counting `[TestMethod]` in `ILGlobals_Tests.cs`: **14**,
against a recorded baseline class size of 12.

Scoped-class discovery-count control: `TOTAL=14 / PASSED=14 / FAILED=0`, with all six criterion-named
tests present in the run's test list
(`evidence/qa-gates/ac11-named-tests.2026-09-09T16-09.md`). Pairing the name list with the total is
what distinguishes "the test passed" from "the test never ran"; both halves are present.

## 7. Code Quality Checks

| Gate | Command form | Result | Verification basis |
|---|---|---|---|
| 1. Format | `dotnet tool run csharpier format .` | exit 0; second pass `FORMAT_CHANGED_TREE=False` | Evidence-attested |
| 2. Format verify | `dotnet tool run csharpier check .` | exit 0; `Checked 1622 files in 4670ms.` | **Re-derived** from `coverage/csharpier-check.log:1` |
| 3. Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0; CoreCompile 12, skipped 0, errors 0, warnings 0 | **Re-derived** from `coverage/msbuild-analyzers-final.log` |
| 4. Nullable | `msbuild TaskMaster.sln /t:Rebuild /m ... /p:TreatWarningsAsErrors=true` | exit 0; CoreCompile 13, skipped 0, errors 0, CS86xx 0 | **Re-derived** from `coverage/msbuild-nullable-final.log` |
| 5. Coverage-enabled tests | Coverage Command Of Record | exit 0; 7212 / 7212 / 0 | **Re-derived** from `coverage/coverage-post-change.log` |

Loop discipline: one restart, triggered by CSharpier reflowing this feature's own hand-written line
wrapping in both source files on the first P5-T1 pass. The loop correctly restarted at step 1 rather
than continuing. The final pass completed all five steps with no file modified by the formatter.

The differing `CoreCompile:` counts between the analyzer gate (12) and the nullable gate (13) on the
same tree were re-derived and are consistent with the artifacts' recorded explanation. Both are
greater than zero, which is what non-vacuity requires of each; the count is a property of what MSBuild
scheduled on a given invocation, not a fixed property of the tree.

**No policy document, coverage threshold, analyzer severity, or exclusion list was lowered, weakened,
or deleted.** Re-derived: none of `.claude/rules/**`, `.editorconfig`, `coverage.config`,
`scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, `CLAUDE.md`, or any `.csproj` appears in
the change footprint, and `Grep` confirms zero `ExcludeFromCodeCoverage` attributes in the changed
production file.

## 8. Gaps and Exceptions

Four non-blocking findings. None blocks merge.

### N-1 — `LoadOpCodes()` doc comment overstates its remaining call-site count (Low)

- **Location:** `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:172-173`
- **Text:** "Forces the opcode tables to be published, if they have not been already. **Retained for
  the five existing call sites**; the tables themselves are built by the static constructor..."
- **Rule:** CLAUDE.md §C#6.3 and `.claude/rules/general-code-change.md` — "Keep comments
  synchronized with behavior."
- **Verification (re-derived):** a repository-wide `Grep` for `LoadOpCodes` over `*.cs` at head
  returns exactly **two** invocations of `ILGlobals.LoadOpCodes()`:
  `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs:50` and
  `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs:364`. Zero production
  invocations.
- **Cause:** the count of five was correct for the pre-change tree (`ILGlobals_Tests.cs` lines 15, 26,
  37, 47 plus `MethodBodyReader_Tests.cs:364`). This feature's own AC7 rework removed three of those
  four, so the comment describing the post-change tree became stale within the same change.
- **Impact:** documentation accuracy only. No behavioural effect, and no acceptance criterion is
  affected — AC5 constrains the method declaration and body, not the comment text. The practical
  cost is that the comment is the stated justification for retaining the method, and it now overstates
  the dependency by a factor of 2.5; a future reader evaluating whether `LoadOpCodes()` can be deleted
  would be misled about the cost.
- **Recommendation:** correct "five" to "two" in the follow-up rather than in this branch, since
  reopening the file requires a full toolchain loop restart for a comment.

### N-2 — Plan D5 anchors on `origin/main`, which is the wrong base for an epic child (Medium, process)

- **Location:** `plan.2026-09-08T23-51.md:176` ([P0-T15], the one unchecked task);
  `evidence/baseline/base-inertness.2026-09-09T15-19.md`;
  `evidence/other/executor-deviations.2026-09-09T15-28.md` §D-1.
- **Observation:** the plan's Decision D5 anchors every diff on `git merge-base HEAD origin/main`.
  For this epic child that resolves to `6f08302a`, which predates five merged siblings, so the
  merge-base-anchored listing attributes 277 inherited paths to this feature. P0-T15's acceptance is
  genuinely not met and is correctly left unchecked.
- **Assessment of the executor's decision to proceed rather than halt:** **correct.** Three
  independent reasons support it, and this reviewer verified each is factually available in the
  record rather than asserted: (a) the condition is environmental — the worktree was fast-forwarded
  to the integration tip after the plan cleared preflight — not a defect in the change; (b) the
  authorising delegation, quoted in `base-inertness.md:85-89`, records this exact base state as
  verified and expected and names the integration tip `553f874a` explicitly, so halting would end the
  run on a state the authorising layer had already accounted for; (c) the property every affected
  gate exists to establish remained fully verifiable against a different anchor. Halting here would
  have been the wrong call.
- **Assessment of the adaptation chosen:** sound but strictly weaker than an alternative that was
  available. The executor substituted a symbolic `HEAD` anchor to avoid pinning a SHA. This is valid
  **only while the run has made no commit** — a `HEAD`-anchored diff stops showing work the moment it
  is committed. This reviewer checked the timeline and the constraint holds: every HEAD-anchored gate
  ran between 15:46 and 16:15, and the first commit was made at approximately 16:19, so no gate was
  evaluated against a stale anchor. The post-commit gap is closed separately by the pre-commit and
  post-commit porcelain observations in `evidence/qa-gates/closure.2026-09-09T16-25.md`. The stronger
  alternative — anchoring on the integration tip `553f874a`, which the delegation had already named —
  would have satisfied the plan's intent exactly and would also have survived the commits.
- **Is any acceptance criterion left genuinely unverified by this?** **No.** The four criteria that
  depend on an anchored diff (AC5, AC8, AC9, AC10) each carry an independent secondary observation
  that does not depend on the anchor: unchanged line counts of 489 and 299 re-derived by this
  reviewer from the files themselves, byte-identical Cobertura class rates for
  `SDILReader.MethodBodyReader` across both documents, `Parallelize` intact at `AssemblyInfo.cs:18-21`
  read directly, and the four inherited `<Compile Include>` insertions named and visibly attributable
  to a sibling's `FolderPredictorTests` file set. The caller additionally re-measured the footprint
  against the true base `553f874a` and confirms it equals the Owned Write Set.
- **Recommendation:** fix upstream in the planner. An epic-child plan must resolve its base from the
  epic integration branch, not `origin/main`.

### N-3 — Follow-up issue for the two latent statics is recorded but not filed (Medium, owed)

- **Location:** `evidence/other/follow-up-latent-statics.2026-09-09T16-21.md`; `spec.md:125-135` and
  `:627-630`.
- **Observation:** `ILGlobals.Cache` (`ILGlobals.cs:113`) and `ILGlobals.modules` (`ILGlobals.cs:131`)
  remain unsynchronised public mutable statics. Both are correctly **recorded rather than silently
  dropped**: the artifact names each member, re-derives its current line number, states the shift and
  its cause, and enumerates every reference. `Cache` has exactly one read site, which this reviewer
  confirmed directly at `ILGlobals_Tests.cs:267`; `modules` has zero references repo-wide.
- **Assessment of the scoping decision:** **agreed.** Widening #824 to remediate them would change the
  public surface of a type during an epic fan-in for members that are dormant (`Cache`) or entirely
  unreferenced (`modules`), against the CLAUDE.md Bugfix Workflow rule to open a new issue rather than
  widen scope.
- **Owed item:** the artifact states that filing is owned by the epic layer and is not a plan task. It
  is not yet filed. Evidence prose inside a feature folder does not survive feature-folder archival,
  so this must be promoted into a real GitHub issue before #824 is closed. Same for the `spec.md`
  rollout commitment to report the outcome to issue #811 so its AC4 can be re-evaluated.

### N-4 — Two literal evidence-location clauses in spec.md are satisfied by substitution (Low)

- **Location:** `spec.md:562-564` (AC11: "Logs for the final pass are written under the feature's
  evidence directory") and `spec.md:570-571` (AC12: "no coverage document is written to any other
  location").
- **Observation:** raw msbuild, vstest, and CSharpier logs and both raw Cobertura documents were
  written to the gitignored `coverage/` directory; compact markdown extracts were committed under
  `evidence/qa-gates/` and `evidence/baseline/`. Reconciliation is documented at
  `evidence/other/ac-checkoff-notes.2026-09-09T16-19.md:57-98` with two verifiable reasons:
  `.gitignore:84` is `*.log`, so a raw log under the evidence directory would be untracked and
  uncommittable; and a full-solution Cobertura document is on the order of 10 MB and carries absolute
  host paths.
- **Assessment:** the deviation is documented, justified, and preferable to the literal reading, which
  would have produced the appearance of compliance with nothing in the repository. The criteria's
  purpose — durable evidence in the canonical location — is met. Not a failure.
- **Residual worth stating:** because the raw logs and Cobertura documents are gitignored, the
  independent re-derivation this reviewer performed from them will not be repeatable by a future
  auditor working from the merged history alone. The committed extracts are the only durable record.
  This is an accepted consequence of the size and host-path constraints, not a defect.

### Recorded but not counted as findings

- **`readonly` on an array field prevents reassignment, not element mutation.** Recorded honestly at
  `spec.md:283-287` and, more usefully, in the source itself at `ILGlobals.cs:118-120` and `:127-128`,
  where callers will actually read it. Verified present. Correct treatment.
- **Clearing `beforefieldinit`.** Adding an explicit static constructor clears the type flag, so the
  JIT must honour an initialization check on static accesses it could otherwise elide. Recorded at
  `spec.md:305-308` as an accepted, unmeasured cost with no performance budget asserted, and again as
  risk 2 at `:584-587`. This reviewer's assessment: the cost is real but negligible for a table read
  once per decoded IL instruction, and the flag clearance is not incidental — it is what makes
  `LoadOpCodes()`'s `RunClassConstructor` call redundant for correctness rather than load-bearing,
  since any static member access now triggers the serialised initializer. Correctly recorded.
- **One host token in a committed artifact.** `evidence/baseline/base-inertness.2026-09-09T15-19.md:195`
  reproduces the path `.../DanMoisan_MEGALODON4_2026-09-09_10_10_13_net481.trx` inside the verbatim
  inherited listing. It carries an account name and a machine name. Mitigating: it is a **sibling's**
  already-committed path being reproduced in a listing the task required verbatim, so the token is
  already in the repository independently of this artifact, and this feature's own command strings are
  correctly sanitised to `<worktree-root>` throughout. Noted for awareness; not a finding against
  #824.

## 9. Summary of Changes

| Path | Status | Lines (base → head) | Notes |
|---|---|---|---|
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | Modified | 197 → **228** | Fields become `public static readonly` with no initializer; explicit static constructor builds both tables in locals and publishes each once after the loop; `LoadOpCodes()` reduced to `RunClassConstructor`; `using System.Runtime.CompilerServices;` added; stale three-line comment replaced by four XML doc blocks. |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | Modified | 133 → **270** | Four tests added, two renamed with their `LoadOpCodes()` Act removed, two spot checks deleted. 12 → 14 `[TestMethod]`. |
| `docs/features/.../spec.md` | Modified | — | Twelve `- [ ]` → `- [x]` AC check-offs only; `12 insertions(+), 12 deletions(-)`. No criterion text edited, reworded, renumbered, or reordered. |
| `docs/features/.../plan.2026-09-08T23-51.md` | Modified | — | Task checkboxes; 68 of 69 checked, [P0-T15] deliberately unchecked. |
| `docs/features/.../evidence/**` | Added | 51 files | All under canonical evidence kinds. |

Not modified, and verified not modified: `MethodBodyReader.cs`, `MethodBodyReader_Tests.cs`,
`AssemblyInfo.cs`, `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`, `.editorconfig`,
`coverage.config`, every file under `.claude/rules/`, `CLAUDE.md`, and every `.github/workflows/`
path.

## 10. Compliance Verdict

**PASS. Blocking findings: 0.**

The fix is correct, minimal, and well-matched to the defect. The publication invariant is established
by the construct that actually provides it — the CLR's type-initialization guarantee — rather than by
a lock that would narrow the window without closing it, and the `readonly` modifier converts
reintroduction of the defect from a silent runtime race into a CS0198 compile error. Both halves are
required and both are present.

The test strategy is the strongest available under the repository's determinism ban: the gates assert
the publication property directly and fail deterministically on the unfixed tree, rather than sampling
an intermittent race. The AC4 exhaustive test is correctly and repeatedly labelled a supporting test
in the source, in the check-off notes, and in both run artifacts; **no artifact presents it as
evidence that the race is fixed**, which was checked specifically.

The four non-blocking findings are a stale doc-comment count (N-1), a planner base-resolution defect
that belongs upstream (N-2), an owed follow-up issue filing (N-3), and two documented and justified
evidence-location substitutions (N-4). None requires remediation on this branch.

No `remediation-inputs` artifact is produced.

---

## Appendix A: Test Inventory

`UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` — 14 `[TestMethod]`, all passing.

| # | Test | Line | Role | Deterministic seam |
|---|---|---|---|---|
| 1 | `SingleByteOpCodes_IsPublishedWithFullLength` | 17 | AC7 renamed publication test | field read + `Length` |
| 2 | `MultiByteOpCodes_IsPublishedWithFullLength` | 29 | AC7 renamed publication test | field read + `Length` |
| 3 | `LoadOpCodes_DoesNotRepublishPublishedTables` | 43 | **AC2 primary gate** | reference identity, `BeSameAs` |
| 4 | `SingleByteOpCodes_FieldIsInitOnly` | 77 | **AC3 structural gate** | `FieldInfo.IsInitOnly` reflection |
| 5 | `MultiByteOpCodes_FieldIsInitOnly` | 101 | **AC3 structural gate** | `FieldInfo.IsInitOnly` reflection |
| 6 | `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` | 127 | AC4 supporting test (not a gate) | reflection over `typeof(OpCodes)` |
| 7–13 | `ProcessSpecialTypes_*` (7 tests) | 191–261 | pre-existing | pure string mapping |
| 14 | `Cache_IsInitialized` | 264 | pre-existing | field read |

Deleted by this change: `LoadOpCodes_PopulatesKnownSingleByteOpCodes`,
`LoadOpCodes_PopulatesKnownOpCode_Ret` (both subsumed by test 6, which asserts strictly more).

`UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` — unchanged at 489 lines;
its `ILGlobals.LoadOpCodes()` call at :364 inside `CreateReader` compiles unchanged against the now
`readonly` fields.

## Appendix B: Toolchain Commands Reference

Run in this exact order; any failure or formatter auto-fix restarts at step 1.

1. `dotnet tool run csharpier format .`
2. `dotnet tool run csharpier check .`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
5. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` — realised in this run as the
   Coverage Command Of Record, `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/post-change.cobertura.xml`

Non-vacuity assertions applied to steps 3 and 4: zero occurrences of `Skipping target "CoreCompile"`
paired with a positive count of `^\s*CoreCompile:`. An exit code alone cannot distinguish a clean
compile from a skipped one.
