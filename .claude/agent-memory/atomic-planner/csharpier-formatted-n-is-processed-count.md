---
name: csharpier-formatted-n-is-processed-count
description: CSharpier 1.x "Formatted N files" is a processed-file count, not a rewrite count — a restart-loop keyed on it never terminates; define rewritten-count via before/after SHA-256
metadata:
  type: feedback
---

A format task whose restart rule reads "if the rewritten-file count is greater than 0, restart the
loop" must define that count as the number of target files whose SHA-256 (`Get-FileHash -Algorithm
SHA256`) differs between a capture immediately before and immediately after the `csharpier format`
invocation — and must state explicitly that the console line `Formatted N files` is NOT that count.

**Why:** CSharpier 1.x prints `Formatted N files` as the count of files it *processed*, whether or
not it changed any bytes. A three-file scoped format therefore always prints 3, so a naive reading
makes the restart condition permanently true and the toolchain loop never terminates. Caught as
#511 R1 preflight BLOCKING delta 7 (2026-08-23).

**How to apply:** In every scoped-format task with a restart-on-rewrite rule: (1) require the six
hashes (before/after per file) in the evidence artifact, (2) define rewritten-count as the hash-diff
count, (3) add the explicit prohibition on using `Formatted N files`. Related:
[[csharpier-format-not-pipe-files-gate]], [[csharpier-repowide-format-breaks-zero-diff-acs]].
