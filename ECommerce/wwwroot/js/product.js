var dataTable;

$(document).ready(function (){
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url":"/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" }, //case sensibilite majuscule et minuscule doivent etre le meme
            { "data": "brand", "width": "15%" },
            { "data": "price", "width": "15%" },
            { "data": "category.name", "width": "15%" }, // category.name le . pour specifier l'atribut de l'objet category
            {
                "data": "id",
                "render": function (data) { //pour ajouter les function que normalement on fait avec asp-controller
                    return `
                        <div class="w-75 btn-group" role="group">
                        <a href="/Admin/Product/Upsert?id=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i>modifier </a>
                        <a onClick=Delete('/Admin/Product/Delete/${data}')
                        class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i>Supprimer
                        </a>
                        </div>
                    `
                }, "width": "30%"
            }
            
        ]


    });
}

function Delete(url) {
    Swal.fire({
        title: 'Etes-vous sure?',
        text: "Vous ne pourrez pas revenir en arrière!",
        icon: 'Alerte',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Oui, Supprimer!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
                })
        }
    })
}