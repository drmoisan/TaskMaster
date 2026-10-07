---
name: project-950-r1-inline-continuation-and-reindent-width-seams
description: "#950 R1: a default TaskCompletionSource runs an await continuation inline inside SetResult when the completing thread already has the captured SynchronizationContext, so a drainable-context Drain() gate is vacuous unless the source uses RunContinuationsAsynchronously, and only a delete-the-Drain control proves it; wrapping a body in using() pushes the header and re-indented lines past CSharpier's width, so gate on layout-independent tokens and prove placement with START/END spans"
metadata:
  type: project
---

Seams from revision R1 of plan 950 (worktree agent-a7805823735145ca4, 2026-10-01; no shell in the planning session).

**Inline continuation makes Drain() vacuous.** Test installs a test-owned SynchronizationContext, starts an async-void handler synchronously, then calls `release.SetResult(true)` on the same thread. With a default TCS the awaiting continuation runs inline inside SetResult (current context equals the captured one), so `Drain()` runs nothing and the test passes without it (executor measured posts=0). Fix: `new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)`. A control that withholds the signal ("never set release, then Drain") cannot detect this; add a control that DELETES `Drain()` and requires failure.

**using() wrap vs print width.** `using (IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher())` fits at 12 spaces (98) but not at 16 (102); CSharpier splits it to `using (` / declaration / `)`. Gate on the declaration text (substring in both layouts), count `using (` in the method span, and prove placement with spans: decl-to-first-acquisition (0), decl-to-original-read (1), last-assertion-to-finally-body (one more `}` than BASE). Re-indent also pushes a 100-char member-chain line to 104, so record (not fix) the file's line count.

**Renumbering inserted tasks.** replace_all on a task ID also rewrites the change-log prose you just wrote; renumber first, then write the log, or fix the log afterward.

**How to apply:** for any drainable-context design, check who completes the source and on which thread; for any wrap-in-using re-indent, compute header width at the new indent before choosing a gate token. Related: [[project-950-hygiene-blocks-absolute-prefix-and-always-failing-probe-seams]], [[acceptance-edits-must-be-false-before-true-after]].
