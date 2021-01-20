
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
            {
                "data": "title", "width": "30%",

            },
            { "data": "author", "width": "20%" },
            { "data": "category", "width": "13%" },
            { "data": "yearPublished", "width": "7%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class='text-center clearfix'>   
                            <div class='float-left'>
                            <a href="/Songs/Edit?id=${data}" class='btn btn-outline-primary' 
                                    style='cursor:pointer; width:60px;'>
                                    <i class="fas fa-edit"></i>
                            </a>
                            <a onclick="Delete('/Songs/Delete?id='+${data})" class='btn btn-outline-danger' 
                                    style='cursor:pointer; width:60px;'>
                                   <i class="fas fa-trash-alt"></i>
                            </a>
                            </div>
                            <div class='float-right'>
                            <a onclick="Like('/Songs/Like?id='+${data})" 
                                    class='btn btn-outline-success' 
                                    style='cursor:pointer; width:60px;'>
                                   <i class="fas fa-thumbs-up"></i>
                            </a>
                            <a onclick="Dislike('/Songs/Dislike?id='+${data})" class='btn btn-outline-danger' 
                                    style='cursor:pointer; width:60px;'>
                                   <i class="fas fa-thumbs-down"></i>
                            </a>
                            </div>
                        </div>`
                }, "width": "30%"
            },
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


function Like(url) {
    console.log(url);
    $.ajax({
        url: url,
        type: 'POST',
        success: function (data) {
            if (data.success) {
                Swal.fire({
                    title: "Request successful",
                    text: data.message,
                    icon: data.mark,
                    confirmButtonText: '<i class="fas fa-thumbs-up"></i> &nbsp; OK',
                    confirmButtonColor: data.color
                });
            }

            else Swal.fire({
                title: data.message.substring(0, 14),
                text: data.message.substring(16, 33).charAt(0).toUpperCase() +
                    data.message.substring(16, 33).slice(1),
                icon: "error",
                confirmButtonText: '<i class="fas fa-user-cog"></i> &nbsp; OK',
                confirmButtonColor: '#dc3545'
            });
        }
    })
}