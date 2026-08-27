---
name: pwsh-file-array-param-from-bash
description: "`pwsh -NoProfile -File script.ps1 -Tokens a,b,c` binds ONE string, not a string[] - a gate-literal counter then silently reports HITS=0 for every token; drive multi-value input from a TSV file instead"
metadata:
  type: project
---

`pwsh -NoProfile -File <script>.ps1` passes every argument as a plain string. A `[string[]]$Tokens`
parameter given `a,b,c` from Bash binds a SINGLE element whose text is `a,b,c`.

**Why:** on the #614 remediation cycle a literal-gate counter was invoked as
`-Tokens 'MinimumCreationLength','ResolveArchiveRootOrEmpty',...`. It printed one line,
`0   MinimumCreationLength,ResolveArchiveRootOrEmpty,RootUnavailableDiagnostic,...`. Read carelessly
that is five zero-hit gates on work that was already done — a false blocker on a correct edit. The
failure is silent: no binding error, no warning, just a nonsense count for a token that does not
exist.

**How to apply:** for any script that takes a list (gate literals, file paths, test names), give it
ONE `-TokenFile` parameter and write the pairs to a tab-separated scratchpad file with
`printf '%s\t%s\n'`. Read it with `Get-Content` and `.Split([char]9)`. This also composes with
[[preflight-gate-literal-extract-from-plan-not-retype]]: the same TSV can be produced by parsing the
plan's backtick spans rather than re-typing them.

Count occurrences with `$txt.IndexOf($t, $i, [System.StringComparison]::Ordinal)` in a loop, not
with `grep`, so the count is an ordinal occurrence count immune to shell quoting and to
[[tool-layer-collapses-double-backslash-in-file-content]].
