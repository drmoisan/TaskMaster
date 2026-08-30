---
name: project-637-r6-superseded-spec-claim-seams
description: Issue #637 preflight round 6 — a "the spec already carries X" correction leaves the old figure attributed to the corrected criterion at plan sites the delta did not name; plus a two-throw coverage wrapper, Cobertura node-free lines, and repo-wide format drift outside the staged pathspec.
metadata:
  type: project
---

Preflight round-6 seams from the issue #637 atomic plan (breadcrumb `SelectRow` emits a rooted path).
Complements [[project-637-selectrow-rooted-path-plan-seams]].

**A plan can narrate spec edits it never performs.** Phase 8 of this plan only flips `- [ ]` to
`- [x]`; no task edits `spec.md` prose. Yet six tasks and one tree-observation block asserted that
this plan "corrected", "reworded" or "extended" acceptance criteria — corrections a *prior* `spec.md`
revision had already applied. An executor would have recorded a non-discrepancy under a heading
asserting a discrepancy.
**Why:** the plan and the spec were revised in separate rounds; the plan kept the narration of a fix
whose object had moved into the spec's own history.
**How to apply:** before writing "this plan corrects AC*n* in `spec.md`", grep the plan for a task
that actually writes that file. If the only write is a checkbox flip, restate every such claim as a
verification ("AC*n* already carries X, re-verified against the merged tree") and split the
reconciliation artifact into an already-corrected list and a still-stale list.

**The delta's own site list will be short.** The round-6 delta named `spec.md:401` and `:414-416` as
the surviving 424-line sites; the tree also carries 424 at `:582` and `:710`. More importantly, after
rewriting P8-T25 and tree-observation 1 to say "AC25's parenthetical already reads 485", **three**
other plan sites still said AC25 states 424 — tree observation 1's own preceding sentence, P1-T7's
acceptance ("the artifact records that `spec.md`'s implementation table **and AC25** state 424"), and
P8-T25's own preceding clause. P1-T7's was an acceptance condition demanding the executor record a
falsehood.
**How to apply:** after applying any "the spec already carries the corrected figure" edit, grep the
plan for the OLD numeral and for the criterion's name, and fix every site that still attributes the
old figure to the corrected criterion. See [[acceptance-edits-must-be-false-before-true-after]].

**`Invoke-MSTestWithCoverage.ps1` throws on two independent conditions.** Non-zero inner vstest exit
at `Invoke-MSTestWithCoverage.ps1:235-237`, and a repository line rate below 80 at
`Invoke-MSTestWithCoverage.Helpers.ps1:487-489` (called at `:341`). A clause reading "when the
baseline failure set is empty, EXIT_CODE 0 and 0 failed are required" is therefore unsatisfiable in
exactly the state the plan's own `BASELINE BELOW FLOOR` branch exists for. Judge failures and exit
code separately, keyed on the emitted literal `is below the required 80% threshold.`, and record
`ExpectedExitCode: 1` on the floor-throw branch.
Consequence worth stating in the plan: the threshold assertion at `:341` runs BEFORE the
post-processed write-back at `:343`, so on a floor throw the on-disk Cobertura file is the raw
dotnet-coverage output. Downstream tasks that re-apply `ConvertTo-KoverageCoberturaXml` in memory are
unaffected; a task that reads the file expecting post-processed content is not.
See [[reference-invoke-mstest-with-coverage-script]].

**A mandated literal may legitimately contain `%` or angle brackets.** `is below the required 80%
threshold.` and `<line>` both trip the wrap-tolerant rule's placeholder character list, so the plan
gate skips them. They are real emitted literals, not command shapes. Keep them and say so in the
handoff report rather than substituting a paraphrase that no run emits.

**Cobertura emits a `<line>` node only for a sequence point.** "Every line inside the helper's range
has non-zero hits" is unsatisfiable, because XML documentation lines, the signature and blank lines
carry no node and no `hits` attribute. Correct shape: assert over the lines that DO carry a node,
require the artifact to enumerate and classify the node-free lines, and add "at least one line in the
range carries a node" so the clause cannot pass vacuously on a range with no coverage rows at all.
See [[async-state-machine-coverage-aggregation]] for the sibling aggregation trap.

**A repo-wide write-mode formatter plus a narrow staging pathspec hides drift until the final gate.**
P7-T1 ran `csharpier format .` over the whole repository but observed porcelain only over
`QuickFiler`, `QuickFiler.Test` and the feature folder; P7-T2 staged only the two QuickFiler trees;
P8-T30's scope gate spanned nine trees. Pre-existing drift under `UtilitiesCS`, `TaskMaster`,
`ToDoModel`, `Tags` or `TaskVisualization` would have been repaired, never committed, and would have
made the final gate unsatisfiable with no earlier detector.
**How to apply:** the condition is measurable at the read-only baseline — add an
`OUT_OF_SCOPE_FORMAT_DRIFT:` halt branch to the Phase 0 `csharpier check` task, and give the
write-mode format task its own porcelain span over exactly the trees its staging pathspec omits.
See [[repo-wide-csharpier-format-breaks-zero-diff-acs]] and
[[observation-scope-must-match-blast-radius]].

**An insertion into a non-SDK csproj shifts every later `<Compile Include>` citation.** P2-T6 inserts
after `QuickFiler.Test/QuickFiler.Test.csproj:64`, so P2-T7's "already registered at `:114`" is stale
by the time P2-T7 runs (`:115`). Write both values with the event that separates them. Sweep the same
file's other cited lines (`:116`, `:133`) for acceptance conditions that read them post-insertion —
here none did.
