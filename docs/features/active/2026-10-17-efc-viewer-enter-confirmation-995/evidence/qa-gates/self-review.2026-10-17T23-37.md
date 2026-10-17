# Issue 995 self-review

Timestamp: 2026-10-17T23-37
Command: Resolve three published MCP policy assets; inspect git diff from main merge base including working tree; evaluate every checklist item and asserting-test disposition
EXIT_CODE: 0
Output Summary: PASS: executor adversarial self-review. No blocking implementation or required-unverified checklist item identified. This is the executor self-review, not the coordinator's independent feature review or remote CI.

Base branch: main. Merge base: 438436893d2e7500057b8a4f871e23a702ab9b34. Head: 60478f5090212b275891dbae6c0e59d20a2a702d (bug/efc-viewer-enter-confirmation-995), plus current P5-T11 documentation/check-off. Recomputed complete branch inventory: 168 paths, including inherited62profiles,6skills,2PowerShell runtime files/configuration, EFC source/tests, feature/promoted documentation and canonical evidence. Complete git diff inspected in source/config/test groups, baseline/QA evidence reconciled individually. No workflow file changed. Copied upstream CI prose in execution-environment is not used as TaskMaster CI discovery; actual _pester.yml and wrappers remain authoritative.

Authoritative published MCP assets (all three resolved through resolve_policy_audit_template_asset; source fingerprints):
- policy-audit: MCP selector resolved authoritative bundled asset; SHA256=937960ba829d8e8e4460cc098f4d26103ca7420a72197f4f0b98cdd3e7d1ddfe
- code-review: MCP selector resolved authoritative bundled asset; SHA256=ceab031d45def161c0093632ce686195075ed03c04dc2f8b8bd13e17865ebaf4
- feature-audit: MCP selector resolved authoritative bundled asset; SHA256=478a7a107567f5572011761db023d64b97e6813cea9ca63e7f2f7b163888be09

Checklist extraction: every concrete status-bearing policy table row and checkbox/coverage-evidence bullet receives its own row with published line identity; repeated summary rows remain separate. Placeholder examples are not requirements. Code-review prompts/security rows and feature-audit source/baseline/evaluation/check-off/readiness prompts are individually represented. C# repository policies are additionally enforced through source/gate references because the generic policy template has no C# subsection. N/A always states scope or actual repository-policy reason; no missing required local gate is treated as N/A. 126 concrete policy table items; total checklist rows=203.

| Published item ID | Checklist item | Status | Verified evidence or N/A rationale |
|---|---|---|---|
| policy:L33 |  / Python | N/A | E1 |
| policy:L34 |  / PowerShell | PASS | E2 |
| policy:L35 |  / Bash | N/A | E1 |
| policy:L36 |  / JSON | PASS | E3 |
| policy:L87 | 1.1 Core Principles / Independence - Tests run in any order | PASS | E4 |
| policy:L88 | 1.1 Core Principles / Isolation - Each test targets single behavior | PASS | E5 |
| policy:L89 | 1.1 Core Principles / Fast Execution - Tests complete quickly | PASS | E6 |
| policy:L90 | 1.1 Core Principles / Determinism - Consistent results | PASS | E4 |
| policy:L91 | 1.1 Core Principles / Readability & Maintainability - Clear structure | PASS | E5 |
| policy:L97 | 1.2 Coverage and Scenarios / Baseline Coverage Documented | PASS | E7 |
| policy:L98 | 1.2 Coverage and Scenarios / No Coverage Regression | PASS | E7 |
| policy:L99 | 1.2 Coverage and Scenarios / New Code Coverage ≥90% | PASS | E7 |
| policy:L100 | 1.2 Coverage and Scenarios / Comprehensive Coverage | PASS | E7 |
| policy:L101 | 1.2 Coverage and Scenarios / Positive Flows - Valid inputs | PASS | E5 |
| policy:L102 | 1.2 Coverage and Scenarios / Negative Flows - Invalid inputs | PASS | E5 |
| policy:L103 | 1.2 Coverage and Scenarios / Edge Cases - Boundary conditions | PASS | E5 |
| policy:L104 | 1.2 Coverage and Scenarios / Error Handling - Error paths | PASS | E5 |
| policy:L105 | 1.2 Coverage and Scenarios / Concurrency - If applicable | PASS | E8 |
| policy:L106 | 1.2 Coverage and Scenarios / State Transitions - If applicable | PASS | E5 |
| policy:L118 | 1.3 Test Structure and Diagnostics / Clear Failure Messages | PASS | E5 |
| policy:L119 | 1.3 Test Structure and Diagnostics / Arrange-Act-Assert Pattern | PASS | E5 |
| policy:L120 | 1.3 Test Structure and Diagnostics / Document Intent | PASS | E5 |
| policy:L126 | 1.4 External Dependencies and Environment / Avoid External Dependencies | PASS | E5 |
| policy:L127 | 1.4 External Dependencies and Environment / Use Mocks/Stubs | PASS | E5 |
| policy:L128 | 1.4 External Dependencies and Environment / Environment Stability | PASS | E4 |
| policy:L134 | 1.5 Policy Audit Requirement / Pre-submission Review | PASS | E9 |
| policy:L144 | 2.1 Before Making Changes / Clarify the objective | PASS | E9 |
| policy:L145 | 2.1 Before Making Changes / Read existing change plans | PASS | E9 |
| policy:L146 | 2.1 Before Making Changes / Document the plan | PASS | E9 |
| policy:L152 | 2.2 Design Principles / Simplicity first | PASS | E10 |
| policy:L153 | 2.2 Design Principles / Reusability | PASS | E10 |
| policy:L154 | 2.2 Design Principles / Extensibility | PASS | E10 |
| policy:L155 | 2.2 Design Principles / Separation of concerns | PASS | E10 |
| policy:L161 | 2.3 Module & File Structure / Cohesive modules | PASS | E11 |
| policy:L162 | 2.3 Module & File Structure / Under 500 lines | PASS | E11 |
| policy:L163 | 2.3 Module & File Structure / Public vs internal | PASS | E10 |
| policy:L164 | 2.3 Module & File Structure / No circular dependencies | PASS | E10 |
| policy:L170 | 2.4 Naming, Docs, and Comments / Descriptive names | PASS | E10 |
| policy:L171 | 2.4 Naming, Docs, and Comments / Docs/docstrings | PASS | E10 |
| policy:L172 | 2.4 Naming, Docs, and Comments / Comment why, not what | PASS | E10 |
| policy:L178 | 2.5 After Making Changes - Toolchain Execution / 1. Formatting | PASS | E12 |
| policy:L179 | 2.5 After Making Changes - Toolchain Execution / 2. Linting | PASS | E12 |
| policy:L180 | 2.5 After Making Changes - Toolchain Execution / 3. Type checking | PASS | E12 |
| policy:L181 | 2.5 After Making Changes - Toolchain Execution / 4. Testing | PASS | E12 |
| policy:L182 | 2.5 After Making Changes - Toolchain Execution / Full toolchain loop | PASS | E12 |
| policy:L183 | 2.5 After Making Changes - Toolchain Execution / Explicit reporting | PASS | E12 |
| policy:L189 | 2.6 Summarize and Document / Summarize changes | PASS | E9 |
| policy:L190 | 2.6 Summarize and Document / Design choices explained | PASS | E9 |
| policy:L191 | 2.6 Summarize and Document / Update supporting documents | PASS | E9 |
| policy:L192 | 2.6 Summarize and Document / Provide next steps | PASS | E9 |
| policy:L208 | 3A.1 Tooling & Baseline / Formatting with Black | N/A | E1 |
| policy:L209 | 3A.1 Tooling & Baseline / Linting with Ruff | N/A | E1 |
| policy:L210 | 3A.1 Tooling & Baseline / Type checking with Pyright | N/A | E1 |
| policy:L211 | 3A.1 Tooling & Baseline / Testing with Pytest | N/A | E1 |
| policy:L217 | 3A.2 Python Design & Typing / Strong typing | N/A | E1 |
| policy:L218 | 3A.2 Python Design & Typing / Dataclasses for value objects | N/A | E1 |
| policy:L219 | 3A.2 Python Design & Typing / Protocols/ABCs for interfaces | N/A | E1 |
| policy:L220 | 3A.2 Python Design & Typing / Avoid utility classes | N/A | E1 |
| policy:L226 | 3A.3 Python Error Handling / Specific exceptions | N/A | E1 |
| policy:L227 | 3A.3 Python Error Handling / Logging over print | N/A | E1 |
| policy:L228 | 3A.3 Python Error Handling / Invariants at construction | N/A | E1 |
| policy:L238 | 3B.1 Tooling & Baseline / Formatting with Invoke-Formatter | PASS | E13 |
| policy:L239 | 3B.1 Tooling & Baseline / Linting with PSScriptAnalyzer | PASS | E13 |
| policy:L240 | 3B.1 Tooling & Baseline / Fix all findings | PASS | E13 |
| policy:L241 | 3B.1 Tooling & Baseline / PowerShell 5.1 & 7.6+ compatible | N/A | E14 |
| policy:L247 | 3B.2 PowerShell Design & Safety / Advanced functions | PASS | E15 |
| policy:L248 | 3B.2 PowerShell Design & Safety / Parameter validation | PASS | E16 |
| policy:L249 | 3B.2 PowerShell Design & Safety / Avoid global state | PASS | E4 |
| policy:L250 | 3B.2 PowerShell Design & Safety / Error handling | PASS | E17 |
| policy:L256 | 3B.3 Structure, Naming, and Comments / Cohesive and under 500 lines | PASS | E11 |
| policy:L257 | 3B.3 Structure, Naming, and Comments / Approved verbs | PASS | E18 |
| policy:L258 | 3B.3 Structure, Naming, and Comments / Comment why | PASS | E18 |
| policy:L264 | 3B.4 Running the Toolchain / Step 1: Format | PASS | E13 |
| policy:L265 | 3B.4 Running the Toolchain / Step 2: Analyze | PASS | E13 |
| policy:L266 | 3B.4 Running the Toolchain / Step 3: Type check | N/A | E19 |
| policy:L267 | 3B.4 Running the Toolchain / Step 4: Test | PASS | E13 |
| policy:L268 | 3B.4 Running the Toolchain / Rerun loop if needed | PASS | E13 |
| policy:L278 | 3C.1 Tooling & Baseline / Formatting with shfmt | N/A | E1 |
| policy:L279 | 3C.1 Tooling & Baseline / Linting with shellcheck | N/A | E1 |
| policy:L280 | 3C.1 Tooling & Baseline / Testing with bats | N/A | E1 |
| policy:L286 | 3C.2 Bash Script Design / Portable shebang | N/A | E1 |
| policy:L287 | 3C.2 Bash Script Design / Error handling | N/A | E1 |
| policy:L288 | 3C.2 Bash Script Design / Under 500 lines | N/A | E1 |
| policy:L298 | 3D.1 JSON Tooling / Formatting with jq | N/A | E20 |
| policy:L299 | 3D.1 JSON Tooling / Schema validation | PASS | E21 |
| policy:L300 | 3D.1 JSON Tooling / Required $schema | N/A | E20 |
| policy:L306 | 3D.2 JSON Structure / Strict JSON only | PASS | E21 |
| policy:L307 | 3D.2 JSON Structure / Deterministic key order | N/A | E20 |
| policy:L323 | 4A.1 Framework and Scope / Use Pytest | N/A | E1 |
| policy:L324 | 4A.1 Framework and Scope / Coverage expectation | N/A | E1 |
| policy:L330 | 4A.2 Test Style and Structure / Focused unit tests | N/A | E1 |
| policy:L331 | 4A.2 Test Style and Structure / Mocking sparingly | N/A | E1 |
| policy:L332 | 4A.2 Test Style and Structure / Organization | N/A | E1 |
| policy:L338 | 4A.3 Naming and Readability / Naming conventions | N/A | E1 |
| policy:L339 | 4A.3 Naming and Readability / Docstrings/comments | N/A | E1 |
| policy:L345 | 4A.4 Running the Toolchain / Use Pytest | N/A | E1 |
| policy:L346 | 4A.4 Running the Toolchain / No Alternative Test Runners | N/A | E1 |
| policy:L356 | 4B.1 Framework and Scope / Use Pester v5.x | PASS | E13 |
| policy:L357 | 4B.1 Framework and Scope / Use PoshQC Configuration | PASS | E13 |
| policy:L358 | 4B.1 Framework and Scope / PowerShell 5.1 & 7.6+ Compatible | N/A | E14 |
| policy:L364 | 4B.2 Test Style and Structure / Focused Unit Tests | PASS | E5 |
| policy:L365 | 4B.2 Test Style and Structure / Test Behavior Over Implementation | PASS | E5 |
| policy:L366 | 4B.2 Test Style and Structure / Mocking Used Sparingly | PASS | E5 |
| policy:L367 | 4B.2 Test Style and Structure / Organization | PASS | E22 |
| policy:L373 | 4B.3 Naming and Readability / File Naming - *.Tests.ps1 | PASS | E5 |
| policy:L374 | 4B.3 Naming and Readability / Describe/Context/It Structure | PASS | E5 |
| policy:L375 | 4B.3 Naming and Readability / Logical Grouping | PASS | E5 |
| policy:L376 | 4B.3 Naming and Readability / Docstrings/Comments | PASS | E5 |
| policy:L382 | 4B.4 Running the Toolchain / Use PoshQCTest Command | PASS | E13 |
| policy:L383 | 4B.4 Running the Toolchain / No Alternative Test Runners | PASS | E13 |
| policy:L420 | 6. Test Execution Metrics / Total Tests | PASS | E23 |
| policy:L421 | 6. Test Execution Metrics / Tests Passed | PASS | E23 |
| policy:L422 | 6. Test Execution Metrics / Tests Failed | PASS | E23 |
| policy:L423 | 6. Test Execution Metrics / Execution Time | PASS | E6 |
| policy:L424 | 6. Test Execution Metrics / Average Time per Test | PASS | E6 |
| policy:L425 | 6. Test Execution Metrics / Discovery Time | N/A | E24 |
| policy:L426 | 6. Test Execution Metrics / Functions/Classes Tested | PASS | E25 |
| policy:L427 | 6. Test Execution Metrics / Test File Size | PASS | E11 |
| policy:L428 | 6. Test Execution Metrics / Code Coverage (if applicable) | PASS | E7 |
| policy:L438 | 7. Code Quality Checks / Black Formatting | N/A | E1 |
| policy:L439 | 7. Code Quality Checks / Ruff Linting | N/A | E1 |
| policy:L440 | 7. Code Quality Checks / Pyright Type Checking | N/A | E1 |
| policy:L441 | 7. Code Quality Checks / Pytest Tests | N/A | E1 |
| policy:L447 | 7. Code Quality Checks / Invoke-Formatter | PASS | E13 |
| policy:L448 | 7. Code Quality Checks / PSScriptAnalyzer | PASS | E13 |
| policy:L449 | 7. Code Quality Checks / Pester Tests | PASS | E13 |
| policy:L42 | TypeScript baseline coverage artifact: [path or `N/A - out of scope`] | N/A | E26 |
| policy:L43 | TypeScript post-change coverage artifact: [path or `N/A - out of scope`] | N/A | E26 |
| policy:L44 | PowerShell baseline coverage artifact: [path or `N/A - out of scope`] | PASS | E7 |
| policy:L45 | PowerShell post-change coverage artifact: [path or `N/A - out of scope`] | PASS | E7 |
| policy:L46 | Per-language comparison summary: [section reference or artifact path] | PASS | E7 |
| policy:L61 | `general-code-change.instructions.md` (if applicable) | PASS | E27 |
| policy:L62 | `general-unit-test.instructions.md` (if testing) | PASS | E27 |
| policy:L65 | `python-code-change.instructions.md` + `python-unit-test.instructions.md` | N/A | E26 |
| policy:L66 | `powershell-code-change.instructions.md` + `powershell-unit-test.instructions.md` | PASS | E27 |
| policy:L67 | Bash: shfmt + shellcheck + bats (if applicable) | N/A | E26 |
| policy:L68 | JSON: format_json + validate_json (if applicable) | N/A | E28 |
| policy:L75 | All temporary/one-time scripts created during development have been deleted | N/A | E29 |
| policy:L76 | Any ongoing tooling scripts are fully tested and compliant with repo policies | PASS | E30 |
| policy:L525 | Before Making Changes: [Brief status] | PASS | E27 |
| policy:L526 | Design Principles: [Brief status] | PASS | E27 |
| policy:L527 | Module & File Structure: [Brief status] | PASS | E11 |
| policy:L528 | Naming, Docs, Comments: [Brief status] | PASS | E27 |
| policy:L529 | Toolchain Execution: [Brief status] | PASS | E27 |
| policy:L530 | Summarize & Document: [Brief status] | PASS | E27 |
| policy:L535 | Tooling & Baseline: [Brief status] | N/A | E26 |
| policy:L536 | Python Design & Typing: [Brief status] | N/A | E26 |
| policy:L537 | Error Handling: [Brief status] | N/A | E26 |
| policy:L540 | Tooling & Baseline: [Brief status] | PASS | E27 |
| policy:L541 | PowerShell Design & Safety: [Brief status] | PASS | E27 |
| policy:L542 | Structure & Naming: [Brief status] | PASS | E11 |
| policy:L543 | Toolchain: [Brief status] | PASS | E27 |
| policy:L546 | Core Principles: [Brief status] | PASS | E31 |
| policy:L547 | Coverage & Scenarios: [Brief status] | PASS | E7 |
| policy:L548 | Test Structure: [Brief status] | PASS | E31 |
| policy:L549 | External Dependencies: [Brief status] | PASS | E31 |
| policy:L550 | Policy Audit: [Brief status] | PASS | E27 |
| policy:L555 | Framework & Scope: [Brief status] | N/A | E26 |
| policy:L556 | Test Style & Structure: [Brief status] | N/A | E26 |
| policy:L557 | Naming & Readability: [Brief status] | N/A | E26 |
| policy:L558 | Toolchain: [Brief status] | N/A | E26 |
| policy:L561 | Framework & Scope: [Brief status] | PASS | E31 |
| policy:L562 | Test Style & Structure: [Brief status] | PASS | E31 |
| policy:L563 | Naming & Readability: [Brief status] | PASS | E31 |
| policy:L564 | Toolchain: [Brief status] | PASS | E27 |
| policy:L573 | [N]/[M] tests passing ([X]%) | PASS | E32 |
| policy:L574 | [N]/[M] functions/classes tested ([X]%) | PASS | E32 |
| policy:L575 | [X]% line coverage | PASS | E7 |
| policy:L576 | Proper file organization: [describe] | PASS | E11 |
| policy:L577 | All code quality checks passing | PASS | E27 |
| policy:L578 | Test execution time: [X] seconds ([fast/slow]) | PASS | E32 |
| policy:coverage-verdict | Numeric baseline/post/change evidence and fail-closed rule | PASS | E7 |
| policy:all-evidence | Required baseline/QA/comparison artifacts exist; no synthetic evidence | PASS | E33 |
| policy:gaps | Identified gaps / approved exceptions / removed tests | PASS | E34 |
| policy:inventory | Commit/file/test inventory and Appendix B commands | PASS | E35 |
| code:summary | Executive summary, implementation delta and readiness | PASS | E10 |
| code:risks | Top three residual risks | PASS | E36 |
| code:findings | Severity/file/location/finding/recommendation/rationale/evidence table | PASS | E37 |
| code:ps-api | PowerShell API/safety/error handling | PASS | E38 |
| code:cs-api | C# type safety/public API/lifecycle/error handling | PASS | E39 |
| code:determinism | Test quality / determinism | PASS | E4 |
| code:isolation | Test quality / isolation | PASS | E40 |
| code:speed | Test quality / speed | PASS | E41 |
| code:diagnostics | Test quality / diagnostics | PASS | E42 |
| code:no-secrets | Security / no secrets | PASS | E43 |
| code:subprocess | Security / no unsafe command construction | PASS | E44 |
| code:inputs | Security / boundary input validation | PASS | E40 |
| code:errors | Security / explicit errors | PASS | E45 |
| code:paths | Security / safe configuration/path handling | PASS | E46 |
| code:research | Research log | PASS | E47 |
| feature:scope | Scope/base/head/merge-base/requirements/work-mode | PASS | E48 |
| feature:inventory | Faithful AC inventory/evaluation/check-off/summary | PASS | E49 |
| feature:followup | Readiness and follow-up | PASS | E50 |
| code:python | Python implementation audit | N/A | E51 |
| code:typescript | TypeScript implementation audit | N/A | E51 |
| feature:AC1 | One plain, nonrepeated Enter press with a valid highlighted current destination requests the existing guarded OK action exactly once, and a row-selection message alone does not request OK. | PASS | E52 |
| feature:AC2 | Missing, unknown, banner, stale, or invalid destinations never file an email through confirmation; the existing OK validation remains effective. | PASS | E52 |
| feature:AC3 | Confirmation after ancestor-segment or rendered-child activation retains that activated destination, and list rebinding prevents stale selection confirmation. | PASS | E52 |
| feature:AC4 | Modified Enter and repeated keydown events do not request confirmation; existing arrow navigation, focus transfer, row selection, breadcrumb activation, and cancellation behavior remain unchanged. | PASS | E52 |
| feature:AC5 | A deterministic Enter regression fails before the production fix and passes afterward; codec, router, and controller boundary tests cover the positive, invalid, stale-selection, retained-target, and exception scenarios with no live Outlook/WebView or temporary-file dependencies. | PASS | E52 |
| feature:AC6 | The complete inherited model-workflow repair has consistent profile/configuration/resolver values, preserved attestation rejection behavior, and compatible MCP package version `1.1.21`; deterministic routing tests and actual MCP protocol validation establish those results without bypassing required gates. | PASS | E52 |
| feature:AC7 | The final C# and PowerShell formatting, analysis, type/compatibility checks, tests, and required coverage comparisons pass with canonical evidence for the entire changed-language scope. | PASS | E52 |
| feature:AC8 | Issue, spec, promoted-record links, and supporting workflow references match the final behavior and `full-bug` work mode; all required evidence is stored under this feature's canonical evidence directories. | PASS | E52 |

Evidence key (each checklist row refers to its specific inspected evidence or N/A rationale):
- E1: No Python or Bash source/tests changed; generic published-template language tools do not apply to this TaskMaster C#/PowerShell scope.
- E2: Required native MCP format/analyze/test receipts at23:22/23:23/23:25; direct format/analyze/compatibility at23:23; direct Pester5.6.1 at23:26:513 passed,0failed,0skipped. Scoped findings0; broad existing13 (10warning3info), zero errors/parse errors, delta0; broad actual EXIT1 remains disclosed. evidence/qa-gates/coverage-comparison.2026-10-17T23-27.md: C# 86.0212% -> 86.0331%; expanded PS 84.0257% -> 94.9465%; default PS unchanged 95.1042%; three new C# methods100%; PS wrapper90%.
- E3: Existing orchestration-routing.json changes only model string values; ConvertFrom-Json and actual MCP topology/model validation pass. Configuration has no executable coverage denominator.
- E4: Three seeded file-order runs995,1995,2995 pass50/50; fresh actual CI folders513/513 (53.08s). Dynamic attestation module removed AfterAll; Moq instances per C# test; SynchronizationContext restored in finally. No mutable external authority fixtures.
- E5: Inspected renderer/codec/new router/controller tests and final raw reports: plain Enter discriminator, invalid current identities, ancestor/child retention, rebind, cleanup/reconfiguration and existing fault boundary. No live WebView/Outlook or temporary fixtures. 62 changed profiles parse and match config/resolver; four bands, C3 overlays, forced epics and model_unavailable are tested. Real hook uses optional injected I/O with production defaults unchanged; exclusive access/disposal/single consumption and markers/exit are tested. Published MCP1.1.21 protocol proofs at evidence/other/*23-31.md.
- E6: Fresh full Pester CI-shape run53.08s/513 cases (103.47ms average including suite overhead); coverage-bearing JUnit56.830s. New tests have no timers, processes, network calls or filesystem fixtures. C# TRX durations are retained under SCRATCH.
- E7: evidence/qa-gates/coverage-comparison.2026-10-17T23-27.md: C# 86.0212% -> 86.0331%; expanded PS 84.0257% -> 94.9465%; default PS unchanged 95.1042%; three new C# methods100%; PS wrapper90%.
- E8: Existing router queue tests remain passing; new confirmation adds no thread/queue. Hook authority test verifies exclusive None sharing and single-use receipt transition using memory streams; no concurrency contract weakened.
- E9: issue.md/spec.md/promoted intake identify995/full-bug; sole spec AC8/8; evidence/qa-gates/p14-acceptance-criteria-checkoff.2026-10-17T23-34.md; plan37 stable IDs. Coordinator owns independent review/commit/PR/exact-head CI. Phase0 instructions/branch baseline and approved plan precede execution; red test precedes EFC product edits.
- E10: Full merge-base diff inspected: narrow codec/router/controller additions, existing OK reused, internal lifecycle seam, named event detach, typed boundaries; no dependency, workflow, suppression or persisted product schema changes.
- E11: All changed executable source/test/reusable script files <=459 lines, per branch-contracts23:30. Existing declarative QuickFiler.Test.csproj571 lines (569base+2Compile includes) is metadata; Markdown is exempt. No executable-file exception.
- E12: Final ordered CSharpier check23:13; analyzer and warnings-as-errors Rebuild23:14:EXIT0,0warnings/0errors; full MSTest coverage23:15:7,471/7,471 passes. Canonical artifacts under evidence/qa-gates/. Required native MCP format/analyze/test receipts at23:22/23:23/23:25; direct format/analyze/compatibility at23:23; direct Pester5.6.1 at23:26:513 passed,0failed,0skipped. Scoped findings0; broad existing13 (10warning3info), zero errors/parse errors, delta0; broad actual EXIT1 remains disclosed. Failed analysis/coverage iterations restarted affected language from formatting; final ordered passes pass the approved scope.
- E13: Required native MCP format/analyze/test receipts at23:22/23:23/23:25; direct format/analyze/compatibility at23:23; direct Pester5.6.1 at23:26:513 passed,0failed,0skipped. Scoped findings0; broad existing13 (10warning3info), zero errors/parse errors, delta0; broad actual EXIT1 remains disclosed. TaskMaster lacks copied upstream PoshQC module/config; explicit actual scan folders and bundled settings are used. Existing debt preserved by authorized P5-T6 clarification; every changed-file finding fixed without suppression.
- E14: The template generic 5.1/7.6 requirement is superseded by actual TaskMaster policy PowerShell7+; its bundled-settings PS7 compatibility and parser checks PASS at23:23. No claim of a Windows PowerShell5.1 execution.
- E15: Real runtime functions retain CmdletBinding/named parameters; command wrapper has OutputType. Test-local pure builders do not need a production advanced-function API.
- E16: Resolver validates band/context/ceiling/agent/model availability; hook rejects missing identity, malformed required input and invalid receipt/profile/HEAD. Negative tests verify all boundaries.
- E17: Wrapper returns structured stderr/exit2 on boundary failure; rejection markers retain exit0 transport contract; exclusive stream operations use finally disposal. Codec uses existing BreadcrumbMessageException/log4net; OK uses existing fault sink.
- E18: Scoped bundled analyzer0findings confirms approved verbs/naming; test builders use Get-* pure naming. Comments explain retained-target confirmation, static JavaScript limits and unused boundary-parameter discards. No suppression.
- E19: PowerShell has no separate static type checker; parser and PS7 compatibility checks pass. C# compiler/nullable checking is separately required and passed.
- E20: Template references upstream Poetry/jq governance that this TaskMaster checkout does not own; no $schema or sorted-key contract exists for this existing configuration. Existing key order/schema surface retained; strict parser and real MCP validation pass.
- E21: Strict orchestration-routing.json parses; actual published MCP validates topology/model policy against live persisted receipts. Only existing model string values changed; no JSON5 syntax/schema change.
- E22: New *.Tests.ps1 files are under actual CI-scanned tests/scripts/vscode, per approved plan and _pester.yml; they test .codex runtime files absent from default coverage, which expanded collector explicitly adds. Generic upstream mirror-path convention is adapted to actual discovery.
- E23: C#7,471/7,471 pass; PS513/513 pass,0failed/0skipped. Baselines7,450 and463; new PS cases17resolver+33attestation=50. No removed/ignored tests.
- E24: Discovery timing is informational, not a repository acceptance gate. Actual Pester full suite total53.08s and JUnit56.830s retained; no fabricated isolated discovery measurement.
- E25: All three new C# methods exercised100% line/branch; new PS wrapper36/40lines90%; resolver68/68; hook187/204. Full class totals are not substituted for new-unit measurements.
- E26: No TypeScript/Python/Bash source or tests changed; generic language row is outside this TaskMaster scope.
- E27: issue.md/spec.md/promoted intake identify995/full-bug; sole spec AC8/8; evidence/qa-gates/p14-acceptance-criteria-checkoff.2026-10-17T23-34.md; plan37 stable IDs. Coordinator owns independent review/commit/PR/exact-head CI. Final ordered CSharpier check23:13; analyzer and warnings-as-errors Rebuild23:14:EXIT0,0warnings/0errors; full MSTest coverage23:15:7,471/7,471 passes. Canonical artifacts under evidence/qa-gates/. Required native MCP format/analyze/test receipts at23:22/23:23/23:25; direct format/analyze/compatibility at23:23; direct Pester5.6.1 at23:26:513 passed,0failed,0skipped. Scoped findings0; broad existing13 (10warning3info), zero errors/parse errors, delta0; broad actual EXIT1 remains disclosed.
- E28: Generic upstream format_json/validate_json commands are absent from TaskMaster; actual strict JSON parse and published MCP topology/model validation pass, without introducing schema or ordering changes.
- E29: No temporary one-time script is tracked or left inside checkout. Issue-scoped diagnostic helpers/raw reports are retained outside repository under SCRATCH for audit reproducibility, not claimed deleted.
- E30: Only planned runtime seams/new tests are retained as repository tooling; both runtimes have scoped clean analysis and measured coverage. Throwaway helpers live outside checkout.
- E31: Three seeded file-order runs995,1995,2995 pass50/50; fresh actual CI folders513/513 (53.08s). Dynamic attestation module removed AfterAll; Moq instances per C# test; SynchronizationContext restored in finally. No mutable external authority fixtures. Inspected renderer/codec/new router/controller tests and final raw reports: plain Enter discriminator, invalid current identities, ancestor/child retention, rebind, cleanup/reconfiguration and existing fault boundary. No live WebView/Outlook or temporary fixtures.
- E32: All7,471 C# and513 PS tests pass; new C# units100% and PS wrapper90%. Fresh PS CI-shape53.08s. Complete inventories and raw results retained under SCRATCH; no removed/skipped test.
- E33: P0 evidence exists on disk; latest per-command artifacts reviewed. Red regression declares expected1, cached MCP failure remains EXIT1. No missing required automated metric.
- E34: No blocking gap or new policy exception. Constant-only JS asset0/0 N/A is instrumentation fact; preserved broad analyzer debt is explicit approved scope. No removed/skipped planned tests.
- E35: Full changed-file inventory below; commits fffae7986 and60478f5. Test inventory from real source below; exact final commands/bootstrap recorded.
- E36: Static JavaScript checks do not execute Chromium; actual deployed release unspecified; remote exact-head CI remains coordinator delivery gate. None replaces or blocks required deterministic local evidence.
- E37: No Blocker or Major finding in complete branch self-review. Known preexisting diagnostics and static-JS limitation disclosed with exact QA references; independent reviewer remains separate.
- E38: 62 changed profiles parse and match config/resolver; four bands, C3 overlays, forced epics and model_unavailable are tested. Real hook uses optional injected I/O with production defaults unchanged; exclusive access/disposal/single consumption and markers/exit are tested. Published MCP1.1.21 protocol proofs at evidence/other/*23-31.md.
- E39: Final ordered CSharpier check23:13; analyzer and warnings-as-errors Rebuild23:14:EXIT0,0warnings/0errors; full MSTest coverage23:15:7,471/7,471 passes. Canonical artifacts under evidence/qa-gates/.
- E40: Inspected renderer/codec/new router/controller tests and final raw reports: plain Enter discriminator, invalid current identities, ancestor/child retention, rebind, cleanup/reconfiguration and existing fault boundary. No live WebView/Outlook or temporary fixtures.
- E41: Fresh full Pester CI shape513cases53.08s; new in-memory tests do not introduce external latency. Raw C# TRX retained.
- E42: FluentAssertions/Should named cases, parameterized codec payloads and real red assertion missing rowConfirmed provide actionable failures.
- E43: Full source/config diff has model identifiers, tests and paths only; repository hygiene after proof redaction0findings. No credential/config secret introduced.
- E44: Existing default ReadHead uses structured call operator git -C $Root with separate arguments; injected delegates do not eval strings; no Invoke-Expression/new executable command added.
- E45: Codec rejects malformed confirmation through existing exception/logging; existing OK fault boundary retained; runtime wrapper returns stderr/exit2 on failure with tested markers.
- E46: Existing authority path helpers and exclusive file modes remain defaults; tests inject memory streams. Root output coverage confined to verified coverage directory, copied to SCRATCH before targeted removal. Host roots redacted from committed proof.
- E47: Repository source/research artifacts and actual published MCP assets are authoritative; no external web claim needed for implementation. No invented deployed-version/browser-run claim.
- E48: Explicit main merge base and HEAD below. issue995 suffix selects active feature; issue.md marker full-bug selects spec.md only.
- E49: issue.md/spec.md/promoted intake identify995/full-bug; sole spec AC8/8; evidence/qa-gates/p14-acceptance-criteria-checkoff.2026-10-17T23-34.md; plan37 stable IDs. Coordinator owns independent review/commit/PR/exact-head CI.
- E50: Local execution is complete; coordinator performs final partial commit, independent review, PR creation and exact-head CI. No required local criterion remains unverified.
- E51: No source/tests for this language changed.
- E52: Individually mapped to specific behavior, commands and canonical evidence in p14-acceptance-criteria-checkoff.2026-10-17T23-34.md; exact wording retained in spec.md.

Adversarial checks: confirmation never reselects the row or replaces retained target; invalid/current/nonbanner checks precede event; codec accepts new discriminator only with nonempty rowId; plain/repeat/modifier guard is located in the Enter branch; row clicks remain rowSelected; cleanup and replacement detach the exact handler; default helper invokes ButtonOK_Click, not a second filing pipeline. Hook defaults preserve exclusive sharing, durable flush, output markers and exit behavior; optional injected collaborators are solely testable boundaries. Historical receipts are not rewritten. Hash/path rejection evidence is helper-specific because the current hook does not supply optional expected path/hash arguments.

Findings table:

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | Unchanged PowerShell scripts | Broad analyzer | Existing13findings:10warnings3informational;0errors/parse errors; broad EXIT1 | Track separately; preserve zero new diagnostics | Authorized P5-T6 scope | powershell-analyze-repository-comparison.2026-10-17T23-23.md |
| Info | BreadcrumbDocumentAssets.cs | BridgeJs | Text assertions do not execute Chromium; compiler-only constant has0/0 sequence points | Preserve honest scope; automated host behavior is separately tested | No synthetic browser/coverage claim | red/green and coverage-comparison evidence |

Asserting-test disposition search: rg -l over QuickFiler.Test, UtilitiesCS.Test and tests for rowSelected/rowConfirmed, preserved renderer/cleanup/error names, resolver/hook/profile helpers. No remaining old Issue737 Enter assertion or additional workflow asserting suite found beyond listed cases. Every additional matching suite below is preserved unless explicitly updated or new.
- QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.Activation.cs: preserved unchanged; selection, activation, queue, navigation, focus or controller lifecycle/error contract remains compatible and full suite PASS.
- QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs: preserved unchanged; selection, activation, queue, navigation, focus or controller lifecycle/error contract remains compatible and full suite PASS.
- QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue614Tests.cs: preserved unchanged; selection, activation, queue, navigation, focus or controller lifecycle/error contract remains compatible and full suite PASS.
- QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue637Tests.cs: preserved unchanged; selection, activation, queue, navigation, focus or controller lifecycle/error contract remains compatible and full suite PASS.
- QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue792Tests.cs: preserved unchanged; selection, activation, queue, navigation, focus or controller lifecycle/error contract remains compatible and full suite PASS.
- QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue995Tests.cs: added behavioral coverage; final full suite PASS.
- QuickFiler.Test\Controllers\BreadcrumbBridgeRouterQueueTests.cs: preserved unchanged; selection, activation, queue, navigation, focus or controller lifecycle/error contract remains compatible and full suite PASS.
- QuickFiler.Test\Controllers\BreadcrumbBridgeRouterQueueTests.Part2.cs: preserved unchanged; selection, activation, queue, navigation, focus or controller lifecycle/error contract remains compatible and full suite PASS.
- QuickFiler.Test\Controllers\BreadcrumbBridgeRouterScoreJoinTests.cs: preserved unchanged; selection, activation, queue, navigation, focus or controller lifecycle/error contract remains compatible and full suite PASS.
- QuickFiler.Test\Controllers\BreadcrumbBridgeRouterTests.Selection.cs: preserved unchanged; selection, activation, queue, navigation, focus or controller lifecycle/error contract remains compatible and full suite PASS.
- QuickFiler.Test\Controllers\EfcFormControllerIssue995Tests.cs: added behavioral coverage; final full suite PASS.
- QuickFiler.Test\Controllers\EfcFormControllerTests.cs: preserved unchanged; selection, activation, queue, navigation, focus or controller lifecycle/error contract remains compatible and full suite PASS.
- tests\scripts\vscode\CodexDeployment.Tests.ps1: added behavioral coverage; final full suite PASS.
- tests\scripts\vscode\CodexSubagentRoutingAttestation.Tests.ps1: added behavioral coverage; final full suite PASS.
- UtilitiesCS.Test\OutlookObjects\Folder\BreadcrumbHtmlRendererTests.cs: only defect-pinning Enter expectation updated from rowSelected to rowConfirmed and guards; scrolling and all other tests retained. Static-text limitation explicit.
- UtilitiesCS.Test\OutlookObjects\Folder\BreadcrumbMessageCodecTests.cs: additive valid/invalid confirmation cases; existing assertions retained.

Complete touched-test name inventory (parameterized names retain data cases in source/raw result reports):
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void Issue439ResolvedLineageUsesUnicodeArrowSeparators(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void Issue439ActiveAncestorChildrenAndEmbeddedBridgeUseTypedStoppedActivation(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRowFragment_EveryRowKind_EmitsTrailingPctFlexItem(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRowFragment_CollapsedRow_StillEmitsTrailingPercent(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRowFragment_LeafWithSubfolders_EmitsPlusWhenCollapsedMinusWhenExpanded(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRowFragment_LeafWithoutSubfolders_EmitsNoAffordance(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRowFragment_HostileFolderNames_AreHtmlEncoded(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRowFragment_BannerRow_IsNonInteractive(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRowFragment_TrashPseudoRow_IsSelectableWithoutAffordance(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderDocument_DarkVersusLight_EmbedsMatchingThemeBlock(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRowFragment_CollapsedState_RendersReExpandPlusAtTerminalSegment(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRows_EmptyRowList_ProducesEmptyFragmentAndValidDocument(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRows_WithSelectedRowId_MarksOnlyThatRowSelected(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs: public void RenderRowFragment_ExpandedLeaf_RendersEncodedChildren(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_SegmentDoubleClick_RoundTripsAllFields(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_LeafExpandToggle_RoundTrips(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_ArrowKey_RoundTripsKey(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_RowSelected_RoundTrips(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_RowConfirmed_RoundTrips(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_RowConfirmed_InvalidEnvelopeIsRejected(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_SegmentActivateAndRenderedChildActivate_RoundTripTypedIndexes(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void SerializeOutbound_RenderMessage_EmitsCamelCaseDiscriminatedJson(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void SerializeOutbound_RenderMessage_OmitsNullRowIdForFullDocument(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void SerializeOutbound_SubfolderResult_CarriesRequestIdCorrelationAndChildren(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void SerializeOutbound_FocusSearch_EmitsTypeOnly(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void SerializeOutbound_WithNullMessage_ThrowsArgumentNullException(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_WithInvalidJson_ThrowsBreadcrumbMessageException(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_WithUnknownType_ThrowsBreadcrumbMessageException(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_WithMissingRowId_ThrowsBreadcrumbMessageException(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_WithWrongTypedRowId_ThrowsBreadcrumbMessageException(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_WithWrongTypedSegmentIndex_ThrowsBreadcrumbMessageException(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_SegmentDoubleClickWithoutIndex_ThrowsBreadcrumbMessageException(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_TypedActivationWithoutRequiredIndex_ThrowsBreadcrumbMessageException(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_ArrowKeyWithoutKey_ThrowsBreadcrumbMessageException(
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs: public void DeserializeInbound_WithEmptyPayload_ThrowsBreadcrumbMessageException(
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs: public void Setup(
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs: public async Task ValidSelectedRow_ConfirmsExactlyOnce(
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs: public async Task RowSelected_DoesNotConfirm(
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs: public async Task MissingUnknownBannerOrMismatchedRow_DoesNotConfirm(
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs: public async Task InvalidDestination_DoesNotConfirm(
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs: public async Task RebindThenConfirm_DoesNotUseStaleSelection(
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs: public async Task ConfirmAfterSegmentActivation_PreservesTarget(
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs: public async Task ConfirmAfterChildActivation_PreservesTarget(
- QuickFiler.Test/Controllers/EfcFormControllerIssue995Tests.cs: public void ConfirmationWiring_NullRouter_IsRejected(
- QuickFiler.Test/Controllers/EfcFormControllerIssue995Tests.cs: public async Task ConfirmationWiring_RequestsOkOnceAndDoesNotDuplicateSubscription(
- QuickFiler.Test/Controllers/EfcFormControllerIssue995Tests.cs: public async Task CleanupAndReconfiguration_DetachOldRouterConfirmation(
- QuickFiler.Test/Controllers/EfcFormControllerIssue995Tests.cs: public async Task DefaultConfirmationWiring_UsesExistingOkFaultBoundaryOnce(
- tests/scripts/vscode/CodexDeployment.Tests.ps1: It 'resolves <Band> to its configured model and effort'
- tests/scripts/vscode/CodexDeployment.Tests.ps1: It 'elevates C3 for <Context> and ceiling <Ceiling>'
- tests/scripts/vscode/CodexDeployment.Tests.ps1: It 'forces <Persona> to Astra ultra regardless of assessed band'
- tests/scripts/vscode/CodexDeployment.Tests.ps1: It 'normalizes the feature-review alias without changing its logical identity'
- tests/scripts/vscode/CodexDeployment.Tests.ps1: It 'rejects invalid band <Value>'
- tests/scripts/vscode/CodexDeployment.Tests.ps1: It 'rejects an invalid ceiling and a ceiling below the assessment'
- tests/scripts/vscode/CodexDeployment.Tests.ps1: It 'rejects unsupported execution context and logical agent'
- tests/scripts/vscode/CodexDeployment.Tests.ps1: It 'fails closed when the required model is unavailable'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'rejects empty/malformed required input but tolerates optional absence'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'accepts the native model and records the profile identity'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'rejects an incorrect actual model or profile effort'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'preserves explicit helper path/hash rejection separately from hook routing'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'fails closed for absent receipts, incomplete receipt fields or profile read failures'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'rejects missing startup identity and failed launch authority'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'handles an unrouted agent and hashes fallback identity deterministically'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'accepts valid root authority and preserves forced model identity'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'rejects invalid receipt field <Field>'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'rejects null, extra fields, expired and overly long receipt lifetime'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'validates epic-run reference and kickoff path'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'consumes a root receipt once with exclusive stream access and durable flush'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'writes JSON through exclusive creation and disposes its stream'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'returns success without output for correctly routed input'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'resolves the default repository root through mocked authority boundaries'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'returns the routing marker for malformed optional checkpoint data'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'returns the provenance marker when epic root authority is absent'
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1: It 'returns exit two for invalid HEAD or malformed startup input'

Complete changed-file inventory (base to committed HEAD plus current changes, recomputed):
- .agents/skills/codex-model-routing/SKILL.md
- .agents/skills/epic-orchestrate/SKILL.md
- .agents/skills/epic-plan/SKILL.md
- .agents/skills/execution-environment/SKILL.md
- .agents/skills/orchestrate/SKILL.md
- .agents/skills/orchestrator-workflow/SKILL.md
- .codex/agents/atomic-executor-c1.toml
- .codex/agents/atomic-executor-c2.toml
- .codex/agents/atomic-executor-c3-elevated.toml
- .codex/agents/atomic-executor-c3.toml
- .codex/agents/atomic-executor-c4.toml
- .codex/agents/atomic-executor.toml
- .codex/agents/atomic-planner-c1.toml
- .codex/agents/atomic-planner-c2.toml
- .codex/agents/atomic-planner-c3-elevated.toml
- .codex/agents/atomic-planner-c3.toml
- .codex/agents/atomic-planner-c4.toml
- .codex/agents/atomic-planner.toml
- .codex/agents/commit-steward-c1.toml
- .codex/agents/commit-steward-c2.toml
- .codex/agents/commit-steward-c3-elevated.toml
- .codex/agents/commit-steward-c3.toml
- .codex/agents/commit-steward-c4.toml
- .codex/agents/commit-steward.toml
- .codex/agents/csharp-typed-engineer-c1.toml
- .codex/agents/csharp-typed-engineer-c2.toml
- .codex/agents/csharp-typed-engineer-c3-elevated.toml
- .codex/agents/csharp-typed-engineer-c3.toml
- .codex/agents/csharp-typed-engineer-c4.toml
- .codex/agents/csharp-typed-engineer.toml
- .codex/agents/epic-orchestrator.toml
- .codex/agents/epic-planner.toml
- .codex/agents/feature-reviewer-c1.toml
- .codex/agents/feature-reviewer-c2.toml
- .codex/agents/feature-reviewer-c3-elevated.toml
- .codex/agents/feature-reviewer-c3.toml
- .codex/agents/feature-reviewer-c4.toml
- .codex/agents/feature-reviewer.toml
- .codex/agents/orchestrator-c1.toml
- .codex/agents/orchestrator-c2.toml
- .codex/agents/orchestrator-c3-elevated.toml
- .codex/agents/orchestrator-c3.toml
- .codex/agents/orchestrator-c4.toml
- .codex/agents/orchestrator.toml
- .codex/agents/powershell-typed-engineer-c1.toml
- .codex/agents/powershell-typed-engineer-c2.toml
- .codex/agents/powershell-typed-engineer-c3-elevated.toml
- .codex/agents/powershell-typed-engineer-c3.toml
- .codex/agents/powershell-typed-engineer-c4.toml
- .codex/agents/powershell-typed-engineer.toml
- .codex/agents/pr-author-c1.toml
- .codex/agents/pr-author-c2.toml
- .codex/agents/pr-author-c3-elevated.toml
- .codex/agents/pr-author-c3.toml
- .codex/agents/pr-author-c4.toml
- .codex/agents/pr-author.toml
- .codex/agents/prd-feature-c1.toml
- .codex/agents/prd-feature-c2.toml
- .codex/agents/prd-feature-c3-elevated.toml
- .codex/agents/prd-feature-c3.toml
- .codex/agents/prd-feature-c4.toml
- .codex/agents/prd-feature.toml
- .codex/agents/task-researcher-c1.toml
- .codex/agents/task-researcher-c2.toml
- .codex/agents/task-researcher-c3-elevated.toml
- .codex/agents/task-researcher-c3.toml
- .codex/agents/task-researcher-c4.toml
- .codex/agents/task-researcher.toml
- .codex/config.toml
- .codex/hooks/record-subagent-routing-attestation.ps1
- .codex/lib/codex-routing/CodexDeployment.psm1
- config/orchestration-routing.json
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/branch-scope.2026-10-17T22-54.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/csharp-analyzers.2026-10-17T22-55.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/csharp-format.2026-10-17T22-54.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/csharp-format.2026-10-17T22-55.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/csharp-nullable.2026-10-17T22-56.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/csharp-package-restore.2026-10-17T22-55.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/csharp-sdk-install.2026-10-17T22-54.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/csharp-tests-coverage.2026-10-17T22-57.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/csharp-tool-restore.2026-10-17T22-54.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/csharp-tool-restore.2026-10-17T22-55.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/phase0-instructions-read.2026-10-17T22-54.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/powershell-analyze.2026-10-17T22-58.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/powershell-compatibility.2026-10-17T22-58.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/powershell-format-check.2026-10-17T22-58.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/powershell-format.2026-10-17T22-57.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/powershell-tests-coverage.2026-10-17T23-02.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/baseline/powershell-tests-mcp.2026-10-17T23-00.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/other/issue-995-cached-mcp-validation.2026-10-17T23-31.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/other/issue-995-published-mcp-validation.2026-10-17T23-31.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/other/mcp-default-cache-repair.2026-10-17T23-03.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/other/mcp-normal-startup-validation.2026-10-17T23-31.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/other/mcp-package-pin-repair.2026-10-17T22-40.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/other/preflight-clearance.2026-10-17T22-52.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/other/preflight-fixes.2026-10-17T22-51.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/branch-contracts.2026-10-17T23-30.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/configuration-contracts.2026-10-17T23-30.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/coverage-comparison.2026-10-17T23-27.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-analyzers.2026-10-17T23-10.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-analyzers.2026-10-17T23-14.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-format-apply.2026-10-17T23-10.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-format-apply.2026-10-17T23-13.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-format-check.2026-10-17T23-10.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-format-check.2026-10-17T23-13.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-nullable.2026-10-17T23-11.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-nullable.2026-10-17T23-14.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-tests-coverage.2026-10-17T23-12.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-tests-coverage.2026-10-17T23-13.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/csharp-tests-coverage.2026-10-17T23-15.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/issue-995-mcp-compatibility.2026-10-17T23-09.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/issue-995-routing-parity.2026-10-17T23-08.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/p14-acceptance-criteria-checkoff.2026-10-17T23-34.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-analyze-mcp.2026-10-17T23-16.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-analyze-mcp.2026-10-17T23-19.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-analyze-mcp.2026-10-17T23-23.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-analyze-repository-comparison.2026-10-17T23-18.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-analyze-repository-comparison.2026-10-17T23-23.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-analyze.2026-10-17T23-16.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-analyze.2026-10-17T23-18.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-analyze.2026-10-17T23-23.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-compatibility.2026-10-17T23-18.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-compatibility.2026-10-17T23-23.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-format-check.2026-10-17T23-18.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-format-check.2026-10-17T23-23.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-format.2026-10-17T23-16.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-format.2026-10-17T23-18.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-format.2026-10-17T23-22.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-tests-coverage.2026-10-17T23-21.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-tests-coverage.2026-10-17T23-26.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-tests-mcp.2026-10-17T23-20.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/powershell-tests-mcp.2026-10-17T23-25.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/repository-hygiene-after-proof-redaction.2026-10-17T23-32.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/qa-gates/repository-hygiene.2026-10-17T23-30.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/issue-995-csharp-green.2026-10-17T23-09.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/issue-995-enter-red.2026-10-17T23-03.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/issue-995-red-build.2026-10-17T23-02.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/issue-995-red-build.2026-10-17T23-03.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/issue-995-routing-green.2026-10-17T23-09.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/issue-995-routing-targeted.2026-10-17T23-07.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/issue-995-routing-targeted.2026-10-17T23-08.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/issue-995-targeted-build.2026-10-17T23-05.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/issue-995-targeted-format.2026-10-17T23-05.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/issue-995-targeted-tests.2026-10-17T23-05.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/pester-ci-shape.2026-10-17T23-29.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/pester-isolation-seed-1995.2026-10-17T23-28.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/pester-isolation-seed-2995.2026-10-17T23-28.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/regression-testing/pester-isolation-seed-995.2026-10-17T23-28.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/issue.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/plan.2026-10-17T13-44.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/research/2026-10-17T17-49-issue-995-efc-enter-routing-research.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/research/2026-10-17T22-42-issue-995-efc-enter-routing-research.md
- docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/spec.md
- docs/features/potential/promoted/2026-10-17-efc-viewer-enter-confirmation.md
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs
- QuickFiler.Test/Controllers/EfcFormControllerIssue995Tests.cs
- QuickFiler.Test/QuickFiler.Test.csproj
- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs
- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs
- QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs
- tests/scripts/vscode/CodexDeployment.Tests.ps1
- tests/scripts/vscode/CodexSubagentRoutingAttestation.Tests.ps1
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs

Final commands and reusable bootstrap for a fresh reviewer PowerShell process at repository root:

1. pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1 (installs/recognizes ignored .dotnet-sdk8.0.205; EXIT0).
2. Optional explicit SDK binding for a host that selects only.NET10: set DOTNET_ROOT to the resolved .dotnet-sdk directory and prepend that directory to PATH; then dotnet --version must print8.0.205. global.json already lists .dotnet-sdk before host fallback; current fresh host dotnet resolves8.0.205 without a PATH override.
3. dotnet tool restore (CSharpier1.2.6); pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1.
4. dotnet tool run csharpier format .; dotnet tool run csharpier check . (final EXIT0).
5. pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild (EXIT0;0warnings0errors).
6. pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors (EXIT0;0warnings0errors; existing nullable opt-in preserved).
7. pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/issue-995-final.cobertura.xml (EXIT0;7,471pass); raw reports/results copied to SCRATCH and verified before targeted transient raw removal.
8. Native PoshQC format/analyze/test with explicit TaskMaster scripts/dependencies,scripts/hygiene,scripts/vscode and matching test folders plus .codex/lib/codex-routing,.codex/hooks; direct SCRATCH/issue-995-powershell-static.ps1 -Phase final and SCRATCH/issue-995-pester-gate.ps1 -Phase final establish actual findings/compatibility/counts/coverage. Expanded513passes; scoped EXIT0; broad supplemental EXIT1/delta0.
9. pwsh -NoProfile -File scripts/hygiene/Test-RepositoryHygiene.ps1 (latest post-proof-redaction EXIT0; Findings0).

AC Status Summary: spec.md only; total8; checked8; remaining0. Plan final task check-off follows this saved report. Final readiness: PASS for implementation handoff. Coordinator owns final partial commit, independent review, PR and exact-head CI; none is claimed completed by this report.
Exact optional fresh-process SDK binding (PowerShell, from repository root):
```powershell
pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1
$env:DOTNET_ROOT = (Resolve-Path -LiteralPath '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet --version
dotnet tool restore
```
Expected SDK:8.0.205. The environment changes apply only to that reviewer process.
