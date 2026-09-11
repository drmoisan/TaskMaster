# Code Review (Remediation Cycle 1 Exit Reaudit) — utilitiescs-test-determinism (Issue #811)

- Date: 2026-09-08T16-05
- Branch: `bug/utilitiescs-test-determinism-780-803-594-811`
- Head: `41005c6ba889fd645b1aa4058ba43168a6ff12e4`
- Base: `origin/main` @ `e6fc0e79be93e72bb5007fcd4f4314675470b073`
- Cycle-entry review (still governing for all source-level findings):
  `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/code-review.2026-09-08T11-30.md`

This is a delta review. The cycle-entry review covered all 20 changed source paths; those findings
stand and are not restated. This artifact covers only what changed since, plus a targeted re-check
of whether the two `origin/main` merges introduced any new interaction.

## Delta Under Review

**Zero source files changed in this cycle.** Independently confirmed by enumerating every
`diff --git` header in `coverage/review-811-source-final.patch`: exactly 20 paths, identical to the
pre-merge footprint (12 under `UtilitiesCS.Test/`, 8 under `UtilitiesCS/`).

The cycle's changes are documentation only:

| Path | Change | Assessment |
|---|---|---|
| `docs/features/potential/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md` | Added, 111 lines | Reviewed in detail below |
| `docs/features/active/.../evidence/other/remediation-cycle-1-r1-closure.md` | Added | Accurate in substance; one minor count error (F-7) |
| `docs/features/active/.../{policy-audit,code-review,feature-audit,remediation-inputs}.2026-09-08T11-30.md` | Committed (were untracked) | Correct; these are the cycle-entry record |

## Review of the New Follow-Up Entry

The entry is a defect report, so the review standard applied is technical accuracy against source,
not code style. Every factual claim was checked against the post-change files rather than against
the closure evidence or the remediation-inputs text.

**Strengths.**

1. **Every file-and-line citation is correct.** `ILGlobals.cs:117-118` (declarations), `:123`
   (reassignment), `:137` (fill); `MethodBodyReader.cs:105` and `:110` (readers);
   `ILGlobals_Tests.cs:15,26,37,47`; `MethodBodyReader_Tests.cs:364`; `AssemblyInfo.cs:18-21`. All
   were opened and confirmed at exactly the cited lines.
2. **The load-bearing claim survives falsification.** The entry asserts `CreateReader` is a helper
   "that every test in that class routes through." I attempted to falsify this: the class has 14
   `[TestMethod]`s but only 10 direct `CreateReader`/`CreateReaderWithoutBody` call sites. The
   remaining 5 route through the private `ConstructCustomInstructions` helper, which calls
   `CreateReader` at line 373. The claim is correct as written.
3. **The reasoning chain for the observed signature is sound and checkable.**
   `OperandType.InlineBrTarget` is enum value 0, which is what `default(OpCode)` yields; the
   `InlineBrTarget` branch at `MethodBodyReader.cs:119-123` takes the numeric path and never calls
   `module.ResolveString`. 1879067923 is 0x70004D13, inside the String-metadata-token range. The
   inference that only a partially filled table explains "token read correctly, name empty, wrong
   operand branch" is justified and is not overstated.
4. **Epistemic discipline is appropriate.** The frequency section states the observation (once in
   ten runs) and then explicitly declines to claim a base rate, noting that two clean baseline runs
   do not establish absence. The `## Steps to Reproduce` section states honestly that the failure
   does not reproduce deterministically, rather than inventing a procedure.
5. **The fix ordering is correct engineering.** Immutable publication first (removes the mutable
   window), `lock` second (correct but retains contention), `[DoNotParallelize]` third and
   explicitly labelled interim-only, with a cross-reference to the same reasoning `spec.md` applies
   to the AC3 console stopgap. That last cross-reference is the right instinct: it prevents a future
   maintainer from treating suppression as resolution.
6. **Severity is calibrated.** Medium, with the reasoning stated: low observed frequency and no
   production path affected, weighed against a nondeterministic failure in a required check. Not
   inflated to High to attract attention, not minimised to Low to reduce follow-up burden.

**Findings.**

- **C-7 (new, minor, non-blocking).** `## Actual Behavior` item 4 says the `InlineBrTarget` branch
  "stores a raw metadata token". It actually stores `ReadInt32(il, ref position) + position` —
  token plus current IL offset. Since `position` is a small offset in a short method body, the value
  stays in the 0x70 range and the diagnostic conclusion is unaffected. Recommend a one-line
  correction before promotion so a future investigator reconciling the printed value against a token
  table is not misled by an off-by-`position` delta. Tracked as policy-audit F-8.

- **C-8 (new, minor, non-blocking).** `## Actual Behavior` item 1 cites line 123 for the
  `singleByteOpCodes` reassignment but does not mention that line 124 reassigns `multiByteOpCodes`
  in the same unguarded window. The entry does say "both opcode tables" in the Summary and Suspected
  Cause sections, so the defect is not misdescribed; adding `:124` alongside `:123` would simply make
  the citation complete.

Neither finding changes the defect's description, its mechanism, its severity, or its recommended
fix. Neither is blocking.

## Merge-Interaction Re-Check

The two `origin/main` merges were checked for interaction with this item's change:

- The most recent merge brought sibling item 810 (PR #819), scoped to QuickFiler. None of this
  item's 20 source paths is under `QuickFiler/`, so there is no file-level intersection.
- The item's own source patch is unchanged in content and path set after both merges, which is the
  strongest available evidence that no semantic merge resolution touched this change.
- `spec.md` names `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:82-109` as a *caller* that
  must continue to compile and that now receives an `InvalidOperationException` instead of a
  `NullReferenceException`. That file is not modified by this branch, so it remains a compile-and-
  behaviour dependency on the merged QuickFiler work. The entry-cycle toolchain gates ran before the
  most recent merge. This is noted for completeness; the CI `mstest-coverage` run on the pull
  request exercises the merged tree and is the appropriate detector. Not blocking: the change to
  that path is exception-type only at a `catch (System.Exception)` boundary that logs and rethrows.

## Carried-Forward Findings

C-1 through C-6 from `code-review.2026-09-08T11-30.md` are unchanged and remain non-blocking:
C-1 two-Act test, C-2 test-name strength, C-3 null-writer tests using real `Console.Out`,
C-5 `MessageBoxInvoker` static, C-6 mixed tuple access style.

## Verdict

**PASS. 0 blocking findings.** Two new minor documentation-accuracy findings (C-7, C-8) against the
follow-up entry, both cosmetic and both appropriate to fold in at promotion time rather than gating
this cycle.
