---
name: project-823-self-anchor-diff-base-seams
description: "#823 repair pass — a fixed commit literal as diff anchor degenerates one epic generation later; replace with an execution-time self-anchor and reconcile the inherited-set consequence"
metadata:
  type: project
---

Issue #823 (quickfiler teardown review residuals) required replacing a fixed diff anchor with an execution-time self-anchor. Seams worth carrying forward:

**A fixed commit literal as diff anchor always degenerates in an epic.** The predecessor draft pinned the anchor to the epic-integration head and measured a one-path three-dot diff against it. The epic-planner then fanned sibling work into the same branch; the identical command listed 21 paths a day later. `epic-orchestrator` branches the execution worktree from the integration branch at execution time, so *any* literal fixed at planning time is an ancestor of the future head and the three-dot diff collapses to a two-dot diff.

**Why:** the merge base of `A...B` is `A` itself when `A` is an ancestor of `B`, so the "only my work" property a three-dot diff is chosen for is exactly what an ancestor anchor destroys. It bills every sibling's whole delivery to this feature's footprint criteria.

**How to apply:** have Phase 0 run `git rev-parse HEAD` before any edit and record it as a `BASE-SHA:` artifact field; later tasks transcribe the literal 40-char SHA into their own command spans (same mechanism as `VSTEST-PATH`; no shell variable survives a task boundary). Use the bare token `BASE-SHA` — not `<BASE-SHA>` — as the left ref operand in plan command spans: it satisfies G8's non-flag-ref-operand requirement while avoiding the `<`/`>` placeholder guard that makes a token invisible to G5/G6.

**Reconcile the consequence, don't leave it dangling.** With a self-anchor, `git diff BASE-SHA...HEAD` at capture time is EMPTY by construction, so a two-clause inherited-path rule whose clause A was "already changed relative to the anchor" collapses to the `git status --porcelain --untracked-files=all` set alone. Every place that defines, captures or subtracts the inherited set has to say so, or the plan describes a capture that cannot produce output. Clause B (`.claude/agent-memory/`) stays load-bearing and is NOT redundant: the porcelain snapshot is taken once at Phase 0 and is structurally blind to agent-memory writes the executor makes later in the run.

**Collapse a multi-paragraph decision bullet back to one line.** Rewriting D2 produced a bullet with a blank line and an indented continuation, unlike every sibling decision. The MCP plan validator had returned ok=true on the single-line shape; do not change document shape you cannot re-validate.

## Preflight round 1 (2026-09-09) — honesty seams in footprint and check-off tasks

**A footprint AC that spans two mechanisms cannot be gated on one merged field.** AC26 forbids a `.claude/` path in the anchored diff AND an untracked addition under `.claude/`. Those are different tests, and a *modification* to a file that was already tracked at Phase 0 is outside both. A single `DOTCLAUDE-PATHS:` union field cannot say which clause failed, so it converts a real violation into a judgement call. Split into `DOTCLAUDE-DIFF-PATHS`, `DOTCLAUDE-UNTRACKED-PATHS` and `DOTCLAUDE-MODIFIED-PATHS`, and let only the first two block the check-off.

**Never apply an inherited-path carve-out to an AC whose own text has no carve-out.** AC28 ranges over every untracked path with no exception, so subtracting `.claude/agent-memory/` inside the gate makes the plan report MET where the spec's text is not met. Keep the subtraction for the Write Set boundary check, and additionally record `AC28-RESIDUAL-UNTRACKED` so the divergence between the gate and the criterion is on the artifact.

**Check-off tasks must be able to complete with the AC left unchecked.** If a check-off task's acceptance unconditionally demands `- [x]`, the executor's only route to a completed task is to check off a criterion that failed. Write them as "either `[x]` and the fields hold, or `[ ]` and the note records `<AC>: NOT MET` naming the failing values; this task completes in either case", and say so explicitly so the plan-completion count is unaffected. The matching summary task must then carry `TOTAL: <checked> of N` counted from its own lines plus an `UNMET:` line — a predicted `N of N` makes an honest NOT MET unrepresentable.

**A "zero failed tests" AC is not dischargeable by a baseline-subset argument.** `NEWLY-FAILING: NONE` is subset logic and is the right gate for a regression check, but it does not satisfy a criterion worded "completes with zero failed tests". Gate the check-off on `CONFIRMING-FAILED: 0` and have the NOT MET branch state, per failing test, whether it is also in the baseline failing set.

**A repo-wide token-absence AC is unsatisfiable once the plan quotes the token.** `git grep -c -F "_userEmailRetryAttempted;"` returned 7 files: 1 source declaration and 6 prose quotations across the spec, this plan, the research record and three issue-812 artifacts a decision record requires to stay byte-identical. Scope the gate to the source directories, run the repo-wide form as well, and classify every repo-wide match, so the divergence from the criterion's wording is auditable rather than silent.

**A presence gate is a no-op when an earlier task already wrote the token into the same file.** Three tasks add prose stating the same new bound to one test file; the last one's `git grep ... prints at least 1` would pass whatever it did. Reserve a distinct longer literal for the later task, assert its count as exactly 1, and append an explicit prohibition on that literal to every earlier task that touches the file.

**The same rationale sentence can be spelled differently in two files.** `registration hop runs from a form lookup` in the production file and `registration hop runs from a form-lookup` (hyphen) in its test file. A token baseline scoped to one file does not cover the sibling, and the sibling's copy survives the edit unnoticed.

## Preflight round 2 (2026-09-09) — expectation keying, post-format line numbers, and self-growing match sets

**Never key an `ExpectedExitCode:` to a recorded baseline count.** Four tasks declared "1 when the baseline failing count is non-zero". The loop-closure task then restarts the whole QA loop on any mismatch, and because the declaration is recomputed from the same fixed baseline each iteration, a baseline failure that does NOT recur produces a permanent mismatch: declared 1, observed 0, restart, forever. Key the declaration to the run that carries it — "1 when THIS run reports at least one failed test and every failed name is also in the baseline set, 0 when this run reports none" — which makes the equality clause a consistency check and leaves `NEWLY-FAILING: NONE` as the discriminating gate.

**A committed-history three-dot diff is the wrong instrument once a formatter runs after the last commit.** Changed-line coverage cross-references diff line numbers against a Cobertura document generated from the working tree. If the final format pass rewrites files that earlier phases already committed, and the final commit is later still, `BASE-SHA...HEAD` reports pre-format line numbers against post-format coverage. Use the two-dot `git diff BASE-SHA` there: it still carries an explicit ref operand (G8 satisfied) and it is not a name-listing diff (no G8b porcelain companion needed).

**`--numstat` carries no hunk.** An acceptance that reads a diff hunk to confirm every changed line begins with `//` has no source when the only command in the span is `git diff --numstat`. Pair the counts command with a plain `git diff` over the same pathspec.

**A repo-wide match enumeration written into a plan grows while the plan is executed.** The `_userEmailRetryAttempted;` set was 7 files at round 1 and 8 at round 2 — the eighth being THIS memory file, written during the same planning work. Word such a list as a lower bound ("at least ... classify every match the run actually reports"), never as a closed enumeration.

**Widening an AC-residual field must respect the AC's own permissions.** Extending `AC28-RESIDUAL-UNTRACKED` from clause B to every untracked path exposed a new false positive: the Phase 0 porcelain capture sees this plan's own first evidence artifact, which AC28 explicitly admits as a feature-folder file. Carve out exactly what the criterion's text admits (Write Set plus the feature folder) and nothing more.

Related: [[never-pin-head-sha-as-plan-expectation]], [[diff-gates-need-a-commit-task]], [[agent-memory-is-tracked-scope-git-gates]], [[porcelain-collapses-untracked-directories]], [[feedback_ac_checkoff_one_per_task]], [[zero-hit-grep-gates-need-carveouts]], [[acceptance-edits-must-be-false-before-true-after]].
