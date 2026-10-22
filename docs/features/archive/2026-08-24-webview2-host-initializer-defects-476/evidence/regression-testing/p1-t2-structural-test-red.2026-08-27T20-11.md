# [P1-T2] [expect-fail] — Structural Backing-Field Test, Red State

Timestamp: 2026-08-27T20-11

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~IsCoreInitialized_HasAnExplicitBackingField" "/Logger:trx;LogFileName=p1-t2-structural-test-red.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p1-t2
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 test discovered, 1 failed, 0 passed.** `Total tests: 1` / `Failed: 1` /
  `Test Run Failed.`
- The results directory holds exactly one TRX, `p1-t2-structural-test-red.trx`, whose `<Counters>`
  reads `total="1" executed="1" passed="0" failed="1"`.
- Observed failure, verbatim:

  ```
  Failed IsCoreInitialized_HasAnExplicitBackingField
   Expected explicitField not to be <null> because IsCoreInitialized must be backed by an explicit
   private field so Volatile.Read and Volatile.Write can be applied to it.
  ```

  This is the correct red state: `WebView2BreadcrumbHost.cs:54` is
  `public bool IsCoreInitialized { get; private set; }`, an auto-property, so no field named
  `_isCoreInitialized` exists and the first assertion fails before the
  `<IsCoreInitialized>k__BackingField` assertion is reached.

### Compile Include registration

`<Compile Include="Viewers\WebView2BreadcrumbHostContractTests.cs" />` was inserted immediately
after the `<Compile Include="Controllers\WebView2CoreInitializerTests.cs" />` entry, keeping the
WebView2 entries contiguous. `git diff` on `QuickFiler.Test/QuickFiler.Test.csproj` reports
**1 insertion, 0 deletions**, so no line was moved and the surrounding ItemGroup was not re-sorted.
The file's UTF-8 BOM and CRLF line terminators are preserved.

**Line-number drift recorded rather than glossed over.** The plan, the spec, and the research
artifact all name `QuickFiler.Test/QuickFiler.Test.csproj:159` as the
`Controllers\WebView2CoreInitializerTests.cs` entry and as the insertion anchor. In the current
worktree that entry sits at line **170**: the ItemGroup has grown by eleven lines since the research
was written. The insertion was therefore anchored on the `WebView2CoreInitializerTests.cs` entry
itself, which is what the plan's stated purpose requires ("to keep the WebView2 entries contiguous
and minimise the textual conflict surface"). Inserting after literal line 159 would have placed the
entry inside the `Controllers\QfcItemController.*` run, a region a concurrent sibling epic child
owns, and would have separated the new entry from the WebView2 neighbourhood. The anchor line's own
text is unchanged.

Discovery of the test also proves the `Compile Include` entry took effect: before the edit the file
was not compiled into the assembly and the filter would have matched nothing.

### Artifact hygiene

TRX written with an explicit `LogFileName=`. Host identifiers embedded by vstest were replaced in
place (`REPO-ROOT`, `USER`, `HOST`); `<Counters>` and the failure record are unmodified. The empty
`Deploy_*` deployment directory vstest created, whose name embeds the account and machine names, was
removed.
