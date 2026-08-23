# Remediation Inputs — svg-renderer-null-document-nre (Issue #418)

- Cycle entry timestamp: 2026-08-04T20-25
- Triggered by: `policy-audit.2026-08-04T20-25.md`, `code-review.2026-08-04T20-25.md`, `feature-audit.2026-08-04T20-25.md`
- Base: `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`
- Head at audit: `bug/svg-renderer-null-document-nre-418` @ `ea106111a6daf7e05f8a804ac00b4a713598962a`
- Work mode: `minor-audit`; acceptance-criteria source is `issue.md` § `## Acceptance Criteria`

## Source Audit Artifacts

| Artifact | Path |
|---|---|
| Policy audit | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/policy-audit.2026-08-04T20-25.md` |
| Code review | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/code-review.2026-08-04T20-25.md` |
| Feature audit | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/feature-audit.2026-08-04T20-25.md` |

## Trigger Basis

Remediation is required under `.claude/skills/feature-review-workflow/SKILL.md` step 8 and
`.claude/skills/remediation-handoff-atomic-planner/SKILL.md` § Trigger Conditions on three grounds:

1. An unmet acceptance criterion: AC-11 is FAIL.
2. A mandatory-floor FAIL in the policy audit: gap G-1, modified-file line coverage.
3. A material PARTIAL in the policy audit: gap G-3, the vacuous nullable gate plus a newly reachable
   `CS8630`.

Toolchain checks did **not** fail. Format, analyzer, type-check, and test stages all return exit 0 and
were independently reproduced by the reviewer. The `modified-workflow-needs-green-run` rule did not
fire: the diff contains no path under `.github/workflows/**`, `.github/actions/**`, or
`scripts/benchmarks/**`.

Blocking count: **1** (R-1). Items R-2 through R-6 are non-blocking and may be bundled or deferred at
the planner's discretion, with the caveat noted under each.

## Enumerated Fix List

### R-1 — Execute the AC-11 human designer-load runbook (BLOCKING)

- **Source finding:** feature audit AC-11 FAIL; policy audit gap G-2.
- **Owner:** human operator. This item cannot be delegated to an agent.
- **Files:** none edited by an agent. Evidence is written to
  `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`,
  and `issue.md:104` is changed from `- [ ] **AC-11` to `- [x] **AC-11` only after that capture exists.
- **Expected behavior:** opening `UtilitiesCS/Dialogs/MyBoxViewer.cs` in the Visual Studio WinForms
  designer loads the form without a `NullReferenceException`. Per the AC-3 degrade-and-log decision,
  the load is expected to succeed whether or not the ExCSS bind succeeds; a failed bind should now
  produce a blank icon plus a named exception in the Visual Studio Output window rather than a designer
  load failure.
- **Procedure:** `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md`,
  all steps. Step 10 additionally resolves open question U-2 (whether `ExCSS.dll` is present in Visual
  Studio's `ProjectAssemblies` shadow-copy directory alongside `SVGControl.dll`).
- **Verification:** the capture must record the observed outcome, the Output-window contents including
  any `SvgRenderer could not parse the SVG payload:` line and its exception type and message, and the
  U-2 observation from step 10.
- **Why blocking:** reproducing the designer load is the observation the bug report opened on. Every
  other criterion is delivered; this is the one that confirms the fix in the host where the defect was
  reported.

### R-2 — Add `<LangVersion>` to `SVGControl.Test.csproj` (non-blocking, smallest and highest value)

- **Source finding:** code review CR-1 (Medium); policy audit gap G-3.
- **File:** `SVGControl.Test/SVGControl.Test.csproj`.
- **Expected behavior:** add `<LangVersion>latest</LangVersion>` to the project's first
  `<PropertyGroup>`, matching `SVGControl/SVGControl.csproj` and the sibling test projects
  `TaskMaster.Test`, `UtilitiesCS.Test`, and `VBFunctions.Test`. After the change, a forced recompile
  under the mandated nullable property set must not emit `CS8630`.
- **Verification commands:**
  ```
  MSBuild.exe SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true
  ```
  Expected: exit 0, zero `CS8630`. Then the full mandated toolchain in `CLAUDE.md` order, expecting
  exit 0 at every stage with no new diagnostics relative to
  `evidence/baseline/analyzer-build.2026-08-04T21-04.md` and
  `evidence/baseline/nullable-build.2026-08-04T21-04.md`.
- **Caveat for the planner:** raising the language version can surface previously latent nullable or
  language-version-gated diagnostics in the existing `SVGControl.Test` files, which include
  pre-existing tests this branch did not author (`GetRelativePath_Test.cs`,
  `RelativePathCoverageTests.cs`, `Form1.cs`, and the Designer/Resources files). Verify the whole
  project compiles, not only the three new files. If new diagnostics appear in files outside this
  feature's scope, stop and report rather than editing them.
- **Rationale:** one property removes this branch's only type-check diagnostic that is newly reachable
  relative to the merge-base. `SVGControl.Test` project-references only `SVGControl`, so it is the one
  `LangVersion`-less test project that reaches its own `CoreCompile` in a cold solution-wide nullable
  build; the other five cascade-fail from `UtilitiesCS` first.

### R-3 — Restore exception containment in the `AssemblyResolve` handler (non-blocking)

- **Source finding:** code review CR-2 (Medium).
- **Files:** `SVGControl/SvgRenderer.cs` (lines 84-140), `SVGControl/SvgAssemblyProbe.cs` (lines 41-52).
- **Expected behavior, both parts:**
  1. No exception escapes `ResolveByNameAndKey`. Wrap the strategy-3 body, or restore a handler on the
     outer `try` at line 84, with `catch (Exception ex) { Trace.TraceWarning(...); }`, consistent with
     the two handlers already present at lines 99 and 131. Do not use `log4net` at this site; the
     existing in-code comment states the re-entrancy reason.
  2. `GetProbeDirectories` validates all three candidates identically. Apply the same
     `IndexOfAny(Path.GetInvalidPathChars()) < 0` filter to the `baseDirectory` candidate that is
     already applied to `assemblyLocation` and, via `TryGetDirectoryFromCodeBase`, to the code-base
     candidate.
- **Verification:** add a test to `SvgAssemblyProbeDirectoryTests` supplying a `baseDirectory`
  containing an invalid path character and asserting the candidate is dropped without throwing, in the
  same style as `GetProbeDirectories_WithANonUriString_ReturnsNullWithoutThrowing`. Then the full
  mandated toolchain. `SvgAssemblyProbe` must remain at 100% line and branch coverage.
- **Rationale:** the baseline wrapped this region in `catch { }`; the head does not. `self.Location`
  and `self.CodeBase` can raise `NotSupportedException` and `Path.Combine` can raise
  `ArgumentException` for an unfiltered `baseDirectory`. An exception escaping an `AssemblyResolve`
  handler propagates to whatever triggered the bind, converting a recoverable bind failure into a hard
  construction-time failure — the same class of opaque failure issue #418 exists to eliminate. The
  documented contract of `SvgAssemblyProbe` is "Never raises, so it is safe inside an `AssemblyResolve`
  handler" (`SvgAssemblyProbe.cs:15`), and the unfiltered third candidate is inconsistent with that.

### R-4 — Raise modified-file coverage on `SVGControl/SvgRenderer.cs` (non-blocking)

- **Source finding:** policy audit gap G-1 (FAIL, dispositioned non-blocking); code review CR-5 and
  CR-6 (both Low).
- **Current state:** 424 / 588 = 72.109% line coverage against the 85% modified-file floor. Baseline
  62.559%, so this is an improvement of +9.55 points with no regression on any changed line. The
  residual gap is dominated by pre-existing members this bug fix did not touch.
- **Files:** `SVGControl.Test/SvgRendererParseContractTests.cs` (or a new test file),
  `SVGControl/SvgRenderer.cs` for the CR-6 accessibility change only,
  `SVGControl/SvgAssemblyProbe.cs` if `PublicKeyTokensEqual` is relocated there.
- **Expected behavior, two targeted items only:**
  1. **CR-5.** Add one test constructing `SvgRenderer(byte[], Size, AutoSize)` from
     `Defaults.GetDefault.SvgImage` and asserting `Document` is non-null, mirroring the four-argument
     overload's existing coverage. This drives lines 168-171 and moves the member from 13/17 = 76.471%
     to approximately 100%.
  2. **CR-6.** Make `PublicKeyTokensEqual` testable — relocate it to `SVGControl.SvgAssemblyProbe` or
     change it to `internal static` on `SvgRenderer` — and cover the cases its implementation already
     distinguishes: both null; one null and the other zero-length; one null and the other non-empty;
     equal tokens; unequal tokens of equal length; unequal lengths. It currently measures 0 / 15 =
     0.000%, so no test exercises the public-key-token match that AC-8 requires be preserved.
- **Verification:** rerun
  `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`,
  then record numeric per-member and per-file figures in
  `evidence/qa-gates/coverage-delta.<timestamp>.md` using the same per-`<line>`-descendant counting
  method the existing artifact uses, so the comparison stays like-for-like. Expected effect of the two
  items together: the file moves to roughly 75.7%. Repository-wide line coverage must remain at or
  above 85% and branch coverage at or above 75%.
- **Explicit scope boundary:** do **not** attempt to reach 85% on this file in this cycle. Doing so
  requires writing tests for `AddMargins` (0/15), `Render()` (18/26), and the two `SvgDocument`
  constructor overloads (0/8 each), none of which is part of issue #418. Those, plus the `SVGControl`
  assembly's other untested files (`DropDownEditor` 0/99, `SVGParser` 0/122, `ToggleSwitch` 0/62 plus
  0/23 designer, `SvgFileNameEditor` 0/104, three converters at 0/48, 0/48, and 0/26), belong in a
  separate `SVGControl` coverage-uplift entry. Create that entry under `docs/features/potential/`
  rather than absorbing the work here.

### R-5 — Correct stale and overbroad comments (non-blocking, documentation only)

- **Source finding:** code review CR-4 and CR-7 (both Low).
- **Files:** `SVGControl/SvgRenderer.cs` lines 24-31; `SVGControl.Test/SvgRendererParseContractTests.cs`
  lines 219-222.
- **Expected behavior:**
  1. **CR-4.** Update the header comment block, which is the sole in-code explanation for why the
     `AssemblyResolve` fallback exists. Replace "Svg 3.4.7 was compiled against ExCSS 4.2.3.0 but the
     repo deploys ExCSS 4.3.1.0" with the delivered pins (`Svg 3.4.8`, `ExCSS 4.3.2`, only
     `packages/ExCSS.4.3.2/` on disk). Replace "vstest's testhost ignores the test DLL's .config in
     some modes" with the conclusion this branch's own research reached: the vstest testhost does apply
     the project binding redirects and the ExCSS bind succeeds there; the host that does not apply them
     is `devenv.exe`. Reference
     `research/2026-08-04T15-05-svg-renderer-null-document-research.md` so the explanation has a
     durable source.
  2. **CR-7.** Narrow the Arrange comment's universal claim "No plain byte payload reaches it" to the
     measured statement, matching the hedge the production comment at `SvgRenderer.cs:394-397` already
     carries and the retraction the AC-5 amendment already made. Name open question U-3 explicitly.
- **Verification:** `dotnet tool run csharpier check .` at exit 0, then the remaining mandated stages.
  No behavior change, so no test change is expected and no coverage figure should move.
- **Rationale:** the branch went to real effort to retract this exact overstatement in the evidence
  artifact and in `issue.md`. Leaving it in the test file undermines the retraction. The stale header
  comment is a maintenance hazard on an indirect fallback: a reader who looks for ExCSS 4.2.3.0, does
  not find it, and concludes the fallback is dead code could remove working error handling.

### R-6 — Reduce `SVGControl/SvgRenderer.cs` below the 500-line pressure point (non-blocking)

- **Source finding:** code review CR-3 (Low).
- **File:** `SVGControl/SvgRenderer.cs`, currently 497 lines against the hard 500-line limit in
  `.claude/rules/general-code-change.md`. Compliant today with three lines of headroom.
- **Expected behavior:** extract the `AssemblyResolve` region — the static constructor,
  `_resolverInstalled`, `_resolving`, `ResolveByNameAndKey`, and `PublicKeyTokensEqual`, lines 24-163 —
  into a dedicated file in the `SVGControl` namespace, adding the corresponding `<Compile>` item to
  `SVGControl/SVGControl.csproj`. This continues the separation the branch already began with
  `SvgAssemblyProbe.cs` and leaves the renderer at roughly 360 lines.
- **Verification:** full mandated toolchain at exit 0. Coverage must not regress: the moved members
  carry their existing figures (`ResolveByNameAndKey` 47/69 with its ratified
  `COVERAGE_MEMBER_UNREACHABLE` exception; `PublicKeyTokensEqual` 0/15 unless R-4 item 2 lands first).
- **Sequencing note:** if both R-3, R-4 item 2, and R-6 are planned, do R-6 **last**, so the other two
  edit the file in its current location and the extraction is a pure move with no behavior delta to
  review. Alternatively fold R-3 and R-4 item 2 into the extraction as a single task, but only if the
  plan can keep the move and the behavior change reviewable as separate diffs.
- **Rationale:** the resolver has no renderer state and no conceptual relationship to SVG rendering.
  Extracting it deliberately now is cheaper than being forced into it by the next change to this file.

## Do Not Do

- Do **not** widen scope beyond the enumerated items. The work mode is `minor-audit` and the issue
  #418 Scope Lock applies.
- Do **not** edit `UtilitiesCS`. Its 195 pre-existing `CS86xx` diagnostics at forced-recompile scope
  are tracked outside issue #418 and are not this feature's to fix. They are the reason a cold
  solution-wide nullable build cannot pass on this repository independently of this branch.
- Do **not** attempt to raise `SVGControl/SvgRenderer.cs` to the 85% modified-file floor in this cycle.
  R-4 is deliberately bounded to two targeted items; see its explicit scope boundary.
- Do **not** fix the deferred defects recorded in `docs/features/potential/`
  (`2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`,
  `2026-08-04-invoke-mstest-scalar-count-strictmode.md`). Deferring them was correct. Promote them
  separately.
- Do **not** edit `scripts/vscode/Invoke-MSTest.ps1`. Its single-assembly `Count` defect is real and is
  already captured as a potential-feature entry; it is outside the Scope Lock.
- Do **not** weaken any assertion, delete any test, or add `[ExcludeFromCodeCoverage]` to any
  production file. `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy makes any exclusion
  of a production source path a Blocking finding.
- Do **not** relax any policy, rule, or threshold. Do not edit anything under `.claude/rules/` or
  `.github/instructions/`.
- Do **not** mark AC-11 as `[x]` without the human capture at
  `evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`. No amount of automated evidence
  substitutes for it.
- Do **not** create temporary files in tests. `.claude/rules/general-unit-test.md` UT4 prohibits it
  with zero approved exceptions, and this is specifically the constraint that makes a live
  `Assembly.LoadFrom` test inadmissible.
- Do **not** write evidence to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or
  `artifacts/evidence/`. All evidence goes to
  `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/<kind>/`.
- Do **not** treat the mandated nullable command's exit 0 as evidence of nullable cleanliness. It is
  vacuous in an up-to-date tree; record a forced-recompile result at project scope alongside it.

## Handoff

Per `.claude/skills/remediation-handoff-atomic-planner/SKILL.md`, the remediation plan is authored by
`atomic-planner`, not by `feature-review`. This reviewer has therefore not created a plan file. The
next step in the chain is:

```
orchestrator -> atomic-planner (authors the remediation plan from this file)
             -> atomic-executor (preflight, then task-by-task execution)
             -> feature-review (reaudit)
```

Two notes for whoever drives that chain:

1. `.claude/skills/feature-review-workflow/SKILL.md` step 8 tells `feature-review` to create the plan
   file, while `remediation-handoff-atomic-planner` assigns plan authorship to `atomic-planner` and
   states that the orchestrator must not act on plan content itself. The two skills conflict. This
   reviewer followed `remediation-handoff-atomic-planner`, because writing a stub the planner would
   immediately overwrite adds no value and risks a malformed plan artifact entering the chain. The
   conflict is also recorded as policy-audit gap G-6.
2. The same two skills disagree on artifact layout. `remediation-handoff-atomic-planner` specifies
   `remediation/<entry-ts>/remediation-inputs.md` and `audit/<exit-ts>/policy-audit.md`, while
   `.claude/hooks/validate-feature-review-coverage.ps1` requires the flat form
   `docs/features/active/<slug>/<stem>.<timestamp>.md` and requires the remediation-inputs artifact to
   share the policy audit's folder and timestamp. This cycle's artifacts use the flat form, which is
   what the enforced gate accepts. Resolve the conflict in the skill documents rather than
   rediscovering it each cycle.

R-1 is the only blocking item. If the intent is to unblock the PR with the least work, R-1 alone is
sufficient; R-2 is the highest value-per-line of the remainder.
