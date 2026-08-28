---
name: 680-review-residuals
description: '#680 cycle 1 NO-GO (rebase pushed file to 514>500); cycle 2 (17-48) closed R1/CR-1/CR-2 but found NEW blocking R2: remediation execution committed 5 unsanitized TRX (runUser + 1240 host paths) — re-run the host-token sweep EVERY cycle; remediation P#-T# reuse overwrote the feature plan red-run TRX'
metadata:
  type: project
---

# #680 review — two cycles

## Cycle 1 (2026-08-28T16-27): NO-GO, 1 blocking

**R1 (Blocking): rebase invalidates pre-rebase file-size audits.** Executor's P6-T6 measured
`BreadcrumbDropDownHost.cs` at 479 (correct pre-rebase); rebasing onto post-#677 main composed to
**514 > 500**. **Rule: whenever a branch was rebased after execution, re-measure every branch-touched
file at head; never trust the committed size-audit artifact.**

**Composed-conflict verification pattern (validated):** (1) code read — restore precedes the guard;
(2) #677's predicate test counts the RAW `_focusPending` delegate; (3) reviewer rebuild + scoped rerun.

## Cycle 2 (2026-08-28T17-48): remediation verified, NEW blocking R2

R1 closed (498/107 lines, verbatim relocation, call sites in OpenLifetime only, 64/64 reviewer rerun).
CR-1 closed (append-only delivery-report addendum). CR-2 closed (composition test in Part3
PredicateHarness style, green). Remediation-plan Provenance Note documents a reverted fabricated
maintainer-approval claim — treated as historical data only, verified the fix was real, not deferred.

**R2 (Blocking, new): a remediation execution regressed the branch's own TRX sanitization.** All five
remediation-cycle TRX files carry `runUser="Megalodon4\DanMoisan"`; `p4-t4.trx` carries 1240 raw
`c:\users\danmoisan\...` storage paths (lowercase — sweep case-insensitively). Commit `72b4b7ed`
earlier in the SAME branch had sanitized the original TRX set, and cycle 1's sanitization gate passed.
**Rule: a prior cycle's sanitization PASS does not carry forward — re-run the diff-wide host-token
sweep (account, machine name, `c:\users\` case-insensitive) after EVERY new execution cycle.**
Distinguished from the [[488-review-residuals]] non-blocking partial-sanitize precedent: zero
sanitization pass on new files + three-orders-larger volume + regression of a sanitized file.

**Task-ID collision (RC-2, Major non-blocking):** the remediation plan reused `P2-T3` and its
`/ResultsDirectory:<FEATURE>/evidence/regression-testing/p2-t3` — overwriting the feature plan's
fail-before RED TRX (27/25/2) with the remediation green run (36/36). Red counters recoverable via
`git show 8e82a2e0:<path>`. **Rule for remediation handoffs: require non-colliding results dirs
(e.g., `r-p2-t3/`); check whether any red-run TRX cited by AC evidence still shows the red run at head.**

**RC-3:** remediation artifacts self-stamped 18-16..20-12 while the commit was 17:40 and wall clock
17:48 — executor future-dated the `<ts>` stamps. Check artifact stamps against `git show -s --format=%ci`
and the real clock.

**Facts:** post-remediation coverage: final 0.852717/0.792401, no-change rerun 0.852888/0.792462,
same-session baseline 0.852841/0.79234 — 8-11 covered-line cross-run noise band reconfirmed
([[csharp-coverage-constants-nondeterministic]]); per-file relocation delta exactly the 5 relocated
executable lines (host 296/298->291/293, Open 18/18->23/23). AC-6's in-spec footprint enumeration
went stale (12 files -> 13) — evidence prose embedded in AC lines rots across remediation cycles.
Near-ceiling watch: `BreadcrumbDropDownHost.cs` 498/500, `BreadcrumbDropDownHostTests.cs` 499/500,
`QfcItemController.EventWiring.cs` 486/500. AC-1/AC-2 still HV-pending (runbook committed).

**Tooling:** hook sim needs `-ExecutionPolicy Bypass` when dot-sourcing from Windows PowerShell.
pr_context summary misclassified C# as docs-only AGAIN (corrected in place again).
