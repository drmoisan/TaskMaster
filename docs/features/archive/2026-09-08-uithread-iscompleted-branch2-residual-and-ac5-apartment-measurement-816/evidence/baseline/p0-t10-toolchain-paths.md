# P0-T10 — Visual Studio test runner and MSBuild resolution (baseline)

Timestamp: 2026-09-13T23-02

Command:

```
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"
```

EXIT_CODE: 0

Output Summary:

| Resolution | Paths returned | Leaf file name | Path with the Visual Studio installation root redacted |
|---|---|---|---|
| Test runner | 1 | `vstest.console.exe` | `<vs-install-root>\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` |
| MSBuild | 1 | `MSBuild.exe` | `<vs-install-root>\18\Community\MSBuild\Current\Bin\MSBuild.exe` |

Each resolution returned exactly one path, so neither FAIL condition (zero paths, or more than one
path) is met.

The installation root is written as the token `<vs-install-root>` so that no absolute host path and
no host account name is committed. The redaction replaces everything up to and including the
`Microsoft Visual Studio` path segment.

Every later task in this plan re-resolves these two paths inline with the same two commands, because
no shell variable survives between tasks.
