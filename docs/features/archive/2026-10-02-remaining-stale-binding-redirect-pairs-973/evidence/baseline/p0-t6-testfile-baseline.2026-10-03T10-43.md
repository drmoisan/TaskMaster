# P0-T6 test-file baseline (issue #973; read-only)

Timestamp: 2026-10-03T10-43
Command: Grep tool over tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 with the eight P0-T6 patterns (count or content mode with -n); git -C <execution-worktree-root> hash-object --no-filters -- tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1
EXIT_CODE: 0
Output Summary: 14 It blocks; $expectedDebt literal opens at line 293; System.ClientModel pair at 308; $expectedUnverifiable at 310; collection comparison at 331; unverifiable assertion at 332; examined-count guard at 330; Fizzler/Unsafe exclusion at 333. All counts and line numbers as stated; no TESTFILE-DRIFT.

| Pattern | Expected | Observed |
|---|---|---|
| `^\s*It '` count | 14 | 14 |
| `^\s*\$expectedDebt = @\(\r?$` line | 293 | 293 |
| `'System.ClientModel\|1.3.0.0'` line | 308 | 308 |
| `^\s*\$expectedUnverifiable = @\('Microsoft.IdentityModel.Clients.ActiveDirectory', 'System.Linq.AsyncEnumerable', 'netstandard'\)` | 1 at 310 | 1 at 310 |
| `\$actualDebt \| Should -Be @\(\$expectedDebt` | 1 at 331 | 1 at 331 |
| `\$actualUnverifiable \| Should -Be @\(\$expectedUnverifiable \| Sort-Object -Unique\)\r?$` | line 332 | 332 |
| `Should -Be \$redirectElement` | 1 at 330 | 1 at 330 |
| `Fizzler\|\*` | 1 at 333 | 1 at 333 |

TESTFILE-HASH-START: 3af08570149c6da4dafbc719953ddbc72086993a
