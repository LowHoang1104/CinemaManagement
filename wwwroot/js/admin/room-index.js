/**
 * Room Management - Index Page Logic
 * Handles AJAX filtering, pagination, status toggling, and edit modal.
 */

const editModal = document.getElementById("EditModal");
const editModalContent = document.getElementById("EditModalContent");
const editModalBody = document.getElementById("EditModalBody");

/**
 * Toggles the visibility of the Edit Room Modal with animations
 * @param {boolean} show - Whether to show or hide the modal
 */
function toggleEditModal(show) {
  if (show) {
    editModal.classList.remove("hidden", "pointer-events-none");
    setTimeout(() => {
      editModal.classList.remove("opacity-0");
      editModalContent.classList.remove("translate-y-0");
      editModalContent.classList.add("translate-y-[2rem]");
    }, 10);
    document.body.style.overflow = "hidden";
  } else {
    editModal.classList.add("opacity-0");
    editModalContent.classList.remove("translate-y-[2rem]");
    editModalContent.classList.add("translate-y-0");
    setTimeout(() => {
      editModal.classList.add("hidden", "pointer-events-none");
      // Reset modal body to loading state
      editModalBody.innerHTML = `
                <div class="flex items-center justify-center py-20">
                    <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-900"></div>
                </div>
            `;
    }, 300);
    document.body.style.overflow = "";
  }
}

/**
 * Opens the Edit Modal and loads room data via AJAX
 * @param {string|number} id - Room ID
 */
async function openEditModal(id) {
  closeActionMenus();
  toggleEditModal(true);
  try {
    const response = await fetch(`/Rooms/Edit/${id}`, {
      headers: { "X-Requested-With": "XMLHttpRequest" },
    });
    if (response.ok) {
      const html = await response.text();
      editModalBody.innerHTML = html;

      // Execute any scripts within the loaded HTML
      const scripts = editModalBody.querySelectorAll("script");
      scripts.forEach((oldScript) => {
        const newScript = document.createElement("script");
        Array.from(oldScript.attributes).forEach((attr) =>
          newScript.setAttribute(attr.name, attr.value),
        );
        newScript.textContent = oldScript.textContent;
        oldScript.parentNode.replaceChild(newScript, oldScript);
      });
    } else {
      editModalBody.innerHTML =
        '<div class="p-8 text-center text-red-500 font-bold">Lỗi không thể tải dữ liệu.</div>';
    }
  } catch (e) {
    console.error("OpenEditModal error:", e);
    editModalBody.innerHTML =
      '<div class="p-8 text-center text-red-500 font-bold">Lỗi hệ thống.</div>';
  }
}

// ─── Filter Dropdown Logic ───

let openFilterPanelId = null;

function toggleFilterDropdown(panelId, evt) {
  if (evt) evt.stopPropagation();
  const panel = document.getElementById(panelId);
  const isHidden = panel.classList.contains("hidden");
  closeFilterPanels();
  if (isHidden) {
    panel.classList.remove("hidden");
    openFilterPanelId = panelId;
  }
}

function selectFilterOpt(
  panelId,
  inputId,
  labelId,
  value,
  label,
  hasValue,
  btnElement,
) {
  document.getElementById(inputId).value = value;
  const labelEl = document.getElementById(labelId);
  labelEl.textContent = label;

  // Style the label based on whether a value is selected
  if (hasValue) {
    labelEl.classList.remove("text-slate-500");
    labelEl.classList.add("font-medium", "text-slate-900");
  } else {
    labelEl.classList.add("text-slate-500");
    labelEl.classList.remove("font-medium", "text-slate-900");
  }

  // Update visual ticks in the dropdown
  if (btnElement) {
    const panel = document.getElementById(panelId);
    panel
      .querySelectorAll(".filter-tick")
      .forEach((el) => el.classList.add("hidden"));
    panel.querySelectorAll(".filter-text").forEach((el) => {
      el.classList.remove("font-semibold", "text-indigo-700");
      el.classList.add("text-slate-600");
    });

    const activeTick = btnElement.querySelector(".filter-tick");
    if (activeTick) activeTick.classList.remove("hidden");

    const activeText = btnElement.querySelector(".filter-text");
    if (activeText) {
      activeText.classList.remove("text-slate-600");
      activeText.classList.add("font-semibold", "text-indigo-700");
    }
  }
  closeFilterPanels();
}

function closeFilterPanels() {
  if (openFilterPanelId) {
    const panel = document.getElementById(openFilterPanelId);
    if (panel) panel.classList.add("hidden");
    openFilterPanelId = null;
  }
}

// ─── Action Menu Logic ───

let openActionMenuId = null;

function toggleActionMenu(id, evt) {
  if (evt) evt.stopPropagation();
  const menu = document.getElementById(id);
  const isHidden = menu.classList.contains("invisible");
  closeActionMenus();
  if (isHidden) {
    menu.classList.remove("invisible", "opacity-0", "scale-95");
    openActionMenuId = id;
  }
}

function closeActionMenus() {
  if (openActionMenuId) {
    const m = document.getElementById(openActionMenuId);
    if (m) m.classList.add("invisible", "opacity-0", "scale-95");
    openActionMenuId = null;
  }
}

// Global click listener to close panels/menus
document.addEventListener("click", function (e) {
  if (!e.target.closest("[data-filter-dropdown]")) closeFilterPanels();
  if (!e.target.closest("[data-action-root]")) closeActionMenus();
});

function navigateTo(url) {
  window.location.href = url;
}

// ─── Status Toggling Logic ───

let currentRoomId = null;
let currentRoomName = null;
let currentTargetStatus = null;

/**
 * Opens the Status Change Confirmation Modal
 */
function openStatusModal(type, roomId, roomName) {
  currentRoomId = roomId;
  currentRoomName = roomName;
  currentTargetStatus = type === "activate" ? 1 : 0;

  const modalId = "modal-" + type;
  const nameSpanId = type + "-room-name";
  const span = document.getElementById(nameSpanId);
  if (span) span.textContent = roomName;

  const modal = document.getElementById(modalId);
  if (modal) modal.classList.remove("hidden");
  document.body.style.overflow = "hidden";
  closeActionMenus();
}

function closeStatusModal(type) {
  const modalId = "modal-" + type;
  const modal = document.getElementById(modalId);
  if (modal) modal.classList.add("hidden");
  document.body.style.overflow = "";
}

/**
 * Submits the status change via AJAX
 */
async function submitStatusChange() {
  if (!currentRoomId || currentTargetStatus === null) return;

  const type = currentTargetStatus === 1 ? "activate" : "deactivate";
  const modal = document.getElementById("modal-" + type);
  const btn = modal.querySelector('button[onclick="submitStatusChange()"]');
  const originalText = btn.textContent;

  btn.disabled = true;
  btn.innerHTML =
    '<div class="animate-spin rounded-full h-4 w-4 border-2 border-white/60 border-t-white mx-auto"></div>';

  try {
    const tokenInput = document.querySelector(
      'input[name="__RequestVerificationToken"]',
    );
    if (!tokenInput) throw new Error("Verification token not found");

    const token = tokenInput.value;
    const formData = new FormData();
    formData.append("id", currentRoomId);
    formData.append("status", currentTargetStatus);
    formData.append("__RequestVerificationToken", token);

    const response = await fetch("/Rooms/ToggleStatus", {
      method: "POST",
      body: formData,
      headers: { "X-Requested-With": "XMLHttpRequest" },
    });

    const data = await response.json();
    if (data.success) {
      closeStatusModal(type);
      showToast("Thành công", data.message, "success");
      updateUIAfterStatusChange(currentRoomId, data.newStatus);
    } else {
      showToast("Lỗi", data.message, "error");
    }
  } catch (error) {
    console.error("ToggleStatus error:", error);
    showToast("Lỗi", "Lỗi kết nối khi thay đổi trạng thái.", "error");
  } finally {
    btn.disabled = false;
    btn.textContent = originalText;
  }
}

/**
 * Updates the UI elements (badges/buttons) without refreshing the page
 */
function updateUIAfterStatusChange(roomId, newStatus) {
  const isActive =
    newStatus === 1 || newStatus === "Active" || newStatus === true;

  // Update Badge
  const badge = document.getElementById(`status-badge-${roomId}`);
  if (badge) {
    badge.textContent = isActive ? "Hoạt động" : "Ngừng hoạt động";
    badge.className = `inline-flex items-center rounded-full border px-3 py-1 text-xs font-semibold ${isActive ? "bg-emerald-50 text-emerald-700 border-emerald-200" : "bg-slate-100 text-slate-600 border-slate-300"}`;
  }

  // Update Toggle Button in Action Menu
  const container = document.getElementById(
    `status-toggle-container-${roomId}`,
  );
  if (container) {
    if (isActive) {
      container.innerHTML = `
                <button type="button" class="flex w-full items-center gap-3 px-3 py-2 text-left text-rose-600 hover:bg-rose-50 transition-colors group" onclick="openStatusModal('deactivate', '${roomId}', '${currentRoomName}')">
                    <span class="flex h-7 w-7 items-center justify-center rounded-full bg-rose-50 text-rose-500 group-hover:bg-rose-100 transition-colors">
                        <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                            <rect x="6" y="4" width="4" height="16"></rect><rect x="14" y="4" width="4" height="16"></rect>
                        </svg>
                    </span>
                    <span class="font-medium">Ngừng hoạt động</span>
                </button>
            `;
    } else {
      container.innerHTML = `
                <button type="button" class="flex w-full items-center gap-3 px-3 py-2 text-left text-emerald-600 hover:bg-emerald-50 transition-colors group" onclick="openStatusModal('activate', '${roomId}', '${currentRoomName}')">
                    <span class="flex h-7 w-7 items-center justify-center rounded-full bg-emerald-50 text-emerald-500 group-hover:bg-emerald-100 transition-colors">
                        <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                            <polygon points="5 3 19 12 5 21 5 3"></polygon>
                        </svg>
                    </span>
                    <span class="font-medium">Kích hoạt</span>
                </button>
            `;
    }
  }
}

// ─── AJAX Navigation & Filtering ───

document.addEventListener("DOMContentLoaded", () => {
  const container = document.getElementById("room-list-container");
  const form = document.getElementById("roomFilterForm");

  /**
   * Fetches and updates the room list via AJAX
   */
  function fetchRooms(url) {
    if (!container) return;

    container.style.opacity = "0.5";
    container.style.pointerEvents = "none";

    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then((res) => res.text())
      .then((html) => {
        const tempDiv = document.createElement("div");
        tempDiv.innerHTML = html;

        // Update table/grid content
        const listContent = tempDiv.querySelector("#room-list-ajax-content");
        if (listContent) {
          container.innerHTML = listContent.innerHTML;
        } else {
          container.innerHTML = html;
        }

        // Teleport UI updates (chips, clear button)
        ["ajax-clear-filter-wrapper", "ajax-chips-wrapper"].forEach((id) => {
          const source = tempDiv.querySelector(`#teleport-${id}`);
          const target = document.getElementById(id);
          if (source && target) target.innerHTML = source.innerHTML;
        });

        window.history.pushState(null, "", url);
      })
      .catch((err) => {
        console.error("Error fetching rooms via AJAX:", err);
        showToast("Lỗi", "Không thể tải dữ liệu phòng.", "error");
      })
      .finally(() => {
        container.style.opacity = "1";
        container.style.pointerEvents = "auto";
        // Reset any open panels/menus
        closeFilterPanels();
        closeActionMenus();
      });
  }

  // Handle filter form submission
  if (form) {
    form.addEventListener("submit", function (e) {
      e.preventDefault();
      const formData = new FormData(form);
      const params = new URLSearchParams();
      for (const [key, value] of formData.entries()) {
        if (value) params.append(key, value);
      }
      fetchRooms(`${form.action}?${params.toString()}`);
    });
  }

  // Handle AJAX pagination links
  if (container) {
    container.addEventListener("click", function (e) {
      const link = e.target.closest("a");
      if (link && link.href && link.pathname.includes("/Rooms")) {
        const url = new URL(link.href);
        const action = url.pathname.split("/").pop();

        // Exclude CRUD actions that should navigate normally
        const excluded = [
          "Edit",
          "Details",
          "Delete",
          "ToggleStatus",
          "Create",
        ];
        if (!excluded.includes(action) && !link.hasAttribute("asp-action")) {
          e.preventDefault();
          fetchRooms(link.href);
        }
      }
    });
  }

  // Handle browser back/forward buttons
  window.addEventListener("popstate", () => {
    fetchRooms(window.location.href);
  });
});
