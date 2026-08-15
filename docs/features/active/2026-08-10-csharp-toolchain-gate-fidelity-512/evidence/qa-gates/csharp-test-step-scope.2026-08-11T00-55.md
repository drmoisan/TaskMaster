# C# toolchain step 4 — recorded scope-limited treatment ([P6-T9])

Timestamp: 2026-08-11T00-55
Command: (none — analysis artifact)
EXIT_CODE: (none — analysis artifact)

**This is a recorded deviation, not a skipped planned command.** No task in this plan schedules a
`vstest.console.exe` run; this artifact states why, with evidence.

## 1. No C# source or project file is modified by this feature

`git diff --name-only <MERGE_BASE>` (`MERGE_BASE` = `a5e336e5ae3443d4197caf5f87036fae1d538f89`):

```
.claude/rules/csharp.md
.claude/skills/csharp-qa-gate/SKILL.md
.vscode/tasks.json
CLAUDE.md
docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/plan.2026-08-10T14-08.md
scripts/vscode/Invoke-VSBuild.ps1
tests/scripts/vscode/Invoke-VSBuild.Tests.ps1
```

**No `*.cs`, `*.csproj`, `*.props` or `*.targets` file appears.** Corroborated by:

```
$ git status --porcelain -- '*.cs' '*.csproj' '*.props' '*.targets'
(empty)
```

The only `*.cs` file ever touched in this delivery was
`UtilitiesCS/Extensions/QueueExtensions.cs`, perturbed by [P5-T5] as the AC4 negative control and
reverted by [P5-T6]; its revert is verified in
`FEATURE/evidence/qa-gates/typecheck-negative-control.2026-08-10T23-58.md` (empty `git status`, zero
grep hits for the probe method, line count restored to 21).

Because no C# production code, test code or project file changes, `vstest.console.exe` and C#
coverage capture would validate nothing this feature altered. They are therefore **not run**, and the
`.claude/rules/csharp.md` coverage obligations for C# are not engaged. `.claude/rules/powershell.md`
coverage obligations **are** engaged and are discharged by [P0-T16], [P6-T3] and [P6-T4].

## 2. Pre-existing unrelated failure of the C# test step on `main`

`FEATURE/evidence/baseline/baseline-ci-parity-on-main.2026-08-10T15-05.md` records that on `main`'s
tip (`a682c7a2`) the CI job's step conclusions were:

| Conclusion | Step |
|---|---|
| success | `Verify formatting` |
| success | `Build with analyzers and code style enforcement` |
| success | `Build with nullable warnings treated as errors` |
| **failure** | **`Run MSTest suite with coverage`** |
| success | `Upload test results` |

The MSTest step is failing on `main` for reasons unrelated to this feature — a test and coverage
concern plausibly belonging to the sibling coverage features (issues 441, 457, 494) or to a known
flaky test. This feature neither inherits nor addresses it.

## 3. No `.csproj` rewritten by `Sync-PackageReferences.ps1` remains modified

`scripts/vscode/Invoke-VSBuild.ps1` unconditionally runs `Sync-PackageReferences.ps1`, which can
rewrite `.csproj` HintPaths. Every task that could trigger it captured
`git status --porcelain -- '*.csproj'` before and after:

| Task | Before | After | Sync console line | Revert needed |
|---|---|---|---|---|
| [P0-T16] | (empty) | (empty) | `Sync-PackageReferences: All HintPaths are up to date` | no |
| [P1-T3] | (empty) | (empty) | `Sync-PackageReferences: All HintPaths are up to date` | no |
| [P2-T4] | (empty) | (empty) | `Sync-PackageReferences: All HintPaths are up to date` | no |
| [P2-T7] | (empty) | (empty) | `Sync-PackageReferences: All HintPaths are up to date` | no |
| [P5-T8] | (empty) | (empty) after both the control and the corrected run | `Sync-PackageReferences: All HintPaths are up to date` (both runs) | no |
| [P5-T9] | (empty) | (empty) | `Sync-PackageReferences: All HintPaths are up to date` | no |
| [P5-T10] | (empty) | (empty) | `Sync-PackageReferences: All HintPaths are up to date` | no |
| [P6-T3] | (empty) | (empty) | `Sync-PackageReferences: All HintPaths are up to date` | no |

Citing, respectively:
`FEATURE/evidence/baseline/baseline-poshqc-test.2026-08-10T23-08.md`,
`FEATURE/evidence/regression-testing/red-pester-run.2026-08-10T23-20.md`,
`FEATURE/evidence/qa-gates/csproj-sync-guard-p2t4.2026-08-10T23-26.md`,
`FEATURE/evidence/regression-testing/green-pester-run.2026-08-10T23-32.md`,
`FEATURE/evidence/qa-gates/vscode-task-lint.2026-08-11T00-06.md`,
`FEATURE/evidence/qa-gates/vscode-task-typecheck.2026-08-11T00-12.md`,
`FEATURE/evidence/qa-gates/enablenullable-noop-proof.2026-08-11T00-14.md`,
`FEATURE/evidence/qa-gates/final-poshqc-test.2026-08-11T00-40.md`.

In every case the sync emitted `All HintPaths are up to date` — the message it produces only when it
changed nothing (the `$fixCount -eq 0` early return at `Sync-PackageReferences.ps1:112` guards the
`WriteAllText` at :148). **No `.csproj` was ever rewritten, so none is left modified.**

## Output Summary

C# toolchain step 4 (`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`) is deliberately
not executed and no C# coverage is captured, because this feature modifies **no** `*.cs`, `*.csproj`,
`*.props` or `*.targets` file — verified by `git diff --name-only <MERGE_BASE>` and by an empty
`git status --porcelain` for those globs. The step is additionally failing on `main`'s tip for
reasons outside this feature's scope. Every one of the eight tasks that could have triggered
`Sync-PackageReferences.ps1` recorded empty `.csproj` status before and after and the
`All HintPaths are up to date` console line, so no `.csproj` remains modified.
