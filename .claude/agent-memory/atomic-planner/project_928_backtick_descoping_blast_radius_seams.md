---
name: project_928_backtick_descoping_blast_radius_seams
description: Issue #928 round 5 - the parallel scheduler harvests every inline backtick span in a plan and issue.md as write claims; which token shapes it accepts, which it drops, and how to keep read-only paths out of spans
metadata:
  type: project
---

A plan's inline backtick spans are harvested as WRITE claims by the parallel run scheduler
(`.claude/lib/blast-radius/BlastRadiusExtraction.psm1`, `Get-PathTokenKind`). A read-only path or
glob left inside a span serializes the item behind every sibling that touches the same path.

**Why:** The #928 plan's P2-T14 wrote its Glob step as a span reading "Glob pattern" followed by a
double-star xml glob. The span splits on whitespace, the glob token has a recognized extension, and
it was recorded as a write over every xml file, which overlapped three sibling items and serialized
the item although the task only reads. The revision moved both globs into plain prose.

**How to apply (verified against the module on 2026-09-28):**
- Harvest is per line, inline spans only (a backtick, one or more non-backtick characters, a
  backtick). Fence lines of three backticks match no span and fenced content carries no backticks,
  so fenced command blocks are outside the harvest.
- Accepted: a whitespace-free token with a forward slash not at index 0, no colon before the first
  slash, no placeholder marker (angle brackets, dollar-brace, dollar-paren, percent), and either a
  recognized final extension (cfg cs csproj ini js json jsx lock md ps1 psd1 psm1 py sh sln toml ts
  tsx txt xml yaml yml, after stripping a trailing colon-digits line suffix) or, for a wildcard
  token, a known top-level segment (scripts/ tests/ docs/ config/ schemas/ packages/ extensions/
  .claude/ .codex/ .github/ .agents/).
- Dropped, so safe to leave in spans: directory-shaped pathspecs (scripts/vscode), origin/main, a
  scan_folders JSON literal (the closing quote-and-bracket defeats the extension test), regex
  character classes, a trailing-slash token, anything carrying the timestamp placeholder, and
  line-anchored citations WITHOUT a slash (Foo.ps1:52).
- Keep only Write Set paths and genuinely written files inside spans; cite every context path
  (sibling tests, part files, workflow files, tasks.json, artifacts/pester documents, archived
  evidence) in plain prose. State that convention once in the plan preamble so a reviewer knows it
  is deliberate.
- The working-tree copy of a committed plan and issue.md is CRLF on every line under `* text=auto`
  with Windows autocrlf; the blob is LF. Edit in place; do not normalize (see
  [[crlf-plans-validate-do-not-normalize]]).
