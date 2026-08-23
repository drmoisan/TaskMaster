# Remediation Inputs — svg-renderer-null-document-nre (Issue #418)

- Cycle entry timestamp: 2026-08-04T22-28 (cycle 2 entry)
- Triggered by: `policy-audit.2026-08-04T22-28.md`, `code-review.2026-08-04T22-28.md`, `feature-audit.2026-08-04T22-28.md`
- Base: `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`
- Head at audit: `bug/svg-renderer-null-document-nre-418` @ `a62391f719c6d5ecc3d80115916c95d1966ca514`
- Work mode: `minor-audit`; acceptance-criteria source is `issue.md` § `## Acceptance Criteria`
- Prior cycle: entry `2026-08-05T01-50`, exit audit `2026-08-04T20-25`, blocking count 1

## Source Audit Artifacts

| Artifact | Path |
|---|---|
| Policy audit | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/policy-audit.2026-08-04T22-28.md` |
| Code review | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/code-review.2026-08-04T22-28.md` |
| Feature audit | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/feature-audit.2026-08-04T22-28.md` |

## Trigger Basis

Remediation is required under `.claude/skills/feature-review-workflow/SKILL.md` step 8 and
`.claude/skills/remediation-handoff-atomic-planner/SKILL.md` § Trigger Conditions on three grounds:

1. An unmet acceptance criterion: AC-11 is FAIL, AC-10 is PARTIAL.
2. FAIL findings in the policy audit: G-8 (test-order dependence), plus the two mandatory file-level
   coverage floors G-1 and G-9, both dispositioned non-blocking.
3. A blocker in the code review: CR-8.

Toolchain checks did **not** fail. Format and analyzer stages return exit 0 and were independently
reproduced by the reviewer. The type-check stage returns exit 0 and the changed projects compile
clean under a forced recompile; the gate's structural vacuity is recorded as G-3(b), a repository-level
concern outside this feature's scope. The mandated 9-assembly test run is green at 6150/6150.

The `modified-workflow-needs-green-run` rule did **not** fire: the diff contains no path under
`.github/workflows/**`, `.github/actions/**`, or `scripts/benchmarks/**`.

Blocking count: **2** (R-1 carried forward, R-7 new). Items R-8 through R-12 are non-blocking.

Cycle 1 fully discharged its assignment. R-2 through R-6 are all verified delivered and all seven
actionable cycle-1 code-review findings are verified resolved. R-7 below is not a regression from
cycle 1: the condition was present at cycle 1's head `ea106111` and the reviewer did not detect it
then.

## Enumerated Fix List

### R-1 — Execute the AC-11 human designer-load runbook (BLOCKING, carried forward unchanged)

- **Source finding:** feature audit AC-11 FAIL; policy audit gap G-2.
- **Owner:** human operator. **This item cannot be delegated to an agent and must not be assigned to
  one.** It is tracked as ratified human-interaction requirements H-1 and H-2 in
  `artifacts/orchestration/orchestrator-state.json`, both `response: "exception"` with a
  `runbook_path`.
- **Files:** none edited by an agent. Evidence is written to
  `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`,
  and `issue.md:110` changes from `- [ ] **AC-11` to `- [x] **AC-11` only after that capture exists.
- **Expected behavior:** opening `UtilitiesCS/Dialogs/MyBoxViewer.cs` in the Visual Studio WinForms
  designer loads the form without a `NullReferenceException`. Per the AC-3 degrade-and-log decision the
  load should succeed whether or not the ExCSS bind succeeds; a failed bind should now produce a blank
  icon plus a named exception in the Visual Studio Output window.
- **Procedure:** `runbooks/verify-winforms-designer-load.runbook.md`, all steps. Step 10 additionally
  resolves open question U-2.
- **Verification:** the capture must record the observed outcome, the Output-window contents including
  any `SvgRenderer could not parse the SVG payload:` line with its exception type and message, and the
  U-2 observation from step 10.
- **Note for this cycle:** the reviewer's runtime observation strengthens the prior expectation. In an
  isolated test host where the ExCSS bind genuinely failed, the constructor degraded and emitted
  `SvgRenderer could not parse the SVG payload: System.IO.FileNotFoundException: Could not load file or
  assembly 'ExCSS, Version=4.3.2.0 ...'` on the `Trace` channel with no `NullReferenceException`. The
  designer host should behave the same way, so a successful load is the expected outcome.
- **Alternative disposition:** an explicit maintainer waiver recorded in the orchestrator-state
  `human_interaction` block also clears this item.

### R-7 — Add the missing `ExCSS` reference to `SVGControl.Test` (BLOCKING, new, one-line class of change)

- **Source finding:** code review CR-8 (Blocking); policy audit gap G-8; feature audit AC-10 PARTIAL.
- **Files:** `SVGControl.Test/SVGControl.Test.csproj`, `SVGControl.Test/packages.config`.
- **Current state, measured.** Six tests change outcome with `vstest.console.exe` argument order:

  | Command | Result |
  |---|---|
  | `vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll` | 75 total, 69 passed, **6 failed** |
  | `vstest.console.exe SVGControl.Test\...\SVGControl.Test.dll VBFunctions.Test\...\VBFunctions.Test.dll` | 76 total, 70 passed, **6 failed** |
  | `vstest.console.exe VBFunctions.Test\...\VBFunctions.Test.dll SVGControl.Test\...\SVGControl.Test.dll` | 76 total, **76 passed** |

  Root cause: `SVGControl.Test/bin/Debug` contains `Svg.dll` but not `ExCSS.dll` or `Fizzler.dll`. The
  project references `Svg` (added by this branch) but never `ExCSS`; `ExCSS` is a transitive dependency
  of `Svg` and legacy non-SDK `packages.config` projects do not flow transitive copy-local. The
  `app.config` redirect AC-10 corrected cannot help because redirection presupposes the file is
  findable, and the `AssemblyResolve` fallback probes that same output directory.

  The six tests: `SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull`,
  `GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument`,
  `Constructor_WithTheBuiltInDefaultImageAndNoMargin_LeavesDocumentNonNull`,
  `TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException`,
  `GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner`,
  `GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument`.

- **Expected behavior.** Add to the `<Reference>` `ItemGroup` of `SVGControl.Test.csproj`, placed
  alphabetically to match the surrounding ordering:

  ```xml
  <Reference Include="ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL">
    <HintPath>..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll</HintPath>
    <Private>True</Private>
  </Reference>
  <Reference Include="Fizzler, Version=1.3.0.0, Culture=neutral, PublicKeyToken=4ebff4844e382110, processorArchitecture=MSIL">
    <HintPath>..\packages\Fizzler.1.3.1\lib\netstandard2.0\Fizzler.dll</HintPath>
    <Private>True</Private>
  </Reference>
  ```

  and to `packages.config`, in the existing alphabetical position:

  ```xml
  <package id="ExCSS" version="4.3.2" targetFramework="net481" />
  <package id="Fizzler" version="1.3.1" targetFramework="net481" />
  ```

  Copy the `Include` identity strings from `SVGControl/SVGControl.csproj:55` (ExCSS) and its `Fizzler`
  reference rather than retyping them, so the assembly identities match exactly. `Fizzler` is included
  for parity with the eight sibling test projects; `ExCSS` is the demonstrated need.

- **Verification commands, in order:**

  ```
  dotnet tool run csharpier check .
  pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
  pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors
  vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll
  pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
  ```

  Acceptance: the standalone `vstest.console.exe` run of `SVGControl.Test` alone must report
  **75 total, 75 passed, 0 failed**. Confirm `ls SVGControl.Test/bin/Debug` now lists `ExCSS.dll` and
  `Fizzler.dll`. The 9-assembly wrapper must remain at 6150/6150 or higher with 0 failed. Then update
  the AC-10 evidence note in `issue.md` to record that the redirect's stated objective is now achieved,
  without altering the criterion text or its `[x]` state.

- **Caveat for the planner.** Adding a reference changes the assembly's binding surface. Confirm the
  added identities match what `SVGControl.Test/app.config` already redirects: ExCSS
  `0.0.0.0-4.3.2.0 → 4.3.2.0` and Fizzler `0.0.0.0-1.3.0.0 → 1.3.0.0`. Do **not** change any
  `app.config` redirect as part of this item; the stale-Fizzler-redirect defect is deliberately
  deferred to `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md` and
  must stay there.
- **Why blocking:** the condition violates three explicit policy statements — UT1 Independence and the
  mutable-global-state prohibition in `.claude/rules/general-unit-test.md`, and the IDE/CLI-parity
  requirement in `.claude/rules/csharp.md`. It also degrades the trustworthiness of the AC-1 regression
  tests that are the purpose of issue #418. Production behavior is unaffected, so a maintainer may
  reasonably waive it; it is recorded as blocking because the policy language admits no discretion.

### R-8 — Complete the `SvgAssemblyResolver` separation (non-blocking)

- **Source finding:** code review, first Low finding.
- **Files:** `SVGControl/SvgAssemblyResolver.cs`, `SVGControl/SvgAssemblyProbe.cs`,
  `SVGControl/SvgRenderer.cs`.
- **Expected behavior:** move `DescribeFailure(Exception?)` from `SvgRenderer` to `SvgAssemblyProbe`
  (or a small shared internal helper) and update the three resolver call sites plus `SvgRenderer`'s own
  uses; change `typeof(SvgRenderer).Assembly` at `SvgAssemblyResolver.cs:109` to
  `typeof(SvgAssemblyResolver).Assembly`. Both changes are behavior-preserving: `DescribeFailure` is a
  pure string formatter and both `typeof` expressions name types in the same assembly.
- **Verification:** the full mandated toolchain, plus confirmation that `DescribeFailure` retains 100%
  line coverage at its new home and that `SvgAssemblyProbe` stays at 100% line and 100% branch.
- **Rationale:** removes a mutual dependency between two types inside a CLR callback path and completes
  the separation R-6 began. Bundle with R-9, which touches the same lines.

### R-9 — Correct the resolver's diagnostic message prefixes (non-blocking)

- **Source finding:** code review, second Low finding.
- **File:** `SVGControl/SvgAssemblyResolver.cs` lines 103, 135, 146.
- **Expected behavior:** change the three message prefixes from `SvgRenderer load ...` /
  `SvgRenderer resolve ...` to `SvgAssemblyResolver load ...` / `SvgAssemblyResolver resolve ...`.
- **Verification:** the full mandated toolchain. No test asserts these strings, so no test update is
  expected; confirm that by grepping the three test files for the literal `SvgRenderer load` and
  `SvgRenderer resolve` before changing them.
- **Rationale:** AC-3 makes designer-host observability a functional requirement, so these strings are
  a diagnostic channel rather than cosmetic text. They currently name a type the code no longer lives
  in. Bundle with R-8.

### R-10 — Remove the duplicated byte-array constructor bodies (non-blocking)

- **Source finding:** code review, third Low finding.
- **File:** `SVGControl/SvgRenderer.cs` lines 30-70.
- **Expected behavior:** extract a private helper such as
  `private void InitializeFromBytes(byte[] doc, string constructorLabel)` carrying the shared
  `TryGetSvgDocument` call, the `_doc`/`_original` assignment, and the degrade-and-log block; call it
  from both constructors, passing the existing label literals.
- **Verification:** the full mandated toolchain. Both constructors currently measure 17/17 and 18/18
  line coverage, so the extracted helper must reach 100% and neither constructor may regress. The four
  `SvgRendererParseContractTests` constructor tests must pass unchanged.
- **Rationale:** `.claude/rules/general-code-change.md` lists avoiding copy-paste as a design priority.
  The duplicated failure-handling block is the kind that drifts.

### R-11 — Add `<Private>True</Private>` to the `Svg` reference (non-blocking, bundle with R-7)

- **Source finding:** code review, fifth Low finding.
- **File:** `SVGControl.Test/SVGControl.Test.csproj` lines 282-284.
- **Expected behavior:** add the `<Private>True</Private>` child to the `Svg` reference this branch
  added, matching every neighbouring `HintPath`-resolved reference in the same `ItemGroup`.
- **Verification:** the full mandated toolchain; confirm `Svg.dll` is still present in
  `SVGControl.Test/bin/Debug`.
- **Rationale:** currently harmless, since MSBuild defaults a `HintPath`-resolved reference to
  copy-local. Purely a style-consistency fix, and free while R-7 edits the same `ItemGroup`.

### R-12 — File the repository-level nullable-gate follow-up (non-blocking, documentation only)

- **Source finding:** code review, second Info finding; policy audit gap G-3(b).
- **File:** a new entry under `docs/features/potential/`.
- **Expected behavior:** record that `msbuild TaskMaster.sln /p:Nullable=enable
  /p:TreatWarningsAsErrors=true` returns exit 0 in under one second with zero `CoreCompile` targets,
  because legacy non-SDK up-to-date checks compare timestamps and not properties; that a forced
  recompile returns exit 1 with 195 pre-existing `UtilitiesCS` nullable errors; and that every
  AC-6-style "nullable build EXIT_CODE 0" claim in this repository therefore rests on nothing having
  recompiled. Name the two candidate remedies: force `CoreCompile` for in-scope projects, or replace
  the solution-wide gate with a per-changed-project gate.
- **Verification:** the entry exists and follows the `docs/features/potential/` entry format.
- **Rationale:** not attributable to this branch and not fixable within a `minor-audit` scope, but it
  limits what any C# feature review in this repository can assert about type safety, so it must be
  visible outside this audit. **Do not attempt to fix the 195 `UtilitiesCS` diagnostics in this
  feature** — that is a separate epic already in progress.

### Non-actionable, recorded only

- **G-1 — `SVGControl/SvgRenderer.cs` modified-file line coverage 80.1932% against the 85% floor.**
  FAIL, dispositioned non-blocking. Improved from 62.559% at baseline and 72.109% at cycle 1. Every
  changed or added member measures 100%; the whole 82-line residual is in six pre-existing untouched
  members. Owned by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`. **Do not target
  in this cycle.**
- **G-9 — `SVGControl/SvgAssemblyResolver.cs` new-file line coverage 61.6279% against the 85% floor and
  the 90% new-module threshold.** FAIL, dispositioned non-blocking. The entire shortfall is
  `ResolveByNameAndKey` at 47/80, which carries the ratified
  `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey` exception and was
  relocated verbatim by R-6, not authored this cycle. `Install()`, the only genuinely new member,
  measures 6/6 = 100%. Needs a maintainer decision, not code: either extend the ratified exception to
  file scope or fold the residual into the coverage-uplift follow-up that owns G-1. **Do not attempt to
  raise this by testing the CLR callback end-to-end.**
- **G-4 — test-file location.** Pre-existing repository-wide convention. Accepted, not actionable.

## Do Not Do

- Do not widen scope beyond the enumerated items. Work mode is `minor-audit`.
- Do not attempt R-1. It is a human-only item; assigning it to an agent will produce a false capture.
- Do not weaken, retarget, or delete any existing assertion to make a test pass. In particular, do not
  change the `XmlException` assertions in
  `TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException` or
  `GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner`; those assertions are correct
  and R-7 is what makes them hold unconditionally.
- Do not add `[ExcludeFromCodeCoverage]` or a `coverage.config` exclusion to address G-1 or G-9.
  `.claude/rules/general-unit-test.md` prohibits excluding production files from coverage measurement.
- Do not modify any `app.config` binding redirect. The stale `Fizzler` and `Unsafe` redirects are
  deliberately deferred to `docs/features/potential/`.
- Do not attempt to fix the 195 pre-existing `UtilitiesCS` nullable diagnostics.
- Do not modify policy documents under `.claude/rules/` or `.github/instructions/`.
- Do not alter AC text or clear an existing `[x]`. If R-7 lands, AC-10's existing `[x]` becomes
  accurate on its own; add an evidence note, do not restate the criterion.
- Do not use temporary files in tests.
- Do not report a green toolchain from a build that compiled nothing. When verifying the type-check
  stage, force a recompile of the changed projects and state that you did.

## Handoff

Per `.claude/skills/remediation-handoff-atomic-planner/SKILL.md`, the remediation plan is authored by
**`atomic-planner`**, not by `feature-review`. This artifact is the cycle-entry input to that
delegation. `feature-review` has deliberately not created a stub
`remediation-plan.2026-08-04T22-28.md`: authoring a plan outside `atomic-planner` would bypass the
`atomic-plan-contract` shape requirements and the executor preflight sub-loop, and would leave a
malformed plan artifact on disk. The orchestrator should delegate plan authorship to `atomic-planner`
with this file as input, then route the resulting plan to `atomic-executor` for preflight.

Two deviations from the handoff skill's letter are recorded deliberately:

1. **Flat artifact naming, not folder-per-cycle.** The skill specifies
   `remediation/<entry-ts>/remediation-inputs.md` and `audit/<exit-ts>/policy-audit.md`. That layout
   is incompatible with the enforced gate: `.claude/hooks/validate-feature-review-coverage.ps1`
   matches `^docs/features/active/(?<Folder>.+)/policy-audit\.(?<Timestamp>\d{4}-\d{2}-\d{2}T\d{2}-\d{2})\.md$`,
   which requires the timestamp in the filename and therefore rejects `audit/<ts>/policy-audit.md`.
   This cycle uses the flat timestamped form, consistent with cycle 1 and with the enforced gate.
2. **Validator tooling absent.** `scripts/dev_tools/validate_evidence_locations.py`,
   `scripts/feature-review/Test-ModifiedWorkflowNeedsGreenRun.ps1`, and the MCP tool
   `resolve_policy_audit_template_asset` referenced by the workflow skills do not exist in this
   repository. The equivalent checks were performed directly and are documented in the policy audit
   under Evidence Location Compliance, section 2.1, and gap G-5.

## Exit Criteria for This Cycle

The cycle exits when a reaudit reports `blocking_count == 0`. Concretely:

- R-7 delivered and `vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll` alone returns
  75/75, restoring AC-10 to PASS and closing G-8; **and**
- R-1 discharged by a human designer-load capture under `evidence/regression-testing/`, or explicitly
  waived by the maintainer in the orchestrator-state `human_interaction` block, closing G-2.

R-8 through R-12 do not gate the exit condition and may be bundled or deferred at the planner's
discretion. R-11 should be bundled with R-7 since both edit the same `ItemGroup`; R-8 and R-9 should be
bundled with each other since both edit the same lines.
