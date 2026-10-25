---
name: 1005-review-residuals
description: "#1005 cycle 1: 8/8 AC PASS but 1 blocking - deleting an AC-mandated test helper stripped the only caller of an unchanged member (IDList.MaxLengthOfID 7/7->0/7); found by covered-count arithmetic, not by the executor's changed-line comparison"
metadata:
  type: project
---

#1005 (settings-store seam + FakeTimeProvider for parallel tests), review 2026-10-22T17-10: REMEDIATION_REQUIRED, 1 blocking (B-1, autonomous), all 8 AC PASS and left checked.

B-1: AC2 required deleting `CreateListWithCachedMaxLength` in `IDListGetNextToDoIDTests`; its `_ = list.MaxLengthOfID;` was the only test execution of the public getter. Baseline Cobertura 7/7 lines 4/4 branches -> head 0/7 lines 0/4 branches; `IDList.cs` branch 20.69 -> 19.35 while line rose. The executor's coverage comparison checked changed lines and replaced lines only, so it reported PASS.

**Why:** a changed-line-only comparison is blind to coverage lost on UNCHANGED lines when a test refactor removes their only caller. The tell was arithmetic: added covered lines (13) vs covered-count delta (+6) left 7 unexplained.

Cycle 2 (R4, 2026-10-22T17-23, head 3df0782ab): PASS, 0 blocking. Three store-injected MaxLengthOfID tests restored the getter to 7/7, 4/4; IDList.cs 51/169 = 30.18% / 25.81% (arithmetic matched exactly: 44 + 7). Under a git-diff/git-log-only Bash rule, read "now" from `git log -1 --format="%cI | %cr"` on the head commit (commit time + relative age).

**How to apply:** for every modified production file, compute expected covered = baseline covered + added covered lines; if head is lower, diff `<method>` nodes between the baseline and head Cobertura (the executor's baseline Cobertura usually survives in the session scratchpad under `issue-<N>/`; Glob `AppData/Local/Temp/claude/**/issue-<N>-baseline-cobertura.xml`). Also check branch-rate, not just the line figure the executor quoted. Classify as blocking-autonomous when the modified file is sub-floor and regressed (see [[modified-file-subfloor-nonblocking-disposition-230]]); keep the ACs checked when the loss sits outside their literal text. Line-110-style "branch" on a cached static lambda needs >=2 executions in one run to cover both arms.
