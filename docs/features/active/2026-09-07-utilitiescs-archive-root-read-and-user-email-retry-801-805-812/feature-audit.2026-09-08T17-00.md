# Feature Audit — issue #812 (utilitiescs-archive-root-read-and-user-email-retry-801-805)

- Artifact timestamp: 2026-09-08T17-00
- Work mode: `full-bug` (marker `- Work Mode: full-bug`, `issue.md:12`)
- Acceptance-criteria source: **`spec.md` only**
- Baseline: `origin/main`; audit span `origin/main...HEAD`
- Verdict: **PASS. 6 of 7 acceptance criteria met; AC7 partially met and correctly left unchecked.**

`user-story.md` does not exist in this feature folder. Under `full-bug` that is the correct state,
not a gap: the AC-source table in `acceptance-criteria-tracking` assigns `spec.md` alone to
`full-bug`, and `user-story.md` is required only under `full-feature`.

---

## AC evaluation

Each criterion was verified against the delivered files and the committed evidence rather than
against its existing checkbox mark. Where the mark was already `[x]`, the mark was re-derived
independently; where the re-derivation agreed, the mark stands.

### AC1 — Archive-root degradation across all three display-projection surfaces — **PASS**

Recorded state `[x]`. **Agreed.**

Required: with `IOlObjects.ArchiveRootPath` arranged to throw `InvalidOperationException`, both
`FolderArray` and `FolderRowArray` complete without throwing and return every entry byte-identical to
the stored string, in all three population states; plus a text-parity method; plus a null-root case in
`ArchiveStemProjectionTests`.

Verified:

- `FolderPredictorArchiveRootDegradationTests` exists at the required path and contains one
  `[TestMethod]` per population state per surface — 6 methods (recents-only, suggestions-only, both;
  each on `FolderArray` and `FolderRowArray`). Each asserts `act.Should().NotThrow(…)` and
  entry-for-entry equality against `ExpectedUnprojectedSequence(...)`, which reconstructs the exact
  separator-plus-entries sequence from the seeded strings.
- The text-parity method `FolderArrayAndFolderRowArray_WithThrowingArchiveRoot_ProduceIdenticalText`
  asserts `rows.Select(r => r.Text)` equals `folderArray` under a throwing root, pinning the contract
  documented at `FolderPredictor.cs:234-242`.
- `ToDisplayStem_NullRoot_ReturnsInputUnchanged` is present in `ArchiveStemProjectionTests` and passes
  a literally `null` `archiveRoot`, closing the gap the suite previously had (it covered
  `string.Empty` and whitespace but not `null`).
- Degradation mechanism verified in the delivered code: `GetArchiveRootForDisplayOrNull` returns
  `null` on `InvalidOperationException`, and `ArchiveStemProjection.ToDisplayStem` returns its input
  unchanged for a null root (`ArchiveStemProjection.cs:45-48`).
- All 7 methods recorded `Passed` in the final run; all 7 are among the 10 recorded failures in the
  fail-before artifact `p1-t6-defect-a-red.2026-09-07T22-12.md`.

Scope note carried from the spec: AC1 is verified at the `FolderPredictor` unit level and **not** end
to end in QuickFiler, because `QfcItemController.FolderHandling.cs:233` still throws on the same call
frame. This is spec Non-Goals item 2 and Risk 3, disclosed in advance, and the reviewer confirmed the
site is unchanged at head. The criterion as written is about the two `FolderPredictor` surfaces and
is fully met.

### AC2 — One read and one warning per projection-helper invocation; only `InvalidOperationException` absorbed — **PASS**

Recorded state `[x]`. **Agreed.**

Verified, clause by clause:

- Five suggestions and three recents, one `FolderArray` access, getter invoked `Times.Exactly(2)` —
  `FolderArray_WithThrowingArchiveRootAndBothPopulated_ReadsArchiveRootPathExactlyTwice`.
- Same for `FolderRowArray` — `FolderRowArray_WithThrowingArchiveRootAndBothPopulated_ReadsArchiveRootPathExactlyTwice`.
- Recents-only access invokes it `Times.Once()` —
  `FolderArray_WithThrowingArchiveRootAndRecentsOnly_ReadsArchiveRootPathExactlyOnce`. This one also
  proves the suggestion block is genuinely skipped rather than reading and discarding.
- `FolderPredictor.ArchiveRoot.cs` contains **exactly one** `catch` clause and its type is
  `InvalidOperationException`; **exactly one** `logger.Warn` call; the message text contains no
  archive-root path and no mailbox address. Verified by reading the delivered file, not from the diff.
- A `COMException` arranged on the same getter propagates out of `FolderArray` —
  `FolderArray_WhenArchiveRootPathThrowsComException_PropagatesComException`.

Structural corroboration: `GetArchiveRootForDisplayOrNull` appears exactly 4 times in
`FolderPredictor.cs`, one per projection helper, each hoisted above its loop, so no helper reads the
root more than once per invocation. Three of the four read-count methods failed before the fix.

### AC3 — The five functional reads are not degraded — **PASS**

Recorded state `[x]`. **Agreed**, and independently re-derived rather than taken from the evidence
artifact.

- `_globals.Ol.ArchiveRootPath` occurs at exactly five sites in the post-change `FolderPredictor.cs`:
  `:305` `FindFolder`, `:376` `FindFolderRows`, `:687` `CreateFolder`, `:752` `CreateFolderAsync`,
  `:909` `LoopFolders`. These are the two `emailSearchRoots` seeds, the two `olAncestor` seeds, and
  the `LoopFolders` `olAncestor` seed that AC3 names.
- A search for `\btry\b|\bcatch\b` across the entire file returns **zero matches**, so none of the
  five can be inside a guarded region. None is a call to the accessor.
- Behavioural pin: `FindFolder_WithNullEmailSearchRootsAndThrowingArchiveRoot_StillThrowsInvalidOperationException`.
  Line 305 is the first executable statement of `FindFolder`, so the exception observed is
  deterministically the one from that read.
- `QuickFiler.Test` (1380 tests) and `TaskMaster.Test` (423 tests) are green with 0 failures and
  neither suite appears in the branch diff.

The `:914` → `:909` shift for the `LoopFolders` read is a pure line-number consequence of the
5-line net reduction; the expression is unchanged.

### AC4 — The User Email retry is bounded to one attempt per controller instance — **PASS**

Recorded state `[x]`. **Agreed.**

- Two consecutive `PopulateWithCurrent()` calls on one controller invoke the mocked
  `ExchangeUser.PrimarySmtpAddress` getter `Times.Once()` —
  `PopulateWithCurrent_CalledTwiceOnOneController_RetriesLookupOnlyOnce`. This is the method that
  failed before the fix (the sole failure in `p3-t3-defect-b-red…`, Total 73 / Failed 1).
- A second controller over the same failing store invokes it once more, `Times.Exactly(2)` cumulative
  — `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore`. This pins the
  bound as per-instance rather than static or per-store.
- A populated address invokes it `Times.Never()` —
  `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup`. This pins that
  the latch supplements rather than replaces the null check.
- No production line was added to `StoreWrapperController.Launch()`. Verified: the only hunk in that
  file is `@@ -95,0 +96,11 @@`, a pure insertion consuming zero pre-image lines, which cannot
  intersect the `Launch()` span; `[ExcludeFromCodeCoverage]` still immediately precedes
  `public void Launch()`.
- The three existing #797 AC6 tests are green with no assertion changed. The only edit inside any of
  them is the 3-line Arrange comment AC5 requires.

Reviewer note (advisory, does not affect the verdict): the delivered bound is per controller and
therefore also suppresses the retry for a *different* failing store re-selected in the same dialog
session. That is precisely what AC4 specifies and what the field's XML doc states; the spec's
accepted-behaviour paragraph describes only the same-store case. Recorded as CR-1 in
`code-review.2026-09-08T17-00.md`.

### AC5 — Prose at four sites states the bound the code enforces — **PASS**

Recorded state `[x]`. **Agreed.**

All four sites verified in their delivered form:

1. `StoreWrapperController.Display.cs:40-45` — retry-gate comment. States that
   `PopulateWithCurrent` runs on every store re-selection, that the latch bounds the retry to at most
   one attempt per controller instance, and that this equals one per dialog open **only because**
   `RibbonController.FolderStoresSettings` constructs a fresh controller. It explicitly disclaims the
   bound as "not a property of `PopulateWithCurrent` itself".
2. `StoreWrapper.cs:194-202` — `RefreshUserEmailAddress` comment. The required additional clause is
   present: "This member itself guarantees nothing about how often the lookup runs: it re-runs the
   lookup on every call and republishes whatever it returns. The bound lives in the caller."
3. `StoreWrapperController_Tests.Display.cs:118-121` — the Arrange comment in
   `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup`, now reading "at most once
   per controller instance" and marked "corrected by #812".
4. The #797 `spec.md` — all three passages corrected: Non-Goals item 8, the AC6 detail, and Risks
   item 1. Each carries an explicit "Dated correction, 2026-09-08, issue #812" marker. The AC6-detail
   self-contradiction is reconciled by name: the passage now states that the member does run on every
   re-selection **and** that the latch bounds the retry, rather than asserting both halves of the
   original contradiction.

No unqualified "at most once per dialog open" survives at any of the four sites as a property of a
member that does not enforce it.

The five historical #797 artifacts required to stay unmodified — `plan.2026-09-06T22-00.md`,
`research/research-folder-settings-persistence.md`,
`evidence/issue-updates/issue-797.2026-09-06T22-00.md`, `code-review.2026-09-07T22-40.md`, and
`feature-audit.2026-09-07T22-40.md` — are **absent from the branch diff**, verified against the
enumerated 45-path diff. The only #797 path in the diff is `spec.md`, whose presence makes the
absence finding non-vacuous.

### AC6 — File-size and placement constraints hold — **PASS**

Recorded state `[x]`. **Agreed.**

- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs` is new and declares
  `public partial class FolderPredictor` (`:18`).
- `FolderPredictor.cs` is **997** lines, no greater than its pre-change 1002. Independently confirmed
  by reading the file's last line.
- No other file created or modified by this change exceeds 500 lines: `ArchiveStemProjectionTests.cs`
  194, `FolderPredictorArchiveRootDegradationTests.cs` 394, `StoreWrapperController_Tests.Display.cs`
  373, `FolderPredictor.ArchiveRoot.cs` 81, `StoreWrapper.cs` 305,
  `StoreWrapperController.Display.cs` 184, `StoreWrapperController.cs` 399.
- Neither `FolderPredictorTests.cs` nor `StoreWrapperController_Tests.Launch.cs` appears in the diff.
  Confirmed against the enumerated diff.

AC6's parenthetical figure of 1066 lines for `FolderPredictorTests.cs` is stale — the file now
measures 1067 after an unrelated `origin/main` merge added a `[DoNotParallelize]` line (PR #814).
AC6's operative condition over that file is its **absence from the diff**, not its line count, so the
stale figure does not affect this verdict. No correction to `spec.md` is made, because the figure is a
dated observation rather than a requirement.

### AC7 — Full toolchain pass with non-vacuity and coverage — **PARTIAL; correctly left unchecked**

Recorded state `[ ]`. **Agreed — the criterion must stay unchecked.**

Nine of AC7's ten clauses are satisfied and evidenced:

| Clause | Verdict | Evidence |
|---|---|---|
| `csharpier format .` then `csharpier check .`, zero files needing formatting | PASS | format rewrote nothing; check exit 0 over 1613 files |
| First `msbuild … /t:Rebuild …` at `0 Error(s)` / `0 Warning(s)` | PASS | P6-T3 |
| Second `msbuild … /t:Rebuild …` at `0 Error(s)` / `0 Warning(s)` | PASS | P6-T4 |
| Captured logs contain zero `Skipping target "CoreCompile"` | PASS | 0 in 69105 lines and 0 in 69300 lines — the gates are demonstrably non-vacuous |
| `vstest.console.exe` over the three named assemblies with `/EnableCodeCoverage`, `/InIsolation` and the documented hazard filter, zero failures other than a carve-out member | PASS | 6675 / 6675 passed, 0 failed; no carve-out member needed |
| Coverage of the new accessor `>= 90%` | PASS | 100.0%, 19 of 19 lines, 1 matched class element |
| Coverage of the latch gate; changed lines not losing coverage | PASS | 6 of 6 changed executable lines covered, 0 with `hits` 0 |
| One uninterrupted pass in the stated order | PASS | 0 restarts |
| Evidence under the feature `evidence/` folder | PASS | 28 files under `evidence/{baseline,qa-gates,regression-testing}/` |
| Repository line coverage `>= 80%` **on the testable denominator** | **not verified** | denominator not computed by any task |

The tenth clause is stated over the `CLAUDE.md` UT2 *testable denominator*, which excludes VSTO
add-in lifecycle classes, WinForms form-derived and Designer-generated code, and seamless Outlook
interop handler classes. **No task in this plan computes that denominator.** The figure measured is
the raw root Cobertura `line-rate` over all instrumented code, which includes every excluded
category: 73.639% at the recorded baseline, taken before any source file was edited, and 73.679%
after. The floor was breached before this branch touched anything, and this branch moved the figure
**upward** by 0.040 percentage points.

Under `acceptance-criteria-tracking` rule 4 a criterion that cannot be fully verified is left
unchecked with its gap documented. That is what was done, and the disposition is recorded in full in
`evidence/qa-gates/p6-t7-clean-pass.2026-09-07T22-12.md`. Amending the clause inside the spec was
correctly rejected as an alternative: unlike the AC7 carve-out wording, that clause traces to a
repository policy requirement rather than to a fact about this workstation, so rewriting it inside a
feature spec would waive the policy rather than record an observation.

**The reviewer makes no check-off for AC7 and records no blocking finding against this change on
account of the pre-existing repository floor.**

---

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md
- Total AC items: 7
- Checked off (delivered): 6
- Remaining (unchecked): 1
- Items remaining: AC7 — Full toolchain pass with non-vacuity and coverage (nine of ten clauses
  satisfied; the tenth, repository line coverage >= 80% on the CLAUDE.md UT2 testable denominator, is
  unverified because no task computes that denominator; the raw measured figure is 73.679% against a
  pre-existing baseline of 73.639%)
```

**Check-offs made by this review: none.** AC1 through AC6 were already marked `[x]`; each was
independently re-derived and the mark stands, so no edit to `spec.md` was required. AC7 evaluates
PARTIAL and correctly remains `[ ]`.

**Acceptance criteria whose recorded state the reviewer disagrees with: none.**

---

## Regression assessment relative to baseline

| Dimension | Baseline | Head | Assessment |
|---|---|---|---|
| Test total | 6659 | 6675 | +16, exactly the methods this change adds |
| Test failures | 1 (issue-780 flake) | **0** | improved |
| Raw repository line coverage | 73.639% | 73.679% | improved by 0.040 pts |
| `lines-covered` / `lines-valid` | 166609 / 226251 | 166887 / 226505 | +278 covered against +254 valid |
| `FolderPredictor.cs` line count | 1002 | 997 | improved by 5 lines |
| Analyzer warnings | 0 | 0 | unchanged |
| Nullable errors | 0 | 0 | unchanged |
| Public API surface | — | unchanged | only a private signature changed |

No regression was identified in any dimension.

---

## Residual and owed items

1. **Follow-up issue for `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:233`** — owed
   before #812 is closed, per the spec's own Rollout section. Until it is fixed, an unresolvable
   archive root still throws out of `AssignFolderComboBox`; this change moves the throw site from
   `:212` to `:233` rather than removing it. Confirmed unchanged at head.
2. **Follow-up issue for #797 CR-2** (`SerializeNow` UI-thread file I/O and unbounded write-lock
   wait) — owed per the same section.
3. **Follow-up issue for the non-blocking Outlook COM read** (#797 Non-Goals item 8) — owed per the
   same section.
4. **CR-1 cross-store latch consequence** — recommend a sentence in the PR body; optionally a
   follow-up to key the latch on the store.
5. **Coverage-threshold conflict between `CLAUDE.md` UT2 (80% / 90%) and `.claude/rules` (85% / 75%)**
   — unreconciled repository-governance item, not this change's to decide.
6. **`#801` and `#805` close on merge with a pointer to `#812`**, per the spec's Rollout section.

---

## Verdict

**PASS.** The change delivers both defect fixes as specified, with genuine fail-before evidence for
each, non-vacuous tests, a clean uninterrupted toolchain pass, and coverage that meets every per-file
obligation. Blocking findings: **0**.
