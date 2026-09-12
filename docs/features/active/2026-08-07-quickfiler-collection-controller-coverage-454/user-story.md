# `quickfiler-collection-controller-coverage` — User Story

- Issue: #454
- Parent: epic #136 `quickfiler-per-file-coverage`, child F11, wave 1
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Work Mode: `full-feature` (this file and `spec.md` are both authoritative AC sources)
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-07T23-10

Throughout, `<FEATURE>` denotes
`docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/`. All other paths are
repo-relative. The technical contract — split design, seam inventory, measurement contract, constraints,
and risks — lives in `spec.md`; this document states who benefits, why, and what observable outcome
constitutes success.

---

## 1. This Is an Enabler Feature With No End-User-Visible Change

State this plainly: **a QuickFiler user will not be able to tell that this work happened.** The feature
ships no new capability, changes no screen, alters no keystroke, and modifies no filing behavior. The
epic's governing non-functional requirement is that observable QuickFiler flows do not change, and this
child is a testability refactor plus test authorship performed entirely underneath that requirement.

Inventing an end-user narrative for this work would be dishonest. The beneficiaries are the maintainer
and the autonomous agents that will maintain this code, and the value is delivered as measured
confidence, not as function.

---

## 2. Story Statement

- **As the maintainer of QuickFiler,** I want the repository's single largest production file to be
  broken into readable, single-responsibility units and covered by tests, so that I can change filing
  behavior without discovering the consequences in production.

- **As the maintainer,** I want the `[ExcludeFromCodeCoverage]` attribute removed from a file that a
  blanket sweep applied it to, so that the repository's coverage figure reflects what is actually
  tested rather than what is hidden from measurement.

- **As an autonomous agent asked to modify this controller,** I want each responsibility to live in its
  own file with an injectable seam, so that I can verify my change with a deterministic unit test
  instead of guessing or requiring a live Outlook session.

- **As a reviewer of a future QuickFiler pull request,** I want a per-file coverage figure that is
  computed correctly rather than read from a known-inflated report attribute, so that "the gate passed"
  means something.

---

## 3. Problem / Why

`QuickFiler/Controllers/QfcCollectionController.cs` is 2,349 lines — the largest production file in the
repository — and carries `[ExcludeFromCodeCoverage]` at `:21`. The attribute removes the type from
instrumentation entirely, so the file emits no Cobertura record at all. It is **unmeasured, not
covered**. Its 118-line contract file, `QuickFiler/Interfaces/IQfcCollectionController.cs`, completes the
pair.

Three concrete costs follow:

1. **The 500-line ceiling is breached by nearly five times.** A file of this size cannot be read in one
   pass, and a change to one responsibility sits next to nine unrelated ones.
2. **Coverage measurement is silently misleading.** The exemption came from a blanket 28-class sweep in
   commit `a564add0` (issue #197), not from a file-specific judgment that the code is untestable. The
   repository's coverage figure looks better than it is because 2,349 lines of controller logic are not
   in the denominator.
3. **The code is hard to test, and therefore hard to change safely.** Layout, dispatcher, viewer, and
   COM dependencies are reached statically or by direct construction, so most of the file cannot be
   exercised without a live form or a UI thread.

The starting position is not zero, and the plan must not assume it is. Two compiled test classes already
exercise the type and reach 24 of roughly 95 members. The estimated measured floor once the exemption is
removed is 12%-20%, most likely near 15%. The work is gap closure from a real, if low, base.

Twelve members in this file have **no caller anywhere in the repository** — roughly 227 lines that no
production path exercises. They are isolated into one clearly labelled file and removal is tracked as
issue #468.

---

## 4. Beneficiaries

**Primary — the QuickFiler maintainer.** Gains a controller split into 14 files whose names state their
responsibility, and a numeric per-file coverage figure that says how much of each is verified. Gains a
characterized record of fourteen latent defects that were previously undocumented, each carried by an
issue number rather than by institutional memory.

**Primary — future autonomous agents working in QuickFiler.** Gain injectable seams for the layout
surface, the viewer surface, the UI dispatcher, the item-controller factory, the viewer factory, the
helper factory, the error dialog, the pop-out path, and the sibling-controller skip call. Each seam
converts a change that previously required a live Outlook process into one verifiable by a deterministic
unit test. Gain a per-file measurement recipe that produces the same number twice on the same commit.

**Secondary — reviewers of any future QuickFiler change.** Gain a per-file figure computed from the
class-level line union rather than from the `line-rate` attribute, which is inflated by two separate
defects (issues #441 and #478) and, in one verified case, overstates a file by 5.9 points.

**Not a beneficiary — the QuickFiler end user.** No flow changes. That is the requirement, not a
shortfall.

---

## 5. Scenario — What Changes for a Maintainer

**Today.** A maintainer is asked to change how the collection controller renumbers item groups after a
removal. They open a 2,349-line file, locate the arithmetic among ten unrelated regions, and change it.
There is no coverage figure for the file because it is exempted, so they cannot tell whether the path
they touched is tested. To verify by hand they must launch Outlook, load a batch of mail, and remove an
item. If the change is subtly wrong, the failure surfaces to a user.

**After this feature.** The same maintainer opens
`QuickFiler/Controllers/QfcCollectionController.Layout.cs` — about 122 lines containing exactly the row
arithmetic, renumbering, and selection-index fix-up. They make the change, run
`QfcCollectionControllerLayoutTests.cs`, and see all four branches of
`UpdateSelectionNumberForRemoval` exercised without a form, a dispatcher, or a mail item. The per-file
coverage table in `<FEATURE>/evidence/qa-gates/` tells them the line and branch rate for that specific
file. No Outlook session is required.

**Also after this feature.** An agent asked to touch removal ordering finds the invariants recorded and
tested rather than implicit: unregister navigation before mutating the group list and re-register after;
capture all entry IDs before removing any so renumbering cannot cause index drift mid-iteration; read the
mail item before removing the group. These were previously enforced only by the original author's
sequencing.

---

## 6. Acceptance Criteria

Each criterion is an observable outcome with the artifact or command that demonstrates it. The
parenthetical references map to the technical criteria in `spec.md` §15, which carry the full detail.
Both files are authoritative AC sources under `full-feature` mode and are checked off independently.

- [ ] **US-AC1 — Nothing a QuickFiler user can observe has changed.** The production diff is confined to file layout, `using` removal, optional trailing constructor parameters, and private or internal seam fields whose production defaults behave identically to the code they replace. Both pre-existing test classes pass **without being edited**. Demonstrated by: `git diff <merge-base>..HEAD -- QuickFiler/` reviewed against that list, plus a full MSTest run showing `QfcCollectionControllerTests` and `QfcCollectionControllerDarkModeTests` green with no source change to either file. (spec AC16)
- [ ] **US-AC2 — The largest file in the repository is gone, replaced by units a maintainer can read.** `QfcCollectionController.cs` is split along responsibility seams into the retained root plus 13 named partials, and **no production file created or modified by this feature exceeds 500 lines**. A mechanical 500-line chop does not satisfy this criterion. Demonstrated by: the line-count listing in `<FEATURE>/evidence/qa-gates/file-sizes.<timestamp>.md`, every entry below 500, with file names matching the responsibility table in `spec.md` §6.1. (spec AC1)
- [ ] **US-AC3 — The file is measured rather than hidden.** `[ExcludeFromCodeCoverage]` is removed, and no file in scope carries it afterwards except at most `QfcItemViewerSurface.cs` under a file-specific rationale with an exact uncovered-member list recorded in F1's ledger. A blanket re-exemption of the controller is not acceptable. Demonstrated by: the attribute grep recorded in `<FEATURE>/evidence/qa-gates/`, and the file appearing in the final Cobertura report where it previously appeared nowhere. (spec AC3)
- [ ] **US-AC4 — Every unit of the controller is verified to a stated numeric level.** The controller and every partial reach **>= 80% line and >= 75% branch** coverage, and every newly created production file reaches **>= 90% line**. Demonstrated by: `<FEATURE>/evidence/qa-gates/per-file-coverage.<timestamp>.md`, one row per file with both figures. (spec AC6, AC7)
- [ ] **US-AC5 — The coverage numbers are trustworthy, not read from a known-inflated attribute.** Every per-file rate is recomputed from the union of class-level `./lines/line` entries keyed on line number with maximum hits, excluding the method subtree; the `line-rate` and `branch-rate` attributes are never read. A file with no coverable lines or no branch conditions reports **N/A**, never 0%, and never counts as a failure. Demonstrated by: the same per-file table stating explicitly that the attributes were not used and citing issues #441 and #478, with the two interface-only seam files shown as N/A. (spec AC6, AC7)
- [ ] **US-AC6 — The same commit measures the same twice.** Coverage figures for the file containing the process-global static counter are reproducible across two consecutive full test runs, because the tests that touch it are confined to one `[DoNotParallelize]` class that resets the counter by reflection before each test. Demonstrated by: two run records in `<FEATURE>/evidence/qa-gates/` showing identical per-file line and branch figures for that file. (spec AC14)
- [ ] **US-AC7 — No sibling's contract is disturbed.** `QuickFiler/Interfaces/IQfcCollectionController.cs` ends the feature with a zero diff; `xComma` remains `public static` on the controller type for `EfcHomeController.Metrics.cs:79`; `EmailsToMove` and `GetMoveDiagnostics` (including its `ref` parameter) are unchanged; and no sibling-owned file is edited. F7's "no contract additions needed" conclusion remains true. Demonstrated by: `git diff --exit-code <merge-base>..HEAD -- QuickFiler/Interfaces/IQfcCollectionController.cs` returning 0, and `git diff --stat` showing no sibling-owned file. (spec AC4, AC5)
- [ ] **US-AC8 — Latent defects are documented and dated, not silently absorbed or silently fixed.** A characterization test asserts the current behavior of the dormant duplicate-`KaKey` registration (#444) and of the process-global static counter (#286); issues #468, #469, #470, #471, #472, #473, #474, and #478 are referenced by number and none of them is fixed here. Demonstrated by: the named characterization tests passing, and a review of the diff showing no change to the code paths those issues describe. (spec AC15)
- [ ] **US-AC9 — The repository is no worse off, measured like for like.** Repository-wide line and branch coverage is retained or improved against this child's own before-figure, comparing harness-native to harness-native, with both the harness-native and the recomputed figures reported for each run. The transient drop when 17 files enter the denominator at once is stated in the evidence so a reviewer does not read it as a regression. Demonstrated by: the before/after table in `<FEATURE>/evidence/qa-gates/coverage-delta.<timestamp>.md` showing the identical command for both runs. (spec AC10, AC11)
- [ ] **US-AC10 — The upstream ledger dependency is honored, not assumed.** The four-gate Phase 0 check against F1's ledger runs before any production edit, halting when the ledger is missing or when it does not classify this controller as `testable`, and recording-and-proceeding on the interface-file classification and on harness discovery with the documented recompute recipe applied as the mandatory fallback. Seventeen ledger rows are appended in the same change as the csproj entries. Demonstrated by: `<FEATURE>/evidence/other/phase0-f1-gate.<timestamp>.md` and a single contiguous addition-only block in `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. (spec AC8, AC9)
- [ ] **US-AC11 — The new tests are ones a maintainer can trust and rerun anywhere.** All new tests use MSTest, Moq, and FluentAssertions in Arrange-Act-Assert form; are deterministic and isolated; create no temporary files; contact no external services; construct no live shown forms; raise no popups; and contain no `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, or unseeded `Random`. STA-bound tests are confined to dedicated `*.StaTests.cs` files, each documenting why no seam was feasible. No test is added to the 500-line `QfcCollectionControllerTests.cs`, and no new test file exceeds 500 lines. Demonstrated by: `<FEATURE>/evidence/qa-gates/test-policy-audit.<timestamp>.md` and the test file line-count listing. (spec AC12, AC13)
- [ ] **US-AC12 — The change lands clean.** The full C# toolchain — csharpier, analyzer build, nullable and warnings-as-errors build, MSTest with coverage — passes in that order in a single final pass with no step auto-fixing files, and the `QuickFiler.csproj` edit is one contiguous addition-only hunk with CRLF preserved. Demonstrated by: `<FEATURE>/evidence/qa-gates/toolchain.<timestamp>.md` recording each command with its exit code, and `git diff -- QuickFiler/QuickFiler.csproj` showing a single hunk with the file's `\r$` count equal to its line count. (spec AC2, AC17)

---

## 7. Non-Goals

- **No end-user-visible change of any kind.** No new capability, no UI change, no behavior change to
  filing, navigation, conversation expansion, or moves.
- **No defect fixes.** Fourteen latent defects were found and promoted to issues (#468-#474, #478, plus
  new findings on #444 and #286). They are characterized by test and referenced by number. Fixing any of
  them in this feature would violate the no-behavior-change requirement and make the coverage delta
  unattributable.
- **No contract additions.** `CleanupAsync` is public on the class and absent from the interface, and
  `LoadItemViewer_03` returns a concrete WinForms control rather than `IItemViewer`. Both are real design
  gaps and both are cross-child changes (F6 and F14). Neither is promoted here.
- **No deletion of the twelve unreachable members.** Deleting `public` members is a public-API change.
  They are isolated and covered this cycle; removal is issue #468.
- **No edits to any sibling child's files, to `UtilitiesCS/Properties/AssemblyInfo.cs`, to the shared
  coverage harness scripts, or to `coverage.config`.**
- **No change to the repository's coverage thresholds.** This feature meets the existing bars; it does
  not move them.
