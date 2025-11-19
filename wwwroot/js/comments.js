// Глобальні змінні для відстеження стану
let currentReplyTo = null;

// Чекаємо поки DOM завантажиться
document.addEventListener('DOMContentLoaded', function () {
    loadComments();
    initializeCommentForm();
});

function loadComments() {
    try {
        var slugElement = document.getElementById('postSlug');
        if (!slugElement) {
            throw new Error('postSlug element not found');
        }

        var slug = slugElement.value;

        if (!slug) {
            throw new Error('Slug is empty');
        }

        var container = document.getElementById('commentsContainer');
        if (container) container.innerHTML = '<p style="color: blue;">🔄 Loading comments from API...</p>';

        // Використовуємо fetch API (без jQuery)
        fetch('/Ajax/GetAllComments?slug=' + encodeURIComponent(slug))
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Network error: ' + response.status);
                }
                return response.json();
            })
            .then(function (data) {
                if (data.Code === 200) {
                    var comments = JSON.parse(data.Data);
                    console.log('📝 Comments loaded:', comments.length);
                    displayComments(comments);
                } else {
                    throw new Error('API error: ' + data.Message);
                }
            })
            .catch(function (error) {
                if (container) container.innerHTML = '<p style="color: red;">❌ Error: ' + error.message + '</p>';
            });

    } catch (error) {
        console.error('❌ Error:', error);
        var container = document.getElementById('commentsContainer');
        if (container) container.innerHTML = '<p style="color: red;">❌ Error: ' + error.message + '</p>';
    }
}


function displayComments(comments) {
    var container = document.getElementById('commentsContainer');

    if (!comments || comments.length === 0) {
        container.innerHTML = '<p>No comments yet. Be the first to comment!</p>';
        return;
    }

    var html = '<h4 class="mb-4">Comments</h4>';

    comments.forEach(function (comment) {
        var margin = (comment.Level || 0) * 30;

        // Простий індикатор вкладеності
        var levelIndicator = '';
        if (comment.Level > 0) {
            levelIndicator = `
                <div class="level-indicator d-flex align-items-center mb-2">
                    <div class="d-flex">
                        ${'<i class="bi bi-arrow-return-right text-muted me-1"></i>'.repeat(comment.Level)}
                    </div>
                    <small class="text-muted">
                        Reply #${comment.Level}
                    </small>
                </div>
            `;
        }

        var userAvatar = comment.UserAvatar && comment.UserAvatar.trim() !== ''
            ? comment.UserAvatar
            : '/img/user.jpg';

        html += `
            <div class="mb-3" style="margin-left: ${margin}px;">
                <div class="bg-light rounded p-3 border">
                    ${levelIndicator}
                    <div class="d-flex justify-content-between align-items-start">
                        <div class="d-flex align-items-center">
                            <img src="${userAvatar}" class="rounded me-2" style="width: 40px; height: 40px; object-fit: cover;">
                            <div>
                                <h6 class="mb-0">${comment.UserLogin || 'Anonymous'}</h6>
                                <small class="text-muted">${new Date(comment.DateOfCreated).toLocaleString()}</small>
                            </div>
                        </div>
                        <button class="btn btn-outline-primary btn-sm reply-btn"
                                data-comment-id="${comment.Id}"
                                data-user-name="${comment.UserLogin || 'Anonymous'}">
                            <i class="bi bi-reply me-1"></i>Reply
                        </button>
                    </div>
                    <p class="mt-2 mb-0">${comment.Text || 'No text'}</p>
                </div>
            </div>
        `;
    });

    container.innerHTML = html;
    setupReplyButtons();
}
function initializeCommentForm() {
    const form = document.getElementById('commentForm');

    if (!form) {
        return;
    }

    // Обробка відправки форми
    form.addEventListener('submit', function (e) {
        console.log('🔄 Form submit intercepted, preventing default');
        e.preventDefault();
        submitComment();
    });
}

function setupReplyButtons() {
    // Додаємо невелику затримку, щоб гарантувати, що кнопки вже відрендерені
    setTimeout(() => {
        const replyButtons = document.querySelectorAll('.reply-btn');

        replyButtons.forEach(button => {
            button.addEventListener('click', function () {
                const commentId = this.getAttribute('data-comment-id');
                const userName = this.getAttribute('data-user-name');

                replyToComment(commentId, userName);
            });
        });
    }, 100);
}

function replyToComment(commentId, userName) {
    currentReplyTo = commentId;

    // Оновлюємо UI для показу режиму відповіді
    document.getElementById('parentCommentId').value = commentId;
    document.getElementById('replyToUser').textContent = userName;
    document.getElementById('replyCancel').classList.remove('d-none');

    // Прокручуємо до форми
    document.getElementById('commentForm').scrollIntoView({ behavior: 'smooth' });

    // Фокусуємося на тексті коментаря
    document.getElementById('commentText').focus();
}

function cancelReply() {
    currentReplyTo = null;
    document.getElementById('parentCommentId').value = '';
    document.getElementById('replyCancel').classList.add('d-none');
    console.log('✅ Reply cancelled');
}

function submitComment() {
    const submitBtn = document.getElementById('submitComment');
    const submitText = document.getElementById('submitText');
    const spinner = document.getElementById('submitSpinner');

    // Показуємо спінер
    submitBtn.disabled = true;
    submitText.textContent = 'Posting Comment...';
    spinner.classList.remove('d-none');

    const formData = {
        userName: document.getElementById('userName').value,
        userEmail: document.getElementById('userEmail').value,
        commentText: document.getElementById('commentText').value,
        parentCommentId: document.getElementById('parentCommentId').value || null,
        postSlug: document.getElementById('postSlug').value
    };

    // Відправляємо на сервер
    fetch('/Ajax/AddComment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData)
    })
        .then(response => {
            return response.json();
        })
        .then(data => {
            if (data.Code === 200) {
                // Очищаємо форму
                document.getElementById('commentForm').reset();
                cancelReply();

                // Показуємо повідомлення про успіх
                showAlert('Comment added successfully!', 'success');

                // Перезавантажуємо коментарі
                setTimeout(() => {
                    loadComments();
                }, 1000);
            } else {
                showAlert('Error: ' + data.Message, 'error');
            }
        })
        .catch(error => {
            showAlert('Network error. Please try again.', 'error');
        })
        .finally(() => {
            // Ховаємо спінер
            submitBtn.disabled = false;
            submitText.textContent = 'Leave Your Comment';
            spinner.classList.add('d-none');
        });
}

// Функція для показу сповіщень
function showAlert(message, type) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show`;
    alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

    const formContainer = document.querySelector('.bg-light.rounded.p-5');
    if (formContainer) {
        formContainer.prepend(alertDiv);
    }

    // Автоматично ховаємо через 5 секунд
    setTimeout(() => {
        if (alertDiv.parentElement) {
            alertDiv.remove();
        }
    }, 5000);
}





















//// Глобальні змінні для відстеження стану
//let currentReplyTo = null;

//// Чекаємо поки DOM завантажиться
//document.addEventListener('DOMContentLoaded', function () {
//    loadComments();
//    initializeCommentForm();
//});

//function loadComments() {
//    try {
//        var slugElement = document.getElementById('postSlug');
//        if (!slugElement) {
//            throw new Error('postSlug element not found');
//        }

//        var slug = slugElement.value;

//        if (!slug) {
//            throw new Error('Slug is empty');
//        }

//        var container = document.getElementById('commentsContainer');
//        if (container) container.innerHTML = '<p style="color: blue;">🔄 Loading comments from API...</p>';

//        // Використовуємо fetch API (без jQuery)
//        fetch('/Ajax/GetAllComments?slug=' + encodeURIComponent(slug))
//            .then(function (response) {
//                if (!response.ok) {
//                    throw new Error('Network error: ' + response.status);
//                }
//                return response.json();
//            })
//            .then(function (data) {
//                if (data.Code === 200) {
//                    var comments = JSON.parse(data.Data);
//                    console.log('📝 Comments loaded:', comments.length);
//                    displayComments(comments);
//                } else {
//                    throw new Error('API error: ' + data.Message);
//                }
//            })
//            .catch(function (error) {
//                if (container) container.innerHTML = '<p style="color: red;">❌ Error: ' + error.message + '</p>';
//            });

//    } catch (error) {
//        console.error('❌ Error:', error);
//        var container = document.getElementById('commentsContainer');
//        if (container) container.innerHTML = '<p style="color: red;">❌ Error: ' + error.message + '</p>';
//    }
//}

//function displayComments(comments) {
//    var container = document.getElementById('commentsContainer');

//    if (!comments || comments.length === 0) {
//        container.innerHTML = '<p>No comments yet. Be the first to comment!</p>';
//        return;
//    }

//    var html = '<h4 class="mb-4">Comments</h4>';

//    // Відображаємо коментарі з правильним вкладенням
//    comments.forEach(function (comment) {
//        var margin = (comment.Level || 0) * 40; // Відступ залежить від рівня
//        var borderClass = (comment.Level || 0) > 0 ? 'border-start border-3 ps-3' : '';
//        var replyIndicator = (comment.Level || 0) > 0 ? '<small class="text-primary ms-2">↳ reply</small>' : '';

//        html += `
//                <div class="d-flex mb-4 ${borderClass}" style="margin-left: ${margin}px;">
//                    <img src="/img/user.jpg" class="rounded me-3" style="width: 45px; height: 45px;">
//                    <div class="flex-grow-1">
//                        <div class="d-flex justify-content-between align-items-start">
//                            <h6 class="mb-1">${comment.UserLogin || 'Anonymous'}</h6>
//                            <button class="btn btn-outline-primary btn-sm reply-btn"
//                                    data-comment-id="${comment.Id}"
//                                    data-user-name="${comment.UserLogin || 'Anonymous'}">
//                                <i class="bi bi-reply me-1"></i>Reply
//                            </button>
//                        </div>
//                        <p class="mb-1">${comment.Text || 'No text'}</p>
//                        <small class="text-muted">${new Date(comment.DateOfCreated).toLocaleString()}</small>
//                        ${replyIndicator}
//                    </div>
//                </div>
//            `;
//    });

//    container.innerHTML = html;

//    // Додаємо обробники подій для кнопок Reply
//    setupReplyButtons();
//}

//function initializeCommentForm() {
//    const form = document.getElementById('commentForm');

//    if (!form) {
//        return;
//    }

//    // Обробка відправки форми
//    form.addEventListener('submit', function (e) {
//        console.log('🔄 Form submit intercepted, preventing default');
//        e.preventDefault();
//        submitComment();
//    });
//}

//function setupReplyButtons() {
//    // Додаємо невелику затримку, щоб гарантувати, що кнопки вже відрендерені
//    setTimeout(() => {
//        const replyButtons = document.querySelectorAll('.reply-btn');

//        replyButtons.forEach(button => {
//            button.addEventListener('click', function () {
//                const commentId = this.getAttribute('data-comment-id');
//                const userName = this.getAttribute('data-user-name');

//                replyToComment(commentId, userName);
//            });
//        });
//    }, 100);
//}

//function replyToComment(commentId, userName) {
//    currentReplyTo = commentId;

//    // Оновлюємо UI для показу режиму відповіді
//    document.getElementById('parentCommentId').value = commentId;
//    document.getElementById('replyToUser').textContent = userName;
//    document.getElementById('replyCancel').classList.remove('d-none');

//    // Прокручуємо до форми
//    document.getElementById('commentForm').scrollIntoView({ behavior: 'smooth' });

//    // Фокусуємося на тексті коментаря
//    document.getElementById('commentText').focus();
//}

//function cancelReply() {
//    currentReplyTo = null;
//    document.getElementById('parentCommentId').value = '';
//    document.getElementById('replyCancel').classList.add('d-none');
//    console.log('✅ Reply cancelled');
//}

//function submitComment() {
//    const submitBtn = document.getElementById('submitComment');
//    const submitText = document.getElementById('submitText');
//    const spinner = document.getElementById('submitSpinner');

//    // Показуємо спінер
//    submitBtn.disabled = true;
//    submitText.textContent = 'Posting Comment...';
//    spinner.classList.remove('d-none');

//    const formData = {
//        userName: document.getElementById('userName').value,
//        userEmail: document.getElementById('userEmail').value,
//        commentText: document.getElementById('commentText').value,
//        parentCommentId: document.getElementById('parentCommentId').value || null,
//        postSlug: document.getElementById('postSlug').value
//    };

//    // Відправляємо на сервер
//    fetch('/Ajax/AddComment', {
//        method: 'POST',
//        headers: {
//            'Content-Type': 'application/json',
//        },
//        body: JSON.stringify(formData)
//    })
//        .then(response => {
//            return response.json();
//        })
//        .then(data => {
//            if (data.Code === 200) {
//                // Очищаємо форму
//                document.getElementById('commentForm').reset();
//                cancelReply();

//                // Показуємо повідомлення про успіх
//                showAlert('Comment added successfully!', 'success');

//                // Перезавантажуємо коментарі
//                setTimeout(() => {
//                    loadComments();
//                }, 1000);
//            } else {
//                showAlert('Error: ' + data.Message, 'error');
//            }
//        })
//        .catch(error => {
//            showAlert('Network error. Please try again.', 'error');
//        })
//        .finally(() => {
//            // Ховаємо спінер
//            submitBtn.disabled = false;
//            submitText.textContent = 'Leave Your Comment';
//            spinner.classList.add('d-none');
//        });
//}

//// Функція для показу сповіщень
//function showAlert(message, type) {
//    const alertDiv = document.createElement('div');
//    alertDiv.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show`;
//    alertDiv.innerHTML = `
//            ${message}
//            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
//        `;

//    const formContainer = document.querySelector('.bg-light.rounded.p-5');
//    if (formContainer) {
//        formContainer.prepend(alertDiv);
//    }

//    // Автоматично ховаємо через 5 секунд
//    setTimeout(() => {
//        if (alertDiv.parentElement) {
//            alertDiv.remove();
//        }
//    }, 5000);
//}