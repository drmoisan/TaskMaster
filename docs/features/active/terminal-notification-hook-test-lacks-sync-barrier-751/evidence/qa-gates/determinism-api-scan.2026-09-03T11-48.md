# P4-T9 — Banned Determinism API Scan (Issue #751)

Timestamp: 2026-09-03T14-46

Command:

```powershell
git diff f8414ee9..HEAD -- "*.cs" | Select-String -Pattern '^\+.*(Thread\.Sleep|Task\.Delay|DoNotParallelize|SpinWait|Task\.Wait\(\)|WaitOne\()'
```

EXIT_CODE: 0

## Output Summary — complete output

```
(no match)
```

Match count: **0**.

## Acceptance

| Required | Observed | Result |
|---|---|---|
| The command produced no match | 0 matches | PASS |

## Notes on the command shape

- The pathspec restricts the scan to `*.cs`, so the literals quoted in the plan document and in the evidence
  artifacts are outside its reach and cannot produce a self-hit. The scan reads only added lines of C#
  source in the branch diff.
- The `--` separator before the pathspec is required; without it git would read the pathspec as a ref
  operand.
- The `^\+` anchor restricts matches to **added** lines, so a pre-existing occurrence elsewhere in a touched
  file cannot trigger a false positive.

The three lines this branch adds are
`(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);`,
`Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);`, and
`Interlocked.Increment(ref InvokedTerminalHookCount);`. None contains a wall-clock wait, a sleep, a delay, a
polling loop, a spin, a blocking wait, or a `[DoNotParallelize]` attribute. The fix awaits an existing
`TaskCompletionSource` instead of waiting on time.
