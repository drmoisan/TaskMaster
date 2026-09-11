---
name: project-826-console-out-banned-symbol-plan-seams
description: Preflight revision seams for issue #826 (console-out aggressors + BannedSymbols promotion) - R2 preamble/substring/per-site/terminal-recursion; R3 unscoped control, unobservable EXIT_CODE, prose-only $Base, TRX notExecuted; R4 uncompiled control file, undiscriminated shape branch; R5 unanchored provenance phrase vs stale header
metadata:
  type: project
---

Three preflight rounds on the #826 atomic plan. The transferable seams:

## Round 2 (twelve defects)

- **A preamble that reads an artifact kills the task that creates it.** With
  `$ErrorActionPreference = "Stop"`, an unguarded `Get-Content -LiteralPath` on the base-ref file in a
  plan-wide preamble is a terminating error in P0-T2, the very task that writes it. Guard with
  `Test-Path` and state explicitly which tasks legitimately see the empty value.
- **`0 Error(s)` is a substring of `10 Error(s)`.** An MSBuild summary gate of the form
  `-SimpleMatch "0 Error(s)"` with "count at least 1" is satisfied by a build with 10 errors. Assert
  `" 0 Error(s)"` with the leading space MSBuild prints.
- **A census must span every file that consumes it.** Widen the census to the whole write set, not to
  the subset that motivated it.
- **Per-file observation silently weakens a per-site AC.** Express completeness as
  `re-derived set minus observed set is empty`.
- **A fenced block's four-space indent leaks into appended file content.** Add an explicit
  "indentation is not content" note plus a `^\s` zero-count gate.
- **Terminal-residual recursion.** Order the final task write-artifact, check-off, then commit.

## Round 3 (seven defects created BY the round-2 revision)

- **A control gate must be scoped to the rule it certifies.** Counting textual occurrences of a
  control FILE NAME in a SARIF error log is not a control: with `EnableNETAnalyzers` +
  `EnforceCodeStyleInBuild` every control file carries unrelated IDE diagnostics whose result
  locations name it, so the count is >= 1 whether or not the channel carries the rule under test -
  exactly the void channel the control exists to exclude. Parse the SARIF, filter
  `results` on `ruleId`, and extract each result's own location URI. Two further facts: a Roslyn error
  log carries rule METADATA for rules that produced no result (so a `-SimpleMatch "RS0030"` count is
  not a diagnostic count), and the location shape is version-dependent -
  `locations[0].resultFile.uri` in SARIF v1 vs `locations[0].physicalLocation.artifactLocation.uri`
  in v2 - so probe both.
- **A write-then-commit terminal task cannot carry the commit's own `EXIT_CODE:`.** Fixing the
  round-2 recursion (write artifact before committing) makes the commit's exit code unobservable at
  artifact-write time, and stating it in advance ("which is 0") is a prediction, not an observation.
  Scope `EXIT_CODE:` to a pre-commit `git status` invocation the task actually runs and observes, put
  the commit command in a differently named field, report the commit's own result to the orchestrator,
  and add a one-clause scoping carve-out to the plan's fail-closed evidence rule so the rule stays
  true rather than being departed from.
- **A halt rule keyed on "block" misses the acceptance-only reference.** `$Base` appeared in fenced
  blocks in eleven tasks and in ACCEPTANCE PROSE ONLY in one. An empty `$Base` degrades
  `git diff --numstat $Base -- <path>` to an unanchored worktree-versus-index comparison that reads
  the expected added/removed pair for a just-edited unstaged file, so the gate passes with no anchor.
  Word the rule "any task that references `$Base`, whether in a fenced block or in its acceptance
  text", and name the outlier task.
- **A residual-explaining acceptance must name what exists AT OBSERVATION TIME.** A task whose block
  runs before it writes its own artifact and before its own check-off must attribute the porcelain
  span to the PRIOR task's residual, not to its own. The wrong attribution is non-blocking only
  because the asserted constraint holds either way.
- **Assert both halves of a non-vacuity pair everywhere, not only where the AC names it.** Tasks
  asserting `Skipping target "CoreCompile"` count 0, or CS0169/CS0414 counts of 0, are each satisfied
  by a log that recorded no compilation; each needs `Task "Csc"` >= 1 from the same log in the same
  task. Widen the convention bullet so it covers every zero-count assertion read from an msbuild log,
  not just the AC14 pair.
- **`skipped` is NOT a TRX counter attribute.** vstest's `ResultSummary/Counters` carries `total`,
  `executed`, `passed`, `failed`, `error`, `timeout`, `aborted`, `inconclusive`,
  `passedButRunAborted`, `notRunnable`, `notExecuted`. An equality assertion on a "skipped count"
  names a value with no source. Use `notExecuted`.
- **A fail-before dossier written in Phase 1 cannot cite a Phase 6 figure.** A "2 before and 0 after"
  alternative proof records a prediction as an observation. Record the observed pre-change figure and
  NAME the later task that asserts the post-change one.

## Round 4 (two defects)

- **A control site must be COMPILED, not merely live.** `QuickFiler/Legacy/QuickFileController.cs`
  carries three live `DateTime.Now` reads (1010/1013/1021) but is named by no `<Compile Include>`
  item in ANY `*.csproj` in the repo. `QuickFiler.csproj` is a legacy non-SDK project
  (`<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">`) with
  explicit compile items and zero wildcard includes, so the compiler never sees the file and it can
  emit no diagnostic on any channel - the same void-control class as the commented-out-code trap,
  reached through non-compilation. Liveness greps do not detect this. For any legacy non-SDK project,
  grep the `.csproj` for the file name before naming it as a control or as a site under test. The
  usable `QuickFiler` control is `Controllers/EfcHomeControllerDependencies.cs` line 77,
  `MetricsNowFactory = metricsNowFactory ?? (() => DateTime.Now);`, registered at `QuickFiler.csproj`
  line 302 - the only live, compiled `DateTime.Now` read in that project.
- **A block that BRANCHES on a shape must EMIT the discriminator.** The SARIF v1/v2 location-shape
  probe branched correctly but discarded the branch result, so the acceptance's demand that the
  artifact record "the SARIF location shape found" had no source in the transcribed output. Emit
  `$j.version` and have the acceptance read the version plus the shape it implies. Generalization: any
  acceptance field whose value is decided inside a conditional needs an explicit emission of the
  deciding value.

## Round 5 (one minor defect)

- **"during this revision pass" is unresolvable, and a stale header resolves it to the wrong round.**
  A provenance claim written in round N reads as a claim about whatever round the header names. The
  #826 header still said `revision round 2 applied` / `Version: 0.3` after four rounds, so four
  claims added in round 4 resolved to round 2 - the round whose named control was the uncompiled
  `QuickFileController.cs`. A later auditor checking whether the AC10 control argument rests on
  verified sites would draw a false conclusion. Write provenance as
  `re-derived against the tree on <date>, in revision round <N>`, and update the header's
  `Status:`/`Version:` in the same round that introduces the anchored form. A blanket
  "each item was re-derived during plan authoring" preamble also needs an
  "except where an item names a later dated revision round" carve-out once any item is re-derived
  later. Round-number asymmetry is correct and must not be reconciled: body claims name the round
  that performed the re-derivation; the header names the round applying the header edit.
- **A planner under worktree isolation has no clock.** `pwsh` is refused, so a minute-precision
  `Last Updated:` cannot be observed. Take the date from the caller, write date-only precision, and
  state in the header why the minute is omitted - rather than synthesizing a `yyyy-MM-ddTHH-mm`
  value. That convention still governs executor-written evidence filenames.

Verified facts about the tree (re-derive before reuse): `.csharpierignore` does NOT list
`.editorconfig` - the exemption comes from CSharpier processing only `*.cs`, `*.xml` and
`packages.config`. No `.csharpierrc` exists, so the width is the 100-column default. The three named
AC10 control sites at R4, each confirmed live AND registered: `ApplicationIdleTimer.cs` 60/140/236
(`UtilitiesCS.csproj` 1098), `EfcHomeControllerDependencies.cs` 77 (`QuickFiler.csproj` 302),
`MailItemInfoTests.cs` 25 (`QuickFiler.Test.csproj` 221). The five AC10 sites under test are all
registered: `QfcQueueCoverageExpansionTests.cs` and `BreadcrumbCoordinatorLifecycleTests.cs` at
`QuickFiler.Test.csproj` 119 and 71, `ConversationHelper.cs` and `TimeOutTask.cs` at
`UtilitiesCS.csproj` 1010 and 1113, `QfcQueue.cs` at `QuickFiler.csproj` 348.

See [[plan-fenced-powershell-comments-look-like-headings]],
[[single-numeral-gates-must-name-the-role]], [[terminal-phase-planner-traps]],
[[empty-porcelain-clause-is-unsatisfiable]], [[wiring-gates-must-be-wiring-sensitive]],
[[acceptance-edits-must-be-false-before-true-after]].
