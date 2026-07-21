# Batch S2a Nullable Gate — StoreWrapper.cs (P8-T2)

Timestamp: 2026-07-19T14-40

- csharpier format EXIT 0; full /t:Build EXIT 0.
- Scoped gate: **zero CS86xx** for StoreWrapper.cs (AC1).

## Key annotation decisions
- Properties populated only inside Init()/Restore() are nullable: `DisplayName`, `StoreId`, `InnerStore`,
  `Inbox`, `RootFolder`, `UserEmailAddress`, `GlobalAddressBook`.
- `GetSmtpAddressFromStore()` returns `string?` (the `catch (COMException)` path returns null).
- Init()'s COM derefs of the ctor-set `InnerStore` use `InnerStore!` (justified: the ctor and Restore set it
  before Init runs) — no new runtime guards. Configurable `= new()` properties (ArchiveRoot, ArchiveFsRoot,
  JunkPotential, JunkCertain) stay non-null.
- External oblivious `UtilitiesCS.Threading.CurrentStoreContext` consumed at the call site only (not edited).
  No post-condition attributes; no record/init.
