// =======================
// LIKE BUTTON
// =======================
function like(btn) {
    let icon = btn.querySelector("i");
    let countText = btn.innerText.trim();
    let count = parseInt(countText.replace(/\D/g, ""));

    if (!btn.classList.contains("liked")) {
        btn.classList.add("liked");
        icon.classList.remove("bi-heart");
        icon.classList.add("bi-heart-fill");
        btn.style.color = "red";
        btn.innerHTML = `<i class="bi bi-heart-fill"></i> ${count + 1}`;
    } else {
        btn.classList.remove("liked");
        icon.classList.remove("bi-heart-fill");
        icon.classList.add("bi-heart");
        btn.style.color = "";
        btn.innerHTML = `<i class="bi bi-heart"></i> ${count - 1}`;
    }
}


// =======================
// POST (ĐĂNG BÀI)
// =======================
function createPost() {
    let textarea = document.querySelector(".composer-input");
    let content = textarea.value.trim();

    if (content === "") {
        alert("Viết gì đó đi bạn 😄");
        return;
    }

    let feed = document.querySelector(".post-composer");

    let newPost = document.createElement("div");
    newPost.className = "bg-white post-card mb-3";

    newPost.innerHTML = `
        <div class="p-3">
            <div class="d-flex gap-3">
                <img src="https://i.pravatar.cc/60" class="avatar">
                <div>
                    <div class="fw-semibold">Bạn • vừa xong</div>
                    <p class="post-text">${content}</p>

                    <div class="post-actions d-flex gap-4">
                        <button onclick="like(this)">
                            <i class="bi bi-heart"></i> 0
                        </button>
                        <button><i class="bi bi-chat"></i> 0</button>
                        <button><i class="bi bi-arrow-repeat"></i></button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // chèn lên đầu feed
    feed.after(newPost);

    // clear textarea
    textarea.value = "";
}