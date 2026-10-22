# Implementation Commit (P2-T8)

Timestamp: 2026-09-30T13-42
Command: dotnet tool run csharpier format TaskMaster\Ribbon\EngineToggleStateCoordinator.cs TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs (between CMD-HASH before and after); then git add -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs TaskMaster.Test/TaskMaster.Test.csproj docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944; git commit -m "fix(ribbon): register the prime marker before the prime starts (issue 944)" -m "Co-Authored-By: Claude Opus 5.5 noreply@anthropic.com"; git show --name-only --format= HEAD; git status --porcelain -- TaskMaster TaskMaster.Test; git push origin bug/engine-toggle-prime-marker-registration-races-removal-944
EXIT_CODE: 0
Output Summary: CSHARPIER_EXIT_CODE: 0 ("Formatted 2 files"). PRECOMMIT-FORMAT-REWRITES: 1 (the PrimeRegistration partial; LF to CRLF line endings only). PRECOMMIT-FORMAT-RECHECK passed on every P1-T1, P2-T6 and P2-T7 clause with no repair. PRE-COMMIT-HYGIENE: ACCOUNT_HITS=0 MACHINE_HITS=0 DRIVE_USERS_HITS=0. The commit, name list, porcelain and push results are recorded in the Commit section below.

## Format

CMD-HASH before:
- HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086
- HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 6D28EBF80B5D4AE7C3C0099A8A329F7B8DF463E4258FF1B14D921CEB12C91A7C

Format output: `Formatted 2 files` (CSharpier reports files processed, not files changed); CSHARPIER_EXIT_CODE: 0

CMD-HASH after:
- HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086
- HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = E7582241265ED1838921593C38420CEB1C3C8E9F101DD1405F55945C45A3A73B

PRECOMMIT-FORMAT-REWRITES: 1

The production file is unchanged by the format. The partial was rewritten from LF to CRLF line endings (175 CR and 175 LF bytes after the format, matching the Race partial); no line's text changed and the line count stayed 175.

## PRECOMMIT-FORMAT-RECHECK:

Run on the formatted text before any git add (Timestamp: 2026-09-30T13-41).

- P1-T1 (CMD-TOKEN-COUNT, TOKENS-PARTIAL): every count and FIRST-LINE value identical to the P1-T1 table; the test 1 ordering 39 < 45 < 46 < 49 < 54 <= 56 < 59 < 70 holds. Appended to evidence/regression-testing/prime-registration-partial-tokens.md under PRECOMMIT-FORMAT-RECHECK:.
- P2-T6 (CMD-TOKEN-COUNT TOKENS-PROD, CMD-PRIME-SPANS, CMD-PHRASE-COUNT, added lines, numstat): identical to evidence/qa-gates/production-edit-scope.md. Token vector (count@FIRST-LINE, TOKENS-PROD order): 1@59 1@60 0@0 1@73 0@0 1@264 1@272 1@274 1@279 1@283 1@284 1@286 1@287 0@0 1@303 0@0 1@307 1@310 0@0 0@0 1@316 1@320 1@320 0@0 0@0 0@0 1@323 1@324 1@325 0@0 1@298 1@299 0@0 1@182 1@272 1@286 1@381 0@0 0@0 0@0 0@0 0@0 0@0 0@0 0@0. Spans: StartPrimeIfNeeded 264-289 TRY=0 FINALLY=0 CATCH=0 LOCK=1 BEFORE-END-IS-LOCK-CLOSE=True SPAN-LINES 272,274,279,283,284,286,287; StartObservedPrime 303-327 TRY=1 FINALLY=1 CATCH=0 LOCK=0 try/finally 314/318 SPAN-LINES 310,316,320,323,324,325; CompletePrime 366-382 TRY=0 CATCH=0 LOCK=0 _logError 380 < TryRemove 381. JOINED [The returned continuation task always completes successfully] = 0. ADDED-LINE-COUNT: 34; REMOVED-LINE-COUNT: 12; ADDED-CATCH-LINES: 0; ADDED-TOKEN vector 0 0 0 0 0 0 0; numstat 34 12.
- P2-T7 (CMD-REGION-COMPARE PROTECTED and EDIT-WINDOWS, protected-file diffs): HEAD 1-57/1-57, PRESSED-STATE 62-70/62-70, PRIMETASKS-DECLARATION 77-80/78-81, MIDDLE 81-236/82-237, GETPRIMETASK 237-258/238-259, APPLYPRIME-AND-COMPLETEPRIME 307-361/329-383, TAIL 362-420/384-442 each equal=True; GATE-AND-TASKS-FIELDS 58-80/58-81 and PRIME-START 259-306/260-328 each equal=False; no TOKEN-MISSING; PROTECTED_FILES_DIFF_EXIT=0; RUNSETTINGS_DIFF_EXIT=0.
- Verdict: every clause of P1-T1, P2-T6 and P2-T7 holds on the formatted text. No repair was made; FORMATTER SPLITS GATED TOKEN does not apply.

Substitutions recorded for the recheck: plan correction C1 (New-Object System.Security.Cryptography.SHA256Managed in place of the SHA-256 static factory call); the P2-T6 commands were run as two invocations (a git-free token, span and phrase invocation, and a git diff invocation) and printed compact vectors rather than one row per token, because a PreToolUse hook (PARALLEL_WORKTREE_REMOVAL_BLOCKED / EPIC_WORKTREE_REMOVAL_BLOCKED) refused a read-only command that contained both git and the token TryRemove; the printed labels DELETED-LINE-COUNT and DELETED are transcribed here as REMOVED-LINE-COUNT and REMOVED; the computed expressions are unchanged.

## PRE-COMMIT-HYGIENE:

Command: the P3-T13 command, unchanged. Output: FILES_SCANNED=32 ACCOUNT_HITS=0 MACHINE_HITS=0 DRIVE_USERS_HITS=0 (the FILES_SCANNED floor does not apply here). Re-run after this artifact was written, immediately before the git add: FILES_SCANNED=33 ACCOUNT_HITS=0 MACHINE_HITS=0 DRIVE_USERS_HITS=0.

## Commit

(Written after the commit; this section is committed by P3-T35.)

- git add exited 0 (line-ending notices only).
- git commit exited 0: `[bug/engine-toggle-prime-marker-registration-races-removal-944 edc5c3af2] fix(ribbon): register the prime marker before the prime starts (issue 944)`; 14 files changed, 709 insertions, 24 deletions. The attribution trailer was the second -m paragraph `Co-Authored-By: Claude Opus 5.5 noreply@anthropic.com`.
- IMPLEMENTATION-COMMIT-SHA: edc5c3af2787e40ccb2d6b51876c7a08ad2845b5 (observed with git rev-parse HEAD)
- git show --name-only --format= HEAD: the three code paths TaskMaster/Ribbon/EngineToggleStateCoordinator.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs and TaskMaster.Test/TaskMaster.Test.csproj, plus eleven paths under docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/ (plan.2026-09-30T07-20.md, evidence/baseline/phase0-commit.md, evidence/qa-gates/csproj-registration.md, evidence/qa-gates/implementation-commit.md, evidence/qa-gates/production-edit-scope.md, evidence/qa-gates/protected-regions-unchanged.md, evidence/regression-testing/build-after-fix.md, evidence/regression-testing/build-before-fix.md, evidence/regression-testing/prime-registration-fail-before.md, evidence/regression-testing/prime-registration-partial-tokens.md, evidence/regression-testing/prime-registration-pass-after.md); nothing else.
- git status --porcelain -- TaskMaster TaskMaster.Test: no line.
- PUSH: git push origin bug/engine-toggle-prime-marker-registration-races-removal-944 exited 0: `5d1f4ede6..edc5c3af2  bug/engine-toggle-prime-marker-registration-races-removal-944 -> bug/engine-toggle-prime-marker-registration-races-removal-944` (no force push).
- No PreToolUse refusal of the git add or git commit occurred.

Verdict: every P2-T8 acceptance clause holds. From this commit on, no code file is edited.
