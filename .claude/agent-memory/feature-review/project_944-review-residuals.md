---
name: 944-review-residuals
description: '#944 (engine-toggle prime marker registered before start) full-bug review 2026-09-30T16-00 PASS 18/18 AC, 0 blocking, 4 non-blocking, 3 follow-ups; keyed TryRemove safety argument; no-Bash review with caller-supplied MAIN-MERGE-SHA diff; Cobertura root timestamp corroboration; follow-ups owed to the orchestrator'
metadata:
  type: project
---

Full-bug review (parallel cohort bugs-2026-09-28, nested worktree `agent-a308c11880eff133b`, head `1f3614deb`): PASS,
18/18 AC, 0 blocking, 4 non-blocking, 3 follow-ups. Same no-Bash mechanics as [[928-review-residuals]] and
[[929-review-residuals]] (HEAD via `<session>/.git/worktrees/<wt>/HEAD` -> loose ref; raw Cobertura read at the
`<class>` node; 3-`..` advertised path from the session cwd). The session-cwd `artifacts/pr_context.summary.txt`
was #936's (`.md/.yml/.csproj` only) so the hook's language checks were disarmed; clean C# PASS rows written anyway.

**Reusable verification points:**
- Keyed `_primeTasks.TryRemove(engineName, out _)` in `CompletePrime` cannot remove a newer marker: the dictionary has
  one write site (indexer, under `_primeGate`, after a `ContainsKey` probe) and one removal site, so a re-registration
  for the same key is causally after the previous marker's removal. Identity-conditional removal is unnecessary while
  that invariant holds; re-check it if anyone adds a second writer.
- Cobertura root `timestamp="1790781012"` (epoch seconds) decoded to 2026-09-30T15:10:12Z and matched the executor's
  15-10 label and the recorded collection window; use the root epoch as the no-shell clock cross-check (see
  [[evidence-timestamps-are-synthetic-cross-check-commit-dates]]). This executor's labels were UTC-consistent to the
  minute, unlike #929.
- The caller supplied the diff against MAIN-MERGE-SHA (main merged mid-run, plan R3-2); the coverage baseline tree was
  the pre-merge anchor, so the repo-wide delta carried item-929 content. Per-file/per-method figures carried the
  no-regression weight; recorded as non-blocking, not a finding.
- Plan-defined stall probe classified `REPRODUCES` on a *failed* shell-icon test (invalid Win32 icon handle), not a
  hang; the DIRECT route then excluded the four UtilitiesCS.Test shell-icon classes locally. Non-blocking; CI runs them.

**Follow-ups handed to the orchestrator (not filed by the item; recommend promotion):** FU-1 throwing `logError` sink
skips `TryRemove` (marker completes but stays registered; discarded continuation faults unobserved); FU-2 log volume
after a permanently faulted configuration load (every cache-miss poll re-primes and logs); FU-3 `GetPrimeTask`
`<returns>` still says "The prime task" though the value is the registration marker (plan D-3 froze it).
