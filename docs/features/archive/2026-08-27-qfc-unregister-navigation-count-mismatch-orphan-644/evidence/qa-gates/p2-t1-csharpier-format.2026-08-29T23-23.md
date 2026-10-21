# [P2-T1] — CSharpier Format Gate

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P2-T1]
Working directory: `<repo-root>` (the repository root of this worktree)
Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0

Redaction note: no absolute host path, account name, or machine name appears in this artifact.
The repository root is written as `<repo-root>`. The before/after porcelain listings were
staged through a session scratchpad outside the repository, whose path is not reproduced here.

## Toolchain prerequisite

The repo-local .NET SDK pinned by `global.json` (8.0.205, resolved through `.dotnet-sdk`) was
absent in this worktree, so `dotnet tool run` failed with a shim message before this gate could
run. It was installed once with
`pwsh -NoProfile -ExecutionPolicy Bypass -File ./scripts/vscode/Install-RepoDotNetSdk.ps1`,
then `dotnet tool restore` restored the manifest-pinned CSharpier:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier
Restore was successful.
```

CSharpier is therefore the manifest-pinned 1.2.6, invoked through `dotnet tool run`, matching
the version `.github/workflows/ci.yml` uses. No globally installed CSharpier was used.

`.dotnet-sdk/` is matched by `.gitignore` line 350 (`.dotnet*/`), confirmed with
`git check-ignore -v .dotnet-sdk`, so installing it does not perturb the porcelain observation
below. The porcelain line count was 14 both before and after the install.

## Why the exit code alone is not the observation

`csharpier format .` is a write-mode command. It exits 0 both when it rewrites files and when
it changes nothing, so its exit code is identical on a clean run and on a repairing one. Three
further observations are therefore recorded.

### Observation 1 — the console line the command printed

```
Formatted 1562 files in 2051ms.
```

1562 files were processed. `csharpier format .` is file-based and processes `*.cs`, `*.xml`,
and `packages.config` across the whole tree; `*.csproj`, `*.props`, and `*.targets` are held
out by `.csharpierignore`.

### Observation 2 — repository-wide porcelain, before and after

Command: `git status --porcelain -- . ':!.claude/agent-memory'`
EXIT_CODE: 0 (both captures)

The scope is repository-wide rather than scoped to `QuickFiler.Test`, because the command's
blast radius is repository-wide: a rewrite outside the test project would be invisible to a
narrower observation, and the following `csharpier check .` would then pass over an
already-normalised tree. The `.claude/agent-memory` exclusion is the only exclusion; it is
required because that path carries unrelated modifications authored by other agents that would
otherwise make the comparison non-deterministic, and it excludes no C# source.

Result: the two listings are byte-identical. A `diff` of the before and after captures returned
exit 0 with no output. Both listings contain 14 lines.

The listing is not empty, and is not expected to be. The `[P1-T1]` and `[P1-T6]` edits are
already present in both the before and the after listing; identity between the two is what this
acceptance asserts. The before listing is:

```
 M QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
 M docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/research/research.2026-08-29T07-55.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/code-review.2026-08-29T23-06.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t1-cr1-edit.2026-08-29T23-23.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t2-pa7-research-edit.2026-08-29T23-23.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t3-pa7-policy-audit-edit.2026-08-29T23-23.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t4-pa7-verification-line-edit.2026-08-29T23-23.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t5-pa7-sweep.2026-08-29T23-23.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t6-cr1-line222-edit.2026-08-29T23-23.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/feature-audit.2026-08-29T23-06.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/policy-audit.2026-08-29T23-06.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-inputs.2026-08-29T23-23.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-plan.2026-08-29T23-23.md
```

The after listing is identical to it.

### Observation 3 — SHA-256 of the digits test file, before and after

Command: `(Get-FileHash -Algorithm SHA256 -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Hash`
EXIT_CODE: 0

Before: `13068BC448EE8723FB0A3A01225CA990471EF8062CD77259C0A1CF799C832408`
After:  `13068BC448EE8723FB0A3A01225CA990471EF8062CD77259C0A1CF799C832408`
Identical: yes

This hash check is required because the porcelain comparison alone cannot detect a rewrite of
this particular file. `[P1-T1]` and `[P1-T6]` have already left it in ` M` status before this
task runs, so a further CSharpier rewrite of the same file — for example normalising a
mixed-line-ending file back to uniform line endings after an edit introduced an LF into an
otherwise-CRLF file — would change the file's content without changing its porcelain status
letter, and the before/after porcelain listings would stay identical across that rewrite. The
hashes being equal rules that out.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | `0` | `0` | PASS |
| Before/after repository-wide porcelain listings identical | identical | identical (`diff` exit 0, no output; 14 lines each) | PASS |
| Before/after SHA-256 of the digits test file identical | identical | identical (`13068BC4...9C832408`) | PASS |

No file anywhere in the tree changed state across the command, and neither hash differed, so no
loop restart is required and Phase 2 proceeds to `[P2-T2]`.

## Loop restart — pass 2

`[P2-T3]` failed on its first run with `EXIT_CODE 1` and 10 `CS0006` errors caused by a
pre-existing analyzer HintPath skew in the repository (documented in that task's artifact). The
plan directs that the loop restart from `[P2-T1]` when any step fails, so this task was re-run
after the environment was provisioned. The provisioning added only gitignored package
directories and modified no tracked file, confirmed with
`git status --porcelain -- '*.csproj' '*.config' '*.props' '*.targets'`, which returned empty.

Pass 2 observations:

- Command: `dotnet tool run csharpier format .`   EXIT_CODE: 0
- Console line: `Formatted 1562 files in 2400ms.`
- Porcelain before and after: byte-identical, `diff` exit 0 with no output, 16 lines each. The
  count is 16 rather than the 14 of pass 1 because this cycle's own `[P2-T1]` and `[P2-T2]`
  evidence artifacts were written into the feature folder between the two passes.
- SHA-256 of the digits test file before: `13068BC448EE8723FB0A3A01225CA990471EF8062CD77259C0A1CF799C832408`
- SHA-256 of the digits test file after:  `13068BC448EE8723FB0A3A01225CA990471EF8062CD77259C0A1CF799C832408`
- Identical: yes

All three acceptance clauses hold on pass 2 exactly as they did on pass 1.

## Output Summary

`dotnet tool run csharpier format .` exited 0 on both passes. Pass 1 printed
`Formatted 1562 files in 2051ms.` with byte-identical 14-line porcelain listings; pass 2, after
the mandated loop restart, printed `Formatted 1562 files in 2400ms.` with byte-identical
16-line porcelain listings. The SHA-256 of the edited digits test file was
`13068BC4...9C832408` before and after the command on both passes. CSharpier rewrote nothing:
the `[P1-T1]` and `[P1-T6]` edits were already formatter-clean. Format gate PASS.
