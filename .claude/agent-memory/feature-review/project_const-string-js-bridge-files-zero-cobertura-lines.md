---
name: const-string-js-bridge-files-zero-cobertura-lines
description: a C# file that is only public const string fields (e.g. embedded JS/HTML bridge assets) emits no <class> element in Cobertura at all -- absence from the coverage XML is the correct, expected signal, not a coverage gap
metadata:
  type: project
---

At #737 (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`, a `public static class` holding only `public const string` fields built from string-literal `+` concatenation), the file was completely absent from the Cobertura XML's class list -- no `<class>` element, confirmed by grepping the full coverage XML for the class name (zero matches) and for the file's containing-folder prefix (20+ sibling `.cs` files present, this one absent).

**Why:** the C# compiler folds `const string` initializers into constant metadata at compile time with no emitted IL, so there is no coverable line for the instrumentor to see. `lines-valid` staying bit-for-bit identical before/after the edit (64578 -> 64578 in a same-session baseline/final pair) independently corroborates this rather than just trusting the claim.

**How to apply:** when auditing a coverage delta for a Write Set file that is a pure `const`-only asset class (common for embedded JS/HTML/CSS bridge strings in this repo's WebView2 breadcrumb code), do not expect or require a `<class>` entry for it, and do not treat its absence from the coverage XML as a FAIL. Verify the "zero coverable lines" claim by (a) reading the file to confirm it truly has no non-const executable members, and (b) grepping the Cobertura XML for the class/file name to confirm the absence, and (c) checking `lines-valid` is unchanged across the same-session baseline/final pair.
