---
name: project-469-comment-accuracy-plan-seams
description: Issue #469 plan seams — defect-number SWAP voids whole-file token gates; digit-only edits make numstat exact; terminal clean-tree acceptance unreachable; scoped format vs repo-wide check contradiction; base-merge task + Phase 0 renumber; pre-edit vs post-edit table mix-up; evidence dir rule does not constrain recorded path form
metadata:
  type: project
---

Issue #469 turned out to be documentation-accuracy only: three of four defects were already merged,
and the fourth's residual action is open issue #629. The plan is comment/XML-doc/`because:`-string
edits with zero executable-line change.

**A renumbering SWAP makes every whole-file token gate vacuous.** Both `Issue #469 defect 1` and
`Issue #469 defect 2` already existed in BOTH edited files at branch head, so "the file contains
`Issue #469 defect 2`" passes before any work. Every gate had to become a combined single-line token
pairing the defect number with its distinguishing text (`Issue #469 defect 2: exactly one diagnostics
line`), plus the complementary must-become-zero token. This generalises to any A-to-B relabelling
where both labels are already present.

**Why:** a swap conserves the multiset of tokens; only their pairing with surrounding text changes.
**How to apply:** for any swap/rename plan, gate on the PAIRING, and always author the zero-match
companion alongside the one-match assertion.

**A single-character substitution makes exact `--numstat` derivable.** All eight renumbering sites
were one-digit changes on one physical line each, so line length and line count are invariant and the
plan could assert exactly `2 2` and `6 6` per file. Verify the digit-only property by reading each
line before promising an exact numstat; a rewrap would void it.

**Spec line counts were off by one and research line counts were wrong.**
`QfcCollectionControllerDefects468MoveTests.cs` is 497, not the spec's 498;
`QfcHomeController.Metrics.cs` is 215, not the research doc's "232, approximate" and not the 216 the
plan's own first pass asserted in four places; and the research cited `:351` for a site that is
actually `:352`. Re-derive every count and citation even when two upstream documents agree, and count
with a whole-file line count rather than by reading a tail window — a `cat -n` tail read of a file
ending in a final newline is easy to misread as one line longer than it is. See
[[verify-test-provenance-before-planning-deletion]].

**Preflight round 1 (version 0.3) seams — the ones a first pass will miss:**

- **A terminal "clean tree" acceptance is unreachable when the task must tick its own checkbox.**
  P7-T17 asserted empty `git status` over a pathspec containing the plan file it must mark `[x]` and
  the artifact it must write. Committing either re-dirties the other. Author the acceptance as
  "names no path other than the plan file and this task's own artifact" and state that committing
  those two is the orchestrator's step after plan completion.
- **A scoped format pass and a repo-wide `check .` contradict each other.** If the format task
  declines to sweep pre-existing drift (to protect a zero-executable-line-change AC), the check task
  cannot demand repo-wide exit 0. Make the check baseline-relative: reported set must be a SUBSET of
  the Phase 0 enumeration. Then re-check the clean-pass declaration task, which will still say "no
  failure" and now needs "acceptance held, not exit code 0".
- **Branch on the exit code, never on an unobserved output literal.** The first pass keyed the format
  branch on counting lines containing `Was not formatted` at CSharpier 1.2.6. No run was cited. If
  the spelling differs the count is 0, the repo-wide mutating branch is taken silently, and the AC it
  protected disappears.
- **Evidence captured before a mutating formatter is stale.** The changed-line-classification and
  numstat evidence all ran in Phases 2-5; Phase 6's `format` then rewrites the same four files.
  Re-assert the per-file numstat inside the format task itself.
- **`- [x] AC1` is a prefix of `- [x] AC10`..`AC13`.** Assert the spec's em-dash form
  `- [x] AC1 —`. True-only-because-it-runs-first is not an assertion.
- **A `git diff origin/main` footprint enumeration must include what earlier branch commits added.**
  This branch's own `issue.md` and `research/` document appear in every `origin/main`-anchored diff,
  so an exact enumeration omitting them is unsatisfiable. Also scope the pathspec: tracked
  `.claude/agent-memory/` modifications by other agents in the same worktree otherwise show up. See
  [[agent-memory-is-tracked-scope-git-gates]].
- **AC10 named `vstest.console.exe /EnableCodeCoverage`, which no task runs.** The runsettings file
  declares no coverage `DataCollector` and the pipeline is `dotnet-coverage`. Record the AC-to-task
  realisation mapping and state the wording divergence explicitly rather than silently substituting.
- **Two contradictory totals in one sentence.** "must be 20" and "14 added and 14 deleted, 28 diff
  lines" coexisted; a downstream task already depended on 28. Recompute every arithmetic figure from
  its per-file components.

**Preflight round 2 (version 0.4) seams:**

- **A plan with `git diff origin/main` gates needs a Phase 0 task that MERGES `origin/main`.** Every
  numstat/name-only/footprint gate is anchored to `origin/main`, so an upstream advance between
  planning and execution silently re-scopes them. The task is unconditional — `git merge` exits 0 and
  prints `Already up to date.` when there is nothing to do — and it must precede every baseline
  capture, because a merge landing after a baseline invalidates that baseline (most directly the
  `BASELINE_PASSED:` count). Use `merge`, never `rebase`: the force-push guard rejects the rewritten
  history. Exit code alone is not evidence; re-assert rev-equality plus the gated line counts.
- **Inserting one Phase 0 task forces a whole-phase renumber, and the artifact FILENAMES carry the
  task ID too.** The uppercase cross-references were 32 across 27 lines; the lowercase
  `p0-t<N>-<slug>.<timestamp>.md` evidence names and a `/ResultsDirectory:TestResults\p0-t<N>` path
  were a second, disjoint set the reference count did not cover. Remap both, DESCENDING (T14→T15
  first), so a `replace_all` never collides with a number it has not yet processed.
- **A conditional branch stated only in prose is not executable.** P6-T1's prose described a scoped
  `csharpier format` on the non-zero-baseline branch, but its command block held only the repo-wide
  `format .`. An executor following the block verbatim takes the branch the prose exists to prevent.
  Every branch a task's prose describes needs its own line in the task's command block.
- **`acceptance-criteria-tracking` forbids modifying criterion text, so "check it off, appending the
  evidence path" is a policy conflict.** Reword to record the path in the task's progress output; the
  criterion line changes only in its checkbox marker.
- **A base merge silently shifts line citations in files the plan does not edit.** The merge added one
  `<Compile Include>` to `QuickFiler.Test.csproj`, moving two cited entries from `:135`/`:155` to
  `:136`/`:156`. After ANY base reconciliation, re-derive every citation into a merged file, not only
  into the files the plan edits.
- **A class-level summary that enumerates all four defect numbers is not a defect-N citation.**
  `QfcCollectionControllerDefects468MoveTests.cs:17-18` reads "issue #469 defects 1," / "2, 3 and 4",
  so it is invariant under a 1-for-2 swap. Grouping it with the defect-3 sites was a misclassification
  even though the operative "needs no edit" conclusion was right.

**Preflight round 3 (version 0.5) seams:**

- **A plan that quotes both PRE-edit and POST-edit text in two tables will get the wrong one cited.**
  P0-T15's baseline bullet said "the eight pre-edit tokens named in the R3 table". The R3 table's
  third column is headed `Becomes` and holds POST-edit text, every token of which has count 0 at
  branch head — so a Phase 0 acceptance demanding count 1 for them halts the executor before any
  work. Name the SOURCE TABLE and its COLUMN HEADER in the bullet, and quote the eight literals
  inline rather than referring to a table by name.
- **A re-verification task must produce every count its acceptance asserts.** P0-T15 listed eight
  token counts in its acceptance but its PowerShell block held no command producing them. Walk each
  acceptance clause of every command-bearing task back to a line in that task's own block.
- **Where-artifacts-are-written does not constrain what-paths-they-record.** The evidence-location
  rule and the `enforce-evidence-locations.ps1` hook both check the DIRECTORY. Any acceptance
  demanding verbatim tool output (MSBuild diagnostics, CSharpier unformatted-file enumerations)
  publishes absolute paths carrying the account name, machine name and drive letter once a staging
  task commits `evidence/`. Add an explicit "recorded paths are repository-relative" clause naming
  the tasks, and carve out counts/exit codes/summary lines so the assertions keep their observables.
  See [[../_shared_no_absolute_host_paths]].

**Local facts confirmed this pass:** the CSharpier manifest is `dotnet-tools.json` at the repository
ROOT (there is no `.config/` directory); `packages/` and `QuickFiler.Test/bin/Debug/` are absent from
a fresh agent worktree so restore-then-build must precede any test-count baseline;
`Invoke-MSTestWithCoverage.ps1` calls `Assert-CoberturaLineCoverageThreshold`, which throws below 80%
BEFORE the Koverage post-processing writes the XML, so a baseline task must record the thrown
percentage and continue rather than treating it as this change's failure. Related:
[[project_494_threshold_reconciliation_plan_seams]], [[reference_invoke_mstest_with_coverage_script]].
