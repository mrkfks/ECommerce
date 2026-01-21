export const NotificationModule = (function () {
    // Notification polling interval (30 seconds)
    const POLL_INTERVAL = 30000;
    let pollTimer = null;

    // Load notifications
    function loadNotifications() {
        fetch('/Home/GetNotificationSummary')
            .then(response => response.json())
            .then(data => {
                updateNotificationBadge(data.unreadCount);
                renderNotifications(data.recentNotifications || []);
            })
            .catch(err => console.log('Notification load error:', err));
    }

    // Update badge count
    function updateNotificationBadge(count) {
        const badge = document.getElementById('notificationBadge');
        if (badge) {
            if (count > 0) {
                badge.style.display = 'flex';
                badge.textContent = count > 99 ? '99+' : count;
            } else {
                badge.style.display = 'none';
            }
        }
    }

    // Render notification items
    function renderNotifications(notifications) {
        const list = document.getElementById('notificationList');
        if (!list) return;

        if (notifications.length === 0) {
            list.innerHTML = `
                <div class="text-center py-4 text-muted">
                    <i class="fa-regular fa-bell-slash fa-2x mb-2"></i>
                    <p class="mb-0">Yeni bildirim yok</p>
                </div>`;
            return;
        }

        let html = '';
        notifications.forEach(n => {
            const iconClass = getIconClass(n.type);
            const colorClass = getColorClass(n.type, n.priority);
            const itemClass = !n.isRead ? 'unread' : '';
            const priorityClass = n.priority >= 3 ? (n.priority === 4 ? 'critical' : 'warning') : '';

            html += `
                <div class="notification-item ${itemClass} ${priorityClass}" 
                     onclick="NotificationModule.handleClick(${n.id}, '${n.actionUrl || ''}')">
                    <div class="d-flex align-items-start">
                        <div class="notification-icon ${colorClass} me-3">
                            <i class="fa-solid ${iconClass}"></i>
                        </div>
                        <div class="flex-grow-1">
                            <div class="d-flex justify-content-between align-items-start">
                                <strong class="d-block">${escapeHtml(n.title)}</strong>
                                <span class="notification-time">${n.timeAgo}</span>
                            </div>
                            <small class="text-muted">${escapeHtml(n.message)}</small>
                            ${n.actionText ? `<div class="mt-1"><span class="badge bg-primary">${escapeHtml(n.actionText)}</span></div>` : ''}
                        </div>
                    </div>
                </div>`;
        });

        list.innerHTML = html;
    }

    // Get icon class by type
    function getIconClass(type) {
        const icons = {
            1: 'fa-box-open',      // LowStock
            2: 'fa-shopping-cart', // NewOrder
            3: 'fa-undo',          // ReturnRequest
            4: 'fa-credit-card',   // PaymentFailed
            5: 'fa-truck',         // OrderStatusChanged
            6: 'fa-bell'           // System
        };
        return icons[type] || 'fa-bell';
    }

    // Get color class by type and priority
    function getColorClass(type, priority) {
        if (priority === 4) return 'danger';  // Critical
        if (priority === 3) return 'warning'; // High

        const colors = {
            1: 'danger',  // LowStock
            2: 'success', // NewOrder
            3: 'warning', // ReturnRequest
            4: 'danger',  // PaymentFailed
            5: 'info',    // OrderStatusChanged
            6: 'info'     // System
        };
        return colors[type] || 'info';
    }

    // Escape HTML
    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Handle notification click
    function handleNotificationClick(id, actionUrl) {
        // Mark as read
        fetch(`/Home/MarkNotificationAsRead?id=${id}`, { method: 'POST' })
            .then(() => {
                if (actionUrl) {
                    window.location.href = actionUrl;
                } else {
                    loadNotifications();
                }
            });
    }

    function init() {
        if (document.getElementById('notificationBell')) {
            loadNotifications();
            pollTimer = setInterval(loadNotifications, POLL_INTERVAL);

            // Reload on dropdown open
            document.getElementById('notificationBell')?.addEventListener('click', loadNotifications);

            // Mark all as read
            document.getElementById('markAllRead')?.addEventListener('click', function (e) {
                e.preventDefault();
                fetch('/Home/MarkAllNotificationsAsRead', { method: 'POST' })
                    .then(() => loadNotifications());
            });
        }
    }

    return {
        init: init,
        handleClick: handleNotificationClick
    };
})();

// Auto init if bell exists
document.addEventListener('DOMContentLoaded', () => {
    NotificationModule.init();
    // Expose to window for onclick handlers
    window.NotificationModule = NotificationModule;
});
