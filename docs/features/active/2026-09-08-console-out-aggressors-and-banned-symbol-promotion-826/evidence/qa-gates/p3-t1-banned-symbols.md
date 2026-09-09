# Banned-symbol DocID additions (issue #826, [P3-T1])

Timestamp: 2026-09-09T19-17

Command: the eight lines were appended with a `pwsh -NoProfile -Command` block carrying the plan's C2
preamble branch guard, which read `BannedSymbols.txt` with `Get-Content -Raw`, appended the eight
CRLF-separated lines, and wrote the file back with `Set-Content -NoNewline -Encoding UTF8`. The gate
figures below were then read with:

```
(Get-Content -LiteralPath "BannedSymbols.txt").Count
@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch <each of the eight tokens>).Count
@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch "TimeoutAfter").Count
@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch "WaitHandle.WaitOne;").Count
@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -Pattern "^\s").Count
git diff --numstat $Base -- BannedSymbols.txt
```

EXIT_CODE: 0

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `(Get-Content).Count` | 15 | 15 |
| `TimeoutAfter` `-SimpleMatch` count | 0 | 0 |
| `WaitHandle.WaitOne;` `-SimpleMatch` count | 0 | 0 |
| `-Pattern "^\s"` count (leading whitespace) | 0 | 0 |
| anchored numstat | 8 added, 0 removed | exactly 8 added, 0 removed |

The 8-added, 0-removed numstat is what proves the seven pre-existing lines are unchanged.

## Per-DocID verification

Each token below has an occurrence count of exactly 1 and its matched line also contains the token
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

The three distinct messages used are:

- `CancelAfter` overloads: `Do not call CancelAfter. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.`
- `#ctor` overloads: `Do not construct a deadline CancellationTokenSource. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.`
- `WaitOne` overloads: `Do not use a timed WaitOne. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.`

## Exclusions held

- The parameterless `CancellationTokenSource.#ctor()` is not listed. 157 parameterless constructions
  exist across 80 files and none carries a deadline.
- `TimeoutAfter` is not listed. It is a repository-local extension method in
  `UtilitiesCS/Threading/TimeOutTask.cs`, two of whose overloads accept a `TimeProvider` and are the
  documented determinism seam; banning it would ban the remedy every existing message points callers
  toward.
- The parameterless `WaitHandle.WaitOne()` overload, whose DocID form carries no parentheses, is not
  listed. It is a deterministic handshake on a signal rather than a wall-clock deadline, 12 of the 13
  current call sites use it as the repository's own cross-thread determinism idiom, and adding it would
  contribute 12 more unfixable entries to the backlog that already blocks promotion.

## Recorded mechanical note

The first attempt to append the eight lines wrote only one of them. The cause was a PowerShell parsing
rule rather than a content error: in `@("a" + $x, "b" + $x)` the comma operator binds tighter than `+`,
so the expression evaluates to a single concatenated string rather than a two-element array. The append
was reverted with `git checkout -- BannedSymbols.txt`, the array was rewritten with newline-separated
elements and no commas, and a `if ($new.Count -ne 8) { throw }` guard was added before the write so the
same failure could not pass silently. The observed `NewCount` was 8 and the resulting line count is 15.
Recorded because the failed form produced a plausible-looking file that a line-count check alone would
have caught only by luck.

Output Summary: `BannedSymbols.txt` grows from 7 to 15 lines. All eight DocIDs are present exactly once
with a `TimeProvider` message, no appended line carries leading whitespace, both documented exclusions
hold, and the anchored numstat is exactly 8 added and 0 removed. The severity value in `.editorconfig`
is untouched by this task.
