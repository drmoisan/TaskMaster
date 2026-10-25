"use strict";
(function () {
  var list = document.getElementById("list");
  var dropDownButton = document.getElementById("dropDownButton");
  var state = {
    rows: [],
    options: [],
    selectedRowIndex: -1,
    selectedSubfolderIndex: -1,
    selectedFolder: null,
    selectedDescendantIdentity: null,
    viewMode: "collapsed",
    isOpen: false,
    committedIdentity: null,
    pendingIdentity: null,
    pendingNodeIdentity: null
  };

  function post(msg) {
    if (window.chrome && window.chrome.webview) {
      window.chrome.webview.postMessage(msg);
    }
  }

  function selectedRow() {
    return state.rows.find(function (row) { return row.selected; }) || null;
  }

  function optionForRow(row) {
    return state.options[row.rowIndex] || null;
  }

  function selectableRowForIdentity(identity) {
    if (identity === null) {
      return null;
    }
    return state.rows.find(function (row) {
      return row.isSelectable && row.identity === identity;
    }) || null;
  }

  function applySelectorOptions() {
    state.rows.forEach(function (row) {
      var option = optionForRow(row);
      row.identity = option && typeof option.identity === "string" ? option.identity : null;
      row.isSelectable = option ? option.isSelectable === true : false;
    });
  }

  function visibleRows() {
    if (state.viewMode !== "collapsed") {
      return state.rows;
    }

    var selectedRows = state.rows.filter(function (row) { return row.selected; }).slice(0, 1);
    var committedRow = selectableRowForIdentity(state.committedIdentity);
    return committedRow === null ? selectedRows : [committedRow];
  }

  function selectedChildForRow(row) {
    if (row.selected && state.selectedDescendantIdentity !== null && typeof state.selectedFolder === "string") {
      return { identity: state.selectedDescendantIdentity };
    }
    var index = state.selectedSubfolderIndex;
    if (
      !row.selected || !Number.isInteger(state.selectedSubfolderIndex) || index < 0 ||
      !Array.isArray(row.subfolders) || index >= row.subfolders.length ||
      typeof state.selectedFolder !== "string"
    ) {
      return null;
    }
    var child = row.subfolders[index];
    return child && child.folderPath === state.selectedFolder ? child : null;
  }

  function lastSegmentIndex(row) {
    // The terminal segment is the row's own folder; clicking it still commits (#1007 D-Q27).
    var last = -1;
    row.cells.forEach(function (cell) {
      if (cell.kind === "segment") { last = cell.segmentIndex; }
    });
    return last;
  }

  function makeCell(row, cell) {
    var element;
    if (cell.kind === "segment") {
      element = document.createElement("span");
      element.className = cell.truncationEligible ? "seg trunc" : "seg";
      element.textContent = cell.text;
      element.title = cell.text;
      element.addEventListener("click", function (event) {
        // #1007 D-Q27: in the open list a non-leaf segment moves only the pending highlight.
        if (state.isOpen && state.viewMode === "expanded" && row.isSelectable && cell.segmentIndex !== lastSegmentIndex(row)) {
          event.stopPropagation();
          post({ type: "selectorHighlight", identity: row.identity });
        }
      });
      element.addEventListener("dblclick", function (event) {
        event.stopPropagation();
        post({ type: "selectorSegmentDoubleClick", rowIdentity: row.identity, segmentIndex: cell.segmentIndex });
      });
    } else if (cell.kind === "arrow") {
      element = document.createElement("span");
      element.className = "arrow";
      element.textContent = "→";
    } else {
      element = document.createElement("span");
      element.className = "aff";
      element.textContent = cell.kind === "plus" ? "+" : "−";
      element.addEventListener("click", function (event) {
        event.stopPropagation();
        post({ type: "affordanceToggle", rowIndex: row.rowIndex });
      });
    }
    return element;
  }

  function makeRow(row, activeRowIndex, selectedChildOwnsActive) {
    var expanded = state.viewMode === "expanded";
    var selectedChild = selectedChildForRow(row);
    var active =
      expanded && row.isSelectable && row.rowIndex === activeRowIndex && !selectedChildOwnsActive;
    var rowElement = document.createElement("div");
    rowElement.className = active
      ? "row active"
      : (!expanded && row.selected ? "row selected" : "row");

    if (expanded && row.isSelectable) {
      rowElement.id = "folder-option-" + row.rowIndex;
      rowElement.setAttribute("role", "option");
      rowElement.setAttribute("aria-selected", String(active));
      rowElement.addEventListener("click", function () {
        post({ type: "selectorActivate", identity: row.identity });
      });
    }

    var crumbs = document.createElement("div");
    crumbs.className = "crumbs";
    if (!expanded && selectedChild !== null) {
      var selectedPath = document.createElement("span");
      selectedPath.className = "seg";
      selectedPath.textContent = state.selectedFolder;
      selectedPath.title = state.selectedFolder;
      crumbs.appendChild(selectedPath);
    } else {
      row.cells.forEach(function (cell) { crumbs.appendChild(makeCell(row, cell)); });
    }
    rowElement.appendChild(crumbs);

    var pct = document.createElement("span");
    pct.className = "pct";
    pct.textContent = row.percentText;
    rowElement.appendChild(pct);
    return rowElement;
  }

  function makeSubfolders(row, children, selectedChildOwnsActive, depth) {
    depth = depth || 0;
    var subs = document.createElement("div");
    subs.className = "subs";
    children.forEach(function (child, index) {
      var subfolderElement = document.createElement("div");
      // #1007 AC1, AC4: a pending descendant named by the host owns the active option.
      var active = state.pendingNodeIdentity !== null
        ? row.identity === state.pendingIdentity && child.identity === state.pendingNodeIdentity
        : selectedChildOwnsActive && child.selected;
      subfolderElement.id = "folder-option-" + row.rowIndex + "-node-" + child.identity;
      subfolderElement.className = active ? (child.selected ? "sub selected active" : "sub active") : "sub";
      subfolderElement.setAttribute("role", "option");
      subfolderElement.setAttribute("aria-selected", String(active));
      subfolderElement.setAttribute("data-depth", String(depth));
      subfolderElement.setAttribute("style", "padding-left: " + (24 + 16 * depth) + "px");
      subfolderElement.tabIndex = 0;
      var name = document.createElement("span");
      name.className = "seg";
      name.textContent = child.displayName;
      name.title = child.folderPath;
      subfolderElement.appendChild(name);
      var loadedEmpty = child.childrenLoaded === true && Array.isArray(child.children) && child.children.length === 0;
      if (child.hasChildren) {
        var toggle = document.createElement("span");
        toggle.className = "aff";
        toggle.textContent = child.expanded ? "−" : "+";
        toggle.setAttribute("role", "button");
        toggle.setAttribute("aria-label", (child.expanded ? "Collapse " : "Expand ") + child.displayName);
        toggle.setAttribute("aria-expanded", String(child.expanded));
        toggle.tabIndex = 0;
        function toggleDescendant(event) {
          event.preventDefault();
          event.stopPropagation();
          post({ type: "selectorDescendantToggle", rowIdentity: row.identity, nodeIdentity: child.identity });
        }
        toggle.addEventListener("click", toggleDescendant);
        toggle.addEventListener("keydown", function (event) {
          if (event.key === "Enter" || event.key === " ") { toggleDescendant(event); }
        });
        // #1007 AC14: no affordance on a child whose completed fetch returned zero children.
        if (!loadedEmpty) {
          subfolderElement.appendChild(toggle);
        }
      }

      function activateSubfolder(event) {
        event.preventDefault();
        event.stopPropagation();
        post({
          type: "selectorDescendantActivate",
          rowIdentity: row.identity,
          nodeIdentity: child.identity
        });
      }

      subfolderElement.addEventListener("click", activateSubfolder);
      subfolderElement.addEventListener("keydown", function (event) {
        if (event.key === "Enter" || event.key === " ") {
          activateSubfolder(event);
        }
      });
      subs.appendChild(subfolderElement);
      if (child.expanded && Array.isArray(child.children)) {
        subs.appendChild(makeSubfolders(row, child.children, selectedChildOwnsActive, depth + 1));
      }
    });
    return subs;
  }

  function render() {
    var activeRow = null;
    applySelectorOptions();
    var pendingRow = selectableRowForIdentity(state.pendingIdentity);
    var activeRowIndex = pendingRow === null ? -1 : pendingRow.rowIndex;
    list.textContent = "";

    if (state.viewMode === "expanded") {
      list.setAttribute("role", "listbox");
    } else {
      list.removeAttribute("role");
    }
    list.removeAttribute("aria-activedescendant");

    visibleRows().forEach(function (row) {
      var pendingNodeOwnsActive = state.pendingNodeIdentity !== null && row.identity === state.pendingIdentity;
      var selectedChildOwnsActive = state.pendingNodeIdentity === null &&
        selectedChildForRow(row) !== null && row.identity === state.pendingIdentity && hasSelectedDescendant(row.subfolders);
      var rowElement = makeRow(row, activeRowIndex, selectedChildOwnsActive || pendingNodeOwnsActive);
      list.appendChild(rowElement);
      if (rowElement.classList.contains("active")) {
        activeRow = rowElement;
      }
      if (
        state.viewMode === "expanded" &&
        row.leafExpanded &&
        row.subfolders.length > 0
      ) {
        var subfoldersElement = makeSubfolders(row, row.subfolders, selectedChildOwnsActive);
        list.appendChild(subfoldersElement);
        var activeSubfolder = subfoldersElement.querySelector(".sub.active");
        if (activeSubfolder !== null) {
          activeRow = activeSubfolder;
        }
      }
    });

    if (state.viewMode === "expanded" && activeRow !== null) {
      var activeId = activeRow.id;
      list.setAttribute("aria-activedescendant", activeId);
      activeRow.scrollIntoView({ block: "nearest" });
    }
  }

  function onArrow(direction) {
    // #1007 S6: C# decides every Left and Right against the pending entry, so the page never
    // pre-filters by the committed row.
    post({ type: "arrowKey", direction: direction });
  }

  function hasSelectedDescendant(children) {
    return children.some(function (child) {
      return child.selected || (Array.isArray(child.children) && hasSelectedDescendant(child.children));
    });
  }

  function fullyVisibleCount(rects, top, bottom) {
    // #1007 S3, D-Q17: a page is the number of rows fully inside the list, at least one.
    var count = rects.filter(function (rect) { return rect.top >= top && rect.bottom <= bottom; }).length;
    return Math.max(1, count);
  }

  function visiblePageSize() {
    var bounds = list.getBoundingClientRect();
    var entries = Array.prototype.slice.call(list.querySelectorAll(".row"))
      .concat(Array.prototype.slice.call(list.querySelectorAll(".sub")));
    var rects = entries.map(function (entry) { return entry.getBoundingClientRect(); });
    return fullyVisibleCount(rects, bounds.top, bounds.bottom);
  }

  var selectorKeys = {
    ArrowUp: "up",
    ArrowDown: "down",
    Enter: "enter",
    Escape: "escape",
    Home: "home",
    End: "end",
    PageUp: "pageup",
    PageDown: "pagedown"
  };

  function handleKeyDown(event) {
    if (event.key === "ArrowLeft" && event.ctrlKey) {
      event.preventDefault();
      post({ type: "selectorKey", key: "parent" });
      return;
    }
    if (Object.prototype.hasOwnProperty.call(selectorKeys, event.key)) {
      event.preventDefault();
      var message = { type: "selectorKey", key: selectorKeys[event.key] };
      if (event.key === "PageUp" || event.key === "PageDown") {
        message.pageSize = visiblePageSize();
      }
      post(message);
      return;
    }

    if (event.key === "ArrowRight") {
      event.preventDefault();
      onArrow("right");
    } else if (event.key === "ArrowLeft") {
      event.preventDefault();
      onArrow("left");
    }
  }

  function focusForMode() {
    if (state.viewMode === "expanded") {
      list.focus();
    } else {
      dropDownButton.focus();
    }
  }

  list.addEventListener("keydown", handleKeyDown);
  dropDownButton.addEventListener("keydown", handleKeyDown);
  dropDownButton.addEventListener("click", function () {
    post({ type: "selectorToggle" });
  });
  window.addEventListener("focus", focusForMode);

  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener("message", function (event) {
      var msg = event.data;
      if (!msg || typeof msg.type !== "string") {
        return;
      }

      if (msg.type === "render") {
        state.rows = Array.isArray(msg.rows) ? msg.rows : [];
        state.selectedSubfolderIndex = Number.isInteger(msg.selectedSubfolderIndex)
          ? msg.selectedSubfolderIndex
          : -1;
        state.selectedFolder =
          typeof msg.selectedFolder === "string" ? msg.selectedFolder : null;
        state.selectedDescendantIdentity = typeof msg.selectedDescendantIdentity === "string" ? msg.selectedDescendantIdentity : null;
        var selected = selectedRow();
        if (selected === null || selectedChildForRow(selected) === null) {
          state.selectedSubfolderIndex = -1;
          state.selectedFolder = null;
        }
        state.selectedRowIndex = selected ? selected.rowIndex : -1;
        render();
      } else if (msg.type === "selectorView") {
        state.viewMode = msg.mode === "expanded" ? "expanded" : "collapsed";
        state.isOpen = msg.isOpen === true;
        state.committedIdentity = msg.committedIdentity || null;
        state.pendingIdentity = msg.pendingIdentity || null;
        state.pendingNodeIdentity = typeof msg.pendingNodeIdentity === "string" ? msg.pendingNodeIdentity : null;
        state.options = Array.isArray(msg.options) ? msg.options : [];
        document.documentElement.setAttribute("data-mode", state.viewMode);
        document.documentElement.setAttribute("data-view-mode", state.viewMode);
        dropDownButton.setAttribute("aria-expanded", String(state.isOpen));
        render();
      } else if (msg.type === "themeChange") {
        document.documentElement.setAttribute(
          "data-theme",
          msg.theme === "dark" ? "dark" : "light"
        );
      } else if (msg.type === "selectionChange") {
        state.selectedSubfolderIndex =
          typeof msg.subfolderIndex === "number" ? msg.subfolderIndex : -1;
      }
    });
  }
})();
