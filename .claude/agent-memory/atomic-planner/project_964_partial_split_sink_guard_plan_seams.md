---
name: project_964_partial_split_sink_guard_plan_seams
description: #964 R0 minimal-audit plan seams - Grep tool tokens are regex (escape parens/brackets, backslash as \x5C), pure-move split proven by ordinal multiset census, runner exits non-zero on pre-existing local failures so AC "no new failing test" needs a failed-set subset rule
metadata:
  type: project
---

Plan for #964 (EngineToggleStateCoordinator split into three partials + one shared `TryInvokeSink` guard + refusal-path notify guard). 59 tasks, 3 phases, no commits.

- **Grep tool tokens are regular expressions.** A Grep-tool acceptance token such as `GetPrimeTask(string engineName)`, `"(null)"`, `[TestMethod]`, `OnNotify?.Invoke(...)` or `Ribbon\Engine...` either fails to match (parens become a group) or errors (`\E`, `{ get; }`). Escape `\(` `\)` `\[` `\]` `\.` `\?` and write a literal backslash as `\x5C`; state the rule once in Execution conventions. Whitespace-stripped or phrase-normalised pwsh census payloads (Regex.Escape) avoid the problem for multi-token checks.
- **Pure-move split proof:** ordinal multiset (Dictionary with StringComparer.Ordinal, `return ,$bag`) of trimmed non-blank lines, BASE-SHA file via `git show` vs the split files; the exact admitted EXTRA/MISSING set is derivable in advance (partial keyword x3 vs 1, namespace/brace scaffolds, redistributed usings). PowerShell `@{}` and `Sort-Object -Unique` are case-insensitive by default.
- **Doc-phrase gates across wrapped `///` lines:** strip a leading `//`/`///` per trimmed line, join with spaces, collapse whitespace, then count `[regex]::Escape(phrase)`; record base (false-before) values in Phase 0 and final values after the edit.
- **Runner route:** `Invoke-MSTestWithCoverage.ps1` throws on any failed test before post-processing (no summary, no projection, raw doc and trx left on disk). On this machine one shell-icon test fails at baseline, so an AC naming the runner needs the test-step rule "exit 0, or non-zero with every failed name in the Phase 0 baseline failed set and none in the fixture"; post-process with RAW True. Flag this interpretation to the orchestrator.
- **Partial-class coverage:** aggregate `Get-CoberturaClassLineSummary` over every class node whose filename ends with any of the split files; add a stop branch if a split file has no class node (attribution unverified for partials).
- **`.cs` edits must be Edit/Write tool steps,** never pwsh writes (hooks only see the tools); a programmatic split payload is therefore not allowed - transcription by Read+Write, proven by the census.

- **R1 (preflight):** `Get-TrxRunSummary` `FailedTestName` is the trx `testName` (short method name), so a "pre-existing failures only" rule must compare FULLY QUALIFIED names: map `UnitTestResult/@testId` to `TestDefinitions/UnitTest/@id` and build `className` (split at first comma) + "." + `name`; emit `TEST-DEFINITIONS:`, `FAILED-FQN-COUNT:`, `FAILED-FQN` rows and an `UNRESOLVED:` sentinel. Also: every task that runs a command (even a Grep + git numstat registration check) must name and create its artifact, and a host-path sweep must be re-run as the LAST task after check-offs and the audit handoff, with a FILES_SCANNED floor derived from the Write Set evidence count.

Related: [[project_948_r0_repeat_fault_suppression]] (not written), [[async-state-machine-coverage-aggregation]], [[csharpier-formatted-n-is-processed-count]].
