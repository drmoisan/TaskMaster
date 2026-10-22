# P5-T11 — AnalyzerItemRepair suite authored

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = "<execution-worktree-root>\tests\scripts\dependencies\AnalyzerItemRepair.Tests.ps1"; line count, It count, per-token It-name counts, Describe and Context names matching AC\d, temporary-file idiom count'
```

EXIT_CODE: 0

## Output Summary

```
LINES=308
IT=13
AC12=10
AC13=3
BLOCK_ACDIGIT=0
TEMPFILE=0
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| File at most 500 lines | <= 500 | 308 |
| `It` blocks | at least 12 | 13 |
| `It` names beginning `AC12-` | at least 9 | 10 |
| `It` names beginning `AC13-` | at least 3 | 3 |
| `Describe` and `Context` names matching `AC\d` | exactly 0 | 0 |
| Creates no temporary file | 0 idioms | 0 |

## The thirteen cases

Derivation, all supplied with an injected directory listing and none touching disk:

1. `AC12-` plain language-folder shape.
2. `AC12-` Roslyn-qualified shape.
3. `AC12-` multi-assembly shape whose four assembly names do not match the package
   identifier. An implementation computing the path from the package identifier fails here,
   because no assembly is called `Roslynator.Analyzers.dll`.
4. `AC12-` shape with no intermediate folders, the Sonar shape.
5. `AC12-` exclusion of non-C-sharp language folders and satellite resource assemblies.
6. `AC12-` a package whose listing contains no analyzer directory contributes no items —
   an empty derivation, not a throw.

Repair under the preserve rule:

7. `AC12-` Meziantou-shaped fixture: the listing offers `roslyn4.14`, `roslyn4.8`,
   `roslyn5.0`, `roslyn5.6` and `roslyn5.9` and the existing item names `roslyn5.0`. The
   repaired path still names `roslyn5.0`.
8. `AC12-` Roslynator-shaped fixture: the listing offers `roslyn3.8`, `roslyn4.7` and
   `roslyn5.0` and the existing item names `roslyn4.7`. The repaired path still names
   `roslyn4.7`, and the case additionally asserts `roslyn5.0` does not appear on the line.
9. `AC12-` the preserved segment is absent from the new version's listing: the text is
   returned byte-identical, no repair is recorded, and one missing-segment **record** is
   raised naming the project, the item, the missing segment `dotnet\roslyn5.0\cs` and the
   segments the listing does offer. The record, not a report or a count: those belong to
   `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1`.
10. `AC12-` the restored package directory is absent entirely — the listing delegate returns
    nothing — and the repair throws rather than emitting a guessed path.

Sibling survival:

11. `AC13-` the `<AdditionalFiles>` element naming the banned-symbols list and the
    explanatory comment preceding the items both survive.
12. `AC13-` a project with no analyzer item group is returned byte-identical with no item
    group synthesised, and the examined item count is 0.
13. `AC13-` a project carrying **two** separate analyzer item groups has the items in both
    repaired. The fixture exists because `VBFunctions.Test/VBFunctions.Test.csproj` has that
    shape at lines 263-265 and 287-294, and a single-group assumption would silently drop
    one. The case asserts zero residual lines at the stale version and exactly two at the
    manifest version.

## Why both preservation fixtures are constructed this way

In each fixture the highest available folder is deliberately **not** the one the existing
item names. A selection implementation therefore fails both, which is the property AC12
requires the suite to have. The construction reflects the repository as measured:
Meziantou items sit at `roslyn5.0` with `roslyn5.6` and `roslyn5.9` present, and Roslynator
items sit at `roslyn4.7` with `roslyn5.0` present, so a selection rule would rewrite all 80
items in those two families rather than the 15 this change owns.

No constraint is placed on the words an `It` name may contain. Case 7 is named "preserves
roslyn5.0 rather than selecting the highest offered folder", which is the clearest
available name for what it asserts; the implementation-side prohibition on a
folder-ordering expression is enforced at P5-T12, where it belongs.

## State of the tree at authoring time

`scripts/dependencies/AnalyzerItemRepair.psm1` is a declared pass-through for its two
repair surfaces at this point, so these thirteen cases are expected to fail until P5-T12
implements them. P5-T13 and P5-T14 capture the green runs.
