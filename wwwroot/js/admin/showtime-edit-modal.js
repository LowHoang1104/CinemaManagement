// Showtime Management Module - Edit Modal Script
// This script is intended to be executed after the modal content is loaded via AJAX.

(function () {
    const editForm = document.getElementById('EditForm');
    if (!editForm) return;

    const startTimeInput = document.getElementById('EditStartTimeInput');
    const basePriceInput = document.getElementById('EditBasePriceInput');
    const timeRangeDisplay = document.getElementById('EditTimeRangeDisplay');
    const priceStandard = document.getElementById('EditPriceStandard');
    const priceVIP = document.getElementById('EditPriceVIP');
    const priceCouple = document.getElementById('EditPriceCouple');
    const validationBar = document.getElementById('EditValidationBar');
    const submitBtn = document.getElementById('EditSubmitBtn');

    // Data passed from Razor via data attributes or global window vars
    // We expect window.editModalData to be set by the caller or inline
    const data = window.editModalData || {};
    const movieDuration = data.movieDuration || 0;
    const roomId = data.roomId;
    const showTimeId = data.showTimeId;
    const movieId = data.movieId;
    const movieTitle = data.movieTitle;
    const viewOnly = data.viewOnly === true;

    function formatCurrency(n) { 
        return new Intl.NumberFormat('en-US').format(n) + ' VNĐ'; 
    }

    function updatePricing() {
        if (!basePriceInput) return;
        const raw = basePriceInput.value.replace(/\D/g, '');
        const b = parseInt(raw) || 0;
        if (priceStandard) priceStandard.textContent = formatCurrency(b);
        if (priceVIP) priceVIP.textContent = formatCurrency(b + 5000);
        if (priceCouple) priceCouple.textContent = formatCurrency(b * 2);
    }

    async function checkOverlap() {
        if (!startTimeInput || !validationBar || !submitBtn) return;
        const s = startTimeInput.value;
        if (!s) { 
            validationBar.classList.add('hidden'); 
            submitBtn.disabled = true; 
            window.lastEditOverlapData = null; 
            return; 
        }
        try {
            const res = await fetch(`/ShowTimes/CheckOverlap?roomId=${roomId}&startAt=${s}&movieId=${movieId}&excludeId=${showTimeId}`);
            if (res.ok) {
                const d = await res.json();
                window.lastEditOverlapData = d;
                validationBar.classList.remove('hidden');
                if (d.isPast) {
                    window.lastEditOverlapData = null;
                    validationBar.className = "rounded-xl p-4 border border-orange-200 bg-orange-50 text-orange-700 text-sm flex items-start gap-3";
                    validationBar.innerHTML = `
                        <svg class="h-5 w-5 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
                        </svg>
                        <div><span class="font-bold">Không hợp lệ:</span> Thời gian bắt đầu suất chiếu không thể nằm trong quá khứ.</div>
                    `;
                    submitBtn.disabled = true;
                } else if (d.isOverlapping) {
                    validationBar.className = "rounded-xl p-4 border border-red-200 bg-red-50 text-red-700 text-sm flex items-start gap-3";
                    validationBar.innerHTML = `
                        <svg class="h-5 w-5 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
                        </svg>
                        <div class="flex-1">
                            <span class="font-bold">Cảnh báo:</span> Phát hiện xung đột thời gian với suất chiếu khác.<br/>
                            <span class="text-xs font-semibold underline cursor-pointer mt-1 block" onclick="document.getElementById('EditForm').requestSubmit()">
                                Bấm 'Cập nhật' để xem chi tiết >>
                            </span>
                        </div>
                    `;
                    submitBtn.disabled = false;
                } else {
                    window.lastEditOverlapData = null;
                    validationBar.className = "rounded-xl p-4 border border-green-200 bg-green-50 text-green-700 text-sm flex items-center gap-2";
                    validationBar.innerHTML = `
                        <svg class="h-5 w-5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7"/>
                        </svg>
                        <span>Không có xung đột thời gian</span>
                    `;
                    submitBtn.disabled = false;
                }
            }
        } catch (e) {
            console.error(e);
        }
    }

    function updatePreview() {
        if (!startTimeInput || !timeRangeDisplay) return;
        const s = startTimeInput.value;
        if (s && movieDuration > 0) {
            const start = new Date(s), end = new Date(start.getTime() + movieDuration * 60000), cl = new Date(end.getTime() + 15 * 60000);
            const f = (dt) => dt.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', hour12: false });
            timeRangeDisplay.textContent = `${f(start)} - ${f(end)} (Xong dọn dẹp: ${f(cl)})`;
        } else if (s) {
            timeRangeDisplay.textContent = "Không xác định được thời lượng phim";
        } else {
            timeRangeDisplay.textContent = '--:--';
        }
        
        if (s && !viewOnly) checkOverlap();
        else { 
            if (validationBar) validationBar.classList.add('hidden'); 
            if (submitBtn && !viewOnly) submitBtn.disabled = !s; 
            window.lastEditOverlapData = null; 
        }
    }

    function handleConflictModal() {
        const lastOverlap = window.lastEditOverlapData;
        if (lastOverlap && lastOverlap.isOverlapping) {
            const list = document.getElementById('ConflictList');
            if (!list) return;
            list.innerHTML = '';
            document.getElementById('ConflictSubtitle').textContent = `Suất chiếu "${movieTitle}" bị trùng thời gian với ${lastOverlap.conflicts.length} suất chiếu đã tồn tại.`;
            document.getElementById('ConflictNewMovieName').textContent = movieTitle;
            const s = startTimeInput.value;
            if (s) {
                const sD = new Date(s), eD = new Date(sD.getTime() + movieDuration * 60000);
                const f = (dt) => dt.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', hour12: false });
                document.getElementById('ConflictNewTime').textContent = `${f(sD)} - ${f(eD)}`;
                document.getElementById('ConflictNewDate').textContent = sD.toLocaleDateString('vi-VN', { weekday: 'short', year: 'numeric', month: '2-digit', day: '2-digit' });
                document.getElementById('ConflictNewShowBlock').classList.remove('hidden');
            }
            lastOverlap.conflicts.forEach(c => {
                list.innerHTML += `
                    <div class="bg-white rounded-xl border border-red-200/60 p-4 shadow-sm">
                        <h4 class="font-bold text-slate-800">${c.movieTitle}</h4>
                        <div class="flex items-center gap-2 mt-1.5 text-slate-600 text-sm">
                            <svg class="w-4 h-4 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                            ${c.startTime} - ${c.endTime}
                        </div>
                        <div class="text-slate-500 text-xs mt-1">Thời lượng: ${c.duration} phút + 15 phút dọn dẹp</div>
                    </div>
                `;
            });
            if (window.toggleConflictModal) window.toggleConflictModal(true);
            return true;
        }
        return false;
    }

    // Initialize events
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

    if (viewOnly) {
        editForm.addEventListener('submit', e => e.preventDefault());
    } else {
        editForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            if (handleConflictModal()) return;

            try {
                const formData = new FormData(editForm);
                const response = await fetch(editForm.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value,
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                // Xử lý JSON (Success hoặc Managed Error)
                const contentType = response.headers.get("content-type");
                if (contentType && contentType.includes("application/json")) {
                    const data = await response.json();
                    if (data.success) {
                        if (window.toggleEditModal) window.toggleEditModal(false);
                        if (window.showToast) window.showToast('Thành công', data.message || 'Cập nhật thành công', 'success');
                        setTimeout(() => location.reload(), 1000);
                        return;
                    } else {
                        if (window.showToast) window.showToast('Lỗi', data.message || 'Không thể cập nhật.', 'error');
                        return;
                    }
                }

                if (response.redirected) {
                    window.location.href = response.url;
                    return;
                }

                // Fallback cho PartialView (ModelState errors) hoặc status 400 html
                if (response.ok || response.status === 400) {
                    const html = await response.text();
                    const body = document.getElementById('EditModalBody');
                    body.innerHTML = html;
                    if (window.executeScripts) window.executeScripts(body);
                } else {
                    if (window.showToast) window.showToast('Lỗi', 'Lỗi hệ thống khi lưu.', 'error');
                }
            } catch (error) {
                console.error('Submit error:', error);
                if (window.showToast) window.showToast('Lỗi', 'Lỗi hệ thống khi lưu.', 'error');
            }
        });
    }

    // Initial state
    updatePricing();
    updatePreview();
    if (viewOnly && validationBar) validationBar.classList.add('hidden');
})();
