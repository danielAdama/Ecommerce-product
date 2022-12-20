$(document).ready(function () {
    $('#tblData').DataTable({
        "ajax": {
            "url": "Product/GetAll"
        },
        "columns": [
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
                                <a asp-area="Sdmin" asp-controller="Category" asp-action="Delete" asp-route-id="@obj.Id" 
                                class="btn btn-danger mx-2"><i class="bi bi-trash3-fill"></i> Delete</a>
                            </div>
                            `
                },
                "width": "15%"
            },
        ]
    });
});
