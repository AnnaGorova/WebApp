// editTag.js
console.log("🔍 editTag.js завантажено");

document.addEventListener('DOMContentLoaded', function () {
    const nameInput = document.querySelector('input[name="Name"]');
    const previewElement = document.getElementById('tagPreview');
    const slugInput = document.querySelector('input[name="Slug"]');

    

    // Динамічний попередній перегляд тегу
    if (nameInput && previewElement) {
        nameInput.addEventListener('input', function () {
            const newValue = this.value || nameInput.getAttribute('data-initial-value') || '';
            previewElement.textContent = newValue;
           
        });
    }

    // Автогенерація slug з назви
    if (nameInput && slugInput) {
        nameInput.addEventListener('blur', function () {
            if (!slugInput.value) {
                console.log("🔄 Автогенерація slug...");
                // Генеруємо slug з назви
                const slug = this.value
                    .toLowerCase()
                    .replace(/\s+/g, '-')
                    .replace(/[^\w\-]+/g, '')
                    .replace(/\-\-+/g, '-')
                    .replace(/^-+/, '')
                    .replace(/-+$/, '');
                slugInput.value = slug;
                console.log("✅ Згенеровано slug:", slug);
            } else {
                console.log("ℹ️ Slug вже заповнено, автогенерація пропущена");
            }
        });
    }
});