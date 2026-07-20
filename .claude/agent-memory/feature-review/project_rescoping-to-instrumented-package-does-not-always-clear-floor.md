---
name: rescoping-to-instrumented-package-does-not-always-clear-floor
description: Unlike #328's StoresWrapper precedent, #392's QuickFiler package still fails the 85%/75% floor after rescoping the canonical JaCoCo artifact to the one actually-instrumented package
metadata:
  type: project
---

The established correction pattern from [[project_csharp-canonical-jacoco-includes-uninstrumented-assemblies]]
(re-scope a canonical `artifacts/csharp/coverage.xml` aggregate to only the first-party assemblies
whose own dedicated test project actually ran in the local `dotnet-coverage` collection) does **not**
always rescue the number above the 85% line / 75% branch floor. On issue #328
(`outlook-store-exclusion`), re-scoping the six-package aggregate down to the three instrumented
assemblies raised the figure from 70.45%/67.11% to 85.71%/79.34% — clearing the floor. On issue #392
(`folder-combobox-fallback-index-out-of-range`), the single instrumented package (`QuickFiler`, the
only one with its own `.Test` project in a `QuickFiler.Test`-only local collection) was itself
measured at 73.68% line / 64.62% branch — still below the floor. The raw baseline for that same
package (captured before the #392 fix) was 73.67%/64.53%, virtually identical, confirming the gap is
a genuine, broad, pre-existing under-coverage condition in `QuickFiler`'s WinForms/UI surface, not a
scope-measurement artifact this time.

**Why:** Rescoping only removes distortion from packages that were never run in a given local
collection (they read 0% because they're unmeasured, not because they're actually uncovered). It does
not change the true measured coverage of the one package that *was* run. Whether the corrected number
clears the floor is a fact about that specific package's real coverage, not a property of the
correction technique — do not assume rescoping is always sufficient just because it worked on a prior
review.

**How to apply:** After re-scoping to the instrumented package(s), always compare the corrected figure
against the 85%/75% floor explicitly before writing PASS. If it still fails, do not attempt to further
shrink scope to chase a passing number (that would cross into narrowing / cherry-picking, which is
prohibited). Instead: (1) compare against that package's own pre-change baseline to confirm whether the
gap is pre-existing or a regression, (2) if pre-existing and broad (spans many unrelated classes),
route to remediation as a maintainer-disposition decision (ratified exception, analogous to the
[[project_partial-remediation-new-code-floor-still-fails-209]] pattern, or a dedicated coverage-uplift
task) rather than a code fix inside a minor-audit bug-fix cycle, and (3) if a narrower, marginal gap
exists at the class/file level (e.g., #392's `QfcItemController.FolderHandling.cs` at 73.81% branch,
only 1.19 points under floor), it may be closeable with 1-2 targeted tests within the already-touched
file — recommend that as a separate, smaller remediation task from the broader package-wide gap.
