# People Deserialize After Fix — Finding B test 3 + sibling (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Command: `vstest.console.exe "ToDoModel.Test\bin\Debug\ToDoModel.Test.dll" /InIsolation /Tests:People_Deserialize_CanDeserializePatternCorrectly,People_DeserializeShortcut_CanDeserializePatternCorrectly /Logger:trx;LogFileName=people-after-fix.trx`
(VS18 vstest.console.exe; MSYS_NO_PATHCONV=1.)

EXIT_CODE: 0

Output Summary:
- Total tests: 2. Passed: 2. Failed: 0.
- `People_Deserialize_CanDeserializePatternCorrectly` PASSED [269 ms] — the Finding B fix in `WrapperScoDictionary.ToDerived()` now reconstitutes `Config` from the untyped `JObject` under `TypeNameHandling.None`, so `people.Config.Disk.FileName == "pplkey.json"` (previously `""`).
- `People_DeserializeShortcut_CanDeserializePatternCorrectly` PASSED [10 ms] — the working `PeopleScoConverter`/shortcut reference path is unchanged and still green; the JObject fallback only runs when the reflective Config lookup returns null, so the shortcut/typed path is not affected.

## Execution-environment note (`/InIsolation` required — NOT a source/scope change)

- Without `/InIsolation`, ALL `PeopleScoDictionaryNewTests` methods (including the working shortcut sibling) fail at the MSTest `Setup` with `System.IO.FileNotFoundException: Could not load file or assembly 'System.Threading.Tasks.Extensions, Version=4.2.0.1' ... at Moq.Async.AwaitableFactory..cctor()`. The default vstest test host does not apply `ToDoModel.Test.dll.config`'s binding redirect (`0.0.0.0-4.2.4.0 -> 4.2.4.0`) to Moq's dependency chain in this environment.
- This was proven to be PRE-EXISTING and independent of the cycle-5 production edits: with both `WrapperScoDictionary.cs` and `FilePathHelper.cs` edits stashed (reverted to clean HEAD `0883d0f7`) and rebuilt, `People_DeserializeShortcut_CanDeserializePatternCorrectly` STILL failed at the same `Setup`/STTE exception. The edits were then restored.
- `/InIsolation` makes vstest honor the test assembly's app.config binding redirects, the Moq cctor resolves `System.Threading.Tasks.Extensions`, and the tests execute their bodies and PASS. `/InIsolation` is a vstest execution flag, not a change to any of the four authorized production files or any test. No `[Ignore]` was added; no assertion was weakened; no timing hack was introduced.
