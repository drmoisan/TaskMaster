---
name: project-445-keyboard-action-plan-seams
description: "#445 epic-child plan revision: prepared-in-one-worktree/executed-in-another forbids absolute WS literals; assembly-wide `Failed 0` gates collide with sibling-owned intermittent tests; retention gates are not vacuous"
metadata:
  type: project
---

Three seams surfaced when `atomic-executor` preflight rejected the #445 plan (epic
`quickfiler-suite-determinism-foundation`, wave-0 child).

**1. A prepared plan must not pin an absolute workspace root.** Preparation-mode epic children are
planned in one agent worktree and executed later by `epic-orchestrator` in a worktree that does not
exist yet. Any `C:\...\.claude\worktrees\agent-<id>` literal in a "Resolved Environment (verified;
use these, do not re-derive)" block is wrong on arrival — the planning worktree is gone by then.
Write `WS` as "resolve at execution time via `git rev-parse --show-toplevel`" and keep only the
genuinely worktree-independent literals (repo-local `.dotnet-sdk\dotnet.exe` in the PRIMARY clone,
VS-installed msbuild/vstest paths) under the do-not-re-derive header.

**Why:** the "do not re-derive" instruction forbids the one action that repairs a dead path.
**How to apply:** in any epic-child or preparation-mode plan, audit every absolute path for
worktree-dependence before writing the environment block. See [[worktree-root-breaks-dotclaude-exclusion]]
for the related trap that `WS` itself sits under `.claude\worktrees\`.

**2. An epic child must not gate on an assembly-wide `Failed 0`.** `QuickFiler.Test` contains two
intermittently failing pump tests owned by sibling child #511/#571 and a `Form1.cs` defect owned by
#491, all in the same wave with an empty dependency graph, so the sibling fixes are not guaranteed
merged when this child runs. The coverage runsettings (`scripts/vscode/TaskMaster.cli.runsettings`)
sets `<Workers>0</Workers>` with `<Scope>ClassLevel</Scope>` — full CPU parallelism, the exact load
condition those tests fail under. Scope the gate to the test CLASSES this child owns
(subset-of-baseline PLUS "no failing test in <owned classes>"), and drop the `EXIT_CODE: 0` clause
on the run, since vstest exits non-zero on any failure.

**Why:** the executor's only in-task recourses for an unfixable sibling failure are to weaken a
sibling's test or record a false pass, both prohibited.
**How to apply:** when planning any child of a multi-child epic, list which test classes in the
shared assembly the child owns and phrase every pass/fail gate against that set only. Thread the
same class-scoped condition through the AC check-off task's escape clause, or the discharge is
unreachable — see [[thread-granted-discharges-through-consumers]].

**3. Retention gates need their own justification sentence.** A literal register whose preamble
claims "every count moves" is false as soon as it holds any before==after entry. Say plainly that
the register mixes CHANGE gates (count must move) and RETENTION gates (count must hold on a file
the plan edits), and state why each retention gate can still fail. Related:
[[acceptance-edits-must-be-false-before-true-after]].

**4. Assertion form can collide with a retention gate.** A per-element FluentAssertions
`list[0].Should().Be("b")` would push the pinned `Be("b"` occurrence count from 1 to 2 and break the
plan's own AC19 gate. When a plan pins an occurrence count in a test file it also instructs the
executor to edit, name the assertion form explicitly (`.Should().Equal(new[] { ... })`).
