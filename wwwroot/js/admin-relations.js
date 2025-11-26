// Функції для роботи з модальним вікном додавання Relation
document.addEventListener('DOMContentLoaded', function () {
    console.log('🔄 DOM завантажено - ініціалізація модального вікна');
    initializeRelationModal();
});

function initializeRelationModal() {
    const addMoreBtn = document.getElementById('addMoreBtn');
    const closeModalBtn = document.getElementById('closeModalBtn');
    const cancelModalBtn = document.getElementById('cancelModalBtn');
    const modal = document.getElementById('addRelationModal');
    const addRelationForm = document.getElementById('addRelationForm');
    const newRelationInput = document.getElementById('newRelationInput');

    console.log('Елементи модального вікна:', {
        addMoreBtn: !!addMoreBtn,
        closeModalBtn: !!closeModalBtn,
        cancelModalBtn: !!cancelModalBtn,
        modal: !!modal,
        addRelationForm: !!addRelationForm,
        newRelationInput: !!newRelationInput
    });

    // Відкриття модального вікна
    if (addMoreBtn) {
        addMoreBtn.addEventListener('click', openAddRelationModal);
        console.log('✅ Обробник додано для кнопки "Add more"');
    }

    // Закриття модального вікна
    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', closeAddRelationModal);
    }

    if (cancelModalBtn) {
        cancelModalBtn.addEventListener('click', closeAddRelationModal);
    }

    // Закриття при кліку на фон
    if (modal) {
        modal.addEventListener('click', function (e) {
            if (e.target === this) {
                closeAddRelationModal();
            }
        });
    }

    // Обробка форми додавання Relation
    if (addRelationForm) {
        addRelationForm.addEventListener('submit', handleAddRelationSubmit);
    }

    // Закриття по ESC
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            const modal = document.getElementById('addRelationModal');
            if (modal && modal.style.display === 'block') {
                closeAddRelationModal();
            }
        }
    });
}

function openAddRelationModal() {
    console.log('🔓 Відкриття модального вікна...');
    const modal = document.getElementById('addRelationModal');
    const newRelationInput = document.getElementById('newRelationInput');

    if (modal) {
        modal.style.display = 'block';

        // Фокус на поле вводу
        if (newRelationInput) {
            setTimeout(() => {
                newRelationInput.focus();
            }, 100);
        }
    }
}

function closeAddRelationModal() {
    console.log('🔒 Закриття модального вікна...');
    const modal = document.getElementById('addRelationModal');
    const newRelationInput = document.getElementById('newRelationInput');

    if (modal) {
        modal.style.display = 'none';
    }
    if (newRelationInput) {
        newRelationInput.value = '';
    }
}

function handleAddRelationSubmit(e) {
    e.preventDefault();
    console.log('📨 Обробка форми додавання Relation');

    const newRelationInput = document.getElementById('newRelationInput');
    const newRelation = newRelationInput ? newRelationInput.value.trim() : '';

    if (!newRelation) {
        alert('Будь ласка, введіть назву Relation');
        return;
    }

    // Валідація: тільки латинські літери, цифри та дефіси
    if (!/^[a-zA-Z0-9\-]+$/.test(newRelation)) {
        alert('Назва може містити тільки латинські літери, цифри та дефіси');
        return;
    }

    addNewRelation(newRelation);
}

function addNewRelation(newRelation) {
    console.log('🔄 Відправка AJAX запиту для:', newRelation);

    // Відправляємо AJAX запит для додавання нового Relation
    fetch('/Admin/AddNewRelation', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: 'newRelation=' + encodeURIComponent(newRelation)
    })
        .then(response => {
            if (response.ok) {
                return response.text();
            } else {
                throw new Error('Network response was not ok');
            }
        })
        .then(() => {
            // Додаємо нове Relation до select
            addRelationToSelect(newRelation);
            closeAddRelationModal();
            showSuccessMessage('Нове Relation успішно додано!');
        })
        .catch(error => {
            console.error('Error:', error);
            showErrorMessage('Помилка при додаванні Relation');
        });
}

function addRelationToSelect(newRelation) {
    const select = document.getElementById('relationSelect');

    if (!select) {
        console.error('❌ Select не знайдено!');
        return;
    }

    console.log('➕ Додаємо Relation до select:', newRelation);

    // ВИПРАВЛЕННЯ: безпечна перевірка на існування опції
    let existingOption = null;
    try {
        const options = Array.from(select.options);
        existingOption = options.find(option => option.value === newRelation);
    } catch (error) {
        console.error('Помилка при пошуку опції:', error);
    }

    // Якщо опція не існує - створюємо нову
    if (!existingOption) {
        console.log('✅ Створюємо нову опцію');
        const newOption = document.createElement('option');
        newOption.value = newRelation;
        newOption.textContent = newRelation;
        select.appendChild(newOption);
    } else {
        console.log('ℹ️ Така опція вже існує в select');
    }

    // Вибираємо нове значення
    select.value = newRelation;
    console.log('✅ Вибрано нове значення в select:', newRelation);
}

function showSuccessMessage(message) {
    console.log('✅ ' + message);
    alert('✅ ' + message);
}

function showErrorMessage(message) {
    console.error('❌ ' + message);
    alert('❌ ' + message);
}