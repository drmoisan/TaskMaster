# Cache-Step Placement Check (Issue #267, AC1 and AC2)

- Timestamp: 2026-07-07T22-00
- Command: `grep -n "^      - name:" .github/workflows/ci.yml`

## Step ordering ("Setup NuGet" through "Verify formatting")

```
59:      - name: Setup MSBuild
62:      - name: Setup NuGet
67:      - name: Cache NuGet packages
75:      - name: Restore solution
79:      - name: Cache dotnet tools
87:      - name: Setup CSharpier
91:      - name: Verify formatting
```

## Confirmation

- "Cache NuGet packages" (line 67) precedes "Restore solution" (line 75). Satisfies the AC1 ordering requirement.
- "Cache dotnet tools" (line 79) precedes "Setup CSharpier" (line 87). Satisfies the AC2 ordering requirement.
- "Restore solution" (`nuget restore $env:SOLUTION_PATH`, line 76-77) carries no `if:` guard — confirmed by inspecting the two lines immediately following its `- name:` line (`shell: pwsh` / `run: nuget restore $env:SOLUTION_PATH`), with no `if:` key present. It executes unconditionally on both cache hit and cache miss.
- "Setup CSharpier" (`dotnet tool restore`, line 88-89) carries no `if:` guard — confirmed by inspecting the two lines immediately following its `- name:` line (`shell: pwsh` / `run: dotnet tool restore`), with no `if:` key present. It executes unconditionally on both cache hit and cache miss.

Satisfies AC1 and AC2.
