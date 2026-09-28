---
name: conservation-gate-git-show-decodes-ibm437-and-keeps-bom
description: A plan's line-multiset conservation gate that reads the base file via `git show` in pwsh mis-decodes every non-ASCII char (console OutputEncoding is ibm437) and keeps the UTF-8 BOM as U+FEFF on line 1, so a perfect pure move prints a non-zero diff count; set [Console]::OutputEncoding to UTF-8 and strip U+FEFF before comparing, and record the correction
metadata:
  type: project
---

The atomic-plan "conservation gate" shape (`Compare-Object` over trimmed, sorted lines of
`git show BASE:path` versus `Get-Content` of the result files) is asymmetric in decoding:
`git show` emits raw bytes that pwsh decodes with `[Console]::OutputEncoding` (ibm437 on this
workstation), while `Get-Content` decodes the files as UTF-8. Two artifacts follow on any
BOM-bearing C# file with a non-ASCII character in a comment:

- an em-dash (U+2014) on the base side becomes `0393 00C7 00F6`, so that line appears once on
  each side of the diff;
- the BOM arrives as U+FEFF prefixed to `using System;` on line 1; `.Trim()` does not remove
  U+FEFF, so the `^using ` exclusion misses it and it lands in the multiset.

**Why:** observed 2026-09-17 on #792 [P2-T1]: the six-way `EfcFormController.cs` split was
byte-correct on disk (`git diff --numstat` = `1 1056`, only the declaration line changed) yet the
literal gate printed `CONSERVATION-DIFF-COUNT: 3`. The on-disk files were verified by dumping
code points from `ReadAllLines` and `Get-Content` (both `2014`) before touching the instrument.

**How to apply:** in the gate script, set `[Console]::OutputEncoding =
[System.Text.UTF8Encoding]::new($false)` before calling `git show`, strip a leading U+FEFF from the
first returned line, run the plan's filter unchanged, and record the correction plus the
uncorrected count in the evidence. Keep the positive control (re-run with one result file
omitted, expect a non-zero count) so the corrected gate is shown to discriminate. Do not "fix"
the diff by editing files; verify bytes on disk first.

Related: [[project_bom_grep_anchor_false_negative]], [[project_pwsh_stdin_repl_mode_and_nonascii_mangling]],
[[project_tool_layer_collapses_double_backslash_in_file_content]]
