/**
 * StockGuard Enterprise UI/UX Interactivity Engine
 */

document.addEventListener('DOMContentLoaded', function () {
    // 1. Sidebar Toggle for Mobile & Responsive Drawer
    const sidebarToggle = document.getElementById('sgSidebarToggle');
    const sidebar = document.getElementById('sgSidebar');
    const backdrop = document.getElementById('sgSidebarBackdrop');

    if (sidebarToggle && sidebar && backdrop) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('show');
            backdrop.classList.toggle('show');
        });

        backdrop.addEventListener('click', function () {
            sidebar.classList.remove('show');
            backdrop.classList.remove('show');
        });
    }

    // 2. Client-side Table Search & Filters
    const searchInputs = document.querySelectorAll('[data-sg-table-search]');
    searchInputs.forEach(input => {
        const targetTableId = input.getAttribute('data-sg-table-search');
        const targetTable = document.getElementById(targetTableId);
        const clearBtn = input.parentElement.querySelector('.clear-search');
        const countDisplay = document.querySelector(`[data-sg-count-for="${targetTableId}"]`);

        if (!targetTable) return;

        const rows = targetTable.querySelectorAll('tbody tr:not(.no-records-row)');

        function filterTable() {
            const query = input.value.toLowerCase().trim();
            let visibleCount = 0;

            if (clearBtn) {
                clearBtn.style.display = query.length > 0 ? 'block' : 'none';
            }

            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                const matches = text.includes(query);
                row.style.display = matches ? '' : 'none';
                if (matches) visibleCount++;
            });

            // Handle empty state
            let noResultsRow = targetTable.querySelector('.empty-filter-row');
            if (visibleCount === 0 && rows.length > 0) {
                if (!noResultsRow) {
                    noResultsRow = document.createElement('tr');
                    noResultsRow.className = 'empty-filter-row';
                    const colCount = targetTable.querySelectorAll('thead th').length || 1;
                    noResultsRow.innerHTML = `<td colspan="${colCount}" class="text-center py-4 text-muted"><i class="bi bi-search fs-3 d-block mb-2 opacity-50"></i>No matching records found for "<strong>${input.value}</strong>".</td>`;
                    targetTable.querySelector('tbody').appendChild(noResultsRow);
                } else {
                    noResultsRow.style.display = '';
                    noResultsRow.querySelector('strong').textContent = input.value;
                }
            } else if (noResultsRow) {
                noResultsRow.style.display = 'none';
            }

            if (countDisplay) {
                countDisplay.textContent = `Showing ${visibleCount} of ${rows.length} records`;
            }
        }

        input.addEventListener('input', filterTable);

        if (clearBtn) {
            clearBtn.addEventListener('click', function () {
                input.value = '';
                filterTable();
                input.focus();
            });
        }
    });

    // 3. Status and Dropdown Filtering on Tables
    const filterSelects = document.querySelectorAll('[data-sg-table-filter]');
    filterSelects.forEach(select => {
        const targetTableId = select.getAttribute('data-sg-table-filter');
        const targetTable = document.getElementById(targetTableId);
        const colIndex = parseInt(select.getAttribute('data-sg-filter-col') || '0', 10);

        if (!targetTable) return;

        select.addEventListener('change', function () {
            const filterValue = select.value.toLowerCase().trim();
            const rows = targetTable.querySelectorAll('tbody tr:not(.no-records-row):not(.empty-filter-row)');

            rows.forEach(row => {
                const cells = row.querySelectorAll('td');
                if (cells.length > colIndex) {
                    const cellText = cells[colIndex].textContent.toLowerCase().trim();
                    if (!filterValue || cellText.includes(filterValue)) {
                        row.style.display = '';
                    } else {
                        row.style.display = 'none';
                    }
                }
            });
        });
    });

    // 4. Password Visibility Toggle
    const togglePasswordBtns = document.querySelectorAll('.toggle-password-visibility');
    togglePasswordBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const targetInputId = btn.getAttribute('data-target');
            const input = document.getElementById(targetInputId);
            const icon = btn.querySelector('i');

            if (input) {
                if (input.type === 'password') {
                    input.type = 'text';
                    if (icon) {
                        icon.classList.remove('bi-eye');
                        icon.classList.add('bi-eye-slash');
                    }
                } else {
                    input.type = 'password';
                    if (icon) {
                        icon.classList.remove('bi-eye-slash');
                        icon.classList.add('bi-eye');
                    }
                }
            }
        });
    });

    // 5. Auto-dismiss alerts after 5 seconds
    const autoDismissAlerts = document.querySelectorAll('.alert-dismissible');
    autoDismissAlerts.forEach(alert => {
        setTimeout(() => {
            if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
                if (bsAlert) bsAlert.close();
            } else {
                alert.remove();
            }
        }, 6000);
    });
});
