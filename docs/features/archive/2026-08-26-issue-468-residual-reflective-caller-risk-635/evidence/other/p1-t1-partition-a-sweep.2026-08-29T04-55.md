# Partition A Sweep (P1-T1) — discharges AC-2

- **Issue:** #635
- **Plan task:** [P1-T1]

Timestamp: 2026-08-29T06-26

ExpectedExitCode: 1

## Output Summary

The thirteen-identifier sweep over tracked non-`.cs` files outside the docs tree and the .claude tree
selected no line and exited `1`. `git grep` exits `1` when it selects no line, so `1` is the success
code for this gate and the artifact declares it. The scope over which the zero was measured is 683
tracked files, recorded by [P0-T5]. The non-vacuity of that scope is proved separately by [P1-T2],
which runs the identical pathspec for a token that is genuinely present and returns thirteen hits.

## Command

Command:

```
git grep -n -I -F -e WireUpKeyboardHandler -e AnyOpenDropDownsAsync -e LoadGroups_02cAsync -e LoadGroups_02bAsync -e LoadGroup_03bAsync -e LoadConversationsAndFoldersAsync -e LoadItemGroup -e LoadSequentialAsync -e LoadGroupSequential -e CacheTlpForMove -e SwapTlp -e CaptureTlpTemplate -e _templateTlp -- ":(exclude)*.cs" ":(exclude)docs/*" ":(exclude).claude/*"
```

Output, verbatim:

```
(no output)
```

EXIT_CODE: 1

The observed exit code equals the declared expectation, so this gate normalizes to a pass. This
artifact contains this one gate only, and is not combined with any gate whose expected exit code is
`0`, because `ExpectedExitCode` is declared per artifact file rather than per gate.

## Auditable-absence record

SearchScope: tracked files matching the pathspec `":(exclude)*.cs" ":(exclude)docs/*" ":(exclude).claude/*"` — that is, every tracked file in the repository that is not a `.cs` file, is not under the docs tree, and is not under the .claude tree. [P0-T5] measured this identical pathspec and printed `SCOPE_FILES=683`, so the search set contains 683 files and is not empty. The census of those 683 files spans twelve leading extensions, `.md 190`, `.toml 96`, `.svg 77`, `.resx 62`, `.ps1 51`, `.config 38`, `.png 28`, `.json 28`, `.csproj 18`, `.bak 11`, `.txt 9` and `.sh 9`, eight of which lie outside the six build-input extensions the AC-16 search covered.

SearchPatterns: the thirteen fixed strings `WireUpKeyboardHandler`, `AnyOpenDropDownsAsync`, `LoadGroups_02cAsync`, `LoadGroups_02bAsync`, `LoadGroup_03bAsync`, `LoadConversationsAndFoldersAsync`, `LoadItemGroup`, `LoadSequentialAsync`, `LoadGroupSequential`, `CacheTlpForMove`, `SwapTlp`, `CaptureTlpTemplate`, `_templateTlp`, supplied as separate `-e` operands and matched as fixed strings by `-F`. Identifier 7 is supplied as the bare stem `LoadItemGroup`, which is the broader form: it additionally matches the live preserved member `LoadItemGroupsAndViewers_02`, so this sweep is broader than the removed member set rather than narrower.

SearchResult: none. No file in the 683-file scope contains any of the thirteen identifiers on any line.

## Notes on the command form

`-I` suppresses binary files, which prevents `Binary file ... matches` lines from entering a result set
the plan requires to be enumerated line by line. The scope includes 77 `.svg` files and 28 `.png`
files, so this flag is load-bearing rather than decorative.

`-F` matches the operands as fixed strings, so no character in any identifier is interpreted as a
regular-expression metacharacter. The identifier `_templateTlp` and the underscore-and-digit forms
`LoadGroups_02cAsync`, `LoadGroups_02bAsync` and `LoadGroup_03bAsync` are matched literally.

The `":(exclude)..."` pathspec-magic long form is used rather than the `":!..."` shorthand. Both are
valid git pathspecs; the long form avoids any interaction with `!` in a shell with history expansion
enabled.

`git grep` without `--cached`, `--no-index`, or a tree-ish operand searches the tracked files in the
working tree. Untracked and ignored files are therefore outside this sweep by construction. [P1-T5]
runs the supplementary pass over untracked, unignored files, and the specification records the
exclusion of ignored paths — build output, intermediate object directories, restored package payloads,
test result directories, generated coverage output, and local agent state — as a deliberate scoping
decision, on the ground that a hit in any of them would be a consequence of a tracked source file and
never an independent cause.
