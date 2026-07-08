# Baseline Inventory — .github/workflows/ci.yml (pre-edit, Issue #267)

- Timestamp: 2026-07-07T20-58

## (a) Setup NuGet / Restore solution — no cache step between them

```
62	      - name: Setup NuGet
63	        uses: nuget/setup-nuget@v2
64	        with:
65	          nuget-version: latest
66	
67	      - name: Restore solution
68	        shell: pwsh
69	        run: nuget restore $env:SOLUTION_PATH
```

No `actions/cache@v4` (or any cache) step exists between "Setup NuGet" (lines 62-65) and "Restore solution" (lines 67-69).

## (b) Setup CSharpier — no cache step before it

```
71	      - name: Setup CSharpier
72	        shell: pwsh
73	        run: dotnet tool restore
```

No cache step exists immediately before "Setup CSharpier" (lines 71-73); the preceding step is "Restore solution" (lines 67-69).

## (c) Both msbuild /t:Build invocations — no /m, two separate full-solution passes

```
79	      - name: Build with analyzers and code style enforcement
80	        shell: pwsh
81	        run: |
82	          & msbuild $env:SOLUTION_PATH /t:Build /p:Configuration=Debug `
83	              "/p:Platform=Any CPU" `
84	              /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
85	          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

87	      - name: Build with nullable warnings treated as errors
88	        shell: pwsh
89	        run: |
90	          & msbuild $env:SOLUTION_PATH /t:Build /p:Configuration=Debug `
91	              "/p:Platform=Any CPU" `
92	              /p:Nullable=enable /p:TreatWarningsAsErrors=true
93	          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
```

- First invocation (lines 79-85): carries `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. No `/m` flag present.
- Second invocation (lines 87-93): carries `/p:Nullable=enable /p:TreatWarningsAsErrors=true`. No `/m` flag present.
- These are two separate `/t:Build` full-solution passes, each carrying two of the four target properties (`EnableNETAnalyzers`, `EnforceCodeStyleInBuild`, `Nullable`, `TreatWarningsAsErrors`), confirming the redundant-second-compile inefficiency described in the issue.

## Confirmed additional context

- Repo root `dotnet-tools.json` (not under `.config/`) pins `csharpier` version `1.2.6`.
- No `actions/cache@v4` step exists anywhere in the pre-edit file.
