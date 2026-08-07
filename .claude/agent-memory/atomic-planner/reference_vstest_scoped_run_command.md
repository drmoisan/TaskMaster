---
name: reference-vstest-scoped-run-command
description: canonical scoped MSTest run for plans — vswhere-resolved vstest.console.exe + runsettings + /InIsolation + /TestCaseFilter (join with |, not OR); csharpier 1.2.6 needs `format`/`check` subcommands
metadata:
  type: reference
---

Executor-verified (#424 preflight, 2026-08-06) canonical scoped test-run form for plan tasks — `vstest.console.exe` is NOT on PATH in this environment:

```
$vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~<Name>"
```

- `/InIsolation` is mandatory for the Moq-based test assemblies (`scripts/vscode/Invoke-MSTest.ps1:54` already passes it).
- vstest 18.x rejects `OR` inside `/TestCaseFilter` — join clauses with `|`.
- From a bash shell, prefix `MSYS_NO_PATHCONV=1`.
- Neither `Invoke-MSTest.ps1` nor `Invoke-MSTestWithCoverage.ps1` accepts a test filter; the coverage script is reserved for full-suite coverage runs (see [[reference-invoke-mstest-with-coverage-script]]), and `Invoke-MSTest.ps1` still has the single-assembly `.Count` StrictMode defect (see [[reference-invoke-mstest-single-searchroot-defect]]).
- Every plan task that runs tests must state its command explicitly (or cite a plan Decisions-Record item that pins this form); "run the suite" with no command draws a preflight finding because the evidence schema requires a `Command:` field.

Related runnable-command fact from the same preflight: repo-root `dotnet-tools.json` pins **csharpier 1.2.6** whose commands are `format | check | pipe-files | server` — CLAUDE.md's `csharpier .` is v0 syntax and fails. Plans must use `dotnet tool run csharpier format .` and verify with `dotnet tool run csharpier check .` (consistent with [[csharpier-format-not-pipe-files-gate]]).
