# R3 — The Push Gate Reads the Write Set

- Timestamp: 2026-09-20T08-56-40
- Task: [P3-T2]
- Finding: R3, **Blocking**
- EXIT_CODE: 0

## The New Output Line, Verbatim

`.github/workflows/dependabot-repair.yml:97`:

```
          "written-count=$(@($result.WrittenPath).Count)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
```

## The New Condition Line, Verbatim

`.github/workflows/dependabot-repair.yml:103`:

```
        if: steps.repair.outputs.written-count != '0'
```

It replaces `if: steps.repair.outputs.repair-count != '0'`.

## The Comment Naming the Defect

Added above the new output:

```
          # RepairCount counts per-project repair records only. Manifest normalisation and
          # binding-redirect reconciliation write files without producing one, so a run whose
          # only writes fall in those classes reports zero, skips the push, and discards the
          # repair while reporting "No repairs were applied." WrittenPath already carries
          # every written path, so the push gate reads that instead. repair-count stays
          # published because the disclosure body and the beyond-known-weak label read repair
          # records rather than the write set.
```

`repair-count` remains published. It is still the right quantity for the disclosure body and for
the beyond-known-weak label, both of which read repair **records**; it is the wrong quantity only
for the push gate, which must read the write **set**.

## Anchored Numstat

```
git diff --numstat HEAD -- .github/workflows/dependabot-repair.yml
```

```
9	1	.github/workflows/dependabot-repair.yml
```

| Clause | Required | Measured | Result |
|---|---|---|---|
| Additions | at least 1 | **9** | PASS |
| Deletions | exactly 1 | **1** | PASS |

The single deletion is the replaced condition line. The nine additions are the seven comment
lines, the new output line, and the new condition line.

The diff is anchored to `HEAD`. An unanchored `git diff` compares the worktree against the index
and would pass vacuously once anything is staged.

## Gate Rule 20 — Verification Route and Residual

**Verified without a live run:**

- by [P3-T1]'s `R3- gates` assertion, which failed before this edit because the file contained no
  `written-count=` line, and which [P3-T8] re-runs green;
- by [P3-T3]'s unit assertion over the **quantity** the new gate reads. That test drives the
  composition root through its injected delegates over a normalisation-only run and shows
  `RepairCount` at 0 while `@($result.WrittenPath).Count` is 1 — the exact state in which the old
  gate reads 0 and the new gate reads 1.

Those two together cover both halves of the finding: that the workflow now reads a different
quantity, and that the quantity it now reads is the one that moves.

**Unverifiable until the #914 credential exists:** that a real repair run pushes. No run of this
workflow has ever occurred, the repository holds zero Actions secrets, and the token step would
fail before reaching the commit step. Nothing here observes GitHub's evaluation of the `if:`
expression, only its text.

## Output Summary

The repair step publishes `written-count` from `@($result.WrittenPath).Count` and the commit step
gates on it. `repair-count` stays published for the disclosure and the label. Anchored numstat
records 9 additions and exactly 1 deletion.
