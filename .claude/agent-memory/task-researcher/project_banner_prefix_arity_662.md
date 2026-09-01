---
name: banner-prefix-arity-662
description: "Issue #662 EfcSelectionGuard banner-prefix research: AC2's regex contradicts AC5's aliasing remedy; the consistency assertion does NOT catch the widening; /Tests: and /TestCaseFilter: are mutually exclusive"
metadata:
  type: project
---

Issue #662 (`efcselectionguard-banner-prefix-arity-and-stale-comment-662`) research, 2026-08-31.
Five findings that are not derivable by re-reading the code and would cost a later agent real time.

**1. The approved `issue.md` contains an internal contradiction between AC2 and AC5.**
AC2 asserts `const +string +[A-Za-z_]*BannerPrefix` scoped to `*.cs` returns exactly ONE line, but
AC5's natural implementation — aliasing `private const string BannerPrefix = BreadcrumbRowBuilder.BannerPrefix;`
in `FolderSuggestionTree.cs` — still matches AC2's regex, so AC2 returns two and fails while AC5
passes. Resolution recommended: DELETE the constant and reference the producer constant directly at
`FolderSuggestionTree.cs:197`, rather than amending an approved AC.
**Why:** the two criteria were written against different mental models of "dedupe" (remove the
literal vs. remove the declaration).
**How to apply:** when an AC pins a *declaration-shape regex count* and a sibling AC pins a
*literal-value count*, check whether the intended remedy satisfies both before planning; an alias
satisfies the literal count but not the declaration count.

**2. The test that looks like the consistency guard does not catch the prohibited edit.**
In `EfcFormControllerTests.cs:452-465`, line 462 (`creationPath.Should().Be(filingPath)`) still
PASSES if `EfcSelectionGuard`'s prefix is widened to `"===="` — both sides flip to true together.
Only line 463 (`creationPath.Should().BeFalse(...)`) fails, expected false / actual true.
**Why:** an agreement assertion between two predicates that share the relaxed term cannot detect a
relaxation of that term.
**How to apply:** when auditing whether a guard is test-pinned, evaluate the assertion under the
hypothesised bad edit; do not assume a "must agree" assertion is the protective one.

**3. `MinimumCreationLength = 3` is not a backstop for `"==="`.** The comparison is
`value.Length >= MinimumCreationLength`, and `"===".Length` is exactly 3, so the length rule passes
it. The three-character prefix is genuinely the sole rejecting mechanism at both EFC sites.

**4. `/Tests:` and `/TestCaseFilter:` cannot be combined in one `vstest.console.exe` invocation.**
Both forms have committed precedent in this repo (`/Tests:` in the 2026-03 archived features,
`/TestCaseFilter:` throughout #464/#511). #662's AC6/AC7 name `/Tests:`. If a run also needs
`TestCategory!=LiveOutlook`, everything must move to `/TestCaseFilter:`. Separately: no
`[TestCategory("LiveOutlook")]` exists in `QuickFiler.Test` or `UtilitiesCS.Test` — all three
occurrences are in `TaskMaster.Test` — so that filter is a no-op for those two assemblies.

**5. #464's prior art counts CLASSIFICATION SITES, not declarations.** Its "fourth/fifth copy of the
banner constant" language (`research/2026-08-25T12-20-...md:1132-1135`,
`followup-promotions.md:50`) refers to five prefix-classifying predicates. There are exactly THREE
`BannerPrefix` constant declarations. The two figures are not in conflict; do not treat the prior
art as refuting the three-declaration framing.

Related: [[qfc-item-controller-defects-484]], [[breadcrumb-navigation-defects-439-440-498-499]],
[[efc614-store-root-stem-leak]].
