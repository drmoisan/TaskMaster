# Build with the publication fix applied (Issue #824, task P2-T4)

Timestamp: 2026-09-09T15-36

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $msb = & "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msb TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" 2>&1 | Tee-Object -FilePath coverage/build-p2t4.log | Select-Object -Last 5'`

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:10.70
```

Counts read from `coverage/build-p2t4.log`:

| Pattern | Count |
|---|---|
| `: error [A-Z]+[0-9]+:` | 0 |
| `: warning CS86[0-9][0-9]:` | **0** |

Both counts are what this task requires.

Two diagnostics were specifically at risk and neither appeared:

- **CS0198**, "a static readonly field cannot be assigned to (except in a static constructor)", is an
  error and would have appeared in the first count if any assignment to either opcode-table field
  had survived outside the static constructor. The count of 0 is therefore also an independent
  confirmation of the P2-T2 assignment-site invariant, obtained from the compiler rather than from a
  text search.
- **CS8618**, "non-nullable field must contain a non-null value when exiting constructor", is the
  risk `spec.md` records as unverified in its Risks section and in its Assumptions block. The
  research asserted that a non-nullable `static readonly` field definitely assigned in the static
  constructor satisfies the compiler's null-state analysis but did not verify it by running a build.
  A count of 0 for `: warning CS86[0-9][0-9]:` verifies it here at the warning level.

The error form `: error CS86[0-9][0-9]:` is deliberately not asserted against this log:
`/p:TreatWarningsAsErrors=true` is absent from this command, so CS86xx diagnostics appear as
warnings and an error-form assertion would read 0 whatever the compiler decided. The error form is
asserted in P5-T7 against the nullable gate, which supplies that switch.

The log stays under `coverage/` and is not committed.
