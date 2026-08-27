# [P0-T19] Compile-only build to produce the baseline test binaries

Timestamp: 2026-08-27T09-45
Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

`/t:Build` is used here deliberately and only to produce binaries. It is **not** used for the analyzer
gate or the nullable gate; those are `[P0-T17]`, `[P0-T18]`, `[P4-T4]`, and `[P4-T5]`, all of which use
`/t:Rebuild`.

## Summary counts (verbatim)

```
5 Warning(s)
0 Error(s)
```

The 5 warnings are the same pre-existing `System.Reactive 7.0.0` `packages.config` diagnostic recorded
under `[P0-T17]`.

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS.
- `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` exists (`Test-Path` returned `True`). PASS.

Output Summary: exit code 0; 0 errors, 5 pre-existing warnings; `QuickFiler.Test.dll` present in
`bin\Debug`.
