---
name: powershell-like-wildcard-breaks-plan-task-id-checkoff
description: PowerShell -like/-notlike treats the square brackets in a plan task ID such as "- [ ] [P1-T1]" as a character-class wildcard, so a guard written with -notlike always reports NOT FOUND; use .Contains() instead.
metadata:
  type: project
---

When scripting plan check-offs in PowerShell, guard the replacement with `$text.Contains($old)`,
never with `$text -notlike ("*" + $old + "*")`.

**Why:** `-like` / `-notlike` are wildcard operators, and `[` `]` are wildcard metacharacters that
open a character class. A plan task marker is literally `- [ ] [P1-T1]`, so the pattern
`*- [ ] [P1-T1]*` is parsed as a character class rather than as literal brackets and never matches
the text that is plainly present. The observed failure is a self-written guard throwing
`NOT FOUND: - [ ] [P1-T1]` against a plan file that contains that exact string, which reads like a
plan-state defect and is not one.

**How to apply:** In any `pwsh` one-liner that flips `- [ ] [P#-T#]` to `- [x] [P#-T#]` across a
plan file, use `.Contains()` for the presence guard and `.Replace()` for the substitution. Both are
literal-string operations and neither interprets wildcards. `-match` is also wrong here for the same
class of reason: `[` is a regex metacharacter. If a wildcard or regex form is unavoidable, wrap the
task ID with `[regex]::Escape()`.

Related: [[plan-checkoff-fixpoint-breaks-terminal-clean-tree-gate]],
[[blocked-bash-command-silently-drops-chained-checkoff]].
