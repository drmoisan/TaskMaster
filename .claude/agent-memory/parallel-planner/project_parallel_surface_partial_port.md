---
name: parallel-surface-partial-port
description: The parallel surface is now usable at 13-item scale in TaskMaster (measured 33.3% density, 5 cohorts, mean width 2.6); the residual fail-OPENs are all in the path extractor, not the config truth table
metadata:
  type: project
---

**2026-09-02 — re-measured on a real 13-item run (`bugs-2026-09-02`), which supersedes the
83.3%-density headline this file used to carry.** Derived from the 13 pushed plans on
`main @ 5ebaaf10`:

- conflict graph density **33.3%** (26 of 78 pairs)
- `compute-cohorts.sh` yields **5 cohorts for 13 items**, mean width **2.6**
- dominant edge source is `module_overlap` at the **assembly** level, not path overlap

The three spurious-contention defects this file previously listed are gone: the `claude-runtime`
umbrella module is removed, `mandate_reads` covers eleven entries, and placeholder tokens are
rejected (issue #502). **Large parallel runs are no longer contraindicated.** The old guidance to
fix the truth table before planning is obsolete.

**The remaining ceiling is structural, not a defect.** This repository's bugs concentrate in six
assemblies (`UtilitiesCS`, `UtilitiesCS.Test`, `QuickFiler`, `QuickFiler.Test`, `TaskMaster`,
`TaskMaster.Test`), and adding or deleting any `.cs` file in a non-SDK-style project requires
editing that project's `.csproj` compile-entry list. So two items that share no source file still
contend, correctly. Expect roughly a third of pairs to conflict on any bug corpus drawn from this
codebase, and do not read that as an over-reporting defect.

**Genuine contention that must NOT be "fixed away":**
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `.github/workflows/*.yml`, and the `*.Test.csproj`
compile-entry files. Their serialization is correct.

**Every residual fail-OPEN now lives in the path extractor, not in `config/blast-radius.json`.**
See [[blast-radius-extractor-mechanics]] for all of them: bare prose, the whitespace split, polarity
blindness, and the closed extension allow-list that drops `resx`, `config`, `props`, and `targets`.
Two more are worth naming here because they bit this run specifically:

1. **`scripts/vscode/**` is a `mandate_reads` exclusion, which is right for a citation and wrong for
   a rewrite.** Items #565 and #733 both rewrite `Invoke-MSTestWithCoverage.ps1`; neither radius
   named it, the pair reported no conflict, and they would have been co-scheduled onto the same
   file. Recovered only by hand-append. This is the single highest-consequence correction class.
2. **A path named as a pathspec argument inside a backticked shell command span is harvested as a
   write claim.** A plan whose scope-boundary task runs
   `git diff --name-only $base HEAD -- config/blast-radius.json` to assert that file is NOT written
   thereby claims it as written. The plan author cannot remove it: the acceptance-condition
   authoring rules require the literal to be quoted. Item #729 carries two such claims after
   thirteen revision rounds.

Issue **#576** remains open (`shared_surfaces` omits TaskMaster's root build files, so
`TaskMaster.sln`, `Directory.Build.props/.targets`, `coverage.config`, `.editorconfig` drop out of
derived radii and such pairs report `conflict=False`). Hand-append those exact paths.

**How to apply:** plan the run, then audit each derived radius against the plan's declared Write Set
and hand-append what the extractor dropped. Do not audit by trusting V1/V2 silence — every fail-open
above is invisible to them, because the validator extracts from the same text through the same rules
and is therefore self-consistent with the radius. Before demanding another correction round for an
over-report, measure its cost: recompute the cohort table with the suspect edges removed. In this run
the answer was zero cohorts, which retired a fourteenth round. See
[[parallel-surface-cannot-express-ordering]] for why an ordering-blocked cluster needs `/epic-plan`,
and [[parallel-artifact-authoring-gotchas]] for the schema traps.
