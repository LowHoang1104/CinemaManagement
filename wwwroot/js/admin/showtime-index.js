// Showtime Management Module - Admin Index Script
// Dependencies: Tailwind CSS, showToast (global)

(function () {
    // --- State Variables ---
    let openDropdownId = null;
    let openFilterPanelId = null;
    let lastOverlapData = null;
    let currentCancelId = null;

    // --- Selectors ---
    const modal = document.getElementById('CreateModal');
    const modalContent = document.getElementById('CreateModalContent');
    const editModal = document.getElementById('EditModal');
    const editModalContent = document.getElementById('EditModalContent');
    const editModalBody = document.getElementById('EditModalBody');
    const detailsModal = document.getElementById('DetailsModal');
    const detailsModalContent = document.getElementById('DetailsModalContent');
    const detailsModalBody = document.getElementById('DetailsModalBody');
    const conflictModal = document.getElementById('ConflictModal');
    const conflictModalContent = document.getElementById('ConflictModalContent');
    const cancelModal = document.getElementById('CancelModal');
    const cancelModalContent = document.getElementById('CancelModalContent');

    const movieIdInput = document.getElementById('MovieIdInput');
    const roomIdInput = document.getElementById('RoomIdInput');
    const startTimeInput = document.getElementById('StartTimeInput');
    const basePriceInput = document.getElementById('BasePriceInput');
    const movieDurationText = document.getElementById('MovieDurationText');
    const totalDurationText = document.getElementById('TotalDurationText');
    const movieInfo = document.getElementById('MovieInfo');
    const timePreview = document.getElementById('TimePreview');
    const timeRangeDisplay = document.getElementById('TimeRangeDisplay');
    const priceStandard = document.getElementById('PriceStandard');
    const priceVIP = document.getElementById('PriceVIP');
    const priceCouple = document.getElementById('PriceCouple');
    const validationBar = document.getElementById('ValidationBar');
    const submitBtn = document.getElementById('SubmitBtn');

    // --- Utilities ---
    function formatCurrency(n) { 
        return new Intl.NumberFormat('en-US').format(n) + ' VNĐ'; 
    }

    function executeScripts(container) {
        container.querySelectorAll('script').forEach(oldScript => {
            const newScript = document.createElement('script');
            Array.from(oldScript.attributes).forEach(attr =>
                newScript.setAttribute(attr.name, attr.value)
            );
            newScript.textContent = oldScript.textContent;
            oldScript.parentNode.replaceChild(newScript, oldScript);
        });
    }

    // --- Modal Logic ---
    window.toggleModal = function (show) {
        if (!modal || !modalContent) return;
        if (show) {
            modal.classList.remove('hidden', 'pointer-events-none');
            setTimeout(() => {
                modal.classList.remove('opacity-0');
                modalContent.classList.remove('translate-y-0');
                modalContent.classList.add('translate-y-[2rem]');
            }, 10);
            document.body.style.overflow = 'hidden';
        } else {
            modal.classList.add('opacity-0');
            modalContent.classList.remove('translate-y-[2rem]');
            modalContent.classList.add('translate-y-0');
            setTimeout(() => modal.classList.add('hidden', 'pointer-events-none'), 300);
            document.body.style.overflow = '';
        }
    };

    window.toggleEditModal = function (show, viewOnly = false) {
        if (!editModal || !editModalContent || !editModalBody) return;
        if (show) {
            const title = document.getElementById('EditModalTitle');
            const subtitle = document.getElementById('EditModalSubtitle');
            const submitBtn = document.getElementById('EditSubmitBtn');

            if (viewOnly) {
                if (title) title.textContent = 'Chi tiết suất chiếu';
                if (subtitle) subtitle.textContent = 'Thông tin chi tiết về suất chiếu';
                if (submitBtn) submitBtn.classList.add('hidden');
            } else {
                if (title) title.textContent = 'Chỉnh sửa suất chiếu';
                if (subtitle) subtitle.textContent = 'Cập nhật thông tin suất chiếu';
                if (submitBtn) submitBtn.classList.remove('hidden');
            }

            editModal.classList.remove('hidden', 'pointer-events-none');
            setTimeout(() => {
                editModal.classList.remove('opacity-0');
                editModalContent.classList.remove('translate-y-0');
                editModalContent.classList.add('translate-y-[2rem]');
            }, 10);
            document.body.style.overflow = 'hidden';
        } else {
            editModal.classList.add('opacity-0');
            editModalContent.classList.remove('translate-y-[2rem]');
            editModalContent.classList.add('translate-y-0');
            setTimeout(() => {
                editModal.classList.add('hidden', 'pointer-events-none');
                editModalBody.innerHTML = `
                    <div class="flex items-center justify-center py-20">
                        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-900"></div>
                    </div>
                `;
            }, 300);
            document.body.style.overflow = '';
        }
    };

    window.openEditModal = async function (id, viewOnly = false) {
        // Force-close Create modal
        if (modal) modal.style.display = 'none';
        document.body.style.overflow = '';
        toggleEditModal(true, viewOnly);
        try {
            const response = await fetch(`/ShowTimes/Edit/${id}?viewOnly=${viewOnly}`);
            if (response.ok) {
                const html = await response.text();
                editModalBody.innerHTML = html;
                executeScripts(editModalBody);
            } else {
                editModalBody.innerHTML = '<div class="p-8 text-center text-red-500 font-bold">Lỗi không thể tải dữ liệu.</div>';
            }
        } catch (e) {
            editModalBody.innerHTML = '<div class="p-8 text-center text-red-500 font-bold">Lỗi hệ thống.</div>';
        }
    };

    window.openDetailsModal = async function (id) {
        if (!detailsModal || !detailsModalContent || !detailsModalBody) return;
        detailsModal.classList.remove('hidden', 'pointer-events-none');
        setTimeout(() => {
            detailsModal.classList.remove('opacity-0');
            detailsModalContent.classList.remove('translate-y-0');
            detailsModalContent.classList.add('translate-y-[2rem]');
        }, 10);
        document.body.style.overflow = 'hidden';
        try {
            const response = await fetch(`/ShowTimes/Details/${id}`);
            if (response.ok) {
                const html = await response.text();
                detailsModalBody.innerHTML = html;
            } else {
                detailsModalBody.innerHTML = '<div class="p-8 text-center text-red-500 font-bold">Không thể tải chi tiết.</div>';
            }
        } catch (e) {
            detailsModalBody.innerHTML = '<div class="p-8 text-center text-red-500 font-bold">Lỗi kết nối.</div>';
        }
    };

    window.closeDetailsModal = function () {
        if (!detailsModal) return;
        detailsModal.classList.add('opacity-0');
        detailsModalContent.classList.add('translate-y-0');
        detailsModalContent.classList.remove('translate-y-[2rem]');
        setTimeout(() => {
            detailsModal.classList.add('hidden', 'pointer-events-none');
            if (detailsModalBody) detailsModalBody.innerHTML = '<div class="flex items-center justify-center py-20"><div class="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-900"></div></div>';
        }, 300);
        document.body.style.overflow = '';
    };

    window.toggleConflictModal = function (show) {
        if (!conflictModal || !conflictModalContent) return;
        if (show) {
            conflictModal.classList.remove('hidden', 'pointer-events-none');
            setTimeout(() => {
                conflictModal.classList.remove('opacity-0');
                conflictModalContent.classList.add('translate-y-[2rem]');
                conflictModalContent.classList.remove('translate-y-0');
            }, 10);
        } else {
            conflictModal.classList.add('opacity-0');
            conflictModalContent.classList.remove('translate-y-[2rem]');
            conflictModalContent.classList.add('translate-y-0');
            setTimeout(() => conflictModal.classList.add('hidden', 'pointer-events-none'), 300);
        }
    };

    window.toggleCancelModal = function (show) {
        if (!cancelModal || !cancelModalContent) return;
        if (show) {
            cancelModal.classList.remove('hidden', 'pointer-events-none');
            setTimeout(() => {
                cancelModal.classList.remove('opacity-0');
                cancelModalContent.classList.remove('-translate-y-full');
                cancelModalContent.classList.add('translate-y-[2rem]');
            }, 10);
            document.body.style.overflow = 'hidden';
        } else {
            cancelModal.classList.add('opacity-0');
            cancelModalContent.classList.remove('translate-y-[2rem]');
            cancelModalContent.classList.add('-translate-y-full');
            setTimeout(() => {
                cancelModal.classList.add('hidden', 'pointer-events-none');
                currentCancelId = null;
            }, 300);
            document.body.style.overflow = '';
        }
    };

    window.openCancelModal = function (id, movie, time, date) {
        currentCancelId = id;
        document.getElementById('CancelMovieName').textContent = movie;
        document.getElementById('CancelTime').textContent = time;
        document.getElementById('CancelDate').textContent = date;
        toggleCancelModal(true);
    };

    // --- Dropdown Logic ---
    window.toggleDropdown = function (id, e) {
        if (e) e.stopPropagation();
        const panel = document.getElementById(id);
        const isHidden = panel.classList.contains('hidden');
        closeAllDropdowns();
        if (isHidden) { 
            panel.classList.remove('hidden'); 
            openDropdownId = id; 
        }
    };

    window.closeAllDropdowns = function () {
        if (openDropdownId) { 
            document.getElementById(openDropdownId).classList.add('hidden'); 
            openDropdownId = null; 
        }
    };

    window.selectOption = function (panelId, inputId, labelId, value, label, type) {
        document.getElementById(inputId).value = value;
        document.getElementById(labelId).textContent = label;
        closeAllDropdowns();
        updateRoomDetail(roomIdInput.value);
        updatePreview();
    };

    // --- Filter Logic ---
    window.toggleFilterDropdown = function (panelId, evt) {
        if (evt) evt.stopPropagation();
        const panel = document.getElementById(panelId);
        const isHidden = panel.classList.contains('hidden');
        closeFilterPanels();
        if (isHidden) {
            panel.classList.remove('hidden');
            openFilterPanelId = panelId;
        }
    };

    window.selectFilterOpt = function (panelId, inputId, labelId, value, label, hasValue, btnElement) {
        document.getElementById(inputId).value = value;
        const labelEl = document.getElementById(labelId);
        labelEl.textContent = label;
        
        if (hasValue) {
            labelEl.classList.remove('text-slate-500');
            labelEl.classList.add('font-medium', 'text-slate-900');
        } else {
            labelEl.classList.add('text-slate-500');
            labelEl.classList.remove('font-medium', 'text-slate-900');
        }

        if (btnElement) {
            const panel = document.getElementById(panelId);
            panel.querySelectorAll('.filter-tick').forEach(el => el.classList.add('hidden'));
            panel.querySelectorAll('.filter-text').forEach(el => {
                el.classList.remove('font-semibold', 'text-indigo-700');
                el.classList.add('text-slate-600');
            });
            
            const activeTick = btnElement.querySelector('.filter-tick');
            if (activeTick) activeTick.classList.remove('hidden');
            const activeText = btnElement.querySelector('.filter-text');
            if (activeText) {
                activeText.classList.remove('text-slate-600');
                activeText.classList.add('font-semibold', 'text-indigo-700');
            }
        }
        closeFilterPanels();
    };

    window.closeFilterPanels = function () {
        if (openFilterPanelId) {
            document.getElementById(openFilterPanelId)?.classList.add('hidden');
            openFilterPanelId = null;
        }
    };

    // --- Form & Real-time Preview Logic ---
    window.updatePricing = function () {
        if (!basePriceInput) return;
        const raw = basePriceInput.value.replace(/\D/g, '');
        const b = parseInt(raw) || 0;
        priceStandard.textContent = formatCurrency(b);
        priceVIP.textContent = formatCurrency(b + 5000);
        priceCouple.textContent = formatCurrency(b * 2);
    };

    function updateRoomDetail(id) {
        const detail = document.getElementById('RoomDetail');
        const text = document.getElementById('RoomDetailText');
        const lowerId = id?.toLowerCase();
        // movieDurations and roomLocations must be set globally before this script runs
        if (lowerId && window.roomLocations && window.roomLocations[lowerId]) { 
            text.textContent = window.roomLocations[lowerId]; 
            detail.classList.remove('hidden'); 
        } else {
            detail.classList.add('hidden');
        }
    }

    window.updatePreview = function () {
        if (!movieIdInput || !startTimeInput || !roomIdInput) return;
        const m = movieIdInput.value?.toLowerCase(), s = startTimeInput.value, r = roomIdInput.value;
        const duration = (m && window.movieDurations) ? parseInt(window.movieDurations[m]) : 0;
        
        if (m && duration && !isNaN(duration) && duration > 0) {
            movieDurationText.textContent = duration;
            totalDurationText.textContent = duration + 15;
            movieInfo.classList.remove('hidden');
            
            if (s) {
                const start = new Date(s), end = new Date(start.getTime() + duration * 60000), cl = new Date(end.getTime() + 15 * 60000);
                const f = (dt) => dt.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', hour12: false });
                timeRangeDisplay.textContent = `${f(start)} - ${f(end)} (Xong dọn dẹp: ${f(cl)})`;
                timePreview.classList.remove('hidden');
            } else {
                timePreview.classList.add('hidden');
            }
        } else { 
            movieInfo.classList.add('hidden'); 
            timePreview.classList.add('hidden'); 
        }
        
        if (m && r && s) {
            checkOverlap();
        } else {
            if (validationBar) validationBar.classList.add('hidden');
            if (submitBtn) submitBtn.disabled = true;
            lastOverlapData = null;
        }
    };

    async function checkOverlap() {
        if (!validationBar || !submitBtn) return;
        const m = movieIdInput.value?.trim(), r = roomIdInput.value?.trim(), s = startTimeInput.value;
        const year = s ? new Date(s).getFullYear() : 0;
        
        if (!m || m.length < 32 || !r || r.length < 32 || !s || year < 1800) { 
            validationBar.classList.add('hidden'); 
            submitBtn.disabled = true; 
            lastOverlapData = null;
            return; 
        }

        try {
            const url = `/ShowTimes/CheckOverlap?roomId=${r}&startAt=${s}&movieId=${m}`;
            const res = await fetch(url);
            if (res.ok) {
                const d = await res.json();
                lastOverlapData = d;
                validationBar.classList.remove('hidden');
                if (d.isPast) {
                    lastOverlapData = null;
                    validationBar.className = "rounded-xl p-4 border border-orange-200 bg-orange-50 text-orange-700 text-sm flex items-start gap-3 animate-in fade-in slide-in-from-bottom-2 duration-300";
                    validationBar.innerHTML = `
                        <svg class="h-5 w-5 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
                        </svg>
                        <div>
                            <span class="font-bold">Không hợp lệ:</span> Thời gian bắt đầu suất chiếu không thể nằm trong quá khứ. Vui lòng chọn thời điểm khác.
                        </div>
                    `;
                    submitBtn.disabled = true;
                } else if (d.isOverlapping) {
                    validationBar.className = "rounded-xl p-4 border border-red-200 bg-red-50 text-red-700 text-sm flex items-start gap-3 animate-in fade-in slide-in-from-bottom-2 duration-300";
                    validationBar.innerHTML = `
                        <svg class="h-5 w-5 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
                        </svg>
                        <div class="flex-1">
                            <span class="font-bold">Cảnh báo:</span> Phát hiện xung đột thời gian với suất chiếu khác. <br/>
                            <span class="text-xs font-semibold opacity-90 cursor-pointer underline mt-1 block hover:opacity-100" onclick="document.getElementById('CreateForm').requestSubmit()">Bấm 'Tạo suất chiếu' để xem chi tiết >></span>
                        </div>
                    `;
                    submitBtn.disabled = false;
                } else {
                    lastOverlapData = null;
                    validationBar.className = "rounded-xl p-4 border border-green-200 bg-green-50 text-green-700 text-sm flex items-center gap-2 animate-in fade-in slide-in-from-bottom-2 duration-300";
                    validationBar.innerHTML = `
                        <svg class="h-5 w-5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7"/>
                        </svg>
                        <span>Không có xung đột thời gian</span>
                    `;
                    submitBtn.disabled = false;
                }
            } else { 
                validationBar.classList.add('hidden'); 
                submitBtn.disabled = true; 
            }
        } catch (e) { 
            console.error("CheckOverlap error:", e); 
            validationBar.classList.add('hidden');
            submitBtn.disabled = true;
        }
    }

    function buildAndShowConflictModal(conflicts) {
        const list = document.getElementById('ConflictList');
        if (!list) return;
        list.innerHTML = '';
        
        const roomLabel = document.getElementById('RoomLabel').textContent || 'Phòng chiếu';
        document.getElementById('ConflictSubtitle').textContent = `Suất chiếu bạn muốn tạo bị trùng thời gian với ${conflicts.length} suất chiếu đã tồn tại trong ${roomLabel}.`;

        const movieLabel = document.getElementById('MovieLabel').textContent;
        document.getElementById('ConflictNewMovieName').textContent = movieLabel;
        
        const startStr = startTimeInput.value;
        if (startStr) {
            const sDate = new Date(startStr);
            const duration = parseInt(movieDurationText.textContent) || 0;
            const eDate = new Date(sDate.getTime() + duration * 60000);
            const fTime = (dt) => dt.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', hour12: false });
            document.getElementById('ConflictNewTime').textContent = `${fTime(sDate)} - ${fTime(eDate)}`;
            document.getElementById('ConflictNewDate').textContent = sDate.toLocaleDateString('vi-VN', { weekday: 'short', year: 'numeric', month: '2-digit', day: '2-digit' });
            document.getElementById('ConflictNewShowBlock').classList.remove('hidden');
        }

        conflicts.forEach(c => {
            list.innerHTML += `
                <div class="bg-white rounded-xl border border-red-200/60 p-4 shadow-sm">
                    <h4 class="font-bold text-slate-800 text-base">${c.movieTitle}</h4>
                    <div class="flex items-center gap-2 mt-1.5 text-slate-600 font-medium text-sm">
                        <svg class="w-4 h-4 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                        ${c.startTime} - ${c.endTime}
                    </div>
                    <div class="text-slate-500 text-xs mt-1">Thời lượng: ${c.duration} phút + 15 phút dọn dẹp</div>
                </div>
            `;
        });

        toggleConflictModal(true);
    }

    // --- Initialization & Event Listeners ---
    document.addEventListener('DOMContentLoaded', () => {
        // Modal buttons
        const openBtn = document.getElementById('OpenModalBtn');
        if (openBtn) openBtn.addEventListener('click', () => toggleModal(true));
        
        const closeBtn = document.getElementById('CloseModalBtn');
        if (closeBtn) closeBtn.addEventListener('click', () => toggleModal(false));
        
        const cancelBtn = document.getElementById('CancelModalBtn');
        if (cancelBtn) cancelBtn.addEventListener('click', () => toggleModal(false));
        
        const closeEditBtn = document.getElementById('CloseEditModalBtn');
        if (closeEditBtn) closeEditBtn.addEventListener('click', () => toggleEditModal(false));
        
        const cancelEditBtn = document.getElementById('CancelEditModalBtn');
        if (cancelEditBtn) cancelEditBtn.addEventListener('click', () => toggleEditModal(false));

        // Global clicks
        document.addEventListener('click', (e) => {
            closeAllDropdowns();
            if (!e.target.closest('[data-filter-dropdown]')) closeFilterPanels();
        });

        // Form inputs
        if (startTimeInput) {
            startTimeInput.addEventListener('change', updatePreview);
            startTimeInput.addEventListener('input', updatePreview);
        }

        if (basePriceInput) {
            basePriceInput.addEventListener('input', function(e) {
                const oldValue = this.value;
                const cursor = this.selectionStart;
                const digitsBefore = oldValue.substring(0, cursor).replace(/\D/g, '').length;
                const digitsOnly = oldValue.replace(/\D/g, '');
                const clean = digitsOnly.replace(/^0+/, '');

                if (clean === '') {
                    this.value = '';
                    updatePricing();
                    return;
                }

                const formatted = new Intl.NumberFormat('en-US').format(BigInt(clean));
                this.value = formatted;

                let newCursor = 0;
                let digitsFound = 0;
                for (let i = 0; i < formatted.length; i++) {
                    if (digitsFound < digitsBefore) {
                        if (/\d/.test(formatted[i])) digitsFound++;
                        newCursor = i + 1;
                    } else {
                        break;
                    }
                }
                this.setSelectionRange(newCursor, newCursor);
                updatePricing();
            });
        }

        // Mutation Observer for custom dropdowns
        const obs = new MutationObserver(() => updatePreview());
        if (movieIdInput) obs.observe(movieIdInput, { attributes: true });
        if (roomIdInput) obs.observe(roomIdInput, { attributes: true });

        // Form submission intercept
        const createForm = document.getElementById('CreateForm');
        if (createForm) {
            createForm.addEventListener('submit', function(e) {
                if (lastOverlapData && lastOverlapData.isOverlapping) {
                    e.preventDefault();
                    buildAndShowConflictModal(lastOverlapData.conflicts);
                }
            });
        }

        // Cancel confirmation
        const confirmCancelBtn = document.getElementById('ConfirmCancelBtn');
        if (confirmCancelBtn) {
            confirmCancelBtn.addEventListener('click', async function() {
                if (!currentCancelId) return;
                const btn = this;
                btn.disabled = true;
                const originalText = btn.textContent;
                btn.innerHTML = '<div class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mx-auto"></div>';

                try {
                    const response = await fetch(`/ShowTimes/Cancel/${currentCancelId}`, {
                        method: 'POST',
                        headers: {
                            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value,
                            'Content-Type': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });

                    if (response.ok) {
                        const data = await response.json();
                        if (data.success) {
                            toggleCancelModal(false);
                            if (window.showToast) window.showToast('Thành công', 'Suất chiếu đã được hủy thành công!', 'success');
                            setTimeout(() => location.reload(), 1000);
                        } else {
                            if (window.showToast) window.showToast('Lỗi', data.message || 'Không thể hủy suất chiếu.', 'error');
                        }
                    } else {
                        const errorData = await response.json();
                        if (window.showToast) window.showToast('Lỗi', errorData.message || 'Không thể hủy suất chiếu.', 'error');
                    }
                } catch (error) {
                    console.error('Cancel error:', error);
                    if (window.showToast) window.showToast('Lỗi', 'Lỗi hệ thống khi hủy.', 'error');
                } finally {
                    btn.disabled = false;
                    btn.textContent = originalText;
                }
            });
        }

        // AJAX Pagination & Filtering
        const container = document.getElementById('showtime-list-container');
        const filterForm = document.getElementById('filterForm');

        function fetchShowTimes(url) {
            if (!container) return;
            container.style.opacity = '0.5';
            container.style.pointerEvents = 'none';
            
            const btn = document.getElementById('clearFilterBtn');
            if (btn) {
                const urlObj = new URL(url, window.location.origin);
                let hasFilter = false;
                for (const [key, value] of urlObj.searchParams.entries()) {
                    if (value && key !== 'page') { 
                        hasFilter = true; break; 
                    }
                }
                btn.classList.toggle('hidden', !hasFilter);
            }
            
            fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(res => res.text())
            .then(html => {
                container.innerHTML = html;
                window.history.pushState(null, '', url);
            })
            .catch(err => console.error("Error fetching showtimes via AJAX:", err))
            .finally(() => {
                container.style.opacity = '1';
                container.style.pointerEvents = 'auto';
            });
        }

        if (filterForm) {
            filterForm.addEventListener('submit', (e) => {
                e.preventDefault();
                const formData = new FormData(filterForm);
                const params = new URLSearchParams();
                for (const [key, value] of formData.entries()) {
                    if (value) params.append(key, value);
                }
                fetchShowTimes(`${filterForm.action}?${params.toString()}`);
            });
        }

        if (container) {
            container.addEventListener('click', (e) => {
                const link = e.target.closest('a');
                if (link && link.href && link.href.includes('/ShowTimes/Index')) {
                    e.preventDefault();
                    fetchShowTimes(link.href);
                }
            });
        }

        window.addEventListener('popstate', () => fetchShowTimes(window.location.href));

        // Initial calls
        updatePricing();
        if (window.initDropdownLabels) window.initDropdownLabels(); // Usually called by Razor
        updatePreview();
    });

    window.initDropdownLabels = function () {
        if (!movieIdInput || !roomIdInput) return;
        if (movieIdInput.value) {
            const movieBtn = document.querySelector(`#MoviePanel button[onclick*="'${movieIdInput.value}'"]`);
            if (movieBtn) document.getElementById('MovieLabel').textContent = movieBtn.textContent.trim();
        }
        if (roomIdInput.value) {
            const roomBtn = document.querySelector(`#RoomPanel button[onclick*="'${roomIdInput.value}'"]`);
            if (roomBtn) {
                document.getElementById('RoomLabel').textContent = roomBtn.textContent.trim();
                updateRoomDetail(roomIdInput.value);
            }
        }
    };
})();
