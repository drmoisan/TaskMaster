---
name: deployment-premise-check-compiled-references
description: An AC that says a transitive assembly is "deployed beside" a host DLL can rest on a false premise; csc drops unused references, so MSBuild never copies that family. Check the referencing DLL's compiled AssemblyReferences before accepting or rewording such a criterion
metadata:
  type: project
---

On #973 (2026-10-06) AC17 claimed the Azure.Core 1.50.0.0 request from Microsoft.Kiota.Authentication.Azure "redirects to an assembly that exists in the add-in's output directory". The executor found Azure.Core.dll and the whole Graph/Kiota/Azure family absent from TaskMaster\bin\Debug and reported NOT MET.

The cause is not a deployment defect. UtilitiesCS installs those packages, but none of its code uses them, so csc emits no AssemblyReference for them into UtilitiesCS.dll. MSBuild's ResolveAssemblyReferences copies a ProjectReference's dependencies from the referenced DLL's metadata, so nothing in that family is ever copied beside TaskMaster.dll, and the add-in process never issues the request at all. The premise entered through research ("installed in UtilitiesCS and deployed beside TaskMaster.dll").

One pwsh command settles it in seconds (System.Reflection.Metadata, no load): open the DLL with `PEReader`, call `GetMetadataReader()`, and list `AssemblyReferences` names and versions.

**Why:** reading the criterion as a deployment bug would have widened scope into copy-local changes for unused assemblies; rewording it to drop the clause would have weakened it.

**How to apply:** treat it as a premise correction. The planner amends the criterion, keeping every true observation, and replaces the false conclusion with fail-capable observations: a reference scan over every dll/exe in the host output (expected 0 referrers), the identical scan over the producing project's output as a POSITIVE CONTROL (must find the referrer and print its version), and GetAssemblyName version reads of the deployed target where it does exist. The reviewer confirmed this as a correction, not a weakening. It cost one remediation cycle with two preflight rounds. Related: [[executor-blocked-ac-may-be-orchestrator-dischargeable]], [[my-own-negative-claims-need-a-scoped-search]].
