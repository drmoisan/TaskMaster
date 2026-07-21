# Parallel Build Flag Check — `/m` on Both Retained Passes (Issue #267, AC3)

- Timestamp: 2026-07-07T22-00
- Command: `grep -n "/t:Build /m" .github/workflows/ci.yml` and `grep -c "/t:Build" .github/workflows/ci.yml`
- EXIT_CODE: 0

## Quoted lines

```
98:          & msbuild $env:SOLUTION_PATH /t:Build /m /p:Configuration=Debug `
106:          & msbuild $env:SOLUTION_PATH /t:Build /m /p:Configuration=Debug `
```

- Line 98 belongs to the "Build with analyzers and code style enforcement" step (line 95: `- name: Build with analyzers and code style enforcement`).
- Line 106 belongs to the "Build with nullable warnings treated as errors" step (line 103: `- name: Build with nullable warnings treated as errors`).

## Confirmation

- `grep -c "/t:Build" .github/workflows/ci.yml` returns `2`: exactly TWO `msbuild ... /t:Build` invocations exist in the modified file (not one, not three).
- Both invocations carry `/m` immediately after `/t:Build`.

Satisfies AC3.
