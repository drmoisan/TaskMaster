# Increment 1 — MSTest with Coverage (ToDoModel.Test)

Timestamp: 2026-06-14T08-22

Command: vstest.console.exe ToDoModel.Test/bin/Debug/ToDoModel.Test.dll /InIsolation /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~ToDoLoaderSetAndSaveTests|FullyQualifiedName~IDListGetNextToDoIDTests|FullyQualifiedName~ProjectEntryTests|FullyQualifiedName~BaseChangerRemainingBranchesTests"
(vstest 18.7.0; /InIsolation required for Moq-referencing MSTest assemblies in this repo. Raw
.coverage converted to cobertura via `dotnet-coverage merge` into artifacts/csharp/inc1.cobertura.xml,
which is gitignored — not copied into docs per evidence-hygiene.)

EXIT_CODE: 0

## Output Summary

Total tests: 41. Passed: 41. Failed: 0. Total time: ~2.27s. No hang, no dialog, deterministic.
Test breakdown: ToDoLoaderSetAndSaveTests 14, IDListGetNextToDoIDTests 6,
BaseChangerRemainingBranchesTests 12, ProjectEntryTests 9.

Production-class line-rate after Increment 1 (from inc1.cobertura.xml; whole-class including
methods not targeted by this increment, e.g. Outlook-bound members):
- ToDoModel.BaseChanger: 0.9692 (96.92%)
- ToDoModel.IDList: 0.2256 (22.56%)
- ToDoModel.ProjectEntry: 0.3039 (30.39%)
- ToDoModel.Data_Model.ToDo.ToDoLoader: 0.3125 (31.25%)

Note: IDList/ProjectEntry/ToDoLoader whole-class rates remain low because those classes contain
large Outlook-bound regions (RefreshIDList, GetItemsWithRootIdAsync, the GetOrLoad/Load family,
the dialog-bound ProjectID setter) that are out of scope / #197-exempt-adjacent and intentionally
not exercised. The targeted-method coverage is recorded in inc1-coverage-delta.
