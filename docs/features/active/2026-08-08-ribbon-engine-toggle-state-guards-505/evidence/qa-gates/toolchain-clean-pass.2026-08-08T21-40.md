# P5-T9 — Single Uninterrupted Clean Toolchain Pass (AC-16)

Timestamp: 2026-08-08T21-40

## The recorded pass, in order

| # | Task | Artifact | EXIT_CODE |
|---|---|---|---|
| 1 | **P5-T1** format | `<FEATURE>\evidence\qa-gates\csharpier-format.2026-08-08T21-32.md` | **0** |
| 2 | **P5-T2** repo-wide check | `<FEATURE>\evidence\qa-gates\csharpier-check.2026-08-08T21-33.md` | **0** |
| 3 | **P5-T4** analyzer `/t:Rebuild` | `<FEATURE>\evidence\qa-gates\msbuild-analyzers.2026-08-08T21-35.md` | **0** |
| 4 | **P5-T5** type-check `/t:Rebuild` | `<FEATURE>\evidence\qa-gates\msbuild-typecheck.2026-08-08T21-36.md` | **0** |
| 5 | **P5-T6** tests + coverage | `<FEATURE>\evidence\qa-gates\tests-with-coverage.2026-08-08T21-37.md` | **0** |

(P5-T3, the post-format size audit, sits between steps 2 and 3 in the phase order and also passed:
`<FEATURE>\evidence\qa-gates\file-size-audit.2026-08-08T21-34.md`, zero `.cs` files over the
500-line cap.)

## Mechanical proof that the five ran in one pass with no intervening change

All five steps were additionally executed **back-to-back inside a single scripted sequence**, with
a SHA-256 fingerprint of every tracked `.cs`, `.csproj`, `.xml`, and `.sln` file computed
immediately before the first step and immediately after the last. Verbatim output:

```
FINGERPRINT_BEFORE=4429933DA48390ABE527240F77763765A4F2E8D39A9E2AAF467EA0C48B174F0E
PASS_START=2026-08-08T21-32-52
########## P5-T1 csharpier format ##########
Formatted 10 files in 2077ms.
P5T1_EXIT=0
P5T1_REWRITTEN=0
########## P5-T2 csharpier check . ##########
Checked 1517 files in 3615ms.
P5T2_EXIT=0
########## P5-T3 size audit ##########
P5T3_OVER_CAP_CS_FILES=0
########## P5-T4 analyzer /t:Rebuild ##########
6 Warning(s)
0 Error(s)
P5T4_EXIT=0
P5T4_CSC_INVOCATIONS=18
P5T4_ERROR_LINES=0
########## P5-T5 type-check /t:Rebuild ##########
6 Warning(s)
0 Error(s)
P5T5_EXIT=0
########## P5-T6 tests + coverage ##########
Discovered 9 test assemblies.
Test Run Successful.
Total tests: 6435
Passed: 6435
P5T6_EXIT=0
PASS_END=2026-08-08T21-34-27
FINGERPRINT_AFTER=4429933DA48390ABE527240F77763765A4F2E8D39A9E2AAF467EA0C48B174F0E
```

`FINGERPRINT_AFTER == FINGERPRINT_BEFORE`, so **no `.cs`, `.csproj`, `.xml`, or `.sln` file changed
at any point during the pass**. `P5T1_REWRITTEN=0` independently confirms the format step was a
no-op. `P5T4_CSC_INVOCATIONS=18` with `P5T4_ERROR_LINES=0` confirms the analyzer gate was not
vacuous.

## Explicit statement

All five gate steps ran in **one pass**, in the mandated order (format, lint/analyze, type-check,
test), with **no intervening `.cs`/`.csproj`/`.xml`/`.sln` change and no restart within the pass**.
Writing this phase's own Markdown evidence artifacts under `docs/features/` does not count as an
intervening change, and in any case occurred outside the scripted sequence above, whose fingerprint
covers only source and project files.

The type-check step (P5-T5) used **CI's command** —
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
— deliberately **omitting** the `CLAUDE.md` variant's `/p:Nullable=enable`, per **issue #522** and
plan rule 7, as documented in the `## Verification` section of `<FEATURE>\spec.md`. `/nodeReuse:false`
was added to the two MSBuild invocations to suppress persistent worker processes; it is not a
gate-bearing switch and the measured results are identical with and without it.

## Prior aborted attempt (disclosed)

An earlier Phase 5 attempt was aborted at P5-T6 by an environmental failure in `QuickFiler.Test`'s
`WinFormsPumpHost` message-pump test family, diagnosed and resolved in
`<FEATURE>\evidence\other\phase5-attempt1-aborted.2026-08-08T21-30.md`. The phase was restarted at
P5-T1 as the loop rule requires. **The pass recorded above is the restarted pass and contains no
restart of its own.**

Binary outcome: **PASS**.
