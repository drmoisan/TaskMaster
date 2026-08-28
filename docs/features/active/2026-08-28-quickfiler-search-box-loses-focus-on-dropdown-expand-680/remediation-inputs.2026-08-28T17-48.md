# Remediation Inputs — Issue #680 review cycle 2026-08-28T17-48

- Feature folder: `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/`
- Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680` @ `c4e96b72b38fc122a8658ecbeff245814eef09bd`
- Base: merge-base `b0c7fa18a3beb073e7b051f49e28f48159f0f179` (origin/main tip)
- Source artifacts: `policy-audit.2026-08-28T17-48.md` (§ 7, § 8), `code-review.2026-08-28T17-48.md` (RC-1, RC-2), `feature-audit.2026-08-28T17-48.md`
- Prior-cycle closure: R1 (file-size ceiling), CR-1, and CR-2 from cycle 2026-08-28T16-27 are all verified CLOSED. This cycle raises one new Blocking finding introduced by the remediation execution itself.

## Remediation-required findings (Blocking)

### R2 — Host-identity leak in five remediation-cycle TRX evidence files

- **Rule/standard**: committed evidence artifacts must not carry absolute host paths, the account name, or the machine name. This branch's own commit `72b4b7ed` ("fix(680): XML-escape sanitized placeholders in committed TRX evidence") established the exact required treatment, and the prior review cycle's sanitization gate (policy-audit 16-27 § 7) passed on that basis. The remediation plan's D6 restated host-path hygiene for its markdown artifacts but its TRX outputs were committed unsanitized.
- **Measured state** (reviewer-verified, case-insensitive sweep of the full branch diff):
  - `runUser="Megalodon4\DanMoisan"` (machine name + account name) in all five files:
    - `evidence/remediation-baseline/p0-t6/p0-t6.trx`
    - `evidence/remediation-baseline/p0-t7/p0-t7.trx`
    - `evidence/regression-testing/p1-t3/p1-t3.trx`
    - `evidence/regression-testing/p2-t3/p2-t3.trx`
    - `evidence/qa-gates/p4-t4/p4-t4.trx`
  - `p4-t4.trx` additionally carries **1240** occurrences of raw `c:\users\danmoisan\repos\taskmaster-wt\2026-08-28t08-42\...` paths (storage/codebase attributes).
  - Attribution: `git log -S 'Megalodon4' b0c7fa18..HEAD` shows the tokens enter history only at `c4e96b72`. All earlier TRX files (sanitized by `72b4b7ed`) remain clean; no markdown artifact leaks.
- **Required remediation** (mechanical, no code change):
  1. Apply the `72b4b7ed` sanitization treatment to all five TRX files: replace the user-profile worktree prefix with the XML-escaped `&lt;repo-root&gt;` placeholder, and replace the `runUser` value with an escaped `&lt;user&gt;`-style placeholder (machine and account names must not survive in any form, including the lowercase path variants).
  2. Verify with the same checks the prior cycle used: each file parses as well-formed XML; zero case-insensitive matches for the account name, machine name, or `c:\users\` prefix; escaped placeholders only (no raw `<repo-root>` tokens inside XML attribute values).
  3. Re-run the diff-wide sweep (`git diff <merge-base>..HEAD` name list, case-insensitive grep for the three token classes) and record the result in a new evidence artifact under `<FEATURE>/evidence/qa-gates/`.
  4. History note for the PR: the unsanitized content exists in commit `c4e96b72` itself. A follow-up sanitization commit cleans the head state (sufficient for the head-state gate); whether to also rewrite/squash the branch history before merge is a maintainer decision and should be stated in the PR body either way.
- **Acceptance for closure**: zero matches for account name, machine name, or user-profile path prefix (case-insensitive) across every file in the branch diff at head, excluding agent-memory files that quote placeholdered doc text; all five TRX files still well-formed XML; evidence artifact committed.

## Non-blocking follow-ups (may ride the R2 commit; do not gate independently)

1. **RC-2 — restore the AC-3 red-run TRX and de-collide the remediation TRX.** The remediation's P2-T3 overwrote `evidence/regression-testing/p2-t3/p2-t3.trx` (previously the feature plan's fail-before red run: 27 total / 25 passed / 2 predicted failures) with its own 36/36 green run. Restore the sanitized red TRX from `72b4b7ed` to `p2-t3/`, move the remediation green TRX to a non-colliding directory (e.g., `r-p2-t3/`), and update the TRX path reference in `p2-t3-new-test-green.2026-08-28T19-27.md`. Note: the restored red TRX is already sanitized; the relocated green TRX must be sanitized per R2.
2. **RC-3 — timestamp-accuracy note.** Remediation evidence artifacts are self-stamped 2026-08-28T18-16 through T20-12, ahead of both the containing commit (17:40) and the wall clock at review (17:48). Do not rename committed artifacts; add a one-line accuracy note (dated addendum style) to the delivery-report addendum chain or the remediation plan stating that the remediation-cycle `<ts>` stamps are ahead of real time.
3. **AC-6 stale enumeration.** The AC-6 parenthetical in `spec.md` enumerates a twelve-file footprint; the remediation added a thirteenth in-boundary code file (`BreadcrumbDropDownHostTests.Part3.cs`). Add a one-line note in the spec's AC status table (do not edit the AC criterion text).
4. **Owner action (carried, unchanged)**: execute the 9-item HV runbook (`runbooks/quickfiler-search-focus-hv-680.runbook.md`) in a live Outlook session; record the outcome under `evidence/other/`; only then check AC-1/AC-2 in `spec.md`.

## Handoff

Per `remediation-handoff-atomic-planner`, R2 is the sole blocking input. It is a pure evidence-file substitution with verifiable acceptance checks and zero production/test code changes; a minimal single-phase plan (or direct orchestrator-supervised execution with the acceptance checks above) is sufficient. Items 1–3 are small, mechanical, and should ride the same commit. No behavior change of any kind is permitted.
