# svg-renderer-null-document-nre — Remediation Plan, Cycle 1

- **Issue:** #418
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-05T01-50
- **Status:** Draft
- **Version:** 1.0 (initial authoring of this cycle's remediation plan)
- **Work Mode:** `minor-audit` (persisted marker `- Work Mode: minor-audit` in `issue.md`)
- **Language in scope:** C# only
- **Cycle:** remediation cycle 1
- **Cycle entry inputs:** `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-inputs.2026-08-04T20-25.md`
- **Branch / HEAD at authoring:** `bug/svg-renderer-null-document-nre-418` @ `ea106111`
- **Base:** `origin/main` @ `ce0c91e6`
- **Evidence series for this cycle:** `2026-08-05T01-50`

**This plan does not supersede `plan.2026-08-04T14-36.md`.** That plan is complete (all 46 tasks `[x]`) and is
**read-only** for the whole of this cycle. No task in this plan may modify it. Task `[P0-T10]` records its
untouched state and `[P2-T11]` re-confirms it at exit.

**Fail-closed evidence rule:** Every command-bearing task names its exact command and its artifact path. If any
required baseline artifact, QC artifact, or coverage-comparison artifact is missing or incomplete, the verdict
is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Do not mark an evidence-backed task complete without the artifact on disk carrying
`Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

## Scope of This Cycle

`feature-review` returned PARTIAL with blocking count 1. This plan delivers **R-2 through R-6 only**.

**R-1 is excluded and is not represented by any task in this plan.** R-1 is the AC-11 human designer-load
runbook (`runbooks/verify-winforms-designer-load.runbook.md`). No agent can execute it: it requires opening
`UtilitiesCS/Dialogs/MyBoxViewer.cs` in the Visual Studio WinForms designer and observing the load. It is
already recorded as human_interaction requirements H-1 and H-2 with response `exception` and a runbook path,
and it resolves only when the user runs the runbook. **No task in this plan may check off AC-11**, and no
automated evidence substitutes for the human capture at
`evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`.

### Item to task mapping

| Item | Delivered by |
|---|---|
| R-2 — `<LangVersion>` on `SVGControl.Test.csproj` | `[P1-T5]`, `[P1-T6]`, `[P1-T7]`, `[P1-T8]`, `[P1-T9]` |
| R-3 — exception containment in the resolve handler | `[P1-T10]`, `[P1-T11]`, `[P1-T12]`, `[P1-T13]` |
| R-4 — two targeted coverage items (CR-5, CR-6) | `[P1-T1]` (CR-6 accessibility), `[P1-T14]` (CR-5 test), `[P1-T15]` (CR-6 tests), `[P1-T18]` (residual entry) |
| R-5 — stale and overbroad comments (CR-4, CR-7) | `[P1-T16]`, `[P1-T17]` |
| R-6 — reduce `SvgRenderer.cs` below the 500-line pressure point | `[P1-T1]`, `[P1-T2]`, `[P1-T3]`, `[P1-T4]` |

## Required References

- `CLAUDE.md` (repo-root standing instructions; policy compliance order and C# toolchain order)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` — the `## Acceptance Criteria`
  section (AC-1 through AC-11) is the **sole** requirements source for this feature
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-inputs.2026-08-04T20-25.md`
  — the enumerated fix list and the binding `## Do Not Do` list
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/code-review.2026-08-04T20-25.md`
  (CR-1 through CR-7)
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/policy-audit.2026-08-04T20-25.md`
  (gaps G-1 through G-6)
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/research/2026-08-04T15-05-svg-renderer-null-document-research.md`
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md` — **read-only
  reference** for Design Decisions 1 through 12 and for the ratified
  `COVERAGE_MEMBER_UNREACHABLE` exception
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`

**All work must comply with these policies; do not duplicate their content here.**

## Work-Mode Notes (minor-audit, fail-closed)

- `spec.md` and `user-story.md` are **intentionally absent** from this feature folder and must **not** be
  required by any task, validation, or audit. If either is found to exist, execution fails closed and the
  orchestrator must be notified before any Phase 1 task begins (checked by `[P0-T3]`).
- If the `## Acceptance Criteria` section is missing from `issue.md`, execution fails closed (`[P0-T3]`).
- AC-1 through AC-10 are already `[x]`. This cycle changes **no AC check state**. It appends dated
  evidence-note amendments to AC-2, AC-5, and AC-8 only, because R-6 relocates the members those notes cite
  and R-4 moves the figures AC-5 cites (`[P2-T10]`).
- **AC-11 stays `- [ ]`.** It is R-1, excluded from this plan.

## Environment Precondition (why Phase 0 begins with a bootstrap task)

`global.json` pins SDK `8.0.205` with `"paths": [".dotnet-sdk", "$host$"]`, and `.dotnet-sdk/` does not exist
in a fresh checkout. In that state `dotnet tool run csharpier --version` fails with an instruction to run
`scripts/vscode/Install-RepoDotNetSdk.ps1`. That script does not perform `dotnet tool restore`, so csharpier
(manifest at repo-root `dotnet-tools.json`) must be restored separately. Independently, `dotnet-coverage` may
not be present in `~/.dotnet/tools`; `scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws without it. Without
the bootstrap, `[P0-T6]`, `[P2-T1]`, `[P2-T2]` (csharpier) and `[P0-T9]`, `[P1-T19]`, `[P2-T6]` (coverage)
cannot run, and the latter carry the mandatory numeric coverage evidence. `[P0-T1]` exists solely to remove
this precondition.

## Baseline Strategy for This Cycle

Phase 0 captures a **fresh remediation baseline** under `evidence/remediation-baseline/` in series
`2026-08-05T01-50` rather than citing the existing artifacts alone. Reasoning, stated explicitly as the
orchestrator directive requires:

1. Two facts this cycle turns on exist in no artifact on disk: the **current** line count of
   `SVGControl/SvgRenderer.cs` (497 by inspection, three lines of headroom) and the **current** forced
   project-scope nullable diagnostic set for `SVGControl.Test` and `SVGControl`. R-2's whole claim is a
   before/after statement about that diagnostic set, so a same-session before-capture is required.
2. The toolchain bootstrap (`.dotnet-sdk/`, `dotnet tool restore`, `dotnet-coverage`) is session-scoped and
   must be re-verified regardless.
3. The coverage delta in `[P2-T7]` must be computed against numbers measured in this session at this HEAD.

The existing artifacts remain authoritative as **cited comparison bases** and are not re-derived:

- **Diagnostic and coverage comparison basis:** `evidence/qa-gates/*.2026-08-04T14-36.md` — the end state of
  the completed plan, captured at this HEAD with a clean tree. `coverage-delta.2026-08-04T14-36.md` supplies
  the per-member and per-class figures this cycle must not regress.
- **Original pre-change reference:** `evidence/baseline/*.2026-08-04T21-04.md`. Cited for provenance only.
  `nullable-build.2026-08-04T21-04.md` is the source of the `195 UtilitiesCS` pre-existing nullable errors and
  the single `SVGControl.Test` `CS8630` that R-2 addresses.
- The `evidence/baseline/*.2026-08-04T14-36.md` series must **not** be used as a comparison basis: it was
  captured on a host lacking the VSTO runtime assemblies and its diagnostic set includes `CS0234`/`MSB3245`
  failures that do not occur on the current host.

**No existing artifact is overwritten.** Every artifact this plan writes carries the `2026-08-05T01-50` stamp.

## Scope Lock (files this plan is permitted to change)

Line numbers cited in this plan are indicative only; locate each target by the quoted content.
`SVGControl/SvgRenderer.cs` is **497 lines** at `[P1-T1]`, three lines below the hard 500-line limit in
`.claude/rules/general-code-change.md`.

Production C#:

- `SVGControl/SvgRenderer.cs`
- `SVGControl/SvgAssemblyProbe.cs`
- `SVGControl/SvgAssemblyResolver.cs` — **new file**, created by `[P1-T3]`

Build/configuration:

- `SVGControl/SVGControl.csproj` — for the single `<Compile Include="SvgAssemblyResolver.cs" />` item
  **only**. `SVGControl` is a legacy non-SDK project with an explicit `<Compile Include>` list and no glob
  (see `SVGControl/SVGControl.csproj:95-131`), so a new source file requires an explicit item or it will not
  compile. No other change to that `.csproj` is authorized.
- `SVGControl.Test/SVGControl.Test.csproj` — for the single `<LangVersion>latest</LangVersion>` property
  **only** (`[P1-T5]`), reversible at `[P1-T7]`. No `<Compile Include>` change is authorized: this cycle adds
  **no new test file**.

Test C# (existing files only):

- `SVGControl.Test/SvgRendererParseContractTests.cs`
- `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs`
- `SVGControl.Test/SvgRendererNullToleranceTests.cs` — **addition to the directive's list.** Editable only in
  the `R2_KEEP` branch of `[P1-T8]`, and then only to clear a nullable diagnostic that `[P1-T6]` measures in
  it. It is one of the three test files this branch authored, so excluding it would make `R2_KEEP`
  unreachable by construction if it emits a diagnostic. No behavioral edit and no assertion change is
  authorized in it.

Documentation and evidence:

- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` — **evidence-note amendments
  only** (`[P2-T10]`), appended and dated. No AC text may be rewritten, no `- [ ]` may become `- [x]`, and no
  `- [x]` may become `- [ ]`.
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-plan.2026-08-05T01-50.md`
  (this file; checkbox state and preflight revision only)
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/**`
- `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` — new file (`[P1-T18]`)
- `docs/features/potential/2026-08-05-test-project-langversion-alignment.md` — new file, created only in the
  `R2_REVERTED_OUT_OF_SCOPE_NULLABLE` branch of `[P1-T9]`. Scoped repository-wide across all six
  `LangVersion`-less test projects, not to `SVGControl.Test` alone

**Explicitly out of scope** (binding, from `remediation-inputs.2026-08-04T20-25.md` § `## Do Not Do`):

- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`. Read-only for
  this entire cycle.
- AC-11 / R-1. No task may check it off or attempt to automate it.
- Any file under `UtilitiesCS`. Its 195 pre-existing `CS86xx` diagnostics at forced-recompile scope are
  tracked outside issue #418 and are the reason a cold solution-wide nullable build cannot pass on this
  repository independently of this branch.
- Reaching the 85% modified-file coverage floor on `SVGControl/SvgRenderer.cs`. R-4 is bounded to two targeted
  items; the residual is filed by `[P1-T18]`.
- The deferred entries `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md` and
  `docs/features/potential/2026-08-04-invoke-mstest-scalar-count-strictmode.md`.
- `scripts/vscode/Invoke-MSTest.ps1`. Its single-assembly scalar-`.Count` defect is real and already filed.
- Weakening any assertion, deleting any test, or adding `[ExcludeFromCodeCoverage]` to any production file.
  `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy makes any exclusion of a production source
  path a **Blocking** finding.
- Any `<NoWarn>`, `#pragma warning disable`, or `.editorconfig` severity change used to make a nullable or
  analyzer diagnostic disappear. Fix the root cause or revert the change that surfaced it.
- Any edit under `.claude/rules/` or `.github/instructions/`.
- Temporary files in tests. `.claude/rules/general-unit-test.md` UT4 prohibits them with zero approved
  exceptions; this is specifically the constraint that makes a live `Assembly.LoadFrom` test inadmissible.
- `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`,
  `artifacts/coverage/`, `artifacts/evidence/` as evidence destinations.

## Design Decisions Fixed by This Plan

These are settled before execution so that no task requires interpretation.

1. **R-6 runs first, not last.** `remediation-inputs` § R-6 suggests doing the extraction last. That ordering
   is rejected. `SVGControl/SvgRenderer.cs` is 497 of 500 lines. R-3 adds a `catch` block to the same region,
   which breaches the hard limit before R-6 could relieve it. This is the third time the 500-line limit has
   constrained this feature: it produced the `[P1-T19]` `SCOPE_EXCEEDED` in the completed plan, then a stated
   seven-line comment budget in that plan's Phase 2, and now this. Extracting first removes the constraint
   instead of planning into it. Reviewability is preserved either way: a move-only diff followed by a
   fix-only diff, rather than the reverse.
2. **R-6 is a pure move. No behavior change may occur in the same task as the extraction.** `[P1-T3]` moves
   text; `[P1-T10]` and `[P1-T11]` change behavior afterwards. The moved method bodies must be textually
   identical to their pre-move form except for (a) indentation and line wrapping applied by csharpier, and
   (b) the two type qualifications `SvgAssemblyProbe.PublicKeyTokensEqual` and `SvgRenderer.DescribeFailure`
   made necessary by the move. Every string literal, comment, and control-flow construct is carried verbatim,
   including the two `Trace.TraceWarning` messages that begin `SvgRenderer load '`.
3. **The extraction target is `internal static class SvgAssemblyResolver` in
   `SVGControl/SvgAssemblyResolver.cs`.** It holds `_resolverInstalled`, the `[ThreadStatic] _resolving`
   field, a new `internal static void Install()`, and `private static System.Reflection.Assembly?
   ResolveByNameAndKey(object sender, ResolveEventArgs args)`. `ResolveByNameAndKey` stays `private static`,
   which preserves the premise of its ratified coverage exception ("this member is `private static` and is
   invoked only by the CLR on a failed assembly bind").
4. **The install trigger is unchanged.** `SvgRenderer`'s static constructor is retained and its body becomes
   the single statement `SvgAssemblyResolver.Install();`. The `Interlocked.Exchange(ref _resolverInstalled,
   1) == 0` guard and the `AppDomain.CurrentDomain.AssemblyResolve +=` subscription move verbatim into
   `Install()`. Touching `SvgRenderer` therefore still installs the handler exactly once per AppDomain, which
   is the observable behavior AC-8 depends on.
5. **`PublicKeyTokensEqual` relocates to `SvgAssemblyProbe`, not to the new resolver file.** Two reasons.
   (a) Moving it out of `SvgRenderer` forces it from `private static` to `internal static` mechanically,
   because it is then called across types within the assembly — which is exactly the CR-6 accessibility change
   R-4 item 2 asks for, obtained as a side effect of the move rather than as a second edit. (b) Placing it in
   the new resolver file would put a 15-line member with a testable contract into a **new** class alongside
   `ResolveByNameAndKey` (47/69, ratified unreachable), giving that new class an aggregate rate near 74%. A
   reaudit could read a new class as a new module subject to the `>= 90%` rule in `CLAUDE.md`. Putting the
   testable member on the existing, fully covered `SvgAssemblyProbe` avoids manufacturing that conflict.
   `SvgAssemblyProbe`'s class doc comment is widened minimally from "Pure path-string helpers" to cover the
   token comparison.
6. **`SVGControl.SvgAssemblyResolver` is a relocation, not a new module.** The `>= 90%` newly-added-module
   threshold does not attach to it: every member it contains existed at `ea106111` with a measured figure, and
   the ratified `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgRenderer.ResolveByNameAndKey` exception from
   `plan.2026-08-04T14-36.md` (recorded in `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md`) travels
   with the member and is re-recorded as
   `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey`. `[P1-T4]` and `[P2-T7]`
   must both state this.
7. **The only genuinely new member this cycle adds is `SvgAssemblyResolver.Install()`.** It is exercised by
   every test that touches `SvgRenderer`, so its `line-rate` is expected at 100%. Its
   `Interlocked.Exchange(...) == 0` false arm is not driven (the handler installs once per AppDomain), so its
   `branch-rate` is expected at 50%. Per `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` § Metric
   definition, the `>= 90%` gate is assessed on **`line-rate`**; `branch-rate` is recorded for information
   only and member-level branch coverage is not gated.
8. **`DescribeFailure` widens from `private static` to `internal static` on `SvgRenderer`.** The moved
   resolver code calls it at two sites. This is an accessibility-only change inside an `internal` type; it
   adds no public surface, changes no behavior, and leaves its measured 5/5 = 100% line-rate untouched.
9. **`SVGControl.Test` compiles as C# 7.3 until R-2 lands, and may still compile as C# 7.3 after this cycle.**
   The project has no `<LangVersion>`, which is the source of the pre-existing `CS8630`. `[P1-T7]` may revert
   R-2. Therefore **no `?` nullable annotation and no `!` null-forgiving operator may appear in any test code
   this plan authors** — `[P1-T12]`, `[P1-T14]`, and `[P1-T15]` must be written in syntax valid under C# 7.3,
   so that they compile under either gate outcome. Passing `null` into a `string?`/`byte[]?` parameter from
   null-oblivious test code emits no diagnostic, because nullability is metadata-only and the CLR type is
   identical. **Ordering consequence:** R-2 (`[P1-T5]`) precedes every task that authors test code
   (`[P1-T12]`, `[P1-T14]`, `[P1-T15]`), so no annotation-bearing test can be written against a 7.3 project;
   and because R-2's outcome is not known until `[P1-T7]`, the no-`?`/no-`!` rule holds unconditionally.
10. **The mandated nullable command is vacuous in an up-to-date tree and a forced project-scope recompile must
    be recorded alongside it.** `scripts/vscode/Invoke-VSBuild.ps1` uses MSBuild target `Build`; legacy
    non-SDK up-to-date checks are timestamp-based, not property-based, so `/p:Nullable=enable
    /p:TreatWarningsAsErrors=true` triggers no recompile after the preceding analyzer build and re-analyzes no
    source file. `EXIT_CODE: 0` from that command is a true record of what the mandated command returns and is
    **not** evidence of nullable cleanliness. That vacuity must not be "fixed" by forcing a solution rebuild:
    195 pre-existing `UtilitiesCS` nullable errors make a cold solution-wide nullable build unreachable
    independent of this branch. `[P0-T8]` and `[P2-T5]` therefore each record the mandated command **plus**
    two supplementary forced project-scope rebuilds (`SVGControl` and `SVGControl.Test`), clearly labelled as
    supplementary, in the same shape `evidence/baseline/nullable-build.2026-08-04T21-04.md` uses.
11. **R-3 part 1 is scoped to the region CR-2 names.** A single `catch (Exception ex)` clause is added to the
    existing outer `try`/`finally` that encloses strategies 2 and 3. That covers every raising source CR-2
    identifies: `Assembly.Load`, `Assembly.LoadFrom`, `Path.Combine` on an unfiltered candidate, `self
    .Location`, and `self.CodeBase`. The pre-guard region — `new System.Reflection.AssemblyName(args.Name)`
    and `loaded.GetName()` in the already-loaded scan, `SvgRenderer.cs:52-72` pre-move — is **not** wrapped:
    CR-2 does not name it, and widening the guard to the whole method would require either a wrapper method or
    renaming `ResolveByNameAndKey`, which would invalidate the AC-8 evidence note and the ratified coverage
    exception, both of which name that member. The residual is recorded in `[P1-T13]`'s artifact rather than
    silently dropped. The acceptance criterion for `[P1-T10]` is therefore stated as the specific sources, not
    as an absolute "no exception escapes".
12. **`Trace`, never `log4net`, inside the resolve handler.** The existing in-code comment states the reason:
    a `log4net` call inside an `AssemblyResolve` handler can itself trigger a re-entrant assembly load, so the
    diagnostic must not depend on `log4net` being loadable. The new catch clause uses
    `Trace.TraceWarning` only, matching the two handlers already present.
13. **No new test file, and therefore no test-project `<Compile Include>` change.** All new tests go into
    `SVGControl.Test/SvgRendererParseContractTests.cs` (333 lines) and
    `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs` (188 lines). Both have ample headroom against the
    500-line limit, and `SVGControl.Test.csproj` uses `packages.config` with an explicit `<Compile Include>`
    list and no glob, so avoiding a new file avoids a build-configuration edit outside R-2's stated scope.
14. **The `>= 85%` modified-file floor on `SVGControl/SvgRenderer.cs` is not targeted this cycle.** The file
    is expected to land near 75.7% by two mechanisms: R-6 removes 84 measured lines of which only 47 were
    covered, and R-4 item 1 covers the four remaining uncovered lines of the three-argument byte-array
    constructor. Reaching 85% would require tests for `AddMargins` (0/15), `Render()` (18/26), and the two
    `SvgDocument` constructor overloads (0/8 each), none of which is part of issue #418. `[P1-T18]` files that
    residual. Repository-wide line coverage must stay `>= 85%` and branch coverage `>= 75%`.

## Evidence Location Invariant

All evidence artifacts produced by this plan are written under
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/<kind>/`, using the canonical
kinds defined in `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Phase 0 writes to
`evidence/remediation-baseline/`, Phase 1 to `evidence/other/` and `evidence/regression-testing/`, Phase 2 to
`evidence/qa-gates/`, `evidence/issue-updates/`, and `evidence/other/`. `artifacts/`-rooted evidence paths are
forbidden and are blocked by the `.claude/hooks/enforce-evidence-locations.ps1` PreToolUse hook. Every
baseline and final-QC command step has its own artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:`, and
`Output Summary:`. C# has mandatory coverage policy, so baseline and final-QC test artifacts record numeric
coverage values, never placeholders.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Remediation Baseline Capture and Compliance Reads

Artifact directory for this phase:
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/`.

- [x] [P0-T1] Bootstrap the repo-local toolchain so the csharpier and coverage tasks in this plan can run: run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Install-RepoDotNetSdk.ps1` if `.dotnet-sdk/` is absent, then `dotnet tool restore` from the repository root, then `dotnet tool install --global dotnet-coverage` if `dotnet-coverage --version` fails. Acceptance: `dotnet tool run csharpier --version` and `dotnet-coverage --version` both return exit 0. Artifact: `evidence/remediation-baseline/toolchain-bootstrap.2026-08-05T01-50.md` recording each command, its `EXIT_CODE:`, and the two resolved version strings
- [x] [P0-T2] Read `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md` in that exact order, in full. Acceptance: artifact `evidence/remediation-baseline/phase0-instructions-read.2026-08-05T01-50.md` exists carrying `Timestamp:`, `Policy Order:`, and the explicit list of the four files read
- [x] [P0-T3] Read `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` in full and confirm four facts: it contains an explicit `## Acceptance Criteria` section with AC-1 through AC-11; it contains the marker `- Work Mode: minor-audit`; neither `spec.md` nor `user-story.md` exists in `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/`; and AC-1 through AC-10 are `[x]` while AC-11 is `[ ]`. Any failed confirmation halts execution and is reported before `[P1-T1]`. Artifact: `evidence/remediation-baseline/ac-source-check.2026-08-05T01-50.md`
- [x] [P0-T4] Read in full, in this order: `remediation-inputs.2026-08-04T20-25.md`, `code-review.2026-08-04T20-25.md`, `policy-audit.2026-08-04T20-25.md`, `feature-audit.2026-08-04T20-25.md`, and `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md`. Acceptance: artifact `evidence/remediation-baseline/cycle-inputs-read.2026-08-05T01-50.md` lists the five files and reproduces verbatim the `## Do Not Do` list from `remediation-inputs.2026-08-04T20-25.md` as the binding constraint set for this cycle
- [x] [P0-T5] Record the pre-change line counts of the five files in the Scope Lock by running `pwsh -NoProfile -Command "'SVGControl/SvgRenderer.cs','SVGControl/SvgAssemblyProbe.cs','SVGControl.Test/SvgRendererParseContractTests.cs','SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs','SVGControl.Test/SvgRendererNullToleranceTests.cs' | ForEach-Object { '{0} = {1}' -f $_, (Get-Content -LiteralPath $_ | Measure-Object -Line).Lines }"` from the repository root. Acceptance: artifact `evidence/remediation-baseline/file-size.2026-08-05T01-50.md` records all five counts and states the headroom of `SVGControl/SvgRenderer.cs` against the 500-line limit
- [x] [P0-T6] Run `dotnet tool run csharpier check .` from the repository root and capture the pre-change formatting state, which covers `SVGControl/SvgRenderer.cs`, `SVGControl/SvgAssemblyProbe.cs`, and every file under `SVGControl.Test/`. Acceptance: artifact `evidence/remediation-baseline/csharpier-check.2026-08-05T01-50.md` records `EXIT_CODE:` and the count of files needing formatting
- [x] [P0-T7] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` from the repository root and capture the pre-change analyzer state. Acceptance: artifact `evidence/remediation-baseline/analyzer-build.2026-08-05T01-50.md` records `EXIT_CODE:`, the error count, the warning count, and a per-code per-project diagnostic inventory table, and states whether the inventory matches `evidence/qa-gates/analyzer-build.2026-08-04T14-36.md` exactly
- [x] [P0-T8] Run the mandated nullable gate `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` from the repository root, then run two supplementary forced project-scope rebuilds with the identical property set: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m` and the same command with `SVGControl\SVGControl.csproj`. If MSBuild reports the platform is not defined for a project, rerun that project without `/p:Platform`. Acceptance: artifact `evidence/remediation-baseline/nullable-build.2026-08-05T01-50.md` records the mandated command's `EXIT_CODE:` plus, for each supplementary rebuild, its exact command, `EXIT_CODE:`, and a complete per-code per-file diagnostic table; it states explicitly that the mandated command's exit code is not evidence of nullable cleanliness because it executed zero `CoreCompile` targets; and it confirms whether the `SVGControl.Test` supplementary set is exactly one `CS8630` and nothing else. This table is the sole comparison basis for `[P1-T6]` and `[P2-T5]`
- [x] [P0-T9] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` from the repository root and read `coverage/coverage.cobertura.xml`. Acceptance: artifact `evidence/remediation-baseline/test-coverage.2026-08-05T01-50.md` records `EXIT_CODE:`, the assembly count, total/passed/failed test counts, and these numeric coverage headlines: repository-wide `line-rate` and `branch-rate` as covered/total and percent; the `SVGControl` package figures; the `SVGControl.SvgRenderer` class figures; the `SVGControl.SvgAssemblyProbe` class figures; and the per-member `line-rate` of `ResolveByNameAndKey`, `PublicKeyTokensEqual`, `SvgRenderer(byte[], Size, AutoSize)`, and `SvgRenderer(byte[], Size, Padding, AutoSize)`. It must also state whether these figures match `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md`, and if not, which differ and by how much
- [x] [P0-T10] Record the tree state this cycle starts from: run `git rev-parse HEAD`, `git status --porcelain`, and `git diff --stat HEAD -- docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`. Acceptance: artifact `evidence/remediation-baseline/tree-state.2026-08-05T01-50.md` records the HEAD SHA (expected `ea106111`), the porcelain output, and an empty diff for `plan.2026-08-04T14-36.md`, and restates that the completed plan file is read-only for this cycle

### Phase 1 — Remediation Implementation

Task order is R-6, then R-2, then R-3, then R-4, then R-5, then the residual filing. `[P1-T1]` through
`[P1-T4]` must complete before `[P1-T10]`, because `[P1-T10]` adds lines to a region that is at 497 of 500
lines until the extraction relieves it.

- [x] [P1-T1] R-6 and R-4/CR-6, step 1. Relocate `PublicKeyTokensEqual` from `SVGControl/SvgRenderer.cs` (currently at lines 145-163) to `SVGControl/SvgAssemblyProbe.cs` as `internal static bool PublicKeyTokensEqual(byte[]? a, byte[]? b)`, carrying the method body verbatim, and requalify its three call sites inside `ResolveByNameAndKey` (currently lines 68, 92, 126) to `SvgAssemblyProbe.PublicKeyTokensEqual(...)`. Widen the `SvgAssemblyProbe` class doc comment from "Pure path-string helpers used by the SVGControl assembly-resolve fallback" so it covers the token comparison as well. Acceptance: `PublicKeyTokensEqual` appears exactly once in the repository, in `SVGControl/SvgAssemblyProbe.cs`, declared `internal static`; the body is byte-identical to its pre-move form; and `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` returns `EXIT_CODE: 0` with a warning count no higher than `[P0-T7]`
- [x] [P1-T2] R-6, step 2. Change `DescribeFailure` in `SVGControl/SvgRenderer.cs` (currently line 209) from `private static string DescribeFailure(Exception? error)` to `internal static string DescribeFailure(Exception? error)`. Change nothing else about the member: its body, its comment, and all five existing call sites stay as they are. Acceptance: the declaration reads `internal static string DescribeFailure(Exception? error)`, no other line of the file changed, and the analyzer build command from `[P1-T1]` returns `EXIT_CODE: 0`
- [x] [P1-T3] R-6, step 3 — the pure move. Create `SVGControl/SvgAssemblyResolver.cs` containing `#nullable enable`, the using directives `System`, `System.Collections.Generic`, `System.Diagnostics`, `System.IO`, `System.Threading`, and `internal static class SvgAssemblyResolver` in namespace `SVGControl`; move into it, verbatim, the header comment block currently at `SVGControl/SvgRenderer.cs:27-33`, the `private static int _resolverInstalled` field, the `[ThreadStatic] private static HashSet<string>? _resolving` field, and `private static System.Reflection.Assembly? ResolveByNameAndKey(object sender, ResolveEventArgs args)`; add `internal static void Install()` whose body is the `if (Interlocked.Exchange(ref _resolverInstalled, 1) == 0) { AppDomain.CurrentDomain.AssemblyResolve += ResolveByNameAndKey; }` block moved verbatim from the `SvgRenderer` static constructor; requalify the two `DescribeFailure` calls in the moved body to `SvgRenderer.DescribeFailure`; add `<Compile Include="SvgAssemblyResolver.cs" />` to the `<ItemGroup>` at `SVGControl/SVGControl.csproj:95-131`; delete the moved members from `SVGControl/SvgRenderer.cs`; replace the `SvgRenderer` static constructor body with the single statement `SvgAssemblyResolver.Install();`; and remove from `SVGControl/SvgRenderer.cs` only those using directives the move orphans (`System.Threading` and `System.Collections.Generic`, each removed only if the build confirms no remaining reference). Acceptance: `ResolveByNameAndKey` and `_resolverInstalled` appear only in `SVGControl/SvgAssemblyResolver.cs`; `SVGControl/SvgRenderer.cs` is at most 400 lines; every string literal in the moved body is unchanged, including both messages beginning `SvgRenderer load '`; and the analyzer build command from `[P1-T1]` returns `EXIT_CODE: 0` with a warning count no higher than `[P0-T7]`
- [x] [P1-T4] Record the R-6 extraction evidence. Acceptance: artifact `evidence/other/resolver-extraction.2026-08-05T01-50.md` records the before and after line counts of `SVGControl/SvgRenderer.cs`, `SVGControl/SvgAssemblyProbe.cs`, and `SVGControl/SvgAssemblyResolver.cs`; states that the move is behavior-preserving and enumerates the only three permitted deltas (indentation, `SvgAssemblyProbe.PublicKeyTokensEqual` qualification, `SvgRenderer.DescribeFailure` qualification); states that the install trigger is unchanged because `SvgRenderer`'s static constructor now calls `SvgAssemblyResolver.Install()`; and records that the ratified exception `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgRenderer.ResolveByNameAndKey` from `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` travels with the member and is henceforth `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey`, and that `SVGControl.SvgAssemblyResolver` is a relocation rather than a new module so the `>= 90%` new-module threshold does not attach to it
- [x] [P1-T5] R-2, step 1. Add `<LangVersion>latest</LangVersion>` to the first `<PropertyGroup>` of `SVGControl.Test/SVGControl.Test.csproj` (the group at lines 8-27), immediately after the `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` line, matching the placement in `SVGControl/SVGControl.csproj:12`. Acceptance: `git diff -- SVGControl.Test/SVGControl.Test.csproj` shows exactly one added line and no other change
- [x] [P1-T6] R-2, step 2 — measure. Run `& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m` from the repository root. A non-zero exit code here is an expected measurement outcome, not a task failure. Acceptance: artifact `evidence/other/langversion-probe.2026-08-05T01-50.md` records the command, its `EXIT_CODE:`, and a complete table of every diagnostic with code, file, and line; confirms that `CS8630` no longer appears; and partitions every remaining diagnostic into exactly three sets, with a count for each — **in-scope** (`SvgRendererParseContractTests.cs`, `SvgAssemblyProbeDirectoryTests.cs`, `SvgRendererNullToleranceTests.cs`), **out-of-scope** (`Form1.cs`, `Form1.Designer.cs`, `Form2.cs`, `Form2.Designer.cs`, `Resources.Designer.cs`, `Properties/AssemblyInfo.cs`, `GetRelativePath_Test.cs`, `RelativePathCoverageTests.cs`), and **`SVGControl` project reference** for any diagnostic whose emitting project is `SVGControl.csproj` rather than `SVGControl.Test.csproj`. Those eleven files are exactly the `<Compile Include>` list of `SVGControl.Test.csproj`, so the first two sets are total for that project; note in the artifact that `SVGControl.Test/Properties/Resources.Designer.cs` exists on disk but is **absent from that list**, so it is never compiled and cannot emit a diagnostic. The third set exists to keep the gate's premise sound: if it is non-empty, `SVGControl` failed before `SVGControl.Test` reached its own `CoreCompile` and the measurement is vacuous
- [x] [P1-T7] R-2, step 3 — the gate. Read `[P1-T6]`'s partition. **First, check the `SVGControl` project-reference set. If it is non-empty, the measurement is vacuous** — `SVGControl` failed before `SVGControl.Test` reached its own `CoreCompile`, so an empty out-of-scope set proves nothing and must not be read as Branch A. In that case build `SVGControl` alone first (`& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl\SVGControl.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`), re-run `[P1-T6]`, and gate on the re-run. Then take exactly one branch. **Branch A, `R2_KEEP`:** the out-of-scope set is empty on a non-vacuous measurement. Leave the `<LangVersion>` property in place and proceed to `[P1-T8]`. **Branch B, `R2_REVERTED_OUT_OF_SCOPE_NULLABLE`:** the out-of-scope set is non-empty. Revert `[P1-T5]` so that `git diff -- SVGControl.Test/SVGControl.Test.csproj` is empty and the file is byte-identical to its state at `ea106111`, then proceed to `[P1-T9]`. Branch B is mandatory in that case: `remediation-inputs.2026-08-04T20-25.md` § R-2 caveat directs that new diagnostics in files outside this feature's scope must be reported rather than edited, and the `## Do Not Do` list forbids suppression. Acceptance: artifact `evidence/other/langversion-gate.2026-08-05T01-50.md` records the literal outcome token (`R2_KEEP` or `R2_REVERTED_OUT_OF_SCOPE_NULLABLE`), the full out-of-scope diagnostic list that drove the decision, an explicit statement of whether the `SVGControl` project-reference set was empty or whether a vacuous measurement forced a re-run, and the resulting `git diff --stat -- SVGControl.Test/SVGControl.Test.csproj`
- [x] [P1-T8] R-2, step 4, Branch A only. If `[P1-T7]` recorded `R2_KEEP`, clear every in-scope nullable diagnostic by editing only `SvgRendererParseContractTests.cs`, `SvgAssemblyProbeDirectoryTests.cs`, and `SvgRendererNullToleranceTests.cs`, changing no assertion and no test name, then rerun the `[P1-T6]` command until it returns `EXIT_CODE: 0` with zero `CS86xx` and zero `CS8630`. If `[P1-T7]` recorded `R2_REVERTED_OUT_OF_SCOPE_NULLABLE`, make no edit. Acceptance: the artifact `evidence/other/langversion-gate.2026-08-05T01-50.md` is appended with either the final `EXIT_CODE: 0` rerun and the list of edits made, or the line `Branch B taken at [P1-T7]; no edit performed by [P1-T8]`
- [x] [P1-T9] R-2, step 5, Branch B only. If `[P1-T7]` recorded `R2_REVERTED_OUT_OF_SCOPE_NULLABLE`, create `docs/features/potential/2026-08-05-test-project-langversion-alignment.md`, scoped to the **repository-wide** `<LangVersion>` alignment across all six `LangVersion`-less test projects and the generated-file nullable diagnostics that alignment surfaces — not to `SVGControl.Test` alone. It must record: (a) the six affected projects `QuickFiler.Test`, `Tags.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, and `SVGControl.Test`, against the three that already set it (`TaskMaster.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`); (b) that `CS8630` is a pre-existing repository-wide condition and not a defect this branch introduced — `SVGControl.Test` is merely the only one of the six that reaches its own `CoreCompile` in a cold solution-wide nullable build, because it project-references only `SVGControl` while the other five cascade-fail from `UtilitiesCS` first, so wiring up a project that never built made a latent condition observable; (c) the measured out-of-scope diagnostic set from `[P1-T6]` verbatim, alongside the predicted set inspected at plan authoring — `Form1.Designer.cs:8` and `Form2.Designer.cs:8` (`components = null`, `CS8625`) and `Resources.Designer.cs:27,29` (uninitialized `resourceMan` / `resourceCulture` statics, `CS8618`; `return resourceCulture;`, `CS8603`; the `(byte[])(obj)` casts, `CS8600`) — noting that `Properties/Resources.Designer.cs` carries the identical pattern but is **absent from the `SVGControl.Test.csproj` `<Compile Include>` list**, so it never compiles and is excluded from the prediction, while remaining relevant to the repo-wide entry because the other five projects may compile their equivalent; (d) that scoped `#nullable disable` / `#nullable restore` islands in generated files are **not** a durable remedy, because `ResXFileCodeGenerator` and the WinForms designer erase them silently on the next regeneration, so the fix would revert itself with no signal — this supersedes the ratification of that route in `plan.2026-08-04T14-36.md` § Scope Lock; and (e) that the durable options are a directory-level `Directory.Build.props` setting or generator-aware exclusion, either of which belongs repo-wide rather than inside a #418 bug fix. If `[P1-T7]` recorded `R2_KEEP`, create no file. Acceptance: either the entry exists, names issue #418 as its origin, and carries all five elements (a) through (e), or `evidence/other/langversion-gate.2026-08-05T01-50.md` carries the line `Branch A taken at [P1-T7]; no potential entry required`
- [x] [P1-T10] R-3, part 1. In `SVGControl/SvgAssemblyResolver.cs`, add exactly one `catch (Exception ex)` clause to the existing outer `try` that encloses strategies 2 and 3, positioned between the try block and its existing `finally`, with the single-statement body `Trace.TraceWarning($"SvgRenderer resolve '{requested.Name}': {SvgRenderer.DescribeFailure(ex)}");`. Do not add a `log4net` call at this site; the existing in-code comment states the re-entrancy reason. Do not alter the `_resolving.Add`/`Remove` guard, the strategy order, either existing inner catch, or the method's terminal `return null;`. Acceptance: no exception raised by `Assembly.Load`, `Assembly.LoadFrom`, `Path.Combine`, `self.Location`, or `self.CodeBase` can leave `ResolveByNameAndKey`; the outer try has exactly one catch clause and one finally clause; and the analyzer build command from `[P1-T1]` returns `EXIT_CODE: 0`
- [x] [P1-T11] R-3, part 2. In `SVGControl/SvgAssemblyProbe.cs`, apply the invalid-path-character filter to the third candidate in `GetProbeDirectories` so `baseDirectory` is validated identically to `assemblyLocation` and to the code-base candidate: replace the bare `baseDirectory,` entry in the `candidates` initializer with `baseDirectory != null && baseDirectory.IndexOfAny(Path.GetInvalidPathChars()) < 0 ? baseDirectory : null,`. Change nothing else in the method, so that the order, the case-insensitive de-duplication, and the empty-location skip all behave exactly as before for valid inputs. Acceptance: all nine existing `SvgAssemblyProbeDirectoryTests` still pass unchanged, and the analyzer build command from `[P1-T1]` returns `EXIT_CODE: 0`
- [x] [P1-T12] R-3, verification test. Add one test method `GetProbeDirectories_WithAnInvalidCharacterInTheBaseDirectory_DropsThatCandidateWithoutThrowing` to `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs`, in the same style as `TryGetDirectoryFromCodeBase_WithANonUriString_ReturnsNullWithoutThrowing`: construct the base directory as `@"C:\probe\three" + Path.GetInvalidPathChars()[0] + "bad"`, pass `@"C:\probe\one\SVGControl.dll"` as the location and `null` as the code base, assert with FluentAssertions that the call does not throw, that the returned list has exactly one entry, and that the entry ends with `one`. Use no temporary file, no `Assembly.LoadFrom`, and no `?` or `!` token. Acceptance: the test exists, compiles, and passes; `SvgAssemblyProbeDirectoryTests.cs` remains under 500 lines
- [x] [P1-T13] Record the R-3 containment evidence. Acceptance: artifact `evidence/other/resolver-containment.2026-08-05T01-50.md` names the exact catch clause added by `[P1-T10]` and the exact filter added by `[P1-T11]`, states which raising sources are now contained, quotes the `SvgAssemblyProbe.cs:15` contract sentence "Never raises, so it is safe inside an `AssemblyResolve` handler" and states that the third candidate is now consistent with it, and records as a **known residual** that the pre-guard region of `ResolveByNameAndKey` — `new System.Reflection.AssemblyName(args.Name)` and `loaded.GetName()` in the already-loaded scan — remains outside the new catch, with the reason given in Design Decision 11
- [x] [P1-T14] R-4, item 1 (CR-5). Add one test method to `SVGControl.Test/SvgRendererParseContractTests.cs` constructing `new SvgRenderer(Defaults.GetDefault.SvgImage, new Size(64, 64), AutoSize.MaintainAspectRatio)` and asserting with FluentAssertions that `Document` is not null, mirroring the four-argument overload's existing coverage. Use no `?` or `!` token. Acceptance: the test exists, compiles, and passes; the three-argument byte-array constructor's success branch (the two statements `_doc = parsed;` and `_original = parsed!.Draw().Size;`) is driven, taking the member from its measured 13/17 toward 17/17
- [x] [P1-T15] R-4, item 2 (CR-6). Add **eight** test methods for `SvgAssemblyProbe.PublicKeyTokensEqual` to `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs`, one per case, each asserting the returned boolean with FluentAssertions: both arguments null; first null and second zero-length; first zero-length and second null; first null and second non-empty; **first non-empty and second null**; equal non-empty tokens; unequal tokens of equal length; tokens of unequal length. All four null-pairing orderings are required, not three: the early-return expression `return a == b || (a != null && a.Length == 0) || (b != null && b.Length == 0);` carries five conditions and therefore ten condition outcomes, and without the first-non-empty-second-null case two of them are unreachable — `a.Length == 0` false and `b != null` false — leaving the expression at 8/10. That single case drives both. The counting granularity is established empirically by this feature's own `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md`, which records `if (file == null || parse == null)` at 4/4 conditions, so each `||`/`&&` clause contributes two outcomes. Use no `?` or `!` token and no temporary file. Acceptance: all eight tests exist, compile, and pass; `SvgAssemblyProbeDirectoryTests.cs` remains under 500 lines; and `PublicKeyTokensEqual` measures 100% line-rate and 100% branch-rate, which is the figure `[P2-T7]` and `remediation-inputs.2026-08-04T20-25.md` § R-3 Verification both require the artifact to state for `SvgAssemblyProbe` as a whole
- [x] [P1-T16] R-5, CR-4. Rewrite the header comment block now living in `SVGControl/SvgAssemblyResolver.cs` (moved verbatim by `[P1-T3]` from `SVGControl/SvgRenderer.cs:27-33`) so that it states the delivered pins and the measured host conclusion: `Svg 3.4.8` and `ExCSS 4.3.2` are the deployed packages, with only `packages/ExCSS.4.3.2/` present on disk, replacing the stale "Svg 3.4.7 was compiled against ExCSS 4.2.3.0 but the repo deploys ExCSS 4.3.1.0"; and the host that does not apply the project binding redirects is `devenv.exe`, not the vstest testhost — this branch's own research established that the testhost does apply them and the ExCSS bind succeeds there. Cite `research/2026-08-04T15-05-svg-renderer-null-document-research.md` by path so the explanation has a durable source. Change no code. Acceptance: the comment contains no reference to `4.2.3.0` or `4.3.1.0`, contains the research artifact path, and attributes the redirect-ignoring host to `devenv.exe`; `dotnet tool run csharpier check .` returns `EXIT_CODE: 0`
- [x] [P1-T17] R-5, CR-7. Narrow the Arrange comment in `SVGControl.Test/SvgRendererParseContractTests.cs` inside `TryGetSvgDocument_WhenTheParseSeamReturnsNull_ReturnsFalseWithNoCapturedError` (currently lines 219-222) by replacing the universal claim "No plain byte payload reaches it" with the measured statement — malformed input and empty input were both measured to make the XML reader raise, and whether a well-formed-XML-but-no-SVG-element payload reaches the null-returning path was not measured — and name open question U-3 explicitly. Match the hedge the production comment at `SVGControl/SvgRenderer.cs:397-400` already carries. Change no assertion and no test name. Acceptance: the comment no longer asserts the universal claim, names U-3, and `dotnet tool run csharpier check .` returns `EXIT_CODE: 0`
- [x] [P1-T18] Create `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` recording the coverage residual R-4 deliberately does not close, per its explicit scope boundary: on `SVGControl/SvgRenderer.cs`, `AddMargins` (0/15), `Render()` (18/26), and the two `SvgDocument` constructor overloads (0/8 each); and in the rest of the `SVGControl` assembly, `DropDownEditor` (0/99), `SVGParser` (0/122), `ToggleSwitch` (0/62 plus 0/23 designer), `SvgFileNameEditor` (0/104), and three converters (0/48, 0/48, 0/26). State that none of these is part of issue #418, that the modified-file line-coverage floor of 85% on `SVGControl/SvgRenderer.cs` is the gap this entry owns, and cite `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md` as the measurement source. Acceptance: the entry exists, names issue #418 as its origin, and enumerates every figure above
- [x] [P1-T19] Targeted verification before the final QC loop. Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` from the repository root. `-SearchRoot .` is mandatory: the single-project form of the shared MSTest wrapper is defective under `Set-StrictMode`. Acceptance: artifact `evidence/regression-testing/remediation-tests.2026-08-05T01-50.md` records `EXIT_CODE:`, the assembly count, total/passed/failed counts with failed equal to zero, and names each test added by `[P1-T12]`, `[P1-T14]`, and `[P1-T15]` with its individual outcome; it also confirms that no test passing in `evidence/qa-gates/test-coverage.2026-08-04T14-36.md` now fails

### Phase 2 — Final QC Loop

Run stages in the `CLAUDE.md` C# toolchain order: format, then lint, then type-check, then test. **If any stage
fails or changes any file, fix the cause and restart this phase from `[P2-T1]`.** Every command below is
unconditional: `EXIT_CODE: SKIPPED` is not a valid outcome for any task in this phase. Artifact directory:
`evidence/qa-gates/`.

- [x] [P2-T1] Run `dotnet tool run csharpier format .` from the repository root, covering `SVGControl/SvgRenderer.cs`, `SVGControl/SvgAssemblyProbe.cs`, `SVGControl/SvgAssemblyResolver.cs`, and the edited files under `SVGControl.Test/`. Acceptance: artifact `evidence/qa-gates/csharpier-format.2026-08-05T01-50.md` records `EXIT_CODE:` and the count of files reformatted; if that count is non-zero, the loop restarts from this task after the reformat is committed to the working tree
- [x] [P2-T2] Run `dotnet tool run csharpier check .` from the repository root. Acceptance: artifact `evidence/qa-gates/csharpier-check.2026-08-05T01-50.md` records `EXIT_CODE: 0` and zero files needing formatting
- [x] [P2-T3] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"` from the repository root. Acceptance: artifact `evidence/qa-gates/restore.2026-08-05T01-50.md` records `EXIT_CODE: 0`
- [x] [P2-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` from the repository root. Acceptance: artifact `evidence/qa-gates/analyzer-build.2026-08-05T01-50.md` records `EXIT_CODE: 0`, zero errors, and a per-code per-project warning inventory that is compared line by line against `evidence/remediation-baseline/analyzer-build.2026-08-05T01-50.md`; any diagnostic code, count, text, or emitting project not present in that baseline is a newly introduced diagnostic and must be fixed, after which the loop restarts from `[P2-T1]`
- [x] [P2-T5] Run the mandated nullable gate `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` from the repository root, then run the same two supplementary forced project-scope rebuilds `[P0-T8]` ran, with the identical property set, for `SVGControl\SVGControl.csproj` and `SVGControl.Test\SVGControl.Test.csproj`. Acceptance: artifact `evidence/qa-gates/nullable-build.2026-08-05T01-50.md` records `EXIT_CODE: 0` for the mandated command, restates that this exit code is vacuous in an up-to-date tree and is not evidence of nullable cleanliness, and records each supplementary rebuild's command, `EXIT_CODE:`, and complete per-code per-file diagnostic table compared against `evidence/remediation-baseline/nullable-build.2026-08-05T01-50.md`. The `SVGControl` supplementary set must contain no diagnostic absent from that baseline. The `SVGControl.Test` supplementary set must be zero diagnostics if `[P1-T7]` recorded `R2_KEEP`, or exactly the one `CS8630` from the baseline if it recorded `R2_REVERTED_OUT_OF_SCOPE_NULLABLE`; any other outcome is a newly introduced diagnostic, must be fixed, and restarts the loop from `[P2-T1]`
- [x] [P2-T6] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` from the repository root and read `coverage/coverage.cobertura.xml`. `-SearchRoot .` is mandatory. Acceptance: artifact `evidence/qa-gates/test-coverage.2026-08-05T01-50.md` records `EXIT_CODE: 0`, the assembly count, total/passed/failed counts with failed equal to zero, and the numeric repository-wide `line-rate` and `branch-rate` as covered/total and percent
- [x] [P2-T7] Write the coverage comparison to `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md`, using the same per-`<line>`-descendant counting method `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` uses so the comparison stays like-for-like, and assessing every per-member gate on the Cobertura `<method>` element's `line-rate` with `branch-rate` recorded for information only. It must record: repository-wide line and branch figures before and after, with an explicit verdict against the `>= 85%` line floor and the `>= 75%` branch floor; the `SVGControl` package figures before and after; `SVGControl.SvgRenderer`, `SVGControl.SvgAssemblyProbe`, and `SVGControl.SvgAssemblyResolver` class figures, with the relocation accounted for so the reader can see that `SvgRenderer`'s denominator fell because `ResolveByNameAndKey` and `PublicKeyTokensEqual` moved out and not because any line lost coverage; per-member `line-rate` for `PublicKeyTokensEqual`, the three-argument byte-array constructor, `ResolveByNameAndKey`, and the sole genuinely new member `SvgAssemblyResolver.Install()`; a statement that `SVGControl.SvgAssemblyProbe` remains at 100% line and branch coverage; the re-recorded exception line `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey` with a cross-reference to its original ratification; a statement that `SVGControl.SvgAssemblyResolver` is a relocation and not a new module; and an explicit statement that the `>= 85%` modified-file floor on `SVGControl/SvgRenderer.cs` is **not** targeted this cycle per R-4's scope boundary, naming `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` as the entry that owns the residual. Acceptance: every figure above is numeric, no placeholder appears, and the repo-wide verdict is PASS. If `SvgAssemblyResolver.Install()` measures below 90% `line-rate`, or if any repository-wide floor fails, or if any changed line lost coverage, add the necessary test and restart the loop from `[P2-T1]`
- [x] [P2-T8] Verify the file-size gate by rerunning the `[P0-T5]` command extended with `SVGControl/SvgAssemblyResolver.cs`. Acceptance: artifact `evidence/qa-gates/file-size.2026-08-05T01-50.md` records the post-change line count of all six files, confirms `SVGControl/SvgRenderer.cs` is at most 400 lines, and confirms no file exceeds 500 lines. If any file exceeds 500 lines, resolve it and restart the loop from `[P2-T1]`
- [x] [P2-T9] Write `evidence/qa-gates/toolchain-clean-pass.2026-08-05T01-50.md` recording the single consecutive clean pass: the pass number, each of the six commands from `[P2-T1]` through `[P2-T6]` in order with its `EXIT_CODE:`, an explicit statement of whether any loop restart occurred and why, and the confirmation that no source, test, or build-configuration file was modified after the pass was recorded. Acceptance: the artifact records `Pass number:` and shows `EXIT_CODE: 0` for all six commands within one uninterrupted pass
- [x] [P2-T10] Append dated evidence-note amendments to `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` for AC-2, AC-5, and AC-8 only, recording that R-6 relocated the `AssemblyResolve` region to `SVGControl/SvgAssemblyResolver.cs` and `PublicKeyTokensEqual` to `SVGControl/SvgAssemblyProbe.cs`, so the file-and-line citations in those three notes now resolve to the new locations; that AC-2's catch-site inventory gains the containment catch added by `[P1-T10]`, which uses `Trace.TraceWarning` and not `log4net` for the documented re-entrancy reason; and that AC-5's coverage figures are superseded by `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md`. The AC-8 amendment must also carry the new test count: its existing note says "the nine `SvgAssemblyProbeDirectoryTests`", and after `[P1-T12]` (+1) and `[P1-T15]` (+8) there are **eighteen**, so the appended note states that figure and a reaudit does not read "nine" as newly stale. Change no AC text and no checkbox: AC-1 through AC-10 stay `[x]` and **AC-11 stays `[ ]`** because R-1 is excluded from this plan. Mirror the same text to `evidence/issue-updates/issue-418.2026-08-05T01-50.md` with `PostedAs:` recorded. Acceptance: `git diff -- docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` contains only additions under AC-2, AC-5, and AC-8, and no line beginning `- [ ]` or `- [x]` changed state
- [x] [P2-T11] Write `evidence/other/remediation-completion-summary.2026-08-05T01-50.md` reconciling this cycle's exit state: a row per item R-2 through R-6 naming its delivering task IDs, its outcome, and its evidence artifact; the literal `[P1-T7]` gate token and what it means for R-2; the confirmation that R-1 remains open, is human-only, is tracked as human_interaction requirements H-1 and H-2 with response `exception`, and that AC-11 is still `[ ]`; the two `docs/features/potential/` entries created or deliberately not created; and the confirmation from `git diff --stat HEAD -- docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md` that the completed plan file was not modified by this cycle. Acceptance: every row cites an artifact that exists on disk, and checkbox state in this plan file matches the evidence recorded

## Test Plan

- **New tests, all in existing files, all written in C# 7.3-compatible syntax per Design Decision 9:** one
  invalid-character `baseDirectory` test (`[P1-T12]`), one three-argument byte-array constructor success test
  (`[P1-T14]`), eight `PublicKeyTokensEqual` cases (`[P1-T15]`). Ten tests total.
- **Framework and libraries:** MSTest attributes, Moq where a seam is needed, FluentAssertions for every
  assertion, per `.claude/rules/csharp.md`.
- **Determinism:** no temporary file, no live `Assembly.LoadFrom`, no network, no wall-clock read, no mutable
  global state. The invalid-path character is obtained from `Path.GetInvalidPathChars()[0]` rather than a
  hard-coded character, so the test does not depend on the platform's specific ordering.
- **No test may be weakened, renamed, or deleted.** The nine existing `SvgAssemblyProbeDirectoryTests` and the
  fourteen existing `SvgRendererParseContractTests` must pass unchanged.
- **Regression guard:** `[P1-T19]` and `[P2-T6]` both run the full suite; `evidence/qa-gates/test-coverage.2026-08-04T14-36.md`
  (6140/6140 passed, 0 failed, nine assemblies) is the comparison basis.

## Risks and Open Items

1. **R-2 is likely to end in the revert branch.** Inspection of `SVGControl.Test` shows nullable diagnostics
   that will surface in out-of-scope auto-generated files as soon as `<LangVersion>latest</LangVersion>` lets
   `/p:Nullable=enable` take effect: `private System.ComponentModel.IContainer components = null;` in
   `Form1.Designer.cs:8` and `Form2.Designer.cs:8` (CS8625), and the uninitialized `resourceMan` /
   `resourceCulture` static fields plus `return resourceCulture;` and the `(byte[])(obj)` casts in both
   `Resources.Designer.cs` files (CS8618, CS8603, CS8600). `[P1-T6]` measures the real set and `[P1-T7]`
   decides deterministically; the orchestrator should expect `R2_REVERTED_OUT_OF_SCOPE_NULLABLE` and treat
   `[P1-T6]`'s measurement plus `[P1-T9]`'s entry as R-2's delivered value in that case.
2. **The mandated solution-level nullable gate cannot detect the above either way**, because the preceding
   analyzer build leaves every project up to date and the nullable build recompiles nothing. That is why
   `[P0-T8]` and `[P2-T5]` carry supplementary forced project-scope rebuilds.
3. **`SVGControl` forced-rebuild diagnostics are an unmeasured quantity before `[P0-T8]` runs.** `SVGControl`
   contains untouched legacy files (`DropDownEditor`, `SVGParser`, `ToggleSwitch`, `SvgFileNameEditor`, the
   converters) that have never been compiled under `/p:Nullable=enable` at forced scope. `[P0-T8]` captures
   whatever they emit as the baseline, so `[P2-T5]` compares against it rather than against zero. Pre-existing
   `SVGControl` diagnostics are not this cycle's to fix and must not trigger edits outside the Scope Lock.
4. **The `SvgRenderer.cs` coverage figure will move for two reasons at once** — the R-6 denominator reduction
   and the R-4 numerator addition. `[P2-T7]` must separate them, or a reaudit cannot tell an improvement from
   an accounting artifact.
5. **G-6, recorded in the policy audit, is unresolved and is not this plan's to fix.**
   `.claude/skills/feature-review-workflow/SKILL.md` step 8 assigns remediation-plan authorship to
   `feature-review`, while `.claude/skills/remediation-handoff-atomic-planner/SKILL.md` assigns it to
   `atomic-planner`, and the two skills also disagree on artifact layout. This plan follows
   `remediation-handoff-atomic-planner` for authorship and the flat
   `docs/features/active/<slug>/<stem>.<timestamp>.md` layout that
   `.claude/hooks/validate-feature-review-coverage.ps1` enforces. Resolving the skill conflict is outside the
   Scope Lock.

## Exit Criteria

This cycle is complete when all of the following hold:

1. Every task in Phases 0, 1, and 2 is `[x]` with its artifact on disk.
2. `[P2-T9]` records one consecutive clean toolchain pass with `EXIT_CODE: 0` at all six stages.
3. `[P2-T7]` records repository-wide line coverage `>= 85%`, branch coverage `>= 75%`, no regression on any
   changed line, and `SvgAssemblyResolver.Install()` at `>= 90%` `line-rate`.
4. `[P2-T8]` records `SVGControl/SvgRenderer.cs` at most 400 lines and no file above 500.
5. `[P1-T7]` recorded one of the two literal gate tokens, and `[P2-T5]`'s `SVGControl.Test` supplementary
   diagnostic set matches the token.
6. AC-11 is still `- [ ]` and `plan.2026-08-04T14-36.md` is unmodified.
7. The reaudit input set is `evidence/qa-gates/*.2026-08-05T01-50.md`,
   `evidence/other/*.2026-08-05T01-50.md`, `evidence/regression-testing/remediation-tests.2026-08-05T01-50.md`,
   and `evidence/issue-updates/issue-418.2026-08-05T01-50.md`.

R-1 remains open by design. This cycle cannot clear the blocking count; it clears the five non-blocking items
and leaves the single blocking item where only the user can close it.
