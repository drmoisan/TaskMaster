# Issue #270 Acceptance-Criteria Update Mirror

Timestamp: 2026-07-07T22-29

PostedAs: body (local `issue.md` in the feature folder only; NOT posted to GitHub — no posting directive was given to this executor)

## Acceptance criteria state written to `issue.md`

- [x] AC1: `OlToDoItems_ItemChange` no longer contains `catch (System.Exception) { throw; }`; a fault from the awaited `ToDoEvents.OlToDoItems_ItemChange` call is logged (full exception, via the existing `logger`) and contained, with no exception escaping the `async void` method.
- [x] AC2: `OlInboxItems_ItemAdd` no longer contains `catch (System.Exception) { throw; }`; a fault from the awaited `ProcessMailItemAsync` call is logged (full exception, via the existing `logger`) and contained, with no exception escaping the `async void` method.
- [x] AC3: The logged output preserves the original exception object (message and stack), so a previously-lost `sectionGroupName` `ArgumentException` becomes observable in the log rather than being silently swallowed or rethrown.
- [x] AC4: A deterministic MSTest regression test (Moq + FluentAssertions, no COM/network/temp files) drives each handler path with an injected collaborator that throws a synthetic exception and asserts the handler contains and logs it (does not throw). The test fails against the pre-fix `catch { throw; }` and passes after the fix.
- [ ] AC5: NOT satisfied. Format, analyzers (no new warnings), and nullable/type-check (no new diagnostics on touched files) pass, and changed-line coverage does not regress (new core methods 100% / 92.86%). However the full MSTest suite is red (201/202): the pre-existing test `OlInboxItemsItemAdd_WhenProcessingThrows_RethrowsThroughSynchronizationContext` in `AppEventsCoverageExpansionTests.cs:80` asserts the now-removed rethrow. Left unchecked pending remediation.
- [ ] AC6: NOT checked. Production scope is clean (only `AppEvents.ReadinessHookup.cs`; the config trigger and `RibbonViewer` handlers remain untouched follow-ups). Left unchecked because the end-state file set is not yet settled: the 500-line-ceiling split added `AppEventsTests.Helpers.cs` and a `TaskMaster.Test.csproj` `<Compile Include>` (mechanical wiring), and closing AC5 will additionally require editing `AppEventsCoverageExpansionTests.cs` (a file outside this plan's authorized set).

## Verification references

- AC1/AC2: `git grep` confirms no `catch (System.Exception) { throw; }` remains in `AppEvents.ReadinessHookup.cs`; behavior verified by pass-after run.
- AC3: `logger.Error(message, ex)` in both core methods; tests assert `LoggingEvent.ExceptionObject` is the injected exception.
- AC4: fail-before `evidence/regression-testing/fail-before.2026-07-07T22-18.md`; pass-after `evidence/regression-testing/pass-after.2026-07-07T22-20.md`.
- AC5: `evidence/qa-gates/format-final.*`, `analyzer-final.*`, `typecheck-final.*`, `test-final.*`, `coverage-delta.*`.
