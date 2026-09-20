# P1-T5 — `tests/scripts/dependencies/PackageGraph.Tests.ps1` authored

Timestamp: 2026-09-19T12-40

Command: static measurement over the authored file with `Get-Content` and `[regex]::Matches`, run
through `pwsh -NoProfile -Command`

EXIT_CODE: 0

## What was created

`tests/scripts/dependencies/PackageGraph.Tests.ps1`, written with the `Write` tool, never through a
Bash heredoc or redirection, per Scope Decision 4. The path mirrors the production path
`scripts/dependencies/PackageGraph.psm1` as Scope Decision 7 requires.

**32 `It` blocks**, one behaviour each, every one structured Arrange-Act-Assert with the three
sections marked by comment. Every fixture is an in-memory string, hashtable or array; the only
filesystem path the suite touches is the module it imports.

| Behaviour the plan names | `It` blocks covering it |
|---|---|
| manifest parsing of a reflowed multi-line entry and of an inline entry yielding identical records | `parses a reflowed entry and an inline entry into identical records`, plus `records the declared attributes in document order` and `reports an empty target framework when the attribute is absent` |
| rendering a parsed manifest to inline form | `renders a parsed manifest in canonical inline form`, plus `honours a caller-supplied indent` and `renders an empty document when no package records are supplied` |
| rendering being byte-identical when applied twice to its own output | `is byte-identical when applied a second time to its own output` for the manifest renderer, and `leaves a document that carries no reflowed start tag byte-identical` for the application-configuration renderer |
| project-file parsing of each of the five dependent element kinds | `parses an Import element ...`, `parses an Error element ...`, `parses a Reference element ...`, `parses a HintPath element ...`, `parses an Analyzer element ...`, plus the absent-primary-attribute case |
| `app.config` parsing of a binding redirect | `parses a binding redirect into an identity and a redirect range`, plus the no-`bindingRedirect` case |
| rejection of malformed input with an explicit `throw` | six cases: no packages root; no `id`; no `version`; whitespace-only project text; no configuration root; a dependent assembly with no assembly identity; and an unterminated start tag |

The remaining blocks cover manifest discovery through the injected delegate and the normalisation
driver through injected lister, reader and writer delegates, including the already-canonical case
in which the examined count is positive and the changed count is zero.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| The count of `Describe` and `Context` names matching the regex `AC\d` is exactly 0 | **0** | PASS |
| The file is at most 500 lines | **487** | PASS |
| No call to `New-TemporaryFile` | 0 occurrences | PASS |
| No call to `[System.IO.Path]::GetTempPath` | 0 occurrences of `GetTempPath` | PASS |
| No use of `$env:TEMP` | 0 occurrences of `env:TEMP` | PASS |
| No call to `Out-File` | 0 occurrences | PASS |

The prohibited token was measured as the regex `AC\d` and never as the bare two letters, per gate
rule 11: PowerShell matching is case-insensitive, so a bare `AC` matches ordinary words such as
`Package`, `exact` and `character`, and a count written against it could never be satisfied.

## Suite exercised (observation, verified formally at P1-T6)

A smoke run over this file alone reported `Passed=32 Failed=0 Skipped=0 Total=32` with the
JaCoCo `sourcefile` entry for `PackageGraph.psm1` at **164 covered, 0 missed — 100.00 percent
LINE**, and 217 of 217 instructions covered. The formal measurement against the plan's thresholds
is P1-T6.

Output Summary: `tests/scripts/dependencies/PackageGraph.Tests.ps1` is created at 487 lines with 32
Arrange-Act-Assert `It` blocks covering manifest parsing of reflowed and inline forms, canonical
rendering, render idempotence, all five project dependent element kinds, binding-redirect parsing,
seven malformed-input rejections, delegate-based discovery and the normalisation driver. Zero
`Describe` or `Context` names match `AC\d`, and the file contains no temporary-file or `Out-File`
call.
