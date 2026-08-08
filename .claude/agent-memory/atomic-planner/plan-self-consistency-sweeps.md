---
name: plan-self-consistency-sweeps
description: Three sweeps to run before handing a plan to preflight — preamble-vs-task-list reachability, post-format file-size coverage of every new test file, and named input artifacts for any "demonstrated against a concrete file" gate
metadata:
  type: feedback
---

Run these three sweeps over a finished plan before submitting it for `atomic-executor` preflight. Each
one caught a blocking finding on the #456 F14 plan (259 tasks, 15 phases) that no other check would have.

**1. Preamble-vs-task-list reachability.** Every member, file, or seam a phase preamble promises must have
a task that actually creates it. Cross-check by name, not by count: the #456 preamble said "the three
unreferenced members are covered through seam S2b" while the tasks widened only two, leaving one test task
(`[P3-T21]`) unsatisfiable without an unauthorized production edit or private reflection.

**2. Post-format file-size sweep over ALL new test files.** `.claude/rules/general-code-change.md`
§ File Size Limit binds test code, not just production code. A mid-plan size check is insufficient because
`csharpier format` in the final QA phase can add lines. The final phase needs one task that counts every
new test file plus every touched production file **after** the formatter pass, with a restart-at-format
branch if any file reaches 500. A spec-AC Definition-of-Done check does not substitute: those ACs are
usually scoped to production files only.

**3. Name the input for any "demonstrated against a concrete file" clause.** A gate acceptance that says
"demonstrated rather than assumed" but names no input forces the executor to invent one. For a halt gate
this is fatal. Cite a committed artifact from another feature folder (e.g. a prior child's
`evidence/qa-gates/coverage-final.cobertura.xml`) and name the specific rows to read. Do not point at an
artifact a later task in the same plan produces.

**Why:** all three were blocking preflight findings on one plan, and each cost a full revision round-trip.
**How to apply:** run them as a final pass after the plan is otherwise complete, before returning the
preflight signal.

Related: [[ac-source-sweep-definition-of-done]], [[named-coverage-exception-verify-member-body]],
[[verify-line-spans-and-computed-literals]], [[project-456-f14-itemviewer-plan-seams]].
