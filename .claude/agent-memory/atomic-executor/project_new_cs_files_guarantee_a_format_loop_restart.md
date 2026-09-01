---
name: new-cs-files-guarantee-a-format-loop-restart
description: Any plan that creates a new .cs file with the Write tool will fail its first csharpier check, because Write emits LF and csharpier demands CRLF — budget one mandatory final-QA loop restart rather than treating it as drift
metadata:
  type: project
---

A C# file created with the Write tool lands with LF line endings. CSharpier reports it as
`Was not formatted. The file contained different line endings than formatting it would result in.`
So the FIRST pass of a final-QA loop over any plan that adds a new `.cs` file is always a repairing
pass, and the loop always restarts at least once. Plan for two passes, not one.

**Why:** on 2026-08-28 (feature 680, `[P6-T1]`) the pass-1 `PRE_FORMAT_CHECK_EXIT` was 1 on three
files — the two newly Written test files for line endings only, and one Edited file for line endings
plus a member-chain wrap. Pass 2 was clean. The restart is mechanical and expected; it is not
pre-existing drift and it does not mean the baseline format check was wrong. Feature 680's P0-T7
baseline check had exited 0 over 1554 files.

**How to apply:**
- Treat the first-pass `PRE_FORMAT_CHECK_EXIT: 1` as routine when every reported file is inside the
  plan's own edited/created set. Record it in the P6-T1 artifact as a non-final pass, restart the
  loop, and do not investigate it as drift. Only a reported file OUTSIDE that set means pre-existing
  drift entered the pass.
- `git status --porcelain` before/after the format command CANNOT observe this. Those files are
  already `M` or `??` and no commit exists yet, so a content rewrite leaves porcelain byte-identical.
  `PRE_FORMAT_CHECK_EXIT` is the only discriminator; the porcelain pair only detects a path
  ENTERING or LEAVING the changed set.
- The formatter's own `Formatted N files in Xms.` line is a PROCESSED count printed on every run,
  including a run that rewrote nothing. It never discriminates. See
  [[count-idiom-pitfalls-csharpier-and-measureobject]].
- Running `csharpier format` immediately after Writing each new file — before the final QA loop —
  avoids the restart entirely, but only if the plan does not gate on a pristine first pass.

**It is not limited to NEW files (2026-09-01, #648).** Overwriting an EXISTING tracked CRLF `.cs` file
with the Write tool also lands LF, so a single-file plan that edits one long-standing file still burns
a mandatory loop iteration. #648's `[P2-T1]` executed twice for exactly this reason and recorded
`LoopIterations: 2`. Any plan with a Phase 2 loop ceiling must budget the restart even when it creates
no new file.

**A SHA-256 pair DOES discriminate where porcelain does not.** The bullet above is right that
`git status --porcelain` is byte-identical across a repairing format run. #648's `[P2-T1]` recorded
`SHA256Before`/`SHA256After` instead and the pair separated the two executions cleanly: unequal on the
repairing run (`3fa83d3e…` → `c7f4ae79…`), equal on the clean one (`c7f4ae79…` both sides). Prefer a
content hash over a porcelain pair whenever a plan needs a write-mode formatter observation and cannot
use a pre-format `check` exit code.
