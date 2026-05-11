/* ============================================================
   VOLTEX — Categories Create / Edit JavaScript
   wwwroot/js/categories-create.js
   ============================================================ */

(function initCategoryForm() {
    /* ── Live Preview: Name ── */
    const nameInput  = document.getElementById('Name');
    const previewNm  = document.getElementById('cfpName');

    if (nameInput && previewNm) {
        nameInput.addEventListener('input', () => {
            previewNm.textContent = nameInput.value.trim() || 'Category Name';
        });
    }

    /* ── Live Preview: Image ── */
    const fileInput     = document.getElementById('catImageInput');
    const fileDrop      = document.getElementById('catFileDrop');
    const fileNameLabel = document.getElementById('catFileName');
    const fileErrLabel  = document.getElementById('catImageError');
    const previewImg    = document.getElementById('cfpImg');
    const placeholder   = document.getElementById('cfpPlaceholder');

    if (!fileInput) return;

    function handleFile(file) {
        if (!file) return;

        /* Validate type */
        if (!file.type.startsWith('image/')) {
            if (fileErrLabel) { fileErrLabel.textContent = 'Please select an image file (PNG, JPG, WEBP).'; fileErrLabel.style.display = 'block'; }
            return;
        }
        if (fileErrLabel) { fileErrLabel.textContent = ''; fileErrLabel.style.display = 'none'; }
        if (fileNameLabel) fileNameLabel.textContent = file.name;

        /* Read and show preview */
        const reader = new FileReader();
        reader.onload = e => {
            if (previewImg) {
                previewImg.src = e.target.result;
                previewImg.style.display = 'block';
            }
            if (placeholder) placeholder.style.display = 'none';
        };
        reader.readAsDataURL(file);
    }

    fileInput.addEventListener('change', () => handleFile(fileInput.files[0]));

    /* ── Drag & drop on the drop zone ── */
    if (fileDrop) {
        fileDrop.addEventListener('dragover', e => {
            e.preventDefault();
            fileDrop.style.borderColor = 'var(--accent)';
        });
        fileDrop.addEventListener('dragleave', () => {
            fileDrop.style.borderColor = '';
        });
        fileDrop.addEventListener('drop', e => {
            e.preventDefault();
            fileDrop.style.borderColor = '';
            const file = e.dataTransfer.files[0];
            if (file) {
                /* Programmatically set on input so the form picks it up */
                const dt = new DataTransfer();
                dt.items.add(file);
                fileInput.files = dt.files;
                handleFile(file);
            }
        });
    }
})();
