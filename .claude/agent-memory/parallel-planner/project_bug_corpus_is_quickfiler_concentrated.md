---
name: bug-corpus-is-quickfiler-concentrated
description: TaskMaster's open-bug corpus is ~71% QuickFiler, so any large parallel run over "all bugs" is near-serial (measured 51 cohorts for 72 items, mean width 1.41) — the binding constraint is corpus composition, not the config defects
metadata:
  type: project
---

Measured 2026-08-21 on `main @ 7a9ba612` over the 72 open non-`.claude` bug issues. Re-measure
before relying on the numbers; the conclusion about *composition* is the durable part.

**The binding constraint on parallelizing TaskMaster bugs is what the bugs are about, not the
blast-radius config.** 51 of 72 items (71%) target module `QuickFiler`. Every pair of them conflicts
on `module_overlap`, so the cohort barrier serializes them:

- conflict density **53.4%** (1366 of 2556 pairs)
- `compute-cohorts.sh` yields **51 cohorts for 72 items**, max width **6**, mean width **1.41**
- module frequency: `QuickFiler` 51, `scripts/vscode` 11, `QuickFiler.Test` 6, `csproj/packages` 6,
  `UtilitiesCS` 4, `TaskMaster` 4, `UtilitiesCS.Test` 2, `.github/workflows` 2

**Confirmed 2026-08-29 by the `bugs-638-644-647` run, at a scale small enough to audit by hand.**
Three deliberately unrelated bugs — an `EmailFilerConfig` UI-thread crash, a `QfcCollectionController`
registration mismatch, and a `FileIO2` retry defect in `UtilitiesCS` — still produced a COMPLETE
conflict graph (all 3 pairs), hence 3 singleton cohorts and a fully serial run at
`max_concurrency: 2`. Every pair carried a `module_overlap` on `QuickFiler` *in addition to* its
path overlaps, so the serialization was genuine contention and not an artifact of the extractor's
over-reporting. This is the useful diagnostic: when a pair conflicts, check whether it still
conflicts after setting the spurious paths aside. Here it did. Thematic unrelatedness at the issue
level does not imply blast-radius disjointness when one module dominates the corpus.

`max_concurrency` is **inert** at this shape: 51 sequential cohort barriers, each requiring a full
CI cycle plus PR merge before the next starts. Raising the cap changes nothing.

**This contention is genuine, not spurious.** Unlike the config defects in
[[parallel-surface-partial-port]], it cannot be fixed away. Even with `QuickFiler` removed from the
module map, 51 items editing `QuickFiler/**` still contend on `QuickFiler/QuickFiler.csproj` and
`QuickFiler.Test/QuickFiler.Test.csproj` compile entries whenever they add a test file, plus the
shared controller and viewer sources. Deleting the module to widen cohorts would be exactly the
radius manipulation the skill prohibits.

**Maximum genuine parallel width is about 6-8 items** — roughly one per module family. A sensible
parallel run over this corpus is single-digit, chosen one-per-family, not "all bugs".

**Do not derive radii from issue bodies as a proxy for plan radii.** Attempted first and it
under-reports severely: 40 of 72 issue bodies yielded only the feature-folder glob and zero modules,
giving a false 9.4% density, because issue prose names target files without backticks. See
[[blast-radius-extractor-mechanics]]. Recovering the real assignment required matching project
directory names in the prose and resolving bare class names (`QfcItemController`, `ItemViewer`,
`EmailMoveMonitor`) to their files with `git ls-files`.

**How to apply:** when asked to fan out a large parallel run over TaskMaster bugs, measure the
module histogram FIRST and report cohort depth before launching any preparation child. Preparing 72
items costs 72 full orchestrator runs; discovering afterward that the run is 51 cohorts deep wastes
all of it. Route the QuickFiler cluster to a serial queue or `/epic-plan`
(see [[parallel-surface-cannot-express-ordering]]), and reserve `/parallel-plan` for a
one-per-family subset.
