# POSTING BLOCKED — Issue #797 update mirror

Timestamp: 2026-09-07T10-15

PostedAs: unknown

Reason posting is blocked: the executing agent is directed not to create a pull request, not to run
GitHub CLI commands that create or edit issues or pull requests, and not to merge anything. Issue and
pull-request authoring is retained by the caller. The text below is therefore recorded as the intended
update and is not posted from this session. Because `PostedAs:` is not `body`, no additional mirror
into the feature-folder issue.md is made beyond the acceptance-criteria check-offs that P6-T2 through
P6-T9 already applied there.

---

## Exact text intended for the issue

Both root causes are fixed and seven of the eight acceptance criteria are verified by automated test.
AC3 requires a live Outlook restart and is handed over for manual verification.

**Root cause 1 — the bootstrap gap between the loader and the serializer.**

- AC1: `LoadStoresAsync` now applies the already-resolved loader configuration to the freshly built
  stores wrapper, so a fresh install carries the resource-defined path instead of an empty one. The
  fix is confined to the store-loading globals partial; the shared deserialize overload is unchanged,
  because its null return is a load-bearing fail-soft contract for the folder-predictor load path.
- AC2: `SmartSerializable<T>.Serialize()` now rejects a null or empty configured path with an
  error-level log naming the serialized item type and the rejected value, instead of returning
  silently. The previous guard compared only against the empty string, so a null path passed it.
- AC4: a new explicit-save entry point writes inline through the existing thread-safe write method
  rather than through the three-second deferred timer, so a save is not lost when Outlook exits inside
  that window. The deferred behaviour for every other caller is unchanged, and that is pinned by its
  own test. The AC2 guard is evaluated first so the fix does not substitute one silent failure for
  another.
- AC5: the junk-folder call site no longer locates its target by reflecting over a method name. A new
  narrow interface in UtilitiesCS is implemented explicitly by the TaskMaster globals partial, so the
  call is compile-checked and a globals implementation that does not provide the seam is reported at
  error level rather than as a warning.

**Root cause 2 — the unretried COM failure in the Exchange SMTP lookup.**

- AC6: the lookup now falls back in a fixed order — the Exchange primary SMTP address, then the
  address entry's own address when it contains an at-sign, then the store display name when it does —
  with per-step COM handling instead of one outer catch. The failure reason is captured, the dialog
  renders a specific unavailability message naming that reason instead of the generic placeholder, and
  the lookup is retried once per dialog open when the address is null.

**Adjacent defects in the same rendering method.**

- AC7: Inbox and Root Folder are rendered without the leading store prefix.
- AC8: a null current store selection now renders the existing placeholder text instead of throwing.
  One existing test that asserted the throw is deliberately inverted; the inverted assertion is
  stricter, pinning specific rendered values rather than an exception type.

**AC3 — manual verification outstanding.** A value saved in Folder Settings surviving an Outlook
restart cannot be verified without a live VSTO host. The automated tests establish that the disk path
is now populated and that the explicit save writes through the injectable seam, but not that the file
appears on disk in a live host. A nine-step manual procedure is recorded in the feature folder under
`evidence/other/`, together with a fail-before exception dossier explaining why no automated failing
run is possible.

**Verification.** CSharpier format and check, the analyzer rebuild and the warnings-as-errors rebuild
all exit 0 with zero warnings and zero errors. The scoped test run over the two affected assemblies is
green at 5262 passed and 0 failed, up from 5237 at baseline; all sixteen tests that failed before the
fix now pass. Changed-line coverage over the executable, non-relocated changed lines is 91.09 percent,
and document-level line coverage did not regress, moving from 53.23 to 53.26 percent under the same
two-assembly scope.

**Known limitations, recorded rather than resolved.** The serializer file remains over the 500-line
cap, a pre-existing condition this change deliberately does not resolve. The AC6 retry reintroduces one
synchronous Outlook COM read on the UI thread at dialog-open time, bounded to a single lookup and only
when the address is null; a genuinely non-blocking read is out of scope. The QuickFiler
recipient-resolution blocking hazard is a different caller of the same Outlook getter and is not fixed
here. Four shell-icon test classes are excluded from every local run for environmental reasons
unrelated to this change; CI covers them.

Output Summary: The intended issue update is recorded verbatim above and was not posted from this
session, because issue and pull-request authoring is retained by the caller.
