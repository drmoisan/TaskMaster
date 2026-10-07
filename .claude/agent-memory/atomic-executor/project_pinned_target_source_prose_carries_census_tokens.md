---
name: pinned-target-source-prose-carries-census-tokens
description: Preflight check for plans with pinned Target Source blocks and token-count gates - comments inside the pinned code, and remarks moved into a NEW file, both carry gated tokens the planner counted only in code
metadata:
  type: project
---

Found on the #931 preflight (2026-09-28). Three count gates were unsatisfiable, and the planner's own self-review missed all three:

1. **A comment in pinned code contained a counted token.** The rewritten test's Arrange comment said
   `read-only with FileShare.ReadWrite` and `IFileInfo.OpenRead() returns ...`. That made the
   `FileShare.ReadWrite` census one higher than the table, and it also added one line to the
   "added lines containing `OpenRead()`" count.
2. **Moved text becomes added lines in a new file.** Remarks copied verbatim into a new Part2 partial
   contained `<c>Workers=0</c>`. A merge-base `-U0` diff shows every line of a created file as `+`, so
   an `ADDED-WORKERS: 0` gate fails even though no setting changed.
3. **An "added-line" count on an unchanged line depends on diff alignment.** A line identical to an old
   line (for example `stream.Length.Should().BeGreaterThan(0);`) is normally emitted as context rather
   than as `+`, so an expectation of exactly 1 is not deterministic.

**Why:** a planner counts tokens in the code statements it wrote. It does not usually re-scan its own
comments and remarks, or text it relocated, against the token table.

**How to apply:** at preflight, grep each Target Source block, comments included, for every token in the
census table and every added-line token. Treat a created file's full content as added lines.
For a line that also exists verbatim in the old file, require an "other than line X" count or a
bound rather than an exact value.
