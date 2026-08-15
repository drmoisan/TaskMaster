# actionlint Pre-Change Baseline — Issue #553

- Timestamp: 2026-08-14T09-54 (local) / 2026-08-14T13:54:53Z (UTC session timestamp; command executed at 2026-08-14T14:0xZ)
- Task: [P0-T3]

Command (run from the repository root; actionlint auto-discovers `.github/workflows/`):

```powershell
$v = '1.7.7'; $dir = '<SCRATCH>\actionlint-553'
New-Item -ItemType Directory -Force $dir | Out-Null
Invoke-WebRequest "https://github.com/rhysd/actionlint/releases/download/v${v}/actionlint_${v}_windows_amd64.zip" -OutFile "$dir\actionlint.zip"
Expand-Archive "$dir\actionlint.zip" -DestinationPath $dir -Force
& "$dir\actionlint.exe" -no-color
```

`<SCRATCH>` = `C:\Users\DANMOI~1\AppData\Local\Temp\claude\C--Users-DanMoisan-repos-TaskMaster-wt-2026-08-14T09-01\012c26d5-57f2-4f08-bc74-bf50a60b1e4e\scratchpad`

EXIT_CODE: 0

## Output Summary

- **Exit 0, zero findings.** actionlint produced no output, which is its clean
  result form.
- Files linted (auto-discovered from `.github/workflows/`, confirmed by
  `Get-ChildItem .github\workflows -Filter *.yml`): `ci.yml` and
  `codex-web-setup-test.yml` — the two workflow files that exist pre-change. The
  five `_*.yml` callees do not exist yet; they are authored in Phase 1 and linted
  by [P2-T3] and [P5-T1].
- Tool provenance, from `actionlint.exe -version`:
  ```
  1.7.7
  installed by downloading from release page
  built with go1.23.4 compiler for windows/amd64
  ```
  This is the same version the `actionlint` job downloads and runs inside CI
  (`ci.yml` line 33, `version=1.7.7`), so the local gate and the CI gate use
  identical tool behavior.
- Binary location: `<SCRATCH>\actionlint-553\actionlint.exe`. No file was written
  to `$env:TEMP` or inside the repository.

## Flag-Form Correction Recorded During This Task

The first execution of this task used the plan's original command string
`actionlint.exe -color never`, which failed:

```
could not read "never": open never: The system cannot find the file specified.
EXIT_CODE=3
```

Root cause: actionlint 1.7.7's `-color` is a **boolean** flag ("Always enable
colorful output"); it accepts no value. Go's flag parser therefore consumed
`-color` as the boolean and treated `never` as a positional FILE argument, which
does not exist. The correct suppression form is the separate boolean `-no-color`
("Disable colorful output").

The executor halted at [P0-T3] rather than substituting a command silently, the
plan was corrected at all three affected sites ([P0-T3], [P2-T3], [P5-T1]), and a
Conventions note was added to prevent reintroduction. This artifact records the
result of the **corrected** command, which is the command now in the plan of
record. Exit 3 was an argument-parsing error, never a lint finding: no version of
this task ever reported a workflow defect.

## Acceptance ([P0-T3])

- Artifact exists with `EXIT_CODE: 0`.
- The pre-change tree lints clean, so decomposition may proceed. This baseline is
  the comparison point for [P2-T3] (post-change) and [P5-T1] (final), both of
  which must also reach exit 0 — over seven files rather than two.
