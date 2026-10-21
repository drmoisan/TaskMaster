# Scope boundary AC17 — the Finding 3 production file was not touched (P7-T1)

Timestamp: 2026-09-03T00-02

EXIT_CODE: 0

## Base re-derivation (D11)

```
$base = (git merge-base origin/main HEAD).Trim()
```

Observed `$base`: `8be5a6aac3b5a82c86241fbbf989fd9118602c56`

`BaseRef:` recorded by P0-T14: `8be5a6aac3b5a82c86241fbbf989fd9118602c56`

The two values are equal, so this task proceeds on the recorded anchor.

## Command 1 — anchored diff

```
git diff --name-only $base HEAD -- 'UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs'
```

Output:

```
(empty)
```

## Command 2 — working-tree status

```
git status --porcelain -- 'UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs'
```

Output:

```
(empty)
```

The two commands are complementary: the anchored diff covers everything committed on this branch
since the base, and the porcelain status covers uncommitted and untracked working-tree state. Both
being empty establishes that the file is untouched in both states.

## Command 3 — absence of the rejected production-side fix

```
Select-String -SimpleMatch 'TextWriter' -Path 'UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs'
```

Matches: 0

This confirms that no injectable `TextWriter` seam was added to the production parser. Finding 3
was resolved on the test side alone, by marking the two `Console.Out`-capturing test classes
`[DoNotParallelize]`, which is what AC17 requires.

Output Summary: All three checks pass. The anchored diff returns empty, the porcelain status
returns empty, and the file contains zero occurrences of `TextWriter`. The Finding 3 production
file `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs` is unmodified by this change.
