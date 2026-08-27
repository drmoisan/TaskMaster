---
name: preflight-gate-literal-extract-from-plan-not-retype
description: Never re-type a plan's fixed-string gate literal into a shell to verify it; extract it from the plan file programmatically, or quoting bugs manufacture false zero-hit blockers
metadata:
  type: feedback
---

When preflight-validating a "fixed-string search returns zero/N hits" gate, extract the literal
from the plan file with a regex over its backtick spans and feed THAT string to the search. Do not
re-type the literal into a `pwsh -Command` or `bash printf` invocation.

**Why:** On the #614 preflight (2026-08-26) I re-typed the P3-T2 literal
`return root + "\\" + presentedTarget.TrimStart('\', '/');` into a PowerShell double-quoted
string. Inside `"..."` PowerShell does NOT treat `''` as an escaped quote (that is single-quote-string
syntax only), so the pattern was silently corrupted and `Select-String -SimpleMatch` returned 0 hits.
The plan was correct — the literal is on source line 162 verbatim — but a 0-hit result on a
"must be >=1 now" gate reads exactly like a genuine unfailable-gate blocker. Re-typing through a
bash heredoc into `printf` corrupted the same string a second, different way. Two independent
quoting layers (bash -> pwsh -> regex/SimpleMatch) each eat backslashes and quotes differently, and
the corruption is invisible unless you echo the constructed pattern.

**How to apply:** For every gate literal, run a script that does
`[regex]::Matches($planLine, '`([^`]+)`')`, picks the span by a short unambiguous prefix, prints the
extracted literal AND its `.Length`, then searches the target file with it. Also print the target
source line trimmed and assert `-ceq`. If a gate ever reads 0 hits when the plan says it should
match, print the pattern before reporting it — assume your own quoting first, the plan second.
Related: [[verify-line-citations-with-numbered-output]],
[[csharpier-chain-wrap-defeats-singleline-search-gates]].
