# [P5-T3] Final QC loop, step 2 — linting by .NET analyzers

Timestamp: 2026-09-08T02-40

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

## Output Summary

```
    0 Warning(s)
    0 Error(s)
```

## Project-count comparison

The baseline value is located in `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/baseline/p0-t10-analyzer-build.md` by its `BASELINE_PROJECT_COUNT:` token.

| Quantity | Value |
|---|---|
| `BASELINE_PROJECT_COUNT:` recorded by [P0-T10] | 18 |
| Build-output arrow-line count observed here | 18 |

The two counts are equal, which is required: this delivery adds no project and removes none.

`/t:Rebuild` was used rather than `/t:Build`, because MSBuild's up-to-date check does not invalidate on a command-line `/p:` change and a warm `/t:Build` would return exit 0 with `CoreCompile` skipped on every project and no analyzer run at all.

TOOLCHAIN_LOOP_PASS: 1
