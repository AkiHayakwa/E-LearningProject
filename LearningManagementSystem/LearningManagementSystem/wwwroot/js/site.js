// (1) Navbar Scroll
// Thao tác này có thể chạy ngay
window.addEventListener('scroll', function () {
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        if (window.scrollY > 10) {
            navbar.classList.add('navbar-scrolled');
        } else {
            navbar.classList.remove('navbar-scrolled');
        }
    }
});

// (2) Chạy tất cả các script khác sau khi DOM đã tải xong
document.addEventListener('DOMContentLoaded', function () {

    // (A) Logic cho link "Khám phá khóa học"
    const exploreCoursesLink = document.getElementById('exploreCoursesLink');
    if (exploreCoursesLink) {
        exploreCoursesLink.addEventListener('click', function (e) {
            if (window.location.pathname === '/Search/Index' || window.location.pathname === '/Search') {
                e.preventDefault();
                const courseList = document.getElementById('course-list'); // Đảm bảo ID này tồn tại trên trang Search
                if (courseList) {
                    courseList.scrollIntoView({ behavior: 'smooth' });
                }
            }
        });

        // Thêm logic 'active' cho nav-link
        if (window.location.pathname === '/Search/Index' || window.location.pathname === '/Search') {
            document.querySelector('.nav-link[href="/"]').classList.remove('active');
            exploreCoursesLink.classList.add('active');
        } else if (window.location.pathname === '/') {
            document.querySelector('.nav-link[href="/"]').classList.add('active');
            exploreCoursesLink.classList.remove('active');
        }
    }


    // (B) Logic cho thanh tìm kiếm (Search Bar) với lịch sử tìm kiếm
    const searchInput = document.getElementById('searchInput');
    const searchSuggestions = document.getElementById('searchSuggestions');
    const searchForm = document.getElementById('searchForm');

    // Hàm lưu lịch sử tìm kiếm
    function saveSearchHistory(query) {
        if (!query || query.trim().length === 0) return;
        
        let history = JSON.parse(localStorage.getItem('searchHistory') || '[]');
        query = query.trim();
        
        // Xóa nếu đã tồn tại
        history = history.filter(item => item.toLowerCase() !== query.toLowerCase());
        
        // Thêm vào đầu danh sách
        history.unshift(query);
        
        // Giới hạn 10 mục
        if (history.length > 10) {
            history = history.slice(0, 10);
        }
        
        localStorage.setItem('searchHistory', JSON.stringify(history));
    }

    // Hàm lấy lịch sử tìm kiếm
    function getSearchHistory() {
        return JSON.parse(localStorage.getItem('searchHistory') || '[]');
    }

    // Hàm xóa lịch sử tìm kiếm
    function clearSearchHistory() {
        localStorage.removeItem('searchHistory');
        renderSearchHistory();
    }

    // Hàm render lịch sử tìm kiếm
    function renderSearchHistory() {
        const history = getSearchHistory();
        if (history.length === 0) return '';

        let html = `
            <div class="search-history-section">
                <div class="search-history-header">
                    <span>Lịch sử tìm kiếm</span>
                    <button type="button" class="search-history-clear" onclick="clearSearchHistory()">
                        <i class="bi bi-trash"></i> Xóa
                    </button>
                </div>
        `;

        history.forEach(item => {
            html += `
                <div class="search-history-item" data-query="${item.replace(/"/g, '&quot;')}">
                    <i class="bi bi-clock-history"></i>
                    <span>${item}</span>
                </div>
            `;
        });

        html += '</div>';
        return html;
    }

    // Expose clearSearchHistory to global scope
    window.clearSearchHistory = clearSearchHistory;

    if (searchInput && searchSuggestions && searchForm) {
        let currentSuggestions = [];

        searchInput.addEventListener('input', function () {
            const query = this.value.trim();
            if (query.length > 0) {
                fetch(`/Search/GetSearchSuggestions?query=${encodeURIComponent(query)}`)
                    .then(response => response.json())
                    .then(suggestions => {
                        searchSuggestions.innerHTML = '';
                        currentSuggestions = suggestions;
                        
                        if (suggestions.length > 0) {
                            suggestions.forEach(suggestion => {
                                const item = document.createElement('div');
                                item.className = 'search-suggestion-item';
                                item.textContent = suggestion;
                                item.addEventListener('click', function () {
                                    searchInput.value = suggestion;
                                    saveSearchHistory(suggestion);
                                    searchSuggestions.style.display = 'none';
                                    searchForm.submit();
                                });
                                searchSuggestions.appendChild(item);
                            });
                            
                            // Thêm lịch sử tìm kiếm
                            const historyHtml = renderSearchHistory();
                            if (historyHtml) {
                                const historyDiv = document.createElement('div');
                                historyDiv.innerHTML = historyHtml;
                                searchSuggestions.appendChild(historyDiv);
                                
                                // Thêm event listeners cho các mục lịch sử
                                historyDiv.querySelectorAll('.search-history-item').forEach(item => {
                                    item.addEventListener('click', function () {
                                        const query = this.getAttribute('data-query');
                                        searchInput.value = query;
                                        saveSearchHistory(query);
                                        searchSuggestions.style.display = 'none';
                                        searchForm.submit();
                                    });
                                });
                            }
                            
                            searchSuggestions.style.display = 'block';
                        } else {
                            // Nếu không có gợi ý, chỉ hiển thị lịch sử
                            const historyHtml = renderSearchHistory();
                            if (historyHtml) {
                                searchSuggestions.innerHTML = historyHtml;
                                
                                // Thêm event listeners cho các mục lịch sử
                                searchSuggestions.querySelectorAll('.search-history-item').forEach(item => {
                                    item.addEventListener('click', function () {
                                        const query = this.getAttribute('data-query');
                                        searchInput.value = query;
                                        saveSearchHistory(query);
                                        searchSuggestions.style.display = 'none';
                                        searchForm.submit();
                                    });
                                });
                                
                                searchSuggestions.style.display = 'block';
                            } else {
                                searchSuggestions.style.display = 'none';
                            }
                        }
                    })
                    .catch(error => console.error('Error fetching suggestions:', error));
            } else {
                // Nếu input rỗng, chỉ hiển thị lịch sử
                const historyHtml = renderSearchHistory();
                if (historyHtml) {
                    searchSuggestions.innerHTML = historyHtml;
                    
                    // Thêm event listeners cho các mục lịch sử
                    searchSuggestions.querySelectorAll('.search-history-item').forEach(item => {
                        item.addEventListener('click', function () {
                            const query = this.getAttribute('data-query');
                            searchInput.value = query;
                            saveSearchHistory(query);
                            searchSuggestions.style.display = 'none';
                            searchForm.submit();
                        });
                    });
                    
                    searchSuggestions.style.display = 'block';
                } else {
                    searchSuggestions.style.display = 'none';
                }
            }
        });

        // Lưu lịch sử khi submit form
        searchForm.addEventListener('submit', function () {
            const query = searchInput.value.trim();
            if (query.length > 0) {
                saveSearchHistory(query);
            }
        });

        // Ẩn gợi ý khi click ra ngoài
        document.addEventListener('click', function (e) {
            if (!searchInput.contains(e.target) && !searchSuggestions.contains(e.target)) {
                searchSuggestions.style.display = 'none';
            }
        });

        // Hiện lịch sử khi focus (nếu input rỗng)
        searchInput.addEventListener('focus', function () {
            const query = searchInput.value.trim();
            if (query.length === 0) {
                const historyHtml = renderSearchHistory();
                if (historyHtml) {
                    searchSuggestions.innerHTML = historyHtml;
                    
                    // Thêm event listeners cho các mục lịch sử
                    searchSuggestions.querySelectorAll('.search-history-item').forEach(item => {
                        item.addEventListener('click', function () {
                            const query = this.getAttribute('data-query');
                            searchInput.value = query;
                            saveSearchHistory(query);
                            searchSuggestions.style.display = 'none';
                            searchForm.submit();
                        });
                    });
                    
                    searchSuggestions.style.display = 'block';
                }
            } else if (currentSuggestions.length > 0) {
                searchSuggestions.style.display = 'block';
            }
        });
    }

    // (C) Logic Validation cho thanh tìm kiếm
    if (searchForm) {
        searchForm.addEventListener('submit', function (e) {
            const searchInputValue = document.getElementById('searchInput').value.trim();
            if (!searchInputValue) {
                e.preventDefault(); // Ngăn form submit

                // Hiển thị thông báo (Toast)
                const toastContainer = document.createElement('div');
                toastContainer.style.position = 'fixed';
                toastContainer.style.top = '80px'; // Dưới navbar
                toastContainer.style.right = '20px';
                toastContainer.style.zIndex = '10000';

                const toast = document.createElement('div');
                toast.className = 'toast show';
                toast.setAttribute('role', 'alert');
                toast.setAttribute('aria-live', 'assertive');
                toast.setAttribute('aria-atomic', 'true');
                toast.style.backgroundColor = 'white';
                toast.style.color = '#2d3748';
                toast.style.borderRadius = '8px';
                toast.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.15)';
                toast.style.minWidth = '300px';

                toast.innerHTML = `
                    <div class="toast-header" style="border-bottom: 1px solid #e2e8f0;">
                        <strong class="me-auto" style="color: #e53e3e;"><i class="bi bi-exclamation-circle me-2"></i>Lỗi</strong>
                        <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close" onclick="this.closest('.toast-container').remove()"></button>
                    </div>
                    <div class="toast-body">
                        Vui lòng nhập từ khóa tìm kiếm!
                    </div>
                `;

                toastContainer.appendChild(toast);
                document.body.appendChild(toastContainer);

                setTimeout(() => {
                    toastContainer.remove();
                }, 3000);
            }
        });
    }

    // (D) Logic tự động focus vào search bar trên trang Search
    if (window.location.pathname === '/Search/Index' || window.location.pathname === '/Search') {
        const searchInputOnSearchPage = document.getElementById('searchInput');
        if (searchInputOnSearchPage) {
            searchInputOnSearchPage.focus();
        }

        // Tự cuộn đến danh sách khóa học (nếu có)
        const courseList = document.getElementById('course-list');
        if (courseList) {
            // Dùng timeout nhỏ để đảm bảo trang đã render xong
            setTimeout(() => {
                courseList.scrollIntoView({ behavior: 'smooth' });
            }, 100);
        }
    }

    // (E) Kích hoạt Bootstrap Tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // (F) Theme toggle
    const themeToggleBtn = document.querySelector('.theme-toggle');
    const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    const savedTheme = localStorage.getItem('preferredTheme');

    function applyTheme(mode) {
        const body = document.body;
        if (mode === 'dark') {
            body.classList.add('dark-theme');
            if (themeToggleBtn) {
                themeToggleBtn.innerHTML = '<i class="bi bi-sun-fill"></i>';
            }
        } else {
            body.classList.remove('dark-theme');
            if (themeToggleBtn) {
                themeToggleBtn.innerHTML = '<i class="bi bi-moon-fill"></i>';
            }
        }
    }

    let initialTheme = savedTheme || (prefersDark ? 'dark' : 'light');
    applyTheme(initialTheme);

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', function () {
            const isDark = document.body.classList.toggle('dark-theme');
            const mode = isDark ? 'dark' : 'light';
            localStorage.setItem('preferredTheme', mode);
            applyTheme(mode);
        });
    }

    // (G) Language switch (VI / EN)
    const translations = {
        vi: {
            welcome_back: "Chào mừng trở lại",
            continue_journey: "Hãy tiếp tục hành trình chinh phục tri thức của bạn ngay hôm nay.",
            welcome_to_system: "Chào mừng đến với E-Learning System!",
            explore_courses_desc: "Khám phá các khóa học Công nghệ Thông tin và nâng cao kỹ năng của bạn.",
            start_learning_now: "Bắt đầu học ngay",
            home: "Trang chủ",
            courses: "Khóa học",
            search_placeholder: "Tìm kiếm khóa học...",
            search_button: "Tìm"
        },
        en: {
            welcome_back: "Welcome back",
            continue_journey: "Continue your journey of conquering knowledge today.",
            welcome_to_system: "Welcome to E-Learning System!",
            explore_courses_desc: "Explore Information Technology courses and enhance your skills.",
            start_learning_now: "Start learning now",
            home: "Home",
            courses: "Courses",
            search_placeholder: "Search for courses...",
            search_button: "Search"
        }
    };

    const langButtons = document.querySelectorAll('.lang-btn');
    const savedLang = localStorage.getItem('preferredLang') || 'vi';

    function applyLanguage(lang) {
        document.documentElement.setAttribute('lang', lang);
        langButtons.forEach(btn => {
            btn.classList.toggle('active', btn.dataset.lang === lang);
        });
        localStorage.setItem('preferredLang', lang);

        const texts = translations[lang] || translations.vi;
        
        // Update ALL elements with data-i18n attribute
        // This will find all elements including nested ones (like span inside h1)
        document.querySelectorAll('[data-i18n]').forEach(el => {
            const key = el.getAttribute('data-i18n');
            if (texts[key]) {
                // Simply replace text content - this works for span, button, p, h1, etc.
                // For elements with children, it will replace all content including children
                // But since we're selecting the element WITH data-i18n, we want to replace its content
                el.textContent = texts[key];
            }
        });

        // Update elements with data-i18n-placeholder attribute
        document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
            const key = el.getAttribute('data-i18n-placeholder');
            if (texts[key]) {
                el.placeholder = texts[key];
            }
        });

        // Update elements that declare inline translations
        document.querySelectorAll('[data-i18n-en]').forEach(el => {
            const attrName = `data-i18n-${lang}`;
            const value = el.getAttribute(attrName);
            if (value !== null) {
                el.textContent = value;
            }
        });

        // Update placeholders that declare inline translations
        document.querySelectorAll('[data-i18n-placeholder-en]').forEach(el => {
            const attrName = `data-i18n-placeholder-${lang}`;
            const value = el.getAttribute(attrName);
            if (value !== null) {
                el.placeholder = value;
            }
        });

        // Update search button (fallback if not updated by data-i18n)
        const searchButton = document.getElementById('searchButton');
        if (searchButton && texts.search_button) {
            const hasI18n = searchButton.hasAttribute('data-i18n');
            if (!hasI18n) {
                searchButton.textContent = texts.search_button;
            }
        }
    }

    if (langButtons.length > 0) {
        applyLanguage(savedLang);
        langButtons.forEach(btn => {
            btn.addEventListener('click', function () {
                const lang = this.dataset.lang || 'vi';
                applyLanguage(lang);
            });
        });
    }

});