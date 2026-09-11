---
name: utilitiescs-test-log4net-repository-unconfigured
description: MemoryAppender capture of production log output fails silently in UtilitiesCS.Test because that repository is never configured; set Hierarchy.Configured for the capture
metadata:
  type: project
---

A `MemoryAppender` attached from `UtilitiesCS.Test` captures nothing from a production
`logger.Debug` call, however correctly the appender is attached and however the logger level is set.
The capture harness must additionally set `Configured` to true on the `Hierarchy` for the duration of
the capture (and restore the previous value in the same `finally` that detaches the appender).

**Why:** the assembly-level `log4net.Config.XmlConfigurator` attribute lives on the `TaskMaster` and
`QuickFiler` assemblies only. Neither `UtilitiesCS` nor `UtilitiesCS.Test` carries one, so the
default log4net repository is created but never configured in a UtilitiesCS.Test host process, and
log4net's `Hierarchy.IsDisabled` reports *every* level disabled until `Configured` is true. The
production `Debug` call is therefore a no-op and the appender sees zero events. The equivalent
`TaskMaster.Test` helpers (`ApplicationGlobalsStartupTimingTests.AttachMemoryAppender`) work without
this step precisely because the TaskMaster assembly configures the repository.

**How to apply:** when a plan asks for an assertion over `[Df timing]` or any other production log
line from a UtilitiesCS test, expect an empty capture and add the `Configured` toggle. Do not trust a
Phase 0 "cross-assembly capture CONFIRMED" probe verdict on this point: issue #798's P0-T13 probe
recorded CONFIRMED and the same arrangement captured nothing at P3-T6. Verify capture with a
throwaway run of the two tests in isolation before concluding the instrumentation is missing.
Related: [[log4net-memoryappender-shared-per-type-across-parallel-classes]].
