# P4-T6 — AC12 No-Threshold-Lowered Gate

Timestamp: 2026-09-09T11-18
Task: [P4-T6]
EXIT_CODE: 0

## Command 1 — anchored diffstat

Command: `git diff --stat epic/review-residuals-2026-09-08-integration...HEAD -- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`
EXIT_CODE: 0

```
(no output)
```

## Command 2 — tree observation

Command: `git status --porcelain --untracked-files=all -- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`
EXIT_CODE: 0

```
(no output)
```

The two together cover both states: the anchored diff sees committed change and the porcelain status
sees uncommitted change. Both print nothing, so the file is unmodified in either.

## Command 3 — the failure message is retained verbatim

Command: `git grep -c -F -e 'is below the required 80' -- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`
EXIT_CODE: 0

```
scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1:1
```

One line, count 1, matching the P0-T11 baseline exactly.

## Command 4 — content hash

Command: `pwsh -NoProfile -Command '(Get-FileHash -Algorithm SHA256 -LiteralPath "scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1").Hash'`
EXIT_CODE: 0

```
00D96099A91DC7B431414F27F3A3FD3A6BDAE4F8D9B8961CEFEA5F1782ED79C2
```

This **equals** the value recorded in
`evidence/baseline/p0-t11-threshold-and-fixture-baseline.md`, so the file is byte-identical to its
pre-change state. `Assert-CoberturaLineCoverageThreshold` retains its existing comparison value and
its existing failure message.

## Standing finding, recorded and not actioned

This repository carries three written line floors simultaneously and one automated script gate that
implements the lowest of them:

| Source | Line floor |
| --- | --- |
| `CLAUDE.md` section UT2 | 80% repository-wide |
| `.claude/rules/general-unit-test.md` | 85% across all tiers |
| `.claude/rules/quality-tiers.md` | 85% uniform across T1 to T4 |
| `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, the only automated script gate | 80% |

There is no branch-coverage gate anywhere in script code. The divergence between the 80 percent
written in `CLAUDE.md` and enforced by the script, and the 85 percent written in the two rules files,
**predates this issue and is not caused by the aggregation defect**. It is recorded here as a
finding. No threshold is changed by this feature, and none may be: `spec.md` Non-Goal 4 and epic
Non-Goal 5 both state that a corrected figure falling below a threshold is a finding to record, not a
threshold to lower. Reconciling the divergence is a separate promotion; the archived feature
`docs/features/archive/2026-08-10-coverage-threshold-policy-reconciliation-494/` already covers that
ground and should be read first.

For completeness, the corrected first-party figures this delivery measured over the committed
Cobertura document from issue 798 are 84.56% line and 79.24% branch (see
`evidence/qa-gates/p3-t3-real-document-corroboration.md`). The line figure clears the 80 percent
floor the script enforces and sits **below** the 85 percent floor the two rules files state. That
gap is present under both the corrected and the defective computation — the defective computation
reports 84.55% — so it is not created or widened by this correction. It is recorded as a finding
here, per AC12, and nothing was changed to accommodate it.

Output Summary: The anchored diff and the porcelain status both print nothing, the failure-message
search returns one line with count 1, and the SHA-256 equals the P0-T11 baseline value
`00D96099...ED79C2`. `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` is byte-identical to
its pre-change state. No threshold constant, analyzer severity or policy requirement was lowered,
weakened or deleted anywhere in this branch, and the pre-existing 80-versus-85 line-floor divergence
is recorded as a finding rather than actioned.
