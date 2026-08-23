# Remediation Plan — Cycle 1 (Issue #503)

- **Issue:** #503
- **Feature folder:** `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/`
- **Branch:** `bug/ribbon-engine-readiness-guard-503`
- **Merge-base:** `003c5715055d7d1933db68a742531332756e30b2`
- **Work mode:** `full-bug` (per `spec.md` metadata; `spec.md` is the sole authoritative AC source)
- **Remediation inputs:** `remediation-inputs.2026-08-08T14-26.md`
- **Source review:** `code-review.2026-08-08T14-15.md`, `policy-audit.2026-08-08T14-15.md`, `feature-audit.2026-08-08T14-15.md`
- **Prior implementation plan:** `plan.2026-08-08T11-59.md` (its section 4 scope lock and section 5 design remain authoritative and are not re-litigated here)
- **Blocking findings entering this cycle:** 0. The cycle is quality-discretionary; its exit gate is the same reaudit standard.

---

## 1. Objective and expected outcome

Remediate exactly the two in-scope findings recorded in `remediation-inputs.2026-08-08T14-26.md`:

- **F1** — the AC5 ribbon-XML assertion in `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` (approximately lines 197-206) short-circuits through a null-conditional operator, so it does not execute when the `getEnabled` attribute is absent. The corrected assertion must fail when the attribute is missing, when it carries the wrong value, and when it is present but empty, and that non-vacuity must be **demonstrated**, not asserted.
- **F2** — `TaskMaster\Ribbon\RibbonExplorer.xml` grew from 519 to 539 lines while already above the 500-line cap. Three previously single-line `<button>` elements (`TriageSetA`, `TriageSetB`, `TriageSetC`) were reformatted into six-line form. They must be restored to single-line form while retaining their `getEnabled` attribute.

Expected outcome: the corrected AC5 test is proven non-vacuous by a recorded mutate-build-fail-restore cycle; `RibbonExplorer.xml` is at or below **527** lines with all eight `getEnabled="EngineCommand_GetEnabled"` attributes intact; the full C# QC loop passes in a single uninterrupted pass; the change is committed so the reaudit diff observes it. Nothing else in the branch changes.

---

## 2. Path and token conventions

| Token | Value |
|---|---|
| `<REPO>` | `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55` |
| `<FEATURE>` | `<REPO>\docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503` |
| `<MERGE_BASE>` | `003c5715055d7d1933db68a742531332756e30b2` |
| `<TS>` | ISO-8601 capture timestamp in `yyyy-MM-ddTHH-mm` form, substituted at write time |
| `<CSHARPIER>` | `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe` |
| `<MSBUILD>` | `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` |
| `<VSTEST>` | `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe` |
| `<SCRATCH>` | The executor session scratchpad directory. It is **outside** the repository working tree. Helper scripts written there are throwaway session scripts and are never committed. |

Every evidence artifact carries, at minimum: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Test artifacts additionally carry numeric coverage headline values (`line-rate`, `branch-rate`, `lines-covered`, `lines-valid`).

#### 2.1 Evidence locations (non-overridable)

All evidence for this cycle is written under `<FEATURE>\evidence\<kind>\`:

| Kind | Path | Used by |
|---|---|---|
| Remediation baseline | `<FEATURE>\evidence\remediation-baseline\` | Phase 0 |
| Regression testing | `<FEATURE>\evidence\regression-testing\` | Phase 1 |
| QA gates | `<FEATURE>\evidence\qa-gates\` | Phase 2, Phase 3, Phase 4 |
| Other | `<FEATURE>\evidence\other\` | intermediate build steps |

`artifacts\baselines\`, `artifacts\baseline\`, `artifacts\qa\`, `artifacts\qa-gates\`, `artifacts\coverage\`, `artifacts\evidence\`, `artifacts\regression-testing\`, and `artifacts\post-change\` are **forbidden** as evidence output paths. No upstream instruction may override this. The single non-evidence exception is `artifacts\csharp\coverage.xml`, which is the canonical **gate** artifact consumed by `.claude\hooks\validate-feature-review-coverage.ps1`; it is gitignored (`.gitignore:57`), local-only, and regenerated rather than committed.

---

## 3. Fixed execution rules (binding on every task in this plan)

1. **Shell.** Every C# toolchain command runs through `pwsh -NoProfile -Command` or `pwsh -NoProfile -File`. Never invoke MSBuild, vstest, csharpier, or nuget through the Bash tool: the Bash tool is Git Bash and rewrites MSBuild `/switch` arguments into paths (`/m` becomes `M:/`, producing `error MSB1008`). This is a hard requirement.
2. **Absolute tool paths.** `csharpier`, `MSBuild.exe`, and `vstest.console.exe` are not on PATH. Use `<CSHARPIER>`, `<MSBUILD>`, `<VSTEST>` exactly as tabulated. `nuget` is on PATH.
3. **Restore before building.** `nuget restore TaskMaster.sln` runs unconditionally in Phase 0 (P0-T4) before any build task. It is idempotent; packages are currently restored, so exit 0 is the expected result.
4. **CSharpier subcommands only.** Formatting gates use `csharpier format <paths>` (mutates on disk) followed by `csharpier check .` returning `EXIT_CODE: 0`. `csharpier pipe-files` is prohibited as a gate: it writes to stdout only, never mutates, and produces a false "stable" result.
5. **CSharpier scope guard.** `csharpier format` is invoked with the explicit scope-locked path list in section 4 (this cycle: exactly one `.cs` file) and is NEVER invoked repo-wide, and NEVER with `TaskMaster\AppGlobals\AppItemEngines.cs` or `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` in its argument list. A repo-wide `csharpier format .` would reformat any file unformatted at the merge-base and break the AC15 zero-line-diff requirement. The read-only repo-wide `csharpier check .` gate is still run.
6. **CSharpier does not format XML.** `TaskMaster\Ribbon\RibbonExplorer.xml` is hand-edited. No formatter pass applies to it, and it must never be passed to `csharpier`.
7. **The embedded-resource rebuild rule.** `RibbonExplorer.xml` is an **embedded resource**. `RibbonExplorerXmlTests` reads it through `assembly.GetManifestResourceStream("TaskMaster.Ribbon.RibbonExplorer.xml")` on the `TaskMaster.dll` copied into `TaskMaster.Test\bin\Debug\`. A change to the `.xml` on disk is invisible to the test until the assembly is rebuilt and re-copied. Every task in this plan that changes the XML and then observes a test result is therefore split into an explicit **edit → rebuild → assert-embedded-content → run-test** sequence, and the assert-embedded-content step is a hard gate. Skipping it produces a false negative in the F1 fail-proof.
8. **The Phase 1 mutation is temporary and must never be committed.** P1-T5 deliberately removes one `getEnabled` attribute from `RibbonExplorer.xml`. P1-T8 restores it with `git checkout --` and is its own verified task. If execution halts for any reason between P1-T5 and P1-T8, the first action on resume is to run P1-T8. No commit task may execute while the mutation is present; the Phase 4 commit is gated on the P1-T8 restoration artifact.
9. **Pre-existing failure handling.** The suite is NOT green at baseline. `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` is a pre-existing order-dependent flake tracked as issue **#508** and is explicitly OUT OF SCOPE. A full-suite run passes when the ONLY failures are members of the set recorded in P0-T13. Do not attempt to fix #508. Any failure outside that set is a real regression: fix it and restart Phase 3 from P3-T1.
10. **No HEAD SHA is pinned as a plan expectation.** P0-T5 records the current HEAD for the audit trail only. All later gates are expressed as tree invariants (clean `git status --porcelain`, no diff on the protected paths, no source path outside the section 4 scope lock), never as equality against a recorded SHA.
11. **AC19, AC20, AC21 are MANUAL-ONLY and must remain `- [ ]`.** They must NOT be checked off on the strength of unit tests, source inspection, or any automated artifact produced by this cycle. P4-T2 verifies they are still unchecked.
12. **No acceptance criterion is checked off in this cycle.** Every automated AC in `spec.md` was already checked off by the implementation cycle. This cycle changes no checkbox state anywhere in `spec.md`. The only permitted `spec.md` edit is the append-only subsection in P4-T1.
13. **Non-AC checkboxes are not acceptance criteria.** The `- [ ] Blocker` / `- [x] High` / `- [ ] Medium` / `- [ ] Low` markers under `## Impact / Severity` in `spec.md` and `issue.md` are severity markers. Do not modify them.
14. **No `SKIPPED` completion path.** Every command-bearing task in this plan must execute its stated command and record the real result. `EXIT_CODE: SKIPPED` is invalid as a passing outcome for any task in this plan.
15. **Out-of-scope items must not be touched.** The items enumerated in `remediation-inputs.2026-08-08T14-26.md` under *Explicitly out of scope for this cycle* — the nullable debt and type-check gate defect (#512), the `CS2002` duplicate compile entry (#510), the #508 flake, the residual `engine as SpamBayes` / `.Engine` dereference window, the `??=` lazy-initialiser observation, and any change to `AppItemEngines.cs` or `IAppItemEngines.cs` — are not addressed here. Observing one is reportable, not fixable.

#### 3.1 Helper script A — embedded ribbon resource assertion

Written once to `<SCRATCH>\Assert-EmbeddedRibbon.ps1` in P0-T3 and invoked by P1-T3, P1-T6, P1-T9, and P2-T4. Its verbatim text is recorded in the P0-T3 artifact so the assertion is auditable without the scratchpad. It loads the assembly from a byte array so it never holds a file lock that would block the next rebuild.

```powershell
param([Parameter(Mandatory = $true)][string]$RepoRoot)
$ErrorActionPreference = 'Stop'
$dll = Join-Path $RepoRoot 'TaskMaster.Test\bin\Debug\TaskMaster.dll'
$asm = [System.Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($dll))
$stream = $asm.GetManifestResourceStream('TaskMaster.Ribbon.RibbonExplorer.xml')
$text = (New-Object System.IO.StreamReader($stream)).ReadToEnd()
$count = ([regex]::Matches($text, 'getEnabled="EngineCommand_GetEnabled"')).Count
$single = '<button id="TriageSetA" onAction="TriageSetA_Click" getEnabled="EngineCommand_GetEnabled" label="Set A" />'
Write-Output ("EMBEDDED_GETENABLED_COUNT={0}" -f $count)
Write-Output ("EMBEDDED_TRIAGESETA_SINGLELINE={0}" -f $text.Contains($single))
Write-Output ("EMBEDDED_ASSEMBLY_WRITETIME={0}" -f (Get-Item $dll).LastWriteTimeUtc.ToString('o'))
```

#### 3.2 Helper script B — Cobertura to first-party JaCoCo projection

Written to `<SCRATCH>\ConvertCoberturaToJacoco.ps1` in P0-T12 and invoked by P0-T12, P3-T7, and P3-T8. Its verbatim text is recorded in the P0-T12 artifact. Required behaviour:

- Read a Cobertura XML file given by `-Source`; write a JaCoCo XML file to `-Destination`.
- Include only the nine first-party solution packages: `QuickFiler`, `SVGControl`, `Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`, `UtilitiesCS`, `VBFunctions`. Exclude every other package (vendored `log4net`, `Microsoft.IO.RecyclableMemoryStream`, `Mono.Reflection`, `System.Interactive`, `System.Linq.Async`, and any `*.Test` package).
- Per included package: `LINE covered` = count of `<line>` elements with `hits` not equal to `0`; `LINE missed` = count with `hits` equal to `0`. `BRANCH covered` = sum of the covered term parsed from each `condition-coverage="NN% (c/t)"` attribute; `BRANCH missed` = sum of `t - c` over the same set.
- Emit the exact shape of the existing `<FEATURE>\evidence\qa-gates\coverage-final.jacoco.xml`: a `<report name="TaskMaster">` root containing one `<package name="...">` element per included package, each holding a `<counter type="LINE" missed="..." covered="..." />` and a `<counter type="BRANCH" missed="..." covered="..." />`.
- The output must be under 100 lines. Raw multi-megabyte Cobertura reports are **never** committed to the feature evidence tree.

---

## 4. Scope lock for this cycle

Only the paths below may be created or modified. Any change outside this list is out of scope and must be reported rather than made.

#### 4.1 Source paths that may change

| Path | Change | Current lines | Post-change expectation |
|---|---|---|---|
| `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` | F1: replace the null-conditional assertion at approximately lines 197-206 | 309 | approximately 313, hard cap 500 |
| `TaskMaster\Ribbon\RibbonExplorer.xml` | F2: collapse the `TriageSetA` / `TriageSetB` / `TriageSetC` `<button>` elements back to single-line form, retaining `getEnabled` | 539 | **524**, hard gate at or below 527 |

`TaskMaster\Ribbon\RibbonExplorer.xml` is also mutated **temporarily** in P1-T5 and restored in P1-T8. That mutation is not a permitted permanent change.

#### 4.2 Documentation and evidence paths that may change

- `<FEATURE>\remediation-plan.2026-08-08T14-26.md` (this file; checklist state only)
- `<FEATURE>\spec.md` (append-only: one new subsection under `## Delivery Notes and Deviations`, per P4-T1; zero existing lines changed, zero checkbox state changed)
- `<FEATURE>\evidence\**` (artifacts written by this plan)
- `.claude\agent-memory\**` (agent memory updates)
- Pre-existing uncommitted paths carried in from the review cycle (the `code-review`, `policy-audit`, `feature-audit`, and `remediation-inputs` artifacts at the feature root, and entries under `docs\features\potential\promoted\`). This cycle neither creates nor modifies them; P4-T3's `git add -A` commits them as-is. They are recorded in the P0-T5 porcelain and classified as P3-T11 bucket (c).

#### 4.3 Paths that must take a ZERO-LINE DIFF against `<MERGE_BASE>`

- `TaskMaster\AppGlobals\AppItemEngines.cs`
- `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs`
- `TaskMaster\AppGlobals\ApplicationGlobals.cs`

AC15 binds the first two. The third was also a zero-line diff in the implementation cycle and is verified alongside them. P3-T10 and P4-T4 are the verification tasks.

#### 4.4 Gitignored outputs (produced, never committed)

- `coverage\remediation-baseline.cobertura.xml` and `coverage\remediation-final.cobertura.xml` (`.gitignore:144` ignores `coverage/*`)
- `artifacts\csharp\coverage.xml` (`.gitignore:57` ignores `artifacts/`)

#### 4.5 Scope-locked `.cs` path list for `csharpier format`

```
TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs
```

---

## 5. Pinned change specifications (do not re-derive)

#### 5.1 F1 — the corrected assertion

The current body of `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` (`TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs`, lines 198-205) is:

```csharp
elementsById[controlId]
    .Attribute("getEnabled")
    ?.Value.Should()
    .Be(
        EngineCommandGetEnabledCallback,
        "control '{0}' is engine-backed and must be disabled until its engine loads",
        controlId
    );
```

It is replaced by the following shape, which binds the attribute first and asserts on it before dereferencing `Value`:

```csharp
var getEnabled = elementsById[controlId].Attribute("getEnabled");
getEnabled
    .Should()
    .NotBeNull(
        "control '{0}' is engine-backed and must declare a getEnabled callback",
        controlId
    );
getEnabled!
    .Value.Should()
    .Be(
        EngineCommandGetEnabledCallback,
        "control '{0}' is engine-backed and must be disabled until its engine loads",
        controlId
    );
```

Why this satisfies all three required failure conditions:

| Condition | Failing assertion |
|---|---|
| Attribute absent | `getEnabled.Should().NotBeNull(...)` — `Attribute("getEnabled")` returns `null`, and no `?.` short-circuits the chain |
| Attribute present with the wrong value | `getEnabled!.Value.Should().Be(...)` — string inequality |
| Attribute present but empty | `getEnabled!.Value.Should().Be(...)` — `""` is not `"EngineCommand_GetEnabled"` |

The sibling test `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` (line 224) uses `?.Value ==` **inside a LINQ predicate**, where null-means-no-match is the intended semantics. That test is correct, must remain unchanged, and continues to enforce AC5 independently.

The final formatting of the replacement block is whatever `csharpier format` produces in P3-T1. The text above pins semantics, not layout.

#### 5.2 F2 — the three collapsed `<button>` elements

`TaskMaster\Ribbon\RibbonExplorer.xml` lines 448-465 currently hold three six-line elements inside `<group id="TriageGroup" ...>`. They are replaced by exactly three single lines, each indented ten spaces to match the group's children:

```xml
          <button id="TriageSetA" onAction="TriageSetA_Click" getEnabled="EngineCommand_GetEnabled" label="Set A" />
          <button id="TriageSetB" onAction="TriageSetB_Click" getEnabled="EngineCommand_GetEnabled" label="Set B" />
          <button id="TriageSetC" onAction="TriageSetC_Click" getEnabled="EngineCommand_GetEnabled" label="Set C" />
```

This restores the merge-base single-line form of these three elements (`code-review.2026-08-08T14-15.md` records the removed merge-base line as `<button id="TriageSetA" onAction="TriageSetA_Click" label="Set A" />`) with the one functionally required attribute added. Arithmetic: three elements at six lines each become three elements at one line each, so `git diff --numstat` reports `3` added and `18` deleted, and the file moves from 539 to **524** lines — at or below the 527 gate (519 merge-base + 8 functional attribute lines).

The other five engine-backed buttons (`TrainSpam` 99-105, `TrainHam` 106-112, `TestSpam`, `FilterTriageGroup`, `ClearTriage`) were already multi-line at the merge-base. They are **not** touched. No `menu`, `group`, or `tab` element is touched.

#### 5.3 The F1 mutation used for the fail-proof

P1-T5 deletes exactly one line from `TaskMaster\Ribbon\RibbonExplorer.xml` — line 103, `            getEnabled="EngineCommand_GetEnabled"`, inside the `<button id="TrainSpam" ...>` element that spans lines 99-105. No other byte changes. The result is a file with seven occurrences of `getEnabled="EngineCommand_GetEnabled"` and 538 lines.

Restoration in P1-T8 is `git checkout -- TaskMaster/Ribbon/RibbonExplorer.xml`, which is exact and deterministic because Phase 1 makes no other change to that file and Phase 2 has not yet run.

---

## 6. Explicitly out of scope for this cycle

Do not touch, fix, re-promote, or re-litigate any of the following. Each is already routed to its own issue or was assessed and dismissed by the review.

- Issue **#512** — pre-existing repository-wide nullable debt and the vacuous type-check gate.
- Issue **#510** — the `CS2002` duplicate `<Compile Include>` entry in `UtilitiesCS.Test.csproj`.
- Issue **#508** — the `YieldAsync_WithoutDispatcher_RemainsStrict` order-dependent flake.
- The residual `engine as SpamBayes` / `.Engine` dereference window (Low; the click guard makes the reported defect unreachable, and narrowing the readiness contract is a design change, not a fix).
- The `??=` lazy-initialiser thread-safety observation in `RibbonController.EngineCommands.cs` (Low; the runner is immutable and the reviewer assessed it benign).
- Any change to `TaskMaster\AppGlobals\AppItemEngines.cs` or `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` — AC15 requires a zero-line diff on both.
- AC19, AC20, AC21 — MANUAL-ONLY, must remain unchecked.
- Issues **#504**, **#505**, **#506**, **#507**, **#509**, **#511** — already promoted out-of-scope findings.

---

## 7. Phased task list

### Phase 0 — Remediation Baseline Capture

- [x] [P0-T1] Read the policy files in the exact order defined by `.claude\skills\policy-compliance-order\SKILL.md`: (1) `<REPO>\CLAUDE.md`, (2) `<REPO>\.claude\rules\general-code-change.md`, (3) `<REPO>\.claude\rules\general-unit-test.md`, (4) `<REPO>\.claude\rules\csharp.md`, then `<REPO>\.claude\rules\architecture-boundaries.md`, `<REPO>\.claude\rules\quality-tiers.md`, and `<REPO>\.claude\rules\tonality.md`. Acceptance: `<FEATURE>\evidence\remediation-baseline\phase0-instructions-read.<TS>.md` exists containing `Timestamp:`, `Policy Order:`, and an explicit ordered list of every file read with its absolute path. Binary outcome: the artifact exists and lists all seven paths in that order.
- [x] [P0-T2] Read the remediation inputs and supporting context: `<FEATURE>\remediation-inputs.2026-08-08T14-26.md`, `<FEATURE>\spec.md`, `<FEATURE>\plan.2026-08-08T11-59.md`, `<FEATURE>\code-review.2026-08-08T14-15.md`, `<FEATURE>\policy-audit.2026-08-08T14-15.md`, and `<FEATURE>\feature-audit.2026-08-08T14-15.md`. Acceptance: `<FEATURE>\evidence\remediation-baseline\phase0-inputs-read.<TS>.md` exists, records `Timestamp:` and the six absolute paths, states the resolved work mode `full-bug` with `spec.md` named as the sole AC source, and names F1 and F2 as the only two findings in this cycle together with the verbatim out-of-scope list from section 6. Binary outcome: the artifact exists and names exactly two in-scope findings.
- [x] [P0-T3] Write helper script A to `<SCRATCH>\Assert-EmbeddedRibbon.ps1` with the verbatim body given in section 3.1, and run it once to capture the pre-remediation embedded-resource state. Command: `pwsh -NoProfile -File <SCRATCH>\Assert-EmbeddedRibbon.ps1 -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'`. Acceptance: `<FEATURE>\evidence\remediation-baseline\embedded-ribbon-helper.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, an `Output Summary:` recording `EMBEDDED_GETENABLED_COUNT`, `EMBEDDED_TRIAGESETA_SINGLELINE`, and `EMBEDDED_ASSEMBLY_WRITETIME`, and the verbatim script text so the assertion is reproducible without the scratchpad. Binary outcome: the artifact records `EMBEDDED_GETENABLED_COUNT=8` and `EMBEDDED_TRIAGESETA_SINGLELINE=False`.
- [x] [P0-T4] Verify tool availability and restore NuGet packages. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; Test-Path 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe'; Test-Path 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe'; Test-Path 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe'; Test-Path 'TaskMaster.runsettings'; nuget restore TaskMaster.sln"`. Acceptance: `<FEATURE>\evidence\remediation-baseline\toolchain-and-restore.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording one `True`/`False` per probed path plus the restore result. Binary outcome: all four `Test-Path` probes report `True` and the restore exits 0.
- [x] [P0-T5] Record git state for the audit trail. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git rev-parse HEAD; git rev-parse --abbrev-ref HEAD; git status --porcelain"`. Acceptance: `<FEATURE>\evidence\remediation-baseline\git-state.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording the current HEAD SHA, the branch name `bug/ribbon-engine-readiness-guard-503`, the merge-base `003c5715055d7d1933db68a742531332756e30b2`, and the verbatim `git status --porcelain` output. The recorded HEAD is an audit record only and is never used as a later equality gate. Binary outcome: no `.cs`, `.csproj`, `.xml`, or `.sln` path appears in the porcelain output.
- [x] [P0-T6] Record the current line counts and `getEnabled` occurrence counts of the two files this cycle will touch. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; 'TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs','TaskMaster\Ribbon\RibbonExplorer.xml' | ForEach-Object { '{0}={1}' -f $_, (Get-Content $_ | Measure-Object -Line).Lines }; (Select-String -Path 'TaskMaster\Ribbon\RibbonExplorer.xml' -Pattern 'getEnabled=' -AllMatches | Measure-Object).Count"`. Acceptance: `<FEATURE>\evidence\remediation-baseline\file-line-counts.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording both line counts and the occurrence count. Binary outcome: the artifact records `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs=309`, `TaskMaster\Ribbon\RibbonExplorer.xml=539`, and 8 `getEnabled` occurrences; any deviation is recorded verbatim and treated as a finding rather than overwritten.
- [x] [P0-T7] Capture the verbatim pre-remediation text of both change sites. Acceptance: `<FEATURE>\evidence\remediation-baseline\change-site-text.<TS>.md` exists with `Timestamp:` and two fenced blocks: the current body of `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` from `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` lines 188-206 quoted verbatim with line numbers, and the current `TriageSetA` / `TriageSetB` / `TriageSetC` `<button>` elements from `TaskMaster\Ribbon\RibbonExplorer.xml` lines 448-465 quoted verbatim with line numbers. Binary outcome: the artifact contains both blocks and the test block visibly contains the `?.Value.Should()` sequence that F1 removes.
- [x] [P0-T8] Capture the CSharpier state repo-wide, read-only. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' check .; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\remediation-baseline\csharpier-check.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` that, when the exit code is non-zero, lists verbatim every file CSharpier reports as unformatted. This set is the comparison basis for P3-T2. Do not run `csharpier format` in this task. Binary outcome: the measured exit code and unformatted set are recorded verbatim.
- [x] [P0-T9] Capture the analyzer build baseline. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\remediation-baseline\msbuild-analyzers.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording the error count and the warning count. Binary outcome: `EXIT_CODE: 0`.
- [x] [P0-T10] Capture the nullable/type-check build baseline. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\remediation-baseline\msbuild-nullable.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` that also records the known limitation from `code-review.2026-08-08T14-15.md` — MSBuild skips `CoreCompile` when only `/p:` values change, so exit 0 alone does not prove the tree is nullable-clean, and that limitation is issue **#512**, out of scope here. Binary outcome: `EXIT_CODE: 0`.
- [x] [P0-T11] Capture the full-suite test and coverage baseline for this cycle. Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug -CoverageOutput coverage\remediation-baseline.cobertura.xml` run from `<REPO>`. Acceptance: `coverage\remediation-baseline.cobertura.xml` exists (gitignored, never committed) and `<FEATURE>\evidence\remediation-baseline\tests-with-coverage.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` containing total/passed/failed/skipped test counts and the numeric root `<coverage>` attributes `line-rate`, `branch-rate`, `lines-covered`, and `lines-valid` read from the emitted XML. Binary outcome: zero skipped tests, and the failure set is recorded verbatim by fully-qualified name for use by P0-T13.
- [x] [P0-T12] Write helper script B to `<SCRATCH>\ConvertCoberturaToJacoco.ps1` per section 3.2 and project the P0-T11 report into a compact first-party summary. Command: `pwsh -NoProfile -File <SCRATCH>\ConvertCoberturaToJacoco.ps1 -Source coverage\remediation-baseline.cobertura.xml -Destination docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\evidence\remediation-baseline\coverage-remediation-baseline.jacoco.xml`. Acceptance: `<FEATURE>\evidence\remediation-baseline\coverage-remediation-baseline.jacoco.xml` exists and `<FEATURE>\evidence\remediation-baseline\coverage-projection.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, the verbatim script text, and an `Output Summary:` recording the aggregate first-party `LINE covered`/`LINE missed`/`BRANCH covered`/`BRANCH missed` totals and the derived LINE and BRANCH percentages. Binary outcome: the `.jacoco.xml` file exists, is under 100 lines, contains exactly nine `<package>` elements, and no raw `.cobertura.xml` file is written anywhere under `<FEATURE>\evidence\`.
- [x] [P0-T13] Record the pre-existing failing test set and the pass rule that depends on it. Acceptance: `<FEATURE>\evidence\remediation-baseline\preexisting-failures.<TS>.md` exists with `Timestamp:`, names by fully-qualified name every failing test observed in P0-T11, explicitly names `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` as a pre-existing order-dependent flake tracked by issue **#508** and out of scope, and states the rule verbatim: a Phase 3 test run passes when the only failures are members of this recorded set; any test not in this set that fails is a real regression that restarts Phase 3 at P3-T1; issue #508 must not be fixed in this cycle. Binary outcome: the artifact exists and enumerates the set explicitly, including the empty case if there were no failures.

---

### Phase 1 — F1 Non-Vacuous Assertion and Recorded Fail-Proof

This phase deliberately mutates `TaskMaster\Ribbon\RibbonExplorer.xml` between P1-T5 and P1-T8 to prove the corrected assertion can fail. Rule 8 of section 3 governs that window. Rule 7 governs the rebuild ordering: the embedded resource is only observable by the test after a rebuild, so every mutation is followed by a rebuild and an embedded-content assertion **before** any test is run.

- [x] [P1-T1] Replace the null-conditional assertion in `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` inside `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` with the two-step shape pinned in section 5.1: bind `elementsById[controlId].Attribute("getEnabled")` to a local, assert `NotBeNull` on it with a `because` reason naming the control id, then assert `Value.Should().Be(EngineCommandGetEnabledCallback, ...)`. Binary outcome: the method body contains zero occurrences of `?.` , contains both `.Should().NotBeNull(` and `.Value.Should()` `.Be(`, the `ContainKey` assertion above it is unchanged, `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` at approximately line 224 is byte-identical to its pre-change text, no other member of the file changes, and the file remains at or under 500 lines.
- [x] [P1-T2] Record the three-condition non-vacuity argument by source inspection. Acceptance: `<FEATURE>\evidence\regression-testing\f1-assertion-shape.<TS>.md` exists with `Timestamp:`, `Command:` naming the inspection performed, `EXIT_CODE: 0`, the post-change method body quoted verbatim, and an `Output Summary:` mapping each of the three required failure conditions from `remediation-inputs.2026-08-08T14-26.md` (attribute missing, attribute present with the wrong value, attribute present but empty) to the specific assertion line that fails for it, per the table in section 5.1. Binary outcome: all three conditions are mapped to a named assertion line.
- [x] [P1-T3] Rebuild the solution so the corrected test compiles and the **unmutated** ribbon resource is embedded, then assert the embedded content. Commands, in order: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'; Write-Host \"EXIT_CODE=$LASTEXITCODE\""` then `pwsh -NoProfile -File <SCRATCH>\Assert-EmbeddedRibbon.ps1 -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'`. Acceptance: `<FEATURE>\evidence\other\phase1-build-premutation.<TS>.md` exists with `Timestamp:`, both `Command:` lines, `EXIT_CODE: 0` for each, and an `Output Summary:` recording the helper output. Binary outcome: the build exits 0 and the helper reports `EMBEDDED_GETENABLED_COUNT=8`.
- [x] [P1-T4] Run the scoped ribbon-XML test class against the unmutated resource to establish the corrected assertion is green before the mutation. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /InIsolation /Settings:TaskMaster.runsettings /TestCaseFilter:'FullyQualifiedName~TaskMaster.Test.Ribbon.RibbonExplorerXmlTests'; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\regression-testing\f1-green-before-mutation.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` naming `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` as Passed with total/passed/failed/skipped counts. Binary outcome: `EXIT_CODE: 0` with zero failed and zero skipped.
- [x] [P1-T5] Apply the temporary mutation: delete exactly line 103 of `TaskMaster\Ribbon\RibbonExplorer.xml` — the `getEnabled="EngineCommand_GetEnabled"` attribute line inside the `<button id="TrainSpam" ...>` element — and change nothing else. Acceptance: `<FEATURE>\evidence\regression-testing\f1-mutation-applied.<TS>.md` exists with `Timestamp:`, `Command:` recording the edit and the verification, `EXIT_CODE: 0`, and an `Output Summary:` recording the file's `getEnabled="EngineCommand_GetEnabled"` occurrence count, its line count, and the `git diff --numstat -- TaskMaster/Ribbon/RibbonExplorer.xml` output. Binary outcome: the file contains exactly 7 occurrences, is 538 lines, and `git diff --numstat` reports `0` added and `1` deleted for that path.
- [x] [P1-T6] Rebuild so the **mutated** resource is re-embedded, then assert the embedded content. This task is the gate that prevents a false-negative fail-proof: without it the test would read a stale assembly still carrying eight attributes. Commands, in order: the same `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'` invocation as P1-T3, then `pwsh -NoProfile -File <SCRATCH>\Assert-EmbeddedRibbon.ps1 -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'`. If the helper still reports 8, force the resource refresh with `msbuild TaskMaster\TaskMaster.csproj /t:Rebuild /p:Configuration=Debug /p:Platform='Any CPU'` followed by the solution build, and record both invocations in the artifact. Acceptance: `<FEATURE>\evidence\regression-testing\f1-mutated-assembly.<TS>.md` exists with `Timestamp:`, every `Command:` executed, `EXIT_CODE: 0` for the final build, and an `Output Summary:` recording the helper output and the `EMBEDDED_ASSEMBLY_WRITETIME` value. Binary outcome: the helper reports `EMBEDDED_GETENABLED_COUNT=7`.
- [x] [P1-T7] [expect-fail] Run the scoped ribbon-XML test class against the mutated resource and record the failure. Command: identical to P1-T4. Acceptance: `<FEATURE>\evidence\regression-testing\f1-fail-proof.<TS>.md` exists with `Timestamp:`, `Command:`, a **non-zero** `EXIT_CODE:`, and an `Output Summary:` quoting the verbatim failure message for `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback`, recording total/passed/failed counts, listing every other test that failed as a consequence of the same mutation, and cross-referencing `<FEATURE>\evidence\regression-testing\f1-mutated-assembly.<TS>.md` as proof that the assembly under test carried the mutation. Binary outcome: `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` is reported **Failed**. If it is reported Passed, the fail-proof has not been demonstrated: return to P1-T6 and resolve the stale-assembly cause before proceeding.
- [x] [P1-T8] Restore the mutation. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git checkout -- TaskMaster/Ribbon/RibbonExplorer.xml; git status --porcelain -- TaskMaster/Ribbon/RibbonExplorer.xml; (Get-Content 'TaskMaster\Ribbon\RibbonExplorer.xml' | Measure-Object -Line).Lines; (Select-String -Path 'TaskMaster\Ribbon\RibbonExplorer.xml' -Pattern 'getEnabled=\"EngineCommand_GetEnabled\"' -AllMatches | Measure-Object).Count; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\regression-testing\f1-mutation-restored.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording the empty porcelain output, the line count, and the occurrence count. Binary outcome: `git status --porcelain -- TaskMaster/Ribbon/RibbonExplorer.xml` is empty, the file is 539 lines, and it contains exactly 8 occurrences. The permanent tree retains no part of the mutation.
- [x] [P1-T9] Rebuild so the restored resource is re-embedded, then assert the embedded content. Commands, in order: the same solution build as P1-T3, then `pwsh -NoProfile -File <SCRATCH>\Assert-EmbeddedRibbon.ps1 -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'`. Acceptance: `<FEATURE>\evidence\other\phase1-build-postrestore.<TS>.md` exists with `Timestamp:`, both `Command:` lines, `EXIT_CODE: 0` for each, and an `Output Summary:` recording the helper output. Binary outcome: the helper reports `EMBEDDED_GETENABLED_COUNT=8`.
- [x] [P1-T10] Re-run the scoped ribbon-XML test class and record the pass-after state. Command: identical to P1-T4. Acceptance: `<FEATURE>\evidence\regression-testing\f1-pass-after-restore.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` naming `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` as Passed, recording total/passed/failed/skipped, and cross-referencing the `f1-fail-proof.<TS>.md` path so the fail-then-pass pair is traceable from one artifact. Binary outcome: `EXIT_CODE: 0` with zero failed and zero skipped.

---

### Phase 2 — F2 Ribbon XML Line Restoration

- [ ] [P2-T1] Replace the six-line `TriageSetA`, `TriageSetB`, and `TriageSetC` `<button>` elements in `TaskMaster\Ribbon\RibbonExplorer.xml` (currently lines 448-465) with the three single lines pinned verbatim in section 5.2, each indented ten spaces. No other element, attribute, or line in the file changes; in particular no `menu`, `group`, or `tab` element is modified and the five already-multi-line engine-backed buttons are untouched. Binary outcome: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git diff --numstat -- TaskMaster/Ribbon/RibbonExplorer.xml"` reports exactly `3` added and `18` deleted for that path.
- [ ] [P2-T2] Verify the F2 size and attribute gates. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; (Get-Content 'TaskMaster\Ribbon\RibbonExplorer.xml' | Measure-Object -Line).Lines; (Select-String -Path 'TaskMaster\Ribbon\RibbonExplorer.xml' -Pattern 'getEnabled=\"EngineCommand_GetEnabled\"' -AllMatches | Measure-Object).Count; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\qa-gates\f2-xml-line-count.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording the measured line count against the merge-base 519, the post-implementation 539, and the 527 gate, plus the measured attribute count. Binary outcome: the file is **at or below 527 lines** (expected 524) and contains exactly **8** occurrences of `getEnabled="EngineCommand_GetEnabled"`.
- [ ] [P2-T3] Verify the file is still well-formed CustomUI. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; [xml]$d = Get-Content 'TaskMaster\Ribbon\RibbonExplorer.xml' -Raw; $d.DocumentElement.LocalName; $d.DocumentElement.NamespaceURI; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\qa-gates\f2-xml-wellformed.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording the parsed root local name and namespace URI. Binary outcome: the document parses without error, the root local name is `customUI`, and the namespace URI is `http://schemas.microsoft.com/office/2009/07/customui`.
- [ ] [P2-T4] Rebuild so the collapsed resource is re-embedded, then assert the embedded content reflects the single-line form. Commands, in order: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'; Write-Host \"EXIT_CODE=$LASTEXITCODE\""` then `pwsh -NoProfile -File <SCRATCH>\Assert-EmbeddedRibbon.ps1 -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'`. Acceptance: `<FEATURE>\evidence\other\phase2-build.<TS>.md` exists with `Timestamp:`, both `Command:` lines, `EXIT_CODE: 0` for each, and an `Output Summary:` recording the helper output. Binary outcome: the helper reports `EMBEDDED_GETENABLED_COUNT=8` **and** `EMBEDDED_TRIAGESETA_SINGLELINE=True`.
- [ ] [P2-T5] Run the scoped ribbon-XML test class against the collapsed resource. Command: identical to P1-T4. Acceptance: `<FEATURE>\evidence\regression-testing\f2-ribbon-xml-tests.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` naming `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` (AC5), `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` (AC6), `RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled` (AC7), and `RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer` (AC8) as Passed, with total/passed/failed/skipped counts. Binary outcome: `EXIT_CODE: 0` with zero failed and zero skipped.

---

### Phase 3 — Full QC Loop

Loop semantics: the order is format, then the read-only repo-wide format check, then the post-format size audit, then lint (analyzer build), then type-check (nullable build), then test-with-coverage, then the coverage and structural gates. This phase is **unconditional**: every task executes its stated command and records the real result. `EXIT_CODE: SKIPPED` is not a valid completion for any task in this phase. If any task fails, or if any task changes a `.cs`, `.csproj`, `.xml`, or `.sln` file on disk, fix the cause and restart the phase from P3-T1. Writing this phase's own evidence artifacts under `<FEATURE>\evidence\` is not an intervening file change. The phase is complete only when P3-T1 through P3-T12 all succeed within a single uninterrupted pass.

- [x] [P3-T1] Format the scope-locked C# file. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' format TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\qa-gates\csharpier-format.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` stating whether CSharpier rewrote the file. `csharpier pipe-files` is prohibited. `TaskMaster\AppGlobals\AppItemEngines.cs`, `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs`, and `TaskMaster\Ribbon\RibbonExplorer.xml` must not appear in the argument list, and the command must never be invoked repo-wide. Binary outcome: `EXIT_CODE: 0`.
- [x] [P3-T2] Verify formatting repo-wide, read-only. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' check .; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\qa-gates\csharpier-check.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` comparing the reported unformatted set against the P0-T8 baseline set. Binary outcome: either `EXIT_CODE: 0`, or a non-zero exit whose unformatted set is exactly the P0-T8 set and does not contain `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs`. If the scope-locked file is reported unformatted, restart the phase at P3-T1.
- [x] [P3-T3] Run the post-format file-size audit over every path in the working-tree change set. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git status --porcelain | ForEach-Object { $_.Substring(3) } | ForEach-Object { if (Test-Path $_) { '{0}={1}' -f $_, (Get-Content $_ | Measure-Object -Line).Lines } }"`. Acceptance: `<FEATURE>\evidence\qa-gates\file-size-audit.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` listing every changed path with its post-format line count, recording that Markdown documentation and evidence files under `docs/features/` and `.claude/agent-memory/` are exempt from the 500-line cap per `.claude/rules/general-code-change.md`, and recording `TaskMaster\Ribbon\RibbonExplorer.xml` explicitly against both the 519-line merge-base figure accepted by AC25 and the 527-line gate for this cycle. Binary outcome: every `.cs` path is at or under 500 lines, and `TaskMaster\Ribbon\RibbonExplorer.xml` is at or under 527 lines and strictly below its 539-line pre-remediation count.
- [x] [P3-T4] Run the analyzer gate. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\qa-gates\msbuild-analyzers.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording zero errors and no new analyzer diagnostics relative to the P0-T9 baseline. Binary outcome: `EXIT_CODE: 0` with no diagnostic absent from the P0-T9 baseline.
- [x] [P3-T5] Run the nullable/type-check gate. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\qa-gates\msbuild-nullable.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` that also restates the P0-T10 limitation and its routing to issue **#512**, which is out of scope for this cycle. Binary outcome: `EXIT_CODE: 0`.
- [x] [P3-T6] Run the full test suite with coverage. Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug -CoverageOutput coverage\remediation-final.cobertura.xml` run from `<REPO>`. Acceptance: `coverage\remediation-final.cobertura.xml` exists (gitignored, never committed) and `<FEATURE>\evidence\qa-gates\tests-with-coverage.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` containing total/passed/failed/skipped counts, the numeric root `<coverage>` attributes `line-rate`, `branch-rate`, `lines-covered`, and `lines-valid`, and an explicit reconciliation of every failure against the P0-T13 pre-existing set. Binary outcome: zero skipped tests, and either zero failed tests or a failure set that is a subset of the P0-T13 set. Any failure outside that set is a regression: fix it and restart the phase at P3-T1.
- [x] [P3-T7] Project the P3-T6 report into a compact committed summary using helper script B. Command: `pwsh -NoProfile -File <SCRATCH>\ConvertCoberturaToJacoco.ps1 -Source coverage\remediation-final.cobertura.xml -Destination docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\evidence\qa-gates\coverage-remediation-final.jacoco.xml`. Acceptance: `<FEATURE>\evidence\qa-gates\coverage-remediation-final.jacoco.xml` exists and `<FEATURE>\evidence\qa-gates\coverage-projection.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording the aggregate first-party `LINE covered`/`LINE missed`/`BRANCH covered`/`BRANCH missed` totals and the derived LINE and BRANCH percentages. Binary outcome: the `.jacoco.xml` file exists, is under 100 lines, contains exactly nine `<package>` elements, and no raw `.cobertura.xml` file is written anywhere under `<FEATURE>\evidence\`.
- [x] [P3-T8] Regenerate the canonical gate artifact. Command: `pwsh -NoProfile -File <SCRATCH>\ConvertCoberturaToJacoco.ps1 -Source coverage\remediation-final.cobertura.xml -Destination artifacts\csharp\coverage.xml`. Acceptance: `<FEATURE>\evidence\qa-gates\coverage-gate-artifact.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording the LINE and BRANCH percentages that `.claude\hooks\validate-feature-review-coverage.ps1` reads from the generated file, together with the note that `artifacts/` is gitignored (`.gitignore:57`) so this file is local-only and regenerated rather than committed. Binary outcome: `artifacts\csharp\coverage.xml` exists in JaCoCo format and its derived repo-wide LINE percentage is at or above 85 and BRANCH at or above 75.
- [x] [P3-T9] Produce the coverage comparison for this cycle. Acceptance: `<FEATURE>\evidence\qa-gates\coverage-comparison.<TS>.md` exists with `Timestamp:` and a table comparing three points on the same first-party nine-package denominator: the implementation-cycle post-change figures from `<FEATURE>\evidence\qa-gates\coverage-final.jacoco.xml` (LINE covered 95473 / missed 15734; BRANCH covered 22131 / missed 5795), the P0-T12 remediation baseline, and the P3-T7 remediation final. The artifact must state that this cycle changes one test file and one embedded XML resource and therefore is expected to leave production line coverage unchanged, must record the `TaskMaster` package `LINE` counter at all three points, must restate the denominator note from `<FEATURE>\evidence\qa-gates\coverage-artifact-substitution.2026-08-08T17-40.md` (vendored packages excluded by `coverage.config`; the unfiltered root figure is not the policy denominator), and must record the documented threshold conflict (CLAUDE.md 80 percent repo-wide / 90 percent new code versus `.claude/rules/general-unit-test.md` 85 percent line / 75 percent branch) as a known unresolved policy conflict rather than silently selecting one number. Binary outcome: the derived first-party LINE and BRANCH percentages at P3-T7 are greater than or equal to the P0-T12 values to two decimal places, and any shortfall is recorded with its cause rather than reported as a pass.
- [x] [P3-T10] Verify the AC15 zero-line diff across the working tree, not only across HEAD. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git diff --numstat 003c5715055d7d1933db68a742531332756e30b2 -- TaskMaster/AppGlobals/AppItemEngines.cs UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs TaskMaster/AppGlobals/ApplicationGlobals.cs; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. This form compares the working tree against the merge-base, so it catches an uncommitted edit that a `<MERGE_BASE>..HEAD` diff would miss. Acceptance: `<FEATURE>\evidence\qa-gates\zero-line-diff.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` quoting the command output verbatim. Binary outcome: the output is **empty** — none of the three protected paths differs from the merge-base.
- [x] [P3-T11] Verify the source scope lock. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git status --porcelain"`. Acceptance: `<FEATURE>\evidence\qa-gates\scope-lock-audit.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` quoting the porcelain output verbatim and classifying every entry into exactly one of four buckets: **(a)** a section 4.1 source path; **(b)** a section 4.2 documentation or evidence path, which includes any file or collapsed directory entry under `<FEATURE>\evidence\` regardless of extension; **(c)** a pre-existing uncommitted path carried in from the review cycle, which this cycle neither created nor modified, verified by its presence in the P0-T5 `git-state.<TS>.md` porcelain; or **(d)** a violation. Binary outcome: **outside `<FEATURE>\evidence\`** the only `.cs`, `.csproj`, `.xml`, or `.sln` paths present are `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` and `TaskMaster/Ribbon/RibbonExplorer.xml`; every bucket (c) entry also appears in the P0-T5 porcelain; bucket (d) is empty; and no `coverage/` or `artifacts/` path appears (both are gitignored).
- [x] [P3-T12] Record the single uninterrupted clean toolchain pass. Acceptance: `<FEATURE>\evidence\qa-gates\toolchain-clean-pass.<TS>.md` exists with `Timestamp:` and, in order, the artifact path plus `EXIT_CODE:` for P3-T1, P3-T2, P3-T4, P3-T5, and P3-T6, together with an explicit statement that all five ran in one pass with no intervening `.cs`, `.csproj`, `.xml`, or `.sln` change and no restart. Binary outcome: the recorded sequence contains no restart.

---

### Phase 4 — Commit and Post-Commit Verification

- [x] [P4-T1] Append one new subsection to `<FEATURE>\spec.md` at the end of `## Delivery Notes and Deviations`, titled `### Remediation Cycle 1 — 2026-08-08T14-26`, recording: F1 (the AC5 assertion made non-vacuous, with the fail-proof evidence path `evidence/regression-testing/f1-fail-proof.<TS>.md` and the restoration evidence path `evidence/regression-testing/f1-mutation-restored.<TS>.md`); F2 (the three `TriageSet*` buttons restored to single-line form, moving `RibbonExplorer.xml` from 539 to its measured post-change line count, which supersedes the 539 figure while leaving the AC25 pre-existing 519-line exception unchanged in kind); and that no acceptance criterion changed state in this cycle. Binary outcome: exactly one new subsection is appended, `git diff -- docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/spec.md` shows zero deleted lines, and no `- [ ]` or `- [x]` marker anywhere in the file changes state.
- [x] [P4-T2] Verify AC19, AC20, and AC21 remain unchecked in `<FEATURE>\spec.md`. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; Select-String -Path 'docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\spec.md' -Pattern '\*\*AC19|\*\*AC20|\*\*AC21'; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\qa-gates\manual-only-unchecked.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` quoting the three criterion lines verbatim. Binary outcome: all three lines still begin `- [ ] **AC19`, `- [ ] **AC20`, and `- [ ] **AC21`.
- [x] [P4-T3] Commit the remediation so the reaudit diff observes it. Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git add -A; git commit -m 'fix(#503): make the AC5 ribbon-XML assertion non-vacuous and restore RibbonExplorer.xml line count'; git rev-parse HEAD; git status --porcelain; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. This task may execute only after the P1-T8 restoration artifact records an empty porcelain for `TaskMaster/Ribbon/RibbonExplorer.xml`; committing while the P1-T5 mutation is present is a hard failure. Acceptance: `<FEATURE>\evidence\qa-gates\remediation-commit.<TS>.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording the new HEAD SHA and the post-commit porcelain output. Binary outcome: `git status --porcelain` is empty after the commit.
- [x] [P4-T4] Re-verify the protected-path and scope invariants against the committed tree. Two commands, in order. **First**, the protected-path check, path-scoped so the enclosing branch diff cannot mask it: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git diff --numstat 003c5715055d7d1933db68a742531332756e30b2..HEAD -- TaskMaster/AppGlobals/AppItemEngines.cs UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs TaskMaster/AppGlobals/ApplicationGlobals.cs; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. **Second**, the scope check over the **remediation commit's own diff** — not the whole-branch diff, which necessarily contains every implementation-cycle path and would make an unscoped gate unsatisfiable: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git show --numstat --format= HEAD; Write-Host \"EXIT_CODE=$LASTEXITCODE\""`. Acceptance: `<FEATURE>\evidence\qa-gates\zero-line-diff-postcommit.<TS>.md` exists with `Timestamp:`, both `Command:` lines, `EXIT_CODE: 0` for each, and an `Output Summary:` containing both outputs verbatim, an explicit statement that `TaskMaster/AppGlobals/AppItemEngines.cs`, `UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs`, and `TaskMaster/AppGlobals/ApplicationGlobals.cs` are absent from the first output, and a classification of every path in the second output into the four P3-T11 buckets. Binary outcome: the first command's output is **empty**; and in the second command's output, outside `<FEATURE>\evidence\` the only `.cs`, `.csproj`, `.xml`, or `.sln` paths are `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` and `TaskMaster/Ribbon/RibbonExplorer.xml`, with no bucket (d) entry.
- [x] [P4-T5] Record the reaudit handoff. Acceptance: `<FEATURE>\evidence\qa-gates\remediation-reaudit-handoff.<TS>.md` exists with `Timestamp:`, the post-commit HEAD SHA from P4-T3, and one row per finding: **F1** marked resolved with pointers to `evidence/regression-testing/f1-assertion-shape.<TS>.md`, `evidence/regression-testing/f1-fail-proof.<TS>.md`, `evidence/regression-testing/f1-mutation-restored.<TS>.md`, and `evidence/regression-testing/f1-pass-after-restore.<TS>.md`; and **F2** marked resolved with pointers to `evidence/qa-gates/f2-xml-line-count.<TS>.md`, `evidence/qa-gates/f2-xml-wellformed.<TS>.md`, and `evidence/regression-testing/f2-ribbon-xml-tests.<TS>.md`. The artifact must also state that AC19, AC20, and AC21 remain unchecked, that no acceptance criterion changed state in this cycle, and that the out-of-scope items in section 6 were not touched. Binary outcome: both findings are marked resolved with at least one evidence pointer each, and the blocking-finding count entering the reaudit is recorded as 0.

---

## 8. Traceability

| Finding | Source | Phase | Verifying tasks | Binary gate |
|---|---|---|---|---|
| F1 — vacuous AC5 assertion | `remediation-inputs.2026-08-08T14-26.md` §F1; `code-review.2026-08-08T14-15.md` row 1 | 1 | P1-T1, P1-T2, P1-T7, P1-T8, P1-T10 | The corrected test is reported **Failed** with the resource mutated (P1-T7) and **Passed** with it restored (P1-T10); the permanent tree retains no mutation (P1-T8) |
| F2 — `RibbonExplorer.xml` line growth | `remediation-inputs.2026-08-08T14-26.md` §F2; `code-review.2026-08-08T14-15.md` row 2 | 2 | P2-T1, P2-T2, P2-T3, P2-T4, P2-T5 | File at or below **527** lines with exactly **8** `getEnabled="EngineCommand_GetEnabled"` attributes, well-formed CustomUI, AC5/AC6/AC7/AC8 tests passing |
| AC15 zero-line diff preserved | `spec.md` AC15; `remediation-inputs.2026-08-08T14-26.md` out-of-scope list | 3, 4 | P3-T10, P4-T4 | `git diff --numstat` against the merge-base is empty for all three protected paths |
| AC19/AC20/AC21 remain MANUAL-ONLY | `spec.md`; section 3 rule 11 | 4 | P4-T2 | All three lines still begin `- [ ]` |
| Full QC loop | CLAUDE.md CUT3; `.claude/rules/csharp.md` | 3 | P3-T1, P3-T2, P3-T4, P3-T5, P3-T6, P3-T12 | Five gate commands pass in one uninterrupted pass with no restart |
| Coverage evidence | `.claude/skills/atomic-plan-contract` coverage contract | 0, 3 | P0-T11, P0-T12, P3-T6, P3-T7, P3-T8, P3-T9 | Numeric baseline and post-change coverage recorded; compact JaCoCo summaries committed; canonical gate artifact regenerated |

---

## 9. Recorded decisions

1. **The fail-proof mutates the resource, not the test.** F1's acceptance in `remediation-inputs.2026-08-08T14-26.md` requires a deliberate temporary removal of one `getEnabled` attribute from the embedded ribbon resource. Mutating the resource, rather than writing a second test over a synthetic in-memory XML fragment, is what proves the assertion is non-vacuous **against the real embedded artifact the test actually reads**. A synthetic-document test would prove the FluentAssertions shape without proving the resource-loading path.
2. **The rebuild step is a plan-level gate, not an implementation detail.** Because the XML is an embedded resource, an edit-then-run sequence would read a stale assembly and report a false Pass, silently converting the fail-proof into a second vacuous check. P1-T6 asserts the embedded byte content **before** the failing run is attempted, which is the only way to distinguish "the assertion cannot fail" from "the assembly was stale". P1-T6 also carries an explicit `/t:Rebuild` fallback for the case where MSBuild's incremental resource check does not pick up the edit.
3. **`git checkout --` is the restoration mechanism.** It is exact and deterministic because Phase 1 makes no other change to `RibbonExplorer.xml` and Phase 2 has not yet run. A hand re-insertion of the deleted line would be a second opportunity to introduce a whitespace or ordering difference.
4. **`TrainSpam` is the mutation target.** It is already in multi-line form at lines 99-105, so the mutation is a single whole-line deletion with no re-indentation, which makes the `git diff --numstat` gate in P1-T5 exactly `0 1`. Mutating one of the `TriageSet*` buttons would entangle F1's fail-proof with F2's edit.
5. **The three `TriageSet*` buttons are collapsed, not the other five.** `TrainSpam`, `TrainHam`, `TestSpam`, `FilterTriageGroup`, and `ClearTriage` were already multi-line at the merge-base. Collapsing them would be gratuitous churn on lines this change did not author and would exceed the F2 remit, which is limited to reverting incidental reformatting introduced by the implementation cycle.
6. **The 527 gate is a ceiling, not a target.** The pinned edit yields 524 lines (539 minus fifteen). The gate is expressed as "at or below 527" because that is the figure `remediation-inputs.2026-08-08T14-26.md` binds (519 merge-base plus 8 functional attribute lines), and expressing the gate as equality against 524 would make an unrelated future whitespace normalization a spurious failure.
7. **No acceptance criterion is checked off.** Every automated AC was already checked off in the implementation cycle, and F1/F2 are quality defects in delivered work rather than unmet criteria. Re-flipping a checkbox would produce a misleading audit trail. The `spec.md` change in P4-T1 is append-only prose.
8. **Coverage evidence stays compact.** Raw Cobertura reports for this repository are roughly 187,000 lines and 10 MB each. Following the convention established in `<FEATURE>\evidence\qa-gates\coverage-artifact-substitution.2026-08-08T17-40.md`, the raw reports are written to the gitignored `coverage\` directory and only the package-level JaCoCo projections are committed. The projection scripts are session-throwaway scripts held outside the working tree, and their verbatim text is recorded in the P0-T12 and P3-T7 evidence artifacts so the projection remains auditable and reproducible without adding a tracked script file to the repository.
9. **The AC15 gate compares the working tree, not HEAD.** P3-T10 uses `git diff --numstat <MERGE_BASE> -- <paths>` without `..HEAD` so that an uncommitted edit to a protected file is caught before the commit rather than after. P4-T4 repeats the check in path-scoped `<MERGE_BASE>..HEAD -- <paths>` form once the commit exists, and audits scope against the remediation commit's own diff (`git show --numstat --format= HEAD`) rather than the whole-branch diff, which necessarily contains every implementation-cycle path.
