# QA Gate — Phase 5 #584 Corrections (P5-T14)

Timestamp: 2026-09-05T22-58

Command:

```powershell
# Check 1 — every EXIT_CODE field under #584/evidence carries a single signed integer
$f584 = 'docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584'
$all = Select-String -Path "$f584/evidence/*/*.md" -Pattern '^EXIT_CODE:'
$all.Count
($all | Where-Object { $_.Line -notmatch '^EXIT_CODE: -?[0-9]+$' }).Count
```

```powershell
# Check 2 — the six P5-T12 evaluative tokens plus the P5-T4 token, across five files
$scope = @(
    "$f584/spec.md",
    "$f584/policy-audit.2026-09-04T04-05.md",
    "$f584/feature-audit.2026-09-04T04-05.md",
    "$f584/code-review.2026-09-04T04-05.md",
    "$f584/evidence/qa-gates/p2-t3-file-size.md"
)
foreach ($t in 'honest and correct', 'was the right call', 'stronger than typical', 'Exemplary', 'model instance of the rule', 'comfortably inside', 'provable assertion-level') {
    (Select-String -Path $scope -SimpleMatch $t).Count
}
```

```powershell
# Check 3 — the touched path set under #584
git add -N -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
git diff --name-only pre-782-base -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
git status --porcelain --untracked-files=all -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
```

The `git add -N` span and the porcelain span are the companions required alongside the name-listing
diff. The name-listing diff enumerates tracked changes only, so on its own it could not observe a
path this phase created; the porcelain span supplies that observation and additionally reports the
status letter, which is what distinguishes a modification from a rename.

EXIT_CODE: 0

Output Summary:

## Check 1 — `EXIT_CODE:` field conformance

```text
TOTAL_EXIT_CODE_LINES=37
NON_CONFORMING=0
```

All 37 lines match `^EXIT_CODE: -?[0-9]+$`. Fifteen of the 37 were rewritten by P5-T10 and P5-T11;
the remaining 22 already carried the single-integer form and were not touched.

## Check 2 — evaluative tokens

| Token | Hits |
|---|---|
| `honest and correct` | 0 |
| `was the right call` | 0 |
| `stronger than typical` | 0 |
| `Exemplary` | 0 |
| `model instance of the rule` | 0 |
| `comfortably inside` | 0 |
| `provable assertion-level` | 0 |

```text
TOTAL_EVALUATIVE_HITS=0
```

## Check 3 — touched path set

```text
DIFF_PATH_COUNT=23
PORCELAIN_PATH_COUNT=23
ADDED_OR_DELETED_COUNT=0
```

Exactly 23 paths, identical in both spans, every one reported with status `M`. The set is exactly
the one the acceptance condition names:

**The four #584 documents (4)**

- `spec.md`
- `policy-audit.2026-09-04T04-05.md`
- `feature-audit.2026-09-04T04-05.md`
- `code-review.2026-09-04T04-05.md`

**The four non-S3-5 evidence files (4)**

- `evidence/regression-testing/p1-t4-expect-fail.md`
- `evidence/qa-gates/p3-t1-analyzer-build.md`
- `evidence/qa-gates/p2-t3-file-size.md`
- `evidence/issue-updates/issue-584.2026-09-02T09-02.md`

**The fifteen S3-5 files (15)**

From P5-T10: `evidence/qa-gates/p4-t6-quickfiler-tests.md`,
`evidence/qa-gates/p2-t2-nullforgiving-removed.md`,
`evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md`,
`evidence/qa-gates/p1-t5-donotparallelize.md`, `evidence/qa-gates/p4-t1-format.md`,
`evidence/qa-gates/p3-t5-no-timing-tokens.md`,
`evidence/other/p3-t4-progresstrackerasync-unmodified.md`, `evidence/other/p5-t10-footprint.md`,
`evidence/baseline/p0-t13-parallel-bucket-census.md`,
`evidence/baseline/p0-t14-reflective-dispatcher-census.md`,
`evidence/baseline/p0-t5-toolchain-resolution.md`.

From P5-T11: `evidence/baseline/p0-t2-uithread-rederivation.md`,
`evidence/baseline/p0-t3-progresstrackerasync-rederivation.md`,
`evidence/baseline/p0-t4-test-rederivation.md`, `evidence/baseline/p0-t6-mcp-probe.md`.

No path outside that set appears, and no path is listed as added or deleted. No file was renamed and
no existing `Timestamp:` value was altered.

## Recorded deviation — the P5-T10 premise did not hold for three files

P5-T10 states that "for the other ten, the recorded per-command values are all `0`, so the single
integer is `0`". Measured against the tree, that premise is false for three of the ten. Their
recorded per-command breakdowns each contained a value of `1`:

| File | Command whose recorded value was `1` | Reason recorded in the original artifact |
|---|---|---|
| `evidence/qa-gates/p2-t2-nullforgiving-removed.md` | command 1 | `git grep` exits 1 on zero matches |
| `evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md` | commands 2 and 3 | `git grep` exits 1 on zero matches |
| `evidence/baseline/p0-t13-parallel-bucket-census.md` | command 2 | `git grep` exits 1 on zero matches |

Each of those `1` values is the success outcome of a zero-match search gate, which is the same
situation the task text reasons about explicitly for `p3-t5-no-timing-tokens.md`.

**Disposition.** The task's instruction was followed literally: all ten carry `EXIT_CODE: 0`. The
instruction was not silently reinterpreted, and no recorded value was altered. Nothing is concealed
by it: in every one of the eleven rewritten files the original per-command breakdown is preserved
verbatim immediately below the field, so the constituent `1` values remain readable, and each field
is now introduced by a sentence stating that the integer is the gate's normalized outcome rather
than a single process exit status. The falsified premise is recorded here and is reported to the
caller rather than resolved by the executor, because changing which integer is written would change
what the gate measures.
