# 2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling (Spec)

- **Issue:** #873
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12T11-30
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug — this file is the single acceptance-criteria source. `user-story.md` in this folder is narrative only and carries no acceptance criteria.

## Context

This item implements the committed test-evidence convention the maintainer recorded on #671 on 2026-09-11, and removes the identity leaks that live in configuration rather than in historical evidence (#728, the tooling half of #602).

Today the two editor test entry points under the editor script directory commit raw evidence: a raw test-result document that carries the operator account name, the host name and the absolute checkout path in six attributes, and a raw Cobertura document measured at tens of megabytes. After this change the coverage entry point writes a package-level JaCoCo projection plus the existing one-line first-party summary, both test entry points write a test-result summary derived from the raw test-result document, and each raw document is discarded under a stated condition. Both argument builders set an explicit results directory and an explicit log file name so the default test-result filename — which embeds the account name, the host name and a timestamp — is never produced.

The same delivery corrects the publish-destination leak in `TaskMaster/TaskMaster.csproj`, the symbols-directory leak in `.vscode/settings.json`, and the leaks in the five agent-memory files named in the Write Set.

All findings cited as R1 through R12 come from the research artifact in this folder's research directory (2026-09-12T11-00-test-evidence-projection-research.md) and are treated as established fact.

Environment:

- OS/version: Windows 11 Pro 10.0.26200
- Language toolchain: PowerShell 7 with Pester for the script work; one project-file element; one editor settings element; Markdown. No Python is involved, and no Python toolchain exists in this repository.
- Commands exercised: the two editor test entry points named in the Write Set, each of which invokes the Visual Studio test console.
- Data source: the working tree at the head recorded in `issue.md`.

Impact / Severity:

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Repro & Evidence

Steps to Reproduce:

1. Run either editor test entry point. The Visual Studio test console writes a test-result document whose default file name is composed of the account name, the host name and a timestamp. Per R4.2 the document carries the account and host in the run element name and run-user attributes, the host in each result element, the account and host inside a deployment root directory name, and the absolute checkout path twice — lowercased in one attribute and original-cased in another.
2. Run the coverage entry point. It leaves a post-processed Cobertura document at the resolved output path. The #646 substitution record cited in R1.3 measured the two documents it replaced at 26,064,187 and 26,067,082 bytes, against 2,359 bytes for each replacement projection.
3. Read `TaskMaster/TaskMaster.csproj` line 37. The publish-destination element text carries an absolute user-profile path containing the account name and the employer organization name (R7 row 1).
4. Read `.vscode/settings.json` line 27. The single element of the Power Query additional-symbols array is an absolute user-profile path in forward-slash form containing the account name (R7 row 2).
5. Search the agent-memory tree for the account or host token. The five files named in the Write Set each match (R7 rows 3 through 7). R7.1 records that a broader pattern matches further files; those are not in this item's scope.
6. Read the artifact-hygiene rule text in `.claude/agent-memory/_shared_no_absolute_host_paths.md`. Per R10 there is no redaction sweep implemented as executable code anywhere in this repository; the rule text is what agents follow, and it records both the plan-file exclusion gap and the insufficiency of a zero-residual count as a gate.

Expected (after this change):

- The coverage entry point writes a package-level JaCoCo projection in the #646 shape and asserts that the projection reconciles to the source document's root attributes.
- Both entry points write a test-result summary that reports the run verdict, the counts, and the names of failed tests.
- Both argument builders pass an explicit results directory and an explicit log file name.
- No configuration file in the Write Set carries an account name, a host name, an employer organization name, or an absolute user-profile path.
- The artifact-format convention is recorded once, in a TaskMaster-owned document.

Actual (before this change):

Raw test-result and raw Cobertura documents are committed on every feature; the test-result document carries host identity; the ad-hoc angle-bracket redaction applied to those documents in the past made them unparseable (R4.3); the project file, the editor settings file and five memory files leak identifiers; the hygiene rule text does not require a plan-file scan and does not require a parse check.

## Scope & Non-Goals

In scope:

- A new projection part file under the editor script directory holding a pure projection writer, a pure reconciliation assertion, and a pure retention predicate.
- A new test-result summary part file holding a pure reader over a test-result document string and a pure formatter.
- Explicit results directory and log file name on both members of the argument-builder family derived in the research artifact's Numeric Derivation Evidence section.
- Entry-point wiring in both test entry points.
- The project-file, editor-settings, CLAUDE.md, and agent-memory corrections listed in the Write Set.
- Pester coverage for every new pure function and for both argument builders.

Out of scope, stated in plain prose with no path tokens so the change footprint is not overstated:

- The historical sweep over the already-tracked raw evidence documents across every feature folder. That is issue #602 and it must run after this item so that a fresh test run does not reintroduce the prefix. Do not delete already-tracked raw documents here.
- The further agent-memory occurrences that R7.1 found under a broader pattern. Those belong to the same repository-wide sweep item. This spec does not assert that the five named files are the complete population.
- The branch-coverage threshold work owned by a concurrent item. This delivery does not modify the coverage threshold part file or the test file that covers it, and does not add tests to that test file.
- The editor-agent settings file, the editor-agent skills tree, the editor-agent hooks tree, the blast-radius configuration and the orchestration-routing configuration. R9 establishes these are push-down owned and would be reverted on the next push-down. None is written by this delivery.
- The repository ignore file. D6 places the results directory beneath an already-ignored tree precisely so that no ignore-file change is required.
- Building a redaction sweep as executable code. R10 establishes none exists; creating one is materially larger scope and belongs to the sweep item.
- The continuous-integration workflow invocation of the test console. It runs under the runner's account and host, which is a different risk class, and the issue scopes the obligation to the editor scripts.

## Root Cause Analysis

1. No convention existed for the permitted format of committed test evidence, so each item improvised. R1.4 is the load-bearing negative finding: nothing in this repository generates the JaCoCo projection. Every committed instance was produced by a throwaway converter written to a session scratchpad outside the repository and never committed.
2. Neither argument builder passes a results directory or a log file name, and neither runsettings file sets one (R3.1, R3.3). The default file name is therefore produced by construction, at every layer, on every local run.
3. R9 explains why the convention was never written down in a durable place: the evidence-and-timestamp-conventions skill, which is where the issue first proposed putting it, is push-down owned, so an edit there is overwritten. The repository-root instruction file is TaskMaster-owned, already carries the test toolchain step the rule attaches to, and is loaded into every agent session.
4. The publish-destination property is a publish-time ClickOnce property. R8 establishes that nothing in this worktree reads it: no publish target is invoked by any workflow, task or script, so the leak survived because no gate ever exercised it.
5. R10 establishes that the hygiene rule is prose read by agents rather than code, so the plan-file exclusion and the missing parse check are defects in rule text, not in a program.

## Proposed Fix

### Design summary (what changes where)

Two new pure part files plus wiring in the two entry points, and four documentation or configuration corrections.

- `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` — new. Holds `ConvertTo-JacocoPackageProjection` (pure writer), `Assert-JacocoProjectionReconciliation` (pure assertion) and `Test-RawCoverageDocumentRetained` (pure predicate). It is dot-sourced from `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` alongside the four existing part files. It must be a new file rather than an addition to the helpers file: R5 measures 29 lines of headroom in the helpers file against the repository's 500-line ceiling, and two existing part files document the same reason in their own headers.
- `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` — new. Holds `Get-TrxRunSummary` (pure reader over a test-result document string) and `Format-TrxRunSummary` (pure formatter). Dot-sourced by both entry points.
- `scripts/vscode/Invoke-MSTest.ps1` — `Get-VsTestArgumentList` gains an explicit results directory and log file name; `Invoke-MSTestMain` resolves both, dot-sources the summary part file, writes the summary, and discards the raw test-result document.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — `Get-DotnetCoverageArgumentList` gains the same two switches on the test-console segment; `Invoke-MSTestWithCoverageMain` writes the projection, runs the reconciliation assertion, applies the conditional discard, and writes the test-result summary.
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — one added dot-source line only.
- Three existing test files are in the Write Set because the signature changes force them, not by choice. No new describe block is added to any of them; the new behaviour is asserted in the four new test files instead, so each existing file grows by the minimum the signature change forces.
  - `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` — calls `Get-VsTestArgumentList` at four sites, `Get-DotnetCoverageArgumentList` at five sites and `Invoke-DotnetCoverageCollection` at five sites. Adding two mandatory parameters makes every one of those calls fail parameter binding, and the array assertions would change even with optional parameters because the returned arrays gain two elements. This file is already 496 lines against the 500-line ceiling, so adding two arguments at each affected call site in the existing line-continuation style would breach the ceiling. The call sites are therefore converted to splatting from hashtables declared in the containing setup block, which is net line-reducing.
  - `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` — pins the complete argument array handed to the plain wrapper seam as an exact four-element comparison. That array becomes six elements once the plain builder gains the two switches, so the assertion must be updated or it fails.
  - `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` — declares an explicit five-parameter mock body for `Invoke-DotnetCoverageCollection`, which stops binding once that function gains parameters.
- `TaskMaster/TaskMaster.csproj`, `.vscode/settings.json`, `CLAUDE.md`, `.claude/agent-memory/_shared_no_absolute_host_paths.md` and the five named memory files — corrections described below.

### Boundaries and invariants to preserve

1. **Single counting rule.** The projection must not re-derive the de-duplication rule. R2 establishes that `Get-CoberturaClassLineSummary` is the single implementation and that `Get-CoberturaPackageLineSummary` already returns the four per-package numbers. The projection writer calls the per-package helper and performs exactly one new arithmetic step: JaCoCo `missed` equals valid minus covered, for lines and for branches independently.
2. **Projection source is the post-processed document.** The projection is built from the string the post-processor returns, not from raw collector output. R2 records the consequence: because the post-processor writes the root line attributes from the same de-duplicated rule the per-package helper uses, a projection built from the post-processed document reconciles to that document's root attributes by construction. A raw collector document would not reconcile, because its root is written under the collector's own class-level-only rule. The reconciliation assertion is therefore exact and is a required step, not advisory.
3. **Projection package set is inherited, not re-decided.** The projection emits one package element for every package present in the post-processed document. That document is already first-party only: the post-processor removes non-allowlisted packages, and the allowlist function drops any assembly name ending in the test suffix. Inheriting that decision avoids introducing a second, divergent allowlist. The fifteen-package set in the committed #646 instance is not reproduced here; that instance was converted from a raw document.
4. **Conditional discard of the coverage document — load-bearing.** The repository coverage directory is ignored by the repository ignore file, whose own comment records that the directory is read by the editor coverage extension, and the editor settings file points that extension at it. Deleting the document there would be a functional regression for the extension. Invariant: when the resolved coverage output directory is the repository coverage directory, the document is kept; when it is any other directory, the document is discarded, and only after the threshold assertion, the projection write and the reconciliation assertion have all completed.
5. **Results directory placement.** The results directory resolves beneath the already-ignored repository coverage directory. The ignore file has no entry matching test-result files, so a results directory anywhere else would produce a committable artifact; placing it beneath the ignored tree means a raw test-result document cannot be committed even if a discard step fails, and it requires no ignore-file change.
6. **Existing error wording is reused, not duplicated.** A missing packages node throws the wording the first-party helper already throws. A second wording for the same condition is not introduced.
7. **Namespace handling.** The test-result document root declares a default namespace (R4.4), so an unprefixed XPath selects nothing. The reader uses a namespace manager or a local-name predicate. This is mandatory, not stylistic.
8. **Angle-bracket placeholders never enter an XML-family file unescaped.** R4.3 records that a raw left angle bracket in an attribute value made 19 committed test-result documents unparseable, and that the rule mandated in a later plan produced six more. The corrections in this delivery place placeholders only in Markdown inline-code spans and in a JSON string, never in XML markup.

### Dependencies or blocked work

- Issue #602's historical sweep must be sequenced after this item.
- The obligation to have the atomic-plan contract cite the new convention cannot be delivered here, because that contract file is push-down owned per R9. It is recorded as an upstream follow-up in the Rollout section.

### Implementation strategy (what changes, not sequencing)

#### Files to change

See the Write Set section. It is the authoritative list.

#### Functions and commands impacted

New:

- `ConvertTo-JacocoPackageProjection` — parameter `-XmlDocument` of type `[xml]`, the post-processed Cobertura document. Returns the projection as a single string. Iterates `/coverage/packages/package` in document order, calls `Get-CoberturaPackageLineSummary` per package, and serialises the D1 shape.
- `Assert-JacocoProjectionReconciliation` — parameters `-XmlDocument` and `-ProjectionXml`. Sums the LINE counters across the projection's package elements and throws when covered does not equal the source root covered-lines attribute or when missed plus covered does not equal the source root valid-lines attribute.
- `Test-RawCoverageDocumentRetained` — parameters `-OutputPath` and `-RepoRoot`. Returns true when the parent directory of the output path is the repository coverage directory, false otherwise. Pure, so the discard decision is testable without touching the filesystem.
- `Get-TrxRunSummary` — parameter `-TrxContent` of type `[string]`. Returns an object carrying the run verdict from the result-summary outcome attribute, the counts from the counters element, the derived skipped figure, the verbatim not-executed and inconclusive figures, and the names of results whose outcome is Failed.
- `Format-TrxRunSummary` — parameter `-Summary`. Returns the emitted text, which states the skipped derivation inline.

Changed:

- `Get-VsTestArgumentList` and `Get-DotnetCoverageArgumentList` — the complete argument-builder family derived in the research artifact's Numeric Derivation Evidence section, count 2, member sets identical under two independent derivations. Each gains a mandatory results directory parameter and a mandatory log file name parameter, and appends a results-directory switch and a trx logger switch carrying an explicit log file name. In the coverage builder both switches belong to the test-console segment, that is after the argument separator.
- `Invoke-MSTestMain` and `Invoke-MSTestWithCoverageMain` — resolve the results directory and log file name, pass them to the builder, and perform the post-run steps.

#### Data flow and validation changes

Coverage run: collector output is post-processed in memory as today; the post-processed string is then used for the threshold assertion, the first-party report line, the projection write and the reconciliation assertion, in that order; the conditional discard runs last. The projection is written beside the resolved coverage output path with the JaCoCo extension.

Plain run: the test console writes a test-result document at the explicit results directory and log file name; the entry point reads it, writes the summary beside it, then discards the raw document.

#### Error handling and logging updates

- Missing packages node reuses the existing throw wording.
- A reconciliation mismatch throws with both the expected and the observed totals, so the failure is actionable.
- A missing or unreadable test-result document is reported as a non-fatal warning on the plain run path so that a test failure is still surfaced by the existing exit-code check rather than masked by a summary-reader exception.
- No account name, host name or absolute path is written into any emitted summary or projection. R1.5 records that the projection carries no file-name attribute, which is what removes the absolute host paths that the Cobertura class elements carried.

#### Rollback considerations

Each of the four areas is independently revertible. The argument-builder change is the only one that alters an externally observable command line; reverting it restores the previous default file name.

### Technical specifications

#### Projection output format (D1)

- No XML declaration and no DOCTYPE. The first line is the root element.
- Root element `report` with a single attribute `name` whose value is the solution name, that is the literal string TaskMaster.
- One `package` child per package, each with a single attribute `name` carrying the Cobertura package name.
- Exactly two `counter` children per package, in the order LINE then BRANCH, each carrying `missed` and `covered`.
- Two-space indent per level; a space before the self-closing slash.
- No file-name, class, method, source-file, line, instruction or class-count elements or counters anywhere.

#### Test-result summary content (D4)

The counters element has no skipped attribute. Skipped is defined as total minus executed, that derivation is stated in the emitted summary text itself, and the not-executed and inconclusive figures are additionally recorded verbatim so that no figure reported by the test platform is lost.

#### Required configuration values and defaults

- Results directory: a directory beneath the repository coverage directory, supplied as a defaulted parameter on each entry point so a caller can override it.
- Log file name: a fixed, task-specific name per entry point, with no account, host or timestamp component.

#### Backward-compatibility expectations

- The editor coverage extension continues to read the repository coverage directory unchanged, because of invariant 4.
- The two feature-review coverage validation hooks are unaffected. R11 establishes they read four fixed paths, none under a feature evidence tree, and that they read JaCoCo rather than Cobertura or test-result documents.
- The format gate needs no change: the formatter ignore file already excludes every feature evidence tree, so a new artifact format placed there is covered.
- Per-file coverage figures are not carried by a package-level projection. R11 records that the feature-review workflow imposes per-file obligations, and that the established practice is to record the per-file figures in the prose of the dependent evidence artifact while the raw document still exists. That sequencing is preserved by invariant 4.

#### Performance constraints

The projection writer operates on the already-parsed post-processed document, which is orders of magnitude smaller than raw collector output and is already held in memory by the entry point. No streaming reader is required, and none is specified.

## Write Set

`scripts/vscode/Invoke-MSTestWithCoverage.ps1`
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
`scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1`
`scripts/vscode/Invoke-MSTest.ps1`
`scripts/vscode/Invoke-MSTest.TrxSummary.ps1`
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1`
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`
`tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1`
`tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1`
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
`tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1`
`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`
`TaskMaster/TaskMaster.csproj`
`.vscode/settings.json`
`CLAUDE.md`
`.claude/agent-memory/_shared_no_absolute_host_paths.md`
`.claude/agent-memory/epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md`
`.claude/agent-memory/feature-review/project_464-review-residuals.md`
`.claude/agent-memory/feature-review/project_488-review-residuals.md`
`.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md`
`.claude/agent-memory/orchestrator/collect-pr-context-lands-in-main-checkout.md`
`docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873`

Two of the test-file entries above are forced by the builder signature change rather than chosen: `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` pins the complete argument array handed to the plain wrapper seam as an exact four-element assertion, which becomes a six-element array once the plain builder gains the results-directory and log-file-name switches, and `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` declares an explicit five-parameter mock body for `Invoke-DotnetCoverageCollection`, which stops binding once that function gains two parameters.

## Identifier Corrections

The substitution target governs the replacement form, per R7 and the corruption precedent in R4.3.

1. **`TaskMaster/TaskMaster.csproj` line 37 — XML element text.** Replace the element text with the repository-relative value that the other project file in this repository already uses for the same property: the single segment "publish" followed by a trailing backslash. Not an empty value, and never an angle-bracket placeholder, because the element text is XML markup context. R8 establishes that nothing in this worktree reads the property, that no publish target is invoked anywhere, and that the relative form is the established in-repository precedent. A secondary benefit R8 records is that this also removes the deployment-url residual leak class from future build diagnostic logs.
2. **`.vscode/settings.json` line 27 — JSON string that must remain a working directory path.** Replace with the editor workspace-folder variable `${workspaceFolder}` followed by the relative path to the Excel Power Query symbols directory that already exists under the editor configuration folder in this repository. A bare placeholder would break the setting, because the value is consumed by an extension as a real directory path.
3. **The five named agent-memory files — Markdown prose inside inline-code spans.** Angle-bracket placeholders are safe here. Four of the five take a token substitution. The fifth, `.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md`, uses the account name twice on one line to draw a case-sensitivity contrast between a case-sensitive search and a case-insensitive one; substituting the same placeholder into both positions destroys the sentence's meaning. That line requires a short rewrite that preserves the contrast by describing the two searches rather than quoting the token.

## Convention and Rule Text

**Convention home (D8): `CLAUDE.md`.** R9 establishes that the evidence-and-timestamp-conventions skill is push-down owned, so an edit there is reverted on the next push-down and the convention is lost; the obligation cannot be satisfied inside the editor-agent skills tree at all. The repository-root instruction file is TaskMaster-owned, already carries the C# toolchain order including the test-console step the new rule attaches to, and is loaded into every agent session. The added text states the permitted format of committed test evidence — a package-level JaCoCo projection plus the first-party summary line for coverage, a test-result summary for test runs, and no raw collector or test-platform document — and states the explicit results-directory and log-file-name requirement on the test step.

**Hygiene rule text (D7): `.claude/agent-memory/_shared_no_absolute_host_paths.md`.** R10 establishes no sweep exists as executable code; building one is the repository-wide sweep item's scope. The obligation is therefore delivered as rule text, in a file R9 establishes is not push-down owned. The added text states two rules: a per-plan hygiene task must include the plan file itself in its residual scan, because a scan that excludes the plan by path cannot detect a host path reintroduced into the plan; and a residual-match count of zero is necessary but not sufficient, so it must be paired with a parse check on every XML-family file the sweep rewrites.

## Assumptions, Constraints, Dependencies

- Assumptions: the repository coverage directory remains ignored and remains the directory the editor coverage extension reads; the post-processor continues to write the root line attributes from the same de-duplicated rule the per-package helper uses.
- Constraints: no file may exceed 500 lines; the helpers file has 29 lines of headroom per R5 and therefore receives only a dot-source line; temporary files are prohibited in tests with no approved exceptions; the PowerShell change budget caps a batch at three production files and three test files, and this delivery spans five production files and seven test files — four new and three existing files forced by the signature changes — so it will be batched.
- Toolchain prerequisite, load-bearing for AC15 and AC22: the project-file edit puts this delivery under the C# policy, whose analyzer and nullable gates are two full solution rebuilds. An agent worktree does not inherit a built toolchain, so those gates are unsatisfiable until the worktree is bootstrapped. The plan's Phase 0 must therefore restore the tool manifest, restore the solution's packages, and capture a baseline from both msbuild passes before any edit, or the "no new diagnostics" comparison in AC15 has no baseline to compare against and the gate cannot be evaluated.
- External dependencies: none. No new module, package or tool is introduced.

## Data / API / Config Impact

- User-facing changes: the two entry points print a test-result summary, and the coverage entry point additionally writes a projection file beside the coverage output.
- Migration considerations: none. Already-tracked raw evidence is untouched by this item.
- Logging updates: the summary text and the reconciliation failure message are the only new operator-facing output.
- Compatibility notes: both argument builders gain two mandatory parameters. Every in-repository caller is updated in the same delivery; the builders are not a published API.

## Test Strategy

Framework: Pester 5, exercised with `Invoke-Pester`. Static analysis with `Invoke-ScriptAnalyzer`, formatting with `Invoke-Formatter`. The repository has no Python toolchain; no pytest test is written.

**Fixture mechanism — the only compliant one.** R6 establishes the pattern and the repository prohibits temporary files in tests with no approved exceptions. Every fixture document is a PowerShell here-string assigned to a script-scoped variable inside `BeforeAll` and cast to `[xml]` inside the `It` block. No file is written by any test. Test-only helper functions are declared inside `BeforeAll`, not at file scope, because Pester 5 runs each `It` block in a child scope of the containing block and only a function declared there is resolvable from one. Each test file dot-sources `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` once, which resolves the part-file functions transitively.

**Allowlist handling.** Do not mock the project allowlist function. R6 establishes there is no such mock anywhere in the repository. The established pattern is explicit parameter injection at every call site plus an assertion, read from the function's abstract syntax tree, that the parameter's default value text is unchanged. Invoking the allowlist inside a unit test would additionally perform a recursive repository scan, which a unit test must not do.

**Mocking.** Mock only the named wrapper seams around the external executables, with a mock parameter block matching production exactly, and capture the argument array into a script-scoped variable. Do not mock a pure function.

Test files and their content:

- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1` — projection writer and reconciliation assertion. Positive: a multi-package fixture produces one package element per package in document order with correct missed and covered values, asserted as an exact serialised string, because the acceptance is exact-shape equality. Arithmetic: a fixture where covered is strictly less than valid proves missed equals valid minus covered for lines and for branches independently. Boundary: a package with no classes yields zero missed and zero covered. Boundary: a document carrying no branch data yields a BRANCH counter with zeroes rather than an omitted counter. Negative: a document with no packages node throws the existing wording. Reconciliation: one positive test and one negative test proving a mismatch throws. Delegation: an abstract-syntax-tree assertion that the projection part file invokes the per-package helper and contains no line-number map and no condition-coverage parsing, which is what proves the counting rule was not re-derived.
- `tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1` — the pure reader and formatter. Positive: a passing fixture yields the verdict and the counts. Negative: a failing fixture yields the names of results whose outcome is Failed. Namespace: a fixture that declares the default namespace is read correctly, paired with a companion assertion that an unprefixed XPath over the same fixture selects nothing, so a namespace regression cannot pass as a false green. Derivation: skipped equals total minus executed, and the formatter output states that derivation. Edge: a fixture with zero result elements yields an empty failed-name collection rather than a null reference.
- `tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` — one file per argument builder. Call each builder directly and assert membership and ordering of the returned array by index comparison, following the established argument-builder test pattern. For the coverage builder additionally assert that both new switches appear after the argument separator. Assert the entry-point default results-directory value from the function's abstract syntax tree rather than by invoking the entry point. Assert the retention predicate returns true for the repository coverage directory and false for any other directory, and assert the discard ordering by capturing call order through the wrapper seams. No test creates or deletes a file.

New behaviour is asserted in new test files rather than by extending the existing shared argument-builder test file, which R12 records as already holding describe blocks for both entry points and as a contention risk with a concurrent item. That shared file is still edited, but only to repair the call sites the builder signature change forces; no new describe block is added to it. No test is added to the test file that covers the coverage threshold function, for the same reason.

**Regression coverage for the identifier corrections.** An assertion that a token no longer appears is an absence gate and is necessary but not sufficient. Per R4.3 and the shared host-path rule document, pair every absence assertion with a parse check: the project file must re-parse as XML and the editor settings file must re-parse as JSON after the correction.

**Coverage targets.** Every new function is pure and must reach at least 90% line coverage, measured by a direct Pester coverage capture over the two new production part files. Changed lines in the two entry points must not reduce their existing coverage.

**Toolchain.** `Invoke-Formatter`, then `Invoke-ScriptAnalyzer`, then `Invoke-Pester` for the script work; and for the project-file change, `dotnet tool run csharpier check .` followed by the analyzer and nullable msbuild passes named in `CLAUDE.md`. Any step that fails or rewrites a file restarts the loop.

**Manual validation.** Run the coverage entry point once with the output path left at its default and confirm the raw document is retained in the repository coverage directory and the projection is written beside it; run it once with the output path pointed at a scratch directory outside that tree and confirm the raw document is discarded after the projection and reconciliation complete.

**Batching.** Five production files and seven test files exceed the three-production and three-test per-batch cap, so the implementation plan must split the work. Two constraints bind the split. First, each builder signature change and the repair of every existing test file that change breaks must sit in the SAME batch, or that batch's test gate runs against call sites the batch itself has just broken and cannot pass. Second, no batch may carry more than three test files. Delivering each pure part file with its own new test file as a self-contained batch, then one entry point per batch with exactly its forced repairs and its new test file, satisfies both. The project-file, editor-settings and Markdown corrections carry no PowerShell batch cap and form their own batch.

## Acceptance Criteria

- [x] **AC1 — Projection shape.** A Pester test in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1` asserts that `ConvertTo-JacocoPackageProjection` returns a string exactly equal to a here-string literal that has no XML declaration and no DOCTYPE, a root `report` element whose only attribute is `name` carrying the literal string TaskMaster, one `package` child per fixture package with a single `name` attribute, exactly two `counter` children per package in the order LINE then BRANCH each carrying `missed` and `covered`, two-space indentation, and a space before each self-closing slash. The test passes.
- [x] **AC2 — Missed derivation.** A Pester test in the same file uses a fixture in which covered is strictly less than valid for both lines and branches and asserts the emitted `missed` equals valid minus covered independently for the LINE counter and the BRANCH counter. The test passes.
- [x] **AC3 — Counting rule not re-derived.** A Pester test parses the abstract syntax tree of `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` and asserts that `Get-CoberturaPackageLineSummary` is invoked and that the file contains no condition-coverage parsing and no per-line-number map. The test passes.
- [x] **AC4 — Package set inherited.** A Pester test asserts that a three-package post-processed fixture yields exactly three `package` elements whose `name` attributes match the fixture package names in document order. A second assertion, read from the entry point's abstract syntax tree, confirms the projection is invoked with the post-processed content produced by `ConvertTo-KoverageCoberturaXml` and not with the raw collector string. Both pass. The spec does not adopt the fifteen-package set of the committed #646 instance.
- [x] **AC5 — Reconciliation is exact and required.** Two Pester tests exist: one asserts `Assert-JacocoProjectionReconciliation` returns without throwing when the summed package LINE counters equal the source root covered-lines and valid-lines attributes; the other asserts it throws when they disagree, and that the thrown message names both the expected and the observed totals. Both pass. A third assertion, from the entry point's abstract syntax tree, confirms the reconciliation call is present on the coverage path.
- [x] **AC6 — Existing error wording reused, no second wording introduced.** A Pester test asserts that `ConvertTo-JacocoPackageProjection` given a document with no packages node throws a message whose text is byte-identical to the wording the first-party helper already throws for that condition. Separately, a case-sensitive search across the editor script directory for lines containing the phrase "does not contain a" collects the set of distinct message strings; that set has exactly one member both before and after this change. The count of occurrences is not asserted, because the wording already occurs three times across two existing part files and an occurrence count therefore measures nothing about this delivery. Both observations hold.
- [x] **AC7 — Zero-branch uniformity.** A Pester test asserts that a fixture carrying no branch data still emits a BRANCH counter with `missed="0" covered="0"` for every package, rather than omitting it. The test passes.
- [x] **AC8 — Empty package boundary.** A Pester test asserts that a package element containing no class elements emits a LINE counter with `missed="0" covered="0"`. The test passes.
- [x] **AC9 — Namespace handling proven, not assumed.** Two Pester tests in `tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1` exist over the same fixture, which declares the default TeamTest namespace: one asserts `Get-TrxRunSummary` reads the counters correctly; the other asserts that an unprefixed XPath over that fixture selects zero nodes. Both pass, which proves the reader does not depend on the namespace being absent.
- [x] **AC10 — Skipped derivation stated in the output.** A Pester test asserts that for a fixture whose total exceeds its executed count, `Get-TrxRunSummary` reports skipped equal to total minus executed and additionally reports the not-executed and inconclusive figures verbatim; and that the string returned by `Format-TrxRunSummary` contains an explicit statement of that derivation. The test passes.
- [x] **AC11 — Verdict and failed names.** A Pester test asserts that a failing fixture yields the run verdict from the result-summary outcome attribute and the names of exactly those results whose outcome attribute is Failed; a second test asserts a fixture with zero result elements yields an empty collection rather than a null reference. Both pass.
- [x] **AC12 — Both argument-builder family members carry the two switches.** The complete family is the two members derived in the research artifact's Numeric Derivation Evidence section, whose primary and cross-check member sets are identical and whose counts both equal 2. A Pester test per member asserts the returned array contains a results-directory switch carrying the supplied directory and a trx logger switch carrying the supplied explicit log file name; and for the coverage member, that the index of each of those two elements is greater than the index of the argument separator. All assertions pass.
- [x] **AC13 — Results directory is beneath the ignored coverage tree.** A Pester test reads the results-directory parameter default from each entry point's abstract syntax tree and asserts the default text resolves beneath the repository coverage directory. The test passes. Separately, the repository ignore file is unchanged: a name-listing diff anchored to the base commit the plan's Phase 0 records, paired with a porcelain status listing in the same task so untracked additions are also visible, shows no entry for it. The Write Set is not offered as evidence for this, because checking a claim against the document that makes it verifies nothing.
- [x] **AC14 — Conditional discard invariant.** Two Pester tests assert `Test-RawCoverageDocumentRetained` returns true when the output path's parent directory is the repository coverage directory and false for any other directory. A third test captures call order through the wrapper seams and asserts the discard occurs only after the threshold assertion, the projection write and the reconciliation assertion have all completed. All pass, and no test creates or deletes a file.
- [x] **AC15 — Project-file correction, absence paired with a parse check.** `TaskMaster/TaskMaster.csproj` line 37 carries the publish-destination element with the repository-relative value used by the other project file in this repository for the same property; a case-insensitive search of that file for the account, host and employer organization tokens and for a drive-letter-rooted user-profile path returns zero matches; and the file loads without error as an XML document. All three observations hold, and each of the two msbuild passes named in `CLAUDE.md` records an exit code and an error count that are no worse than the Phase 0 baseline recorded for that same command. The comparison is baseline-relative by construction: "no new diagnostics" has no meaning without a recorded prior count, so Phase 0 must capture one per pass as an integer rather than as a prose adjective. An unqualified absence of error from a solution-wide rebuild is deliberately not demanded, because the pre-change state of that rebuild is not this delivery's to repair.
- [x] **AC16 — Editor settings correction, absence paired with a parse check.** The Power Query additional-symbols array element in `.vscode/settings.json` begins with `${workspaceFolder}` and contains no drive letter and no account token; the file parses as JSON; and the directory the value resolves to exists in the repository and contains its symbols document. All three observations hold.
- [x] **AC17 — The five named memory files.** Each of the five agent-memory files listed in the Write Set returns zero matches for the account token and the host token under a case-insensitive search, and each remains valid Markdown with balanced inline-code spans. In `.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md` the case-sensitivity contrast is preserved by a rewrite: the rewritten sentence describes a case-sensitive search and a case-insensitive search and contains neither the account token nor two copies of the same placeholder in the contrasting positions. These five are the named scope of this item; this criterion does not assert that five is the complete repository population, which R7.1 shows it is not, and the remainder is the repository-wide sweep item's scope.
- [x] **AC18 — Convention recorded in a TaskMaster-owned document.** `CLAUDE.md` carries a new section that states the permitted committed test-evidence formats — a package-level JaCoCo projection plus the first-party summary line for coverage runs, a test-result summary for test runs, and no raw collector or test-platform document — and its test-console toolchain step names both the explicit results-directory switch and the explicit log-file-name form. Both observations are confirmed by reading the file. Separately, no push-down-owned governance document is edited: a name-listing diff anchored to the base commit the plan's Phase 0 records, paired with a porcelain status listing in the same task, contains no path under the editor-agent rules, skills, agents, hooks or lib directories, no editor-agent settings document, and neither of the two shared configuration documents under the repository configuration directory.
- [x] **AC19 — Hygiene rule text amended.** `.claude/agent-memory/_shared_no_absolute_host_paths.md` states both rules: that a per-plan hygiene task must include the plan file itself in its residual scan, and that a residual-match count of zero is necessary but not sufficient and must be paired with a parse check on every XML-family file the sweep rewrites. Both statements are present. No executable sweep is added by this delivery.
- [x] **AC20 — File-size ceiling and helpers headroom.** Every PowerShell file in the Write Set is at most 500 lines after the change, and the net line growth of `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` is at most 1 line, that line being the dot-source of the new projection part file. Both are confirmed by a line count over the changed files.
- [x] **AC21 — New-code coverage.** A direct Pester coverage capture over `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` and `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` reports at least 90% line coverage for each, and the capture is recorded in this feature folder's evidence tree under the qa-gates kind. The qa-gates kind is named deliberately: the canonical evidence kinds are baseline, regression-testing, qa-gates, issue-updates, other and remediation-baseline, and a coverage kind is not among them, so an artifact written under a coverage kind would fail the evidence-path rule.
- [x] **AC22 — Full toolchain pass and no temporary files.** A single consecutive pass of `Invoke-Formatter`, `Invoke-ScriptAnalyzer`, and `Invoke-Pester` over the changed script and test files completes with zero new findings and zero failed tests, and the C# format check completes without error. The two msbuild passes named in `CLAUDE.md` are judged against the Phase 0 baseline rather than against absolute zero: each must record an exit code and an error count that are no worse than the Phase 0 baseline recorded for that same command. Absolute success is deliberately not demanded, because the pre-change state of a whole-solution rebuild is not this delivery's to fix and a red baseline would make the clause unsatisfiable for reasons this change does not cause. A review of the seven test files in the Write Set confirms no test creates, writes or deletes a file on disk and no fixture is loaded from a path.
- [ ] **AC23 — End-to-end observation.** The coverage entry point is run twice: once with the coverage output left at its default, after which the raw document is still present in the repository coverage directory and a projection file sits beside it that parses as XML and whose summed LINE counters equal the raw document's root covered-lines and valid-lines attributes; and once with the output pointed at a directory that is not the repository coverage directory itself, after which the raw document is absent and the projection and summary are present. The second run's directory is required to differ from the repository coverage directory; it is not required to sit outside the coverage tree. Invariant 4 states the discard branch as any directory other than the repository coverage directory, so a subdirectory of that tree exercises the discard branch exactly as an unrelated directory would, and keeping the second run inside the already-ignored coverage tree is what stops it adding a path that the footprint inventory and the clean-tree gate would otherwise have to admit. Both runs also produce a test-result summary. Neither run creates a test-result document bearing the default account-and-host file name: the check is scoped to paths that a porcelain status reports as added or modified relative to the base commit the plan's Phase 0 records. A whole-working-tree scan is explicitly not used, because more than one hundred such documents are already tracked from earlier features — one of them still carrying an unredacted default name — so a tree-wide scan would fail no matter what this delivery does. Removing those is the repository-wide sweep item's scope.

## Acceptance Status Summary

Written by plan task [P7-T14] at 2026-09-13T07-24. Every path below is repository-relative and every
evidence path is relative to
`docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/`.

AC_TOTAL: 23
AC_SATISFIED: 22
AC_OUTSTANDING: 1

| AC | Status | Evidence |
|---|---|---|
| AC1 | SATISFIED | `evidence/regression-testing/p1-t3-projection-shape-tests.md` |
| AC2 | SATISFIED | `evidence/regression-testing/p1-t3-projection-shape-tests.md` |
| AC3 | SATISFIED | `evidence/qa-gates/p1-t7-phase1-toolchain.md` |
| AC4 | SATISFIED | `evidence/regression-testing/p3-t11-ac4-check-off.md` |
| AC5 | SATISFIED | `evidence/regression-testing/p3-t12-ac5-check-off.md` |
| AC6 | SATISFIED | `evidence/regression-testing/p1-t4-distinct-wording-set.md` |
| AC7 | SATISFIED | `evidence/qa-gates/p1-t7-phase1-toolchain.md` |
| AC8 | SATISFIED | `evidence/qa-gates/p1-t7-phase1-toolchain.md` |
| AC9 | SATISFIED | `evidence/regression-testing/p2-t2-namespace-tests.md` |
| AC10 | SATISFIED | `evidence/regression-testing/p2-t3-derivation-tests.md` |
| AC11 | SATISFIED | `evidence/regression-testing/p2-t4-verdict-and-failed-name-tests.md` |
| AC12 | SATISFIED | `evidence/regression-testing/p4-t8-ac12-check-off.md` |
| AC13 | SATISFIED | `evidence/regression-testing/p4-t9-ac13-check-off.md` |
| AC14 | SATISFIED | `evidence/regression-testing/p3-t13-ac14-check-off.md` |
| AC15 | SATISFIED | `evidence/regression-testing/p5-t1-project-file-correction.md`, `evidence/qa-gates/p7-t5-final-msbuild-analyzer.md`, `evidence/qa-gates/p7-t6-final-msbuild-nullable.md` |
| AC16 | SATISFIED | `evidence/regression-testing/p5-t2-editor-settings-correction.md` |
| AC17 | SATISFIED | `evidence/regression-testing/p5-t6-memory-substitutions.md` |
| AC18 | SATISFIED | `evidence/regression-testing/p5-t3-convention-section.md`, `evidence/regression-testing/p5-t4-toolchain-step-amendment.md` |
| AC19 | SATISFIED | `evidence/regression-testing/p5-t5-hygiene-rule-amendment.md` |
| AC20 | SATISFIED | `evidence/qa-gates/p7-t8-file-size-audit.md` |
| AC21 | SATISFIED | `evidence/qa-gates/p7-t7-new-code-coverage.md`, `evidence/qa-gates/p7-t7-new-code-coverage.jacoco.xml` |
| AC22 | SATISFIED | `evidence/qa-gates/p7-t1-final-format.md`, `evidence/qa-gates/p7-t2-final-analyze.md`, `evidence/qa-gates/p7-t3-final-test.md`, `evidence/qa-gates/p7-t4-final-csharpier-check.md`, `evidence/qa-gates/p7-t5-final-msbuild-analyzer.md`, `evidence/qa-gates/p7-t6-final-msbuild-nullable.md`, `evidence/qa-gates/p7-t13-no-temporary-files-review.md` |
| AC23 | OUTSTANDING | `evidence/qa-gates/p7-t0-phase6-outstanding-disclosure.md`, `evidence/regression-testing/p6-t1-assembly-inventory.md`, `evidence/regression-testing/p6-t2-default-output-run.md` |

### AC23 — reason for OUTSTANDING

AC23 is recorded as OUTSTANDING and is not recorded as satisfied. Its checkbox above is deliberately
left unmarked.

Reason: plan tasks P6-T2, P6-T3, P6-T4 and P6-T5 have not run. P6-T1 is complete and recorded the
assembly inventory; P6-T2's first attempt was aborted on a pre-existing defect and is recorded rather
than repaired, as the plan's Phase 6 flakiness rule directs. AC23's operative observations are the two
end-to-end runs of the coverage entry point and the default-name scan over their output, and none of
those observations has been made. The evidence paths listed for AC23 are the artifacts that record
that absence and its reason, not artifacts that satisfy the criterion.

The Phase 7 pass that produced this summary was taken with those four Phase 6 tasks outstanding. The
disclosure artifact records that fact, names the tracked paths Phase 6 would modify, and records the
determination that none of them is a file any Phase 7 gate measures, so the Phase 7 results above
stand on their own evidence.

### Headline numeric results behind the Phase 7 criteria

| Gate | Result | Baseline |
|---|---|---|
| PowerShell analyzer diagnostic set | 16 diagnostics, 0 absent from baseline | 16 (P0-T11) |
| PowerShell tests | 133 passed, 0 failed, 0 skipped | 103 passed (P0-T12) |
| C# format check | exit 0, 1626 files checked | exit 0 (P0-T7) |
| C# analyzer rebuild | exit 0, 0 warnings, 0 errors | exit 0, 0 errors (P0-T8) |
| C# nullable rebuild | exit 0, 0 warnings, 0 errors | exit 0, 0 errors (P0-T9) |
| New-code coverage, `Invoke-MSTest.TrxSummary.ps1` | 92.86 percent line | floor 90 |
| New-code coverage, `Invoke-MSTestWithCoverage.Projection.ps1` | 92.50 percent line | floor 90 |
| Largest PowerShell file | 498 lines | ceiling 500 |
| Helpers file growth | +1 line | bound +1 |

## Risks & Mitigations

- **Risk: per-file coverage detail is lost once the raw document is discarded.** R11 records that the feature-review workflow imposes per-file obligations that a package-level projection cannot satisfy. Mitigation: invariant 4 keeps the document in the repository coverage directory on the default path, which is exactly where the established reviewer workaround reads per-file figures from; and the discard is sequenced after every gate that needs the detail.
- **Risk: a placeholder substitution corrupts a markup file.** R4.3 records 19 plus 6 unparseable documents caused by exactly this. Mitigation: AC15, AC16 and AC17 each pair the absence check with a parse or structural check, and the delivery places no angle-bracket placeholder in XML markup.
- **Risk: contention with the concurrent branch-coverage item and the repository-wide sweep item.** Mitigation: the Write Set excludes the coverage threshold part file and its test file entirely, new test files use previously unused names rather than extending the shared argument-builder test file, and #602 is sequenced after this item.
- **Risk: the two new mandatory builder parameters break an unnoticed caller.** Mitigation: R3.1 enumerates the invocation sites exhaustively and finds no third site under the editor script directory; the continuous-integration workflow builds its command line inline and is unaffected.

## Rollout & Follow-up

- Rollout: merge on green. No runtime or deployment change accompanies this item.
- Post-fix task: run issue #602's historical sweep after this item merges, so a fresh test run does not reintroduce the prefix that the sweep just removed.
- Upstream follow-up, recorded in prose because the target is push-down owned and cannot be edited in this repository: the atomic-plan contract's evidence tasks should cite the new convention section. That change must be made in the upstream customization repository, not here, or it will be reverted on the next push-down.
- Links: issue #873; closes #671 and #728 on merge; partially addresses #602 (its acceptance criteria 3 and 4).
