
$(document).ready(function () {
});

$(".removeBtn").click(removeSingleSong);

function removeSingleSong()  {
    var id = $(this).attr("id");
    var songId = id.substring(0, id.indexOf('+'));
    var playlistId = id.substring(id.lastIndexOf('+') + 1);

    $.ajax({
        url: "/playlists/RemoveSingleSong/",
        type: "delete",
        data: {
            songId: songId,
            playlistId: playlistId
        },
        dataType: "json",
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
            toastr.success(data.successMsg);

            $("#" + songId).remove();
        }
    });
}