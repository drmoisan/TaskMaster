# Feature Audit — Issue #873, test-evidence projection convention and identity-leak tooling

- Review timestamp: 2026-09-13T15-20
- Branch: `bug/test-evidence-projection-convention-and-identity-leak-tooling-873`
- Head commit: `5b1d5d93d0cf8b7b6a36aae68b24f7fd6086cd25`
- Review cycle: 1
- All paths are repository-relative. No absolute host path, account token or host token appears in this document.

## Scope and Baseline

Work mode marker read from `issue.md` line 12: `- Work Mode: full-bug`. Under that mode the single acceptance-criteria source is `spec.md`. `user-story.md` exists in this folder and carries the headings Narrative, Why this is being fixed now, What a reviewer will notice after the change, What this story does not cover and Stakeholders; it contains no checkbox and no acceptance criterion, which matches the declaration at `spec.md` line 9 that it is narrative only. It was read for context and is not treated as an acceptance-criteria source.

Baseline resolution. The resolved base branch is `origin/main` at `a5622ab9123a88bfa3ec5b8fccfdc613e74c4df5`. `git merge-base` with the head equals that same commit, because `origin/main` was merged into this branch during Phase 6, so the two-dot and three-dot ranges coincide and the change set is exactly this delivery's own content with no inherited material. The PR context artifacts at `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` were regenerated at 2026-09-13 19:09:02 UTC against head `5b1d5d93d0cf8b7b6a36aae68b24f7fd6086cd25`, which matches the head under review, so they are current and were not regenerated again.

Two base anchors appear in the delivery's own evidence and their relationship matters when reading it. `refs/base-anchor-873` resolves to `5cc7dcd6330b44e3d2238ffe7fa1e5b3317d9ee1`, which is the pre-merge base the plan recorded in Phase 0; the Phase 7 inventory measured 100 changed paths against it. After Phase 6 merged `origin/main` at `a5622ab9`, the same measurement returned 195 paths, and the delivery's own artifact decomposes the +95 difference with no remainder into 91 paths inherited from the merge and 4 of its own artifacts that post-dated the earlier measurement. That decomposition was checked and is arithmetically consistent. The audit below uses the resolved base of `a5622ab9`, against which the change set is 107 paths, 22 of them outside this feature folder.

Change set audited:

| Category | Count | Detail |
|---|---|---|
| PowerShell production | 5 | 2 created, 3 modified |
| PowerShell tests | 7 | 4 created, 3 repaired |
| Repository instruction file | 1 | `CLAUDE.md` |
| Project file | 1 | `TaskMaster/TaskMaster.csproj` |
| Editor settings | 1 | `.vscode/settings.json` |
| Agent-memory documents | 6 | 5 substitutions, 1 rule-text addition |
| Inherited promotion rename | 1 | authored by the pre-Phase-0 preparation commit |
| Feature folder documents and evidence | 85 | including 69 evidence artifacts |

## Acceptance Criteria Inventory

`spec.md` carries 23 acceptance criteria under the heading `## Acceptance Criteria`, identified AC1 through AC23 at lines 274 to 296. All 23 were already marked `[x]` when this audit began.

The file also contains four checkboxes at lines 30 to 33 under the heading `Impact / Severity`, of which Medium is checked and Blocker, High and Low are unchecked. Those four are a severity selector, not acceptance criteria, and they are excluded from the count. The inventory is therefore 23, not 27.

| Identifier | Subject |
|---|---|
| AC1 | Projection shape, asserted as exact serialised equality |
| AC2 | Missed derived as valid minus covered, lines and branches independently |
| AC3 | Counting rule not re-derived, proven from the abstract syntax tree |
| AC4 | Package set inherited from the post-processed document |
| AC5 | Reconciliation is exact and required, positive and negative |
| AC6 | Existing error wording reused, no second wording introduced |
| AC7 | Zero-branch uniformity, counter emitted rather than omitted |
| AC8 | Empty-package boundary |
| AC9 | Namespace handling proven, not assumed |
| AC10 | Skipped derivation stated in the rendered output |
| AC11 | Run verdict and failed test names |
| AC12 | Both argument-builder family members carry the two switches |
| AC13 | Results directory beneath the ignored coverage tree; ignore file unchanged |
| AC14 | Conditional discard invariant and ordering |
| AC15 | Project-file correction, absence paired with a parse check |
| AC16 | Editor settings correction, absence paired with a parse check |
| AC17 | The five named memory files |
| AC18 | Convention recorded in a repository-owned document |
| AC19 | Hygiene rule text amended |
| AC20 | File-size ceiling and helpers headroom |
| AC21 | New-code coverage |
| AC22 | Full toolchain pass and no temporary files |
| AC23 | End-to-end observation |

## Acceptance Criteria Evaluation

Each criterion was evaluated against the tree and the evidence independently of its existing check mark. Where the check mark was found correct, that is stated as a verified result rather than accepted as given.

| AC | Verdict | Verification performed |
|---|---|---|
| AC1 | PASS | Read the test at `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1` lines 65-118. The expected value is a here-string with no XML declaration, no DOCTYPE, root `report` whose only attribute is `name="TaskMaster"`, one `package` per fixture package with a single `name`, exactly two `counter` children per package in the order LINE then BRANCH each carrying `missed` and `covered`, two-space indentation and a space before each self-closing slash, compared with `Should -BeExactly`. Recorded as passing in `evidence/qa-gates/p1-t7-phase1-toolchain.md`. One deviation: both sides pass through a line-ending normaliser, recorded as Non-blocking finding 4 in the code review. |
| AC2 | PASS | Read lines 120-163. The fixture has 3 of 5 lines covered and 1 of 4 conditions, so covered is strictly less than valid for both. The emitted LINE `missed` is asserted as 2 and BRANCH `missed` as 3, then re-derived from `Get-CoberturaPackageLineSummary` so the assertion is the subtraction rule rather than a second copy of the literals. |
| AC3 | PASS | Read lines 447-494. The test parses the projection part file and asserts `Get-CoberturaPackageLineSummary` is invoked at least once, that zero string constants anywhere in the file contain `condition-coverage`, that the file declares zero hashtable literals, and that it performs zero index assignments. The last two stand in for "no per-line-number map" and are the shapes that map takes in the existing summariser; the test comment states the substitution explicitly. |
| AC4 | PASS | Read lines 222-272 for the three-package document-order assertion, whose fixture names are deliberately non-alphabetical. Read `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` lines 224-254 for the abstract-syntax-tree assertion, which binds the projection call's `XmlDocument` argument to a variable and asserts that variable is the same one assigned from `ConvertTo-KoverageCoberturaXml`. Confirmed against `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 395, where the argument is `$processedXmlContent`, assigned at line 383 from the post-processor. |
| AC5 | PASS | Read lines 309-400 for the positive case and the covered-total negative case, whose message is asserted to match `expected 4` and `observed 3`. Read lines 402-432 for the valid-total negative case, asserted to match `valid total`, `expected 2` and `observed 4`. Read `Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` lines 256-267 for the abstract-syntax-tree assertion that the reconciliation call is present on the coverage path. |
| AC6 | PASS | Verified independently rather than from the artifact. A case-sensitive search of `scripts/vscode` for `does not contain a` returns four occurrences across three files, all of exactly one distinct string, `Cobertura XML does not contain a <packages> node.`, so the distinct-wording set has one member. The test at lines 274-305 obtains the expected message by invoking `Get-CoberturaFirstPartyCoverageSummary` on the same fixture and capturing what it throws, rather than restating the literal, and supplies an explicit `-ProjectNames` value so the allowlist default is never evaluated. |
| AC7 | PASS | Read lines 187-220. The test pins the counter count at 2 as well as the zero figures, so an implementation that omitted the BRANCH counter could not satisfy it by emitting nothing. |
| AC8 | PASS | Read lines 165-185. The fixture's `classes` element is empty and its own stale `line-rate` is deliberately non-zero, so a zero result cannot come from copying the input. |
| AC9 | PASS | Read `tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1` lines 98-119. Both tests run over the identical fixture, which declares the default TeamTest namespace. The first asserts the counters read correctly; the second asserts an unprefixed `//ResultSummary` path selects exactly zero nodes. Confirmed against `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` lines 49, 54 and 75, which use `local-name()` predicates throughout. |
| AC10 | PASS | Read lines 121-155. The fixture has total 12 against executed 7, so derived skipped is 5, which differs from its own `notExecuted` figure of 4; an implementation that copied `notExecuted` would fail rather than coincide. The formatter output is asserted to match `derived as total minus executed`, `Skipped 5`, `notExecuted 4` and `inconclusive 2`. Confirmed against the production text at lines 142-146. |
| AC11 | PASS | Read lines 158-192. The verdict comes from the result-summary `outcome` attribute, not a counter. The failing fixture holds four results of four outcomes, two of them Failed, and exactly those two names are asserted in document order. The zero-result fixture is asserted non-null, an array, and of count zero, deliberately without piping into `Should`. |
| AC12 | PASS | Read `tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1` lines 58-71 and `Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` lines 106-128. The plain member's two switches are asserted by index so the order is pinned; the coverage member's two switches are asserted present and each index asserted greater than the separator index. Both members are the complete family: `Get-VsTestArgumentList` and `Get-DotnetCoverageArgumentList` are the only two functions under `scripts/vscode` that build a test-console argument list, and both received the two mandatory parameters. |
| AC13 | PASS | Read both parameter-default tests, which read the declared default from the abstract syntax tree and assert it is exactly `'coverage\test-results'`. Confirmed at `scripts/vscode/Invoke-MSTest.ps1` line 164 and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 283. The repository ignore file is unchanged: no `.gitignore` path appears in the 107-path change set, in the 100-path Phase 7 inventory, or in the 195-path post-merge union. |
| AC14 | PASS | Read the three predicate tests at `Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` lines 146-175 — the coverage directory retains, an unrelated directory discards, and a subdirectory of the coverage tree discards. The third is not redundant: a containment implementation would return true there while still returning false for the unrelated directory, so without it a containment implementation passes both others. Read the call-order test at lines 177-221, which captures order through the seams and asserts the discard index exceeds the threshold, projection-write and reconciliation indices separately. No test creates or deletes a file, verified by independent scan. |
| AC15 | PASS | Read `TaskMaster/TaskMaster.csproj` line 37: `<PublishUrl>publish\</PublishUrl>`. Searched the file for the account, host and organization tokens and for a drive-letter-rooted user-profile path: zero matches, and no `OneDrive` segment, against a Phase 0 baseline that recorded one. The file re-parses as XML. Both msbuild passes recorded exit 0, 0 warnings and 0 errors in `evidence/qa-gates/p7-t5-final-msbuild-analyzer.md` and `p7-t6-final-msbuild-nullable.md`, equal to the Phase 0 baselines of exit 0 with 0 errors, and both were re-run after the merge changed C# compilation inputs with identical results recorded in `evidence/qa-gates/p6-post-merge-csharp-revalidation.md`. |
| AC16 | PASS | Read `.vscode/settings.json`. The Power Query additional-symbols array's single element is `${workspaceFolder}/.vscode/excel-pq-symbols`, beginning with the workspace-folder variable and containing no drive letter and no account token. The file is well-formed JSON. The resolved directory `.vscode/excel-pq-symbols` exists and contains `excel-pq-symbols.json`. All three observations hold. |
| AC17 | PASS | Verified independently rather than from the artifact. A case-insensitive search of `.claude/agent-memory/` for the account and host tokens returns five matching files, and none of the five is among the five this delivery corrects. Read all five corrected files: each uses `<account>`, `<host>`, `<user>` or `<repo-root>` placeholders with balanced inline-code spans. The case-sensitivity contrast in `.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md` is preserved by a rewrite at lines 17-21 that describes a case-sensitive search returning clean against a case-insensitive search finding nineteen files, containing neither the account token nor two copies of the same placeholder in the contrasting positions. |
| AC18 | PASS | Read `CLAUDE.md` lines 412-424. The new section `## Committed Test Evidence Format` states all three permitted forms — a package-level JaCoCo projection, the first-party summary line committed alongside it, and a test-result summary — and states that a raw collector document and a raw test-platform document are both prohibited, including under a feature folder's evidence tree. Both test-console toolchain steps at lines 390 and 408 carry `/ResultsDirectory:coverage\test-results` and `/Logger:trx;LogFileName=mstest-run.trx`. No push-down-owned governance document is edited: the only paths under `.claude/` in the change set are the six agent-memory documents, and the change set contains no path under `.claude/rules`, `.claude/skills`, `.claude/agents`, `.claude/hooks` or `.claude/lib`, no `.claude/settings.json`, and neither `config/blast-radius.json` nor `config/orchestration-routing.json`. |
| AC19 | PASS | Read `.claude/agent-memory/_shared_no_absolute_host_paths.md` lines 104-122. The section "Two obligations on the hygiene task itself" states both required rules: that a per-plan hygiene task must include the plan file in its residual scan, with the reason that a scan excluding the plan by path cannot detect a host path reintroduced into it; and that a zero residual-match count is necessary but not sufficient and must be paired with a parse check on every XML-family file the sweep rewrites. The closing sentence states that no executable sweep is added by this delivery and why. |
| AC20 | PASS | Twelve line counts recorded in `evidence/qa-gates/p7-t8-file-size-audit.md`, largest 498, none exceeding 500, taken after the final format step of the second toolchain pass. The helpers file measures 471 against a post-format Phase 0 baseline of 470, a growth of exactly one line, and that line is the dot-source of the projection part file at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` line 6. Spot-checked against the files themselves: the projection part file is 197 lines and the summary part file 150, matching the recorded figures. |
| AC21 | PASS | `evidence/qa-gates/p7-t7-new-code-coverage.md` pass 2 records 92.86% for `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` (39 of 42 line elements) and 92.50% for `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` (37 of 40), each above the 90 floor. The emitted JaCoCo document is committed at `evidence/qa-gates/p7-t7-new-code-coverage.jacoco.xml` under the `qa-gates` kind, with the artifact stating why that kind was chosen. The first measurement failed at 82.50% and is recorded with its diagnosis and the in-Write-Set remediation that closed it. |
| AC22 | PASS | A single consecutive clean pass exists. Pass 2 of Phase 7 ran format, analyze and test in order with zero new findings and zero failed tests: 16 analyzer diagnostics with zero entries absent from the Phase 0 tuple set, and 133 passed with 0 failed and 0 skipped. The C# format check recorded exit 0 over 1626 files. Both msbuild passes recorded exit 0 with 0 errors and 0 warnings, no worse than their Phase 0 baselines. The no-temporary-files clause was verified independently: a scan of all seven Write Set test files for thirteen filesystem-touching patterns returned 31 hits, every one a `Mock` declaration or a `Should -Invoke` assertion against a mock, with zero real filesystem calls, zero temporary files and zero fixtures loaded from a path. |
| AC23 | PASS | Both runs are recorded. The default-output run at `evidence/regression-testing/p6-t2-default-output-run.md` Run 2 recorded exit 0, 7222 of 7222 passed, the raw document retained in the repository coverage directory, the projection present beside it and parsing as XML, and its summed LINE counters equal to the raw root attributes at 56066 covered and 65416 valid. The external-output run at `evidence/regression-testing/p6-t3-external-output-run.md` recorded exit 0, the raw document absent from the chosen directory and the projection and summary present, which is the discard branch invariant 4 requires. Both runs produced a test-result summary. The default-name scan at `evidence/regression-testing/p6-t4-default-name-scan.md` recorded zero matches over each results-directory listing under both a strict and a looser pattern, and zero over the 195-path changed-path union, which contains no test-result document and no Cobertura document at all. The coverage tree was emptied immediately before the first run so every observation is attributable to it rather than to a stale file. |

### Criteria evaluated as PASS despite a recorded intermediate failure

Two criteria passed only after a recorded failure, and both are correctly marked because the criterion is judged on the final state while the failure remains in the audit trail.

AC21 failed its first measurement at 82.50% for the projection part file against a 90 floor. The diagnosis named three distinct unexercised regions — the no-root-element throw, the second reconciliation equality, and the whole body of the retention predicate, the last of which is tested but by a file the gate's fixed run path excludes. The remediation added two tests to a file already inside the Write Set, so no footprint widened and no production file changed, and the toolchain loop restarted from the format step as the General Code Change Policy requires. Both the failing and the passing measurement are recorded.

AC23 was recorded as outstanding when the Phase 7 status summary was first written, because the first end-to-end attempt aborted on three `QuickFiler.Test` failures rooted in a failed bind of `netstandard, Version=2.1.0.0`. The executor attributed the failure to a pre-existing defect outside the Write Set and reported rather than repaired it. That attribution is confirmed correct: the defect was repaired on `main` by item #877 and carried in by the Phase 6 merge, after which the re-run passed 7222 of 7222 with no Write Set file modified to make it pass. The spec records both states rather than erasing the earlier one, which is the right handling.

### Objectives from `issue.md` not covered by any acceptance criterion

`issue.md` states an Expected Behavior bullet that no acceptance criterion in `spec.md` carries forward: "The test runner path emits a summary (passed, failed, skipped, total, names of failed tests) and deletes the raw `.trx`." The plain entry point does delete it, at `scripts/vscode/Invoke-MSTest.ps1` line 256. The coverage entry point does not: its only `Remove-Item` targets the Cobertura document, and the post-run listing in `evidence/regression-testing/p6-t3-external-output-run.md` retains `mstest-coverage-run.trx`.

This is recorded here as an objective gap rather than an acceptance-criteria failure, because no criterion demands it of the coverage path and `spec.md`'s own data-flow section describes the coverage run's discard as applying to the collector document only. It is carried as Non-blocking finding 1 in `code-review.2026-09-13T15-20.md` with a recommendation. It produces nothing committable: both results directories sit beneath the already-ignored repository coverage tree, and the changed-path union contains no test-result document.

## Acceptance Criteria Check-off

No checkbox in `spec.md` was modified by this audit. All 23 were already marked `[x]` when the audit began, and the check-off decision is the caller's.

Every one of the 23 was evaluated independently of its existing mark and every one was found correct. No criterion is checked without supporting evidence, and no criterion required a mark to be cleared.

The Acceptance Status Summary in `spec.md` at lines 298 to 336 records `AC_TOTAL: 23`, `AC_SATISFIED: 23` and `AC_OUTSTANDING: 0`, with a per-criterion evidence table. Those figures agree with this audit. The table's evidence paths were spot-checked against the evidence tree and every cited artifact exists.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/spec.md`
- Total AC items: 23
- Checked off (delivered): 23
- Remaining (unchecked): 0
- Items remaining: none

## Summary

Verdict: **PASS**. 23 of 23 acceptance criteria satisfied, 0 not satisfied, 0 Blocking findings, 11 Non-blocking findings.

The delivery does what the issue asked. Committed test evidence is now a projection rather than a raw document, the convention is written down in the one document in this repository that is both loaded into every session and not liable to be overwritten from upstream, both test entry points pass an explicit results directory and log file name so the account-and-host-and-timestamp default file name is never produced, and the named identifier leaks in the project file, the editor settings file and five agent-memory documents are cleared with each absence check paired with a parse or structural check.

The verification behind the criteria is stronger than typical in three specific respects. The counting rule's non-duplication is proven from the abstract syntax tree rather than asserted. The namespace handling carries a companion test asserting that the naive path selects zero nodes, so a namespace regression cannot pass as a false green. The retention predicate carries a third test supplying a subdirectory of the coverage tree specifically so that a containment implementation cannot pass the other two by coincidence. In each case the test was designed to fail for the right reason, which is the property that distinguishes a test from a formality.

Two observations qualify the result without undermining it. The coverage entry point does not discard the raw test-result document although the plain path does, which is a gap against an `issue.md` objective that no criterion carried forward; and no post-change folder-wide or repository-wide PowerShell line-coverage figure was captured, so the per-new-file figures are the only measured coverage evidence for the PowerShell change. The reasoning behind treating the second as non-blocking is set out in section 5.3 of `policy-audit.2026-09-13T15-20.md`: the two new files are covered well above the folder's prevailing rate and therefore raise the folder figure, and the residual shortfall belongs to three scripts that carry no test file and that this delivery neither creates nor modifies.

Remediation is not required. The six recommendations in `code-review.2026-09-13T15-20.md` are improvements rather than corrections, and the first two are the ones worth acting on either before merge or as a small follow-up.
