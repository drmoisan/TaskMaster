# P1-T2 — Analyzer rebuild compiling the RED test against unchanged production code

Timestamp: 2026-08-28T03-47
Task: [P1-T2]
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal /fl "/flp:LogFile=docs\features\active\itemviewer-surface-defects-489\evidence\regression-testing\rem1-p1-t2-build.2026-08-28T03-46.msbuild.txt;Verbosity=normal"
EXIT_CODE: 0

Solution build with the **spaced** platform spelling `"/p:Platform=Any CPU"`, which is correct for
`TaskMaster.sln`. No single-`.csproj` build was performed, so the no-space `/p:Platform=AnyCPU` form
was not needed. No `/p:Nullable=enable` was passed. `/t:Rebuild` was used, never `/t:Build`.

**State of the tree at this build:** the RED test is present in
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs`; the production
`UnwireIntentEvents()` is **unchanged** and still performs 16 detachments. This build exists only to
compile the test against the defective production code so P1-T3 can observe a real assertion failure
rather than a compile error.

## Log

`FEATURE/evidence/regression-testing/rem1-p1-t2-build.2026-08-28T03-46.msbuild.txt`, 11982 lines,
normal verbosity, `.msbuild.txt` extension (not `.log`, which `.gitignore:84` would exclude from the
commit).

### Non-vacuity

| Signal | Value |
|---|---:|
| Occurrences of the literal `Skipping target "CoreCompile"` | **0** |
| `(Rebuild target)` entries in the file log | 10 |
| `csc.exe /noconfig` compiler invocations in the log | 18 |

Zero skip occurrences, and eighteen real compiler invocations, so `CoreCompile` ran and the analyzers
ran with it. The gate is not vacuous. The `(Rebuild target)` figure of 10 is the count in the **file
log**; the same figure quoted as 50 in `p11-t4-analyzer-build.2026-08-28T02-18.md` was taken from the
console output, which repeats project headers in its end-of-build summary. The two are not
inconsistent, they count different streams; 10 is what this file log contains and 10 is what the
identically-configured P1-T2 baseline log also contains when counted the same way.

### Warnings

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

Deduplicated warning count: **5**, not greater than the P0-T5 baseline of 5. All five are the same
pre-existing non-Roslyn advisory, one per project that still carries a `packages.config`:

| # | Project | Warning |
|---|---|---|
| 1 | `QuickFiler.csproj` | System.Reactive v7.0 `packages.config` unsupported-scenario advisory |
| 2 | `TaskMaster.csproj` | same |
| 3 | `ToDoModel.csproj` | same |
| 4 | `UtilitiesCS.csproj` | same |
| 5 | `UtilitiesCS.Test.csproj` | same |

Zero Roslyn analyzer diagnostics and zero errors. No `.csproj` analyzer entry was touched by this
remediation, so the repo-wide stale-analyzer-HintPath condition recorded as out-of-scope finding E1
remains cleared for this worktree and did not surface.

### Sanitisation

The log was sanitised in place before being recorded, case-insensitively:

| Token | Replacement | Occurrences |
|---|---|---:|
| worktree root | `<repo-root>` | 13631 |
| main checkout root | `<main-checkout-root>` | 36 |
| machine name | `<host>` | 0 |
| account name | `<user>` | 0 |

A post-sanitisation search for the worktree root, the main checkout root, the machine name and the
account name (both long and 8.3 forms, case-insensitive) returns **0** residual occurrences. The only
absolute paths left in the log are `C:\Program Files` and `C:\Program Files (x86)` — standard install
locations that carry no account or machine identity. This is the identical placeholder profile the
Phase 11 baseline log carries: 13631 `<repo-root>` and 36 `<main-checkout-root>`.

## Acceptance

| P1-T2 condition | Result |
|---|---|
| `EXIT_CODE: 0` | **Yes** — observed `0` |
| Zero occurrences of `Skipping target "CoreCompile"` | **Yes** — 0 |
| Deduplicated warning count not greater than 5 | **Yes** — 5, equal to the baseline |

Output Summary: The solution rebuilt clean with the new RED test compiled in and production code
untouched — `Build succeeded.`, `5 Warning(s)`, `0 Error(s)`, `EXIT_CODE: 0`. The gate is provably
non-vacuous: **zero** occurrences of `Skipping target "CoreCompile"` and 18 real `csc.exe`
invocations across 10 `(Rebuild target)` entries in the 11982-line normal-verbosity file log. The
deduplicated warning count is **5**, equal to and not greater than the P0-T5 baseline, and all five
are the pre-existing `System.Reactive` `packages.config` advisory across `QuickFiler`, `TaskMaster`,
`ToDoModel`, `UtilitiesCS` and `UtilitiesCS.Test`, with zero Roslyn diagnostics. The log is sanitised
to the established placeholder profile with zero residual host tokens.
