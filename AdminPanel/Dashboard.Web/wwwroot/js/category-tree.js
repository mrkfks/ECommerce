// Kategori Ağacı Yönetimi - Dinamik Hiyerarşik Yapı

let categoryCounter = 0;
let brandCounter = 0;
let modelCounters = {};
let attributeCounter = 0;
let valueCounters = {};

// Ana Kategori Ekle
function addRootCategory() {
    addCategory(null, 0);
}

// Kategori Ekle (parent varsa alt kategori, yoksa ana kategori)
function addCategory(parentPath, level) {
    const container = parentPath 
        ? document.querySelector(`[data-category-path="${parentPath}"] .subcategories-container`)
        : document.getElementById('categoryTree');
    
    // İlk kategori ekleniyorsa placeholder'ı kaldır
    const placeholder = container.querySelector('.text-muted');
    if (placeholder) {
        placeholder.remove();
    }

    const categoryId = categoryCounter++;
    const categoryPath = parentPath ? `${parentPath}.${categoryId}` : `${categoryId}`;
    const indent = level * 20;

    const categoryHtml = `
        <div class="category-item mb-3" data-category-path="${categoryPath}" style="margin-left: ${indent}px;">
            <div class="card border-start border-4 border-primary">
                <div class="card-body p-3">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <h6 class="mb-0 text-primary">
                            <i class="fa-solid fa-folder me-2"></i>
                            ${level === 0 ? 'Ana Kategori' : `Alt Kategori (Seviye ${level})`}
                        </h6>
                        <div class="btn-group btn-group-sm">
                            <button type="button" class="btn btn-outline-success" onclick="addCategory('${categoryPath}', ${level + 1})">
                                <i class="fa-solid fa-plus me-1"></i> Alt Kategori
                            </button>
                            <button type="button" class="btn btn-outline-danger" onclick="removeCategory('${categoryPath}')">
                                <i class="fa-solid fa-trash"></i>
                            </button>
                        </div>
                    </div>
                    
                    <div class="row g-2">
                        <div class="col-md-4">
                            <input type="text" class="form-control form-control-sm cat-name" placeholder="Kategori Adı *" required />
                        </div>
                        <div class="col-md-4">
                            <input type="text" class="form-control form-control-sm cat-description" placeholder="Açıklama" />
                        </div>
                        <div class="col-md-2">
                            <input type="text" class="form-control form-control-sm cat-image" placeholder="Resim URL" />
                        </div>
                        <div class="col-md-1">
                            <input type="number" class="form-control form-control-sm cat-order" value="${level}" min="0" placeholder="Sıra" />
                        </div>
                        <div class="col-md-1">
                            <div class="form-check form-switch">
                                <input class="form-check-input cat-active" type="checkbox" checked>
                            </div>
                        </div>
                    </div>
                    
                    <div class="subcategories-container mt-2"></div>
                </div>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', categoryHtml);
}

// Kategori Sil
function removeCategory(categoryPath) {
    if (confirm('Bu kategoriyi ve tüm alt kategorilerini silmek istediğinizden emin misiniz?')) {
        const categoryElement = document.querySelector(`[data-category-path="${categoryPath}"]`);
        if (categoryElement) {
            const parent = categoryElement.parentElement;
            categoryElement.remove();
            
            // Eğer container boşsa placeholder ekle
            if (parent.id === 'categoryTree' && !parent.querySelector('.category-item')) {
                parent.innerHTML = '<p class="text-muted text-center py-4">Henüz kategori eklenmedi. "Ana Kategori Ekle" butonuna tıklayarak başlayın.</p>';
            }
        }
    }
}

// Kategori Verisini Topla (Recursive)
function collectCategoryData(categoryElement) {
    if (!categoryElement) return null;

    const data = {
        Name: categoryElement.querySelector('.cat-name').value,
        Description: categoryElement.querySelector('.cat-description').value || '',
        ImageUrl: categoryElement.querySelector('.cat-image').value || null,
        DisplayOrder: parseInt(categoryElement.querySelector('.cat-order').value) || 0,
        IsActive: categoryElement.querySelector('.cat-active').checked,
        SubCategories: []
    };

    // Alt kategorileri topla
    const subContainer = categoryElement.querySelector('.subcategories-container');
    if (subContainer) {
        const subCategories = subContainer.querySelectorAll(':scope > .category-item');
        subCategories.forEach(subCat => {
            const subData = collectCategoryData(subCat);
            if (subData) {
                data.SubCategories.push(subData);
            }
        });
    }

    return data;
}

// Marka Ekle
function addBrand() {
    const container = document.getElementById('brandsContainer');
    
    if (container.querySelector('.text-muted')) {
        container.innerHTML = '';
    }

    const brandId = brandCounter++;
    modelCounters[brandId] = 0;

    const brandHtml = `
        <div class="brand-item border rounded p-3 mb-3" data-brand-id="${brandId}">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h6 class="mb-0"><i class="fa-solid fa-tag me-2"></i> Marka #${brandId + 1}</h6>
                <button type="button" class="btn btn-danger btn-sm" onclick="removeBrand(${brandId})">
                    <i class="fa-solid fa-trash"></i> Sil
                </button>
            </div>
            
            <div class="row g-3">
                <div class="col-md-6">
                    <input type="text" class="form-control brand-name" placeholder="Marka Adı *" required />
                </div>
                <div class="col-md-6">
                    <input type="text" class="form-control brand-image" placeholder="Resim URL" />
                </div>
                <div class="col-md-12">
                    <textarea class="form-control brand-description" rows="2" placeholder="Açıklama"></textarea>
                </div>
                <div class="col-12">
                    <div class="form-check form-switch">
                        <input class="form-check-input brand-active" type="checkbox" checked>
                        <label class="form-check-label">Aktif</label>
                    </div>
                </div>
                <div class="col-12">
                    <hr class="my-2" />
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <label class="fw-bold mb-0"><i class="fa-solid fa-cube me-2"></i> Modeller</label>
                        <button type="button" class="btn btn-sm btn-outline-success" onclick="addModel(${brandId})">
                            <i class="fa-solid fa-plus me-1"></i> Model Ekle
                        </button>
                    </div>
                    <div class="models-container-${brandId}">
                        <small class="text-muted">Henüz model eklenmedi</small>
                    </div>
                </div>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', brandHtml);
}

function removeBrand(brandId) {
    document.querySelector(`[data-brand-id="${brandId}"]`).remove();
    delete modelCounters[brandId];
    
    const container = document.getElementById('brandsContainer');
    if (!container.querySelector('.brand-item')) {
        container.innerHTML = '<p class="text-muted text-center py-3">Henüz marka eklenmedi.</p>';
    }
}

function addModel(brandId) {
    const container = document.querySelector(`.models-container-${brandId}`);
    
    if (container.querySelector('.text-muted')) {
        container.innerHTML = '';
    }

    const modelId = modelCounters[brandId]++;

    const modelHtml = `
        <div class="model-item border-start border-3 border-success ps-3 mb-2" data-model-id="${brandId}-${modelId}">
            <div class="row g-2">
                <div class="col-md-5">
                    <input type="text" class="form-control form-control-sm model-name" placeholder="Model adı" required />
                </div>
                <div class="col-md-5">
                    <input type="text" class="form-control form-control-sm model-description" placeholder="Açıklama" />
                </div>
                <div class="col-md-2 text-end">
                    <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeModel(${brandId}, ${modelId})">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </div>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', modelHtml);
}

function removeModel(brandId, modelId) {
    document.querySelector(`[data-model-id="${brandId}-${modelId}"]`).remove();
    
    const container = document.querySelector(`.models-container-${brandId}`);
    if (!container.querySelector('.model-item')) {
        container.innerHTML = '<small class="text-muted">Henüz model eklenmedi</small>';
    }
}

// Özellik Ekle
function addAttribute() {
    const container = document.getElementById('attributesContainer');
    
    if (container.querySelector('.text-muted')) {
        container.innerHTML = '';
    }

    const attrId = attributeCounter++;
    valueCounters[attrId] = 0;

    const attrHtml = `
        <div class="attribute-item border rounded p-3 mb-3" data-attr-id="${attrId}">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h6 class="mb-0"><i class="fa-solid fa-sliders me-2"></i> Özellik #${attrId + 1}</h6>
                <button type="button" class="btn btn-danger btn-sm" onclick="removeAttribute(${attrId})">
                    <i class="fa-solid fa-trash"></i> Sil
                </button>
            </div>
            
            <div class="row g-3">
                <div class="col-md-4">
                    <input type="text" class="form-control attr-name" placeholder="Özellik Adı *" required />
                </div>
                <div class="col-md-4">
                    <input type="text" class="form-control attr-display-name" placeholder="Görünen Ad" />
                </div>
                <div class="col-md-2">
                    <input type="number" class="form-control attr-order" value="0" min="0" placeholder="Sıra" />
                </div>
                <div class="col-md-2">
                    <div class="form-check form-switch mt-2">
                        <input class="form-check-input attr-required" type="checkbox">
                        <label class="form-check-label">Zorunlu</label>
                    </div>
                </div>
                <div class="col-12">
                    <hr class="my-2" />
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <label class="fw-bold mb-0"><i class="fa-solid fa-list me-2"></i> Değerler</label>
                        <button type="button" class="btn btn-sm btn-outline-info" onclick="addAttributeValue(${attrId})">
                            <i class="fa-solid fa-plus me-1"></i> Değer Ekle
                        </button>
                    </div>
                    <div class="values-container-${attrId}">
                        <small class="text-muted">Henüz değer eklenmedi</small>
                    </div>
                </div>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', attrHtml);
}

function removeAttribute(attrId) {
    document.querySelector(`[data-attr-id="${attrId}"]`).remove();
    delete valueCounters[attrId];
    
    const container = document.getElementById('attributesContainer');
    if (!container.querySelector('.attribute-item')) {
        container.innerHTML = '<p class="text-muted text-center py-3">Henüz özellik eklenmedi.</p>';
    }
}

function addAttributeValue(attrId) {
    const container = document.querySelector(`.values-container-${attrId}`);
    
    if (container.querySelector('.text-muted')) {
        container.innerHTML = '';
    }

    const valueId = valueCounters[attrId]++;

    const valueHtml = `
        <div class="value-item border-start border-3 border-info ps-3 mb-2" data-value-id="${attrId}-${valueId}">
            <div class="row g-2">
                <div class="col-md-6">
                    <input type="text" class="form-control form-control-sm value-name" placeholder="Değer" required />
                </div>
                <div class="col-md-4">
                    <input type="text" class="form-control form-control-sm value-color" placeholder="Renk kodu (#FF0000)" />
                </div>
                <div class="col-md-2 text-end">
                    <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeAttributeValue(${attrId}, ${valueId})">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </div>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', valueHtml);
}

function removeAttributeValue(attrId, valueId) {
    document.querySelector(`[data-value-id="${attrId}-${valueId}"]`).remove();
    
    const container = document.querySelector(`.values-container-${attrId}`);
    if (!container.querySelector('.value-item')) {
        container.innerHTML = '<small class="text-muted">Henüz değer eklenmedi</small>';
    }
}

// Form Submit
document.getElementById('categoryForm').addEventListener('submit', function(e) {
    e.preventDefault();

    const categories = [];
    
    // Ana kategorileri topla
    const rootCategories = document.querySelectorAll('#categoryTree > .category-item');
    rootCategories.forEach(catEl => {
        const catData = collectCategoryData(catEl);
        if (catData && catData.Name) {
            categories.push(catData);
        }
    });

    if (categories.length === 0) {
        alert('En az bir kategori eklemelisiniz!');
        return;
    }

    // Markaları topla
    const brands = [];
    document.querySelectorAll('.brand-item').forEach(brandEl => {
        const brandData = {
            Name: brandEl.querySelector('.brand-name').value,
            Description: brandEl.querySelector('.brand-description').value || '',
            ImageUrl: brandEl.querySelector('.brand-image').value || null,
            IsActive: brandEl.querySelector('.brand-active').checked,
            Models: []
        };

        const brandId = brandEl.dataset.brandId;
        document.querySelectorAll(`.models-container-${brandId} .model-item`).forEach(modelEl => {
            brandData.Models.push({
                Name: modelEl.querySelector('.model-name').value,
                Description: modelEl.querySelector('.model-description').value || '',
                IsActive: true
            });
        });

        brands.push(brandData);
    });

    // Özellikleri topla
    const attributes = [];
    document.querySelectorAll('.attribute-item').forEach(attrEl => {
        const attrData = {
            Name: attrEl.querySelector('.attr-name').value,
            DisplayName: attrEl.querySelector('.attr-display-name').value || attrEl.querySelector('.attr-name').value,
            DisplayOrder: parseInt(attrEl.querySelector('.attr-order').value) || 0,
            IsRequired: attrEl.querySelector('.attr-required').checked,
            IsActive: true,
            Values: []
        };

        const attrId = attrEl.dataset.attrId;
        document.querySelectorAll(`.values-container-${attrId} .value-item`).forEach(valueEl => {
            attrData.Values.push({
                Value: valueEl.querySelector('.value-name').value,
                ColorCode: valueEl.querySelector('.value-color').value || null,
                IsActive: true
            });
        });

        attributes.push(attrData);
    });

    // Tüm veriyi birleştir
    const formData = {
        Categories: categories,
        Brands: brands,
        Attributes: attributes
    };

    document.getElementById('categoryData').value = JSON.stringify(formData);
    this.submit();
});
