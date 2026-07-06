# AC6 Deferral (Issue #240)

Timestamp: 2026-07-06T08-00

AC6 ("All required PR CI checks are green against the PR head SHA") is deferred to post-PR-creation. Verification of AC6 requires a PR to exist with a head SHA and a completed CI run against that SHA; neither exists during local plan execution. AC6 is out of scope for this executor run and will be verified once a PR is opened and CI completes against the PR head SHA.

The AC6 checkbox in `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/issue.md` remains unchecked (`- [ ] AC6: ...`) pending that CI evidence.
