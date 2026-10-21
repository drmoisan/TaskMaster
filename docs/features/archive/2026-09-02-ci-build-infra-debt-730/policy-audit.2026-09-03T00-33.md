# Policy Audit — 2026-09-02-ci-build-infra-debt-730 (Re-audit, remediation cycle 1)

- Timestamp: 2026-09-03T00-33
- Issue: #730
- Work Mode: `full-bug` (from `issue.md` line 12) — AC source is `spec.md` only
- Branch: `bug/ci-build-infra-debt-730` @ `ff2106fe`
- Base branch (resolved): `origin/main` @ `8be5a6aa`
- Merge base: `8be5a6aa` (equal to `origin/main` tip; branch is a straight descendant)
- Audit range: `8be5a6aa..ff2106fe` — 34 changed files
- Prior audit: `policy-audit.2026-09-02T23-47.md` (1 blocking finding: PA-1/CR-1)

## Verdict

**PASS — zero blocking findings remaining.**

The single blocking finding from the prior cycle (PA-1/CR-1, absolute host path with operator
account name in `research/research.2026-09-02T09-15.md` line 8) is **closed and independently
verified**. The remediation introduced no new blocking issue and did not touch any shipped
configuration file. This cycle may be closed and the branch may proceed to PR authoring.

---

## Scope Statement

This audit covers the **full branch diff against the resolved base branch** (`8be5a6aa..ff2106fe`,
34 files), not the remediation subset. The two remediation commits were additionally examined in
isolation to answer the caller's containment question, but that examination is a supplement to the
full-diff audit, not a substitute for it.

### Rejected Scope Narrowing

No narrowing was accepted. One phrase in the caller prompt could be read as narrowing and is
recorded verbatim for completeness:

> "these were explicitly out of scope for this remediation cycle and should remain open
> observations, not new blockers, unless you find them newly problematic"

Justification for proceeding with the full audit: the quoted phrase scopes the *remediation task*,
not this audit. Both referenced observations (CR-2, CR-3) were re-evaluated against the full branch
diff on their own merits in this cycle; the conclusion that they are non-blocking is this reviewer's
independent finding, not an acceptance of the caller's framing.

### Deviation: stale PR context artifacts

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are **tracked** files in
this repository and currently carry a **stale pair belonging to issue #564**:

- Summary "Head ref (resolved)": `bug/claude-md-cites-ciyml-for-moved-toolchain-commands-564 @ fafe3d4d`
- This branch head: `ff2106fe` — no match
- Summary "Changed files overview" lists 4 files, all under the #564 feature folder

Per the established repository handling for tracked PR-context files, these were **not regenerated
over** (doing so would dirty a tracked file belonging to another feature's delivery). Scope and
evidence for this audit were derived directly from `git merge-base` and `git diff` against
`origin/main`, which is the authoritative base-resolution path. This deviation is recorded rather
than silently accepted. It is **non-blocking** and pre-existing on `main`.

Consequence for coverage enumeration: the coverage hook derives changed languages from the summary
file. Both the stale summary and the correctly derived git diff yield the **same** result — zero
changed files in any coverage language — so the stale artifact does not mask a coverage obligation.

---

## PA-1 Closure Verification (prior blocking finding)

**Finding as filed:** `research/research.2026-09-02T09-15.md` line 8 embedded an absolute worktree
path including the operator account name, present in the committed tree.

**Status: CLOSED — PASS.**

### Direct read of line 8 at HEAD

```
7  - All evidence below was read directly from the working tree at
8    `<repo-root>`
9    on branch `bug/ci-build-infra-debt-730` (based on `origin/main`) on 2026-09-02.
```

The literal path is replaced by the `<repo-root>` placeholder — the same convention already
established by the sanitized `.log` files. The sentence remains grammatical and the meaning is
preserved. No absolute path and no account name remain on this line.

### Tree-wide sweep (all 34 changed files, committed blobs at HEAD)

The operator account name was derived at run time via `Split-Path -Leaf $env:USERPROFILE` and is
**not reproduced anywhere in this artifact**. Sweep performed case-insensitively against the
committed blob of every changed file (not the working tree).

| Pattern | Matches across all 34 changed files | Verdict |
|---|---|---|
| Operator account name (run-time derived) | **0** | PASS |
| Host name (`$env:COMPUTERNAME`) | **0** | PASS |
| User domain (`$env:USERDOMAIN`) | **0** | PASS |
| `[A-Za-z]:[\\/]Users[\\/]<name>` (any user directory) | **0** | PASS |
| `AppData[\\/](Local\|Roaming)` | **0** | PASS |
| `worktrees[\\/]agent-<hex>` | **0** | PASS |
| Bare `C:\Users` / `C:/Users` prefix text | 11 (all methodology description — see below) | PASS |
| UNC-shaped `\\host\` | 3, all pre-existing and identical in base | PASS |

### Adjudication of the 11 bare-prefix occurrences

Every one of the 11 occurrences is the **search-pattern string itself**, quoted as prose or as a
table column header while describing how the sweep was performed. None is an embedded path, and
none is followed by a user directory name. Locations:

| File | Lines | Form |
|---|---|---|
| `evidence/qa-gates/research-sanitization-verification.2026-09-02T23-56.md` | 11, 15 | `Select-String -Pattern` argument; result prose |
| `policy-audit.2026-09-02T23-47.md` | 127 | table column header ``  `C:\Users\` matches `` |
| `remediation-inputs.2026-09-02T23-47.md` | 49, 50 | sweep-description prose |
| `remediation-plan.2026-09-02T23-56.md` | 20, 46, 87, 88, 139, 141 | sweep-instruction and citation prose |

This is a self-referential-documentation pattern, not a leak: a document that instructs a sweep for
a prefix must name the prefix. The decisive control is the account-name sweep, which is run with
**no exclusions** and returns 0 across all 34 files, including these six documents.

### Adjudication of the 3 UNC-shaped matches

`.github/workflows/_mstest-coverage.yml` contains `\\bin\`, `\\obj\`, `\\ref\` — regex escapes in
the pre-existing test-assembly path filter. Byte-identical in base `8be5a6aa` and head `ff2106fe`.
Not introduced by this branch and not a UNC path. Dismissed as a false positive.

### Residual: pre-sanitization blob remains in branch history

The sanitized content is correct at HEAD, but `research.2026-09-02T09-15.md` was **modified**, not
added, by the remediation. Scanning every commit unique to this branch:

| Commit | Account-name occurrences in feature/config files |
|---|---|
| `ff2106fe` | 0 |
| `56240773` | 0 |
| `e80f74c9` | 1 (`research/research.2026-09-02T09-15.md`) |
| `48ea849e` | 1 (same file) |
| `acbe9f4c` | 1 (same file) |
| `9d7b78a9` | 1 (same file) |

The four earlier commits retain a reachable blob containing the account name. A plain merge would
carry those objects into `main`'s history.

**Disposition: non-blocking, with a merge-strategy requirement.** The deliverable tree is clean,
which is what the policy requires of the shipped artifact. To prevent the superseded blob from
entering `main`, **this PR must be squash-merged**. This is the same remedy applied to the
equivalent situation on issue #648. Recorded here so the requirement is visible at merge time; it
requires no further code or document change.

By contrast, `remediation-plan.2026-09-02T23-56.md` was **added** (status `A`) in `56240773`, so its
pre-redaction text was never committed and leaves no blob in history.

---

## Remediation Commit Containment

Re-derived independently via `git show --name-status`.

### `56240773` — sanitization plus review record

| Status | Path |
|---|---|
| M | `docs/.../research/research.2026-09-02T09-15.md` |
| M | `docs/.../plan.2026-09-02T08-57.md` |
| A | `docs/.../code-review.2026-09-02T23-47.md` |
| A | `docs/.../feature-audit.2026-09-02T23-47.md` |
| A | `docs/.../policy-audit.2026-09-02T23-47.md` |
| A | `docs/.../remediation-inputs.2026-09-02T23-47.md` |
| A | `docs/.../remediation-plan.2026-09-02T23-56.md` |
| A | `docs/.../evidence/other/research-line8-pre-edit.2026-09-02T23-56.md` |
| A | `docs/.../evidence/qa-gates/research-sanitization-verification.2026-09-02T23-56.md` |

### `ff2106fe` — checklist marker

| Status | Path |
|---|---|
| M | `docs/.../remediation-plan.2026-09-02T23-56.md` (one checkbox, `[ ]` to `[x]`, on P0-T4) |

### Containment verdict — PASS

All 10 touched paths are documentation or evidence files under
`docs/features/active/2026-09-02-ci-build-infra-debt-730/`. Confirmed **not** touched by either
commit:

| Protected surface | Touched? |
|---|---|
| `.github/workflows/**` | No |
| `Directory.Build.props` | No |
| `Directory.Build.targets` | No |
| Any `*.csproj` | No |
| Any `packages.config` | No |
| Any `*.cs` | No |
| `spec.md` (AC source) | No — blob `843eaeda` identical at `e80f74c9` and `ff2106fe` |
| `issue.md` | No — blob `51efa7ed` identical at `e80f74c9` and `ff2106fe` |

`git diff --stat` restricted to `*.csproj`, `*packages.config`, `Directory.Build.targets`, and
`*.cs` over the **entire branch range** returns empty output, confirming the containment holds for
the whole branch, not merely the remediation commits.

---

## Sanitized Log Integrity

Verified against the committed blobs at HEAD, with newline-counting rather than
`Measure-Object -Line` (which undercounts). Blob SHAs compared between the pre-remediation commit
`e80f74c9` and HEAD `ff2106fe` to prove the files were not touched.

| Log file | Expected lines | Actual | Account matches | `C:\Users` matches | Blob vs pre-remediation |
|---|---|---|---|---|---|
| `evidence/baseline/msbuild-analyzers-pre.log` | 11878 | **11878** | 0 | 0 | UNCHANGED (`fd757e3e948e`) |
| `evidence/baseline/msbuild-nullable-pre.log` | 12030 | **12030** | 0 | 0 | UNCHANGED (`a3222fcda846`) |
| `evidence/qa-gates/msbuild-analyzers-post.log` | 11906 | **11906** | 0 | 0 | UNCHANGED (`5ec06350a396`) |
| `evidence/qa-gates/msbuild-nullable-post.log` | 11742 | **11742** | 0 | 0 | UNCHANGED (`178241d4bc20`) |

All four: line counts match exactly, zero residual matches, and byte-identical blob SHAs before and
after the remediation. **PASS.** The identical blob SHA is stronger evidence than a content re-scan
alone, because it proves the files were not rewritten and re-sanitized.

---

## Coverage Verification

### Changed-language enumeration (derived from `git diff`, not from the stale summary)

Extension census across the 34 changed files: `.md` 26, `.log` 4, `.yml` 3, `.props` 1.

| Language | Changed files on branch | Coverage artifact | Verdict |
|---|---|---|---|
| C# / .NET | **0** | not required | N/A — zero changed files of this language on the branch |
| PowerShell / Pester | **0** | not required | N/A — zero changed files of this language on the branch |
| Python | **0** | not required | N/A — zero changed files of this language on the branch |
| TypeScript | **0** | not required | N/A — zero changed files of this language on the branch |

`N/A` is used here in the one circumstance the policy permits it: a language with **zero** changed
files in the branch diff. No `.cs`, `.ps1`, `.psm1`, `.psd1`, `.py`, `.ts`, or `.tsx` file appears
anywhere in `8be5a6aa..ff2106fe`. The branch changes only workflow YAML, one MSBuild `.props` file,
Markdown documents, and build-transcript evidence, so there is no coverage denominator to move and
no coverage artifact obligation is triggered.

This matches `spec.md`'s Test Strategy, which states that no application source line is added,
removed, or modified and therefore there is no coverage-denominator impact. The claim was verified
against the diff rather than accepted from the spec.

---

## Evidence Location Compliance

Scan of the branch diff for files written under non-canonical artifact paths
(`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`):

**Result: none. PASS.**

All 20 evidence artifacts are correctly located under
`docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/<kind>/`, using the canonical
`baseline/`, `qa-gates/`, and `other/` subdirectories.

`validate_evidence_locations.py` is **not present** in this repository, so the scripted check could
not be executed. The equivalent scan was performed directly over the diff file list. This is a tool
availability gap, not an evidence gap.

---

## Policy Compliance Matrix

| Policy | Requirement | Verdict | Evidence |
|---|---|---|---|
| CLAUDE.md — toolchain order | format, analyze, type-check, test | PASS | CSharpier exit 0 (1576 files); both `/t:Rebuild` passes 0 errors; vstest 6095/6095 |
| CLAUDE.md — CSharpier invocation | via `dotnet tool run`, pinned version | PASS | `dotnet tool run csharpier check .`, 1.2.6 from `dotnet-tools.json` |
| CLAUDE.md — `/t:Rebuild` not `/t:Build` | analyzer/nullable gates must not be vacuous | PASS | Both logs use `/t:Rebuild`; 36 `csc.exe` invocations per log confirm real compilation |
| CLAUDE.md — no `/p:Nullable=enable` | must match CI command exactly | PASS | Property absent from both build commands |
| general-code-change — 500-line limit | production, test, reusable script files | PASS | `Directory.Build.props` 18; workflows 69/76/112. See note below |
| general-code-change — fail fast, no silent errors | n/a to config-only change | PASS | No error-handling code path added |
| general-code-change — I/O boundaries | n/a to config-only change | PASS | No application code touched |
| general-unit-test — coverage floors | 85% line / 75% branch | N/A | Zero changed files in every coverage language |
| general-unit-test — no temp files in tests | prohibited | PASS | No test authored or modified |
| general-unit-test — coverage exclusions | no production path excluded | PASS | `.csharpierignore` excludes only `evidence/**` and project files; no coverage config changed |
| quality-tiers — uniform thresholds | 85/75 across T1–T4 | N/A | No coverage language changed |
| tonality | professional, no hype or humor | PASS | Reviewed all 9 documents added or modified by the remediation commits |
| evidence-and-timestamp-conventions | `<FEATURE>/evidence/<kind>/` | PASS | See Evidence Location Compliance |
| acceptance-criteria-tracking | AC checked off only with evidence | PASS | 8/8 checked in `spec.md`, each independently re-verified |
| Artifact hygiene — no host paths | no account or host names | PASS | 0 account, 0 host, 0 domain matches across all 34 files |

### Note on the 500-line limit

Four `.log` files exceed 500 lines (11878, 12030, 11906, 11742) and
`research/research.2026-09-02T09-15.md` is 518 lines. Neither category is in scope for the limit:

- The rule applies to "production code, test code, or reusable script file." Build transcripts are
  tool output committed as audit-trail evidence — the repository formalizes this classification in
  `.csharpierignore`, which excludes `**/evidence/**` on exactly that basis.
- Markdown documentation files are explicitly exempted by the rule's own exception list.

No file subject to the limit approaches it. **PASS.**

---

## Findings Register (this cycle)

| ID | Severity | Status | Title |
|---|---|---|---|
| PA-1 | was Blocking | **CLOSED** | Absolute host path with account name in `research.2026-09-02T09-15.md:8` |
| PA-2 | Non-blocking | **OPEN (new)** | Pre-sanitization blob reachable in 4 earlier branch commits; requires squash-merge |
| PA-3 | Non-blocking | **OPEN (carried)** | Tracked `artifacts/pr_context.*` stale, belongs to issue #564; pre-existing on `main` |
| PA-4 | Informational | **OPEN (carried)** | `validate_evidence_locations.py` absent; manual diff scan substituted |

**Blocking findings: 0.**
