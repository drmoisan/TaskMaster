# Policy Audit — utilitiescs-nullable-svgcontrol (Issue #368)

- Component: `SVGControl/` (net481 WinForms control project; independent of `UtilitiesCS`)
- Feature branch: `feature/utilitiescs-nullable-svgcontrol-368`
- Base / merge-base: `origin/epic/utilitiescs-nullable-remediation-integration` @ `6d4da8bb4d881dc26c421440464ce5575e3fb15f` (recomputed via `git merge-base HEAD origin/epic/utilitiescs-nullable-remediation-integration`; matches the caller-supplied base)
- Head commit under review: `c194362d612497f1fd5a6ee36aec7f52c4b949d4`
- Work mode: `full-feature` (per `issue.md`); AC sources: `spec.md` + `user-story.md`
- Reviewer: feature-review agent
- Timestamp: 2026-07-19T11-15

## Executive Summary

This is a per-file `#nullable enable` opt-in remediation of 12 hand-authored `.cs` files in
`SVGControl/`, plus one unrelated one-line-scope PowerShell tooling fix
(`scripts/vscode/Invoke-MSTestWithCoverage.ps1`). Independent re-verification (see Section 5 and
Section 6) confirms: zero CS86xx diagnostics anywhere in a full solution-wide `/t:Rebuild
/p:TreatWarningsAsErrors=true`; zero `csharpier` formatting diffs; 37/37 `SVGControl.Test` tests
passing; no `<Nullable>` element introduced anywhere; no behavior-breaking signature changes; no
Designer/generated file touched. The two documented pre-existing-and-unrelated build errors
(`CS0649` x2 in `SvgImageSelector.cs`, `CS0006` x4 in `VBFunctions.csproj`) were independently
reproduced and confirmed unrelated to this feature's diff.

The feature fails the **mandatory coverage-artifact gate**: no canonical
`artifacts/csharp/coverage.xml` or `artifacts/pester/powershell-coverage.xml` exists in this
worktree for either language with changed files on the branch. This is a known, repository-wide,
systemic gap (local full-solution C# coverage generation is independently blocked; see prior
review precedent for issues #309/#354) and is not a defect newly introduced by this feature's
`SVGControl/`-scoped source edits. It is nonetheless recorded as **FAIL** per the mandatory
coverage-verification procedure and is carried to `remediation-inputs`.

A second, narrower finding: the `Invoke-MSTestWithCoverage.ps1` one-line bug fix (StrictMode
scalar/array coercion) did not follow the repo's Bugfix Workflow (no failing regression test
added before the fix). This is recorded as **PARTIAL**.

**Overall disposition: BLOCKED on the coverage-artifact gate (procedural/systemic), not on code
correctness.** All AC1–AC6 acceptance criteria are independently verified PASS (see
`feature-audit`). No blocking code-correctness or behavior-change defect was found in the 12
remediated `SVGControl/` files.

## Rejected Scope Narrowing

No narrowing of the audit scope was attempted by the delegating prompt for this review cycle. The
prompt's "Known, pre-existing, out-of-scope findings to expect" section supplied hypotheses to
independently verify (not instructions to skip verification), and each hypothesis was in fact
independently reproduced and confirmed (Section 6). No caller text instructed this agent to treat
any language's coverage as informational-only, to skip a toolchain check, or to narrow scope to a
plan/task/phase subset. Full branch-diff scope (against the resolved merge-base) was audited.

## Evidence Location Compliance

- `scripts/validate_evidence_locations.py` does not exist in this repository (searched
  repository-wide; not found). Fallback: manual scan performed via
  `git diff --name-only origin/epic/utilitiescs-nullable-remediation-integration...HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"`.
- Result: **zero matches.** All evidence for this feature is written under the canonical
  `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/{baseline,qa-gates,regression-testing,other}/`
  tree, per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. No
  `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition applies; no non-canonical path was supplied or
  used.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles (Independence, Isolation, Fast, Deterministic, Readable)

- No new automated tests were added or required by this feature (annotation-only scope, per
  `spec.md` "Seeded Test Conditions"). The existing `SVGControl.Test` suite (`GetRelativePath_Test.cs`,
  `RelativePathCoverageTests.cs`; MSTest + FluentAssertions) was re-run at every batch and at final
  QC — 37/37 passed each time (independently re-verified, see Section 6). **PASS.**
- No temp files were created or used by any test in this feature. **PASS.**

### 1.2 Coverage Requirements

#### 1.2.1 Per-language coverage rows (mandatory for every language with changed files)

- **TypeScript coverage:** N/A — 0 changed `.ts`/`.tsx` files on this branch (confirmed via `git
  diff --numstat`).
- **Python coverage:** N/A — 0 changed `.py` files on this branch.
- **PowerShell coverage:**
  - Baseline: unavailable (no `artifacts/pester/powershell-coverage.xml` exists in this worktree
    prior to this review).
  - Post-change: unavailable (no `artifacts/pester/powershell-coverage.xml` exists in this
    worktree after this feature's commit).
  - Change: N/A (no artifact pair to diff).
  - New/changed-code coverage: 0% (the one changed line, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
    line 133/139, is top-level script-scope glue code, not wrapped in a testable function in
    `Invoke-MSTestWithCoverage.Helpers.ps1`, and has no Pester test exercising it before or after
    this change).
  - Disposition: **FAIL, coverage artifact absent for PowerShell; coverage verification is
    mandatory for all languages with changed files.** This is a repository-wide tooling gap
    (Pester coverage instrumentation is not wired into this local environment), not a regression
    introduced by this feature.
  - Evidence: directory listing confirming `artifacts/pester/` does not exist in this worktree;
    `git diff` confirms the sole PowerShell change is a 2-line array-coercion fix in
    `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
- **C# (CSharp) coverage:**
  - Baseline: unavailable (no `artifacts/csharp/coverage.xml` exists in this worktree).
  - Post-change: unavailable (no `artifacts/csharp/coverage.xml` exists in this worktree).
  - Change: N/A (no canonical artifact pair to diff).
  - New/changed-code coverage: 0% for all 12 modified `SVGControl/` files. The feature's own
    project-scoped Cobertura evidence at `evidence/qa-gates/final-coverage-delta.md` documents a
    pre-existing 0% baseline for these 12 files, not introduced or worsened by this feature, since
    `SVGControl.Test` has never exercised any of them. `RelativePath.cs` is a separate,
    verify-only file that was not modified by this feature; its independently re-confirmed
    coverage is unchanged at line-rate 56.75% / branch-rate 54.35% both before and after
    (byte-identical), and is excluded from the "modified files" count above.
  - Disposition: **FAIL, coverage artifact absent for CSharp; coverage verification is mandatory
    for all languages with changed files.** Supplementary (non-canonical) evidence from this
    feature's own project-scoped Cobertura capture (`evidence/qa-gates/final-coverage.cobertura.xml`,
    independently spot-checked against the raw XML in this review) shows the `SVGControl` package
    line-rate moving from 26.65% (870/3264) to 26.64% (870/3266) — a -0.02 percentage-point
    change fully explained by 2 new instrumentable-but-never-covered lines (no previously-covered
    line lost coverage; `lines-covered` is unchanged at 870); branch-rate is unchanged at 32.28%
    (368/1140 both before and after). This is not a canonical repo-wide C# coverage figure and does
    not substitute for the missing `artifacts/csharp/coverage.xml` gate.
  - Evidence: directory listing confirming `artifacts/csharp/` does not exist in this worktree;
    `evidence/qa-gates/final-coverage-delta.md`; direct inspection of
    `evidence/qa-gates/final-coverage.cobertura.xml` headline attributes
    (`line-rate="0.266381"`, `lines-covered="870"`, `lines-valid="3266"`) performed independently
    in this review and matching the evidence document's claims exactly.

#### 1.2.2 Threshold conflict (flagged, not resolved by this feature or this review)

`CLAUDE.md`'s embedded General Unit Test Policy states repository-wide line coverage `>= 80%`
(with a COM/VSTO/WinForms testable-denominator exemption) while `.claude/rules/general-unit-test.md`
states a uniform `>= 85%` line / `>= 75%` branch floor with no such exemption. The executing plan
explicitly flagged this conflict in its "Open Questions / Notes" section rather than silently
picking one value, and deferred resolution to the epic's Wave-2 CI-capstone child, consistent with
prior epic-sibling precedent. This review does not resolve the conflict; it is recorded here as a
pre-existing, repository-wide condition, not a defect of this feature.

### 1.3 Scenario Completeness / Test Structure / External Dependencies

Not applicable to new test authoring (none added). Existing test structure in `SVGControl.Test`
was not modified by this feature. **N/A.**

## 2. General Code Change Policy Compliance

### 2.1 Design Principles / Simplicity / Reusability / Extensibility / Separation of Concerns

**PASS.** Every change is a nullable-annotation edit (`?`, `!`, flow-narrowed locals) on an
existing member. No new class, method, or abstraction was introduced. No refactor occurred.

### 2.2 File Size Limit (500 lines)

**PASS.** All 12 remediated files are well under 500 lines (largest is `SvgRenderer.cs` at 344
lines, independently confirmed). `RelativePath.cs` (1678 lines) already exceeded the limit before
this feature and was not touched (verify-only, confirmed byte-identical via `git diff --stat`
showing no entry for it) — a pre-existing condition explicitly out of scope, not a new violation.

### 2.3 Error Handling and Logging

**PASS.** No new error-handling or logging code was introduced. Existing guard clauses
(`if (x == null)`, `??=`) were preserved unchanged; no new `if (x is null) throw` guard was added
anywhere (confirmed via per-batch evidence and independent diff review), consistent with the
spec's explicit instruction to prefer annotation/`!` over new runtime guards.

### 2.4 Bugfix Workflow (applies to `scripts/vscode/Invoke-MSTestWithCoverage.ps1`)

**PARTIAL.** The `Invoke-MSTestWithCoverage.ps1` change fixes a genuine defect (a PowerShell
`Set-StrictMode`-triggered scalar/array coercion bug: `$testAssemblies.Count` throws when exactly
one test assembly matches). Per the General Code Change Policy's Bugfix Workflow, a defect fix
must be preceded by a failing regression test. No such test was added (confirmed: `git diff`
shows only the production script changed; no file under `tests/scripts/vscode/` was added or
modified in this diff). The one changed line is top-level script-scope glue code, not a function
extracted into the testable `Invoke-MSTestWithCoverage.Helpers.ps1` module, so it was not
practical to unit-test without further refactor (which itself would exceed the fix's minimal
scope). This is recorded as a Partial finding, carried to remediation, rather than silently
accepted or silently failed.

### 2.5 Naming / Public APIs / Compatibility / Dependencies / I/O Boundaries

**PASS.** No public API was removed, renamed, or had a parameter added/removed. All public
signature changes are additive nullability annotations only (independently re-verified against
`evidence/qa-gates/final-signature-compat.md`, cross-checked against `git diff` for all 12 files).
No new dependency was added.

## 3. Language-Specific Code Change Policy Compliance (C#, `.claude/rules/csharp.md`)

### 3.1 Toolchain Order and Commands

**PASS.** `csharpier` -> analyzer/code-style `msbuild` -> nullable-pragma `msbuild /t:Rebuild
/p:TreatWarningsAsErrors=true` -> `vstest` (via `Invoke-MSTestWithCoverage.ps1`) was followed in
the documented order at every batch (Section 6 independently re-confirms the final pass).

### 3.2 MSTest / Moq / FluentAssertions

**N/A** for this feature (no new tests were authored). Existing tests already use MSTest +
FluentAssertions.

### 3.3 Per-file `#nullable enable` architecture (this feature's specific mandate)

**PASS.** `#nullable enable` was added to exactly the 12 hand-authored files named in scope; no
`<Nullable>` element was added to `SVGControl.csproj` or `TaskMaster.sln` (independently
re-confirmed via `grep -c "Nullable"` returning 0 for both files, Section 6). The 3 already-enabled
verify-only files (`PathInternal.cs`, `RelativePath.cs`, `ValueStringBuilder.cs`) remain
byte-identical (confirmed: absent from `git diff --stat`). The 5 Designer/generated files remain
byte-identical (independently re-confirmed: `git diff --stat` against those 5 exact paths returns
no output).

### 3.4 Nullable post-condition attributes / polyfills

**PASS.** Independently re-confirmed via grep: the sole match for a post-condition-attribute
keyword anywhere in `SVGControl/*.cs` is one inert, pre-existing, commented-out line in
`PathInternal.cs` (a verify-only file, untouched by this feature). No polyfill declaration for
`System.Diagnostics.CodeAnalysis` exists anywhere in `SVGControl/`.

### 3.5 Legacy project format / `record`/`init` prohibition

**PASS.** `SVGControl.csproj` remains a non-SDK-style legacy project (unmodified in this diff —
confirmed absent from `git diff --stat`). `SvgResource` (in `ISvgResource.cs`) remains a plain
`class` with settable properties, not converted to `record`/`record struct`/`init` (independently
re-confirmed via grep).

## 4. Language-Specific Unit Test Policy Compliance (C#)

**N/A.** No new C# tests were authored or required by this annotation-only feature. Existing
`SVGControl.Test` tests (MSTest + FluentAssertions) continue to pass unmodified (37/37,
independently re-verified in Section 6).

## 5. Test Coverage Detail

See Section 1.2.1 above for the mandatory per-language rows. Summary table:

| Language | Changed files | Canonical artifact | Baseline | Post-change | Verdict |
|---|---|---|---|---|---|
| TypeScript | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 1 (`Invoke-MSTestWithCoverage.ps1`) | `artifacts/pester/powershell-coverage.xml` — absent | unavailable | unavailable | **FAIL** (artifact absent) |
| C# (CSharp) | 12 (`SVGControl/*.cs`) | `artifacts/csharp/coverage.xml` — absent | unavailable | unavailable | **FAIL** (artifact absent) |

Supplementary, non-canonical, feature-scoped Cobertura evidence (`SVGControl` project only, not a
repo-wide figure): line-rate 26.65% -> 26.64% (delta explained entirely by 2 new
instrumentable-but-uncovered lines; zero previously-covered lines lost coverage); branch-rate
unchanged at 32.28%. `RelativePath.cs` (the only file in scope with a real pre-existing test
baseline) is byte-identical in coverage before/after (56.75% line / 54.35% branch), confirming no
changed-line regression for that file. These figures were independently spot-checked against the
raw XML headline attributes in `evidence/qa-gates/final-coverage.cobertura.xml` during this review
and matched the evidence document's claims exactly.

## 6. Test Execution Metrics (Independently Re-Verified in This Review)

- `dotnet tool run csharpier check SVGControl/` — **EXIT 0**, "Checked 18 files", zero residual
  formatting diffs (re-run independently in this review).
- `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
  /p:TreatWarningsAsErrors=true` — **EXIT 1**, 6 total errors, **zero** `CS8xxx` diagnostics
  (confirmed via `grep -c "CS8[0-9][0-9][0-9]"` on the full build log = 0). The 6 errors are
  exactly: 2x `CS0649` in `SvgImageSelector.cs` (pre-existing, unrelated to nullable) and 4x
  `CS0006` in `VBFunctions.csproj` (pre-existing analyzer-package-version-pin mismatch,
  `UtilitiesCS.csproj` fails only via dependency short-circuit on `SVGControl.csproj`). This
  precisely matches the evidence artifacts' claims and was reproduced independently in this
  review, not merely read from the evidence.
- `vstest.console.exe SVGControl.Test/bin/Debug/SVGControl.Test.dll` — **EXIT 0**, "Total tests:
  37, Passed: 37, Failed: 0" (re-run independently in this review after rebuilding
  `SVGControl.Test.csproj`, since the prior `/t:Rebuild` cleaned `SVGControl`'s output).
- `grep -n "Nullable" SVGControl/SVGControl.csproj` and `... TaskMaster.sln` — both return 0
  matches (re-run independently in this review).
- `git diff --stat` on the 5 named Designer/generated files — zero output (re-run independently
  in this review).

## 7. Code Quality Checks

See `code-review.2026-07-19T11-15.md` for the full findings table. Summary: no Blocking
code-quality findings; two Partial findings (missing PowerShell regression test for the bugfix;
absent canonical coverage artifacts for both changed languages), both carried to remediation.

## 8. Gaps and Exceptions

1. **Coverage artifact absence (C# and PowerShell).** No `artifacts/csharp/coverage.xml` or
   `artifacts/pester/powershell-coverage.xml` exists in this worktree. This is a known,
   repository-wide, systemic gap (local full-solution C# coverage generation is independently
   blocked by environment constraints unrelated to this feature) and is not newly introduced by
   this feature's source edits. Carried to remediation per the mandatory coverage-verification
   procedure.
2. **Missing regression test for the `Invoke-MSTestWithCoverage.ps1` bugfix.** The Bugfix Workflow
   requires a failing test before the fix; none was added. Carried to remediation as a
   process-compliance gap, not a functional defect (the fix itself was independently re-verified
   as correct: it forces array semantics on a `Get-ChildItem | ... | Select-Object` pipeline that
   previously collapsed to a scalar under `Set-StrictMode` when exactly one match existed).
3. **Threshold conflict** between `CLAUDE.md` (80%/90%) and `.claude/rules/general-unit-test.md`
   (85%/75% uniform) remains unresolved repo-wide; the executing plan explicitly flagged rather
   than silently resolved it. Not a defect of this feature.

## 9. Summary of Changes

- 12 hand-authored `.cs` files in `SVGControl/` received a `#nullable enable` pragma and were
  brought to zero CS86xx diagnostics via `?` annotations, flow-narrowed locals, and justified `!`
  operators. No behavior change.
- `ISvgResource`/`SvgResource` (`Name`, `Data`) were made nullable to resolve a `CS8766`
  interface-implementation mismatch (necessary consequence of the architecture, not scope creep —
  see `feature-audit` for full analysis).
- One shared PowerShell tooling script (`scripts/vscode/Invoke-MSTestWithCoverage.ps1`) received a
  2-line array-coercion fix unrelated to nullable content.
- 47/47 atomic-plan tasks checked off; all AC1–AC6 checked off in `issue.md`.

## 10. Compliance Verdict

**BLOCKED (procedural) / PASS (code correctness).** All code-correctness, behavior-preservation,
and per-file nullable-gate requirements are independently verified PASS. The mandatory
coverage-artifact gate is FAIL for both changed languages (C#, PowerShell) due to absent canonical
artifacts — a systemic, pre-existing environment gap, not a defect in this feature's `SVGControl/`
changes. A minor process gap (missing regression test for the PowerShell bugfix) is recorded as
Partial. See `remediation-inputs.2026-07-19T11-15.md` for the specific remediation triggers.

## Appendix A: Test Inventory

| Test file | Framework | Scope | Result (independently re-run) |
|---|---|---|---|
| `SVGControl.Test/GetRelativePath_Test.cs` | MSTest + FluentAssertions | `RelativePath.cs` (verify-only) | Pass (part of 37/37) |
| `SVGControl.Test/RelativePathCoverageTests.cs` | MSTest + FluentAssertions | `RelativePath.cs` (verify-only) | Pass (part of 37/37) |

No test in `SVGControl.Test` exercises any of the 12 remediation-target files (`ButtonSVG.cs`,
`PictureBoxSVG.cs`, `ToggleSwitch.cs`, `SVGParser.cs`, `SvgRenderer.cs`, `SvgImageSelector.cs`,
`ISvgResource.cs`, `SvgOptionsConverter.cs`, `SvgOptionsConverter2.cs`, `SvgResourceConverter.cs`,
`DropDownEditor.cs`, `SVGFileNameEditor.cs`) — a pre-existing condition documented explicitly by
the plan and spec, not introduced by this feature.

No Pester test exercises `scripts/vscode/Invoke-MSTestWithCoverage.ps1`'s top-level script-scope
glue code (the changed line); `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`
exercises only the extracted, testable `Invoke-MSTestWithCoverage.Helpers.ps1` module, which was
not touched by this feature's fix.

## Appendix B: Toolchain Commands Reference

```
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true
pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput <path>
vstest.console.exe SVGControl.Test/bin/Debug/SVGControl.Test.dll
```

All commands above were independently re-run (or, for the coverage-wrapped test run, reproduced
via a direct `vstest.console.exe` invocation against the rebuilt test assembly) during this review
pass, in addition to being documented in the feature's own evidence artifacts.
