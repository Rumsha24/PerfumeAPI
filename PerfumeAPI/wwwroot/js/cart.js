// Cart functionality
document.addEventListener('DOMContentLoaded', function () {
    // Update cart count on page load
    updateCartCount();

    // Add to cart buttons
    document.querySelectorAll('.add-to-cart').forEach(button => {
        button.addEventListener('click', function () {
            const productId = this.dataset.id;
            const quantity = this.closest('.product-actions')?.querySelector('.quantity')?.value || 1;

            addToCart(productId, quantity);
        });
    });

    // Quantity adjustments
    document.querySelectorAll('.quantity-increment').forEach(button => {
        button.addEventListener('click', function () {
            const input = this.previousElementSibling;
            input.value = parseInt(input.value) + 1;
        });
    });

    document.querySelectorAll('.quantity-decrement').forEach(button => {
        button.addEventListener('click', function () {
            const input = this.nextElementSibling;
            if (parseInt(input.value) > 1) {
                input.value = parseInt(input.value) - 1;
            }
        });
    });

    // Remove item from cart
    document.querySelectorAll('.remove-cart-item').forEach(button => {
        button.addEventListener('click', function () {
            const itemId = this.dataset.id;
            removeFromCart(itemId);
        });
    });
});

async function addToCart(productId, quantity) {
    try {
        const response = await fetch('/api/cart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                productId: parseInt(productId),
                quantity: parseInt(quantity)
            })
        });

        if (response.ok) {
            showToast('Product added to cart!', 'success');
            updateCartCount();
        } else if (response.status === 401) {
            window.location.href = '/Account/Login?returnUrl=' + window.location.pathname;
        } else {
            const error = await response.json();
            showToast(error.message || 'Failed to add to cart', 'error');
        }
    } catch (error) {
        showToast('An error occurred', 'error');
        console.error('Error:', error);
    }
}

async function removeFromCart(itemId) {
    if (confirm('Are you sure you want to remove this item?')) {
        try {
            const response = await fetch(`/api/cart/${itemId}`, {
                method: 'DELETE'
            });

            if (response.ok) {
                showToast('Item removed from cart', 'success');
                window.location.reload();
            } else {
                const error = await response.json();
                showToast(error.message || 'Failed to remove item', 'error');
            }
        } catch (error) {
            showToast('An error occurred', 'error');
            console.error('Error:', error);
        }
    }
}

async function updateCartCount() {
    try {
        const response = await fetch('/api/cart/count');
        if (response.ok) {
            const data = await response.json();
            document.querySelectorAll('.cart-count').forEach(el => {
                el.textContent = data.count;
            });
        }
    } catch (error) {
        console.error('Error updating cart count:', error);
    }
}

function showToast(message, type) {
    // Implement toast notification or use a library like Toastr
    alert(message); // Simple fallback
}