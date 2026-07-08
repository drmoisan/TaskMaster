# AC Source Confirmation (P0-T6)

Timestamp: 2026-07-07T23-01

Work Mode: full-bug (from spec.md metadata and plan). Authoritative AC source: `spec.md`.

Authoritative requirements source: `docs/features/active/2026-07-07-folder-settings-store-model-null-262/spec.md`

Acceptance Criteria heading location: `## Acceptance Criteria` at spec.md line 296.

AC items found beneath the heading (count = 7):
- AC1 (spec.md:297) — config missing -> fresh build via BuildFreshStoresWrapper(), not null.
- AC2 (spec.md:301) — null deserialize -> fresh-build fallback; AwaitStoreRewireAsync not invoked.
- AC3 (spec.md:304) — genuine failure surfaced at Error with exception; no throw; no retry; no new dialog.
- AC4 (spec.md:309) — Launch() opens with populated model; StoreWrapperController.cs unmodified.
- AC5 (spec.md:313) — deterministic MSTest suite (fail-before/pass-after); invert mis-specified test; Moq; no live Outlook/temp files.
- AC6 (spec.md:317) — AppOlObjects.cs <= 500 via new partial AppOlObjects.StoreLoading.cs; both files <= 500.
- AC7 (spec.md:320) — full C# toolchain in order; new-code coverage target; no repo regression; net48.

user-story.md: ABSENT. This is expected and NOT a blocker for full-bug mode (spec.md is the
sole authoritative AC source; user-story.md is optional/absent by default). Verified via
directory listing of the feature folder.
