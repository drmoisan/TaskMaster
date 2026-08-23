# P5 Open Coordinator Preservation Reconciliation

Timestamp: 2026-07-22T10:25:00Z

Command: One deterministic read-only PowerShell reconciliation verified P5-T121 through P5-T130 checkmarks; validated the seven required named artifacts and all four required fields in each; compared every current protected path/hash/line count against the exact P5-T122 ordered baseline; reversed only the Host guard and three identity-literal deltas in memory and re-hashed them; enumerated the five-class test, project-include, threshold, exclusion, package, runsettings, configuration, filter, designer, and assertion invariants; verified P5-T120, P5-T132, P5-T158, and P5-T159 remained unchecked; and ran git diff --check against the two authorized files.

EXIT_CODE: 0

Output Summary: PASS. P5-T121 through P5-T130 are complete. Every required named artifact exists and contains Timestamp, Command, numeric EXIT_CODE, and Output Summary fields. P5-T121 is valid expected-failure evidence with 35 passed and 2 intended failures; P5-T130 is passing replacement evidence with 37 passed, 0 failed, and 0 skipped. Exactly BreadcrumbDropDownHost.cs and BreadcrumbDropDownIntegrationTests.cs differ from P5-T122, and in-memory reversal of only the authorized Host guard and three identity literals reproduces both exact baseline hashes. Host remains 472 lines, the integration test remains exactly 500 lines with ten tests, and the inventory remains 5+10+8+4+10. All remaining protected invariants match. Mapping: P5-T120 -> P5-T130. P5-T120 is eligible to be marked complete by replacement evidence. P5-T132, P5-T158, and P5-T159 remain pending.

## Evidence prerequisites

| Task | Evidence | Result |
|---|---|---|
| P5-T121 | p5-open-coordinator-preservation-fail-before.2026-07-22T10-16.md | Valid expected failure: 35 passed, 2 intended failed, 0 skipped |
| P5-T122 | p5-open-coordinator-preservation-diagnosis.2026-07-22T10-18.md | Protected baseline and diagnosis complete |
| P5-T126 | p5-open-coordinator-preservation-change-ledger.2026-07-22T10-22.md | Exact two-file delta complete |
| P5-T127 | p5-open-coordinator-preservation-csharpier.2026-07-22T10-23.md | Two-file formatter stable |
| P5-T128 | p5-open-coordinator-preservation-analyzers.2026-07-22T10-23.md | Analyzer build passed |
| P5-T129 | p5-open-coordinator-preservation-nullable.2026-07-22T10-24.md | Nullable build passed |
| P5-T130 | p5-open-coordinator-pass-after.2026-07-22T10-24.md | 37/37 passed |

Each artifact contains exactly one Timestamp field, one Command field, one EXIT_CODE field, and one Output Summary field.

## Final protected-surface state

- BreadcrumbDropDownHost.cs: SHA-256 17E186B7EE7F684A2310BD06A9787D29884F3CE6B4D25BD83EDB3000EC718C4A, 472 physical lines. One authorized public-boundary null guard. Removing only that guard in memory restores baseline SHA-256 7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28.
- BreadcrumbDropDownIntegrationTests.cs: SHA-256 B614351681956E2A9427412807FD6F22B270A6C7B6C6F2D331468241D4BFD990, exactly 500 physical lines, ten non-data-row tests. Reverting only two committed plain:0:A literals and one pending plain:1:B literal in memory restores baseline SHA-256 455A0B76AC2606FDA73FB0CF715FC370194CBCE5D5760A3DA99FB305538AFFDB.
- All other P5-T122 hashes are unchanged, including Popup operations, OpenCoordinator, ItemViewer breadcrumb, WebView surface factory, all four protected J1/preservation tests, both projects, both packages.config files, runsettings, coverage.config, and ItemViewer.Designer.cs.
- The five-class inventory is 5+10+8+4+10.
- All ten relevant production/test Compile includes remain unchanged and unique.
- Thresholds remain Host at most 480 lines, integration test exactly 500 lines with no increase, and applicable measurable line coverage at least 90 percent.
- Exclusions remain the seven Popup adapter declarations and two ItemViewer breadcrumb declarations; no exclusion was added, removed, or widened.
- Pending-after-close remains null, output path remains A, selection publication remains zero, and focus return remains one.
- git diff --check returned 0.

## Replacement mapping

P5-T120 -> P5-T130

The focused preservation replacement is complete. It does not replace P5-T158 or P5-T159, which remain the later authoritative numeric-coverage gates.
