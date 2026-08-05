# Policy Audit — svg-renderer-null-document-nre (Issue #418)

- Component: `SVGControl` (production), `SVGControl.Test` (tests), `TaskMaster.sln`
- Audit timestamp: 2026-08-04T20-25
- Reviewer: feature-review agent
- Work mode: `minor-audit` (marker `- Work Mode: minor-audit` at `issue.md:12`)
- Acceptance-criteria source: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`, section `## Acceptance Criteria`

## Baseline Resolution

| Item | Value |
|---|---|
| Base branch (requested) | `main` |
| Base ref (resolved) | `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Merge-base SHA | `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Head ref | `bug/svg-renderer-null-document-nre-418` @ `ea106111a6daf7e05f8a804ac00b4a713598962a` |
| Merge-base recomputed by reviewer | `git merge-base HEAD origin/main` returned `ce0c91e6...`, identical to the supplied value |
| Working tree at audit time | clean (`git status --porcelain` empty) |
| Commits in range | 5 (`0162567d`, `a5695656`, `296eac95`, `82badeba`, `ea106111`) |
| PR context summary | `artifacts/pr_context.summary.txt` (head `ea106111`, matches `git rev-parse HEAD`) |
| PR context appendix | `artifacts/pr_context.appendix.txt` |

The PR-context artifacts were current for the audited head and were not regenerated. Three factual
corrections were annotated into `artifacts/pr_context.summary.txt` in place; see
`## PR-Context Artifact Corrections` below.

## Executive Summary

This branch fixes issue #418 by replacing a silent exception swallow in
`SVGControl/SvgRenderer.cs` with a single logged parse-failure boundary, converting both byte-array
`SvgRenderer` constructors from unguarded dereference to degrade-and-log, adding a `Try`-style and a
throwing parse API, extending the `AssemblyResolve` fallback with a directory-probing strategy,
extracting the probe decision logic into a new pure type `SVGControl/SvgAssemblyProbe.cs`, wiring
`SVGControl.Test` into `TaskMaster.sln`, and adding 28 MSTest tests.

Overall verdict: **PARTIAL**. The change is well-engineered, thoroughly evidenced, and the toolchain
is clean. Four dispositions drive the PARTIAL:

1. **FAIL** — modified-file line coverage. `SVGControl/SvgRenderer.cs` measures 72.109% against the
   85% modified-file floor. This is an improvement of +9.55 points over its 62.559% baseline and
   contains no regression on any changed line; the residual gap is pre-existing untested code in the
   same file that this bug fix did not touch. Recorded as FAIL per the mandatory floor rule, with a
   non-blocking disposition and a concrete remediation path.
2. **FAIL** — acceptance criterion AC-11 is undelivered. The documented human designer-load runbook
   was not executed and no capture exists at the expected evidence path.
3. **PARTIAL** — the mandated solution-wide nullable/`TreatWarningsAsErrors` gate returns exit 0 only
   because legacy MSBuild up-to-date checks are timestamp-based, not property-based. Independently
   confirmed: the reviewer's solution-level run executed 0 `CoreCompile` targets in 1.70 s. A forced
   recompile of `SVGControl.Test` under the same property set emits `CS8630`, which is newly reachable
   relative to the merge-base because this branch is what makes the project a solution member.
4. **PASS with residual risk** — the resolver's outer `catch` was removed, so a small set of throw
   sites can now escape an `AssemblyResolve` handler.

Repository-wide coverage clears both floors and improved in both metrics. All four toolchain stages
pass. No policy document, rule file, or unrelated source file was modified.

## Rejected Scope Narrowing

None. The caller prompt supplied the resolved base branch, the merge-base SHA, the head SHA, the
active feature folder, the work mode, and two factual notes about defects in the PR-context
collector. It instructed the reviewer to determine review scope from the branch diff per the skill
contract and explicitly stated that neither factual note constrains scope or findings. No caller
instruction limited the audit to a plan, task, or phase, to a subset of changed files, or attempted to
mark any language as excluded from assessment.

The audited scope is the full branch diff against `origin/main` @ `ce0c91e6`: 74 changed files.

## Evidence Location Compliance

`git diff --name-only ce0c91e6...HEAD` returns zero files under `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. Every feature evidence artifact on
this branch is written under
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/<kind>/` with `<kind>` in
`{baseline, qa-gates, regression-testing, other}`, matching
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. **PASS.**

`scripts/dev_tools/validate_evidence_locations.py` does not exist in this repository, so the scan was
performed with the `git diff --name-only` path filter above rather than that script. No
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose: this audit writes its own four artifacts to the
active feature folder root, which is the location required by both the reviewer contract and the
`validate-feature-review-coverage.ps1` SubagentStop hook.

## Change Inventory (feature-vs-base)

74 files changed. Code and build files, 10:

| File | Diff | Class |
|---|---|---|
| `SVGControl/SvgRenderer.cs` | +167/-24 | modified production |
| `SVGControl/SvgAssemblyProbe.cs` | +67/-0 | new production |
| `SVGControl/SVGControl.csproj` | +1/-0 | build (adds the new `<Compile>`) |
| `SVGControl.Test/SvgRendererParseContractTests.cs` | +332/-0 | new test |
| `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs` | +187/-0 | new test |
| `SVGControl.Test/SvgRendererNullToleranceTests.cs` | +143/-0 | new test |
| `SVGControl.Test/SVGControl.Test.csproj` | +6/-0 | build (3 `<Compile>`, 1 `Svg` reference) |
| `SVGControl.Test/app.config` | +1/-1 | binding redirect (ExCSS to `4.3.2.0`) |
| `SVGControl.Test/packages.config` | +1/-0 | pins `Svg 3.4.8` |
| `TaskMaster.sln` | +14/-0 | adds `SVGControl.Test` as a solution member |

Documentation and agent-memory files, 64 (all `.md`): 47 under the active feature folder (issue, plan,
research, runbook, handoff, 30 evidence artifacts), 15 under `.claude/agent-memory/`, and 2 new
`docs/features/potential/` deferral entries.

Languages with changed files in the branch diff: **C# only**. Zero `.ts`/`.tsx`, zero `.py`, zero
`.ps1`/`.psm1`.

## PR-Context Artifact Corrections

Three defects in the collector output were corrected in place in
`artifacts/pr_context.summary.txt`, each annotated with a dated `CORRECTION` or `NOTE` block that
preserves the collector's original text:

1. **Changed-files misclassification (material).** The overview reported `Core logic changes: 0 files`
   and classified the branch as docs-only, enumerating only 40 `.md` paths. This is factually wrong:
   the branch changes one new and one modified production C# file, three new test files, two project
   files, a binding-redirect config, a `packages.config` entry, and the solution file. The
   misclassification is not cosmetic — `.claude/hooks/validate-feature-review-coverage.ps1`
   derives its changed-language set from exactly these overview bullets
   (`Get-ChangedLanguageSet`, lines 121-138), so the uncorrected artifact caused the hook to
   enumerate zero languages and skip all per-language enforcement. The corrected enumeration was
   verified by simulating the hook against the repaired artifact: it now enumerates `CSharp`.
2. **False GitHub CLI unavailability.** The artifact states `gh` is not installed. It is installed
   (2.87.3) and on PATH. All downstream `GitHub CLI unavailable` and `(not available)` sections are
   collector limitations, not environment facts.
3. **Polluted auto-close list.** The extraction regex emitted `#419`, `#AC-1` through `#AC-11`, and
   `#DE06-4337` alongside `#418`. `#419` is the already-merged package-update PR this branch rebased
   onto; `#AC-*` are acceptance-criteria labels lifted from commit messages; `#DE06-4337` is a
   fragment of the `SVGControl.Test` project GUID `{13AC39E6-DE06-4337-8EB0-41CE674A4C3B}` added to
   `TaskMaster.sln`. None carries a closing keyword. `#418` is the only issue this branch closes.

## 1. General Unit Test Policy Compliance

Reference: `.claude/rules/general-unit-test.md`, `CLAUDE.md` § General Unit Test Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| UT1 Independence | PASS | All 28 new tests construct their own subject in-method. No `[ClassInitialize]`, `[AssemblyInitialize]`, or static mutable state is introduced. The only shared statics are two `private static readonly Size` constants. |
| UT1 Isolation | PASS | Each `[TestMethod]` exercises one member or one contract. Names encode subject plus scenario plus expectation, e.g. `TryGetSvgDocument_WhenTheParseSeamReturnsNull_ReturnsFalseWithNoCapturedError`. |
| UT1 Fast execution | PASS | Full nine-assembly suite 58.2533 s for 6140 tests; the +28 new tests are pure in-memory parse and path-string assertions with no I/O. Source: `evidence/qa-gates/test-coverage.2026-08-04T14-36.md`. |
| UT1 Determinism | PASS | No clock, no RNG, no network, no `Thread.Sleep`/`Task.Delay`. The one non-deterministic-in-principle path (a live assembly bind) is deliberately not asserted; `SvgAssemblyProbeDirectoryTests` documents this at lines 12-13. |
| UT1 Readability | PASS | Every test uses explicit `// Arrange` / `// Act` / `// Assert` comments and a `because` reason string on every FluentAssertions call. |
| UT2 Scenario completeness | PASS | Positive (`GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument`, `GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument`), negative (malformed and empty payloads on both constructor overloads), argument boundary (`ArgumentNullException` on both `GetSvgDocument` and `TryGetSvgDocument`), error handling (inner-exception preservation, exception-instance identity via the seam), and edge cases (empty `Location`, unparsable code base, case-variant de-duplication, all-null inputs). |
| UT3 Arrange-Act-Assert | PASS | Verified by inspection of all three new test files. |
| UT3 Clear failure messages | PASS | Every assertion carries a `because` argument stating the policy or criterion it defends. |
| UT4 No external dependencies | PASS | No database, network, remote API, or external process. The only external-boundary seam is `Func<byte[], SvgDocument?>`, mocked with Moq. |
| UT4 No temporary files | PASS | `grep` for `Path.GetTempPath`, `GetTempFileName`, `File.Create`, `File.Write` across the three new test files returns zero matches. All filesystem-shaped inputs are string literals never touched on disk. |
| Test file location | PASS with note | `.claude/rules/general-unit-test.md` requires a mirrored `tests/` tree. `SVGControl.Test/` is this repository's established, pre-existing sibling-test-project layout, shared by all nine test projects. The new files match the repository's actual convention; imposing `tests/` here would diverge from every sibling. Recorded as a pre-existing repository-level convention divergence, not a defect introduced by this branch. |
| File size limit (test files) | PASS | 332, 187, and 143 lines against the 500-line limit. |
| Coverage exclusion policy | PASS | No `[ExcludeFromCodeCoverage]` attribute and no `coverage.config` entry is added. Zero production paths are excluded from measurement. |

### 1.2 Coverage Verification

Method: the coverage artifacts produced by the executor run were inspected rather than regenerated,
per the reviewer contract. The reviewer independently re-parsed
`coverage/coverage.cobertura.xml` (the post-change run, written 2026-08-04 20:02) with a fresh
XML parse and reproduced every figure the feature evidence claims, to four decimal places.

Canonical artifact status:

| Language | Canonical artifact | Present | Reviewer action |
|---|---|---|---|
| C# | `artifacts/csharp/coverage.xml` | yes (291 bytes, JaCoCo conversion of the Cobertura run, written 2026-08-04 20:15) | counters cross-checked against the source Cobertura; exact match |
| TypeScript | `coverage/lcov.info` | absent | zero changed files of this language on the branch; not assessed |
| Python | `artifacts/python/lcov.info` | stale (2026-07-18) | zero changed files of this language on the branch; not assessed |
| PowerShell | `artifacts/pester/powershell-coverage.xml` | stale (2026-06-12) | zero changed files of this language on the branch; not assessed |

The canonical C# artifact carries `<counter type="LINE" missed="16002" covered="93484" />` and
`<counter type="BRANCH" missed="5878" covered="21528" />`, giving 85.38% and 78.55%. The source
Cobertura root reads `line-rate="0.853844" lines-covered="93484" lines-valid="109486"` and
`branch-rate="0.785521" branches-covered="21528" branches-valid="27406"`. The two agree exactly.

Denominator scope check: the Cobertura report contains exactly nine `<package>` elements, all
first-party (`UtilitiesCS`, `QuickFiler`, `TaskMaster`, `SVGControl`, `ToDoModel`,
`TaskVisualization`, `Tags`, `TaskTree`, `VBFunctions`). No vendor or third-party assembly inflates
the denominator, so the repository-wide figure is a genuine first-party measurement and needs no
by-name exclusion pass.

#### 1.2.1 Per-language comparison

- **C#** — Baseline: line 93252 / 109252 = 85.3550%, branch 21448 / 27310 = 78.5353%. Post-change:
  line 93484 / 109486 = **85.3844%**, branch 21528 / 27406 = **78.5521%**. Change: line **+0.0294**
  points, branch **+0.0168** points, both improvements. New/changed-code coverage: **100.000%** line
  coverage across all seven newly added members, and 100.000% for the one new file. Disposition:
  **PASS** on both repository-wide floors (`>= 85%` line, `>= 75%` branch) and on the `>= 90%`
  new-member gate. Evidence: `coverage/coverage.cobertura.xml`,
  `artifacts/csharp/coverage.xml`, `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md`,
  `evidence/qa-gates/test-coverage.2026-08-04T14-36.md`, all reproduced independently by the reviewer.
- **TypeScript** — zero changed files of this language on the branch. Baseline: not measured.
  Post-change: not measured. Change: none. Disposition: not assessed, no changed files.
- **Python** — zero changed files of this language on the branch. Baseline: not measured.
  Post-change: not measured. Change: none. Disposition: not assessed, no changed files.
- **PowerShell** — zero changed files of this language on the branch. Baseline: not measured.
  Post-change: not measured. Change: none. Disposition: not assessed, no changed files.

#### 1.2.2 Coverage checklist

- Repo-wide line coverage for C# is `85.3844%` against the `>= 85%` floor: **PASS**.
- Repo-wide branch coverage for C# is `78.5521%` against the `>= 75%` floor: **PASS**.
- New-file line coverage for C# is `100.000%` (`SVGControl/SvgAssemblyProbe.cs`, 68/68) against the
  `>= 85%` new-file floor and the `>= 90%` new-code gate: **PASS**.
- Modified-file line coverage for C# is `72.109%` (`SVGControl/SvgRenderer.cs`, 424/588) against the
  `>= 85%` modified-file floor: **FAIL**, dispositioned non-blocking (see `## 8. Gaps and Exceptions`,
  gap G-1: no regression on any changed line, +9.55 points over baseline, residual gap is pre-existing
  untested code in the same file).
- New-member line coverage for C# is `100.000%` across all seven newly added members against the
  `>= 90%` gate: **PASS**.
- No-regression-on-changed-lines for C# holds: **PASS**. Every changed member improved or held, and
  the class numerator rose from 264 to 424 covered lines.

#### 1.2.3 Newly added members

Independently reproduced from `coverage/coverage.cobertura.xml`:

| Type | Member | line-rate | Lines | branch-rate |
|---|---|---|---|---|
| `SVGControl.SvgRenderer` | `OpenFromBytes(byte[])` | 100.000% | 5/5 | 100.0% |
| `SVGControl.SvgRenderer` | `TryGetSvgDocument(byte[], Func, out, out)` (seam) | 100.000% | 23/23 | 87.5% |
| `SVGControl.SvgRenderer` | `TryGetSvgDocument(byte[], out, out)` | 100.000% | 3/3 | 100.0% |
| `SVGControl.SvgRenderer` | `GetSvgDocumentOrThrow(byte[])` | 100.000% | 6/6 | 100.0% |
| `SVGControl.SvgRenderer` | `DescribeFailure(Exception)` | 100.000% | 5/5 | 100.0% |
| `SVGControl.SvgAssemblyProbe` | `TryGetDirectoryFromCodeBase(string)` | 100.000% | 11/11 | 100.0% |
| `SVGControl.SvgAssemblyProbe` | `GetProbeDirectories(string, string, string)` | 100.000% | 23/23 | 100.0% |

Minimum observed: 100.000%, ten points above the `>= 90%` gate.

#### 1.2.4 Changed pre-existing members

| Member | Baseline line-rate | Post-change line-rate | Lines | Direction |
|---|---|---|---|---|
| `GetSvgDocument(byte[])` | 62.50% | 100.000% | 4/4 | improved |
| `.ctor(byte[], Size, AutoSize)` | 0% | 76.471% | 13/17 | improved from zero |
| `.ctor(byte[], Size, Padding, AutoSize)` | 100.00% | 100.000% | 18/18 | unchanged |
| `ResolveByNameAndKey(object, ResolveEventArgs)` | 72.09% | 68.116% | 47/69 | rate fell, covered lines rose 31 to 47 |

`ResolveByNameAndKey` is the only rate decline. It is a denominator effect, not a loss: the member
grew from 43 to 69 measured lines while covered lines rose by 16. The reviewer confirmed the
partition the feature evidence claims: the pre-existing strategy-1 and strategy-2 inner blocks were
already uncovered at baseline (both call `PublicKeyTokensEqual`, which measures 0/15 = 0.000% and is
therefore never invoked in any test), and the newly uncovered region is the strategy-3
`Assembly.LoadFrom` block, which carries the plan's ratified
`COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgRenderer.ResolveByNameAndKey` exception. No changed line
lost coverage.

## 2. General Code Change Policy Compliance

Reference: `.claude/rules/general-code-change.md`, `CLAUDE.md` § General Code Change Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow: failing regression test first | PASS | `evidence/regression-testing/ac1-fail-before.2026-08-04T14-36.md` records the four regression tests failing with `NullReferenceException` at `SvgRenderer.cs:133` at branch commit `296eac95`, before any production edit. `ac1-pass-after.2026-08-04T14-36.md` records the same four tests passing with unchanged assertions after the fix. |
| Bugfix workflow: minimal targeted fix | PASS | Production edits are confined to `SVGControl/SvgRenderer.cs` and one new file in the same assembly. `SVGControl/SvgImageSelector.cs`, and every one of the eleven forms that host `PictureBoxSVG`, are untouched. |
| Bugfix workflow: no opportunistic widening | PASS | Two defects discovered during the work were deferred to `docs/features/potential/` rather than fixed in-branch: `2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md` and `2026-08-04-invoke-mstest-scalar-count-strictmode.md`. The reviewer independently confirmed both underlying conditions still hold. |
| Design: simplicity first | PASS | The fix collapses three parse paths onto one boundary method rather than adding guards at each call site. |
| Design: reusability | PASS | `DescribeFailure` is shared by five call sites; `ParseFailed` is a single named constant. |
| Design: separation of concerns | PASS | The pure path-string decision logic was extracted from the host-bound `AssemblyResolve` handler into `SvgAssemblyProbe`, which is the change that made it directly testable. |
| Error handling: fail fast, no silent ignore | PASS | The single `catch (Exception ex)` at the parse boundary logs through two channels and returns the exception in an `out` parameter the caller must inspect. The two resolver catches log through `Trace` and continue to the next strategy, which is the documented contract of an `AssemblyResolve` handler. Zero bare `catch` blocks remain in the file. |
| Error handling: broad catch only at a boundary with added context | PASS | All three surviving `catch (Exception ex)` sites are at defined boundaries and all three add context (`ParseFailed` prefix plus exception type and message, or the requested assembly name / probed path). |
| Logging: project pattern | PASS | Production logging uses the file's pre-existing `log4net.ILog logger`. The added `System.Diagnostics.Trace` calls are a second channel required by AC-3 because no `log4net` appender is known to be configured inside `devenv.exe`; the rationale is documented in-code at the resolver catches and at the parse boundary. No `Console.WriteLine` is introduced. |
| File size limit, 500 lines | PASS with note | `SVGControl/SvgRenderer.cs` is **497** lines (baseline 354), three lines under the hard limit. `SVGControl/SvgAssemblyProbe.cs` is 67. All three new test files are under 340. Verified with both `wc -l` and `awk END{print NR}` to avoid the known PowerShell `Measure-Object -Line` undercount. |
| Module cohesion | PASS | The new file has one responsibility (assembly-probe path decisions) and its header comment states why it is separate from the renderer. |
| Naming | PASS | `PascalCase` types and public members, `camelCase` locals, no cryptic abbreviations. |
| Comment why, not what | PASS with note | Non-obvious choices carry rationale comments: why `Trace` rather than `log4net` inside a resolve handler, why candidates are null-checked explicitly instead of with `IsNullOrWhiteSpace` (net481 lacks `NotNullWhen`), why strategy 3 is ordered last, why an empty `Location` is skipped. Note: two pre-existing comments in the header block are now stale; see finding CR-4 in the code review. |
| No dependency additions beyond approved | PASS | The only new reference is `Svg 3.4.8`, already pinned by `SVGControl`, `QuickFiler`, and `UtilitiesCS`. `SVGControl.Test/packages.config` pins the identical version already restored under `packages/Svg.3.4.8/`. No new third-party package enters the repository. |
| No policy or rule files modified | PASS | `git diff --name-only ce0c91e6...HEAD` contains no path under `.claude/rules/` or `.github/instructions/`. |
| I/O isolation | PASS | `SvgAssemblyProbe` is pure string arithmetic and performs no I/O; the `File.Exists` and `Assembly.LoadFrom` calls remain in the host-bound handler. |
| Toolchain loop, single clean pass | PASS with note | See `## 7. Code Quality Checks`. Recorded as one pass with no restart in `evidence/qa-gates/toolchain-clean-pass.2026-08-04T14-36.md`; the reviewer independently re-ran stages 1, 2, and 3 and reproduced each result. The note concerns the incrementality of the mandated nullable command, detailed in gap G-3. |

### 2.1 modified-workflow-needs-green-run

`git diff --name-only ce0c91e6...HEAD` returns zero paths matching `.github/workflows/**`,
`.github/actions/**`, or `scripts/benchmarks/**`. The rule does not fire. **Not triggered.**

`scripts/feature-review/Test-ModifiedWorkflowNeedsGreenRun.ps1` does not exist in this repository, so
the trigger-path test was performed with the `git diff --name-only` path filter above.

## 3. Language-Specific Code Change Policy Compliance (C#)

Reference: `.claude/rules/csharp.md`, `CLAUDE.md` § C# Code Change Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| Strong contracts, explicit types at public boundaries | PASS | `TryGetSvgDocument`, `GetSvgDocumentOrThrow`, and `GetSvgDocument` all declare explicit parameter and return types. `var` is used only where the initializer names the type. |
| Null safety, nullable reference types enabled | PASS | `#nullable enable` is the first line of both production files. `_doc` is declared `private SvgDocument? _doc;`, so leaving it null on the degrade path is a modelled state rather than a suppressed warning. `SvgDocument?` and `Exception?` `out` parameters are correctly annotated. |
| Guard clauses for optional values | PASS | `TryGetSvgDocument` guards both arguments with `ArgumentNullException` before use; `SvgAssemblyProbe` null-checks all three inputs and documents why it does not use `IsNullOrWhiteSpace`. |
| Composition over inheritance | PASS | The new type is a `static` class with no inheritance. No type hierarchy is introduced. |
| `using` for disposables | PASS and improved | The baseline `GetSvgDocument` created `Stream stream = new MemoryStream(file);` with no `using` and never disposed it. `OpenFromBytes` wraps the same stream in `using`. This is a real improvement no acceptance criterion claims. |
| Fail fast with explicit exceptions | PASS | `GetSvgDocumentOrThrow` raises `InvalidOperationException` with the parser exception as `InnerException`. `ArgumentNullException` is raised with the correct `paramName` overload. |
| Public surface intentional and minimal, prefer `internal` | PASS | `SvgRenderer` is `internal class` (`SvgRenderer.cs:19`) and `SvgAssemblyProbe` is `internal static class`, so the `public static` members added are an assembly-internal surface reachable only from `SVGControl` and, through `[assembly: InternalsVisibleTo("SVGControl.Test")]` at `SVGControl/RelativePath.cs:19`, from the test assembly. `issue.md` AC-4 states this explicitly. `OpenFromBytes` and the seam overload of `TryGetSvgDocument` are `internal`, correctly narrower than the production entry points. |
| XML docs on non-obvious public APIs | PASS | All three new production entry points carry `<summary>` blocks that state the contract, including the `InnerException` asymmetry on the element-free path. |
| Argument-name correctness | PASS | `throw new ArgumentNullException(file == null ? nameof(file) : nameof(parse))` uses the single-string `paramName` constructor correctly and reports the actually-null argument. |
| No banned APIs introduced | PASS | No `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, or `Task.Delay` appears in the diff. |
| Analyzer configuration untouched | PASS | No `.editorconfig`, `.globalconfig`, `BannedSymbols.txt`, or `<Analyzer Include>` item is changed. |
| Backward compatibility of the tolerant API | PASS | `GetSvgDocument(byte[])` retains its null-returning contract for unparsable input. Its behavior for a null argument is also unchanged: the baseline constructed `new MemoryStream(file)` **outside** its `try`, so a null argument already raised `ArgumentNullException` rather than returning null. The reviewer verified this against `git show ce0c91e6:SVGControl/SvgRenderer.cs`. A test pins the preserved behavior. |
| Architecture boundaries | PASS | `.claude/rules/architecture-boundaries.md` bans new references to `Microsoft.Office.Tools.*`, `Microsoft.Office.Interop.Outlook`, and `[ComVisible(true)]`. The diff introduces none. `SVGControl.Test` project-references only `SVGControl`. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

Reference: `.claude/rules/csharp.md` § Testing Standards, `CLAUDE.md` § C# Unit Test Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| CUT1 MSTest framework | PASS | All three files use `Microsoft.VisualStudio.TestTools.UnitTesting` with `[TestClass]` and `[TestMethod]`. No xUnit or NUnit reference is added. |
| CUT2 Moq for mocking | PASS | `Mock<Func<byte[], SvgDocument>>` in `SvgRendererParseContractTests` drives both the null-returning branch (`.Returns((SvgDocument)null)`) and the sentinel-identity branch (`.Throws(sentinel)`). |
| CUT2 FluentAssertions for assertions | PASS | Every assertion in all three files is FluentAssertions. Zero MSTest `Assert.*` calls. |
| Seam preference order | PASS | `.claude/rules/csharp.md` prefers an interface seam, then an injectable delegate, then an adapter. A full interface for one static parse call would be excessive; the narrow `Func<byte[], SvgDocument?>` overload with a safe default (`OpenFromBytes`) is exactly option 2, and the default keeps production behavior deterministic. |
| Deterministic test rules | PASS | No network, no machine PATH dependence, no working-directory assumption, no live executable. Path inputs are literals. The tests are runnable identically from CLI and Test Explorer. |
| Repo line coverage `>= 80%` (CLAUDE.md) and `>= 85%` (rules) | PASS | 85.3844%, clearing both figures. |
| New members `>= 90%` | PASS | All seven at 100.000%. |
| No coverage regression on changed lines | PASS | Class numerator rose 264 to 424; no changed line lost coverage. |
| Named exception documented | PASS | `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgRenderer.ResolveByNameAndKey` is declared in `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` with its measured percentage, the reason (host-bound `AssemblyResolve` wiring that cannot be driven without staging a mismatched-key assembly on disk, which UT4 prohibits), and the mitigation (the decision logic was extracted to two helpers that both measure 100%). |
| No assertion weakening | PASS | The AC-5 amendment discloses that two test assertions were retargeted during `[P1-T20]` and states both retargeted assertions are strictly stronger than the originals. The reviewer confirmed the direction: `TryGetSvgDocument_WithEmptyBytes_...CapturesAnXmlException` asserts `BeOfType<XmlException>()`, which is strictly stronger than an untyped non-null check, and `GetSvgDocumentOrThrow_WithEmptyBytes_...` asserts `InnerException.Should().BeOfType<XmlException>()` rather than merely non-null. |

## 5. Test Coverage Detail

Post-change per-package line coverage, reproduced by the reviewer by summing per-`<line>` descendants
across all nine deduplicated `<package>` elements. The total matches the Cobertura root attributes
exactly (93484 / 109486).

| Package | Covered / Valid | Percent |
|---|---|---|
| `UtilitiesCS` | 68375 / 76065 | 89.890% |
| `QuickFiler` | 13993 / 17158 | 81.554% |
| `TaskMaster` | 2762 / 4244 | 65.080% |
| `SVGControl` | 1648 / 3500 | 47.086% |
| `ToDoModel` | 2032 / 3442 | 59.035% |
| `TaskVisualization` | 2736 / 3012 | 90.837% |
| `Tags` | 1374 / 1480 | 92.838% |
| `TaskTree` | 556 / 577 | 96.360% |
| `VBFunctions` | 8 / 8 | 100.000% |

`SVGControl` package movement: 1412 / 3266 = 43.2333% at baseline to 1648 / 3500 = 47.0857%
post-change, **+3.85 points**, with 236 newly covered lines against 234 newly measured lines. Branch:
460 / 1140 = 40.3509% to 544 / 1236 = 44.0129%, **+3.66 points**.

The two changed production files:

| File | Covered / Valid | Percent | Floor | Verdict |
|---|---|---|---|---|
| `SVGControl/SvgAssemblyProbe.cs` (new) | 68 / 68 | 100.000% | 85% line, 90% new code | PASS |
| `SVGControl/SvgRenderer.cs` (modified) | 424 / 588 | 72.109% | 85% line | FAIL, dispositioned non-blocking as gap G-1 |

Residual uncovered members inside `SVGControl/SvgRenderer.cs`, all pre-existing and none touched by
this change:

| Member | Covered / Valid | Note |
|---|---|---|
| `PublicKeyTokensEqual(byte[], byte[])` | 0 / 15 | pure, `private static`, never invoked by any test |
| `AddMargins(int, int)` | 0 / 15 | unreferenced helper, pre-existing |
| `.ctor(SvgDocument, Size, AutoSize)` | 0 / 8 | pre-existing |
| `.ctor(SvgDocument, Size, Padding, AutoSize)` | 0 / 8 | pre-existing |
| `get_Margin()` | 0 / 1 | pre-existing |
| `Render()` | 18 / 26 | pre-existing partial |
| `AdjustSizeProportionately(Size, Size)` | 22 / 23 | pre-existing partial |
| `ResolveByNameAndKey(object, ResolveEventArgs)` | 47 / 69 | named exception applies to the new strategy-3 wiring |
| `.ctor(byte[], Size, AutoSize)` | 13 / 17 | changed; the four uncovered lines are its success branch, see finding CR-5 |

The dominant drag on the `SVGControl` package rate is pre-existing untested code in other files of
the same assembly, unchanged by this branch and measured at 0.000% both before and after:
`DropDownEditor` 0/99, `SVGParser` 0/122, `ToggleSwitch` 0/62 plus 0/23 designer,
`SvgFileNameEditor` 0/104, and three converters at 0/48, 0/48, and 0/26.

## 6. Test Execution Metrics

| Metric | Baseline `2026-08-04T21-04` | Post-change | Delta |
|---|---|---|---|
| Test assemblies discovered | 9 | 9 | 0 |
| Total tests | 6112 | 6140 | +28 |
| Passed | 6112 | 6140 | +28 |
| Failed | 0 | 0 | 0 |
| Skipped | 0 | 0 | 0 |
| Wall time | not recorded | 58.2533 s | — |

The +28 is 27 tests delivered in Phase 1 plus one added by `[P2-T1]` to close the
`GetSvgDocumentOrThrow` success-return gap. The nine assemblies are the same nine as the baseline;
`SVGControl.Test` was already a solution member at that baseline, having been added by this branch's
first commit `0162567d`.

Disclosed rerun inside the test stage: the first invocation of the coverage-enabled test command
aborted with `Test host process crashed` after 1266 passing tests and zero reported failures, inside
`TaskVisualization.Test`. It was handled as environmental contention. The identical command was rerun
unchanged with no intervening file edit and returned exit 0 with 6140/6140 passing. The reviewer
accepts this disposition: no test reported `Failed`, the crash was in an assembly unrelated to the
change, the process table was verified clear before the rerun, and the recorded artifact discloses the
event rather than concealing it. Source: `evidence/qa-gates/test-coverage.2026-08-04T14-36.md` lines
69-85.

## 7. Code Quality Checks

Stages 1, 2, and 3 were re-executed independently by the reviewer. Stage 4 was verified from the
executor artifact plus an independent re-parse of the coverage report it produced.

| Stage | Command | Executor result | Reviewer independent result | Verdict |
|---|---|---|---|---|
| 1 Format | `dotnet tool run csharpier check .` | exit 0, 0 files need formatting | exit 0, `Checked 1466 files in 4405ms`, 0 need formatting | PASS |
| 2 Lint / analyzers | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` | exit 0, 0 errors, 6 warnings | exit 0, 0 errors, 6 warnings, elapsed 11.14 s with real recompilation; identical code set (2 `CS2002` occurrences of one pre-existing duplicate `<Compile>` in `UtilitiesCS.Test.csproj`, 4 code-less `System.Reactive.PackagesConfigCheck.targets` warnings) | PASS |
| 3 Type-check / nullable | `pwsh ... -EnableNullable -TreatWarningsAsErrors` | exit 0, 0 errors, 5 warnings | exit 0, 0 errors, 5 warnings, but elapsed 1.70 s with 0 `CoreCompile` targets, so vacuous | PASS as the mandated gate, PARTIAL as evidence of nullable cleanliness; see gap G-3 |
| 3a Type-check, forced recompile of changed production project | `MSBuild.exe SVGControl\SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true` | not run by executor at project scope | **exit 0, 0 errors, 0 warnings** — a genuine recompilation of the changed production code under the strictest property set | PASS |
| 3b Type-check, forced recompile of changed test project, project-native language version | `MSBuild.exe SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0, 0 errors, 0 warnings | **exit 0, 0 errors, 0 warnings** | PASS |
| 3c Type-check, forced recompile of changed test project, mandated nullable property | `MSBuild.exe SVGControl.Test\SVGControl.Test.csproj /t:Rebuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | disclosed as 1 `CS8630` in the supplementary baseline inventory | **exit 1, `CS8630: Invalid 'nullable' value: 'Enable' for C# 7.3`** | FAIL at forced-recompile scope; see gap G-3 and finding CR-1 |
| 4 Test | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` | exit 0, 6140/6140 passed, 0 failed | not re-executed; the coverage report it wrote was independently re-parsed and every claimed figure reproduced | PASS |
| Toolchain loop integrity | one consecutive pass, no restart | `Pass number: 1`, 0 files reformatted, no non-zero exit in stages 1-4 | corroborated: the working tree is clean, `csharpier check` is clean at head, and no `.cs` file mtime postdates the recorded pass | PASS |

Toolchain order followed matches `CLAUDE.md` § C# Toolchain: format, analyze, type-check, test.

## 8. Gaps and Exceptions

### G-1 — Modified-file line coverage below the 85% floor (FAIL, non-blocking)

`SVGControl/SvgRenderer.cs` measures **424 / 588 = 72.109%** line coverage against the 85%
modified-file floor in `.claude/rules/quality-tiers.md` and `.claude/rules/general-unit-test.md`.
Recorded as **FAIL** because the floor is mandatory and admits no tier-specific reduction.

Dispositioned **non-blocking** on this evidence:

- The baseline for the same file was **62.559%**, already far below the floor before this change. The
  change **improved** it by +9.55 points and raised covered lines from 264 to 424.
- There is **no regression on any changed line**. Every changed member improved or held.
- The residual 164 uncovered lines are dominated by pre-existing members this bug fix did not touch,
  itemized in `## 5. Test Coverage Detail`. Bringing the file to 85% requires writing tests for
  `AddMargins`, `Render()`, the two `SvgDocument` constructor overloads, and `PublicKeyTokensEqual` —
  none of which is part of issue #418 and all of which would widen a `minor-audit` bug fix.
- The repository-wide floors, which are the gate the coverage hook enforces, both pass and both
  improved.

Remediation path, in priority order: cover `PublicKeyTokensEqual` (pure, 15 lines, directly testable
once relocated or made internal — see CR-6); cover the `SvgRenderer(byte[], Size, AutoSize)` success
branch (4 lines, one test — see CR-5). Those two alone move the file to roughly 75.7%. Closing the
remainder is a separate coverage-uplift item for the `SVGControl` assembly and should be tracked as
its own entry rather than absorbed here.

### G-2 — AC-11 undelivered (FAIL, blocking for PR readiness)

AC-11 requires executing `runbooks/verify-winforms-designer-load.runbook.md` and capturing evidence
that `UtilitiesCS/Dialogs/MyBoxViewer.cs` loads in the Visual Studio WinForms designer without a
`NullReferenceException`. The criterion is `- [ ]` unchecked in `issue.md`, and the expected evidence
path `evidence/regression-testing/designer-load-<timestamp>.md` does not exist. The directory contains
only `ac1-fail-before` and `ac1-pass-after`.

`evidence/other/ac11-runbook-handoff.2026-08-04T14-36.md` correctly documents why the step is not
automatable (it requires a live `devenv.exe` / `DesignToolsServer.exe`, and
`.claude/rules/general-unit-test.md` UT4 prohibits unit tests from depending on external processes)
and records the handoff to a human operator. The reviewer agrees the step is not automatable and that
the executor's decision to leave it unchecked rather than claim it was correct.

This is the one criterion that still requires action before the feature is complete. It is a
human-execution step, not a code defect.

Open question U-2, recorded in the research artifact and repeated in the handoff, remains genuinely
open: whether `ExCSS.dll` is present in Visual Studio's `ProjectAssemblies` shadow-copy directory
alongside `SVGControl.dll` determines whether the AC-8 directory probe can succeed in the designer
host. The runbook's step 10 captures that observation. Note that the AC-3 degrade-and-log behavior
makes the designer load succeed regardless of the bind outcome, so a failed bind would now produce a
blank icon plus a named exception in the Output window rather than a designer load failure.

### G-3 — Mandated nullable gate is vacuous, and `CS8630` is newly reachable from it (PARTIAL)

Two related facts:

1. The mandated command `msbuild TaskMaster.sln /t:Build /p:Nullable=enable
   /p:TreatWarningsAsErrors=true` returns exit 0 in this tree, but legacy non-SDK up-to-date checks
   are timestamp-based rather than property-based, so it recompiles nothing. The reviewer confirmed
   this independently: the run completed in 1.70 s and executed 0 `CoreCompile` targets. The
   executor disclosed this caveat explicitly in
   `evidence/baseline/nullable-build.2026-08-04T21-04.md` lines 39-50, which is the correct handling.
   The exit code is a true record of what the mandated command returns; it is not evidence of nullable
   cleanliness.
2. At forced-recompile scope, `SVGControl.Test` emits `CS8630: Invalid 'nullable' value: 'Enable' for
   C# 7.3`, reproduced independently by the reviewer. The executor's artifact calls this diagnostic
   "present in the baseline". That is true of the cited baseline but not of the merge-base: the
   baseline was captured at branch commit `0162567d`, which is the commit that added `SVGControl.Test`
   to `TaskMaster.sln`. Against `origin/main` @ `ce0c91e6` the project is not a solution member and the
   diagnostic is unreachable from the solution-wide gate. Relative to the resolved base, this branch
   makes it reachable.

Mitigating context, verified by the reviewer: five other test projects already in the solution
(`QuickFiler.Test`, `Tags.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`) also
declare no `<LangVersion>` and would emit the same diagnostic, but they never reach their own
`CoreCompile` because they cascade-fail from `UtilitiesCS`, which contributes 195 pre-existing
`CS86xx` errors at forced-recompile scope. `SVGControl.Test` surfaces because it project-references
only `SVGControl`. A cold solution-wide nullable build therefore already cannot pass on this
repository independently of this branch; this change adds a 196th error to an already-failing
non-mandated command.

Disposition: **PARTIAL**, non-blocking for merge, with a one-line fix recorded as finding CR-1. See
that finding for the recommendation.

### G-4 — Test-file location diverges from the mirrored-`tests/` rule (accepted, pre-existing)

`.claude/rules/general-unit-test.md` § Test File Location requires tests in a `tests/` tree mirroring
production source, and states that colocation is not permitted. This repository uses sibling test
projects (`SVGControl.Test/`, `UtilitiesCS.Test/`, and seven more) for all nine of its test
assemblies. The new files follow the repository's actual convention. Recorded as a pre-existing
repository-wide convention divergence rather than a defect of this branch; changing it here would
diverge from every sibling and is not in the remit of a `minor-audit` bug fix.

### G-5 — Template resolution assumption (documented)

`.claude/skills/policy-audit-template-usage/SKILL.md` and `.claude/skills/feature-review-workflow/SKILL.md`
require resolving the three review-artifact templates through the MCP tool
`mcp__drm-copilot__resolve_policy_audit_template_asset`, and validating the results through
`mcp__drm-copilot__validate_orchestration_artifacts`. No MCP tool is present in this session's tool
surface. Rather than emit a `BLOCKED` stub, this artifact reproduces the canonical major section set
that `policy-audit-template-usage` § Required Steps enumerates in prose (`## Executive Summary`,
`## 1` through `## 10`, `## Appendix A`, `## Appendix B`), because that prose fully specifies the
required structure. Validation was performed against the deterministic gate that does exist in this
repository, `.claude/hooks/validate-feature-review-coverage.ps1`, by dot-sourcing it and running its
`Get-ChangedLanguageSet`, `Get-LanguageRepoCoverage`, and `Get-LanguageBranchCoverage` functions
against this artifact's inputs. Assumption documented; no requirement was silently skipped.

### G-6 — Layout conflict between two skills (documented, resolved in favor of the enforced gate)

`.claude/skills/remediation-handoff-atomic-planner/SKILL.md` specifies a folder-per-cycle artifact
layout (`audit/<ts>/policy-audit.md`, `remediation/<ts>/remediation-inputs.md`). The
`validate-feature-review-coverage.ps1` hook requires the flat, timestamp-suffixed form
`docs/features/active/<slug>/policy-audit.<timestamp>.md` (regex at lines 107-118) and requires the
remediation-inputs artifact to share the policy audit's folder and timestamp. The two are mutually
exclusive. This audit uses the flat form, which is what the enforced gate and the reviewer contract
both require. The conflict is recorded here so it can be resolved in the skill documents rather than
rediscovered each cycle.

### G-7 — Reviewer side effect, remediated in-session (disclosed)

To obtain a non-vacuous type-check result the reviewer ran `/t:Rebuild` against three projects,
including one (`Tags.Test`) whose rebuild cascaded into `UtilitiesCS` and aborted on that project's
pre-existing nullable debt, leaving build outputs partially stale. The reviewer then re-ran the
mandated solution analyzer build, which returned exit 0 with 0 errors and 6 warnings in 11.14 s and
restored a consistent build state. No source file, test file, project file, or policy document was
modified by the reviewer. The only file the reviewer wrote outside this audit's own four artifacts is
`artifacts/pr_context.summary.txt`, annotated as described in `## PR-Context Artifact Corrections`.

## 9. Summary of Changes

Production behavior, `SVGControl/SvgRenderer.cs`:

1. `GetSvgDocument(byte[])`'s `catch (Exception) { return null; }` is gone. Parsing now goes through
   `internal static SvgDocument? OpenFromBytes(byte[])`, which has no handler of its own, and every
   failure is funnelled through one boundary, `TryGetSvgDocument`.
2. `TryGetSvgDocument` logs each failure through both `logger.Error` and `Trace.TraceError`, returns
   `false`, and hands the caught exception back in an `out Exception?` the caller must inspect. The
   dual channel is required by AC-3 because no `log4net` appender is known to be configured inside
   `devenv.exe`.
3. Both byte-array constructors now branch on `TryGetSvgDocument` instead of dereferencing a swallowed
   null. On failure they log a constructor-scoped record through both channels, leave `_doc` null, and
   set `_original = Size.Empty`. Neither throws and neither contains an unguarded `_doc.Draw()`.
4. Two new explicit-failure entry points: `TryGetSvgDocument(byte[], out SvgDocument?, out Exception?)`
   and `GetSvgDocumentOrThrow(byte[])`, whose `InvalidOperationException.InnerException` is the
   original parser exception. `GetSvgDocument(byte[])` keeps its tolerant null-returning contract.
5. The `AssemblyResolve` fallback gains strategy 3: probe the ordered candidate directories derived
   from the loaded `SVGControl` assembly and `Assembly.LoadFrom` a same-key file found there. The
   re-entrance guard still encloses strategies 2 and 3, the public-key-token match is still required
   on every returned assembly, and the method still ends `return null;`. The previous blanket
   `catch { }` is replaced by a narrower `catch (Exception ex)` around strategy 2 and another around
   each `LoadFrom`, both logging through `Trace`.
6. `SvgAssemblyProbe` is new: two pure static helpers that convert a `file://` code base to a
   directory and build the ordered, case-insensitively de-duplicated candidate list, both tolerant of
   null, empty, whitespace, and unparsable input.

Build and configuration: `SVGControl.Test` becomes a `TaskMaster.sln` member; the test project gains
three `<Compile>` items and a direct `Svg 3.4.8` reference; its ExCSS binding redirect moves from
`4.2.4.0`, a version present nowhere in the repository, to `4.3.2.0`, which matches both
`packages/ExCSS.4.3.2/` and `SVGControl/app.config`.

Tests: 28 new MSTest tests across three files — 14 parse-contract, 5 null-tolerance, 9 probe-directory.

Deferrals: two out-of-band defects discovered during the work were written to
`docs/features/potential/` instead of being fixed in-branch.

## 10. Compliance Verdict

| Area | Verdict |
|---|---|
| 1. General Unit Test Policy | PARTIAL — all qualitative requirements PASS; modified-file coverage floor FAIL (G-1) |
| 2. General Code Change Policy | PASS |
| 3. C# Code Change Policy | PASS |
| 4. C# Unit Test Policy | PASS |
| 5. Test Coverage Detail | PARTIAL — repository-wide and new-code gates PASS; modified-file gate FAIL (G-1) |
| 6. Test Execution Metrics | PASS |
| 7. Code Quality Checks | PASS as mandated, PARTIAL as evidence (G-3) |
| Evidence Location Compliance | PASS |
| modified-workflow-needs-green-run | Not triggered |
| Acceptance criteria | PARTIAL — 10 of 11 delivered; AC-11 FAIL (G-2) |

**Overall: PARTIAL. Remediation is required.**

Remediation is triggered by G-1 (a mandatory-floor FAIL), G-2 (an unmet acceptance criterion), and
G-3 (a material PARTIAL). Remediation inputs are enumerated in
`remediation-inputs.2026-08-04T20-25.md` in this folder.

Go / no-go for PR: **conditional go**. The code change itself is sound, fully evidenced, and clears
every repository-wide gate. The blocking item is administrative rather than technical — AC-11's
human designer-load verification has not been performed, and that verification is the entire point of
the bug report. G-1 and G-3 are pre-existing-debt items with concrete, small remediation paths and do
not warrant blocking the merge on their own.

## Appendix A: Test Inventory

28 tests added, all in `SVGControl.Test`, all passing.

`SvgRendererParseContractTests.cs`, 14 tests:

| Test | Contract |
|---|---|
| `Constructor_WithMalformedBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull` | AC-1, AC-3 regression, 3-arg overload |
| `Constructor_WithMalformedBytesAndMargin_DoesNotThrowAndLeavesDocumentNull` | AC-1, AC-3 regression, 4-arg overload |
| `Constructor_WithEmptyBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull` | AC-1, AC-3 regression, empty payload, 3-arg |
| `Constructor_WithEmptyBytesAndMargin_DoesNotThrowAndLeavesDocumentNull` | AC-1, AC-3 regression, empty payload, 4-arg |
| `GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument` | success path of the tolerant parse |
| `GetSvgDocument_WithNullPayload_ThrowsArgumentNullException` | preserved argument-boundary behavior |
| `TryGetSvgDocument_WithNullPayload_ThrowsArgumentNullException` | argument boundary on the new API |
| `TryGetSvgDocument_WithMalformedBytes_ReturnsFalseAndCapturesTheException` | AC-2, AC-4 exception surfacing |
| `TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException` | typed failure shape for empty input |
| `TryGetSvgDocument_WhenTheParseSeamReturnsNull_ReturnsFalseWithNoCapturedError` | element-free path via the Moq seam |
| `GetSvgDocumentOrThrow_WithMalformedBytes_ThrowsWithTheParserExceptionInner` | AC-4 fail-fast API |
| `GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner` | AC-4 typed inner exception |
| `GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument` | AC-5 gap closure, success return |
| `TryGetSvgDocument_WithInjectedParseSeam_SurfacesTheSameExceptionInstance` | exception-instance identity |

`SvgRendererNullToleranceTests.cs`, 5 tests:

| Test | Contract |
|---|---|
| `DocumentSetter_AssignedNull_SucceedsAndLeavesDocumentNull` | AC-4 tolerant setter |
| `Render_WithNullDocument_ReturnsNull` | AC-4 tolerant render |
| `SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull` | AC-4 and AC-7 corroboration, ExCSS bind in the test host |
| `DefaultImageConstructor_DoesNotThrow` | AC-3 designer-host construction path |
| `UseDefaultImageSetterToFalse_DoesNotThrowAndRecordsTheNewValue` | AC-4 tolerant setter |

`SvgAssemblyProbeDirectoryTests.cs`, 9 tests:

| Test | Contract |
|---|---|
| `TryGetDirectoryFromCodeBase_WithAValidFileUri_ReturnsTheContainingDirectory` | AC-8 happy path |
| `TryGetDirectoryFromCodeBase_WithNull_ReturnsNull` | AC-8 null tolerance |
| `TryGetDirectoryFromCodeBase_WithEmptyString_ReturnsNull` | AC-8 empty tolerance |
| `TryGetDirectoryFromCodeBase_WithWhitespaceOnly_ReturnsNull` | AC-8 whitespace tolerance |
| `TryGetDirectoryFromCodeBase_WithANonUriString_ReturnsNullWithoutThrowing` | AC-8 never raises in a resolve handler |
| `GetProbeDirectories_WithAllThreeInputsPopulated_PreservesTheStatedOrder` | AC-8 documented precedence |
| `GetProbeDirectories_WithAnEmptyAssemblyLocation_SkipsThatCandidate` | AC-8 empty-`Location` requirement |
| `GetProbeDirectories_WithDirectoriesDifferingOnlyByCase_DeduplicatesThem` | AC-8 case-insensitive de-duplication |
| `GetProbeDirectories_WithAllInputsNull_ReturnsAnEmptyListWithoutThrowing` | AC-8 empty-list edge case |

## Appendix B: Toolchain Commands Reference

Commands the reviewer executed, all check-only or read-only except the disclosed `/t:Rebuild`
invocations in G-7:

```
git rev-parse HEAD
git merge-base HEAD origin/main
git status --porcelain
git diff --numstat ce0c91e686bf7e060aaab6f185ee6883269e4fd4...HEAD
git diff ce0c91e6...HEAD -- SVGControl/SvgRenderer.cs SVGControl/SvgAssemblyProbe.cs
git show ce0c91e6:SVGControl/SvgRenderer.cs

dotnet tool run csharpier check .

pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild

pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors

MSBuild.exe SVGControl\SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true
MSBuild.exe SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true
MSBuild.exe SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

Repository-mandated toolchain reference, per `CLAUDE.md` § C# Toolchain:

```
dotnet tool run csharpier .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage
```

Coverage inspection, read-only:

```
python -c "xml.etree parse of coverage/coverage.cobertura.xml, root and per-package and per-member aggregation"
cat artifacts/csharp/coverage.xml
pwsh -Command ". .\.claude\hooks\validate-feature-review-coverage.ps1; Get-ChangedLanguageSet; Get-LanguageRepoCoverage; Get-LanguageBranchCoverage"
```
