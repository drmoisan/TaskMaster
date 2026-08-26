# [P2-T3] Build after adding the test-support file

Timestamp: 2026-08-26T08-45

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build`

Emitted MSBuild command line (host paths replaced with `<WS>`):

```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" <WS>\TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /m
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

```
Build succeeded.
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.36
```

Exactly one project recompiled — `/out:obj\Debug\QuickFiler.Test.dll` — which is correct: P2-T1 and
P2-T2 added a file to `QuickFiler.Test` only and changed nothing that `QuickFiler` or any other
project depends on.

The 5 warnings are the unchanged System.Reactive `packages.config` baseline set from P0-T12; 0
errors.

### Acceptance verification

- `EXIT_CODE: 0`.
- `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` exists:

  ```
  -rwxr-xr-x  1328128  Aug 26 08:58  <WS>/QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
  ```

  Size grew from 1,327,104 bytes (the P1-T8 build) to 1,328,128 bytes, confirming the new
  `QfcCollectionControllerTestSupport` type was compiled in rather than the up-to-date check
  short-circuiting.

### Note on `/t:Build` here

`-Target Build` is used deliberately and is what the plan's Conventions specify as the precondition
for a test task (`pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build`). This is a
**compile precondition**, not an analyzer or nullable gate. The vacuous-gate hazard that requires
`/t:Rebuild` applies only to gates that assert on diagnostics, because MSBuild's up-to-date check
does not invalidate on a command-line `/p:` change. Here no `/p:` switch is being relied on, and the
up-to-date check correctly detected the changed source file and recompiled the affected project. The
authoritative analyzer and nullable gates for this phase run `/t:Rebuild` (P1-T6/P1-T7 for the
current tree; the final QC loop repeats them).

### P2-T1 and P2-T2 acceptance, recorded here for continuity

**P2-T1** — `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs`:

| Condition | Required | Measured |
|---|---|---|
| File exists | yes | yes |
| Line count | under 500 | **158** |
| `[TestMethod]` count | 0 | **0** |

It provides asserting `SetField`, `GetField`, `GetFieldInfo`, `SetStaticField`, `GetStaticField`,
and `InvokeNonPublic`, plus a `CreateUninitializedController` builder that injects `_digits = 1`.
Per D14 the helpers use the asserting form from `QfcItemController.TestSupport.cs:37-47`
(`field.Should().NotBeNull(...)` before use), not the silently-no-op `?.SetValue(...)` form at
`QfcCollectionControllerTests.cs:380-383`.

**P2-T2** — `QuickFiler.Test/QuickFiler.Test.csproj`:

```
$ git diff --stat -- QuickFiler.Test/QuickFiler.Test.csproj
 QuickFiler.Test/QuickFiler.Test.csproj | 1 +
 1 file changed, 1 insertion(+)
```

```diff
@@ -117,4 +117,5 @@
     <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
     <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
+    <Compile Include="Controllers\QfcCollectionController.TestSupport.cs" />
     <Compile Include="Controllers\QfcDatamodelTests.cs" />
     <Compile Include="Controllers\QfcDatamodelLivenessTests.cs" />
```

The three entries appear in the required order on consecutive lines, and the diff is **1 insertion,
0 deletions** — no other line of the csproj changed. The file remains CRLF-terminated with no BOM
added, so the insertion produces no whitespace churn for the sibling epic children that share this
item group.

Result: PASS.
