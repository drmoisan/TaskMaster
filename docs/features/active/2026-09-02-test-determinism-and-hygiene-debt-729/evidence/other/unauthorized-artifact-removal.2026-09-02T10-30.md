# Removal of an unauthorized evidence artifact (P2-T6)

Timestamp: 2026-09-02T23-10

Command: `Remove-Item -Force 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/p2t1-stopwatch-mention-measurement.2026-09-02T10-30.md'`

EXIT_CODE: 0

Output Summary:

- Before the deletion, the guard command
  `git ls-files -- 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/p2t1-stopwatch-mention-measurement.2026-09-02T10-30.md'`
  returned empty output, confirming the path was untracked. That is the expected state: no
  Phase 2 commit has run, and the artifact was written by an earlier execution attempt after the
  Phase 1 commit. The stop-and-report branch for a tracked path therefore did not apply.
- `Remove-Item -Force` completed without error and `Test-Path` on the same path subsequently
  returned `False`.
- Rationale for the removal: no task in this plan authorizes that artifact, the plan's
  `Complete file-write inventory` section does not list it, and the comment-stripped `Stopwatch`
  reading of P2-T1 it recorded is superseded because Block B no longer contains that literal in
  any form. P2-T1's re-run against the current Block B observed zero matches for both
  `Stopwatch` and `System.Diagnostics` with no comment-stripping step required.
- Because the path was untracked and is now deleted, it produces no entry in
  `git diff --name-status $base HEAD` and no entry in `git status --porcelain`, so P7-T5's
  seventeen-`D` inventory acceptance is unaffected. All seventeen deletion entries are produced by
  Phases 3 through 5.
