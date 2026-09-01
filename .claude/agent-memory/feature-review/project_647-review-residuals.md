---
name: 647-review-residuals
description: "Issue #647 (FileIO2 write-retry reports success) review outcome: PASS/0 blocking, 18 non-blocking; AC20 graded PASS-with-deviation on in-spec provisions; call-site coverage regression found only by measuring the non-primary changed files"
metadata:
  type: project
---

Review 2026-08-31 (`policy-audit.2026-08-31T19-44.md` et al., head `8e773f35`, base `9b6aff2e` = `origin/main` tip = recomputed merge-base). **PASS, 0 blocking, 21/21 AC, GO.** 18 non-blocking (N-1..N-8 policy, C-1..C-7 code, F-1..F-3 feature).

**Why these adjudications are reusable:**

- **AC20 PASS-with-deviation.** Two literal AC sub-clauses failed: "every changed line ... exercised" (lines 74 and 101 of `FileIO2.cs` read `hits="0"`) and "repository-wide figure ... not lowered" (0.853296 -> 0.852919). Both are pre-authorized by provisions in `spec.md`'s **own Test Strategy section** — it pre-accepts an uncovered production-default line and states "no repository-wide figure is asserted as a blocking gate here. The blocking obligations are change-scoped." Graded PASS, checkbox left checked, deviations recorded in full under F-1 so a maintainer can overturn on the evidence. Distinguishes from a plan-only provision: the authorization is in the AC's own source document. Also: unchecking would have created a remediation loop with **no achievable remedy** (covering line 74 needs filesystem I/O, prohibited by UT4) — proportionality argued explicitly.
- **Provenance via SHA-256, not mtime.** The five footprint files' SHA-256 at head match, byte for byte, the ten post-format hashes in `evidence/qa-gates/p6-t1-format.md`. That single check binds the analyzer build, nullable build, 6899-test run and the Cobertura document to the reviewed tree without re-running anything. Cheapest strong provenance available; look for a `p*-format.md` hash table in every TaskMaster execution.
- **Evidence timestamps drifted ahead of the clock.** Recorded ISO timestamps run +1h15m to +1h40m ahead of file mtimes and commit dates (`p8-t4-commit.md` says 21:10; commit `8e773f35` is 19:32:56 -0400). Monotonic drift, not a timezone offset. Ordering preserved so sequencing arguments hold; absolute values are not citable as wall-clock facts. Cross-check evidence timestamps against `git log --date=iso` and `ls -la` on every review.
- **Toolchain restart taken in place.** First P6-T5 run exited 1 with 14 one-minute timeouts in `QuickFiler.Test` pump-host/dispatcher fixtures under `/EnableCodeCoverage`; byte-identical re-run passed 6899/6899. Accepted as substance-over-form because the footprint hashes prove steps 1-3 would have been no-ops. Same pattern as [[same-commit-differing-outcome-flake-check]]. That 14-test timeout class is pre-existing debt and will recur in future full-suite gates — worth promoting.

**Residuals owed:** (1) orchestrator still owes three MCP promotions recorded as *requests only* in `evidence/qa-gates/p8-t3-promotion-requests.md` (narrow retryable exception set; supported async text writer for the `To Depricate` migration; remove the method-local `Interlocked.Increment`); (2) C-3 logging seam on `QfcHomeController` so the new `if (!metricsWritten)` log becomes assertable; (3) C-7 pump-host timeout promotion; (4) C-1 `AppOlObjects.cs` is at 494/500 — extract the `TimedDiskWriter` construction before the next edit; (5) F-2 `spec.md` Test Strategy describes the accepted-uncovered line as being in the public forwarder, but the production defaults landed in the internal seam overload — correct at close-out.

**Post-647 same-session Cobertura baseline:** line 0.852919 (54835/64291), branch 0.792754 (13063/16478), 9 assemblies. Do not gate cross-session on these per [[csharp-coverage-constants-nondeterministic]].

Artifacts were mirrored into the session cwd worktree per [[review-worktree-differs-from-session-cwd-mirror-artifacts]]; the hook simulated `Ok=True` from **both** roots.
