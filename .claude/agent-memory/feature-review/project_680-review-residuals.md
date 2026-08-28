---
name: 680-review-residuals
description: '#680 review NO-GO/1 blocking: rebase onto post-#677 main pushed BreadcrumbDropDownHost.cs to 514 lines (pre-rebase P6-T6 said 479) — always re-measure file sizes after a rebase; composed FocusPending verification pattern; residuals CR-1 stale delivery report, CR-2 predicate-false restore test'
metadata:
  type: project
---

# #680 review (2026-08-28T16-27): NO-GO, 1 blocking finding

**R1 (Blocking): rebase invalidates pre-rebase file-size audits.** The executor's P6-T6 measured
`BreadcrumbDropDownHost.cs` at 479 (correct pre-rebase). Rebasing onto main — which had merged #677
into the SAME file — composed to **514 > 500**. Merge-base was 498; this branch's +16 net lines own
the violation. Post-rebase re-verification (build + tests) does not gate file size.
**Rule for future reviews: whenever a branch was rebased after execution, re-run `(Get-Content).Count`
on every branch-touched file at head; never trust the committed size-audit artifact.**
Remediation: relocate #680-owned `ShowPopup` into `BreadcrumbDropDownHost.Open.cs` (90 lines, the
type's established ceiling-relief partial). Near-ceiling watch: `QfcItemController.EventWiring.cs`
486/500, `BreadcrumbDropDownHostTests.cs` 499/500.

**Composed-conflict verification pattern (validated).** The orchestrator's manual resolution
(`Schedule(() => { DropDown.AutoClose = true; FocusPending(); })`) was verified by three legs:
(1) code read — restore precedes the guard, so it is unconditional w.r.t. `MayTakeFocus`;
(2) #677's `AlreadyOpenRefocus_PredicateFalse_DoesNotFocusPending` counts the RAW `_focusPending`
delegate, so it fails if a resolution bypasses the wrapper — run it at head;
(3) reviewer rebuild + scoped rerun (55/55). Residual gap CR-2: no test does non-capturing open →
`AllowFocus=false` → `takeFocus:true` reopen → assert AutoClose restored.

**Residuals owed at remediation/PR:**
- CR-1: delivery-report stale post-rebase (says `_focusPending()` and "#677 has not merged"); owed a
  dated addendum, not a rewrite.
- CR-2 test above (Part3 `PredicateHarness` style).
- HV runbook (9 items, HV-7/HV-9 = DR-8 composition risks) still pending; AC-1/AC-2 unchecked by
  design with checkpoint `human_interaction` response `exception` — validated non-blocking treatment.

**Facts verified this cycle:** post-680-execution coverage baseline 0.85269/0.792133 → final
0.85279/0.792235 (reviewer re-parsed raw Cobertura in gitignored `coverage/`; per-file counters
reproduced p6-t5 exactly). `QfcItemController.EventHandlers.cs` 89/108 = 82.41% — FAIL at 85 rules
floor, non-blocking per the [[modified-file-subfloor-nonblocking-disposition-230]] bar (>=80,
zero regression: 19 uncovered before and after, +16 covered). TRX escaping fix (72b4b7ed) verified
complete: 5/5 valid XML, escaped placeholders only, zero raw tokens; full-diff leak sweep clean
(only placeholdered `C:\Users\<user>` doc text in agent-memory).

**Tooling notes:** `enforce-powershell-batch-budget.ps1` can carry STALE entries from another
session's scratchpad and block Write of any .ps1; the error message's sanctioned reset is deleting
`.claude/state/powershell-batch-budget.default.json` (git-restore it afterwards). Corrected
pr_context summary in place again (recurring misclassifier, [[pr-context-summary-misclassifies-cs]]);
after correction the hook enumerated CSharp and the sim passed.
