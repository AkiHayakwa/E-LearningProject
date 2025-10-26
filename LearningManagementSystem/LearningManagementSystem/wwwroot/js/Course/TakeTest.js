function initializeTest(durationInMinutes, totalQuestions, redirectUrl) {
    // Khai báo biến cho timer
    let totalSeconds = durationInMinutes * 60;
    let isSubmitting = false;
    let isTimeUp = false;

    // Tính toán tiến độ làm bài
    function updateProgress() {
        const inputs = document.querySelectorAll('.question-input:checked, .question-input[type="textarea"]:not(:placeholder-shown)');
        const total = totalQuestions;
        const completed = inputs.length;

        document.getElementById('completedQuestions').textContent = completed;
        document.getElementById('progressBar').style.width = `${(completed / total) * 100}%`;

        // Cập nhật trạng thái nút điều hướng
        document.querySelectorAll('.question-input').forEach((input, index) => {
            if (input.checked || (input.tagName === 'TEXTAREA' && input.value.trim() !== '')) {
                const questionIndex = Array.from(document.querySelectorAll('.question-card')).findIndex(card => card.contains(input));
                const navButton = document.querySelector(`.nav-button[data-question="${questionIndex}"]`);
                if (navButton) {
                    navButton.classList.add('answered');
                }
            }
        });
    }

    // Cập nhật timer
    function updateTimer() {
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;
        const timeDisplay = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

        document.getElementById('timer').textContent = timeDisplay;
        document.getElementById('timer-display').textContent = timeDisplay;

        if (totalSeconds <= 300) { // 5 phút cuối
            document.querySelector('.time-circle').style.background = 'linear-gradient(135deg, #ff9800, #f44336)';
        }

        if (totalSeconds <= 0 && !isTimeUp) {
            clearInterval(timerInterval);
            isTimeUp = true;

            const inputs = document.querySelectorAll('.question-input');
            inputs.forEach(input => input.disabled = true);
            document.getElementById('submitButton').disabled = true;

            const form = document.getElementById('testForm');
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
                    window.location.href = redirectUrl;
                })
                .catch(error => {
                    console.error('Error during fetch:', error);
                    alert("Đã có lỗi xảy ra khi lưu bài. Vui lòng thử lại.");
                    window.location.href = redirectUrl;
                });
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

    // Khởi tạo
    document.addEventListener('DOMContentLoaded', function () {
        // Khởi tạo timer
        updateTimer();
        const timerInterval = setInterval(updateTimer, 1000);

        // Khởi tạo điều hướng
        setupNavigation();

        // Khởi tạo tiến độ
        updateProgress();

        // Theo dõi sự kiện submit
        document.querySelector('#testForm').addEventListener('submit', function () {
            isSubmitting = true;
        });

        // Cảnh báo khi rời trang
        window.onbeforeunload = function () {
            if (!isSubmitting && totalSeconds > 0 && !isTimeUp) {
                return "Bạn có chắc chắn muốn rời khỏi trang? Bài làm của bạn sẽ không được lưu.";
            }
        };
    });
}