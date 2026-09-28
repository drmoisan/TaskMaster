---
name: poshqc-format-strips-bom-and-crlf-only-when-it-rewrites
description: PoshQC format emits LF and drops the UTF-8 BOM on any file it actually rewrites, and leaves both intact on a clean file — which turns BOM/CRLF retention into the write-mode observation the exit code cannot give
metadata:
  type: project
---

`mcp__drm-copilot__run_poshqc_format` exits 0 and returns the same `ok:true` summary whether it
rewrote a file or not, so its exit code is not an observation. There is a usable one: when the
formatter **rewrites** a `.ps1`, it writes the file back with line-feed endings and **no** UTF-8
byte-order mark. When the file is already formatter-clean it is not touched at all, so a pre-existing
CRLF + BOM file keeps both.

Observed on 2026-09-13 (item 873): a newly created test file written as CRLF + BOM came back
LF + no-BOM after the first format invocation; the sibling part file written the same way kept CRLF +
BOM. Re-adding the BOM and re-converting to CRLF, then formatting a second time, left both properties
intact — proving the content was clean at that point, not that the formatter preserves encoding.

**This is not limited to new files.** In the same item's Phase 3, an *existing tracked* BOM-less
production script (`scripts/vscode/Invoke-MSTestWithCoverage.ps1`) that had been hand-edited came back
`crlf=0 lfonly=438` from the first format pass. So any file whose content the formatter decides to
touch loses CRLF, whether this delivery created it or merely edited it. Restore each rewritten file to
the encoding it is *stored* with — BOM-less CRLF for a pre-existing file, CRLF + BOM for a new one —
rather than applying one rule to both, and confirm with a line-scoped `git diff --numstat` against the
base anchor: a small insert/delete pair proves the restore worked, whereas a whole-file count means the
encoding is still wrong.

**How to apply:** two things follow.

1. Record `bom=` and `crlf=`/`lfonly=` counts before and after each format invocation and use
   retention-versus-loss as the "did it rewrite anything" evidence a write-mode gate demands. Per-file
   byte counting is needed; `Get-Content` hides both.
2. This repository's tracked `.ps1` files are CRLF in the working copy (core.autocrlf checks out
   CRLF, index blobs are LF), and the plan convention for TaskMaster requires a BOM on new `.ps1`
   files. So after a rewrite, restore BOM + CRLF and format once more to confirm stability. Do not
   accept the formatter's LF output as final just because "the formatter wins": the second pass shows
   CRLF + BOM is a fixpoint, so there is no conflict to concede.

**The Edit tool is what triggers the rewrite.** Edit/Write insert LF-terminated text into a
CRLF file, leaving mixed terminators; PoshQC then normalises the whole file to LF on the next
invocation. So *every* PowerShell file touched by Edit guarantees one format-loop restart. Budget
for two format invocations, not one.

**Do not reflexively restore CRLF in TaskMaster.** `git check-attr text -- <path>` reports
`text: auto` for `scripts/**` and `tests/**`, so Git normalises terminators into the object
database on write and restores CRLF on checkout. The formatter's LF output therefore never
reaches the blob and `git diff --numstat` already reports content lines only. Verified
2026-09-20 on issue #911: four files came back all-LF from the first format pass and numstat
read `9 1`, `8 3`, `41 0`, `48 0` — content-sized, with no restore performed. The restore
procedure above is needed only where `.gitattributes` pins `eol=crlf`; check `git check-attr`
before spending edits on it.

**The idempotence observation that costs nothing:** take an aggregate SHA-256 over the per-file
`Get-FileHash` of every `*.ps1`/`*.psm1`/`*.psd1` in scope immediately before and after the
invocation. Identical aggregates is the "rewrote nothing" evidence the exit code cannot give,
and it needs no per-file BOM/CRLF accounting. From bash, single-quote the whole `pwsh -NoProfile
-Command '...'` argument so bash does not collapse the backslashes in the path.

Related: [[powershell-bom-required]], [[project_bom_grep_anchor_false_negative]],
[[poshqc-analyze-exit1-on-warning]].
