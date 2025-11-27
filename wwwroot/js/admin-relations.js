// ===== GLOBAL FUNCTIONS =====
window.openDeleteModal = function (optionId, optionName) {
    console.log('🔓 Opening delete modal:', optionId, optionName);
    const deleteOptionId = document.getElementById('deleteOptionId');
    const deleteOptionName = document.getElementById('deleteOptionName');
    const deleteModal = document.getElementById('deleteModal');

    if (deleteOptionId && deleteOptionName && deleteModal) {
        deleteOptionId.value = optionId;
        deleteOptionName.textContent = optionName;
        deleteModal.style.display = 'block';
        document.body.style.overflow = 'hidden';
    }
}

window.closeDeleteModal = function () {
    console.log('🔒 Closing delete modal');
    const deleteModal = document.getElementById('deleteModal');
    if (deleteModal) {
        deleteModal.style.display = 'none';
        document.body.style.overflow = '';
    }
}

window.openAddOptionModal = function () {
    console.log('🔓 Opening add option modal');
    const modal = document.getElementById('addOptionModal');
    if (modal) {
        modal.style.display = 'block';
        document.body.style.overflow = 'hidden';
        // Завантажити Relations при відкритті модального вікна
        loadRelationsToSelect();
    }
}

window.closeAddOptionModal = function () {
    console.log('🔒 Closing add option modal');
    const modal = document.getElementById('addOptionModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = '';
    }
}

window.openAddRelationModal = function () {
    console.log('🔓 Opening add relation modal');
    const modal = document.getElementById('addRelationModal');
    if (modal) {
        modal.style.display = 'block';
        document.body.style.overflow = 'hidden';
        // Очистити поле вводу
        const input = document.getElementById('newRelationInput');
        if (input) input.value = '';
    }
}

window.closeAddRelationModal = function () {
    console.log('🔒 Closing add relation modal');
    const modal = document.getElementById('addRelationModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = '';
    }
}

// ===== RELATION MANAGEMENT =====
async function loadRelationsToSelect() {
    console.log('🔄 Loading relations to select...');
    try {
        const response = await fetch('/Admin/GetRelations');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const relations = await response.json();

        const select = document.getElementById('relationSelect');
        if (select) {
            // Зберегти поточне значення ПЕРЕД очищенням
            const currentValue = select.value;

            // Очистити опції (крім першої)
            while (select.options.length > 1) {
                select.remove(1);
            }

            // Додати нові опції
            relations.forEach(relation => {
                const option = document.createElement('option');
                option.value = relation;
                option.textContent = relation;
                select.appendChild(option);
            });

            // Відновити поточне значення, якщо воно ще існує
            if (currentValue && relations.includes(currentValue)) {
                select.value = currentValue;
            }

            console.log(`✅ Loaded ${relations.length} relations to select`);
        }
    } catch (error) {
        console.error('❌ Error loading relations:', error);
    }
}

// ===== ADD RELATION FUNCTIONALITY =====
function handleAddRelationSubmit(e) {
    e.preventDefault();
    console.log('🔄 Handling relation form submit...');

    const newRelationInput = document.getElementById('newRelationInput');
    if (!newRelationInput) {
        console.error('❌ Relation input not found');
        return;
    }

    const newRelation = newRelationInput.value.trim();

    if (!newRelation) {
        alert('Будь ласка, введіть назву Relation');
        return;
    }

    // Валідація: тільки латинські літери, цифри, дефіси
    const relationRegex = /^[a-zA-Z0-9-]+$/;
    if (!relationRegex.test(newRelation)) {
        alert('Назва Relation може містити тільки латинські літери, цифри та дефіси');
        return;
    }

    addNewRelation(newRelation);
}

async function addNewRelation(newRelation) {
    console.log('🔄 Adding new relation:', newRelation);

    try {
        const response = await fetch('/Admin/AddNewRelation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `newRelation=${encodeURIComponent(newRelation)}`
        });

        if (response.ok) {
            console.log('✅ Relation added successfully');

            // ВИПРАВЛЕННЯ: Зачекати поки оновиться список і тоді встановити значення
            await loadRelationsToSelect();

            // Даємо трохи часу для оновлення DOM
            setTimeout(() => {
                const relationSelect = document.getElementById('relationSelect');
                if (relationSelect) {
                    // Шукаємо опцію з потрібним значенням
                    const options = Array.from(relationSelect.options);
                    const targetOption = options.find(option => option.value === newRelation);

                    if (targetOption) {
                        relationSelect.value = newRelation;
                        console.log(`✅ Successfully set relation select to: ${newRelation}`);
                    } else {
                        console.warn(`⚠️ Option ${newRelation} not found in select after reload`);
                        // Спробуємо додати вручну
                        const newOption = document.createElement('option');
                        newOption.value = newRelation;
                        newOption.textContent = newRelation;
                        relationSelect.appendChild(newOption);
                        relationSelect.value = newRelation;
                        console.log(`✅ Manually added and set relation: ${newRelation}`);
                    }
                }
            }, 100);

            // Закрити модальне вікно
            closeAddRelationModal();

            // Показати повідомлення про успіх
           // alert(`Relation "${newRelation}" успішно додано!`);

        } else {
            const errorText = await response.text();
            console.error('❌ Error adding relation:', errorText);
            alert('Помилка при додаванні Relation: ' + errorText);
        }
    } catch (error) {
        console.error('❌ Network error:', error);
        alert('Мережева помилка при додаванні Relation');
    }
}

function initializeRelationModal() {
    console.log('🔄 Initializing relation modal...');

    const addRelationForm = document.getElementById('addRelationForm');
    const closeModalBtn = document.getElementById('closeModalBtn');
    const cancelModalBtn = document.getElementById('cancelModalBtn');
    const addRelationModal = document.getElementById('addRelationModal');
    const addMoreBtn = document.getElementById('addMoreBtn');

    // Для EditOption сторінки - кнопка "Add more"
    if (addMoreBtn) {
        addMoreBtn.addEventListener('click', openAddRelationModal);
        console.log('✅ Add more button initialized');
    }

    if (addRelationForm) {
        addRelationForm.addEventListener('submit', handleAddRelationSubmit);
        console.log('✅ Relation form initialized');
    }

    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', closeAddRelationModal);
    }

    if (cancelModalBtn) {
        cancelModalBtn.addEventListener('click', closeAddRelationModal);
    }

    // Закрити при кліку на фон
    if (addRelationModal) {
        addRelationModal.addEventListener('click', function (e) {
            if (e.target === this) {
                closeAddRelationModal();
            }
        });
    }

    console.log('✅ Relation modal initialized');
}

// ===== DELETE MODAL FUNCTIONS =====
function initializeDeleteModal() {
    const deleteModal = document.getElementById('deleteModal');

    console.log('🔧 Ініціалізація delete modal:', deleteModal);

    if (deleteModal) {
        // Закрити при кліку на фон
        deleteModal.addEventListener('click', function (e) {
            if (e.target === this) {
                closeDeleteModal();
            }
        });

        console.log('✅ Delete modal initialized');
    } else {
        console.log('ℹ️ Delete modal not found (normal for EditOption page)');
    }
}

// ===== ADD OPTION MODAL FUNCTIONS =====
function initializeAddOptionModal() {
    const addOptionModal = document.getElementById('addOptionModal');

    console.log('🔧 Ініціалізація add option modal:', addOptionModal);

    if (addOptionModal) {
        // Закрити при кліку на фон
        addOptionModal.addEventListener('click', function (e) {
            if (e.target === this) {
                closeAddOptionModal();
            }
        });

        console.log('✅ Add option modal initialized');
    } else {
        console.log('ℹ️ Add option modal not found (normal for EditOption page)');
    }
}

// ===== UTILITY FUNCTIONS =====
function showSuccessMessage(message) {
    console.log('✅ ' + message);
    alert('✅ ' + message);
}

function showErrorMessage(message) {
    console.error('❌ ' + message);
    alert('❌ ' + message);
}

// ===== INITIALIZATION =====
document.addEventListener('DOMContentLoaded', function () {
    console.log('🔄 DOM завантажено - ініціалізація всіх модальних вікон');

    // Ініціалізація для всіх сторінок
    initializeDeleteModal();
    initializeAddOptionModal();

    // Ініціалізація для сторінок з Relation modal
    if (document.getElementById('addRelationModal')) {
        initializeRelationModal();
    }

    // Завантажити Relations при завантаженні сторінки (якщо є select)
    if (document.getElementById('relationSelect')) {
        loadRelationsToSelect();
    }

    console.log('✅ All modals initialized');
});

// ===== GLOBAL EVENT LISTENERS =====
// Закриття по ESC (для всіх модальних вікон)
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const relationModal = document.getElementById('addRelationModal');
        const deleteModal = document.getElementById('deleteModal');
        const addOptionModal = document.getElementById('addOptionModal');

        if (relationModal && relationModal.style.display === 'block') {
            closeAddRelationModal();
        }

        if (deleteModal && deleteModal.style.display === 'block') {
            closeDeleteModal();
        }

        if (addOptionModal && addOptionModal.style.display === 'block') {
            closeAddOptionModal();
        }
    }
});











//// Функції для роботи з модальним вікном додавання Relation
//document.addEventListener('DOMContentLoaded', function () {
//    console.log('🔄 DOM завантажено - ініціалізація модального вікна');
//    initializeRelationModal();
//});

//function initializeRelationModal() {
//    const addMoreBtn = document.getElementById('addMoreBtn');
//    const closeModalBtn = document.getElementById('closeModalBtn');
//    const cancelModalBtn = document.getElementById('cancelModalBtn');
//    const modal = document.getElementById('addRelationModal');
//    const addRelationForm = document.getElementById('addRelationForm');
//    const newRelationInput = document.getElementById('newRelationInput');

//    console.log('Елементи модального вікна:', {
//        addMoreBtn: !!addMoreBtn,
//        closeModalBtn: !!closeModalBtn,
//        cancelModalBtn: !!cancelModalBtn,
//        modal: !!modal,
//        addRelationForm: !!addRelationForm,
//        newRelationInput: !!newRelationInput
//    });

//    // Відкриття модального вікна
//    if (addMoreBtn) {
//        addMoreBtn.addEventListener('click', openAddRelationModal);
//        console.log('✅ Обробник додано для кнопки "Add more"');
//    }

//    // Закриття модального вікна
//    if (closeModalBtn) {
//        closeModalBtn.addEventListener('click', closeAddRelationModal);
//    }

//    if (cancelModalBtn) {
//        cancelModalBtn.addEventListener('click', closeAddRelationModal);
//    }

//    // Закриття при кліку на фон
//    if (modal) {
//        modal.addEventListener('click', function (e) {
//            if (e.target === this) {
//                closeAddRelationModal();
//            }
//        });
//    }

//    // Обробка форми додавання Relation
//    if (addRelationForm) {
//        addRelationForm.addEventListener('submit', handleAddRelationSubmit);
//    }

//    // Закриття по ESC
//    document.addEventListener('keydown', function (e) {
//        if (e.key === 'Escape') {
//            const modal = document.getElementById('addRelationModal');
//            if (modal && modal.style.display === 'block') {
//                closeAddRelationModal();
//            }
//        }
//    });
//}

//function openAddRelationModal() {
//    console.log('🔓 Відкриття модального вікна...');
//    const modal = document.getElementById('addRelationModal');
//    const newRelationInput = document.getElementById('newRelationInput');

//    if (modal) {
//        modal.style.display = 'block';

//        // Фокус на поле вводу
//        if (newRelationInput) {
//            setTimeout(() => {
//                newRelationInput.focus();
//            }, 100);
//        }
//    }
//}

//function closeAddRelationModal() {
//    console.log('🔒 Закриття модального вікна...');
//    const modal = document.getElementById('addRelationModal');
//    const newRelationInput = document.getElementById('newRelationInput');

//    if (modal) {
//        modal.style.display = 'none';
//    }
//    if (newRelationInput) {
//        newRelationInput.value = '';
//    }
//}

//function handleAddRelationSubmit(e) {
//    e.preventDefault();
//    console.log('📨 Обробка форми додавання Relation');

//    const newRelationInput = document.getElementById('newRelationInput');
//    const newRelation = newRelationInput ? newRelationInput.value.trim() : '';

//    if (!newRelation) {
//        alert('Будь ласка, введіть назву Relation');
//        return;
//    }

//    // Валідація: тільки латинські літери, цифри та дефіси
//    if (!/^[a-zA-Z0-9\-]+$/.test(newRelation)) {
//        alert('Назва може містити тільки латинські літери, цифри та дефіси');
//        return;
//    }

//    addNewRelation(newRelation);
//}

//function addNewRelation(newRelation) {
//    console.log('🔄 Відправка AJAX запиту для:', newRelation);

//    // Відправляємо AJAX запит для додавання нового Relation
//    fetch('/Admin/AddNewRelation', {
//        method: 'POST',
//        headers: {
//            'Content-Type': 'application/x-www-form-urlencoded',
//        },
//        body: 'newRelation=' + encodeURIComponent(newRelation)
//    })
//        .then(response => {
//            if (response.ok) {
//                return response.text();
//            } else {
//                throw new Error('Network response was not ok');
//            }
//        })
//        .then(() => {
//            // Додаємо нове Relation до select
//            addRelationToSelect(newRelation);
//            closeAddRelationModal();
//            showSuccessMessage('Нове Relation успішно додано!');
//        })
//        .catch(error => {
//            console.error('Error:', error);
//            showErrorMessage('Помилка при додаванні Relation');
//        });
//}

//function addRelationToSelect(newRelation) {
//    const select = document.getElementById('relationSelect');

//    if (!select) {
//        console.error('❌ Select не знайдено!');
//        return;
//    }

//    console.log('➕ Додаємо Relation до select:', newRelation);

//    // ВИПРАВЛЕННЯ: безпечна перевірка на існування опції
//    let existingOption = null;
//    try {
//        const options = Array.from(select.options);
//        existingOption = options.find(option => option.value === newRelation);
//    } catch (error) {
//        console.error('Помилка при пошуку опції:', error);
//    }

//    // Якщо опція не існує - створюємо нову
//    if (!existingOption) {
//        console.log('✅ Створюємо нову опцію');
//        const newOption = document.createElement('option');
//        newOption.value = newRelation;
//        newOption.textContent = newRelation;
//        select.appendChild(newOption);
//    } else {
//        console.log('ℹ️ Така опція вже існує в select');
//    }

//    // Вибираємо нове значення
//    select.value = newRelation;
//    console.log('✅ Вибрано нове значення в select:', newRelation);
//}

//function showSuccessMessage(message) {
//    console.log('✅ ' + message);
//    alert('✅ ' + message);
//}

//function showErrorMessage(message) {
//    console.error('❌ ' + message);
//    alert('❌ ' + message);
//}








//// ===== DELETE MODAL FUNCTIONS =====
//// Робимо функції глобальними
//window.openDeleteModal = function (optionId, optionName) {
//    console.log('🔓 Opening delete modal:', optionId, optionName);

//    const deleteOptionId = document.getElementById('deleteOptionId');
//    const deleteOptionName = document.getElementById('deleteOptionName');
//    const deleteModal = document.getElementById('deleteModal');

//    if (deleteOptionId && deleteOptionName && deleteModal) {
//        deleteOptionId.value = optionId;
//        deleteOptionName.textContent = optionName;
//        deleteModal.style.display = 'block';
//        document.body.style.overflow = 'hidden';
//        console.log('✅ Delete modal opened successfully');
//    } else {
//        console.error('❌ Cannot open delete modal - elements not found');
//    }
//}

//window.closeDeleteModal = function () {
//    console.log('🔒 Closing delete modal');

//    const deleteModal = document.getElementById('deleteModal');
//    if (deleteModal) {
//        deleteModal.style.display = 'none';
//        document.body.style.overflow = '';
//        console.log('✅ Delete modal closed');
//    }
//}

//// Функції для модального вікна видалення
//document.addEventListener('DOMContentLoaded', function () {
//    console.log('🔄 DOM завантажено - ініціалізація delete modal');
//    initializeDeleteModal();
//});

//function initializeDeleteModal() {
//    const deleteModal = document.getElementById('deleteModal');

//    console.log('🔧 Ініціалізація delete modal:', deleteModal);

//    if (deleteModal) {
//        // Закрити при кліку на фон
//        deleteModal.addEventListener('click', function (e) {
//            if (e.target === this) {
//                closeDeleteModal();
//            }
//        });

//        // Закриття по ESC
//        document.addEventListener('keydown', function (e) {
//            if (e.key === 'Escape' && deleteModal.style.display === 'block') {
//                closeDeleteModal();
//            }
//        });

//        console.log('✅ Delete modal initialized');
//    } else {
//        console.error('❌ Delete modal not found!');
//    }
//}