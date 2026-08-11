# AC8 — hunk-by-hunk review for policy relaxation ([P5-T14])

Timestamp: 2026-08-11T00-25
Command: `git diff <MERGE_BASE> -- CLAUDE.md .claude/rules/csharp.md .claude/skills/csharp-qa-gate/SKILL.md scripts/vscode/Invoke-VSBuild.ps1 .vscode/tasks.json tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`
EXIT_CODE: 0

`MERGE_BASE` = `a5e336e5ae3443d4197caf5f87036fae1d538f89`.

## Diff scope

```
 .claude/rules/csharp.md                       | 14 ++++++++----
 .claude/skills/csharp-qa-gate/SKILL.md        |  8 ++++---
 .vscode/tasks.json                            |  5 ++++-
 CLAUDE.md                                     | 32 ++++++++++++++++-----------
 scripts/vscode/Invoke-VSBuild.ps1             | 16 +++++++++++---
 tests/scripts/vscode/Invoke-VSBuild.Tests.ps1 | 25 +++++++++++++++++++--
 6 files changed, 74 insertions(+), 26 deletions(-)
```

## Hunk-by-hunk review

| File | Hunk | Change | Relaxation? |
|---|---|---|---|
| `CLAUDE.md` | § C#1 item 1 | CSharpier scope prose corrected; global-install alternative deleted; `dotnet tool restore` prerequisite and manifest-pinning rule **added** | **No** — deleting the global-install alternative *removes* a permitted looser path; the pinning rule is a new constraint |
| `CLAUDE.md` | § C#1 item 2 | `/t:Build` -> `/t:Rebuild /m`; `/t:Rebuild` rationale added | **No** — the command now compiles instead of skipping; strictly more enforcement |
| `CLAUDE.md` | § C#1 item 3 | "Enable nullable reference types and fail builds on warnings for touched code paths" -> per-file opt-in statement; `/t:Build` + `/p:Nullable=enable` -> `/t:Rebuild /m` without the flag; two "must not be restored" prohibitions added | **No** — see the dedicated argument below |
| `CLAUDE.md` | § CUT3 items 1-3 | same three command substitutions | **No** |
| `CLAUDE.md` | § "C# Toolchain (run in this exact order)" items 1-3 | same three command substitutions | **No** |
| `.claude/rules/csharp.md` | § Toolchain items 1-3 | same three command substitutions plus R4/R5 rationale | **No** |
| `.claude/rules/csharp.md` | § Severity-first ordering invariant | embedded command string only | **No** — the invariant text is preserved verbatim |
| `.claude/skills/csharp-qa-gate/SKILL.md` | § Toolchain Execution Sequence steps 1-3 | same three command substitutions | **No** |
| `.claude/skills/csharp-qa-gate/SKILL.md` | § Evidence Storage | bullet R6 **added**, requiring an `/fl` log and a zero `CoreCompile` skip count for steps 2 and 3 | **No** — this is a **new** mandatory evidence obligation |
| `.vscode/tasks.json` | `lint:` task args | `"-Target", "Rebuild"` inserted | **No** — forces a genuine compile |
| `.vscode/tasks.json` | `type-check:` task args | `"-EnableNullable"` -> `"-Target", "Rebuild"`; `"-TreatWarningsAsErrors"` retained | **No** — the warnings-as-errors switch is retained; the removed switch was making the gate unpassable |
| `scripts/vscode/Invoke-VSBuild.ps1` | `param(...)` | `-Target` added with `[ValidateSet('Build','Rebuild')]`; deprecation comment on `-EnableNullable` | **No** — `ValidateSet` is a new input constraint |
| `scripts/vscode/Invoke-VSBuild.ps1` | `Get-MSBuildBuildArguments` | `-Target` parameter added; `'/t:Build'` -> `"/t:$Target"` | **No** — default remains `Build`, so no existing caller changes behaviour |
| `scripts/vscode/Invoke-VSBuild.ps1` | `Get-RequestedMSBuildProperties` | `$properties += 'Nullable=enable'` -> `Write-Warning '...'` | **No** — see the dedicated argument below |
| `scripts/vscode/Invoke-VSBuild.ps1` | call site | `-Target $Target` added | **No** |
| `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | `Describe 'Get-MSBuildBuildArguments'` | one **new** `It` added; the default-target `It` left byte-identical | **No** — test coverage increases by one case |
| `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | `Describe 'Get-RequestedMSBuildProperties'` | nullable `It` renamed and its expected array reduced to `@('TreatWarningsAsErrors=true')` | **No** — the assertion is not weakened; it now asserts the *corrected* contract exactly, and it still asserts an exact array equality rather than a subset |

## Explicit checks required by AC8

### 1. No numeric threshold reduced

No numeric threshold appears anywhere in the diff. Searched the added lines for threshold language:
the only numerals introduced are `1.2.6` (a version pin), `195` (a measured error count quoted as
rationale) and `2026-08-10` (a date). Coverage floors (80% / 90% in `CLAUDE.md` § UT2 and
`.claude/rules/csharp.md` § Testing Standards; 85% / 75% in `.claude/rules/general-unit-test.md` and
`.claude/rules/quality-tiers.md`) are **all outside the diff** — the latter two files are byte-
identical to the merge base ([P5-T13]).

### 2. No mandatory step removed

The four-step order and the restart rule survive at **every** site:

| Site | Four-step order present? | Restart-from-step-1 rule present? |
|---|---|---|
| `CLAUDE.md` § C#1 (items 1-4, with step 4 delegated to the unit-test policy) | yes, unchanged | n/a (stated in § CUT3) |
| `CLAUDE.md` § CUT3 | yes — items 1, 2, 3, 4 all present; item 4 (`vstest.console.exe`) **unchanged** | yes — "The loop behavior (restart rules, ...) is defined by the General Code Change Policy above." unchanged |
| `CLAUDE.md` § "C# Toolchain (run in this exact order)" | yes — items 1-4; item 4 unchanged | yes — "If any step fails, fix and restart from step 1." unchanged |
| `.claude/rules/csharp.md` § Toolchain | yes — items 1-4; item 4 unchanged | yes — "Run the toolchain in order: format → lint → type-check → test. Restart from step 1 if any step fails or changes files." unchanged |
| `.claude/skills/csharp-qa-gate/SKILL.md` | yes — steps 1-4; step 4 unchanged | yes — "If any step fails or modifies files, fix the issue and restart from step 1. Do not stop the loop until all four steps complete without errors in a single pass." unchanged |

### 3. No new suppression token

```
$ git diff <MERGE_BASE> -- <the six files> | grep -E '^\+' \
    | grep -i -E 'pragma warning disable|\[Ignore\]|SuppressMessage|NoWarn|WarningsNotAsErrors'
(no output)
```

**Zero** added lines contain `#pragma warning disable`, `[Ignore]`, `SuppressMessage`, `NoWarn` or
`WarningsNotAsErrors`. `/p:TreatWarningsAsErrors=true` is **retained** at every corrected site, so
the CS8032 protection described in `.claude/rules/csharp.md` § "Deferred analyzer" remains in force.

## "Strengthening, not a relaxation" — summary for the PR body

A reviewer will reasonably challenge the removal of `/p:Nullable=enable`. The argument, with its
measurements from this delivery:

1. **The property was never enforced by any merge gate.** `.github/workflows/ci.yml` has never passed
   it, and branch protection consumes CI's checks.
2. **The documented gate performed zero enforcement.** [P0-T11] measured `EXIT_CODE: 0` in **1.8 s**
   with `CoreCompile` skipped on **18 of 18** projects. The property never reached the compiler in a
   warm tree — the normal state during a toolchain loop.
3. **The corrected gate performs strictly more enforcement.** [P5-T4] measured `EXIT_CODE: 0` with
   **0** skips in 15.0 s, and `/p:TreatWarningsAsErrors=true` promotes every compiler warning to an
   error. The delta against today is from nothing to CI parity.
4. **No opted-in file loses coverage.** [P5-T5] introduced a nullable violation into
   `UtilitiesCS/Extensions/QueueExtensions.cs` (which carries `#nullable enable`) and the corrected
   command — **without** `/p:Nullable=enable` — returned `EXIT_CODE: 1` with
   `error CS8603: Possible null reference return.` The pragma, not the property, is what enrols a
   file.
5. **The alternative reading is the actual relaxation.** [P0-T13] measured that retaining the flag
   yields `EXIT_CODE: 1` with **195 errors**, all pre-existing debt this feature is charged not to
   fix. A mandatory command that cannot pass is not a gate; it is a permanent blocker that every
   session must override, as deliveries #507 and #508 did on 2026-08-08.
6. **The analyzer step is strengthened by the same mechanism.** [P0-T10] measured the documented
   analyzer command returning exit 0 in 2.8 s with 18 of 18 skips (no analyzers ran); [P5-T3]
   measured the corrected form at exit 0 with 0 skips in 20.3 s.
7. **A new obligation is added, not removed.** Bullet R6 in
   `.claude/skills/csharp-qa-gate/SKILL.md` now requires an `/fl` log and a **zero**
   `Skipping target "CoreCompile"` count for steps 2 and 3, and declares a non-zero skip count
   **unverified, not passed**.

## Output Summary

AC8 is satisfied. Hunk-by-hunk review of all six edited files finds **no numeric threshold reduced**,
**no mandatory toolchain step removed** (the four-step order and the restart rule survive verbatim at
all five documentation sites, and step 4 is untouched everywhere), and **no added
`#pragma warning disable`, `[Ignore]`, `SuppressMessage`, `NoWarn` or `WarningsNotAsErrors` token**.
The diff adds one new mandatory evidence obligation (R6) and one new input constraint
(`ValidateSet`), and replaces two gates that could not enforce with two that measurably do.
