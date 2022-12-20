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
                                <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> edit</a>
              <a asp-area="admin" asp-controller="category" asp-action="delete" asp-route-id="@obj.Id" class="btn btn-danger mx-2"><i class="bi bi-trash3-fill"></i> delete</a>
             </div>
                            `
                },
                "width": "15%"
            },
        ]
    });
});
