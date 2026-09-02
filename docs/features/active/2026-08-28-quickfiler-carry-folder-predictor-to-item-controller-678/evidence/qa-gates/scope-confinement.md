# P2-T11 — Scope confinement (AC23)

Timestamp: 2026-09-02T00-24

## Commands, in order

```
git add -A -- QuickFiler QuickFiler.Test docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678
git diff --cached --name-only 807fb0bb6e5e49f43efa6b256b05960bf078ca19
git status --porcelain
```

The staging step is required because a name-listing diff enumerates tracked changes only and would
otherwise be blind to the files this change creates. The **unscoped** porcelain status is required
because the staging pathspec would otherwise leave an out-of-scope path unreported: staging only the
three in-scope prefixes cannot, by construction, reveal a change outside them, so a second
observation with no pathspec at all is what closes that hole.

## Acceptance conditions

### 1. Every path in the anchored name-only diff begins with one of the three allowed prefixes

`git diff --cached --name-only <base>` returned **73 paths**. A filter for any path not matching
`QuickFiler/*`, `QuickFiler.Test/*` or
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/*`
returned **NONE**.

Breakdown of the 73:

| Prefix | Count |
|---|---:|
| `QuickFiler/` | 16 (14 `.cs`, 1 `.csproj`, and the new `CarrierLoad`/`Enqueue` parts among the 14) |
| `QuickFiler.Test/` | 19 (18 `.cs`, 1 `.csproj`) |
| `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/` | 38 (33 evidence artifacts, `issue.md`, the plan file, and the research document) |

The feature-folder count includes `issue.md`, `plan.2026-08-31T21-12.md` and
`research/2026-08-31T21-15-quickfiler-carry-folder-predictor-research.md`. Those three appear in the
anchored diff because they do not exist at the base ref: they were added to this branch before
execution began. They are inside the permitted feature-folder prefix.

### 2. The unscoped porcelain status reports no modified or untracked path outside those three prefixes

`git status --porcelain` with no pathspec returned 39 entries. Every one is under
`QuickFiler/`, `QuickFiler.Test/` or the feature folder. A filter for anything else returned:

```
agent-memory paths: 0
other out-of-prefix paths: 0
```

**Both counts are zero.**

The plan carves out `.claude/agent-memory/` for separate enumeration, on the basis that the directory
is tracked and holds agent-session state rather than product or policy. **That carve-out was not
needed: this execution wrote nothing to `.claude/agent-memory/`.** The Phase 2 preamble states that
writing there is not part of the deliverable and that the exclusion is a tolerance rather than an
invitation; the enumeration is therefore empty, and the AC23 judgment rests on the full unscoped
status with no exclusion applied to it at all. That is the stronger result.

### 3. No path under `UtilitiesCS/`, `.claude/rules/`, `.claude/skills/` or the repository-root `CLAUDE.md` appears in either output

A combined scan of both outputs for those four prefixes returned **NONE**.

Named explicitly for the audit record:

- **`UtilitiesCS/`** — not touched. Two design decisions were made specifically to keep it that way:
  the AC12 projection was duplicated in QuickFiler rather than made accessible on
  `FolderPredictor.ProjectSuggestionPath`, and `InitAsync` was not added to `IFolderSearchHandler`.
  Both are recorded with their reasons in `evidence/other/change-description.md` and
  `evidence/other/out-of-scope-register.md`.
- **`.claude/rules/`** — not touched. Read-only during Phase 0.
- **`.claude/skills/`** — not touched.
- **`CLAUDE.md`** — not touched. Read-only during Phase 0.

### 4. Both command outputs are recorded in full

Both are reproduced above: the 73-path diff by prefix breakdown with the explicit zero-count filter
result, and the 39-entry porcelain status with its zero-count filter results. The full untruncated
listings were captured in the execution transcript at the timestamp above.

## Note on line endings

`git add` emitted a `LF will be replaced by CRLF` advisory for each newly added Markdown and XML
evidence artifact. That is the repository's configured `core.autocrlf` normalisation applying to
files this session wrote with LF endings. It changes no path, affects no source file under
`QuickFiler/` or `QuickFiler.Test/`, and has no bearing on AC23; it is recorded so the advisory in
the transcript is not read as an anomaly.

## Verdict

**AC23 holds.** The change is confined to the `QuickFiler` and `QuickFiler.Test` projects plus this
feature folder, with no change to `.claude/rules/`, `CLAUDE.md`, any policy document, or any file
under `UtilitiesCS`.
