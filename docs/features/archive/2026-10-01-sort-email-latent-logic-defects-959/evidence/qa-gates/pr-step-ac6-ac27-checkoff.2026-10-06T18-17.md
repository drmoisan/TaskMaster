# PR Step: AC6 and AC27 Check-off (PD-11 deferrals discharged)

Timestamp: 2026-10-06T18-17
Command: gh pr view 978 --repo drmoisan/TaskMaster --json number,headRefOid,baseRefName,closingIssuesReferences,url
EXIT_CODE: 0
Output Summary: Pull request 978 (base main, head b7cb3c94a223498c34c7cbd7284c01f6675c73a0) was created from the pr-author body; GitHub lists closing references 959 and 966. The body carries the four Data / API / Config Impact behavior changes and the UT5 call-out of Test Strategy marked as applying to phase one of L3 only. AC6 and AC27 are checked off in FEATURE/spec.md.

## Pull request

- URL: https://github.com/drmoisan/TaskMaster/pull/978
- Body source: artifacts/pr_body_959.md (gitignored), SHA-256 9110ece0f7189bfa17fb3efd7e0aebfd43b7a06a3a56ecf6d750b49253b3f59f, receipt artifacts/pr_body_959.receipt.json
- closingIssuesReferences: 959, 966

## AC27 (closure)

- Closing references to this item's issue (959) and the folded review-residuals issue (966): present in the body section "GitHub Auto-close" and confirmed by GitHub closingIssuesReferences.
- Behavior changes from Data / API / Config Impact: the body section "Behavior changes (spec, Data / API / Config Impact)" lists rethrow instead of hang, re-rooted alternate path, single tab-separated header line on first use, and prompt-state reset after a filer exception.
- UT5 call-out of Test Strategy: the body section "UT5 call-out (from the spec's Test Strategy; applies to phase one of L3 only)" quotes the call-out verbatim from FEATURE/evidence/qa-gates/pr-description-inputs.2026-10-06T15-12.md.
- Result: met.

## AC6 (L3 phase one)

- Plan-side observations: recorded as holding in FEATURE/evidence/qa-gates/p6-t24-ac6-deferred.2026-10-06T15-15.md (fail-before and pass-after rows, the one-line fix census, the four DataRow rows, no DoNotParallelize).
- Pull-request clause ("the UT5 call-out for its reflective static write appears ... in the pull request change description, marked as applying to this phase only"): met by the UT5 section of the PR 978 body, whose heading and first sentence both state that it applies to phase one of L3 only.
- Result: met.

## Check-off

- FEATURE/spec.md AC6: `- [ ]` changed to `- [x]`, criterion text unchanged.
- FEATURE/spec.md AC27: `- [ ]` changed to `- [x]`, criterion text unchanged.
- Spec totals after the check-off: 27 of 27 checked.
