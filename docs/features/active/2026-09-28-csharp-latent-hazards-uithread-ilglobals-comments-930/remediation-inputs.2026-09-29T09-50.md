# Remediation Inputs: Cycle 1 (Issue 930)

- Timestamp: 2026-09-29T09-50
- Cycle: 1
- Work Mode: minor-audit
- Branch: bug/csharp-latent-hazards-uithread-ilglobals-comments-930
- Source audits: policy-audit.2026-09-29T00-45.md, code-review.2026-09-29T00-45.md, feature-audit.2026-09-29T00-45.md (this folder)
- AC source: issue.md, section `## Acceptance Criteria`

## Trigger

The reduced audit evaluated AC1 to AC6 as PASS and AC7 as PARTIAL, and unchecked AC7 in issue.md. Blocking findings: 0. Non-blocking findings: 1 (NB-1). Informational findings: 9. AC7 is unmet, so the item cannot complete; this cycle closes NB-1 and restores AC7.

## Finding NB-1 (the only finding in scope for this cycle)

AC7 requires that no committed file contains an absolute host path. Two committed evidence files transcribe the coverage runner's `Using vstest.console:` line with its absolute Visual Studio install path (a drive-rooted path under Program Files):

- evidence/baseline/baseline-04-mstest-coverage.md, line 11 (line beginning `- Runner output: Using vstest.console:`)
- evidence/qa-gates/final-06-mstest-coverage.md, line 12 (same line shape)

The three audit artifacts of 2026-09-29T00-45 quote the same absolute path when describing the finding (policy-audit lines 182 and 188, code-review line 18, feature-audit lines 39 and 53, located by a Grep for a drive-letter-colon-separator pattern over the feature folder on 2026-09-29T09-50). Those quotations are also committed content and fall under the same AC7 clause.

Root cause of the escape: the plan's CMD-SANITIZE gate searches for the account name, the host name, the worktree root and a drive-rooted `Users` directory. A drive-rooted path outside `Users` (for example under Program Files) matches none of these patterns, so the gate reported zero hits while the path was present.

## Invariant to restore (state the invariant, not the symptom)

No file under this feature folder contains any drive-rooted absolute path, meaning any occurrence of a drive letter, a colon and a path separator followed by a path segment. This must hold for every file in the folder, including the plan, issue.md, all evidence and all audit artifacts, and it must be demonstrated by a gate whose pattern is drive-rooted in general, not by a list of known directories. The gate must carry a positive control that proves the pattern can match (for example the raw coverage log under the gitignored coverage directory, which carries drive-rooted paths).

The replacement text must use a placeholder rather than a real path, consistent with the placeholders the plan already uses (REPO-ROOT, USER-PROFILE, USER, HOST); a placeholder such as VS-INSTALL-ROOT for the Visual Studio installation directory is acceptable. The substitution changes only the path text; no figure, exit code or other field in any evidence file changes.

## Out of scope for this cycle (Informational, recorded only)

- The now-unused `using System.Collections.Generic;` directive in ILGlobals.cs (Informational; no analyzer diagnostic; not a finding to act on here).
- The analyzer HintPath version skew between csproj files and packages.config (pre-existing on origin/main; follow-up for the caller).
- The Phase 0 `Timestamp:` correction disclosure (Informational; affects no figure or AC).
- No source file, test file or project file may change in this cycle. The cycle is documentation-only.

## Exit condition

After execution, a fresh reaudit (code-review, feature-audit, policy-audit at the cycle-exit timestamp) reports zero Blocking and zero blocking-PARTIAL findings, and AC7 is checked off in issue.md on verified evidence.
