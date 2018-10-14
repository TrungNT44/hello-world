app.instance.controller("roleActionController", roleActionController);
roleActionController.$inject = ['$scope', '$rootScope', '$injector'];
function roleActionController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.selectedAction = {};
        $scope.role = {};
        $scope.role.RoleName = '';
        $scope.roleActions = [];
        $scope.actions = [];
        $scope.permissions = permissions;
        self.resetPermission();        
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
       
    }

    this.resetPermission = function () {
        $scope.selectedPermissionId = $scope.permissions[0].PermissionId;
    }

    $scope.onSelectAction = function (action) {
        // Reset the permission
        $scope.selectedPermissionId = $scope.permissions[0].PermissionId;
    }    

    this.onLoadRoleActionsSuccess = function (response) {
        if (response && response.Data) {
            var data = response.Data;
            $scope.role.RoleId = data.RoleId;
            $scope.role.RoleName = data.RoleName;
            $scope.roleActions = data.PermittedResources;
            $scope.actions = data.NonePermittedResources;
        } 
    }

    $scope.show = function(roleId) {
        $scope.role.RoleId = roleId;
        self.requestRemoteUrl('/Admin/LoadRoleActions',
            { roleId: roleId }, self.onLoadRoleActionsSuccess, null);       
    }

    this.onAddRoleActionSuccess = function (response) {
        if (response && response.Data) {
            self.moveAction($scope.selectedAction.selected, $scope.actions, $scope.roleActions);
            $scope.selectedAction = {}; 
            self.resetPermission(); 
            app.notice.message('Thêm quyền truy cập tài nguyên thành công!');
        }
    }

    $scope.addAction = function(action) {
        self.requestRemoteUrl('/Admin/AddRoleAction',
            { roleId: $scope.role.RoleId, resourceId: action.ResourceId, permissionId: $scope.selectedPermissionId },
                function (response) {
                    if (response && response.Data) {
                        action.PermissionId = $scope.selectedPermissionId;
                        self.moveAction(action, $scope.actions, $scope.roleActions);                        
                        $scope.selectedAction = {};
                        self.resetPermission();
                        app.notice.message('Thêm quyền truy cập tài nguyên thành công!');
                    }
                }, null);
    }   

    $scope.deleteAction = function (action) {
        self.requestRemoteUrl('/Admin/RemoveRoleAction',
            { roleId: $scope.role.RoleId, resourceId: action.ResourceId },
            function (response) {
                if (response && response.Data) {
                    self.moveAction(action, $scope.roleActions, $scope.actions);
                    app.notice.message('Xóa quyền truy cập tài nguyên thành công!');
                }
            }, null);      
    }

    this.onUpdatePermissionSuccess = function (response) {
        if (response && response.Data) {
            $scope.role.RoleId = data.roleActionData.roleId;
            $scope.role.RoleName = data.roleActionData.roleName;
            $scope.roleActions = data.roleActionData.roleActions;
            $scope.actions = data.roleActionData.actions;
        }
    }
    $scope.updatePermisson = function (action) {
        self.requestRemoteUrl('/Admin/UpdatePermission',
            { roleId: $scope.role.RoleId, resourceId: action.ResourceId, permissionId: action.PermissionId }, function (response) {
                if (response && response.Data) {
                    app.notice.message('Thay đổi quyền truy cập tài nguyên thành công!');
                }
            }, null);      
    }    

    this.moveAction = function (action, source, destination) {
        var index = -1;
        if (action != undefined) {
            index = source.indexOf(action);
        }
        if (index == -1) {
            return false;
        }
        destination.push(action);
        source.splice(index, 1);

        return true;
    }

    $scope.getDeleteIconSrc =function(){
        return "/Images/bin.png";
    }

    function isAddActionDisabled() {
        return $scope.selectedAction.selected == undefined;
    }


    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};

app.instance.directive("roleActionDirective", roleActionDirective);
roleActionDirective.$inject = ['$timeout'];
function roleActionDirective($timeout) {
    return {
        restrict: 'A',
        scope: {
            roleId: "=",
            editAble: "="
        },
        link: function (scope, element, attrs) {
            scope.$watch('roleId', function (value) {
                if (value != undefined) {
                    scope.show(scope.roleId);
                }
            });
        },
        replace: false,
        templateUrl: "/tplRoleAction.html",
        controller: 'roleActionController'
    }
};

