# Host-identifier sanitisation sweep (scoped remediation)

Timestamp: 2026-08-26T10-11

Scope of this artifact: a scoped remediation executed immediately before Phase 6 of
`plan.2026-08-24T09-39.md`, committed together with P6-T7. It is not a plan task; it repairs an
artifact-hygiene violation discovered in already-committed evidence.

## Rule being enforced

No committed artifact may contain an absolute host path, a bare operator account name, or a machine
name. The approved placeholders are `<repo-root>`, `<user-profile>`, `<user>`, and `<host>`.

This artifact therefore names every search pattern **by class** and never reproduces the raw token.
Reproducing a token in the record of its own removal would recreate the violation the sweep exists
to close.

## Search scope

`SearchScope:`

1. Every file under `docs/features/active/qfc-collection-controller-defects-468`, recursively
   (`find <FEATURE> -type f`).
2. Every file this branch has touched relative to the epic integration base `61edc19b`
   (`git diff --name-only 61edc19b..HEAD`) plus every path reported by `git status --porcelain`,
   excluding `.claude/agent-memory/**` and `.claude/state/**`, which are not owned by this feature.

The union is 71 distinct files: 60 evidence artifacts and TRX files, `issue.md`, `spec.md`,
`plan.2026-08-24T09-39.md`, one research document, five `QuickFiler.Test` source files, one csproj,
and `QuickFiler/Controllers/QfcCollectionController.cs`.

`SearchPatterns:` four fixed-string, case-insensitive patterns, one per token class:

| # | Token class searched | Why this class |
|---|---|---|
| P1 | the operator's account name, standing alone | appears in `runUser`, TRX run `name`, `runDeploymentRoot`, and inside every absolute path |
| P2 | the machine name with its trailing digit dropped, so the pattern is a superset of the machine name itself | appears in the `computerName` attribute, the TRX run `name`, and the domain component of `runUser` |
| P3 | the drive-letter-plus-`Users` absolute-path prefix | the leading segment of every absolute path rooted in the user profile |
| P4 | the 8.3 short-name form of the account name | Windows emits this form in some tool output, and it is not matched by P1 |

`SearchResult:` none, for all four patterns. See the Output Summary table.

Command:

```
# Build the scope list.
find docs/features/active/qfc-collection-controller-defects-468 -type f > <scope>
git diff --name-only 61edc19b..HEAD >> <scope>
git status --porcelain | awk "{print \$2}" >> <scope>
# (.claude/agent-memory/** and .claude/state/** filtered out; list de-duplicated)

# Sweep, once per token class. <P1>..<P4> stand for the four literal patterns
# described in the table above; the literals themselves are deliberately not
# reproduced in this artifact.
for p in <P1> <P2> <P3> <P4>; do
    grep -ilF "$p" -- $(cat <scope>) | wc -l
done
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Post-remediation sweep, per pattern, over all 71 in-scope files:

| Pattern | Files with at least one hit | Total hits |
|---|---|---|
| P1 - account name | **0** | **0** |
| P2 - machine name (superset match) | **0** | **0** |
| P3 - `Users` absolute-path prefix | **0** | **0** |
| P4 - 8.3 short-name form of the account name | **0** | **0** |

No hit remains anywhere in scope. There is no deliberately-unfixed residue to list.

Pre-remediation the same sweep returned: P1 in 17 files, P2 in 0 files, P3 in 18 files, P4 in 0
files.

## What was remediated, and why it was still present

### 1. `evidence/qa-gates/p1-t8-suite.<ts>.md` - self-defeating documentation of a sanitisation

Introduced by this branch at commit `63eebd47`. The artifact correctly documented the TRX
sanitisation it had performed, but its substitution table's *From* column and its `BEFORE:` example
lines quoted the raw values verbatim. The effect was that the artifact recording the removal of the
identifiers was itself a committed file containing all of them.

Remediation: the substitution table now names the class of token each substitution targeted
(workspace-root prefix, user-profile path, machine name, account name) and states the ordering
constraint between them; the `BEFORE:` lines are removed and the already-clean `AFTER:` lines are
retained, so each substitution remains auditable at its exact position. The residual-scan table's
*Pattern* column is likewise described by class. The artifact's `Timestamp:`, `Command:`,
`EXIT_CODE:`, and `ExpectedExitCode:` fields, its counts, and its narrative findings are unchanged.

### 2. Sixteen committed TRX files - a case-sensitivity gap in the original substitution

This is the material finding. The P1-T8 sanitisation was applied case-insensitively and
`p1-t8.trx` is clean. Every later TRX (`p2-t6`, `p2-t10`, `p2-t11`, `p2-t11-attempt1-flaky`,
`p3-t2`, `p3-t3`, `p3-t5`, `p3-t6`, `p4-t3`, `p4-t7`, `p4-t8`, `p5-t1`, `p5-t2`, `p5-t5`, `p5-t6`,
`p5-t6-attempt1-flaky`) was sanitised **case-sensitively** against the mixed-case spelling of the
workspace root. That cleared the TRX header - the machine-name pattern P2 was already at zero hits
across all sixteen files before this remediation - but vstest writes the `storage=` attribute of
every `<UnitTest>` element in **all-lower-case**, and the mixed-case substitution did not match it.

Consequence: one full-suite TRX carried the account name and the absolute worktree path once per
test, 946 times in `p5-t6.trx`.

Remediation: all sixteen files were rewritten in binary mode with case-insensitive substitutions,
in the order workspace-root prefix, user-profile path, machine name, account name. Byte-identical
handling otherwise; no BOM was added or removed and no line ending was altered. Substitutions
applied:

| TRX | Substitutions | Bytes removed |
|---|---|---|
| `qa-gates/p2-t11/p2-t11.trx` | 939 | 61,974 |
| `qa-gates/p2-t11/p2-t11-attempt1-flaky.trx` | 939 | 61,974 |
| `qa-gates/p3-t6/p3-t6.trx` | 941 | 62,106 |
| `qa-gates/p4-t8/p4-t8.trx` | 943 | 62,238 |
| `qa-gates/p5-t6/p5-t6.trx` | 946 | 62,436 |
| `qa-gates/p5-t6/p5-t6-attempt1-flaky.trx` | 946 | 62,436 |
| `regression-testing/p2-t6/p2-t6.trx` | 1 | 66 |
| `regression-testing/p2-t10/p2-t10.trx` | 1 | 66 |
| `regression-testing/p3-t2/p3-t2.trx` | 1 | 66 |
| `regression-testing/p3-t3/p3-t3.trx` | 1 | 66 |
| `regression-testing/p3-t5/p3-t5.trx` | 2 | 132 |
| `regression-testing/p4-t3/p4-t3.trx` | 1 | 66 |
| `regression-testing/p4-t7/p4-t7.trx` | 2 | 132 |
| `regression-testing/p5-t1/p5-t1.trx` | 1 | 66 |
| `regression-testing/p5-t2/p5-t2.trx` | 1 | 66 |
| `regression-testing/p5-t5/p5-t5.trx` | 3 | 198 |
| **Total** | **5,668** | **373,922** |

The substitution touches only path and identity attribute values. No `<Counters>` element, no
`outcome` attribute, and no test name was modified, so every count asserted by an earlier phase
artifact against these TRX files still reads the same value.

**Forward instruction for the remaining phases:** run every TRX sanitisation case-insensitively.
The mixed-case workspace-root spelling is not the only spelling vstest emits.

### 3. `evidence/baseline/p0-t14-tests-coverage.<ts>.md` - a quoted pattern name

One line recorded that the committed coverage XML contained zero occurrences of the absolute-path
prefix, and quoted that prefix in order to say so. The finding is correct and is retained; the
pattern is now named by class. No count changed.

### 4. `research/test-harness-feasibility.md` - pre-existing debt, not a branch regression

Line 10 stated the worktree root as an absolute path including the account name. This file is **not**
a product of this branch: it arrived at commit `0ac4b11b`, which is an ancestor of the epic
integration base `61edc19b`, so the violation predates `bug/qfc-collection-controller-defects-468`
and is being cleaned up in passing rather than being a regression this branch introduced.

Remediation: replaced with the `<repo-root>` placeholder form plus a `<user-profile>`-rooted
description of where such a worktree lives. The file is CRLF-terminated; all 826 CRLF line endings
were verified intact after the edit and the file carries no BOM before or after.

## Verification of encoding neutrality

- `research/test-harness-feasibility.md`: 826 CR bytes before, 826 after; no BOM before or after.
- `evidence/qa-gates/p1-t8-suite.<ts>.md` and `evidence/baseline/p0-t14-tests-coverage.<ts>.md`:
  LF-terminated, no BOM, unchanged in both respects.
- All sixteen TRX files: read and written through a raw byte layer; leading three bytes unchanged
  (no BOM present before or after), CRLF state unchanged.
