# [P3-T2] Build the solution so the #482 red test is present

Timestamp: 2026-08-27T09-45
Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

## Result

```
5 Warning(s)
0 Error(s)
```

The 5 warnings are the pre-existing `System.Reactive 7.0.0` `packages.config` diagnostic baselined in
`[P0-T17]`.

## First attempt and its correction

The first invocation of this task returned exit code 1 with four `error CS1061` diagnostics, all of the
same shape:

```
QuickFiler.Test\Controllers\QfcItemController.NavigationTests.cs(445,22): error CS1061:
'KbdActions<char, KaChar, Action<char>>' does not contain a definition for 'Count' and no accessible
extension method 'Count' accepting a first argument of type 'KbdActions<char, KaChar, Action<char>>'
could be found (are you missing a using directive or an assembly reference?)
```

`QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` did not import `System.Linq`, so the
`Enumerable.Count(source, predicate)` extension over `KbdActions<>`'s `IEnumerable<UClass>`
implementation did not resolve. `using System.Linq;` was added to the file's using block and the build
then returned exit code 0. No production code was involved in the failure or the fix.

Output Summary: exit code 0; 0 errors; the four `CS1061` diagnostics from the first attempt were
resolved by adding the missing `System.Linq` using directive to the test file.
