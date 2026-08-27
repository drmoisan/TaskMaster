---
name: grep-cr-empty-pattern-false-crlf
description: grep -c $'\r' in Git Bash can report 100% CRLF on a pure-LF file because the CR is stripped in argument passing, leaving an empty pattern that matches every line
metadata:
  type: feedback
---

`grep -c $'\r' <file>` is not a reliable CRLF detector in this environment. On a plan file that a
binary read proved contained **0 CR bytes**, it returned `239` — exactly the total line count.
The CR is stripped during argument passing, so grep receives an **empty pattern**, and an empty
pattern matches every line. The result looks like "100% of lines are CRLF" and is indistinguishable
from a real all-CRLF file by eye.

Use a binary read instead:

```
python3 -c "b=open(p,'rb').read(); print(b.count(b'\r'))"
```

Also note `sed -i 's/\r$//'` round-trips through Git Bash text mode and is not a trustworthy fix or
a trustworthy no-op signal.

**Why:** This is the second instance of the same root cause in one run. Earlier, a plan-literal
verification returned a false zero-hit because embedded double quotes were mangled by shell
escaping; the fix there was `grep -Ff` with a pattern file. Both failures share the shape: *the
shell silently altered the pattern, and the tool reported a confident, wrong count.* A count that
is suspiciously round — 0, or exactly the line total — is the tell.

**How to apply:** Before acting on any shell-measured count that gates a decision, re-measure with a
method that does not pass the pattern through the shell (binary read, `grep -Ff patternfile`, or a
Python one-liner). Applies with special force to CRLF checks before the MCP plan validator
(see [[mcp-plan-validator-requires-lf]]), because "the file is CRLF" would otherwise send you into
a pointless normalization detour — or, worse, a rewrite that risks the pervasive-diff failure in
[[mcp-plan-validator-editwrite-pervasive-diff]].
