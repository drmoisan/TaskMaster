# claude-md-cites-ciyml-for-moved-toolchain-commands (Plan)

- **Issue:** #564
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T08-58
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug (spec.md is the AC source; no user-story.md)

## Scope Summary

This is a documentation-only, single-file fix. The only file this plan writes is `CLAUDE.md`. It replaces three stale .github/workflows/ci.yml citations with the reusable workflow file that actually contains the cited command, verified directly against the current tree:

| `CLAUDE.md` line | Claim | Current citation | Corrected citation |
| --- | --- | --- | --- |
| 194 | CSharpier pinned-version parity | .github/workflows/ci.yml | .github/workflows/_format-check.yml |
| 202 | Analyzer `/t:Build /m` step | .github/workflows/ci.yml | .github/workflows/_build-analyzers.yml |
| 210 | Nullable `TreatWarningsAsErrors` step ("Build with nullable warnings treated as errors") | .github/workflows/ci.yml | .github/workflows/_build-nullable.yml |

No command text changes on any of the three lines. No other file is edited.

## Scope & Non-Goals (binding)

- **In scope:** the three citation edits in `CLAUDE.md` listed above, plus their commit and verification.
- **Out of scope / explicitly no-change:**
  - .claude/rules/csharp.md — confirmed unchanged; re-verified in Phase 0 (P0-T5) to contain zero occurrences of ci.yml and zero occurrences of `workflows`.
  - Any path under .claude/**, .codex/**, .agents/**, config/blast-radius.json, config/orchestration-routing.json — these are published from the upstream `drm-copilot` repository with zero templating and are silently overwritten on the next push-down; no task in this plan touches them.
  - Any .github/workflows/*.yml file — this plan corrects a citation, not workflow behavior; no workflow YAML is edited.
  - Python commands, scripts/dev_tools/, or extensions/ paths — this repository has none of these; no task references them.
  - Any C# toolchain gate (CSharpier, msbuild analyzers, msbuild nullable, vstest) — no *.cs/*.csproj/*.props/*.targets file is touched, so the toolchain loop in .claude/rules/general-code-change.md does not apply. Final QA substitutes a targeted text/diff verification loop (Phase 3), which this plan states explicitly rather than silently omitting the toolchain section.
  - A failing-regression-test phase — the General Code Change Policy's Bugfix Workflow step 1 requires a failing regression test for code defects; this defect has no executable behavior (spec.md Test Strategy: "a Markdown-text change has no unit-test surface, so no MSTest/Pester/pytest additions apply"). Phase 0's baseline citation capture (P0-T5) substitutes as the false-before evidence for AC1–AC4, and Phase 1 records this exemption explicitly rather than omitting the rung.

## Acceptance Criteria Traceability (spec.md, 7 items)

| AC | Text (spec.md `## Acceptance Criteria`) | Delivered by | Verified by |
| --- | --- | --- | --- |
| AC1 | `CLAUDE.md` line ~194 cites .github/workflows/_format-check.yml (not ci.yml) for the CSharpier pinned-version claim. | P2-T1 | P3-T1 |
| AC2 | `CLAUDE.md` line ~202 cites .github/workflows/_build-analyzers.yml (not ci.yml) for the analyzer `/t:Build /m` claim. | P2-T2 | P3-T2 |
| AC3 | `CLAUDE.md` line ~210-211 cites .github/workflows/_build-nullable.yml (not ci.yml) for the nullable `TreatWarningsAsErrors` claim, retaining the step-name parenthetical "Build with nullable warnings treated as errors". | P2-T3 | P3-T3 |
| AC4 | No remaining citation to .github/workflows/ci.yml exists in `CLAUDE.md` for any of the three relocated commands. | P2-T1, P2-T2, P2-T3 (collectively remove all three) | P3-T4 |
| AC5 | .claude/rules/csharp.md is unchanged. | N/A (no task edits this file) | P0-T5 (baseline), P3-T5 (post-change) |
| AC6 | No file under .claude/**, .codex/**, .agents/**, config/blast-radius.json, or config/orchestration-routing.json is changed. | N/A (no task edits these paths) | P3-T6 |
| AC7 | The command text in all three cited bullets is unchanged (only the file citation is edited). | P2-T1, P2-T2, P2-T3 (each edit is scoped to the citation token only) | P3-T7 |

---

### Phase 0 — Policy Reads & Baseline Capture

- [ ] [P0-T1] Read `CLAUDE.md` (repository root) in full.
- [ ] [P0-T2] Read .claude/rules/general-code-change.md in full.
- [ ] [P0-T3] Read .claude/rules/general-unit-test.md in full.
- [ ] [P0-T4] Write the Phase 0 policy-read evidence artifact at `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/baseline/phase0-instructions-read.2026-09-02T08-58.md` containing `Timestamp:`, `Policy Order: CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md`, and the explicit list of the three files read in P0-T1–P0-T3. Record that no language-specific rule file (e.g. .claude/rules/csharp.md) is read as an applicability trigger because no *.cs/*.csproj/*.props/*.targets file is in scope for this change.
- [ ] [P0-T5] Run `Select-String -Path CLAUDE.md -Pattern 'ci\.yml'` and `Select-String -Path .claude/rules/csharp.md -Pattern 'ci\.yml|workflows'`; record the verbatim pre-fix text of `CLAUDE.md` lines 194, 202, and 210 (each currently containing the literal token .github/workflows/ci.yml) and confirm the .claude/rules/csharp.md search returns zero matches, in a baseline evidence artifact at `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/baseline/claude-md-citations-baseline.2026-09-02T08-58.md`. Include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` for both commands. This is the false-before evidence for AC1–AC4 and the pre-fix baseline for AC5.

### Phase 1 — Scope Boundary Declarations

- [ ] [P1-T1] Write a scope-boundary evidence artifact at `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/baseline/scope-boundary-declarations.2026-09-02T08-58.md` recording: (a) this plan performs zero edits under .claude/**, .codex/**, .agents/**, config/blast-radius.json, and config/orchestration-routing.json; (b) .claude/rules/csharp.md requires no edit, per the zero-match search recorded in P0-T5; (c) no C# toolchain gate (CSharpier, msbuild analyzers, msbuild nullable, vstest) applies because the only file this plan edits is `CLAUDE.md`, a Markdown file, and Phase 3 substitutes a targeted text/diff verification loop in its place; (d) no failing-regression-test task is included because the defect has no executable behavior, per spec.md's Test Strategy section ("a Markdown-text change has no unit-test surface"), and P0-T5's baseline citation capture stands in as the false-before evidence for AC1–AC4.

### Phase 2 — Implementation

- [ ] [P2-T1] In `CLAUDE.md` line 194, replace the citation token .github/workflows/ci.yml with .github/workflows/_format-check.yml within the sentence "Always invoke through `dotnet tool run` so the manifest-pinned version is used. Do not invoke a globally installed `csharpier`: a different global version produces diffs that disagree with .github/workflows/ci.yml, which runs the pinned version after `dotnet tool restore`." Leave every other word on the line unchanged.
- [ ] [P2-T2] In `CLAUDE.md` line 202, replace the citation token .github/workflows/ci.yml with .github/workflows/_build-analyzers.yml within the sentence ".github/workflows/ci.yml uses `/t:Build /m` for its analyzer step because a runner checkout is always cold; a local working tree is not." Leave every other word on the line unchanged, including the preceding `/t:Rebuild`/`/t:Build` explanation.
- [ ] [P2-T3] In `CLAUDE.md` line 210, replace the citation token .github/workflows/ci.yml with .github/workflows/_build-nullable.yml within the sentence "This is character-for-character the command in .github/workflows/ci.yml (step "Build with nullable warnings treated as errors"). Two properties of it are load-bearing and must not be "restored":" Retain the parenthetical step name "Build with nullable warnings treated as errors" and every other word on the line unchanged.
- [ ] [P2-T4] Commit the three-line edit to `CLAUDE.md` with message `docs(claude-md): repoint stale ci.yml citations to reusable workflow files (#564)`, so that `git diff origin/main...HEAD` reflects the change for Phase 3 verification.

### Phase 3 — Final Verification (substitutes the C# toolchain loop; no code/test files changed)

- [ ] [P3-T1] Run `Select-String -Path CLAUDE.md -Pattern '_format-check\.yml'` and confirm exactly one match, at `LineNumber` 194. Record `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (the matched line number and text) in `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/citation-verification-ac1.2026-09-02T08-58.md`. (AC1)
- [ ] [P3-T2] Run `Select-String -Path CLAUDE.md -Pattern '_build-analyzers\.yml'` and confirm exactly one match, at `LineNumber` 202. Record the same four fields in `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/citation-verification-ac2.2026-09-02T08-58.md`. (AC2)
- [ ] [P3-T3] Run `Select-String -Path CLAUDE.md -Pattern '_build-nullable\.yml'` and confirm exactly one match, at `LineNumber` 210; separately confirm that the same line 210 still contains the literal substring `Build with nullable warnings treated as errors`. Record both results in `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/citation-verification-ac3.2026-09-02T08-58.md`. (AC3)
- [ ] [P3-T4] Run `Select-String -Path CLAUDE.md -Pattern 'ci\.yml'` over the full file and confirm the match count is exactly 0. Record the count and `EXIT_CODE:` in `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/citation-verification-ac4.2026-09-02T08-58.md`. (AC4)
- [ ] [P3-T5] Run `git diff origin/main...HEAD -- .claude/rules/csharp.md` and confirm the output is empty (zero bytes / no output lines). Record the command and the empty result in `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/scope-verification-ac5.2026-09-02T08-58.md`. (AC5)
- [ ] [P3-T6] Run `git diff origin/main...HEAD --name-only` together with a companion `git status --porcelain`, and confirm the name-only output is exactly the single line `CLAUDE.md` with no entry under .claude/, .codex/, .agents/, config/blast-radius.json, or config/orchestration-routing.json. Record both command outputs in `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/scope-verification-ac6.2026-09-02T08-58.md`. (AC6)
- [ ] [P3-T7] Run `git diff origin/main...HEAD -- CLAUDE.md` and confirm the diff body contains exactly 3 removed lines (lines beginning with a single `-`, excluding the `---` file header) and exactly 3 added lines (lines beginning with a single `+`, excluding the `+++` file header) — proving no line other than 194, 202, and 210 changed, and that the command text on lines 192–193, 201, and 211–212 is unchanged. Record the diff and the counts in `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/line-scope-verification-ac7.2026-09-02T08-58.md`. (AC7)
- [ ] [P3-T8] Write a Final QA summary artifact at `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/final-qa-summary.2026-09-02T08-58.md` stating that no C# toolchain step (CSharpier format/check, msbuild analyzer rebuild, msbuild nullable rebuild, vstest) was run because P3-T6 confirms zero *.cs/*.csproj/*.props/*.targets files are in the diff, and listing the pass/fail result of P3-T1 through P3-T7.

---

## Planner Self-Review

`SELF-REVIEW: RE-DERIVED THIS PASS`

- `CLAUDE.md` | line 194 — read directly this pass via `Read` (offset 175, limit 45); citation text .github/workflows/ci.yml confirmed present verbatim in the CSharpier bullet.
- `CLAUDE.md` | line 202 — read directly this pass via the same `Read` call; citation text .github/workflows/ci.yml confirmed present verbatim in the analyzer bullet.
- `CLAUDE.md` | line 210 — read directly this pass via the same `Read` call; citation text .github/workflows/ci.yml and the parenthetical "Build with nullable warnings treated as errors" confirmed present verbatim.
- `CLAUDE.md` | full-file `ci\.yml` search — re-derived this pass via `Grep` (not carried forward from the research artifact); returned exactly 3 matches, at lines 194, 202, 210, matching the research artifact and the delegation prompt's stated line numbers.
- .claude/rules/csharp.md | full-file `ci\.yml|workflows` search — re-derived this pass via `Grep` (not carried forward); returned zero matches, confirming the out-of-scope declaration in Phase 1 (P1-T1) and the AC5 baseline in Phase 0 (P0-T5).
- .github/workflows/_format-check.yml — re-derived this pass via `Glob`; file exists in the current tree.
- .github/workflows/_build-analyzers.yml — re-derived this pass via `Glob`; file exists in the current tree.
- .github/workflows/_build-nullable.yml — re-derived this pass via `Glob`; file exists in the current tree.
- docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/spec.md — read directly this pass; confirmed exactly 7 `## Acceptance Criteria` items, matching the AC-INVENTORY below and the delegation prompt's stated count.
- docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/issue.md — read directly this pass; confirmed `Work Mode: full-bug` and the same three line/citation targets as spec.md.
- .claude/rules/plan-acceptance-gates.md — read directly this pass; confirmed the wrap-tolerant / anchored-diff / G8b-companion authoring requirements applied to Phase 3's `git diff` and `Select-String` tasks (ref operand present in every `git diff`; `git status --porcelain` companion present alongside every `--name-only` diff).

`PLANNER-INTERNAL-REVIEW: PASS`

`CITATION-TO-TREE: PASS`
`CITATION: CLAUDE.md | line 194 (.github/workflows/ci.yml in the CSharpier bullet)`
`CITATION: CLAUDE.md | line 202 (.github/workflows/ci.yml in the analyzer bullet)`
`CITATION: CLAUDE.md | line 210 (.github/workflows/ci.yml and parenthetical "Build with nullable warnings treated as errors")`
`CITATION: .claude/rules/csharp.md | full-file search, zero occurrences of ci.yml or workflows`
`CITATION: .github/workflows/_format-check.yml | file exists (Glob-confirmed)`
`CITATION: .github/workflows/_build-analyzers.yml | file exists (Glob-confirmed)`
`CITATION: .github/workflows/_build-nullable.yml | file exists (Glob-confirmed)`
`CITATION: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/spec.md | ## Acceptance Criteria section, 7 items`

`AC-TRACEABILITY: PASS`
`AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7`
`AC-MAPPING: AC1 | IMPLEMENTATION: P2-T1 | TESTS: P3-T1 | EVIDENCE: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/citation-verification-ac1.2026-09-02T08-58.md`
`AC-MAPPING: AC2 | IMPLEMENTATION: P2-T2 | TESTS: P3-T2 | EVIDENCE: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/citation-verification-ac2.2026-09-02T08-58.md`
`AC-MAPPING: AC3 | IMPLEMENTATION: P2-T3 | TESTS: P3-T3 | EVIDENCE: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/citation-verification-ac3.2026-09-02T08-58.md`
`AC-MAPPING: AC4 | IMPLEMENTATION: P2-T1,P2-T2,P2-T3 | TESTS: P3-T4 | EVIDENCE: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/citation-verification-ac4.2026-09-02T08-58.md`
`AC-MAPPING: AC5 | IMPLEMENTATION: P1-T1 | TESTS: P3-T5 | EVIDENCE: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/scope-verification-ac5.2026-09-02T08-58.md`
`AC-MAPPING: AC6 | IMPLEMENTATION: P1-T1 | TESTS: P3-T6 | EVIDENCE: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/scope-verification-ac6.2026-09-02T08-58.md`
`AC-MAPPING: AC7 | IMPLEMENTATION: P2-T1,P2-T2,P2-T3 | TESTS: P3-T7 | EVIDENCE: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/qa-gates/line-scope-verification-ac7.2026-09-02T08-58.md`

`SCOPE-BOUNDARY: PASS`
`UNRESOLVED-GAPS: NONE`

---

## Revision Round — Write-Set Presentation Fix (2026-09-02T09-30)

`SELF-REVIEW: RE-DERIVED THIS PASS`

This revision round is text-presentation only: it adds a `## Write Set` section to `spec.md` naming CLAUDE.md as the sole file this plan's diff creates, modifies, or deletes, and it removes backtick formatting from every path in `spec.md` and this plan that names an exclusion, a file verified unchanged, a reference target, or a context citation, so that a backtick-token harvester used for downstream parallel-scheduling blast-radius derivation no longer misreads those mentions as write targets. No task, acceptance criterion, command, or evidence path was changed.

- CLAUDE.md — re-derived this pass via `Grep 'ci.yml' CLAUDE.md`; lines 194, 202, and 210 still contain the unedited literal citation `.github/workflows/ci.yml`, confirming Phase 2 has not executed and this revision changed no CLAUDE.md content.
- .github/workflows/_format-check.yml — re-derived this pass via a directory listing; file exists in the current tree.
- .github/workflows/_build-analyzers.yml — re-derived this pass via a directory listing; file exists in the current tree.
- .github/workflows/_build-nullable.yml — re-derived this pass via a directory listing; file exists in the current tree.
- docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/spec.md — edited this pass to add the `## Write Set` section (listing only `CLAUDE.md`) and to remove backtick formatting from every exclusion/reference/context path; re-read in full after editing to confirm no task substance, AC checkbox text, or command text changed.
- This plan file — edited this pass to remove backtick formatting from every exclusion/reference/context path (including the malformed nested-backtick citation tokens in P2-T1/P2-T2/P2-T3 and the CITATION lines in the Planner Self-Review block); re-read in full after editing to confirm every P#-T# task ID, acceptance-criterion mapping, and command string is byte-identical to the pre-revision text apart from backtick removal.

`PLANNER-INTERNAL-REVIEW: PASS`

`CITATION-TO-TREE: PASS`
`CITATION: CLAUDE.md | lines 194, 202, 210 unchanged, Grep-confirmed this pass`
`CITATION: .github/workflows/_format-check.yml | file exists (directory-listing-confirmed this pass)`
`CITATION: .github/workflows/_build-analyzers.yml | file exists (directory-listing-confirmed this pass)`
`CITATION: .github/workflows/_build-nullable.yml | file exists (directory-listing-confirmed this pass)`
`CITATION: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/spec.md | Write Set section added, all AC/task substance unchanged`

`AC-TRACEABILITY: PASS`
`AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7`
`AC-MAPPING: unchanged from the prior round (see AC-MAPPING block above); this revision did not alter implementation, test, or evidence assignments for any AC`

`SCOPE-BOUNDARY: PASS`
`UNRESOLVED-GAPS: NONE`
