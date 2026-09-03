# Code Review — 2026-09-02-ci-build-infra-debt-730 (Re-review, remediation cycle 1)

- Timestamp: 2026-09-03T00-33
- Issue: #730
- Branch: `bug/ci-build-infra-debt-730` @ `ff2106fe`
- Base: `origin/main` @ `8be5a6aa` (merge base identical)
- Prior review: `code-review.2026-09-02T23-47.md` @ `e80f74c9`
- Reviewable surface: 4 non-document files (3 modified workflow YAML, 1 added MSBuild `.props`).
  No application source code in the diff.

## Summary

The shipped configuration is **byte-identical to the previously reviewed state**. The remediation
commits changed documentation only, so the engineering assessment from the prior cycle stands
unchanged. The one blocking finding (CR-1) is closed by a correct, minimal, single-line edit that
preserves the sentence's meaning and adopts the placeholder convention already used by the
sanitized logs.

The remediation was checked for the failure mode it could most plausibly have introduced — a
"fix" that edits the target line while quietly disturbing surrounding structure or other files.
It did not. Both remediation commits are confined to the feature folder, and the two shipped
configuration surfaces were re-verified from scratch rather than carried forward on trust.

| ID | Severity | Status | Title |
|---|---|---|---|
| CR-1 | was Blocking | **CLOSED** | Absolute host path with account name in `research.2026-09-02T09-15.md:8` |
| CR-2 | Low | OPEN (carried) | Comment block indented deeper than the keys it sits between |
| CR-3 | Low | OPEN (carried) | Four references to a `docs/features/active/...` path that will move on archive |
| CR-4 | Low | **CLOSED** | Worktree left dirty with one uncommitted checkbox edit |
| CR-5 | Info | OPEN (carried) | Tool install paths remain in committed logs — no action recommended |
| CR-6 | Info | OPEN (carried) | Root `Directory.Build.props` applies to all 18 projects — documented risk |
| CR-7 | Info | OPEN (carried) | Identical 16-line comment copied into three files — no better expression available |
| CR-8 | Low | **OPEN (new)** | Superseded blob with the account name remains reachable in branch history |

**Blocking findings: 0.**

---

## CR-1 — Closed

The edit is exactly the recommended one:

```
- All evidence below was read directly from the working tree at
  `<repo-root>`
  on branch `bug/ci-build-infra-debt-730` (based on `origin/main`) on 2026-09-02.
```

Assessment of the fix as a code-quality matter:

- **Correct convention.** `<repo-root>` is the placeholder already established by the four
  sanitized `.log` files, so the feature folder now uses one consistent placeholder vocabulary
  rather than inventing a second form.
- **Meaning preserved.** The sentence still asserts what it needs to assert — that evidence was
  read from a working tree on a named branch on a named date. The absolute path was never
  load-bearing; it identified a machine, not a fact about the change.
- **Minimal.** One line, one file. The diff for this file contains exactly one changed line.

The commit message accurately describes what was done and why, and correctly notes that the same
literal was also removed from the remediation plan before that plan was first committed.

### On the follow-up recommendation from the prior cycle

The prior review recommended widening the sanitization step from an explicit four-file list to a
sweep over the whole feature folder, because a path list is "silently incomplete by construction."
The remediation adopted this: `remediation-plan.2026-09-02T23-56.md` P0-T3 sub-item 1 specifies an
account-name sweep over the feature folder **with no exclusions**. That is the right shape. It is
what allowed the plan document's own four self-inflicted occurrences to be caught and redacted
before the document was ever committed.

## CR-4 — Closed

`git status --porcelain` in the item worktree now returns zero entries. The `[P3-T9]` checkbox
residue identified in the prior cycle was committed in `56240773`, and the equivalent `[P0-T4]`
residue from the remediation plan was committed in `ff2106fe`. The branch is delivered clean.

`ff2106fe` is a one-character content change (`[ ]` to `[x]`). Splitting it into its own commit is
defensible: it is the completion marker for the task that verifies the tree is clean, which cannot
record its own completion before it runs. The commit message states this reasoning.

## CR-8 — Superseded blob in branch history (Low, new)

`research.2026-09-02T09-15.md` was modified rather than added, so the pre-sanitization content
remains reachable in four earlier commits on this branch (`9d7b78a9`, `acbe9f4c`, `48ea849e`,
`e80f74c9`), each carrying one occurrence of the operator account name.

This is not a defect in the remediation — sanitizing a file that has already been committed cannot
retroactively remove the earlier blob, and rewriting branch history to do so would be a
disproportionate response to a single path string in a research note.

**Recommendation: squash-merge this PR.** That collapses the branch to a single commit whose tree
is the clean HEAD tree, and the superseded blobs never become reachable from `main`. This costs
nothing here because the branch's commit history has no independent value worth preserving: it is
one implementation commit plus documentation and review-record commits.

## CR-2 — Comment block indentation (Low, carried, re-verified)

The inserted block sits at indent 12; `key:` above it and `restore-keys:` below it are both at
indent 10:

```
   L39  indent=10  key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}
   L40  indent=12  # The bare-prefix fallback below is safe against stale package
   ...
   L55  indent=12  # research/ for the full analysis (issue #730).
   L56  indent=10  restore-keys: |
```

This remains a **cosmetic** issue, and I re-tested the functional question from scratch this cycle
rather than carrying the prior conclusion. The concern is real in principle: a more-indented line
following a plain scalar can be absorbed into that scalar, which would silently corrupt the cache
key without any syntax error. That failure mode is invisible in a line-based diff, so it warrants
a parser-level check every time the file changes.

Two independent parsers, order-sensitive full parse-tree comparison, base versus head:

| File | PyYAML 6.0.3 tree identical | YamlDotNet (powershell-yaml 0.4.12) tree identical | Resolved `key` scalar at head |
|---|---|---|---|
| `_build-analyzers.yml` | Yes | Yes | `nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}` |
| `_build-nullable.yml` | Yes | Yes | `nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}` |
| `_mstest-coverage.yml` | Yes | Yes | `nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}` |

No resolved `key` value contains a `#` character, and `restore-keys` is present exactly once in
both base and head trees for every file. The comments are provably inert. The code is correct.

**Recommendation (unchanged):** dedent to 10 spaces. Purely cosmetic, and it removes the chance
that a future editor reads the block as belonging to `key:` and reflows it into a position where
indentation would matter.

## CR-3 — Archive-fragile path references (Low, carried, re-verified)

Four references remain, one per shipped configuration file:

| File | Line | Text |
|---|---|---|
| `_build-analyzers.yml` | 54 | `# names. See docs/features/active/2026-09-02-ci-build-infra-debt-730/` |
| `_build-nullable.yml` | 54 | same |
| `_mstest-coverage.yml` | 54 | same |
| `Directory.Build.props` | 12 | `and docs/features/active/2026-09-02-ci-build-infra-debt-730/ for the` |

The repository contains **6770 tracked files under `docs/features/archive/`**, which confirms
archiving is routine practice rather than a theoretical concern. When this feature folder moves,
all four pointers become dead paths, and the entire value of these comments is that a reader can
follow them to the reasoning.

**Recommendation (unchanged):** drop the fragile path prefix. All four comments already cite
"issue #730", which is a stable identifier that survives archiving. Four one-line edits now versus
a stale pointer that will not be noticed until someone tries to follow it.

Not blocking: the comments remain independently readable, and the rationale they summarize is
stated inline in the comment body, so a dead path degrades the reference without destroying the
explanation.

## CR-5, CR-6, CR-7 — Informational, carried unchanged

No change in this cycle; the shipped files are byte-identical to the previously reviewed state.

- **CR-5:** committed logs retain `C:\Program Files\...` tool paths. These carry no account name
  and no host name — re-confirmed this cycle at 0 matches for both — so they do not violate the
  hygiene rule. Stripping them would reduce diagnostic value. No action recommended.
- **CR-6:** the root `Directory.Build.props` is auto-imported by all 18 projects. Re-verified this
  cycle: exactly one `Directory.Build.props` is tracked, no `NuGet.Config` exists anywhere in the
  repository to disturb the import chain, and `Directory.Build.targets` is present, which is what
  proves the auto-import mechanism is already live here. The file declares exactly one property,
  inert for non-consumers. The forward-looking risk is documented in `spec.md` with a mitigation.
- **CR-7:** the 16-line block is byte-identical across the three workflows. YAML has no
  comment-include mechanism and the three are independent reusable-workflow definitions, so
  duplication is the only available expression. Identical copies are the desirable outcome.

---

## Verification Performed This Cycle

Recorded explicitly because the caller asked that the remediation not be taken on trust. Every row
was re-derived in this session; none is carried forward from the prior review.

| Claim | How it was independently checked | Result |
|---|---|---|
| Line 8 no longer leaks a path | Direct read of the file at HEAD | Confirmed `<repo-root>` |
| No account name anywhere on the branch | Run-time-derived token, case-insensitive, all 34 committed blobs | Confirmed 0 |
| No host or domain name either | Same sweep extended to `COMPUTERNAME` and `USERDOMAIN` | Confirmed 0 / 0 |
| The 11 `C:\Users` hits are not leaks | Printed and read every matching line | Confirmed all methodology description |
| The 3 UNC-shaped hits are not leaks | Compared base and head match sets | Confirmed pre-existing `\\bin\`, `\\obj\`, `\\ref\` |
| Remediation touched docs only | `git show --name-status` on both commits | Confirmed 10 paths, all in the feature folder |
| No config file touched branch-wide | `git diff --stat` filtered to `*.csproj`, `packages.config`, `*.targets`, `*.cs` | Confirmed empty |
| `spec.md` unaffected by remediation | Blob SHA compared at `e80f74c9` and `ff2106fe` | Confirmed identical (`843eaeda`) |
| The four logs are unchanged | Blob SHA compared at `e80f74c9` and `ff2106fe` | Confirmed identical, all four |
| The four logs still have correct line counts | Newline count on committed blobs, not `Measure-Object -Line` | Confirmed 11878/12030/11906/11742 |
| Comments do not fold into the cache key | Order-sensitive parse-tree compare, two parsers | Confirmed identical, key clean |
| Rx warnings eliminated | Counted vendor token and MSBuild `N Warning(s)` in pre and post logs | Confirmed 5 to 0, errors 0 to 0 |
| Exactly five projects were affected | Extracted `[...csproj]` tag from each inline warning | Confirmed the five named in `spec.md` |
| No new diagnostic introduced | Distinct diagnostic-code sets, pre versus post, both log pairs | Confirmed no new codes |
| `Directory.Build.props` is well-formed | `[xml]` load and property enumeration | Confirmed one property, 18 lines |
| CSharpier does not process the new file | Read `.csharpierignore` for `*.props` | Confirmed excluded |
| Worktree is clean | `git status --porcelain` | Confirmed 0 entries |
| Superseded blob persists in history | Scanned all 6 branch commits for the account token | Confirmed present in 4 |

## Conclusion

The remediation is correct, minimal, and confined to its stated scope. It closed the blocking
finding without disturbing the shipped configuration, which was re-verified independently rather
than assumed unchanged. CR-4 also closed as a side effect of committing the outstanding checklist
residue.

One new low-severity finding (CR-8) was identified — a superseded blob reachable in branch history
— with a merge-strategy remedy that requires no further edits.

**No blocking issues remain. The branch is ready for PR authoring, to be squash-merged.**
