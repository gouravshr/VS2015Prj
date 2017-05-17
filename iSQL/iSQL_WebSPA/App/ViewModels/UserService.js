/*
userModule.service("userWpiService", function ($http) {

    //get All Eployee
    this.getUsers = function () {
        return $http.get("http://localhost:56913/api/Users");
    };

    // get Employee By Id
    this.getUser = function (userId) {
        var response = $http({
            method: "post",
            url: "http://localhost:56913/api/Users",
            params: {
                id: JSON.stringify(userId)
            }
        });
        return response;
    }

    // Update Employee
    this.updateUser = function (user) {
        var response = $http({
            method: "put",
            url: "http://localhost:56913/api/Users",
            data: JSON.stringify(user),
            dataType: "json"
        });
        return response;
    }

    // Add Employee
    this.AddEmp = function (user) {
        var response = $http({
            method: "post",
            url: "http://localhost:56913/api/Users",
            data: JSON.stringify(user),
            dataType: "json"
        });
        return response;
    }

    //Delete Employee
    this.DeleteUser = function (userId) {
        var response = $http({
            method: "delete",
            url: "http://localhost:56913/api/Users",
            params: {
                employeeId: JSON.stringify(userId)
            }
        });
        return response;
    }
});
*/