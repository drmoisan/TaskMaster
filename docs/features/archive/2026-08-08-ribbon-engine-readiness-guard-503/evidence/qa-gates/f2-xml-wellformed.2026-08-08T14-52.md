# F2 — Ribbon XML Well-Formedness and Namespace (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P2-T3]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; [xml]$d = Get-Content 'TaskMaster\Ribbon\RibbonExplorer.xml' -Raw; $d.DocumentElement.LocalName; $d.DocumentElement.NamespaceURI"`
EXIT_CODE: 0

## Output Summary

```text
customUI
http://schemas.microsoft.com/office/2009/07/customui
```

| Property | Value | Required |
|---|---|---|
| Parse result | **no error** — the `[xml]` cast succeeded, so the document is well-formed | no error |
| Root local name | **`customUI`** | `customUI` |
| Root namespace URI | **`http://schemas.microsoft.com/office/2009/07/customui`** | `http://schemas.microsoft.com/office/2009/07/customui` |

The `[xml]` type accelerator invokes `XmlDocument.LoadXml`, which throws `XmlException` on malformed content. It did not throw, so the collapse of the three `<button>` elements introduced no unbalanced tag, unescaped character, or malformed attribute.

The namespace is the 2009 (`customUI14`) namespace, which is what the ribbon-XML tests assert against namespace-aware. It is unchanged by the F2 edit, which touched only the internal layout of three self-closing `<button>` elements and no namespace declaration.

Binary outcome satisfied: the document parses without error, the root local name is `customUI`, and the namespace URI is `http://schemas.microsoft.com/office/2009/07/customui`.
