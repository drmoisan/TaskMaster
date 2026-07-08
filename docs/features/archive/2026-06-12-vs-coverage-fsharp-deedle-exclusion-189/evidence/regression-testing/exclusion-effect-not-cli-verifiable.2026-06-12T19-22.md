# Phase 2 — Exclusion Effect Not CLI-Verifiable (AC2 effect / AC8)

Timestamp: 2026-06-12T19-22

Command: N/A — empirically established non-reproducible at CLI

EXIT_CODE: N/A

Output Summary:
The Visual Studio static-coverage `System.Security.VerificationException` ("Operation could destabilize the
runtime") CANNOT be reproduced via standalone `vstest.console`. Per the empirically established scope-change
finding `evidence/other/scope-change-finding.2026-06-12T19-45.md`:

1. Standalone `vstest.console` uses DYNAMIC coverage and does NOT exercise the Visual Studio STATIC coverage
   data collector (`datacollector://microsoft/CodeCoverage/2.0`) that throws the `VerificationException` when it
   instruments `FSharp.Core`/`Deedle`. The CLI therefore cannot reproduce the failure that the exclusion is meant
   to clear.

The executor did NOT attempt to reproduce the `VerificationException` at the CLI and did NOT block on it, per the
plan directive.

Verification split for AC2:
- AC2 CONTENT (the `<DataCollector friendlyName="Code Coverage">` Exclude block with the seven mirrored
  `<ModulePath>` entries, MSTest block preserved, no `enabled="true"`) is verified STATICALLY in P1-T2 — the file
  is well-formed and contains exactly the required exclusions.
- AC2 EFFECT (the exclusion actually preventing the `VerificationException` under VS "Analyze Code Coverage") is
  verified ONLY through AC8 (user action in Visual Studio), instrumented by the P2-T6 verification checklist.

No CLI reproduction is attempted or required.
