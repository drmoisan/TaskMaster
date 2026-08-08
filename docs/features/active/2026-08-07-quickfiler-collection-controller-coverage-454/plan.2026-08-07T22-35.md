# quickfiler-collection-controller-coverage — Atomic Implementation Plan

- **Issue:** #454
- **Parent:** epic #136 `quickfiler-per-file-coverage`, child F11, wave 1
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Work Mode:** `full-feature` (`spec.md` §15 and `user-story.md` §6 are both authoritative AC sources — 29 AC items total)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08
- **Status:** Revised; ready for preflight re-validation
- **Version:** 1.2 — preflight cycle 2 deltas applied (BLOCKING-1, REC-1). Revision notes:
  [P0-T15] no longer asserts a whole-tree invariant that no real worktree can satisfy — the assertion
  is now "no production or build-input file is modified or untracked", explicitly tolerating untracked
  documentation, feature-folder, evidence, agent-memory, and promoted-defect paths, with the full
  porcelain output recorded verbatim for audit; [P14-T20] re-attributed `ScrollIntoView` from the
  viewer surface to `IQfcTlpSurface` (the method's `ItemViewer` arrives as a parameter, not a field),
  with the panel `Top`/`Bottom` reads added to the [P2-T1] member list and to the [P3-T11] adapter
  test so the two new interface members carry coverage. Task count reconciled mechanically at
  **437** (see below); no task was lost between v1.0 and v1.1.
- **Task Count:** 437 lines match `^- \[ \] \[P\d+-T\d+\]`, across 24 phases (P0–P23), each phase
  numbered `T1..Tn` with no gaps and no duplicate IDs. Per-phase totals: P0 23, P1 26, P2 4, P3 15,
  P4 4, P5 12, P6 27, P7 6, P8 23, P9 15, P10 24, P11 20, P12 30, P13 17, P14 26, P15 18, P16 27,
  P17 17, P18 13, P19 24, P20 20, P21 9, P22 31, P23 6.

---

## Path Convention (binding)

**Every path in this plan is repo-relative.** This plan is authored in one worktree and executed in a
different one; an absolute path naming the authoring worktree would be stale and wrong at execution
time. Resolve every path against the repository root of whatever worktree the executor is running in.

`<FEATURE>` is written out in full in every evidence-bearing task and denotes
`docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454`.

## Evidence Location (non-overridable)

All evidence artifacts are written under
`docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/<kind>/`
where `<kind>` is one of `baseline`, `qa-gates`, `regression-testing`, `other`, per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Paths under `artifacts/` for evidence
(`artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, `artifacts/evidence/`) are FORBIDDEN
and fail preflight. Timestamps use `yyyy-MM-ddTHH-mm`.

Every command-step artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
Baseline and final-QC test artifacts additionally record numeric coverage headline values.

## Required References

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `docs/features/epics/quickfiler-per-file-coverage/epic.md`
- `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/spec.md`
- `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/user-story.md`
- `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/qfc-collection-controller.md`
- `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/iqfc-collection-controller.md`
- `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/coverage-harness-contract.md`

All work must comply with these policies; this plan does not duplicate their content.

---

## Mandated Sequencing

Split FIRST, then seam extraction, then coverage (`spec.md` §5). Each stage strictly depends on the
previous. A mechanical 500-line chop is explicitly rejected; the split follows the logical
responsibility seams in `research/qfc-collection-controller.md` §A3.

| Stage | Phases | Content |
| --- | --- | --- |
| Stage 0 — gate and baseline | Phase 0 | F1 four-gate check, policy reads, merge-base baseline with the attribute still present |
| Stage 1 — split | Phase 1 | 13 partials + root; attribute and three `using` directives removed; csproj + ledger; post-removal measurement |
| Stage 2 — seam extraction | Phases 2–7 | 4 seam files, then seams S1–S10 declared and every consumption site rewired, then the `Removal.cs` contingency |
| Stage 3 — coverage | Phases 8–21 | One phase per production file; each `research` §F test case is its own task; then measurement, gap closure, and delta |
| Close-out | Phases 22–23 | 29-item AC verification, then the full C# toolchain loop |

## Phase Map — one phase per production file

| Production file (all repo-relative) | Created in | Owning phase |
| --- | --- | --- |
| `QuickFiler/Controllers/IQfcTlpSurface.cs` | Phase 2 | Phase 2 |
| `QuickFiler/Controllers/QfcTlpSurface.cs` | Phase 3 | Phase 3 |
| `QuickFiler/Controllers/IQfcItemViewerSurface.cs` | Phase 4 | Phase 4 |
| `QuickFiler/Controllers/QfcItemViewerSurface.cs` | Phase 5 | Phase 5 |
| `QuickFiler/Controllers/QfcCollectionController.cs` (retained root) | pre-existing | Phase 6 |
| `QuickFiler/Controllers/QfcCollectionController.RemoveGroup.cs` (contingency) | Phase 7 | Phase 7 |
| `QuickFiler/Controllers/QfcCollectionController.State.cs` | Phase 1 | Phase 8 |
| `QuickFiler/Controllers/QfcCollectionController.LoadSync.cs` | Phase 1 | Phase 9 |
| `QuickFiler/Controllers/QfcCollectionController.LoadAsync.cs` | Phase 1 | Phase 10 |
| `QuickFiler/Controllers/QfcCollectionController.GroupFactory.cs` | Phase 1 | Phase 11 |
| `QuickFiler/Controllers/QfcCollectionController.Removal.cs` | Phase 1 | Phase 12 |
| `QuickFiler/Controllers/QfcCollectionController.KeyboardWiring.cs` | Phase 1 | Phase 13 |
| `QuickFiler/Controllers/QfcCollectionController.Selection.cs` | Phase 1 | Phase 14 |
| `QuickFiler/Controllers/QfcCollectionController.NavigationToggle.cs` | Phase 1 | Phase 15 |
| `QuickFiler/Controllers/QfcCollectionController.Conversation.cs` | Phase 1 | Phase 16 |
| `QuickFiler/Controllers/QfcCollectionController.Layout.cs` | Phase 1 | Phase 17 |
| `QuickFiler/Controllers/QfcCollectionController.Theme.cs` | Phase 1 | Phase 18 |
| `QuickFiler/Controllers/QfcCollectionController.Move.cs` | Phase 1 | Phase 19 |
| `QuickFiler/Controllers/QfcCollectionController.LegacyLoadPaths.cs` | Phase 1 | Phase 20 |
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | pre-existing | zero edits; classification only (Phase 0 G0.3, Phase 22) |

**Why Stage 1 and Stage 2 are whole-controller stages, not per-file phases.** A C# `partial` split
must land as one compiling unit and `spec.md` §5 requires the second (post-exemption-removal)
measurement to be taken before any seam work or test authorship. Seam fields are likewise declared
and consumed in the same phase (Phase 6) so that no phase boundary leaves an assigned-but-never-read
private field, which the analyzer and `TreatWarningsAsErrors` builds would reject. Each per-file
phase in Stage 3 is self-validating: it authors that file's tests and proves them green with a
scoped run before the phase closes.

## Structural Facts (established; do not re-derive or contradict)

1. `[ExcludeFromCodeCoverage]` sits at `QuickFiler/Controllers/QfcCollectionController.cs:21`, above
   the class declaration at `:22`. It came from a blanket 28-class sweep (commit `a564add0`, issue
   #197), not a file-specific ratification.
2. Cobertura emits one `<class>` per `(type, source file)` pair, so per-file attribution survives the
   partial split. **The `<class>` `line-rate` and `branch-rate` attributes must never be read** —
   they are wrong due to issues #441 and #478. Per-file rates are recomputed by unioning
   `./lines/line` (class-level only, `MAX(@hits)` per `@number`) per CMD-RECOMPUTE.
3. Branch coverage IS emitted and the 75% gate is enforceable. A file with no branching lines yields
   `0/0` and reports **N/A**, never 0%.
4. CI produces no Cobertura, so the repository-wide figure is produced locally and requires the
   stale-`.claude/worktrees` pre-flight assertion (CMD-PREFLIGHT) before every measurement run.
5. `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is at EXACTLY 500 lines — zero
   headroom. **No new test may be added to it.** All new tests go in new files.
6. `QuickFiler/Controllers/EfcHomeController.Metrics.cs:79` (child F8) calls
   `public static string xComma(string)` at `QuickFiler/Controllers/QfcCollectionController.cs:2330`.
   The split MUST keep it `public static` on the same type.
7. The seam work requires ZERO edits to `QuickFiler/Interfaces/IQfcCollectionController.cs`. All 10
   seams are `private`/`internal`; the only public change is optional trailing constructor
   parameters. F6 constructs the concrete type at `QuickFiler/Controllers/QfcFormController.Actions.cs:49`,
   `:83`, `:139` — those must compile unchanged and are not edited.
8. `scripts/vscode/TaskMaster.cli.runsettings:3-8` (the file the harness applies) and
   `TaskMaster.runsettings:3-8` both set `<Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope>`.
   Tests touching `private static int removespecificcontrolgroupcounter`
   (`QuickFiler/Controllers/QfcCollectionController.cs:1157`) must not run in parallel with each
   other; all three `spec.md` §9.6 mitigations are mandatory.
9. STA helpers already exist at `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:267-278`
   and `:302-317` (with `ShutdownDispatcher` at `:323-326`). **No new NuGet package is required.**
   Keep the STA set minimal — exactly two `*.StaTests.cs` files.
10. `QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`, so
    `internal` seams are directly testable. `UtilitiesCS` grants no such access to `QuickFiler.Test`;
    build a local seam rather than editing `UtilitiesCS/Properties/AssemblyInfo.cs`.
11. `QuickFiler.Test/QuickFiler.Test.csproj` is a legacy non-SDK project with **no globbing**. Its
    existing `<Compile Include>` entries span `:58-168` — **107 entries**. Every new test file needs
    its own explicit `<Compile Include>` entry or it will not compile.
12. Both `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj` are
    CRLF-terminated. **Never use `sed -i`.** Use the Edit tool or `perl -0777` with explicit `\r\n`.

## Documented Deviation Carried Forward

**D-6 (`spec.md` §14).** The largest projected partial is
`QuickFiler/Controllers/QfcCollectionController.Removal.cs` at **~348 lines**, NOT the 293 figure
that appeared in an earlier brief (293 is `LoadAsync.cs`). Research pre-authorized a further split
into `QuickFiler/Controllers/QfcCollectionController.RemoveGroup.cs` if seam work pushes `Removal.cs`
past ~430 lines. Phase 7 is that contingency, with an explicit trigger. If taken, the
`QuickFiler/QuickFiler.csproj` and coverage-ledger additions become **18** entries/rows rather than 17.

## Out of Scope — Characterize, Do NOT Fix

Nine promoted issues plus two pre-existing ones with new findings. Under the epic's no-behavior-change
NFR **none is fixed here**: #468, #469, #470, #471, #472, #473, #474, #478, and new findings on #444
and #286.

| Issue | Disposition | Task |
| --- | --- | --- |
| #444 (**DORMANT** — `WireUpKeyboardHandler` has no caller anywhere in the repository) | characterize | [P20-T2], [P20-T3], [P20-T4] |
| #286 (process-global static counter) | characterize | [P12-T17] |
| #468 (twelve unreachable members, ~227 lines) | isolate into `LegacyLoadPaths.cs`, cover by direct call, do not delete | [P1-T14], all of Phase 20 |
| #469 (move-diagnostics defects) | characterize | [P19-T14], [P19-T15] |
| #470 (conversation index defects) | characterize | [P16-T7], [P16-T23], [P16-T25] |
| #471 (`EliminateSpaceForItems` sign error) | characterize | [P17-T12] |
| #472 (navigation `Digits` desync) | characterize | [P13-T15] |
| #473 (background-task and catch defects) | characterize; task cites research §F6 and confirms the issue number at execution time | [P12-T20] |
| #474 (concrete downcast and modal property getter) | seam half only; design half deferred | [P6-T6], [P6-T8] |
| #478 (harness merge defect) | never read `line-rate`/`branch-rate` | [P0-T22], [P1-T26], [P21-T3] |

Per `spec.md` §10.1, the per-defect mapping back to the research inventory
(`research/qfc-collection-controller.md` §E1-E19) is **confirmed against the live issues at execution
time**. Where a task's issue number does not match the live issue on inspection, correct the citation
in the test docstring; the binding rule does not depend on the mapping — characterize, do not fix.
Two further asymmetries carry no promoted issue number and are cited to the research section instead:
the `RemoveControlsAsync`/`RemoveControls` `UnhookAll` asymmetry
(`research/qfc-collection-controller.md` §F6) and the `ResetPanelHeight`/`ResetPanelHeightAsync`
asymmetry (§F11).

---

## Command Reference

Each command below is executed verbatim from the repository root of the executing worktree. Every
task that names a command must run it and record `Timestamp:`, `Command:`, `EXIT_CODE:`, and
`Output Summary:` in its evidence artifact. `EXIT_CODE: SKIPPED` is not a valid passing outcome for
any command-bearing task in this plan.

**CMD-BOOTSTRAP**
```powershell
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet tool run csharpier --version
$dotnetCoverageOk = $true
try { & dotnet-coverage --version | Out-Null; if ($LASTEXITCODE -ne 0) { $dotnetCoverageOk = $false } } catch { $dotnetCoverageOk = $false }
if (-not $dotnetCoverageOk) { dotnet tool install --global dotnet-coverage }
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "**\vstest.console.exe"
pwsh -NoProfile -Command '$PSVersionTable.PSVersion'
```

**CMD-FORMAT** (formatting; run first in every toolchain pass)
```powershell
dotnet tool run csharpier format .
dotnet tool run csharpier check .
```

**CMD-ANALYZE** (linting / .NET analyzers)
```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

**CMD-NULLABLE** (type checking / nullable flow)
```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
```

**Baseline-relative gate convention for CMD-ANALYZE and CMD-NULLABLE (binding for every task in this
plan that names either command).** Both switches apply SOLUTION-WIDE, and a repository-wide nullable
remediation epic is still in flight, so the pre-change exit code for either command is not guaranteed to
be 0. This child is an epic-wave-1 child: its per-child gate is scoped to its own branch, and cross-child
CS86xx fan-in accumulates on the integration branch, so an absolute exit-0 gate would make this child
responsible for sibling debt it cannot fix. Therefore, wherever this plan says a task "meets the
baseline-relative gate" for CMD-ANALYZE or CMD-NULLABLE, the passing condition is BOTH of:

1. `EXIT_CODE` equals the corresponding baseline exit code recorded in [P0-T23] (`ANALYZE_BASELINE_EXIT`
   for CMD-ANALYZE, `NULLABLE_BASELINE_EXIT` for CMD-NULLABLE); and
2. the diagnostic set scoped to the files this feature creates or modifies is EMPTY — no analyzer
   diagnostic and no `CS86xx` nullable diagnostic whose reported file path is one of this feature's
   touched files.

If the baseline exit code is 0, condition 1 degenerates to exit 0 and the gate is strictly the stronger
of the two. A diagnostic count increase in an untouched file relative to the [P0-T19]/[P0-T20] baselines
fails condition 1 and must be investigated before the task closes. CMD-FORMAT is NOT baseline-relative:
`csharpier check .` must exit 0 unconditionally.

**CMD-PREFLIGHT** (mandatory before every measurement run; `spec.md` §7.5)

`.claude` is evaluated RELATIVE to the executing repository root, never against the absolute path. The
executing worktree is itself rooted at `...\TaskMaster\.claude\worktrees\agent-<id>`, so an absolute-path
match would flag every one of this worktree's own freshly built assemblies and could never pass after a
build. Only a `.claude` segment BELOW the executing root (a nested stale worktree) is a finding.
```powershell
$root = (Resolve-Path .).Path.TrimEnd('\')
$stale = Get-ChildItem -Path . -Recurse -Filter '*.Test.dll' |
    Where-Object {
        $_.FullName -match '\\bin\\Debug\\' -and
        $_.FullName -notmatch '\\obj\\' -and
        $_.FullName -notmatch '\\ref\\'
    } |
    Where-Object { $_.FullName.Substring($root.Length) -match '^\\\.claude\\' }
if ($stale) { throw "Stale worktree test assemblies present; remove before measuring:`n$($stale.FullName -join "`n")" }
```

**CMD-REBUILD**
```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

**CMD-COVERAGE** (the identical command for the before-run, the post-removal run, and the final run)
```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 `
    -SearchRoot . -Configuration Debug `
    -CoverageOutput 'coverage\coverage.cobertura.xml'
```

**CMD-SCOPED** (per-phase scoped test run; `<Filter>` is supplied by the calling task)

The build is the FIRST line of this block and is not optional. `vstest.console.exe` runs a prebuilt
assembly and never compiles; without the leading build, a test file authored earlier in the same phase
is absent from `QuickFiler.Test.dll` and `/TestCaseFilter` matches zero tests. Every task that names
CMD-SCOPED runs all three lines verbatim.
```powershell
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "**\vstest.console.exe" | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings /TestCaseFilter:"<Filter>"
```
The scoped run is a passing outcome only when the build exits 0 AND the filter matches at least one
test; a zero-match run is a failure, not a pass.

**CMD-RECOMPUTE** (the `spec.md` §7.3 recipe; authoritative for every per-file figure). Write this
block to a session-scratch path OUTSIDE the repository and invoke it there. It is never committed, so
it adds no PowerShell production file to the repository and does not modify
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`,
or `scripts/temp-extract-coverage.ps1`. This plan is the durable copy.
```powershell
param([Parameter(Mandatory)][string]$Xml, [Parameter(Mandatory)][string[]]$Files)
$doc = [xml](Get-Content -LiteralPath $Xml -Raw)
foreach ($f in $Files) {
  $classes = @($doc.SelectNodes('//class') | Where-Object { $_.GetAttribute('filename') -ieq $f })
  if ($classes.Count -eq 0) { "$f`tABSENT`tABSENT"; continue }
  $lines = @{}; $br = @{}
  foreach ($c in $classes) {
    foreach ($ln in @($c.SelectNodes('./lines/line'))) {
      $n = [int]$ln.GetAttribute('number'); $h = [int]$ln.GetAttribute('hits')
      if (-not $lines.ContainsKey($n) -or $lines[$n] -lt $h) { $lines[$n] = $h }
      if ($ln.GetAttribute('branch') -ieq 'True' -and $ln.GetAttribute('condition-coverage') -match '\((\d+)/(\d+)\)') {
        $cov = [int]$Matches[1]; $tot = [int]$Matches[2]
        if (-not $br.ContainsKey($n) -or $br[$n].Total -lt $tot) { $br[$n] = [pscustomobject]@{ Covered = $cov; Total = $tot } }
      }
    }
  }
  $lt = $lines.Count; $lc = @($lines.Values | Where-Object { $_ -gt 0 }).Count
  $lineRate = if ($lt -eq 0) { 'N/A' } else { '{0:P2} ({1}/{2})' -f ($lc / $lt), $lc, $lt }
  $bt = [int](($br.Values | Measure-Object -Property Total -Sum).Sum)
  $bc = [int](($br.Values | Measure-Object -Property Covered -Sum).Sum)
  $branchRate = if ($bt -eq 0) { 'N/A' } else { '{0:P2} ({1}/{2})' -f ($bc / $bt), $bc, $bt }
  "$f`t$lineRate`t$branchRate"
}
```
Three states are reported distinctly: **ABSENT** (filename emits no `<class>`), **N/A** (present with
zero coverable lines or zero branch conditions), and a numeric rate. `ABSENT` and `N/A` are never
rendered as 0% and never count as a failure.

---

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture, Policy Reads, and the F1 Ledger Gate

- [ ] [P0-T1] Bootstrap the C# toolchain by running CMD-BOOTSTRAP and recording the resolved csharpier version, `dotnet-coverage` availability, the `vstest.console.exe` path, and the PowerShell version to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/other/toolchain-bootstrap.<timestamp>.md`
  - Acceptance: artifact exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; csharpier reports `1.2.6`; a `vstest.console.exe` path is resolved; `dotnet-coverage --version` returns a version
- [ ] [P0-T2] Read `CLAUDE.md` in full
- [ ] [P0-T3] Read `.claude/rules/general-code-change.md` in full
- [ ] [P0-T4] Read `.claude/rules/general-unit-test.md` in full
- [ ] [P0-T5] Read `.claude/rules/csharp.md` in full
- [ ] [P0-T6] Read `docs/features/epics/quickfiler-per-file-coverage/epic.md` in full, with specific attention to the "Mid-Wave File Creation" rules and the "Cross-Child Constraints" csproj rules
- [ ] [P0-T7] Read `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/spec.md` and `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/user-story.md` in full and confirm the 29-item AC inventory (17 in `spec.md` §15, 12 in `user-story.md` §6)
- [ ] [P0-T8] Read the three research artifacts under `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/` in full: `qfc-collection-controller.md`, `iqfc-collection-controller.md`, `coverage-harness-contract.md`
- [ ] [P0-T9] Write `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/other/phase0-instructions-read.md` recording `Timestamp:`, `Policy Order:` (the `policy-compliance-order` sequence), and the explicit list of every file read in [P0-T2]–[P0-T8]
- [ ] [P0-T10] Evaluate gate **G0.1** by asserting `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` exists; if absent, **HALT** and emit the literal `F1_LEDGER_MISSING: coverage-ledger.md not present; F1 (#432) has not landed on the integration branch.` — do not author a substitute ledger and do not remove `[ExcludeFromCodeCoverage]`
- [ ] [P0-T11] Evaluate gate **G0.2** by locating the ledger row whose text contains the literal `Controllers/QfcCollectionController.cs` (the root-file row, not a `.<Concern>.cs` partial row) and asserting its bucket token is `testable`; **HALT** with `F1_LEDGER_ROW_MISSING` if no such row exists, or `F1_LEDGER_CONFLICT` if it is bucketed `ratified-exempt` or `interface-only`
- [ ] [P0-T12] Evaluate gate **G0.3** by locating the ledger row containing the literal `IQfcCollectionController.cs` and checking its bucket token has the prefix `interface-only`; **RECORD, DO NOT HALT** — if absent or otherwise classified, record `F1_LEDGER_RECONCILE: IQfcCollectionController.cs classified <X>; expected interface-only / not-measured.` and append or correct that row in Phase 1, citing `research/iqfc-collection-controller.md` §A.3-A.5
- [ ] [P0-T13] Evaluate gate **G0.4** by searching for an F1 per-file coverage harness that emits line and branch rates from a Cobertura path; **RECORD, DO NOT HALT, FALLBACK MANDATORY** — if it computes from class-level `<lines>` children, use it and record a one-line confirmation; if it reads the `line-rate` attribute, use CMD-RECOMPUTE as authoritative, record F1's figure alongside, and record `F1_HARNESS_DISAGREES`; if not found, use CMD-RECOMPUTE and record `F1_HARNESS_ABSENT_FALLBACK_APPLIED`
- [ ] [P0-T14] Write `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/other/phase0-f1-gate.<timestamp>.md` recording each of G0.1–G0.4, its outcome, and any of the literal codes `F1_LEDGER_RECONCILE`, `F1_HARNESS_DISAGREES`, `F1_HARNESS_ABSENT_FALLBACK_APPLIED` that applied
  - Acceptance: satisfies spec AC9 and user-story US-AC10 (gate half)
- [ ] [P0-T15] Record the current `HEAD` SHA and the merge-base SHA against the integration branch into `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/tree-state.<timestamp>.md`, and assert that `git status --porcelain` reports **no modified or untracked path under `QuickFiler/`, `QuickFiler.Test/`, or `UtilitiesCS/`, and no modified or untracked `*.cs`, `*.csproj`, `packages.config`, or `app.config` path anywhere in the repository**
  - Acceptance: the SHAs are recorded as observed values, not as expected literals; the tree invariant asserted is "no production or build-input file is modified or untracked before the first production edit", not an empty `git status --porcelain` and not a specific SHA. Untracked documentation, feature-folder, evidence, agent-memory, and promoted-defect paths are expected at this moment and are excluded by construction; any `.cs`, `.csproj`, `packages.config`, or `app.config` entry in the porcelain output is a failure. Record the full porcelain output verbatim in the artifact so the exclusion is auditable
- [ ] [P0-T16] Verify and record the pre-change tree invariants in the same artifact: `[ExcludeFromCodeCoverage]` present at `QuickFiler/Controllers/QfcCollectionController.cs:21`; `using System.Diagnostics.CodeAnalysis;` at `:4`; `using System.Net.NetworkInformation;` at `:6`; `using System.Windows;` at `:10`; `<Compile Include="Controllers\QfcCollectionController.cs" />` at `QuickFiler/QuickFiler.csproj:311` with `Controllers\EmailSorter.cs` at `:312`; `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` at exactly 500 lines; `QuickFiler/Interfaces/IQfcCollectionController.cs` at 118 lines
- [ ] [P0-T17] Run CMD-PREFLIGHT and record its output to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/other/preflight-stale-worktrees.<timestamp>.md`
  - Acceptance: `EXIT_CODE: 0` and no stale `*.Test.dll` under any `.claude` path BELOW the executing repository root; the executing worktree's own root path segment `...\TaskMaster\.claude\worktrees\agent-<id>` is NOT a finding, which is exactly what the relative-path predicate in CMD-PREFLIGHT encodes; if the assertion throws, remove the stale assemblies and re-run before proceeding
- [ ] [P0-T18] Run CMD-FORMAT and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/format.<timestamp>.md`
- [ ] [P0-T19] Run CMD-ANALYZE and record the result, including the warning and error counts, to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/analyze.<timestamp>.md`
- [ ] [P0-T20] Run CMD-NULLABLE and record the result, including the warning and error counts, to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/nullable.<timestamp>.md`
- [ ] [P0-T21] Run CMD-REBUILD then CMD-COVERAGE with `[ExcludeFromCodeCoverage]` STILL PRESENT, copy the produced report to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/coverage-baseline.cobertura.xml`, and write the companion command record to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/coverage-baseline.<timestamp>.md`
  - Acceptance: the copied XML contains a `<sources>` element proving it was post-processed; the companion `.md` carries `Timestamp:`, `Command:` (both CMD-REBUILD and CMD-COVERAGE verbatim), `EXIT_CODE:`, and `Output Summary:` with total tests, passed, failed, and the harness-native `/coverage/@line-rate` and `/coverage/@branch-rate` as numeric values. The `.cobertura.xml` is a data artifact and cannot itself carry the four fields; the companion `.md` is the command-step artifact
- [ ] [P0-T22] Run CMD-RECOMPUTE against the baseline XML for `QuickFiler\Controllers\QfcCollectionController.cs` and `QuickFiler\Interfaces\IQfcCollectionController.cs`, and record both results plus the recomputed repository-wide line and branch figures to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/coverage-baseline-recomputed.<timestamp>.md`
  - Acceptance: `QfcCollectionController.cs` reports **ABSENT** (that absence is the baseline, not 0%); the artifact states explicitly that the `line-rate`/`branch-rate` attributes were not used, citing issues #441 and #478; both harness-native and recomputed repository-wide figures are recorded as numbers
- [ ] [P0-T23] Read the `EXIT_CODE:` values recorded by [P0-T19] and [P0-T20] and write them as the two NAMED baseline values `ANALYZE_BASELINE_EXIT` and `NULLABLE_BASELINE_EXIT`, together with the pre-change analyzer diagnostic count and the pre-change `CS86xx` nullable diagnostic count, to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/toolchain-baseline-exit-codes.<timestamp>.md`
  - Acceptance: the artifact carries `Timestamp:`, `Command:` (naming CMD-ANALYZE and CMD-NULLABLE as the commands whose results are being transcribed), `EXIT_CODE:` for each, and `Output Summary:` stating both named values as literal integers plus both diagnostic counts. These two named values are the comparator for every later baseline-relative CMD-ANALYZE / CMD-NULLABLE gate in this plan ([P1-T21], [P1-T22], [P2-T4], [P4-T4], [P6-T25], [P7-T5], [P23-T2], [P23-T3]). No value here is asserted to be 0; both are recorded as observed

### Phase 1 — Partial Split, Exemption Removal, and Post-Removal Measurement

- [ ] [P1-T1] Change `public class QfcCollectionController : IQfcCollectionController` to `public partial class ...` at `QuickFiler/Controllers/QfcCollectionController.cs:22`, and delete `[ExcludeFromCodeCoverage]` at `:21`, `using System.Diagnostics.CodeAnalysis;` at `:4`, `using System.Net.NetworkInformation;` at `:6`, and `using System.Windows;` at `:10`
  - Acceptance: every `System.Drawing.Size`/`Point` construction in the file remains fully qualified as-is (no churn), and `MessageBox` at `:186` still resolves to `System.Windows.Forms.MessageBox`
- [ ] [P1-T2] Create `QuickFiler/Controllers/QfcCollectionController.State.cs` as `public partial class QfcCollectionController`, moving `_activeIndex`/`ActiveIndex` (86-91), `ActiveSelection` (92-96), `_token`/`Token` (98-103), `_tokenSource`/`TokenSource` (105-110), `_digitRefreshNeeded`/`_digits`/`Digits` (112-128), `SetVisualDigits` (130-146), `EmailsLoaded` (148), `EmailsToMove` (150), `ReadyForMove` (152-194), `_tlpLayout`/`TlpLayout` (196-231), `SafeSetTlpLayout` (233-238), `_itemGroups`/`ItemGroups` (240-247) out of the root file
- [ ] [P1-T3] Create `QuickFiler/Controllers/QfcCollectionController.LoadSync.cs`, moving `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` (253-266), `LoadControlsAndHandlers_01(IList<MailItem>, RowStyle, RowStyle)` (268-296), `LoadItemGroupsAndViewers_02` (740-754), `LoadConversationsAndFolders_04` (756-759), `LoadSequential_5` (798-825)
- [ ] [P1-T4] Create `QuickFiler/Controllers/QfcCollectionController.LoadAsync.cs`, moving `GetPartiallyInitializedHelperAsync` (298-318), `ValidateParams` (320-339), `LoadControlsAndHandlers_01Async(IList<MailItem>, ...)` (341-418), `LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, ...)` (420-507), `LoadSecondaryAsync` (525-579), `CreateEmptyKbdHandlerCharActions` (581-585), and deleting the commented-out block at 509-523
- [ ] [P1-T5] Create `QuickFiler/Controllers/QfcCollectionController.GroupFactory.cs`, moving `EncapsulateItemGroup` (607-633), `LoadItemToTlp` (904-949), `LoadItemViewer_03` (951-962), `InitializeGroup` (1849-1864), `AddItemGroup` (1924-1968)
- [ ] [P1-T6] Create `QuickFiler/Controllers/QfcCollectionController.Removal.cs`, moving `ActivateQueuedTlp` (859-863), `CacheItemGroupsForMove` (876-881), `ActivateQueuedItemGroups` (883-886), `SwapItemGroups` (888-896), `CacheMoveObjects` (898-902), `PopOutControlGroup` (964-974), `PopOutControlGroupAsync` (976-989), `RemoveControls` (991-1011), `CleanupBackground` (1013-1022), `RemoveControlsAsync` (1024-1044), `RemovedItemMonitor` (1046-1051), `RemoveSpecificControlGroup(string)` (1053-1058), `_removeGroupByEntryId`/`RemoveGroupByEntryId` (1060-1074), `RemoveBelowThresholdAsync` (1076-1097), `RemoveSpecificControlGroup(int)` (1099-1155), `removespecificcontrolgroupcounter` (1157), `RemoveSpecificControlGroupAsync` (1159-1248)
- [ ] [P1-T7] Create `QuickFiler/Controllers/QfcCollectionController.KeyboardWiring.cs`, moving `WireUpAsyncKeyboardHandler` (1275-1280), `RegisterAsyncKeyActions` (1282-1291), `RegisterAlwaysOnAsyncKeyActions` (1293-1305), `CustomReturnKeyHandler` (1307-1314), `AnyOpenDropDowns` (1316-1322), `RegisterNavigation` (1330-1341), `UnregisterNavigation` (1343-1356), `RegisterNavigationAsyncAction` (1358-1361), `GenerateStringKbdAction` (1363-1385)
- [ ] [P1-T8] Create `QuickFiler/Controllers/QfcCollectionController.Selection.cs`, moving `ActivateByIndex` (1391-1394), `ActivateByIndexAsync` (1396-1399), `ActivateBySelection` (1401-1424), `ActivateBySelectionAsync` (1426-1448), `ChangeByIndex` (1450-1464), `ChangeByIndexAsync` (1466-1484), `SelectNextItem` (1486-1496), `SelectNextItemAsync` (1498-1501), `SelectPreviousItem` (1503-1514), `SelectPreviousItemAsync` (1516-1519), `ScrollIntoView` (1521-1541), `ToggleOffActiveItem` (1667-1685), `ToggleOffActiveItemAsync` (1687-1702)
- [ ] [P1-T9] Create `QuickFiler/Controllers/QfcCollectionController.NavigationToggle.cs`, moving `ToggleExpansionStyle` (1543-1589), `ToggleExpansionStyleAsync` (1591-1598), `ToggleOffNavigation` (1600-1613), `ToggleOffNavigationAsync` (1615-1632), `ToggleOnNavigation` (1634-1646), `ToggleOnNavigationAsync` (1648-1665)
- [ ] [P1-T10] Create `QuickFiler/Controllers/QfcCollectionController.Conversation.cs`, moving `ChangeConversationSilently(int, bool)` (1714-1717), `ChangeConversationSilently(QfcItemGroup, bool)` (1725-1731), `ToggleGroupConv(string)` (1733-1766), `ToggleGroupConv(int, int)` (1768-1798), `ToggleUnGroupConv` (1808-1847), `EnumerateConversationMembers` (1875-1922), `PromoteFirstChild` (1970-1985)
- [ ] [P1-T11] Create `QuickFiler/Controllers/QfcCollectionController.Layout.cs`, moving `InsertItemGroups` (2004-2011), `EliminateSpaceForItems` (2013-2027), `MakeSpaceForItems` (2029-2042), `UpdateSelectionNumberForRemoval` (2044-2062), `RenumberGroups()` (2064-2070), `RenumberGroups(int)` (2072-2078), `ResetPanelHeightAsync` (2080-2090), `ResetPanelHeight` (2092-2107)
- [ ] [P1-T12] Create `QuickFiler/Controllers/QfcCollectionController.Theme.cs`, moving `SetupLightDark` (2113-2118), `DarkMode_CheckedChanged` (2120-2156), `SetDarkMode` (2158-2164), `SetLightMode` (2166-2172)
- [ ] [P1-T13] Create `QuickFiler/Controllers/QfcCollectionController.Move.cs`, moving `MoveEmailsAsync` (2206-2228), `TryMoveEmailByGroupIndexAsync` (2230-2234), `TryMoveEmailByGroupAsync` (2236-2258), `TryGetItemGroupByIndex` (2260-2270), `GetMoveDiagnostics` (2272-2328), `xComma` (2330-2345)
  - Acceptance: `xComma` retains the exact signature `public static string xComma(string)` on the `QfcCollectionController` type; `EmailsToMove` and `GetMoveDiagnostics` (including its `ref AppointmentItem` parameter) are byte-identical apart from indentation
- [ ] [P1-T14] Create `QuickFiler/Controllers/QfcCollectionController.LegacyLoadPaths.cs`, moving `WireUpKeyboardHandler` (1254-1273), `AnyOpenDropDownsAsync` (1324-1328), `LoadGroups_02cAsync` (587-605), `LoadGroups_02bAsync` (635-652), `LoadGroup_03bAsync` (654-738), `LoadConversationsAndFoldersAsync` (761-774), `LoadItemGroup` (776-796), `LoadSequentialAsync` (827-840), `LoadGroupSequential` (842-857), `CacheTlpForMove` (865-868), `SwapTlp` (870-874), `CaptureTlpTemplate` (1991-1996)
  - Acceptance: the file header states that no member in this file has a production caller anywhere in the repository and cites issue #468
- [ ] [P1-T15] Verify the retained root `QuickFiler/Controllers/QfcCollectionController.cs` contains exactly the `log4net` logger (24-26), the constructor (30-53), all private instance fields (60-80), `CleanupAsync` (2178-2190), and `Cleanup` (2192-2204), and that its `using` list omits `System.Diagnostics.CodeAnalysis`, `System.Net.NetworkInformation`, and `System.Windows`
- [ ] [P1-T16] Insert 13 `<Compile Include>` entries into `QuickFiler/QuickFiler.csproj` contiguously immediately after line 311 and before `<Compile Include="Controllers\EmailSorter.cs" />`, one per file created in [P1-T2]–[P1-T14], using the Edit tool or `perl -0777` with explicit `\r\n`
  - Acceptance: `git diff -- QuickFiler/QuickFiler.csproj` shows one contiguous addition-only hunk; a `\r$` count over the file equals its line count; no property change, no reference change, no reordering of unrelated entries
- [ ] [P1-T17] Append 13 ledger rows to `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` as one contiguous additive block, one per file created in [P1-T2]–[P1-T14], each row containing the file's repo-relative path as a literal substring and the bucket token `testable`, matching the row shape observed when the ledger was read in [P0-T11]
  - Acceptance: no assumption is made about the ledger's column names, count, ordering, or path separator; the diff is addition-only
- [ ] [P1-T18] Append or correct the `QuickFiler/Interfaces/IQfcCollectionController.cs` ledger row to bucket `interface-only / not-measured` only if [P0-T12] recorded `F1_LEDGER_RECONCILE`; otherwise record that F1's row was already correct and make no ledger edit for that file
- [ ] [P1-T19] Verify no production file created or modified in this phase exceeds 500 lines and record the line-count listing to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/file-sizes-post-split.<timestamp>.md`
- [ ] [P1-T20] Run CMD-FORMAT, confirm `dotnet tool run csharpier check .` exits 0 with no residual formatting diff, and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase1-format.<timestamp>.md`
  - Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CMD-FORMAT is not baseline-relative and must exit 0
- [ ] [P1-T21] Run CMD-ANALYZE, confirm it meets the baseline-relative gate defined in the Command Reference, and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase1-analyze.<timestamp>.md`
  - Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; `EXIT_CODE` equals `ANALYZE_BASELINE_EXIT` from [P0-T23]; the analyzer diagnostic set scoped to the files created or modified in this phase is empty
- [ ] [P1-T22] Run CMD-NULLABLE, confirm it meets the baseline-relative gate defined in the Command Reference, and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase1-nullable.<timestamp>.md`
  - Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; `EXIT_CODE` equals `NULLABLE_BASELINE_EXIT` from [P0-T23]; the `CS86xx` diagnostic set scoped to the files created or modified in this phase is empty
- [ ] [P1-T23] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionController` and confirm `QfcCollectionControllerTests` and `QfcCollectionControllerDarkModeTests` pass with zero source change to either file, recording the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/regression-testing/existing-tests-post-split.<timestamp>.md`
- [ ] [P1-T24] Verify the WORKING-TREE comparator `git diff --exit-code <merge-base> -- QuickFiler/Interfaces/IQfcCollectionController.cs` (single-dot, no `..HEAD`) returns 0 with no output, and that `git diff --stat <merge-base>` names no sibling-owned file from `spec.md` §2.5
  - Acceptance: the single-dot working-tree form is mandatory. No task between [P0-T15] and [P23-T6] commits anything, so a `<merge-base>..HEAD` comparator would compare committed history only, produce an empty diff trivially, and prove nothing. The working-tree form compares the merge-base against the files as they exist on disk right now, which is the state this task must verify
- [ ] [P1-T25] Run CMD-PREFLIGHT, then CMD-REBUILD, then CMD-COVERAGE, copy the produced report to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/coverage-post-exemption-removal.<timestamp>.cobertura.xml`, and write the companion command record to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/coverage-post-exemption-removal.<timestamp>.md`
  - Acceptance: identical command to [P0-T21]; the copied XML carries a `<sources>` element; the companion `.md` records `Timestamp:`, `Command:` (all three commands verbatim), `EXIT_CODE:`, and `Output Summary:` with numeric harness-native line and branch rates. The `.cobertura.xml` is a data artifact and cannot itself carry the four fields; the companion `.md` is the command-step artifact
- [ ] [P1-T26] Run CMD-RECOMPUTE against the post-removal XML for all 14 `QuickFiler\Controllers\QfcCollectionController*.cs` files and record the per-file line and branch figures — the true starting figure — to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/coverage-post-exemption-removal-recomputed.<timestamp>.md`
  - Acceptance: the artifact states in prose that a transient drop in the QuickFiler package line rate is EXPECTED at this moment because 14 files entered the denominator at once before any new test landed, so a reviewer does not read it as a regression; it also states that the `line-rate`/`branch-rate` attributes were not used, citing #441 and #478

### Phase 2 — Seam File IQfcTlpSurface

- [ ] [P2-T1] Create `QuickFiler/Controllers/IQfcTlpSurface.cs` declaring `internal interface IQfcTlpSurface` in namespace `QuickFiler.Controllers`, with members covering every B-CTRL-R site on `_itemTlp`, `_itemPanel`, and `_itemTlpToMove`: suspend/resume layout, `InvokeRequired`, `Invoke`, `SetCellPosition`, `SetColumnSpan`, row-style read/write, `MinimumSize`, `Size`, `Height`, parent height, panel `Top` and `Bottom` reads, `AutoScrollPosition`, and the `InsertSpecificRow`/`RemoveSpecificRow` operations used at 999, 1034, 1121, 1183, 2015, 2036
  - Acceptance: file is <= 55 lines; no member is added to `QuickFiler/Interfaces/IQfcCollectionController.cs`
- [ ] [P2-T2] Insert `<Compile Include="Controllers\IQfcTlpSurface.cs" />` into `QuickFiler/QuickFiler.csproj` immediately after the last entry added by [P1-T16], preserving CRLF, so the whole added region remains one contiguous block
- [ ] [P2-T3] Append a ledger row for `QuickFiler/Controllers/IQfcTlpSurface.cs` to the contiguous block started in [P1-T17], using the interface-file bucket token observed in [P0-T12], defaulting to `interface-only` if none was observed
- [ ] [P2-T4] Run CMD-ANALYZE and CMD-NULLABLE with the interface declared and not yet implemented, confirm both meet the baseline-relative gate defined in the Command Reference, and record both results to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase2-analyze-nullable.<timestamp>.md`
  - Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` for EACH of the two commands; CMD-ANALYZE's `EXIT_CODE` equals `ANALYZE_BASELINE_EXIT` and CMD-NULLABLE's equals `NULLABLE_BASELINE_EXIT` from [P0-T23]; the analyzer and `CS86xx` diagnostic sets scoped to `QuickFiler/Controllers/IQfcTlpSurface.cs` are both empty

### Phase 3 — Seam File QfcTlpSurface

- [ ] [P3-T1] Create `QuickFiler/Controllers/QfcTlpSurface.cs` implementing `IQfcTlpSurface` as a thin adapter that resolves its target panels through ACCESSOR DELEGATES rather than by capturing panel instances: the constructor takes `Func<TableLayoutPanel> itemTlpAccessor`, `Func<Panel> itemPanelAccessor`, and `Func<TableLayoutPanel> itemTlpToMoveAccessor`, stores the three delegates in `readonly` fields, and every member invokes the relevant accessor at CALL TIME to obtain the current panel before forwarding; `InsertSpecificRow`/`RemoveSpecificRow` delegate to `UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs:13,55`
  - Acceptance: file is <= 130 lines; the adapter holds no logic beyond invoking an accessor, null-guarding its result, and forwarding. **No panel instance is captured in a field.** `_itemTlp` is REASSIGNED at `QuickFiler/Controllers/QfcCollectionController.cs:862` (`ActivateQueuedTlp`) and nulled at `:2188` and `:2202`, and `_itemTlpToMove` is assigned at `:867` and `:900`; a constructor-time captured reference would go stale across the page swap and across cleanup, which would break the bit-identical-in-effect requirement of [P6-T1] and contradict [P6-T21] and [P6-T22]. The accessor form is robust to every reassignment site without the plan having to enumerate them, which is why it is chosen over rebuilding the adapter at each assignment site
  - Acceptance: each accessor result is null-guarded at call time; a null panel produces the same observable no-op or same exception the pre-seam direct field access produced, never a `NullReferenceException` introduced by the adapter
- [ ] [P3-T2] Insert `<Compile Include="Controllers\QfcTlpSurface.cs" />` into `QuickFiler/QuickFiler.csproj` immediately after the [P2-T2] entry, preserving CRLF
- [ ] [P3-T3] Append a ledger row for `QuickFiler/Controllers/QfcTlpSurface.cs` with the bucket token `testable` and the `>= 90%` new-file target to the contiguous ledger block
- [ ] [P3-T4] Create `QuickFiler.Test/Controllers/QfcTlpSurface.StaTests.cs` using the manual STA-thread helper pattern at `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:302-317` with `ShutdownDispatcher` at `:323-326`, and add `<Compile Include="Controllers\QfcTlpSurface.StaTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
  - Acceptance: every control is created in memory and never shown; no new NuGet package and no `packages.config` edit; the class docstring states why no seam can isolate `TableLayoutPanel` member access
  - Acceptance: the arrange helper constructs `QfcTlpSurface` with the three accessor delegates of [P3-T1], each closing over a mutable local holder so a test can re-point the holder and prove call-time resolution
- [ ] [P3-T5] Add a test asserting suspend and resume layout forward to the `TableLayoutPanel` returned by the accessor at CALL TIME, re-pointing the accessor's holder to a second in-memory `TableLayoutPanel` between the suspend and the resume and asserting the resume landed on the second panel, proving the adapter holds no stale captured reference across the `:862`/`:2188` reassignment pattern
- [ ] [P3-T6] Add a test asserting `InvokeRequired` is reported from the accessor-resolved panel and that `Invoke` executes the supplied delegate
- [ ] [P3-T7] Add a test asserting `SetCellPosition` and `SetColumnSpan` place a control at the requested cell
- [ ] [P3-T8] Add a test asserting the row-style read and write path returns and applies a `RowStyle`
- [ ] [P3-T9] Add a test asserting `MinimumSize`, `Size`, and `Height` round-trip through the adapter
- [ ] [P3-T10] Add a test asserting the parent-height read returns the accessor-resolved panel's `Parent.Height`, including the `Parent == null` guard
- [ ] [P3-T11] Add a test asserting the accessor-resolved `Panel` geometry surface round-trips through the adapter: `AutoScrollPosition` reads back the value written through the adapter, and the panel `Top` and `Bottom` reads return the accessor-resolved panel's own `Top` and `Bottom` (the three members `ScrollIntoView` consumes per [P14-T20])
- [ ] [P3-T12] Add a test asserting `InsertSpecificRow` adds exactly one row at the requested index on an in-memory `TableLayoutPanel`, per the precedent at `UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs:11-23`
- [ ] [P3-T13] Add a test asserting `RemoveSpecificRow` removes exactly one row at the requested index, per the precedent at `UtilitiesCS.Test/HelperClasses/WindowsForms/ScreenAndTableLayoutTests.cs:41-54`
- [ ] [P3-T14] Verify `QuickFiler.Test/Controllers/QfcTlpSurface.StaTests.cs` is <= 500 lines; if it would exceed, split into `QuickFiler.Test/Controllers/QfcTlpSurface.StaTests.Part2.cs` with a second `[TestClass]` and add its `<Compile Include>` entry
- [ ] [P3-T15] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcTlpSurface` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase3-scoped-tests.<timestamp>.md`

### Phase 4 — Seam File IQfcItemViewerSurface

- [ ] [P4-T1] Create `QuickFiler/Controllers/IQfcItemViewerSurface.cs` declaring `internal interface IQfcItemViewerSurface` in namespace `QuickFiler.Controllers` as a **STATELESS FACADE**: every member takes the target `ItemViewer` as its FIRST parameter (for example `void SetItemNumberText(ItemViewer viewer, string text);`, `void FocusSubject(ItemViewer viewer);`, `void SetConversationChecked(ItemViewer viewer, bool value);`, `void SetParent(ItemViewer viewer, Control parent);`, `void SetDock(ItemViewer viewer, DockStyle dock);`, `void SetAutoSize(ItemViewer viewer, bool value);`, `void SetBorderStyle(ItemViewer viewer, BorderStyle style);`), covering the `ItemViewer` touches at 141-142, 926-947, 1417, 1729, and 1976-1980: item-number text write, subject focus, conversation-menu-item checked state with event suppression, parent assignment, `Dock`, `AutoSize`, and `BorderStyle`
  - Acceptance: **no member is parameterless with respect to the viewer** — the interface declares no instance state and wraps no single `ItemViewer`. Every `ItemViewer` touch this plan names targets a PER-GROUP `grp.ItemViewer` (141-142 sits inside the `_itemGroups.ForEach`), so a single instance-wrapping adapter field cannot serve them. The stateless facade is the chosen shape; a `Func<ItemViewer, IQfcItemViewerSurface>` per-viewer factory is explicitly NOT used
  - Acceptance: file is <= 45 lines; no member is added to `QuickFiler/Interfaces/IQfcCollectionController.cs`
- [ ] [P4-T2] Insert `<Compile Include="Controllers\IQfcItemViewerSurface.cs" />` into `QuickFiler/QuickFiler.csproj` immediately after the [P3-T2] entry, preserving CRLF
- [ ] [P4-T3] Append a ledger row for `QuickFiler/Controllers/IQfcItemViewerSurface.cs` to the contiguous ledger block, using the interface-file bucket token observed in [P0-T12], defaulting to `interface-only` if none was observed
- [ ] [P4-T4] Run CMD-ANALYZE and CMD-NULLABLE, confirm both meet the baseline-relative gate defined in the Command Reference, and record both results to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase4-analyze-nullable.<timestamp>.md`
  - Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` for EACH of the two commands; CMD-ANALYZE's `EXIT_CODE` equals `ANALYZE_BASELINE_EXIT` and CMD-NULLABLE's equals `NULLABLE_BASELINE_EXIT` from [P0-T23]; the analyzer and `CS86xx` diagnostic sets scoped to `QuickFiler/Controllers/IQfcItemViewerSurface.cs` are both empty

### Phase 5 — Seam File QfcItemViewerSurface

- [ ] [P5-T1] Create `QuickFiler/Controllers/QfcItemViewerSurface.cs` implementing `IQfcItemViewerSurface` as a **STATELESS** facade: the class declares NO fields and NO constructor parameters, and each member operates solely on the `ItemViewer` supplied as its first parameter, forwarding to the `ItemViewer` members named in [P4-T1]
  - Acceptance: file is <= 75 lines; the class has zero instance fields; the facade holds no logic beyond null-guarding the supplied viewer and its named child controls and forwarding; a single shared instance is safe to reuse across every `_itemGroups` element because it carries no per-viewer state
- [ ] [P5-T2] Insert `<Compile Include="Controllers\QfcItemViewerSurface.cs" />` into `QuickFiler/QuickFiler.csproj` immediately after the [P4-T2] entry, preserving CRLF, completing the contiguous 17-entry addition block
  - Acceptance: `git diff -- QuickFiler/QuickFiler.csproj` shows exactly one contiguous addition-only hunk of 17 lines (18 if Phase 7 is taken), inserted immediately after the pre-existing `Controllers\QfcCollectionController.cs` entry and before `Controllers\EmailSorter.cs`
- [ ] [P5-T3] Append a ledger row for `QuickFiler/Controllers/QfcItemViewerSurface.cs` with the bucket token `testable` and the `>= 90%` new-file target, completing the contiguous 17-row ledger block
- [ ] [P5-T4] Create `QuickFiler.Test/Controllers/QfcItemViewerSurface.StaTests.cs` using the manual STA-thread helper pattern, constructing the `ItemViewer` with `FormatterServices.GetUninitializedObject` per the precedent at `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:97,128,143,208,280`, and add `<Compile Include="Controllers\QfcItemViewerSurface.StaTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
  - Acceptance: no live form is shown, no popup is raised; the class docstring states why no seam can isolate `ItemViewer` member access
  - Acceptance: because the facade is stateless, every test constructs ONE `QfcItemViewerSurface` and passes the target `ItemViewer` per call
- [ ] [P5-T5] Add a test asserting the item-number text write forwards to the supplied viewer's `LblItemNumber.Text` when the label is present, and that a second call with a DIFFERENT `ItemViewer` writes to that second viewer's label, proving the facade carries no per-viewer state
- [ ] [P5-T6] Add a test asserting the subject-focus call forwards to the supplied viewer's `LblSubject.Focus()` when the label is present
- [ ] [P5-T7] Add a test asserting the conversation-menu-item checked write suppresses and restores events around the write on the supplied viewer
- [ ] [P5-T8] Add a test asserting parent assignment, `Dock`, `AutoSize`, and `BorderStyle` round-trip through the facade for the supplied viewer
- [ ] [P5-T9] Add a test asserting the facade's null-guard behavior for each member reachable when `LblItemNumber`, `LblSubject`, or `ConversationMenuItem` is null on an uninitialized supplied `ItemViewer`
- [ ] [P5-T10] Verify `QuickFiler.Test/Controllers/QfcItemViewerSurface.StaTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with its own `[TestClass]` and `<Compile Include>` entry
- [ ] [P5-T11] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcItemViewerSurface` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase5-scoped-tests.<timestamp>.md`
- [ ] [P5-T12] If and only if a full `new ItemViewer()` proves unsafe or slow in the runner and the >= 90% gate for `QuickFiler/Controllers/QfcItemViewerSurface.cs` is unreachable, prepare the single permitted ratified-exemption request for that one file with a file-specific rationale meeting one of the three exemption grounds in `docs/features/epics/quickfiler-per-file-coverage/epic.md`, the EXACT uncovered member list, and a ledger entry, recorded to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/other/exemption-request-qfcitemviewersurface.<timestamp>.md`
  - Acceptance: a blanket file exemption is not acceptable, and a blanket re-exemption of `QuickFiler/Controllers/QfcCollectionController.cs` is not acceptable under any circumstances; if the gate is met, record that no exemption is requested

### Phase 6 — Root File Seam Declaration, Rewiring, and Construction Tests

Each task in this phase declares a seam field AND rewires every one of its consumption sites in the
same task, so no phase or task boundary leaves an assigned-but-never-read private field for the
analyzer or nullable build to reject.

- [ ] [P6-T1] Declare `private readonly IQfcTlpSurface _tlpSurface` and `private readonly IQfcItemViewerSurface _viewerSurface` (seam **S1**) on the root `QuickFiler/Controllers/QfcCollectionController.cs`, add both as optional trailing constructor parameters, and rewire all ~45 B-CTRL-R sites listed in `research/qfc-collection-controller.md` §B0 across `State.cs`, `GroupFactory.cs`, `Removal.cs`, `Selection.cs`, `NavigationToggle.cs`, `Conversation.cs`, `Layout.cs`, and `LegacyLoadPaths.cs`
  - Acceptance (TLP half): **only `_tlpSurface` is constructed over the panel fields**, and it is constructed as `new QfcTlpSurface(() => _itemTlp, () => _itemPanel, () => _itemTlpToMove)` — three accessor lambdas per [P3-T1], never captured panel instances. This is required because `_itemTlp` is reassigned at `:862` and nulled at `:2188`/`:2202` and `_itemTlpToMove` is assigned at `:867` and `:900`; a snapshot would go stale across the page swap and contradict [P6-T21] and [P6-T22]
  - Acceptance (viewer half): `_viewerSurface`'s production default is the parameterless `new QfcItemViewerSurface()` — the STATELESS facade of [P4-T1] and [P5-T1]. It is NOT constructed over `_itemTlp`, `_itemPanel`, or `_itemTlpToMove`: those are `TableLayoutPanel`/`Panel` fields while the facade adapts `ItemViewer` members, and every rewired viewer touch (141-142 inside the `_itemGroups.ForEach`, 926-947, 1417, 1729, 1976-1980) targets a PER-GROUP `grp.ItemViewer`. Each rewired call site therefore passes its own `grp.ItemViewer` as the first argument; a single instance-wrapping adapter field could not serve them
  - Acceptance: the production default is bit-identical in effect to the code it replaces; the interfaces stay `internal` and the fields stay `private`
- [ ] [P6-T2] Declare `private readonly UtilitiesCS.Threading.IUiDispatcher _uiDispatcher` (seam **S2**, mandatory) with a `WpfUiDispatcher` production default and an optional trailing constructor parameter, and rewire all seven static `UiThread.Dispatcher` sites at 1195, 1226, 1472, 1482, 1500, 1518, 1595
  - Acceptance: no `UiThread.Dispatcher` reference remains in any `QfcCollectionController*.cs` file; the shape matches the sibling at `QuickFiler/Controllers/QfcItemController.Initialization.cs:38`
- [ ] [P6-T3] Declare `private readonly Func<CancellationToken, ItemViewer> _itemViewerFactory` (seam **S3**) defaulting to the existing `ItemViewerQueue.Dequeue` behavior, and rewire both sites at 617 and 958
- [ ] [P6-T4] Declare `private readonly Func<QfcItemGroup, int, int, TlpCellStates, string, IQfcItemController> _itemControllerFactory` (seam **S4**) defaulting to `new QfcItemController(...)`, and rewire all six sites at 620-630, 681-690, 778-787, 803-812, 844-853, 1853-1862
- [ ] [P6-T5] Declare `private readonly Func<MailItem, Task<MailItemHelper>> _helperFactory` (seam **S5**) defaulting to `MailItemHelper.FromMailItemAsync`, and rewire the site at 300-305
- [ ] [P6-T6] Declare `private readonly Action<string, string> _showError` (seam **S6**) defaulting to the EXPLICIT LAMBDA `(text, caption) => MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error)`, and rewire the modal call inside the `ReadyForMove` getter at 186-191
  - Acceptance: the default is the lambda above, NOT the method group `MessageBox.Show`. The live call at `:186-191` is the four-argument `MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error)`, which returns `DialogResult` and takes four parameters, so no `MessageBox.Show` overload is assignable to `Action<string, string>`; the lambda both compiles and preserves the buttons and icon bit-identically
  - Acceptance: because the default is a lambda rather than a named method, [P6-T20] asserts `NotBeSameAs` against a sentinel for this seam and never asserts `Method.Name`
  - Acceptance: only the seam half of #474/#469's modal-getter finding is taken; the modal-in-a-getter design defect is NOT corrected
- [ ] [P6-T7] Declare `private readonly Func<MailItem, bool, Task> _popOutAsync` (seam **S7**) defaulting to the existing `new EfcHomeController(...)` construction, and rewire both sites at 972-973 and 986-988
- [ ] [P6-T8] Declare `private readonly Func<Task> _skipGroupAsync` (seam **S8**) defaulting to `((QfcFormController)_parent).SkipGroupAsync()`, and rewire the site at 1232
  - Acceptance: only the seam half of #474's concrete-downcast finding is taken; the downcast design defect is NOT corrected
- [ ] [P6-T9] Convert the hard-wired `IEmailMoveMonitor` field initializer at `:78` (seam **S9**) into an optional trailing constructor parameter with the same production default
  - Acceptance: the field keeps the exact name `_moveMonitor` because `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:359` injects it by reflection
- [ ] [P6-T10] Confirm the already-present `Func<string, Task> _removeGroupByEntryId` (seam **S10**, at `:1067`) is preserved verbatim in `QuickFiler/Controllers/QfcCollectionController.Removal.cs` and remains injectable by the existing tests at `QfcCollectionControllerTests.cs:185-288`
- [ ] [P6-T11] Verify all three production construction sites `QuickFiler/Controllers/QfcFormController.Actions.cs:49`, `:83`, `:139` compile unchanged against the new optional trailing parameters, and that no F6-owned file has any diff
- [ ] [P6-T12] Verify `QuickFiler/Interfaces/IQfcCollectionController.cs` still has a zero diff after all seam work, and that `QuickFiler/Controllers/QfcItemGroup.cs` (F2-owned) has no diff
- [ ] [P6-T13] Create `QuickFiler.Test/Controllers/QfcCollectionControllerTests.Construction.cs` with a `[TestClass]` and a `CreateController` arrange helper extending the shape at `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:31-60`, and add `<Compile Include="Controllers\QfcCollectionControllerTests.Construction.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
  - Acceptance: no test is added to `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (exactly 500 lines, zero headroom)
- [ ] [P6-T14] Add a test asserting the constructor stores each collaborator, verified by reflection on `_globals`, `_formViewer`, `_homeController`, `_parent`, `_tlpStates`, `_token`, `_tokenSource`, and `_initType`
- [ ] [P6-T15] Add a test asserting the constructor reads `_formViewer.L1v0L2L3v_TableLayout` into `_itemTlp` and `_formViewer.L1v0L2_PanelMain` into `_itemPanel` (44-45)
- [ ] [P6-T16] Add a test asserting the constructor takes `_kbdHandler` from `_homeController.KeyboardHandler` (49)
- [ ] [P6-T17] Add a test asserting the constructor calls `SetupLightDark(_globals.Ol.DarkMode)` for `DarkMode == true` (52)
- [ ] [P6-T18] Add a test asserting the constructor calls `SetupLightDark(_globals.Ol.DarkMode)` for `DarkMode == false` (52)
- [ ] [P6-T19] Add a test asserting that when no seam is injected, each of the ten seam fields S1-S10 is non-null after construction
- [ ] [P6-T20] Add a test asserting that when a seam instance is injected for each of S1-S10, the injected instance is the one stored on the field
  - Acceptance: for any seam whose production default is a lambda rather than a named method, assert `NotBeSameAs` against a sentinel rather than asserting `Method.Name`
- [ ] [P6-T21] Add a test asserting `Cleanup` nulls `_formViewer`, `_globals`, `_parent`, `_itemTlp`, and `_itemGroups` and unsubscribes `PropertyChanged`
- [ ] [P6-T22] Add a test asserting `CleanupAsync` nulls the same fields and unsubscribes `PropertyChanged`
- [ ] [P6-T23] Add a test covering the `_globals?.Ol is null` branch in both `CleanupAsync` (2182) and `Cleanup` (2196)
- [ ] [P6-T24] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerTests.Construction.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with its own `[TestClass]` and `<Compile Include>` entry
- [ ] [P6-T25] Run CMD-FORMAT, CMD-ANALYZE, and CMD-NULLABLE after the full seam rewiring, confirm CMD-FORMAT exits 0 and that CMD-ANALYZE and CMD-NULLABLE each meet the baseline-relative gate defined in the Command Reference, and record all three results to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase6-toolchain.<timestamp>.md`
  - Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` for EACH of the three commands; CMD-FORMAT exits 0 unconditionally; CMD-ANALYZE's `EXIT_CODE` equals `ANALYZE_BASELINE_EXIT` and CMD-NULLABLE's equals `NULLABLE_BASELINE_EXIT` from [P0-T23]; the analyzer and `CS86xx` diagnostic sets scoped to the files touched by this feature are both empty
- [ ] [P6-T26] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionController` and confirm `QfcCollectionControllerTests` and `QfcCollectionControllerDarkModeTests` still pass with zero source change to either file, recording the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/regression-testing/existing-tests-post-seams.<timestamp>.md`
- [ ] [P6-T27] Verify no production file in scope exceeds 500 lines after seam work and record the line-count listing to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/file-sizes-post-seams.<timestamp>.md`
  - Acceptance: this listing supplies the Phase 7 trigger value for `QuickFiler/Controllers/QfcCollectionController.Removal.cs`

### Phase 7 — Contingency Split of Removal into RemoveGroup

**Trigger (evaluate against the [P6-T27] listing):** execute this phase if and only if
`QuickFiler/Controllers/QfcCollectionController.Removal.cs` exceeds **430 lines** after the Phase 6
seam rewiring. If the file is at or below 430 lines, record the measured line count and mark this
phase not-taken with that number as the evidence; the csproj and ledger additions remain 17.

- [ ] [P7-T1] Record the measured post-seam line count of `QuickFiler/Controllers/QfcCollectionController.Removal.cs` and the taken/not-taken decision against the 430-line trigger to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/other/removal-split-decision.<timestamp>.md`
- [ ] [P7-T2] If taken, create `QuickFiler/Controllers/QfcCollectionController.RemoveGroup.cs` and move `RemoveSpecificControlGroup(int)`, `RemoveSpecificControlGroupAsync`, and the `removespecificcontrolgroupcounter` static field (current lines 1099-1248) into it
- [ ] [P7-T3] If taken, insert `<Compile Include="Controllers\QfcCollectionController.RemoveGroup.cs" />` into `QuickFiler/QuickFiler.csproj` inside the existing contiguous addition block, preserving CRLF, taking the block to 18 entries
- [ ] [P7-T4] If taken, append an 18th ledger row for `QuickFiler/Controllers/QfcCollectionController.RemoveGroup.cs` with the bucket token `testable` and the `>= 90%` new-file target, inside the existing contiguous ledger block
- [ ] [P7-T5] If taken, verify both `Removal.cs` and `RemoveGroup.cs` are under 500 lines, run CMD-FORMAT, CMD-ANALYZE, and CMD-NULLABLE confirming CMD-FORMAT exits 0 and that CMD-ANALYZE and CMD-NULLABLE each meet the baseline-relative gate defined in the Command Reference, and record all three results to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase7-toolchain.<timestamp>.md`
  - Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` for EACH of the three commands; CMD-ANALYZE's `EXIT_CODE` equals `ANALYZE_BASELINE_EXIT` and CMD-NULLABLE's equals `NULLABLE_BASELINE_EXIT` from [P0-T23]; the diagnostic sets scoped to `Removal.cs` and `RemoveGroup.cs` are empty. If Phase 7 is not taken, [P7-T1] records not-taken and this task records `NOT TAKEN` with the [P7-T1] artifact as its authorization
- [ ] [P7-T6] If taken, update the Phase 12 measurement target list and the Phase 21 per-file table to include `QuickFiler\Controllers\QfcCollectionController.RemoveGroup.cs`, and note that the `[DoNotParallelize]` test class in Phase 12 now measures that file rather than `Removal.cs` for the counter lines

### Phase 8 — Coverage for QfcCollectionController.State.cs

- [ ] [P8-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerStateTests.cs` with a `[TestClass]`, MSTest/Moq/FluentAssertions in Arrange-Act-Assert form using mocked `IQfcTlpSurface` and `IQfcItemViewerSurface`, and add `<Compile Include="Controllers\QfcCollectionControllerStateTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
- [ ] [P8-T2] Add a test asserting `ActiveIndex` and `ActiveSelection` round-trip and hold the `ActiveSelection == ActiveIndex + 1` relationship (86-96)
- [ ] [P8-T3] Add a test asserting `Token` and `TokenSource` round-trip (98-110)
- [ ] [P8-T4] Add a test asserting `Digits` returns 1 when `_itemGroups.Count < 10`
- [ ] [P8-T5] Add a test asserting `Digits` returns 2 when `_itemGroups.Count >= 10`
- [ ] [P8-T6] Add a test asserting `Digits` returns 1 when `_itemGroups` is null (119)
- [ ] [P8-T7] Add a test asserting the `Digits` change path sets `_digitRefreshNeeded` (122-124) and the no-change path leaves it unset
- [ ] [P8-T8] Add a test asserting `SetVisualDigits` skips the loop when `EmailsLoaded == 0` (132)
- [ ] [P8-T9] Add a test asserting `SetVisualDigits` uses the format string `"0"` for 1 digit and `"00"` for 2 digits (134-137) and that the item-viewer surface receives the formatted text per group
- [ ] [P8-T10] Add a test asserting `SetVisualDigits` clears `_digitRefreshNeeded` (145)
- [ ] [P8-T11] Add a test asserting `EmailsLoaded` and `EmailsToMove` return correctly for null and non-null backing state (148, 150)
- [ ] [P8-T12] Add a test asserting `ReadyForMove` returns `true` when every group is assigned and that `_showError` is never invoked
- [ ] [P8-T13] Add a test asserting `ReadyForMove` returns `false` and invokes `_showError` exactly once when one group has a null `SelectedFolder`
- [ ] [P8-T14] Add a test asserting `ReadyForMove` returns `false` for each of the three header sentinels at 164-168
- [ ] [P8-T15] Add a test asserting the `ReadyForMove` notification text passed to `_showError` contains the item number, the date, and the subject (176-182)
- [ ] [P8-T16] Add a test asserting `TlpLayout` is a no-op when the assigned value equals the current value (205)
- [ ] [P8-T17] Add a test asserting `TlpLayout = true` resumes layout through the TLP surface
- [ ] [P8-T18] Add a test asserting `TlpLayout = false` suspends layout through the TLP surface
- [ ] [P8-T19] Add a test covering both `InvokeRequired` branches of `TlpLayout` (209-227)
- [ ] [P8-T20] Add a test asserting `SafeSetTlpLayout` returns the PREVIOUS value (233-238)
- [ ] [P8-T21] Add a test asserting `ItemGroups` get and set round-trip (241-247)
- [ ] [P8-T22] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerStateTests.cs` is <= 500 lines; if it would exceed, split into `QfcCollectionControllerStateTests.Part2.cs` with a second `[TestClass]` and add its `<Compile Include>` entry
- [ ] [P8-T23] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerStateTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase8-scoped-tests.<timestamp>.md`

### Phase 9 — Coverage for QfcCollectionController.LoadSync.cs

- [ ] [P9-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerLoadSyncTests.cs` with a `[TestClass]` and MSTest/Moq/FluentAssertions arrange helper, and add `<Compile Include="Controllers\QfcCollectionControllerLoadSyncTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
- [ ] [P9-T2] Add a test asserting `LoadControlsAndHandlers_01(tlp, groups)` hooks every incoming group's mail into the move monitor (255-257)
- [ ] [P9-T3] Add a test asserting `LoadControlsAndHandlers_01(tlp, groups)` suspends and resumes the viewer and routes through `SwapItemGroups`
- [ ] [P9-T4] Add a test asserting `LoadControlsAndHandlers_01(tlp, groups)` sets `ActiveIndex = -1` (265)
- [ ] [P9-T5] Add a test asserting `LoadControlsAndHandlers_01(items, template, templateExpanded)` saves `_template` and `_templateExpanded` (279-280)
- [ ] [P9-T6] Add a test asserting that overload hooks each mail item into the move monitor (283-285)
- [ ] [P9-T7] Add a test asserting that overload calls `LoadItemGroupsAndViewers_02` and `LoadConversationsAndFolders_04`
- [ ] [P9-T8] Add a test asserting that overload sets `_formViewer.WindowState = Maximized` (289) and restores `TlpLayout`
- [ ] [P9-T9] Add a test asserting `LoadItemGroupsAndViewers_02` creates exactly one group per item and resets both `CharActions` collections (743-744)
- [ ] [P9-T10] Add a test asserting `LoadItemGroupsAndViewers_02` returns an empty list for empty input
- [ ] [P9-T11] Add a test asserting `LoadConversationsAndFolders_04` fans out once per group (756-759)
- [ ] [P9-T12] Add a test asserting `LoadSequential_5` applies 1-based `++i` numbering (808)
- [ ] [P9-T13] Add a test covering both the dark and the light branch of `LoadSequential_5` (816-823)
- [ ] [P9-T14] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerLoadSyncTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P9-T15] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerLoadSyncTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase9-scoped-tests.<timestamp>.md`

### Phase 10 — Coverage for QfcCollectionController.LoadAsync.cs

- [ ] [P10-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerLoadAsyncTests.cs` with a `[TestClass]`, seam **S5** returning a completed `MailItemHelper` mock in the shape at `QfcCollectionControllerTests.cs:40-44`, and a `IQfcFormViewer.UiSyncContext` setup returning a real `SynchronizationContext` whose `Post` executes inline; add `<Compile Include="Controllers\QfcCollectionControllerLoadAsyncTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
  - Acceptance: a Moq default of `null` for `UiSyncContext` would throw `ArgumentNullException` from `SynchronizationContextAwaiter` (`UtilitiesCS/Threading/UiThread.cs:93-96`), so the explicit setup is mandatory
- [ ] [P10-T2] Add a test asserting `ValidateParams` throws when `items` is null
- [ ] [P10-T3] Add a test asserting `ValidateParams` throws when `template` is null
- [ ] [P10-T4] Add a test asserting `ValidateParams` throws when `templateExpanded` is null
- [ ] [P10-T5] Add a test asserting `ValidateParams` throws `InvalidOperationException` whose message names `LoadControlsAndHandlers_01Async` when `InvokeRequired == true` (334)
- [ ] [P10-T6] Add a test asserting `ValidateParams` throws `OperationCanceledException` for an already-cancelled token (338)
- [ ] [P10-T7] Add a test asserting `GetPartiallyInitializedHelperAsync` throws for a null `mailItem`
- [ ] [P10-T8] Add a test asserting `GetPartiallyInitializedHelperAsync` returns the value produced by the **S5** factory
- [ ] [P10-T9] Add a test asserting the seven helper property touches at 308-314 do not throw against a loose mock
- [ ] [P10-T10] Add a test covering the digits 1-versus-2 boundary at 10 items in `LoadControlsAndHandlers_01Async(IList<MailItem>, ...)` (373)
- [ ] [P10-T11] Add a test asserting that overload creates exactly one group per item
- [ ] [P10-T12] Add a test asserting `InitializeGraphicsAsync` is awaited exactly once per group (384)
- [ ] [P10-T13] Add a test asserting helper-to-group correlation is performed by `EntryID` (392)
- [ ] [P10-T14] Add a test asserting `BackgroundLoadingTasks` is empty after the load completes (399)
- [ ] [P10-T15] Add a test asserting `WireUpAsyncKeyboardHandler` is called (403)
- [ ] [P10-T16] Add a test covering both `InvokeRequired` branches on resume (407-414)
- [ ] [P10-T17] Add a test asserting `LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, ...)` carries `PredeterminedFolder` through to `EncapsulateItemGroup` (471) as a real call-path assertion, generalizing the shape-only assertion at `QfcCollectionControllerTests.cs:303-326`
- [ ] [P10-T18] Add a test asserting `LoadSecondaryAsync` throws for an already-cancelled token (528)
- [ ] [P10-T19] Add a test asserting conversation-task completion in `LoadSecondaryAsync` calls `RenderConversationCount` (565)
- [ ] [P10-T20] Add a test asserting folder-task completion in `LoadSecondaryAsync` calls `AssignFolderComboBox` (572)
- [ ] [P10-T21] Add a test asserting a foreign completed task causes `LoadSecondaryAsync` to throw `InvalidOperationException` (576), reached by injecting a group list such that neither list contains the completed task
- [ ] [P10-T22] Add a test asserting `CreateEmptyKbdHandlerCharActions` replaces both `CharActions` collections (583-584)
- [ ] [P10-T23] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerLoadAsyncTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P10-T24] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerLoadAsyncTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase10-scoped-tests.<timestamp>.md`

### Phase 11 — Coverage for QfcCollectionController.GroupFactory.cs

- [ ] [P11-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerGroupFactoryTests.cs` with a `[TestClass]` driving seams **S3** and **S4** so no `ItemViewerQueue` static mutation is required, and add `<Compile Include="Controllers\QfcCollectionControllerGroupFactoryTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
- [ ] [P11-T2] Add a test asserting `EncapsulateItemGroup` carries `PredeterminedFolder` onto the created group (616)
- [ ] [P11-T3] Add a test asserting `EncapsulateItemGroup` takes its viewer from the **S3** factory rather than `ItemViewerQueue.Dequeue`
- [ ] [P11-T4] Add a test asserting `EncapsulateItemGroup` invokes `LoadItemToTlp` with `(i, template, true, 0)` (618)
- [ ] [P11-T5] Add a test asserting `EncapsulateItemGroup` builds the controller with `viewerPosition == i + 1` (625)
- [ ] [P11-T6] Add a test asserting `EncapsulateItemGroup` propagates `Token` to the created controller (631)
- [ ] [P11-T7] Add a test asserting `LoadItemViewer_03` returns the dequeued viewer and places it (958-961)
- [ ] [P11-T8] Add a test asserting `LoadItemToTlp` forwards the `columnNumber == 0` path to the TLP surface (912-942)
- [ ] [P11-T9] Add a test asserting `LoadItemToTlp` forwards the `columnNumber != 0` path to the TLP surface
- [ ] [P11-T10] Add a test covering the third `LoadItemToTlp` path identified in the branch-attention list (912-942) so all three paths are exercised
- [ ] [P11-T11] Add a test asserting `InitializeGroup(child: true)` places the group in column 1 and sets `IsChild == true` (1851, 1863)
- [ ] [P11-T12] Add a test asserting `InitializeGroup(child: false)` places the group in column 0
- [ ] [P11-T13] Add a test asserting `AddItemGroup` unregisters navigation before mutating the group list and re-registers afterwards (1926, 1966)
- [ ] [P11-T14] Add a test asserting `AddItemGroup` appends at `_itemGroups.Count` (1929)
- [ ] [P11-T15] Add a test asserting `AddItemGroup` hooks the new mail into the move monitor (1942)
- [ ] [P11-T16] Add a test covering both sides of the `_digitRefreshNeeded` branch in `AddItemGroup` (1936-1939)
- [ ] [P11-T17] Add a test covering both the dark and the light branch of `AddItemGroup` (1957-1964)
- [ ] [P11-T18] Add a test covering both sides of the `KbdActive` branch in `AddItemGroup` (1950)
- [ ] [P11-T19] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerGroupFactoryTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P11-T20] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerGroupFactoryTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase11-scoped-tests.<timestamp>.md`

### Phase 12 — Coverage for QfcCollectionController.Removal.cs

- [ ] [P12-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerRemovalTests.cs` as a **single** `[TestClass]` carrying `[DoNotParallelize]` (precedent: `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:11`, `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:22`), with a `[TestInitialize]` that resets `removespecificcontrolgroupcounter` to 0 by reflection, and add `<Compile Include="Controllers\QfcCollectionControllerRemovalTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
  - Acceptance: every test driving `RemoveSpecificControlGroup(int)` or `RemoveSpecificControlGroupAsync` lives in this one class; the reset defends against the missing `finally` between the increment at 1161 and the decrement at 1247
- [ ] [P12-T2] Add a test asserting `RemoveSpecificControlGroup(string)` delegates with the matched group's `ItemNumber` when a match is found
- [ ] [P12-T3] Add a test asserting `RemoveSpecificControlGroup(string)` is a no-op when no group matches (1056-1057)
- [ ] [P12-T4] Add a test asserting `RemovedItemMonitor` performs unregister, then remove, then register in that order (1048-1050)
- [ ] [P12-T5] Add a test asserting `RemoveGroupByEntryId` routes through the `_removeGroupByEntryId` delegate default (1060-1074)
- [ ] [P12-T6] Add a test covering the active and inactive branches of `RemoveSpecificControlGroup(int)` (1110)
- [ ] [P12-T7] Add a test covering the expanded and collapsed branches of `RemoveSpecificControlGroup(int)` (1139)
- [ ] [P12-T8] Add a test covering the `Count > 0` renumber path of `RemoveSpecificControlGroup(int)` (1129-1143)
- [ ] [P12-T9] Add a test covering the `Count == 0 && KbdActive` path of `RemoveSpecificControlGroup(int)` that toggles the keyboard dialog (1144-1147)
- [ ] [P12-T10] Add a test covering the `Count == 0` path of `RemoveSpecificControlGroup(int)` that calls `_parent.ActionOkAsync()` (1151-1154)
- [ ] [P12-T11] Add a test asserting `RemoveSpecificControlGroup(int)` unhooks the removed group's mail from the move monitor (1124)
- [ ] [P12-T12] Add a test covering the active and inactive branches of `RemoveSpecificControlGroupAsync` through a `Mock<IUiDispatcher>` that executes inline, per `QfcItemController.TestSupport.cs:102-120`
- [ ] [P12-T13] Add a test covering the expanded and collapsed branches of `RemoveSpecificControlGroupAsync`
- [ ] [P12-T14] Add a test covering the digit-refresh branch of `RemoveSpecificControlGroupAsync` (1197-1201)
- [ ] [P12-T15] Add a test covering the zero-item `SkipGroupAsync` branch of `RemoveSpecificControlGroupAsync` that sets `swapAlreadyRegistered` (1230-1235), driven through seam **S8**
- [ ] [P12-T16] Add a test covering the guarded trailing register in `RemoveSpecificControlGroupAsync` (1243-1246)
- [ ] [P12-T17] Add a test covering the counter `> 1` log branch (1237-1242) by pre-seeding `removespecificcontrolgroupcounter` by reflection, and a second test characterizing issue **#286**'s current process-global counter behavior across two independent controller instances
  - Acceptance: current behavior is asserted, not corrected; #286 is referenced by number in the test docstring; this satisfies spec AC15 (second half) and user-story US-AC8 (second half)
- [ ] [P12-T18] Add a test asserting `RemoveControls` exits early when `_itemGroups` is null
- [ ] [P12-T19] Add a test asserting the non-null `RemoveControls` path calls `Cleanup()` per group, clears the list, and calls `UnhookAll` (1007)
- [ ] [P12-T20] Add a test characterizing that `RemoveControlsAsync` does **not** call `UnhookAll` (1024-1044), asserting the asymmetry against `RemoveControls` as current behavior, citing `research/qfc-collection-controller.md` §F6 and, if the execution-time issue sweep confirms a matching promoted issue (candidate #473), that issue number
- [ ] [P12-T21] Add a test covering `CleanupBackground` with null `_itemGroupsToMove` and null `_itemTlpToMove`
- [ ] [P12-T22] Add a test covering `CleanupBackground` with non-null `_itemGroupsToMove` and `_itemTlpToMove` (1015-1021)
- [ ] [P12-T23] Add a test asserting `PopOutControlGroup` reads the mail item before removing the group (967, 970) and delegates to seam **S7**
- [ ] [P12-T24] Add a test asserting `PopOutControlGroupAsync` delegates to seam **S7** (986-988) and throws for an already-cancelled token (978)
- [ ] [P12-T25] Add a test asserting `CacheMoveObjects` captures the outgoing page state (898-902)
- [ ] [P12-T26] Add a test asserting `CacheItemGroupsForMove` (876-881) and `ActivateQueuedItemGroups` (883-886) swap the cached and live group lists
- [ ] [P12-T27] Add a test asserting `SwapItemGroups` (888-896) and `ActivateQueuedTlp` (859-863) perform the page swap through the TLP surface
- [ ] [P12-T28] Verify that the six existing `RemoveBelowThresholdAsync` cases at `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:185-288` remain in place unedited and are NOT ported into the new file
  - Acceptance: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` still measures exactly 500 lines and has a zero diff
- [ ] [P12-T29] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerRemovalTests.cs` is <= 500 lines; if it would exceed, split the non-counter tests into `QfcCollectionControllerRemovalTests.Part2.cs` with its own `[TestClass]` and `<Compile Include>` entry, keeping ALL counter-driving tests in the single `[DoNotParallelize]` class
- [ ] [P12-T30] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerRemovalTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase12-scoped-tests.<timestamp>.md`

### Phase 13 — Coverage for QfcCollectionController.KeyboardWiring.cs

- [ ] [P13-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerKeyboardWiringTests.cs` with a `[TestClass]` using a real `KbdActions<string, KaStringAsync, Func<string, Task>>` behind a Loose `IQfcKeyboardHandler`, per the proven shape at `QfcCollectionControllerTests.cs:338-365`, and add `<Compile Include="Controllers\QfcCollectionControllerKeyboardWiringTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
- [ ] [P13-T2] Add a test asserting `WireUpAsyncKeyboardHandler` calls all three registrars (1277-1279)
- [ ] [P13-T3] Add a test asserting `RegisterAsyncKeyActions` registers exactly two entries, `Keys.Up` to `SelectPreviousItemAsync` and `Keys.Down` to `SelectNextItemAsync` (1287-1288)
- [ ] [P13-T4] Add a test asserting `RegisterAlwaysOnAsyncKeyActions` registers exactly one `Keys.Return` entry (1302)
- [ ] [P13-T5] Add a test characterizing `CustomReturnKeyHandler`: because `AnyOpenDropDowns` always returns `false` (1321), `ActionOkAsync` is always called (1312) — assert the always-clear gate as current behavior, citing `research/qfc-collection-controller.md` as the source for this asymmetry
  - Acceptance: no promoted issue number is cited for this finding. It carries no entry in this plan's Out of Scope inventory and is not among the issue numbers AC15 requires, so the research section is the only correct reference
- [ ] [P13-T6] Add a test asserting `AnyOpenDropDowns` returns `false` for every input (1319-1322)
- [ ] [P13-T7] Add a test covering both the `_digitRefreshNeeded == true` and `== false` branches of `RegisterNavigation` (1333-1336) and asserting one action is registered per group
- [ ] [P13-T8] Add a test asserting `UnregisterNavigation` uses the `Digits == 1` key format (1349)
- [ ] [P13-T9] Add a test asserting `UnregisterNavigation` uses the `Digits == 2` key format (1353)
- [ ] [P13-T10] Add a test asserting `RegisterNavigationAsyncAction` registers the expected async action (1358-1361)
- [ ] [P13-T11] Add a test asserting `GenerateStringKbdAction` produces the expected key for `digits == 1` (1366-1374)
- [ ] [P13-T12] Add a test asserting `GenerateStringKbdAction` produces the expected key for `digits == 2` (1366-1374)
- [ ] [P13-T13] Add a test asserting the unhandled `digits == 3` default leaves `key == ""` (1366-1374), which is required for the 75% branch gate on this file
- [ ] [P13-T14] Add a test asserting `RegisterNavigation` called twice without an intervening `UnregisterNavigation` throws `ArgumentException`
- [ ] [P13-T15] Add a test characterizing the `Digits` desync between `RegisterNavigation` (1330-1341, evaluates `Digits` once) and `UnregisterNavigation` (1343-1356, evaluates `Digits` per iteration): drive a group-count change between register and unregister and assert the CURRENT key-mismatch behavior, citing issue #472 — do NOT correct it
- [ ] [P13-T16] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerKeyboardWiringTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P13-T17] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerKeyboardWiringTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase13-scoped-tests.<timestamp>.md`

### Phase 14 — Coverage for QfcCollectionController.Selection.cs

- [ ] [P14-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerSelectionTests.cs` with a `[TestClass]` using seams **S1** and **S2** with a `Mock<IUiDispatcher>` that executes inline, and add `<Compile Include="Controllers\QfcCollectionControllerSelectionTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
- [ ] [P14-T2] Add a test asserting `ActivateBySelection` handles a below-range selection (`<= 0`) (1403)
- [ ] [P14-T3] Add a test asserting `ActivateBySelection` handles an in-range selection
- [ ] [P14-T4] Add a test asserting `ActivateBySelection` handles an above-range selection (1403)
- [ ] [P14-T5] Add a test covering both the `blExpanded == true` and `== false` branches of `ActivateBySelection` (1412)
- [ ] [P14-T6] Add a test asserting `ActivateBySelection` updates `ActiveSelection` (1419), restores `TlpLayout` (1421), and returns `ActiveSelection`
- [ ] [P14-T7] Add a test asserting `ActivateBySelectionAsync` covers the same below/in/above-range matrix
- [ ] [P14-T8] Add a test characterizing that `ActivateBySelectionAsync` does **not** call `LblSubject.Focus()` whereas `ActivateBySelection` does, asserting the 1417-versus-1441 asymmetry as current behavior
- [ ] [P14-T9] Add a test asserting `ActivateByIndex` delegates with `index + 1` (1393)
- [ ] [P14-T10] Add a test asserting `ActivateByIndexAsync` delegates with `index + 1` (1398)
- [ ] [P14-T11] Add a test asserting `ChangeByIndex` is a no-op when the requested index equals the current index (1453)
- [ ] [P14-T12] Add a test asserting `ChangeByIndex` skips `ToggleOffActiveItem` when `ActiveIndex == -1` (1458)
- [ ] [P14-T13] Add a test asserting `ChangeByIndex` is a no-op for an out-of-range index
- [ ] [P14-T14] Add a test asserting `ChangeByIndexAsync` covers the same matrix through the dispatcher seam (1472, 1482)
- [ ] [P14-T15] Add a test asserting `SelectNextItem` is a no-op at the last item (1488)
- [ ] [P14-T16] Add a test asserting `SelectNextItem` advances otherwise
- [ ] [P14-T17] Add a test asserting `SelectPreviousItem` is a no-op at index 0 (1505)
- [ ] [P14-T18] Add a test asserting `SelectPreviousItem` retreats otherwise
- [ ] [P14-T19] Add a test asserting `SelectNextItemAsync` and `SelectPreviousItemAsync` delegate through the dispatcher seam (1500, 1518)
- [ ] [P14-T20] Add a test asserting `ScrollIntoView` forwards to the TLP surface (1521-1541): the `_itemPanel` `Top`/`Bottom`/`Height` reads and the `AutoScrollPosition` write route through `IQfcTlpSurface`, while the `ItemViewer` argument's `Top`/`Bottom` are read directly from the supplied parameter and require no viewer-surface member
- [ ] [P14-T21] Add a test asserting `ToggleOffActiveItem` returns its parameter unchanged when `ActiveIndex == -1` (1670)
- [ ] [P14-T22] Add a test asserting `ToggleOffActiveItem` returns its parameter unchanged when `KbdActive == false`
- [ ] [P14-T23] Add a test asserting `ToggleOffActiveItem` calls `ToggleExpansion` and returns `true` for an expanded active item (1675-1681)
- [ ] [P14-T24] Add a test characterizing that `ToggleOffActiveItemAsync` calls `ToggleFocusAsync(Off)` only and that `ToggleExpansionAsync` is **never** called, because the expansion block at 1694-1698 is commented out
- [ ] [P14-T25] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerSelectionTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P14-T26] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerSelectionTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase14-scoped-tests.<timestamp>.md`

### Phase 15 — Coverage for QfcCollectionController.NavigationToggle.cs

- [ ] [P15-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationToggleTests.cs` with a `[TestClass]` using seams **S1** and **S2**, and add `<Compile Include="Controllers\QfcCollectionControllerNavigationToggleTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
- [ ] [P15-T2] Add a test asserting `ToggleExpansionStyle` throws `ArgumentOutOfRangeException` whose message names the valid range when `itemIndex < 0` (1545-1551)
- [ ] [P15-T3] Add a test asserting `ToggleExpansionStyle` throws `ArgumentOutOfRangeException` when `itemIndex >= Count` (1545-1551)
- [ ] [P15-T4] Add a test asserting `ToggleExpansionStyle` throws `InvalidOperationException` whose message includes subject, sender, and date when `IsActiveUI == false` (1553-1561)
- [ ] [P15-T5] Add a test asserting `ToggleExpansionStyle(On)` applies `_templateExpanded` (1566-1567)
- [ ] [P15-T6] Add a test asserting `ToggleExpansionStyle(Off)` applies `_template` (1571-1572)
- [ ] [P15-T7] Add a test covering the `heightChange < 0` invoke branch of `ToggleExpansionStyle` (1580-1585)
- [ ] [P15-T8] Add a test asserting `ToggleExpansionStyle(On)` scrolls the item into view (1587-1588)
- [ ] [P15-T9] Add a test asserting `ToggleExpansionStyleAsync` throws for an already-cancelled token (1593)
- [ ] [P15-T10] Add a test asserting `ToggleExpansionStyleAsync` otherwise dispatches through seam **S2** (1595)
- [ ] [P15-T11] Add a test asserting `ToggleOffNavigation` skips when `ActiveIndex == -1` (1602)
- [ ] [P15-T12] Add a test asserting `ToggleOffNavigation` fans out with `desiredState = Off` to every group (1607)
- [ ] [P15-T13] Add a test asserting `ToggleOffNavigationAsync` saves and restores `TlpLayout` (1617, 1631)
- [ ] [P15-T14] Add a test covering the `ActiveIndex == -1` branch of `ToggleOffNavigationAsync`
- [ ] [P15-T15] Add a test asserting `ToggleOnNavigation` fans out with `desiredState = On` and reactivates when `ActiveIndex != -1` (1642-1645)
- [ ] [P15-T16] Add a test asserting `ToggleOnNavigationAsync` fans out with `desiredState = On` and reactivates when `ActiveIndex != -1` (1659-1662)
- [ ] [P15-T17] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationToggleTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P15-T18] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerNavigationToggleTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase15-scoped-tests.<timestamp>.md`

### Phase 16 — Coverage for QfcCollectionController.Conversation.cs

- [ ] [P16-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerConversationTests.cs` with a `[TestClass]` using seam **S1**, and add `<Compile Include="Controllers\QfcCollectionControllerConversationTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
- [ ] [P16-T2] Add a test asserting `ChangeConversationSilently(int, bool)` delegates to the `QfcItemGroup` overload (1716)
- [ ] [P16-T3] Add a test asserting `ChangeConversationSilently(QfcItemGroup, bool)` saves and restores `SuppressEvents` around the write when the prior state is `true` (1727-1730)
- [ ] [P16-T4] Add a test asserting the same save-and-restore behavior when the prior `SuppressEvents` state is `false`
- [ ] [P16-T5] Add a test asserting `ToggleGroupConv(string)` handles the case where the original group is present
- [ ] [P16-T6] Add a test asserting `ToggleGroupConv(string)` takes the promotion path when the original is absent and a child is present (1743-1746)
- [ ] [P16-T7] Add a test characterizing that `ToggleGroupConv(string)` throws `ArgumentOutOfRangeException` when the original is absent and no child exists, asserting the current `_itemGroups[-1]` behavior and citing issue #470
- [ ] [P16-T8] Add a test asserting `ToggleGroupConv(string)` skips the collapse when `childCount == 0` (1752)
- [ ] [P16-T9] Add a test covering both the `reactivate == true` and `== false` branches of `ToggleGroupConv(string)` (1755-1764)
- [ ] [P16-T10] Add a test asserting `ToggleGroupConv(int, int)` removes exactly `childCount` groups starting at `indexOriginal + 1` (1775-1785)
- [ ] [P16-T11] Add a test asserting `ToggleGroupConv(int, int)` calls `Cleanup()` on each removed controller (1783)
- [ ] [P16-T12] Add a test asserting `ToggleGroupConv(int, int)` renumbers the remaining groups (1787)
- [ ] [P16-T13] Add a test asserting `ToggleGroupConv(int, int)` unregisters navigation before mutating and re-registers afterwards (1773, 1796)
- [ ] [P16-T14] Add a test asserting `ToggleUnGroupConv` skips the whole block when `insertCount <= 0` (1825)
- [ ] [P16-T15] Add a test asserting `ToggleUnGroupConv` reserves space, inserts, and renumbers from `insertionIndex + insertCount` when `insertCount > 0` (1827-1830)
- [ ] [P16-T16] Add a test covering both sides of the `_digitRefreshNeeded` branch in `ToggleUnGroupConv` (1839-1842)
- [ ] [P16-T17] Add a test asserting `EnumerateConversationMembers` excludes the base `entryID` (1884)
- [ ] [P16-T18] Add a test asserting `EnumerateConversationMembers` orders members by `SentOn` descending (1885)
- [ ] [P16-T19] Add a test asserting `EnumerateConversationMembers` sets `ConvOriginID` from the group before the insertion point (1900-1902)
- [ ] [P16-T20] Add a test covering both the `KbdActive == true` and `== false` branches of `EnumerateConversationMembers` (1905)
- [ ] [P16-T21] Add a test covering both the dark and the light branch of `EnumerateConversationMembers` (1912-1919)
- [ ] [P16-T22] Add a test asserting `EnumerateConversationMembers` unchecks the conversation box (1920)
- [ ] [P16-T23] Add a test characterizing the `EnumerateConversationMembers` count mismatch (1875-1922) as current behavior, citing issue #470
- [ ] [P16-T24] Add a test asserting `PromoteFirstChild` decrements `childCount` through its `ref` parameter (1983), clears `ConvOriginID` and `IsChild` (1981-1982), and returns the promoted index
- [ ] [P16-T25] Add a test characterizing the `PromoteFirstChild` `-1` return case as current behavior, citing issue #470
- [ ] [P16-T26] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerConversationTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P16-T27] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerConversationTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase16-scoped-tests.<timestamp>.md`

### Phase 17 — Coverage for QfcCollectionController.Layout.cs

- [ ] [P17-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerLayoutTests.cs` with a `[TestClass]` requiring no mocks for the pure list/index members, using a reflection-injected `_itemGroups`, and add `<Compile Include="Controllers\QfcCollectionControllerLayoutTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
- [ ] [P17-T2] Add a test asserting `InsertItemGroups` inserts `insertCount` empty groups at `insertionIndex` (2004-2011)
- [ ] [P17-T3] Add a test asserting `InsertItemGroups` is a no-op for a zero count
- [ ] [P17-T4] Add a test asserting `InsertItemGroups` handles insertion at the end of the list
- [ ] [P17-T5] Add a test asserting `UpdateSelectionNumberForRemoval` decrements when `ActiveSelection == selection && selection == Count` (2049-2052)
- [ ] [P17-T6] Add a test asserting `UpdateSelectionNumberForRemoval` leaves the selection unchanged when `ActiveSelection == selection && selection < Count`
- [ ] [P17-T7] Add a test asserting `UpdateSelectionNumberForRemoval` decrements `ActiveIndex` when `ActiveSelection > selection` (2056-2060)
- [ ] [P17-T8] Add a test asserting `UpdateSelectionNumberForRemoval` is a no-op when `ActiveSelection < selection`
- [ ] [P17-T9] Add a test asserting `RenumberGroups()` assigns `i + 1` to every group (2064-2070)
- [ ] [P17-T10] Add a test asserting `RenumberGroups(int)` renumbers only from `beginningIndex` (2072-2078)
- [ ] [P17-T11] Add a test asserting `RenumberGroups(int)` is a no-op when `beginningIndex >= Count`
- [ ] [P17-T12] Add a test characterizing `EliminateSpaceForItems`: assert the CURRENT sign of the computed height delta at 2017-2026 as forwarded to the TLP surface, citing issue #471 — do NOT correct the sign
- [ ] [P17-T13] Add a test asserting `MakeSpaceForItems` forwards the computed height delta to the TLP surface (2029-2042)
- [ ] [P17-T14] Add a test characterizing the `ResetPanelHeight` `RowStyles.Count - 1` versus `ResetPanelHeightAsync` full-sum asymmetry between 2097 and 2084 as current behavior, citing `research/qfc-collection-controller.md` §F11 and, if the execution-time issue sweep confirms a matching promoted issue, that issue number
- [ ] [P17-T15] Add a test asserting `ResetPanelHeightAsync` forwards the computed height through the dispatcher and TLP surface seams (2080-2090)
- [ ] [P17-T16] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerLayoutTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P17-T17] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerLayoutTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase17-scoped-tests.<timestamp>.md`

### Phase 18 — Coverage for QfcCollectionController.Theme.cs

- [ ] [P18-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerThemeTests.cs` with a `[TestClass]` extending the arrange shape of `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`, and add `<Compile Include="Controllers\QfcCollectionControllerThemeTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
  - Acceptance: `QfcCollectionControllerDarkModeTests.cs` itself is not edited
- [ ] [P18-T2] Add a test asserting `SetupLightDark` sets `_darkMode` for both inputs and subscribes to `PropertyChanged` exactly once (2117)
- [ ] [P18-T3] Add a test covering the `_formViewer is null` early return of `DarkMode_CheckedChanged` (2125-2128)
- [ ] [P18-T4] Add a test covering `DarkMode_CheckedChanged` when `sender is IOlObjects` with `DarkMode == true` (2134-2136)
- [ ] [P18-T5] Add a test covering `DarkMode_CheckedChanged` when `sender is IOlObjects` with `DarkMode == false` (2134-2136)
- [ ] [P18-T6] Add a test covering `DarkMode_CheckedChanged` when `sender` is not `IOlObjects` and `_globals` is non-null (2138-2141)
- [ ] [P18-T7] Add a test covering `DarkMode_CheckedChanged` when `sender` is not `IOlObjects` and `_globals` is null, asserting the early return (2142-2145)
- [ ] [P18-T8] Add a test asserting `DarkMode_CheckedChanged` updates `_darkMode` (2155)
- [ ] [P18-T9] Add a test asserting `SetDarkMode` fans out to every group with the supplied `async` flag (2158-2164)
- [ ] [P18-T10] Add a test asserting `SetLightMode` fans out to every group with the supplied `async` flag (2166-2172)
- [ ] [P18-T11] Add a test asserting both `SetDarkMode` and `SetLightMode` are no-ops for an empty group list
- [ ] [P18-T12] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerThemeTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P18-T13] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerThemeTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase18-scoped-tests.<timestamp>.md`

### Phase 19 — Coverage for QfcCollectionController.Move.cs

- [ ] [P19-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerMoveTests.cs` with a `[TestClass]` using a reflection-injected `_itemGroupsToMove` per the proven shape at `QfcCollectionControllerTests.cs:83-132`, and add `<Compile Include="Controllers\QfcCollectionControllerMoveTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
  - Acceptance: no `[DoNotParallelize]` is required for this class; `xComma` is a pure static function with no field access and a deterministic hit map
- [ ] [P19-T2] Add a test asserting `MoveEmailsAsync` returns early when `_itemGroupsToMove` is null (2209-2213)
- [ ] [P19-T3] Add a test asserting `MoveEmailsAsync` returns early when `_itemGroupsToMove` is empty
- [ ] [P19-T4] Add a test asserting `MoveEmailsAsync` attempts exactly N moves for N groups
- [ ] [P19-T5] Add a test asserting a throwing `MoveMailAsync` is swallowed and iteration continues (2242-2257)
- [ ] [P19-T6] Add a test covering the inner `Subject` throw path inside `TryMoveEmailByGroupAsync` (2245-2252)
- [ ] [P19-T7] Add a test asserting `TryMoveEmailByGroupIndexAsync` resolves the group and delegates (2230-2234)
- [ ] [P19-T8] Add a test asserting `TryGetItemGroupByIndex` returns the group for a valid index
- [ ] [P19-T9] Add a test asserting `TryGetItemGroupByIndex` returns `null` for an out-of-range index (2266-2269)
- [ ] [P19-T10] Add a test covering `GetMoveDiagnostics` with a null `olAppointment`
- [ ] [P19-T11] Add a test covering `GetMoveDiagnostics` with a non-null appointment and an empty `Body` (2299-2303)
- [ ] [P19-T12] Add a test covering `GetMoveDiagnostics` with a non-null appointment and a non-empty `Body` (2304-2308)
- [ ] [P19-T13] Add a test covering `GetMoveDiagnostics` across more than one group
- [ ] [P19-T14] Add a test characterizing that `GetMoveDiagnostics` returns a trailing null element (2284) as current behavior, citing issue #469
- [ ] [P19-T15] Record in the test file's docstring and in `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/unreachable-members.<timestamp>.md` that the `else` at 2318-2322 in `GetMoveDiagnostics` is unreachable (issue #469) and is documented rather than tested
- [ ] [P19-T16] Add a test asserting `xComma(null)` returns the current value (2332-2343)
- [ ] [P19-T17] Add a test asserting `xComma("")` returns the current value
- [ ] [P19-T18] Add a test asserting `xComma` handles a `", "` input
- [ ] [P19-T19] Add a test asserting `xComma` handles a `","` input
- [ ] [P19-T20] Add a test asserting `xComma` handles an input with no comma
- [ ] [P19-T21] Add a test asserting `xComma` handles accented text
- [ ] [P19-T22] Add a test asserting by reflection that `xComma` is declared `public static string xComma(string)` on the `QfcCollectionController` type, protecting the F8 call site at `QuickFiler/Controllers/EfcHomeController.Metrics.cs:79`
- [ ] [P19-T23] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerMoveTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P19-T24] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerMoveTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase19-scoped-tests.<timestamp>.md`

### Phase 20 — Coverage for QfcCollectionController.LegacyLoadPaths.cs

- [ ] [P20-T1] Create `QuickFiler.Test/Controllers/QfcCollectionControllerLegacyLoadPathsTests.cs` with a `[TestClass]` in which every test docstring states that the member under test has no production caller anywhere in the repository and cites issue #468, and add `<Compile Include="Controllers\QfcCollectionControllerLegacyLoadPathsTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF
- [ ] [P20-T2] Add the first #444 characterization assertion: the `KbdActions(IEnumerable<UClass>)` constructor at `QuickFiler/Controllers/KbdActions.cs:26-29` does **not** throw when `WireUpKeyboardHandler` registers two `KaKey` entries sharing `SourceId="Collection"` and `Keys.Down` (1265-1272), because it performs no duplicate check
  - Acceptance: the docstring records that #444 is **DORMANT** — `WireUpKeyboardHandler` has no caller, and production wires keys through `WireUpAsyncKeyboardHandler` (1275-1280) then `RegisterAsyncKeyActions` (1282-1291), which registers `Keys.Up`/`Keys.Down` exactly once each
- [ ] [P20-T3] Add the second #444 characterization assertion: `FilterKeys(Keys.Down)` returns **two** entries without throwing
- [ ] [P20-T4] Add the third #444 characterization assertion: `Find(Keys.Down)` throws `InvalidOperationException`
- [ ] [P20-T5] Add a test asserting `AnyOpenDropDownsAsync` throws for an already-cancelled token (1326)
- [ ] [P20-T6] Add a test asserting `AnyOpenDropDownsAsync` otherwise returns `false` (1327)
- [ ] [P20-T7] Add a test asserting `LoadGroups_02cAsync` throws for an already-cancelled token (593)
- [ ] [P20-T8] Add a test covering the digits boundary in `LoadGroups_02cAsync` (595) and asserting one group is created per item
- [ ] [P20-T9] Add a test asserting `LoadGroups_02bAsync` throws for an already-cancelled token (641)
- [ ] [P20-T10] Add a test covering the digits boundary in `LoadGroups_02bAsync` (643) and asserting one group is created per item
- [ ] [P20-T11] Add a test driving `LoadGroup_03bAsync` with `SynchronizationContext.SetSynchronizationContext(new SynchronizationContext())` set first, per the pattern at `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:87-93`, because `TaskScheduler.FromCurrentSynchronizationContext()` at 662 requires a non-null current context
  - Acceptance: no STA thread is required for this test
- [ ] [P20-T12] Add a test asserting `LoadConversationsAndFoldersAsync` issues one call per group (761-774)
- [ ] [P20-T13] Add a test covering the digits boundary in `LoadItemGroup` (784)
- [ ] [P20-T14] Add a test asserting `LoadSequentialAsync` issues one call per group (827-840)
- [ ] [P20-T15] Add a test covering the digits boundary in `LoadGroupSequential` (850)
- [ ] [P20-T16] Add a test asserting `CacheTlpForMove` forwards to the TLP surface and assigns the cached panel (865-868)
- [ ] [P20-T17] Add a test asserting `SwapTlp` forwards to the TLP surface (870-874)
- [ ] [P20-T18] Add a test asserting `CaptureTlpTemplate` forwards to the TLP surface and assigns `_templateTlp` (1991-1996)
- [ ] [P20-T19] Verify `QuickFiler.Test/Controllers/QfcCollectionControllerLegacyLoadPathsTests.cs` is <= 500 lines; if it would exceed, split into a `.Part2.cs` partner with a second `[TestClass]` and its `<Compile Include>` entry
- [ ] [P20-T20] Run CMD-SCOPED with `<Filter>` = `FullyQualifiedName~QfcCollectionControllerLegacyLoadPathsTests` and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/phase20-scoped-tests.<timestamp>.md`

### Phase 21 — Per-File Coverage Measurement, Gap Closure, and Delta Reporting

- [ ] [P21-T1] Run CMD-PREFLIGHT and record its output to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/other/preflight-stale-worktrees-final.<timestamp>.md`
  - Acceptance: `EXIT_CODE: 0` and no `*.Test.dll` under any `.claude` path BELOW the executing repository root; the executing worktree's own root path segment is NOT a finding
- [ ] [P21-T2] Run CMD-REBUILD then CMD-COVERAGE — the identical command used in [P0-T21] and [P1-T25] — copy the produced report to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/coverage-final.cobertura.xml`, and write the companion command record to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/coverage-final.<timestamp>.md`
  - Acceptance: the copied XML carries a `<sources>` element proving post-processing; the companion `.md` records `Timestamp:`, `Command:` (both commands verbatim), `EXIT_CODE:`, and `Output Summary:` with total/passed/failed counts and numeric harness-native line and branch rates. The `.cobertura.xml` is a data artifact and cannot itself carry the four fields; the companion `.md` is the command-step artifact
- [ ] [P21-T3] Run CMD-RECOMPUTE against `coverage-final.cobertura.xml` for all 18 in-scope production files (the retained root, the 13 partials, the 4 seam files, plus `RemoveGroup.cs` if Phase 7 was taken) and write the per-file line and branch table to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/per-file-coverage.<timestamp>.md`
  - Acceptance: one row per file with both figures; the two `interface-only` seam interface files show **N/A**; any file with zero branch conditions shows **N/A** for branch, never 0%; the artifact states explicitly that the `line-rate`/`branch-rate` attributes were NOT used and why, citing issues #441 and #478
- [ ] [P21-T4] Close every per-file gap by adding cases to the owning phase's test file and re-running [P21-T2] and [P21-T3] until EVERY in-scope production file meets its gate: **>= 80% line and >= 75% branch** for the retained root and the 13 partials, and **>= 90% line** for every file newly created by this feature, with `N/A` never counted as a failure
  - Acceptance: the final `per-file-coverage.<timestamp>.md` shows no file below its gate; the only permitted deviation is the single `QuickFiler/Controllers/QfcItemViewerSurface.cs` exemption request prepared in [P5-T12], which must carry a file-specific rationale, the exact uncovered member list, and an F1 ledger entry
- [ ] [P21-T5] Verify AC14 reproducibility by running CMD-COVERAGE and CMD-RECOMPUTE a SECOND consecutive time on the same commit and confirming identical per-file line and branch figures for the file containing `removespecificcontrolgroupcounter` (`Removal.cs`, or `RemoveGroup.cs` if Phase 7 was taken), recording both run records to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/reproducibility-two-runs.<timestamp>.md`
- [ ] [P21-T6] Write the before/after repository-wide comparison to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/coverage-delta.<timestamp>.md`, reporting for BOTH the [P0-T21] before-run and the [P21-T2] after-run: the harness-native `/coverage/@line-rate` and `/coverage/@branch-rate` (the "retain or improve" comparator) AND the CMD-RECOMPUTE figure (the honest figure), plus the identical command used for each
  - Acceptance: harness-native is compared only to harness-native and recomputed only to recomputed; the epic's 70.19% figure is NOT used as the comparator; the transient package-rate drop recorded in [P1-T26] is restated so a reviewer does not read it as a regression; baseline, post-change, and new-file coverage figures are all reported as numbers
- [ ] [P21-T7] Write `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/file-sizes.<timestamp>.md` listing the line count of every production file created or modified by this feature and every new or modified test file, and confirm every entry is below 500
- [ ] [P21-T8] Write `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/test-policy-audit.<timestamp>.md` recording a banned-API grep over every new test file for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, and unseeded `Random`, plus the STA file listing and confirmation that MSTest, Moq, and FluentAssertions are used in Arrange-Act-Assert form with no temporary files, no external services, no live shown forms, and no popups
  - Acceptance: exactly two `*.StaTests.cs` files exist, each documenting why no seam is feasible
- [ ] [P21-T9] Write `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/exemption-attribute-grep.<timestamp>.md` recording a repo-wide grep for `ExcludeFromCodeCoverage` restricted to this child's files, confirming the attribute is absent from `QuickFiler/Controllers/QfcCollectionController.cs` and every partial, with the single possible exception of `QuickFiler/Controllers/QfcItemViewerSurface.cs` under [P5-T12]

### Phase 22 — Acceptance-Criteria Verification

Twenty-nine AC items: 17 in `spec.md` §15 and 12 in `user-story.md` §6. Both files are authoritative
under `full-feature` mode and are checked off independently.

- [ ] [P22-T1] Verify **spec AC1** — the split produced the retained root plus the 13 named partials and no production file created or modified exceeds 500 lines — against `evidence/qa-gates/file-sizes.<timestamp>.md`, and check the AC box in `spec.md`
- [ ] [P22-T2] Verify **spec AC2** — `git diff -- QuickFiler/QuickFiler.csproj` shows one contiguous addition-only hunk of 17 entries (18 if Phase 7 was taken) with no property, reference, or ordering change, and the file's `\r$` count equals its line count — and check the AC box
- [ ] [P22-T3] Verify **spec AC3** — the exemption is removed and no blanket re-exemption exists — against `evidence/qa-gates/exemption-attribute-grep.<timestamp>.md`, and check the AC box
- [ ] [P22-T4] Verify **spec AC4** — the WORKING-TREE comparator `git diff --exit-code <merge-base> -- QuickFiler/Interfaces/IQfcCollectionController.cs` (single-dot, no `..HEAD`) returns 0 with no output — and check the AC box
  - Acceptance: the single-dot working-tree form is mandatory for the same reason given in [P1-T24] — no task between [P0-T15] and [P23-T6] commits anything, so a `<merge-base>..HEAD` comparator would be vacuous
- [ ] [P22-T5] Verify **spec AC5** — `xComma` is still `public static string xComma(string)`, `EmailsToMove` and `GetMoveDiagnostics` (with its `ref AppointmentItem` parameter) are unchanged, `_moveMonitor` keeps its name, and the WORKING-TREE comparator `git diff --stat <merge-base>` (single-dot, no `..HEAD`) names no sibling-owned file from `spec.md` §2.5 — and check the AC box
  - Acceptance: the single-dot working-tree form is mandatory; a `<merge-base>..HEAD` comparator would be vacuous per [P1-T24]
- [ ] [P22-T6] Verify **spec AC6** — every in-scope production file reaches >= 80% line and >= 75% branch, computed by the CMD-RECOMPUTE recipe with the attributes never read — against `evidence/qa-gates/per-file-coverage.<timestamp>.md`, and check the AC box
- [ ] [P22-T7] Verify **spec AC7** — every newly created production file reaches >= 90% line, and every zero-coverable-line or zero-branch-condition file reports N/A rather than 0% — against the same table, and check the AC box
- [ ] [P22-T8] Verify **spec AC8** — the ledger carries one contiguous addition-only block containing every new file path, and the `IQfcCollectionController.cs` row is verified or reconciled to `interface-only / not-measured` — and check the AC box
- [ ] [P22-T9] Verify **spec AC9** — all four Phase 0 gates were evaluated before any production edit and their outcomes plus any literal codes are recorded — against `evidence/other/phase0-f1-gate.<timestamp>.md`, and check the AC box
- [ ] [P22-T10] Verify **spec AC10** — the three coverage artifacts exist, all carry a `<sources>` element, both measurement runs used the identical command, and the stale-worktree assertion ran before every measurement — and check the AC box
- [ ] [P22-T11] Verify **spec AC11** — repository-wide line and branch coverage is retained or improved against this child's own before-figure, harness-native to harness-native, with both figure kinds reported for both runs and the transient drop explained — against `evidence/qa-gates/coverage-delta.<timestamp>.md`, and check the AC box
- [ ] [P22-T12] Verify **spec AC12** — test-policy compliance across every new test file — against `evidence/qa-gates/test-policy-audit.<timestamp>.md`, and check the AC box
- [ ] [P22-T13] Verify **spec AC13** — `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` has a zero diff and is still exactly 500 lines, and no test file created by this feature exceeds 500 lines — and check the AC box
- [ ] [P22-T14] Verify **spec AC14** — the two consecutive runs in [P21-T5] produced identical per-file figures for the counter-bearing file, and all three `spec.md` §9.6 mitigations are present in the test source — and check the AC box
- [ ] [P22-T15] Verify **spec AC15** — the #444 and #286 characterization tests exist and pass, and issues #468, #469, #470, #471, #472, #473, #474, and #478 are referenced by number with none fixed — by grepping the diff for changes to the code paths those issues describe, and check the AC box
- [ ] [P22-T16] Verify **spec AC16** — read with the WORKING-TREE comparator `git diff <merge-base> -- QuickFiler/ QuickFiler.Test/` (single-dot, no `..HEAD`), the production diff is confined to file layout, `using` removal, optional trailing constructor parameters, and `private`/`internal` seam fields with bit-identical production defaults, and both existing test files pass unedited — and check the AC box
  - Acceptance: the single-dot working-tree form is mandatory; a `<merge-base>..HEAD` comparator would be vacuous per [P1-T24] and would let this AC pass on an empty diff without inspecting anything
- [ ] [P22-T17] Verify **spec AC17** — the full toolchain passed in order in a single final pass — against `evidence/qa-gates/toolchain.<timestamp>.md` produced in Phase 23, and check the AC box
- [ ] [P22-T18] Verify **user-story US-AC1** (nothing user-observable changed) and check the AC box in `user-story.md`
- [ ] [P22-T19] Verify **user-story US-AC2** (the largest file is replaced by readable units matching the `spec.md` §6.1 responsibility table, not a mechanical chop) and check the AC box
- [ ] [P22-T20] Verify **user-story US-AC3** (the file is measured rather than hidden, and now appears in the final Cobertura report where it previously appeared nowhere) and check the AC box
- [ ] [P22-T21] Verify **user-story US-AC4** (every unit reaches its stated numeric level) and check the AC box
- [ ] [P22-T22] Verify **user-story US-AC5** (the numbers are recomputed, not read from the inflated attribute, with N/A handled) and check the AC box
- [ ] [P22-T23] Verify **user-story US-AC6** (the same commit measures the same twice) and check the AC box
- [ ] [P22-T24] Verify **user-story US-AC7** (no sibling's contract is disturbed; F7's "no contract additions needed" conclusion remains true) and check the AC box
- [ ] [P22-T25] Verify **user-story US-AC8** (latent defects documented, not absorbed or fixed) and check the AC box
- [ ] [P22-T26] Verify **user-story US-AC9** (the repository is no worse off, measured like for like) and check the AC box
- [ ] [P22-T27] Verify **user-story US-AC10** (the upstream ledger dependency is honored via the four-gate check, with 17 rows appended in the same change as the csproj entries) and check the AC box
- [ ] [P22-T28] Verify **user-story US-AC11** (the new tests are trustworthy and rerunnable anywhere) and check the AC box
- [ ] [P22-T29] Verify **user-story US-AC12** (the change lands clean: toolchain in order, single contiguous CRLF-preserved csproj hunk) and check the AC box
- [ ] [P22-T30] Verify the seven `spec.md` §16 Definition of Done items and record the outcome to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/definition-of-done.<timestamp>.md`
  - Acceptance: §16 is a numbered non-checkbox list and is NOT counted toward the 29-item AC tally; item 7 requires a clean working tree with every evidence artifact committed
- [ ] [P22-T31] Write the AC status summary — 29 of 29 checked, with the verifying artifact named for each — to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/ac-status.<timestamp>.md`

### Phase 23 — Final QC Toolchain Loop

Run the four steps in this exact order. If ANY step fails or changes any file, restart the loop from
step 1. `EXIT_CODE: SKIPPED` is not a valid outcome for any task in this phase.

- [ ] [P23-T1] Run CMD-FORMAT (step 1, formatting) and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/toolchain-format.<timestamp>.md`
  - Acceptance: `dotnet tool run csharpier check .` exits 0 with no file modified; if `format` changed any file, restart the loop at [P23-T1] after committing the formatting change
- [ ] [P23-T2] Run CMD-ANALYZE (step 2, linting) and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/toolchain-analyze.<timestamp>.md`
  - Acceptance: the baseline-relative gate defined in the Command Reference is met — `EXIT_CODE` equals `ANALYZE_BASELINE_EXIT` from [P0-T23] AND the analyzer diagnostic set scoped to the files this feature created or modified is empty; if any file changed, restart at [P23-T1]
- [ ] [P23-T3] Run CMD-NULLABLE (step 3, type checking) and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/toolchain-nullable.<timestamp>.md`
  - Acceptance: the baseline-relative gate defined in the Command Reference is met — `EXIT_CODE` equals `NULLABLE_BASELINE_EXIT` from [P0-T23] AND the `CS86xx` diagnostic set scoped to the files this feature created or modified is empty; if any file changed, restart at [P23-T1]
- [ ] [P23-T4] Run CMD-PREFLIGHT, then CMD-REBUILD, then CMD-COVERAGE (step 4, testing with coverage), and record the result to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/toolchain-test.<timestamp>.md`
  - Acceptance: `EXIT_CODE: 0`; the `Output Summary:` records total/passed/failed counts, the numeric harness-native repository-wide line and branch rates, the numeric recomputed repository-wide line and branch rates, and the numeric per-file line and branch rate for every in-scope production file
- [ ] [P23-T5] Write the consolidated single-pass record to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/toolchain.<timestamp>.md` naming each of the four commands, its `EXIT_CODE`, and its output summary, and asserting that all four met their gate in that order in one uninterrupted pass with no step auto-fixing files
  - Acceptance: the record states the gate applied to each step — exit 0 for CMD-FORMAT and for the CMD-PREFLIGHT/CMD-REBUILD/CMD-COVERAGE test step, and the baseline-relative gate (equality with `ANALYZE_BASELINE_EXIT` / `NULLABLE_BASELINE_EXIT` plus an empty feature-scoped diagnostic set) for CMD-ANALYZE and CMD-NULLABLE — and restates both baseline exit codes as literal integers
- [ ] [P23-T6] Verify `git status --porcelain` is empty with every evidence artifact committed, and record the final `HEAD` SHA to `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/qa-gates/final-tree-state.<timestamp>.md`

---

## Test Plan

- **Unit (MSTest + Moq + FluentAssertions):** 16 new test files under `QuickFiler.Test/Controllers/`
  mirroring the partial names, one per production file, each wired into
  `QuickFiler.Test/QuickFiler.Test.csproj` with its own `<Compile Include>` entry. All new tests go in
  new files; `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (exactly 500 lines) and
  `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` are not edited.
- **STA (last resort, exactly two files):** `QuickFiler.Test/Controllers/QfcTlpSurface.StaTests.cs` and
  `QuickFiler.Test/Controllers/QfcItemViewerSurface.StaTests.cs`, using the manual STA-thread helpers
  at `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:267-278` and `:302-317`. No new
  NuGet package. Every control is created in memory and never shown.
- **Determinism:** all `removespecificcontrolgroupcounter`-driving tests live in one `[DoNotParallelize]`
  class that resets the counter by reflection in `[TestInitialize]`; any test using
  `ItemViewerQueue.SetCoreForTesting` carries `[DoNotParallelize]` plus a `[TestCleanup]` calling
  `ResetProductionCoreDefaultsForTesting()` and `ResetCoreForTesting()`. Seam **S3** exists so most
  tests avoid the static core entirely.
- **Characterization (no fix):** #444 (dormant duplicate `KaKey`), #286 (process-global counter), #468,
  #469, #470, #471, #472, #473, #474, #478.
- **Branch-attention list (75% gate is independent of the 80% line gate):** `ReadyForMove`, `TlpLayout`,
  `Digits`, `GenerateStringKbdAction`, `UpdateSelectionNumberForRemoval`, `DarkMode_CheckedChanged`,
  `RemoveSpecificControlGroup(int)`, `RemoveSpecificControlGroupAsync`, `GetMoveDiagnostics`,
  `ToggleGroupConv(string)`, `LoadItemToTlp`.
- **Coverage evidence:**
  - baseline: `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/evidence/baseline/coverage-baseline.cobertura.xml` and `.../evidence/baseline/coverage-baseline-recomputed.<timestamp>.md`
  - post-exemption-removal: `.../evidence/baseline/coverage-post-exemption-removal.<timestamp>.cobertura.xml` and `.../evidence/baseline/coverage-post-exemption-removal-recomputed.<timestamp>.md`
  - post-change: `.../evidence/qa-gates/coverage-final.cobertura.xml` and `.../evidence/qa-gates/per-file-coverage.<timestamp>.md`
  - comparison: `.../evidence/qa-gates/coverage-delta.<timestamp>.md`

## Open Questions / Notes

1. **Epic rule-4 tension (research §H2, spec R7).** The epic's "new files default to >= 90%" rule was
   written for new logic, but the 13 partials carry pre-existing extracted code. [P21-T4] applies the
   90% bar to them as written and does not unilaterally lower it. If the bar proves unreachable for a
   specific partial, raise it with F1 explicitly rather than assuming the 80% figure applies.
2. **`DynamicProxyGenAssembly2` fragility (spec R2).** The grant that lets Moq proxy `QuickFiler`
   internals is declared at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11`, an F2-owned
   file, not in `QuickFiler/Properties/AssemblyInfo.cs`. Recorded as a cross-child coupling; do not
   propose editing that file. Prefer seams that do not require proxying a `QuickFiler` internal where a
   plain interface or delegate will do.
3. **`issue.md` metadata is stale (spec D-10).** `issue.md:5` records a folder path missing the
   `2026-08-07-` prefix and the `-454` suffix, and `issue.md:11` records a Last Updated date one day
   ahead of the preparation date. Correcting it is outside this plan's write scope and is flagged for
   the caller.
4. **csproj fan-in conflict is expected, not a defect.** Seventeen (or eighteen) `<Compile Include>`
   entries is the largest csproj delta of any wave-1 child. Both sides of any fan-in conflict are
   additive, so the correct resolution is to keep both. Handled by this child's own remediation loop.
