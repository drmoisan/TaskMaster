# Gate: the code diff contains none of the prohibited constructs — issue #877

Timestamp: 2026-09-13T11-02
Command: `git -C <repo-root> diff main...HEAD -- QuickFiler.Test UtilitiesCS.Test TestSupport`, with the captured diff text searched for four tokens
EXIT_CODE: 0
Output Summary: The diff captured 306 lines. Lines containing `DoNotParallelize`: 0. Lines containing `Workers`: 0. Lines containing `Thread.Sleep`: 0. Lines containing `Task.Delay`: 0. All four counts are zero.

## Token counts

| Token | Lines containing it |
|---|---|
| `DoNotParallelize` | 0 |
| `Workers` | 0 |
| `Thread.Sleep` | 0 |
| `Task.Delay` | 0 |

## Working-tree companion search

The porcelain companion recorded at [P2-T15] printed zero lines, so there is no uncommitted working-tree change in any of the five write-set paths and no additional per-file search was required. No class-(b) path exists to exclude, because [P2-T4] recorded `Class (b) list: EMPTY`.

## Pathspec scoping

The pathspec is deliberately limited to `QuickFiler.Test`, `UtilitiesCS.Test` and `TestSupport`. It excludes `docs` and `.claude`, so this plan's own discussion of the prohibited constructs, and the same discussion in `issue.md` `## Out of Scope (non-negotiable)`, can neither satisfy nor unsatisfy this gate.

## Independent enforcement

`Thread.Sleep` and `Task.Delay` are additionally banned repository-wide by the BannedApiAnalyzers entries at `BannedSymbols.txt` lines 4 to 7, which name `System.Threading.Thread.Sleep(System.Int32)`, `System.Threading.Thread.Sleep(System.TimeSpan)`, `System.Threading.Tasks.Task.Delay(System.Int32)` and `System.Threading.Tasks.Task.Delay(System.TimeSpan)`. The analyzer gate at [P2-T5] enforces that set independently of this token search.

## Base ref

`main` resolves as a local ref at `e4349a62c0fe6a5daece0b0554a0da5f508129d6`, so no `origin/main...HEAD` substitution was made.
