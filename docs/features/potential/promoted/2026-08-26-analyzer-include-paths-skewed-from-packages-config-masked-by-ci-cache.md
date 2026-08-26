# analyzer-include-paths-skewed-from-packages-config-masked-by-ci-cache (Issue #615)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/analyzer-include-paths-skewed-from-packages-config-masked-by-ci-cache/ (Issue #615)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #615
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/615
- Last Updated: 2026-08-26
## Summary

All 16 C# project files pin `<Analyzer Include>` paths to `Meziantou.Analyzer.3.0.156` and
`Roslynator.Analyzers.4.16.0`, but every `packages.config` requests `Meziantou.Analyzer 3.0.174` and
`Roslynator.Analyzers 4.16.1`. A restore therefore never produces the directories the projects
reference, and a build on a clean checkout fails with `error CS0006: Metadata file
'..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could
not be found` (five such errors per project, 80 in total) before a single analyzer runs.

The skew was introduced by Dependabot commit `f8e22af7` ("Bump the analyzers-dev-deps group with 2
updates"), which updated `packages.config` and the `<Import>`/`<Error>` lines to the new versions
but left the `<Analyzer Include>` item paths at the old ones. This is the known partial-update
failure mode of Dependabot against `packages.config`-style projects.

CI does not catch it, and the reason CI stays green is itself the more serious half of the defect.
`.github/workflows/_build-analyzers.yml`, `_build-nullable.yml` and `_mstest-coverage.yml` all cache
`packages` with `key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}` and
`restore-keys: nuget-${{ runner.os }}-`. When `packages.config` changes, the exact key misses, the
prefix restore-key hits an older cache entry, and that older entry still contains the
`Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` folders left over from before the
bump. The build resolves against those stale directories and succeeds.

Two consequences follow. First, CI's green result is an artifact of cache carry-over, not of a
correct build: any cache eviction or a change to `runner.os` reproduces the 80 `CS0006` errors and
takes the entire pipeline red with no code change. Second, and continuously true today, the
analyzers actually executing in CI are the OLD versions (3.0.156 / 4.16.0) that the `<Analyzer
Include>` items name, not the versions `packages.config` declares. Every analyzer-version bump in
this group has been silently inert since `f8e22af7`, so the repository's static-analysis gate is not
running the ruleset the dependency manifest claims.

Reproduced on 2026-08-26 in a clean worktree at commit `c279d40b`: `nuget restore TaskMaster.sln`
succeeded (172 packages), then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug
"/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exited 1 with the
`CS0006` errors above. Manually installing the two referenced older analyzer packages into
`packages/` — which is exactly what CI's stale cache supplies — made the same command exit 0. That
substitution is the direct evidence for the cache-masking mechanism.

This defect is off the issue #614 defect chain and is filed separately rather than absorbed into it.
It was found while bootstrapping a clean worktree for #614; the #614 branch works around it locally
by installing the referenced analyzer versions into the gitignored `packages/` directory, and
changes no project file.

Likely fix: update the `<Analyzer Include>` paths in all 16 `.csproj` files to match
`packages.config`, then remediate whatever diagnostics the newer analyzers surface. Separately,
remove the `restore-keys` prefix fallback (or scope it so it cannot serve a cache built from a
different `packages.config`) so a partial dependency update fails CI instead of being masked. The
project-file repair and the workflow repair should be scoped and validated together, because the
first will expose analyzer diagnostics that the second must not re-hide.

## Environment

- OS/version: Windows 11 Pro 10.0.26200; Visual Studio 18 Community MSBuild 18.8.2; NuGet CLI latest.
- Python version: Not applicable; this is a C# / MSBuild build-configuration defect.
- Command/flags used: `nuget restore TaskMaster.sln` then
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Data source or fixture: Clean git worktree at commit `c279d40b` with no pre-existing `packages/`
  directory.

## Steps to Reproduce

1. Create a clean checkout or worktree with no `packages/` directory present.
2. Run `nuget restore TaskMaster.sln`. It succeeds and installs `Meziantou.Analyzer.3.0.174` and
   `Roslynator.Analyzers.4.16.1`.
3. Run the analyzer build command above.
4. Observe `error CS0006` for `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` on every
   C# project, and a non-zero exit code.

## Expected Behavior

A clean checkout restores exactly the analyzer packages the projects reference, and the analyzer
build succeeds. The analyzer versions that execute are the versions `packages.config` declares. CI
fails when a dependency update leaves project files inconsistent with `packages.config`.

## Actual Behavior

A clean checkout fails the analyzer build with 80 `CS0006` errors. CI passes only because a
prefix `restore-keys` cache hit supplies analyzer directories left over from before the version
bump, so CI silently runs the superseded analyzer versions.

```
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [UtilitiesCS\UtilitiesCS.csproj]
CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll' could not be found [UtilitiesCS\UtilitiesCS.csproj]
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: the `CS0006` errors above, captured from the failing `msbuild /t:Rebuild` run.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The static-analysis gate is running versions other than the declared ones, and the pipeline is one
cache eviction away from going red across every job with no code change.

## Suspected Cause / Notes

- `UtilitiesCS/UtilitiesCS.csproj` line 3 imports `Meziantou.Analyzer.3.0.174` while lines 1301-1305
  reference `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0`; the same split exists in
  all 16 project files.
- Introduced by Dependabot commit `f8e22af7`.
- Cache configuration: `.github/workflows/_build-analyzers.yml`, `_build-nullable.yml` and
  `_mstest-coverage.yml`, `actions/cache@v4` step, `restore-keys: nuget-${{ runner.os }}-`.

## Proposed Fix / Validation Ideas

- [ ] Align `<Analyzer Include>` paths in all 16 `.csproj` files with `packages.config`, then
      remediate the diagnostics the newer analyzers surface.
- [ ] Remove or tighten the `restore-keys` prefix fallback so a `packages.config` change cannot be
      served by a cache built from a different manifest.
- [ ] Add a build-configuration consistency check that fails when a `<Analyzer Include>` version does
      not match the corresponding `packages.config` entry.
- [ ] Unit coverage areas: not applicable; this is build configuration. Validate by a cold-cache CI
      run.
- [ ] Integration scenario to retest: a CI run with the NuGet cache disabled or its key invalidated
      must pass.
- [ ] Manual verification notes: delete `packages/`, run `nuget restore` then the analyzer build, and
      confirm exit 0.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
