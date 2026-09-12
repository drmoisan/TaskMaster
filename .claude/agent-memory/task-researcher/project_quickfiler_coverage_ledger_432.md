---
name: quickfiler-coverage-ledger-432
description: Issue #432 (epic #136 F1) ground truth — QuickFiler compiled surface is 121 files, manifest's "33 ExcludeFromCodeCoverage" is really 40 usages / 21 files, and 24 files are fully suppressed via partial-class inheritance
metadata:
  type: project
---

Research completed 2026-08-07 for epic #136 child F1 (issue #432, the QuickFiler coverage ledger).
Findings that are expensive to re-derive:

- **121 compiled files confirmed** from `<Compile Include=>` in `QuickFiler/QuickFiler.csproj`
  (lines 290-461). The epic manifest's "Feature File Assignments" table is **sound**: all 121 map
  to exactly one child, no gap, no duplicate, no phantom. It can be adopted verbatim as the
  ledger's `owning_child` column.
- **The manifest's "33 `[ExcludeFromCodeCoverage]`" is a file-containing-the-string count**:
  21 compiled files with a real attribute + 5 compiled files with comment/XML-doc mentions only +
  7 non-compiled orphan files = 33. Reconciled truth: **40 attribute usages** (14 type-level,
  26 member-level) across **21 compiled files**. The manifest's own `[X]` markers number 26,
  agreeing with neither figure.
- **24 compiled files are fully coverage-suppressed** once partial-class inheritance is applied.
  A type-level attribute on one partial suppresses every partial of that type: `ItemViewer.cs:20`
  suppresses 7 files including the 6,224-line Designer; `QfcDatamodel.cs:25` suppresses 3;
  `EfcViewer.cs:20`, `QfcFormViewer.cs:17`, `QfcItemViewerExpanded.cs:18` suppress 2 each.
  Removing a type-level attribute is a per-*type* decision, never per-file.
- **Absence from the Cobertura report means "no instrumentable code emitted", not "never
  executed".** `Properties/Settings.Designer.cs` proves it: never loaded, still reported at
  `line-rate="0"`. `Properties/Resources.Designer.cs` is absent because it carries
  `DebuggerNonUserCodeAttribute`. 51 of the 121 compiled files are absent (24 suppressed +
  23 interface-only + QfEnums + cInfoMail (fully commented out) + AssemblyInfo + Resources.Designer).
- **No `coverage.config` or `.runsettings` entry excludes QuickFiler.** The only exclusion
  mechanism in play is the source attribute plus the framework's `DebuggerNonUserCode` handling.
- Existing repo gates treat the coverage boundary as **inclusive pass** (`-lt 80`):
  `scripts/temp-extract-coverage.ps1:17` and `.codex/hooks/validate-feature-review-coverage.ps1:204`.
- `scripts/temp-extract-coverage.ps1` is dead prior art (throwaway, untested, `Write-Host`,
  hard-coded to UtilitiesCS, reads the unreliable `line-rate` attribute).

**Why:** 15 sibling children plus a capstone are blocked on this ledger; an incorrect denominator
or attribute inventory propagates to all of them.

**How to apply:** when any QuickFiler coverage work cites the "33 exemptions" figure, correct it.
When judging an `[ExcludeFromCodeCoverage]` removal, check whether the attribute is type-level
(affects every partial) or member-level (affects one method) before scoping the change. Full
evidence with line numbers is in
`docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/research/2026-08-07T22-15-quickfiler-coverage-ledger-research.md`.
See [[cobertura-line-double-count]] for why per-file rates must be recomputed from `<line>` nodes.
