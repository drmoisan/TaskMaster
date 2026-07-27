# Surface factory owner-thread pass-after

- Timestamp: `2026-07-23T14-08Z`
- Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~QuickFiler.Test.Viewers.BreadcrumbPopupControlDispatchTests /Logger:console;Verbosity=detailed`
- EXIT_CODE: `0`
- Output Summary: `Uninstrumented VSTest discovered 13 cases; 13 passed, zero failed, and zero skipped in 1.3643 seconds.`

## Instrumented confirmation

- Command: `dotnet-coverage collect --output docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/surface-factory-owner-thread.2026-07-23T14-08.cobertura.xml --output-format cobertura --settings coverage.config -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~QuickFiler.Test.Viewers.BreadcrumbPopupControlDispatchTests /Logger:console;Verbosity=detailed`
- EXIT_CODE: `0`
- Output Summary: `Instrumented VSTest discovered 13 cases; 13 passed, zero failed, and zero skipped in 2.6274 seconds; Cobertura output was created.`

## Required behavior

`SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUp` passed in both modes and
retained these assertions:

- the thrown exception is the original initialization failure;
- the error queue contains that failure exactly once;
- operation order is exactly `create`, `initialize`, `cleanup`;
- the control is disposed exactly once.

The worker-completion case retained exact order `create`, `initialize`, `core`,
`navigate`, `cleanup`; all recorded operations executed on the fixture creator thread;
the five required posts were observed; and the control and messenger were each disposed
once.

The ambient-null readiness-disposal case retained one post, creator-thread handler
detachment, no off-boundary entry, and no reported error. The fixture has no static member
or mutable shared state.

| Measurement | Value |
|---|---|
| Source SHA-256 | `3FE231161F91AB05FE28F4E99AE047B5D56B95FC8C09EF263B2FC4FB39676D38` |
| Physical lines | `480` |
| Discovered cases | `13` |
| `.Should()` assertions | `52` |
| Static members | `0` |
| Uninstrumented | `13/13 passed` |
| Instrumented | `13/13 passed` |
