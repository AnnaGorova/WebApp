// removeCategories.js
console.log("🔍 removeCategories.js завантажено");

let currentRemoveId = null;

// Чекаємо, коли вся сторінка завантажиться
$(window).on('load', function () {
   

    // Обробник кліку на Remove
    $('body').on('click', '.remove_category_button', function (e) {
        e.preventDefault();
        e.stopPropagation();

        currentRemoveId = $(this).data('remove-id');
        console.log("ID категорії:", currentRemoveId);

        // Відкриваємо модальне вікно
        $('#remove_category_modal').show().css('opacity', 1);
        console.log("✅ Модальне вікно відкрито!");

        return false;
    });

    // Обробник для Cancel
    $('body').on('click', '.remove_category_form_button_cancel', function () {
       
        $('#remove_category_modal').hide().css('opacity', 0);
        currentRemoveId = null;
    });

    // Обробник для Confirm
    $('body').on('click', '.remove_category_form_button_confirm', function () {
        console.log("🟡 Confirm clicked for ID:", currentRemoveId);

        if (currentRemoveId) {
            console.log("🟡 Відправляю запит на видалення...");

            $.ajax({
                url: '/Admin/RemoveCategory?categoryId=' + currentRemoveId,
                type: 'GET',
                success: function (response) {
                    
                    // Закриваємо модальне вікно
                    $('#remove_category_modal').hide().css('opacity', 0);

                    if (response.success) {
                        console.log("✅ Успішне видалення, оновлюю сторінку...");
                        location.reload();
                    } else {
                        alert("Помилка: " + response.message);
                    }
                },
                error: function (xhr, status, error) {
                    console.error("❌ Помилка AJAX:", error);
                    alert("Помилка при з'єднанні з сервером: " + error);
                }
            });
        } else {
            alert("Не вибрано категорію для видалення");
        }
    });

    // Закриття модального вікна по кнопці X
    $('body').on('click', '[data-modal-dismiss="true"]', function () {
       
        $('#remove_category_modal').hide().css('opacity', 0);
        currentRemoveId = null;
    });

    // Закриття модального вікна по кліку на фон
    $('body').on('click', '#remove_category_modal', function (e) {
        if (e.target === this) {
           $(this).hide().css('opacity', 0);
            currentRemoveId = null;
        }
    });
});