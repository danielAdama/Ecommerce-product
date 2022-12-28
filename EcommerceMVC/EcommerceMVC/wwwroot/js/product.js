var dataTable

$(document).ready(function () {
    loadDataTable()
});


function loadDataTable() {
    fetch("https://localhost:44392/Admin/Product/GetAll")
        .then(resp => resp.json())
        .then(data => {
            $('#tblData').DataTable({
                data: data,
                column: [
                    { "data": "name", "width": "15%" },
                    { "data": "description", "width": "15%" },
                    { "data": "price", "width": "15%" },
                    { "data": "category.name", "width": "15%" },
                    {
                        "data": "id",
                        "render": function (data) {
                            return `
                                <div class="w-75 btn-group" role="group">
                                    <a href="/Admin/Product/Upsert?id=${data}"
                                    class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit</a>
                                    <a onclick=Delete('/Admin/Product/Delete/${data}')
                                    class="btn btn-danger mx-2"><i class="bi bi-trash3-fill"></i> Delete</a>
                                </div>
                                `
                        },
                        "width": "15%"
                    },
                ],
            })
        })
};



function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        console.log(url)
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    console.log(data)
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
                })
        }
    })
}