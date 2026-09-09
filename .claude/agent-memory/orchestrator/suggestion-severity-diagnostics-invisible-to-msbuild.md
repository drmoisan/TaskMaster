---
name: suggestion-severity-diagnostics-invisible-to-msbuild
description: A `severity = suggestion` analyzer rule emits nothing into the msbuild console log at default or -v:m verbosity, so any acceptance condition that observes such a rule through the build log is unsatisfiable — and for BannedApiAnalyzers, absence additionally proves nothing because a malformed DocID is ignored silently
metadata:
  type: project
---

TaskMaster holds every third-party analyzer rule at `severity = suggestion` (`.editorconfig`
line 26 sets `dotnet_analyzer_diagnostic.severity = suggestion` as a catch-all, and RS0030 is
pinned individually at line 548). A suggestion maps to Roslyn **info** severity, and info-severity
diagnostics do not reach the msbuild console log.

**Two independent measurements in this repository, at different verbosities:**

- `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/baseline/analyzer-build-baseline.md`
  — a full `/t:Rebuild` analyzer build at default verbosity produced `5 Warning(s) / 0 Error(s)`,
  all five being the same System.Reactive `packages.config` advisory, and the artifact states
  explicitly that no `RS0030` is present. Roughly 143 banned-symbol usages existed at the time.
- `docs/features/archive/2026-06-28-qfc-banned-api-time-delay-seams-222/evidence/baseline/baseline-analyzer.md:9`
  — "Suggestion-level diagnostics are not emitted by `-v:m` and do not fail the analyzer build."

**Why it matters beyond the obvious.** For `Microsoft.CodeAnalysis.BannedApiAnalyzers`
specifically, the failure compounds: the analyzer **silently ignores a DocID it cannot resolve**.
A typo in a `BannedSymbols.txt` entry produces no diagnostic, no warning and no error. So on a
tree where the entry is garbage and a tree where the entry is correct, the build output is
byte-identical. "The build was clean" and "no RS0030 appeared" are each equally consistent with
the ban working and with it being inert. An acceptance condition of the form "the analyzer gate
exits 0" or "no RS0030 error appears" therefore verifies nothing at all about the ban list. This
is the same shape as [[absence-from-failure-list-is-not-a-pass-gate]].

**How to apply.** If a plan must prove a suggestion-severity rule actually fires, do not assert
against the console log. Certify a channel first, in this order, and pair it with a control:

1. `/p:ErrorLog=<path>.sarif` — Roslyn's error log records info-severity diagnostics regardless of
   console verbosity. Hazard: a command-line `/p:ErrorLog=` is a global property and is not
   re-expanded per project, so a `/m` solution build has projects overwrite one another's log.
   Build each relevant project separately with its own SARIF path.
2. A detailed-verbosity file logger (`/fl /flp:LogFile=...;Verbosity=detailed`), searched for the
   rule ID.
3. Last resort: temporarily raise the severity to `warning`, run the **analyzer** gate (which
   passes no `TreatWarningsAsErrors`, so warnings do not fail it), capture the diagnostics, then
   revert and prove the revert with an anchored `git diff`. Raising a severity to measure and
   restoring it is not weakening a gate; leaving it raised, or lowering something else to
   compensate, would be.

**The control is not optional.** The channel must show the rule firing for a symbol with known
current usages in the same run — `P:System.DateTime.Now` (54 textual sites) or
`M:System.Threading.Tasks.Task.Delay(System.Int32)` (58) both work. A channel that reports zero
for the control has not been shown to carry diagnostics at all, so a zero for the symbol under
test is uninformative.

**Corollary for severity promotion.** RS0030 cannot be promoted to `warning` while the
pre-existing usages stand, because toolchain step 3 (`/p:TreatWarningsAsErrors=true`, mirrored by
`.github/workflows/_build-nullable.yml`) promotes it to an error. The analyzer gate
(`_build-analyzers.yml`) passes no such switch and would not break — name the right gate when
reporting this. No scoping mechanism is available: one `.editorconfig` with three
language-scoped sections and no path scoping, zero `WarningsNotAsErrors`/`NoWarn`/`AnalysisLevel`
across all 18 csproj, no `.globalconfig`, one `BannedSymbols.txt`. `.claude/rules/csharp.md:87`
records an explicit precedent of declining `WarningsNotAsErrors` for CS8032, so reaching for it
here contradicts a documented decision.

Related: [[msbuild-analyzer-gate-vacuous-without-rebuild]], [[msbuild-non-vacuity-which-pattern-to-count]].
