# QA Gate — Scope Boundary (anchored diff against `origin/main`)

- Task: [P2-T2]
- Phase: Phase 2 — Verification & Final QC

Timestamp: 2026-09-02T23-08

Command: `git diff --cached origin/main --name-status -- Directory.Build.props .github/workflows/_build-analyzers.yml .github/workflows/_build-nullable.yml .github/workflows/_mstest-coverage.yml`

EXIT_CODE: 0

Rationale for the scoping: the diff is anchored to the `origin/main` ref and scoped to the same four Phase-1 pathspecs used by [P2-T1]. An unscoped diff against `origin/main` would additionally list the four feature-folder documents (`issue.md`, this feature's plan file, `research/research.2026-09-02T09-15.md`, `spec.md`) already committed ahead of `origin/main` by a prior preparation-mode commit, producing 8 lines instead of 4. The companion `git status --porcelain` capture required alongside a name-listing diff is recorded in [P2-T1]'s artifact (`git-stage-scope.2026-09-02T23-07.md`).

## Output Summary

Full name-status output (4 lines):

```
M	.github/workflows/_build-analyzers.yml
M	.github/workflows/_build-nullable.yml
M	.github/workflows/_mstest-coverage.yml
A	Directory.Build.props
```

- Measured output line count: 4. This matches the four-path set recorded in [P2-T1] exactly; there is no fifth or further line.
- Path set identity with [P2-T1]: `Directory.Build.props` (added), `.github/workflows/_build-analyzers.yml`, `.github/workflows/_build-nullable.yml`, `.github/workflows/_mstest-coverage.yml` (all modified). Same set, same statuses.

### Excluded-substring occurrence counts in the name-status output

Each count was measured programmatically over the joined output text (escaped literal match, not a manual read):

| Substring | Occurrences |
|---|---|
| `.csproj` | 0 |
| `packages.config` | 0 |
| `Directory.Build.targets` | 0 |
| `.claude/` | 0 |
| `.codex/` | 0 |
| `.agents/` | 0 |
| `config/blast-radius.json` | 0 |
| `config/orchestration-routing.json` | 0 |

All eight excluded substrings occur zero times. Note that `Directory.Build.props` is a distinct filename from `Directory.Build.targets`; the latter is not present in the change set and was not edited.

## Acceptance

- Scoped name-status output matches the four-line set from [P2-T1] exactly, with no fifth or further line: PASS.
- None of the eight listed excluded substrings appears in the output: PASS.
