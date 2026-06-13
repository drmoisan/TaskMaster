---
name: runsettings-datacollector-default-enabled
description: A <DataCollector> declared in TaskMaster.runsettings activates by default under CLI vstest even without /collect; enabled="false" then breaks /collect with an Initialize exception
metadata:
  type: project
---

In `.runsettings` `<DataCollectionRunSettings><DataCollectors>`, a declared `<DataCollector friendlyName="Code Coverage">` with NO `enabled` attribute is treated as ENABLED by CLI `vstest.console.exe` (VSTest 18.7.0, VS 18 Community). A normal `/Tests:... /Settings:<runsettings>` run (no `/collect`) then produces a `.coverage` attachment — coverage is NOT opt-in at the CLI just because `enabled="true"` was omitted.

Adding `enabled="false"` makes the no-`/collect` run produce no attachment, but then a `/collect:"Code Coverage"` run throws in `Microsoft.VisualStudio.Coverage.DynamicCoverageDataCollector.Initialize` / `OnFirstCollectorToInitialize` and exits 1 (the `/collect`-supplied collector conflicts with the declared `enabled="false"` collector).

**Why:** Discovered executing issue #189 (FSharp/Deedle coverage exclusion). The plan/AC assumed "omit enabled=true => opt-in"; empirically false at the CLI. The VS IDE distinguishes "Run Tests" vs "Analyze Code Coverage" as separate commands, so in the IDE the present block does not force coverage on plain Run Tests — the divergence is CLI-specific. This drove a scope-change halt at P2-T2.

**How to apply:** When validating runsettings coverage opt-in via CLI `/collect` vs no-`/collect` as a proxy for VS behavior, expect the CLI to activate a declared collector without `/collect`. Do not use `enabled="false"` to suppress it if a `/collect` path must also pass — the two conflict. Treat the CLI no-`/collect` attachment as a possible CLI-only artifact, not necessarily a VS-IDE opt-in violation. See [[project_vstest_isolation_and_filepathhelper_serialization]] for related vstest /InIsolation behavior.
