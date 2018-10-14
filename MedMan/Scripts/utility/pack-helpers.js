app.packHelpers = app.packHelpers || {};

app.packHelpers = function () {
    return {
        getBatchesInPack: getBatchesInPack,
        getBatchIdsInPack: getBatchIdsInPack,
        getTemplateIdsInPack: getTemplateIdsInPack,
        getImageIdsInPack: getImageIdsInPack,

        getSelectedObjectsInList: getSelectedObjectsInList,
        getSelectedImageIdsInPack: getSelectedImageIdsInPack,
    }
    function getBatchIdsInPack(pack) {
        var batchIds = [];
        var batches = getBatchesInPack(pack);
        for (var i = 0; i < batches.length; i++) {
            var batch = batches[i];
            batchIds.push(batch.BatchId);
        }
        return batchIds;
    }

    function getTemplateIdsInPack(pack) {
        var templateIds = [];
        var batches = getBatchesInPack(pack);
        for (var i = 0; i < batches.length; i++) {
            var batch = batches[i];
            for (var j = 0; j < batch.Templates.length; j++) {
                var templateId = batch.Templates[j].TemplateId;
                if (templateIds.indexOf(templateId) < 0) {
                    templateIds.push(templateId);
                }
            }
        }
        return templateIds;
    }

    function getBatchesInPack(pack) {
        var batches = [];
        if (pack.Batches) {
            batches = pack.Batches.slice();
        } else if (pack.Batch) {
            batches.push(pack.Batch);
        }
        return batches;
    }

    function getImageIdsInPack(pack) {
        var imageIds = [];
        for (var i = 0; i < pack.Images.length; i++) {
            imageIds.push(pack.Images[i].ImageId);
        }
        return imageIds;
    }

    function getSelectedImageIdsInPack(pack) {
        var selectedImages = getSelectedObjectsInList(pack.Images);
        var imageIds = [];
        for (var i = 0; i < selectedImages.length; i++) {
            imageIds.push(selectedImages[i].ImageId);
        }
        return imageIds;
    }

    function getSelectedObjectsInList(arrayObjects) {
        var selectedObjects = [];
        if (arrayObjects)
            for (var i = 0; i < arrayObjects.length; i++) {
                if (arrayObjects[i].isSelected) {
                    selectedObjects.push(arrayObjects[i]);
                }
            }
        return selectedObjects;
    }


}();