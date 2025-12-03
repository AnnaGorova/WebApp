// editPost.js

function updateImagePreview(imageUrl) {
    const previewContainer = document.getElementById('imagePreviewContainer');
    const noImageMessage = document.getElementById('noImageMessage');
    let previewImage = document.getElementById('previewImage');
    let imageError = document.getElementById('imageError');

    // Якщо URL порожній
    if (!imageUrl || imageUrl.trim() === '') {
        // Показуємо повідомлення про відсутність зображення
        if (!noImageMessage) {
            const messageDiv = document.createElement('div');
            messageDiv.id = 'noImageMessage';
            messageDiv.className = 'text-sm text-gray-400 italic';
            messageDiv.textContent = 'Введіть URL зображення для попереднього перегляду';
            previewContainer.innerHTML = '';
            previewContainer.appendChild(messageDiv);
        } else {
            noImageMessage.style.display = 'block';
        }

        // Приховуємо зображення та помилку
        if (previewImage) previewImage.style.display = 'none';
        if (imageError) imageError.style.display = 'none';
        return;
    }

    // Якщо зображення ще не існує, створюємо його
    if (!previewImage) {
        // Видаляємо повідомлення про відсутність зображення
        if (noImageMessage) {
            noImageMessage.style.display = 'none';
        }

        // Створюємо контейнер для превью
        const wrapper = document.createElement('div');
        wrapper.className = 'image-preview-wrapper';

        // Створюємо заголовок
        const title = document.createElement('div');
        title.className = 'text-xs text-gray-500 mb-1';
        title.textContent = 'Попередній перегляд:';
        wrapper.appendChild(title);

        // Створюємо зображення
        previewImage = document.createElement('img');
        previewImage.id = 'previewImage';
        previewImage.src = imageUrl;
        previewImage.alt = 'Попередній перегляд';
        previewImage.className = 'w-32 h-32 object-cover rounded border';
        previewImage.onerror = function () {
            showImageError();
        };
        wrapper.appendChild(previewImage);

        // Створюємо повідомлення про помилку
        imageError = document.createElement('div');
        imageError.id = 'imageError';
        imageError.className = 'text-xs text-red-500 mt-1';
        imageError.style.display = 'none';
        imageError.textContent = '⚠️ Не вдалося завантажити зображення';
        wrapper.appendChild(imageError);

        previewContainer.innerHTML = '';
        previewContainer.appendChild(wrapper);
    } else {
        // Якщо зображення вже існує, просто оновлюємо src
        previewImage.src = imageUrl;
        previewImage.style.display = 'block';

        // Приховуємо повідомлення про відсутність зображення
        if (noImageMessage) {
            noImageMessage.style.display = 'none';
        }

        // Приховуємо помилку
        if (imageError) {
            imageError.style.display = 'none';
        }
    }

    // Перевіряємо чи зображення завантажується
    checkImageLoad(imageUrl);
}

// Функція для перевірки завантаження зображення
function checkImageLoad(url) {
    const previewImage = document.getElementById('previewImage');
    const imageError = document.getElementById('imageError');

    if (!previewImage) return;

    const img = new Image();
    img.onload = function () {
        // Якщо зображення завантажилось
        if (imageError) {
            imageError.style.display = 'none';
        }
        if (previewImage) {
            previewImage.style.display = 'block';
        }
    };
    img.onerror = function () {
        // Якщо помилка завантаження
        if (imageError) {
            imageError.style.display = 'block';
        }
        if (previewImage) {
            previewImage.style.display = 'none';
        }
    };
    img.src = url;
}

// Функція для показу помилки
function showImageError() {
    const imageError = document.getElementById('imageError');
    const previewImage = document.getElementById('previewImage');

    if (imageError) {
        imageError.style.display = 'block';
    }
    if (previewImage) {
        previewImage.style.display = 'none';
    }
}

// Автоматичне створення slug з назви
document.addEventListener('DOMContentLoaded', function () {
    const nameInput = document.querySelector('input[name="Name"]');
    const slugInput = document.querySelector('input[name="Slug"]');

    if (nameInput && slugInput) {
        nameInput.addEventListener('input', function () {
            if (!slugInput.value || slugInput.value === '@Model.Slug') {
                const slug = this.value
                    .toLowerCase()
                    .replace(/[^\w\sа-яА-ЯіІїЇєЄґҐ-]/g, '')
                    .replace(/\s+/g, '-')
                    .replace(/--+/g, '-')
                    .trim();
                slugInput.value = slug;
            }
        });
    }

    // Оновлюємо превью при завантаженні сторінки (якщо є зображення)
    const imageInput = document.getElementById('imageInput');
    if (imageInput && imageInput.value) {
        updateImagePreview(imageInput.value);
    }
});