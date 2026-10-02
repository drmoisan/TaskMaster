---
name: 947-review-residuals
description: '#947 (throwing logError sink at both EngineToggleStateCoordinator sites) minor-audit review 2026-10-01T19-30 PASS 7/7 AC, 0 blocking, 6 non-blocking; documented broad-catch disposition via the SafeLog precedent; a THIRD unguarded sink (_notifyUnavailable) left as follow-up; no-Bash mechanics repeated'
metadata:
  type: project
---

Minor-audit review (parallel cohort bugs-2026-09-28, nested worktree `agent-ad4c3492ba7a853c5`, head `ec312202e`,
BASE-SHA `2e6ce2cab` = the origin/main merge commit): PASS, 7/7 AC, 0 blocking, 6 non-blocking, 5 gaps, 5 follow-ups.
No-Bash mechanics exactly as [[940-review-residuals]] / [[944-review-residuals]]: worktree `logs/HEAD` reflog for head
and commit epochs, Cobertura root `timestamp=` epochs as the clock cross-check (labels matched to the minute at UTC-4),
gitignored `coverage/final-947.cobertura.xml` read at the `<class>` node to confirm every new catch-arm line `hits="1"`.
Plain repo-relative paths advertised plus absolute worktree paths in prose; no mirror written (caller forbade touching the
session checkout).

**Reusable verification points:**
- Disposition pattern for an intentional empty `catch (Exception)` around a logging sink: PASS under CLAUDE.md C#4 as a
  boundary catch when (a) the enclosed statement is the type's last reporting channel, (b) the method remarks and an
  in-block comment document the discard, (c) `RibbonCommandBoundary.SafeLog` (lines 96-113, `catch (System.Exception)` +
  `// Intentionally discarded`) is cited as precedent, and (d) `RCS1075` is at `suggestion`. Record it as an accepted
  exception to the general "re-raise or propagate" rule (X-row in section 8), not as a violation.
- When a change guards one injected sink, grep the method for EVERY other injected delegate call outside the guarded
  `try`. Here `_notifyUnavailable(...)` (refusal path, line 177) and `_enginesAccessor()` (line 175) remain unguarded, so
  the remark "This method therefore never throws" is overstated — same defect class, third site. Recorded non-blocking
  because AC6 named the `logError` sink only; owed as follow-up F-1.
- A test whose headline assertion holds pre-fix (here `RanToCompletion`, satisfied by the #944 `finally`) is fine when
  the SAME test carries a discriminating assertion that fails pre-fix and the plan/test summary disclose it; check the
  fail-before MESSAGE names the discriminating assertion.
- Canonical `artifacts/csharp/coverage.xml` absent in the worktree again (runner writes `coverage/`; CLAUDE.md forbids
  committing the raw document). Wrote one line "C# canonical-path coverage artifact ... rated FAIL as evidence ... not
  used" alongside the clean "C# coverage verdict: PASS" line, so a stale session-cwd copy below 85% cannot block the hook.

**Follow-ups owed to the orchestrator:** F-1 guard `_notifyUnavailable` (+ shared private SafeLog helper to dedupe the two
guards); F-2 `GetPrimeTask` `<returns>` still says "The prime task" (#944 FU-3, outside edit window E2); F-3 production
file 476/500 and main fixture 470/500 — split before the next change; F-4 #944 FU-2 log volume still open; F-5 canonical
C# coverage path convention vs the `coverage/` route.
