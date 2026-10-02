---
name: git-grep-binary-flag-skips-utf16-evidence
description: git grep -I treats UTF-16 tracked files as binary, so identifier/profile-path gates built on it undercount versus any scanner that decodes UTF-16; one tracked UTF-16 msbuild log in docs/features/archive (issue 155 folder) carries ~100 profile-path lines
metadata:
  type: project
---

`git grep -I` skips every UTF-16 file (NUL bytes read as binary), and `git ls-files --eol` reports such files as `i/-text`. Measured 2026-09-28 during #927 preflight: the only UTF-16 (FF FE) text record outside .claude is the msbuild-analyzers.txt baseline log in the archive folder for issue 155; stripping NULs shows 100 profile-path lines. The inventory, GATE1-4 and any "UTF16-BOM" probe computed over `git grep -I` output are all blind to it.

**Why:** a guard or redaction helper that decodes UTF-16 by byte-order mark finds one more profile-path file than the `-I` gate, so an equality like "guard findings = raw docs + GATE4 files" fails by exactly one; and a UTF-16 probe that filters `git grep -I` results is vacuous (always 0).

**How to apply:** when a plan asserts parity between a content scanner and a `git grep -I` count, require a separate UTF-16 probe over `i/-text` records whose first two bytes are FF FE / FE FF. Related: [[project_tool_layer_collapses_double_backslash_in_file_content]].
