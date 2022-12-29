var dataTable

$(document).ready(function () {
    loadDataTable()
});

function loadDataTable() {
    fetch("https://localhost:44392/Admin/Company/GetAll")
        .then(resp => resp.json())
        .then(data => {
            console.log(data.data)
            $('#tblData').DataTable({
                data: data.data,
                columns: [
                    { data: "name", "width": "15%" },
                    { data: "streetAddress", "width": "15%" },
                    { data: "city", "width": "15%" },
                    { data: "state", "width": "15%" },
                    { data: "phoneNumber", "width": "15%" },
                    {
                        "data": "id",
                        "render": function (data) {
                            return `
                                <div class="w-75 btn-group" role="group">
                                    <a href="/Admin/Company/Upsert?id=${data}"
                                    class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit</a>
                                    <a onclick=Delete("/Admin/Company/Delete/${data}")
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
    }).then(async (result) => {
        if (result.isConfirmed) {
            console.log(result);
            const response = await fetch(url, { method: 'DELETE' });
            console.log(response)
            if (response.ok) {
                dataTable.ajax.reload();
                toastr.success("successful");
            } else {
                toastr.error("something went wrong");
            }

            //$.ajax({
            //    url: url,
            //    type: 'DELETE',
            //    success: function (data) {
            //        console.log(data)
            //        if (data.success) {
            //            dataTable.ajax.reload();
            //            toastr.success(data.message);
            //        }
            //        else {
            //            toastr.error(data.message);
            //        }
            //    }
            //    })
        }
    })
}