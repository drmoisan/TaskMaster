# Remediation Inputs — quickfiler-session-metrics-twelve-hour-time-format-645

- Timestamp: 2026-09-03T12-00
- Source audits: `policy-audit.2026-09-03T12-00.md` § "Evidence Hygiene — Host Path Leak";
  `code-review.2026-09-03T12-00.md` § "Findings Requiring Attention" item 1.

## Blocking Finding 1 — Host path leak in committed Cobertura evidence

**Files:**
- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml`
  (added in commit `9cc37d01`)
- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml`
  (added in commit `6c1ac1f1`)

**Defect:** Every `<class filename="...">` attribute in both files embeds the literal absolute
path `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a6cd1c774527c71c3\...`, disclosing
the operator's Windows account name and local worktree layout. Confirmed count: 2,007 occurrences
per file (`git grep -c "DanMoisan" <file>`), 4,014 total. This class of defect has recurred across
multiple prior feature reviews in this repository (host path / TRX identifier leaks) and is treated
as a required-remediation finding, not an advisory note.

**Impact:** No impact on production/test code correctness (already verified PASS in the code
review). Impact is confined to repository hygiene: merging as-is permanently embeds the operator's
account name and directory layout into shared git history.

**Required remediation (does not touch the production or test files):**
1. Regenerate or redact both Cobertura files so no `filename=` attribute contains an absolute
   host path. Preferred approach: substitute the absolute worktree-root prefix
   (`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a6cd1c774527c71c3\`) with a
   repository-relative marker, applied case-insensitively across the whole file, in binary/raw mode
   so backslash-vs-forward-slash variants are both caught.
2. After substitution, verify each rewritten file still parses as well-formed XML (a textual
   substitution pass that corrupts an attribute value is a known failure mode for this class of
   fix in this repository).
3. Re-run a case-insensitive fixed-string sweep for the account name and worktree-id token across
   both files and confirm a zero count before re-committing.
4. Because both leaking files were already committed (in `9cc37d01` and `6c1ac1f1`), a sanitizing
   commit alone leaves the original blob reachable in git history (`git log --all` / reflog). If
   this branch is merged via a fast-forward or merge commit, the leaked blobs remain permanently
   reachable from `main`. Recommend **squash-merging** this branch into `main` so only the final,
   sanitized tree is preserved in `main`'s history, consistent with how this class of defect has
   been resolved in prior reviews in this repository.

**Verification the reviewer performed (for the remediator's reference):**
```
git grep -c "DanMoisan" \
  docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml \
  docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml
```
Both returned `2007`.

## Non-Blocking Items (informational only, no remediation required for merge)

- `QfcHomeController.Metrics.cs:46` — a commented-out dead-code line retains the pre-fix `"hh:mm"`
  literal, now inconsistent with the live code beneath it. Explicitly out of scope per spec.md;
  no action required.
- Repo-wide C# coverage (23.8225%) is below the 80%/85% floor cited by CLAUDE.md and
  `.claude/rules/quality-tiers.md` respectively, but is unchanged from baseline (Delta = 0.0000 pp)
  and is a pre-existing, repository-wide condition per the task's documented environment-defect
  carve-out. Not attributable to this branch; no remediation owed by this branch.

## Handoff

Route this remediation to `atomic_planner` / an executor with write access to the branch, scoped
strictly to the two evidence files named above. No production or test source file requires any
change.
