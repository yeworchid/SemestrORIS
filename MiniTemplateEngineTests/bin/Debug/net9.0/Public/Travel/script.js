/**
 * Tour Website JavaScript
 * Handles tabs, gallery, and interactive elements
 */

(function() {
    'use strict';

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
        initTabs();
        initGallery();
    });

})();
