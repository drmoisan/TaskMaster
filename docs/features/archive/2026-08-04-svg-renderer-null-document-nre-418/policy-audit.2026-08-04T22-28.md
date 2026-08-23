# Policy Audit — svg-renderer-null-document-nre (Issue #418)

- Audit timestamp: 2026-08-04T22-28
- Reviewer: feature-review agent
- Cycle: 2 (re-audit after remediation cycle 1)
- Work mode: `minor-audit` (marker `- Work Mode: minor-audit` at `issue.md:12`)
- Acceptance-criteria source: `issue.md` section `## Acceptance Criteria` (AC-1 .. AC-11)

## Baseline Resolution

| Item | Value |
|---|---|
| Base branch (requested) | `main` |
| Base ref (resolved) | `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Head ref | `bug/svg-renderer-null-document-nre-418` @ `a62391f719c6d5ecc3d80115916c95d1966ca514` |
| Merge-base SHA | `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Diff range | `ce0c91e6...a62391f7` (three-dot, merge-base) |
| Prior cycle head | `ea106111a6daf7e05f8a804ac00b4a713598962a` |
| Working tree | clean (`git status --porcelain` empty at audit start) |

Merge-base independently recomputed with `git merge-base HEAD origin/main`; the recomputed value
equals the caller-supplied value, so the supplied base was not stale.

The PR-context summary records `Head ref (resolved): ... a62391f7`, which matches
`git rev-parse HEAD`. The artifacts are current for this head, not stale.

## Executive Summary

Verdict: **PARTIAL**. Blocking findings: **2**.

Remediation cycle 1 is high quality. All seven actionable findings from cycle 1's code review
(CR-1 through CR-7) are verified resolved by direct measurement, and cycle 1's two non-AC-11 policy
gaps are materially improved. Repository-wide C# coverage rose above both mandatory floors for the
first time in this feature's history.

Two findings block PR readiness:

1. **G-2 (carried forward, unchanged): AC-11 is undelivered.** The human WinForms-designer runbook
   has not been executed. This is correctly tracked as ratified human-interaction requirements H-1
   and H-2 with `response: exception` and a runbook path in
   `artifacts/orchestration/orchestrator-state.json`. No agent can discharge it.
2. **G-8 (new this cycle, missed in cycle 1): six tests in `SVGControl.Test` produce different
   outcomes depending on the ordinal position of the assembly on the `vstest.console.exe` command
   line.** `ExCSS.dll` is absent from `SVGControl.Test/bin/Debug`, so a successful SVG parse
   succeeds only when a sibling test assembly's output directory supplies ExCSS to the test host.
   This violates the Independence principle in `.claude/rules/general-unit-test.md` and the
   IDE/CLI-parity requirement in `.claude/rules/csharp.md`. The fix is one `<Reference>` item plus
   one `packages.config` line, mirroring the `Svg` reference this branch already added. This
   defect was present at cycle 1's head `ea106111` and the reviewer did not catch it then; it is
   newly surfaced, not newly introduced.

Coverage: repository-wide C# line coverage is 85.4097% and branch coverage is 78.7220%, both above
the mandatory floors. Two file-level floors are not met and are recorded as FAIL under G-1 and G-9;
both residuals are dominated by code this feature did not author, and both are dispositioned
non-blocking with reasons stated.

## Rejected Scope Narrowing

The caller prompt supplied four factual notes about the inputs and stated explicitly: "None
constrains your scope or your findings." The prompt further instructed: "Determine scope yourself
from the branch diff per the SKILL contract; do not narrow it to the remediation delta."

No attempted scope narrowing was detected. The caller actively directed the reviewer to the full
feature-vs-base scope, which is what this audit performs. Nothing is recorded verbatim here because
there is nothing to reject.

For the avoidance of doubt, the audit scope is the complete branch diff against
`ce0c91e686bf7e060aaab6f185ee6883269e4fd4`: 83 changed files, comprising 6 C# source files, 5 C#
project and binding-configuration files, and 72 documentation and agent-memory files. Every
language with changed files receives an explicit PASS or FAIL coverage verdict in section 1.2.

## Evidence Location Compliance

Scan of the branch diff for files written under non-canonical evidence roots:

```
git diff --name-only ce0c91e6...a62391f7 | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
-> no matches
```

**PASS.** All 47 evidence artifacts are written under
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/<kind>/`, using the
canonical kinds `baseline/`, `qa-gates/`, `regression-testing/`, `remediation-baseline/`,
`issue-updates/`, and `other/`. Zero occurrences under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/`.

`scripts/dev_tools/validate_evidence_locations.py` does not exist in this repository, so the scan
above is the enforcement mechanism used. Recorded as an assumption, not a gap.

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events occurred: no delegation instruction specified a
non-canonical evidence path.

## Change Inventory (feature-vs-base)

| Category | Files | Detail |
|---|---|---|
| C# production source | 3 | `SVGControl/SvgRenderer.cs` (+115/-107, modified), `SVGControl/SvgAssemblyResolver.cs` (+157, new), `SVGControl/SvgAssemblyProbe.cs` (+93, new) |
| C# test source | 3 | `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs` (+347, new), `SvgRendererParseContractTests.cs` (+358, new), `SvgRendererNullToleranceTests.cs` (+144, new) |
| C# project / config | 5 | `SVGControl/SVGControl.csproj`, `SVGControl.Test/SVGControl.Test.csproj`, `SVGControl.Test/app.config`, `SVGControl.Test/packages.config`, `TaskMaster.sln` |
| Feature documentation | 44 | `issue.md`, `plan.2026-08-04T14-36.md`, `remediation-plan.2026-08-05T01-50.md`, cycle-1 audit artifacts, research, runbook, HANDOFF, 34 evidence artifacts |
| Agent memory | 25 | `.claude/agent-memory/{atomic-executor,atomic-planner,feature-review,human-exception-runbook,task-researcher}/` |
| Deferred follow-ups | 3 | `docs/features/potential/` entries |

Changed-language set: **C# only.** Zero `.ts`/`.tsx`, zero `.py`, zero `.ps1`/`.psm1` files in the
branch diff. TypeScript, Python, and PowerShell therefore have zero changed files on this branch.

## PR-Context Artifact Corrections

The collector produced two false statements that the reviewer corrected in place.

1. **C# misclassified as documentation.** `artifacts/pr_context.summary.txt` reported
   `Core logic changes: 0 files` and `Docs/templates/agents/tooling: 72 files`, classifying all 11
   changed C# source and project files as documentation. This is the same collector defect recorded
   for issues #171, #181, #244, #251, #253, #270, #278, #283, #208, #292, #328 and #354. The
   consequence is material rather than cosmetic: `.claude/hooks/validate-feature-review-coverage.ps1`
   derives its changed-language set from those overview bullets via `Get-ChangedLanguageSet`, so the
   misclassification would have caused the coverage gate to skip C# enforcement silently. The
   reviewer appended a labelled correction with the full C# enumeration in the hook's expected
   `- <path> (+N/-N)` bullet form so C# is enumerated. Corrected in place; the original collector
   text is preserved above the correction.
2. **`gh` reported unavailable.** The summary states `GitHub CLI unavailable: GitHub CLI (gh) is not
   installed.` This is a false negative from the collector's own PATH resolution. `gh` version
   2.87.3 is installed and resolves in this session. Consequences: the "Issues to autoclose" and
   "CI status (HEAD)" sections are unpopulated, and the "Auto-close issues (author asserted)" list
   is polluted with fifteen non-issue tokens parsed out of prose (`#AC-1` .. `#AC-11`, `#CR-2`,
   `#DE06-4337` — the last being a fragment of the `SVGControl.Test` project GUID
   `{13AC39E6-DE06-4337-8EB0-41CE674A4C3B}` added to `TaskMaster.sln`). None of these are GitHub
   issues. The canonical autoclose issue for this feature is **#418** and no other.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| UT1 Independence | **FAIL** | Six tests change outcome with `vstest.console.exe` argument order. See G-8. |
| UT1 Isolation | PASS | Each test targets one member. 38 `[TestMethod]` across three files. |
| UT1 Fast execution | PASS | Full `SVGControl.Test` assembly executes in 1.16 s (reviewer-measured). |
| UT1 Determinism | **FAIL** | Same binary, same host, two argument orders, two different outcomes. See G-8. |
| UT1 Readability | PASS | Descriptive names; Arrange-Act-Assert sections present with comments. |
| UT2 Scenario completeness | PASS | Positive, negative, boundary (`ArgumentNullException`), and error-path cases all present. |
| UT3 Arrange-Act-Assert | PASS | Verified by inspection of all three new test files. |
| UT4 No external dependencies | PASS | No network, database, or external process. Parse boundary is mocked through an injected `Func<byte[], SvgDocument?>` seam. |
| UT4 No temporary files | PASS | Zero occurrences of `GetTempPath`, `GetTempFileName`, `File.WriteAllText`, `File.Create`, `Directory.CreateDirectory` in the three new test files. |
| UT4 No mutable global state | **FAIL** | The tests depend on the test host's assembly-probing path, which is external configuration that changes between runs. See G-8. |
| Banned APIs in test code | PASS | Zero occurrences of `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Random`. |
| Test file location (mirrored `tests/`) | PARTIAL | Tests live in `SVGControl.Test/` alongside 8 sibling `*.Test` projects. Repository-wide pre-existing convention. See G-4. |

### 1.2 Coverage Verification

Coverage was verified by inspecting the pre-existing artifacts produced during execution. Coverage
generation was **not** re-run, per the SKILL contract.

| Artifact | Present | Notes |
|---|---|---|
| `artifacts/csharp/coverage.xml` | yes | JaCoCo, converted from `coverage/coverage.cobertura.xml`; one `LINE` and one `BRANCH` counter |
| `coverage/coverage.cobertura.xml` | yes | Cobertura source, `timestamp="1785895464"`, generated at this head |
| `coverage/lcov.info` (TypeScript) | not applicable | zero `.ts`/`.tsx` files changed on this branch |
| `artifacts/python/lcov.info` (Python) | not applicable | zero `.py` files changed on this branch |
| `artifacts/pester/powershell-coverage.xml` (PowerShell) | not applicable | zero `.ps1`/`.psm1` files changed on this branch |

The JaCoCo conversion was independently validated against the Cobertura root element. The root
declares `lines-covered="93539" lines-valid="109518" branches-covered="21584"
branches-valid="27418"`, and the converted JaCoCo declares
`<counter type="LINE" missed="15979" covered="93539"/>` and
`<counter type="BRANCH" missed="5834" covered="21584"/>`. 93539 + 15979 = 109518 and
21584 + 5834 = 27418, so the conversion is arithmetically faithful and carries exactly one counter
per type, which is what the hook's summing parser requires.

#### 1.2.1 Per-language coverage rows

- **C# (`SVGControl`, `SVGControl.Test`) — coverage verdict: FAIL; repo-wide line coverage 85.4097% PASS and branch coverage 78.7220% PASS, with two file-level floors not met.** Baseline: line 84.9% region,
  `SVGControl.SvgRenderer` class 264/422 = 62.559%. Post-change: repository-wide line
  **85.4097%** (93539/109518) and branch **78.7220%** (21584/27418); both clear the mandatory
  floors of 85% line and 75% branch. Change: repository-wide line and branch coverage both improved
  relative to cycle 1. New/changed-code coverage: **61.6279%** on the new file
  `SVGControl/SvgAssemblyResolver.cs`, which is the lowest measured value among changed files and
  is the figure this row reports. Disposition: FAIL on two file-level floors — the new file
  `SVGControl/SvgAssemblyResolver.cs` at 61.6279% line / 53.8462% branch (see G-9) and the modified
  file `SVGControl/SvgRenderer.cs` at 80.1932% line (see G-1). Both are dispositioned non-blocking
  with reasons recorded in section 8; the repository-wide gate passes and no changed line regressed.
  Evidence: reviewer re-parse of `coverage/coverage.cobertura.xml` per `<line>` descendant, plus
  `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md`.
- **TypeScript — verdict: not required.** Zero `.ts`/`.tsx` files in the branch diff, so no
  TypeScript coverage obligation attaches to this branch.
- **Python — verdict: not required.** Zero `.py` files in the branch diff, so no Python coverage
  obligation attaches to this branch.
- **PowerShell — verdict: not required.** Zero `.ps1`/`.psm1` files in the branch diff, so no
  Pester coverage obligation attaches to this branch.

#### 1.2.2 File-level coverage against the uniform tier rule

Thresholds per `.claude/rules/quality-tiers.md` Authoritative Decision #2: line >= 85%, branch
>= 75%, uniform across T1-T4. New files additionally carry the >= 90% new-module line threshold from
`.claude/rules/csharp.md`.

| File | Status | Line | Branch | Floor met |
|---|---|---|---|---|
| `SVGControl/SvgAssemblyProbe.cs` | new | 102/102 = 100.0000% | 92/92 = 100.0000% | yes |
| `SVGControl/SvgAssemblyResolver.cs` | new | 106/172 = 61.6279% | 28/52 = 53.8462% | **no** (G-9) |
| `SVGControl/SvgRenderer.cs` | modified | 332/414 = 80.1932% | 64/84 = 76.1905% | line **no** (G-1); branch yes |

Repository-wide: line 93539/109518 = 85.4097% (floor 85%, met); branch 21584/27418 = 78.7220%
(floor 75%, met).

#### 1.2.3 No regression on changed lines

Verified by member-level measurement rather than by assertion. Every member this feature added or
modified in `SVGControl/SvgRenderer.cs` measures 100% line coverage:

| Member | Line coverage |
|---|---|
| `.cctor()` | 6/6 = 100% |
| `.ctor(byte[], Size, AutoSize)` | 17/17 = 100% |
| `.ctor(byte[], Size, Padding, AutoSize)` | 18/18 = 100% |
| `DescribeFailure(Exception)` | 5/5 = 100% |
| `OpenFromBytes(byte[])` | 5/5 = 100% |
| `TryGetSvgDocument(byte[], Func<>, out, out)` | 23/23 = 100% |
| `TryGetSvgDocument(byte[], out, out)` | 3/3 = 100% |
| `GetSvgDocumentOrThrow(byte[])` | 6/6 = 100% |
| `GetSvgDocument(byte[])` | 4/4 = 100% |

The entire 82-line residual in `SvgRenderer.cs` sits in members this feature did not touch:
`.ctor(SvgDocument, Size, AutoSize)` 0/8, `.ctor(SvgDocument, Size, Padding, AutoSize)` 0/8,
`get_Margin()` 0/1, `Render()` 18/26, `AddMargins(int, int)` 0/15,
`AdjustSizeProportionately(Size, Size)` 22/23. Had those pre-existing members been covered, the file
would measure 372/414 = 89.86%. **No changed line regressed.** PASS.

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | Two small single-purpose types extracted; no new abstraction layers. |
| Reusability | PASS | Probe and token-comparison logic factored into a reusable pure helper. One residual duplication noted as Low in the code review. |
| Extensibility | PASS | `TryGetSvgDocument` seam parameter permits test injection without altering the public shape. |
| Separation of concerns | PASS | Assembly-binding concern separated from SVG rendering into `SvgAssemblyResolver.cs`; pure path logic into `SvgAssemblyProbe.cs`. |
| Fail fast, no silent swallow | PASS | Zero bare `catch` blocks in changed files. All four catch sites declare `Exception ex` and log. See section 3. |
| Logging pattern | PASS | Existing `log4net` logger used for the parse boundary; `Trace` used inside the `AssemblyResolve` handler with a documented re-entrancy rationale. |
| File size limit (500 lines) | PASS | `SvgRenderer.cs` 362 (was 497 at cycle 1, 354 at baseline); `SvgAssemblyResolver.cs` 157; `SvgAssemblyProbe.cs` 93; test files 347, 358, 144. Counted with `awk 'END{print NR}'`. |
| Toolchain loop | PARTIAL | Format, analyzer, and test stages verified clean. The mandated solution-wide nullable gate is non-probative as recorded. See G-3. |

### 2.1 modified-workflow-needs-green-run

**Rule does not fire.** The branch diff contains zero paths matching `.github/workflows/**`,
`.github/actions/**`, or `scripts/benchmarks/**`, verified by
`git diff --name-only ce0c91e6...a62391f7 | grep -E "^(\.github/workflows/|\.github/actions/|scripts/benchmarks/)"`
returning no matches. No green-run evidence is required.

`scripts/feature-review/Test-ModifiedWorkflowNeedsGreenRun.ps1` does not exist in this repository;
the trigger-path determination above is the enforcement mechanism used.

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | **PASS** | Reviewer ran `dotnet tool run csharpier check .`: `Checked 1467 files in 3965ms`, exit 0. |
| .NET analyzer build | **PASS** | Reviewer ran the mandated analyzer command: 0 errors, 6 warnings, exit 0. All 6 warnings are pre-existing (`System.Reactive` packages.config advisory x5, `CS2002` duplicate source in `UtilitiesCS.Test` x1); none in changed files. |
| Nullable / type check | **PARTIAL** | See G-3. The isolated `SVGControl`/`SVGControl.Test` compile is clean; the solution-wide form fails on pre-existing out-of-scope diagnostics once anything genuinely recompiles. |
| Nullable reference types enabled | PASS | `#nullable enable` at line 1 of all three production files and all three test files. |
| Naming conventions | PASS | `PascalCase` types and members, `camelCase` locals, `_camelCase` private fields. |
| Minimal public surface | PASS | `SvgAssemblyResolver` and `SvgAssemblyProbe` are `internal static`. `SvgRenderer` is `internal class`, so its `public static` members are assembly-internal, reachable from `SVGControl.Test` via `InternalsVisibleTo`. Documented in AC-4. |
| XML docs on non-obvious contracts | PASS | `TryGetSvgDocument`, `GetSvgDocumentOrThrow`, `GetSvgDocument`, and both new types carry XML or block documentation stating contract and rationale. |
| No broad catch without context | PASS | Four catch sites, all adding context. Detail below. |
| Exception containment in `AssemblyResolve` | **PASS (resolved this cycle)** | Cycle-1 finding CR-2 is fixed. Outer containment `catch (Exception ex)` at `SvgAssemblyResolver.cs:143`; `baseDirectory` now filtered through `Path.GetInvalidPathChars()` at `SvgAssemblyProbe.cs:52-54`. |

Catch-site inventory across changed files (four sites, zero bare):

| Location | Channel | Purpose |
|---|---|---|
| `SvgRenderer.cs:302` | `logger.Error` + `Trace.TraceError` | parse-failure boundary in `TryGetSvgDocument`; returns `false` with the exception in `out error` |
| `SvgAssemblyResolver.cs:100` | `Trace.TraceWarning` | strategy-2 `Assembly.Load` failure |
| `SvgAssemblyResolver.cs:132` | `Trace.TraceWarning` | strategy-3 `Assembly.LoadFrom` failure |
| `SvgAssemblyResolver.cs:143` | `Trace.TraceWarning` | outer containment boundary (added this cycle) |

`log4net` is deliberately not used at the three resolver sites. The in-code comment at
`SvgAssemblyResolver.cs:98-99` states the reason: a `log4net` call inside an `AssemblyResolve`
handler can itself trigger a re-entrant assembly load. This is a sound and correctly documented
deviation from the standard logging pattern, not a policy violation.

A known residual is disclosed by the executor and confirmed by the reviewer: the pre-guard region of
`ResolveByNameAndKey` (`new AssemblyName(args.Name)` at line 50 and `loaded.GetName()` at line 54)
executes before the outer `try` begins and is therefore outside the containment catch. Both calls
operate on CLR-supplied values inside a CLR-invoked callback. Accepted, with the rationale recorded
in the remediation plan's Design Decision 11.

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `using Microsoft.VisualStudio.TestTools.UnitTesting;` in all three files; `[TestClass]`/`[TestMethod]` throughout. |
| Moq for mocking | PASS | `using Moq;` in `SvgRendererParseContractTests.cs`; used to drive the null-returning parse branch through the injected delegate. |
| FluentAssertions for assertions | PASS | `using FluentAssertions;` in all three files. |
| No xUnit / NUnit introduced | PASS | Zero references. |
| Seam-based mocking of boundaries | PASS | `Func<byte[], SvgDocument?>` delegate seam, which is option 2 in the `.claude/rules/csharp.md` DI-seam preference order and appropriate for a single call path. |
| IDE / CLI parity | **FAIL** | See G-8. Running `SVGControl.Test` alone, as Test Explorer does, yields 6 failures; running it after a sibling assembly yields 0. |
| Deterministic (no ambient environment reliance) | **FAIL** | See G-8. Outcome depends on the test host's ambient assembly-probing path. |

## 5. Test Coverage Detail

Per-member measurement from `coverage/coverage.cobertura.xml` at this head.

`SVGControl.SvgAssemblyProbe` — 102/102 line = 100%, 92/92 branch = 100%.

| Member | Line | Branch |
|---|---|---|
| `TryGetDirectoryFromCodeBase(string)` | covered | covered |
| `GetProbeDirectories(string, string, string)` | covered | covered |
| `PublicKeyTokensEqual(byte[], byte[])` | 15/15 = 100% | 18/18 = 100% |

`PublicKeyTokensEqual` rose from 0/15 = 0% at cycle 1 to 15/15 = 100%, closing cycle-1 finding CR-6.
AC-8's public-key-token requirement is now verified by measurement rather than by inspection alone.

`SVGControl.SvgAssemblyResolver` — 106/172 line = 61.6279%, 28/52 branch = 53.8462%.

| Member | Line | Assessment |
|---|---|---|
| `Install()` | 6/6 = 100% | The only genuinely new member introduced by this cycle. Clears the >= 90% new-member gate. |
| `ResolveByNameAndKey(object, ResolveEventArgs)` | 47/80 = 58.75% | `private static`, invoked only by the CLR on a failed assembly bind. Carries the ratified `COVERAGE_MEMBER_UNREACHABLE` exception. |

Excluding the exempted `ResolveByNameAndKey`, the instrumented remainder of the file measures
6/6 = 100%. The file-level 61.6279% is therefore entirely attributable to the exempted, relocated
member. Recorded as FAIL under G-9 because the file-level new-file floor is a mandatory gate, with
the disposition stated there.

`SVGControl.SvgRenderer` — 332/414 line = 80.1932%, 64/84 branch = 76.1905%. Per-member detail in
section 1.2.3. Class line coverage rose from 264/422 = 62.559% at baseline through 424/588 = 72.109%
at cycle 1 to 332/414 = 80.1932% at this head. The denominator fell from 588 to 414 because R-6
relocated `ResolveByNameAndKey` and `PublicKeyTokensEqual` out of the class; no line lost coverage.

## 6. Test Execution Metrics

| Run | Command | Result |
|---|---|---|
| Reviewer, `SVGControl.Test` alone | `vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll` | **Failed.** 75 total, 69 passed, 6 failed. All 6 `FileNotFoundException` for `ExCSS, Version=4.3.2.0`. |
| Reviewer, `SVGControl.Test` first, sibling second | `vstest.console.exe SVGControl.Test\...\SVGControl.Test.dll VBFunctions.Test\...\VBFunctions.Test.dll` | **Failed.** 76 total, 70 passed, 6 failed. |
| Reviewer, sibling first, `SVGControl.Test` second | `vstest.console.exe VBFunctions.Test\...\VBFunctions.Test.dll SVGControl.Test\...\SVGControl.Test.dll` | **Successful.** 76 total, 76 passed, 0 failed. |
| Executor, mandated 9-assembly wrapper | `Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` | 6150 total, 6150 passed, 0 failed. `evidence/qa-gates/test-coverage.2026-08-05T01-50.md`. |

The three reviewer runs used the same binaries in the same session. The only variable is the ordinal
position of `SVGControl.Test.dll` on the command line. This is the measurement behind G-8.

The six order-sensitive tests: `SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull`,
`GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument`,
`Constructor_WithTheBuiltInDefaultImageAndNoMargin_LeavesDocumentNonNull`,
`TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException`,
`GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner`,
`GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument`.

Test inventory: 38 `[TestMethod]` in the three new files (18 + 15 + 5). The `SVGControl.Test`
assembly reports 75 tests, the balance being the pre-existing `GetRelativePath_Test` and
`RelativePathCoverageTests` classes this branch did not author.

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Formatting | `dotnet tool run csharpier check .` | exit 0; 1467 files checked; 0 need formatting |
| Analyzer build | `Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` | exit 0; 0 errors; 6 pre-existing warnings |
| Nullable build, as mandated | `Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` | exit 0 in 0.93 s with 0 `CoreCompile` targets — non-probative. See G-3. |
| Nullable build, forced recompile | Same command after touching the six changed C# source files | exit 1; 195 errors, 0 warnings; **all 195 in `UtilitiesCS.csproj`**, zero in `SVGControl` or `SVGControl.Test`; **0 `CS8630`** |
| `SVGControl.Test` isolated compile | Observed within the forced run above | Compiled with `/nullable:enable /langversion:latest`; emitted to `SVGControl.Test\bin\Debug\SVGControl.Test.dll`; zero diagnostics |
| File size | `awk 'END{print NR}'` per changed file | All six changed C# source files under the 500-line limit |

The forced-recompile result is the decisive control for G-3. `UtilitiesCS.csproj` carries a
`ProjectReference` to `SVGControl.csproj` at line 1114, so recompiling `SVGControl` invalidates
`UtilitiesCS`, which then compiles under `/p:Nullable=enable /p:TreatWarningsAsErrors=true` and
surfaces 195 pre-existing nullable diagnostics (`CS8600` x18, `CS8601` x16, `CS8602` x6,
`CS8603` x4, `CS8604` x14, `CS8618` x46, `CS8625` x24) in files such as
`UtilitiesCS/EmailIntelligence/Bayesian/Obsolete/BayesianClassifier.cs` and
`UtilitiesCS/Interfaces/IOutlookObjects/IEmailDetailsWrapper.cs`. Zero `UtilitiesCS` files appear in
the branch diff, so all 195 are definitionally pre-existing and out of scope for this feature.

## 8. Gaps and Exceptions

### G-1 — Modified-file line coverage below the 85% floor (FAIL, non-blocking, carried forward and improved)

`SVGControl/SvgRenderer.cs` measures 332/414 = 80.1932% line coverage against the 85% floor. Branch
coverage 64/84 = 76.1905% clears the 75% floor.

Improved this cycle from 72.109% (cycle 1) and 62.559% (baseline). Every member this feature added or
modified measures 100%; the entire 82-line residual sits in six pre-existing members this bug fix
did not touch (section 1.2.3). No changed line regressed.

Disposition: **non-blocking.** The residual is pre-existing debt in a WinForms/GDI-bound rendering
class, not a deficiency in the delivered change. `issue.md` AC-5 records that the floor was
explicitly out of R-4's remediation scope and that the residual is owned by the filed follow-up
`docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`, which the reviewer confirms
exists in the branch diff.

### G-2 — AC-11 undelivered (FAIL, BLOCKING)

The AC-11 WinForms-designer runbook has not been executed, so `issue.md:110` remains `- [ ]`.

Correctly tracked. `artifacts/orchestration/orchestrator-state.json` carries a well-formed
`human_interaction.requirements` block with H-1 (satisfies AC-11) and H-2 (satisfies AC-7), both
`response: "exception"`, both citing
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md`.
Both satisfy the `.claude/rules/orchestrator-state.md` invariant that an `exception` response carry a
non-empty `runbook_path`. The runbook exists in the diff at 283 lines.

The caller states R-1 was deliberately not attempted because no agent can execute it. The reviewer
agrees: opening a form in the legacy in-process Visual Studio WinForms designer has no unattended
automation surface.

Disposition: **blocking for PR readiness, not remediable by an agent.** This requires a human
operator session or an explicit maintainer waiver.

### G-3 — Mandated nullable gate is non-probative; `CS8630` resolved (PARTIAL, improved)

Two parts, tracked together since cycle 1.

Part (a), `CS8630` — **RESOLVED.** `SVGControl.Test/SVGControl.Test.csproj:17` now declares
`<LangVersion>latest</LangVersion>`. The reviewer forced a recompile and observed the project compile
with `/nullable:enable /langversion:latest` emitting zero diagnostics and zero `CS8630`. Cycle-1
finding CR-1 is closed.

Part (b), vacuity — **UNRESOLVED, and now shown to be more consequential than cycle 1 recorded.** The
mandated solution-wide command returns exit 0 in 0.93 s with zero `CoreCompile` targets: legacy
non-SDK up-to-date checks are timestamp-based, not property-based, so the gate passes by not
compiling anything. When the reviewer forced a genuine recompile, the same command returned exit 1
with 195 errors. All 195 are pre-existing `UtilitiesCS` diagnostics in files absent from the branch
diff; zero are in `SVGControl` or `SVGControl.Test`.

Disposition: **PARTIAL, non-blocking, not attributable to this branch.** The correct adjudication of
AC-6's type-check stage is the isolated `SVGControl`/`SVGControl.Test` compile, which is clean. This
matches the precedent for the `utilitiescs-nullable-remediation` epic children, where a plan-literal
full-solution nullable build fails on pre-existing out-of-scope diagnostics and DoD is adjudicated
against the isolated project build. The finding that the repository's mandated nullable gate is
structurally non-probative is a repository-level concern that exceeds this feature's scope and
warrants a separate follow-up entry.

### G-4 — Test-file location diverges from the mirrored-`tests/` rule (accepted, pre-existing)

`.claude/rules/general-unit-test.md` requires tests in a `tests/` tree mirroring production. This
repository places C# tests in sibling `*.Test` projects; `SVGControl.Test` follows the convention
established by 8 existing test projects. Accepted as a pre-existing repository-wide convention; not
a defect in this branch.

### G-5 — MCP template assets unavailable (documented assumption)

The SKILL directs artifact creation from the MCP tool `resolve_policy_audit_template_asset` with
selectors `template`, `code-review-template`, and `feature-audit-template`. No such tool is present
in this session's tool surface. The reviewer therefore mirrored the structure of cycle 1's
`policy-audit.2026-08-04T20-25.md`, `code-review.2026-08-04T20-25.md`, and
`feature-audit.2026-08-04T20-25.md` in the same feature folder, which preserves the canonical major
headings and both appendices. Documented as an assumption, not a gap in the delivery.

### G-6 — Reviewer side effect, disclosed

To make the nullable gate probative the reviewer used `touch` to update the modification timestamps
of the six changed C# source files, forcing `CoreCompile`. File contents were not altered;
`git status --porcelain` remained empty throughout. The reviewer then re-ran the analyzer build to
restore a consistent `Debug` output tree and confirmed both
`SVGControl.Test/bin/Debug/SVGControl.Test.dll` and `UtilitiesCS/bin/Debug/UtilitiesCS.dll` are
present. Disclosed for completeness; no lasting effect.

### G-7 — PR-context collector defects (documented, corrected in place)

Two collector false statements, both corrected in `artifacts/pr_context.summary.txt` and detailed in
the PR-Context Artifact Corrections section: C# misclassified as documentation, and `gh` falsely
reported as not installed. The first would have caused the coverage gate to skip C# enforcement.
Neither is a defect in the feature under review.

### G-8 — Six tests are order-dependent on the `vstest` command line (FAIL, BLOCKING, new this cycle)

Six tests in `SVGControl.Test` pass or fail depending on the ordinal position of the assembly on the
`vstest.console.exe` command line. Reviewer measurements are in section 6: alone, 6 failed; with a
sibling second, 6 failed; with the same sibling first, 0 failed.

Root cause, established empirically rather than by inference. `SVGControl.Test/bin/Debug` contains
`Svg.dll` but **not** `ExCSS.dll` or `Fizzler.dll`. `SVGControl.Test.csproj` references `Svg`
(added by this branch) but never references `ExCSS`; `ExCSS` is a transitive dependency of `Svg`, and
legacy non-SDK `packages.config` projects do not flow transitive copy-local. Consequently:

- the `ExCSS` binding redirect in `SVGControl.Test/app.config`, which AC-10 corrected to
  `newVersion="4.3.2.0"`, cannot help, because redirection presupposes the file is findable; and
- the `AssemblyResolve` fallback's strategy 3 probes the directory containing `SVGControl.dll`,
  which is that same `SVGControl.Test/bin/Debug`, and finds no `ExCSS.dll`.

The bind then succeeds only when another test assembly's output directory has already supplied
`ExCSS` to the test host, which depends on which assembly vstest processes first. All eight sibling
test projects reference `ExCSS` explicitly and carry `ExCSS.dll` in their output.

Policy violations, each direct and each quoted:

- `.claude/rules/general-unit-test.md`, UT1: "Tests must be able to run in any order without
  impacting each other."
- `.claude/rules/general-unit-test.md`, External Dependencies: "Tests must not rely on mutable
  global state or external configuration that can change between runs."
- `.claude/rules/csharp.md`, Deterministic Test Rules: "Tests must produce identical results in the
  IDE test runner and in CLI runs so local and CI behavior agree." A developer opening
  `SVGControl.Test` in Test Explorer sees 6 red tests.

Secondary consequences for this feature's evidence:

- AC-7's corroboration cites
  `SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull` passing "with its full
  `Document`-non-null assertion intact". That test is one of the six. The claim holds under the
  mandated 9-assembly wrapper but is conditional on assembly ordering, which the citation does not
  state.
- The AC-5 amendment's measured premise that `Array.Empty<byte>()` raises `XmlException` is itself
  environment-conditional: in an isolated run the same input raises `FileNotFoundException`, and the
  two tests asserting `XmlException` are among the six failures.

The executor disclosed this condition in
`evidence/other/resolver-containment.2026-08-05T01-50.md:130-152` and assessed it as "not a
regression". The reviewer accepts that it is not a regression and confirms the condition predates
cycle 1 (present at head `ea106111`, where the `Svg` reference already existed). The reviewer did
not identify it in cycle 1; it is newly surfaced, not newly introduced. Two accuracy notes on the
disclosure: it records "fails 5 of 65 tests", whereas the figure at this head is 6 of 75 — the
delta is the 10 tests added by later tasks `[P1-T12]`, `[P1-T14]` and `[P1-T15]`, one of which
(`Constructor_WithTheBuiltInDefaultImageAndNoMargin_LeavesDocumentNonNull`) also requires a
successful parse. And the disclosure's proof command places the sibling first, which is the passing
order; reversing the two arguments reproduces the failure, so ordering rather than mere co-execution
is the operative variable.

Disposition: **blocking.** It is an unambiguous violation of a core unit-test policy, it degrades the
trustworthiness of the AC-1 regression tests that are the entire purpose of issue #418, and the
remedy is a one-line project change of the same shape this branch already applied for `Svg`. It has
zero production-behavior impact, so a maintainer may reasonably choose to waive it; the reviewer
records it as blocking because the policy language admits no discretion.

Recommended fix, for the remediation planner:

```xml
<Reference Include="ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL">
  <HintPath>..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll</HintPath>
  <Private>True</Private>
</Reference>
```

plus `<package id="ExCSS" version="4.3.2" targetFramework="net481" />` in
`SVGControl.Test/packages.config`. Add `Fizzler 1.3.1` on the same pattern for parity with the eight
sibling projects. Verification: `vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll`
alone must return 75/75. Note that the existing `Svg` reference added by this branch omits
`<Private>True</Private>`; it copies anyway by default, but adding it explicitly would match the
surrounding style.

### G-9 — New-file coverage floor not met on `SvgAssemblyResolver.cs` (FAIL, non-blocking, new this cycle)

`SVGControl/SvgAssemblyResolver.cs` is a file added by this branch and measures 106/172 = 61.6279%
line and 28/52 = 53.8462% branch, against the new-file floors of 85% line and 75% branch, and the
>= 90% new-module line threshold in `.claude/rules/csharp.md`.

The entire shortfall is one member. `ResolveByNameAndKey` measures 47/80 = 58.75% and is
`private static`, invoked only by the CLR when an assembly bind fails. It is not new code: R-6
relocated it verbatim from `SVGControl/SvgRenderer.cs`, where it already carried the ratified
`COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey` exception recorded
in `issue.md` AC-5. `Install()`, the only genuinely new member in the file, measures 6/6 = 100% and
clears the >= 90% gate. Excluding the exempted member, the instrumented remainder is 100%.

The exemption is consistent with the COM/VSTO/host-bound coverage exemption in `CLAUDE.md`, which
excludes code that "cannot be unit-tested without a live" host process and has no injectable seam. A
CLR-invoked `AssemblyResolve` callback is in that class: the handler can only be driven end-to-end by
inducing a real failed bind in the test host, and this branch already extracted every seam-testable
fragment of it into `SvgAssemblyProbe`, which measures 100% line and 100% branch.

Disposition: **non-blocking, recorded as FAIL because the file-level gate is mandatory and admits no
partial credit.** The rescoping check was applied and does not rescue the file-level number, only the
member-level one; unlike the `StoresWrapper` case in issue #328, re-scoping to the instrumented
package does not clear the floor here. Recommended for maintainer adjudication as either a
file-scoped extension of the existing ratified exception or an entry in the same coverage-uplift
follow-up that owns G-1.

## 9. Summary of Changes

The feature eliminates the `NullReferenceException` reported in issue #418 by three coordinated
changes:

1. **Parse-failure boundary.** `SvgRenderer.GetSvgDocument(byte[])`'s silent
   `catch (Exception) { return null; }` is replaced by `TryGetSvgDocument`, a single boundary that
   logs on both `log4net` and `Trace` and returns `false` with the exception in `out error`. Three
   surfaces are offered: the tolerant `GetSvgDocument`, the try-style `TryGetSvgDocument`, and the
   fail-fast `GetSvgDocumentOrThrow` whose `InnerException` preserves the original parser exception.
   Both byte-array constructors now degrade to `Size.Empty` with a logged cause instead of
   dereferencing a null document.
2. **Assembly-binding fallback.** The `AssemblyResolve` handler gains a directory-probing strategy
   and is extracted to `SvgAssemblyResolver.cs`, with its pure decision logic in
   `SvgAssemblyProbe.cs` at 100% line and branch coverage.
3. **Test project repair.** `SVGControl.Test` is added to `TaskMaster.sln`, gains
   `<LangVersion>latest</LangVersion>`, has its `ExCSS` binding redirect corrected from a
   nonexistent `4.2.4.0` to the deployed `4.3.2.0`, and receives 38 new tests.

Remediation cycle 1 closed all seven actionable cycle-1 code-review findings and improved
repository-wide coverage above both mandatory floors. An incidental undisclosed improvement carried
forward from cycle 1: `OpenFromBytes` disposes its `MemoryStream` via `using`, where the baseline
leaked it on every call.

## 10. Compliance Verdict

| Area | Verdict |
|---|---|
| General Code Change Policy | PASS |
| General Unit Test Policy | **FAIL** (UT1 Independence and Determinism — G-8) |
| C# Code Change Policy | PARTIAL (nullable gate non-probative — G-3) |
| C# Unit Test Policy | **FAIL** (IDE/CLI parity — G-8) |
| Coverage, repository-wide | PASS (line 85.4097%, branch 78.7220%) |
| Coverage, changed files | **FAIL** (G-1 modified file, G-9 new file; both dispositioned non-blocking) |
| Evidence location compliance | PASS |
| `modified-workflow-needs-green-run` | Not triggered |
| Acceptance criteria | PARTIAL (10 of 11 PASS; AC-11 FAIL) |

**Overall: PARTIAL. Blocking count: 2** (G-2 AC-11 undelivered, human-only; G-8 test order
dependence, one-line fix).

Cycle 1 recorded blocking count 1. The count changed from 1 to 2. The cycle-1 blocker G-2 is
unchanged and remains blocking. G-8 is added: it is a pre-existing condition on this branch that
the reviewer failed to detect in cycle 1, not a regression caused by remediation. All six items the
remediation plan set out to address (R-2 through R-6) are verified delivered.

## Appendix A: Coverage Verification Detail

Repository-wide C#, from the root element of `coverage/coverage.cobertura.xml`:

```
line-rate="0.854097" branch-rate="0.78722"
lines-covered="93539" lines-valid="109518"
branches-covered="21584" branches-valid="27418"
```

- Line: 93539 / 109518 = **85.4097%** against the 85% floor. **PASS.**
- Branch: 21584 / 27418 = **78.7220%** against the 75% floor. **PASS.**

Canonical JaCoCo artifact `artifacts/csharp/coverage.xml`:

```xml
<counter type="LINE" missed="15979" covered="93539" />
<counter type="BRANCH" missed="5834" covered="21584" />
```

Conversion validated: 93539 + 15979 = 109518 and 21584 + 5834 = 27418 both reconcile to the
Cobertura root. Exactly one counter per type, as the hook's summing parser requires.

Checklist of the four language coverage artifacts:

- TypeScript coverage artifact `coverage/lcov.info`: not required; zero `.ts`/`.tsx` files changed.
- Python coverage artifact `artifacts/python/lcov.info`: not required; zero `.py` files changed.
- PowerShell coverage artifact `artifacts/pester/powershell-coverage.xml`: not required; zero
  `.ps1`/`.psm1` files changed.
- C# coverage artifact `artifacts/csharp/coverage.xml`: present, parsed, reconciled, and reported
  above; verdict recorded in section 1.2.1.

Baseline / post-change comparison, C#: Baseline: 62.559% on the primary changed class. Post-change:
85.4097% repository-wide line and 78.7220% branch. Change: improved. Disposition: repository-wide
floors met; two file-level floors not met and dispositioned non-blocking under G-1 and G-9.

## Appendix B: Toolchain Commands Reference

Commands the reviewer executed, in the order run. All are check-only except the two builds, which
write to `bin/obj` only, and the disclosed `touch`.

```powershell
# Scope and baseline
git rev-parse HEAD
git merge-base HEAD origin/main
git diff --numstat ce0c91e686bf7e060aaab6f185ee6883269e4fd4...a62391f719c6d5ecc3d80115916c95d1966ca514
git status --porcelain

# 1. Formatting
dotnet tool run csharpier check .

# 2. Linting / analyzers
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 `
  -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" `
  -EnableNETAnalyzers -EnforceCodeStyleInBuild

# 3. Type check, as mandated (returns 0 vacuously)
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 `
  -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" `
  -EnableNullable -TreatWarningsAsErrors

# 3b. Type check, forced probative (touch the six changed C# files first)
touch SVGControl/SvgRenderer.cs SVGControl/SvgAssemblyResolver.cs SVGControl/SvgAssemblyProbe.cs `
      SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs `
      SVGControl.Test/SvgRendererParseContractTests.cs `
      SVGControl.Test/SvgRendererNullToleranceTests.cs
# then re-run the command in step 3

# 4. Tests — order-dependence probe
vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll
vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
vstest.console.exe VBFunctions.Test\bin\Debug\VBFunctions.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll

# 5. Coverage — inspected, not regenerated, per the SKILL contract
#    coverage/coverage.cobertura.xml and artifacts/csharp/coverage.xml re-parsed per <line> descendant

# Policy scans
git diff --name-only ce0c91e6...a62391f7 | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
git diff --name-only ce0c91e6...a62391f7 | grep -E "^(\.github/workflows/|\.github/actions/|scripts/benchmarks/)"
awk 'END{print NR}' SVGControl/SvgRenderer.cs   # 500-line limit, avoids the Measure-Object undercount
```

Not run, with reasons: coverage generation, because valid artifacts exist at this head and the SKILL
directs inspection over regeneration; the full 9-assembly suite, for the same reason, its result
being taken from `evidence/qa-gates/test-coverage.2026-08-05T01-50.md` and corroborated by the
coverage artifact's generation timestamp at this head; PoshQC and Pester, because no PowerShell files
changed; `npm run test:unit:coverage` and `poetry run pytest --cov`, because no TypeScript or Python
files changed.
