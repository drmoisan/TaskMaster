# AC9 and AC12 against the final state of `BannedSymbols.txt` (issue #826, [P6-T3])

Timestamp: 2026-09-09T19-47

Command, run as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
(Get-Content -LiteralPath "BannedSymbols.txt").Count
@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch "TimeoutAfter").Count
@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch "WaitHandle.WaitOne;").Count
@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch "M:System.Threading.").Count
@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch <each of the eight DocID tokens>).Count
git diff --numstat $Base -- "BannedSymbols.txt"
```

EXIT_CODE: 0

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| line count | 15 | 15 |
| `TimeoutAfter` | 0 | 0 |
| `WaitHandle.WaitOne;` | 0 | 0 |
| `M:System.Threading.` | 12 | recorded |
| anchored numstat | 8 added, 0 removed | exactly 8 added, 0 removed |

The `M:System.Threading.` count of 12 is the four pre-existing `Thread.Sleep` and `Task.Delay` lines plus
the eight added ones. The three `P:` property lines (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`)
do not carry that prefix, and 12 + 3 = 15 reconciles with the line count.

## Per-DocID verification

Each token has an occurrence count of exactly 1 and its matched line also contains the token
`TimeProvider`:

| DocID token | Count | Line contains `TimeProvider` |
|---|---|---|
| `M:System.Threading.CancellationTokenSource.CancelAfter(System.Int32);` | 1 | yes |
| `M:System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan);` | 1 | yes |
| `M:System.Threading.CancellationTokenSource.#ctor(System.Int32);` | 1 | yes |
| `M:System.Threading.CancellationTokenSource.#ctor(System.TimeSpan);` | 1 | yes |
| `M:System.Threading.WaitHandle.WaitOne(System.Int32);` | 1 | yes |
| `M:System.Threading.WaitHandle.WaitOne(System.TimeSpan);` | 1 | yes |
| `M:System.Threading.WaitHandle.WaitOne(System.Int32,System.Boolean);` | 1 | yes |
| `M:System.Threading.WaitHandle.WaitOne(System.TimeSpan,System.Boolean);` | 1 | yes |

## Anchored numstat

```
8	0	BannedSymbols.txt
```

Exactly 8 added and 0 removed lines against base `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`, which is
what proves the seven pre-existing lines are unchanged.

## Exclusions (AC12)

Both documented exclusions hold in the final state:

- `TimeoutAfter` count is **0**. It is a repository-local extension method in
  `UtilitiesCS/Threading/TimeOutTask.cs`, two of whose overloads accept a `TimeProvider` and are the
  documented determinism seam; banning it would ban the remedy every existing message points callers
  toward.
- `WaitHandle.WaitOne;` count is **0**. That is the parameterless-overload DocID form, which carries no
  parentheses. The overload is a deterministic handshake on a signal rather than a wall-clock deadline,
  12 of the 13 current call sites use it as the repository's own cross-thread determinism idiom, and
  adding it would contribute 12 more unfixable entries to the backlog that already blocks promotion.

Independent corroboration that the eight added DocIDs are not silently unresolvable strings:
[P4-T2] observed an RS0030 diagnostic at every one of the 15 enumerated sites through the certified SARIF
channel, with three live controls firing in the same run.

Output Summary: `BannedSymbols.txt` holds 15 lines, all eight DocIDs are present exactly once with a
`TimeProvider` message, both documented exclusions are at count 0, and the anchored numstat is exactly 8
added and 0 removed. AC9 and AC12 are satisfied.
