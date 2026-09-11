---
name: 730-review-residuals
description: "#730 ci-build-infra-debt review: closed PASS/0 blocking at cycle 2; the cycle-1 blocker was a sanitization task scoped to an explicit four-file list that missed a host path in an earlier-phase research doc"
metadata:
  type: project
---

#730 (comment-only workflow edits + new root `Directory.Build.props` setting
`RxUseUnsupportedPackagesConfig`). Cycle 1: 8/8 AC PASS, **1 blocking finding**.
Cycle 2 (re-audit after remediation): **PASS, 0 blocking**, 8/8 AC still PASS.

**Remediation-cycle verification that paid off:** prove the *shipped* files were untouched by
comparing **blob SHAs** at the pre-remediation commit vs HEAD (`git rev-parse <rev>:<path>`), not by
re-reading content. That proved `spec.md` and all four sanitized `.log` files were byte-identical, so
the AC basis was undisturbed, in one cheap command per file.

**New finding worth expecting again (CR-8):** sanitizing a file that was already committed fixes the
tree but leaves the pre-sanitization **blob reachable in the earlier branch commits** (here 4 of 6
commits still carried the account name in `research.md`). A file *added* in the remediation commit has
no such residue. Disposition: non-blocking, remedy is **squash-merge**, same as
[[cobertura-substitution-leaves-blobs-in-history]]. Scan with a per-commit `git ls-tree` + token count.

**The blocking finding is the reusable lesson.** `[P2-T10]` sanitized absolute host paths out of an
explicit list of four `.log` files and verified 0 residual matches — correctly, I re-swept and confirmed
0. But `research/research.2026-09-02T09-15.md:8` still carried the full worktree path with the operator
account name, because it was written in an earlier preparation commit and was never on the list.

**Why:** a sanitization step expressed as a *path list* is incomplete by construction against files
added by a different phase. The executor's own residual check passed because it swept only its own
list. Always re-sweep the **whole feature folder** yourself, not just the files the sanitization
artifact names.

**How to apply:** grep every committed artifact for the run-time-derived account token, `C:\Users`,
`C:/Users`, and `worktrees/agent-`. Expect two benign match classes that are NOT findings:
`$env:USERPROFILE` appearing literally inside `Split-Path -Leaf $env:USERPROFILE` (deliberate run-time
derivation), and `C:\Program Files\...` tool paths in MSBuild logs (no account/host identifier).
See [[_shared_no_absolute_host_paths]].

Other verifications worth repeating (all passed): MSBuild's normal-verbosity file logger prints each
warning **twice** (inline + end-of-build summary), so a naive whole-log token count doubles it — count
strictly before the single `Build succeeded.` line and cross-check MSBuild's own once-emitted
`N Warning(s)` line; three methods agreeing is the proof. Guard against a vacuous `/t:Rebuild` by
counting `csc.exe` invocations per log (36 here). Parse the TRX `ResultSummary/Counters` directly rather
than trusting the prose count, and check the TRX run window against the commit timestamp for
contemporaneity ([[evidence-timestamps-are-synthetic-cross-check-commit-dates]]).

Also see [[yaml-comment-only-diff-proof-via-parse-tree]] and
[[pr-context-artifacts-are-tracked-not-gitignored]], both first established on this review.
