---
name: project-generic-seam-over-embedded-interop-type-cs1769
description: A test seam typed Func<Attachment,...> (Outlook interop embedded in UtilitiesCS) compiles in production but every test call site fails CS1769; preflight cannot see it
metadata:
  type: project
---

UtilitiesCS references Microsoft.Office.Interop.Outlook with `EmbedInteropTypes=True` (UtilitiesCS.csproj ~line 223). Any internal seam whose signature uses a generic type over an interop type (e.g. `Func<Attachment, string, Task<bool>>`) compiles in UtilitiesCS but fails CS1769 at every call site in UtilitiesCS.Test ("cannot be used across assembly boundaries because it has a generic type argument that is an embedded interop type"). Observed on #959 P4-T7 (2026-10-03): 16 errors, plan stopped for a planner amendment.

**Why:** embedded (NoPIA) types are per-assembly copies; a constructed generic over one cannot be unified across assemblies.

**How to apply:** in preflight, flag any seam listing whose public/internal signature has a generic argument that is an Outlook/Office interop type in an embedding project; the fix is a non-generic delegate type declared in the production assembly (or object/IAttachment wrapper). Related: [[project-preflight-csc-probe-for-mandated-csharp-shapes]] — a csc probe must compile the call from a second assembly to catch this.
