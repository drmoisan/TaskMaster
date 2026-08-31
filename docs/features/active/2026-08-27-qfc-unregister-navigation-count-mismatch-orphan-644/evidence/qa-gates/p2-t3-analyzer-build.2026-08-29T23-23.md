# [P2-T3] — Analyzer Build Gate

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P2-T3]
Working directory: `<repo-root>` (the repository root of this worktree)
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0

Redaction note: no absolute host path, account name, or machine name appears in this artifact.
The repository root is written as `<repo-root>` and per-project paths are written
repository-relative, following the convention already used by
`evidence/qa-gates/p4-t5-vstest-final.2026-08-29T08-15.md`. This is load-bearing here because
the worktree root is itself an absolute path under the user profile and msbuild echoes full
project paths in its per-project output.

`/t:Rebuild` was used, not `/t:Build`. A warm `/t:Build` skips `CoreCompile` through MSBuild
incrementality and runs no analyzers. The log records 57 `CoreCompile` invocations, confirming
the compilation actually ran rather than being skipped.

## First run — FAILED on a pre-existing repository condition

The first invocation returned `EXIT_CODE: 1` with `0 Warning(s)` and `10 Error(s)`. Every error
was `CS0006`, raised by `UtilitiesCS.csproj` and `VBFunctions.csproj`, of the form:

```
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found
CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll' could not be found
```

Diagnosis. `UtilitiesCS.csproj` and `VBFunctions.csproj` carry an internal version skew: their
`<Import>` and `<Error Condition>` items name `Meziantou.Analyzer.3.0.174`, while their
`<Analyzer Include>` HintPaths still name `Meziantou.Analyzer.3.0.156` and
`Roslynator.Analyzers.4.16.0`. A NuGet restore therefore installs 3.0.174 and 4.16.1, exactly
as `packages.config` asks, and `csc` is then handed HintPaths to two versions that were never
installed.

This condition is pre-existing and is not introduced by this branch or this cycle:

- The skew is present in the tracked `UtilitiesCS.csproj` at the cycle-entry head
  `a2c69aead286ad0ec6c7087f1bd8c46d39d0d472`, verified with `git show <head>:UtilitiesCS/UtilitiesCS.csproj`.
- The identical skew is present on `origin/main`, verified with `git show origin/main:UtilitiesCS/UtilitiesCS.csproj`.
- `git diff --name-only origin/main...<head> -- '*.csproj' '*.config'` lists only
  `QuickFiler.Test/QuickFiler.Test.csproj`, so this branch never touched either skewed project.
- This cycle's own edits are comment and string-literal text in one test file and cannot
  produce `CS0006`.

The condition only surfaces in a cold worktree. A long-lived worktree accumulates package
directories across bumps, and `nuget restore` never deletes superseded ones, so the older
3.0.156 and 4.16.0 folders remain present and the stale HintPaths still resolve. This worktree
had no `packages/` directory at all at cycle entry.

## Resolution — environment provisioning only, no tracked file changed

The two package versions the HintPaths name were installed into the gitignored `packages/`
directory:

```
nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages -DependencyVersion Ignore
nuget install Roslynator.Analyzers -Version 4.16.0 -OutputDirectory packages -DependencyVersion Ignore
```

Both exited 0. This reproduces the build inputs a warm worktree would already hold. It is
environment provisioning of the same class as installing the repo-local .NET SDK and running
the packages.config restore, both of which this cycle also had to perform in this cold
worktree.

No project file was edited. `UtilitiesCS.csproj` and `VBFunctions.csproj` are outside this
plan's change footprint, and repairing their HintPaths would be a new independent outcome no
task in this plan describes. Confirmation:

- `git status --porcelain -- '*.csproj' '*.config' '*.props' '*.targets'` returns empty.
- The repository-wide porcelain line count was 16 before and 16 after the provisioning.

The skew remains an open pre-existing repository defect and is reported to the orchestrator as
a finding rather than fixed here.

## Second run — PASSED

Per the plan's restart rule, the loop restarted from `[P2-T1]`. `[P2-T1]` and `[P2-T2]` were
re-run and passed again (recorded in their own artifacts), then this gate was re-run.

Final summary block:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

EXIT_CODE: 0

### Warning and error counts

- Errors: `0` — matching the `0 Error(s)` the acceptance requires.
- Warnings: `5`.

All five warnings are the same pre-existing advisory, emitted once per affected project by a
package-supplied targets file:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference.
```

The five affected projects are `QuickFiler.csproj`, `TaskMaster.csproj`, `ToDoModel.csproj`,
`UtilitiesCS.csproj`, and `UtilitiesCS.Test.csproj`. None originates in
`QuickFiler.Test.csproj`, none is an analyzer diagnostic, and none relates to this cycle's
change, which is comment and string-literal text only.

### Build outputs

`QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` was produced, which `[P2-T5]` requires as its
test source.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | `0` | `0` | PASS |
| msbuild final-summary error count | `0 Error(s)` | `0 Error(s)` | PASS |
| msbuild final-summary warning count | recorded | `5 Warning(s)` | RECORDED |

## Output Summary

The analyzer gate failed on its first run with `EXIT_CODE 1` and 10 `CS0006` errors traced to a
pre-existing analyzer HintPath skew in `UtilitiesCS.csproj` and `VBFunctions.csproj` that is
also present on `origin/main` and that this branch never touched. The two HintPath-referenced
package versions were provisioned into the gitignored `packages/` directory, changing no
tracked file, and the toolchain loop restarted from `[P2-T1]` as the plan directs. On the
second run the gate passed: `Build succeeded`, `EXIT_CODE 0`, `0 Error(s)`, `5 Warning(s)`, all
five the same pre-existing System.Reactive packages.config advisory unrelated to this change.
57 `CoreCompile` invocations confirm analyzers actually ran.
