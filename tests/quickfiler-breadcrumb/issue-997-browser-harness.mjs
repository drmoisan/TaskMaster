import { runInNewContext } from "node:vm";
import { pathToFileURL } from "node:url";

class Element {
  constructor(tag, document) {
    this.tagName = tag;
    this.document = document;
    this.children = [];
    this.attributes = new Map();
    this.listeners = new Map();
    this.className = "";
    this.parentElement = null;
    this.focusCount = 0;
    this.scrolls = [];
    // Issue 1007: layout stub for page-size measurement; a test assigns `rect` per element.
    this.rect = { top: 0, bottom: 0 };
    this._text = "";
    this.classList = { contains: (name) => this.className.split(/\s+/).includes(name) };
  }

  set textContent(value) {
    for (const child of this.children) child.parentElement = null;
    this.children = [];
    this._text = String(value);
  }
  get textContent() {
    return this._text + this.children.map((child) => child.textContent).join("");
  }
  appendChild(child) {
    child.parentElement = this;
    this.children.push(child);
    return child;
  }
  setAttribute(name, value) { this.attributes.set(name, String(value)); }
  getAttribute(name) { return this.attributes.get(name) ?? null; }
  removeAttribute(name) { this.attributes.delete(name); }
  addEventListener(type, listener) {
    const listeners = this.listeners.get(type) || [];
    listeners.push(listener);
    this.listeners.set(type, listeners);
  }
  querySelectorAll(selector) {
    const classes = selector.split(".").slice(1);
    const matches = (element) => selector.startsWith(".")
      ? classes.every((name) => element.classList.contains(name))
      : element.tagName === selector;
    const found = [];
    const walk = (element) => {
      for (const child of element.children) {
        if (matches(child)) found.push(child);
        walk(child);
      }
    };
    walk(this);
    return found;
  }
  querySelector(selector) { return this.querySelectorAll(selector)[0] || null; }
  focus() { this.focusCount++; this.document.activeElement = this; }
  scrollIntoView(options) { this.scrolls.push(options); }
  getBoundingClientRect() { return { top: this.rect.top, bottom: this.rect.bottom }; }
}

export function createBrowser(source, options = {}) {
  const document = { activeElement: null };
  const root = new Element("html", document);
  const selector = root.appendChild(new Element("div", document));
  const list = selector.appendChild(new Element("div", document));
  const dropdown = selector.appendChild(new Element("button", document));
  const elements = new Map([["list", list], ["dropDownButton", dropdown]]);
  document.documentElement = root;
  document.getElementById = (id) => elements.get(id) || null;
  document.createElement = (tag) => new Element(tag, document);
  const messages = [];
  const nativeListeners = [];
  const windowListeners = new Map();
  const window = {
    addEventListener(type, listener) {
      const listeners = windowListeners.get(type) || [];
      listeners.push(listener);
      windowListeners.set(type, listeners);
    }
  };
  if (options.chrome !== false) {
    window.chrome = options.webview === false ? {} : {
      webview: {
        postMessage(message) { messages.push(JSON.parse(JSON.stringify(message))); },
        addEventListener(type, listener) {
          if (type === "message") nativeListeners.push(listener);
        }
      }
    };
  }
  runInNewContext(source.text, { document, window }, { filename: pathToFileURL(source.filename).href });

  function dispatch(element, type, values = {}) {
    const event = {
      ...values, type, target: element, defaultPrevented: false, propagationStopped: false,
      preventDefault() { this.defaultPrevented = true; },
      stopPropagation() { this.propagationStopped = true; }
    };
    for (let current = element; current !== null; current = current.parentElement) {
      event.currentTarget = current;
      for (const listener of current.listeners.get(type) || []) listener(event);
      if (event.propagationStopped) break;
    }
    return event;
  }
  return {
    root, list, dropdown, document, messages, dispatch,
    receive(message) { for (const listener of nativeListeners) listener({ data: message }); },
    windowFocus() { for (const listener of windowListeners.get("focus") || []) listener({ type: "focus" }); },
    clearMessages() { messages.length = 0; }
  };
}

export function suggestionRow(overrides = {}) {
  return {
    rowIndex: 0, isSuggestion: true, selected: true, collapsed: false,
    leafExpanded: false, percentText: "73%", subfolders: [],
    cells: [{ kind: "segment", text: "Apollo", segmentIndex: 0, truncationEligible: false }],
    ...overrides
  };
}

export function selectorView(overrides = {}) {
  return {
    type: "selectorView", mode: "expanded", isOpen: true,
    committedIdentity: "row-a", pendingIdentity: "row-a",
    options: [{ identity: "row-a", isSelectable: true }], ...overrides
  };
}
