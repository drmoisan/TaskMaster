# Host-identifier sweep of the feature folder (P7-T8)

Timestamp: 2026-09-03T00-22

EXIT_CODE: 0

ResidualMatchCount: 0

FilesRewritten: 0

ExternalPathsRewritten: 0

XmlReparseFailures: 0

## Scope of the sweep

Every file under `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729`,
enumerated with `Get-ChildItem -Recurse -File`. The swept set contained **52** files at the time
of the substitution pass, and 53 once this artifact was written, since this artifact is counted in
the same set.

## The four searches

All four are case-insensitive and run over the whole swept set.

| # | Kind | Subject | How it was derived at run time |
|---|---|---|---|
| 1 | fixed string | account name | `Split-Path -Leaf $env:USERPROFILE` (9 characters) |
| 2 | fixed string | machine name | `$env:COMPUTERNAME` (10 characters) |
| 3 | fixed string | absolute workspace-root prefix | `(Resolve-Path .).Path` (77 characters), searched in both its native separator form and its forward-slash form |
| 4 | pattern | residual detector only | a single ASCII letter immediately followed by the two-character drive-root sequence, that sequence constructed at run time as `([string][char]58 + [string][char]92)` so that neither the plan nor this artifact contains the literal |

The workspace root was resolved from the absolute path of the worktree root rather than from a
`cd` into it, because this execution addresses the worktree by absolute path throughout. The value
resolved is identical to what `(Resolve-Path .).Path` returns from that root.

None of the four subjects is spelled literally anywhere in this artifact.

## Pre-substitution counts

| Search | Matching lines |
|---|---|
| 1 — account name | 0 |
| 2 — machine name | 0 |
| 3 — workspace-root prefix (native separator form) | 0 |
| 3 — workspace-root prefix (forward-slash form) | 0 |
| 4 — letter-anchored drive-root | 0 |

All four searches returned zero before any substitution, so no substitution was required and none
was performed. `FilesRewritten: 0` and `ExternalPathsRewritten: 0` follow from that, not from a
skipped pass.

### Positive control on the search harness

An all-zero result is also what a broken harness returns, so the harness was validated against two
strings known to be present in the swept set before the zero results were accepted:

| Control string | Matching lines |
|---|---|
| `Timestamp` | 59 |
| `coverage` | 52648 |

Both controls return large non-zero counts over the same file set with the same enumeration and
the same search cmdlet, which establishes that the four zero results are properties of the tree
rather than of the harness.

### The three known non-host occurrences

A search for the **bare** two-character drive-root sequence, without the letter anchor, returns
exactly three matching lines, which is the count the task text predicts:

| File | Matching lines |
|---|---|
| `plan.2026-09-02T08-59.md` | 2 |
| `research/research-729.2026-09-02T09-30.md` | 1 |

In the plan file the sequence is preceded by a straight quote and by a backtick; in the research
artifact it sits inside a regular-expression literal, preceded by an asterisk. None of the three is
a host path, and rewriting any of them would corrupt a recorded acceptance command or a research
citation. The letter-anchored form matches none of the three, which is why the residual detector
reports zero while the bare sequence reports three. This is the behaviour the letter anchor exists
to produce.

## The fourth substitution

The `<external-path>` substitution was not needed. It is load-bearing only when
`CoberturaProcessingState:` is `raw`, because a raw dotnet-coverage artifact embeds third-party
build-machine source paths that lie outside the workspace root and are therefore unreachable by
the workspace-root-prefix substitution. Both Cobertura artifacts in this feature folder declare
`CoberturaProcessingState: processed` (P0-T11 for the baseline, P6-T5 for the post-change run), and
a processed Cobertura carries no such path because the koverage conversion removes every
non-allowlisted package and rewrites each retained `class/@filename` to a repository-relative path.
That is confirmed empirically here: the letter-anchored detector returns zero matches over both
Cobertura documents. `ExternalPathsRewritten: 0`.

## XML well-formedness

Two XML-family files exist in the swept set, both `.cobertura.xml`. Neither was rewritten, so no
re-parse was required by the task. Both were nevertheless parsed with
`[xml](Get-Content -Raw -Encoding UTF8 $path)` so that `XmlReparseFailures: 0` records an
observation rather than a default:

| File | Parse result |
|---|---|
| `evidence/baseline/coverage-baseline.cobertura.xml` | well-formed |
| `evidence/qa-gates/coverage-final.cobertura.xml` | well-formed |

`XmlReparseFailures: 0`.

## Post-write re-scan

The residual count must be taken over the same file set including this artifact, so it is recorded
below after this artifact was written to disk rather than predicted before it.

The re-scan enumerated 53 files, one more than the substitution pass, the additional file being
this artifact. Results:

| Search | Matching lines |
|---|---|
| 1 — account name | 0 |
| 2 — machine name | 0 |
| 3 — workspace-root prefix (native separator form) | 0 |
| 3 — workspace-root prefix (forward-slash form) | 0 |
| 4 — letter-anchored drive-root | 0 |
| **Total** | **0** |

`ResidualMatchCount: 0`, taken over the swept set including this artifact.

Output Summary: The feature folder is clean of host identifiers. All four searches return zero
both before and after the pass, over 52 files at substitution time and 53 including this artifact.
No file needed rewriting, so `FilesRewritten: 0` and `ExternalPathsRewritten: 0`. Both Cobertura
artifacts are processed rather than raw, carry no out-of-workspace absolute path, and parse as
well-formed XML, so `XmlReparseFailures: 0`. The search harness was validated against two positive
controls before the zero results were accepted, and the three known non-host occurrences of the
bare drive-root sequence are correctly excluded by the letter anchor.
`ResidualMatchCount: 0`.
