# Remediation Inputs — Cycle 1 (Issue #743)

Timestamp: 2026-09-13T15-45
Cycle: 1
Author: orchestrator (item 743, parallel run bugs-2026-09-11)
Branch: bug/quickfiler-itemviewer-ui-marshalling-seam-743 (head 1d00eed1 at cycle entry; base main at 39ce2892b)
Feature folder: docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743

## Source audit artifacts (initial review, 2026-09-13T15-30)

- `policy-audit.2026-09-13T15-30.md` — section 7 (host-path row, FAIL Blocking), section 8 items 1 and 7, section 10 verdict.
- `code-review.2026-09-13T15-30.md` — Findings Table rows 1 (Blocking, host path) and 2 (Blocking, AC1 verdict overstated); non-blocking rows N-1 through N-8; observations.
- `feature-audit.2026-09-13T15-30.md` — `### AC1 — detailed evaluation` (PARTIAL), `## Remediation-Required Findings` (R-1, R-2, R-3), `## Acceptance Criteria Check-off`.

Blocking count at cycle entry: 2 (R-1, R-2). R-3 was resolved by the orchestrator before this cycle opened: `git diff origin/main...HEAD -G "DanMoisan|Users" --name-only -- .claude/agent-memory` printed nothing, so no branch-added line under `.claude/agent-memory` carries the account name or a user-profile path. R-3 needs no task.

## Cycle scope decision

This cycle is DOCUMENTATION-ONLY. No file under `QuickFiler`, `QuickFiler.Test` or any other source project is edited, so no C# toolchain pass is required by the plan contract (no language with code changes is touched). The reviewer's non-blocking code items (N-1 comment reword in the ViewerSetup partial; the `VerifyGet` assertion suggestion in the seam test file; the misnamed test outside the Write Set) are deferred to follow-ups and enumerated in the final report; each would require a full csharpier/analyzer/nullable/vstest pass under the shared build lock and none changes behaviour.

## Fix list

### F-1 (R-1, Blocking) — Remove the user-profile absolute path from a branch-added evidence file

- File: `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/evidence/other/orchestrator-citation-verification.2026-09-12T13-50.md`, line 6.
- Current content of line 6 is a backticked absolute path to an agent worktree under the operator's user profile on the primary checkout, ending in `.claude/worktrees/agent-a190dd2fffe21a25d`.
- Expected content: the same line with the user-profile prefix replaced so it reads, inside the existing backticks, `<repo-root>/.claude/worktrees/agent-a190dd2fffe21a25d` (the literal token `<repo-root>` followed by the repo-relative remainder). Change nothing else in the file.
- Verification (falsifiable, single-line tokens): `Select-String -Path <feature-folder>/**/*.md -SimpleMatch -Pattern "Users"` over every Markdown file in the feature folder (recursively, including `remediation/` and `audit/`) prints zero matches; and `Select-String -SimpleMatch -Pattern "agent-a190dd2fffe21a25d"` over the edited file prints exactly 1 match. Note for the planner: the pre-edit count of `Users` in the feature folder is exactly 1 (this file, line 6); the planner must re-derive this and must ensure that no cycle document (this inputs file, the plan, the reaudit artifacts) reintroduces the token — this file deliberately spells the prefix out in prose rather than reproducing it.
- History note (not a task): the original blob remains reachable in branch history (commit fca5396e8). The reviewer's remedy of a squash-merge is NOT available: squash merges are disallowed repository-wide and the coordinator merges with the merge-commit method. The orchestrator reports this residual to the coordinator; the plan must not attempt any history rewrite, force-push, or branch recreation.

### F-2 (R-2, Blocking for acceptance) — Correct the AC1 verdict wording and withdraw the AC1 check-off

- File: `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md`, section (iii).
- Current: states `H-LEAK REJECTED by direct observation; H-COST is the surviving mechanism` (decision-rule row 1).
- Expected: section (iii) is amended (append a dated amendment block under section (iii) rather than deleting the original text, so the audit trail shows what was claimed and what replaced it) to state: (a) both instrumented runs recorded `timeout=0`, so no test was abandoned and, by the spec section 4.2 definition of H-LEAK as a leak that follows a timed-out `async` test whose `finally` MSTest stops observing, no leak could occur under either hypothesis; the serial `contended=0` reading was therefore predetermined and does not discriminate between H-LEAK and H-COST; (b) H-COST (elapsed fixture cost) is the only available ORIGINATING mechanism within the spec's two-hypothesis frame, because H-LEAK is a cascade conditional on an initial expiry; the sufficiency evidence is the measured parallel-regime elongation (largest pump-test duration 6,460.4 ms on an idle machine, 58x its serial figure) which, multiplied by the recorded upper load multiplier of 26x, exceeds the 60,000 ms bound; (c) spec unknown U2 (whether a first expiry cascades through a leaked permit) remains OPEN and UNTESTED by this item; (d) per spec AC1's final sentence and the risk-table instruction "Escalate rather than infer", the recorded no-expiry result is a negative result and AC1 is NOT claimed as PASS by this artifact; acceptance of the negative result for AC1 requires the maintainer's explicit ratification, which is requested (see F-4) and not yet given; (e) the phrase `REJECTED by direct observation` is withdrawn. Also append the reviewer's structural point (code-review N-3): under H-LEAK the serial-regime signature is the balance test blocking on `WaitAsync` and expiring under its own `[Timeout]` with no `GATECOUNTERS` line printed, so decision-rule rows 2 and 3 cannot be observed as printed counter values and row 1 is the only row observable as a printed triple.
- Same N-3 addendum, dated, appended to `evidence/baseline/ac1-serial-measurement.2026-09-12T17-00.md` (after its decision-rule table). Do NOT edit `evidence/baseline/ac1-observable-declaration.2026-09-12T16-30.md`: it is the in-advance declaration and its pre-instrumentation content must remain as written; the addendum lives in the verdict and measurement artifacts only.
- File: `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/spec.md`, line 413. Change `- [x] **AC1` to `- [ ] **AC1` (checkbox character only; no other character of the criterion changes). Verification: `Select-String -SimpleMatch -Pattern "- [ ] **AC1"` on spec.md prints 1 and `"- [x] **AC1"` prints 0; the four other criteria remain `- [x]` (counts for `- [x] **AC2`, `- [x] **AC3`, `- [x] **AC4`, `- [x] **AC5` each print 1).
- File: `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/evidence/other/acceptance-status.2026-09-12T19-30.md`. Update the AC1 row's verdict from PASS to PARTIAL, with the recorded figures (serial `contended=0`, `timeout=0` in both regimes) and the pending-ratification reason; leave the other five rows unchanged. Verification: the table still has exactly six rows; the AC1 row contains `PARTIAL`.
- Do NOT change any instrumentation code, any decision-rule table row text in the declaration artifact, or any figure.

### F-3 (N-6 wording, bundled, non-blocking) — Timestamp convention note

- Add one sentence to the plan file's status area (directly below the `- **Status:** Executed` bullet in `plan.2026-09-12T13-23.md`) stating that executor-authored evidence artifacts record their `Timestamp:` in a 12-hour-offset convention relative to the orchestrator checkpoint receipts (for example the P0-T4 artifact `02-11` corresponds to the run-B receipt `13-12`), so cross-checks between evidence and checkpoint should add 11 hours; existing artifacts are not rewritten. Verification: `Select-String -SimpleMatch -Pattern "12-hour-offset"` on the plan file prints 1 (pre-edit count 0).

### F-4 (manual gate surfacing) — Request the maintainer's AC1 ratification on the GitHub issue

- Post exactly one comment on issue #743 with `gh issue comment 743 --repo drmoisan/TaskMaster --body-file <path>` where the body file is written first under the repository-root `coverage` directory (gitignored) so no untracked artifact enters the tree. The comment text must: (a) state that the instrumented AC1 measurement produced no expiry in either regime (`timeout=0`, serial `GATECOUNTERS acquisitions=11 releases=10 contended=0`, parallel `19/18/14`), that H-COST is the only available originating mechanism on this evidence, that U2 (cascade via a leaked permit) remains untested, and that per spec AC1's no-expiry clause this is a recorded negative result; (b) request the maintainer's explicit decision: ratify the negative result as satisfying AC1 (in which case a follow-up cycle transcribes the ratification into `issue.md` and re-checks AC1), or direct an alternative; (c) point at the branch, the verdict artifact path and the feature-audit path; (d) contain no absolute host path and no account name other than the GitHub handle in the `--repo` argument.
- Write the mirror `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/evidence/issue-updates/issue-743.<ts>.md` with `Timestamp:`, the exact text posted, `PostedAs: comment` and the comment URL, or a `POSTING BLOCKED` header with the `gh` error text if posting fails. Verification: the mirror exists and contains either `PostedAs: comment` with a URL containing `issuecomment-` or the `POSTING BLOCKED` header.
- The plan must NOT write any ratification into `issue.md`; only the maintainer's own words, once given, may be transcribed there (that is cycle 2's work, if it occurs).

### F-5 — Commit

- Stage only the feature folder (`git add docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743`), commit with the message `issue 743 remediation cycle 1: host-path hygiene, AC1 verdict correction, ratification request`, and verify `git status --porcelain --untracked-files=all -- docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743` prints nothing. Do not stage `.claude/agent-memory` paths.

## Do-not-do list

- No edits to any file under `QuickFiler`, `QuickFiler.Test`, `UtilitiesCS`, `VBFunctions` or any other source or project directory; no `.csproj` edit (the Meziantou HintPath skew is a follow-up issue, not this item).
- No history rewrite, no force-push, no branch recreation, no squash; report the R-1 history residual instead.
- No edit to `evidence/baseline/ac1-observable-declaration.2026-09-12T16-30.md` (the in-advance declaration must remain as declared).
- No fabricated ratification: nothing is written to `issue.md` in this cycle.
- No re-check of AC1; no change to the AC2-AC5 checkboxes.
- No new `.trx` or `.cobertura.xml`; no raw output committed.
- No absolute host path (any path beginning with a drive letter followed by the `Users` segment) and no account name in any file written or edited by this cycle, including the remediation plan and the reaudit artifacts.
- No `cd` in any command; git is addressed as `git -C <absolute worktree path>`; plan commands assume the worktree root and are run via `Set-Location` inside one `pwsh -NoProfile -Command` invocation.
- No policy weakening, no scope creep beyond F-1 through F-5, no `SKIPPED` outcomes on planned command tasks.

## Verification commands (for the planner to convert into ACCEPT conditions)

- Host-path sweep over the feature folder: `Select-String -Path (Get-ChildItem -Recurse -Filter *.md -Path docs\features\active\2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743 | Select-Object -ExpandProperty FullName) -SimpleMatch -Pattern "Users" | Measure-Object | Select-Object -ExpandProperty Count` prints `0` after F-1 (pre-edit `1`).
- Spec checkbox counts as stated under F-2.
- Verdict artifact: `Select-String -SimpleMatch -Pattern "REJECTED by direct observation"` on the verdict artifact — the original sentence remains in the original section (audit trail) and the amendment block states the withdrawal; assert instead that the amendment tokens `ORIGINATING` and `U2` each appear at least once in the verdict artifact (pre-edit: `ORIGINATING` 0; `U2` re-derive).
- Mirror presence and `PostedAs: comment` as stated under F-4.
- Commit gate as stated under F-5.

## Manual gate (reported to the coordinator by name)

MAINTAINER RATIFICATION OF THE AC1 NEGATIVE RESULT. AC1 cannot reach PASS by agent action: the spec's no-expiry clause forecloses it, and an expiry did not reproduce on this machine (62 clean targeted runs, 1400/1400 serial). Only the maintainer can ratify the negative result. This cycle corrects the evidence so that the ratification request is truthful, and posts the request on issue #743. Until ratification is given and transcribed, AC1 stays unchecked and the reaudit is expected to keep one acceptance-level blocking item open.
