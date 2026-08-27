# taskmaster-csproj-publishurl-leaks-user-profile-path-and-org-name (Issue #628)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/taskmaster-csproj-publishurl-leaks-user-profile-path-and-org-name/ (Issue #628)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

> Redaction note: this record deliberately does not reproduce the leaked values. It names the file
> and line so a maintainer can read them locally.

- Issue: #628
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/628
- Last Updated: 2026-08-26
## Summary

`TaskMaster/TaskMaster.csproj` line 37 contains a `<PublishUrl>` element whose value is an absolute
path under a specific developer's Windows user profile, including that developer's account name and
their employer's organization name as part of a OneDrive commercial folder name. Both are committed
to version control and are visible to anyone who can read the repository, including in the public
GitHub UI, in every clone, and in the full history.

This is the same class of host-identifier leakage tracked by open issue #602, which to date has been
scoped to runtime exception messages. This occurrence is different in kind and worse in durability:
an exception message exists only in a log on one machine, whereas a committed project file
republishes the identifiers to every reader of the repository indefinitely, and rewriting them out
of history is disruptive in a way that fixing a log message is not.

The element is ClickOnce publish configuration. It is a per-developer local convenience setting that
has no business being shared: a second developer publishing from a different machine would be
publishing into a path that does not exist for them, so the committed value is not merely a leak but
is also wrong for everyone except its author.

Found during the issue #614 redaction sweep, which searched every file that change touched for the
executing account name and for `@`-bearing strings. The sweep is scoped to changed hunks; this line
is not in any hunk of #614 (that file's only #614 hunk is at line 416), so the finding is reported
rather than fixed there. It is filed separately rather than absorbed because #614 is a
path-representation defect chain and this is committed configuration, and because the correct fix
touches build configuration and possibly history rather than the filing boundary.

Recommended remediation, in order: remove the element from the tracked file, or replace it with a
non-identifying relative or placeholder value; move the real value into a gitignored local settings
file such as a `.user` file if the developer still wants it; then decide deliberately whether the
historical occurrences warrant a history rewrite or are accepted as already-published. That last
decision is a maintainer call, not an automated one, and should be recorded either way.

Worth checking as part of the same fix whether any sibling project file carries an equivalent
per-developer `<PublishUrl>`, `<InstallUrl>`, or similar ClickOnce path.

## Environment

- OS/version: Windows 11 Pro 10.0.26200; .NET Framework 4.8.1 VSTO project.
- Python version: Not applicable; this is MSBuild project configuration.
- Command/flags used: redaction sweep over files changed by issue #614; the hit is in a file that
  #614 touches at an unrelated line.
- Data source or fixture: `TaskMaster/TaskMaster.csproj` at line 37 on the current branch and on
  `main`.

## Steps to Reproduce

1. Open `TaskMaster/TaskMaster.csproj` and read line 37.
2. Observe the `<PublishUrl>` value contains a real Windows user-profile path including an account
   name and an organization name.
3. Run `git log -p -- TaskMaster/TaskMaster.csproj` and observe the value is present historically,
   not only at `HEAD`.

## Expected Behavior

No tracked file contains a real user account name, user-profile path, host name, or organization
name. Per-developer publish settings live in a gitignored local file, not in the shared project.

## Actual Behavior

The identifiers are committed in a tracked project file and present in history.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: intentionally omitted so this record does not itself republish the values. See
  `TaskMaster/TaskMaster.csproj:37`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

No functional impact and no credential exposure. The disclosure is of a developer account name and
an employer name, is already published to every reader of the repository, and is durable in history.

## Suspected Cause / Notes

- `TaskMaster/TaskMaster.csproj` line 37, `<PublishUrl>`.
- ClickOnce publish settings written by the IDE into the shared project file rather than a
  per-user file.
- Related: open issue #602 (host-identifier leakage), which currently covers runtime exception
  messages; this extends the same concern to committed build configuration.

## Proposed Fix / Validation Ideas

- [ ] Remove `<PublishUrl>` from the tracked project file, or replace it with a non-identifying
      placeholder or relative value.
- [ ] Relocate any genuinely wanted local value to a gitignored per-user file.
- [ ] Audit sibling project files for equivalent per-developer ClickOnce paths (`<PublishUrl>`,
      `<InstallUrl>`, and similar).
- [ ] Make an explicit, recorded maintainer decision on whether to rewrite history or accept the
      historical occurrences.
- [ ] Add a repository guard that fails when a tracked file contains an absolute path under
      `C:\Users\`.
- [ ] Unit coverage areas: not applicable; this is build configuration.
- [ ] Integration scenario to retest: solution build and, if ClickOnce publish is still used, a
      publish from a clean clone.
- [ ] Manual verification notes: confirm the build succeeds after removal, since `<PublishUrl>` is
      not required for a normal compile.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
