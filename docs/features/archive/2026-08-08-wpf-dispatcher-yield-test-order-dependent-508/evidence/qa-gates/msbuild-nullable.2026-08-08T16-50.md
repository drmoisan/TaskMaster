# Toolchain Step 3 (type-check) — Nullable Analysis — FINAL CLEAN PASS (pass 4)

Timestamp: 2026-08-08T16-50

Task: [P2-T4] — final QC loop, pass 4

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true /m`

EXIT_CODE: 0

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.25
```

## Like-for-like comparison with the P0-T9 baseline

| Metric | Baseline (P0-T9) | Pass 4 (P2-T4) | Delta |
|---|---|---|---|
| EXIT_CODE | 0 | 0 | 0 |
| Warnings | 5 | 5 | 0 |
| Errors | 0 | 0 | 0 |
| Elapsed | 1.20s | 1.25s | — |
| CoreCompile invoked | no | no | same |

Identical result by an identical method.

## Vacuousness disclosure (carried forward from P0-T9)

This step is an incremental no-op in this repository, at the gate exactly as at the baseline.
MSBuild's `/t:Build` up-to-date check compares source and output timestamps and ignores `/p:`
property changes; because P2-T3 had just built every project, nothing recompiled here. Signals:
1.25s elapsed (versus 13.61s for P2-T3), and the `CS2002` warning — emitted by `CoreCompile` — is
absent, which is why the count is 5 rather than 6.

This is disclosed rather than presented as a clean enumeration. It is **structurally identical to
the baseline**: P0-T9 ran immediately after P0-T8 the same way and produced the same 5/0 no-op. The
plan ordered both sequences this way, so the comparison is like-for-like and the gate neither
improved nor regressed.

## What actually verifies nullable correctness on the changed code

Full detail in `<FEATURE>/evidence/baseline/msbuild-nullable.2026-08-08T16-19.md`:

1. A forced `/t:Rebuild` with the same properties at baseline exposed **195 pre-existing
   repository-wide nullable errors** (CS8766, CS8618, CS8625, CS8600, CS8601, CS8604, CS8602,
   CS8603, CS8714). `/p:Nullable=enable` forces nullable analysis onto every project including the
   many never opted in, so this is the untouched legacy surface. **None is attributed to
   `WpfDispatcherYield.cs`** — the only two occurrences of that name in the rebuild log are `csc.exe`
   command lines listing it as a compilation input.
2. Both changed files are file-scoped `#nullable enable`, so nullable analysis runs on them in the
   ordinary analyzer build. P2-T3 recompiled both projects (CS2002 present, 13.61s) and reported
   6 warnings / 0 errors with **zero CS86xx**, identical to the P0-T8 baseline. That is the
   non-vacuous nullable measurement on the changed code and it satisfies P1-T6.

The pre-existing 195-error repository-wide nullable debt is out of scope for this `minor-audit`
cycle: it predates the change, is unaffected by it, and remediating it would be a repository-wide
refactor far outside the two-file scope boundary. It is reported, not absorbed.

## Loop state

Step passed, no file rewritten. Proceed to P2-T5.

Output Summary: PASS, EXIT_CODE 0, 5 warnings / 0 errors in 1.25s — identical to the P0-T9 baseline
by an identical method. Disclosed: the step is an incremental no-op (no CoreCompile) at both
baseline and gate, so it is like-for-like but enumerates nothing itself; the effective nullable
check on the changed files is P2-T3, which recompiled both projects and reported zero CS86xx.
Pre-existing repository-wide nullable debt (195 errors under forced rebuild, none in
`WpfDispatcherYield.cs`) is recorded and out of scope.
