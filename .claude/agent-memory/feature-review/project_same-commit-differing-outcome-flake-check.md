---
name: same-commit-differing-outcome-flake-check
description: technique for confirming a failing test is environment-flaky (not code-caused) when two runs at the identical commit disagree
metadata:
  type: project
---

When a full-suite run shows a failure and the caller claims it's "pre-existing," don't just trust
a prose claim — look for two evidence runs recorded at the *identical* commit SHA with *different*
pass/fail outcomes for that one test. If found, that is direct proof the failure is a function of
local environment/COM-server state, not of any code diff, because there is zero code difference
between the two runs.

Example (#261 F1 remediation cycle 1): the entry-cycle audit ran the full suite at commit `88366ad4`
and reported 0 failures; the remediation cycle's own Phase-0 baseline re-ran the full suite at the
same commit `88366ad4` and reported 1 failure (`LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`,
a live-Outlook COM/STA integration test). Same SHA, different outcome, zero code delta -> environment-
dependent, not a regression. Also cross-check the diff's changed-file list to confirm the failing
test's file (and its dependency path) isn't touched at all.

**How to apply:** whenever a policy/feature audit needs to disposition a "pre-existing failure"
claim, don't stop at "the baseline evidence doc says it failed before too" — check whether that
baseline run was actually at the pre-feature merge-base or merely at an earlier point within the
same feature's commits, and look for a same-SHA outcome mismatch across the review's own evidence
files as corroboration before writing PASS/not-Blocking.
