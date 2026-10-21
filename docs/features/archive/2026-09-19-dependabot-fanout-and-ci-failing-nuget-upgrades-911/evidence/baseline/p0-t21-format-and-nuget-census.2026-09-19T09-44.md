# P0-T21 — Formatting-Scope and NuGet-Selector Census

Timestamp: 2026-09-19T23-13

Commands:

```
git show HEAD:.csharpierignore
git grep -c -E "packages\.config|app\.config" -- ".csharpierignore"
git grep -n -E "nuget-version|setup-nuget" -- ".github/workflows/*.yml"
git ls-files -- ".github/workflows/*.yml"
```

EXIT_CODE: 0

## 1. `.csharpierignore`, full verbatim contents

```
# CSharpier formats C# source only. Generated coverage and test-result
# artifacts are committed as audit-trail evidence (not source) and must not
# be subject to formatting checks (e.g. trailing-newline rules on tool output).
**/evidence/**
*.cobertura.xml
*.coverage
*.coveragexml
*.trx
# Project files (*.csproj/*.props/*.targets) are owned by Visual Studio and are
# not C# source. CSharpier formats C# source only (per CLAUDE.md C#1), so exclude
# project files from the formatting check.
*.csproj
*.props
*.targets
```

**Line count: 14.**

Line 4 is `**/evidence/**`, which is the exclusion gate rule 12 relies on: it keeps every copy this
plan places under the feature `evidence/` tree out of the formatter's reach, so a copied coverage
projection cannot be rewritten by a later format step.

## 2. Lines in `.csharpierignore` matching `packages.config` or `app.config`

**0.**

Neither pattern is present. The formatter therefore currently owns both file kinds, which is
exactly the condition P1-T2 changes: it adds a line whose text is exactly `**/packages.config` and
a line whose text is exactly `**/app.config`, each preceded by a one-line comment, leaving the 14
lines above unchanged.

This zero is the reason P1-T7's normalisation must run **after** P1-T2. A normalisation performed
while the formatter still owns those paths is undone by the next format step, which would make AC3
unsatisfiable.

## 3. NuGet selector lines across `.github/workflows/*.yml`

Every line matching `nuget-version` or `setup-nuget`, with file and line number:

```
.github/workflows/_build-analyzers.yml:31:        uses: nuget/setup-nuget@v2
.github/workflows/_build-analyzers.yml:33:          nuget-version: latest
.github/workflows/_build-nullable.yml:31:        uses: nuget/setup-nuget@v2
.github/workflows/_build-nullable.yml:33:          nuget-version: latest
.github/workflows/_mstest-coverage.yml:47:        uses: nuget/setup-nuget@v2
.github/workflows/_mstest-coverage.yml:49:          nuget-version: latest
```

| Measurement | Count | Sites |
|---|---|---|
| `nuget-version: latest` lines | **3** | `_build-analyzers.yml:33`, `_build-nullable.yml:33`, `_mstest-coverage.yml:49` |
| `nuget/setup-nuget@v2` step lines | **3** | `_build-analyzers.yml:31`, `_build-nullable.yml:31`, `_mstest-coverage.yml:47` |

The three `nuget-version: latest` sites are exactly the three the plan records, at exactly the line
numbers it records. These are the sites P1-T12 pins to `7.9.0`.

## 4. Workflow YAML file count

**8.**

```
.github/workflows/_actionlint.yml
.github/workflows/_build-analyzers.yml
.github/workflows/_build-nullable.yml
.github/workflows/_format-check.yml
.github/workflows/_mstest-coverage.yml
.github/workflows/_pester.yml
.github/workflows/ci.yml
.github/workflows/codex-web-setup-test.yml
```

This is the figure P4-T5 compares against: it becomes **9** once `dependabot-repair.yml` is
created.

## Non-vacuity

The zero in section 2 is guarded by four positive counts, per gate rule 2: the 14-line
`.csharpierignore` recorded verbatim, the 3 `nuget-version: latest` lines, the 3
`nuget/setup-nuget@v2` step lines, and the 8 workflow files. A census that resolved no file or
matched no pattern would have reported 0 for all five and failed on the four positives, so the
single zero cannot pass for a reason unrelated to the property it asserts.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `.csharpierignore` match count for `packages.config` or `app.config` | exactly 0 | 0 | PASS |
| `nuget-version: latest` lines | exactly 3, at `_mstest-coverage.yml:49`, `_build-nullable.yml:33`, `_build-analyzers.yml:33` | 3, at those exact sites | PASS |
| `nuget/setup-nuget@v2` step lines | exactly 3 | 3 | PASS |
| Workflow YAML file count | exactly 8 | 8 | PASS |
| `.csharpierignore` recorded verbatim with its line count | required | recorded, 14 lines | PASS |

Output Summary: `.csharpierignore` is **14** lines and contains **0** lines matching
`packages.config` or `app.config`, so the formatter currently owns both kinds — the condition P1-T2
changes and the reason P1-T7 must follow it. `.github/workflows/` carries exactly **3**
`nuget-version: latest` lines, at `_build-analyzers.yml:33`, `_build-nullable.yml:33` and
`_mstest-coverage.yml:49`, alongside exactly **3** `nuget/setup-nuget@v2` step lines; these are the
sites P1-T12 pins to `7.9.0`. The workflow YAML file count is **8**, the figure P4-T5 compares
against once `dependabot-repair.yml` makes it 9.
