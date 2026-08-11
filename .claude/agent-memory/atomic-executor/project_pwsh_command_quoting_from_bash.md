---
name: pwsh-command-quoting-from-bash
description: A `pwsh -NoProfile -Command` payload invoked from the Bash tool must use a single-quoted OUTER wrapper and double quotes INSIDE; a double-quoted outer wrapper silently empties every `$var` and inverts exit-code gates.
metadata:
  type: project
---

Any `pwsh -NoProfile -Command '<script>'` run through the Bash tool must use a
**single-quoted outer wrapper with only double quotes inside**. Never `\"`, never a
double-quoted outer wrapper, and never `''` as an escaped quote inside that wrapper.

**Why:** bash consumes the outer double quotes and expands `$` before pwsh ever parses
the string. Measured on this repo:
- `... | ForEach-Object { "{0:o} {1}" -f [datetime]::UtcNow, $_ }` under a double-quoted
  outer wrapper → `$_` expands to bash's last argument → `ParserError: Missing expression
  after ','.` and the wrapped command never runs at all.
- A Pester payload emitting `"Passed=$($r.PassedCount) Failed=$($r.FailedCount)"` under a
  double-quoted outer wrapper printed `Passed= Failed=` and exited 1 on an all-passing
  run — inverting every gate that reads `EXIT_CODE:`.
- `''` inside a bash single-quoted string does NOT produce an escaped quote; bash closes
  and reopens the quote, so pwsh receives a bareword and throws
  `Unexpected token ... in expression or statement`. Use a here-string (`@"…"@`) or
  `[char]39` / `[char]44` instead of embedding literal single quotes.

**How to apply:** when a plan or task specifies a `pwsh -NoProfile -Command` payload,
execute it once before trusting it. `-File` invocations are unaffected (no shell-visible
`$` in the payload). Related: [[project_build_test_env]],
[[project_poshqc_pester_mcp_exit_minus1]].

Corollary measured at the same time: Pester 5.6.1 creates `CodeCoverage.OutputPath`'s
parent directory (`New-Item -Force -ItemType Container`), so redirecting coverage into a
not-yet-existing evidence folder is safe. Pester also ignores `Run.Exit` by default, so a
direct run's `EXIT_CODE:` is only load-bearing if the payload ends with an explicit
`if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.
