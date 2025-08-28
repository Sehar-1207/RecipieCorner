
document.addEventListener("DOMContentLoaded", function () {
    const reviewForm = document.getElementById('review-form');
    if (!reviewForm) return;

    const starsInputContainer = document.getElementById('rating-stars-input');
    const starIcons = starsInputContainer.querySelectorAll('.fa-star');
    const starsValueInput = document.getElementById('starsValue');
    const recipeIdInput = document.getElementById('recipeId');
    const ratingIdInput = document.getElementById('ratingId');
    const commentInput = document.getElementById('comment');
    const reviewList = document.getElementById('review-list');
    const messageEl = document.getElementById('review-form-message');
    const tokenInput = reviewForm.querySelector('input[name="__RequestVerificationToken"]');

    function setStarsVisual(ratingValue) {
        starIcons.forEach(star => {
            const val = parseInt(star.dataset.value);
            star.classList.toggle('fas', val <= ratingValue);
            star.classList.toggle('far', val > ratingValue);
            star.classList.toggle('filled', val <= ratingValue);
        });
    }

    starIcons.forEach(star => {
        star.addEventListener('click', () => {
            const val = parseInt(star.dataset.value);
            starsValueInput.value = val;
            setStarsVisual(val);
        });
    });

    setStarsVisual(parseInt(starsValueInput.value));

    reviewForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        if (!tokenInput) {
            showMessage('Security token not found. Please refresh the page.', 'danger');
            return;
        }

        const submission = {
            Id: parseInt(ratingIdInput.value) || 0,
            RecipeId: parseInt(recipeIdInput.value),
            Stars: parseInt(starsValueInput.value),
            Comment: commentInput.value.trim()
        };

        if (submission.Stars === 0) {
            showMessage('Please select a star rating.', 'danger');
            return;
        }

        showMessage('Saving...', 'info');

        try {
            const response = await fetch('@Url.Action("CreateOrUpdate", "RatingUi")', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': tokenInput.value
                },
                body: JSON.stringify(submission)
            });

            if (response.ok) {
                const updatedReview = await response.json();
                updateOrAddReviewInDOM(updatedReview);
                showMessage('Review saved successfully!', 'success');
            } else if (response.status === 409) {
                const errorMessage = await response.text();
                showMessage(errorMessage, 'warning');
            }
            else {
                const errorText = await response.text();
                showMessage(`Error: ${response.statusText}`, 'danger');
                console.error('Failed to save review. Server response:', errorText);
            }
        } catch (err) {
            showMessage('A network error occurred. Please check your connection.', 'danger');
            console.error('Fetch API error:', err);
        }
    });

    function updateOrAddReviewInDOM(reviewData) {
        const noReviewsMsg = document.getElementById('no-reviews-message');
        if (noReviewsMsg) noReviewsMsg.remove();

        let reviewEl = reviewList.querySelector(`[data-rating-id="${reviewData.id}"]`);

        const starsHtml = [1, 2, 3, 4, 5].map(i => `<i class="fa-star ${i <= reviewData.stars ? 'fas' : 'far'}"></i>`).join('');
        const reviewDate = new Date(reviewData.commentAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
        const userImageUrl = reviewData.profileImageUrl || '/images/default-author.png';
        const youBadge = '<span class="badge bg-success ms-1 fw-normal">You</span>';

        if (reviewEl) { // Update existing review
            reviewEl.querySelector('.rating-stars').innerHTML = starsHtml;
            reviewEl.querySelector('.review-comment').textContent = reviewData.comment;
            reviewEl.querySelector('.review-date').textContent = reviewDate;
        } else { // Add new review
            const newHtml = `
    <div class="list-group-item p-3 review-item" data-rating-id="${reviewData.id}">
        <div class="d-flex w-100 justify-content-between">
            <div class="review-author">
                <img src="${userImageUrl}" class="rounded-circle" style="width:32px;height:32px;object-fit:cover;" alt="User">
                    <span>${reviewData.userName} ${youBadge}</span>
            </div>
            <span class="rating-stars text-warning">${starsHtml}</span>
        </div>
        <small class="text-muted review-date d-block mt-1 mb-2">${reviewDate}</small>
        <p class="mb-1 review-comment">${reviewData.comment}</p>
    </div>`;
            reviewList.insertAdjacentHTML('afterbegin', newHtml);
            document.getElementById('review-form-title').textContent = 'Edit Your Review';
            ratingIdInput.value = reviewData.id;
        }
    }

    function showMessage(text, type) {
        messageEl.textContent = text;
        messageEl.className = `me-3 text-${type}`;
        const timeout = (type === 'danger' || type === 'warning') ? 5000 : 3000;
        setTimeout(() => messageEl.textContent = '', timeout);
    }
});
