
$(document).ready(function () {
})

function Add(url)
{
    event.preventDefault();
    var title = $("#title").val();
    var author = $("#author").val();
    var category = $("#category").val();
    var yearPublished = parseInt($("#year_published").val());

    var data = {
        title,
        author,
        category,
        yearPublished
    };

    if (title && author && category && (1799 < yearPublished && yearPublished < 2101)) {
        Swal.fire({
            icon: 'success',
            title: 'Song added',
            text: 'New song successfully added to database!'
        }).then((toAdd) => {
            if (toAdd) {
                event.currentTarget.submit(); 
            }
        });
    }
}