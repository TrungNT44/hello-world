app.instance.controller("RecruitController", RecruitController);
RecruitController.$inject = ['$scope', '$rootScope', '$injector'];
function RecruitController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        self.readCachedValues();
        $scope.InputData = {};
        $scope.IsAdmin = permission == true?false:true;
        if (typeof (idTuyenDung) != "undefined" && idTuyenDung != null && idTuyenDung != "") {
            self.GetRecruitInfo(idTuyenDung);
        }
    }
    this.onInitializeFinished = function () {
        if (typeof (isView) != "undefined") {
            $scope.checked = true;
        }
        if (idTuyenDung == null) {
            self.LoadDataForDDL_Province();
            self.LoadDataForDDL_DrugStores(idNhaThuoc);
            self.LoadDataDefault();
        }
    }
    this.GetRecruitInfo = function (sid) {
        self.loadData("/Recruitment/GetRecruitInfo", { id: sid }, function (response) {
            if (response) {
                response.IdTinhThanh = response.IdTinhThanh.toString();
                $scope.InputData = response;
                self.LoadDataForDDL_Province(response.IdTinhThanh);
                self.LoadDataForDDL_DrugStores(response.MaNhaThuoc);
            }
        });
    }
    this.LoadDataForDDL_Province = function (defautValue) {
        self.loadData("/Recruitment/GetListProvinces", null, function (response) {
            $scope.ArrTinhThanhs = response;
        });
    }
    this.LoadDataForDDL_DrugStores = function (defautValue) {
        self.loadData("/Recruitment/GetListDrugStores", null, function (response) {
            $scope.ArrNhaThuocs = response;
            if (defautValue) {
                $scope.InputData.NhaThuoc = response.find(function (item) {
                    if (item.MaNhaThuoc === defautValue)
                        return item;
                });
                if (idTuyenDung == null) {
                    $scope.DDL_DrugStoresChange();
                }
            }
        });
    }
    this.LoadDataDefault = function () {
        if (idTuyenDung == null) {
            var NgayDang = new Date();
            var NgayHetHan = new Date();
            var dd = NgayDang.getDate();
            var mm = NgayDang.getMonth() + 1; //January is 0!
            var mm_het = mm + 1;
            var yyyy = NgayDang.getFullYear();
            if (dd < 10) {
                dd = '0' + dd
            }
            if (mm < 10) {
                mm = '0' + mm
            }
            NgayDang = dd + '/' + mm + '/' + yyyy;
            
            if (mm_het < 10) {
                mm_het = '0' + mm_het
            }
            NgayHetHan = dd + '/' + mm_het + '/' + yyyy;
            $scope.InputData.NgayDang_View = NgayDang;
            $scope.InputData.NgayHetHan_View = NgayHetHan;
        }
    }
    $scope.DDL_DrugStoresChange = function () {
        if ($scope.InputData && $scope.InputData.NhaThuoc) {
            $scope.InputData.TenNhaThuoc = $scope.InputData.NhaThuoc.TenNhaThuoc;
            $scope.InputData.DiaChiNhaTuyenDung = $scope.InputData.NhaThuoc.DiaChi;
            $scope.InputData.LienHe = "Người liên hệ: " +  ($scope.InputData.NhaThuoc.NguoiDaiDien || "") + " - Email: " + ($scope.InputData.NhaThuoc.Email || "") + " - SĐT: " + ($scope.InputData.NhaThuoc.DienThoai || "");
            $scope.InputData.IdTinhThanh = ($scope.InputData.NhaThuoc.TinhThanhId ? $scope.InputData.NhaThuoc.TinhThanhId.toString() : "");
            $scope.InputData.TieuDe = "Nhà thuốc " + $scope.InputData.NhaThuoc.TenNhaThuoc + " tuyển dụng nhân viên bán thuốc";
        }
        else {
            $scope.InputData.LienHe = "";
            $scope.InputData.DiaChiNhaTuyenDung = "";
            $scope.InputData.TenNhaThuoc = "";
            $scope.InputData.IdTinhThanh = "";
        }
    }
    $scope.Create = function (FormVali) {
        $scope.submited = true;
        if (FormVali) {
            var oData = {
                TieuDe: $scope.InputData.TieuDe,
                NoiDung: $scope.InputData.NoiDung,
                MaNhaThuoc: $scope.InputData.NhaThuoc.MaNhaThuoc,
                TenNhaThuoc: $scope.InputData.TenNhaThuoc,
                LienHe: $scope.InputData.LienHe,
                DiaChiNhaTuyenDung: $scope.InputData.DiaChiNhaTuyenDung,
                IdTinhThanh: $scope.InputData.IdTinhThanh,
                LienKet: $scope.InputData.LienKet,
                NgayHetHan_View: $scope.InputData.NgayHetHan_View,
                NgayDang_View: $scope.InputData.NgayDang_View
            };
            self.loadData("/Recruitment/CreateRecruit", oData, function (response) {
                if (response === "OK")
                    window.location.href = "/Recruitment/ListRecruits";
                else
                    app.notice.error('Không tạo được tin');
            });
        }
    }
    $scope.Save = function (FormVali) {
        $scope.submited = true;
        if (FormVali) {
            var oData = {
                IdTinTuyenDung: $scope.InputData.IdTinTuyenDung,
                TieuDe: $scope.InputData.TieuDe,
                NoiDung: $scope.InputData.NoiDung,
                MaNhaThuoc: $scope.InputData.NhaThuoc.MaNhaThuoc,
                TenNhaThuoc: $scope.InputData.TenNhaThuoc,
                LienHe: $scope.InputData.LienHe,
                DiaChiNhaTuyenDung: $scope.InputData.DiaChiNhaTuyenDung,
                IdTinhThanh: $scope.InputData.IdTinhThanh,
                LienKet: $scope.InputData.LienKet,
                NgayHetHan_View: $scope.InputData.NgayHetHan_View,
                NgayDang_View: $scope.InputData.NgayDang_View,
                HoatDong: $scope.InputData.HoatDong
            };
            self.loadData("/Recruitment/UpdateRecruit", oData, function (response) {
                if (response === "OK")
                    app.notice.message('Cập nhật thành công');
                else
                    app.notice.error('Không cập nhật được tin');
            });
        }
    };
    $scope.Delete = function () {
        self.loadData("/Recruitment/RemoveRecruit", {
            idRecruit: idTuyenDung
        }, function (response) {
            if (response === "OK")
                window.location.href = "/Recruitment/ListRecruits";
            else
                app.notice.error('Không tồn tại tin');
        });
    }
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
}