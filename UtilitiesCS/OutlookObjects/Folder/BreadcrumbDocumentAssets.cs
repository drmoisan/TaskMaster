#nullable enable
namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Inline CSS and JS string constants for the generated breadcrumb document (#349).
    /// The percent CSS class is a fixed non-shrinking flex item (research §D.4); only the crumb
    /// class may truncate. The bridge JS posts inbound messages via
    /// <c>window.chrome.webview.postMessage</c> and applies <c>render</c>/<c>subfolderResult</c>
    /// updates from <c>window.chrome.webview.addEventListener('message', ...)</c>.
    /// </summary>
    public static class BreadcrumbDocumentAssets
    {
        /// <summary>
        /// Layout CSS: the §D.4 flex row (fixed trailing percent, truncating crumb) plus
        /// affordance, banner, selection, and children-list styling.
        /// </summary>
        public const string BaseCss =
            ".rows { margin: 0; padding: 0; font-family: 'Segoe UI', sans-serif; font-size: 13px; }\n"
            + ".row { display: flex; align-items: center; }\n"
            + ".crumb { flex: 1 1 auto; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }\n"
            + ".pct { flex: 0 0 auto; margin-left: auto; white-space: nowrap; }\n"
            + ".rowwrap { padding: 2px 6px; }\n"
            + ".row.selectable { cursor: default; }\n"
            + ".seg { cursor: default; }\n"
            + ".sep { opacity: 0.6; }\n"
            + ".affordance { cursor: pointer; font-weight: bold; padding: 0 4px; }\n"
            + ".row.banner { justify-content: center; opacity: 0.7; pointer-events: none; }\n"
            + ".children { margin-left: 18px; }\n"
            + ".child { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }\n";

        /// <summary>Light-theme CSS block.</summary>
        public const string LightThemeCss =
            "body { background: #ffffff; color: #1b1b1b; }\n"
            + ".rowwrap.selected .row { background: #cce8ff; }\n"
            + ".affordance { color: #005a9e; }\n";

        /// <summary>Dark-theme CSS block.</summary>
        public const string DarkThemeCss =
            "body { background: #1e1e1e; color: #e6e6e6; }\n"
            + ".rowwrap.selected .row { background: #264f78; }\n"
            + ".affordance { color: #6cb6ff; }\n";

        /// <summary>
        /// Bridge JS: emitters for segment double-click, leaf affordance activation,
        /// left/right/up/down arrow keys, and row selection; plus the inbound listener applying
        /// <c>render</c> and <c>subfolderResult</c> updates. Child names are inserted via
        /// <c>textContent</c> (never markup) so provider data cannot inject HTML.
        /// </summary>
        public const string BridgeJs =
            "(function () {\n"
            + "  'use strict';\n"
            + "  function post(msg) {\n"
            + "    if (window.chrome && window.chrome.webview) { window.chrome.webview.postMessage(msg); }\n"
            + "  }\n"
            + "  function rowIdOf(el) {\n"
            + "    var wrap = el.closest('[data-row-id]');\n"
            + "    return wrap ? wrap.getAttribute('data-row-id') : null;\n"
            + "  }\n"
            + "  document.addEventListener('dblclick', function (e) {\n"
            + "    var seg = e.target.closest('.seg');\n"
            + "    if (!seg) { return; }\n"
            + "    var id = rowIdOf(seg);\n"
            + "    var idx = seg.getAttribute('data-segment-index');\n"
            + "    if (id !== null && idx !== null) {\n"
            + "      post({ type: 'segmentDoubleClick', rowId: id, segmentIndex: parseInt(idx, 10) });\n"
            + "    }\n"
            + "  });\n"
            + "  document.addEventListener('click', function (e) {\n"
            + "    var aff = e.target.closest('.affordance');\n"
            + "    if (aff) {\n"
            + "      var affId = rowIdOf(aff);\n"
            + "      if (affId !== null) { post({ type: 'leafExpandToggle', rowId: affId }); }\n"
            + "      return;\n"
            + "    }\n"
            + "    var row = e.target.closest('.row.selectable');\n"
            + "    if (row) {\n"
            + "      var rowId = rowIdOf(row);\n"
            + "      if (rowId !== null) { post({ type: 'rowSelected', rowId: rowId }); }\n"
            + "    }\n"
            + "  });\n"
            + "  document.addEventListener('keydown', function (e) {\n"
            + "    var map = { ArrowLeft: 'Left', ArrowRight: 'Right', ArrowUp: 'Up', ArrowDown: 'Down' };\n"
            + "    var key = map[e.key];\n"
            + "    if (!key) { return; }\n"
            + "    var selected = document.querySelector('.rowwrap.selected');\n"
            + "    var id = selected ? selected.getAttribute('data-row-id') : '';\n"
            + "    post({ type: 'arrowKey', rowId: id || '', key: key });\n"
            + "    e.preventDefault();\n"
            + "  });\n"
            + "  if (window.chrome && window.chrome.webview) {\n"
            + "    window.chrome.webview.addEventListener('message', function (e) {\n"
            + "      var msg = e.data;\n"
            + "      if (!msg || !msg.type) { return; }\n"
            + "      if (msg.type === 'render') {\n"
            + "        if (msg.rowId) {\n"
            + "          var target = document.querySelector('[data-row-id=\"' + msg.rowId + '\"]');\n"
            + "          if (target) { target.outerHTML = msg.html; }\n"
            + "        } else {\n"
            + "          var list = document.getElementById('rows');\n"
            + "          if (list) { list.innerHTML = msg.html; }\n"
            + "        }\n"
            + "      } else if (msg.type === 'subfolderResult') {\n"
            + "        var host = document.querySelector('[data-row-id=\"' + msg.rowId + '\"] .children');\n"
            + "        if (!host) { return; }\n"
            + "        while (host.firstChild) { host.removeChild(host.firstChild); }\n"
            + "        (msg.children || []).forEach(function (c) {\n"
            + "          var div = document.createElement('div');\n"
            + "          div.className = 'child';\n"
            + "          div.textContent = c.displayName;\n"
            + "          div.title = c.fullPath;\n"
            + "          host.appendChild(div);\n"
            + "        });\n"
            + "      }\n"
            + "    });\n"
            + "  }\n"
            + "})();\n";
    }
}
