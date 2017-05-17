/// <reference path="../App.js" />

var userModule = angular.module('userModule', ['common']);

userModule.config(function ($routeProvider,
                                $locationProvider) {
    $routeProvider.caseInsensitiveMatch = true;
    $routeProvider.when('/user', {
        templateUrl: '/App/Views/UserListView.html',
        controller: 'userViewModel'
    });
    $routeProvider.when('/user/list', {
        templateUrl: '/App/Views/UserListView.html',
        controller: 'userViewModel'
    });
    $routeProvider.when('/user/show/:userId', {
        templateUrl: '/App/Views/UserView.html',
        controller: 'userViewModel'
    });
    $routeProvider.otherwise({
        redirectTo: '/User'
    });
    $locationProvider.html5Mode({
        enabled: true,
        requireBase: false
    });
});

userModule.factory('userService',
    function ($http, $location, viewModelHelper) {
        return MyApp.userService($http,
            $location, viewModelHelper);
    });

(function (myApp) {
    var userService = function ($http, $location,
        viewModelHelper) {

        var self = this;
        self.userId = "";
        return this;
    };
    myApp.userService = userService;
}(window.MyApp));

userModule.controller("userViewModel",
    function ($scope, userService, $http, viewModelHelper, $log) {

        // This is the parent controller/viewmodel for 'userModule' and its $scope is accesible
        // down controllers set by the routing engine. This controller is bound to the user.cshtml in the
        // Home view-folder.

        $scope.viewModelHelper = viewModelHelper;
        $scope.userService = userService;
        $scope.flags = { shownFromList: false };
        $scope.pageHeading = "User Section";
        
        var initialize = function () {
            $scope.editIndex = -1;
            $scope.addUpdateUser = false;
            $scope.refreshUsers();
        }

        $scope.deleteUser = function (userId) {
            if (confirm("Are you sure to delete the record? " + userId)) {
                viewModelHelper.apiDelete('http://localhost:56913/api/Users/DeleteUser?id=' + userId,null, function (msg) {
                    initialize();
                    alert('User Deleted');
                }, function () {
                    alert('Error in Deleting Record');
                });
            }
        }

        /*
        //To Get All Records 
        $scope.refreshUsers = function () {
            debugger;
            var getData = userWpiService.getUsers();
            debugger;
            getData.then(function (obj) {
                $scope.Users = obj.data;
            }, function () {
                alert('Error in getting records');
            });
        }
        */

        $scope.refreshUsers = function () {
            viewModelHelper.apiGet('http://localhost:56913/api/Users', null,
                function (result) {
                    $scope.Users = result.data;
                });
        }
        
        $scope.updateUsers = function (user) {
            alert(user.UserId + '  ' + user.PreferredEmailId);
            var User = {
                UserId: user.UserId,
                SystemRole: user.SystemRole,
                PreferredEmailId: user.PreferredEmailId,
                IsActive: user.IsActive
            };
        /*
            $http({
                method: "put",
                url: "http://localhost:56913/api/Users/",
                data: JSON.stringify(User),
                dataType: "json"
            }).success(function (data) {
                $location.path('/list');
            }).error(function (data) {
                console.log(data);
                alert("An Error has occured while Saving User! " + data.ExceptionMessage);
            })
        });
            */
            viewModelHelper.apiPut('http://localhost:56913/api/Users/', User,
                function (result) {
                    $scope.Users = result.data;
                    initialize();
                }, function (msg) {
                    alert('Error' + msg);
                });
               
            alert("Click Modify");
        }

        $scope.cancelUpdate = function () {
            if (confirm("Are you sure to cancel the update?")) {
                $scope.editIndex = -1;
                $scope.addUpdateUser = false;
            }
        };

        $scope.modify = function (user, index) {
            $scope.editIndex = index;
        };

        $scope.addNew = function (userId) {
            $scope.addUpdateUser = true;
            //viewModelHelper.apiGet('http://localhost:56913/api/Users', userId, function (result) {
            //    $scope.user = result.data;
            //    $scope.addUpdateUser = true;
            //});
        };

        $scope.addUser = function () {
            var User = {
                UserId: $scope.UserId,
                PreferredEmailId: $scope.PreferredEmailId,
                SystemRole: $scope.SystemRole,
                IsActive: false
            };
            viewModelHelper.apiPost('http://localhost:56913/api/Users', User, function (result) {
                $scope.user = result.data;
                initialize();
            });
        };

        //initialize();
    });
