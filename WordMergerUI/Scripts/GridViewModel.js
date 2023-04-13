var self;
var DocumentsGridModel = function (documents) {

    self = this;
    self.documents = ko.observableArray(documents);
    self.selectedDocument = ko.observable(null);
    self.IsDesc = false;

    self.sortByName = function () {
        self.documents.sort(function (a, b) {
            if (self.IsDesc) {
                return a.name < b.name ? -1 : 1;
            }
            else {
                return a.name > b.name ? -1 : 1;
            }
        });

        self.IsDesc = !self.IsDesc;
    };

    self.isselected = function isselected(id) {
        if (!self.selectedDocument())
            return false;

        return id == self.selectedDocument().id;
    }

    self.clicks = 0;

    self.selectDocument = function () {
        $("#ID").val(this.id);
        self.selectedDocument(this);

        self.clicks++;

        if (self.clicks === 1) {
            setTimeout(function () {
                if (self.clicks === 1) {
                } else {
                    IsParameterItem(self.selectedDocument().id, self.selectedDocument().isChooseFormat);
                }
                self.clicks = 0;
            }, 400); 
        }
    };

    self.condition = ko.observable();

    self.mergeDocument = function () {
        if (!self.selectedDocument()) {
            ShowErrorMessage("Необходимо выбрать документ для формирования.");
            return;
        }

        IsParameterItem(self.selectedDocument().id, self.selectedDocument().isChooseFormat);
    };

    self.previewParamsDocument = function () {
        if (!self.selectedDocument()) {
            ShowErrorMessage("Необходимо выбрать документ для формирования.");
            return;
        }

        IsPreviewParameterItem(self.selectedDocument().id);
    };

    self.filterDocuments = ko.computed(function () {
        if (!self.condition()) {
            return self.documents();
        } else {
            return ko.utils.arrayFilter(self.documents(), function (WMDocument) {
                var documentName = WMDocument.name.toLowerCase();
                return documentName.indexOf(self.condition().toLowerCase()) > -1;
            });
        }
    });
};

function MergeDocument() {
    IsParameterItem(self.selectedDocument().id, self.selectDocument().isChooseFormat);
}