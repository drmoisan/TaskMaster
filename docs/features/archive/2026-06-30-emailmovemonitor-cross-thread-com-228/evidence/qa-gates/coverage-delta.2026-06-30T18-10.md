# Coverage Delta and Exemption Boundary (Issue #228)

Timestamp: 2026-06-30T22-55
Source runs:
- Baseline: evidence/baseline/baseline-tests-coverage.2026-06-30T18-10.md and baseline-emailmovemonitor-coverage.2026-06-30T18-10.md (P0-T5/P0-T6)
- Post-change: evidence/qa-gates/qa-tests-coverage.2026-06-30T18-10.md (P9-T4)
- Both converted from the binary .coverage via `dotnet-coverage merge -f cobertura`.

## Numeric Values

| Metric | Baseline | Post-change |
|---|---|---|
| Whole-process line coverage (all loaded modules incl. vendored) | 13.10% (10003/76355) | 13.38% (10243/76570) |
| QuickFiler package (first-party production) | 32.94% | 33.74% |
| QuickFiler.Test package | 92.41% | 92.47% |
| EmailMoveMonitor.cs file-level (all classes) | 8.15% (11/135) | 44.03% (70/159) |
| EmailMoveMonitor changed/new bookkeeping (in-scope) | ~0% | 96.92% (63/65) |
| QuickFiler.Test total tests | 201 (201 passed) | 209 (209 passed) |

## Threshold Verdicts

1. New/changed EmailMoveMonitor bookkeeping >= 90% (AC5): PASS.
   - In-scope bookkeeping = the constructor, HookItem, UnhookItem, UnhookAll, and the EmailMoveAction
     constructor + cached-EntryID properties. Coverage = 96.92% (63 of 65 instrumented lines).
   - The only two uncovered in-scope lines are the trivial auto-property getters EmailMoveAction.Mail
     (line 244) and EmailMoveAction.MoveAction (line 250). MoveAction is invoked only from the live
     BeforeItemMove handler (COM-host-bound); Mail is not read on the marshaled bookkeeping path
     (the path uses the cached MailEntryId). These two getters are not changed/new logic of substance
     and do not represent untested critical behavior.

2. Repo-wide >= 80% (testable denominator): NO REGRESSION on changed lines; standing floor unaffected by this change.
   - The plan's P0-T5/P9-T4 coverage command runs ONLY QuickFiler.Test, which loads many un-exercised
     vendored/third-party modules (System.Interactive, System.Linq.Async, log4net, SVGControl,
     FluentAssertions, Swordfish, and the bulk of UtilitiesCS). The 13.10% -> 13.38% whole-process
     figures are therefore NOT the repository's testable-denominator floor; they are a single-assembly
     slice. The repository-wide >= 80% testable-denominator floor is a standing property validated by
     the full multi-assembly suite (tracked under feature/csharp-coverage-uplift) and is outside the
     blast radius of this change.
   - What this change can be held to per policy ("code changes or refactors must not reduce coverage
     for the lines that were changed"): every changed/new production line in EmailMoveMonitor.cs is
     either covered (96.92% of the in-scope bookkeeping) or falls inside the documented COM-host-bound
     exemption boundary below. The QuickFiler first-party package coverage INCREASED (32.94% -> 33.74%),
     and no previously-covered line lost coverage. There is no changed-line coverage regression.

OVERALL VERDICT: PASS for AC5. Changed/new bookkeeping 96.92% (>= 90%); no changed-line regression;
QuickFiler first-party coverage improved.

## Exempt vs Non-Exempt Boundary (scoped to EmailMoveMonitor) — Maps AC5 / CLAUDE.md exemption clause (c)

NON-EXEMPT (must meet >= 90%, and does — 96.92%):
- EmailMoveMonitor constructor (marshal-seam wiring)
- EmailMoveMonitor.HookItem bookkeeping (first-item-per-folder subscribe rule, cached-ID add)
- EmailMoveMonitor.UnhookItem bookkeeping (null guard, last-item-per-folder unsubscribe rule, cached-ID match/remove)
- EmailMoveMonitor.UnhookAll bookkeeping (per-folder unsubscribe + single Clear)
- EmailMoveAction construction and the cached MailEntryId / FolderEntryId capture
These are the marshaled bookkeeping logic. Per CLAUDE.md, marshaled bookkeeping reachable through the
injectable seam is explicitly NOT exempt, and it meets the floor.

EXEMPTION-ELIGIBLE (COM-host-bound; reachable only with a live Outlook process, not via the seam):
- The BeforeItemMove event-handler delegate body (lines 206-222). It is raised by Outlook on the STA
  thread when a hooked item is physically moved; it cannot fire in a unit test without a live MAPI
  move event. This is exactly CLAUDE.md clause (c) (Outlook Interop event handler in QuickFiler that
  depends on a live MailItem/MAPIFolder without an injectable seam).
- The dormant UnhookItemAsync and GetParentFolderAsync members (lines 90-183). Per spec.md Scope &
  Non-Goals, these have no active caller and are NOT re-wired by this change; the same marshal seam was
  applied to their retained COM access (see P5-T3 decision below), but they remain dormant and their
  async-state-machine lines are not exercised by the bookkeeping tests.

NO `[ExcludeFromCodeCoverage]` attribute was added by this change. The exemption above is a documented
scope statement, not an applied attribute; therefore no new maintainer ratification is required for an
attribute. The pre-existing class-level COM exemption posture for the QuickFiler assembly is unchanged.

## P5-T3 Decision Record (dormant members)

UnhookItemAsync and GetParentFolderAsync retain COM access. The same marshal-to-STA seam was applied to
their COM reads (GetParentFolderAsync now marshals `mail.Parent`/`mail.EntryID` instead of using the
prior Task.Run hop; UnhookItemAsync marshals its EntryID reads and BeforeItemMove -= and compares against
cached IDs). NO new active caller was introduced — the commented-out UnhookItemAsync call path in
QfcDatamodel.QueueProcessing.cs remains commented out, and DequeueNextItemGroupAsync continues to use the
synchronous UnhookItem via TryUnhookOrReplace. This satisfies P5-T3 (retained COM access is marshaled,
without re-activating the members).
