# Research: CI/build-infrastructure debt (Issue #730)

- Feature folder: `docs/features/active/2026-09-02-ci-build-infra-debt-730/`
- Scope: two independent findings — (1) NuGet cache bare-prefix fallback with no
  restore-verification; (2) unsuppressed `System.Reactive` unsupported-`packages.config`
  warning.
- All evidence below was read directly from the working tree at
  `<repo-root>`
  on branch `bug/ci-build-infra-debt-730` (based on `origin/main`) on 2026-09-02.
- Both findings are configuration-only (workflow YAML / MSBuild property). No
  application source code was read or is proposed to change.

---

## Finding 1 — NuGet cache bare-prefix `restore-keys` fallback

### 1.1 Current state (confirmed)

All three named files carry byte-identical "Cache NuGet packages" / "Restore
solution" step pairs:

- `.github/workflows/_build-analyzers.yml:35-45`
- `.github/workflows/_build-nullable.yml:35-45`
- `.github/workflows/_mstest-coverage.yml:35-45`

```yaml
      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}
          restore-keys: |
            nuget-${{ runner.os }}-

      - name: Restore solution
        shell: pwsh
        run: nuget restore $env:SOLUTION_PATH
```

`nuget restore` is invoked **unconditionally** in all three files, immediately
after the cache step and regardless of whether the cache step reports an
exact-key hit, a `restore-keys` fallback hit, or a full miss (no
`if: steps.cache.outputs.cache-hit == 'true'` gate exists on any step in any of
the three files).

#### Numeric Derivation Evidence — "exactly 3 workflow files carry this pattern"

- **Complete Family**: every `.github/workflows/*.yml` step named "Cache NuGet
  packages" that feeds the classic `nuget restore $env:SOLUTION_PATH`
  (packages.config-style) restore.
- **Exhaustive Search Scope**: all files under `.github/workflows/`.
- **Inclusion Rules**: step is named `Cache NuGet packages` (or an equivalent
  variant) and its cached `path:` is the classic `packages` directory feeding a
  `nuget restore` of the solution.
- **Exclusion Rules**: cache steps for unrelated tooling (e.g., the .NET SDK
  global-tools NuGet cache used by `dotnet tool restore`).
- **Primary Search Strategy**: `grep -r "name: Cache NuGet packages" .github/workflows/`
- **Primary Member Set**: `_mstest-coverage.yml`, `_build-analyzers.yml`,
  `_build-nullable.yml`
- **Primary Count**: 3
- **Cross-check Search Strategy**: `grep -r "restore-keys:" .github/workflows/`
  (broader query — any bare-prefix cache fallback in the workflow set, not
  restricted to the "Cache NuGet packages" step name)
- **Cross-check Member Set**: `_format-check.yml`, `_mstest-coverage.yml`,
  `_build-analyzers.yml`, `_build-nullable.yml` (4 files)
- **Cross-check Count**: 4
- **Member-set Comparison**: The cross-check surfaces one extra file,
  `_format-check.yml:27-33`. Inspection shows its cache step is named "Cache
  dotnet tools", caches `~/.nuget/packages` under key
  `dotnet-tools-${{ runner.os }}-${{ hashFiles('dotnet-tools.json') }}`, and
  feeds `dotnet tool restore` (the CSharpier/SDK-style tool manifest restore),
  not `nuget restore $env:SOLUTION_PATH` against `packages.config`. It is a
  structurally different cache (SDK-style global-packages-folder restore, not
  the classic packages.config `packages/` directory) and is **not** one of the
  three files named in `issue.md`/`spec.md`. After excluding it on that basis,
  the cross-check member set reduces to exactly the primary member set (3
  files, same names). The two searches agree once the exclusion rule is
  applied; the numeric claim "3 workflow files" is confirmed.

### 1.2 Is the "stale package tree" risk real? (investigated, refuted)

The finding's premise is that a `restore-keys` fallback hit could let a build
"silently run against stale package versions." This premise does **not** hold
for this specific combination (packages.config + classic `nuget.exe restore`
+ an unconditional restore step), for a structural reason confirmed against
this repository's own project files:

1. **Packages are stored in version-qualified directories.** Every
   `packages.config` entry pins an exact version (e.g.
   `<package id="System.Reactive" version="7.0.0" .../>` at
   `TaskMaster/packages.config:71`), and every consuming `.csproj` resolves
   that package via a `HintPath` that embeds the same version string, e.g.
   `TaskMaster\TaskMaster.csproj:270`:
   `<HintPath>..\packages\System.Reactive.7.0.0\lib\net472\System.Reactive.dll</HintPath>`.
   The on-disk restore target for any given package+version is always
   `packages\{id}.{version}\...` — never a version-agnostic path.
2. **`nuget.exe restore` for packages.config projects is a per-package,
   existence-checked, idempotent operation.** For each entry in
   `packages.config`, it checks whether `packages\{id}.{version}\` is already
   present and valid; if so it is left alone, if not it is fetched from the
   configured feed and extracted. It does not consult or trust the cache
   wholesale, and it does not need cache metadata to decide correctness — the
   version string embedded in the directory name **is** the correctness check.
3. **Consequence:** whatever a bare-prefix `restore-keys` fallback happens to
   restore into `packages/` (content from some *other*, differently-hashed
   prior cache entry) can only ever contain one of two things relative to the
   *current* `packages.config`:
   - version-folders that exactly match a current entry (a legitimate,
     desired reuse — this is the entire point of having a fallback tier), or
   - orphaned version-folders for packages that are no longer referenced by
     the current `packages.config` (because they were bumped or removed) —
     these are inert; no current `.csproj` `HintPath` or NuGet `<Import>`
     points at them, so they cannot be silently built against.
   Any package whose version *changed* in the current `packages.config` will
   have no matching folder in the stale fallback content, so `nuget restore`
   fetches exactly that delta from the network before the build step runs.
   There is no code path by which the fallback cache can cause the build to
   see a version of a package other than the one named in the current
   `packages.config`.
4. **Corroborating repo-local precedent for the underlying restore-target
   convention:** the `<Import>`/`<Error Condition="!Exists(...)">` pairs the
   repo already uses for every NuGet-package-with-build-assets dependency
   (e.g. `TaskMaster\TaskMaster.csproj:565,582` for `System.Reactive.7.0.0`)
   are themselves conditioned on the exact version-qualified path existing —
   i.e., the repository's own build files already assume and depend on this
   version-folder-is-the-correctness-check convention; a bare-prefix fallback
   cannot violate an invariant the `.csproj` files themselves enforce.
5. **Scope note:** this reasoning is specific to classic `nuget.exe restore`
   against `packages.config` (all three named workflows). It would not
   automatically transfer to a `dotnet restore`/`PackageReference`/lock-file
   restore model, which has different (though also idempotent) semantics —
   not relevant here since none of the three named workflows or the five
   `System.Reactive`-consuming projects use `PackageReference`.
6. **What is NOT confirmed from repo-local evidence alone:** the exact
   "most-recently-created-cache-entry-wins" tie-break behavior of GitHub
   Actions' `restore-keys` prefix matching is documented `actions/cache`
   behavior, not something verifiable by reading files in this repository. It
   does not change the conclusion above (the conclusion holds regardless of
   *which* prior cache entry is selected, because it holds for any content the
   fallback could plausibly contain).

**Conclusion:** the "stale package tree silently used" risk described in the
issue is not realizable in this configuration. `nuget restore`'s per-package,
version-folder-scoped idempotency, combined with the fact that `nuget restore`
runs unconditionally on every job regardless of cache-hit tier, already
guarantees the build always resolves the exact package versions named in the
current `packages.config` before compiling.

### 1.3 Candidate fixes

**(a) Remove the bare-prefix `restore-keys` fallback entirely.**
Correct in isolation, but it is a **regression with no offsetting benefit**:
since the risk it "closes" is not real, removing it only removes the
legitimate reuse case (§1.2 item 3, first bullet) — every `packages.config`
change (even a single-package bump) would force a full from-network restore
of every package in every one of the three jobs, increasing CI time and
external network calls for zero correctness gain.

**(b) Add an explicit restore-verification step after `nuget restore`.**
This would add a new script step (and thus new code to maintain, per the
General Code Change Policy's "smallest correct fix" and file-size/complexity
guidance) whose entire job is to re-verify an invariant that `nuget.exe`'s own
restore logic already enforces structurally (§1.2). It is redundant
verification of a already-guaranteed invariant, not a fix for a real defect.
It also does not match "smallest fix": it is strictly more moving parts than
option (c) below for no additional correctness coverage.

**(c) Recommended — no functional change to the cache/restore mechanics;
document why the fallback is safe.**
Add an inline YAML comment directly above `restore-keys:` in each of the three
files explaining the version-folder-scoped idempotency argument from §1.2, so
a future reader (or a future policy audit) does not have to re-derive it from
scratch, and so the state changes from "silently tolerated, unexamined" to
"deliberately kept, with a documented rationale" — the same resolution shape
`spec.md`'s "Expected Behavior" already asks for ("a deliberately-accepted
state, documented, rather than an unacknowledged risk"), applied here as "the
existing behavior is already correct, and that correctness is now
documented" rather than as a suppression.

### 1.4 Recommendation and literal replacement text

**Recommended fix: option (c).** No behavior change to any of the three
workflow files' cache/restore steps. Add the following comment block
immediately above the `restore-keys:` key in each of the three files (comment
text identical across all three; only the surrounding file differs):

Exact replacement for `.github/workflows/_build-analyzers.yml:35-41`,
`.github/workflows/_build-nullable.yml:35-41`, and
`.github/workflows/_mstest-coverage.yml:35-41` (identical in all three files):

```yaml
      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}
          # The bare-prefix fallback below is safe against stale package
          # versions: `nuget restore` (next step) always runs unconditionally
          # and is idempotent per package for packages.config-style restores —
          # each package is materialized under a version-qualified directory
          # (packages/{id}.{version}/, matching every HintPath in this repo's
          # .csproj files). A fallback cache populated under an older
          # packages.config hash can therefore only ever contribute either
          # (a) version-folders that still match the current packages.config
          # (a legitimate, desired reuse) or (b) inert orphaned version-
          # folders for packages no longer referenced by any HintPath. Either
          # way, `nuget restore` fetches exactly the delta implied by the
          # current packages.config from the network before the build step
          # runs, so a fallback hit can never cause the build to compile
          # against a package version other than the one packages.config
          # names. See docs/features/active/2026-09-02-ci-build-infra-debt-730/
          # research/ for the full analysis (issue #730).
          restore-keys: |
            nuget-${{ runner.os }}-

      - name: Restore solution
        shell: pwsh
        run: nuget restore $env:SOLUTION_PATH
```

If the orchestrator or maintainer prefers defense-in-depth over the
documentation-only fix (e.g., to guard against a future change to
`nuget.exe`'s restore semantics, unrelated to any currently-real risk), option
(b) is the fallback choice — but it should be scoped as "add a redundant
safety net for a currently-unconfirmed future risk," not as "fix a confirmed
correctness bug," since §1.2 shows there is no bug today.

Out of scope, noted for completeness only: `.github/workflows/_format-check.yml:27-33`
carries a structurally similar bare-prefix `restore-keys` fallback for a
different cache (`~/.nuget/packages`, keyed by `dotnet-tools.json`, feeding
`dotnet tool restore`). It is not one of the three files named in
`issue.md`/`spec.md` and was not analyzed for correctness here; the same
idempotency argument likely applies (NuGet's global-packages-folder restore
target is also version-qualified: `~/.nuget/packages/{id}/{version}/`) but
this was not independently confirmed against that specific restore code path
and should not be assumed in-scope for this issue's fix.

---

## Finding 2 — unsuppressed `System.Reactive` unsupported-`packages.config` warning

### 2.1 Current state (confirmed)

`System.Reactive 7.0.0` is pinned via `packages.config` in exactly five
project directories, each a direct child of the repository root (no
intervening directory level):

- `QuickFiler\packages.config:75-76` — `System.Reactive 7.0.0` +
  `System.Reactive.Async 6.0.0-alpha.18`
- `TaskMaster\packages.config:71-72` — `System.Reactive 7.0.0` +
  `System.Reactive.Async 6.0.0-alpha.18`
- `ToDoModel\packages.config:43` — `System.Reactive 7.0.0` only (no
  `System.Reactive.Async` entry)
- `UtilitiesCS\packages.config:161-162` — `System.Reactive 7.0.0` +
  `System.Reactive.Async 6.0.0-alpha.18`
- `UtilitiesCS.Test\packages.config:198-199` — `System.Reactive 7.0.0` +
  `System.Reactive.Async 6.0.0-alpha.18`

Each of the same five `.csproj` files also carries, near the bottom of the
file, an unconditional `<Import>` of the package's own build-time targets:

```xml
<Import Project="..\packages\System.Reactive.7.0.0\build\System.Reactive.targets"
        Condition="Exists('..\packages\System.Reactive.7.0.0\build\System.Reactive.targets')" />
```
(confirmed at `QuickFiler\QuickFiler.csproj:603`, `TaskMaster\TaskMaster.csproj:582`,
`ToDoModel\ToDoModel.csproj:200`, `UtilitiesCS\UtilitiesCS.csproj:1312`,
`UtilitiesCS.Test\UtilitiesCS.Test.csproj:970`).

#### Numeric Derivation Evidence — "exactly 5 affected project directories"

- **Complete Family**: every project directory in the repository whose
  `packages.config` references `System.Reactive` (any version) and whose
  `.csproj` therefore imports the package's `PackagesConfigCheck` guard.
- **Exhaustive Search Scope**: all `*/packages.config` files repo-wide (18
  found via `Glob **/packages.config`) and all `*.csproj` files repo-wide (18
  found via `Glob **/*.csproj`).
- **Inclusion Rules**: `packages.config` contains a `<package id="System.Reactive" .../>`
  line (any version), regardless of `System.Reactive.Async` presence.
- **Exclusion Rules**: none — every `packages.config` in the repo was searched,
  not a pre-selected subset.
- **Primary Search Strategy**: `grep -n 'package id="System.Reactive"' **/packages.config`
- **Primary Member Set**: `UtilitiesCS\packages.config`,
  `UtilitiesCS.Test\packages.config`, `ToDoModel\packages.config`,
  `TaskMaster\packages.config`, `QuickFiler\packages.config`
- **Primary Count**: 5
- **Cross-check Search Strategy**: `grep -n 'Reference Include="System.Reactive,' *.csproj`
  (independent overload family — the compiled-reference declaration in the
  `.csproj` itself, not the `packages.config` package-pin declaration)
- **Cross-check Member Set**: `UtilitiesCS\UtilitiesCS.csproj`,
  `UtilitiesCS.Test\UtilitiesCS.Test.csproj`, `ToDoModel\ToDoModel.csproj`,
  `TaskMaster\TaskMaster.csproj`, `QuickFiler\QuickFiler.csproj`
- **Cross-check Count**: 5
- **Member-set Comparison**: normalizing both sets to project-directory names
  (`UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel`, `TaskMaster`, `QuickFiler`)
  yields an identical 5-member set from both independent search strategies.
  This matches the five projects already named in `issue.md`/`spec.md` and in
  the two prior promoted-potential documents for issues #395 and #570 (see
  §2.4). The numeric claim "5 affected projects" is confirmed.

### 2.2 Vendor mechanism confirmation

The property name and its effect (`RxUseUnsupportedPackagesConfig=true`
suppresses the guard warning) is confirmed from two independent repo-local
sources, both predating this issue:

1. **Verbatim vendor warning text**, captured from a real local build and
   committed to the repo at
   `docs/features/potential/promoted/2026-08-15-system-reactive-7-packages-config-unsupported.md:44-50`:
   ```
   packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5):
   warning : The project contains a packages.config file, which is not supported by
   System.Reactive v7.0 or later. Please migrate to PackageReference. (You can
   suppress this message by setting the RxUseUnsupportedPackagesConfig property to
   true, but be aware this is an unsupported scenario.)
   ```
   This confirms the guard target's exact repo-local path
   (`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets`),
   which is chained from the unconditional `<Import>` of `System.Reactive.targets`
   confirmed in each `.csproj` (§2.1) — i.e., the guard target *is* reachable
   from every one of the five projects' normal build, not merely a
   theoretical package feature.
2. **Independent prior research note**, `docs/features/potential/promoted/2026-07-20-system-reactive-7-packages-config-migration.md:19`,
   which — a month earlier, from a different investigation of the same
   package/version pair — independently names "the documented
   `RxUseUnsupportedPackagesConfig=true` escape hatch" as one of two accepted
   remediation paths.

The `packages/System.Reactive.7.0.0/` directory itself is **not present** in
this worktree (`packages/` is not restored here — confirmed via
`Glob packages/**` returning no results), so the package's `.targets` source
could not be read directly in this session. The two independent repo-local
sources above (vendor warning text captured from a real prior build, plus an
independent prior researcher's note naming the same property) are the
evidence this finding relies on; both agree on the property name and its
"suppress-the-warning" effect. This is not a from-scratch fabrication of the
mechanism — it is corroborated, but not a live read of the vendor `.targets`
file's XML in this session.

### 2.3 `Directory.Build.props` auto-import applicability (confirmed)

- **No `Directory.Build.props` exists anywhere in the repository today**
  (`Glob **/Directory.Build.props` → no results).
- **A `Directory.Build.targets` file already exists at the repository root**
  (`Directory.Build.targets:1-31`) and is **already relied upon in
  production** for the `TaskMaster` project specifically: it conditions a
  `PropertyGroup` on `'$(MSBuildProjectName)' == 'TaskMaster'` to disable VSTO
  manifest/assembly signing under `$(CI) == 'true'`, and a
  `BeforeTargets="ResolveKeySource"` target for developer (non-CI) builds.
  This is direct, repo-local, already-working proof that the
  `Directory.Build.*` auto-import mechanism **is** functioning correctly for
  these legacy non-SDK `.csproj` files — the same mechanism that would import
  a root `Directory.Build.props`.
- **No `RxUseUnsupportedPackagesConfig` property exists anywhere in the repo
  today**, in any `.csproj`, `.props`, `.targets`, or `.config` file
  (`grep -r RxUseUnsupportedPackagesConfig` repo-wide returns only prose hits
  inside Markdown research/evidence documents, never inside a build file).
- **No `ImportDirectoryBuildProps` (or similarly-named import-disabling
  property) exists anywhere in the repository** (`grep -r ImportDirectoryBuildProps`
  and a broader `grep -r "Directory.Build|ImportDirectoryBuildProps|ImportProjectExtensionProps"`
  restricted to `*.csproj` both return no results).
- **No `NuGet.Config` file exists in the repository** (`Glob NuGet.Config` →
  no results), so there is no competing global MSBuild-property injection
  mechanism to reconcile with.
- All five affected project files (`QuickFiler.csproj`, `TaskMaster.csproj`,
  `ToDoModel.csproj`, `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`) are
  legacy non-SDK-style projects (`<Project ToolsVersion="15.0"|"17.0" ...>`,
  no `Sdk="..."` attribute) that each explicitly
  `<Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" .../>`
  near the top of the file (confirmed at
  `UtilitiesCS\UtilitiesCS.csproj:6`, `QuickFiler\QuickFiler.csproj:4`,
  `TaskMaster\TaskMaster.csproj:3`, `ToDoModel\ToDoModel.csproj:4`,
  `UtilitiesCS.Test\UtilitiesCS.Test.csproj:8`). The `Directory.Build.props`
  auto-import search-and-import logic lives inside `Microsoft.Common.props`
  itself (standard MSBuild behavior since MSBuild 15.5 / VS 2017.3, applying
  equally to SDK-style and classic non-SDK projects that import
  `Microsoft.Common.props`) — this is standard, well-established MSBuild
  behavior; the point specific to this repository (that these five *legacy*
  project files actually exercise it) is independently confirmed by the
  already-working `Directory.Build.targets` precedent above, not merely
  assumed from general MSBuild documentation.
- **Import ordering is compatible with the fix.** `Directory.Build.props` is
  imported at the point `Microsoft.Common.props` is imported — i.e., near the
  **top** of each `.csproj`, before that project's own `PropertyGroup` blocks
  and, critically, before the package's own guard-target import
  (`<Import Project="..\packages\System.Reactive.7.0.0\build\System.Reactive.targets" .../>`),
  which sits near the **bottom** of each of the five `.csproj` files (line
  numbers in §2.1). A property set in `Directory.Build.props` is therefore
  guaranteed to be defined before the `PackagesConfigCheck` guard target
  evaluates its condition on `RxUseUnsupportedPackagesConfig`.

**No reason was found for a root-level `Directory.Build.props` to fail to
apply to all five projects.** There is no competing/closer
`Directory.Build.props`, no per-project import-disabling property, and the
existing `Directory.Build.targets` precedent demonstrates the mechanism is
already live for at least one of the five (`TaskMaster`) in this exact
project family.

One structural note (not a blocker): a repository-root `Directory.Build.props`
is also picked up by the other, non-`System.Reactive`-consuming project
directories in the solution (`SVGControl`, `Tags`, `TaskTree`,
`TaskVisualization`, `VBFunctions`, and their `.Test` counterparts — 13
projects total beyond the five named here, all direct children of the repo
root per the `Glob **/*.csproj` results in §2.1's search). Setting
`RxUseUnsupportedPackagesConfig=true` there is harmless for those projects:
the property is only consumed by `System.Reactive.PackagesConfigCheck.targets`,
which none of the other thirteen projects import (none reference
`System.Reactive` — confirmed by the same `packages.config`/`.csproj` grep in
§2.1 numeric derivation returning exactly the five named projects and no
others).

### 2.4 Candidate fixes

**(a) Root-level `Directory.Build.props`, single new file.** Sets the
property once; all five affected projects (and, harmlessly, the other
thirteen) pick it up via the confirmed auto-import mechanism. Smallest
possible change (one new 6-line file, zero edits to any existing file).

**(b) Edit each of the five `.csproj` files individually**, adding a
`<PropertyGroup><RxUseUnsupportedPackagesConfig>true</RxUseUnsupportedPackagesConfig></PropertyGroup>`
block to each. Achieves the same effect with five times the edit surface and
five places to keep the accompanying rationale comment in sync, for no
additional correctness benefit given §2.3 found no reason the root-level
import would fail.

**(c) Migrate the five projects to `PackageReference`** (the alternative the
vendor warning itself suggests). Rejected as out of scope for this issue: both
prior promoted-potential documents (`2026-07-20-system-reactive-7-packages-config-migration.md:18,29`
and this issue's own `spec.md:60`) already flag this as a materially larger,
conflicting change — it contradicts the repository's documented convention of
keeping legacy non-SDK VSTO projects on `packages.config` with file-based
`<Analyzer Include>` items, and risks breaking `HintPath`/binding-redirect/VSTO
manifest behavior that depends on the `packages\` folder layout.

**(d) Pin `System.Reactive` back to a 6.x release.** Also rejected as out of
scope here: it is a dependency-version rollback, not a build-infrastructure
configuration fix, and both prior promoted-potential documents present it as
an alternative decision for the *original* #395/#570 issues, not as something
this consolidated infra-debt issue should re-litigate.

### 2.5 Recommendation and literal file content

**Recommended fix: option (a).** Create a new file at the repository root:

`Directory.Build.props`:

```xml
<Project>
  <!--
    System.Reactive 7.0.0+ refuses to build cleanly against packages.config
    projects (see System.Reactive.PackagesConfigCheck.targets) and instead
    emits an "unsupported scenario" warning on every build of every project
    that references it. This repository intentionally keeps its legacy
    non-SDK VSTO / .NET Framework 4.8.1 projects on packages.config (see
    .claude/rules/csharp.md) rather than migrating to PackageReference, so
    the warning is accepted here as a known, deliberate trade-off rather than
    fixed by migration. RxUseUnsupportedPackagesConfig=true is the package's
    own documented suppression switch for this exact scenario. See issue #730
    and docs/features/active/2026-09-02-ci-build-infra-debt-730/ for the
    accepted-trade-off rationale.
  -->
  <PropertyGroup>
    <RxUseUnsupportedPackagesConfig>true</RxUseUnsupportedPackagesConfig>
  </PropertyGroup>
</Project>
```

No edits to any of the five `.csproj` files, any `packages.config`, or
`Directory.Build.targets` are required for this fix.

---

## Testing implications (both findings)

Both findings are build/CI-pipeline configuration changes with no application
source code touched, so MSTest/Moq/FluentAssertions unit-test coverage is not
applicable to either fix directly. The verification strategy consistent with
repository policy is:

- **Finding 1 (comment-only change):** no build-behavior change, so the
  existing toolchain gates (`_build-analyzers.yml`, `_build-nullable.yml`,
  `_mstest-coverage.yml`) continuing to pass on the PR is the only expected
  evidence; the change is verifiable by re-reading the three files' diffs
  (comment-only) and confirming YAML validity (e.g., `_format-check.yml`'s
  existing CSharpier/YAML-adjacent checks, or a simple `yamllint`/GH Actions
  workflow-syntax check if available) rather than by a new test.
- **Finding 2 (`Directory.Build.props` addition):** the verifiable outcome is
  a warning-count change, not a test-pass/fail change. Evidence should be a
  full local rebuild transcript (`msbuild TaskMaster.sln /t:Rebuild /m
  /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true
  /p:EnforceCodeStyleInBuild=true` and the nullable-errors variant) captured
  before and after the change, confirming the five
  `System.Reactive.PackagesConfigCheck` warnings are gone and no new warnings
  or errors were introduced. This is a build-log diff, not a unit test, and
  should be captured as evidence under
  `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/` per the
  evidence-and-timestamp-conventions skill, not invented as a new test file.
- Existing MSTest suites in `UtilitiesCS.Test` and `QuickFiler.Test` (the
  Rx-dependent test assemblies) should be re-run once after the
  `Directory.Build.props` addition to confirm no behavior change — this is
  regression re-verification of existing tests, not new test authorship,
  consistent with a configuration-only change.

---

## Summary for spec-writing / atomic-planning agents

| Finding | Files touched | Nature of change | Behavior change? |
|---|---|---|---|
| 1 | `.github/workflows/_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml` (3 files) | Add explanatory YAML comment above `restore-keys:` in each | No — the fallback was already correct; this documents why |
| 2 | New file `Directory.Build.props` at repo root (1 file) | Add `RxUseUnsupportedPackagesConfig=true` property with rationale comment | Yes — suppresses 5 build warnings; no functional/runtime change |

Both fixes are additive/comment-only from an MSBuild-evaluation-semantics
perspective (Finding 1 adds YAML comments with zero effect on the cache/restore
steps; Finding 2 adds a new property that only ever gates a warning message,
never a build target's execution or output). Neither fix requires touching
any of the five affected `.csproj` files, `packages.config` files, or any
application source file.
