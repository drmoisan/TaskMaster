---
name: suggestion-severity-invisible-to-msbuild
description: Analyzer diagnostics configured at severity=suggestion (e.g. RS0030 banned symbols) are not emitted by a command-line msbuild at -v:m, so any AC about adding a banned DocID must demand positive observation plus a control
metadata:
  type: reference
---

`.editorconfig` in this repo holds several analyzer rules at `severity = suggestion` (RS0030,
RS0031, RS0035, the AsyncFixer set, ROS000x). A suggestion is emitted at info level:
`TreatWarningsAsErrors` cannot promote it, and it does not appear in a normal msbuild console log.

Verified prior measurement, worth citing rather than re-deriving:
`docs/features/archive/2026-06-28-qfc-banned-api-time-delay-seams-222/evidence/baseline/baseline-analyzer.md`
records that RS0030 for eight known banned call sites was "NOT surfaced as build warnings" and that
"suggestion-level diagnostics are not emitted by `-v:m`".

**Consequence for AC authoring.** `Microsoft.CodeAnalysis.BannedApiAnalyzers` silently ignores a
malformed or wrong-but-well-formed DocID: no diagnostic, no error. So "the build succeeded" and "no
RS0030 appeared" are each equally consistent with the DocIDs being correct and with them being
garbage. An AC that adds entries to `BannedSymbols.txt` must therefore:

1. require **positive observation** of a diagnostic at each enumerated call site, naming the
   observation channel and exact command;
2. pair it with a **control** — an already-banned symbol with known live usages (e.g.
   `Task.Delay(System.Int32)`, `DateTime.Now`) observed in the same run — so a channel that reports
   nothing is detected instead of being read as a pass;
3. treat the candidate channel (the `/p:ErrorLog=` SARIF property, or a scratch-branch build at
   `warning` severity that is never committed) as **unverified** and say so, rather than quietly
   downgrading the criterion to an absence check.

Also record honestly that adding a DocID while the severity stays `suggestion` delivers **zero**
build enforcement; the value is pre-staging the list so promotion later becomes a one-line change.
Promotion itself is blocked by the pre-existing usage backlog, and the gate that would break is the
nullable one (`_build-nullable.yml`, `/p:TreatWarningsAsErrors=true`), not `_build-analyzers.yml`,
which passes no `TreatWarningsAsErrors`. Related: [[ac-gates-verify-satisfiability]].
