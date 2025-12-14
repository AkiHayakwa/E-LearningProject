function initializeTest(durationInMinutes, totalQuestions, redirectUrl) {
    // Khai báo biến cho timer
    let totalSeconds = durationInMinutes * 60;
    let isSubmitting = false;
    let isTimeUp = false;
    let timerInterval = null; // Khai báo ở scope ngoài
    let autoSaveInterval = null; // Khai báo ở scope ngoài
    let hasStartedTest = false; // Khai báo ở scope ngoài

    // Tính toán tiến độ làm bài
    function updateProgress() {
        let completed = 0;
        const total = totalQuestions;
        
        // Đếm số câu hỏi đã trả lời
        document.querySelectorAll('.question-card').forEach(card => {
            const radioInputs = card.querySelectorAll('input[type="radio"]:checked');
            const textarea = card.querySelector('textarea.question-input');
            
            // Nếu có radio được chọn hoặc textarea có nội dung
            if (radioInputs.length > 0 || (textarea && textarea.value.trim() !== '')) {
                completed++;
            }
        });

        const completedElement = document.getElementById('completedQuestions');
        const progressBar = document.getElementById('progressBar');
        
        if (completedElement) completedElement.textContent = completed;
        if (progressBar) progressBar.style.width = `${(completed / total) * 100}%`;

        // Cập nhật trạng thái nút điều hướng
        document.querySelectorAll('.question-card').forEach((card, cardIndex) => {
            const radioInputs = card.querySelectorAll('input[type="radio"]:checked');
            const textarea = card.querySelector('textarea.question-input');
            const navButton = document.querySelector(`.nav-button[data-question="${cardIndex}"]`);
            
            if (navButton) {
                if (radioInputs.length > 0 || (textarea && textarea.value.trim() !== '')) {
                    navButton.classList.add('answered');
                } else {
                    navButton.classList.remove('answered');
                }
            }
        });
    }

    // Cập nhật timer
    function updateTimer() {
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;
        const timeDisplay = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

        const timerElement = document.getElementById('timer');
        const timerDisplayElement = document.getElementById('timer-display');
        
        if (timerElement) timerElement.textContent = timeDisplay;
        if (timerDisplayElement) timerDisplayElement.textContent = timeDisplay;

        if (totalSeconds <= 300) { // 5 phút cuối
            const timeCircle = document.querySelector('.time-circle');
            if (timeCircle) {
                timeCircle.style.background = 'linear-gradient(135deg, #ff9800, #f44336)';
            }
        }

        if (totalSeconds <= 0 && !isTimeUp) {
            if (timerInterval) clearInterval(timerInterval);
            isTimeUp = true;

            const inputs = document.querySelectorAll('.question-input');
            inputs.forEach(input => input.disabled = true);
            const submitButton = document.getElementById('submitButton');
            if (submitButton) submitButton.disabled = true;

            const form = document.getElementById('testForm');
            if (form) {
                const formData = new FormData(form);
                console.log('Form data being sent:', Object.fromEntries(formData));

                fetch(form.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'RequestVerificationToken': form.querySelector('input[name="__RequestVerificationToken"]').value
                    }
                })
                    .then(response => {
                        if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
                        console.log('Response received:', response.statusText);
                        return response.text();
                    })
                    .then(text => {
                        console.log('Response text:', text);
                        // Đánh dấu cần reload trang AllTests
                        sessionStorage.setItem('reloadAllTests', 'true');
                        window.location.href = redirectUrl;
                    })
                    .catch(error => {
                        console.error('Error during fetch:', error);
                        alert("Đã có lỗi xảy ra khi lưu bài. Vui lòng thử lại.");
                        // Đánh dấu cần reload trang AllTests
                        sessionStorage.setItem('reloadAllTests', 'true');
                        window.location.href = redirectUrl;
                    });
            }
        } else {
            totalSeconds--;
        }
    }

    // Điều hướng câu hỏi
    function setupNavigation() {
        const navButtons = document.querySelectorAll('.nav-button');
        const questionCards = document.querySelectorAll('.question-card');

        navButtons.forEach(button => {
            button.addEventListener('click', () => {
                const questionIndex = button.getAttribute('data-question');
                if (questionCards[questionIndex]) {
                    // Cuộn đến câu hỏi
                    questionCards[questionIndex].scrollIntoView({ behavior: 'smooth', block: 'start' });

                    // Cập nhật trạng thái active
                    navButtons.forEach(btn => btn.classList.remove('active'));
                    button.classList.add('active');
                }
            });
        });

        // Theo dõi sự kiện input để cập nhật tiến độ
        document.querySelectorAll('.question-input').forEach(input => {
            input.addEventListener('change', updateProgress);
            if (input.tagName === 'TEXTAREA') {
                input.addEventListener('input', updateProgress);
            }
        });
    }

    // Tự động lưu bài làm
    async function autoSaveTest() {
        if (isSubmitting || isTimeUp) return false;

        const form = document.getElementById('testForm');
        if (!form) return false;
        
        // Chuyển đổi FormData sang Dictionary format
        const selectedAnswers = {};
        const questionInputs = document.querySelectorAll('.question-input');
        
        questionInputs.forEach(input => {
            if (input.type === 'radio') {
                if (input.checked) {
                    const questionId = input.name.match(/\[(.+?)\]/)?.[1];
                    if (questionId) {
                        selectedAnswers[questionId] = input.value;
                    }
                }
            } else if (input.tagName === 'TEXTAREA') {
                const questionId = input.name.match(/\[(.+?)\]/)?.[1];
                if (questionId) {
                    selectedAnswers[questionId] = input.value || '';
                }
            }
        });

        // Lưu tất cả câu hỏi, kể cả câu trả lời trống
        const allQuestions = document.querySelectorAll('.question-card');
        allQuestions.forEach(card => {
            const questionId = card.querySelector('input[type="radio"], textarea')?.name?.match(/\[(.+?)\]/)?.[1];
            if (questionId && !selectedAnswers.hasOwnProperty(questionId)) {
                selectedAnswers[questionId] = ''; // Lưu câu trả lời trống
            }
        });

        // Gửi request để lưu
        const assignmentId = form.querySelector('input[name="assignmentId"]').value;
        const data = JSON.stringify({
            AssignmentId: assignmentId,
            SelectedAnswers: selectedAnswers
        });

        // Sử dụng fetch với keepalive cho beforeunload để đảm bảo request được gửi
        try {
            const response = await fetch('/Assignment/AutoSaveTest', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: data,
                keepalive: true // Đảm bảo request được gửi ngay cả khi trang đang đóng
            });

            if (response.ok) {
                const result = await response.json();
                console.log('Auto-saved:', result.message);
                return true;
            } else {
                console.error('Auto-save failed:', response.status);
                return false;
            }
        } catch (error) {
            console.error('Error auto-saving:', error);
            return false;
        }
    }

    // Khởi tạo
    document.addEventListener('DOMContentLoaded', function () {
        // Khởi tạo timer
        updateTimer();
        timerInterval = setInterval(updateTimer, 1000);

        // Khởi tạo điều hướng
        setupNavigation();

        // Khởi tạo tiến độ
        updateProgress();

        // Theo dõi sự kiện submit
        const testForm = document.querySelector('#testForm');
        if (testForm) {
            testForm.addEventListener('submit', async function (e) {
                isSubmitting = true;
                if (autoSaveInterval) clearInterval(autoSaveInterval); // Dừng auto-save khi submit
                
                // Lưu lần cuối trước khi submit
                await autoSaveTest();
            });
        }

        // Đánh dấu khi người dùng bắt đầu làm bài (thay đổi bất kỳ câu trả lời nào)
        document.querySelectorAll('.question-input').forEach(input => {
            input.addEventListener('change', function() {
                hasStartedTest = true;
            });
            if (input.tagName === 'TEXTAREA') {
                input.addEventListener('input', function() {
                    hasStartedTest = true;
                });
            }
        });

        // Cảnh báo và tự động lưu khi rời trang
        window.addEventListener('beforeunload', function (e) {
            if (!isSubmitting && totalSeconds > 0 && !isTimeUp && hasStartedTest) {
                // Tự động lưu trước khi rời trang (không dùng await vì beforeunload không hỗ trợ async)
                // Gọi sync để đảm bảo request được gửi
                autoSaveTest();
                
                // Đánh dấu cần reload trang AllTests khi quay lại
                sessionStorage.setItem('reloadAllTests', 'true');
                
                // Hiển thị cảnh báo
                e.preventDefault();
                e.returnValue = "Bạn có chắc chắn muốn rời khỏi trang? Bài làm của bạn sẽ được tự động lưu và bạn không thể làm lại.";
                return e.returnValue;
            }
        });

        // Lưu khi người dùng bấm nút quay lại của trình duyệt
        window.addEventListener('popstate', function (e) {
            if (!isSubmitting && totalSeconds > 0 && !isTimeUp && hasStartedTest) {
                autoSaveTest();
                // Đánh dấu cần reload trang AllTests khi quay lại
                sessionStorage.setItem('reloadAllTests', 'true');
            }
        });

        // Lưu khi người dùng bấm nút back của trình duyệt (sử dụng history API)
        const originalPushState = history.pushState;
        history.pushState = function() {
            if (!isSubmitting && totalSeconds > 0 && !isTimeUp && hasStartedTest) {
                autoSaveTest();
            }
            return originalPushState.apply(history, arguments);
        };

        // Tự động lưu định kỳ mỗi 30 giây
        autoSaveInterval = setInterval(async () => {
            if (!isSubmitting && totalSeconds > 0 && !isTimeUp) {
                await autoSaveTest();
            }
        }, 30000); // Lưu mỗi 30 giây
    });
}