# Policy Audit — svg-renderer-null-document-nre (Issue #418)

- Artifact timestamp: `2026-08-05T00-04`
- Reviewer: feature-review agent
- Review cycle: reaudit 3 (remediation cycle 2 verification)
- Prior artifact sets: `2026-08-04T20-25` (cycle 1), `2026-08-04T22-28` (cycle 2)

## Baseline Resolution

| Item | Value |
|---|---|
| Base branch (requested) | `main` |
| Base ref (resolved) | `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Merge-base SHA | `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Head ref | `bug/svg-renderer-null-document-nre-418` @ `69e675d014d001b2e17ee15c3279ce6a5ba46609` |
| Merge-base recomputed by reviewer | `git merge-base HEAD origin/main` returned `ce0c91e6...`, matching the supplied value |
| Head recomputed by reviewer | `git rev-parse HEAD` returned `69e675d0...`, matching the supplied value and the PR-context summary |
| Working tree | clean (`git status --porcelain` empty at review start) |
| Active feature folder | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418` |
| Work mode marker | `- Work Mode: minor-audit` (read from `issue.md` line 12) |
| Acceptance-criteria source | `issue.md`, section `## Acceptance Criteria` |
| Commits in range | 12 |

The supplied base and head were both independently recomputed rather than trusted, per the
stale-merge-base failure mode recorded in reviewer memory. Both matched.

## Executive Summary

Verdict: **PARTIAL**. Blocking findings: **1**.

The blocking count fell from **2 to 1**. The cycle-2 blocker **G-8** — six tests in `SVGControl.Test`
producing different outcomes depending on `vstest.console.exe` argument order — is **CLOSED**, and the
closure was verified by the reviewer's own independent test run rather than by reading the executor's
evidence. The single remaining blocker, **G-2**, is AC-11, which requires a human to open a form in the
Visual Studio WinForms designer. No agent can execute it.

What changed since cycle 2:

1. **G-8 closed (blocking → resolved).** Commit `69e675d0` added an `ExCSS` `<Reference>` and the
   matching `packages.config` entry to `SVGControl.Test`, plus `<Private>True</Private>` on the
   pre-existing `Svg` reference. The reviewer independently ran
   `vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll` at this head and observed
   **75 total, 75 passed, 0 failed, `EXIT_CODE: 0`**, against the 75/69/**6** recorded before the fix.
   The order-dependence that violated UT1 Independence and Determinism is gone.
2. **G-2 unchanged (blocking, carried forward).** AC-11 remains `- [ ]`. It is registered in
   `artifacts/orchestration/orchestrator-state.json` as `human_interaction` requirements H-1 and H-2,
   both with `response: "exception"` and a `runbook_path` that resolves to an existing runbook. The
   reviewer verified that block directly rather than accepting the assertion.
3. **G-1 and G-9 unchanged (FAIL, non-blocking).** Both file-level coverage floors carry forward at
   byte-identical figures. The executor recorded that the anticipated `SVGControl` coverage improvement
   **did not materialize** rather than claiming a gain — an accurate negative report, which the reviewer
   confirmed against the regenerated Cobertura.
4. **The `Fizzler` reference this reviewer's cycle-2 remediation inputs directed was correctly
   refused.** The reviewer's own inputs asserted parity with "the eight sibling test projects." That
   justification is false on disk, and the reviewer verified the refutation independently: zero test
   projects reference `Fizzler`, no `Fizzler.dll` exists in any test output, and the on-disk package is
   `Fizzler 1.3.1` while `SVGControl.Test/app.config` redirects to `1.3.0.0`. Adding the assembly would
   have activated a stale redirect that is inert today only because the file is absent. This is recorded
   below as a correction to a reviewer-authored artifact, not as a defect in the branch.

Repository-wide C# coverage passes both mandatory floors. The toolchain passes. Two file-level coverage
floors remain unmet and are dispositioned non-blocking with reasons stated, one of which (G-9) is
explicitly surfaced for a maintainer decision.

## Rejected Scope Narrowing

**None detected.** The caller prompt contains no instruction that narrows the audit scope. The caller
stated the opposite, verbatim:

> Determine scope yourself from the branch diff per the SKILL contract; do not narrow it to the
> remediation delta.

and, regarding its six factual notes:

> None constrains your scope or findings.

The reviewer nonetheless derived scope independently from
`git diff --numstat ce0c91e686bf7e060aaab6f185ee6883269e4fd4..69e675d014d001b2e17ee15c3279ce6a5ba46609`,
covering all 152 changed files, not the 2-file remediation delta. Every one of the caller's six factual
notes was independently re-verified before being relied upon; none was accepted on assertion. The audit
scope is the full branch-vs-base diff.

## Evidence Location Compliance

Scanned the branch diff for files written under non-canonical evidence roots:

```
git diff --name-only ce0c91e6..HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
```

**Zero matches.** All 76 feature evidence artifacts are written under the canonical
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/<kind>/` path, using the
`baseline/`, `qa-gates/`, `regression-testing/`, `remediation-baseline/`, `issue-updates/`, and `other/`
kinds. Verdict: **PASS**.

`scripts/dev_tools/validate_evidence_locations.py` does not exist in this repository; the scan above is
the substitute, and the absence of that script is recorded under G-5.

## Change Inventory (feature-vs-base)

152 files changed. Language classification derived from the diff, not from the PR-context summary:

| Category | Files | Notes |
|---|---|---|
| C# source (`.cs`) | 6 | 3 production, 3 test |
| Build configuration (`.csproj`, `.config`, `.sln`) | 5 | 2 csproj, 2 config, 1 sln |
| Markdown (`.md`) | 141 | feature docs, evidence, agent memory, potential-feature entries |
| TypeScript / Python / PowerShell | 0 | none |

C# and build-configuration files, with line deltas:

| File | Status | +/- |
|---|---|---|
| `SVGControl/SvgRenderer.cs` | modified | +115 / −107 |
| `SVGControl/SvgAssemblyResolver.cs` | new | +157 |
| `SVGControl/SvgAssemblyProbe.cs` | new | +93 |
| `SVGControl.Test/SvgRendererParseContractTests.cs` | new | +358 |
| `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs` | new | +347 |
| `SVGControl.Test/SvgRendererNullToleranceTests.cs` | new | +144 |
| `SVGControl.Test/SVGControl.Test.csproj` | modified | +12 |
| `SVGControl/SVGControl.csproj` | modified | +2 |
| `SVGControl.Test/packages.config` | modified | +2 |
| `SVGControl.Test/app.config` | modified | +1 / −1 |
| `TaskMaster.sln` | modified | +14 |

Delta attributable to the functional remediation commit `69e675d0`: `SVGControl.Test.csproj` +5 and
`SVGControl.Test/packages.config` +1. The remaining five commits in the range are documentation.

## PR-Context Artifact Corrections

The PR-context summary at `artifacts/pr_context.summary.txt` was regenerated at this head and is not
stale — its recorded head ref matches `git rev-parse HEAD`. It is, however, **factually wrong** in its
language classification, for the third consecutive cycle on this feature and consistently with a defect
this reviewer has recorded across at least a dozen prior features.

| Field | Generator output | Measured truth |
|---|---|---|
| `Core logic changes` | `0 files` | 11 files (6 `.cs`, 2 `.csproj`, 2 `.config`, 1 `.sln`) |
| `Docs/templates/agents/tooling` | `104 files` | 141 `.md` files |

This misclassification is not cosmetic. The SubagentStop hook
`.claude/hooks/validate-feature-review-coverage.ps1` derives its changed-language set by regex-matching
`- <path> (+N/-N)` lines in that overview section (function `Get-ChangedLanguageSet`, line 127). With
every `.cs` file filed under a docs heading and omitted from the truncated top-10 listing, the hook
enumerates **zero** languages and silently skips all per-language coverage enforcement. A summary defect
therefore disables the coverage gate.

**Reviewer action:** the overview section was corrected in place, enumerating all 11 C# and
build-configuration paths in the generator's own `- <path> (+N/-N)` format, annotated with the
correction and the `git diff --numstat` command that produced it. This is a correction to a review input
artifact, not to source code or a policy document. Recorded as a disclosed reviewer side effect under
G-6.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| UT1 Independence | **PASS** | Closed this cycle. Reviewer-run standalone `SVGControl.Test.dll` returns 75/75/0. Tests no longer depend on command-line position. Previously FAIL under G-8. |
| UT1 Isolation | **PASS** | Each test targets one member; failures name the unit. |
| UT1 Fast execution | **PASS** | Reviewer-measured standalone run: 1.3090 s for 75 tests. |
| UT1 Determinism | **PASS** | Closed this cycle. Same binary, both argument orders, identical outcome. Previously FAIL under G-8. |
| UT1 Readability | **PASS** | Descriptive `Member_Condition_Expectation` names throughout. |
| UT2 Scenario completeness | **PASS** | Positive, negative, boundary, and error paths covered for the changed members; see section 5. |
| UT3 Arrange-Act-Assert | **PASS** | All three new test files follow AAA with commented sections. |
| UT4 No external dependencies | **PASS** | No network, database, or external process. Parse seam injected via `Func<byte[], SvgDocument?>` with Moq. |
| UT4 No temporary files | **PASS** | Zero temporary-file creation in the three new test files. |
| UT4 No mutable global state | **PASS** | Closed this cycle. The tests no longer depend on the host's ambient assembly-probing path, because `ExCSS.dll` is now deployed to the test output. Previously FAIL under G-8. |
| Test file location | **accepted deviation** | Test files sit beside the project rather than in a mirrored `tests/` tree. Pre-existing repository-wide convention. See G-4. |

### 1.2 Coverage Verification

Coverage was verified by inspecting the pre-existing artifacts produced during execution. Coverage
generation was **not** re-run, per the SKILL contract.

| Artifact | Present | Notes |
|---|---|---|
| `artifacts/csharp/coverage.xml` | yes | JaCoCo, regenerated at this head; exactly one `LINE` and one `BRANCH` counter |
| `coverage/coverage.cobertura.xml` | yes | Cobertura source, `timestamp="1785901758"`, 10,269,980 bytes, generated at this head |
| `coverage/lcov.info` (TypeScript) | absent | zero `.ts`/`.tsx` files changed on this branch, so no obligation attaches |
| `artifacts/python/lcov.info` (Python) | absent | zero `.py` files changed on this branch, so no obligation attaches |
| `artifacts/pester/powershell-coverage.xml` (PowerShell) | absent | zero `.ps1`/`.psm1` files changed on this branch, so no obligation attaches |

The reviewer parsed both artifacts independently. The Cobertura root declares
`lines-covered="93529" lines-valid="109518" branches-covered="21576" branches-valid="27418"`, and the
converted JaCoCo declares `<counter type="LINE" missed="15989" covered="93529"/>` and
`<counter type="BRANCH" missed="5842" covered="21576"/>`. 93529 + 15989 = 109518 and
21576 + 5842 = 27418, so the conversion is arithmetically faithful and carries exactly one counter per
type, which is what the hook's summing parser requires.

#### 1.2.1 Per-language coverage rows

- **C# (`SVGControl`, `SVGControl.Test`) — coverage verdict: FAIL; repository-wide line coverage
  85.4006% PASS and branch coverage 78.6928% PASS, with two file-level floors not met.**
  Baseline: line 93539/109518 = 85.4097% and branch 21584/27418 = 78.7220% at the cycle-2 head.
  Post-change: repository-wide line **85.4006%** (93529/109518) and branch **78.6928%**
  (21576/27418); both clear the mandatory floors of 85% line and 75% branch.
  Change: line −0.0091 points and branch −0.0292 points, a movement confined to `UtilitiesCS` and
  `QuickFiler` and amounting to 10 covered lines out of a 109,518-line denominator; every `SVGControl`
  package and class figure is byte-identical because this cycle modified no `.cs` file.
  New/changed-code coverage: **61.6279%** on the new file `SVGControl/SvgAssemblyResolver.cs`, which is
  the lowest measured value among changed files and is the figure this row reports.
  Disposition: FAIL on two file-level floors — the new file `SVGControl/SvgAssemblyResolver.cs` at
  61.6279% line and 53.8462% branch (see G-9) and the modified file `SVGControl/SvgRenderer.cs` at
  80.1932% line (see G-1). Both are dispositioned non-blocking with reasons recorded in section 8; the
  repository-wide gate passes and no changed line regressed.
  Evidence: reviewer re-parse of `coverage/coverage.cobertura.xml` and `artifacts/csharp/coverage.xml`,
  plus `evidence/qa-gates/coverage-delta.2026-08-05T05-00.md`.
- **TypeScript — verdict: not required.** Zero `.ts`/`.tsx` files in the branch diff, so no TypeScript
  coverage obligation attaches to this branch.
- **Python — verdict: not required.** Zero `.py` files in the branch diff, so no Python coverage
  obligation attaches to this branch.
- **PowerShell — verdict: not required.** Zero `.ps1`/`.psm1` files in the branch diff, so no Pester
  coverage obligation attaches to this branch.

#### 1.2.2 File-level coverage against the uniform tier rule

Thresholds per `.claude/rules/quality-tiers.md` Authoritative Decision #2: line >= 85%, branch >= 75%,
uniform across T1-T4. New files additionally carry the >= 90% new-module line threshold from
`.claude/rules/csharp.md`.

| File | Status | Line | Branch | Floor met |
|---|---|---|---|---|
| `SVGControl/SvgAssemblyProbe.cs` | new | 102/102 = 100.0000% | 92/92 = 100.0000% | yes |
| `SVGControl/SvgAssemblyResolver.cs` | new | 106/172 = 61.6279% | 28/52 = 53.8462% | **no** (G-9) |
| `SVGControl/SvgRenderer.cs` | modified | 332/414 = 80.1932% | 64/84 = 76.1905% | line **no** (G-1); branch yes |

Repository-wide: line 93529/109518 = 85.4006% (floor 85%, met); branch 21576/27418 = 78.6928%
(floor 75%, met).

All six `SVGControl` class figures are byte-identical to the cycle-2 measurement. This is the expected
result, and the executor reported it as such rather than claiming the improvement the remediation plan
had anticipated. The reviewer verified the identity by re-parsing the regenerated Cobertura.

#### 1.2.3 No regression on changed lines

No `.cs` file was modified by remediation cycle 2, so no changed line could regress within this cycle.
Across the full branch, every member this feature added or modified in `SVGControl/SvgRenderer.cs`
measures 100% line coverage: `.cctor()` 6/6, `.ctor(byte[], Size, AutoSize)` 17/17,
`.ctor(byte[], Size, Padding, AutoSize)` 18/18, `DescribeFailure(Exception)` 5/5,
`OpenFromBytes(byte[])` 5/5, `TryGetSvgDocument(byte[], Func<>, out, out)` 23/23,
`TryGetSvgDocument(byte[], out, out)` 3/3, `GetSvgDocumentOrThrow(byte[])` 6/6, and
`GetSvgDocument(byte[])` 4/4.

The 82-line residual in `SvgRenderer.cs` sits entirely in members this feature did not touch:
`.ctor(SvgDocument, Size, AutoSize)` 0/8, `.ctor(SvgDocument, Size, Padding, AutoSize)` 0/8,
`get_Margin()` 0/1, `Render()` 18/26, `AddMargins(int, int)` 0/15, and
`AdjustSizeProportionately(Size, Size)` 22/23. Had those pre-existing members been covered, the file
would measure 372/414 = 89.86%. **No changed line regressed.** PASS.

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | **PASS** | The cycle-2 fix is 6 added lines of build configuration. No abstraction introduced. |
| Reusability | **PASS** | Probe-directory logic factored into `SvgAssemblyProbe`; resolver into `SvgAssemblyResolver`. |
| Separation of concerns | **PASS** | Assembly binding separated from SVG rendering; the extracted class carries no renderer state. |
| Fail fast, no silent swallow | **PASS** | Zero bare `catch` blocks remain. All four catch sites declare `Exception ex` and log. |
| Logging pattern | **PASS** | `log4net` on the parse path; `Trace` inside the `AssemblyResolve` handler, with the re-entrancy rationale stated in-code. |
| File size <= 500 lines | **PASS** | Largest changed file is `SvgRendererParseContractTests.cs` at 358 lines. `SvgRenderer.cs` fell from 497 to 362. Measured with `awk 'END{print NR}'`. |
| Dependencies | **PASS** | No new package added. `ExCSS 4.3.2` was already restored under `packages/`; the change declares an existing dependency, it does not introduce one. |
| Public API compatibility | **PASS** | `SvgRenderer` is `internal`; the surface is assembly-internal plus `InternalsVisibleTo("SVGControl.Test")`. |

### 2.1 modified-workflow-needs-green-run

The rule fires when the branch diff touches `.github/workflows/**`, `scripts/benchmarks/**`, or
`.github/actions/**`.

```
git diff --name-only ce0c91e6..HEAD | grep -cE '^(\.github/workflows/|scripts/benchmarks/|\.github/actions/)'
0
```

**Zero matching paths. The rule does not fire.** No Blocking finding on this ground. The check was run
directly against the diff rather than read from the summary's truncated overview, per the failure mode
recorded in reviewer memory.

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | **PASS** | Reviewer ran `dotnet tool run csharpier check .`: 1467 files checked, 0 need formatting, `EXIT_CODE: 0`. |
| .NET analyzers | **PASS** | Analyzer build `EXIT_CODE: 0`, 0 errors, 5 warnings, 0 added diagnostics. One removal (`CS2002` in `UtilitiesCS.Test`) dispositioned non-regressive. |
| Nullable analysis | **PARTIAL** | The mandated solution-wide command returns 0 vacuously. Forced per-project rebuilds supply the probative evidence. See G-3. |
| Naming conventions | **PASS** | `PascalCase` types and members, `camelCase` locals, `_camelCase` private statics. |
| Null safety | **PASS** | `#nullable enable` at the head of both new files; nullable annotations on all out-parameters and returns. |
| `internal` preference | **PASS** | Both new types are `internal static`. |
| XML docs on non-obvious contracts | **PASS** | Both new types carry `<summary>` blocks stating why they exist. |
| No broad refactor | **PASS** | The extraction is confined to `SVGControl`; no unrelated project touched. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | **PASS** | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting` throughout. |
| Moq for mocking | **PASS** | Parse seam mocked via Moq in `SvgRendererParseContractTests`. |
| FluentAssertions | **PASS** | Assertions use FluentAssertions; MSTest `Assert` not used where FluentAssertions is practical. |
| Arrange-Act-Assert | **PASS** | All 75 tests follow AAA. |
| No external dependencies | **PASS** | Seam-based; no process, network, or filesystem dependency. |
| IDE / CLI parity | **PASS** | Closed this cycle. The standalone run is the Test Explorer shape and now agrees with the multi-assembly CLI run at 0 failures. Previously FAIL under G-8. |
| Deterministic, no ambient environment reliance | **PASS** | Closed this cycle. `ExCSS.dll` is deployed to the test output, so the outcome no longer depends on which assembly the host probes first. Previously FAIL under G-8. |

## 5. Test Coverage Detail

Reviewer-run standalone execution of `SVGControl.Test\bin\Debug\SVGControl.Test.dll` at this head:

```
Test Run Successful.
Total tests: 75
     Passed: 75
 Total time: 1.3090 Seconds
EXIT_CODE: 0
```

The 75 tests decompose as 18 `SvgAssemblyProbeDirectoryTests`, 5 `SvgRendererNullToleranceTests`,
the `SvgRendererParseContractTests` set, and the pre-existing `GetRelativePath` and
`RelativePathCoverage` tests. Among those the reviewer observed passing by name are the four
constructor tests that produced `NullReferenceException` before the fix
(`Constructor_WithMalformedBytesAndNoMargin_...`, `Constructor_WithMalformedBytesAndMargin_...`,
`Constructor_WithEmptyBytesAndNoMargin_...`, `Constructor_WithEmptyBytesAndMargin_...`), the
`GetSvgDocumentOrThrow_*` inner-exception assertions, the `TryGetSvgDocument_*` seam tests, and
`SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull`, which is the test that exercises the
real ExCSS bind and was among the six that failed standalone before this cycle.

## 6. Test Execution Metrics

| Run shape | Before (Phase 0) | After (this head) | Source |
|---|---|---|---|
| Standalone `SVGControl.Test.dll` | 75 total, 69 passed, **6 failed**, exit 1 | **75 / 75 / 0**, exit 0 | **reviewer-executed** |
| `SVGControl.Test` first, sibling second | 76 total, 70 passed, **6 failed**, exit 1 | 76 / 76 / 0, exit 0 | `evidence/qa-gates/order-independence.2026-08-05T05-00.md` |
| Sibling first, `SVGControl.Test` second | 76 / 76 / 0, exit 0 | not re-run — it passed before the fix, so it cannot discriminate | executor disclosure |
| Full suite, 9 assemblies | 6112 / 6112 / 0 | 6150 / 6150 / 0 | `evidence/qa-gates/test-coverage.2026-08-05T05-00.md` |

The standalone row is the discriminating shape, and it is the one the reviewer executed directly. The
before-figures were measured by the executor on binaries verified identical by SHA-256, which is the
correct control for an ordering defect: it isolates the command line as the only varying input.

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Formatting | `dotnet tool run csharpier check .` | exit 0; 1467 files checked; 0 need formatting; reviewer-executed |
| Analyzer build | `Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` | exit 0; 0 errors; 5 warnings; 0 added diagnostics |
| Nullable build as mandated | `Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` | exit 0 in 0.90 s with 0 of 18 `CoreCompile` targets executed — non-probative. See G-3. |
| Nullable build forced on `SVGControl.Test` | `MSBuild.exe SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Nullable=enable /p:TreatWarningsAsErrors=true` | exit 0; **0 diagnostics** |
| Nullable build forced on `SVGControl` | `MSBuild.exe SVGControl\SVGControl.csproj /t:Rebuild /p:Nullable=enable /p:TreatWarningsAsErrors=true` | exit 0; **0 diagnostics** |
| Standalone test execution | `vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll` | exit 0; 75 / 75 / 0; reviewer-executed |
| File size | `awk 'END{print NR}'` per changed file | all six changed C# source files under the 500-line limit |
| Reference identity parity | `grep -rn 'Reference Include="ExCSS,' --include=*.csproj .` | the added reference is byte-identical in identity to those in `SVGControl.csproj`, `UtilitiesCS.csproj`, and `QuickFiler.csproj` |

The two forced rebuilds are the decisive control for G-3, and this cycle's result is materially better
than cycle 2's. In cycle 2 the forced run exited 1 with 195 pre-existing `UtilitiesCS` nullable
diagnostics, because touching `SVGControl.cs` files invalidated the downstream `ProjectReference`. This
cycle changed no `.cs` file, so the two in-scope projects could be rebuilt in isolation, and both
returned exit 0 with zero diagnostics. That is direct, uncontaminated evidence that the in-scope
projects are nullable-clean.

## 8. Gaps and Exceptions

### G-1 — Modified-file line coverage below the 85% floor (FAIL, non-blocking, carried forward unchanged)

`SVGControl/SvgRenderer.cs` measures 332/414 = **80.1932%** line against the >= 85% uniform floor.
Branch is 64/84 = 76.1905%, which clears the >= 75% floor.

Byte-identical to cycle 2, as expected: this cycle modified no `.cs` file.

The entire 82-line shortfall sits in six members this feature never touched, enumerated in section
1.2.3. Every member the feature added or modified measures 100%. Had the untouched members been
covered, the file would measure 89.86%.

Disposition: **non-blocking.** The residual is pre-existing debt in WinForms/GDI-bound rendering code
(`Render()`, `AddMargins`, `AdjustSizeProportionately`, and the `SvgDocument`-taking constructors), it
predates this branch, and no changed line regressed. Ownership is recorded in
`docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`. Recorded as FAIL because the
file-level gate is mandatory and admits no verdict other than PASS or FAIL.

### G-2 — AC-11 undelivered (FAIL, BLOCKING, carried forward unchanged)

AC-11 requires opening `UtilitiesCS/Dialogs/MyBoxViewer.cs` in the Visual Studio WinForms designer and
confirming the form loads without a `NullReferenceException`. It remains `- [ ]` in `issue.md`.

The reviewer verified the human-interaction registration directly by reading
`artifacts/orchestration/orchestrator-state.json` rather than accepting the assertion. The
`human_interaction.requirements` array contains:

| id | response | satisfies | runbook_path resolves |
|---|---|---|---|
| H-1 | `exception` | AC-11 | yes |
| H-2 | `exception` | AC-7 | yes |

Both carry `runbook_path` = `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md`, which exists at 283 lines. The block satisfies all three
`human_interaction` invariants in `.claude/rules/orchestrator-state.md`: `requirements` is a list, both
`response` values are in the enum, and both `exception` entries carry a non-empty `runbook_path`.

Disposition: **BLOCKING and not agent-remediable.** No unattended automation surface exists for the
legacy in-process WinForms designer. This finding cannot be closed by any remediation cycle; it requires
the maintainer to execute the runbook and attach the capture. It is the sole reason this audit is
PARTIAL rather than PASS.

### G-3 — Mandated nullable gate is non-probative; mitigated by forced rebuilds (PARTIAL, improved)

The mandated solution-wide command
`msbuild TaskMaster.sln /p:Nullable=enable /p:TreatWarningsAsErrors=true` returns exit 0 in 0.90 s with
0 of 18 `CoreCompile` targets executed. An exit code from a build that compiled nothing is not evidence
of nullable cleanliness.

The executor disclosed this in its own evidence rather than presenting the exit 0 as a pass, and ran two
forced per-project rebuilds to supply probative evidence. Both returned exit 0 with zero diagnostics.

Disposition: **PARTIAL, improved, non-blocking.** The gate as written in `CLAUDE.md` is structurally
non-probative for incremental builds, which is a defect in the mandated command rather than in this
branch. The compensating evidence is adequate and, unlike cycle 2's, is uncontaminated by downstream
`UtilitiesCS` diagnostics because no `.cs` file changed. Recommend the mandated command be revised
repository-wide to force recompilation of changed projects; that is out of this feature's scope.

### G-4 — Test-file location diverges from the mirrored-`tests/` rule (accepted, pre-existing)

`.claude/rules/general-unit-test.md` requires test files to live in a `tests/` tree mirroring production
source. The three new test files sit in `SVGControl.Test/` beside the project file.

Disposition: **accepted, non-blocking.** This is the established convention for all nine test projects
in this repository. Relocating them would be a repository-wide restructuring far outside a `minor-audit`
bug fix, and would break the `packages.config`/legacy-csproj `<Compile Include>` wiring. Recorded for
visibility, not for remediation.

### G-5 — MCP template and validator assets unavailable (documented assumption)

The SKILL contract directs artifact creation through the MCP tool
`resolve_policy_audit_template_asset` with `template`, `code-review-template`, and
`feature-audit-template` selectors. No MCP tools are present in this session's tool surface.
Additionally, `scripts/dev_tools/validate_evidence_locations.py` does not exist in this repository.

Disposition: **documented assumption, non-blocking.** The three artifacts were structured to satisfy
every heading and table-header requirement the SKILL contract enumerates, carrying forward the structure
of the `2026-08-04T22-28` set which passed. The evidence-location scan was performed with a direct
`git diff | grep` in place of the missing validator. Consistent with reviewer memory recording that
several validator scripts named in shared skills do not exist in TaskMaster.

### G-6 — Reviewer side effect, disclosed

The reviewer modified `artifacts/pr_context.summary.txt`, replacing the incorrect
`Core logic changes: 0 files` overview with the measured 11-file enumeration. This is a review input
artifact, not source code or a policy document. Rationale and the exact defect are in the
`## PR-Context Artifact Corrections` section. Disclosed so the change is not mistaken for an
unattributed edit.

### G-7 — PR-context collector defects (documented, corrected in place)

Two generator defects observed at this head:

1. **Language misclassification.** All 11 C# and build-configuration files filed under
   `Docs/templates/agents/tooling`, with `Core logic changes: 0 files`. Third consecutive cycle on this
   feature.
2. **Spurious close candidates.** The `Auto-close issues (author asserted)` list contains `#AC-1`
   through `#AC-11`, `#CR-2`, and `#DE06-4337`. These are acceptance-criteria labels, a code-review
   finding label, and a fragment of the `SVGControl.Test` project GUID
   `{13AC39E6-DE06-4337-8EB0-41CE674A4C3B}` added to `TaskMaster.sln`. The extractor is matching
   `#`-prefixed tokens without validating them as issue numbers. `#419` is also listed; it is the
   already-merged package-update PR this branch was rebased onto, not an issue this branch closes.

Disposition: **non-blocking, not attributable to this branch.** Defect 1 is corrected in place; defect 2
is left as-is because it is generator output, and correcting it would mask a defect worth fixing at
source. Both belong to the PR-context collector.

### G-8 — Test order-dependence on the `vstest` command line (CLOSED this cycle)

**Cycle-2 status: FAIL, BLOCKING.** Six tests in `SVGControl.Test` produced different outcomes depending
on the assembly's position on the `vstest.console.exe` command line, failing with
`FileNotFoundException` for `ExCSS, Version=4.3.2.0` (innermost request `4.2.3.0`) when the assembly ran
alone or first, and passing when a sibling assembly ran first. This violated UT1 Independence, UT1
Determinism, UT4 no-mutable-global-state, and the C# IDE/CLI parity rule.

**Root cause, confirmed:** legacy `packages.config` projects do not flow transitive copy-local, so
`SVGControl.Test` referenced `Svg` but never `ExCSS`, and was the only one of nine test projects whose
output lacked `ExCSS.dll`. The test host probes along the first assembly's directory, so a sibling with
`ExCSS.dll` in its output masked the omission.

**Fix delivered in `69e675d0`:** one `<Reference Include="ExCSS, Version=4.3.2.0, ...">` with
`<HintPath>..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll</HintPath>` and `<Private>True</Private>`, one
`<package id="ExCSS" version="4.3.2" targetFramework="net481" />` line, and `<Private>True</Private>`
added to the pre-existing `Svg` reference.

**Reviewer verification, independent of executor evidence:**

| Check | Method | Result |
|---|---|---|
| Standalone run passes | reviewer executed `vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll` | 75 / 75 / 0, exit 0 |
| `ExCSS.dll` deployed | `ls SVGControl.Test/bin/Debug/` | present, 368,128 bytes |
| Reference identity parity | `grep -rn 'Reference Include="ExCSS,' --include=*.csproj .` | identical to `SVGControl`, `UtilitiesCS`, `QuickFiler` |
| Redirect target agrees | `SVGControl.Test/app.config` | `oldVersion="0.0.0.0-4.3.2.0" newVersion="4.3.2.0"`, matching the deployed `4.3.2.0` |

Disposition: **CLOSED.** The blocking count falls from 2 to 1.

#### G-8a — The `Fizzler` reference this reviewer directed was correctly refused

The `remediation-inputs.2026-08-04T22-28.md` produced by this reviewer directed adding a `Fizzler`
reference alongside `ExCSS`, justified as "parity with the eight sibling test projects." The executor
declined and recorded why. The reviewer has now verified the refutation independently:

| Reviewer claim | Measured truth | Command |
|---|---|---|
| Eight sibling test projects reference `Fizzler` | **Zero** test projects do. Only `SVGControl.csproj` and `UtilitiesCS.csproj`, both production. | `grep -rn "Fizzler" --include=*.csproj .` |
| Adding it creates parity | It would create **divergence** — no test project carries `Fizzler.dll` | `ls SVGControl.Test/bin/Debug/Fizzler.dll` → not found |
| The redirect is sound | `SVGControl.Test/app.config:27` redirects to `1.3.0.0`; the on-disk package is `Fizzler.1.3.1` and both production references declare `Version=1.3.1.0` | `ls -d packages/Fizzler*` |

Had the executor complied, it would have deployed a `1.3.1.0` assembly into a project whose config
redirects `Fizzler` to `1.3.0.0` — activating a stale redirect that is inert today only because the file
is absent. That is the same defect class as issue #418 itself. The stale redirect is correctly filed at
`docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`.

**This is a defect in a reviewer-authored artifact, not in the branch.** It is recorded here so the
error is not silently carried forward, and so future remediation inputs verify on-disk parity claims
before directing a change.

### G-9 — New-file coverage floor not met on `SvgAssemblyResolver.cs` (FAIL, non-blocking, maintainer decision required)

`SVGControl/SvgAssemblyResolver.cs` measures 106/172 = **61.6279%** line and 28/52 = **53.8462%** branch,
against the >= 85% line / >= 75% branch uniform floors and the >= 90% new-module line threshold.
Byte-identical to cycle 2.

The entire shortfall is one member: `ResolveByNameAndKey` at 47/80 = 58.75%. It is `private static`,
subscribed to `AppDomain.CurrentDomain.AssemblyResolve`, and invoked only by the CLR on a failed
assembly bind. It carries the plan's ratified exception:

```
COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey
```

Two facts bear on the adjudication:

1. The member was **relocated verbatim** by R-6, not authored. `SvgAssemblyResolver` is a relocation of
   existing code, so the >= 90% new-module threshold arguably does not attach. `Install()`, the only
   genuinely new member, measures 6/6 = 100%.
2. The file exists at all because of a sequencing decision: the resolver was extracted first to relieve
   `SvgRenderer.cs`, which stood at 497 of the 500-line limit before a `catch` block was added. Had the
   extraction not happened, these same 172 lines would have counted against `SvgRenderer.cs` and no
   new-file threshold would have been triggered. The coverage shortfall is therefore an artifact of
   where the line-count pressure forced the boundary, not a reduction in tested behavior.

Disposition: **non-blocking, recorded as FAIL because the file-level gate is mandatory and admits no
verdict other than PASS or FAIL.** This is **surfaced for a maintainer decision**, not routed to
remediation. The decision required is whether the ratified `COVERAGE_MEMBER_UNREACHABLE` exception, or
the COM/VSTO host-bound exemption class in `CLAUDE.md` UT2, extends to a CLR-invoked `AssemblyResolve`
handler. `.claude/rules/general-unit-test.md` prohibits excluding production files from coverage
measurement and directs refactoring instead; the counter-argument is that the handler's remaining
uncovered lines are `Assembly.Load`/`LoadFrom` failure paths that cannot be driven without a genuine
failed bind in a real AppDomain. The reviewer takes no position beyond recording that further
agent-side remediation would not change the figure without either a new host-level seam or a ratified
exemption.

## 9. Summary of Changes

The branch fixes issue #418 in three parts.

1. **Error-handling fix (AC-1 to AC-5).** `SvgRenderer.GetSvgDocument(byte[])` no longer swallows parse
   exceptions and returns `null` into an immediate dereference. `TryGetSvgDocument` returns a boolean
   with the captured exception in an out-parameter, `GetSvgDocumentOrThrow` raises with the parser
   exception as `InnerException`, and the byte-array constructors degrade to `Size.Empty` while logging
   through both `log4net` and `Trace` rather than throwing — a deliberate choice, since `PictureBoxSVG`
   is constructed by designer-generated code in eleven forms including one inside the Outlook add-in.
2. **Binding fallback (AC-7, AC-8).** The `AssemblyResolve` handler gained directory probing against the
   `SVGControl` assembly's own location, and was extracted to `SvgAssemblyResolver` with the ordered
   candidate logic in `SvgAssemblyProbe`.
3. **Test project repair (AC-9, AC-10, and this cycle's fix).** `SVGControl.Test` was added to the
   solution, its ExCSS binding redirect corrected from a nonexistent `4.2.4.0` to `4.3.2.0`, and — in
   this cycle — the missing `ExCSS` reference added so the assembly is actually deployed and the tests
   run identically in any order.

## 10. Compliance Verdict

| Policy | Verdict |
|---|---|
| General Code Change Policy | **PASS** |
| General Unit Test Policy | **PASS** (UT1 Independence and Determinism restored; G-8 closed) |
| C# Code Change Policy | **PARTIAL** (G-3, mandated nullable gate non-probative; compensating evidence adequate) |
| C# Unit Test Policy | **PASS** (IDE/CLI parity restored) |
| Coverage, repository-wide | **PASS** (line 85.4006%, branch 78.6928%) |
| Coverage, changed files | **FAIL** (G-1 modified file, G-9 new file; both dispositioned non-blocking) |
| Evidence location conventions | **PASS** |
| `modified-workflow-needs-green-run` | **not triggered** (zero matching paths) |
| Acceptance criteria | **PARTIAL** (10 of 11 met; AC-11 human-only) |

**Overall: PARTIAL. Blocking count: 1** (G-2, AC-11 undelivered, human-only).

**Change from cycle 2: the blocking count fell from 2 to 1.** G-8 is closed and verified by
reviewer-executed measurement. G-2 is unchanged and is not agent-remediable.

No agent-actionable blocking finding remains. The two open items — AC-11 and the G-9 coverage decision —
both require the maintainer. A further remediation cycle would have nothing to act on.

## Appendix A: Coverage Verification Detail

Repository-wide C#, from the root element of `coverage/coverage.cobertura.xml`:

```
line-rate="0.854006" branch-rate="0.786928"
lines-covered="93529" lines-valid="109518"
branches-covered="21576" branches-valid="27418"
timestamp="1785901758"
```

- Line: 93529 / 109518 = **85.4006%** against the 85% floor. **PASS.**
- Branch: 21576 / 27418 = **78.6928%** against the 75% floor. **PASS.**

Canonical JaCoCo artifact `artifacts/csharp/coverage.xml`:

```xml
<counter type="LINE" missed="15989" covered="93529" />
<counter type="BRANCH" missed="5842" covered="21576" />
```

Conversion validated: 93529 + 15989 = 109518 and 21576 + 5842 = 27418 both reconcile to the Cobertura
root. Exactly one counter per type, as the hook's summing parser requires.

Checklist of the four language coverage artifacts:

- TypeScript coverage artifact `coverage/lcov.info`: not required; zero `.ts`/`.tsx` files changed.
- Python coverage artifact `artifacts/python/lcov.info`: not required; zero `.py` files changed.
- PowerShell coverage artifact `artifacts/pester/powershell-coverage.xml`: not required; zero
  `.ps1`/`.psm1` files changed.
- C# coverage artifact `artifacts/csharp/coverage.xml`: present, parsed, reconciled, and reported above;
  verdict recorded in section 1.2.1.

Baseline / post-change comparison, C#: Baseline: 85.4097% line and 78.7220% branch at the cycle-2 head.
Post-change: 85.4006% repository-wide line and 78.6928% branch. Change: −0.0091 line points and −0.0292
branch points, confined to `UtilitiesCS` and `QuickFiler`; all six `SVGControl` class figures
byte-identical. Disposition: repository-wide floors met with 0.40 and 3.69 points of margin; two
file-level floors not met and dispositioned non-blocking under G-1 and G-9.

## Appendix B: Toolchain Commands Reference

Commands the reviewer executed, in the order run. All are check-only; none mutates tracked source.

```powershell
# Scope and baseline
git rev-parse HEAD
git merge-base HEAD origin/main
git status --porcelain
git diff --numstat ce0c91e686bf7e060aaab6f185ee6883269e4fd4..69e675d014d001b2e17ee15c3279ce6a5ba46609
git log --oneline ce0c91e6..HEAD
git show --stat 69e675d0

# 1. Formatting
dotnet tool run csharpier check .

# 2. Tests — the discriminating order-independence shape
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' `
    'SVGControl.Test\bin\Debug\SVGControl.Test.dll'

# 3. Coverage — inspected, not regenerated, per the SKILL contract
#    artifacts/csharp/coverage.xml parsed per counter; coverage/coverage.cobertura.xml root read
python -c "import xml.etree.ElementTree as ET; ..."   # counter aggregation

# 4. Policy scans
git diff --name-only ce0c91e6..HEAD | Select-String '^artifacts/(baselines|qa|evidence|coverage)/'
git diff --name-only ce0c91e6..HEAD | Select-String '^(\.github/workflows/|scripts/benchmarks/|\.github/actions/)'
awk 'END{print NR}' SVGControl/SvgRenderer.cs        # and each other changed C# file

# 5. Claim verification
grep -rn 'Reference Include="ExCSS,' --include=*.csproj .
grep -rn 'Fizzler' --include=*.csproj .
ls SVGControl.Test/bin/Debug/                         # ExCSS.dll present, Fizzler.dll absent
python -c "import json; json.load(open('artifacts/orchestration/orchestrator-state.json'))['human_interaction']"
```

Reviewer-executed gates: formatting and the standalone test run. Analyzer and nullable build results are
inspected from executor evidence at this head, per the SKILL contract's preference for inspecting
pre-existing artifacts over regenerating them.
