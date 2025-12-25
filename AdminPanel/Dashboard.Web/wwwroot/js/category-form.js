// Kategori Formu - Dinamik Marka, Model ve Özellik Yönetimi

let brandCounter = 0;
let modelCounters = {};
let attributeCounter = 0;
let valueCounters = {};

// Marka Ekle
function addBrand() {
    const container = document.getElementById('brandsContainer');
    
    // İlk marka ekleniyorsa placeholder'ı kaldır
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
                    <label class="form-label fw-bold">Marka Adı <span class="text-danger">*</span></label>
                    <input type="text" class="form-control brand-name" placeholder="Örn: Apple, Samsung" required />
                </div>
                
                <div class="col-md-6">
                    <label class="form-label fw-bold">Resim URL</label>
                    <input type="text" class="form-control brand-image" placeholder="https://..." />
                </div>
                
                <div class="col-md-12">
                    <label class="form-label fw-bold">Açıklama</label>
                    <textarea class="form-control brand-description" rows="2"></textarea>
                </div>
                
                <div class="col-12">
                    <div class="form-check form-switch">
                        <input class="form-check-input brand-active" type="checkbox" checked>
                        <label class="form-check-label">Marka Aktif</label>
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

// Marka Sil
function removeBrand(brandId) {
    const brandElement = document.querySelector(`[data-brand-id="${brandId}"]`);
    if (brandElement) {
        brandElement.remove();
        delete modelCounters[brandId];
        
        // Eğer hiç marka kalmadıysa placeholder göster
        const container = document.getElementById('brandsContainer');
        if (!container.querySelector('.brand-item')) {
            container.innerHTML = '<p class="text-muted text-center py-3">Henüz marka eklenmedi. "Marka Ekle" butonuna tıklayın.</p>';
        }
    }
}

// Model Ekle
function addModel(brandId) {
    const container = document.querySelector(`.models-container-${brandId}`);
    
    // İlk model ekleniyorsa placeholder'ı kaldır
    if (container.querySelector('.text-muted')) {
        container.innerHTML = '';
    }

    const modelId = modelCounters[brandId]++;

    const modelHtml = `
        <div class="model-item border-start border-3 border-success ps-3 mb-2" data-model-id="${brandId}-${modelId}">
            <div class="row g-2">
                <div class="col-md-5">
                    <input type="text" class="form-control form-control-sm model-name" placeholder="Model adı (Örn: iPhone 15 Pro)" required />
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

// Model Sil
function removeModel(brandId, modelId) {
    const modelElement = document.querySelector(`[data-model-id="${brandId}-${modelId}"]`);
    if (modelElement) {
        modelElement.remove();
        
        const container = document.querySelector(`.models-container-${brandId}`);
        if (!container.querySelector('.model-item')) {
            container.innerHTML = '<small class="text-muted">Henüz model eklenmedi</small>';
        }
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
                    <label class="form-label fw-bold">Özellik Adı <span class="text-danger">*</span></label>
                    <input type="text" class="form-control attr-name" placeholder="Örn: Renk, Boyut, RAM" required />
                </div>
                
                <div class="col-md-4">
                    <label class="form-label fw-bold">Görünen Ad</label>
                    <input type="text" class="form-control attr-display-name" placeholder="Görünen ad" />
                </div>
                
                <div class="col-md-2">
                    <label class="form-label fw-bold">Sıra</label>
                    <input type="number" class="form-control attr-order" value="0" min="0" />
                </div>
                
                <div class="col-md-2">
                    <div class="form-check form-switch mt-4">
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

// Özellik Sil
function removeAttribute(attrId) {
    const attrElement = document.querySelector(`[data-attr-id="${attrId}"]`);
    if (attrElement) {
        attrElement.remove();
        delete valueCounters[attrId];
        
        const container = document.getElementById('attributesContainer');
        if (!container.querySelector('.attribute-item')) {
            container.innerHTML = '<p class="text-muted text-center py-3">Henüz özellik eklenmedi. "Özellik Ekle" butonuna tıklayın.</p>';
        }
    }
}

// Özellik Değeri Ekle
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
                    <input type="text" class="form-control form-control-sm value-name" placeholder="Değer (Örn: Kırmızı, XL, 8GB)" required />
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

// Özellik Değeri Sil
function removeAttributeValue(attrId, valueId) {
    const valueElement = document.querySelector(`[data-value-id="${attrId}-${valueId}"]`);
    if (valueElement) {
        valueElement.remove();
        
        const container = document.querySelector(`.values-container-${attrId}`);
        if (!container.querySelector('.value-item')) {
            container.innerHTML = '<small class="text-muted">Henüz değer eklenmedi</small>';
        }
    }
}

// Form Submit
document.getElementById('categoryForm').addEventListener('submit', function(e) {
    e.preventDefault();

    // Kategori verilerini topla
    const categoryData = {
        Name: document.getElementById('categoryName').value,
        Description: document.getElementById('categoryDescription').value || '',
        ImageUrl: document.getElementById('categoryImageUrl').value || null,
        ParentCategoryId: document.getElementById('parentCategoryId').value || null,
        DisplayOrder: parseInt(document.getElementById('categoryDisplayOrder').value) || 0,
        IsActive: document.getElementById('categoryIsActive').checked,
        Brands: [],
        Attributes: []
    };

    // Markaları topla
    document.querySelectorAll('.brand-item').forEach(brandEl => {
        const brandData = {
            Name: brandEl.querySelector('.brand-name').value,
            Description: brandEl.querySelector('.brand-description').value || '',
            ImageUrl: brandEl.querySelector('.brand-image').value || null,
            IsActive: brandEl.querySelector('.brand-active').checked,
            Models: []
        };

        // Marka modellerini topla
        const brandId = brandEl.dataset.brandId;
        document.querySelectorAll(`.models-container-${brandId} .model-item`).forEach(modelEl => {
            brandData.Models.push({
                Name: modelEl.querySelector('.model-name').value,
                Description: modelEl.querySelector('.model-description').value || '',
                IsActive: true
            });
        });

        categoryData.Brands.push(brandData);
    });

    // Özellikleri topla
    document.querySelectorAll('.attribute-item').forEach(attrEl => {
        const attrData = {
            Name: attrEl.querySelector('.attr-name').value,
            DisplayName: attrEl.querySelector('.attr-display-name').value || attrEl.querySelector('.attr-name').value,
            DisplayOrder: parseInt(attrEl.querySelector('.attr-order').value) || 0,
            IsRequired: attrEl.querySelector('.attr-required').checked,
            IsActive: true,
            Values: []
        };

        // Özellik değerlerini topla
        const attrId = attrEl.dataset.attrId;
        document.querySelectorAll(`.values-container-${attrId} .value-item`).forEach(valueEl => {
            attrData.Values.push({
                Value: valueEl.querySelector('.value-name').value,
                ColorCode: valueEl.querySelector('.value-color').value || null,
                IsActive: true
            });
        });

        categoryData.Attributes.push(attrData);
    });

    // JSON olarak gönder
    document.getElementById('categoryData').value = JSON.stringify(categoryData);
    this.submit();
});
