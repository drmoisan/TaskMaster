# Baseline — candidate replacement format commands and CSharpier scope (#509, AC1, AC10)

Timestamp: 2026-08-10T14-45
Branch: bug/csharp-toolchain-gate-fidelity-512 (from origin/epic/build-ci-coverage-gate-fidelity-integration @ edf3d34c)

## Candidate replacement commands

| Form | Command | EXIT_CODE | Output Summary |
|---|---|---|---|
| A | `./.dotnet-sdk/dotnet.exe csharpier check .` (the `ci.yml:93` form) | 0 | `Checked 1517 files in 5183ms.` |
| B | `./.dotnet-sdk/dotnet.exe tool run csharpier check .` | 0 | `Checked 1517 files in 5703ms.` |
| C | `./.dotnet-sdk/dotnet.exe tool run csharpier --version` | 0 | `1.2.6` |
| D (documented) | `./.dotnet-sdk/dotnet.exe tool run csharpier .` | **1** | rejected; see `baseline-csharpier-documented-command.2026-08-10T14-25.md` |

Both working forms resolve the manifest-pinned 1.2.6 and both return EXIT 0 against the current tree.
The repository is presently CSharpier-clean, so acceptance criteria AC1 and AC3 are achievable without
any formatting churn.

`git status --porcelain` after the `check` runs showed only the untracked new feature folder,
confirming `check` is non-mutating as documented.

## Correction to issue #509's stated correctness trap

Issue #509 asserts: "a globally installed CSharpier (1.3.0 was present on the affected machine)
satisfies the bare-path form, so a session can format with an unpinned version and produce diffs that
disagree with CI."

**The first half of that claim does not reproduce.** A global CSharpier is present on this machine:

```
Get-Command csharpier -> C:\Users\DanMoisan\.dotnet\tools\csharpier.exe
global version: 1.3.0
```

Invoking the documented global form against it:

```
$ csharpier .
'.' was not matched. Did you mean one of the following?
-h
Required command was not provided.
Unrecognized command or argument '.'.
```

CSharpier 1.3.0 is also a v1.x release and therefore also requires a subcommand. The bare-path form
documented at `CLAUDE.md:192` and `.claude/rules/csharp.md:14` (`or csharpier . (if installed
globally)`) fails against **both** the pinned 1.2.6 and the global 1.3.0.

**The underlying risk is nonetheless real, but its mechanism is version skew rather than the bare-path
form succeeding.** A session that runs `csharpier format .` (a valid v1 invocation) resolves the
global 1.3.0 rather than the manifest-pinned 1.2.6, and a formatter version difference can produce
diffs that disagree with CI, which runs the pinned version via `dotnet tool restore`. The corrected
documentation should therefore direct sessions to the manifest-pinned tool explicitly, and the spec
should restate the trap accurately rather than repeating issue #509's wording.

## CSharpier file-type scope (evidence for AC10)

`CLAUDE.md:188` asserts: "`csharpier` is file-based and formats only `*.cs` without touching project
files."

Measured file populations in the worktree (excluding `obj/`, `bin/`, `packages/`, `.dotnet-sdk/`,
`.git/`, `node_modules/`):

| Extension | Count |
|---|---|
| `*.cs` | 1558 |
| `*.xml` | 220 |
| `*.config` | 38 |
| `*.csproj` | 18 |
| `*.targets` | 1 |
| `*.props` | 0 |

Direct probes of non-`.cs` files:

```
$ dotnet tool run csharpier check QuickFiler\packages.config
Checked 1 files in 425ms.
EXIT_CODE: 0

$ dotnet tool run csharpier check TaskMaster\Ribbon\RibbonExplorer.xml
Checked 1 files in 444ms.
EXIT_CODE: 0
```

Both non-`.cs` files were accepted and processed rather than ignored.

`.csharpierignore` contents:

```
**/evidence/**
*.cobertura.xml
*.coverage
*.coveragexml
*.trx
*.csproj
*.props
*.targets
```

### Assessment

The sentence at `CLAUDE.md:188` is factually wrong in its first clause and misleading in its second:

- "formats only `*.cs`" is **false**. CSharpier 1.2.6 also processes `*.xml` and `packages.config`.
- "without touching project files" is **true only because `.csharpierignore` explicitly excludes
  `*.csproj`, `*.props` and `*.targets`**, not because of any inherent CSharpier behavior. The ignore
  file's own comment repeats the same incorrect premise ("CSharpier formats C# source only (per
  CLAUDE.md C#1)"), so the error is propagated in two places.

This sentence sits directly adjacent to the defective format command at `CLAUDE.md:191`-`:192` and is
therefore within the epic's authorization to edit "the toolchain command text and its surrounding
rationale at the enumerated sites". Correcting it is in scope under AC10. Note that `packages.config`
being formatter-governed is a live operational hazard already recorded in repository agent memory: a
hand-edited single-line `<package />` entry fails `csharpier check` until reformatted.
