
var dataTable;

$(document).ready(function () {
    loadData();
});

function loadData() {
    dataTable = $("#data_table").DataTable({
        "ajax": {
            "url": "/songs/getallsongs/",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "title", "width": "20%" },
            { "data": "author", "width": "30%" },
            { "data": "category", "width": "15%" },
            { "data": "yearPublished", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class='text-center'>
                            <a href="/Songs/Edit?id=${data}" class='btn btn-outline-primary' 
                                    style='cursor:pointer; width:75px;'>
                                    <i class="fas fa-edit"></i>
                            </a>
                            <a onclick="Delete('/Songs/Delete?id='+${data})" class='btn btn-outline-danger' 
                                    style='cursor:pointer; width:75px;'>
                                   <i class="fas fa-trash-alt"></i>
                            </a>
                        </div>`
                }, "width": "15%"
            }
        ],
        "language": {
            "emptyTable": "there are no songs in database yet"
        }, "width": "100%"
    });
}


function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "This method will permanently delete song from the list.",
        icon: "warning",
        showCloseButton: true,
        showCancelButton: true,
        confirmButtonText:
            '<i class="fa fa-thumbs-up"></i> &nbsp; Yes!',
        cancelButtonText:
            '<i class="fa fa-thumbs-down"></i> &nbsp; Cancel',
        confirmButtonColor: '#dc3545',
        cancelButtonColor:'#6c757d',

    }).then((toDelete) => {
        if (toDelete.isConfirmed) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    toastr.options = {
                        "closeButton": true,
                        "debug": false,
                        "newestOnTop": true,
                        "progressBar": true,
                        "positionClass": "toast-top-right",
                        "preventDuplicates": false,
                        "onclick": null,
                        "showDuration": "1000",
                        "hideDuration": "1000",
                        "timeOut": "3000",
                        "extendedTimeOut": "3000",
                        "showEasing": "swing",
                        "hideEasing": "linear",
                        "showMethod": "fadeIn",
                        "hideMethod": "fadeOut"
                    }
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }

            });
        }
    });
}