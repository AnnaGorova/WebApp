// removeNavigation.js
let currentNavigationId = null;

$(window).on('load', function () {
    // Обробник кліку на Remove
    $('body').on('click', '.remove_navigation_button', function (e) {
        e.preventDefault();
        e.stopPropagation();

        currentNavigationId = $(this).data('navigation-id');
        const navigationName = $(this).data('navigation-title');
        const childrenCount = $(this).data('children-count') || 0;

        // Оновлюємо інформацію в модальному вікні
        $('#navigation_name_display').text(navigationName);
        $('#navigation_id_display').text(currentNavigationId);

        if (childrenCount > 0) {
            $('#children_count_display').html(
                `<span class="text-red-600 font-medium">⚠️ Цей пункт має ${childrenCount} дочірніх елементів!</span>`
            );
        } else {
            $('#children_count_display').html('');
        }

        // Показуємо модальне вікно
        $('#remove_navigation_modal').show().css('opacity', 1);

        return false;
    });

    // Cancel
    $('body').on('click', '.remove_navigation_form_button_cancel', function () {
        $('#remove_navigation_modal').hide().css('opacity', 0);
        currentNavigationId = null;
    });

    // Confirm
    $('body').on('click', '.remove_navigation_form_button_confirm', function () {
        if (currentNavigationId) {
            // Блокуємо кнопку
            const confirmBtn = $(this);
            confirmBtn.prop('disabled', true).html('Видалення...');

            // Виконуємо GET запит (як у тегів)
            $.ajax({
                url: '/Admin/DeleteNavigation?id=' + currentNavigationId,
                type: 'GET',
                success: function (response) {
                    console.log('Відповідь від сервера:', response);

                    $('#remove_navigation_modal').hide().css('opacity', 0);

                    if (response.success) {
                        // Видаляємо рядок з таблиці
                        const row = $('a[data-navigation-id="' + currentNavigationId + '"]').closest('tr');
                        if (row.length) {
                            row.fadeOut(300, function () {
                                $(this).remove();

                                // Оновлюємо лічильник
                                updateNavigationCount();

                                // Перевіряємо, чи є ще пункти
                                if ($('#navigation_table tbody tr').length === 0) {
                                    $('#navigation_table tbody').html(`
                                        <tr>
                                            <td colspan="7" class="text-center py-8 text-gray-500">
                                                <i class="ki-filled ki-menu text-3xl mb-2 block"></i>
                                                <p>Пункти меню відсутні</p>
                                            </td>
                                        </tr>
                                    `);
                                }
                            });
                        }

                        // Показуємо повідомлення про успіх
                        showNotification('success', response.message);
                    } else {
                        // Показуємо повідомлення про помилку
                        alert("Помилка: " + response.message);
                    }

                    currentNavigationId = null;
                    confirmBtn.prop('disabled', false).html('Видалити');
                },
                error: function (xhr, status, error) {
                    console.error('Помилка AJAX:', error);
                    alert("Помилка при з'єднанні з сервером.");
                    confirmBtn.prop('disabled', false).html('Видалити');
                }
            });
        }
    });

    // Close modal
    $('body').on('click', '[data-modal-dismiss="true"]', function () {
        $('#remove_navigation_modal').hide().css('opacity', 0);
        currentNavigationId = null;
    });
});

// Функція для оновлення лічильника
function updateNavigationCount() {
    const countElement = $('.card-title.font-medium.text-sm');
    if (countElement.length) {
        const currentText = countElement.text();
        const match = currentText.match(/\d+/);
        if (match) {
            const currentCount = parseInt(match[0]);
            const newCount = Math.max(0, currentCount - 1);
            countElement.text('Показ ' + newCount + ' пунктів меню');
        }
    }
}

// Функція для показу сповіщень
function showNotification(type, message) {
    // Видаляємо старі сповіщення
    $('.custom-notification').remove();

    // Створюємо нове сповіщення
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    const icon = type === 'success' ? '✓' : '✗';

    const alertHtml = `
        <div class="custom-notification alert ${alertClass} alert-dismissible fade show mb-3" role="alert" style="animation: fadeIn 0.3s;">
            ${icon} ${message}
            <button type="button" class="btn-close" onclick="$(this).parent().remove()"></button>
        </div>
    `;

    // Додаємо на початок card-body
    $('.card-body').prepend(alertHtml);

    // Автоматично видаляємо через 5 секунд
    setTimeout(function () {
        $('.custom-notification').fadeOut(300, function () {
            $(this).remove();
        });
    }, 5000);
}