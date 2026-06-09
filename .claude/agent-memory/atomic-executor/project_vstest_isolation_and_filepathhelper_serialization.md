---
name: vstest-isolation-and-filepathhelper-serialization
description: TaskMaster vstest /InIsolation needed for Moq-using test assemblies, and FilePathHelper.FilePath null-vs-empty-string serialization quirk
metadata:
  type: project
---

Two TaskMaster test/serialization facts that cost a remediation round if unknown.

**Why:** Both surfaced during issue #181 cycle-5 execution and are non-obvious from reading the code.

**How to apply:**

1. vstest `/InIsolation` is required for Moq-using test assemblies. Running `ToDoModel.Test.dll` (and other Moq-heavy assemblies) through `vstest.console.exe` WITHOUT `/InIsolation` makes every test in a `[TestInitialize]`/`Setup` that creates a Moq mock fail at `Moq.Async.AwaitableFactory..cctor()` with `System.IO.FileNotFoundException: Could not load file or assembly 'System.Threading.Tasks.Extensions, Version=4.2.0.1'`. The on-disk assembly IS 4.2.4.0 and the test assembly's `.dll.config` has a correct binding redirect (`0.0.0.0-4.2.4.0 -> 4.2.4.0`), but the default vstest test host does not apply the test assembly's app.config redirects to Moq's dependency chain. Adding `/InIsolation` makes vstest honor the app.config and the tests run. This failure is environmental and intermittent/host-load dependent (it sometimes reaches the assertion when many assemblies are co-loaded), so always pass `/InIsolation` for deterministic results. It is a vstest flag, not a code change.

2. `FilePathHelper.FilePath` is `""` when default-constructed but `null` after JSON deserialization of an empty helper. A `new FilePathHelper()` reports `FilePath == ""` (field initializer), but a `FilePathHelper` materialized from JSON with no/empty `FileName` ends up with `FilePath == null`, because the `FilePath` setter routes through `FilePathHelper_PropertyChanged`, whose empty-path branch leaves the backing field null. This breaks full-graph `BeEquivalentTo` equivalence (e.g. `ScoDictionaryConverterTests` integration tests assert `Config.Disk.FilePath` equals the default `""`; `SmartSerializable_Tests` asserts `Config.Disk.FilePath.Should().BeEmpty()`). Any code that reconstructs a `NewSmartSerializableConfig` from JSON (e.g. `WrapperScoDictionary.ToDerived()` reading a JObject `Config` under `TypeNameHandling.None`) must normalize empty disks' `FilePath` back to `""` to avoid regressing those tests. Tests that only read `FileName` (e.g. the People `pplkey` deserialization) are tolerant of the null and do not surface this.
