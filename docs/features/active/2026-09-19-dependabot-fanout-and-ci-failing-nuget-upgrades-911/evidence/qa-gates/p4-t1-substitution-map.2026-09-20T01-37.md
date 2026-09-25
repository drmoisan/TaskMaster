# R4 Substitution Map — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-05-10
- Task: [P4-T1]
- Finding: R4, **Blocking**
- EXIT_CODE: 0

**No source literal's value appears in this artifact.** Every entry is recorded by its building
expression, its replacement token and its length as an integer, per **gate rule 17**.

## The Building Expressions

Every source literal is built at run time from `$HOME`. Nothing is typed.

```powershell
$acct   = Split-Path $HOME -Leaf
$short8 = $acct.Substring(0, 6).ToUpper() + '~1'
$home8  = $HOME -replace ([regex]::Escape($acct)), $short8

$roots = @(
    @{ Suffix = '\repos\TaskMaster-wt\dependabot-911';    Token = '<execution-worktree-root>' }
    @{ Suffix = '\repos\TaskMaster-wt\2026-09-12T10-15';  Token = '<session-worktree-root>'   }
    @{ Suffix = '\repos\TaskMaster';                      Token = '<repo-root>'               }
    @{ Suffix = '';                                       Token = '<user-home>'               }
)
```

For each of `$HOME` and `$home8`, and for each of the four roots, two entries are generated:

- **backslash**: `$base + $r.Suffix`
- **forwardslash**: `($base + $r.Suffix) -replace '\\', '/'`

and two further entries cover the **dashed** spelling of the home prefix alone:

```powershell
$base -replace '[:\\]', '-'
```

That last class is the one the [P0-T3] census discovered and the plan's four-root enumeration did
not anticipate: the session scratchpad key mangles the drive colon and both separators to a single
dash, so the account name survives in a form no separator spelling matches. Only the home prefix
is mapped, because the tail names a third worktree and carries no account name.

Four roots, two separator spellings, two case spellings, plus two dashed-home entries:
**18 entries**, well above the required 8.

## The Map, Ordered Longest-First

| # | Entry name | Source literal length | Replacement token |
|---|---|---|---|
| 1 | `<session-worktree-root>` \| long \| backslash | 55 | `<session-worktree-root>` |
| 2 | `<session-worktree-root>` \| long \| forwardslash | 55 | `<session-worktree-root>` |
| 3 | `<session-worktree-root>` \| 8.3 \| forwardslash | 54 | `<session-worktree-root>` |
| 4 | `<session-worktree-root>` \| 8.3 \| backslash | 54 | `<session-worktree-root>` |
| 5 | `<execution-worktree-root>` \| long \| backslash | 53 | `<execution-worktree-root>` |
| 6 | `<execution-worktree-root>` \| long \| forwardslash | 53 | `<execution-worktree-root>` |
| 7 | `<execution-worktree-root>` \| 8.3 \| forwardslash | 52 | `<execution-worktree-root>` |
| 8 | `<execution-worktree-root>` \| 8.3 \| backslash | 52 | `<execution-worktree-root>` |
| 9 | `<repo-root>` \| long \| forwardslash | 35 | `<repo-root>` |
| 10 | `<repo-root>` \| long \| backslash | 35 | `<repo-root>` |
| 11 | `<repo-root>` \| 8.3 \| backslash | 34 | `<repo-root>` |
| 12 | `<repo-root>` \| 8.3 \| forwardslash | 34 | `<repo-root>` |
| 13 | `<user-home>` \| long \| dashed | 18 | `<user-home>` |
| 14 | `<user-home>` \| long \| backslash | 18 | `<user-home>` |
| 15 | `<user-home>` \| long \| forwardslash | 18 | `<user-home>` |
| 16 | `<user-home>` \| 8.3 \| backslash | 17 | `<user-home>` |
| 17 | `<user-home>` \| 8.3 \| forwardslash | 17 | `<user-home>` |
| 18 | `<user-home>` \| 8.3 \| dashed | 17 | `<user-home>` |

**Ordering check: each entry's source-literal length is at least the next one's. Monotonic
non-increasing: true**, verified as a measurement rather than asserted.

The ordering is load-bearing. Each root is a prefix of the shorter one below it —
`<user-home>` prefixes `<repo-root>`, which prefixes both worktree roots — so an unordered pass
would consume the short root first and leave a partial rewrite like
`<user-home>\repos\TaskMaster-wt\dependabot-911`.

## Every Census Variant Has a Map Entry

The [P0-T3] census found 7 distinct variants. Each maps to an entry above:

| Census variant | Map entry |
|---|---|
| `<execution-worktree-root>` \| long \| backslash, 53 occurrences | 5 |
| `<session-worktree-root>` \| long \| backslash, 23 | 1 |
| `<user-home>` \| long \| dashed, 9 | 13 |
| `<execution-worktree-root>` \| long \| forwardslash, 8 | 6 |
| `<user-home>` \| long \| backslash, 7 | 14 |
| `<repo-root>` \| long \| backslash, 2 | 10 |
| `<user-home>` \| 8.3 \| forwardslash, 1 | 17 |

The census reported `UNCLASSIFIED: 0` against this map, which is the check that no variant lacks
an entry. The 11 unused entries cover spellings that are absent from this branch's footprint and
cost nothing to carry.

## Substitution Method

Substitution is performed with **`[regex]::Replace` under `IgnoreCase`**, not with
`String.Replace`.

`String.Replace` is case-sensitive and the [P0-T3] census matched case-insensitively, so a
mixed-case spelling of any root would survive a case-sensitive pass and appear in the [P4-T3]
residual. The 8.3 spellings in particular are conventionally upper-case while the long spellings
are mixed-case, and both appear in this footprint.

The rewrite itself is byte-exact through `[System.IO.File]::ReadAllText` and `WriteAllText`, per
**gate rule 15**. `sed` through the Bash tool is prohibited.

## Output Summary

18 map entries derived at run time from `$HOME`, ordered longest-first with a verified monotonic
non-increasing length sequence, each recorded by building expression, token and length. All 7
census variants are covered. No source literal value appears in this artifact.
