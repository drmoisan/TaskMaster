# Final AC6 — No post-condition attributes / records / init (P12-T7)

Timestamp: 2026-07-19T16-40

## Post-condition attributes (grep across the 63 remediated files)
Command: `grep -rlE "NotNullWhen|MaybeNullWhen|NotNullIfNotNull|\[MaybeNull\]|\[AllowNull\]|\[DisallowNull\]|DoesNotReturn|MemberNotNull" UtilitiesCS/OutlookObjects/Folder UtilitiesCS/OutlookObjects/Store`
Result: no matches. No nullable post-condition attribute was added anywhere in the cluster.

## Polyfill declaration
Command: `grep -rn "namespace System.Diagnostics.CodeAnalysis" UtilitiesCS/OutlookObjects/Folder UtilitiesCS/OutlookObjects/Store`
Result: no matches. No `IsExternalInit` or post-condition-attribute polyfill was introduced.

## record / record struct / init accessors
Command: `grep -rnE "\brecord\s+(struct\s+)?[A-Z]|get; init;|\binit;" <cluster>`
Result: the only match is the PRE-EXISTING `public sealed record StoreRehookResult` in `StoreRehookResult.cs`
(one of the 18 already-`#nullable enable` verify-only files; a hand-written sealed record with constructor-set
get-only properties, net481-safe as-is per the plan's Scope Invariants). **No new `record`, `record struct`, or
`init` accessor was introduced by this feature** in any of the 63 remediated files (AC6). `FolderRow`,
`FolderScore`, `StoreIdentity` remain plain `readonly struct`; no `init` was added.
