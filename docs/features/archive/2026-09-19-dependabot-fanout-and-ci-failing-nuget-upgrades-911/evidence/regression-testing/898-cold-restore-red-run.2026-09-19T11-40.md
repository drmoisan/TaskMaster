# Issue #898 — cold-restore failing control (pre-fix red run)

This artifact records the unfixed-tree failure that acceptance criteria AC5, AC6 and AC22 are
verified against. It exists so that the corresponding green run after the fix cannot be mistaken for
a gate that passes for an unrelated reason: the failing condition is demonstrated here to be
reachable from the environment the check runs in.

Timestamp: 2026-09-19T11:40:00Z

Worktree: `<execution-worktree-root>`
Branch: `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`, cut from `origin/main` at `734112ed2`
Condition: clean worktree, **cold package restore** (no CI cache, no pre-existing `packages/` tree)

## Step 1 — restore

Command: `nuget restore TaskMaster.sln`

EXIT_CODE: 0

Output Summary: `Installed: 172 package(s) to packages.config projects`. The restore honours
`packages.config`, so the only Meziantou package materialised is the one the manifests declare:

```
packages/Meziantou.Analyzer.3.0.235/
```

`packages/Meziantou.Analyzer.3.0.203/` does **not** exist. No manifest in the repository declares
that version, so no restore will ever produce it.

## Step 2 — build an affected project

Command:

```
msbuild VBFunctions\VBFunctions.csproj /t:Rebuild /p:Configuration=Debug "/p:Platform=AnyCPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 1

Output Summary:

```
CSC : error CS0006: Metadata file
'..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll'
could not be found [.../VBFunctions/VBFunctions.csproj]
```

## Finding

`origin/main` **does not build from a clean checkout with a clean restore.** This is stronger than
the position recorded in `issue.md`, which described the repository as one cache eviction away from
an unbuildable state. It is already unbuildable for any environment without a warm package cache:

- a fresh clone by a new contributor,
- a new git worktree,
- a CI runner after the `packages/` cache entry is evicted or its key changes.

CI is green today only because `.github/workflows/_*.yml` declare a bare-prefix
`restore-keys: nuget-${{ runner.os }}-` fallback, which restores a `packages/` tree populated under
an older `packages.config` hash. That tree still contains `Meziantou.Analyzer.3.0.203` from before
the manifests moved to `3.0.235`, so the stale `<Analyzer Include>` path resolves against a cached
artefact that no current manifest declares.

The cache comment in those workflows asserts that a fallback hit can only contribute "inert orphaned
version-folders for packages no longer referenced by any HintPath." That reasoning holds for
`<HintPath>` and `<Reference>`, which `nuget restore` reconciles. It does not hold for
`<Analyzer Include>`, which nothing reconciles, so the orphaned folder is not inert — it is
load-bearing.

## Secondary consequence

Because the analyzer assembly resolves from a stale package in the warm-cache case, the 15 affected
projects have been running an older Meziantou ruleset than their manifests declare. In the cold case
they do not compile at all. Either way the analyzer gate has not been asserting what it appears to
assert in those projects.

## Scope note

Exactly one line per project is stale — the `<Analyzer Include>` item. The `<Import>` and the
`EnsureNuGetPackageBuildImports` `<Error>` in the same files correctly reference `3.0.235`. That
asymmetry is the signature of the NuGet CLI update path, which writes the latter two and never the
former.
