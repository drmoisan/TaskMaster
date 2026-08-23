# Toolchain Clean Pass — Remediation Cycle 2

- Task: `[P2-T10]`
- Issue: #418
- Evidence series: `2026-08-05T05-00`
- Timestamp: 2026-08-05T00-26

## Pass number: 1

**One uninterrupted pass. No loop restart occurred.**

## The six mandated commands, in `CLAUDE.md` toolchain order

| # | Stage | Task | Command | `EXIT_CODE` | Artifact |
|---|---|---|---|---|---|
| 1 | Format | `[P2-T1]` | `dotnet tool run csharpier format .` | **0** | `evidence/qa-gates/csharpier-format.2026-08-05T05-00.md` |
| 2 | Format check | `[P2-T2]` | `dotnet tool run csharpier check .` | **0** | `evidence/qa-gates/csharpier-check.2026-08-05T05-00.md` |
| 3 | Restore | `[P2-T3]` | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"` | **0** | `evidence/qa-gates/restore.2026-08-05T05-00.md` |
| 4 | Lint / analyzers | `[P2-T4]` | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` | **0** | `evidence/qa-gates/analyzer-build.2026-08-05T05-00.md` |
| 5 | Type check / nullable | `[P2-T6]` | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` | **0** | `evidence/qa-gates/nullable-build.2026-08-05T05-00.md` |
| 6 | Test + coverage | `[P2-T7]` | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` | **0** | `evidence/qa-gates/test-coverage.2026-08-05T05-00.md` |

**All six commands returned `EXIT_CODE: 0` within one uninterrupted pass.**

### Two supplementary commands run alongside stage 5, per `[P2-T6]`

The mandated nullable command returns `EXIT_CODE: 0` vacuously — 18 of 18 `CoreCompile` targets skipped,
0 `csc.exe` invocations, 0.90 s — so it is **not** evidence of nullable cleanliness. The binding
`## Do Not Do` list requires a forced recompile of the changed projects and an explicit statement that one
was performed. Both were run:

| Supplementary forced rebuild | `EXIT_CODE` | Diagnostics |
|---|---|---|
| `MSBuild.exe SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m` | **0** | **0** |
| `MSBuild.exe SVGControl\SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m` | **0** | **0** |

**A forced recompile of both in-scope projects was performed, and this is that statement.**

## Key result figures from the pass

| Stage | Headline result |
|---|---|
| 1 Format | 1467 files processed, **0 reformatted** (verified against the working tree, not inferred from tool wording) |
| 2 Format check | 1467 checked, **0 needing formatting** — matches the basis exactly |
| 3 Restore | 0 warnings, 0 errors; `packages/` directory count 262 before and after, **no `packages/` mutation** |
| 4 Analyzers | **0 errors**, 5 warnings; **added diagnostics: 0**; removed: 1 (`CS2002`, `CoreCompile`-gated, dispositioned non-regressive at `[P2-T5]`) |
| 5 Nullable | mandated exit 0 (vacuous, disclosed); both forced rebuilds **0 diagnostics**, identical to the basis |
| 6 Tests + coverage | **9 assemblies, 6150 total, 6150 passed, 0 failed**; line **85.4006%** PASS, branch **78.6928%** PASS |

## Whether any loop restart occurred, and why not

**No restart occurred.** Each restart trigger was evaluated explicitly:

| Trigger | Stage | Evaluated outcome |
|---|---|---|
| Formatting changed a file | `[P2-T1]` | **No.** `git diff --numstat` returned the identical 5/0 and 1/0 figures before and after, and the changed-path set was unchanged. |
| Formatting non-conformance | `[P2-T2]` | **No.** 0 files needing formatting. |
| Restore added or modified a file under `packages/` | `[P2-T3]` | **No.** Directory count 262 → 262; `git status --porcelain -- packages/` empty. |
| A newly introduced analyzer diagnostic | `[P2-T4]` / `[P2-T5]` | **No.** Zero added diagnostics of any code. The one removal (`CS2002` in `UtilitiesCS.Test`) is `CoreCompile`-gated and its emitting project did not recompile, which `[P2-T5]` explicitly classifies as not a regression, requiring no fix and triggering no restart. |
| A nullable diagnostic absent from the basis | `[P2-T6]` | **No.** Both forced rebuilds produced 0-row diagnostic tables, identical to the basis's 0. |
| A failing test or a coverage-floor failure | `[P2-T7]` / `[P2-T8]` | **No.** 0 failed; both repository floors PASS with margin; no changed line lost coverage. |
| Failed greater than zero in either order-proof run | `[P2-T9]` | **No.** 0 failed in both. |

## `[P2-T9]` order-independence outcome

| Run shape | Before (Phase 0) | After (`[P2-T9]`) |
|---|---|---|
| Standalone `SVGControl.Test.dll` | exit 1 — 75 / 69 passed / **6 failed** | **exit 0 — 75 / 75 passed / 0 failed** |
| `SVGControl.Test` first, `VBFunctions.Test` second | exit 1 — 76 / 70 passed / **6 failed** | **exit 0 — 76 / 76 passed / 0 failed** |

Test outcomes are invariant under assembly ordering. This closes G-8 and the code review's single Blocking
finding. Recorded in full at `evidence/qa-gates/order-independence.2026-08-05T05-00.md`.

## No source, test, or build-configuration file was modified after the pass was recorded

Verified by measurement, not asserted.

**Diff line counts are unchanged from the pre-pass state** recorded in
`evidence/other/scope-guard.2026-08-05T05-00.md`:

```
Command: git diff --numstat -- SVGControl.Test/SVGControl.Test.csproj SVGControl.Test/packages.config
Output:  5	0	SVGControl.Test/SVGControl.Test.csproj
         1	0	SVGControl.Test/packages.config
```

**No `.cs` file and no `app.config` appears in the diff:**

```
git diff --name-only | grep -c '\.cs$'          -> 0
git diff --name-only | grep -ci 'app\.config$'  -> 0
```

**Modification timestamps prove the ordering.** The two functional files were last written during Phase 1
and were untouched throughout Phase 2:

```
-rw-r--r-- 29246 2026-08-04 23:34:55.063292200 -0400 SVGControl.Test/SVGControl.Test.csproj
-rw-r--r--  6107 2026-08-04 23:34:29.249372800 -0400 SVGControl.Test/packages.config
```

Both mtimes are 23:34, whereas the pass began with `[P2-T1]` at 00:06 and ended with `[P2-T9]` at 00:24.
**Neither file was written after the pass began.**

The tracked-modified set at the close of the pass is the same three paths as at `[P1-T7]`:

```
SVGControl.Test/SVGControl.Test.csproj
SVGControl.Test/packages.config
docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-plan.2026-08-05T05-00.md
```

The third is this plan's own checkbox state, authorized by its Scope Lock and required by the executor
protocol; it is documentation, not source, test, or build configuration. Disclosed in full at
`evidence/other/scope-guard.2026-08-05T05-00.md`.

## Output Summary

**`Pass number: 1`.** All six mandated commands — `csharpier format`, `csharpier check`, `Invoke-Restore`,
the analyzer build, the nullable build, and the coverage-enabled nine-assembly test run — returned
`EXIT_CODE: 0` within a single uninterrupted pass, in `CLAUDE.md` toolchain order. Two supplementary forced
`/t:Rebuild` project-scope runs (`SVGControl.Test.csproj`, `SVGControl.csproj`) also returned
`EXIT_CODE: 0` with 0 diagnostics each, supplying the probative type-check evidence the vacuous mandated
gate cannot. **No loop restart occurred**, and each restart trigger is evaluated explicitly above with its
negative outcome. `[P2-T9]` confirms 0 failed in both the standalone and the previously failing paired
ordering. **No source, test, or build-configuration file was modified after the pass was recorded** —
verified by unchanged diff counts (5/0 and 1/0), zero `.cs` and zero `app.config` paths in the diff, and
modification timestamps of 23:34 on both functional files against a pass that began at 00:06.
