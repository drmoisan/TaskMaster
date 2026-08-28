---
name: edit-tool-crlf-ifies-lf-markdown
description: Under core.autocrlf=true the Edit tool can rewrite an LF markdown file entirely to CRLF — measure with a binary read and normalize before scripted checkbox edits
metadata:
  type: feedback
---

Two `Edit` tool calls against an LF-only `spec.md` (blob in `HEAD`: 0 CRLF, 970 LF) left the working file at **971 CRLF of 971 lines** — a whole-file line-ending conversion, not a per-hunk one. `core.autocrlf` is `true` in this repository, which is the enabling condition.

Consequences:
- A later scripted edit that asserts LF (or that writes with `newline=''`) either fails or mixes endings.
- `git diff` emits `warning: in the working copy of '<path>', LF will be replaced by CRLF the next time Git touches it`, which is *normal* under `autocrlf` and is **not** evidence of a problem — the committed blob is still LF. Do not "fix" that warning.
- The MCP plan/orchestration validators reject CRLF plans ([[mcp-plan-validator-requires-lf]]).

**Why:** a silent whole-file ending flip is invisible in `git diff` output (git normalizes on read under `autocrlf`) but breaks any tool that reads the bytes directly.

**How to apply:**
- Measure with a binary read, never a shell grep: `python -c "b=open(p,'rb').read(); print(b.count(b'\r\n'), b.count(b'\n'))"`. A `grep`-based CR probe is unreliable here — see [[grep-cr-empty-pattern-false-crlf]].
- Compare against the blob: `git show HEAD:<path>` counted the same way tells you whether you introduced the change or inherited it.
- Normalize before scripted edits: read `rb`, `replace(b'\r\n', b'\n')`, write `wb`.
- Prefer a small Python script over the `Edit` tool for mechanical, repetitive markdown edits (checkbox flips, timestamp substitutions). It is faster, it preserves endings deterministically, and it can assert the anchor matched — the `Edit` tool's per-call anchor requirement makes 25 checkbox flips 25 round trips.
- After editing, confirm the diff is surgical with `git diff --numstat <path>`; a line count near the whole file means the endings flipped, not the content.

Related: [[mcp-plan-validator-editwrite-pervasive-diff]].
