# P2-T2 — Analyzer rebuild after the production fix

Timestamp: 2026-08-28T03-50
Task: [P2-T2]
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal /fl "/flp:LogFile=docs\features\active\itemviewer-surface-defects-489\evidence\regression-testing\rem1-p2-t2-build.2026-08-28T03-50.msbuild.txt;Verbosity=normal"
EXIT_CODE: 0

Identical command shape to P1-T2 — solution build, `/t:Rebuild`, spaced `"/p:Platform=Any CPU"`, no
`/p:Nullable=enable` — differing only in the log filename. The tree now carries **both** the RED test
and the P2-T1 production fix, so this build compiles the corrected `UnwireIntentEvents()`.

## Log

`FEATURE/evidence/regression-testing/rem1-p2-t2-build.2026-08-28T03-50.msbuild.txt`, 11948 lines,
normal verbosity, `.msbuild.txt` extension.

### Non-vacuity

| Signal | Value |
|---|---:|
| Occurrences of the literal `Skipping target "CoreCompile"` | **0** |
| `(Rebuild target)` entries | 10 |
| `csc.exe /noconfig` compiler invocations | 18 |

Zero skips and eighteen real compiler invocations: `CoreCompile` ran on every project and the
analyzers ran with it.

### Warnings

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

Deduplicated warning count: **5**, not greater than the P0-T5 baseline of 5, and identical to the
P1-T2 figure. The set is unchanged — the same `System.Reactive` `packages.config` advisory on
`QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS` and `UtilitiesCS.Test`. The added detachment
line introduced **no** new analyzer diagnostic, which is the expected outcome for a statement that
mirrors the sixteen already in the member.

### Comparison against P1-T2

| Signal | P1-T2 (pre-fix) | P2-T2 (post-fix) |
|---|---:|---:|
| `EXIT_CODE` | 0 | 0 |
| Errors | 0 | 0 |
| Deduplicated warnings | 5 | 5 |
| `Skipping target "CoreCompile"` | 0 | 0 |
| `csc.exe` invocations | 18 | 18 |

The one-line production change is diagnostic-neutral.

### Sanitisation

| Token | Replacement | Occurrences |
|---|---|---:|
| worktree root | `<repo-root>` | 13631 |
| main checkout root | `<main-checkout-root>` | 36 |
| machine name | `<host>` | 0 |
| account name | `<user>` | 0 |

Zero residual host tokens after sanitisation. The only absolute paths remaining are `C:\Program Files`
and `C:\Program Files (x86)`, which carry no account or machine identity.

## Acceptance

| P2-T2 condition | Result |
|---|---|
| `EXIT_CODE: 0` | **Yes** — observed `0` |
| Zero occurrences of `Skipping target "CoreCompile"` | **Yes** — 0 |
| Deduplicated warning count not greater than 5 | **Yes** — 5 |

Output Summary: The solution rebuilt clean with the 17th detachment in place — `Build succeeded.`,
`5 Warning(s)`, `0 Error(s)`, `EXIT_CODE: 0`. Non-vacuity holds: **zero** occurrences of
`Skipping target "CoreCompile"` and 18 `csc.exe` invocations across 10 `(Rebuild target)` entries in
the 11948-line file log. The deduplicated warning count is **5**, equal to the P0-T5 baseline and
identical to P1-T2's, with the same five pre-existing `System.Reactive` `packages.config` advisories
and zero Roslyn diagnostics: the production change is diagnostic-neutral. The log is sanitised with
zero residual host tokens.
