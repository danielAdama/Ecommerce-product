
$(document).ready(function () {
    fetch("https://localhost:44392/Admin/Order/GetAll")
        .then(resp => resp.json())
        .then(data => {
            console.log(data.data)
            $('#tblData').DataTable({
                data: data.data,
                columns: [
                    { data: "id", "width": "5%" },
                    { data: "name", "width": "15%" },
                    { data: "phoneNumber", "width": "15%" },
                    { data: "ecommerceUser.email", "width": "15%" },
                    { data: "orderStatus", "width": "15%" },
                    { data: "orderTotal", "width": "15%" },
                    {
                        data: "id",
                        "render": function (data) {
                            return `
                                    <div class="w-75 btn-group" role="group">
                                        <a href="/Admin/Order/Details?orderid=${data}"
                                        class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                                    </div>
                                    `
                        },
                        "width": "15%"
                    },
                    ],
                })
        })
});