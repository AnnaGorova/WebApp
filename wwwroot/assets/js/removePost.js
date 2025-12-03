// removePost.js - спрощена версія як у тегів
let currentRemoveId = null;

$(window).on('load', function () {
    // Обробник кліку на Remove
    $('body').on('click', '.remove_post_button', function (e) {
        e.preventDefault();
        e.stopPropagation();

        currentRemoveId = $(this).data('post-id');
        const postName = $(this).data('post-name');

        // Оновлюємо інформацію в модальному вікні
        $('#post_name_display').text(postName);
        $('#post_id_display').text(currentRemoveId);

        // Показуємо модальне вікно
        $('#remove_post_modal').show().css('opacity', 1);

        return false;
    });

    // Cancel
    $('body').on('click', '.remove_post_form_button_cancel', function () {
        $('#remove_post_modal').hide().css('opacity', 0);
        currentRemoveId = null;
    });

    // Confirm
    $('body').on('click', '.remove_post_form_button_confirm', function () {
        if (currentRemoveId) {
            // Блокуємо кнопку
            $(this).prop('disabled', true).html('Видалення...');

            $.ajax({
                url: '/Admin/DeletePost?postId=' + currentRemoveId,
                type: 'POST',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    $('#remove_post_modal').hide().css('opacity', 0);
                    if (response.success) {
                        // Видаляємо рядок з таблиці
                        $('[data-post-id="' + currentRemoveId + '"]').closest('tr').fadeOut(300, function () {
                            $(this).remove();

                            // Перевіряємо, чи є ще пости
                            if ($('#posts_table tbody tr').length === 0) {
                                $('#posts_table tbody').html(`
                                    <tr>
                                        <td colspan="11" class="text-center py-8 text-gray-500">
                                            <i class="ki-filled ki-document text-3xl mb-2 block"></i>
                                            <p>Пости відсутні</p>
                                        </td>
                                    </tr>
                                `);
                            }
                        });
                    } else {
                        alert("Помилка: " + response.message);
                    }
                    currentRemoveId = null;
                },
                error: function (xhr) {
                    alert("Помилка при з'єднанні з сервером.");
                    $('.remove_post_form_button_confirm').prop('disabled', false).html('Видалити');
                }
            });
        }
    });

    // Close modal
    $('body').on('click', '[data-modal-dismiss="true"]', function () {
        $('#remove_post_modal').hide().css('opacity', 0);
        currentRemoveId = null;
    });
});