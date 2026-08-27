# P8-T3 - Redaction sweep (#614; AC21)

Timestamp: 2026-08-26T18-50

The executor's real Windows account name and the host name are passed to every search as a
PowerShell variable and are deliberately never written into this artifact. They are referred to
below as `<user>` and `<host>`.

## Commands

All run under `pwsh -NoProfile` from the repository root. EXIT_CODE: 0 for every command.

```
$account = [Environment]::UserName
$host8   = [Environment]::MachineName
$base    = "c279d40bddacdba00c29a9724d1b5b17f9ebbc90"

# 1. Account-name and host-name search over WHOLE changed files
$changed = @(git diff --name-only $base) + @(git ls-files --others --exclude-standard) |
           Sort-Object -Unique | Where-Object { $_ -and (Test-Path $_) }
foreach ($f in $changed) { Get-Content -Raw -LiteralPath $f | Select-String -SimpleMatch $account }
foreach ($f in $changed) { Get-Content -Raw -LiteralPath $f | Select-String -SimpleMatch $host8 }

# 2. Address search restricted to ADDED lines (changed hunks) plus every new file
git diff -U0 $base -- "*.cs" "*.csproj" "packages.config"   # '+' lines only
#   regex: [A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}
#   allowlist: mailbox@example.com, other@example.org

# 3. Pre-existing-placeholder confirmation
git diff -U0 $base -- UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs
```

EXIT_CODE: 0

## Output Summary

- Changed-file set size: 64 files (merge-base diff plus untracked additions).
- Added-line set size: 2276 lines across `*.cs`, `*.csproj`, `packages.config`, and every new
  source file.
- Host-name search over whole changed files: **0 hits**.
- Address search over added lines: **0 violations**. Every `@`-bearing string added by this change
  is one of the two allowlisted fabricated placeholders.
- Account-name search over whole changed files: **1 hit**, analysed below as a pre-existing
  exception outside every hunk this change made.

## Negative claims (auditable)

### Claim 1 - account name, scoped to whole changed files

- `SearchScope:` all 64 paths reported by `git diff --name-only <merge-base>` plus
  `git ls-files --others --exclude-standard`, read whole-file.
- `SearchPatterns:` `[Environment]::UserName` as a literal (SimpleMatch), value withheld.
- `SearchResult:` **1 file** - `TaskMaster/TaskMaster.csproj`, at line 37. See the recorded
  exceptions below. Restricted to the lines this change actually adds or modifies, the result is
  `none`.

### Claim 2 - host name, scoped to whole changed files

- `SearchScope:` the same 64 paths, read whole-file.
- `SearchPatterns:` `[Environment]::MachineName` as a literal (SimpleMatch), value withheld.
- `SearchResult:` `none`.

### Claim 3 - mail addresses, scoped to changed hunks

- `SearchScope:` every `+` line of `git diff -U0 <merge-base>` over `*.cs`, `*.csproj` and
  `packages.config`, plus the full text of every untracked new `.cs`/`.csproj` file. 2276 lines.
- `SearchPatterns:` the regex `[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}`, with
  `mailbox@example.com` and `other@example.org` allowlisted as the fabricated placeholders this
  change is required to use.
- `SearchResult:` `none`.

## Recorded pre-existing exceptions (not violations introduced by this change)

### Exception 1 - `first.last@company.com` in `FolderConverterTests.cs`

`UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` contains the fabricated literal
`first.last@company.com` at lines **22 and 23**. It is a pre-existing test placeholder, not a real
address. This change's ONLY edit to that file is the P5-T4 assertion correction at `:329`, which
`git diff -U0` confirms:

```
@@ -329 +329 @@ namespace UtilitiesCS.Test.OutlookObjects.Folder
-            result["Remove illegal characters"]().GetAwaiter().GetResult().Should().BeEmpty();
+            result["Remove illegal characters"]().GetAwaiter().GetResult().Should().Be("BadName");
```

No changed hunk contains the literal. Recorded as a pre-existing fabricated placeholder.

### Exception 2 - `<PublishUrl>` in `TaskMaster/TaskMaster.csproj`

`TaskMaster/TaskMaster.csproj` line 37 is a pre-existing ClickOnce `<PublishUrl>` element carrying
a user-profile path and an organization name. It reads, with the account token redacted:

```
    <PublishUrl>C:\Users\<user>\OneDrive - <Org>\TM\</PublishUrl>
```

This change did **not** introduce, modify, or touch that line. Proof: `git diff -U0` for the file
against the merge-base produces exactly one hunk, and it is 379 lines away:

```
@@ -415,0 +416 @@
+    <Compile Include="AppGlobals\ArchiveRootPathGuard.cs" />
```

The file appears in this change's path set only because the optional `ArchiveRootPathGuard.cs`
required a `<Compile Include>` item, which the plan's P8-T2 in-scope list explicitly permits.
Recorded as a pre-existing repository condition. Remediating it is outside this change's scope and
is flagged for follow-up triage.

### Exception 3 - two regex false positives in pre-existing markdown

A wider address sweep over the feature folder's markdown reported `Contosomailbox@example.com`
twice: once in `evidence/regression-testing/p1-t2-primary-regression-fail-before.2026-08-26T11-45.md`
and once in `research/2026-08-26T10-30-store-root-path-leak-defect-census-research.md`. Both are
concatenation artifacts of a quoted exception message whose path separators were dropped when the
document was authored (`...OneDrive - Contoso` immediately followed by `\\mailbox@example.com`).
The organization token is the fabricated `Contoso` and the domain is the fabricated `example.com`,
so neither is a real identifier and neither is an AC21 violation. Both documents predate this
task's edits and are left unmodified.

## Path literals in files added or modified by this change

Every Outlook and filesystem path literal introduced by this change uses a fabricated placeholder:
`\\mailbox@example.com` and `\\mailbox@example.com\Archive` for the Outlook store and archive
root, `\\other@example.org` for the cross-store cases, `\\fileserver\Archive` for the UNC
ancestor case, `C:\Users\testuser\OneDrive - Contoso` for the OneDrive-for-business root, and
`C:\Mail`, `C:\Mail Archive [2026]`, `C:\OneDrive` for the remaining filesystem roots. No
production message, log line, or exception message added by this change embeds any path value at
all: each names the violated rule only and states that the value is withheld.
