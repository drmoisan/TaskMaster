# dependabot-fanout-and-ci-failing-nuget-upgrades (Issue #911)

- Date captured: 2026-09-19
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/dependabot-fanout-and-ci-failing-nuget-upgrades/ (Issue #911)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #911
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/911
- Last Updated: 2026-09-19
- Work Mode: full-bug

## Summary

Dependabot opens one pull request per configured group each cycle, and those pull requests fail the
required CI checks, so dependency upgrades are effectively unmergeable without manual repair.

The cause is **not** that Dependabot fails to maintain `.csproj` state. It maintains it. The cause
is that while updating one group, Dependabot also rewrites the `<Import>` and `<Error>` package-import
guards of packages **outside** that group to a version that no `packages.config` in the repository
declares. Restore honours the manifest, the build honours the project file, and
`EnsureNuGetPackageBuildImports` fails closed.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (.NET Framework 4.8.1 VSTO solution, 18 non-SDK projects)
- Command/flags used: `.github/dependabot.yml` weekly NuGet schedule
- Required checks (repository ruleset 18572843, `strict_required_status_checks_policy: true`), five:
  `actionlint / actionlint`, `format-check / Verify formatting`,
  `build-analyzers / Build with analyzers and code style enforcement`,
  `build-nullable / Build with nullable warnings treated as errors`,
  `mstest-coverage / Run MSTest suite with coverage`.
  A sixth check, `pester / Run Pester suite with coverage`, runs but is not required.
- Data source or fixture: 18 `packages.config` manifests and their sibling `.csproj` and `app.config` files

## Steps to Reproduce

1. Allow the weekly Dependabot NuGet schedule to run against `main`.
2. Observe one pull request per configured group (four groups produced #907, #908, #909).
3. Open any one of them and inspect the required checks.
4. Compare, for a package that is **not** in that pull request's group, the version in
   `packages.config` against the version in the sibling `.csproj` `<Import>` element.

## Expected Behavior

One consolidated pull request per cycle whose manifest, project-file and `app.config` state are
mutually consistent, and which passes all five required checks without human edits.

## Actual Behavior

Measured history: **14 of 59 Dependabot pull requests have ever been merged, and none since
2026-08-21**; every merge that did land carried human commits repairing the branch.

Verified on pull request #908 (the `test-frameworks` group) at run 35264873270:

- It changed **30 files**: 10 `.csproj`, 10 `app.config`, 10 `packages.config`, across ten project
  directories in a single pull request.
- It correctly rewrote `<Import>`, `<Error>`, `<Reference>` and `<HintPath>`, added
  `<Private>True</Private>` and dropped `processorArchitecture=MSIL` — the signature of NuGet
  regenerating references rather than patching version strings.
- **But** in nine project files it also moved `Meziantou.Analyzer` — which belongs to the
  `analyzers-dev-deps` group, not this one — from `3.0.235` to `3.0.259` in `<Import>` and
  `<Error>`, while leaving `packages.config` at `3.0.235`.

The resulting three-way divergence inside a single project:

| Location | Version |
|---|---|
| `packages.config` | `3.0.235` (unchanged) |
| `.csproj` `<Import>` / `<Error>` | `3.0.259` (rewritten, out of scope) |
| `.csproj` `<Analyzer Include>` | `3.0.203` (never rewritten by anything) |

`nuget restore` honoured `packages.config` and fetched `3.0.235`; the restore log lists every
package it pulled and `Meziantou.Analyzer.3.0.259` is not among them. MSBuild then failed in nine
projects with:

```
error : This project references NuGet package(s) that are missing on this computer.
The missing file is ..\packages\Meziantou.Analyzer.3.0.259\build\Meziantou.Analyzer.props.
```

The `csc` command line in the same log shows
`/analyzer:..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`,
confirming the third version is live in the compile.

`format-check` separately rejected **all 20** touched files — 10 `packages.config` **and** 10
`app.config` — because Dependabot writes them in inline form while CSharpier requires its own
wrapping.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet (measured against `origin/main` at 734112ed2 and pull request #908, 2026-09-19):

```
gh pr view 908 --json files   -> 10 .csproj, 10 app.config, 10 packages.config

Meziantou changes in #908:
  9x  packages.config        version="3.0.235"   (unchanged; reflowed to inline only)
  9x  csproj Import/Error    3.0.235 -> 3.0.259  (out of this PR's group)
  0x  csproj Analyzer Include                     (never touched)

restore log, job 105349413710: FluentAssertions.8.11.0, Microsoft.Testing.*.2.4.1,
MSTest.*.4.4.1, Microsoft.TestPlatform.*.18.10.1 -- no Meziantou.Analyzer.3.0.259

On origin/main today: 15 of 18 projects still carry a stale
  <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.203\...\Meziantou.Analyzer.dll" />
while every manifest pins 3.0.235 -- the residue of an earlier merged bot pull request.
```

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Dependency upgrades — including security-relevant ones — cannot land. Separately, `main` is one
cache eviction away from an unbuildable state, because the stale `<Analyzer Include>` resolves today
only through the workflow cache `restore-keys:` prefix fallback, and analyzers are silently disabled
in the 15 affected projects.

## Suspected Cause / Notes

Four distinct defects, in descending order of consequence:

- **D1 — out-of-scope project-file rewrites.** Dependabot writes `<Import>`/`<Error>` versions for
  packages outside the pull request's declared group, to a version no manifest declares. This is the
  direct cause of the build failure. Why it selects that version is an inference (its own restore
  most likely resolved the floating latest for that package id); the divergence and its consequence
  are verified.
- **D2 — `<Analyzer Include>` is never rewritten by anything.** Confirmed in `dependabot-core`'s
  `MSBuildNuGetProject` handling, which contains no analyzer-item logic, and confirmed empirically:
  #908 contains zero `<Analyzer Include>` lines. This is issue **#898**. The naive
  `analyzers\dotnet\cs\<Id>.dll` mapping is wrong for three of the five analyzer families in use —
  Meziantou uses `dotnet\roslyn5.0\cs`, Roslynator `dotnet\roslyn4.7\cs` (four mangled assemblies),
  SonarAnalyzer a bare `analyzers\` directory — so a repair must enumerate the restored package on
  disk rather than compute the path. The repair must also preserve the sibling
  `<AdditionalFiles ... BannedSymbols.txt>` element, since dropping it silently disables
  BannedApiAnalyzers.
- **D3 — formatting.** CSharpier formats both `packages.config` and `app.config`; Dependabot writes
  both inline. `.csharpierignore` currently excludes neither.
- **D4 — fan-out.** Four groups produce four pull requests. Grouping already consolidates across
  directories (#908 spans ten), so a single group yields a single pull request.

Adjacent defects folded in so that a repaired pipeline passes on its first run:

- **#898** — the 15 stranded `<Analyzer Include>` sites described above.
- **#902** — `scripts/vscode/Sync-PackageReferences.ps1` ranks `netstandard2.1` above
  `netstandard2.0`, which would reintroduce #895 on the next local build. `net481` cannot consume
  `netstandard2.1` at all. This script is the only one in `scripts/vscode/` with no Pester test file,
  which is why the defect went undetected. It runs from `Invoke-VSBuild.ps1` lines 250-253 before
  every local build and **never** in CI.
- **#903** — `ToDoModel.Test/packages.config` omits packages whose `.csproj` carries `<HintPath>`
  entries (confirmed: `FSharp.Core`, `Deedle`).

Execution constraint discovered during analysis: a push made with the default `GITHUB_TOKEN` does
not re-trigger workflows. An automated repair that pushes with it would leave the required checks red
on the pre-repair commit, so a self-fixing pull request requires a GitHub App installation token.

## Proposed Fix / Validation Ideas

Dependabot remains the upgrade engine — it already invokes the NuGet CLI update command, so
reimplementing it would duplicate the work and inherit D1. The capability set settled in the design
session is delivered as a **repair pass over Dependabot's own pull request** instead of as a
replacement pipeline.

- [ ] **Detection and consolidation.** Collapse the four groups to one so each cycle yields exactly
      one pull request; `open-pull-requests-limit: 1`; Deedle ignored entirely; the eight existing
      major-version ignores retained; the inert `group-by: "dependency-name"` keys removed.
- [ ] **Repair workflow**, triggered automatically on Dependabot pull requests, performing:
  - [ ] **Compatibility gate** — asset-level: a candidate passes only if it ships an asset `net481`
        can consume, with `netstandard2.1` excluded outright rather than merely ranked last. An
        incompatible package is skipped with a recorded reason and the remaining upgrades proceed.
  - [ ] **Version reconciliation (D1)** — every `<Import>`, `<Error>`, `<Reference>` and `<HintPath>`
        is forced to agree with the version its own `packages.config` declares.
  - [ ] **Analyzer-item repair (D2)** — `<Analyzer Include>` regenerated by enumerating the restored
        package directory, preserving sibling `<AdditionalFiles>` elements.
  - [ ] **Binding-redirect repair** — `app.config` redirects reconciled to the resolved assembly
        versions.
  - [ ] **Formatting (D3)** — CSharpier run over `packages.config` and `app.config`.
  - [ ] **Verifier** — repairs freely; fails only if the post-repair tree is still inconsistent.
  - [ ] **Disclosure** — a "Repairs applied" block added to the pull request body and a
        `deps:autofixed` label when a repair outside the known-weak classes was applied.
  - [ ] The repair commit is pushed onto Dependabot's existing branch, preserving the single-pull-request
        rule, using a GitHub App installation token so the required checks re-run.
- [ ] **Prerequisites** landed in the same change: #898, #902, #903, `.csharpierignore` coverage for
      both `packages.config` and `app.config`, all 18 manifests normalised once, and the NuGet CLI
      version pinned (currently floating in three workflows).
- [ ] Unit coverage: the compatibility evaluator, the version reconciler and the analyzer-path
      resolver are pure functions over parsed manifest and project state, unit-testable with Pester
      without network access.
- [ ] Integration scenario: replay the #908 divergence as a fixture and assert the repair produces a
      tree that builds.
- [ ] Manual verification: confirm `main` builds from a cold cache after the #898 correction.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
