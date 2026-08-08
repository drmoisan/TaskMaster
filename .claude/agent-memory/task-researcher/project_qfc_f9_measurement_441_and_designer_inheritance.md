---
name: qfc-f9-measurement-441-and-designer-inheritance
description: Issue #452/epic #136 F9 — issue #441 also corrupts per-file class/@line-rate (6dp vs 16dp tell); EfcViewer.Designer.cs is exempt only by partial-type attribute inheritance; csharpier 1.2.6 needs the `format` subcommand.
metadata:
  type: project
---

Three findings from F9 (#452) measurement research, 2026-08-07. All verified against the worktree;
re-verify paths before acting, since the epic is in flight.

**1. Issue #441 corrupts per-file rates, not just the repo total.**
`Merge-CoberturaClassesByFilename` (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:167-292`)
rebuilds a merged class's `<lines>` correctly (max-hits union) but then **recomputes** its
`@line-rate`/`@branch-rate` via the defective `Get-CoberturaCoverageSummary`, which selects
`.//lines/line` and so counts method-level lines plus the class-level rollup.
Proof: `FilerQueue.cs` in `.../424/evidence/qa-gates/coverage-final.cobertura.xml:18365-18480`
records `line-rate="0.405797"` = 28/69 = (18+10)/(49+20); the true class-level rate is 18/49 =
0.367347. Branch likewise: recorded 6/14 = 0.428571 vs true 5/10 = 0.5.
**Tell:** `Get-CoberturaCoverageSummary` rounds to 6 decimals, dotnet-coverage emits full double
precision. A 16-digit rate was never merged and is correct; a <=6-decimal rate was rewritten.
**Escape route that needs no shared-file edit:** the class-level `<lines>` is correct in every case,
so compute rates from the direct-child axis `class/lines/line` grouped by `@filename`, deduped by
`@number` with `max(@hits)`. Never read `@line-rate`; never use `.//`.
Consequence: the epic's "Measured Coverage Baseline" table (`epic.md:155-178`) is inflated for every
merged file.

**Why:** F9's acceptance criteria are numeric per-file line and branch rates, so a silently wrong
rate is an acceptance failure, not a cosmetic one.
**How to apply:** any child of epic #136 (or anything reading a committed Cobertura report in this
repo) must state its derivation axis explicitly and disclose #441.

**2. `[ExcludeFromCodeCoverage]` on one partial suppresses the whole type, including the Designer
partial.** `QuickFiler/Viewers/EfcViewer.Designer.cs` (4,276 lines) carries no attribute of its own;
it is absent from coverage only because `EfcViewer.cs:20` decorates the type. Removing that attribute
to bring the 162-line `EfcViewer.cs` into the denominator drags 4,276 generated lines in with it, in
the same edit. Mitigation with precedent: classify the Designer file `ratified-exempt` in the ledger
(not via an attribute) and have one test construct the control headlessly —
`ItemViewerExpanded.Designer.cs` sits at 99.5% purely because its owner is constructed in tests.

**Why:** an unnoticed ~4,000-line near-0% addition would breach the epic's AC8 "retain or improve"
repo-wide gate on its own.
**How to apply:** before de-exempting any WinForms partial in QuickFiler, check whether a
`*.Designer.cs` partial rides along.

**3. `csharpier .` is wrong for this repo.** `dotnet-tools.json:6` pins csharpier **1.2.6**, whose
CLI requires a subcommand: `dotnet tool run csharpier format .` (or `check .`). `CLAUDE.md` § C#1 and
§ CUT3 still show the v0 form. `.vscode/tasks.json:54-66` uses the correct form. Also: the tool
manifest is at the **repo root**, not `.config/`; `.dotnet-sdk/` was absent from the worktree
(`global.json:2-11` expects it); msbuild and vstest.console.exe resolve via `vswhere`, not `PATH`.

See [[quickfiler-percoverage-epic-136]] and
[[quickfiler-per-file-coverage-interface-only-bucket]].
