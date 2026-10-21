# Policy Audit — breadcrumb-bridge-keyboard-navigation-defects (Issue #737)

- Timestamp: 2026-09-03T02-30
- Feature folder: `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/`
- Branch: `bug/breadcrumb-bridge-keyboard-navigation-defects-737`
- HEAD: `69619556` (merge commit reconciling this branch against `origin/main` at `b13d5b7b`, unpushed)
- Base branch (resolved): `origin/main` at `b13d5b7b` — confirmed via `git merge-base origin/main HEAD` == `b13d5b7b`, i.e. `origin/main` IS the merge-base, so the three-dot diff below is current and not stale.
- Work Mode: `full-bug` (from `issue.md` line 12) — AC source is `spec.md` only, per `acceptance-criteria-tracking`.

## Template Resolution Deviation

Per `policy-audit-template-usage`, the canonical source for this template is the MCP tool `mcp__drm-copilot__resolve_policy_audit_template_asset`. That MCP tool is not present in this session's available tool set (no `mcp__*` tools were exposed). Per the skill's fallback instruction ("If MCP asset resolution fails, create a minimal policy audit artifact marked BLOCKED and document the missing template resolution"), this artifact is hand-authored preserving all twelve canonical major headings from the skill's `Required Steps` §5 list. The final validation step (`mcp__drm-copilot__validate_orchestration_artifacts`) could likewise not be run for the same reason. This is a tooling-availability deviation, not a scope narrowing, and is recorded here per the skill's own fallback path.

## Rejected Scope Narrowing

No caller-supplied scope narrowing was present in the task prompt. The task explicitly instructed computing the full three-dot diff against `origin/main` and flagging anything outside the Write Set plus feature-folder docs as a blocking scope violation — this is the Scope Invariant's own default posture, not a narrowing to reject. No `## Rejected Scope Narrowing` entries apply.

## Evidence Location Compliance

All evidence produced by the atomic-executor lives under the canonical `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/{baseline,qa-gates}/` paths — 9 baseline artifacts and 15 QA-gate artifacts (24 total, all timestamped `2026-09-02T00-00`), all confirmed present via `git diff --name-only origin/main...HEAD`. No file under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` appears anywhere in the branch diff (`git diff --name-only origin/main...HEAD` contains no `artifacts/` path at all). `validate_evidence_locations.py` was not run (not present in the discovered `.claude/lib`/`scripts` tree for this repo layout); the manual diff scan above is the substitute evidence. **PASS** — no evidence-location violations found.

## Full Branch Diff (Scope Invariant)

Command run: `git diff --name-only origin/main...HEAD` (three-dot, against `origin/main` = the resolved merge-base).

Result — 34 changed paths, all inside the Write Set or this feature's own folder:
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` (Write Set)
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` (Write Set)
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs` (Write Set)
- `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/issue.md`
- `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`
- `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/plan.2026-09-02T00-00.md`
- `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/research/2026-09-02T14-20-breadcrumb-bridge-keyboard-defects-research.md`
- 24 files under `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/{baseline,qa-gates}/`

No file outside the Write Set + this feature's own docs/evidence appears in the diff. The task's noted second merge (bringing a root `Directory.Build.props` with `RxUseUnsupportedPackagesConfig=true` and updates to `scripts/vscode/Invoke-MSTest*.ps1`) was independently verified: `git diff --name-only origin/main...HEAD -- Directory.Build.props scripts/vscode/` returns empty, and `git log --oneline -- Directory.Build.props scripts/vscode/Invoke-MSTestWithCoverage.ps1` shows those files were last touched by already-merged PRs #730/#733 on `origin/main` itself, not by this branch. **PASS** — no scope violation.

**Verdict: PASS.** 0 files outside the permitted footprint.

## Coverage Verification

Languages with changed files in this branch diff: **C# only** (`.cs` production file + 2 `.cs` test files). No TypeScript, Python, or PowerShell production files changed.

| Language | Changed files? | Artifact | Verdict |
|---|---|---|---|
| C# | Yes | `evidence/qa-gates/qa-coverage-fullrepo.2026-09-02T00-00.md` (Cobertura, `coverage/breadcrumb-737-final.cobertura.xml`) | **PASS** |
| TypeScript | No | n/a | N/A (zero changed files) |
| Python | No | n/a | N/A (zero changed files) |
| PowerShell | No | n/a | N/A (zero changed files) |

C# evidence detail, independently re-verified by reading the coverage XML directly (`coverage/breadcrumb-737-final.cobertura.xml`, present on disk, gitignored):
- `line-rate="0.853867"` (85.3867%), `lines-covered="55141"`, `lines-valid="64578"` — read directly from the `<coverage>` root element, matches the executor's reported figures exactly.
- `branch-rate="0.794649"` (79.4649%), `branches-covered="13188"`, `branches-valid="16596"` — read directly; the executor's evidence artifact did not report branch-rate, but it is present in the same artifact and clears the >= 75% uniform floor per `.claude/rules/quality-tiers.md` / `general-unit-test.md`.
- Repo-wide line coverage 85.39% clears the >= 85% uniform floor (`.claude/rules/general-unit-test.md`) with a ~0.39-point margin. Note: CLAUDE.md's own UT2 section states an 80%/90% floor pair, which is superseded in this repo by the newer uniform 85%/75% floor in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` per the Policy Compliance Order (rule files layer on top of / supersede CLAUDE.md's older figures for this exact metric — a known, previously-recorded unreconciled conflict in this repo). Both floors are cleared regardless of which governs, so the conflict is immaterial to this branch's verdict but is recorded for completeness.
- New/modified-file coverage: `BreadcrumbDocumentAssets.cs` (the sole modified production file) emits **no `<class>` element at all** in the Cobertura XML — independently confirmed by grepping the XML for `BreadcrumbDocumentAssets` (zero matches) and for the `UtilitiesCS\OutlookObjects\Folder\` filename prefix (class list present for 20+ sibling files, `BreadcrumbDocumentAssets.cs` absent from all of them). This is consistent with the executor's stated basis: the file's only content is a `public static class` holding `public const string` fields built from string-literal concatenation, which the C# compiler folds into constant metadata with no emitted IL and therefore no coverable line — `lines-valid` is bit-for-bit identical between the same-session baseline (64578) and final (64578) run, corroborating this claim independently rather than merely repeating it.
- The two Write Set test files are correctly excluded from the coverage denominator (standard test-file exclusion per `general-unit-test.md`).
- Same-session baseline vs. final comparison (not a stale cross-session repo-wide constant, per this repo's own established pitfall): `lines-covered` rose 55139 -> 55141 (+2, favorable), `lines-valid` unchanged (64578 -> 64578). No regression on any changed line.

**Coverage verdict: PASS** for C#, the only language with changed files.

## 1. General Unit Test Policy Compliance

- **Independence/Isolation/Fast/Deterministic**: PASS. Both changed tests are ordinary MSTest methods with no shared mutable state, no timing dependency, no I/O. The new `Issue737BridgeJsPostsRowSelectedOnEnterAndScrollsSelectedRowIntoView` test reads a `public const string` in-process; the modified `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` uses the file's existing `PopulatedRouterAsync`/`ProviderMock`/`ArrowAsync` helpers unchanged.
- **Readability**: PASS. New test has a descriptive name and an XML-doc `<summary>` explaining intent, scope, and the JS-execution-harness limitation, following the established `Issue439...` precedent in the same file.
- **AAA structure**: PASS for both. New test: `// Act` / `// Assert` (Arrange is trivial — reading a constant, no setup needed). Modified test: `// Arrange` / `// Act` / `// Assert` preserved, with two additional Arrange-phase captures now asserted.
- **External dependencies / mocks**: PASS. No new mocks required; the router test reuses the file's existing `Mock<IFolderHierarchyProvider>`-based `ProviderMock()` factory, unmodified.
- **No temporary files**: PASS. Neither test touches the filesystem.
- **Coverage — no regression on changed lines**: PASS (see Coverage Verification above).
- **Test file location**: PASS. Both files already live under `UtilitiesCS.Test/OutlookObjects/Folder/`, mirroring the production `UtilitiesCS/OutlookObjects/Folder/` tree; no colocation violation.
- **File size (500-line limit)**: PASS. `FolderBreadcrumbBridgeRouterTests.cs` is 495 lines (verified independently via `awk 'END{print NR}'`), 5 lines under the limit. `BreadcrumbHtmlRendererTests.cs` is 356 lines. `BreadcrumbDocumentAssets.cs` is 147 lines.
- **Scenario completeness**: PASS with a documented, spec-accepted limitation. The new JS-content test is string-containment only (positive-flow assertion of literal presence); this is an explicit, repo-precedented design choice recorded in spec.md's Test Strategy section ("no JS-execution test harness... this is an accepted, repo-conventional limitation") and mirrored in the test's own XML-doc comment. Not a policy gap.

## 2. General Code Change Policy Compliance

- **Design principles (simplicity, reusability, extensibility, separation of concerns)**: PASS with one minor observation (see code-review artifact — duplicated `.rowwrap.selected` lookup pattern between the Enter branch and the pre-existing arrow-key branch; this is a spec-mandated mirror of existing code, not a new defect).
- **File size limit**: PASS (see above).
- **Error handling / logging**: N/A for this change — the JS additions have no error path of their own; the C#-side consumer (`BreadcrumbBridgeRouter.ProcessInboundAsync`, unmodified, read as context) already fails safe (logs and returns) on an unresolvable `rowId`, which independently covers the Enter-with-nothing-selected edge case (empty `id` -> `FindRow('')` returns `null` -> logged and dropped, no exception). Verified by reading `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` lines 233-241.
- **Naming**: PASS. New test method name and JS variable names are descriptive and consistent with existing conventions in the file.
- **Public APIs / compatibility**: PASS. No public API changed; `BridgeJs` remains a `public const string`, unchanged in signature.
- **Dependencies**: PASS. No new dependency introduced.
- **I/O boundaries**: PASS. No I/O introduced.

## 3. Language-Specific Code Change Policy Compliance (C#)

- **Formatting (CSharpier)**: PASS. `evidence/qa-gates/qa-csharpier-pre-format-hashes...md` records pre-format SHA-256 hashes for all 3 Write Set files; `qa-csharpier-format...md` shows `dotnet tool run csharpier format` on those 3 files (exit 0) produced identical post-format hashes (no rewrite needed); `qa-csharpier-check...md` shows a full-repo `dotnet tool run csharpier check .` (exit 0, "Checked 1571 files", 0 unformatted). Independently re-verified: current on-disk SHA-256 hashes of all 3 Write Set files match the evidence-recorded hashes exactly (re-computed via `Get-FileHash -Algorithm SHA256` in this review).
- **Linting / analyzers**: PASS. `evidence/qa-gates/qa-analyzer-rebuild...md` records `pwsh -File scripts\vscode\Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild` (exit 0, "5 Warning(s)", "0 Error(s)", matching the pre-existing System.Reactive packages.config baseline warning count — no new diagnostics). The wrapper script was inspected directly (`scripts/vscode/Invoke-VSBuild.ps1`) and confirmed to construct the exact `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform="Any CPU" ...` invocation mandated by CLAUDE.md C#1 item 2, using `/t:Rebuild` (not `/t:Build`) by default for `-Target Rebuild`.
- **Type checking / nullable**: PASS. `evidence/qa-gates/qa-nullable-rebuild...md` records the same wrapper with `-TreatWarningsAsErrors` (exit 0, "5 Warning(s)", "0 Error(s)"), matching baseline — no CS86xx nullable diagnostics promoted. `BreadcrumbDocumentAssets.cs` already carries `#nullable enable` (file line 1, pre-existing, unchanged by this feature).
- **`/t:Build` vs `/t:Rebuild` trap**: PASS. All evidence-recorded analyzer/nullable runs use `-Target Rebuild`, avoiding the documented up-to-date-check false-pass trap.
- **`/p:Nullable=enable` trap**: PASS. Not used anywhere in this feature's evidence; the wrapper script's `-EnableNullable` switch is explicitly documented as deprecated/no-op (confirmed by reading the script directly).

## 4. Language-Specific Unit Test Policy Compliance (C#)

- **MSTest**: PASS. Both changed test methods use `[TestMethod]` in a `[TestClass]`-decorated file (pre-existing, unmodified attribute), no xUnit/NUnit introduced.
- **FluentAssertions**: PASS. New test uses `.Should().Contain(...)` throughout. Modified test uses `.Should().ContainSingle()` and `.Should().BeOfType<RenderMessage>()`, matching the file's established pattern (identical shape to the pre-existing `Route_ThemeChange_EchoesThemeAndReRenders` assertion at line 408, confirmed by direct read).
- **Moq**: N/A for the new test (no mocking needed); the modified router test reuses the file's pre-existing `Mock<IFolderHierarchyProvider>`-based factories, unmodified.

## 5. Test Coverage Detail

See "Coverage Verification" above for the full breakdown. Summary: repo-wide C# line coverage 85.3867% (>= 85% floor), branch coverage 79.4649% (>= 75% floor); the sole modified production file (`BreadcrumbDocumentAssets.cs`) has zero coverable lines (compile-time constant only, independently confirmed absent from the Cobertura class list); both test files are correctly excluded from the coverage denominator; no regression on any changed line.

## 6. Test Execution Metrics

- Scoped run covering every touched/added test (`FolderBreadcrumbBridgeRouterTests` + `BreadcrumbHtmlRendererTests` FQN filter): Total 41, Passed 41, Failed 0 (`evidence/qa-gates/qa-vstest-scoped...md`) — matches the expected 40-baseline + 1 new test.
- Isolated run of the new test: 1 passed (`qa-vstest-phase3-new-test...md`).
- Isolated run of the modified test: 1 passed (`qa-vstest-phase4-modified-test...md`).
- Isolated run of the #440 sibling regression test (`ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition`): 1 passed (`qa-vstest-phase4-sibling-test...md`) — confirms Finding 3's fix is consistent with the pre-existing #440 contract.
- Full-repo coverage run: 6956 total, 6956 passed (6955 baseline + 1 new) (`qa-coverage-fullrepo...md`). All exit codes 0.

## 7. Code Quality Checks

See `code-review.2026-09-03T02-30.md` for the full write-up. Summary verdict: no blocking findings; one minor DRY observation (duplicated `.rowwrap.selected`/`data-row-id` lookup pattern across the Enter and arrow-key branches) is a spec-mandated mirror of pre-existing code, not a new defect, and is not blocking.

## 8. Gaps and Exceptions

- The `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__validate_orchestration_artifacts` MCP tools were unavailable in this review session; this artifact was hand-authored preserving the canonical heading set as the documented fallback (see "Template Resolution Deviation" above).
- CLAUDE.md's UT2 section states an 80%/90% coverage floor pair that is superseded by the newer uniform 85%/75% floor in `.claude/rules/general-unit-test.md`/`quality-tiers.md`; both floors are cleared by this branch's evidence, so the unreconciled conflict is immaterial here but is recorded per repo history of prior audits flagging it.
- `quality-tiers.yml` does not exist at the repo root in this worktree, so no per-project tier classification could be read; the uniform coverage floors (which apply identically across all tiers per Authoritative Decision #2) were used directly, so this absence does not affect the verdict.

## 9. Summary of Changes

Three files changed, all inside the declared Write Set:
1. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` — 9 lines added (7 for the Enter-key `keydown` branch, 2 for the post-render `scrollIntoView` call), 0 removed.
2. `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` — 8 lines added/2 modified: captures and asserts the two previously-discarded `ArrowAsync` results in `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft`.
3. `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs` — 20 lines added: one new `[TestMethod]` asserting the JS content added to file 1.

## 10. Compliance Verdict

**PASS.** No blocking findings. All four policies (general code-change, general unit-test, C# code-change, C# unit-test) are satisfied by direct, independently-verified evidence. Scope is exactly the declared Write Set plus this feature's own docs/evidence. Coverage clears both the legacy CLAUDE.md floor and the newer uniform `.claude/rules` floor for the only language with changed files (C#).

## Appendix A: Test Inventory

| Test | File | Status | Type |
|---|---|---|---|
| `Issue737BridgeJsPostsRowSelectedOnEnterAndScrollsSelectedRowIntoView` | `BreadcrumbHtmlRendererTests.cs` | New | String-containment (JS content) |
| `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` | `FolderBreadcrumbBridgeRouterTests.cs` | Modified (assertion-only) | Router unit test |
| `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition` | `FolderBreadcrumbBridgeRouterTests.cs` | Unchanged (used as sibling-regression confirmation) | Router unit test |

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier format .` (scoped to the 3 Write Set files; verified with `dotnet tool run csharpier check .` repo-wide)
2. `pwsh -File scripts\vscode\Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild` (constructs `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`)
3. `pwsh -File scripts\vscode\Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors` (constructs `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`)
4. `vstest.console.exe <UtilitiesCS.Test.dll> /TestCaseFilter:... /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings /Logger:trx ...` (per-test and scoped runs)
5. `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\breadcrumb-737-final.cobertura.xml` (full-repo Cobertura coverage capture)
