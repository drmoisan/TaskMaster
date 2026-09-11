---
name: porcelain-diff-cannot-detect-rewrite-of-already-modified-file
description: A before/after git status --porcelain comparison cannot show that a formatter rewrote a file that already carried M status, so it is not a valid "format changed nothing" gate
metadata:
  type: project
---

The plan convention of taking `git status --porcelain --untracked-files=all` before and after a
`csharpier format` pass and concluding "the format pass rewrote no file" is **unsound for any file
that already carries `M` or `??`**. A rewritten `M` file stays `M`; a rewritten `??` file stays `??`.
The comparison can only detect a path whose tracked status CHANGED or a path that appeared or
disappeared — which, mid-plan, is nearly nothing.

**Why:** On 2026-09-07 (#798 P4-T4) the before/after porcelain sets were identical element-for-element
and I recorded "the format pass rewrote no file". Modification times then showed csharpier HAD
rewritten `UtilitiesCS/Extensions/DfDeedle.cs` during that pass, which under the repo toolchain rule
requires restarting the loop from step 1. The artifact had to be corrected after the fact.

**How to apply:** To decide whether a write-mode formatter changed anything, use one of these instead
of porcelain status:
- **Cheapest and fully decidable — run `check` BEFORE `format`.** `dotnet tool run csharpier check .`
  is read-only and exits 0 only when every file already matches formatter output. A clean pre-format
  check therefore *proves* the following `format` pass had nothing to rewrite, with no mtime
  reasoning at all. Used successfully on #798 P5-T4 and P6-T5 (2026-09-07); costs one extra ~6s
  whole-tree scan. Pair it with an mtime spot-check of the files you edited for a second
  independent observation, since edited-file mtimes will predate the format run when it wrote nothing.
- Hash the candidate files before and after the pass, or
- Run the formatter a SECOND time and compare `LastWriteTime`. If the second pass rewrites nothing,
  the tool writes only files it changes; then a first-pass mtime falling inside the first pass's
  window proves that file was rewritten.

Keep the porcelain observation for change-scope evidence, but label it as scope, not as the rewrite
gate. `csharpier format`'s own `Formatted N files in Xms.` line counts files SCANNED and reads
identically on a clean run and a repairing one, so it cannot serve either.
