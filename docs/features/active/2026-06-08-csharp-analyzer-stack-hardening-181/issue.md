# csharp-analyzer-stack-hardening (Issue #181)

- Date captured: 2026-06-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/csharp-analyzer-stack-hardening/ (Issue #181)
- Origin: Deferred "decision 2" from the .claude governance sync (issue #178, PR #179)

- Issue: #181
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/181
- Last Updated: 2026-06-08
- Work Mode: full-feature

## Problem / Why

The .claude governance sync (issue #178, PR #179) intentionally deferred adopting the
reference repo's hardened C# analyzer-stack + central-config + clock-seam mechanism because
it risks breaking this repo's legacy VSTO/.NET Framework build. This repo's 19 `.csproj`
files are all legacy NON-SDK-style; 16 use `packages.config`; CI restores via
`nuget restore`. The hardened mechanism must be adapted so it builds cleanly with zero new
build/CI failures and without violating the repo's retained policy (MSTest/Moq, 80/90
coverage, msbuild + vstest).

## Proposed Behavior

Adopt the C# analyzer-stack and determinism hardening as a repository mechanism, adapted to
this repo's toolchain:

- Add analyzer-only package references (Meziantou.Analyzer, SonarAnalyzer.CSharp,
  Roslynator.Analyzers, AsyncFixer, SecurityCodeScan.VS2019, BannedApiAnalyzers) to
  first-party projects only (exclude vendored SVGControl, UtilitiesSwordfish.NET.General).
- Activate BannedApiAnalyzers + BannedSymbols.txt for the 5 banned time/random symbols.
- Add TimeProvider/FakeTimeProvider seam guidance to rules/csharp.md.
- Add analyzer severities, file-scoped-namespace preference, and naming rules to
  .editorconfig/.globalconfig scoped so the existing codebase does not produce
  build-breaking errors.

## Acceptance Criteria (early draft)

- [x] AC1: Analyzer packages referenced by first-party projects; restore cleanly via `nuget restore`.
- [x] AC2: BannedApiAnalyzers + BannedSymbols.txt active; 5 banned symbols flagged in new/touched code.
- [x] AC3: TimeProvider/FakeTimeProvider seam + guidance added to rules/csharp.md; no runtime behavior changed.
- [x] AC4: .editorconfig/.globalconfig carries new severities, file-scoped-namespace pref, naming rules, scoped to avoid build-breaking errors.
- [x] AC5: All four toolchain stages pass locally to the extent the environment allows; nullable TreatWarningsAsErrors step does NOT regress.
- [x] AC6: PR CI is GREEN, including nullable-as-errors and MSTest-with-coverage steps.
- [x] AC7: No do_not_change invariant violated; rules/csharp.md updated retaining MSTest/Moq, 80/90 coverage, msbuild+vstest.
- [x] AC8: Change scoped to C# build-config + rules/csharp.md (+ .editorconfig/.globalconfig + Directory.Build.props if used + per-project analyzer refs). No application logic changes except seam introductions required to compile.

## Constraints & Risks

- CPM (Directory.Packages.props) is INCOMPATIBLE with packages.config; must not be introduced.
- Mixing PackageReference + packages.config in one project is unsupported; prefer existing style.
- New strict analyzers WILL emit warnings across legacy code; the nullable
  `/p:TreatWarningsAsErrors=true` CI step will break if severities are promoted to errors.
  Introduce analyzers at warning/suggestion severity or scope to new/touched files.
- Directory.Build.props at repo root is imported by ALL projects including vendored ones;
  scope strict analyzers to first-party projects only.
- CI restores via `nuget restore` (not `dotnet restore`); package additions must restore with it.

## Test Conditions to Consider

- [ ] `nuget restore TaskMaster.sln` succeeds with new analyzer packages.
- [ ] Both msbuild stages (analyzers; nullable-as-errors) stay green.
- [ ] vstest MSTest run unaffected.
- [ ] PR GitHub Actions CI green.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/csharp-analyzer-stack-hardening/` folder from the template