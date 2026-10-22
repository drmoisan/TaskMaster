# Remediation Inputs — 2026-09-02-ci-build-infra-debt-730

- Timestamp: 2026-09-02T23-47
- Issue: #730
- Branch: `bug/ci-build-infra-debt-730` @ `e80f74c9`
- Base: `origin/main` @ `8be5a6aa`

## Source Artifacts

- `docs/features/active/2026-09-02-ci-build-infra-debt-730/policy-audit.2026-09-02T23-47.md`
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/code-review.2026-09-02T23-47.md`
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/feature-audit.2026-09-02T23-47.md`

## Scope Note

All eight acceptance criteria in `spec.md` PASS. The change itself — the three comment-only workflow
edits and the new `Directory.Build.props` — requires no correction. Remediation is confined to one
artifact-hygiene defect in a documentation file, plus two optional maintainability items.

---

## R-1 — Remove the absolute host path from the research document (REMEDIATION REQUIRED)

**Severity:** Blocking. Repository-wide rule: no committed artifact may embed an absolute host path or
the operator account name.

**File:** `docs/features/active/2026-09-02-ci-build-infra-debt-730/research/research.2026-09-02T09-15.md`
**Line:** 8

**Current state.** Line 8 is a backticked absolute path containing the operator account name, inside
the bullet beginning "All evidence below was read directly from the working tree at". Confirmed in the
committed tree (`git grep -i HEAD`), so it ships with the PR. It is the only such occurrence in the
entire branch diff.

**Required change.** Replace the literal path with the `<repo-root>` placeholder already used as the
convention throughout the four sanitized logs, so the bullet reads:

```markdown
- All evidence below was read directly from the working tree at
  `<repo-root>`
  on branch `bug/ci-build-infra-debt-730` (based on `origin/main`) on 2026-09-02.
```

Do **not** hardcode the replacement source path when scripting this; derive it at run time
(`git rev-parse --show-toplevel`) so the fix and any artifact recording it stay self-exempt from the
rule being enforced, consistent with how `[P2-T10]` was written.

**Acceptance for R-1.** A case-insensitive sweep of the whole feature folder for the run-time-derived
account token (`Split-Path -Leaf $env:USERPROFILE`) returns 0 matches, and a sweep for `C:\Users` and
`C:/Users` returns 0 matches, across every committed file in the branch diff. Note that
`plan.2026-09-02T08-57.md` and `log-sanitization.2026-09-02T23-31.md` legitimately contain the literal
token `USERPROFILE` inside `$env:USERPROFILE` expressions; those are correct and must not be altered.

**Do not regress.** The four `.log` files are currently clean (0 residual matches each) and their line
counts are 11878 / 12030 / 11906 / 11742. Any sweep must leave those four files and their line counts
unchanged.

## R-2 — Widen the sanitization step beyond an explicit file list (RECOMMENDED)

**Severity:** Low, process. Not blocking on its own; it is the root cause of R-1.

`[P2-T10]` enumerates exactly four `.log` paths. Any artifact added by a different phase — such as
`research.2026-09-02T09-15.md`, written in the earlier preparation commit `9d7b78a9` — is invisible to
it by construction. A path list cannot be complete against files it does not know about.

**Recommended change.** Make the sanitization sweep operate over the whole feature folder rather than a
fixed list, and make its residual-verification step assert 0 matches folder-wide rather than
list-wide. The existing two-step ordering (worktree root first, then main-checkout root) must be
preserved, since the second string is a prefix of the first.

## R-3 — Replace archive-fragile path references (OPTIONAL)

**Severity:** Low, maintainability. Not blocking.

Four files each carry one reference to `docs/features/active/2026-09-02-ci-build-infra-debt-730/...`:
`.github/workflows/_build-analyzers.yml`, `.github/workflows/_build-nullable.yml`,
`.github/workflows/_mstest-coverage.yml`, and `Directory.Build.props`. The repository has a
`docs/features/archive/` tree, so the folder moves once the feature completes and all four pointers go
dead.

**Recommended change.** Drop the fragile path prefix and rely on the issue reference the comments
already carry (`issue #730`), or point at a location that does not move.

**Constraint if this is actioned.** These are comment-only edits and must remain comment-only. Re-verify
afterwards by parsing the base and head revisions of each workflow and deep-comparing the documents
with key order preserved; all three parse trees must remain identical to `origin/main`. Consider also
dedenting the block from 12 spaces to 10 to match the surrounding keys (code review CR-2) while making
this edit, since it touches the same lines.

## R-4 — Commit the residual checklist edit (OPTIONAL)

**Severity:** Low, delivery hygiene. Not blocking.

`git status --porcelain` reports `M docs/features/active/2026-09-02-ci-build-infra-debt-730/plan.2026-09-02T08-57.md`.
The delta is a single `[ ]` -> `[x]` checkbox flip on `[P3-T9]`, which is structurally unavoidable — the
task that verifies the clean tree cannot record its own completion beforehand. Committing it leaves the
branch clean, matching the repository convention that delivery ends with an empty `git status`.

If R-1 is actioned, this can be folded into the same commit.

---

## Explicitly Not Required

For clarity, so remediation does not over-reach:

- No change to `Directory.Build.props` content, placement, or the property it sets.
- No change to the semantics of any workflow file.
- No `.csproj`, `packages.config`, or `Directory.Build.targets` edit.
- No `System.Reactive` `PackageReference` migration or version rollback.
- No coverage threshold, Pester job, or coverage gate change.
- No re-run of the msbuild or vstest gates, unless R-3 is actioned — the current evidence is valid,
  contemporaneous, and independently corroborated, and R-1/R-2/R-4 touch only documentation.
