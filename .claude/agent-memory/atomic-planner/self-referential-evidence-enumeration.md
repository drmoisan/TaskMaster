---
name: self-referential-evidence-enumeration
description: A gate that captures git status and then asserts the capture lists every evidence artifact the plan names is unsatisfiable — its own artifact and every later one do not exist yet
metadata:
  type: feedback
---

An evidence-enumeration gate must be bounded by the task that performs the capture. A clause like "the recorded `git status --porcelain --untracked-files=all` output must list every `EVIDENCE/<kind>/` artifact path this plan names" cannot hold: the capturing task writes its own artifact *after* running its git commands, and every artifact written by a later task is likewise absent from the capture.

**Why:** issue #731 round 2 found this in `[P5-T9]`. Three named artifacts post-dated the capture — the task's own `scope-boundary.md`, `[P5-T10]`'s `file-size-audit.md`, and `[P6-T1]`'s `ac-traceability.md`. The condition read as a thorough completeness check and was in fact impossible to satisfy.

**How to apply:** bound the enumeration by an explicit task-ID range ending at the last task that writes before the capture (for example "every artifact path named by `[P0-T1]` through `[P5-T8]`"), and then name the excluded later artifacts individually with the task that writes each one. Naming them makes the exclusion auditable instead of implicit, so a reviewer can confirm the boundary is at the capture point rather than a convenient place to stop.

The same shape appears whenever a plan asserts over a snapshot it takes of its own output: file-size audits that must run after the final formatter, clean-tree commits that must precede the artifacts recording them ([[terminal-phase-planner-traps]]), and coverage baselines destroyed by the run that consumes them. Before writing any "must list every X" clause, place the capture on the task timeline and delete from the enumeration everything downstream of it.

Related: [[porcelain-collapses-untracked-directories]], [[diff-gates-need-a-commit-task]].
