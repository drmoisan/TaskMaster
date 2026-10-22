# Final nullable / type-check gate (P7-T5)

Timestamp: 2026-09-01T11-04
Task: [P7-T5]
Working directory: WORKTREE

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:logfile=FEATURE/evidence/qa-gates/p7-t5-nullable.msbuild.txt;verbosity=normal"
```

EXIT_CODE: 0

`/p:Nullable=enable` was not added and `/t:Build` was not substituted for `/t:Rebuild`. Both
prohibitions are stated in `CLAUDE.md` with their reasons, and this command matches the one
`.github/workflows/ci.yml` runs for its nullable step apart from the `/fl` file logger.

File log: `FEATURE/evidence/qa-gates/p7-t5-nullable.msbuild.txt` (12005 lines).

## Verbatim summary line

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

## Vacuity check

Count of occurrences of the literal `Skipping target "CoreCompile"` in
`FEATURE/evidence/qa-gates/p7-t5-nullable.msbuild.txt`: **0**.

## Diagnostic searches relevant to the acceptance criteria

| Search | Count |
|---|---|
| `CS0518` | **0** |
| `CS86` followed by two digits | 0 |

Output Summary: The type-check gate passes on the final, formatted tree. The full-solution rebuild under
`/p:TreatWarningsAsErrors=true` exited 0 with 0 errors, and the warning count is unchanged from the
P0-T9 baseline at 5 — the same pre-existing System.Reactive `packages.config` warnings, which are raised
by a NuGet-supplied `.targets` file with no diagnostic code and are therefore not reached by the
warnings-as-errors promotion.

Zero `CS0518` occurrences is the compile-side half of AC15. `CS0518` is the diagnostic that an `init`
accessor, a `record`, or a `record struct` would produce on .NET Framework 4.8.1, which has no
`IsExternalInit` polyfill. The search-side half is the P6-T2 sweep, which found zero matches for
`\binit\s*[;{]|\brecord\b` across both changed production files. Together they establish that the
solution compiles on net481 without CS0518.

Zero `CS86xx` occurrences confirms that no nullable-flow diagnostic was introduced, consistent with
P6-T3's finding that neither changed production file carries a `#nullable` directive and so neither has
opted into nullable analysis.

This artifact is one of the four that the AC19 check-off in P8-T23 depends on, and it supplies the
compile-side evidence for the AC15 check-off in P8-T19.
