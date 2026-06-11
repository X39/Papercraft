(function () {
  function activatePreviewTab(preview, target) {
    var tabs = preview.querySelectorAll("[data-preview-tab]");
    var panels = preview.querySelectorAll("[data-preview-panel]");

    tabs.forEach(function (tab) {
      tab.setAttribute("aria-selected", tab.getAttribute("data-preview-tab") === target ? "true" : "false");
    });

    panels.forEach(function (panel) {
      var isActive = panel.getAttribute("data-preview-panel") === target;
      panel.hidden = !isActive;
      if (!isActive) {
        return;
      }

      var frame = panel.querySelector("iframe[data-src]");
      if (frame && !frame.getAttribute("src")) {
        frame.setAttribute("src", frame.getAttribute("data-src"));
      }
    });
  }

  function moveTabFocus(preview, currentTab, direction) {
    var tabs = Array.prototype.slice.call(preview.querySelectorAll("[data-preview-tab]"));
    var index = tabs.indexOf(currentTab);
    if (index < 0) {
      return;
    }

    var nextIndex = (index + direction + tabs.length) % tabs.length;
    tabs[nextIndex].focus();
    activatePreviewTab(preview, tabs[nextIndex].getAttribute("data-preview-tab"));
  }

  document.querySelectorAll("[data-sample-preview]").forEach(function (preview) {
    preview.addEventListener("click", function (event) {
      var tab = event.target.closest("[data-preview-tab]");
      if (!tab || !preview.contains(tab)) {
        return;
      }

      activatePreviewTab(preview, tab.getAttribute("data-preview-tab"));
    });

    preview.addEventListener("keydown", function (event) {
      var tab = event.target.closest("[data-preview-tab]");
      if (!tab || !preview.contains(tab)) {
        return;
      }

      if (event.key === "ArrowRight") {
        event.preventDefault();
        moveTabFocus(preview, tab, 1);
      } else if (event.key === "ArrowLeft") {
        event.preventDefault();
        moveTabFocus(preview, tab, -1);
      } else if (event.key === "Home") {
        event.preventDefault();
        var firstTab = preview.querySelector("[data-preview-tab]");
        if (firstTab) {
          firstTab.focus();
          activatePreviewTab(preview, firstTab.getAttribute("data-preview-tab"));
        }
      } else if (event.key === "End") {
        event.preventDefault();
        var tabs = preview.querySelectorAll("[data-preview-tab]");
        var lastTab = tabs[tabs.length - 1];
        if (lastTab) {
          lastTab.focus();
          activatePreviewTab(preview, lastTab.getAttribute("data-preview-tab"));
        }
      }
    });
  });
})();
