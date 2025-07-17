// Authentication related scripts
document.addEventListener('DOMContentLoaded', function () {
    // Login form validation
    const loginForm = document.querySelector('#login-form');
    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            const email = this.querySelector('input[name="Email"]').value;
            const password = this.querySelector('input[name="Password"]').value;

            if (!email || !password) {
                e.preventDefault();
                showToast('Please fill in all fields', 'error');
            }
        });
    }

    // Registration form validation
    const registerForm = document.querySelector('#register-form');
    if (registerForm) {
        registerForm.addEventListener('submit', function (e) {
            const password = this.querySelector('input[name="Password"]').value;
            const confirmPassword = this.querySelector('input[name="ConfirmPassword"]').value;

            if (password !== confirmPassword) {
                e.preventDefault();
                showToast('Passwords do not match', 'error');
                return false;
            }

            return true;
        });
    }

    // Password visibility toggle
    document.querySelectorAll('.password-toggle').forEach(button => {
        button.addEventListener('click', function () {
            const input = this.previousElementSibling;
            const icon = this.querySelector('i');

            if (input.type === 'password') {
                input.type = 'text';
                icon.classList.remove('fa-eye');
                icon.classList.add('fa-eye-slash');
            } else {
                input.type = 'password';
                icon.classList.remove('fa-eye-slash');
                icon.classList.add('fa-eye');
            }
        });
    });
});

function showToast(message, type) {
    // Implement toast notification or use a library like Toastr
    alert(message); // Simple fallback
}