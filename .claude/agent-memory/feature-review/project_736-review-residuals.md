---
name: 736-review-residuals
description: "#736 EFC archive-root: PASS/0 blocking, 11 PASS + 2 PARTIAL of 13 AC; the D2 coverage-escape precondition verified true but its set U was mislabelled 'unreachable'"
metadata:
  type: project
---

Issue #736 (`bug/efc-archiveroot-boundary-sink-defects-736`, HEAD `54da9e4d`) reviewed 2026-09-04:
**PASS, 0 blocking**, 11 AC PASS + **2 PARTIAL** (AC11, AC12), 0 FAIL, 0 UNVERIFIED. Repo coverage
85.4332% -> 85.459% line, 79.5348% -> 79.5242% branch. 6995 -> 7013 tests, 0 failures.

**Why:** the caller flagged AC12 as the item to adjudicate independently — checked off while its
literal ">= 90%" was unmet at 88.14% strict, discharged on the plan's D2 `10U` escape branch. The
escape held up; the *label* on its unreachable set did not.

**How to apply:**

- **AC12 outcome.** The escape's precondition is genuinely satisfied — I rebuilt the changed-line
  set from `git diff -U0` and joined it to the post-change Cobertura: 59 coverable / 52 covered /
  88.1356%, uncovered set = exactly the 7-member `U`, **0 members outside it**. Every executor figure
  matched to 4 decimals. Still PARTIAL, on three grounds: the literal 90% is unmet; under the AC's own
  "per UT2" qualifier the new module is 85.71% strict and the new method `EfcDataModel.InvokeFilerAsync`
  is **0/3 = 0.00%**; and 3 of `U`'s 7 members are not host-unreachable (see
  [[dont-trust-the-unreachable-label-on-a-coverage-escape-set]]).
- **AC11 outcome.** PARTIAL. 16 `.claude/agent-memory/**` paths sit in `origin/main...HEAD`, outside
  both the ratified 11-path Write Set and the Write Set section's own carve-out (which names only the
  feature folder's docs/evidence). They vanish only under the plan's D11 pathspec
  `":(exclude).claude/**"`. Rejecting that narrowing is right — cf. [[644-review-residuals]] — but here
  it found only a documentation gap, not a defect: `evidence/other/p7-t2-commit.md` enumerates all 16
  and states plainly they are outside the Write Set and invisible to the AC11 gate. Closes with a
  one-sentence amendment to the Write Set exclusion clause.
- **`Application.OpenForms` is app-domain-wide mutable global state.** `ShowModelessFaultNotice`'s
  only guard against building a WinForms window on an MSTest thread is
  `if (Application.OpenForms.Count == 0) return;`. Two tests invoke the default sink with no notifier
  installed and depend on that. In .NET Framework `Application.OpenForms` is one static
  `FormCollection` per app domain, filled from `Form.OnHandleCreated` — not per-thread. Exposure here
  is cross-assembly only (`QuickFiler.Test` has zero `new Form(` / `: Form` matches;
  `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:73` calls `viewer.Show()` on a real Form under
  `ClassLevel` / `Workers=0`). Non-blocking, did not fire in 7013/7013. Closes by installing a
  capturing `UserFaultNotifier` in both tests, which the sibling test already does correctly.
- **Retained `.min.log.txt` extracts proved nothing.** AC13's "logs retained as evidence" is met by two
  19-line project->DLL extracts that contain **zero** occurrences of `Skipping target "CoreCompile"`
  or `Task "Csc"` — the two literals AC13 turns on. The 10.6 MB `/fl` logs that do carry them are
  gitignored under `coverage/`. Still PASS (retention was asked for and exists), but re-derive the
  counts from the gitignored logs while they exist; after merge nothing substantiates them.
- **Chain of custody worth copying.** `coverage/p6-t6-run.log` names the emitted Cobertura path, and
  its SHA-256 (`A462D34E...44A777`) matched both the value transcribed in `p6-t6-coverage.md` and my
  own `Get-FileHash`. That binds the document I analysed to the 7013/7013 green run — do this instead
  of assuming the artifact on disk is the one the evidence describes.
- Sanitization was done **in-task**: `git grep -i -E "<account>|<host>|<alt-account>"` over all 8 commits'
  feature-folder trees returned 0, so no pre-sanitization blob is reachable (contrast
  [[730-review-residuals]]). All 407 `C:\Users\` lines read `C:\Users\REDACTED\...`.
- Promotions owed: the 2 PARTIALs plus CR-1 (OpenForms), CR-2 (`ex.Message` now reaches a user-visible
  TextBox, not just a log), CR-3 (`UnresolvableRule` reused for a COM transport fault it does not
  describe), F-4 (`EfcFormController.cs` 1216 -> 1320 lines, ceiling 500, D7-budgeted at 1330).
- `EfcDataModel.cs` is now **499 of 500** lines. The next one-line addition breaks the ceiling.
