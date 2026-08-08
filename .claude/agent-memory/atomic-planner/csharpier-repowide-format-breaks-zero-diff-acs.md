---
name: csharpier-repowide-format-breaks-zero-diff-acs
description: A repo-wide `csharpier format .` in a final-QC phase can silently violate a zero-line-diff acceptance criterion; scope the mutating pass to the plan's own file list and keep `csharpier check .` as the read-only gate
metadata:
  type: feedback
---

When a plan carries a zero-line-diff acceptance criterion for specific files (an R4-style "do not touch `X.cs`" constraint), the final-QC formatting task must run `csharpier format` with an EXPLICIT scope-locked path list, never `csharpier format .`. The repo-wide read-only `csharpier check .` stays as the gate, with acceptance expressed as "exit 0, OR the reported unformatted set is exactly the merge-base set captured in Phase 0 and contains none of the scope-locked paths".

**Why:** `csharpier format .` rewrites every file that is unformatted at the merge-base, not just the files the change touched. If any protected file happens to be unformatted at merge-base, the format pass silently produces a diff on it and converts the zero-line-diff AC from PASS to FAIL — after the executor has already recorded a clean toolchain pass. Encountered while planning #503 (2026-08-08), where AC15 requires a literal zero-line diff on `TaskMaster/AppGlobals/AppItemEngines.cs` and `UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs` and the merge-base csharpier state was unmeasured.

**How to apply:** (1) Add a Phase 0 task that runs `csharpier check .` read-only and records the merge-base unformatted-file set verbatim — this is the comparison basis, not just a pass/fail. (2) Add a Phase 0 task that derives the formatting-scope rule and records whether any protected file is in that set. (3) Name the scope-locked `.cs` path list once in the plan and paste it into the `csharpier format` command. (4) Re-verify the zero-line diff AFTER the format pass, not only before it — a second `git diff --numstat <merge-base>..HEAD` task at the end of the QC phase. Related: [[csharpier-format-not-pipe-files-gate]], [[feedback_postformat_file_size_audit]].
