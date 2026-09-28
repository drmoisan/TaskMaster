# Preflight round 1 delta

Timestamp: 2026-09-12T11-40
Signal returned: PREFLIGHT: REVISIONS REQUIRED
Convergence line returned: CONVERGENCE: NO FURTHER ROUNDS EXPECTED
Blocking defects: 2. Non-blocking observations: 1, declined.

## Defect 1 (blocking) — a leading-slash search literal is mangled before git sees it

The reviewer ran the plan's own command form rather than reasoning about it, and found that

```
git grep -n -F -- "/EnableCodeCoverage" CLAUDE.md
```

returns zero matches and exit 1 against a file that demonstrably contains the literal twice. The Git
Bash layer rewrites an argument beginning with a forward slash into a Windows path before git sees
it, so the search silently degrades to a token that matches nothing regardless of file content.

I reproduced this independently rather than accepting the report:

- `git grep -n -F -- "/EnableCodeCoverage" CLAUDE.md` returned no output.
- `git grep -n -F -- "EnableCodeCoverage" CLAUDE.md` returned lines 390 and 408.
- `git grep -n -E -- "[/]EnableCodeCoverage" CLAUDE.md` returned lines 390 and 408.

The consequence is a mixture of both failure directions in one defect class. Task P0-T6 becomes
unsatisfiable, because its acceptance demands exit 0 and two matching lines from a command that
always exits 1. Tasks P1-T1 and P2-T1 become vacuous, because a zero-match result is produced by the
mangling rather than by the correction, so those gates pass identically against a corrected file and
an uncorrected one.

Three sites carry the defective form: P0-T6, P1-T1, P2-T1. No other search literal in the plan begins
with a forward slash, so the class is closed at three.

### Adjudication: the reviewer's proposed fix was not adopted

The reviewer proposed prefixing each call with an environment-variable assignment that disables the
path conversion. That form does work in this shell, and I verified it does. I rejected it for two
reasons.

First, it is shell-specific. PowerShell is refused inside this preparation sandbox, but the execution
run happens later and may not be sandboxed the same way. An environment-variable assignment prefixed
to a command is not valid PowerShell syntax, so the fix would convert a silent wrong answer into a
hard parse failure in the one environment where the plan is most likely to be executed.

Second, it changes the first token of the command line away from the git executable. This
repository's command allowlist checks the first token of every chained segment, so a prefixed form
depends on an allowance that a stricter executor session may not grant.

The substituted form keeps the git executable as the first token, passes an argument that does not
begin with a forward slash so no path conversion is triggered in any shell, and expresses the leading
slash as a single-character bracket expression that the regular-expression engine matches literally.
I verified the substitute against both outcomes it must distinguish: it returns both expected lines
when the literal is present, and returns nothing when it is absent.

## Defect 2 (blocking) — AC5 demands four figures and the gate tested two

Accepted as reported, and the reviewer's replacement text for P2-T3 is adopted verbatim. The gate now
asserts all four settled figures rather than the two that the correction introduces in new wording.

Note on the fourth figure: the new-code figure already exists in the file before the change, so a
presence assertion on it cannot fail because of the correction itself. It is retained deliberately
anyway, because its reachable failure mode is the one that matters here: the correction edits a line
two lines above it, and a mis-scoped edit that deleted or corrupted the new-code line would otherwise
go undetected while AC5 was still checked off.

## Non-blocking observation, declined

The reviewer noted a prose imprecision in the parenthetical of P2-T6, which implies only one of the
two corrected step-4 lines shifts position when in fact both shift by the same amount. The reviewer
marked it non-blocking and stated it has no effect on the acceptance criterion, which is a match-count
check carrying no line number. I declined it. A reviewer's judgment that its own finding does not gate
the round is a decision, and an elective edit taken against that judgment is authored with less
scrutiny than a required one while still consuming a full confirming round.
