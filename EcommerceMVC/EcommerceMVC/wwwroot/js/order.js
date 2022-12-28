﻿//var dataTable

//$(document).ready(function () {
//    loadDataTable()
//});

//function loadDataTable() {
//    dataTable = $('#tblData').DataTable({
//            'ajax': {
//                'url': 'Order/GetAll'
//            },
//            'columns': [
//                { "data": "id", "width": "15%" },
//                { "data": "name", "width": "15%" },
//                { "data": "phoneNumber", "width": "15%" },
//                { "data": "ecommerceUser.email", "width": "15%" },
//                { "data": "orderStatus", "width": "15%" },
//                { "data": "orderTotal", "width": "15%" },
//                {
//                    "data": "id",
//                    "render": function (data) {
//                        return `
//                                <div class="w-75 btn-group" role="group">
//                                    <a href="/Admin/Order/Details?orderid=${data}"
//                                    class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Details</a>
//                                </div>
//                                `
//                    },
//                    "width": "15%"
//                },
//            ]
//        });
//}


//function getData() {

//}

$(document).ready(function () {
    fetch('Order/GetAll')
        .then(resp => resp.json())
        .then(data => {
            $('#tblData').DataTable({
                data: data,
                column: [
                    { "data": "id", "width": "15%" },
                    { "data": "name", "width": "15%" },
                    { "data": "phoneNumber", "width": "15%" },
                    { "data": "ecommerceUser.email", "width": "15%" },
                    { "data": "orderStatus", "width": "15%" },
                    { "data": "orderTotal", "width": "15%" },
                    {
                        "data": "id",
                        "render": function (data) {
                            return `
                                    <div class="w-75 btn-group" role="group">
                                        <a href="/Admin/Order/Details?orderid=${data}"
                                        class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Details</a>
                                    </div>
                                    `
                        },
                        "width": "15%"
                    },
                    ],
                })
        })
});