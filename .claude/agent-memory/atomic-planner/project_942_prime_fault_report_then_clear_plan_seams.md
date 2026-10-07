---
name: project-942-prime-fault-report-then-clear-plan-seams
description: "#942 plan seams, rounds 0 and 1 (EngineToggleStateCoordinator.CompletePrime report-then-clear reorder) - the coverage runner DELETES the raw Cobertura unless it sits in the repo coverage dir and emits a jacoco projection plus a trx summary; ONE class node per filename after post-process; [DataTestMethod] lacks the [TestMethod] substring; racy original may fail at baseline; BeSameAs prints 'to refer to'; R1: format before the code commit so the Phase 3 format must rewrite 0, two-window hunk rule on single-occurrence anchors, no backtick-n or backslash escapes in code-span pwsh payloads, background coverage payloads with a sentinel, reflog readable at .git/worktrees/<id>/logs/HEAD without Bash, amend AC13 to name the inherited promotion record; R2: column-count every gated single-line token (a 101-column BeEmpty call breaks into a chain), same-line FIRST-LINE equality for reason strings, name the numstat command, cite POST-FORMAT sections, never pin a commit count, bump the spec header after an AC amendment"
metadata:
  type: project
---

Seams re-derived while authoring the issue #942 plan (2026-09-29, revision round 0) in a parallel-run
worktree branched from `ddbab26a0149bf2ca5d0256e60686ad79e74d90c`.

**Why:** each one either changed a gate's data source or would have drawn a preflight finding.

**How to apply:** re-check before planning any coverage-bearing C# bugfix on the current runner, any
partial-class test fixture edit, or any plan whose expect-fail control is a racy existing test.

1. **`scripts/vscode/Invoke-MSTestWithCoverage.ps1` (462 lines on this tree) discards the raw
   Cobertura document after writing a JaCoCo projection and a trx summary** unless the output's
   parent directory IS the repository coverage directory (Projection.ps1 `Test-RawCoverageDocumentRetained`
   148-197; discard at the entry point 449-453). Leave `-CoverageOutput` at its default so the document
   survives under the git-ignored `coverage\` tree, copy it under a stage name, and read per-file figures
   from the copy. The projection lands beside it as `coverage.cobertura.jacoco.xml`; the summary at
   `coverage\test-results\mstest-coverage-run.summary.txt`. AC text that forbids "any xml" in the diff
   means the projection must be transcribed into Markdown, not copied.
2. **A post-processed document carries exactly one `class` element per source file per package**
   (`Merge-CoberturaClassesByFilename`, Helpers.ps1 260-405) with backslash-separated relative
   `filename` attributes on Windows. Match with a separator-normalised `EndsWith`, gate
   `COORD-CLASS-NODES: 1`, and use `Get-CoberturaClassLineSummary` (160-258) for lines, branches and the
   per-line `LineMap`. Use `.Replace([string][char]92, "/")` - the `[char]` first argument is
   overload-ambiguous against `(string,string)`.
3. **`"[DataTestMethod]".Contains("[TestMethod]")` is false.** The coordinator fixture has 15
   `[TestMethod]` and 1 `[DataTestMethod]`; I first wrote 17. Count with Grep before pinning.
4. **A racy original test may fail in the Phase 0 baseline run and in the expect-fail run.** Do not
   require it `Passed` there; require its `RESULT` line present with the outcome recorded, and let the
   fail-before `FAILED` set be a subset of {new test, original test}. Only the pass-after run requires
   all `Passed`.
5. **FluentAssertions 8 `BeSameAs` failure fragment is `to refer to`** (`Expected {context} to refer
   to {0}{reason}, but found {1}.`); the subject name may not resolve, so gate on the fragment.
6. **Only `### Phase N — ` may be an H3.** A `###` heading for a delivered-source sub-section is
   heading-shaped for the line-based validator; use a bold paragraph instead. The `## ` sections are
   safe. Also never put a `## ... Phase 0 ...` H2 directly above the real Phase 0 heading.
7. **Backticked slash tokens the harvest catches this time:** msbuild property switches
   (`/p:TreatWarningsAsErrors=true`), the blame switch, `coverage/*`, `!coverage/.gitkeep`,
   `Properties\AssemblyInfo.cs`, and built-assembly paths in a substitutions table. Write all of them
   as prose or inside a multi-word command span.
8. **Sibling-run precedent (#931, same run) for the coverage route:** stall-probe the four UtilitiesCS
   shell-icon classes at Phase 0, then RUNNER (script verbatim) or DIRECT (inner `dotnet-coverage
   collect` with the four-class exclusion, dot-sourcing the runner for `ConvertTo-DerivedCoverageSettingsXml`
   and the helpers for post-processing). Admit `TryAddValuesAsync_UpdatesExistingValue` (issue 780,
   UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs:237) by name with ONE same-command re-run
   at the final gate, never a second.
9. **The Bash tool was not in the planner tool surface this session** (Read/Grep/Glob/Edit/Write
   only), so the caller-supplied base SHA could not be verified with git; the plan verifies it at P0
   (`git merge-base --is-ancestor` plus equality with `git merge-base origin/main HEAD`, halt on
   mismatch) and records the inherited promotion-commit paths for subtraction in the footprint gate.
10. **`git show <sha>:<path>` output compared with `Get-Content` lines needs `TrimEnd([char]13)` on
    both sides** so a CR difference between blob and checkout cannot fake a method-text mismatch
    (R1 narrowed it from `TrimEnd()` so a trailing-whitespace change still fails the gate).

Round 1 (2026-09-30, 18 deltas, all adopted):

11. **Format the code files BEFORE the implementation commit** (scoped `csharpier format <files>` with
    CMD-HASH before/after as `PRECOMMIT-FORMAT-REWRITES:`); the Phase 3 repo-wide format then MUST
    rewrite 0 files and any rewrite or failing post-format gate is `POST-COMMIT CODE REWRITE: stop`.
    No code edit after the commit, so the Phase 3 "fix and restart" wording must go.
12. **A single lower bound for hunk positions admits hunks in every member below it.** When the edit
    touches two separated regions, use two windows anchored on single-occurrence tokens: [G-6, G-1]
    around the doc element and [S-2, R] from the summary token to the moved statement, span = c..c+d-1
    (c alone when d is 0). Also add a span-scoped measurement (SPAN_RETURN/RANTOCOMPLETION/TRY/CATCH/LOCK)
    instead of file-level counts for "no try/catch/lock added to the method".
13. **Backtick-n escapes inside a pwsh payload that sits in a Markdown code span break the span and do
    not survive Bash-to-pwsh transport**; use `([string][char]10)`. Likewise normalise backslashes with
    `.Replace([string][char]92, "/")` and match `[A-Za-z]:/Users/` rather than a `[\\/]` class.
14. **Every csharpier check/format task needs its own pwsh payload** with `Set-Location` and
    `"CSHARPIER_EXIT_CODE: $LASTEXITCODE"`; `dotnet tool list --local` transcribes Package Id and
    Version only (the Manifest column is an absolute path).
15. **Long-running coverage payloads run in the background**: end the payload with a sentinel line
    (`PAYLOAD-COMPLETE`), redirect its stdout to a result log, poll for the sentinel, and after any
    foreground timeout require `STRAY_TEST_PROCESSES: 0` (vstest*, testhost*, dotnet-coverage*) before
    a rerun.
16. **AC13-style footprint gates: amend the AC to name the inherited path** rather than recording a
    divergence note. The worktree reflog (`.git/worktrees/<id>/logs/HEAD`) is readable with the Read
    tool when Bash is absent and shows the branch cut point and every commit since; here it proved the
    inherited set was the feature folder plus the promotion record only, and that the harness-supplied
    gitStatus described a different worktree.
17. **Quote-what-the-task-creates for a strict-mock token**: the Harness declares
    `new Mock<IAppItemEngines>(MockBehavior.Strict);` alone on its own line, so the exact literal exists
    today and counts 1; check the split across `=` before quoting.
18. Reviewer asked for `// Arrange`, `// Act`, `// Assert`, reason-string-bearing call tokens and an
    exact `.BeSameAs(` count; write reason-bearing calls in CSharpier's member-chain layout so the
    tokens stay single-line after the format pass, and have the check-off tasks cite the POST-FORMAT
    re-count, never the plan's Delivered Source text.

Round 2 (2026-09-30, 6 deltas R1-R6 plus advisories A1, A2, A4; A3 rejected by the caller):

19. **Count the columns of every gated single-line token at its in-file indentation before quoting it.**
    A FluentAssertions call with a reason string is one physical line only while it fits CSharpier's
    100-column width; `harness.Invalidations.Should().BeEmpty("...")` at 16-space indent was 101
    columns, so CSharpier would have broken it into a member chain and the call token would count 0.
    Shorten the reason or re-split a long reason into `"..." + "..."` operands at a word boundary so the
    gated fragment sits whole on one operand; a joined concatenation over 100 columns stays broken.
20. **Reviewer asked for reason-string evidence that can fail**: gate `FIRST-LINE` of the reason
    fragment equal to `FIRST-LINE` of its call token (same physical line) rather than "call counts 1",
    and enumerate every assertion so the check-off can state that every call carries a reason.
21. **A `Command, X:` line whose acceptance reads a numstat count must name the numstat command**;
    "together with" a token count is not a numstat.
22. **Every format re-run clause must name every token list it re-runs** (P1-T1 and P1-T2 lists, not
    only the P2-T6/P2-T7 checks), and every check-off that cites an artifact with a `POST-FORMAT:`
    section must cite that section, not the file.
23. **A stated commit count goes stale on every revision commit**; write "documentation-only commits
    (N at round K; the count is not gated)" and sweep facts, P0 tasks and self-review lines together.
24. Amending an AC in the spec obliges a spec header bump (Last Updated, Version) in the same cycle.

Related: [[project_839_createcancellationtoken_init_plan_seams]], [[project_838_gettableinviewasync_null_contract_plan_seams]],
[[reference_invoke_mstest_with_coverage_script]], [[worktree-root-breaks-dotclaude-exclusion]],
[[validate-planner-output-hook-line-anchored-gotchas]], [[repo-wide-cobertura-line-rate-is-nondeterministic]].
