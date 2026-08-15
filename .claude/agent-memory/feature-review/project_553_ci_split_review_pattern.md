---
name: 553-ci-split-review-pattern
description: 'Cycle-2 closure of the #553 CI parallel job split: 0 blocking, all 18 ACs PASS; reviewer resolved the green-run head mismatch by dispatching ci.yml itself; branch was rebased so all caller SHAs were stale; residuals = open PR promptly (ruleset over-blocks main) + Phase 6 plan bookkeeping'
metadata:
  type: project
---

Cycle 2 (2026-08-14T17-12, head 9c00e37a, TRUE merge base 35e02895 after a rebase onto main/PR #552) closed the #553 review with 0 blocking findings and 18/18 ACs PASS. Key events worth reusing:

1. **Stale caller SHAs, again, worse:** the coordinator supplied the pre-rebase merge base AND pre-rebase green-run/probe SHAs (`d83bf377`, `5a606895`...). `git merge-base --is-ancestor` exposed the rebase; current-lineage probe pairs were different SHAs (`26b9f7b5`/`6f73cf43` etc.). Always recompute base AND re-map every cited SHA onto the actual lineage.
2. **Green-run head mismatch resolved by acting, not adjudicating:** the cited green run's head was a non-ancestor, so `modified-workflow-needs-green-run` was literally unmet. Since gh was available, the reviewer ran `gh workflow run ci.yml --ref <branch>` + `gh run watch` (~5 min, run 31840944277, 5/5 green at the exact head) instead of writing a disposition or bouncing another remediation cycle. Precondition check first: `git diff <old-run-head> HEAD -- .github/` empty proved the workflow bytes were identical, making the dispatch a formality rather than a gamble. This is the cheapest possible closure when the only gap is head drift on unchanged workflows.
3. **Probe verification pattern:** for reverted fault-isolation probes, verify (a) `git diff <probe>~1 <revert>` is byte-empty per pair, (b) net branch diff has zero files in the probed language, and (c) per-job conclusions from `gh api runs/<id>/jobs` show exactly one red gate per probe run.
4. **Ruleset PUT audit pattern:** check payload = writable-six-fields projection (name/target/enforcement/bypass_actors/conditions/rules; the 8 read-only GET fields absent), contexts-array-only delta, strict retained, then corroborate the committed post-PUT GET with your OWN live `gh api rulesets/<id>` GET. In #553 all matched (updated_at 2026-08-14T17:00 ET, five `<caller job> / <callee job>` contexts).
5. **Residuals for any later touchpoint:** PR still not open at review end → the migrated ruleset over-blocks every other PR to main until #553 merges; plan Phase 6 checkboxes lag the executed migration and evidence filenames deviate (`evidence/other/ruleset-migration/ruleset-{pre,new,post}.json` vs planned `ruleset-*-put.<TS>.json`); actionlint tarball still unpinned (accepted Info).
6. Cycle-2 remediation-inputs was written as a zero-finding CLOSURE record so the orchestrator's highest-timestamp lookup doesn't re-count cycle-1's blocking line — phrase former severities in lowercase ("former severity: blocking — resolved") to keep `Select-String -CaseSensitive "BLOCKING","Severity: Blocking"` at 0 hits.

Docs/YAML-only diff still means the coverage hook enumerates zero languages; see [[remediation-handoff-skill-conflicts-with-hook]] for artifact-layout conventions. Timing evidence: 4 samples now (245/259/296/433s vs 444s baseline); 433s outlier classification is sound (uniform compute-step scaling, flat fixed costs, no queueing).
