# P1-T3 — Phase 1 Evidence Schema Check (Issue #751)

Timestamp: 2026-09-03T14-36

Command:

```powershell
Select-String -Path 'docs\features\active\terminal-notification-hook-test-lacks-sync-barrier-751\evidence\regression-testing\*.md' -Pattern '^(Timestamp:|SearchScope:|SearchPatterns:|SearchResult:|## WhyFailingRunImpossible)'
```

EXIT_CODE: 0

## Output Summary — matched line set

```
fail-before-route-selection.2026-09-03T11-48.md:3: Timestamp: 2026-09-03T14-33
no-fail-before-rationale.2026-09-03T11-48.md:3: Timestamp: 2026-09-03T14-35
no-fail-before-rationale.2026-09-03T11-48.md:10: SearchScope: `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/regression-testing/`
no-fail-before-rationale.2026-09-03T11-48.md:12: SearchPatterns: `fail-before-exception.*.md`
no-fail-before-rationale.2026-09-03T11-48.md:14: SearchResult: none. At the time of this search that directory existed and contained exactly one entry,
no-fail-before-rationale.2026-09-03T11-48.md:20: ## WhyFailingRunImpossible:
```

Total matched lines: 6.

At the time of this scan the directory contained the two Phase 1 artifacts named above and this file had not
yet been written, so the scan covered exactly the two artifacts the acceptance concerns.

## Acceptance

| Required | Observed | Result |
|---|---|---|
| `Timestamp:` present in **both** artifacts | `fail-before-route-selection...:3` and `no-fail-before-rationale...:3` | PASS |
| `SearchScope:` present in `no-fail-before-rationale.2026-09-03T11-48.md` | line 10 | PASS |
| `SearchPatterns:` present in `no-fail-before-rationale.2026-09-03T11-48.md` | line 12 | PASS |
| `SearchResult:` present in `no-fail-before-rationale.2026-09-03T11-48.md` | line 14 | PASS |
| `WhyFailingRunImpossible` heading present in `no-fail-before-rationale.2026-09-03T11-48.md` | line 20, as the level-2 heading `## WhyFailingRunImpossible:` | PASS |

The heading matched in its level-2 Markdown form, which is the form the pattern requires and the form the
precedent artifact at
`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/no-fail-before-rationale.2026-09-02T10-30.md`
uses at its line 21. A plain field line would not have matched.
