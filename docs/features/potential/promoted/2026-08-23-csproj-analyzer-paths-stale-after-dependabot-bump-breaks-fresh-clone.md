# csproj-analyzer-paths-stale-after-dependabot-bump-breaks-fresh-clone (Issue #597)

- Date captured: 2026-08-23
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/csproj-analyzer-paths-stale-after-dependabot-bump-breaks-fresh-clone/ (Issue #597)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #597
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/597
- Last Updated: 2026-08-23
## Summary

`packages.config` pins `Meziantou.Analyzer 3.0.174` and `Roslynator.Analyzers 4.16.1`, but 80 unconditional `<Analyzer Include>` items across 16 first-party `.csproj` files still point at the previous `3.0.156` / `4.16.0` package folders. A clean checkout restores only the pinned versions, so those 80 analyzer paths do not exist and the build fails with `error CS0006`. **A fresh clone of `main` cannot build.** CI passes only because its NuGet cache has a prefix `restore-keys` fallback that restores a pre-bump `packages` tree.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a — C# / MSBuild
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Data source or fixture: `main` at `d15f9510`; introduced by PR #573 (merged `eb6f6836`)

## Steps to Reproduce

1. Clone the repository into a directory with no pre-existing `packages/` folder, or delete `packages/` entirely.
2. Run `nuget restore TaskMaster.sln`. Only `Meziantou.Analyzer.3.0.174` and `Roslynator.Analyzers.4.16.1` are installed, because those are what `packages.config` pins.
3. Run the analyzer build command above.
4. Observe the failure.
5. For the CI side, inspect `.github/workflows/_build-analyzers.yml` and note the cache `restore-keys` prefix fallback.

## Expected Behavior

A clean checkout restores the pinned analyzer packages and builds. `<Analyzer Include>` paths agree with the versions `packages.config` pins.

## Actual Behavior

The 80 stale `<Analyzer Include>` paths reference package folders that a clean restore never creates, so MSBuild reports `error CS0006` (metadata file not found) and exits 1.

Counted over tracked first-party project files only (`git ls-files '*.csproj'`, 18 files):

```
Meziantou 3.0.156 lines : 16
Roslynator 4.16.0 lines : 64
TOTAL stale lines       : 80  across 16 files
lines citing 3.0.174 / 4.16.1 : 0
```

The items carry no `Condition` attribute, so they are evaluated unconditionally and a missing file is fatal rather than skipped. PR #573 was a Dependabot group bump that correctly updated `packages.config` and the `Condition`-guarded `Import` / `Error` lines, but not these hand-authored items — they were added by issue #181 and sit under the comment `<!-- Issue #181: analyzer-only references (first-party scope). -->`.

**Why nothing caught it.** Two independent maskings:

1. **Local verification.** The analyzer, nullable, and MSTest gates were run against PR #573 and were green. They were green because a long-lived `packages/` directory holds `Meziantou.Analyzer.3.0.101`, `3.0.123`, `3.0.156`, `3.0.174`, `Roslynator.Analyzers.4.16.0`, and `4.16.1` from historical restores, so the stale paths resolve. A clean environment has only the two pinned versions.
2. **CI.** `.github/workflows/_build-analyzers.yml` uses `key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}` with `restore-keys: nuget-${{ runner.os }}-`. When the content-hash key misses — which is exactly what a `packages.config` bump causes — the prefix fallback restores an older cache containing the pre-bump packages, and the stale paths resolve. Green CI on a `packages.config` change is therefore not evidence that a clean restore builds.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet — the offending shape, `QuickFiler/QuickFiler.csproj:581-586`:

  ```xml
  <!-- Issue #181: analyzer-only references (first-party scope). Severities are set to suggestion in .editorconfig so none break the nullable TreatWarningsAsErrors build. -->
  <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
  <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll" />
  <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll" />
  <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll" />
  <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll" />
  ```

## Impact / Severity

- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

Blocker for onboarding and for any clean-environment build: a new clone, a fresh CI runner with a cold cache, or a developer who clears `packages/` all hit `error CS0006`. It is not a blocker for the current working tree or for CI as presently configured, which is precisely why it has gone unnoticed since PR #573 merged. The severity reflects that the masking is incidental and can evaporate at any time — a cache eviction is enough.

## Suspected Cause / Notes

- 16 first-party `.csproj` files carry the `<Analyzer Include>` block. Repoint all 80 paths to `Meziantou.Analyzer.3.0.174` and `Roslynator.Analyzers.4.16.1`.
- Verify the target paths exist inside the new packages before editing; the Roslynator 4.16.1 layout should be confirmed rather than assumed, since analyzer sub-paths (`analyzers/dotnet/roslyn4.7/cs/...`) can move between versions.
- `*.csproj` is excluded from CSharpier by `.csharpierignore`, so the edit does not interact with the format gate.
- **The durable fix is to stop hardcoding the version.** An MSBuild property or a glob would make the next Dependabot bump a one-line change instead of an 80-line one. That is the actual root cause: a version pinned in two places that nothing reconciles.
- **The CI cache masking deserves its own remedy.** A prefix `restore-keys` fallback on a lockfile-hash key defeats the purpose of the hash. Either drop `restore-keys`, or add a step that fails when the restored `packages` tree does not satisfy `packages.config`.
- Related: the same PR left the `ErrorText` boilerplate reverted to older Visual Studio wording, and advanced binding redirects. Both cosmetic and out of scope here.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas — not applicable; this is a build-graph defect with no unit-testable surface. The meaningful check is a build in a clean environment.
- [x] Integration scenario to retest — delete `packages/`, run `nuget restore TaskMaster.sln`, then the analyzer and nullable builds. Both must exit 0. Running this against the current `main` first, to observe `CS0006`, establishes the defect before the fix.
- [x] Manual verification notes — after repointing, grep the tracked project files and require **zero** references to `3.0.156` or `4.16.0` and exactly 80 to the new versions. Then confirm the analyzers actually load rather than merely resolving: an analyzer path that exists but is never loaded produces a silently weaker build, so check that a known Meziantou or Roslynator diagnostic still appears at `suggestion` severity.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
