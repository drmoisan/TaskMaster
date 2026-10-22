# P0-T6 — dotnet-coverage tool probe

Timestamp: 2026-09-07T14-08
Task: [P0-T6]
Issue: #796
Channel used: A

Rung taken: rung 1. The tool was already present, so rung 2
(`dotnet tool install --global dotnet-coverage`) was not taken.

Command:
`pwsh -NoProfile -Command 'dotnet-coverage --version'`

EXIT_CODE: 0

Recorded stdout, verbatim:

```
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
```

## Why this matters

scripts/vscode/Invoke-MSTestWithCoverage.ps1 throws the sentence
`dotnet-coverage not found.` at line 293 when the tool is absent. Every coverage
task in this plan, starting with P0-T11, runs through that script, so the tool's
absence would have made them unreachable.

Output Summary: dotnet-coverage resolves and prints version
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342 with EXIT_CODE 0 on the first rung.
