/**
 * Tour Website JavaScript
 * Handles tabs, gallery, and interactive elements
 */

(function() {
    'use strict';

    /**
     * AJAX фильтрация туров
     */
    function initSearch() {
        const searchForm = document.querySelector('.search-form-wrapper form');
        if (!searchForm) return;
        
        searchForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // собираем значения из формы
            const searchInput = document.querySelector('input[placeholder="задать название тура"]');
            const countrySelect = document.querySelectorAll('.search-form-wrapper select')[1];
            const dateFromInput = document.querySelector('input[name="date_from"]');
            const dateToInput = document.querySelector('input[name="date_to"]');
            
            const search = searchInput ? searchInput.value : '';
            const country = countrySelect ? countrySelect.value : '';
            const dateFrom = dateFromInput ? dateFromInput.value : '';
            const dateTo = dateToInput ? dateToInput.value : '';
            
            // строим URL с параметрами
            let url = '/travel?';
            const params = [];
            
            if (search) {
                params.push('search=' + encodeURIComponent(search));
            }
            
            if (country) {
                params.push('country=' + encodeURIComponent(country));
            }
            
            if (dateFrom) {
                params.push('date_from=' + encodeURIComponent(dateFrom));
            }
            
            if (dateTo) {
                params.push('date_to=' + encodeURIComponent(dateTo));
            }
            
            url += params.join('&');
            
            console.log('отправляем запрос:', url);
            
            // показываем индикатор загрузки
            const resultsContainer = document.querySelector('.results-list');
            if (resultsContainer) {
                resultsContainer.innerHTML = '<li style="text-align: center; padding: 50px;">Загрузка...</li>';
            }
            
            // делаем AJAX запрос
            fetch(url)
                .then(function(response) {
                    return response.text();
                })
                .then(function(html) {
                    // парсим HTML и достаем список туров
                    const parser = new DOMParser();
                    const doc = parser.parseFromString(html, 'text/html');
                    const newResults = doc.querySelector('.results-list');
                    
                    if (newResults && resultsContainer) {
                        resultsContainer.innerHTML = newResults.innerHTML;
                        
                        // если туров нет - показываем сообщение
                        if (newResults.children.length === 0) {
                            resultsContainer.innerHTML = '<li style="text-align: center; padding: 50px;">Туры не найдены 😢</li>';
                        }
                    }
                })
                .catch(function(error) {
                    console.error('ошибка:', error);
                    if (resultsContainer) {
                        resultsContainer.innerHTML = '<li style="text-align: center; padding: 50px;">Ошибка загрузки 😢</li>';
                    }
                });
        });
    }

    /**
     * Initialize tabs functionality
     */
    function initTabs() {
        const tabLinks = document.querySelectorAll('[data-toggle="tab"]');
        
        tabLinks.forEach(function(link) {
            link.addEventListener('click', function(e) {
                e.preventDefault();
                
                const targetId = this.getAttribute('href');
                const targetPane = document.querySelector(targetId);
                
                if (!targetPane) return;
                
                // Remove active class from all tabs
                tabLinks.forEach(function(tab) {
                    tab.classList.remove('active', 'show');
                    tab.setAttribute('aria-selected', 'false');
                });
                
                // Remove active class from all panes
                const allPanes = document.querySelectorAll('.tab-pane');
                allPanes.forEach(function(pane) {
                    pane.classList.remove('active', 'show');
                });
                
                // Add active class to clicked tab
                this.classList.add('active', 'show');
                this.setAttribute('aria-selected', 'true');
                
                // Add active class to target pane
                targetPane.classList.add('active', 'show');
            });
        });
    }

    /**
     * Initialize gallery thumbnails functionality
     */
    function initGallery() {
        const galleryItems = document.querySelectorAll('.view-gallery-owl .item');
        const thumbItems = document.querySelectorAll('.owl-thumbs .item');
        
        if (galleryItems.length === 0 || thumbItems.length === 0) return;
        
        // Set first thumb as active
        thumbItems[0].classList.add('active');
        
        thumbItems.forEach(function(thumb, index) {
            thumb.addEventListener('click', function() {
                // Hide all gallery items
                galleryItems.forEach(function(item) {
                    item.style.display = 'none';
                });
                
                // Remove active class from all thumbs
                thumbItems.forEach(function(t) {
                    t.classList.remove('active');
                });
                
                // Show selected gallery item
                if (galleryItems[index]) {
                    galleryItems[index].style.display = 'block';
                }
                
                // Add active class to clicked thumb
                this.classList.add('active');
            });
        });
    }

    /**
     * Initialize all functionality when DOM is ready
     */
    document.addEventListener('DOMContentLoaded', function() {
        initSearch();
        initTabs();
        initGallery();
    });

})();
