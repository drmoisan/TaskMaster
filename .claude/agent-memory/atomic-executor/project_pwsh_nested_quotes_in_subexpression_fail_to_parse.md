---
name: pwsh-nested-quotes-in-subexpression-fail-to-parse
description: Inside "$( ... )" an empty nested string "" or a doubled-quote literal ""Csc"" is read as an escaped quote, so the whole span exits 1 printing nothing (or a cmdlet fails to bind) — two plan-command defect shapes that look like transport problems
metadata:
  type: project
---

Within a PowerShell **expandable** string, a `$( ... )` subexpression may contain nested double-quoted strings — `"$("hi")"` prints `hi`. But two shapes break, because the enclosing string's scanner treats `""` as the escape for a literal quote before the subexpression is parsed:

1. **Empty nested string.** `"$($k -replace "X", "") OK"` → exits 1, prints nothing at all, no error text on stdout. `"$($k -replace "X", "Y") OK"` works, so the empty replacement alone is the trigger. Fix: use the single-operand form `-replace "X"`, which replaces with empty by definition and is semantically identical.
2. **Doubled-quote literal as an argument.** `"CSC_TASK_LINES=$(@($log | Select-String -SimpleMatch -CaseSensitive "Task ""Csc""").Count)"` → `Select-String` reports *per input line* that "the input object cannot be bound to any parameters", because the pattern argument never binds. With a large `$log` this emits megabytes of identical errors. Fix: build the literal outside the interpolation with an explicit quote char — `$q = [string][char]34; $pat = "Task " + $q + "Csc" + $q` — and pass it by variable.

**Why it matters beyond the syntax:** both shapes appear in *plan* command spans that read plausible and were never executed by the planner, and both fail in a way that mimics a transport problem, so the instinct is to blame the Bash-to-pwsh boundary. They are not transport: they fail identically from a pwsh host. Verify with a one-line probe before rewriting anything, and record the adaptation plus the probe in the artifact's `Command:` field so the substitution is auditable rather than silent.

**Sibling failure in the same family, genuinely transport-caused, so do not conflate them:** a doubled backslash IS de-doubled between Bash and a native exe (see [[project_doubled_backslash_dedoubles_bash_to_native_exe.md]]). The dangerous instance is a regex character class: `-match "[\\/]Foo[.]cs$"` arrives as the 4-char `[\/]`, which in .NET regex is an escaped forward slash and matches `/` **only**. Probe: `$s = "[\\/]"; $s.Length` prints 4, chars 91,92,47,93. Consequence observed 2026-09-13 on issue #839 — a Cobertura per-file coverage parse returned `QFC_CLASS_NODES=0`, `QFC_LINES_VALID=0`, `LINE88_HITS=absent` and exit 0, which reads like a real measurement of an uncovered file rather than a broken predicate, because raw `dotnet-coverage` output uses Windows separators in `filename`. Fix without any literal backslash: normalise first, `($v -replace [regex]::Escape([string][char]92), "/") -match "/Foo[.]cs$"`. Single backslashes (`\s`, `\b`, `\d`) survive untouched.

**How to apply:** when a plan span exits non-zero with no output, or a cmdlet reports a binding failure, or a search/parse returns a suspiciously clean zero, suspect one of these three before suspecting the tree under test. Apply the same adaptation to every task citing that command label so before-and-after comparisons stay method-identical.
