// Product page interactions
document.addEventListener('DOMContentLoaded', function () {
    // Product image gallery
    const mainImage = document.querySelector('.product-main-image');
    const thumbnails = document.querySelectorAll('.product-thumbnail');

    if (thumbnails.length > 0) {
        thumbnails.forEach(thumbnail => {
            thumbnail.addEventListener('click', function () {
                // Update main image
                mainImage.src = this.src;

                // Update active thumbnail
                thumbnails.forEach(t => t.classList.remove('active'));
                this.classList.add('active');
            });
        });
    }

    // Rating stars
    const ratingStars = document.querySelectorAll('.rating-star');
    if (ratingStars.length > 0) {
        ratingStars.forEach(star => {
            star.addEventListener('click', function () {
                const rating = this.dataset.rating;
                document.querySelector('input[name="Rating"]').value = rating;

                // Update star display
                ratingStars.forEach((s, index) => {
                    if (index < rating) {
                        s.classList.add('active');
                    } else {
                        s.classList.remove('active');
                    }
                });
            });

            star.addEventListener('mouseover', function () {
                const rating = this.dataset.rating;
                ratingStars.forEach((s, index) => {
                    if (index < rating) {
                        s.classList.add('hover');
                    } else {
                        s.classList.remove('hover');
                    }
                });
            });

            star.addEventListener('mouseout', function () {
                ratingStars.forEach(s => s.classList.remove('hover'));
            });
        });
    }

    // Review form submission
    const reviewForm = document.querySelector('#review-form');
    if (reviewForm) {
        reviewForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            const formData = new FormData(this);
            try {
                const response = await fetch(this.action, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    window.location.reload();
                } else {
                    const error = await response.json();
                    showToast(error.message || 'Failed to submit review', 'error');
                }
            } catch (error) {
                showToast('An error occurred', 'error');
                console.error('Error:', error);
            }
        });
    }
});