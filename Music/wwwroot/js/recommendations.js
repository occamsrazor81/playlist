

$(document).ready(function () {
    $('#searchAttributes').on('change', loadCheckboxes);
});


$("body").on("click", "#findButton", findRecommendationsRequest);


function loadCheckboxes(event) {
    $('#checkboxTableDiv').html("");
    $("#searchAttributes option:first").attr("disabled", true);
    var currentAttribute = $("#searchAttributes option:selected").text();
    $.ajax({
        url: "/playlists/GetBySelectedAttribute/",
        type: "GET",
        data: {
            attr: currentAttribute
        },
        dataType: "json",
        success: function (data) {

            $("#resultTableDiv").html("");
            var checkboxTable = $('<table class="table table-bordered text-light"></table>');

            var tHead = $('<thead></thead>');
            var trHeader = $('<tr></tr>');
            var thColName = $('<th id="colName" style="width:100%"></th>');
            var thColCheckbox = $('<th></th>');

            var tBody = $('<tbody></tbody>');

            trHeader.append(thColName);
            trHeader.append(thColCheckbox);

            tHead.append(trHeader);
            checkboxTable.append(tHead);

            if (data.artists) {
                thColName.html("Artist");
                for (var i = 0; i < data.artists.length; i++) {
                    var tr = $('<tr></tr>');

                    var tdArtist = $('<td style="width:100%"></td>');
                    tdArtist.html(data.artists[i]);
                    var tdCheckbox = $('<td></td>');
                    var checkbox = $('<input type="checkbox" id="' + i + '" name="artist" value="' + data.artists[i] + '">');

                    tdCheckbox.append(checkbox);
                    tr.append(tdArtist);
                    tr.append(tdCheckbox);

                    //console.log("ARTISTS");
                    //console.log(tdArtist.html());
                    //console.log(tdCheckbox.html());

                    tBody.append(tr);
                    checkboxTable.append(tBody);

                }

                $('#checkboxTableDiv').append(checkboxTable);
            }
            else if (data.categories) {
                thColName.html("Category");
                for (var i = 0; i < data.categories.length; i++) {
                    var tr = $('<tr></tr>');

                    var tdCategory = $('<td style="width:100%"></td>');
                    tdCategory.html(data.categories[i]);
                    var tdCheckbox = $('<td></td>');
                    var checkbox = $('<input type="checkbox" id="' + i + '" name="category" value="' + data.categories[i] + '">');

                    tdCheckbox.append(checkbox);
                    tr.append(tdCategory);
                    tr.append(tdCheckbox);

                    tBody.append(tr);
                    checkboxTable.append(tBody);

                }

                $('#checkboxTableDiv').append(checkboxTable);
            }
            else if (data.yearsPublished) {
                thColName.html("Year Published");
                for (var i = 0; i < data.yearsPublished.length; i++) {
                    var tr = $('<tr></tr>');

                    var tdYP = $('<td style="width:100%"></td>');
                    tdYP.html(data.yearsPublished[i]);
                    var tdCheckbox = $('<td></td>');
                    var checkbox = $('<input type="checkbox" id="' + i + '" name="year_published" value="' + data.yearsPublished[i] + '">');

                    tdCheckbox.append(checkbox);
                    tr.append(tdYP);
                    tr.append(tdCheckbox);

                    tBody.append(tr);
                    checkboxTable.append(tBody);

                }
                $('#checkboxTableDiv').append(checkboxTable);
            }

            // treba jos dodat submit button
            var findButton = $('<button id="findButton" class="btn btn-outline-info p-2"></button>');
            findButton.html('<i class="fas fa-search"></i> &nbsp; Find Recommendations');

            $('#checkboxTableDiv').append(findButton);
        }
    });
}


function findRecommendationsRequest(event) {
    var searchAttribute = $("#colName").html().toLowerCase().replace(" ", "_");
    //console.log(searchAttribute);
    var selectedCheckboxes = $('input[name="' + searchAttribute + '"]:checked');

    var selectedValuesList = [];

    selectedCheckboxes.each(function () {
        //var listInList = this.value.split(" / ");
        //for (var j = 0; j < listInList.length; ++j)
        //    if (!selectedValuesList.includes(listInList[j]))
        //        selectedValuesList.push(listInList[j]);
        selectedValuesList.push(this.value);
    });



    $.ajax({
        url: "/playlists/MyRecommendations/",
        type: "POST",
        data: {
            attr: searchAttribute,
            values: selectedValuesList
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
            if (data.msg) {
                // dogodila se neka greska prilikom dohvata
                toastr.error(data.msg);
            }

            else {

                $("#checkboxTableDiv").html("");

                console.log(data);

                var attrRefined = data.attr.replace("_", " ");
                attrRefined = attrRefined.charAt(0).toUpperCase() + attrRefined.slice(1);
                toastr.success("Recommendations based on " + attrRefined);

                var resultTable = $('<table class="table table-bordered text-light"></table>');
                var tHead = $("<thead></thead>");
                var trHead = $("<tr></tr>");

                var thTitle = $("<th></th>");
                var thArtist = $("<th></th>");
                var thCategory = $("<th></th>");
                var thPublished = $("<th></th>");
                var thOption = $("<th></th>");

                thTitle.html("Title");
                thArtist.html("Artist");
                thCategory.html("Category");
                thPublished.html("Published");

                trHead.append(thTitle);
                trHead.append(thArtist);
                trHead.append(thCategory);
                trHead.append(thPublished);
                trHead.append(thOption);

                tHead.append(trHead);
                resultTable.append(tHead);


                var tBody = $("<tbody></tbody>");

                for (var i = 0; i < data.recommendations.length; ++i) {

                    var tr = $("<tr></tr>");

                    var tdTitle = $("<td></td>");
                    var tdArtist = $("<td></td>");
                    var tdCategory = $("<td></td>");
                    var tdPublished = $("<td></td>");
                    var tdOption = $("<td style='max-width:150px'></td>");

                    var titleLink = $('<a href="' + data.recommendations[i].link +'" class="text-decoration-none text-info"></a>');

                    titleLink.html(data.recommendations[i].title);
                    tdArtist.html(data.recommendations[i].artist);
                    tdCategory.html(data.recommendations[i].category);
                    tdPublished.html(data.recommendations[i].yearPublished);


                    tdTitle.append(titleLink);
                    tr.append(tdTitle);
                    tr.append(tdArtist);
                    tr.append(tdCategory);
                    tr.append(tdPublished);


                    var divOut = $('<div class="text-center clearfix"></div>');
                    var divLike = $('<div class="float-md-left m-1"></div>');
                    var divDislike = $('<div class="float-md-right m-1"></div>');

                    var aLike = $('<a id="#' + data.recommendations[i].id + '" class="btn btn-outline-success like" style="cursor: pointer; width:60px;"></a>');
                    var aDislike = $('<a id="#' + data.recommendations[i].id + '" class="btn btn-outline-danger dislike"  style="cursor: pointer; width:60px;"></a>');

                    var iconThumbsUp = $('<i class="fas fa-thumbs-up"></i>');
                    var iconThumbsDown = $('<i class="fas fa-thumbs-down"></i>');

                    aLike.append(iconThumbsUp);
                    aDislike.append(iconThumbsDown);                    

                    divLike.append(aLike);
                    divDislike.append(aDislike);

                    divOut.append(divLike);
                    divOut.append(divDislike);
                    tdOption.append(divOut);
                    tr.append(tdOption);

                    tBody.append(tr);
                }
                resultTable.append(tBody);


                $("#resultTableDiv").append(resultTable);
            }
        }
    });
}

$("body").on("click", ".like", like);
$("body").on("click", ".dislike", dislike);

function like(event) {
    var id = $(this).attr("id").replace("#", "");   

    var url = '/Songs/Like?id=' + id;

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

function dislike(event) {
    var id = $(this).attr("id").replace("#", "");
    var url = '/Songs/Dislike?id=' + id;

    $.ajax({
        url: url,
        type: 'POST',
        success: function (data) {
            if (data.success) {
                Swal.fire({
                    title: "Request successful",
                    text: data.message,
                    icon: data.mark,
                    confirmButtonText: '<i class="fas fa-thumbs-down"></i> &nbsp; OK',
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