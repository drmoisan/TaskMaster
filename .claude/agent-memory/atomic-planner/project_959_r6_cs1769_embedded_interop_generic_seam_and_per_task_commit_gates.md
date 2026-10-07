---
name: project-959-r6-cs1769-embedded-interop-generic-seam-and-per-task-commit-gates
description: "#959 revision 1.6 (execution-time amendment): a Func/Action seam over an Outlook interop type cannot cross UtilitiesCS -> UtilitiesCS.Test (CS1769, EmbedInteropTypes True vs False); use a nested non-generic delegate; and how to restate porcelain/diff gates when the executor commits after every task"
metadata:
  type: project
---

Two seams found when #959 execution stopped at P4-T7 (2026-10-03), fixed in place as plan revision 1.6.

**1. CS1769: a generic instantiation over an embedded interop type does not cross the assembly boundary.**
UtilitiesCS/UtilitiesCS.csproj embeds Microsoft.Office.Interop.Outlook (`EmbedInteropTypes` True, line 223 at the time); UtilitiesCS.Test references it with `False` (line 749). A production seam parameter typed `Func<Attachment, string, Task<bool>>` compiled in UtilitiesCS but every test call site failed with CS1769. Direct `Attachment` parameters are fine (interop type equivalence), and test-local `Mock<Attachment>` is fine; only a generic instantiation with an interop type argument in a cross-assembly signature fails.
- Remedy used: a nested non-generic delegate inside the static partial class (`internal delegate Task<bool> TrySaveAttachmentDelegate(Attachment attachment, string filePath)`), tests return `SortEmail.TrySaveAttachmentDelegate` from their recording helper. Method-group conversion from the overloaded `TrySaveAttachmentAsync` still resolves to the two-argument overload.
- Precedent check result: UtilitiesCS has no delegate declaration over an Outlook type and no `Func<`/`Action<` over one anywhere else; nearest precedent is `StoreLockupNotifier` (UtilitiesCS/Threading/StoreLockupResponder.cs), a public non-generic delegate seam in CSharpier's wrapped form.
- Plan mechanics: executed tasks (P4-T1/P4-T2/P4-T4 wrote the Func form) stay checked; the next unexecuted task rewrites the three files from the amended listings before its format/build; the census table gets a new state column (EXTRACT as executed, SEAMED after the rewrite) rather than editing the executed column; the re-run artifact carries `ITERATION: 2` and the filename convention must admit a re-run after a stop record.
- Spec: the spec's technical section and D-decision spelled `Func<...>`; AC8 said "exact delegate types in Technical specifications", so no AC text changed and spec.md was not edited; the drift is reported to the orchestrator and carried into the PR-inputs task.

**Why:** the plan was authored without checking `EmbedInteropTypes`; a seam over an interop type must be checked against both csproj values before choosing a generic delegate type.

**How to apply:** before authoring any `Func<`/`Action<`/`IEnumerable<` etc. over `Attachment`, `MailItem`, `MAPIFolder`, `Store` or any `Microsoft.Office.Interop.Outlook` type in a UtilitiesCS signature a test will call, grep both csproj files for `EmbedInteropTypes`; if the producer embeds and the consumer does not (or vice versa), declare a named non-generic delegate instead and add `CS1769` to the build task's stop vocabulary.

**2. Per-task commit directive versus working-tree gates.**
When the maintainer orders commit-and-push after every task, porcelain gates that expect earlier tasks' edits to be uncommitted become unsatisfiable (e.g. "porcelain shows the five partials modified and the legacy file ` D`" after the delete was committed two tasks earlier). Rules that worked:
- The plan's existing `git diff <op> MERGE-BASE -- paths` (working tree vs anchor, no second ref) is already valid in every commit state and includes the task's own uncommitted edits; keep it for gates over the task's own edits (numstat of an edit made in the same task), because `git diff MERGE-BASE HEAD` cannot see them before the commit.
- Use `git diff --name-status MERGE-BASE HEAD -- dir` for gates over earlier tasks' state; read porcelain only as "this task's own uncommitted edits, taken before this task's commit"; an empty porcelain for an untouched file proves nothing after commits, so pair it with an anchored `--name-only` diff.
- Fix the commit point in one plan-level rule (after evidence written, acceptance checked, box ticked), stage explicit pathspecs, and name the exceptions: a mutation-control task stages evidence only so the mutated production file is never committed; the restore task commits the file back.
- A deletion the executor cannot run (hook-blocked `Remove-Item`) is handed to the coordinator with the fully substituted payload, and the task records the coordinator-run output under an `ITERATION: 2`-style heading, as P4-T5 of #959 did.

**How to apply:** when a run directive says "commit after every task", sweep every remaining task for `porcelain`, `git status`, `git diff` (no ref), `numstat` and classify each as own-edit (working-tree form, pre-commit porcelain), earlier-task state (HEAD form) or within-payload comparison (unchanged); report the disposition list.
