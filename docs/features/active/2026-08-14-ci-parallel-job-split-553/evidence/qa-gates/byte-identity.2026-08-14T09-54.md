# Byte-Identity of Transplanted Gate Blocks — Issue #553

- Timestamp: 2026-08-14T09-54 (local) / 2026-08-14T13:54:53Z (UTC session timestamp)
- Task: [P1-T6]
- Evidence pointer for: spec.md acceptance criterion 5, spec invariants 1, 2, 3, 4, 5, 8

Command (helpers dot-sourced from `<SCRATCH>\helpers-553.ps1` per the plan's
Helper-persistence convention):

```powershell
. "<SCRATCH>\helpers-553.ps1"
Test-BlockContained '<pre-split>\actionlint-steps.txt' '.github/workflows/_actionlint.yml'      'actionlint-steps'
Test-BlockContained '<pre-split>\format-step.txt'      '.github/workflows/_format-check.yml'    'format-step'
Test-BlockContained '<pre-split>\analyzer-step.txt'    '.github/workflows/_build-analyzers.yml' 'analyzer-step'
Test-BlockContained '<pre-split>\nullable-step.txt'    '.github/workflows/_build-nullable.yml'  'nullable-step'
Test-BlockContained '<pre-split>\vstest-step.txt'      '.github/workflows/_mstest-coverage.yml' 'vstest-step'
Test-BlockContained '<pre-split>\upload-step.txt'      '.github/workflows/_mstest-coverage.yml' 'upload-step'
```

`<pre-split>` = `docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/other/pre-split`
(the reference blocks extracted from the **pre-split** `.github/workflows/ci.yml`
by [P0-T5], before `ci.yml` was rewritten).

EXIT_CODE: 0

## Output Summary — the six canonical checks

```
BYTE-IDENTICAL: actionlint-steps
BYTE-IDENTICAL: format-step
BYTE-IDENTICAL: analyzer-step
BYTE-IDENTICAL: nullable-step
BYTE-IDENTICAL: vstest-step
BYTE-IDENTICAL: upload-step
```

All six gated blocks pass. `Test-BlockContained` throws on failure, so six
`BYTE-IDENTICAL` lines and exit 0 constitute a complete pass.

## Demonstration, not assertion — SHA-256 of the reference block vs the same span inside the callee

`Test-BlockContained` proves containment. The table below independently proves
**equality of content** by locating each reference block inside its callee,
slicing exactly `reference.Length` characters at the located offset, and hashing
both sides. A single changed character anywhere in a block changes its digest.

Both sides are LF-normalized (CRLF → LF) before hashing, and the reference's
trailing newline is trimmed. Line endings are the only permitted difference; git
manages them via `* text=auto` in `.gitattributes` with `core.autocrlf=true`, and
both the pre-split `ci.yml` and every new callee are CRLF in the working tree.

| Block | Callee file | Located at | Length | Lines | SHA-256 (reference == callee span) | Result |
| --- | --- | --- | --- | --- | --- | --- |
| `actionlint-steps` | `_actionlint.yml` | L16 | 455 chars | 14 | `73f620a346c1b7ea71ff1b0d42f8bc4cc5fcfc6f751a59b3c57d145aab5035e5` | MATCH |
| `format-step` | `_format-check.yml` | L39 | 89 chars | 3 | `06b59782a55eff65b25a281224856122ac1f6846edc7388ceb5550ad4f5e4f5b` | MATCH |
| `analyzer-step` | `_build-analyzers.yml` | L47 | 341 chars | 7 | `3b1739dc5b6f769f9570b7eed9ff6e69e08e6254b58d6e3426fe6c625c402238` | MATCH |
| `nullable-step` | `_build-nullable.yml` | L47 | 903 chars | 14 | `321615d62e56dbb0b498d91c740c3d0c4c5286e8566d7cd80828cd398ff7a0b0` | MATCH |
| `vstest-step` | `_mstest-coverage.yml` | L54 | 1686 chars | 33 | `75ef35baabaf61768c2bbab953ecb91bdc0fa231ff14382576c485b29bedc9fa` | MATCH |
| `upload-step` | `_mstest-coverage.yml` | L88 | 261 chars | 9 | `894b0ce75a70c838b94ab6ef272b27280bca7ce8b4cd76b3801e9468f9a3c195` | MATCH |

In every row the reference digest and the callee-span digest are the same value,
which is why a single column suffices. Six of six blocks match.

## Fidelity by construction

Each callee was assembled by a script that **read the extracted reference file
from disk and appended its lines**, rather than by re-typing the block. The
transplanted text therefore could not drift during authoring; the checks above
confirm the result. The same technique was used for the shared setup steps
(checkout, `setup-dotnet`, `setup-msbuild`, `setup-nuget`, both caches,
`nuget restore`, `dotnet tool restore`), which are sliced directly from
`ci.yml.pre-split.txt` even though only the six blocks above are formally gated.

## Critical fragment presence with exact line citations

Spec invariants 2, 3, 5, and 8 name specific fragments that must survive the move.
Each is verified by literal (`-SimpleMatch`) search in the authored callee:

| Fragment (spec invariant) | Callee file | Line | Count | Expected |
| --- | --- | --- | --- | --- |
| `/t:Rebuild` rationale comment, first line (inv. 2) | `_build-nullable.yml` | L50 | 1 | 1 |
| `/t:Rebuild` rationale comment, last line (inv. 2) | `_build-nullable.yml` | L56 | 1 | 1 |
| `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` (inv. 5) | `_build-analyzers.yml` | L53 | 1 | 1 |
| `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` (inv. 5) | `_build-nullable.yml` | L60 | 1 | 1 |
| `$_.FullName -match "\\bin\\$($env:BUILD_CONFIGURATION)\\" -and` (inv. 8) | `_mstest-coverage.yml` | L72 | 1 | 1 |
| `$_.FullName -notmatch '\\obj\\' -and` (inv. 8) | `_mstest-coverage.yml` | L73 | 1 | 1 |
| `$_.FullName -notmatch '\\ref\\'` (inv. 8) | `_mstest-coverage.yml` | L74 | 1 | 1 |
| `throw "No test assemblies found` — zero-assembly guard (inv. 3) | `_mstest-coverage.yml` | L79 | 1 | 1 |
| `throw "MSTest execution failed` — failure guard (inv. 1) | `_mstest-coverage.yml` | L85 | 1 | 1 |
| `/EnableCodeCoverage /InIsolation /Logger:trx` (inv. 1) | `_mstest-coverage.yml` | L83 | 1 | 1 |
| `name: test-results` (inv. 4) | `_mstest-coverage.yml` | L92 | 1 | 1 |
| `if: always()` (inv. 4) | `_mstest-coverage.yml` | L89 | 1 | 1 |

Twelve of twelve present at the expected count. The `/t:Rebuild` rationale
comment's first and last lines are both cited, so the whole seven-line comment
moved with its step rather than being truncated.

The complete comment as it now stands in `_build-nullable.yml` (L50-L56):

```
          # Use /t:Rebuild (not /t:Build) so this step always performs a genuine full
          # recompile. Enforcement now relies entirely on each file's own #nullable
          # enable pragma (the repo's per-file opt-in convention; UtilitiesCS.csproj and
          # SVGControl.csproj carry no project-level <Nullable> element) plus
          # /p:TreatWarningsAsErrors=true. MSBuild's incremental up-to-date check does
          # not invalidate on this command-line property change alone, so a plain
          # /t:Build would silently skip recompilation and never enforce this gate.
```

## What is deliberately NOT byte-identical

One step in the new pipeline has no pre-split counterpart and is therefore not
gated by this artifact: the plain `Build solution` step in `_mstest-coverage.yml`
(L46-L51), which the MSTest job needs because it no longer inherits build output
from a preceding gate in the same job. Per spec, it carries **no** analyzer or
warning-promotion properties, verified by literal search:

| Property that must be absent | Match count in `_mstest-coverage.yml` | Required |
| --- | --- | --- |
| `EnableNETAnalyzers` | 0 | 0 |
| `TreatWarningsAsErrors` | 0 | 0 |

This keeps the analyzer and nullable gates as the sole enforcers of their
respective criteria and prevents the MSTest job from silently duplicating or
weakening them.

## File sizes (500-line limit, `.claude/rules/general-code-change.md`)

| File | Lines |
| --- | --- |
| `.github/workflows/_actionlint.yml` | 29 |
| `.github/workflows/_format-check.yml` | 41 |
| `.github/workflows/_build-analyzers.yml` | 53 |
| `.github/workflows/_build-nullable.yml` | 60 |
| `.github/workflows/_mstest-coverage.yml` | 96 |

All well under 500. [P5-T4] audits the full set including `ci.yml` and the README.

## Acceptance ([P1-T6])

- Artifact exists; all six containment checks pass and are independently
  corroborated by matching SHA-256 digests.
- This is the evidence pointer for spec AC 5 ("the four gate commands and the
  actionlint step are byte-identical to their pre-split counterparts, including
  the `/t:Rebuild` rationale comment, the `$LASTEXITCODE` guards, and the
  zero-test-assembly `throw` guard"), checked off in [P5-T10].
