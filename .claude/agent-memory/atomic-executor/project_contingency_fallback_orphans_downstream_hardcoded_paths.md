---
name: contingency-fallback-orphans-downstream-hardcoded-paths
description: A plan task's bounded fallback branch that writes DIFFERENT artifact filenames leaves every downstream task that hard-codes the primary filename pointing at a file the fallback never produced
metadata:
  type: project
---

When a plan task authorises a bounded fallback (for example, "if the whole-solution
coverage run stalls for twenty minutes, terminate it and fall back to two per-assembly
runs writing `coverage/coverage.quickfiler.cobertura.xml` and
`coverage/coverage.utilitiescs.cobertura.xml`"), check every LATER task that reads the
primary artifact by literal filename.

**Why:** the fallback changes the output filename, but the downstream reader tasks were
authored against the happy path and embed the primary name (`coverage/coverage.cobertura.xml`)
inside their `pwsh` command literal. On the fallback branch those commands throw on a
missing file rather than returning a value, and a downstream acceptance clause such as
"records three explicit numbers, none of them a placeholder" can additionally contradict
the fallback's own instruction to record the repository-wide figure as *unavailable*.
Observed on the #821 plan (2026-09-09) between `[P0-T10]`'s fallback and `[P0-T11]` /
`[P0-T12]`, and mirrored at `[P6-T5]` / `[P6-T9]` / `[P6-T10]`. It is easy to miss because
the forward/backward citation sweep passes: every reference reads backwards, and only the
*branch* the citation assumes is wrong.

**How to apply:** at preflight, for each fallback branch, list the artifact names it
produces and diff them against the names the downstream tasks quote. If they differ,
decide whether the gap is blocking or a note. It is usually a NOTE rather than a blocking
revision when (a) the fallback prose already states which substituted file supplies which
production-file figure, so the substitution is mechanical, and (b) a later task already
declares the repository-wide figure report-only. Say so explicitly in the return rather
than silently omitting it. Prefer the delta "on the fallback branch, run the query once per
per-assembly Cobertura file and record one row per file" over widening the acceptance
clause. See [[feedback_confirmatory_preflight_proportionate_bar]].
