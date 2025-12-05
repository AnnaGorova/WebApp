// editPost.js - Спрощений та функціональний код

// editPost.js - Працююча версія з Trumbowyg
$(document).ready(function () {
    console.log('Edit post script loading...');

    // ===== TRUMBOWYG РЕДАКТОР =====
    function initTrumbowygEditor() {
        const editorContainer = $('#editorContainer');
        const contextInput = $('#contextInput');

        if (!editorContainer.length || !contextInput.length) {
            console.log('Editor elements not found');
            return;
        }

        console.log('Initializing Trumbowyg editor...');

        // Перевіряємо, чи Trumbowyg завантажений
        if (typeof $.fn.trumbowyg === 'undefined') {
            console.error('Trumbowyg not loaded! Check script order.');
            showFallbackEditor();
            return;
        }

        try {
            // Спочатку очищаємо контейнер
            editorContainer.empty();

            // Створюємо новий div для редактора
            const editorDiv = $('<div class="trumbowyg-editor"></div>');
            editorContainer.append(editorDiv);

            // Ініціалізуємо Trumbowyg з вимкненим авто-завантаженням зображень
            editorDiv.trumbowyg({
                lang: 'ua',
                btns: [
                    ['viewHTML'],
                    ['undo', 'redo'], // Додано
                    ['formatting'],
                    ['fontsize', 'fontfamily'], // Додано
                    ['strong', 'em', 'del', 'underline'], // Додано underline
                    ['superscript', 'subscript'], // Додано
                    ['link'],
                    ['insertImage'],
                    ['foreColor', 'backColor'], // Кольори тексту/фону
                    ['emoji'], // Емодзі
                    ['justifyLeft', 'justifyCenter', 'justifyRight', 'justifyFull'], // Вирівнювання
                    ['unorderedList', 'orderedList'],
                    ['horizontalRule'],
                    ['removeformat'],
                    ['table'],
                    ['fullscreen']
                ],
                autogrow: true,
                minimalLinks: true,
                urlProtocol: true,
                // ВИМКНУТИ автоматичне завантаження зображень
                imageWidthModalEdit: false,
                checkImageUrls: false,
                // Власна обробка вставки зображень
                imageModal: function () {
                    return true; // Повертаємо true, щоб зупинити стандартну обробку
                },
                plugins: {
                    fontsize: {
                        sizeList: [
                            '14px',
                            '16px',
                            '18px',
                            '20px',
                            '24px',
                            '28px',
                            '32px'
                        ]
                    },
                    fontfamily: {
                        fontList: [
                            { name: 'Arial', family: 'Arial, Helvetica, sans-serif' },
                            { name: 'Times New Roman', family: 'Times New Roman, Times, serif' },
                            { name: 'Courier New', family: 'Courier New, Courier, monospace' },
                            { name: 'Georgia', family: 'Georgia, serif' },
                            { name: 'Verdana', family: 'Verdana, Geneva, sans-serif' }
                        ]
                    }
                }
            });

            // Завантажуємо контент
            const initialContent = contextInput.val();
            if (initialContent) {
                // Виправляємо пошкоджені URL зображень
                const fixedContent = initialContent.replace(/src="\/img\/cat-2"/g, 'src="data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2VlZSIvPjwvc3ZnPg=="');
                editorDiv.trumbowyg('html', fixedContent);
                // Оновлюємо оригінальне поле
                contextInput.val(fixedContent);
            }

            // Відстежуємо зміни
            editorDiv.on('tbwchange', function () {
                const html = $(this).trumbowyg('html');
                contextInput.val(html);
                console.log('Editor content updated');
            });

            console.log('Trumbowyg initialized successfully');

        } catch (error) {
            console.error('Error initializing Trumbowyg:', error);
            showFallbackEditor();
        }
    }


    $(document).ready(function () {
        // Коли редактор готовий
        setTimeout(function () {
            // Знаходимо всі зображення в редакторі
            $('.trumbowyg-editor img').each(function () {
                const src = $(this).attr('src');

                // Якщо це відносний шлях
                if (src && src.startsWith('/')) {
                    // Конвертуємо в абсолютний URL
                    const absoluteUrl = window.location.origin + src;
                    $(this).attr('src', absoluteUrl);
                    console.log('Конвертовано:', src, '→', absoluteUrl);
                }
            });
        }, 500); // Невелика затримка для завантаження редактора
    });





    // Фолбек редактор
    function showFallbackEditor() {
        console.log('Showing fallback editor');
        const contextInput = $('#contextInput');
        const editorContainer = $('#editorContainer');

        if (!contextInput.length || !editorContainer.length) return;

        const initialContent = contextInput.val() || '';

        editorContainer.html(`
            <textarea id="fallbackEditor" class="w-full h-64 border rounded p-2">${initialContent}</textarea>
            <div class="text-xs text-gray-500 mt-1">
                Простий текстовий редактор. Для форматування використовуйте HTML
            </div>
        `);

        $('#fallbackEditor').on('input', function () {
            contextInput.val($(this).val());
        });
    }

    // Запускаємо редактор
    initTrumbowygEditor();

    // ===== SLUG ГЕНЕРАЦІЯ =====
    if ($('input[name="Name"]').length && $('input[name="Slug"]').length) {
        const slugInput = $('input[name="Slug"]');
        slugInput.data('original', slugInput.val());

        $('input[name="Name"]').on('input', function () {
            if (!slugInput.val() || slugInput.val() === slugInput.data('original')) {
                const slug = $(this).val()
                    .toLowerCase()
                    .replace(/[^\w\sа-яіїєґ-]/gi, '')
                    .replace(/\s+/g, '-')
                    .replace(/-+/g, '-')
                    .trim();
                slugInput.val(slug);
            }
        });
    }

    // ===== ПРЕВ'Ю ЗОБРАЖЕННЯ =====
    if ($('#imageInput').length && $('#imagePreviewContainer').length) {
        console.log('Initializing image preview...');

        // Функція для прев'ю
        function updatePreview(url, isFile = false) {
            const container = $('#imagePreviewContainer');

            if (!url || url.trim() === '') {
                container.html(`
                    <div class="text-sm text-gray-400 italic p-3">
                        Введіть URL або виберіть файл для попереднього перегляду
                    </div>
                `);
                return;
            }

            let previewUrl = url;
            if (!isFile && url.startsWith('/')) {
                previewUrl = window.location.origin + url;
            }

            container.html(`
                <div class="border rounded p-2">
                    <div class="text-xs text-gray-500 mb-1">Попередній перегляд:</div>
                    <img src="${previewUrl}" 
                         class="w-32 h-32 object-cover rounded"
                         onerror="this.onerror=null; this.src='data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0iI2VlZSIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBmb250LWZhbWlseT0iQXJpYWwiIGZvbnQtc2l6ZT0iMTIiIGZpbGw9IiM5OTkiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGR5PSIuM2VtIj5ubyBpbWFnZTwvdGV4dD48L3N2Zz4='">
                </div>
            `);
        }

        // Поле URL
        $('#imageInput').on('input', function () {
            $('#imageFileInput').val('');
            updatePreview(this.value);
        });

        // Поле файлу
        $('#imageFileInput').on('change', function () {
            if (this.files && this.files[0]) {
                const file = this.files[0];

                if (!file.type.match('image.*')) {
                    alert('Будь ласка, виберіть файл зображення');
                    $(this).val('');
                    return;
                }

                const reader = new FileReader();
                reader.onload = function (e) {
                    $('#imageInput').val('');
                    updatePreview(e.target.result, true);
                };
                reader.readAsDataURL(file);
            }
        });

        // Ініціалізація
        if ($('#imageInput').val()) {
            updatePreview($('#imageInput').val());
        }

        console.log('Image preview initialized');
    }

    // ===== ВАЛІДАЦІЯ ФОРМИ =====
    $('form.edit-post-form').on('submit', function (e) {
        console.log('Form validation started...');

        let isValid = true;
        const errors = [];

        // Назва
        const name = $('input[name="Name"]').val();
        if (!name || name.trim() === '') {
            isValid = false;
            errors.push('Назва є обов\'язковим полем');
            $('input[name="Name"]').addClass('border-red-500');
        } else {
            $('input[name="Name"]').removeClass('border-red-500');
        }

        // Slug
        const slug = $('input[name="Slug"]').val();
        if (!slug || slug.trim() === '') {
            isValid = false;
            errors.push('Slug є обов\'язковим полем');
            $('input[name="Slug"]').addClass('border-red-500');
        } else {
            $('input[name="Slug"]').removeClass('border-red-500');
        }

        // Статус
        const status = $('select[name="PostStatuses"]').val();
        if (!status) {
            isValid = false;
            errors.push('Статус є обов\'язковим полем');
            $('select[name="PostStatuses"]').addClass('border-red-500');
        } else {
            $('select[name="PostStatuses"]').removeClass('border-red-500');
        }

        // Контент
        const content = $('#contextInput').val();
        if (!content || content.trim() === '' ||
            content.trim() === '<p></p>' ||
            content.trim() === '<p><br></p>') {
            isValid = false;
            errors.push('Контент є обов\'язковим полем');
            $('#editorContainer').addClass('border-red-500');
        } else {
            $('#editorContainer').removeClass('border-red-500');
        }

        // Якщо є помилки
        if (!isValid) {
            e.preventDefault();

            // Видаляємо старі помилки
            $('.validation-error').remove();

            // Додаємо нові
            const errorHtml = errors.map(error =>
                `<div class="text-sm text-red-600 mb-1">• ${error}</div>`
            ).join('');

            $('.card-body').prepend(`
                <div class="validation-error bg-red-50 border-l-4 border-red-500 p-4 mb-4">
                    <div class="font-medium text-red-700 mb-2">Знайдено помилки:</div>
                    ${errorHtml}
                </div>
            `);

            // Прокручуємо до помилок
            $('html, body').animate({
                scrollTop: $('.validation-error').offset().top - 100
            }, 500);

            return false;
        }

        console.log('Form validation passed');
        return true;
    });

    // Очищення помилок при зміні полів
    $('input[name="Name"], input[name="Slug"], select[name="PostStatuses"]').on('input change', function () {
        $(this).removeClass('border-red-500');
        $('.validation-error').remove();
    });

    console.log('Edit post script loaded successfully');
});







//// editPost.js - Повний функціонал для редагування посту

//// Глобальні змінні для WYSIWYG редактора
//var trumbowygInitialized = false;

//// Ініціалізація редактора Trumbowyg
//function initTrumbowygEditor() {
//    if (typeof $ === 'undefined' || typeof $.fn.trumbowyg === 'undefined') {
//        console.error('jQuery або Trumbowyg не завантажені');
//        setTimeout(initTrumbowygEditor, 100);
//        return;
//    }

//    // Перевіряємо чи редактор вже ініціалізований
//    if (trumbowygInitialized) return;

//    // Ініціалізуємо редактор
//    $('#editorContainer').trumbowyg({
//        lang: 'ua',
//        btns: [
//            ['viewHTML'],
//            ['formatting'],
//            ['strong', 'em', 'del'],
//            ['link'],
//            ['insertImage'],
//            ['unorderedList', 'orderedList'],
//            ['horizontalRule'],
//            ['removeformat'],
//            ['fullscreen']
//        ],
//        autogrow: true,
//        minimalLinks: true,
//        urlProtocol: false, // Автоматично НЕ додає протокол для зовнішніх URL або треба true
//        removeScriptHost: false, // Не видаляє хост з URL
//        convertUrls: false, // Не конвертує відносні URL в абсолютні

//        // Вкажіть базовий URL вашого сайту
//        baseHref: window.location.origin,

//        // Для картинок додайте префікс
//        imagePath: function () {
//            return '';
//            // АБО return window.location.origin;
//        },


//        plugins: {
//            // Додайте плагін для завантаження зображень
//            upload: {
//                serverPath: '/Admin/UploadImage', // Ваш ендпоінт для завантаження
//                fileFieldName: 'image',
//                urlPropertyName: 'file'
//            }
//        },

//        // Кастомна обробка вставки зображень
//        imageModal: function () {
//            var $modal = $('#trumbowyg-image-modal');
//            var url = $modal.find('.trumbowyg-image-url').val();

//            // Якщо відносний шлях - перетворюємо на абсолютний
//            if (url && url.startsWith('/')) {
//                url = window.location.origin + url;
//            }

//            return url;
//        }
//    });

//    // Заповнюємо контентом
//    var initialContent = $('#contextInput').val();
//    if (initialContent) {
//        $('#editorContainer').trumbowyg('html', initialContent);
//    }

//    // Коли контент змінюється
//    $('#editorContainer').on('tbwchange', function () {
//        var htmlContent = $(this).trumbowyg('html');
//        $('#contextInput').val(htmlContent);
//    });

//    // Перед відправкою форми
//    $('form.edit-post-form').on('submit', function () {
//        if ($('#editorContainer').length) {
//            var htmlContent = $('#editorContainer').trumbowyg('html');
//            $('#contextInput').val(htmlContent);
//        }
//        return true;
//    });

//    trumbowygInitialized = true;
//}

//// Превью зображення
//function updateImagePreview(imageUrl) {
//    const container = document.getElementById('imagePreviewContainer');

//    if (!container) return;

//    if (!imageUrl || imageUrl.trim() === '') {
//        container.innerHTML = `
//            <div id="noImageMessage" class="text-sm text-gray-400 italic">
//                Введіть URL зображення або завантажте файл
//            </div>
//        `;
//        return;
//    }

//    // Показуємо завантаження
//    container.innerHTML = `
//        <div class="flex items-center gap-2 text-sm text-gray-500">
//            <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-500"></div>
//            Завантаження...
//        </div>
//    `;

//    const img = new Image();
//    img.onload = function () {
//        container.innerHTML = `
//            <div class="image-preview-wrapper">
//                <div class="text-xs text-gray-500 mb-1">Попередній перегляд:</div>
//                <img src="${imageUrl}" alt="Попередній перегляд" 
//                     class="w-32 h-32 object-cover rounded border">
//            </div>
//        `;
//    };

//    img.onerror = function () {
//        container.innerHTML = `
//            <div class="text-xs text-red-500 mt-1">
//                ⚠️ Не вдалося завантажити зображення
//            </div>
//        `;
//    };

//    img.src = imageUrl;
//}

//// Генерація slug з назви
//function initSlugGeneration() {
//    $(document).ready(function () {
//        $('input[name="Name"]').on('input', function () {
//            var slugInput = $('input[name="Slug"]');
//            var originalSlug = slugInput.data('original') || '';

//            if (!slugInput.val() || slugInput.val() === originalSlug) {
//                var slug = $(this).val()
//                    .toLowerCase()
//                    .replace(/[^\w\sа-яіїєґ-]/gi, '')
//                    .replace(/\s+/g, '-')
//                    .replace(/-+/g, '-')
//                    .trim();
//                slugInput.val(slug);
//            }
//        });
//    });
//}

//// Ініціалізація превью зображення
//function initImagePreview() {
//    $(document).ready(function () {
//        // Превью зображення з URL
//        $('#imageInput').on('input', function () {
//            updateImagePreview(this.value);
//        });

//        // Завантаження файлу
//        $('input[name="ImageFile"]').on('change', function (e) {
//            var file = this.files[0];
//            if (file) {
//                // Перевірка типу файлу
//                if (!file.type.match('image.*')) {
//                    alert('Будь ласка, виберіть зображення');
//                    $(this).val('');
//                    return;
//                }

//                // Можна додати превью для завантаженого файлу
//                var reader = new FileReader();
//                reader.onload = function (e) {
//                    $('#imageInput').val(''); // Очищаємо URL поле
//                    updateImagePreview(e.target.result);
//                };
//                reader.readAsDataURL(file);
//            }
//        });

//        // Ініціалізація превью
//        var initialImage = $('#imageInput').val();
//        if (initialImage) {
//            updateImagePreview(initialImage);
//        }
//    });
//}

//// Функція для перевірки завантаження зображення (запасний варіант)
//function checkImageLoad(url) {
//    const previewImage = document.getElementById('previewImage');
//    const imageError = document.getElementById('imageError');

//    if (!previewImage) return;

//    const img = new Image();
//    img.onload = function () {
//        // Якщо зображення завантажилось
//        if (imageError) {
//            imageError.style.display = 'none';
//        }
//        if (previewImage) {
//            previewImage.style.display = 'block';
//        }
//    };
//    img.onerror = function () {
//        // Якщо помилка завантаження
//        if (imageError) {
//            imageError.style.display = 'block';
//        }
//        if (previewImage) {
//            previewImage.style.display = 'none';
//        }
//    };
//    img.src = url;
//}

//// Функція для показу помилки (запасний варіант)
//function showImageError() {
//    const imageError = document.getElementById('imageError');
//    const previewImage = document.getElementById('previewImage');

//    if (imageError) {
//        imageError.style.display = 'block';
//    }
//    if (previewImage) {
//        previewImage.style.display = 'none';
//    }
//}

//// Основний ініціалізатор
//function initPostEditor() {
//    $(document).ready(function () {
//        // Ініціалізуємо WYSIWYG редактор
//        if ($('#editorContainer').length && $('#contextInput').length) {
//            initTrumbowygEditor();
//        }

//        // Ініціалізуємо генерацію slug
//        if ($('input[name="Name"]').length && $('input[name="Slug"]').length) {
//            initSlugGeneration();

//            // Зберігаємо оригінальний slug для порівняння
//            var slugInput = $('input[name="Slug"]');
//            slugInput.data('original', slugInput.val());
//        }

//        // Ініціалізуємо превью зображення
//        if ($('#imageInput').length && $('#imagePreviewContainer').length) {
//            initImagePreview();
//        }

//        // Підтвердження при скасуванні
//        $('a[href="/Admin/Posts"]').on('click', function (e) {
//            if (trumbowygInitialized) {
//                var currentContent = $('#editorContainer').trumbowyg('html');
//                var originalContent = $('#contextInput').data('original') || '';

//                if (currentContent !== originalContent) {
//                    if (!confirm('У вас є незбережені зміни. Ви впевнені, що хочете вийти?')) {
//                        e.preventDefault();
//                        return false;
//                    }
//                }
//            }
//            return true;
//        });
//    });
//}

//// Ініціалізація при завантаженні сторінки
//if (document.readyState === 'loading') {
//    document.addEventListener('DOMContentLoaded', initPostEditor);
//} else {
//    initPostEditor();
//}

//// Експортуємо функції для глобального використання
//window.initPostEditor = initPostEditor;
//window.updateImagePreview = updateImagePreview;
//window.initTrumbowygEditor = initTrumbowygEditor;

//// Валідація форми перед відправкою
//function validatePostForm() {
//    let isValid = true;
//    let errorMessages = [];

//    // Перевірка назви
//    const nameInput = $('input[name="Name"]');
//    if (!nameInput.val() || nameInput.val().trim() === '') {
//        isValid = false;
//        errorMessages.push('Назва є обов\'язковим полем');
//        nameInput.addClass('border-red-500');
//    } else {
//        nameInput.removeClass('border-red-500');
//    }

//    // Перевірка slug
//    const slugInput = $('input[name="Slug"]');
//    if (!slugInput.val() || slugInput.val().trim() === '') {
//        isValid = false;
//        errorMessages.push('Slug є обов\'язковим полем');
//        slugInput.addClass('border-red-500');
//    } else {
//        slugInput.removeClass('border-red-500');
//    }

//    // Перевірка статусу
//    const statusSelect = $('select[name="PostStatuses"]');
//    if (!statusSelect.val()) {
//        isValid = false;
//        errorMessages.push('Статус є обов\'язковим полем');
//        statusSelect.addClass('border-red-500');
//    } else {
//        statusSelect.removeClass('border-red-500');
//    }

//    // Перевірка контенту
//    const contextInput = $('#contextInput');
//    if (!contextInput.val() || contextInput.val().trim() === '' ||
//        contextInput.val().trim() === '<p></p>' ||
//        contextInput.val().trim() === '<p><br></p>') {

//        isValid = false;
//        errorMessages.push('Контент є обов\'язковим полем');
//        $('#editorContainer').addClass('border-red-500');
//    } else {
//        $('#editorContainer').removeClass('border-red-500');
//    }

//    // Показуємо помилки
//    if (!isValid) {
//        showValidationErrors(errorMessages);
//        return false;
//    }

//    return true;
//}

//// Показ помилок валідації
//function showValidationErrors(errors) {
//    // Видаляємо старі помилки
//    $('.validation-error').remove();

//    // Додаємо нові помилки
//    errors.forEach(function (error) {
//        $('form.edit-post-form').prepend(`
//            <div class="alert alert-danger mb-4 validation-error">
//                ${error}
//            </div>
//        `);
//    });

//    // Прокручуємо до першої помилки
//    $('html, body').animate({
//        scrollTop: $('.validation-error').first().offset().top - 100
//    }, 500);
//}

//// Додайте в initPostEditor функцію:
//function initPostEditor() {
//    $(document).ready(function () {
//        // ... інший код ...

//        // Валідація форми
//        $('form.edit-post-form').on('submit', function (e) {
//            // Оновлюємо контент з редактора
//            if (trumbowygInitialized) {
//                var htmlContent = $('#editorContainer').trumbowyg('html');
//                $('#contextInput').val(htmlContent);
//            }

//            // Перевіряємо валідацію
//            if (!validatePostForm()) {
//                e.preventDefault(); // Зупиняємо відправку
//                return false;
//            }

//            return true;
//        });

//        // Автоматична валідація при зміні полів
//        $('input[name="Name"], input[name="Slug"], select[name="PostStatuses"]').on('input change', function () {
//            $(this).removeClass('border-red-500');
//            $('.validation-error').remove();
//        });
//    });
//}
//$(document).ready(function () {
//    $('form').on('submit', function () {
//        // Якщо поле ImageFile не заповнене, встановлюємо значення
//        if (!$('input[name="ImageFile"]').val()) {
//            $('input[name="ImageFile"]').val(''); // Пусте значення
//        }
//        return true;
//    });
//});































//// editPost.js

//function updateImagePreview(imageUrl) {
//    const previewContainer = document.getElementById('imagePreviewContainer');
//    const noImageMessage = document.getElementById('noImageMessage');
//    let previewImage = document.getElementById('previewImage');
//    let imageError = document.getElementById('imageError');

//    // Якщо URL порожній
//    if (!imageUrl || imageUrl.trim() === '') {
//        // Показуємо повідомлення про відсутність зображення
//        if (!noImageMessage) {
//            const messageDiv = document.createElement('div');
//            messageDiv.id = 'noImageMessage';
//            messageDiv.className = 'text-sm text-gray-400 italic';
//            messageDiv.textContent = 'Введіть URL зображення для попереднього перегляду';
//            previewContainer.innerHTML = '';
//            previewContainer.appendChild(messageDiv);
//        } else {
//            noImageMessage.style.display = 'block';
//        }

//        // Приховуємо зображення та помилку
//        if (previewImage) previewImage.style.display = 'none';
//        if (imageError) imageError.style.display = 'none';
//        return;
//    }

//    // Якщо зображення ще не існує, створюємо його
//    if (!previewImage) {
//        // Видаляємо повідомлення про відсутність зображення
//        if (noImageMessage) {
//            noImageMessage.style.display = 'none';
//        }

//        // Створюємо контейнер для превью
//        const wrapper = document.createElement('div');
//        wrapper.className = 'image-preview-wrapper';

//        // Створюємо заголовок
//        const title = document.createElement('div');
//        title.className = 'text-xs text-gray-500 mb-1';
//        title.textContent = 'Попередній перегляд:';
//        wrapper.appendChild(title);

//        // Створюємо зображення
//        previewImage = document.createElement('img');
//        previewImage.id = 'previewImage';
//        previewImage.src = imageUrl;
//        previewImage.alt = 'Попередній перегляд';
//        previewImage.className = 'w-32 h-32 object-cover rounded border';
//        previewImage.onerror = function () {
//            showImageError();
//        };
//        wrapper.appendChild(previewImage);

//        // Створюємо повідомлення про помилку
//        imageError = document.createElement('div');
//        imageError.id = 'imageError';
//        imageError.className = 'text-xs text-red-500 mt-1';
//        imageError.style.display = 'none';
//        imageError.textContent = '⚠️ Не вдалося завантажити зображення';
//        wrapper.appendChild(imageError);

//        previewContainer.innerHTML = '';
//        previewContainer.appendChild(wrapper);
//    } else {
//        // Якщо зображення вже існує, просто оновлюємо src
//        previewImage.src = imageUrl;
//        previewImage.style.display = 'block';

//        // Приховуємо повідомлення про відсутність зображення
//        if (noImageMessage) {
//            noImageMessage.style.display = 'none';
//        }

//        // Приховуємо помилку
//        if (imageError) {
//            imageError.style.display = 'none';
//        }
//    }

//    // Перевіряємо чи зображення завантажується
//    checkImageLoad(imageUrl);
//}

//// Функція для перевірки завантаження зображення
//function checkImageLoad(url) {
//    const previewImage = document.getElementById('previewImage');
//    const imageError = document.getElementById('imageError');

//    if (!previewImage) return;

//    const img = new Image();
//    img.onload = function () {
//        // Якщо зображення завантажилось
//        if (imageError) {
//            imageError.style.display = 'none';
//        }
//        if (previewImage) {
//            previewImage.style.display = 'block';
//        }
//    };
//    img.onerror = function () {
//        // Якщо помилка завантаження
//        if (imageError) {
//            imageError.style.display = 'block';
//        }
//        if (previewImage) {
//            previewImage.style.display = 'none';
//        }
//    };
//    img.src = url;
//}

//// Функція для показу помилки
//function showImageError() {
//    const imageError = document.getElementById('imageError');
//    const previewImage = document.getElementById('previewImage');

//    if (imageError) {
//        imageError.style.display = 'block';
//    }
//    if (previewImage) {
//        previewImage.style.display = 'none';
//    }
//}

//// Автоматичне створення slug з назви
//document.addEventListener('DOMContentLoaded', function () {
//    const nameInput = document.querySelector('input[name="Name"]');
//    const slugInput = document.querySelector('input[name="Slug"]');

//    if (nameInput && slugInput) {
//        nameInput.addEventListener('input', function () {
//            if (!slugInput.value || slugInput.value === '@Model.Slug') {
//                const slug = this.value
//                    .toLowerCase()
//                    .replace(/[^\w\sа-яА-ЯіІїЇєЄґҐ-]/g, '')
//                    .replace(/\s+/g, '-')
//                    .replace(/--+/g, '-')
//                    .trim();
//                slugInput.value = slug;
//            }
//        });
//    }

//    // Оновлюємо превью при завантаженні сторінки (якщо є зображення)
//    const imageInput = document.getElementById('imageInput');
//    if (imageInput && imageInput.value) {
//        updateImagePreview(imageInput.value);
//    }
//});