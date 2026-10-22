# Preflight Clearance - Issue #973 (scope fold, plan revision 1.6)

- Timestamp: 2026-10-03T10-30
- Timestamp source: `git var GIT_COMMITTER_IDENT` epoch 1791037842 (-0400), offset from commit bda40c06b local time 2026-10-03T10-29-02 (epoch 1791037742); pwsh `Get-Date` is refused by the worktree-isolation guard in this session.
- Plan: `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/plan.2026-10-02T22-16.md`
- Plan revision: 1.6 (commit bda40c06b)
- Plan blob SHA cleared: 6d856911285a4f68867224adad77e44328ae902c
- Command: git rev-parse bda40c06b:docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/plan.2026-10-02T22-16.md
- EXIT_CODE: 0
- Plan validator: `mcp__drm-copilot__validate_orchestration_artifacts` (artifact_type plan) returned ok with no warnings on revision 1.6.
- Supersedes: `preflight-clearance.2026-10-03T00-41.md`, which cleared revision 1.2 before the 2026-10-03 maintainer scope fold.

PREFLIGHT: ALL CLEAR

CONVERGENCE: NO FURTHER ROUNDS EXPECTED

## Scope covered

Revision 1.6 covers the original item (15 stale redirect pairs, ADAL block deletion, System.Linq.AsyncEnumerable install, regression test) plus the 2026-10-03 maintainer scope fold:

- removal of six unused `using Microsoft.Graph.*` directives in five UtilitiesCS files;
- rewording of the CLAUDE.md C#1 item 3 Nullable bullet;
- split of `CategoryClassifierGroup.cs` into `CategoryClassifierGroup.ConditionalEngine.cs`.

These correspond to spec AC19 to AC23 and Parts F, G and H.

## Round history

| Round | Plan revision reviewed | Commit | Signal | Defects |
|---|---|---|---|---|
| 1 | 1.0 | 095614a9b | PREFLIGHT: REVISIONS REQUIRED | 13 |
| 2 | 1.1 | bb0f2be3d | PREFLIGHT: REVISIONS REQUIRED | 3 |
| 3 | 1.2 | 7d7895c67 | PREFLIGHT: ALL CLEAR | 0 |
| 4 | 1.3 (scope fold) | bbdcb73e5 | PREFLIGHT: REVISIONS REQUIRED | 7 |
| 5 | 1.4 | 82f3a24ea | PREFLIGHT: REVISIONS REQUIRED | 3 |
| 6 | 1.5 | 534778a9d | PREFLIGHT: REVISIONS REQUIRED | 1 |
| 7 | 1.6 | bda40c06b | PREFLIGHT: ALL CLEAR | 0 |

Round count: 7 in total, 4 of them on the scope fold.

## Orchestrator rulings recorded during the scope-fold rounds

- Spec Planner Amendment 5 accepted. The two blank lines next to the moved region are removed with it because CSharpier strips them. No line carrying code or comment text is exempted, and every other clause of AC16 and AC23 is unchanged.
- The round-5 planner clarification on the directive-restore branch was ruled correct in round 6 and governs P4-T5 in revision 1.6.

## Non-blocking observation from round 7

The phrase "moved up into the directive's former fact 10 position" is inexact for the CategoryClassifierGroup.cs line-12 directive. The old_string is identified by its quoted text, so no gate is affected.

## Execution precondition

Launch the atomic-executor without worktree isolation in the item worktree. The isolation guard refuses pwsh, and plan constraint C5 stops at P0-T1 with `LAUNCH-TOPOLOGY: ISOLATED` otherwise.
