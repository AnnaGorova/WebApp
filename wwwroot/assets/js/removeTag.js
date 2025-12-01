// removeTag.js
let currentRemoveId = null;


// Обробник кліку на Remove
$(window).on('load', function () {

    $('body').on('click', '.remove_tag_button', function (e) {
        e.preventDefault();
        e.stopPropagation();


        currentRemoveId = $(this).data('remove-id');
        $('#remove_tag_modal').show().css('opacity', 1);

        return false;

    });



    //Cancel
    $('body').on('click', '.remove_tag_form_button_cancel', function () {

        $('#remove_tag_modal').hide().css('opacity', 0);
       currentRemoveId = null;
    });


    //Confirm
    $('body').on('click', '.remove_tag_form_button_confirm', function () {

        if (currentRemoveId) {
            $.ajax({
                url: '/Admin/RemoveTag?tagId=' + currentRemoveId,
                type: 'GET',
                success: function (response) {
                    $('#remove_tag_modal').hide().css('opacity', 0);
                    if (response.success) {
                        location.reload();
                    } else {
                        alert("Помилка: " + response.message);
                    }

                },
                error: function (xhr) {
                    alert("Помилка при з'єднанні з сервером.")
                }
            })
        }
    });

    //Close modal
    $('body').on('click', '[data-modal-dismiss="true"]', function () {
        $('#remove_tag_modal').hide().css('opacity', 0);
        currentRemoveId = null;
    });
});


