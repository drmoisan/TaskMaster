# svg-renderer-null-document-nre — Plan

- **Issue:** #418
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-04T14-36
- **Status:** Draft
- **Version:** 0.3 (revision pass 1 — applies six blocking and seven non-blocking preflight findings)
- **Work Mode:** `minor-audit` (persisted marker `- Work Mode: minor-audit` in `issue.md`)
- **Language in scope:** C# only

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QC artifact tasks, and coverage-comparison tasks. If any required baseline artifact, QC artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path in each evidence-producing task. Do not mark evidence-backed work complete without the artifact on disk.

## Required References

- `CLAUDE.md` (repo-root standing instructions; policy compliance order and C# toolchain order)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` — the `## Acceptance Criteria` section (AC-1 through AC-11) is the **sole** requirements source for this plan
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/research/2026-08-04T15-05-svg-renderer-null-document-research.md`
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md`
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`

**All work must comply with these policies; do not duplicate their content here.**

## Work-Mode Notes (minor-audit, fail-closed)

- `spec.md` and `user-story.md` are **intentionally absent** from this feature folder and must **not** be required by any task, validation, or audit.
- If `spec.md` or `user-story.md` is found to exist in this feature folder, execution fails closed and the orchestrator must be notified before any Phase 1 task begins (checked by task P0-T3).
- If the `## Acceptance Criteria` section is missing from `issue.md`, execution fails closed (checked by task P0-T3).
- AC-7 is already satisfied in writing by the research artifact; this plan only records the check-off.
- **AC-11 is not an executable task.** It is satisfied by a human runbook execution and is represented here as an explicit handoff (task P2-T10). The executor must leave `- [ ] **AC-11 ...` unchecked in `issue.md`.

## Environment Precondition (why Phase 0 begins with a bootstrap task)

`global.json` pins SDK `8.0.205` with `"paths": [".dotnet-sdk", "$host$"]`, and `.dotnet-sdk/` does not exist in a fresh checkout. In that state `dotnet tool run csharpier --version` fails with an instruction to run `scripts/vscode/Install-RepoDotNetSdk.ps1`. That script does not perform `dotnet tool restore`, so csharpier `1.2.6` (manifest at repo-root `dotnet-tools.json`) must be restored separately. Independently, `dotnet-coverage` is not installed and is not present in `~/.dotnet/tools`; `scripts/vscode/Invoke-MSTestWithCoverage.ps1:129-131` throws without it. Without the bootstrap, tasks P0-T6, P2-T1, and P2-T2 (csharpier) and tasks P0-T9 and P2-T6 (coverage) cannot run — and the latter two carry the mandatory numeric coverage evidence. Task P0-T1 exists solely to remove this precondition.

Package restore itself is viable as written: `packages/` is gitignored and is restored by `scripts/vscode/Invoke-Restore.ps1` running `msbuild /t:Restore /p:RestorePackagesConfig=true`; the pinned packages resolve from nuget.org; and the `EnsureNuGetPackageBuildImports` target is `BeforeTargets="PrepareForBuild"`, so it does not fire during restore.

## Scope Lock (files this plan is permitted to change)

Production C#:

- `SVGControl/SvgRenderer.cs`

Build/configuration:

- `TaskMaster.sln` (add the `SVGControl.Test` project entry plus its twelve `GlobalSection(ProjectConfigurationPlatforms)` mapping lines covering all six solution configurations)
- `SVGControl.Test/app.config` (ExCSS binding redirect only, line 23)
- `SVGControl.Test/SVGControl.Test.csproj` (`<Compile Include>` entries for the new test files; one new `<Reference Include="Svg, ...>` item per task P1-T4; package version paths in `<Reference>` `Version=`/`<HintPath>` and the `MSTest.TestAdapter` `<Import>`/`<Error>` paths only under the task P1-T3 contingency)
- `SVGControl.Test/packages.config` (unconditionally in scope: the `Svg 3.4.7` entry required by task P1-T4; plus package version retargeting under the task P1-T3 contingency)

New test C# (all in `SVGControl.Test`, all requiring explicit `<Compile Include>` wiring because the project uses `packages.config` with no glob):

- `SVGControl.Test/SvgRendererParseContractTests.cs`
- `SVGControl.Test/SvgRendererNullToleranceTests.cs`
- `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs`

Pre-existing `SVGControl.Test` files, editable ONLY to clear an analyzer or nullable
diagnostic newly introduced by the project entering the solution gate (tasks P1-T6/P1-T7):

- `SVGControl.Test/Form1.cs`, `SVGControl.Test/Form1.Designer.cs`
- `SVGControl.Test/Form2.cs`, `SVGControl.Test/Form2.Designer.cs`
- `SVGControl.Test/Resources.Designer.cs`
- `SVGControl.Test/Properties/AssemblyInfo.cs`
- `SVGControl.Test/GetRelativePath_Test.cs`
- `SVGControl.Test/RelativePathCoverageTests.cs`

Preferred remediation for auto-generated files (`*.Designer.cs`) is a scoped
`#nullable disable` / `#nullable restore` pair around the offending member, not a
whole-file directive and not a behavioral rewrite, because these files are regenerated
by `ResXFileCodeGenerator` / the WinForms designer.

Measured exposure across those eight files is 663 lines total. The realistic diagnostics are `components = null` (CS8625) in both Designer files and the `resourceMan` / `resourceCulture` / `GetObject` members in `Resources.Designer.cs` — roughly ten diagnostics. This is a bounded remediation, not a nullable sweep; tasks P1-T6 and P1-T7 carry an explicit `SCOPE_EXCEEDED` stop clause.

Documentation and evidence:

- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` (AC check-offs only)
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md` (checkbox state only)
- `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/**`

**Explicitly out of scope** (do not change):

- The Fizzler binding redirects in any `app.config`. Research §5.3 classifies these as a latent, currently inert defect deferred to a separate issue.
- Removal of the `<style>` element from `SVGControl/SvgImageSelector.cs`'s default SVG. Research §2.2 establishes that `Svg` binds ExCSS at JIT time for the whole `SvgDocument.Create<T>` method body regardless of `styles.Any()`, so this approach does not work.
- `SVGControl/SvgImageSelector.cs`, `SVGControl/PictureBoxSVG.cs`, `SVGControl/ButtonSVG.cs`, `SVGControl/SVGParser.cs`. AC-4 requires the existing null-tolerant consumers keep their current behavior, which they do without modification.
- Any ExCSS package downgrade or `devenv.exe.config` edit (research §10.3, rejected alternatives).
- The `newVersion="4.2.4.0"` binding redirects for `System.Threading.Tasks.Extensions` that appear in 16 tracked `app.config` files and 6 `.csproj` files. These are unrelated to ExCSS, are correct as written, and must not be touched by task P1-T2.

## Design Decisions Fixed by This Plan

These are settled before execution so that no task requires interpretation.

1. **Failure mode is degrade-and-log, not throw from the constructor.** AC-3 is explicit: the byte-array `SvgRenderer` constructors must not throw. They leave `_doc` null and initialize `_original` to `Size.Empty`.
2. **Dual diagnostic channel.** Every parse-failure diagnostic is written both through the existing `log4net.ILog logger` field (`SVGControl/SvgRenderer.cs:20-22`) at error level **and** through `System.Diagnostics.Trace.TraceError`, because there is no evidence a `log4net` appender is configured inside `devenv.exe`. Both channels carry the exception type and message.
3. **Fail-fast API shape.** Two new members on `SvgRenderer`:
   - `public static bool TryGetSvgDocument(byte[] file, out SvgDocument document, out Exception error)` — the `Try`-style member that surfaces the captured exception (AC-4).
   - `public static SvgDocument GetSvgDocumentOrThrow(byte[] file)` — throws `InvalidOperationException` whose `InnerException` is the original exception from `SvgDocument.Open` when one exists, and whose `InnerException` is `null` for the element-free path where `SvgDocument.Open` returns `null` without throwing (AC-5's stated asymmetry).
   - `public static SvgDocument GetSvgDocument(byte[] file)` keeps its existing tolerant `null`-returning contract and contains **no** `catch` block of its own; it delegates to `TryGetSvgDocument`.
4. **Single catch site.** All exception handling for the parse path lives in one `catch (Exception)` inside the seam-bearing `TryGetSvgDocument` overload. It logs on both channels and returns `false` with the exception in `error`, which is a result the caller is required to inspect (AC-2).
5. **Seam.** The smallest seam per `.claude/rules/csharp.md` DI Seams: an `internal static bool TryGetSvgDocument(byte[] file, Func<byte[], SvgDocument> parse, out SvgDocument document, out Exception error)` overload. Production supplies `SvgRenderer.OpenFromBytes`. No mutable static hook is introduced, so no test mutates global state. Plain malformed bytes and `Array.Empty<byte>()` are used wherever they already exercise the path; the seam is used only to assert exact `InnerException` identity.
6. **`AssemblyResolve` strategy order is 1 → 2 → 3.** Strategy 1 (already-loaded scan) and strategy 2 (`Assembly.Load` by simple name) are preserved unchanged in behavior and order, per research §4.4's instruction to prefer an already-loaded match. Strategy 3 (directory probing with `Assembly.LoadFrom`) is new and runs after strategy 2 inside the same re-entrance-guarded region.
7. **New test files require csproj wiring.** `SVGControl.Test.csproj` uses `packages.config` and explicit `<Compile Include>` items with no glob. Every new `.cs` file must be added to the `<ItemGroup>` at `SVGControl.Test/SVGControl.Test.csproj:61-82` or it will not compile.
8. **`SVGControl.Test` needs a direct compile-time `Svg` reference.** `SVGControl.Test.csproj:122-151` contains no `<Reference Include="Svg" ...>` and `SVGControl.Test/packages.config` has no `Svg` entry; the only path to `SVGControl` is the `ProjectReference` at `SVGControl.Test/SVGControl.Test.csproj:84-87`. Because this is a legacy non-SDK project, transitive assembly references do not flow to the compiler — they land in `ReferenceDependencyPaths` (copy-local), not `ReferencePath`. Every planned test names `SvgDocument` (task P1-T8 asserts `renderer.Document` is null, typed `SvgDocument` at `SVGControl/SvgRenderer.cs:218`; task P1-T20 declares `Mock<Func<byte[], SvgDocument>>`; task P1-T21 assigns `SvgRenderer.Document = null`), so without the direct reference the tests fail to compile with `CS0012` for `Svg, Version=3.4.0.0, PublicKeyToken=12a0bac221edeae2`. Task P1-T4 adds it.
9. **AC-4's "public API" is assembly-internal by design.** `SvgRenderer` is declared `internal class` at `SVGControl/SvgRenderer.cs:18`, reachable only from within `SVGControl` and, via `InternalsVisibleTo`, from `SVGControl.Test`. The `public static` modifiers on `TryGetSvgDocument` / `GetSvgDocumentOrThrow` / `GetSvgDocument` therefore describe the type-internal surface, not a cross-assembly public surface. This is deliberate and is not a defect to remediate; task P1-T24 must state it explicitly when checking off AC-4.

## Evidence Location Invariant

All evidence artifacts produced by this plan are written under
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/<kind>/`.
`artifacts/`-rooted evidence paths are forbidden and are blocked by the
`.claude/hooks/enforce-evidence-locations.ps1` PreToolUse hook. Every baseline and final-QC
command step has its own artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:`, and
`Output Summary:`. C# has mandatory coverage policy, so baseline and final-QC test artifacts
record numeric coverage values, never placeholders.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Compliance Reads

- [x] [P0-T1] Bootstrap the repo-local toolchain: run
  `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Install-RepoDotNetSdk.ps1`
  from the repository root, then `dotnet tool restore`, then
  `dotnet tool install --global dotnet-coverage` (skip the install only if
  `Get-Command dotnet-coverage` already resolves)
  - Acceptance: `evidence/baseline/toolchain-bootstrap.2026-08-04T14-36.md` created
    containing `Timestamp:`, `Command:` (all three commands), `EXIT_CODE: 0` for each,
    and `Output Summary:` recording that `.dotnet-sdk/` exists, that
    `dotnet tool run csharpier --version` prints `1.2.6`, and that
    `dotnet-coverage --version` resolves
- [x] [P0-T2] Read `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md` in that exact order, in full
  - Acceptance: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/baseline/phase0-instructions-read.md` exists containing `Timestamp:`, a `Policy Order:` line listing those four files in that order, and an explicit list of files read
- [x] [P0-T3] Read `issue.md` and confirm it contains an explicit `## Acceptance Criteria` section with AC-1 through AC-11, contains the marker `- Work Mode: minor-audit`, and confirm that neither `spec.md` nor `user-story.md` exists in `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/`
  - Acceptance: `phase0-instructions-read.md` updated with an `AC source:` line naming `issue.md` `## Acceptance Criteria`, an `AC count: 11` line, a `Work Mode: minor-audit` line, and a `Fail-closed check:` line recording `spec.md: absent` and `user-story.md: absent`. If either document is present, stop and report `MODE_FAIL_CLOSED` instead of continuing
- [x] [P0-T4] Read `research/2026-08-04T15-05-svg-renderer-null-document-research.md` and `runbooks/verify-winforms-designer-load.runbook.md` in full
  - Acceptance: `phase0-instructions-read.md` lists both documents under "files read" with their exact relative paths
- [x] [P0-T5] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"` from the repository root and capture the baseline restore state into `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/baseline/`
  - Acceptance: `evidence/baseline/restore.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE:`, and `Output Summary:` recording whether restore succeeded and any package-resolution warning text
- [x] [P0-T6] Run `dotnet tool run csharpier check .` from the repository root and capture the baseline formatting state, which covers `SVGControl/SvgRenderer.cs` and every file under `SVGControl.Test/`
  - Acceptance: `evidence/baseline/csharpier-check.2026-08-04T14-36.md` created containing `Timestamp:`, `Command: dotnet tool run csharpier check .`, `EXIT_CODE:`, and `Output Summary:` recording the number of files reported as needing formatting (`0` if clean)
- [x] [P0-T7] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` and capture the baseline analyzer state
  - Acceptance: `evidence/baseline/analyzer-build.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE:`, and `Output Summary:` recording build success/failure and the warning/error counts
- [x] [P0-T8] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` and capture the baseline nullable/type-check state
  - Acceptance: `evidence/baseline/nullable-build.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE:`, and `Output Summary:` recording build success/failure and the error count
- [x] [P0-T9] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and capture the baseline test and coverage state, then read `coverage/coverage.cobertura.xml` for the numeric coverage headline
  - Acceptance: `evidence/baseline/test-coverage.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE:`, and `Output Summary:` recording total tests / passed / failed / skipped, the numeric repository-wide `line-rate` and `branch-rate` read from `coverage/coverage.cobertura.xml` expressed as percentages, and the numeric line coverage for the `SVGControl` package element. Placeholder values such as `UNVERIFIED` are not acceptable
- [x] [P0-T10] Record the actual baseline buildability state of `SVGControl.Test`: run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath SVGControl.Test/SVGControl.Test.csproj -Configuration Debug -Platform AnyCPU`, and separately record (a) whether `SVGControl.Test` appears in `TaskMaster.sln`, and (b) which of `packages/Castle.Core.5.1.1`, `packages/FluentAssertions.6.12.0`, `packages/Moq.4.20.69`, `packages/MSTest.TestAdapter.3.1.1`, `packages/MSTest.TestFramework.3.1.1`, `packages/System.Runtime.CompilerServices.Unsafe.6.0.0`, `packages/System.Threading.Tasks.Extensions.4.5.4` exist on disk
  - Acceptance: `evidence/baseline/svgcontrol-test-buildability.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact build command above), the observed non-zero `EXIT_CODE:`, and `Output Summary:` recording the verbatim `EnsureNuGetPackageBuildImports` error text, `SVGControl.Test present in TaskMaster.sln: false`, and a per-package present/absent line for all seven pinned packages. This artifact records the real broken state; it must not record a fabricated passing baseline

### Phase 1 — Constrained Small-Path Implementation

**Prerequisite sub-block.** Nothing downstream in this phase can be verified until `SVGControl.Test` builds and runs, so the seven prerequisite tasks come first and each carries its own verification.

- [x] [P1-T1] Add `SVGControl.Test` to `TaskMaster.sln`: one `Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "SVGControl.Test", "SVGControl.Test\SVGControl.Test.csproj", "{13AC39E6-DE06-4337-8EB0-41CE674A4C3B}"` entry plus **twelve** `GlobalSection(ProjectConfigurationPlatforms)` mapping lines covering all six solution configurations (`Debug|Any CPU`, `Debug|x64`, `Debug|x86`, `Release|Any CPU`, `Release|x64`, `Release|x86`), following the `UtilitiesCS.Test` pattern at `TaskMaster.sln:118-129` (x64 maps to `Any CPU`; x86 maps to `x86`, which `SVGControl.Test/SVGControl.Test.csproj:53-59` defines)
  - Acceptance: `TaskMaster.sln` contains exactly one `Project(...) = "SVGControl.Test"` line with GUID `{13AC39E6-DE06-4337-8EB0-41CE674A4C3B}` and exactly twelve `{13AC39E6-DE06-4337-8EB0-41CE674A4C3B}.` configuration-mapping lines; the file remains CRLF-encoded and its BOM is preserved — contributes to AC-9
  - Note: git-bash `sed -i` on `TaskMaster.sln` produces whole-file line-ending churn and loses the BOM. Use the Edit tool, or `perl -0777` with explicit `\r\n`, to make this change.
- [x] [P1-T2] Change `SVGControl.Test/app.config:23` from `<bindingRedirect oldVersion="0.0.0.0-4.2.4.0" newVersion="4.2.4.0" />` to `<bindingRedirect oldVersion="0.0.0.0-4.3.1.0" newVersion="4.3.1.0" />`, matching `SVGControl/app.config:15`
  - Acceptance: `SVGControl.Test/app.config` contains the literal string `oldVersion="0.0.0.0-4.3.1.0" newVersion="4.3.1.0"` inside the `ExCSS` `dependentAssembly` block, and a repository-wide search for `newVersion="4.2.4.0"` inside any `ExCSS` `dependentAssembly` block returns zero matches. Matches of `newVersion="4.2.4.0"` for `System.Threading.Tasks.Extensions` are unrelated, correct, and explicitly out of scope — satisfies AC-10
- [x] [P1-T3] Make every `..\packages\`-rooted path referenced by `SVGControl.Test/SVGControl.Test.csproj` resolve on disk. Primary action: run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"` so the seven pins in `SVGControl.Test/packages.config` (`Castle.Core 5.1.1`, `FluentAssertions 6.12.0`, `Moq 4.20.69`, `MSTest.TestAdapter 3.1.1`, `MSTest.TestFramework 3.1.1`, `System.Runtime.CompilerServices.Unsafe 6.0.0`, `System.Threading.Tasks.Extensions 4.5.4` — the last two referenced at `SVGControl.Test/SVGControl.Test.csproj:143-148`) are restored under `packages/`. **Authorized contingency:** if restore cannot obtain a pinned version, retarget that pin in `SVGControl.Test/packages.config` and every corresponding `<Reference>` `Version=`/`<HintPath>` plus the `<Import>` and `<Error>` `MSTest.TestAdapter` paths at `SVGControl.Test/SVGControl.Test.csproj:7-10`, `:158-170`, and `:171-174` to a version verified present under `packages/` after restore, preferring the version used by `UtilitiesCS.Test/packages.config`, and record the substitution together with the on-disk `packages/` folder name that was verified
  - Acceptance: `evidence/other/package-restore-decision.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, a per-path table listing every `..\packages\` path appearing in `SVGControl.Test/SVGControl.Test.csproj` with `resolves: true` for each, and a `Route:` line reading either `restored pinned versions` or `retargeted to <version> (contingency)` with the substituted versions named and the verified on-disk `packages/<id>.<version>` folder named for each substitution — contributes to AC-9
- [x] [P1-T4] Add the compile-time `Svg` reference that the new tests require. In `SVGControl.Test/packages.config` add `<package id="Svg" version="3.4.7" targetFramework="net481" />`, and in `SVGControl.Test/SVGControl.Test.csproj` add `<Reference Include="Svg, Version=3.4.0.0, Culture=neutral, PublicKeyToken=12a0bac221edeae2, processorArchitecture=MSIL">` with `<HintPath>..\packages\Svg.3.4.7\lib\net481\Svg.dll</HintPath>`, matching `SVGControl/SVGControl.csproj:66-67`. Rationale: `SVGControl.Test` is a legacy non-SDK project, so the `SVGControl` ProjectReference does not flow `Svg` to the compiler; every planned test names `SvgDocument` and would otherwise fail with CS0012
  - Acceptance: `SVGControl.Test/SVGControl.Test.csproj` contains exactly one `<Reference Include="Svg,` item whose `<HintPath>` resolves on disk, and `SVGControl.Test/packages.config` contains the `Svg 3.4.7` entry
- [x] [P1-T5] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath SVGControl.Test/SVGControl.Test.csproj -Configuration Debug -Platform AnyCPU` and confirm the project compiles
  - Acceptance: `evidence/qa-gates/svgcontrol-test-build.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE: 0`, and `Output Summary:` confirming the `EnsureNuGetPackageBuildImports` `<Error>` did not fire and that `SVGControl.Test/bin/Debug/SVGControl.Test.dll` exists on disk — satisfies the "compiles" half of AC-9
- [ ] [P1-T6] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` and confirm that bringing `SVGControl.Test` into the solution introduces no analyzer diagnostic that was absent from the task P0-T7 baseline; remediate any new diagnostic within `SVGControl.Test`-owned files only, restricted to the Scope Lock's pre-existing-`SVGControl.Test`-files list. If clearing the new diagnostics requires editing any file outside the list above, or requires more than 20 diagnostic-clearing edits, stop and report `SCOPE_EXCEEDED` to the orchestrator rather than continuing — an unbounded nullable sweep exceeds the minor-audit budget
  - Acceptance: `evidence/qa-gates/prereq-analyzer-build.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE: 0`, and `Output Summary:` with a `New diagnostics vs baseline: 0` line and a `Files edited for remediation:` line naming each edited file (or `none`)
- [ ] [P1-T7] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` and confirm that bringing `SVGControl.Test` into the solution introduces no nullable/type-check error that was absent from the task P0-T8 baseline; remediate any new error within `SVGControl.Test`-owned files only, restricted to the Scope Lock's pre-existing-`SVGControl.Test`-files list, preferring a scoped `#nullable disable` / `#nullable restore` pair in `*.Designer.cs`. If clearing the new diagnostics requires editing any file outside the list above, or requires more than 20 diagnostic-clearing edits, stop and report `SCOPE_EXCEEDED` to the orchestrator rather than continuing — an unbounded nullable sweep exceeds the minor-audit budget
  - Acceptance: `evidence/qa-gates/prereq-nullable-build.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE: 0`, and `Output Summary:` with a `New errors vs baseline: 0` line and a `Files edited for remediation:` line naming each edited file (or `none`)

**Failing-regression sub-block.** Per the Bugfix Workflow in `CLAUDE.md`, the deterministic regression test is written and observed failing before any production change.

- [ ] [P1-T8] [expect-fail] Create `SVGControl.Test/SvgRendererParseContractTests.cs` containing a `[TestClass]` with exactly four `[TestMethod]` regression tests that assert the post-fix contract and therefore fail against the current code, and add a `<Compile Include="SvgRendererParseContractTests.cs" />` entry to the `<ItemGroup>` at `SVGControl.Test/SVGControl.Test.csproj:61-82`. The four tests are: constructing `new SvgRenderer(Encoding.ASCII.GetBytes("this is not xml"), new Size(16,16), AutoSize.MaintainAspectRatio)` does not throw and leaves `Document` null; the same for the four-argument overload `new SvgRenderer(byte[], Size, Padding, AutoSize)`; constructing from `Array.Empty<byte>()` does not throw and leaves `Document` null (the exception-free null path per research §1.4); and the same for the four-argument overload. Use MSTest attributes, FluentAssertions assertions restricted to APIs present in both FluentAssertions 6 and 8 (`Should().BeNull()`, `Should().NotBeNull()`, `Should().NotThrow()`, `Should().Throw<T>()`, `Should().Be(...)`), Arrange–Act–Assert structure, no temporary files, and no network
  - Acceptance: `SVGControl.Test/SvgRendererParseContractTests.cs` exists with exactly four `[TestMethod]` members named for the scenario under test; `SVGControl.Test/SVGControl.Test.csproj` contains `<Compile Include="SvgRendererParseContractTests.cs" />`; the file contains no `Thread.Sleep`, `Task.Delay`, `Path.GetTempPath`, or `File.` write call
- [ ] [P1-T9] [expect-fail] Build `SVGControl.Test` with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath SVGControl.Test/SVGControl.Test.csproj -Configuration Debug -Platform AnyCPU`, then run the suite with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot SVGControl.Test -Configuration Debug`, and capture the pre-fix failures. Note: `Invoke-MSTest.ps1 -SearchRoot SVGControl.Test` discovers `*.Test.dll` under `bin\Debug\` and therefore runs the whole assembly, so the pre-existing `GetRelativePath_Test` and `RelativePathCoverageTests` results appear in the same run; report those separately from the four expected failures
  - Acceptance: `evidence/regression-testing/ac1-fail-before.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (both commands above), the build `EXIT_CODE: 0`, the observed non-zero test-run `EXIT_CODE:`, and `Output Summary:` recording all four new test names as failed together with the verbatim `System.NullReferenceException` message and the `SVGControl.SvgRenderer..ctor` stack frame for at least the malformed-bytes case, plus a separate `Pre-existing tests in same run:` line reporting the `GetRelativePath_Test` and `RelativePathCoverageTests` pass/fail counts — this is the AC-1 fail-before evidence

**Production-fix sub-block.**

- [ ] [P1-T10] In `SVGControl/SvgRenderer.cs`, add `internal static SvgDocument OpenFromBytes(byte[] file)` which wraps the byte array in a `MemoryStream` inside a `using` block and returns `SvgDocument.Open<SvgDocument>(stream)`, with no `catch` of its own
  - Acceptance: `SVGControl/SvgRenderer.cs` contains an `OpenFromBytes` method whose body uses a `using` statement over the `MemoryStream` and contains no `catch` keyword — closes research §10.2 constraint 6 (the current `MemoryStream` leak)
- [ ] [P1-T11] In `SVGControl/SvgRenderer.cs`, add `internal static bool TryGetSvgDocument(byte[] file, Func<byte[], SvgDocument> parse, out SvgDocument document, out Exception error)`. It must guard `file` and `parse` for null with `ArgumentNullException`, invoke `parse(file)` inside a single `try`, and in its one `catch (Exception ex)` block log through the existing `logger` field at error level **and** through `System.Diagnostics.Trace.TraceError`, with both messages carrying the exception type name and message, then set `document = null`, `error = ex`, and return `false`. When `parse` returns `null` without throwing it must set `document = null`, `error = null`, log the element-free condition on both channels, and return `false`. On success it sets `document` to the parsed value, `error = null`, and returns `true`
  - Acceptance: `SVGControl/SvgRenderer.cs` contains exactly one `catch (Exception` block on the parse path, located in this method; that block contains both a `logger.Error` call and a `Trace.TraceError` call; the method returns `bool` and has `out SvgDocument` and `out Exception` parameters — satisfies AC-2
- [ ] [P1-T12] In `SVGControl/SvgRenderer.cs`, add `public static bool TryGetSvgDocument(byte[] file, out SvgDocument document, out Exception error)` delegating to the task P1-T11 overload with `OpenFromBytes` as the `parse` argument, and add `public static SvgDocument GetSvgDocumentOrThrow(byte[] file)` which calls the public `Try` overload and, on `false`, throws `InvalidOperationException` whose message names the failure and whose `InnerException` is the `error` value (which is `null` for the element-free path)
  - Acceptance: `SVGControl/SvgRenderer.cs` declares both members with those exact signatures; `GetSvgDocumentOrThrow` contains no `catch` keyword and constructs its `InvalidOperationException` with the captured `error` as the inner exception — satisfies AC-4's fail-fast API requirement
- [ ] [P1-T13] Rewrite `public static SvgDocument GetSvgDocument(byte[] file)` in `SVGControl/SvgRenderer.cs` so it delegates to the public `TryGetSvgDocument` overload and returns the document on success or `null` on failure, with no `try`/`catch` of its own, preserving the existing `null`-returning contract relied on by `SvgImageSelector.ResourceName` (`SvgImageSelector.cs:130`) and `SvgImageSelector.SetDefaultImage()` (`SvgImageSelector.cs:284`)
  - Acceptance: the `GetSvgDocument` method body in `SVGControl/SvgRenderer.cs` contains no `try` or `catch` keyword, still returns `SvgDocument`, and `SVGControl/SvgImageSelector.cs` is unchanged — satisfies AC-4's null-tolerant-consumer requirement
- [ ] [P1-T14] Change both byte-array constructors in `SVGControl/SvgRenderer.cs` (currently lines 126-133 and 135-142) to call `TryGetSvgDocument`, assign `_doc` from the out parameter, set `_original` to `Size.Empty` when the document is null instead of dereferencing it, and on the failure path emit a constructor-scoped error record through both `logger.Error` and `System.Diagnostics.Trace.TraceError` naming the constructor and carrying the exception type and message. Neither constructor may throw as a result of a parse failure
  - Acceptance: neither byte-array constructor in `SVGControl/SvgRenderer.cs` contains an unguarded `_doc.Draw()` expression; both contain a null-document branch that assigns `Size.Empty` and calls both `logger.Error` and `Trace.TraceError`; neither contains a `throw` statement on the parse-failure path — satisfies AC-3
- [ ] [P1-T15] In `SVGControl/SvgRenderer.cs`, replace the bare `catch { }` at lines 94-97 inside `ResolveByNameAndKey` with `catch (Exception ex)` that writes a diagnostic through `System.Diagnostics.Trace.TraceWarning` carrying the requested assembly name and the exception type and message, then continues to the next strategy. Use `Trace` only in this handler — not `log4net` — and add a `why` comment stating that `log4net` is avoided here because logging inside an `AssemblyResolve` handler can trigger a re-entrant assembly load
  - Acceptance: `SVGControl/SvgRenderer.cs` contains no bare `catch` (a `catch` with no exception declaration) anywhere in the file; the resolver's catch declares `Exception ex`, calls `Trace.TraceWarning`, and is preceded by a comment explaining the `log4net` exclusion — completes AC-2 for the second swallow site

**`AssemblyResolve` sub-block.**

- [ ] [P1-T16] In `SVGControl/SvgRenderer.cs`, add `internal static string TryGetDirectoryFromCodeBase(string codeBase)` — a pure helper that converts a `file://` code-base URI to a directory path, returning `null` for a null, empty, whitespace-only, or unparsable input, and never throwing
  - Acceptance: the method exists with that exact signature, contains no `throw` statement, and returns `null` on all of null, `""`, `"   "`, and a non-URI string such as `"not a uri"`
- [ ] [P1-T17] In `SVGControl/SvgRenderer.cs`, add `internal static IReadOnlyList<string> GetProbeDirectories(string assemblyLocation, string assemblyCodeBase, string baseDirectory)` — a pure helper producing the ordered candidate-directory list: the directory of `assemblyLocation` (skipped when `assemblyLocation` is null, empty, or whitespace, which is the byte-array-load case), then `TryGetDirectoryFromCodeBase(assemblyCodeBase)`, then `baseDirectory`; with null/empty entries removed and duplicates removed case-insensitively while preserving first-occurrence order. The method must never throw for any input
  - Acceptance: the method exists with that exact signature, contains no `throw` statement, and for input `(null, null, null)` returns an empty list rather than throwing
- [ ] [P1-T18] In `SVGControl/SvgRenderer.cs`, add strategy 3 to `ResolveByNameAndKey`: after the existing `Assembly.Load` attempt and inside the same re-entrance-guarded region, iterate `GetProbeDirectories(typeof(SvgRenderer).Assembly.Location, typeof(SvgRenderer).Assembly.CodeBase, AppDomain.CurrentDomain.BaseDirectory)`, and for each candidate directory probe for `<requested.Name>.dll`; load the first existing hit with `Assembly.LoadFrom`, return it only when `PublicKeyTokensEqual` confirms the loaded assembly's public key token matches the requested token, and otherwise continue. The handler must still return `null` when no candidate matches, must not throw out of the handler, and must preserve the existing `_resolving` re-entrance guard and strategy ordering (loaded-assembly scan first, then `Assembly.Load`, then directory probing)
  - Acceptance: `ResolveByNameAndKey` contains an `Assembly.LoadFrom` call reached only after the loaded-assembly scan and the `Assembly.Load` attempt; the `PublicKeyTokensEqual` check is applied to the `LoadFrom` result before it is returned; the `_resolving.Add`/`_resolving.Remove` guard still encloses strategies 2 and 3; the method's final statement is still `return null;` — satisfies AC-8

**Coverage sub-block.**

- [ ] [P1-T19] Verify `SVGControl/SvgRenderer.cs` is at most 500 lines after all production edits; if it exceeds 500, tighten the added code (for example by collapsing duplicated logging into one private helper) until it does
  - Acceptance: `evidence/qa-gates/svgrenderer-file-size.2026-08-04T14-36.md` created containing `Timestamp:`, `Command: (Get-Content SVGControl/SvgRenderer.cs).Count`, `EXIT_CODE: 0`, and `Output Summary:` recording a line count `<= 500` — enforces the 500-line limit in `.claude/rules/general-code-change.md`. This measurement is taken before formatting; task P2-T2 re-records it after formatting
- [ ] [P1-T20] Extend `SVGControl.Test/SvgRendererParseContractTests.cs` with the remaining parse-path coverage: the success path (`SvgRenderer.GetSvgDocument(SVGControl.Defaults.GetDefault.SvgImage)` returns non-null), the argument-boundary paths (`GetSvgDocument(null)` and `TryGetSvgDocument(null, out _, out _)` each throw `ArgumentNullException`), `TryGetSvgDocument` returning `false` with a non-null `error` for malformed bytes and `false` with a `null` `error` for `Array.Empty<byte>()`, `GetSvgDocumentOrThrow` throwing `InvalidOperationException` for both null-producing inputs, and — using the task P1-T11 `Func<byte[], SvgDocument>` seam supplied as a `Mock<Func<byte[], SvgDocument>>().Object` configured with `Setup(...).Throws(sentinel)` — an assertion that the exception surfaced in `error` is reference-equal to the injected sentinel exception. Any `Bitmap` produced by a success-path assertion must be disposed
  - Acceptance: `SvgRendererParseContractTests.cs` contains at least nine `[TestMethod]` members total, uses `Moq` for the delegate seam and `FluentAssertions` for assertions, contains a `BeSameAs`-style identity assertion against the injected sentinel exception, and the file is at most 500 lines — contributes to AC-5
- [ ] [P1-T21] Create `SVGControl.Test/SvgRendererNullToleranceTests.cs` covering the AC-4 null-tolerant consumers and add a `<Compile Include="SvgRendererNullToleranceTests.cs" />` entry to `SVGControl.Test/SVGControl.Test.csproj`. Tests: setting `SvgRenderer.Document = null` succeeds and leaves `Document` null; `SvgRenderer.Render()` returns `null` when `Document` is null; `SvgImageSelector.SetDefaultImage()` leaves the renderer's `Document` non-null in the test host; constructing `new SvgImageSelector(size, padding, AutoSize.MaintainAspectRatio, useDefaultImage: true)` does not throw; and setting `SvgImageSelector.UseDefaultImage = false` clears the document without throwing. No temporary files, no network, no live Outlook or designer process
  - Acceptance: `SVGControl.Test/SvgRendererNullToleranceTests.cs` exists with at least five `[TestMethod]` members covering those five behaviors; `SVGControl.Test/SVGControl.Test.csproj` contains `<Compile Include="SvgRendererNullToleranceTests.cs" />`; the file is at most 500 lines — contributes to AC-4 and AC-5
- [ ] [P1-T22] Create `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs` covering the task P1-T16 and task P1-T17 pure helpers and add a `<Compile Include="SvgAssemblyProbeDirectoryTests.cs" />` entry to `SVGControl.Test/SVGControl.Test.csproj`. Tests must cover: `TryGetDirectoryFromCodeBase` for a valid `file://` URI, for `null`, for `""`, and for a non-URI string; `GetProbeDirectories` with all three inputs populated (order preserved), with an empty `assemblyLocation` (that candidate skipped, no throw), with duplicate directories differing only by case (deduplicated), and with all three inputs null (empty list, no throw). Do not write any test that asserts the `AssemblyResolve` handler is absent, because the handler is process-wide and permanently installed (research §8.5)
  - Acceptance: `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs` exists with at least eight `[TestMethod]` members covering those cases; `SVGControl.Test/SVGControl.Test.csproj` contains `<Compile Include="SvgAssemblyProbeDirectoryTests.cs" />`; the file contains no assertion referencing `AppDomain.CurrentDomain.AssemblyResolve`; the file is at most 500 lines — contributes to AC-8 and AC-5

**Verification and check-off sub-block.**

- [ ] [P1-T23] Rebuild `SVGControl.Test` with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath SVGControl.Test/SVGControl.Test.csproj -Configuration Debug -Platform AnyCPU`, then run the full suite with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot SVGControl.Test -Configuration Debug` and confirm every test passes, including the four task P1-T8 regression tests that failed in task P1-T9
  - Acceptance: `evidence/regression-testing/ac1-pass-after.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (both commands above), the build `EXIT_CODE: 0`, the test-run `EXIT_CODE: 0`, and `Output Summary:` recording total/passed/failed counts with `failed: 0`, naming the four task P1-T8 tests as passed, reporting the pre-existing `GetRelativePath_Test` and `RelativePathCoverageTests` results on a separate line, and cross-referencing `ac1-fail-before.2026-08-04T14-36.md` — satisfies AC-1 and the "tests execute under the test runner" half of AC-9
- [ ] [P1-T24] In `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`, change `- [ ]` to `- [x]` for AC-1, AC-2, AC-3, AC-4, AC-7, AC-8, AC-9, and AC-10 only, appending to each a short evidence pointer naming the artifact that proves it. The AC-4 pointer must state explicitly that `SvgRenderer` is `internal class` (`SVGControl/SvgRenderer.cs:18`) and that the new `public static` members form an assembly-internal surface reachable only from `SVGControl` and, via `InternalsVisibleTo`, from `SVGControl.Test` — this is deliberate, not a defect, and the pointer must not imply a cross-assembly public surface. Leave AC-5, AC-6, and AC-11 unchecked at this point
  - Acceptance: `issue.md` shows `- [x] **AC-1`, `- [x] **AC-2`, `- [x] **AC-3`, `- [x] **AC-4` (with the assembly-internal-surface statement in its evidence pointer), `- [x] **AC-7`, `- [x] **AC-8`, `- [x] **AC-9`, `- [x] **AC-10`, and still shows `- [ ] **AC-5`, `- [ ] **AC-6`, and `- [ ] **AC-11`

### Phase 2 — Final QC Loop

All command tasks in this phase are unconditional. Each states an exact command that must be executed and recorded. `EXIT_CODE: SKIPPED` is not a valid outcome for any task in this phase.

- [ ] [P2-T1] Run `dotnet tool run csharpier format .` from the repository root, covering `SVGControl/SvgRenderer.cs` and the new test files under `SVGControl.Test/`
  - Acceptance: `evidence/qa-gates/csharpier-format.2026-08-04T14-36.md` created containing `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, and `Output Summary:` recording the number of files reformatted
- [ ] [P2-T2] Run `dotnet tool run csharpier check .` from the repository root and confirm zero formatting drift remains in `SVGControl/SvgRenderer.cs` or under `SVGControl.Test/`
  - Acceptance: `evidence/qa-gates/csharpier-check.2026-08-04T14-36.md` created containing `Timestamp:`, `Command: dotnet tool run csharpier check .`, `EXIT_CODE: 0`, and `Output Summary: 0 files need formatting`, and re-record `(Get-Content SVGControl/SvgRenderer.cs).Count <= 500` after formatting
- [ ] [P2-T3] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"` from the repository root
  - Acceptance: `evidence/qa-gates/restore.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE: 0`, and `Output Summary:` confirming restore completed with no missing-package error for `SVGControl.Test`
- [ ] [P2-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` from the repository root
  - Acceptance: `evidence/qa-gates/analyzer-build.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE: 0`, and `Output Summary:` recording zero analyzer errors and a diagnostic count no worse than the task P0-T7 baseline
- [ ] [P2-T5] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` from the repository root
  - Acceptance: `evidence/qa-gates/nullable-build.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE: 0`, and `Output Summary: 0 errors`
- [ ] [P2-T6] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` from the repository root and read the numeric coverage values from `coverage/coverage.cobertura.xml`
  - Acceptance: `evidence/qa-gates/test-coverage.2026-08-04T14-36.md` created containing `Timestamp:`, `Command:` (the exact command above), `EXIT_CODE: 0`, and `Output Summary:` recording total/passed/failed/skipped test counts with `failed: 0`, the numeric repository-wide `line-rate` and `branch-rate` as percentages, and the numeric line coverage for the `SVGControl` package element. Placeholder values such as `UNVERIFIED` are not acceptable
- [ ] [P2-T7] Confirm a single consecutive clean toolchain pass and record it in `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/qa-gates/toolchain-clean-pass.2026-08-04T14-36.md`: if any of tasks P2-T1 through P2-T6 reported a non-zero exit code, or if task P2-T1 reformatted one or more files, rerun tasks P2-T1 through P2-T6 in order until one consecutive pass completes in which task P2-T1 reformats zero files and tasks P2-T2 through P2-T6 all report `EXIT_CODE: 0`
  - Acceptance: `evidence/qa-gates/toolchain-clean-pass.2026-08-04T14-36.md` created containing `Timestamp:`, a `Pass number:` line, and a six-row table listing each of the six commands with its `EXIT_CODE: 0` from that single final pass, plus a `Files reformatted in final pass: 0` line — satisfies AC-6
- [ ] [P2-T8] Produce the coverage comparison: report the baseline repository-wide line and branch coverage from `evidence/baseline/test-coverage.2026-08-04T14-36.md`, the post-change values from `evidence/qa-gates/test-coverage.2026-08-04T14-36.md`, the per-member coverage for every member added or changed in `SVGControl/SvgRenderer.cs` (`OpenFromBytes`, both `TryGetSvgDocument` overloads, `GetSvgDocumentOrThrow`, `GetSvgDocument`, both byte-array constructors, `TryGetDirectoryFromCodeBase`, `GetProbeDirectories`, `ResolveByNameAndKey`), an explicit repo-wide floor verdict against `>= 85%` line / `>= 75%` branch, and an explicit note that the denominator changed because `SVGControl.Test` entered the solution and the run for the first time, pulling previously-unmeasured `SVGControl` production code (`SvgImageSelector`, `PictureBoxSVG`, `ButtonSVG`, `SVGParser`, `ToggleSwitch`, `DropDownEditor`, and the converters) into the measured set. Decision rule: if the repo-wide rate falls below the floor solely because `SVGControl` production code entered the measured set for the first time, record it as an explained denominator-change regression with the before/after package-level numbers and report `COVERAGE_DENOMINATOR_CHANGE` to the orchestrator; do not attempt to raise repo-wide coverage inside this minor-audit change
  - Acceptance: `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` created containing `Timestamp:`, `Baseline line/branch coverage:`, `Post-change line/branch coverage:`, a per-member table with a numeric percentage for each of the nine named members, a `New/changed member minimum: >= 90%` verdict line, a `No regression on changed lines: yes/no` verdict line, a `Repo-wide floor verdict:` line stating pass or explained-denominator-change against `>= 85%` line and `>= 75%` branch with the before/after per-package numbers for `SVGControl`, and a `Denominator change note:` paragraph. If any new or changed member is below 90%, the task is not complete and additional tests must be added and task P2-T6 rerun. If the repo-wide floor verdict is an explained denominator change, the task completes with `COVERAGE_DENOMINATOR_CHANGE` reported to the orchestrator and no further coverage work inside this plan
- [ ] [P2-T9] In `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`, change `- [ ]` to `- [x]` for AC-5 and AC-6 only, appending to each a short evidence pointer naming the artifact that proves it
  - Acceptance: `issue.md` shows `- [x] **AC-5` (pointing at `coverage-delta.2026-08-04T14-36.md`) and `- [x] **AC-6` (pointing at `toolchain-clean-pass.2026-08-04T14-36.md`), and still shows `- [ ] **AC-11`
- [ ] [P2-T10] Record the AC-11 human handoff. AC-11 is satisfied only by a human executing `runbooks/verify-winforms-designer-load.runbook.md`; the executor must not check it off and must not attempt to automate it
  - Acceptance: `evidence/other/ac11-runbook-handoff.2026-08-04T14-36.md` created containing `Timestamp:`, `Runbook: docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md`, `Owner: human operator`, `Cue: after AC-6 toolchain-clean-pass is recorded and before the feature is reported done`, `Expected evidence path: docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`, and `AC-11 state: unchecked pending human execution`; and `issue.md` still shows `- [ ] **AC-11`
- [ ] [P2-T11] Record the final plan-completion summary in `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/other/plan-completion-summary.2026-08-04T14-36.md`, reconciling checkbox state on disk against evidence
  - Acceptance: `evidence/other/plan-completion-summary.2026-08-04T14-36.md` created listing each of AC-1 through AC-11 with its final checkbox state in `issue.md` and the evidence artifact path that supports it, and stating explicitly that AC-11 is intentionally unchecked pending the human runbook; and every `- [ ]` task in this plan file that was completed has been changed to `- [x]`

## Test Plan

- **Unit (C#, MSTest + Moq + FluentAssertions, in `SVGControl.Test`):**
  - `SvgRendererParseContractTests.cs` — the four AC-1 regression tests (malformed bytes and `Array.Empty<byte>()` against both byte-array constructors), the success path against `Defaults.GetDefault.SvgImage`, argument-boundary `ArgumentNullException` cases, `TryGetSvgDocument` true/false outcomes with and without a captured exception, `GetSvgDocumentOrThrow` inner-exception behavior, and the seam-injected sentinel-exception identity assertion.
  - `SvgRendererNullToleranceTests.cs` — the AC-4 null-tolerant consumer contracts (`Document` setter, `Render()`, `SvgImageSelector.SetDefaultImage`, the default-image constructor, `UseDefaultImage`).
  - `SvgAssemblyProbeDirectoryTests.cs` — the AC-8 pure probe-directory helpers, including empty `Location`, unparsable code base, case-insensitive deduplication, and the all-null no-throw case.
- **Compile prerequisite:** all three files name `SvgDocument`, so they require the direct `Svg` reference added by task P1-T4; without it the assembly fails to compile with `CS0012`.
- **Pre-existing tests in the same assembly:** `GetRelativePath_Test` and `RelativePathCoverageTests` already exist in `SVGControl.Test` and run in every `Invoke-MSTest.ps1 -SearchRoot SVGControl.Test` invocation. Their results are reported separately from the new tests in tasks P1-T9 and P1-T23.
- **Determinism constraints:** no temporary files (UT4, zero approved exceptions), no network, no live Outlook, no designer process, no `Thread.Sleep`/`Task.Delay`. Parse failure is produced purely from in-memory input; the delegate seam is used only where exact exception identity must be asserted. No test asserts the absence of the process-wide `AssemblyResolve` handler.
- **Integration:** none automatable. The designer-host path is covered by the human runbook (AC-11).
- **Coverage evidence:** baseline `evidence/baseline/test-coverage.2026-08-04T14-36.md`; post-change `evidence/qa-gates/test-coverage.2026-08-04T14-36.md`; comparison `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md`. New and changed members must reach `>= 90%`; changed lines must not regress; the repo-wide floor verdict follows the task P2-T8 denominator-change decision rule.

## Evidence Artifact Index

All under `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/`:

- `baseline/toolchain-bootstrap.2026-08-04T14-36.md`
- `baseline/phase0-instructions-read.md`
- `baseline/restore.2026-08-04T14-36.md`
- `baseline/csharpier-check.2026-08-04T14-36.md`
- `baseline/analyzer-build.2026-08-04T14-36.md`
- `baseline/nullable-build.2026-08-04T14-36.md`
- `baseline/test-coverage.2026-08-04T14-36.md`
- `baseline/svgcontrol-test-buildability.2026-08-04T14-36.md`
- `regression-testing/ac1-fail-before.2026-08-04T14-36.md`
- `regression-testing/ac1-pass-after.2026-08-04T14-36.md`
- `regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md` (human-produced, AC-11)
- `qa-gates/svgcontrol-test-build.2026-08-04T14-36.md`
- `qa-gates/prereq-analyzer-build.2026-08-04T14-36.md`
- `qa-gates/prereq-nullable-build.2026-08-04T14-36.md`
- `qa-gates/svgrenderer-file-size.2026-08-04T14-36.md`
- `qa-gates/csharpier-format.2026-08-04T14-36.md`
- `qa-gates/csharpier-check.2026-08-04T14-36.md`
- `qa-gates/restore.2026-08-04T14-36.md`
- `qa-gates/analyzer-build.2026-08-04T14-36.md`
- `qa-gates/nullable-build.2026-08-04T14-36.md`
- `qa-gates/test-coverage.2026-08-04T14-36.md`
- `qa-gates/toolchain-clean-pass.2026-08-04T14-36.md`
- `qa-gates/coverage-delta.2026-08-04T14-36.md`
- `other/package-restore-decision.2026-08-04T14-36.md`
- `other/ac11-runbook-handoff.2026-08-04T14-36.md`
- `other/plan-completion-summary.2026-08-04T14-36.md`

## Open Questions / Notes

- **U-2 (research §9.3) remains open by design.** Whether `ExCSS.dll` is present in Visual Studio's `ProjectAssemblies` shadow-copy directory alongside `SVGControl.dll` determines whether the AC-8 directory probe can succeed in the designer host. Step 10 of the runbook captures this observation. AC-3's degrade-and-log behavior is host-independent and is the primary deliverable regardless of the U-2 answer.
- **`LoadFrom` context divergence** is a known and accepted risk of AC-8 (research §4.4). It is mitigated by preserving strategy 1 (return an already-loaded match first), which this plan requires unchanged in task P1-T18.
- **Coverage format for the downstream reduced audit.** `scripts/.../validate-feature-review-coverage.ps1` reads `artifacts/csharp/coverage.xml` in JaCoCo format, while this plan's toolchain emits Cobertura at `coverage/coverage.cobertura.xml`. If the reduced audit requires the JaCoCo artifact, that conversion is a separate audit-stage step and is not part of this plan's scope.
- **Repo-wide coverage floor and the denominator change.** Bringing `SVGControl.Test.dll` into the measured set adds `SVGControl` production code that was previously unmeasured. A resulting repo-wide drop is a denominator artifact, not a regression caused by this change; task P2-T8 defines the explicit decision rule and the `COVERAGE_DENOMINATOR_CHANGE` report path.
