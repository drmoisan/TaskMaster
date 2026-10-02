---
name: project_952_runbook_and_workflow_comment_plan_seams
description: "#952 R0 minimal-audit plan seams (Markdown runbook line + YAML workflow comment rewrap) - .yml edits are pre-implementation-gated, the prompt's line-length figure was off by 5 (CRLF), a removed Markdown bullet shows as `-- ` in a diff so exclude only `--- `, actionlint binary is tracked and prints nothing on success, Pester static tests read both files"
metadata:
  type: project
---

Plan for #952 (runbook line 301 `App ID` to `Client ID`; dependabot-repair.yml header comment rewrapped to 100 columns). 28 tasks, 3 phases, no commits, no C# or coverage tasks.

- **`.yml` edits are implementation to the pre-implementation gate.** `.claude/hooks/enforce-orchestration-preimplementation-gate.ps1:123` lists `yml|yaml` (and `json`), and `:141` classifies `pwsh ... Invoke-Pester` / `tests/scripts/` commands. A comment-only workflow edit therefore needs the ready checkpoint; `.md` edits do not. Read the checkpoint read-only in Phase 0 and stop if not ready.
- **Re-count a caller-supplied line length.** The prompt said line 14 was 150 characters; a hand count gave 145 and a ripgrep probe matched `^.{146}$` because the file is CRLF (ripgrep's `.` counts the CR; `ReadAllLines` strips it). Probe exact lengths with `^.{N}$` and check `\r$` counts before writing a baseline figure; gate the baseline only on "greater than the limit" and record the measured value.
- **A removed Markdown bullet appears as `-- text` in a git diff.** Excluding diff headers with `StartsWith("--- ")` (three dashes plus space) keeps the `-- Private key...` body line; a two-dash exclusion would drop it and make the "exactly one removed line" gate vacuous.
- **Line 13 of the old block equals line 1 of the new block**, so git reports 3 removed / 4 added, not 4 / 5. Gate net growth (`ADDED - REMOVED = 1`, `CHANGED = ADDED + REMOVED`) and record the exact split as expected-not-gated.
- **actionlint:** `actionlint-bin/actionlint.exe` is tracked (no `.gitignore` rule; only `.paket/paket.exe` is ignored at line 300) and present in a fresh worktree; `scripts/dev-tools/run-actionlint.ps1` throws `actionlint executable not found` when absent, runs the binary with no args, prints nothing and exits 0 on success (recorded in #927 p5-t4 and #929 p0-t19 evidence). Run it as a child `pwsh -File` by absolute path and read `$LASTEXITCODE`; gate exit 0 AND output-line count 0.
- **Pester static tests read both target files:** `tests/scripts/dependencies/DependabotConfig.Tests.ps1` (17 `It`, reads the workflow at 296-420) and `RepositoryTreeConsistency.Tests.ps1` (4 `It`, reads workflow and runbook). They assert step inputs and Part D text, never the header comment, but they are the test stage for the touched files; baseline + final run with `NEWLY-FAILING: NONE`.
- **Apostrophes in asserted text:** the comment paragraph contains `App's`; a single-quoted `pwsh -Command` payload cannot carry it, so build the expected string with `+ [char]39 +`.
- **Post-edit line numbers shift.** A five-for-four replacement moves every later line by one; any later task that reads the file by line number (the AC5 disposition read of the `on:` and `if:` blocks) must cite post-edit numbers and say so.
- The feature-review rule `modified-workflow-needs-green-run` (`.claude/skills/feature-review-workflow/SKILL.md:68-74`) cannot be met locally for a `workflow_run`-only workflow with a `dependabot/` branch filter and no `workflow_dispatch`; record a disposition under `evidence/other/`, leave the AC unchecked, list it under Items remaining.

**Why:** each seam would have produced a vacuous gate, a spurious stop, or a preflight finding.
**How to apply:** reuse for any docs-plus-workflow-comment item. Related: [[project_945_sortemail_trysave_directory_seam_plan_seams]], [[empty-porcelain-clause-is-unsatisfiable]], [[pwsh-command-quoting-in-plan-tasks]], [[validate-planner-output-hook-line-anchored-gotchas]].
