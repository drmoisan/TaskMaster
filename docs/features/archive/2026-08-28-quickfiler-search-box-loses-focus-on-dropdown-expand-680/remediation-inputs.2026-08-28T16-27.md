# Remediation Inputs — Issue #680 review cycle 2026-08-28T16-27

- Feature folder: `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/`
- Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680` @ `79a8500a2ffffc6449ffc0bbabe9acc66558f91f`
- Base: merge-base `b0c7fa18a3beb073e7b051f49e28f48159f0f179` (origin/main tip)
- Source artifacts: `policy-audit.2026-08-28T16-27.md` (§ 2, § 8, § 10), `code-review.2026-08-28T16-27.md` (CR-0), `feature-audit.2026-08-28T16-27.md` (Summary)

## Remediation-required findings (Blocking)

### R1 — `BreadcrumbDropDownHost.cs` exceeds the 500-line file ceiling (514 lines)

- **Rule**: `.claude/rules/general-code-change.md` § File Size Limit — no production file may exceed 500 lines. CLAUDE.md General Code Change Policy § 4.1 states the same limit.
- **Measured state**: `(Get-Content QuickFiler\Viewers\BreadcrumbDropDownHost.cs).Count` = **514** at head. At the merge-base the file is **498** lines; this branch's diff adds +17/-1 (net +16), so the violation is attributable to this branch.
- **How it was missed**: the executor's P6-T6 file-size audit measured 479 lines — correct at execution time, against the pre-#677 base. The subsequent rebase onto main composed #677's additions (~35 lines: `MayTakeFocus`, `FocusPending`, `FocusAnchorIfPermitted`, and documentation) into the same file, and no post-rebase size re-audit was run. The post-rebase verification (build + tests) does not gate file size.
- **Required remediation** (small, mechanical; no behavior change):
  1. Relocate #680-owned members out of `BreadcrumbDropDownHost.cs` into an existing or new partial part of the same type. Candidates, in preference order:
     - Move `ShowPopup(Point, bool)` with its issue-#680 comment block (~14 lines) into `BreadcrumbDropDownHost.Open.cs` (currently 90 lines) — it is an open-path member, so the relocation is thematically correct and uses the partial-part pattern this type already established for exactly this ceiling.
     - If more headroom is desired, also move `PublishPopupMessengerReady` or condense the #680 comment blocks; do NOT relocate #677-owned members (`MayTakeFocus`, `FocusPending`, `FocusAnchorIfPermitted`) — keeping ownership-aligned placement avoids cross-issue churn.
  2. Constraint: `BreadcrumbDropDownHost.Open.cs` must itself stay <= 500 lines (ample headroom) and keep its `#nullable enable` directive intact; the moved member compiles under it, which is acceptable only if no new nullable warnings result (the member has none — parameters and fields are non-nullable).
  3. Re-run the toolchain in order after the move: `dotnet tool run csharpier format .` (verify with `check`), analyzer rebuild, nullable rebuild, then at minimum the scoped host suites (`/TestCaseFilter:"FullyQualifiedName~BreadcrumbDropDownHostTests"`; 35 tests at head) — the relocation must produce zero test deltas.
  4. Re-run the file-size audit over every file touched by the remediation plus `BreadcrumbDropDownHost.cs` and record the counts in a new evidence artifact under `<FEATURE>/evidence/qa-gates/`.
- **Acceptance for closure**: `(Get-Content).Count` <= 500 for `BreadcrumbDropDownHost.cs` and every remediation-touched file; format/analyzer/nullable gates exit 0; scoped host suites green with the same test population; evidence artifact committed.

## Non-blocking follow-ups (do not gate the PR; may ride the remediation commit)

1. **CR-1** — append a dated post-rebase addendum to `delivery-report.2026-08-28T16-40.md` correcting the two stale statements (the composed lambda calls `FocusPending()`, not `_focusPending()`; #677 HAS merged into the base and the shipped code composes with its `MayTakeFocus` machinery). Do not rewrite the execution-time record.
2. **CR-2** — add one composition test in the `BreadcrumbDropDownHostTests.Part3.cs` `PredicateHarness` style: non-capturing open, `AllowFocus = false`, `takeFocus: true` reopen, assert `DropDown.AutoClose` is `true` and the pending-focus delegate was not invoked. This pins the restore-before-guard ordering that only the manual conflict resolution produced.
3. Owner action (unchanged from delivery): execute the 9-item HV runbook (`runbooks/quickfiler-search-focus-hv-680.runbook.md`) in a live Outlook session; record the outcome under `evidence/other/`; only then check AC-1/AC-2 in `spec.md`. A negative outcome on any item (including HV-7/HV-9) routes to the promotion lifecycle per the runbook's fallback contract.

## Handoff

Per `remediation-handoff-atomic-planner`, R1 is the sole blocking input for a remediation plan. The change is a single-file-pair mechanical relocation with a bounded gate list; a minimal single-phase plan is sufficient. No production behavior change is permitted in the remediation.
