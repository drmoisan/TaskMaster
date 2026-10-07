---
name: hygiene-pattern-array-comma-precedence-and-regex-token-hits
description: Plan hygiene scans built as @(a, "x" + b + "y", ...) mis-parse (comma binds tighter than +) and a bare letter-colon-backslash drive pattern matches regex tokens like client-id:\s; both make HITS=0 unsatisfiable
metadata:
  type: project
---

Two defects found in the #929 plan's CMD-HYGIENE at preflight round 2 (2026-09-28), both invisible to a positive-only SELFTEST.

1. **Comma precedence.** In PowerShell the comma operator binds tighter than `+`, so `@(("D:" + $bs), "(^|[^A-Za-z0-9])" + $acct + "([^A-Za-z0-9]|$)", ...)` becomes a 7-element array whose boundary fragments `(^|...)` and `(...|$)` match every line. Every element that is a concatenation must be parenthesised.
2. **Drive pattern hits regex text.** `[A-Za-z]:` followed by a backslash matches `d:\s` inside `client-id:\s*` in plans and in test files that assert YAML keys. Require the letter to be preceded by start-of-line or a non-`[A-Za-z0-9_-]` character.

**Why:** a positive self-test (`Q:` + backslash + `x` must match) passes under both defects, so the scan looks verified while being unsatisfiable.

**How to apply:** when preflighting any hygiene or leak scan, require a negative self-test (`client-id:` + [char]92 + `s` must NOT match) and a pattern-count check, and grep the plan plus files the plan will create for the drive pattern. Also make the hit listing print repo-relative paths, since `MatchInfo.Path` is absolute. Related: [[project_doubled_backslash_dedoubles_bash_to_native_exe]].
