// ══════════════════════════════════════════════════════════════
// GLOBAL UTILITIES & STATE
// ══════════════════════════════════════════════════════════════
let openFilterPanelId = null;
let openActionMenuId = null;
let currentCinemaId = null;

function debounce(func, timeout = 500) {
    let timer;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => { func.apply(this, args); }, timeout);
    };
}

function resetInputState(input, feedback, btn, tick) {
    input.classList.remove('!border-red-500', '!ring-red-100', '!border-emerald-500', '!ring-emerald-100');
    feedback.classList.add('hidden');
    if (tick) tick.classList.add('hidden');
    btn.disabled = false;
}

function setInputError(input, feedback, btn, tick, message) {
    input.classList.add('!border-red-500', '!ring-red-100');
    input.classList.remove('!border-emerald-500', '!ring-emerald-100');
    if (tick) tick.classList.add('hidden');
    feedback.textContent = message;
    feedback.classList.remove('hidden', 'text-emerald-500');
    feedback.classList.add('text-red-500');
    btn.disabled = true;
}

function setInputSuccess(input, feedback, btn, tick) {
    input.classList.add('!border-emerald-500', '!ring-emerald-100');
    input.classList.remove('!border-red-500', '!ring-red-100');
    feedback.classList.add('hidden');
    if (tick) tick.classList.remove('hidden');
    btn.disabled = false;
}

// ══════════════════════════════════════════════════════════════
// AJAX FILTERING & PAGINATION (Global Scope)
// ══════════════════════════════════════════════════════════════
function fetchCinemas(url) {
    const gridContainer = document.getElementById('cinema-grid-container');
    if (!gridContainer) return;

    // Show loading state
    gridContainer.style.opacity = '0.5';
    gridContainer.style.pointerEvents = 'none';

    fetch(url, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    .then(res => res.text())
    .then(html => {
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = html;

        // 1. Update Grid Content
        const gridContent = tempDiv.querySelector('#cinema-grid-ajax-content');
        if (gridContent) {
            gridContainer.innerHTML = gridContent.innerHTML;
        }

        // 2. Teleport UI elements
        ['ajax-clear-filter-wrapper', 'ajax-chips-wrapper'].forEach(id => {
            const source = tempDiv.querySelector(`#teleport-${id}`);
            const target = document.getElementById(id);
            if (source && target) target.innerHTML = source.innerHTML;
        });

        // 3. Update URL
        window.history.pushState(null, '', url);
    })
    .catch(err => console.error("Error fetching cinemas:", err))
    .finally(() => {
        gridContainer.style.opacity = '1';
        gridContainer.style.pointerEvents = 'auto';
    });
}

document.addEventListener('DOMContentLoaded', () => {
    const filterForm = document.getElementById('cinemaFilterForm');
    const gridContainer = document.getElementById('cinema-grid-container');

    if (filterForm) {
        filterForm.addEventListener('submit', (e) => {
            e.preventDefault();
            const formData = new FormData(filterForm);
            const params = new URLSearchParams();
            for (const [key, value] of formData.entries()) {
                if (value) params.append(key, value);
            }
            fetchCinemas(`${filterForm.action || window.location.pathname}?${params.toString()}`);
        });
    }

    if (gridContainer) {
        gridContainer.addEventListener('click', (e) => {
            const link = e.target.closest('a[data-page], a[href*="sortBy"]');
            if (link) {
                e.preventDefault();
                fetchCinemas(link.href);
            }
        });
    }

    window.addEventListener('popstate', () => fetchCinemas(window.location.href));
});

// ══════════════════════════════════════════════════════════════
// Custom Dropdowns (Filters)
// ══════════════════════════════════════════════════════════════
function toggleFilterDropdown(panelId, evt) {
    evt.stopPropagation();
    const panel = document.getElementById(panelId);
    if (!panel) return;
    const isHidden = panel.classList.contains('hidden');
    closeFilterPanels();
    if (isHidden) {
        panel.classList.remove('hidden');
        openFilterPanelId = panelId;
    }
}

function selectFilterOpt(panelId, inputId, labelId, value, label, btnElement) {
    document.getElementById(inputId).value = value;
    
    // Update trigger label
    const labelEl = document.getElementById(labelId);
    if (labelEl) {
        labelEl.textContent = label;
        if (value !== '') {
            labelEl.classList.add('text-indigo-700', 'font-bold');
            labelEl.classList.remove('text-slate-700', 'font-medium');
        } else {
            labelEl.classList.add('text-slate-700', 'font-medium');
            labelEl.classList.remove('text-indigo-700', 'font-bold');
        }
    }

    // Update active highlight on buttons
    const panel = document.getElementById(panelId);
    if (panel) {
        panel.querySelectorAll('.filter-opt-btn').forEach(btn => {
            btn.classList.remove('bg-slate-50', 'font-bold', 'text-indigo-700');
            btn.classList.add('text-slate-600', 'font-medium');
        });
        if (btnElement) {
            btnElement.classList.add('bg-slate-50', 'font-bold', 'text-indigo-700');
            btnElement.classList.remove('text-slate-600', 'font-medium');
        }
    }
    
    closeFilterPanels();
}

function closeFilterPanels() {
    if (openFilterPanelId) {
        const panel = document.getElementById(openFilterPanelId);
        if (panel) panel.classList.add('hidden');
        openFilterPanelId = null;
    }
}

// ══════════════════════════════════════════════════════════════
// Action Menus (⋮)
// ══════════════════════════════════════════════════════════════
function toggleActionMenu(id, evt) {
    evt.stopPropagation();
    const menu = document.getElementById(id);
    const isHidden = menu.classList.contains('invisible');
    closeActionMenus();
    if (isHidden) {
        menu.classList.remove('invisible', 'opacity-0', 'scale-95');
        openActionMenuId = id;
    }
}

function closeActionMenus() {
    if (openActionMenuId) {
        const m = document.getElementById(openActionMenuId);
        if (m) m.classList.add('invisible', 'opacity-0', 'scale-95');
        openActionMenuId = null;
    }
}

document.addEventListener('click', (e) => {
    if (!e.target.closest('[data-filter-dropdown]')) closeFilterPanels();
    if (!e.target.closest('[data-action-root]'))    closeActionMenus();
});

// ══════════════════════════════════════════════════════════════
// Status Modals & Lifecycle
// ══════════════════════════════════════════════════════════════
function openStatusModal(type, cinemaId, cinemaName) {
    currentCinemaId = cinemaId;
    const modalId = 'modal-' + type;
    const nameSpanId = type + '-room-name';
    document.getElementById(nameSpanId).textContent = cinemaName;
    document.getElementById(modalId).classList.remove('hidden');
    document.body.style.overflow = 'hidden';
    closeActionMenus();
}

function closeStatusModal(type) {
    document.getElementById('modal-' + type).classList.add('hidden');
    document.body.style.overflow = '';
}

async function handleActivateSubmit() {
    if (!currentCinemaId) return;
    const submitBtn = document.getElementById('confirmActivateBtn');
    const spinner = document.getElementById('activateSpinner');
    const btnText = document.getElementById('activateBtnText');
    
    submitBtn.disabled = true;
    spinner.classList.remove('d-none');
    btnText.textContent = 'Đang xử lý...';

    try {
        const form = document.getElementById('statusChangeForm');
        const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
        const formData = new FormData();
        formData.append('id', currentCinemaId);
        formData.append('__RequestVerificationToken', token);

        const response = await fetch(`/Cinemas/Activate/${currentCinemaId}`, {
            method: 'POST',
            body: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const data = await response.json();

        if (data.success) {
            closeStatusModal('activate');
            showToast('Thành công', data.message, 'success');
            fetchCinemas(window.location.href);
        } else {
            showToast('Lỗi', data.message, 'error');
        }
    } catch (err) {
        showToast('Lỗi', 'Lỗi hệ thống khi thực hiện yêu cầu.', 'error');
    } finally {
        submitBtn.disabled = false;
        spinner.classList.add('d-none');
        btnText.textContent = 'Xác nhận';
    }
}

async function handleDeactivateSubmit() {
    if (!currentCinemaId) return;
    const submitBtn = document.getElementById('confirmDeactivateBtn');
    const spinner = document.getElementById('deactivateSpinner');
    const btnText = document.getElementById('deactivateBtnText');
    
    submitBtn.disabled = true;
    spinner.classList.remove('d-none');
    btnText.textContent = 'Đang xử lý...';

    try {
        const form = document.getElementById('statusChangeForm');
        const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
        const formData = new FormData();
        formData.append('id', currentCinemaId);
        formData.append('__RequestVerificationToken', token);

        const response = await fetch(`/Cinemas/Deactivate/${currentCinemaId}`, {
            method: 'POST',
            body: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const data = await response.json();

        if (data.success) {
            closeStatusModal('deactivate');
            showToast('Thành công', data.message, 'success');
            fetchCinemas(window.location.href);
        } else {
            showToast('Lỗi', data.message, 'error');
        }
    } catch (err) {
        showToast('Lỗi', 'Lỗi hệ thống khi thực hiện yêu cầu.', 'error');
    } finally {
        submitBtn.disabled = false;
        spinner.classList.add('d-none');
        btnText.textContent = 'Xác nhận';
    }
}

// ══════════════════════════════════════════════════════════════
// Edit Cinema (Slide-out Drawer)
// ══════════════════════════════════════════════════════════════
const cinemaEditDrawer = document.getElementById('cinemaEditDrawer');
const cinemaEditPanel = document.getElementById('cinemaEditPanel');
const cinemaEditBackdrop = document.getElementById('cinemaEditBackdrop');
const cinemaEditContent = document.getElementById('cinemaEditContent');

function toggleCinemaEdit(show) {
    if (show) {
        cinemaEditDrawer.classList.remove('invisible');
        setTimeout(() => {
            cinemaEditBackdrop.classList.remove('opacity-0');
            cinemaEditPanel.classList.remove('translate-x-full');
        }, 10);
        document.body.style.overflow = 'hidden';
    } else {
        cinemaEditBackdrop.classList.add('opacity-0');
        cinemaEditPanel.classList.add('translate-x-full');
        setTimeout(() => {
            cinemaEditDrawer.classList.add('invisible');
            cinemaEditContent.innerHTML = `<div class="flex h-full items-center justify-center py-20"><div class="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-900"></div></div>`;
        }, 500);
        document.body.style.overflow = '';
    }
}

async function openCinemaEdit(id) {
    closeActionMenus();
    toggleCinemaEdit(true);
    try {
        const response = await fetch(`/Cinemas/Edit/${id}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        if (response.ok) {
            const html = await response.text();
            cinemaEditContent.innerHTML = html;
            initEditLiveValidation();
        } else {
            cinemaEditContent.innerHTML = '<div class="p-8 text-center text-red-500 font-bold">Lỗi không thể tải dữ liệu.</div>';
        }
    } catch (e) {
        cinemaEditContent.innerHTML = '<div class="p-8 text-center text-red-500 font-bold">Lỗi hệ thống.</div>';
    }
}

function initEditLiveValidation() {
    const nameInput = document.getElementById('editCinemaNameInput');
    const cinemaId = document.getElementById('editCinemaId')?.value;
    const submitBtn = document.getElementById('submitEditBtn');
    const nameFeedback = document.getElementById('editNameFeedback');
    const tickIcon = document.getElementById('editNameTickIcon');
    
    if (!nameInput) return;

    const validateName = debounce(async (name) => {
        if (!name || name.trim().length === 0) {
            resetInputState(nameInput, nameFeedback, submitBtn, tickIcon);
            return;
        }

        submitBtn.disabled = true;
        if (tickIcon) tickIcon.classList.add('hidden');

        try {
            const url = `/Cinemas/VerifyName?name=${encodeURIComponent(name)}&currentId=${cinemaId}`;
            const response = await fetch(url);
            const data = await response.json();
            
            if (!data.isUnique) {
                setInputError(nameInput, nameFeedback, submitBtn, tickIcon, "Tên rạp này đã tồn tại trong hệ thống.");
            } else {
                setInputSuccess(nameInput, nameFeedback, submitBtn, tickIcon);
            }
        } catch (error) {
            console.error("Validation error:", error);
            submitBtn.disabled = false;
        }
    });

    nameInput.addEventListener('input', (e) => validateName(e.target.value));
}

async function handleEditSubmit(event) {
    event.preventDefault();
    const form = event.target;
    const submitBtn = document.getElementById('submitEditBtn');
    const spinner = document.getElementById('submitEditSpinner');
    const btnText = document.getElementById('submitEditText');

    submitBtn.disabled = true;
    spinner.classList.remove('d-none');
    btnText.textContent = 'Đang xử lý...';

    try {
        const formData = new FormData(form);
        const response = await fetch(form.action, {
            method: 'POST',
            body: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        if (response.ok) {
            const contentType = response.headers.get("content-type");
            if (contentType && contentType.indexOf("application/json") !== -1) {
                    const data = await response.json();
                    if (data.success) {
                    toggleCinemaEdit(false);
                    showToast('Thành công', data.message, 'success');
                    fetchCinemas(window.location.href);
                } else {
                    showToast('Lỗi', data.message || 'Có lỗi xảy ra', 'error');
                }
            } else {
                const html = await response.text();
                cinemaEditContent.innerHTML = html;
                initEditLiveValidation();
            }
        } else {
            showToast('Lỗi', 'Lỗi hệ thống khi gửi yêu cầu.', 'error');
        }
    } catch (err) {
        showToast('Lỗi', 'Lỗi kết nối khi gửi yêu cầu.', 'error');
    } finally {
        submitBtn.disabled = false;
        spinner.classList.add('d-none');
        btnText.textContent = 'Lưu thay đổi';
    }
}

// ══════════════════════════════════════════════════════════════
// Create Cinema (Slide-out Drawer)
// ══════════════════════════════════════════════════════════════
const cinemaCreateDrawer = document.getElementById('cinemaCreateDrawer');
const cinemaCreatePanel = document.getElementById('cinemaCreatePanel');
const cinemaCreateBackdrop = document.getElementById('cinemaCreateBackdrop');
const cinemaCreateContent = document.getElementById('cinemaCreateContent');

function toggleCinemaCreate(show) {
    if (show) {
        cinemaCreateDrawer.classList.remove('invisible');
        setTimeout(() => {
            cinemaCreateBackdrop.classList.remove('opacity-0');
            cinemaCreatePanel.classList.remove('translate-x-full');
        }, 10);
        document.body.style.overflow = 'hidden';
    } else {
        cinemaCreateBackdrop.classList.add('opacity-0');
        cinemaCreatePanel.classList.add('translate-x-full');
        setTimeout(() => {
            cinemaCreateDrawer.classList.add('invisible');
            cinemaCreateContent.innerHTML = `<div class="flex h-full items-center justify-center py-20"><div class="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-900"></div></div>`;
        }, 500);
        document.body.style.overflow = '';
    }
}

async function openCinemaCreate() {
    toggleCinemaCreate(true);
    try {
        const response = await fetch('/Cinemas/Create', {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        if (response.ok) {
            const html = await response.text();
            cinemaCreateContent.innerHTML = html;
            initCreateLiveValidation();
        } else {
            cinemaCreateContent.innerHTML = '<div class="p-8 text-center text-red-500 font-bold">Lỗi không thể tải dữ liệu.</div>';
        }
    } catch (e) {
        cinemaCreateContent.innerHTML = '<div class="p-8 text-center text-red-500 font-bold">Lỗi hệ thống.</div>';
    }
}

function initCreateLiveValidation() {
    const nameInput = document.getElementById('cinemaNameInput');
    const submitBtn = document.getElementById('submitCreateBtn');
    const nameFeedback = document.getElementById('nameFeedback');
    const tickIcon = document.getElementById('nameTickIcon');
    
    if (!nameInput) return;

    const validateName = debounce(async (name) => {
        if (!name || name.trim().length === 0) {
            resetInputState(nameInput, nameFeedback, submitBtn, tickIcon);
            return;
        }

        submitBtn.disabled = true;
        if (tickIcon) tickIcon.classList.add('hidden');
        
        try {
            const response = await fetch(`/Cinemas/VerifyName?name=${encodeURIComponent(name)}`);
            const data = await response.json();
            
            if (!data.isUnique) {
                setInputError(nameInput, nameFeedback, submitBtn, tickIcon, "Tên rạp này đã tồn tại trong hệ thống.");
            } else {
                setInputSuccess(nameInput, nameFeedback, submitBtn, tickIcon);
            }
        } catch (error) {
            console.error("Validation error:", error);
            submitBtn.disabled = false;
        }
    });

    nameInput.addEventListener('input', (e) => validateName(e.target.value));
}

async function handleCreateSubmit(event) {
    event.preventDefault();
    const form = event.target;
    const submitBtn = document.getElementById('submitCreateBtn');
    const spinner = document.getElementById('submitCreateSpinner');
    const btnText = document.getElementById('submitCreateText');

    submitBtn.disabled = true;
    spinner.classList.remove('d-none');
    btnText.textContent = 'Đang xử lý...';

    try {
        const formData = new FormData(form);
        const response = await fetch(form.action, {
            method: 'POST',
            body: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        if (response.ok) {
            const contentType = response.headers.get("content-type");
            if (contentType && contentType.indexOf("application/json") !== -1) {
                    const data = await response.json();
                    if (data.success) {
                    toggleCinemaCreate(false);
                    showToast('Thành công', data.message, 'success');
                    fetchCinemas(window.location.href);
                } else {
                    showToast('Lỗi', data.message || 'Có lỗi xảy ra', 'error');
                }
            } else {
                const html = await response.text();
                cinemaCreateContent.innerHTML = html;
                initCreateLiveValidation();
            }
        } else {
            showToast('Lỗi', 'Lỗi hệ thống khi gửi yêu cầu.', 'error');
        }
    } catch (err) {
        showToast('Lỗi', 'Lỗi kết nối khi gửi yêu cầu.', 'error');
    } finally {
        submitBtn.disabled = false;
        spinner.classList.add('d-none');
        btnText.textContent = 'Tạo rạp mới';
    }
}
