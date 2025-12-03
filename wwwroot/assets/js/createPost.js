
// createPost.js
// =========== КОНФІГУРАЦІЯ ===========
const CONFIG = {
    maxFileSize: 5 * 1024 * 1024, // 5MB
    previewMaxHeight: '60'
};

// =========== ДОПОМІЖНІ ФУНКЦІЇ ===========
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function showNoImageMessage() {
    const previewContainer = getPreviewContainer();
    previewContainer.innerHTML = `
        <div id="noImageMessage" class="text-sm text-gray-400 italic">
            Введіть URL або виберіть файл для попереднього перегляду
        </div>
    `;
}

function clearAllImageInputs() {
    const imageUrlInput = document.getElementById('imageInput');
    const imageFileInput = document.getElementById('imageFileInput');

    if (imageUrlInput) imageUrlInput.value = '';
    if (imageFileInput) imageFileInput.value = '';

    showNoImageMessage();
}

function getPreviewContainer() {
    return document.getElementById('imagePreviewContainer');
}

// =========== ОСНОВНІ ФУНКЦІЇ ===========
function updateImagePreview(url) {
    const previewContainer = getPreviewContainer();
    const imageFileInput = document.getElementById('imageFileInput');

    // Очищаємо поле файлу, якщо вводимо URL
    if (imageFileInput) {
        imageFileInput.value = '';
    }

    // Очищаємо превью
    previewContainer.innerHTML = '';

    if (url && url.trim() !== '') {
        createUrlImagePreview(url);
    } else {
        showNoImageMessage();
    }
}

function createUrlImagePreview(url) {
    const previewContainer = getPreviewContainer();

    const img = document.createElement('img');
    img.src = url;
    img.alt = 'Попередній перегляд зображення';
    img.className = `max-w-full max-h-${CONFIG.previewMaxHeight} object-contain rounded border border-gray-200`;

    img.onerror = function () {
        previewContainer.innerHTML = `
            <div class="text-sm text-red-500 italic mb-2">
                Не вдалося завантажити зображення з цього URL
            </div>
            <div class="text-xs text-gray-500">
                Введений URL: ${url}
            </div>
        `;
    };

    previewContainer.appendChild(img);
}

function handleFileSelect(input) {
    const file = input.files[0];
    const imageUrlInput = document.getElementById('imageInput');

    // Очищаємо поле URL, якщо вибираємо файл
    if (imageUrlInput) {
        imageUrlInput.value = '';
    }

    if (!file) {
        showNoImageMessage();
        return;
    }

    createFileImagePreview(file);
}

function createFileImagePreview(file) {
    const previewContainer = getPreviewContainer();

    // Перевірка типу файлу
    if (!file.type.match('image.*')) {
        previewContainer.innerHTML = `
            <div class="text-sm text-red-500 italic">
                Будь ласка, виберіть файл зображення (JPG, PNG, GIF тощо)
            </div>
            <div class="text-xs text-gray-500 mt-1">
                Обраний файл: ${file.name}
            </div>
        `;
        return;
    }

    // Перевірка розміру файлу
    if (file.size > CONFIG.maxFileSize) {
        previewContainer.innerHTML = `
            <div class="text-sm text-red-500 italic">
                Файл занадто великий. Максимальний розмір: ${formatFileSize(CONFIG.maxFileSize)}
            </div>
            <div class="text-xs text-gray-500 mt-1">
                Розмір вашого файлу: ${formatFileSize(file.size)}
            </div>
        `;
        return;
    }

    // Читаємо файл
    const reader = new FileReader();

    reader.onload = function (e) {
        previewContainer.innerHTML = '';

        // Створюємо зображення
        const img = document.createElement('img');
        img.src = e.target.result;
        img.alt = 'Попередній перегляд завантаженого зображення';
        img.className = `max-w-full max-h-${CONFIG.previewMaxHeight} object-contain rounded border border-gray-200`;

        // Створюємо інформацію про файл
        const fileInfo = document.createElement('div');
        fileInfo.className = 'text-xs text-gray-500 mt-2';
        fileInfo.innerHTML = `
            <div><strong>Файл:</strong> ${file.name}</div>
            <div><strong>Розмір:</strong> ${formatFileSize(file.size)}</div>
            <div><strong>Тип:</strong> ${file.type}</div>
        `;

        // Додаємо все до контейнера
        previewContainer.appendChild(img);
        previewContainer.appendChild(fileInfo);
    };

    reader.onerror = function () {
        previewContainer.innerHTML = '<div class="text-sm text-red-500 italic">Помилка при читанні файлу</div>';
    };

    reader.readAsDataURL(file);
}

function setupSlugGeneration() {
    const nameInput = document.querySelector('input[name="Name"]');
    const slugInput = document.getElementById('slugInput');

    if (nameInput && slugInput) {
        nameInput.addEventListener('input', function () {
            if (!slugInput.value) {
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
}

function setupClearButton() {
    const previewContainer = getPreviewContainer();
    if (!previewContainer) return;

    const clearButton = document.createElement('button');
    clearButton.type = 'button';
    clearButton.textContent = 'Очистити';
    clearButton.className = 'btn btn-sm btn-light mt-2';
    clearButton.onclick = clearAllImageInputs;

    const previewParent = previewContainer.parentElement;
    if (previewParent) {
        // Перевіряємо, чи кнопка вже існує
        if (!previewParent.querySelector('.clear-image-btn')) {
            clearButton.classList.add('clear-image-btn');
            previewParent.appendChild(clearButton);
        }
    }
}

function initializePage() {
    // Налаштовуємо генерацію slug
    setupSlugGeneration();

    // Налаштовуємо кнопку очищення
    setupClearButton();

    // Перевіряємо наявність зображення при завантаженні
    const imageUrlInput = document.getElementById('imageInput');
    if (imageUrlInput && imageUrlInput.value) {
        updateImagePreview(imageUrlInput.value);
    }

    // Налаштовуємо обробники для файлового input
    const imageFileInput = document.getElementById('imageFileInput');
    if (imageFileInput) {
        imageFileInput.addEventListener('change', function () {
            handleFileSelect(this);
        });
    }

    // Налаштовуємо обробники для URL input
    const urlInput = document.getElementById('imageInput');
    if (urlInput) {
        urlInput.addEventListener('input', function () {
            updateImagePreview(this.value);
        });
    }
}

// =========== ІНІЦІАЛІЗАЦІЯ ===========
document.addEventListener('DOMContentLoaded', initializePage);

// =========== ЕКСПОРТ ФУНКЦІЙ (якщо потрібно) ===========
// Для доступу з інших файлів або консолі
if (typeof window !== 'undefined') {
    window.ImagePreview = {
        update: updateImagePreview,
        handleFile: handleFileSelect,
        clear: clearAllImageInputs,
        formatSize: formatFileSize
    };
}



















