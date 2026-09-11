---
name: bash-tool-eats-dollar-vars-and-drops-git-output-flag
description: Two ways a file-writing shell command silently produces no file — bash expands $vars inside a pwsh -Command double-quoted string, and git diff --output=<file> writes to stdout instead
metadata:
  type: reference
---

Both failures are silent: the command reports success and the file is simply absent. Verify
the file exists before depending on it.

**1. `git diff --output=<file>` did not write the file.** The 90 KB diff came back as tool
stdout and `Glob` found nothing at the target path. Do not trust `--output` through this tool.

**2. The Bash tool expands `$name` before pwsh ever sees it.** The Bash tool is Git Bash, so a
double-quoted `pwsh -NoProfile -Command "... $w ..."` has `$w` substituted by *bash* first —
usually to the empty string. The observed symptom is a pwsh parse error on the orphaned
operator, for example `The term '=' is not recognized`, followed by cascading type-conversion
errors on the now-fragmented arguments.

The same mechanism produces a **false pass**: `"$null = Get-Content X | ConvertFrom-Json; 'JSON OK'"`
became `"= Get-Content X | ConvertFrom-Json; 'JSON OK'"`, which errored on `=` and then printed
`JSON OK` from the surviving second statement. The JSON was never parsed, and the output looked
like a successful validation.

**How to apply.** Write pwsh one-liners with **no `$` variables at all** — nest the calls
instead:

```
pwsh -NoProfile -Command "Set-Content -LiteralPath 'OUT' -Encoding utf8 -Value (git -C 'WT' diff BASE HEAD -- . ':(exclude).claude')"
```

This also avoids a `|` in the command text, which matters because hooks pattern-match the raw
Bash command string (see [[hooks-pattern-match-bash-command-text]]).

For JSON validity, prefer the MCP `validate_orchestration_artifacts` call over a hand-rolled
pwsh parse: it both parses and schema-checks, and it cannot degrade to a vacuous pass.

Related: [[pwsh-double-quoted-command-refused-in-worktree]],
[[feedback_no_cd_or_non_allowlisted_bash_segments]],
[[feature-review-git-c-form-hangs-unattended]] (the consumer of the patch file).
