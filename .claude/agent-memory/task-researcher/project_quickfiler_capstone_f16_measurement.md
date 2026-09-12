---
name: quickfiler-capstone-f16-measurement
description: Epic #136 F16 capstone — the raw-vs-post-processed Cobertura discriminator that explains the 70.19%/85.65% swing, and the fact that F1 sidesteps rather than fixes #441/#478
metadata:
  type: project
---

Researched 2026-08-08 for epic #136 child F16 (capstone). Findings expensive to re-derive:

- **A Cobertura artifact in this repo is either RAW or POST-PROCESSED, and the two are not
  comparable.** Three one-glance discriminators, all verified on feature #424's pair: post-processed
  has a `<sources><source>.</source></sources>` element, repo-relative backslash `filename` values,
  and a 6-decimal root `line-rate`; raw has no `<sources>`, absolute capturing-worktree paths, and a
  full-double `line-rate`. `epic.md` blames the 70.19% -> 85.65% swing on "different instrumented
  scope"; **that is wrong**. The real causes are (a) the vendored-package strip and (b) the #441
  double count — `lines-valid="110849"` equals the literal count of `<line number=` elements in the
  same file exactly, while raw `lines-valid="79957"` is dotnet-coverage's own correct figure.
- **The vendored-package strip is automatic, not a separate step.** `Get-KoverageProjectAllowlist`
  enumerates repo csproj `AssemblyName` values minus `*.Test`, and `ConvertTo-KoverageCoberturaXml`
  removes every `<package>` not in that list. Verified: raw = 14 packages, post-processed = 9; the 5
  removed are exactly log4net, Mono.Reflection, Microsoft.IO.RecyclableMemoryStream,
  System.Interactive, System.Linq.Async.
- **F1 (#432) FIXES NEITHER #441 NOR #478.** Its folder has 11 mentions of 441 and **zero** of 478
  (its plan predates #478). It declares `Invoke-MSTestWithCoverage*.ps1` read-only and only avoids
  reproducing the defects in a new, separate harness. Both defects stay live in the repo, and every
  committed Cobertura artifact keeps a corrupted root `lines-valid` and corrupted merged-class
  `line-rate`.
- **F1's harness is package-scoped to `QuickFiler` by name and fails if that package is absent**, so
  it cannot produce a repository-wide figure. Nothing in the epic delivers a correct repo-wide
  number — that computation is capstone-owned.
- **`line-rate="0"` in this repo's reports means genuinely uncovered, not "no lines".** A search for
  `<lines />` in #424's report returns zero; declaration-only files are ABSENT, not present-at-0%.
  `Properties\Settings.Designer.cs` is present at 0% with three real `<line>` children.
- **The compile set is not the file set.** 156 `.cs` on disk under `QuickFiler/` vs 121 compiled.
  Besides Legacy (11) and Notes (2), there are 20 orphan `Viewers/` files and — not recorded anywhere
  in the epic or F1 — `QuickFiler\Helper Classes\FormFocusListener.cs`, on disk but not compiled. A
  filesystem-glob denominator would false-positive on it.
- `.csharpierignore` now excludes `*.csproj`/`*.props`/`*.targets`, so the old csproj-churn hazard
  from a repo-wide `csharpier format .` is closed.

**Why:** the capstone must close issue #136 AC8 (repository-wide retained-or-improved) with a
self-consistent before/after pair, and every existing producer of that number is defective.

**How to apply:** never compare a raw artifact to a post-processed one; check `<sources>` first.
Recompute both sides from class-level `./lines/line` only. Do not repeat epic.md's
"different instrumented scope" explanation. See [[cobertura-line-double-count]],
[[cobertura-perfile-attribution-contract]], [[quickfiler-coverage-ledger-432]]. Full evidence with
line numbers:
`docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497/research/measurement-harness-and-denominator.2026-08-08T00-45.md`.
