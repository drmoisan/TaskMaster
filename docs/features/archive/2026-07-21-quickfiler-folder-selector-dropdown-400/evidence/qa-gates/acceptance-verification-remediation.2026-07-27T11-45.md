# P10-T1 acceptance verification remediation

Timestamp: 2026-07-27T11:45Z

This reconciliation does not use the superseded 2026-07-21 acceptance-verification or spec-checkbox artifacts as approval. It reads the retained behavioral test evidence together with the current P8/P9 successor evidence named below. Historical failed boundaries remain historical: P8-T55 is the source-range inventory, P8-T66 is the isolation diagnosis, and P8-T73 remains unchecked; P8-T82 and P9-T56 through P9-T61 are the current passing successors.

| AC | Direct current evidence and command/result | Result |
| --- | --- | --- |
| AC-1 | `pass-after-probability-upgrade.2026-07-21T16-19.md` and `pass-after-html-asset.2026-07-21T17-04.md` verify committed scored-row projection and unchanged formatter output; P8-T82's two direct eight-assembly VSTest runs each pass 6,056/6,056. | PASS |
| AC-2 | `pass-after-html-asset.2026-07-21T17-04.md` verifies the single accessible dropdown and no collapsed overflow; P8-T82 revalidates the test corpus at 6,056/6,056 twice. | PASS |
| AC-3 | `runtime-selector-toggle-thread-affinity.2026-07-22T01-29.md` provides the P5/P6 owner-thread and toggle evidence; `pass-after-popup-host.2026-07-21T16-37.md` verifies owned non-topmost host behavior; P9-T60 independently verifies direct adapter boundaries and production seams with `P9_T7_AUDIT: PASS`. | PASS |
| AC-4 | `pass-after-popup-host.2026-07-21T16-37.md` records the active-monitor geometry/placement cases; P8-T82 direct determinism passes twice. | PASS |
| AC-5 | `pass-after-selector-domain.2026-07-21T16-14.md` records closed Up/Down commit, skip, clamp, and no-wrap behavior; P8-T82 passes twice. | PASS |
| AC-6 | `pass-after-selector-domain.2026-07-21T16-14.md` records original/pending session isolation, selectable navigation, and active visibility; P8-T82 passes twice. | PASS |
| AC-7 | `pass-after-selector-domain.2026-07-21T16-14.md` records one-time commit, close, projection, and focus behavior; P9-T44's 19 focused cases pass 19/19 with zero failed/skipped. | PASS |
| AC-8 | `pass-after-selector-domain.2026-07-21T16-14.md` and `pass-after-popup-host.2026-07-21T16-37.md` record rollback-only automatic close behavior; P8-T82 passes twice. | PASS |
| AC-9 | `pass-after-selector-domain.2026-07-21T16-14.md` records preserved Left/Right transitions; P8-T82 passes twice. | PASS |
| AC-10 | `pass-after-probability-upgrade.2026-07-21T16-19.md` and `pass-after-selector-domain.2026-07-21T16-14.md` cover synchronous, resolved, unresolved, empty, and provider-failure score/identity retention; P8-T82 passes twice. | PASS |
| AC-11 | `pass-after-probability-upgrade.2026-07-21T16-19.md` records atomic replacement, in-flight selection, readback consistency, and stale-generation rejection; P8-T82 passes twice. | PASS |
| AC-12 | `pass-after-html-asset.2026-07-21T17-04.md` and `issue-400-integrated.2026-07-21T17-08.md` cover surface-mode state and bridge routing; P8-T82 passes twice. | PASS |
| AC-13 | `pass-after-html-asset.2026-07-21T17-04.md` and `pass-after-popup-host.2026-07-21T16-37.md` cover theme, listbox semantics, pending focus, and deterministic return; P8-T82 passes twice. | PASS |
| AC-14 | `pass-after-popup-host.2026-07-21T16-37.md` covers lazy reuse, reset, disposal, and no callbacks; P9-T60 independently confirms the direct adapter seams and unexcluded lifecycle ownership. | PASS |
| AC-15 | `pass-after-selector-domain.2026-07-21T16-14.md` and `pass-after-popup-host.2026-07-21T16-37.md` cover empty/invalid/zero-space/init-failure/reuse edges; P8-T82 passes twice. | PASS |
| AC-16 | `issue-400-integrated.2026-07-21T17-08.md` records failure-first behavior families without prohibited dependencies; P8-T66 retains the initial diagnostic boundary and P8-T82 supplies two current direct 6,056/6,056, zero-fail/skip passes. | PASS |
| AC-17 | P9-T61 `nonnumeric-adapter-member-coverage-relative-output-final-diff-integrity.2026-07-27T11-39.md` proves only the authorized post-P9-T34 C# paths changed, at 327, 302, and 494 lines, with project/configuration inputs unchanged. | PASS |
| AC-18 | P9-T10 `ac18-nonnumeric-adapter-reconciliation.2026-07-27T07-10.md` confirms the 68-path SHA-256 `2B63B4B315A68A72F23F8D5CDA3A055CEEB314BB9ADB7929B291477E7C7504A9`; P9-T41/P9-T42/P9-T43/P9-T44 provide formatter, analyzer, nullable, and 19/19 focused successors; P9-T50 cleans the only derived setting; P9-T56/P9-T57 provide exact relative-output validation and 6,075/6,075 coverage; P9-T59 reports repository 84.5568% and measured types 90.5660% and 90.6977% with named members at 100%; P9-T60 independently passes bounded nonnumeric accounting; P9-T61 passes both final diff checks. | PASS |
| AC-19 | `issue-400-integrated.2026-07-21T17-08.md` identifies the existing regression families; P8-T82 passes the full eight-assembly direct corpus 6,056/6,056 twice; P9-T57 passes the full coverage successor 6,075/6,075 with no failed/skipped tests. | PASS |

Independent verification commands were `Test-Path` and `Select-String` over every cited artifact, `Get-FileHash` for the live `spec.md`, `git rev-parse HEAD`, and inspection of P8-T55, P8-T66/P8-T67/P8-T82, P9-T10/P9-T41 through P9-T44/P9-T50/P9-T56 through P9-T61 plan and evidence records. All cited current successor artifacts exist at HEAD/worktree state `47dcc98a4991467187adadcb39e99a4c53c2ca58`.

Output Summary: 19 of 19 acceptance criteria PASS with direct current evidence. P10-T2 is authorized to change checkbox markers only.
