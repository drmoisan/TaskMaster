# Binding, Serialization and COM-Visibility Surface (P2-T4)

- **Issue:** #635
- **Plan task:** [P2-T4]

Timestamp: 2026-08-29T06-35

## Output Summary

All eight data-binding and serialization patterns return a production-tree count of zero, and the
QuickFiler production assembly declares `[assembly: ComVisible(false)]`. The affected type therefore
carries no property-name string binding surface and no serialization surface, and the assembly is not
COM-visible, so no host-side late-binding path can reach a member of that type by name.

BINDING_SERIALIZATION_PATTERNS: 8

## Command 1 — data-binding and serialization patterns

Command:

```
pwsh -NoProfile -Command '@("DataBindings.Add","DisplayMember","ValueMember","DataPropertyName","[Serializable","DataContract","JsonProperty","XmlElement") | ForEach-Object { $p = $_; $prod = @(git grep -n -I -F -e $p -- "QuickFiler/*").Count; Write-Output ($p + " prod=" + $prod) }'
```

Output, verbatim:

```
DataBindings.Add prod=0
DisplayMember prod=0
ValueMember prod=0
DataPropertyName prod=0
[Serializable prod=0
DataContract prod=0
JsonProperty prod=0
XmlElement prod=0
```

EXIT_CODE: 0

The command printed eight rows and every row printed `prod=0`. The `pwsh -NoProfile -Command` wrapper
exits `0` regardless of what runs inside it, so only the printed values are asserted for this command.

## Command 2 — COM visibility

Command:

```
git grep -n -I -F -e "[assembly: ComVisible(false)]" -- "QuickFiler/*"
```

Output, verbatim:

```
QuickFiler/Properties/AssemblyInfo.cs:22:[assembly: ComVisible(false)]
```

EXIT_CODE: 0

The command exited `0` and its printed output names one line in the QuickFiler production tree's
assembly-information source file, QuickFiler/Properties/AssemblyInfo.cs, at line 22.

The literal `[assembly: ComVisible(false)]` is present in the tracked tree at the base commit; this
task neither creates nor modifies it. The QuickFiler production tree is read and searched only for the
duration of this item.

## The affirmative conclusion this evidence supports

The affected type carries no property-name string binding surface and no serialization surface, and the
assembly is not COM-visible, so no host-side late-binding path — a VBA `CallByName`, an
`Application.Run`, or an Outlook macro — can reach a member of that type by name.

Each of the three limbs rests on a distinct measurement:

- **No property-name string binding surface.** `DataBindings.Add`, `DisplayMember`, `ValueMember` and
  `DataPropertyName` are the WinForms constructs that resolve a member by a property-name string at run
  time. All four return zero over the production tree.
- **No serialization surface.** `[Serializable`, `DataContract`, `JsonProperty` and `XmlElement` are the
  attribute forms by which a serializer would be directed at named members. All four return zero over
  the production tree, so no serializer is configured to resolve a member of `QfcCollectionController`
  by name.
- **Not COM-visible.** `[assembly: ComVisible(false)]` at QuickFiler/Properties/AssemblyInfo.cs line 22
  suppresses COM registration for every type in the assembly, so no `IDispatch` late-binding client can
  obtain a dispatch identifier for a member name on any of them.

This is the affirmative argument the AC-16 record did not make. It complements the negative evidence of
[P2-T1] and [P2-T2]: those establish that nothing inside the production assembly resolves a member by
name, and this establishes that nothing outside it can either.

## Auditable-absence record for the eight zero results

SearchScope: the tracked files matching the pathspec `QuickFiler/*`. [P2-T1] measured that scope as `QF_PROD_SCOPE_FILES=228` tracked files, so the search set is non-empty for every one of the eight rows.

SearchPatterns: the eight fixed strings `DataBindings.Add`, `DisplayMember`, `ValueMember`, `DataPropertyName`, `[Serializable`, `DataContract`, `JsonProperty` and `XmlElement`, matched with `git grep -F` so that the leading `[` of `[Serializable` is treated literally rather than as a character-class opener.

SearchResult: none for all eight patterns. Each printed `prod=0`.
