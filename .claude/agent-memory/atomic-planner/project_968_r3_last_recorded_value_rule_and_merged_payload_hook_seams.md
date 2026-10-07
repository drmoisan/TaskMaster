---
name: project-968-r3-last-recorded-value-rule-and-merged-payload-hook-seams
description: Issue #968 preflight round 3 seams — a post-format census that demands "every earlier task's value" is unsatisfiable once a later task supersedes an interim value; classification stop-rules must enumerate method-group assignments; merged pwsh payloads trip the gh+issue+new hook; spec line counts drift after amendments
metadata:
  type: project
---

Four seams from the #968 round-3 reviewer report (2026-10-03), plus one session constraint.

1. **"Every P2-T6, P3-T9, P4-T9, P5-T5 ... value holds after formatting" cannot pass when a later task deliberately supersedes an earlier task's interim value** (P4-T6 records `Task.Yield` 3 before the P5-T3 rewrite makes it 0; P4-T9 records the project file at "plus 2" before P5-T2 makes it "plus 3"; P1-T3 records numstat `1	0` before two more items make it `3	0`). Phrase the post-format gate as "as last recorded for its file (task X for file A; task Y for file B ...)" and restate any numstat row that moved. The sibling task-description line that merely enumerates *which commands* to re-run is satisfiable and should stay.
2. **A classification stop-rule ("any other category is INVOCATION: stop") must enumerate every hit shape the primary pattern returns.** The `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` method-group assignments were neither a declaration nor a cref nor a nameof, so a correct executor would have stopped. Walk every hit line of the pattern against the category list before writing the stop rule.
3. **A fact that cites "N lines" for a Grep over `*.cs` must be re-counted after every file-list change** — the 24 was carried from the research addendum while the real count was 25 (the two method-group lines were the ones missed). Per-file breakdowns (17 in file X, with line numbers) make the drift detectable next round.
4. **Spec line totals and phrase counts drift when the spec is amended after the facts were captured** (335 -> 334 lines; the amended line 10 picked up a phrase that was counted at 3). Re-count spec facts in the same pass as any spec amendment, even when the gate is only "at least 1".
5. **Hook: `hook-command-scanner.ps1` treats a `pwsh` payload as raw text and refuses it when `gh` + `issue` + (`create`|`new`) all appear as plain substrings anywhere** (`High`, `through`, `issue #424`, `new BackgroundWorker()` together trip it). The plan now states that payloads are never merged into one call. A reviewer who combines several plan payloads into one probe for speed can hit this while the plan's own single payloads pass.

Session constraint repeated from #964 R4: a planner session with no Bash tool cannot run `git hash-object` or `Get-Date`; report the blob SHA and timestamp as orchestrator-computed items and verify everything with Grep/Read.

**Why:** Each of 1 to 4 would have been a run stop on a correct execution (1 and 2) or a false citation (3 and 4); 5 turns into a run stop under a D-10 rule that treats any pwsh refusal as `PRE-IMPLEMENTATION GATE BLOCKED`.

**How to apply:** When a plan has interim-then-final edits to the same file across phases, the post-format census must name the last recording task per file. When a plan classifies grep hits with a fall-through stop category, read every hit. When a fact cites a repository-wide count, record the per-file breakdown. See also [[project-964-r4-post-merge-footprint-reanchor-with-negative-controls]] for the no-Bash constraint.
