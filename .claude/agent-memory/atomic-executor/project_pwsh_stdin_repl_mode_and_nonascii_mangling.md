---
name: pwsh-stdin-repl-mode-and-nonascii-mangling
description: pwsh -Command - over a bash heredoc executes line-by-line like a REPL, so multi-line blocks silently produce NO output; it also mangles non-ASCII in both patterns and rendered output
metadata:
  type: project
---

When Bash allowlisting blocks `grep`/`cat`/`sed` and only `pwsh *` is available, `pwsh -NoProfile -Command - <<'EOF'` is the workaround. It has two traps.

**1. Stdin mode is a REPL, not a script.** Each physical line is executed as its own statement. A multi-line `try { ... } catch { }`, `foreach (...) { ... }`, or `function Name { ... }` spanning lines produces **no output and no error** — the Bash tool reports "completed with no output". This looks identical to a genuine zero result.

**Why:** the failure is silent and mimics a real negative finding, so a gate that "returns zero matches" can be a broken harness rather than a clean tree.

**How to apply:** put every statement on ONE physical line, using `;` inside braces. Variables and functions persist across lines in the same session, so build the script as a sequence of single-line statements. Always run a positive control (search for a token you know is present) before accepting any zero result.

**2. Non-ASCII is mangled in both directions.** A pattern containing an em dash or `§` passed through the heredoc arrives corrupted, so `Select-String -SimpleMatch` returns 0 for text that IS present. Rendered output is mangled too: an em dash in a `Get-Content` line prints as `-`, which can make a correct file look wrong.

**How to apply:** evaluate any search whose literal contains non-ASCII with the Grep tool (ripgrep), which is UTF-8-correct. Reserve the pwsh harness for ASCII-only literals. Observed on #729 P7-T7/P8-T16, where three em-dash searches read 0 through pwsh and 1 through ripgrep.

Related: [[pwsh-command-quoting-from-bash]], [[no-cd-or-non-allowlisted-bash-in-taskmaster]]
