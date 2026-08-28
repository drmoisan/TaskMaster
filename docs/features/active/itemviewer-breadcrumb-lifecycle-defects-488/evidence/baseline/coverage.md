# Phase 0 — Baseline Repository-Wide Coverage ([P0-T14])

Timestamp: 2026-08-28T05-19

Command (run under `pwsh -NoProfile` from this worktree root):

```
.\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\baseline\coverage-baseline.cobertura.xml
```

EXIT_CODE: 0

## Cobertura file

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/baseline/coverage-baseline.cobertura.xml`
exists, 10,749,342 bytes. It is retained rather than summarised away because `[P8-T7]` recomputes the
baseline **testable-denominator** figure from it without starting a second long run.

## Root `coverage` element attributes

```
<coverage line-rate="0.852607" branch-rate="0.791925" complexity="25352" version="1.9"
          timestamp="1787894304"
          lines-covered="54670" lines-valid="64121"
          branches-covered="13005" branches-valid="16422">
```

| Measure | Value |
| --- | --- |
| **Baseline raw line-rate** | **0.852607** (85.2607%) |
| **Baseline raw branch-rate** | **0.791925** (79.1925%) |
| Lines covered / valid | 54670 / 64121 |
| Branches covered / valid | 13005 / 16422 |

Both figures clear the uniform repository floors of `>= 85%` line and `>= 75%` branch stated in
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`, and the line figure also
clears the `>= 80%` floor stated in `CLAUDE.md` § UT2 and `.claude/rules/csharp.md`. These are the
**raw uninstrumented** figures over the whole repository; the CLAUDE.md § UT2 testable-denominator
figure is a different, higher number and is computed in `[P8-T7]` for both the baseline and the
post-change state.

## Test run inside the coverage collection

| Measure | Value |
| --- | --- |
| Total tests | **6812** |
| Passed | **6812** |
| Failed | **0** |
| Run result | `Test Run Successful.` |

The runner applies `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook` to every assembly,
matching CI and preventing a real Outlook process from being launched.

## Discovered test assemblies

**Total discovered: 9.** The runner's own output line reads `Discovered 9 test assemblies.` The
verbatim list, re-derived by applying the runner's own discovery predicate (`*.Test.dll` under a
`bin\Debug` path, excluding `obj` and `ref`) from this worktree root:

```
./QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
./SVGControl.Test/bin/Debug/SVGControl.Test.dll
./Tags.Test/bin/Debug/Tags.Test.dll
./TaskMaster.Test/bin/Debug/TaskMaster.Test.dll
./TaskTree.Test/bin/Debug/TaskTree.Test.dll
./TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll
./ToDoModel.Test/bin/Debug/ToDoModel.Test.dll
./UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll
./VBFunctions.Test/bin/Debug/VBFunctions.Test.dll
```

The re-derived list has nine entries, agreeing with the runner's count.

### Every entry is under this worktree root, and none carries a nested `worktrees` segment

Each of the nine paths is rendered relative to this worktree root, which is what establishes that
every discovered assembly lies beneath it. None contains a `worktrees` path segment **below** this
worktree root. That property was additionally established structurally: `.claude/` in this worktree
contains `agent-memory`, `agents`, `hooks`, `lib`, `rules`, `settings.json`, `settings.local.json`,
and `skills`, and **no `worktrees` directory exists** under it, so recursive discovery from this root
cannot reach a sibling agent worktree. The worktree root's own absolute path does contain a
`worktrees` segment, since this worktree lives under the primary checkout's `.claude/worktrees/`; that
segment is above the root, not below it, and is outside the scope of the acceptance condition.

Output Summary: EXIT_CODE 0. Baseline repository-wide **line-rate 0.852607** and **branch-rate
0.791925** (54670/64121 lines, 13005/16422 branches). **9** test assemblies discovered, all under this
worktree root with no nested `worktrees` segment. 6812 tests ran, 6812 passed, 0 failed. The Cobertura
file is retained for `[P8-T7]`'s baseline testable-denominator recomputation.
