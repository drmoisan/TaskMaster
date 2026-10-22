# QA Gate — Build-Configuration Footprint (Issue #656)

Timestamp: 2026-09-01T14-55
Task: [P4-T12]
Satisfies: AC-11

Command (authoritative):
```
git diff --name-only origin/main...HEAD -- '*.csproj' '*.props' '*.targets' '*packages.config'
git status --porcelain -- '*.csproj' '*.props' '*.targets' '*packages.config'
```

EXIT_CODE: 0

## Authoritative diff output (base `origin/main`, verbatim)

```
```

(empty)

## Porcelain output (verbatim)

```
```

(empty)

Both outputs are empty, which is the acceptance condition. AC-11 is satisfied: no `.csproj`,
`.props`, `.targets` or `packages.config` path appears in this item's change set, and none is
modified or untracked in the working tree.

This is a meaningful result rather than a trivially empty one. Two mechanisms could have added a
build-configuration file to this change set and neither did:

- The new test method was appended to an **existing** test file,
  `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`. Creating a new `.cs`
  file would have required a `<Compile Include>` entry in `QuickFiler.Test.csproj`, because these
  are legacy non-SDK projects that enumerate their sources explicitly. Appending to the existing
  partial avoided that entirely.
- The plan's execution preconditions forbid `scripts/vscode/Invoke-VSBuild.ps1`, which calls
  `Sync-PackageReferences.ps1` over every `.csproj` and rewrites `HintPath` values. Every msbuild
  invocation in this run resolved msbuild through vswhere directly instead, so no build script
  rewrote a project file as a side effect. The NuGet restore in P0-T4 used
  `scripts/vscode/Invoke-Restore.ps1`, which populates `packages/` without editing project files.

## Base-ref substitution (recorded, not silent)

Against the plan's stale pinned base the same query is **not** empty:

```
git diff --name-only 2b85134b42872e405602e6064e02dc9cda6c319b...HEAD -- '*.csproj' '*.props' '*.targets' '*packages.config'
QuickFiler.Test/QuickFiler.Test.csproj
```

That single path is a pre-existing change that arrived on `main` and was merged into this branch
before execution began; it is not this item's work. Its presence is precisely why the pinned base
cannot be used to evaluate AC-11: it would report a build-configuration edit that this item did not
make. `origin/main` (`5670b3cfe6a52e3b890bf80f0cd85a20d4fe4723`, an ancestor of HEAD) isolates this
branch's own contribution and is used as authoritative. Both measurements are recorded so the
substitution is auditable.

Output Summary: Build-configuration footprint verified empty against `origin/main` for both the
anchored diff and the porcelain status. AC-11 is satisfied. The plan's pinned base reports one
unrelated `.csproj` inherited from `main`, recorded here for audit.
