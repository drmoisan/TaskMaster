---
name: doubled-backslash-dedoubles-bash-to-native-exe
description: A `[\\/]` regex passed from Bash to a native .exe arrives de-doubled as `[\/]`, silently degrading to forward-slash-only; verify the delivered pattern against a [char]92 construction
metadata:
  type: project
---

A regex containing a doubled backslash — the canonical host-path shape pattern
`[A-Za-z]:[\\/]Users[\\/]` — arrives at a native Windows executable as `[A-Za-z]:[\/]Users[\/]`
even when the Bash argument is **single-quoted**. Bash single quotes do not do this; the
MSYS/Git-Bash argv-to-Windows-command-line conversion layer does.

**Why it matters:** `[\\/]` is a class matching backslash OR slash. The de-doubled `[\/]` is a
class matching an escaped `/` only, so it CANNOT match a backslash separator. The degraded
pattern still returns the right count on a tree whose leaks happen to use forward slashes, so a
zero-hit or exact-count gate passes and nothing looks wrong — but the sweep is blind to any
`C:\Users\...` spelling it was written to catch. This is a silently-wrong measurement, not an
error.

**How to apply:**
- Quadruple in the Bash source: `"[A-Za-z]:[\\\\/]Users[\\\\/]"` delivers the correct doubled form.
- Then PROVE it, in the same run, before trusting any count:
  ```
  $b=[char]92; $canon = "[A-Za-z]:[" + $b + $b + "/]Users[" + $b + $b + "/]"
  Write-Output ("PATTERN_IS_CANONICAL: " + ($shape -ceq $canon) + " LEN=" + $shape.Length)
  ```
  Expect `True` and `LEN=24`. A `[char]92` construction cannot be touched by any shell layer, so
  it is the reference. Record the check in the evidence artifact.
- Cheap early detector: `Write-Output "PATTERN_ECHO_CHECK: <pattern>"` at the top of the run and
  read what actually printed. That is how this was caught.
- Note the same hazard does NOT apply to the Write tool: doubled backslashes written into a
  `.md` artifact survived intact (verified by searching the written file for the `[char]92`
  construction), contradicting the older blanket note in
  [[project_tool_layer_collapses_double_backslash_in_file_content]] for this case.

**Reliable counting idiom when every escaping route fails.** Verifying a "letter-anchored
absolute path" claim needed a count of drive-letter-plus-separator occurrences. Three routes
failed in a row: the Grep tool with a quadrupled class returned `across 0 files`; a bash
`grep -cE` with the same class reported `Trailing backslash`; and writing the pattern to a file
with `printf '%s\n'` and using `grep -f` ALSO reported `Trailing backslash`, because the
doubling was collapsed before bash ever saw the argument. What works is to remove backslashes
from the problem entirely by translating them first:
```
tr '\\' '/' < "$f" | grep -cE '[A-Za-z]:/'          # drive-anchored count
tr '\\' '~' < "$f" | grep -o ':~' | wc -l           # bare drive-root count, no URL false hits
```
`tr` receives a backslash whether or not the layer collapses `\\`, so the translation is
order-independent. Use `~` (not `/`) when the file is Markdown, or every `https://` counts as a
hit. This is the idiom to reach for whenever a preflight claim is stated in terms of Windows
path separators.

Related: [[project_unquoted_backslash_in_bash_arg_silently_redirects_output]],
[[project_bash_heredoc_collapses_doubled_backslashes]],
[[project_preflight_gate_literal_extract_from_plan_not_retype]]
