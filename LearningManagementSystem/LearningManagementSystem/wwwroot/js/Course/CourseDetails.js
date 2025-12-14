// wwwroot/js/CourseDetails.js
document.addEventListener('DOMContentLoaded', function () {
    const studyTimers = new Map();
    // Đồng bộ chiều cao cho tất cả các khung bài học
    function syncAllLessonContentHeights() {
        const sidebar = document.querySelector('.syllabus-sidebar .bg-white');
        const allLessonWrappers = document.querySelectorAll('.lesson-content-main-wrapper');
        
        if (sidebar && allLessonWrappers.length > 0) {
            const sidebarHeight = sidebar.offsetHeight;
            // Áp dụng cùng chiều cao cho tất cả các khung
            allLessonWrappers.forEach(wrapper => {
                wrapper.style.height = `${sidebarHeight}px`;
                wrapper.style.minHeight = `${sidebarHeight}px`;
            });
        }
    }

    // Gọi khi tải trang và khi resize
    setTimeout(syncAllLessonContentHeights, 100);
    window.addEventListener('resize', () => {
        setTimeout(syncAllLessonContentHeights, 100);
    });
    
    // Gọi lại sau khi tab lesson được chuyển
    const lessonTabs = document.querySelectorAll('[data-bs-toggle="pill"]');
    lessonTabs.forEach(tab => {
        tab.addEventListener('shown.bs.tab', () => {
            setTimeout(syncAllLessonContentHeights, 100);
        });
    });

    initStudyTimers();
    initLessonTranslations();

    function getActiveLessonId() {
        const activeTab = document.querySelector('#lesson-pills-tabContent .tab-pane.active');
        if (activeTab) {
            const lessonContent = activeTab.querySelector('[data-lesson-id]');
            if (lessonContent) {
                return lessonContent.getAttribute('data-lesson-id');
            }
            // Try to extract from tab ID
            const tabId = activeTab.id;
            const match = tabId.match(/lesson-content-(.+)/);
            if (match) {
                return match[1];
            }
        }
        return null;
    }

    function initStudyTimers() {
        const cards = document.querySelectorAll('.study-timer-card');
        cards.forEach(card => {
            const courseId = card.dataset.courseId;
            if (!courseId) return;

            const duration = parseInt(card.dataset.duration || '1500', 10);
            const display = card.querySelector('.study-timer-display');
            const toggleBtn = card.querySelector('.study-timer-toggle');
            const resetBtn = card.querySelector('.study-timer-reset');
            const logBtn = card.querySelector('.study-timer-log');
            const infoEl = card.querySelector('.study-timer-session-info');

            if (!display || !toggleBtn || !resetBtn || !logBtn) return;

            const state = {
                lessonId: null, // Will be set dynamically from active tab
                courseId,
                duration,
                remaining: duration,
                elapsed: 0,
                isRunning: false,
                intervalId: null,
                sessionStart: null,
                lastTick: null,
                display,
                toggleBtn,
                resetBtn,
                logBtn,
                infoEl,
                pendingPayload: null
            };

            studyTimers.set(courseId, state);
            display.textContent = formatTimerDisplay(state.remaining);

            toggleBtn.addEventListener('click', () => {
                state.lessonId = getActiveLessonId();
                handleToggleTimer(state);
            });
            resetBtn.addEventListener('click', () => resetTimerState(state));
            logBtn.addEventListener('click', () => {
                state.lessonId = getActiveLessonId();
                finalizeStudySession(state, false);
            });
        });

        const refreshBtn = document.querySelector('.study-stats-refresh');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => refreshStudyStats(refreshBtn.dataset.courseId));
        }

        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                studyTimers.forEach(state => {
                    if (state.isRunning) {
                        pauseTimer(state);
                    }
                });
            }
        });
    }

    function initLessonTranslations() {
        const widgets = document.querySelectorAll('.lesson-translation-widget');
        widgets.forEach(widget => {
            const options = widget.querySelectorAll('.translation-option');
            const closeBtn = widget.querySelector('.translation-close');
            options.forEach(btn => {
                btn.addEventListener('click', () => handleLessonTranslation(widget, btn));
            });
            closeBtn?.addEventListener('click', () => {
                const result = widget.querySelector('.translation-result');
                if (result) {
                    result.setAttribute('hidden', 'hidden');
                }
            });
        });
    }

    function handleLessonTranslation(widget, trigger) {
        const lessonId = widget.dataset.lessonId;
        const courseId = widget.dataset.courseId;
        const langCode = trigger.dataset.langCode;
        const langName = trigger.dataset.langName;

        if (!lessonId || !courseId || !langCode) return;

        const resultBox = widget.querySelector('.translation-result');
        const targetLabel = widget.querySelector('.translation-target-label');
        const contentBox = widget.querySelector('.translation-content');

        if (!resultBox || !contentBox) return;

        resultBox.removeAttribute('hidden');
        contentBox.innerHTML = `
            <div class="translation-loading">
                <span class="spinner-border spinner-border-sm text-primary" role="status"></span>
                <span>Đang dịch sang ${langName}...</span>
            </div>`;
        if (targetLabel) {
            targetLabel.textContent = `Ngôn ngữ đích: ${langName}`;
        }

        const sourceEl = document.querySelector(`#lesson-content-${lessonId} .lesson-content-formatted`);
        let plainText = '';
        
        if (sourceEl) {
            // Lấy text thuần túy, loại bỏ các ký tự không cần thiết
            plainText = sourceEl.innerText || sourceEl.textContent || '';
            // Loại bỏ các dòng trống thừa và trim
            plainText = plainText.replace(/\n\s*\n\s*\n/g, '\n\n').trim();
            // Loại bỏ các khoảng trắng thừa
            plainText = plainText.replace(/\s+/g, ' ');
            // Giữ lại các xuống dòng hợp lý
            plainText = plainText.replace(/\n /g, '\n');
        }

        if (!plainText) {
            contentBox.innerHTML = '<div class="text-danger small">Không tìm thấy nội dung để dịch.</div>';
            return;
        }

        trigger.disabled = true;

        fetch('/AITutor/TranslateLesson', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getToken()
            },
            body: JSON.stringify({
                courseId,
                lessonId,
                targetLanguage: langCode,
                text: plainText
            })
        })
            .then(async response => {
                if (!response.ok) {
                    const message = await response.text();
                    throw new Error(message || 'Không thể dịch nội dung.');
                }
                return response.json();
            })
            .then(data => {
                let translatedText = data?.translation ?? '';
                
                // Làm sạch kết quả dịch - loại bỏ các phần không cần thiết
                if (translatedText) {
                    // Loại bỏ các prefix như "Bản dịch:", "Translation:", etc.
                    translatedText = translatedText.replace(/^(Bản dịch|Translation|Dịch|Translated):\s*/i, '').trim();
                    
                    // Nếu văn bản gốc ngắn (có thể là câu hỏi) nhưng kết quả dài bất thường
                    // thì chỉ lấy câu đầu tiên để tránh lấy phần giải thích - đặc biệt quan trọng với tiếng Anh
                    if (plainText.length < 50 && translatedText.length > plainText.length * 2) {
                        // Tách theo dấu chấm, xuống dòng, hoặc dấu hai chấm - lấy câu đầu tiên
                        const sentences = translatedText.split(/[\.\n:]/);
                        let firstSentence = sentences[0].trim();
                        
                        // Nếu câu đầu tiên vẫn quá dài, thử tách theo dấu phẩy
                        if (firstSentence.length > plainText.length * 3 && firstSentence.includes(',')) {
                            firstSentence = firstSentence.split(',')[0].trim();
                        }
                        
                        if (firstSentence && firstSentence.length <= plainText.length * 3) {
                            translatedText = firstSentence;
                        } else if (sentences.length > 1) {
                            // Nếu vẫn không hợp lý, thử lấy phần ngắn nhất từ các câu
                            const validSentences = sentences
                                .filter(s => s.trim() && s.trim().length <= plainText.length * 3)
                                .sort((a, b) => a.trim().length - b.trim().length);
                            if (validSentences.length > 0) {
                                translatedText = validSentences[0].trim();
                            }
                        }
                    }
                    
                    // Loại bỏ các dòng trống thừa
                    translatedText = translatedText.replace(/\n\s*\n\s*\n/g, '\n\n');
                    // Trim toàn bộ
                    translatedText = translatedText.trim();
                }
                
                if (targetLabel && data?.targetLanguage) {
                    targetLabel.textContent = `Ngôn ngữ đích: ${data.targetLanguage}`;
                }
                
                if (translatedText) {
                    // Sử dụng textContent để tránh render HTML, nhưng giữ lại xuống dòng
                    contentBox.textContent = translatedText;
                    // Thêm white-space để giữ format
                    contentBox.style.whiteSpace = 'pre-wrap';
                } else {
                    contentBox.innerHTML = '<div class="text-warning small">AI không trả về bản dịch.</div>';
                }
            })
            .catch(err => {
                contentBox.innerHTML = `<div class="text-danger small">Không thể dịch nội dung. ${err.message || ''}</div>`;
            })
            .finally(() => {
                trigger.disabled = false;
            });
    }

    function handleToggleTimer(state) {
        if (state.isRunning) {
            pauseTimer(state);
        } else {
            startTimer(state);
        }
    }

    function startTimer(state) {
        if (state.isRunning) return;
        if (!state.lessonId) {
            if (state.infoEl) state.infoEl.textContent = 'Hãy mở một bài học trước khi bắt đầu Pomodoro.';
            return;
        }
        if (!state.sessionStart) {
            state.sessionStart = new Date();
        }
        state.lastTick = Date.now();
        state.intervalId = setInterval(() => tickTimer(state), 1000);
        state.isRunning = true;
        state.toggleBtn.innerHTML = '<i class="bi bi-pause-fill me-1"></i>Tạm dừng';
        state.resetBtn.disabled = false;
        state.logBtn.disabled = false;
        if (state.infoEl) {
            state.infoEl.textContent = 'Đang đếm thời gian học...';
        }
    }

    function pauseTimer(state) {
        if (!state.isRunning) return;
        stopTimer(state);
        state.toggleBtn.innerHTML = '<i class="bi bi-play-fill me-1"></i>Tiếp tục';
        if (state.infoEl) {
            state.infoEl.textContent = 'Đã tạm dừng. Nhấn Tiếp tục để học tiếp.';
        }
    }

    function stopTimer(state) {
        if (state.intervalId) {
            clearInterval(state.intervalId);
            state.intervalId = null;
        }
        state.isRunning = false;
        state.lastTick = null;
    }

    function tickTimer(state) {
        if (!state.isRunning) return;
        const now = Date.now();
        const delta = Math.floor((now - state.lastTick) / 1000);
        if (delta <= 0) return;
        if (delta > 5) {
            pauseTimer(state);
            if (state.infoEl) {
                state.infoEl.textContent = 'Timer đã tạm dừng do chuyển tab quá lâu. Nhấn Tiếp tục để tiếp tục học.';
            }
            return;
        }
        state.lastTick += delta * 1000;
        state.elapsed += delta;
        state.remaining = Math.max(0, state.remaining - delta);
        state.display.textContent = formatTimerDisplay(state.remaining);

        if (state.remaining <= 0) {
            finalizeStudySession(state, true);
        }
    }

    function resetTimerState(state) {
        stopTimer(state);
        state.remaining = state.duration;
        state.elapsed = 0;
        state.sessionStart = null;
        state.pendingPayload = null;
        state.display.textContent = formatTimerDisplay(state.remaining);
        state.toggleBtn.innerHTML = '<i class="bi bi-play-fill me-1"></i>Bắt đầu';
        state.resetBtn.disabled = true;
        state.logBtn.disabled = true;
        if (state.infoEl) {
            state.infoEl.textContent = 'Thời gian học của bạn sẽ được lưu cho bài này.';
        }
    }

    function finalizeStudySession(state, autoComplete) {
        if (state.elapsed <= 0) {
            if (state.infoEl) {
                state.infoEl.textContent = 'Chưa có thời gian học để lưu.';
            }
            return;
        }
        if (!state.lessonId) {
            if (state.infoEl) state.infoEl.textContent = 'Không xác định được bài học. Hãy mở bài muốn lưu rồi thử lại.';
            return;
        }

        stopTimer(state);
        state.toggleBtn.innerHTML = '<i class="bi bi-play-fill me-1"></i>Tiếp tục';
        logStudySession(state, autoComplete);
    }

    function logStudySession(state, autoComplete) {
        if (!state.lessonId) {
            if (state.infoEl) state.infoEl.textContent = 'Không xác định được bài học. Không thể lưu.';
            return;
        }
        const payload = {
            courseId: state.courseId,
            lessonId: state.lessonId,
            durationSeconds: state.elapsed,
            startedAt: state.sessionStart ? new Date(state.sessionStart).toISOString() : null,
            endedAt: new Date().toISOString(),
            note: null
        };

        const originalLabel = state.logBtn.innerHTML;
        state.pendingPayload = payload;
        state.logBtn.disabled = true;
        state.logBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
        if (state.infoEl) {
            state.infoEl.textContent = autoComplete ? 'Pomodoro hoàn tất! Đang lưu phiên học...' : 'Đang lưu phiên học...';
        }

        fetch('/StudyTimer/LogSession', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getToken()
            },
            body: JSON.stringify(payload)
        })
            .then(r => {
                if (!r.ok) {
                    return r.text().then(t => { throw new Error(t || 'Không thể lưu phiên học'); });
                }
                return r.json();
            })
            .then(stats => {
                const minutes = Math.max(1, Math.round(state.elapsed / 60));
                resetTimerState(state);
                if (state.infoEl) {
                    state.infoEl.textContent = `Đã lưu ${minutes} phút học tập.`;
                }
                state.logBtn.innerHTML = originalLabel;
                state.pendingPayload = null;
                updateStudyStatsUI(stats);
            })
            .catch(err => {
                console.error(err);
                state.logBtn.disabled = false;
                state.logBtn.innerHTML = originalLabel;
                if (state.infoEl) {
                    state.infoEl.textContent = 'Lưu phiên học thất bại, vui lòng thử lại.';
                }
            });
    }

    function refreshStudyStats(courseId) {
        if (!courseId) return;
        fetch(`/StudyTimer/GetStats?courseId=${courseId}`)
            .then(r => {
                if (!r.ok) {
                    throw new Error('Không thể làm mới thống kê.');
                }
                return r.json();
            })
            .then(stats => updateStudyStatsUI(stats))
            .catch(err => {
                console.error(err);
            });
    }

    function formatTime(minutes) {
        const hours = Math.floor(minutes / 60);
        const mins = minutes % 60;
        if (hours > 0) {
            return `${hours} giờ ${mins} phút`;
        } else {
            return `${mins} phút`;
        }
    }

    function updateStudyStatsUI(stats) {
        if (!stats) return;
        const totalEl = document.getElementById('study-stat-total');
        const weeklyEl = document.getElementById('study-stat-weekly');
        const streakEl = document.getElementById('study-stat-streak');
        const sessionsEl = document.getElementById('study-stat-sessions');
        const lastEl = document.getElementById('study-stat-last');

        if (totalEl) {
            const minutes = Math.max(0, Math.round((stats.totalSeconds || 0) / 60));
            totalEl.textContent = formatTime(minutes);
        }
        if (weeklyEl) {
            const minutes = Math.max(0, Math.round((stats.weeklySeconds || 0) / 60));
            weeklyEl.textContent = formatTime(minutes);
        }
        if (streakEl) {
            const streak = stats.currentStreakDays || 0;
            streakEl.textContent = `${streak} ngày`;
        }
        if (sessionsEl) {
            const sessions = parseInt(stats.totalSessions) || 0;
            sessionsEl.textContent = `${sessions} phiên`;
        }

        if (lastEl) {
            if (stats.lastStudiedAt) {
                const dt = new Date(stats.lastStudiedAt);
                lastEl.textContent = dt.toLocaleString('vi-VN', { hour12: false });
            } else {
                lastEl.textContent = 'Chưa có';
            }
        }
    }

    function formatTimerDisplay(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
    }


    // Hiệu ứng hiển thị khi tải trang
    window.addEventListener('DOMContentLoaded', () => {
        document.querySelector('.course-info-card')?.classList.add('show');
        document.querySelector('.course-image-card')?.classList.add('show');
        document.querySelector('.progress-card')?.classList.add('show');
        document.querySelector('.comment-form-card')?.classList.add('show');
    });

    // Hàm ẩn lời nhắc đăng ký
    function hideEnrollPrompt(courseId) {
        const enrollPrompt = document.querySelector(`#enrollForm-${courseId}`)?.parentElement;
        if (enrollPrompt) enrollPrompt.style.display = 'none';
    }

    // Xử lý video
    // Gắn handler cho các video bài học
    const videos = document.querySelectorAll('video[data-lesson-id]');
    if (!videos || videos.length === 0) {
        console.warn('Không tìm thấy video nào với class "custom-video".');
        //return;
    }
    videos.forEach(video => {
        video.addEventListener('play', function () {
            const lessonId = this.dataset.lessonId;
            if (!lessonId) return;
            // Cập nhật UI ngay: vàng = đang xem
            setLessonUiState(lessonId, false);
            updateProgress(lessonId, false);
        });
        video.addEventListener('ended', function () {
            const lessonId = this.dataset.lessonId;
            if (!lessonId) return;
            // Cập nhật UI ngay: xanh = đã hoàn thành
            setLessonUiState(lessonId, true);
            updateProgress(lessonId, true);
        });
    });

    // Cập nhật tiến trình
    function updateProgress(lessonId, completionStatus) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) { alert('Không tìm thấy token.'); return; }

        const formData = new FormData();
        formData.append('lessonId', lessonId);
        formData.append('completionStatus', completionStatus);
        formData.append('__RequestVerificationToken', token);

        fetch('/Progress/UpdateProgress', { method: 'POST', body: formData })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    // Đồng bộ lại UI sidebar nếu cần
                    setLessonUiState(lessonId, completionStatus);
                    updateProgressSection();
                }
            })
            .catch(console.error);
    }

    // Cập nhật biểu tượng trạng thái bài học ở sidebar: vàng (đang xem), xanh (hoàn thành)
    function setLessonUiState(lessonId, isCompleted) {
        const tabBtn = document.getElementById(`lesson-tab-${lessonId}`);
        if (!tabBtn) return;
        const statusWrap = tabBtn.querySelector('span.me-2');
        const iconEl = statusWrap?.querySelector('i');
        if (!statusWrap || !iconEl) return;

        // Reset classes
        statusWrap.classList.remove('text-warning', 'text-success', 'text-muted');
        iconEl.classList.remove('bi-check-circle-fill', 'bi-clock-fill', 'bi-circle');
        tabBtn.classList.toggle('lesson-completed', !!isCompleted);

        if (isCompleted) {
            statusWrap.classList.add('text-success');
            iconEl.classList.add('bi', 'bi-check-circle-fill');
        } else {
            statusWrap.classList.add('text-warning');
            iconEl.classList.add('bi', 'bi-clock-fill');
        }
    }

    function updateProgressSection() {
        const courseId = document.querySelector('.course-details-container')?.dataset.courseId;
        if (!courseId) return;

        fetch(`/Course/GetProgress?courseId=${courseId}`)
            .then(r => r.json())
            .then(data => {
                if (data.progressPercent !== undefined) {
                    const circle = document.querySelector('.progress-circle circle:nth-child(2)');
                    const percent = document.querySelector('.percentage');
                    const completed = document.querySelector('.stat-item:nth-child(1) .stat-value');
                    const remaining = document.querySelector('.stat-item:nth-child(2) .stat-value');
                    if (circle && percent && completed && remaining) {
                        circle.setAttribute('stroke-dasharray', `${data.progressPercent}, 100`);
                        percent.textContent = `${data.progressPercent}%`;
                        completed.textContent = data.completedLessons || 0;
                        remaining.textContent = data.remainingLessons || 0;
                    }
                }
            });
    }

    // Đánh giá sao
    const starRating = document.getElementById('commentRating');
    if (starRating) {
        const stars = starRating.querySelectorAll('input[type="radio"]');
        const labels = starRating.querySelectorAll('label');
        const ratingInput = document.getElementById('ratingValue');
        let currentValue = parseInt(ratingInput?.value || '0', 10) || 0;

        function applyRating(value) {
            currentValue = value;
            labels.forEach(label => {
                const starValue = parseInt(label.dataset.value || '0', 10);
                label.style.color = starValue <= currentValue ? '#fbbc05' : '#ddd';
            });
            if (ratingInput) ratingInput.value = currentValue;
        }

        // Hover: chỉ preview
        labels.forEach(label => {
            label.addEventListener('mouseover', function () {
                const value = parseInt(this.dataset.value || '0', 10) || 0;
                labels.forEach(l => {
                    const v = parseInt(l.dataset.value || '0', 10);
                    l.style.color = v <= value ? '#fbbc05' : '#ddd';
                });
            });

            label.addEventListener('mouseout', function () {
                applyRating(currentValue);
            });

            label.addEventListener('click', function () {
                const value = parseInt(this.dataset.value || '0', 10) || 0;
                // set radio tương ứng (để form gửi đúng name/value)
                const forId = this.getAttribute('for');
                const radio = document.getElementById(forId);
                if (radio) radio.checked = true;
                applyRating(value);
            });
        });

        // Khởi tạo trạng thái
        applyRating(currentValue);
    }

    // ===== XỬ LÝ SỬA & XÓA BÌNH LUẬN (ĐÃ SỬA 100%) =====
    function getCourseId() {
        return document.querySelector('.container-custom')?.dataset.courseId || '';
    }

    function getToken() {
        return document.querySelector('#hiddenForm input[name="__RequestVerificationToken"]')?.value ||
            document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    document.addEventListener('click', function (e) {
        // === NÚT SỬA ===
        const editBtn = e.target.closest('.btn-edit');
        if (editBtn) {
            e.preventDefault();
            const commentId = editBtn.dataset.commentId;
            const content = editBtn.dataset.content || '';
            const rating = parseInt(editBtn.dataset.rating) || 0;

            const item = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
            if (!item || item.querySelector('.edit-comment-form')) return;

            const commentContent = item.querySelector('.comment-content');
            const textDiv = commentContent.querySelector('.comment-text');
            const actionsDiv = commentContent.querySelector('.comment-actions');

            // Tạo form sửa – ĐÃ SỬA DẤU ` ĐÓNG ĐÚNG
            const form = document.createElement('form');
            form.className = 'edit-comment-form';
            form.innerHTML = `
            <input type="hidden" name="id" value="${getCourseId()}" />
            <input type="hidden" name="commentId" value="${commentId}" />
            <input type="hidden" name="rating" id="editRating-${commentId}" value="${rating}" />
            <div class="star-rating mb-2" id="editStarRating-${commentId}">
                ${[5, 4, 3, 2, 1].map(i => `
                    <input type="radio" id="editStar${i}-${commentId}" name="star-${commentId}" value="${i}" ${rating === i ? 'checked' : ''} />
                    <label for="editStar${i}-${commentId}"><i class="bi bi-star-fill"></i></label>
                `).join('')}
            </div>
            <textarea class="form-control form-control-sm mb-2" name="content" rows="3" required>${content}</textarea>
            <div class="d-flex gap-2">
                <button type="submit" class="btn btn-success btn-sm">Lưu</button>
                <button type="button" class="btn btn-secondary btn-sm cancel-edit-btn">Hủy</button>
            </div>
        `; // ← ĐÓNG ĐÚNG CHỖ

            textDiv.style.display = 'none';
            actionsDiv.style.display = 'none';
            commentContent.appendChild(form);

            // === Xử lý sao trong form sửa ===
            const starRatingDiv = form.querySelector(`#editStarRating-${commentId}`);
            const inputs = starRatingDiv.querySelectorAll('input[type="radio"]');
            const labels = starRatingDiv.querySelectorAll('label');
            const ratingInput = form.querySelector(`#editRating-${commentId}`);

            function highlight(value) {
                labels.forEach((label, i) => {
                    const starValue = 5 - i;
                    label.style.color = starValue <= value ? '#fbbc05' : '#ddd';
                });
                if (ratingInput) ratingInput.value = value;
            }

            inputs.forEach(input => {
                input.addEventListener('change', () => {
                    if (input.checked) highlight(parseInt(input.value));
                });
            });

            labels.forEach(label => {
                label.addEventListener('click', () => {
                    const radio = document.getElementById(label.getAttribute('for'));
                    if (radio) {
                        radio.checked = true;
                        highlight(parseInt(radio.value));
                    }
                });

                label.addEventListener('mouseover', () => {
                    const match = label.getAttribute('for').match(/editStar(\d)/);
                    if (match) highlight(parseInt(match[1]));
                });

                label.addEventListener('mouseout', () => {
                    const checked = starRatingDiv.querySelector('input:checked');
                    highlight(checked ? parseInt(checked.value) : 0);
                });
            });

            highlight(rating);
            return;
        }

        // === NÚT HỦY ===
        const cancelBtn = e.target.closest('.cancel-edit-btn');
        if (cancelBtn) {
            e.preventDefault();
            const form = cancelBtn.closest('.edit-comment-form');
            const item = form.closest('.comment-item');
            form.remove();
            const textDiv = item.querySelector('.comment-text');
            const actionsDiv = item.querySelector('.comment-actions');
            if (textDiv) textDiv.style.display = 'block';
            if (actionsDiv) actionsDiv.style.display = 'flex';
            return;
        }

        // === NÚT XÓA ===
        const deleteBtn = e.target.closest('.btn-delete');
        if (deleteBtn) {
            e.preventDefault();
            if (!confirm('Bạn có chắc chắn muốn xóa bình luận này?')) return;

            const commentId = deleteBtn.dataset.commentId;
            const fd = new FormData();
            fd.append('id', getCourseId());
            fd.append('commentId', commentId);
            fd.append('__RequestVerificationToken', getToken());

            fetch('/Course/DeleteComment', {
                method: 'POST',
                body: fd
            })
                .then(r => {
                    if (!r.ok) throw new Error('Xóa thất bại');
                    return r.text();
                })
                .then(() => location.reload())
                .catch(err => {
                    console.error('Delete error:', err);
                    alert('Lỗi khi xóa bình luận');
                });
        }
    });

    // === SUBMIT FORM SỬA ===
    document.addEventListener('submit', function (e) {
        if (e.target.classList.contains('edit-comment-form')) {
            e.preventDefault();
            const fd = new FormData(e.target);
            fd.append('__RequestVerificationToken', getToken());

            fetch('/Course/EditComment', {
                method: 'POST',
                body: fd
            })
                .then(r => {
                    if (!r.ok) throw new Error('Sửa thất bại');
                    return r.text();
                })
                .then(() => location.reload())
                .catch(err => {
                    console.error('Edit error:', err);
                    alert('Lỗi khi sửa bình luận');
                });
        }
    });

    // Nộp bài tập
    document.querySelectorAll('.assignment-form').forEach(form => {
        form.addEventListener('submit', e => {
            e.preventDefault();
            const fd = new FormData(form);
            fd.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);
            fetch(form.action, { method: 'POST', body: fd })
                .then(() => location.reload());
        });
    });

    // Fix scroll reset sau khi gửi bình luận
    const commentForm = document.getElementById('commentForm');
    if (commentForm) {
        commentForm.addEventListener('submit', function(e) {
            // Lưu scroll position vào sessionStorage
            const scrollPosition = window.pageYOffset || document.documentElement.scrollTop;
            const scrollTarget = this.dataset.scrollTarget || 'comments-section';
            sessionStorage.setItem('commentScrollPosition', scrollPosition.toString());
            sessionStorage.setItem('commentScrollTarget', scrollTarget);
        });
    }

    // Restore scroll position sau khi reload
    window.addEventListener('load', function() {
        const savedPosition = sessionStorage.getItem('commentScrollPosition');
        const scrollTarget = sessionStorage.getItem('commentScrollTarget');
        
        if (savedPosition && scrollTarget) {
            // Đợi một chút để đảm bảo DOM đã render
            setTimeout(() => {
                const targetElement = document.getElementById(scrollTarget) || document.querySelector(`.${scrollTarget}`);
                if (targetElement) {
                    targetElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    window.scrollTo(0, parseInt(savedPosition));
                } else {
                    window.scrollTo(0, parseInt(savedPosition));
                }
                // Xóa sau khi đã restore
                sessionStorage.removeItem('commentScrollPosition');
                sessionStorage.removeItem('commentScrollTarget');
            }, 100);
        }
    });
});

// ===== Compact helpers and AI functions (kept compatible with existing IDs) =====
function getToken() {
    return document.querySelector('#hiddenForm input[name="__RequestVerificationToken"]')?.value || '';
}

function aiLoadStats(lessonId) {
    const statsEl = document.getElementById(`ai-quiz-stats-${lessonId}`);
    if (!statsEl) return;
    const courseId = document.querySelector('.course-details-container')?.dataset.courseId;
    if (!courseId) return;

    fetch(`/AITutor/GetStats?courseId=${encodeURIComponent(courseId)}`, {
        method: 'GET',
        headers: { 'RequestVerificationToken': getToken() }
    })
        .then(r => r.ok ? r.json() : null)
        .then(d => {
            if (!d) return;
            if (d.total && d.total > 0) {
                statsEl.textContent = `Đã luyện ${d.total} câu, chính xác ${d.accuracy}% cho khóa này.`;
            } else {
                statsEl.textContent = 'Chưa có dữ liệu luyện tập nào.';
            }
        })
        .catch(() => { });
}

function switchAITab(e, id) {
    const parent = e.target.closest('.ai-section') || e.target.closest('.ai-tutor-compact');
    if (!parent) return;
    parent.querySelectorAll('.ai-content').forEach(c => c.classList.remove('active'));
    parent.querySelectorAll('.ai-tab').forEach(b => b.classList.remove('active'));
    document.getElementById(id)?.classList.add('active');
    e.target.classList.add('active');

    if (id && id.startsWith('quiz-')) {
        const lessonId = id.replace('quiz-', '');
        aiLoadStats(lessonId);
    }
}

// resolve helper to support both ai-q- and ai-question-
function getElByEitherId(prefix, lessonId) {
    return document.getElementById(`${prefix}-${lessonId}`) || document.getElementById(`${prefix === 'ai-q' ? 'ai-question' : prefix}-${lessonId}`);
}

function aiAsk(courseId, lessonId) {
    const qInput = getElByEitherId('ai-q', lessonId);
    const ans = document.getElementById(`ai-answer-${lessonId}`);
    const q = qInput?.value?.trim();
    if (!q) return;
    if (q.toLowerCase().includes('tóm tắt') || q.toLowerCase() === 'summary') {
        fetch('/AITutor/SummarizeLesson', { method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() }, body: JSON.stringify({ courseId, lessonId }) })
            .then(r => r.json()).then(d => ans.innerText = d.summary || 'Không có tóm tắt.');
        return;
    }
    // Lấy nội dung bài học từ phần lesson-content-formatted, không phải video
    const contentEl = document.querySelector(`#lesson-content-${lessonId} .lesson-content-formatted`);
    const context = contentEl?.innerText || contentEl?.textContent || '';
    fetch('/AITutor/Ask', { method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() }, body: JSON.stringify({ courseId, question: q, lessonContext: context }) })
        .then(r => r.json()).then(d => ans.innerText = d.answer || 'Không có phản hồi.');
}

function aiGenerate(courseId, lessonId) {
    // Chặn spam tạo câu hỏi theo từng lesson
    window.__aiGenBusy = window.__aiGenBusy || {};
    if (window.__aiGenBusy[lessonId]) return;
    window.__aiGenBusy[lessonId] = true;
    const countInput = document.getElementById(`ai-count-${lessonId}`);
    const levelSelect = document.getElementById(`ai-level-${lessonId}`);
    const mcqWrap = document.getElementById(`ai-mcq-${lessonId}`);
    const qEl = document.getElementById(`ai-mcq-question-${lessonId}`);
    const optsEl = document.getElementById(`ai-mcq-options-${lessonId}`);
    const practiceEl = document.getElementById(`ai-mcq-practice-${lessonId}`);
    const correctEl = document.getElementById(`ai-mcq-correct-${lessonId}`);
    const answerEl = document.getElementById(`ai-answer-${lessonId}`);
    const summaryEl = document.getElementById(`ai-summary-${lessonId}`);
    // Lấy nội dung bài học từ phần lesson-content-formatted, không phải video
    const contentEl = document.querySelector(`#lesson-content-${lessonId} .lesson-content-formatted`);
    const lessonContext = contentEl?.innerText || contentEl?.textContent || '';

    const errorEl = document.getElementById(`ai-quiz-error-${lessonId}`);
    if (errorEl) {
        errorEl.classList.add('d-none');
        errorEl.textContent = '';
    }

    // Không dùng topic nữa, dùng nội dung bài học làm context
    let topic = ''; // Để trống, server sẽ tự động dùng lessonContext

    let count = parseInt((countInput && countInput.value) ? countInput.value : '1', 10) || 1;
    // Server giới hạn 1-10 với thông báo lỗi
    if (count > 10 || count < 1) {
        if (errorEl) {
            errorEl.textContent = 'Số lượng câu phải từ 1 đến 10.';
            errorEl.classList.remove('d-none');
        }
        window.__aiGenBusy[lessonId] = false;
        return;
    }

    const difficulty = (levelSelect?.value || 'trung bình');

    // Disable nút và hiển thị loading
    const section = document.getElementById(`quiz-${lessonId}`) || document.getElementById(`ai-tutor-${lessonId}`);
    const genBtn = section?.querySelector('.ai-btn-warning');
    if (genBtn) { genBtn.disabled = true; genBtn.classList.add('disabled'); }
    if (mcqWrap) { mcqWrap.style.display = ''; mcqWrap.innerHTML = '<div class="text-muted">Đang tạo câu hỏi...</div>'; }

    if (count > 1) {
        fetch('/AITutor/GenerateQuestions', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() },
            body: JSON.stringify({ courseId, lessonId, topic, lessonContext, count, difficulty })
        }).then(async r => {
            if (!r.ok) {
                const t = await r.text();
                throw new Error(t || 'Request failed');
            }
            return r.json();
        }).then(d => {
            if (qEl) qEl.innerText = '';
            if (optsEl) optsEl.innerHTML = '';
            if (mcqWrap) mcqWrap.style.display = 'none';
            const listElId = `ai-mcq-list-${lessonId}`;
            let listEl = document.getElementById(listElId);
            if (!listEl) {
                listEl = document.createElement('div');
                listEl.id = listElId;
                answerEl?.parentNode?.insertBefore(listEl, answerEl.nextSibling);
            }
            listEl.innerHTML = '';
            (d.items || []).forEach((it, idx) => {
                const block = document.createElement('div');
                block.className = 'mt-3 p-2 border rounded';
                const qId = `${lessonId}-multi-${idx}`;
                block.innerHTML = `
                    <div class="fw-semibold">${it.question}</div>
                    <div class="mt-2" id="opts-${qId}"></div>
                    <div class="mt-2 d-flex gap-2">
                        <button type="button" class="btn btn-success" id="btn-${qId}">Nộp đáp án</button>
                        <div class="align-self-center" id="res-${qId}"></div>
                    </div>`;
                listEl.appendChild(block);
                const opts = document.getElementById(`opts-${qId}`);
                (it.options || []).forEach(opt => {
                    const label = document.createElement('label');
                    label.className = 'd-block';
                    const radioId = `opt-${qId}-${Math.random().toString(36).slice(2)}`;
                    label.innerHTML = `<input type="radio" name="multi-${qId}" value="${(opt||'').trim().substring(0,1).toUpperCase()}" class="form-check-input me-2" id="${radioId}" /> <span>${opt}</span>`;
                    opts.appendChild(label);
                });
                const optsContainer = document.getElementById(`opts-${qId}`);
                const correctAnswer = it.correctAnswer || '';
                document.getElementById(`btn-${qId}`).onclick = () => {
                    // Kiểm tra nếu đã nộp đáp án rồi
                    if (optsContainer.dataset.submitted === 'true') {
                        return;
                    }
                    
                    const chosen = document.querySelector(`input[name="multi-${qId}"]:checked`);
                    const resEl = document.getElementById(`res-${qId}`);
                    const submitBtn = document.getElementById(`btn-${qId}`);
                    
                    if (!chosen) { 
                        resEl.innerHTML = '<span class="text-muted">Vui lòng chọn đáp án.</span>'; 
                        return; 
                    }
                    
                    const userAnswer = chosen.value;
                    const wrongAnswers = JSON.parse(optsContainer.dataset.wrongAnswers || '[]');
                    
                    // Kiểm tra đáp án đã chọn sai trước đó
                    if (wrongAnswers.includes(userAnswer)) {
                        resEl.innerHTML = '<span class="text-danger">Bạn đã chọn đáp án này và nó sai. Vui lòng chọn đáp án khác.</span>';
                        return;
                    }
                    
                    fetch('/AITutor/SubmitAnswer', {
                        method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() },
                        body: JSON.stringify({ practiceId: it.practiceId, userAnswer: userAnswer })
                    }).then(r => r.json()).then(d => {
                        const isCorrect = d.isCorrect || (userAnswer === correctAnswer);
                        optsContainer.dataset.submitted = isCorrect ? 'true' : 'false';
                        
                        const allRadios = optsContainer.querySelectorAll('input[type="radio"]');
                        
                        if (isCorrect) {
                            allRadios.forEach(radio => radio.disabled = true);
                            chosen.closest('label').classList.add('answer-correct');
                            resEl.innerHTML = '<span class="text-success fw-bold">✓ Đúng!</span>';
                            if (submitBtn) submitBtn.disabled = true;
                        } else {
                            chosen.disabled = true;
                            chosen.closest('label').classList.add('answer-wrong');
                            wrongAnswers.push(userAnswer);
                            optsContainer.dataset.wrongAnswers = JSON.stringify(wrongAnswers);
                            
                            allRadios.forEach(radio => {
                                if (radio.value === (d.correctAnswer || correctAnswer)) {
                                    radio.closest('label').classList.add('answer-correct');
                                }
                            });
                            
                            resEl.innerHTML = `<span class="text-danger fw-bold">✗ Sai. Đáp án đúng: ${d.correctAnswer || correctAnswer}</span>`;
                        }
                        
                        aiLoadStats(lessonId);
                    }).catch(() => { 
                        resEl.innerHTML = '<span class="text-danger">Chấm điểm bị lỗi.</span>'; 
                    });
                };
            });
        }).catch((err) => {
            const listElId = `ai-mcq-list-${lessonId}`;
            let listEl = document.getElementById(listElId);
            if (listEl) listEl.innerHTML = `<div class="text-danger">Không tạo được câu hỏi. ${err?.message || ''}</div>`;
        }).finally(() => {
            window.__aiGenBusy[lessonId] = false;
            if (genBtn) { genBtn.disabled = false; genBtn.classList.remove('disabled'); }
            aiLoadStats(lessonId);
        });
        return;
    }

    fetch('/AITutor/GenerateQuestion', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() },
        body: JSON.stringify({ courseId, topic, lessonContext, lessonId, difficulty })
    }).then(async r => {
        if (!r.ok) {
            const t = await r.text();
            throw new Error(t || 'Request failed');
        }
        return r.json();
    }).then(d => {
        if (!d || !d.practiceId) { throw new Error('Dữ liệu không hợp lệ'); }
        // Render UI trực tiếp vào mcqWrap thay vì tham chiếu phần tử không tồn tại trong view
        mcqWrap.style.display = '';
        const radios = (d.options || []).map(opt => {
            const id = `ai-opt-${lessonId}-${Math.random().toString(36).slice(2)}`;
            const value = (opt || '').trim().substring(0,1).toUpperCase();
            return `<label class="d-block"><input type="radio" name="ai-option-${lessonId}" value="${value}" class="form-check-input me-2" id="${id}" /> <span>${opt}</span></label>`;
        }).join('');
        mcqWrap.innerHTML = `
            <div class="fw-semibold" id="ai-mcq-question-${lessonId}">${d.question}</div>
            <div class="mt-2" id="ai-mcq-options-${lessonId}">${radios}</div>
            <input type="hidden" id="ai-mcq-practice-${lessonId}" value="${d.practiceId}" />
            <input type="hidden" id="ai-mcq-correct-${lessonId}" value="${d.correctAnswer || ''}" />
            <div class="mt-2 d-flex gap-2">
                <button type="button" class="btn btn-success" onclick="aiSubmitAnswer('${lessonId}')">Nộp đáp án</button>
                <div class="align-self-center" id="ai-mcq-result-${lessonId}"></div>
            </div>
        `;
    }).catch((err) => {
        if (mcqWrap) {
            mcqWrap.style.display = '';
            mcqWrap.innerHTML = `<div class="text-danger">Không tạo được câu hỏi. ${err?.message || ''}</div>`;
        }
    }).finally(() => {
        window.__aiGenBusy[lessonId] = false;
        if (genBtn) { genBtn.disabled = false; genBtn.classList.remove('disabled'); }
        aiLoadStats(lessonId);
    });
}

function aiSubmitAnswer(lessonId) {
    const practiceEl = document.getElementById(`ai-mcq-practice-${lessonId}`);
    const resultEl = document.getElementById(`ai-mcq-result-${lessonId}`);
    const correctEl = document.getElementById(`ai-mcq-correct-${lessonId}`);
    const optionsContainer = document.getElementById(`ai-mcq-options-${lessonId}`);
    const submitBtn = document.querySelector(`button[onclick="aiSubmitAnswer('${lessonId}')"]`);
    
    // Kiểm tra nếu đã nộp đáp án rồi
    if (optionsContainer.dataset.submitted === 'true') {
        return; // Không cho nộp lại nếu đã nộp
    }
    
    const selected = document.querySelector(`input[name="ai-option-${lessonId}"]:checked`);
    if (!selected) { 
        resultEl.innerHTML = '<span class="text-muted">Vui lòng chọn đáp án.</span>'; 
        return; 
    }
    
    const userAnswer = selected.value;
    const correctAnswer = correctEl?.value || '';
    
    // Kiểm tra đáp án đã chọn sai trước đó
    const wrongAnswers = JSON.parse(optionsContainer.dataset.wrongAnswers || '[]');
    if (wrongAnswers.includes(userAnswer)) {
        resultEl.innerHTML = '<span class="text-danger">Bạn đã chọn đáp án này và nó sai. Vui lòng chọn đáp án khác.</span>';
        return;
    }
    
    const payload = { practiceId: practiceEl.value, userAnswer: userAnswer };
    fetch('/AITutor/SubmitAnswer', {
        method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() },
        body: JSON.stringify(payload)
    }).then(r => r.json()).then(d => {
        const isCorrect = d.isCorrect || (userAnswer === correctAnswer);
        
        // Đánh dấu đã nộp
        optionsContainer.dataset.submitted = 'true';
        
        // Lấy tất cả các label chứa radio buttons
        const allLabels = optionsContainer.querySelectorAll('label');
        const allRadios = optionsContainer.querySelectorAll('input[type="radio"]');
        
        if (isCorrect) {
            // Đúng: disable tất cả, highlight màu xanh cho đáp án đúng
            allRadios.forEach(radio => {
                radio.disabled = true;
            });
            
            selected.closest('label').classList.add('answer-correct');
            resultEl.innerHTML = '<span class="text-success fw-bold">✓ Đúng!</span>';
            
            // Disable nút nộp
            if (submitBtn) submitBtn.disabled = true;
        } else {
            // Sai: disable đáp án đã chọn sai, highlight màu đỏ
            selected.disabled = true;
            selected.closest('label').classList.add('answer-wrong');
            
            // Lưu đáp án sai vào danh sách
            wrongAnswers.push(userAnswer);
            optionsContainer.dataset.wrongAnswers = JSON.stringify(wrongAnswers);
            
            // Highlight đáp án đúng màu xanh
            allRadios.forEach(radio => {
                if (radio.value === correctAnswer) {
                    radio.closest('label').classList.add('answer-correct');
                }
            });
            
            resultEl.innerHTML = `<span class="text-danger fw-bold">✗ Sai. Đáp án đúng: ${correctAnswer}</span>`;
            
            // Cho phép chọn lại (nhưng không cho chọn lại đáp án đã sai)
            // Không disable nút nộp, cho phép chọn đáp án khác
            optionsContainer.dataset.submitted = 'false'; // Cho phép nộp lại
        }
        
        aiLoadStats(lessonId);
    }).catch(() => { 
        resultEl.innerHTML = '<span class="text-danger">Chấm điểm bị lỗi.</span>'; 
    });
}

function aiSummarize(courseId, lessonId) {
    const sumEl = document.getElementById(`ai-summary-${lessonId}`);
    const payload = { courseId: courseId, lessonId: lessonId };
    fetch('/AITutor/SummarizeLesson', {
        method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() },
        body: JSON.stringify(payload)
    }).then(r => r.json()).then(d => { sumEl.innerText = d.summary || 'Không có tóm tắt.'; })
      .catch(() => { sumEl.innerText = 'Lỗi tóm tắt.'; });
}

function aiGenerateMany(courseId, lessonId) {
    const count = parseInt(document.getElementById(`ai-count-${lessonId}`).value || '1');
    const listEl = document.getElementById(`ai-mcq-list-${lessonId}`);
    // Lấy nội dung bài học từ phần lesson-content-formatted, không phải video
    const contentEl = document.querySelector(`#lesson-content-${lessonId} .lesson-content-formatted`);
    const lessonContext = contentEl?.innerText || contentEl?.textContent || '';
    const payload = { courseId: courseId, lessonId: lessonId, topic: '', lessonContext: lessonContext, count: count };
    fetch('/AITutor/GenerateQuestions', {
        method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() },
        body: JSON.stringify(payload)
    }).then(r => r.json()).then(d => {
        listEl.innerHTML = '';
        (d.items || []).forEach((it, idx) => {
            const block = document.createElement('div');
            block.className = 'mt-3 p-2 border rounded';
            const qId = `${lessonId}-multi-${idx}`;
            block.innerHTML = `
                <div class="fw-semibold">${it.question}</div>
                <div class="mt-2" id="opts-${qId}"></div>
                <div class="mt-2 d-flex gap-2">
                    <button type=\"button\" class=\"btn btn-success\" id=\"btn-${qId}\">Nộp đáp án</button>
                    <div class=\"align-self-center\" id=\"res-${qId}\"></div>
                </div>`;
            listEl.appendChild(block);
            const opts = document.getElementById(`opts-${qId}`);
            (it.options || []).forEach(opt => {
                const label = document.createElement('label');
                label.className = 'd-block';
                const radioId = `opt-${qId}-${Math.random().toString(36).slice(2)}`;
                label.innerHTML = `<input type=\"radio\" name=\"multi-${qId}\" value=\"${(opt||'').trim().substring(0,1).toUpperCase()}\" class=\"form-check-input me-2\" id=\"${radioId}\" /> <span>${opt}</span>`;
                opts.appendChild(label);
            });
            document.getElementById(`btn-${qId}`).onclick = () => {
                const chosen = document.querySelector(`input[name=\"multi-${qId}\"]:checked`);
                const resEl = document.getElementById(`res-${qId}`);
                if (!chosen) { resEl.innerText = 'Vui lòng chọn đáp án.'; return; }
                const payload2 = { practiceId: it.practiceId, userAnswer: chosen.value };
                fetch('/AITutor/SubmitAnswer', {
                    method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() },
                    body: JSON.stringify(payload2)
                }).then(r => r.json()).then(d => { resEl.innerText = d.isCorrect ? 'Đúng!' : `Sai. Đáp án đúng: ${d.correctAnswer}`; })
                  .catch(() => { resEl.innerText = 'Chấm điểm bị lỗi.'; });
            };
        });
    }).catch(() => { listEl.innerHTML = '<div class="text-danger">Không tạo được câu hỏi.</div>'; });
}

function switchAITab(event, tabId) {
    const wrapper = event.target.closest('.ai-section') || event.target.closest('.ai-tutor-compact');

    // Ẩn tất cả content
    wrapper.querySelectorAll('.ai-content').forEach(content => {
        content.classList.remove('active');
    });

    // Bỏ active tất cả tab
    wrapper.querySelectorAll('.ai-tab').forEach(tab => {
        tab.classList.remove('active');
    });

    // Hiển thị tab và content được chọn
    document.getElementById(tabId).classList.add('active');
    event.target.classList.add('active');
}
