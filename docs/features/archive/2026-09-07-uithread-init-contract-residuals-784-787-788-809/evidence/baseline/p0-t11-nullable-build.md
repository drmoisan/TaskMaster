# [P0-T11] Baseline nullable build

Timestamp: 2026-09-08T00-34

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Output Summary: the two trailing summary lines, verbatim:

```
    0 Warning(s)
    0 Error(s)
```

`/p:Nullable=enable` was not passed, because no project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so that property is a solution-wide opt-in that would conscript every file which has never adopted the `#nullable enable` pragma; CI omits it deliberately and omitting it loses no enforcement over any file that has opted in. `/t:Rebuild` was used rather than `/t:Build` for the reason recorded in [P0-T10].
