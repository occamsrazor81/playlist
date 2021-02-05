
var dataTable;

$(document).ready(function () {
    loadData();
});

function loadData() {
    dataTable = $("#playlist_table").DataTable({
        "ajax": {
            "url": "/playlists/getplaylists/",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "title", "width": "70%",
                "fnCreatedCell": function (nTd, sData, oData, iRow, iCol) {
                    if (oData.isPrivate)
                        $(nTd).html(
                            "<i class='fas fa-key'></i> &nbsp;" + oData.title
                        );
                    else $(nTd).html(
                        "<i class='far fa-eye'> &nbsp;" + oData.title + "</i>")
                }
            },
            {
                "data": { "id": "id", "title": "id" },
                "render": function (data) {
                    if (data.title !== "FAVORITES" && data.title !== "BLACKLIST")
                        return `<div class='text-center'>
                            <a href="/Playlists/EditPlaylist?id=${data.id}" 
                               class="btn btn-outline-primary mr-1 mb-1" style="width:75px">
                                <i class="fas fa-edit"></i>
                            </a>
                            <a onclick="DeletePlaylist('/Playlists/DeletePlaylist?id='+${data.id})" class='btn btn-outline-danger' 
                                    style='cursor:pointer; width:75px;'>
                                   <i class="fas fa-trash-alt"></i>
                            </a>
                        </div>`
                    else return `<div class='text-center'>
                           <a href="/Playlists/EditPlaylist?id=${data.id}" 
                               class="btn btn-outline-primary mr-1 mb-1" style="width:75px">
                                <i class="fas fa-edit"></i>
                            </a> 
                        </div>`
                }, "width": "30%"
            }
        ],
        "language": {
            "emptyTable": "You didn't create any playlists yet. "
        }, "width": "100%"
    });
}


function DeletePlaylist(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "This method will permanently delete your playlist (and also remove all songs from it).",
        icon: "warning",
        showCloseButton: true,
        showCancelButton: true,
        confirmButtonText:
            '<i class="fa fa-thumbs-up"></i> &nbsp; Yes!',
        cancelButtonText:
            '<i class="fa fa-thumbs-down"></i> &nbsp; Cancel',
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',

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