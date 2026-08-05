# svg-renderer-null-document-nre — Remediation Plan, Cycle 2

- **Issue:** #418
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-05T05-00
- **Status:** Draft
- **Version:** 1.0 (initial authoring of this cycle's remediation plan)
- **Work Mode:** `minor-audit` (persisted marker `- Work Mode: minor-audit` at `issue.md:12`)
- **Language in scope:** C# only (build configuration only; **no `.cs` file is modified by this plan**)
- **Cycle:** remediation cycle 2
- **Cycle entry inputs:** `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-inputs.2026-08-04T22-28.md`
- **Branch / HEAD at authoring:** `bug/svg-renderer-null-document-nre-418` @ `ad608825` (working tree clean;
  `git status --porcelain` empty). `ad608825` commits the carried-in reaudit state that was uncommitted at
  `a62391f7` — four audit artifacts and three feature-review memory files. This plan encodes **no** permitted-dirt
  exception: `[P0-T5]` and `[P1-T7]` remain strict, and a non-empty porcelain output at execution time is a halt,
  not an enumerated allowance.
- **Base:** `origin/main` @ `ce0c91e6`
- **Evidence series for this cycle:** `2026-08-05T05-00` (no existing artifact is overwritten)
- **Plan shape:** minimal-audit contract — exactly three phases

**Two prior plans are read-only for the whole of this cycle.** `plan.2026-08-04T14-36.md` is complete at 46/46
and `remediation-plan.2026-08-05T01-50.md` is complete at 40/40. **No task in this plan may modify either
file.** `[P0-T5]` records their untouched state and `[P2-T12]` re-confirms it at exit.

**Fail-closed evidence rule:** every command-bearing task names its exact command and its artifact path. If any
required baseline artifact, QC artifact, or coverage artifact is missing or incomplete, the verdict is BLOCKED
or INCOMPLETE, never PASS.

**Evidence accounting rule:** do not mark an evidence-backed task complete without the artifact on disk carrying
`Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

**Evidence location:** every artifact this plan writes resolves to
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/<kind>/` per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. The cycle-entry inputs and the orchestrator
directive supplied only canonical paths, so there is no override to reject. Any instruction naming
`artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/coverage/`, or
`artifacts/evidence/` as an evidence destination is rejected by this plan.

## Scope of This Cycle — Exactly One Finding

`feature-review` returned PARTIAL at `a62391f7` with blocking count 2. This plan delivers **one item**: the
missing `ExCSS` reference on `SVGControl.Test`, which makes six of its 75 tests pass or fail depending on
`vstest.console.exe` argument order.

### Item label reconciliation (read this before mapping tasks to findings)

The cycle-entry inputs enumerate the missing-`ExCSS` finding as **R-7** (`remediation-inputs.2026-08-04T22-28.md`
§ `### R-7`, sourced from code review CR-8, policy audit G-8, feature audit AC-10 PARTIAL). The orchestrator
directive that commissioned this plan refers to the same finding as **R-11**. In the inputs, `R-11` is a
different, non-blocking item: adding `<Private>True</Private>` to the `Svg` reference, which the inputs
explicitly recommend bundling with R-7 because both edit the same `<ItemGroup>`.

This plan resolves the collision by delivering **both readings**, which is possible because both fall inside the
same Scope Lock and neither can conflict with the other:

| Item | Inputs label | Delivered by |
|---|---|---|
| Missing `ExCSS` reference on `SVGControl.Test` (the blocking finding) | R-7 | `[P1-T1]`, `[P1-T2]` |
| `<Private>True</Private>` on the existing `Svg` reference | R-11 | `[P1-T3]` |

No other item is delivered. The label discrepancy is recorded here so a reaudit does not read either item as
unaddressed.

### Explicitly excluded (binding)

- **R-1 / AC-11 — the human WinForms-designer runbook.** Excluded and represented by **no task**. No agent can
  execute it: it requires opening `UtilitiesCS/Dialogs/MyBoxViewer.cs` in the Visual Studio WinForms designer
  and observing the load. It is tracked as human_interaction requirements H-1 and H-2 with response `exception`
  and a `runbook_path`. **No task in this plan may check off AC-11**, and no automated evidence substitutes for
  the human capture at `evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`.
- **G-9 — `SVGControl/SvgAssemblyResolver.cs` file-level coverage floor (61.6279%).** Dispositioned non-blocking
  by the reviewer, who stated it needs a maintainer decision rather than code. It is being surfaced to the user,
  not remediated. **No task targets it.** In particular, no task may relocate a testable member into that file
  to lift its ratio; that would game the metric rather than measure behavior.
- **G-1 — `SVGControl/SvgRenderer.cs` file-level floor (80.1932%).** Residual is pre-existing members outside
  issue #418, already owned by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`.
- **R-12 — repository-level nullable-gate vacuity.** Not this feature's to fix.
- **R-8, R-9, R-10.** All three modify production `.cs` files, which the Scope Lock below forbids.

## The Finding, and Why the Fix Is a Reference

`SVGControl.Test/bin/Debug` contains `Svg.dll` but not `ExCSS.dll`. `Svg.dll`'s manifest depends on `ExCSS`, but
the dependency is never copied into the test project's output, because legacy non-SDK `packages.config` projects
do not flow a `ProjectReference`'s package assemblies transitively, and `ExCSS.dll` does not sit beside
`Svg.dll` under `packages/Svg.3.4.8/lib/net481/` for `ResolveAssemblyReference` to find. The test host's
probing path follows the directory of the **first** assembly on the `vstest.console.exe` command line, and all
eight sibling test projects do carry `ExCSS.dll`. Hence the measured asymmetry recorded in
`remediation-inputs.2026-08-04T22-28.md` § R-7: `SVGControl.Test` alone → 6 failed; `SVGControl.Test` then a
sibling → 6 failed; a sibling then `SVGControl.Test` → 76 passed.

The `app.config` redirect that AC-10 corrected cannot help, because redirection presupposes the file is
findable, and the `AssemblyResolve` fallback in `SVGControl/SvgAssemblyResolver.cs` probes that same absent
output directory.

**This is the second instance of a class this branch already fixed once.** Task `[P1-T4]` of
`plan.2026-08-04T14-36.md` added a `<Reference>` plus a `packages.config` entry for `Svg` for exactly this
reason. The `Svg` case was caught because test source names `SvgDocument` at compile time; the `ExCSS` case was
missed because no source names `ExCSS`, so it manifests only at runtime probing. This plan mirrors that
precedent.

## Design Decisions Fixed by This Plan

**Decision 1 — Reference identities are derived from disk, not from prose.** The identity strings, versions, and
hint paths below were read from the repository at authoring time and must be reproduced verbatim. The snippet
quoted in `remediation-inputs.2026-08-04T22-28.md` § R-7 is **not** authoritative and contains one factual error
(see Decision 3).

Derived `ExCSS` facts, with the on-disk source of each:

| Fact | Value | Derived from |
|---|---|---|
| Assembly identity | `ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL` | `SVGControl/SVGControl.csproj:55` (verbatim) |
| Public key token | `bdbe16be9b936b9a` | `SVGControl/SVGControl.csproj:55`; matches `SVGControl.Test/app.config:22` |
| `HintPath` | `..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll` | `SVGControl/SVGControl.csproj:56`; the file exists at `packages/ExCSS.4.3.2/lib/net48/ExCSS.dll` |
| Package version | `4.3.2` | `SVGControl/packages.config:3`; `packages/ExCSS.4.3.2/` is the only ExCSS package directory on disk |
| `targetFramework` | `net481` | every entry in `SVGControl.Test/packages.config` |
| Binding redirect | already present and consistent: `oldVersion="0.0.0.0-4.3.2.0" newVersion="4.3.2.0"` | `SVGControl.Test/app.config:22-24` |

`ExCSS.4.3.2` has no `lib\net481` folder — its .NET Framework asset is `lib\net48`. `net48` is therefore correct
for a `v4.8.1` project and matches the production precedent.

**Decision 2 — `<Private>True</Private>` is mandatory on the new `ExCSS` reference.** Copy-local is the entire
mechanism of this fix. Every other `HintPath`-resolved reference in the target `<ItemGroup>` carries it
explicitly; the `Svg` reference is the sole exception, which is what `[P1-T3]` corrects.

**Decision 3 — `Fizzler` is deliberately NOT added. This plan departs from the snippet in the cycle inputs, and
the departure is measured, not stylistic.** The inputs direct adding a `Fizzler` reference "for parity with the
eight sibling test projects". That justification does not hold on disk:

1. **No test project references `Fizzler`.** A repository-wide search of `*.csproj` finds `Fizzler` references
   only in `SVGControl/SVGControl.csproj:58` and `UtilitiesCS/UtilitiesCS.csproj:63`, both production projects.
   A search of every `packages.config` finds `Fizzler` entries only in `SVGControl/packages.config:4` and
   `UtilitiesCS/packages.config:11`.
2. **No test project's output contains `Fizzler.dll`.** The glob `*.Test/bin/Debug/Fizzler.dll` returns zero
   files. By contrast `*.Test/bin/Debug/ExCSS.dll` returns eight files — every test project except
   `SVGControl.Test`. Adding `Fizzler` would make `SVGControl.Test` the **only** test project carrying it, which
   is divergence from the siblings, not parity with them.
3. **`Fizzler` is empirically unnecessary.** The passing ordering measured by the reviewer (sibling first, 76
   passed) probed a directory that contains `ExCSS.dll` and **no** `Fizzler.dll`. `ExCSS` alone is therefore
   demonstrated sufficient for all 76 tests.
4. **Adding it carries a new, real risk.** The on-disk `Fizzler` identity is `Version=1.3.1.0`
   (`SVGControl/SVGControl.csproj:58`), **not** the `Version=1.3.0.0` the inputs' snippet states, while
   `SVGControl.Test/app.config:26-28` redirects `Fizzler` `0.0.0.0-1.3.0.0 → 1.3.0.0`. Placing a `1.3.1.0`
   assembly into an output directory governed by a redirect that terminates at `1.3.0.0` activates a stale
   redirect that is currently inert precisely because no `Fizzler.dll` is present. The only remedies would be an
   `app.config` edit — forbidden by `remediation-inputs.2026-08-04T22-28.md` § `## Do Not Do`, and deferred to
   `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md` — or reverting the
   reference.

The minimal change that fixes the measured defect adds `ExCSS` only. If the `[P1-T5]` standalone run does not
reach 75/75 with `ExCSS` alone, `[P1-T5]` halts and reports rather than expanding scope; see the halt rule in
that task.

**Decision 4 — No `app.config` file is modified.** Binding, from the `## Do Not Do` list.

**Decision 5 — Baseline strategy: reuse the `2026-08-05T01-50` series as the comparison basis; capture fresh
only what no artifact on disk holds.** The orchestrator directive authorizes this, and the reasoning is stated
here as it requires:

- The `evidence/qa-gates/*.2026-08-05T01-50.md` series was committed **in** `a62391f7` and records the end state
  of that commit's source tree. The current HEAD is `ad608825`, one commit later. **Reuse remains valid because
  `ad608825` changes no input to any of those gates:** its diff contains only markdown under
  `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/` and agent-memory files under
  `.claude/agent-memory/`, and **no `.cs`, no `.csproj`, no `packages.config`, and no `app.config` change**.
  The source and build-configuration tree at `ad608825` is therefore byte-identical to the tree that series
  describes, so the formatting, analyzer, nullable, and coverage figures are unaffected by the intervening
  commit. `[P0-T5]` re-verifies this by recording the HEAD SHA and an empty porcelain output before any
  comparison is drawn.
- That series is the correct comparison basis for the analyzer inventory, the nullable diagnostic tables, the
  formatting state, and the coverage headline figures. `[P0-T10]` and `[P0-T11]` transcribe those numbers into
  fresh artifacts so this cycle's comparisons are like-for-like and no figure in this plan is a placeholder.
- Re-running the full nine-assembly coverage suite as a *baseline* would produce numbers identical to that
  series, for the reason above. It is not re-derived.
- **What must be captured fresh, because it exists in no artifact on disk:** the order-dependence measurement on
  this host and in this session (`[P0-T7]`, `[P0-T8]`), and the build-configuration and output-directory census
  that establishes the mechanism (`[P0-T9]`). These are the before-halves of this cycle's only proof.
- The toolchain bootstrap is session-scoped and must be re-verified regardless (`[P0-T1]`).
- The `evidence/baseline/*.2026-08-04T14-36.md` series must **not** be used as a comparison basis: it was
  captured on a host lacking the VSTO runtime assemblies and its diagnostic set includes `CS0234`/`MSB3245`
  failures that do not occur on the current host.

**Decision 6 — The order-dependence proof, not the nine-assembly run, is the decisive verification.** A full
nine-assembly run passes with or without this fix, which is exactly why the defect survived two audits. The
acceptance clauses that gate this cycle are `[P1-T5]` (standalone, 75 total / 75 passed / 0 failed),
`[P1-T6]` (two assemblies, `SVGControl.Test.dll` **first**, 0 failed), and their re-confirmation inside the
final clean pass at `[P2-T9]`.

**Decision 7 — The order-proof runs use bare `vstest.console.exe` invocations.** `scripts/vscode/Invoke-MSTest.ps1`
throws under `Set-StrictMode` when a single assembly matches its search (a scalar `.Count` defect, already filed
at `docs/features/potential/2026-08-04-invoke-mstest-scalar-count-strictmode.md`), so the wrapper cannot express
a single-assembly or a two-assembly ordered run. The repo-wide runs use
`scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`, where `-SearchRoot .` is mandatory. The order-proof
runs pass no `/EnableCodeCoverage`, no `/InIsolation`, and no `/Settings`, so they reproduce the reviewer's
measured command form exactly; changing the switch set could change probing behavior and would invalidate the
before/after comparison.

## Scope Lock (files this plan is permitted to change)

Line numbers are indicative only; locate each target by the quoted content.

Build configuration — the only functional change in this cycle:

- `SVGControl.Test/SVGControl.Test.csproj` — three additions only: the `ExCSS` `<Reference>` block
  (`[P1-T1]`) and the single `<Private>True</Private>` child on the existing `Svg` reference (`[P1-T3]`). No
  other property, item, or target may change.
- `SVGControl.Test/packages.config` — one added `<package>` line only (`[P1-T2]`).

Documentation and evidence:

- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` — **append-only evidence note on
  AC-10 only** (`[P2-T11]`). No AC text may be rewritten, no `- [ ]` may become `- [x]`, and no `- [x]` may
  become `- [ ]`.
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-plan.2026-08-05T05-00.md`
  (this file; checkbox state and preflight revision only).
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/**`.

**Explicitly out of scope** (binding, from `remediation-inputs.2026-08-04T22-28.md` § `## Do Not Do`):

- `plan.2026-08-04T14-36.md` and `remediation-plan.2026-08-05T01-50.md`. Read-only for this entire cycle.
- **Every `.cs` file in the repository.** This cycle changes no production and no test source. If any task
  appears to require a `.cs` edit, halt and report instead.
- **Every `app.config` file**, including `SVGControl.Test/app.config` and `SVGControl/app.config`. No binding
  redirect may be added, removed, or retargeted.
- AC-11 / R-1. No task may check it off or attempt to automate it.
- Weakening, retargeting, or deleting any assertion. In particular the `XmlException` assertions in
  `TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException` and
  `GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner` are correct as written; this cycle is
  what makes them hold unconditionally.
- `[ExcludeFromCodeCoverage]` on any production file and any `coverage.config` exclusion.
  `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy makes excluding a production source path a
  **Blocking** finding.
- The 195 pre-existing `UtilitiesCS` nullable diagnostics.
- Any `<NoWarn>`, `#pragma warning disable`, or `.editorconfig` severity change used to make a diagnostic
  disappear.
- Any edit under `.claude/rules/` or `.github/instructions/`.
- Temporary files in tests.
- `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/coverage/`,
  `artifacts/evidence/` as evidence destinations.

## Required References

- `CLAUDE.md` (standing instructions; policy compliance order and C# toolchain order)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` — the `## Acceptance Criteria`
  section (AC-1 through AC-11) is the **sole** requirements source for this feature
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-inputs.2026-08-04T22-28.md`
  — the enumerated fix list and the binding `## Do Not Do` list
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/code-review.2026-08-04T22-28.md` (CR-8)
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/policy-audit.2026-08-04T22-28.md` (G-8)
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/feature-audit.2026-08-04T22-28.md` (AC-10)
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md` — **read-only**;
  cited for the `Svg` reference precedent at its `[P1-T4]`
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-plan.2026-08-05T01-50.md` —
  **read-only**; its `evidence/qa-gates/*.2026-08-05T01-50.md` series is this cycle's comparison basis
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`

**All work must comply with these policies; do not duplicate their content here.**

## Work-Mode Notes (minor-audit, fail-closed)

- `spec.md` and `user-story.md` are **intentionally absent** from this feature folder and must **not** be
  required by any task, validation, or audit. If either is found to exist, execution fails closed and the
  orchestrator must be notified before any Phase 1 task begins (`[P0-T3]`).
- If the `## Acceptance Criteria` section is missing from `issue.md`, execution fails closed (`[P0-T3]`).
- AC-1 through AC-10 are already `[x]`. This cycle changes **no AC check state**. It appends one dated
  evidence note to AC-10 only (`[P2-T11]`).
- **AC-11 stays `- [ ]`.** It is R-1, excluded from this plan.

## Environment Precondition (why Phase 0 begins with a bootstrap task)

`global.json` pins SDK `8.0.205` with `"paths": [".dotnet-sdk", "$host$"]`, and `.dotnet-sdk/` does not exist in
a fresh checkout. In that state `dotnet tool run csharpier --version` fails with an instruction to run
`scripts/vscode/Install-RepoDotNetSdk.ps1`. That script does not perform `dotnet tool restore`, so csharpier
(manifest at repo-root `dotnet-tools.json`) must be restored separately. Independently, `dotnet-coverage` may
not be present in `~/.dotnet/tools`; `scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws without it. Without
the bootstrap, `[P2-T1]`, `[P2-T2]` (csharpier) and `[P0-T11]`, `[P2-T7]` (coverage) cannot run. `[P0-T1]`
exists solely to remove this precondition.

### Phase 0 — Remediation Baseline Capture and Compliance Reads

Artifact directory for this phase:
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/`.

- [ ] [P0-T1] Bootstrap the repo-local toolchain so the csharpier and coverage tasks in this plan can run: run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Install-RepoDotNetSdk.ps1` if `.dotnet-sdk/` is absent, then `dotnet tool restore` from the repository root, then `dotnet tool install --global dotnet-coverage` if `dotnet-coverage --version` fails. Acceptance: `dotnet tool run csharpier --version` and `dotnet-coverage --version` both return exit 0, and artifact `evidence/remediation-baseline/toolchain-bootstrap.2026-08-05T05-00.md` records each command, its `EXIT_CODE:`, `Output Summary:`, and the two resolved version strings
- [ ] [P0-T2] Read `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md` in that exact order, in full. Acceptance: artifact `evidence/remediation-baseline/phase0-instructions-read.2026-08-05T05-00.md` exists carrying `Timestamp:`, `Policy Order:`, and the explicit list of the four files read
- [ ] [P0-T3] Read `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` in full and confirm four facts: it contains an explicit `## Acceptance Criteria` section with AC-1 through AC-11; it contains the marker `- Work Mode: minor-audit`; neither `spec.md` nor `user-story.md` exists in `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/`; and AC-1 through AC-10 are `[x]` while AC-11 is `[ ]`. Any failed confirmation halts execution and is reported to the orchestrator before `[P1-T1]`. Acceptance: artifact `evidence/remediation-baseline/ac-source-check.2026-08-05T05-00.md` records all four confirmations with the quoted evidence line for each
- [ ] [P0-T4] Read in full, in this order: `remediation-inputs.2026-08-04T22-28.md`, `code-review.2026-08-04T22-28.md`, `policy-audit.2026-08-04T22-28.md`, and `feature-audit.2026-08-04T22-28.md`. Acceptance: artifact `evidence/remediation-baseline/cycle-inputs-read.2026-08-05T05-00.md` lists the four files, reproduces verbatim the `## Do Not Do` list from `remediation-inputs.2026-08-04T22-28.md` as the binding constraint set for this cycle, and restates the R-7 / R-11 label reconciliation from this plan's § Scope of This Cycle
- [ ] [P0-T5] Record the tree state this cycle starts from: run `git rev-parse HEAD`, `git status --porcelain`, `git diff --stat HEAD -- docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`, and `git diff --stat HEAD -- docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-plan.2026-08-05T01-50.md`. Acceptance: artifact `evidence/remediation-baseline/tree-state.2026-08-05T05-00.md` records the HEAD SHA (expected `ad608825`), the porcelain output (expected **empty**), an empty diff for both prior plan files, and the statement that both are read-only for this cycle. If the porcelain output is non-empty for any file outside this plan's Scope Lock, halt and report before `[P1-T1]`. This clause is strict by design: there is no carried-in permitted-dirt set, and the executor must not revert or otherwise act on another agent's files — halting and reporting is the only permitted response
- [ ] [P0-T6] Resolve the absolute path to `vstest.console.exe` for the order-proof runs by running `& (Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe') -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'` from the repository root, which is the same resolution `scripts/vscode/Invoke-MSTest.ps1:102` performs. Acceptance: artifact `evidence/remediation-baseline/vstest-path.2026-08-05T05-00.md` records the command, `EXIT_CODE:`, and the single resolved absolute path, and confirms the file exists. That path is `<VSTEST>` in `[P0-T7]`, `[P0-T8]`, `[P1-T5]`, `[P1-T6]`, and `[P2-T9]`
- [ ] [P0-T7] [expect-fail] Capture the pre-change standalone baseline: build nothing and change nothing, then run `& '<VSTEST>' SVGControl.Test\bin\Debug\SVGControl.Test.dll` from the repository root, passing no `/EnableCodeCoverage`, no `/InIsolation`, and no `/Settings`. A non-zero exit code is the **expected** measurement outcome here, not a task failure. Acceptance: artifact `evidence/remediation-baseline/order-standalone.2026-08-05T05-00.md` records the command, `EXIT_CODE:`, `Output Summary:`, the total/passed/failed counts (expected 75 total, 69 passed, 6 failed), the name of every failed test, and the full text of the assembly-load exception cited in at least one failure including the requested assembly identity. If the failed count is zero on this host, the defect does not reproduce here: halt, record that outcome in the artifact, and report to the orchestrator before `[P1-T1]` — do not proceed to a fix whose before-state cannot be demonstrated
- [ ] [P0-T8] [expect-fail] Capture the pre-change two-assembly asymmetry with two runs of the identical binaries, differing only in argument order. Run A: `& '<VSTEST>' SVGControl.Test\bin\Debug\SVGControl.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll`. Run B: `& '<VSTEST>' VBFunctions.Test\bin\Debug\VBFunctions.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll`. A non-zero exit code for Run A is the expected measurement outcome. Acceptance: artifact `evidence/remediation-baseline/order-paired.2026-08-05T05-00.md` records both commands, both `EXIT_CODE:` values, `Output Summary:`, and both total/passed/failed triples (expected Run A 76/70/6, Run B 76/76/0), and states explicitly that the two runs executed the same binaries and that the failed counts differ, which is the order-dependence this cycle closes
- [ ] [P0-T9] Capture the mechanism census with no build and no edit: (a) confirm `SVGControl.Test/SVGControl.Test.csproj` contains a `Svg` `<Reference>` and **no** `ExCSS` `<Reference>`; (b) confirm `SVGControl.Test/packages.config` contains a `Svg` entry and **no** `ExCSS` entry; (c) list `SVGControl.Test/bin/Debug` for `Svg.dll`, `ExCSS.dll`, and `Fizzler.dll`; (d) run the globs `*.Test/bin/Debug/ExCSS.dll` and `*.Test/bin/Debug/Fizzler.dll` across the repository; (e) confirm `packages/ExCSS.4.3.2/lib/net48/ExCSS.dll` exists and record that no `lib\net481` folder exists in that package. Acceptance: artifact `evidence/remediation-baseline/reference-census.2026-08-05T05-00.md` records all five results with the exact commands used, states that `SVGControl.Test` is the only one of the nine test projects whose output lacks `ExCSS.dll`, states that **no** test project's output contains `Fizzler.dll`, and quotes the `ExCSS` `<Reference>` block from `SVGControl/SVGControl.csproj` verbatim as the identity source for `[P1-T1]`. The artifact must also record why the glob in (d) returns **8** and not 9: **ten** directories match `*.Test`, but `UtilitiesSwordfish.Test` is excluded on three independent grounds — it is not a `TaskMaster.sln` member (a search of `TaskMaster.sln` for `Swordfish` returns zero matches), its project file is `UtilitiesSwordfish.NET.Test.csproj`, and its output is `Swordfish.NET.Test.exe` rather than any `*.Test.dll`, so the coverage runner does not discover it either. 8 sibling outputs carrying `ExCSS.dll` plus `SVGControl.Test` equals the nine assemblies the coverage run discovers, so the count must not be read as an off-by-one
- [ ] [P0-T10] Register the build and formatting comparison basis by transcribing, without re-running them, the recorded end state of HEAD `a62391f7` from `evidence/qa-gates/csharpier-check.2026-08-05T01-50.md`, `evidence/qa-gates/analyzer-build.2026-08-05T01-50.md`, and `evidence/qa-gates/nullable-build.2026-08-05T01-50.md`. Acceptance: artifact `evidence/remediation-baseline/build-basis.2026-08-05T05-00.md` records, as numbers and not as placeholders, the csharpier `EXIT_CODE:` and files-needing-formatting count; the analyzer build `EXIT_CODE:`, error count, warning count, and complete per-code per-project warning inventory table; and the nullable gate's `EXIT_CODE:` plus the complete per-code per-file diagnostic tables of both forced project-scope rebuilds (`SVGControl.csproj` and `SVGControl.Test.csproj`) exactly as that artifact records them. It must state the source artifact for each figure and the reason for reuse per Design Decision 5: `ad608825` changes no `.cs`, `.csproj`, `packages.config`, or `app.config`, so the source tree is identical to the one those artifacts describe. **Field shape:** this artifact is a transcription, not an execution, so its `Timestamp:` is the transcription time and its `Command:` and `EXIT_CODE:` values are **quoted from each source artifact** with that source named alongside; it must say so explicitly so a reaudit does not read the quoted exit codes as commands run in this cycle. This table is the sole comparison basis for `[P2-T4]`, `[P2-T5]`, and `[P2-T6]`
- [ ] [P0-T11] Register the numeric coverage baseline by transcribing, without re-running the suite, the figures recorded in `evidence/qa-gates/test-coverage.2026-08-05T01-50.md` and `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md`. Acceptance: artifact `evidence/remediation-baseline/coverage-basis.2026-08-05T05-00.md` records, as numbers and not as placeholders, the assembly count, the total/passed/failed test counts, the repository-wide `line-rate` and `branch-rate` as covered/total and percent, the `SVGControl` package figures, and the `SVGControl.SvgRenderer`, `SVGControl.SvgAssemblyProbe`, and `SVGControl.SvgAssemblyResolver` class figures. It must state the source artifact for each figure and state that this cycle modifies no `.cs` file, so the expected post-change delta on every coverage figure is zero and any non-zero delta requires an explanation at `[P2-T8]`. **Field shape:** as with `[P0-T10]`, this artifact is a transcription — its `Timestamp:` is the transcription time, and its `Command:` and `EXIT_CODE:` values are quoted from the named source artifact rather than produced by a run in this cycle, which the artifact must state explicitly

### Phase 1 — Remediation Implementation (build configuration only)

Task order is fixed. `[P1-T1]` through `[P1-T3]` are the only edits; `[P1-T4]` confirms the mechanism;
`[P1-T5]` and `[P1-T6]` are the decisive order-dependence proof; `[P1-T7]` is the scope guard.

- [ ] [P1-T1] Add one `ExCSS` `<Reference>` block to the `<Reference>` `<ItemGroup>` of `SVGControl.Test/SVGControl.Test.csproj`, in the alphabetical position that `ItemGroup` already uses — immediately after the closing `</Reference>` of the `Castle.Core` reference and immediately before the `FluentAssertions` reference. The `Include` identity must be copied byte-for-byte from `SVGControl/SVGControl.csproj:55` rather than retyped, and `<Private>True</Private>` is required per Design Decision 2, matching every other `HintPath`-resolved reference in that group. The block is exactly: `<Reference Include="ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL">`, then `<HintPath>..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll</HintPath>`, then `<Private>True</Private>`, then `</Reference>`. Acceptance: `git diff -- SVGControl.Test/SVGControl.Test.csproj` shows exactly four added lines and zero removed or modified lines; the `Include` string is byte-identical to `SVGControl/SVGControl.csproj:55`; and the `HintPath` resolves to an existing file on disk
- [ ] [P1-T2] Add one line to `SVGControl.Test/packages.config` in its existing alphabetical position — immediately after the `Castle.Core` entry and immediately before the `FluentAssertions` entry — reading exactly `<package id="ExCSS" version="4.3.2" targetFramework="net481" />`. Keep it on one line, matching the neighbouring `Castle.Core` entry. **`packages.config` is not formatter-exempt:** `.csharpierignore` excludes `*.csproj`, `*.props`, and `*.targets` but **not** `packages.config`, and that file is visibly csharpier-reflowed — 26 of its entries are already broken across four lines. What protects the single-line form here is width, not exemption: single-line entries survive to at least 98 characters (`System.Diagnostics.DiagnosticSource` at `SVGControl.Test/packages.config:120`), and the new ExCSS entry is 62 characters of element text (64 including its two-space indent), so `[P2-T1]` will not reflow it. If `[P2-T1]` reflows it anyway, the reflowed form is correct and this task's acceptance is re-evaluated against the post-format file. Acceptance: `git diff -- SVGControl.Test/packages.config` shows exactly one added line and zero removed or modified lines; the `version` matches `SVGControl/packages.config:3`; and the `targetFramework` matches every other entry in the file
- [ ] [P1-T3] Deliver the inputs' R-11 by adding the single child element `<Private>True</Private>` to the existing `Svg` `<Reference>` in `SVGControl.Test/SVGControl.Test.csproj` (the block whose `HintPath` is `..\packages\Svg.3.4.8\lib\net481\Svg.dll`), positioned after the `<HintPath>` line so the block matches its neighbours. This is behavior-preserving: MSBuild already defaults a `HintPath`-resolved reference to copy-local, which is why `Svg.dll` is present in the output today. Acceptance: `git diff -- SVGControl.Test/SVGControl.Test.csproj` shows exactly one added line beyond the four from `[P1-T1]`, for five added lines total in that file, and no removed or modified line
- [ ] [P1-T4] Confirm the copy-local mechanism. Delete nothing, then rebuild the test project with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"` followed by `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`, then list `SVGControl.Test/bin/Debug` for `ExCSS.dll`, `Svg.dll`, and `Fizzler.dll`, and re-read the `ExCSS` `<Reference>` block in `SVGControl.Test/SVGControl.Test.csproj` **after** the build. The post-build re-read is required because `Invoke-VSBuild.ps1` invokes `Sync-PackageReferences.ps1`, which rewrites a `HintPath` when the current one fails to resolve. `packages/ExCSS.4.3.2/lib/net48/ExCSS.dll` exists, so it cannot retarget this reference to a nonexistent `net481` path — but a silent rewrite is the one mechanism that could break this plan undetected and would invalidate `[P1-T7]`'s five-added-lines count, so it is checked rather than assumed. Acceptance: artifact `evidence/other/excss-copy-local.2026-08-05T05-00.md` records both commands with `EXIT_CODE: 0`, `Output Summary:`, and the directory listing showing `ExCSS.dll` **present** with its file version, `Svg.dll` still present, and `Fizzler.dll` still absent; it records the post-build `HintPath` text verbatim and confirms it is still `..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll` and that `git diff -- SVGControl.Test/SVGControl.Test.csproj` still shows exactly five added lines; and it records any `MSB3243`, `MSB3245`, or `MSB3277` line emitted for `SVGControl.Test`, verbatim, for disposition at `[P2-T5]`. If the `HintPath` was rewritten, halt and report the rewritten value rather than accepting it. If `ExCSS.dll` is absent from the output after a successful build, halt and report: the reference did not take effect and no further verification is meaningful
- [ ] [P1-T5] **Decisive verification, part 1 — the standalone run.** Run `& '<VSTEST>' SVGControl.Test\bin\Debug\SVGControl.Test.dll` from the repository root with the identical switch set `[P0-T7]` used: no `/EnableCodeCoverage`, no `/InIsolation`, no `/Settings`. Acceptance: artifact `evidence/regression-testing/order-standalone-after.2026-08-05T05-00.md` records the command, `EXIT_CODE: 0`, `Output Summary:`, and **75 total, 75 passed, 0 failed**; and it names each of the six tests that failed in `[P0-T7]` — `SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull`, `GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument`, `Constructor_WithTheBuiltInDefaultImageAndNoMargin_LeavesDocumentNonNull`, `TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException`, `GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner`, `GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument` — with its individual passing outcome. If the failed count is greater than zero, **halt and report to the orchestrator**; record the remaining failures and the full text of any assembly-load exception with its requested assembly identity in the artifact. Do not add a second reference, do not edit any `app.config`, and do not weaken any assertion in response
- [ ] [P1-T6] **Decisive verification, part 2 — the previously failing ordering.** Run `& '<VSTEST>' SVGControl.Test\bin\Debug\SVGControl.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll` from the repository root, the same Run A ordering `[P0-T8]` measured at 6 failed, with the identical switch set. Acceptance: artifact `evidence/regression-testing/order-paired-after.2026-08-05T05-00.md` records the command, `EXIT_CODE: 0`, `Output Summary:`, and total/passed/failed with **failed equal to zero**, and states that this is the same ordering and the same sibling assembly `[P0-T8]` Run A used, so the comparison is like-for-like. If failed is greater than zero, halt and report under the same constraints as `[P1-T5]`
- [ ] [P1-T7] Scope guard before the QC loop. Run `git status --porcelain` and `git diff --stat` from the repository root. Acceptance: artifact `evidence/other/scope-guard.2026-08-05T05-00.md` records both outputs and confirms that exactly two tracked files are modified — `SVGControl.Test/SVGControl.Test.csproj` (five added lines) and `SVGControl.Test/packages.config` (one added line) — that no `.cs` file appears in the diff, that no `app.config` appears in the diff, that neither `plan.2026-08-04T14-36.md` nor `remediation-plan.2026-08-05T01-50.md` appears in the diff, and that every other changed path is untracked evidence under this feature's `evidence/` tree. Any other modified path is a scope violation: revert it and re-run this task

### Phase 2 — Final QC Loop

Run stages in the `CLAUDE.md` C# toolchain order: format, then lint, then type-check, then test. **If any stage
fails or changes any file, fix the cause and restart this phase from `[P2-T1]`.** Every command below is
unconditional: `EXIT_CODE: SKIPPED` is not a valid outcome for any task in this phase. Artifact directory:
`evidence/qa-gates/`.

- [ ] [P2-T1] Run `dotnet tool run csharpier format .` from the repository root. This cycle modifies no `.cs` file, so the expected reformatted count is zero. Acceptance: artifact `evidence/qa-gates/csharpier-format.2026-08-05T05-00.md` records the command, `EXIT_CODE:`, `Output Summary:`, and the count of files reformatted; if that count is non-zero, identify why a `.cs` file changed, resolve it, and restart the loop from this task
- [ ] [P2-T2] Run `dotnet tool run csharpier check .` from the repository root. Acceptance: artifact `evidence/qa-gates/csharpier-check.2026-08-05T05-00.md` records `EXIT_CODE: 0`, `Output Summary:`, and zero files needing formatting, matching the figure transcribed in `evidence/remediation-baseline/build-basis.2026-08-05T05-00.md`
- [ ] [P2-T3] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"` from the repository root. Acceptance: artifact `evidence/qa-gates/restore.2026-08-05T05-00.md` records `EXIT_CODE: 0` and `Output Summary:`, and confirms that the new `ExCSS` entry in `SVGControl.Test/packages.config` resolved without adding or modifying any file under `packages/`
- [ ] [P2-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` from the repository root. Acceptance: artifact `evidence/qa-gates/analyzer-build.2026-08-05T05-00.md` records `EXIT_CODE: 0`, zero errors, `Output Summary:`, and a per-code per-project warning inventory compared line by line against `evidence/remediation-baseline/build-basis.2026-08-05T05-00.md`, with every difference listed explicitly as added or removed. Differences are dispositioned by `[P2-T5]`, not by this task
- [ ] [P2-T5] Disposition the `[P2-T4]` inventory delta. If the inventory is identical to the basis, record that. Otherwise, for each added diagnostic decide by code: any added diagnostic that is **not** one of `MSB3243`, `MSB3245`, or `MSB3277` naming `ExCSS`, or that is emitted by a project other than `SVGControl.Test`, is a newly introduced diagnostic — fix the cause and restart the loop from `[P2-T1]`. An added `MSB3243`/`MSB3245`/`MSB3277` line naming `ExCSS` and emitted by `SVGControl.Test` is a **reference-resolution consequence of the intended fix**: record it verbatim, record whether `SVGControl` already emits the same code for the same assembly in the basis inventory, and **escalate it to the orchestrator as an accepted-with-evidence finding rather than silently accepting it**. Do not respond by editing any `app.config`, by adding `<NoWarn>`, or by removing the reference — the first is forbidden by the `## Do Not Do` list, the second is forbidden by this plan's Scope Lock, and the third undoes the fix. **Removals are dispositioned too, and a removal is expected rather than anomalous:** a diagnostic that is `CoreCompile`-gated in a project this cycle does not touch may simply be absent because that project did not recompile. The basis `CS2002` row in `UtilitiesCS.Test` is the known instance — this cycle's only changed inputs are under `SVGControl.Test`, so `UtilitiesCS.Test` may not recompile and the code may not be emitted. A `CoreCompile`-gated diagnostic that disappears because its emitting project did not recompile is **not** a regression, requires no fix, and triggers **no** loop restart; record it with that reason and the emitting project. A removal in a project that *did* recompile, or a removal of a diagnostic that is not `CoreCompile`-gated, must be explained on its merits before the pass is accepted. Acceptance: artifact `evidence/qa-gates/reference-resolution-disposition.2026-08-05T05-00.md` records either the literal line `Inventory identical to basis; no delta to disposition` or every added and removed diagnostic with its code, emitting project, verbatim text, and its disposition under the rule above
- [ ] [P2-T6] Run the mandated nullable gate `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` from the repository root, then run two supplementary **forced** project-scope rebuilds with the identical property set: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m` and the same command with `SVGControl\SVGControl.csproj`. If MSBuild reports the platform is not defined for a project, rerun that project without `/p:Platform`. If the MSBuild path above does not exist on this host, resolve it with `vswhere` as `scripts/vscode/Invoke-VSBuild.ps1:132` does and record the resolved path. Acceptance: artifact `evidence/qa-gates/nullable-build.2026-08-05T05-00.md` records the mandated command's `EXIT_CODE: 0`, states explicitly that this exit code is **not** evidence of nullable cleanliness because a legacy up-to-date check can execute zero `CoreCompile` targets, and records for each supplementary rebuild its exact command, `EXIT_CODE:`, and complete per-code per-file diagnostic table compared against the tables transcribed in `evidence/remediation-baseline/build-basis.2026-08-05T05-00.md`. Any diagnostic in either supplementary set that is absent from that basis is newly introduced: fix the cause and restart the loop from `[P2-T1]`
- [ ] [P2-T7] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` from the repository root and read `coverage/coverage.cobertura.xml`. `-SearchRoot .` is mandatory: the single-project form of that wrapper family is defective under `Set-StrictMode`. Acceptance: artifact `evidence/qa-gates/test-coverage.2026-08-05T05-00.md` records `EXIT_CODE: 0`, `Output Summary:`, the assembly count (expected 9), total/passed/failed counts with **failed equal to zero** and total at least the figure transcribed in `evidence/remediation-baseline/coverage-basis.2026-08-05T05-00.md`, and the numeric repository-wide `line-rate` and `branch-rate` as covered/total and percent
- [ ] [P2-T8] Write the coverage comparison to `evidence/qa-gates/coverage-delta.2026-08-05T05-00.md` against the figures transcribed in `evidence/remediation-baseline/coverage-basis.2026-08-05T05-00.md`, using the same counting method `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md` uses so the comparison stays like-for-like. It must record: repository-wide line and branch figures before and after with an explicit verdict against the `>= 85%` line floor and the `>= 75%` branch floor; the `SVGControl` package figures before and after; the `SVGControl.SvgRenderer`, `SVGControl.SvgAssemblyProbe`, and `SVGControl.SvgAssemblyResolver` class figures before and after; and a statement that this cycle modified no `.cs` file, so the expected delta on every figure is zero. Acceptance: every figure is numeric with no placeholder; the repository-wide verdict is PASS; no changed line lost coverage; and any non-zero delta is explained by name. It must also state that the `>= 85%` file-level floors on `SVGControl/SvgRenderer.cs` (G-1) and `SVGControl/SvgAssemblyResolver.cs` (G-9) are **not** targeted this cycle, naming `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` as the entry that owns the G-1 residual and recording that G-9 awaits a maintainer decision rather than code
- [ ] [P2-T9] Re-confirm order independence **inside** the final clean pass, because a nine-assembly run passes with or without this fix and is not evidence of it. Run both of `& '<VSTEST>' SVGControl.Test\bin\Debug\SVGControl.Test.dll` and `& '<VSTEST>' SVGControl.Test\bin\Debug\SVGControl.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll` from the repository root with the same switch set `[P0-T7]` and `[P0-T8]` used. Acceptance: artifact `evidence/qa-gates/order-independence.2026-08-05T05-00.md` records both commands, both `EXIT_CODE: 0`, `Output Summary:`, and both total/passed/failed triples with **failed equal to zero in both**, with the standalone run at **75 total, 75 passed**; and it tabulates the before figures from `evidence/remediation-baseline/order-standalone.2026-08-05T05-00.md` and `evidence/remediation-baseline/order-paired.2026-08-05T05-00.md` beside the after figures so the closure of G-8 and CR-8 is readable in one place. If failed is greater than zero in either run, the loop restarts from `[P2-T1]` after the cause is fixed within this plan's Scope Lock
- [ ] [P2-T10] Write `evidence/qa-gates/toolchain-clean-pass.2026-08-05T05-00.md` recording the single consecutive clean pass: the pass number, each of the six commands from `[P2-T1]`, `[P2-T2]`, `[P2-T3]`, `[P2-T4]`, `[P2-T6]`, and `[P2-T7]` in `CLAUDE.md` order with its `EXIT_CODE:`, an explicit statement of whether any loop restart occurred and why, the `[P2-T9]` order-independence outcome, and the confirmation that no source, test, or build-configuration file was modified after the pass was recorded. Acceptance: the artifact records `Pass number:` and shows `EXIT_CODE: 0` for all six commands within one uninterrupted pass
- [ ] [P2-T11] Append one dated, append-only evidence note to the **AC-10** entry in `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`, recording that the redirect's stated objective — the test host resolving `ExCSS` through the binding redirect rather than depending on the `AssemblyResolve` fallback to mask its absence — is now achievable in the standalone `SVGControl.Test` host, because `ExCSS.dll` is present in `SVGControl.Test/bin/Debug` as of `[P1-T1]`/`[P1-T2]`; cite `evidence/qa-gates/order-independence.2026-08-05T05-00.md` and `evidence/other/excss-copy-local.2026-08-05T05-00.md`; and state that `SVGControl.Test/app.config` was not modified by this cycle. **Change no AC text and no checkbox: AC-1 through AC-10 stay `[x]` and AC-11 stays `[ ]`.** Mirror the same text to `evidence/issue-updates/issue-418.2026-08-05T05-00.md` with `PostedAs:` recorded. Acceptance: `git diff -- docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` contains only additions under AC-10, and no line beginning `- [ ]` or `- [x]` changed state
- [ ] [P2-T12] Write `evidence/other/remediation-completion-summary.2026-08-05T05-00.md` reconciling this cycle's exit state: a row for the `ExCSS` reference item (inputs label R-7, directive label R-11) and a row for the `<Private>True</Private>` item (inputs label R-11), each naming its delivering task IDs, its outcome, and its evidence artifact; the before/after order-dependence table; the confirmation that R-1 remains open, is human-only, is tracked as human_interaction requirements H-1 and H-2 with response `exception`, and that AC-11 is still `[ ]`; the confirmation that G-9, G-1, R-8, R-9, R-10, and R-12 were deliberately not addressed, with the one-line reason recorded in this plan's § Explicitly excluded for each; the recorded decision not to add a `Fizzler` reference with its four measured grounds from Design Decision 3; the `[P2-T5]` disposition outcome; and the confirmation from `git diff --stat HEAD` that neither `plan.2026-08-04T14-36.md` nor `remediation-plan.2026-08-05T01-50.md` was modified by this cycle. Acceptance: every row cites an artifact that exists on disk, and checkbox state in this plan file matches the evidence recorded

## Exit Criteria for This Plan

This plan is complete when all 30 tasks are `[x]` and:

- `evidence/qa-gates/order-independence.2026-08-05T05-00.md` records the standalone `SVGControl.Test` run at 75
  total / 75 passed / 0 failed and the `SVGControl.Test`-first pair at 0 failed, closing G-8 and CR-8 and
  restoring AC-10 to PASS; **and**
- `evidence/qa-gates/toolchain-clean-pass.2026-08-05T05-00.md` records one uninterrupted clean pass of all six
  mandated commands; **and**
- `evidence/other/scope-guard.2026-08-05T05-00.md` confirms exactly two modified tracked files and no `.cs` or
  `app.config` change.

The **cycle** exit condition (`blocking_count == 0`) additionally requires R-1 to be discharged by a human
designer-load capture or explicitly waived by the maintainer in the orchestrator-state `human_interaction`
block. That is outside this plan and no task here can satisfy it.
