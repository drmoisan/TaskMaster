---
name: tool-layer-collapses-double-backslash-in-file-content
description: Bash/Write tool content silently collapses `\\` to `\`, corrupting C# verbatim path literals, PowerShell regexes and evidence quotes; author such files via a Python generator using a sentinel
metadata:
  type: project
---

Any file content passed through the Bash tool's `command` parameter (heredocs included) or the
Write tool has its `\\` sequences collapsed to a single `\`. A single `\` survives; `\"` survives.
The collapse is silent — nothing errors, the file just contains the wrong text.

**Why:** observed three times in one #614 run, each with a different failure mode:

1. A C# test literal `@"\\mailbox@example.com"` was written as `@"\mailbox@example.com"`. The test
   still failed pre-fix (a single leading `\` is also a full Outlook path), so the `[expect-fail]`
   gate passed and the corruption nearly shipped. Only re-reading the written file caught it.
2. `private const char BackslashSeparator = '\\';` was written as `'\';` → CS1010 "Newline in
   constant". This one fails loudly.
3. A PowerShell regex `"\\(obj|bin|...)\\\\"` became `"\(obj|bin|...)\\"` → "Invalid pattern ...
   Too many )'s", which aborted a whole QC-loop script mid-run.

Evidence artifacts are also affected: a quoted FluentAssertions message
`not to be "\\mailbox@example.com"` landed in a committed `.md` as `"\mailbox@example.com"`,
misquoting the recorded failure output.

**How to apply:** when creating or editing ANY file whose content contains backslashes — C# verbatim
path literals, `char` escapes, regex patterns, Windows paths in markdown — do NOT write it directly.
Write a Python generator script (Write tool) that builds the text with a `BS = chr(92)` sentinel:

```python
BS = chr(92)
body = body.replace("BSBS", BS + BS).replace("BS", BS)
```

Order matters: replace the two-char sentinel first. Then run the generator with Bash and grep the
result to confirm the backslashes are right before building. The same sentinel trick is needed for
`.ps1` scripts you generate, which is more reliable than any level of inline `pwsh -Command`
quoting.

**Simpler variant, confirmed working on the #614 remediation cycle (2026-08-26):** author the file
with the Write tool using a literal placeholder such as `@@BS@@` for EVERY backslash (so the written
content contains zero backslashes and there is nothing to collapse), then run a tiny reusable
`.ps1` in the scratchpad that does `$txt.Replace('@@BS@@', [string][char]92)` and rewrites the file
with a BOM-less `UTF8Encoding($false)`. It reported the replacement count each time (20, 2, 6, 13),
which doubles as a check that no sentinel was missed. This survives repeated `Edit` calls on the
same file — edit with sentinels, re-run the desentinel script, done — and needs no Python.

The same hazard bit a large Markdown append: a `cat > file <<'EOF'` heredoc carrying ~110 lines of
prose died with ``unexpected EOF while looking for matching `'``. Write the body with the Write tool
to the scratchpad and `cat` it onto the target instead of embedding prose in a heredoc.

Related: [[preflight-gate-literal-extract-from-plan-not-retype]] covers the read side (extract gate
literals programmatically); this note covers the write side. Verify with Python `str.count()` on a
fixed string rather than `grep -F`, which returned 0 for a literal Python counted as 1.
