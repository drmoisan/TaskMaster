# P2-T3 Test Step in Coverage Mode, Iteration 1

Timestamp: 2026-09-29T09-23
Task: P2-T3
Iteration: 1
Command: mcp__drm-copilot__run_poshqc_test with workspace_root = <repo-root> and scan_folders = ["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]; artifacts/pester/pester-junit.xml and artifacts/pester/powershell-coverage.xml read with the Grep tool; then CMD-PESTER-DIRECT (Route C) through the Bash tool with the one substitution recorded below; git diff -U0 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1; git diff --numstat 177b6d78e -- scripts/vscode; git status --porcelain -uall. The EXIT_CODE row is derived per D15 from the bundled JUnit root failures plus errors.
EXIT_CODE: 0

## CMD-TEST (bundled MCP route)

- Payload: {"ok":true,"tool":"run_poshqc_test","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC test against '<repo-root>' with 4 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG: true

### JUnit document (artifacts/pester/pester-junit.xml; derived figures only)

- Root tests: 334 (TESTS_BASELINE 320 plus 14; condition met)
- Root failures: 0
- Root errors: 0
- testsuite element count: 27 (the 26 baseline suites plus the new one)
- New testsuite Invoke-MSTestWithCoverage.Scope.Tests.ps1: tests 14, failures 0, errors 0
- Grep count of `<skipped`: 0
- Grep count of `<failure`: 0
- Every other testsuite reports the same tests value as in P0-T7, each with failures 0 and errors 0.

## Coverage route

- Grep count of `Invoke-MSTestWithCoverage.Scope.ps1` in artifacts/pester/powershell-coverage.xml: 0, so Route A does not apply.

COVERAGE-ROUTE: C

CMD-PESTER-DIRECT was run verbatim with one substitution: the argument of `Set-Location -LiteralPath` changed from `(git rev-parse --show-toplevel)` to the literal worktree path, transcribed here as `"<repo-root>"` (the same substitution P0-T7 recorded). `$o` kept its plan value `coverage/p2-t3-pester-coverage.jacoco.xml` (ignored by .gitignore line 144; not committed, not copied into the feature folder). Console lines printed by the tests themselves are not transcribed.

- DIRECT_PESTER_EXIT: 0
- PESTER_COUNTS: passed=334 failed=0 skipped=0
- POPULATION_LINE: covered=1620 missed=95
- FILE_LINE: Invoke-MSTest.ps1 total=56 uncovered=7 uncovered_lines=243,245,246,248,252,256,261
- FILE_LINE: Invoke-MSTestWithCoverage.ps1 total=129 uncovered=14 uncovered_lines=149,171,187,229,333,341,345,358,364,370,408,441,457,465
- FILE_LINE: Invoke-MSTestWithCoverage.Scope.ps1 total=5 uncovered=0 uncovered_lines=
- FINAL_POPULATION_LINE_PERCENT: 94.46 (100 x 1620 / (1620 + 95) = 94.460..., two decimals)
- BASELINE_POPULATION_LINE_PERCENT (P0-T7, Route C): 94.49
- Per-file line percentages: Invoke-MSTestWithCoverage.ps1 89.15 (115 of 129); Invoke-MSTestWithCoverage.Scope.ps1 100.00 (5 of 5); Invoke-MSTest.ps1 87.50 (49 of 56; unchanged from baseline)

## Changed-line derivation

`git diff --numstat 177b6d78e -- scripts/vscode` (verbatim):

```
49	0	scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1
29	2	scripts/vscode/Invoke-MSTestWithCoverage.ps1
```

Invoke-MSTest.ps1 is not listed (the formatter did not rewrite it), so no changed-line derivation applies to it.

Plus-side hunk headers of `git diff -U0 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1`:

- `@@ -277,0 +278,14 @@` changed lines 278 to 291 (help paragraphs; comment-based help carries no coverage point)
- `@@ -296,0 +311,4 @@` changed lines 311 to 314 (comment, dot-source, blank)
- `@@ -386,2 +404,11 @@` changed lines 404 to 414 (the conditional block)

Uncovered lines of the entry point intersected with the changed lines: 408 only. Each other uncovered entry-point line is a baseline uncovered line shifted by the 18 lines inserted above it (315, 323, 327, 340, 346, 352 become 333, 341, 345, 358, 364, 370) or by 27 lines (414, 430, 438 become 441, 457, 465); 149, 171, 187 and 229 are unchanged.

- CHANGED-LINES-UNCOVERED (Invoke-MSTestWithCoverage.ps1): 408
- CHANGED-LINES-UNCOVERED (Invoke-MSTestWithCoverage.Scope.ps1): none (every line the document lists for the file is covered)

Line 408 is the `Write-Warning` statement of the scoped arm.

## Root cause of the uncovered changed line (diagnostic micro-actions, measured)

Three diagnostic runs of the CMD-PESTER-DIRECT command shape were made, each with a narrowed Run.Path and CodeCoverage.Path of scripts/vscode/Invoke-MSTestWithCoverage.ps1 and its `$o` under the ignored coverage directory:

1. Run.Path = the new test file alone: 14 passed; line 408 ci=2 (covered).
2. Run.Path = Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1, then the new test file: 19 passed; line 408 ci=0 (uncovered).
3. The same two files in the reverse order (container order confirmed from the result object): 19 passed; line 408 ci=2 (covered).

Reading: the Pester 5.6.1 run uses breakpoint-based coverage (`CodeCoverage.UseBreakpoints` printed True). Each test file that invokes `Invoke-MSTestWithCoverageMain` imports it by `Parser::ParseFile` and dot-sources the resulting script block, so each file gets its own compiled copy of the function. The line breakpoints bind to the first copy whose function body executes; a later file's copy executes without recording hits. In the full population run Invoke-MSTest.RunSettings.Tests.ps1 sorts before the new file and executes the function first, so every entry-point line is credited only by the files that sort first. Line 408 is reached only by the scoped-run cases in the new test file, so it is executed (It 10 asserts the warning with its message, and passes) but never credited.

The same mechanism is the only cause of the population decrease: relative to the baseline the run adds 7 covered lines and 1 missed line (408), and 1620 / 1715 rounds to 94.46 against 94.49.

## Iteration verdict

- JUnit conditions: all met (root failures 0, errors 0, tests 334 = 320 + 14, new suite 14 and 0, `<skipped` 0, MCP ok true).
- Coverage conditions: every figure is numeric; the new part file is at 100.00 (at least 90); CHANGED-LINES-UNCOVERED is not `none` for the entry point (line 408). The condition is not met.
- RESTART: yes is what D16 prescribes for an uncovered changed line, conditional on "a fix inside the Write Set". No such fix exists under the approved specifications: the Test Specification mandates the parse-and-dot-source import that every sibling uses, the Production Specification fixes the text of the conditional block, and the only files that could credit line 408 (the earlier-sorting sibling test files) are Out of Scope. An iteration 2 without a change would reproduce the same deterministic result, so no further iteration was run.
- LOOP-STATUS: NOT CLOSED (escalated to the orchestrator for a plan revision; see P2-T4 and P2-T13).

## Tree hygiene

- git status --porcelain -uall after the runs: no line reads `?? coverage.xml`, so no file was removed. The listing names only the pre-existing agent-memory paths, the plan file, and the P2-T1 and P2-T2 artifacts.

Output Summary: FAIL (coverage condition only). JUnit: 334 tests, 0 failures, 0 errors, 0 skipped, 27 suites; the new suite 14 of 14; MCP ok true. COVERAGE-ROUTE: C. POPULATION_LINE covered 1620, missed 95; FINAL_POPULATION_LINE_PERCENT 94.46 against a baseline of 94.49. Entry point 129 lines with 14 uncovered; new part file 5 of 5 (100.00); Invoke-MSTest.ps1 unchanged at 56 lines with 7 uncovered. CHANGED-LINES-UNCOVERED for the entry point is 408, the scoped-arm Write-Warning. Diagnostic runs show the line is executed but not credited, because breakpoint coverage binds to the first test file's parsed copy of the entry point. No in-scope fix exists, so the loop is not closed and the finding is escalated.
