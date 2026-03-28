// ══════════════════════════════════════════════════════════════
// GLOBAL UTILITIES & STATE
// ══════════════════════════════════════════════════════════════
let openCinemaFilterPanelId = null;
let openCinemaActionMenuId = null;
let currentCinemaId = null;

function debounce(func, timeout = 500) {
  let timer;
  return (...args) => {
    clearTimeout(timer);
    timer = setTimeout(() => {
      func.apply(this, args);
    }, timeout);
  };
}

function resetInputState(input, feedback, btn, tick) {
  input.classList.remove(
    "!border-red-500",
    "!ring-red-100",
    "!border-emerald-500",
    "!ring-emerald-100",
  );
  feedback.classList.add("hidden");
  if (tick) tick.classList.add("hidden");
  btn.disabled = false;
}

function setInputError(input, feedback, btn, tick, message) {
  input.classList.add("!border-red-500", "!ring-red-100");
  input.classList.remove("!border-emerald-500", "!ring-emerald-100");
  if (tick) tick.classList.add("hidden");
  feedback.textContent = message;
  feedback.classList.remove("hidden", "text-emerald-500");
  feedback.classList.add("text-red-500");
  btn.disabled = true;
}

function setInputSuccess(input, feedback, btn, tick) {
  input.classList.add("!border-emerald-500", "!ring-emerald-100");
  input.classList.remove("!border-red-500", "!ring-red-100");
  feedback.classList.add("hidden");
  if (tick) tick.classList.remove("hidden");
  btn.disabled = false;
}

// ══════════════════════════════════════════════════════════════
// AJAX FILTERING & PAGINATION (Global Scope)
// ══════════════════════════════════════════════════════════════
function cinema_fetchCinemas(url) {
  const gridContainer = document.getElementById("cinema-grid-container");
  if (!gridContainer) return;

  // Show loading state
  gridContainer.style.opacity = "0.5";
  gridContainer.style.pointerEvents = "none";

  fetch(url, {
    headers: { "X-Requested-With": "XMLHttpRequest" },
  })
    .then((res) => res.text())
    .then((html) => {
      const tempDiv = document.createElement("div");
      tempDiv.innerHTML = html;

      // 1. Update Grid Content
      const gridContent = tempDiv.querySelector("#cinema-grid-ajax-content");
      if (gridContent) {
        gridContainer.innerHTML = gridContent.innerHTML;
      }

      // 2. Teleport UI elements
      ["ajax-clear-filter-wrapper", "ajax-chips-wrapper"].forEach((id) => {
        const source = tempDiv.querySelector(`#teleport-${id}`);
        const target = document.getElementById(id);
        if (source && target) target.innerHTML = source.innerHTML;
      });

      // 3. Update URL
      window.history.pushState(null, "", url);
    })
    .catch((err) => console.error("Error fetching cinemas:", err))
    .finally(() => {
      gridContainer.style.opacity = "1";
      gridContainer.style.pointerEvents = "auto";
    });
}

document.addEventListener("DOMContentLoaded", () => {
  const filterForm = document.getElementById("cinemaFilterForm");
  const gridContainer = document.getElementById("cinema-grid-container");

  if (filterForm) {
    filterForm.addEventListener("submit", (e) => {
      e.preventDefault();
      const formData = new FormData(filterForm);
      const params = new URLSearchParams();
      for (const [key, value] of formData.entries()) {
        if (value) params.append(key, value);
      }
      cinema_fetchCinemas(
        `${filterForm.action || window.location.pathname}?${params.toString()}`,
      );
    });
  }

  if (gridContainer) {
    gridContainer.addEventListener("click", (e) => {
      const link = e.target.closest('a[data-page], a[href*="sortBy"]');
      if (link) {
        e.preventDefault();
        cinema_fetchCinemas(link.href);
      }
    });
  }

  window.addEventListener("popstate", () => cinema_fetchCinemas(window.location.href));
});

// ══════════════════════════════════════════════════════════════
// Custom Dropdowns (Filters)
// ══════════════════════════════════════════════════════════════
function cinema_toggleFilterDropdown(panelId, evt) {
  if (evt) evt.stopPropagation();
  const panel = document.getElementById(panelId);
  if (!panel) return;
  const isHidden = panel.classList.contains("hidden");
  cinema_closeFilterPanels();
  if (isHidden) {
    panel.classList.remove("hidden");
    openCinemaFilterPanelId = panelId;
  }
}

function cinema_selectFilterOpt(panelId, inputId, labelId, value, label, btnElement) {
  document.getElementById(inputId).value = value;

  // Update trigger label
  const labelEl = document.getElementById(labelId);
  if (labelEl) {
    labelEl.textContent = label;
    if (value !== "") {
      labelEl.classList.add("text-indigo-700", "font-bold");
      labelEl.classList.remove("text-slate-700", "font-medium");
    } else {
      labelEl.classList.add("text-slate-700", "font-medium");
      labelEl.classList.remove("text-indigo-700", "font-bold");
    }
  }

  // Update active highlight on buttons
  const panel = document.getElementById(panelId);
  if (panel) {
    panel.querySelectorAll(".filter-opt-btn").forEach((btn) => {
      btn.classList.remove("bg-slate-50", "font-bold", "text-indigo-700");
      btn.classList.add("text-slate-600", "font-medium");
    });
    if (btnElement) {
      btnElement.classList.add("bg-slate-50", "font-bold", "text-indigo-700");
      btnElement.classList.remove("text-slate-600", "font-medium");
    }
  }

  cinema_closeFilterPanels();
}

function cinema_closeFilterPanels() {
  if (openCinemaFilterPanelId) {
    const panel = document.getElementById(openCinemaFilterPanelId);
    if (panel) panel.classList.add("hidden");
    openCinemaFilterPanelId = null;
  }
}

// ══════════════════════════════════════════════════════════════
// Action Menus (⋮)
// ══════════════════════════════════════════════════════════════
function cinema_toggleActionMenu(id, evt) {
  evt.stopPropagation();
  const menu = document.getElementById(id);
  const isHidden = menu.classList.contains("invisible");
  cinema_closeCinemaActionMenus();
  if (isHidden) {
    menu.classList.remove("invisible", "opacity-0", "scale-95");
    openCinemaActionMenuId = id;
  }
}

function cinema_closeCinemaActionMenus() {
  if (openCinemaActionMenuId) {
    const m = document.getElementById(openCinemaActionMenuId);
    if (m) m.classList.add("invisible", "opacity-0", "scale-95");
    openCinemaActionMenuId = null;
  }
}

document.addEventListener("click", (e) => {
  if (!e.target.closest("[data-filter-dropdown]")) cinema_closeFilterPanels();
  if (!e.target.closest("[data-action-root]")) cinema_closeCinemaActionMenus();
});

// ══════════════════════════════════════════════════════════════
// Status Modals & Lifecycle
// ══════════════════════════════════════════════════════════════
function cinema_openStatusModal(type, cinemaId, cinemaName) {
  currentCinemaId = cinemaId;
  const modalId = "modal-cinema-" + type;
  const nameSpanId = "cinema-" + type + "-name";
  const span = document.getElementById(nameSpanId);
  if (span) span.textContent = cinemaName;
  const modal = document.getElementById(modalId);
  if (modal) modal.classList.remove("hidden");
  document.body.style.overflow = "hidden";
  cinema_closeCinemaActionMenus();
}

function cinema_closeStatusModal(type) {
  const modal = document.getElementById("modal-cinema-" + type);
  if (modal) modal.classList.add("hidden");
  document.body.style.overflow = "";
}

async function cinema_handleStatusSubmit(type) {
  const isActivate = type === "activate";
  const submitBtn = document.getElementById(isActivate ? "confirmActivateBtn" : "confirmDeactivateBtn");
  const spinner = document.getElementById(isActivate ? "activateSpinner" : "deactivateSpinner");
  const btnText = document.getElementById(isActivate ? "activateBtnText" : "deactivateBtnText");

  if (!submitBtn) return;
  if (!currentCinemaId) return;

  submitBtn.disabled = true;
  if (spinner) spinner.classList.remove("d-none");
  if (btnText) btnText.textContent = "Đang xử lý...";

  try {
    const form = document.getElementById("statusChangeForm");
    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    const formData = new FormData();
    formData.append("id", currentCinemaId);
    formData.append("__RequestVerificationToken", token);

    const url = isActivate ? `/Cinemas/Activate/${currentCinemaId}` : `/Cinemas/Deactivate/${currentCinemaId}`;
    const response = await fetch(url, {
      method: "POST",
      body: formData,
      headers: { "X-Requested-With": "XMLHttpRequest" },
    });

    const data = await response.json();

    if (response.ok && data.success) {
      cinema_closeStatusModal(isActivate ? "activate" : "deactivate");
      showToast("Thành công", data.message, "success");
      if (document.getElementById("cinema-grid-container")) {
        cinema_fetchCinemas(window.location.href);
      } else {
        setTimeout(() => location.reload(), 1000);
      }
    } else {
      showToast("Lỗi", data.message || `Không thể ${isActivate ? 'kích hoạt' : 'ngừng hoạt động'} rạp.`, "error");
    }
  } catch (err) {
    console.error("Status submit error:", err);
    showToast("Lỗi", "Lỗi hệ thống khi thực hiện yêu cầu.", "error");
  } finally {
    if (submitBtn) submitBtn.disabled = false;
    if (spinner) spinner.classList.add("d-none");
    if (btnText) btnText.textContent = isActivate ? "Kích hoạt" : "Xác nhận";
  }
}

// ══════════════════════════════════════════════════════════════
// Edit Cinema (Slide-out Drawer)
// ══════════════════════════════════════════════════════════════
// Edit Cinema (Slide-out Drawer)
function getCinemaEditDrawer() { return document.getElementById("cinemaEditDrawer"); }
function getCinemaEditPanel() { return document.getElementById("cinemaEditPanel"); }
function getCinemaEditBackdrop() { return document.getElementById("cinemaEditBackdrop"); }
function getCinemaEditContent() { return document.getElementById("cinemaEditContent"); }

function cinema_toggleCinemaEdit(show) {
  const drawer = getCinemaEditDrawer();
  const panel = getCinemaEditPanel();
  const backdrop = getCinemaEditBackdrop();
  const content = getCinemaEditContent();

  if (!drawer || !panel || !backdrop) return;

  if (show) {
    drawer.classList.remove("invisible");
    setTimeout(() => {
      backdrop.classList.remove("opacity-0");
      panel.classList.remove("translate-x-full");
    }, 10);
    document.body.style.overflow = "hidden";
  } else {
    backdrop.classList.add("opacity-0");
    panel.classList.add("translate-x-full");
    setTimeout(() => {
      drawer.classList.add("invisible");
      if (content) {
        content.innerHTML = `<div class="flex h-full items-center justify-center py-20"><div class="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-900"></div></div>`;
      }
    }, 500);
    document.body.style.overflow = "";
  }
}

async function cinema_openCinemaEdit(id) {
  const content = getCinemaEditContent();
  if (!content) return;

  cinema_closeCinemaActionMenus();
  cinema_toggleCinemaEdit(true);
  try {
    const response = await fetch(`/Cinemas/Edit/${id}`, {
      headers: { "X-Requested-With": "XMLHttpRequest" },
    });
    if (response.ok) {
      const html = await response.text();
      content.innerHTML = html;
      initEditLiveValidation();
    } else {
      content.innerHTML =
        '<div class="p-8 text-center text-red-500 font-bold">Lỗi không thể tải dữ liệu.</div>';
    }
  } catch (e) {
    content.innerHTML =
      '<div class="p-8 text-center text-red-500 font-bold">Lỗi hệ thống.</div>';
  }
}

function initEditLiveValidation() {
  const nameInput = document.getElementById("editCinemaNameInput");
  const cinemaId = document.getElementById("editCinemaId")?.value;
  const submitBtn = document.getElementById("submitEditBtn");
  const nameFeedback = document.getElementById("editNameFeedback");
  const tickIcon = document.getElementById("editNameTickIcon");

  if (!nameInput) return;

  const validateName = debounce(async (name) => {
    if (!name || name.trim().length === 0) {
      resetInputState(nameInput, nameFeedback, submitBtn, tickIcon);
      return;
    }

    submitBtn.disabled = true;
    if (tickIcon) tickIcon.classList.add("hidden");

    try {
      const url = `/Cinemas/VerifyName?name=${encodeURIComponent(name)}&currentId=${cinemaId}`;
      const response = await fetch(url);
      const data = await response.json();

      if (!data.isUnique) {
        setInputError(
          nameInput,
          nameFeedback,
          submitBtn,
          tickIcon,
          "Tên rạp này đã tồn tại trong hệ thống.",
        );
      } else {
        setInputSuccess(nameInput, nameFeedback, submitBtn, tickIcon);
      }
    } catch (error) {
      console.error("Validation error:", error);
      submitBtn.disabled = false;
    }
  });

  nameInput.addEventListener("input", (e) => validateName(e.target.value));
}

async function cinema_handleEditSubmit(event) {
  event.preventDefault();
  const form = event.target;
  const submitBtn = document.getElementById("submitEditBtn");
  const spinner = document.getElementById("submitEditSpinner");
  const btnText = document.getElementById("submitEditText");

  submitBtn.disabled = true;
  spinner.classList.remove("d-none");
  btnText.textContent = "Đang xử lý...";

  try {
    const formData = new FormData(form);
    const response = await fetch(form.action, {
      method: "POST",
      body: formData,
      headers: { "X-Requested-With": "XMLHttpRequest" },
    });

    if (response.ok) {
      const contentType = response.headers.get("content-type");
      if (contentType && contentType.indexOf("application/json") !== -1) {
        const data = await response.json();
        if (data.success) {
          cinema_toggleCinemaEdit(false);
          showToast("Thành công", data.message, "success");
          if (document.getElementById("cinema-grid-container")) {
            cinema_fetchCinemas(window.location.href);
          } else {
            setTimeout(() => location.reload(), 1000);
          }
        } else {
          showToast("Lỗi", data.message || "Có lỗi xảy ra", "error");
        }
      } else {
        const html = await response.text();
        cinemaEditContent.innerHTML = html;
        initEditLiveValidation();
      }
    } else {
      showToast("Lỗi", "Lỗi hệ thống khi gửi yêu cầu.", "error");
    }
  } catch (err) {
    showToast("Lỗi", "Lỗi kết nối khi gửi yêu cầu.", "error");
  } finally {
    submitBtn.disabled = false;
    spinner.classList.add("d-none");
    btnText.textContent = "Lưu thay đổi";
  }
}

// ══════════════════════════════════════════════════════════════
// Create Cinema (Slide-out Drawer)
// ══════════════════════════════════════════════════════════════
// Create Cinema (Slide-out Drawer)
function getCinemaCreateDrawer() { return document.getElementById("cinemaCreateDrawer"); }
function getCinemaCreatePanel() { return document.getElementById("cinemaCreatePanel"); }
function getCinemaCreateBackdrop() { return document.getElementById("cinemaCreateBackdrop"); }
function getCinemaCreateContent() { return document.getElementById("cinemaCreateContent"); }

function cinema_toggleCinemaCreate(show) {
  const drawer = getCinemaCreateDrawer();
  const panel = getCinemaCreatePanel();
  const backdrop = getCinemaCreateBackdrop();
  const content = getCinemaCreateContent();

  if (!drawer || !panel || !backdrop) return;

  if (show) {
    drawer.classList.remove("invisible");
    setTimeout(() => {
      backdrop.classList.remove("opacity-0");
      panel.classList.remove("translate-x-full");
    }, 10);
    document.body.style.overflow = "hidden";
  } else {
    backdrop.classList.add("opacity-0");
    panel.classList.add("translate-x-full");
    setTimeout(() => {
      drawer.classList.add("invisible");
      if (content) {
        content.innerHTML = `<div class="flex h-full items-center justify-center py-20"><div class="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-900"></div></div>`;
      }
    }, 500);
    document.body.style.overflow = "";
  }
}

async function cinema_openCinemaCreate() {
  const content = getCinemaCreateContent();
  if (!content) return;

  cinema_toggleCinemaCreate(true);
  try {
    const response = await fetch("/Cinemas/Create", {
      headers: { "X-Requested-With": "XMLHttpRequest" },
    });
    if (response.ok) {
      const html = await response.text();
      cinemaCreateContent.innerHTML = html;
      initCreateLiveValidation();
    } else {
      cinemaCreateContent.innerHTML =
        '<div class="p-8 text-center text-red-500 font-bold">Lỗi không thể tải dữ liệu.</div>';
    }
  } catch (e) {
    cinemaCreateContent.innerHTML =
      '<div class="p-8 text-center text-red-500 font-bold">Lỗi hệ thống.</div>';
  }
}

function initCreateLiveValidation() {
  const nameInput = document.getElementById("cinemaNameInput");
  const submitBtn = document.getElementById("submitCreateBtn");
  const nameFeedback = document.getElementById("nameFeedback");
  const tickIcon = document.getElementById("nameTickIcon");

  if (!nameInput) return;

  const validateName = debounce(async (name) => {
    if (!name || name.trim().length === 0) {
      resetInputState(nameInput, nameFeedback, submitBtn, tickIcon);
      return;
    }

    submitBtn.disabled = true;
    if (tickIcon) tickIcon.classList.add("hidden");

    try {
      const response = await fetch(
        `/Cinemas/VerifyName?name=${encodeURIComponent(name)}`,
      );
      const data = await response.json();

      if (!data.isUnique) {
        setInputError(
          nameInput,
          nameFeedback,
          submitBtn,
          tickIcon,
          "Tên rạp này đã tồn tại trong hệ thống.",
        );
      } else {
        setInputSuccess(nameInput, nameFeedback, submitBtn, tickIcon);
      }
    } catch (error) {
      console.error("Validation error:", error);
      submitBtn.disabled = false;
    }
  });

  nameInput.addEventListener("input", (e) => validateName(e.target.value));
}

async function cinema_handleCreateSubmit(event) {
  event.preventDefault();
  const form = event.target;
  const submitBtn = document.getElementById("submitCreateBtn");
  const spinner = document.getElementById("submitCreateSpinner");
  const btnText = document.getElementById("submitCreateText");

  submitBtn.disabled = true;
  spinner.classList.remove("d-none");
  btnText.textContent = "Đang xử lý...";

  try {
    const formData = new FormData(form);
    const response = await fetch(form.action, {
      method: "POST",
      body: formData,
      headers: { "X-Requested-With": "XMLHttpRequest" },
    });

    if (response.ok) {
      const contentType = response.headers.get("content-type");
      if (contentType && contentType.indexOf("application/json") !== -1) {
        const data = await response.json();
        if (data.success) {
          cinema_toggleCinemaCreate(false);
          showToast("Thành công", data.message, "success");
          cinema_fetchCinemas(window.location.href);
        } else {
          showToast("Lỗi", data.message || "Có lỗi xảy ra", "error");
        }
      } else {
        const html = await response.text();
        const content = getCinemaCreateContent();
        if (content) {
          content.innerHTML = html;
          initCreateLiveValidation();
        }
      }
    } else {
      showToast("Lỗi", "Lỗi hệ thống khi gửi yêu cầu.", "error");
    }
  } catch (err) {
    showToast("Lỗi", "Lỗi kết nối khi gửi yêu cầu.", "error");
  } finally {
    submitBtn.disabled = false;
    spinner.classList.add("d-none");
    btnText.textContent = "Tạo rạp mới";
  }
}
