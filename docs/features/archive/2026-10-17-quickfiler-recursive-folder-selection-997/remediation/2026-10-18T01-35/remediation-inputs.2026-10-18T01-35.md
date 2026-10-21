# Remediation Inputs: QuickFiler recursive folder selection (#997)

Timestamp: 2026-10-18T01-35
REVIEW_STATUS: REMEDIATION_REQUIRED
Remediation-Mode: proportionate
Primary requirements source: this file.
Source HEAD: 08d02c07f8a981cb34aa626951931fd78af28fd7.
Base/merge base: origin/main, 09a4522eccece6aeb0294bb2e4f9888cce24dd08.
Required plan target: docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/remediation/2026-10-18T01-35/remediation-plan.2026-10-18T01-35.md.

## R1 — Missing executable JavaScript coverage

Severity: Blocking
Remediability: autonomous
Remediability-Evidence: installed Node built-in execution/coverage tools support in-memory seams without new packages.
File: QuickFiler/Resources/FolderBreadcrumb.js:1-347; original script is inline in base FolderBreadcrumb.html.
Evidence: evidence/qa-gates/coverage-comparison.2026-10-18T01-27.md:15; self-review.2026-10-18T01-32.md:10.

Add deterministic execution tests of the actual embedded script through in-memory document/window/WebView/event seams. Cover recursive nonleaf/leaf rendering, independent toggle click/key propagation, selection payloads, selected descendant collapsed/reopen display, and existing message/key behavior. Use installed Node tooling without adding packages or a JavaScript engine.

Measure real baseline execution coverage for the original inline script at the merge base and final coverage for the extracted script, including all executable script code. Record numeric covered/valid counts, line/branch percentages, new/changed-code coverage and no changed-line regression. Do not invent 0% for absent measurements. New code must reach >=90% line coverage; changed-language aggregate/modified code >=85% lines and >=75% branches where measured. C# retains >=80% lines, >=75% branches, >=90% new code and no changed-line regression. Existing instrumentation/exclusions remain unchanged.

Verify node --check QuickFiler/Resources/FolderBreadcrumb.js and planner-selected deterministic Node execution/coverage commands. Store sanitized numeric baseline/final/comparison artifacts under evidence/baseline and evidence/qa-gates. Runner raw files belong in issue-997 SCRATCH. Tests create no temporary files or live services. Preserve runtime behavior unless an executable regression proves a defect.

## R2 — Full-branch Markdown whitespace

Severity: Minor, nonblocking
Remediability: autonomous
Remediability-Evidence: git diff --check origin/main...HEAD exited 2.

Normalize only changed Markdown evidence trailing spaces and final blank lines without altering recorded meaning. Exact inventory: lexicographically greatest evidence/qa-gates/reviewer-diff-whitespace.*.md.
Specific trailing spaces: evidence/regression-testing/fail-before-LegacySelectionChange_OutOfRootKnownChild_DoesNotMutateOrAcknowledge.2026-10-18T01-24.md lines 26, 28, 32, 34, 35, 41, 43 and 47. Other file/line EOF findings are in that inventory.
Verify the full delivered branch comparison, not only git diff --check of unstaged changes. After commit, git diff --check origin/main...HEAD must exit 0.

## R3 — Final-head CI

Severity: Blocking delivery gate
Remediability: awaiting_ci
Remediability-Evidence: canonical summary has no PR and unavailable CI.
Orchestrator owns required actionlint, format-check, build-analyzers, build-nullable, mstest-coverage, pester and hygiene checks at the exact final PR head. Check AC8 only after success, then commit/push/reverify the resulting head. No manual user step is required.

## R4 — Policy validator rejects missing metrics even for FAIL

Remediability: external_dependency for current schema behavior.
Canonical MCP policy-audit validation currently fails with missing numeric baseline/post-change/new-code coverage for JavaScript and corresponding comparison-line fields. Do not fabricate evidence to satisfy it.
Use exact comparison labels Baseline:, Post-change:, Change:, Disposition:, New/changed-code coverage:, Evidence:. Correct the C# comparison label syntax using existing numeric facts. After R1 provides actual JavaScript numbers, regenerate independent audits and require each canonical validator to pass.

## Do Not Do

- Do not edit policies, weaken coverage/AC/assertions, add exclusions or packages.
- Do not replace script execution coverage with C# text assertions or Node syntax.
- Do not widen into EFC, general UI redesign or unrelated refactoring.
- Do not repeat promotion/research/lifecycle work.
- Do not claim review/plan completion while required validators reject artifacts.

## Delegation Context Package

Primary requirements: this file.
Canonical context: artifacts/pr_context.summary.txt and artifacts/pr_context.appendix.txt.
Review artifacts: docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/audit/2026-10-18T01-35/policy-audit.2026-10-18T01-35.md; docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/audit/2026-10-18T01-35/code-review.2026-10-18T01-35.md; docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/audit/2026-10-18T01-35/feature-audit.2026-10-18T01-35.md.
Original feature plan: docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/plan.2026-10-17T20-42.md.
Requirements/mode: docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/spec.md and issue.md.
Local evidence: evidence/qa-gates/reviewer-local-qa.2026-10-18T01-42.md and coverage-comparison.2026-10-18T01-27.md.
The coordinating orchestrator owns the routed atomic-planner handoff and must supply the complete context package. The copied target is unapproved until authored, preflighted and canonically validated.
