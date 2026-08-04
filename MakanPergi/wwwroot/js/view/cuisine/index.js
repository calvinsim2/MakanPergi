(function ($) {
    if (DataTable.isDataTable('#cuisineTable')) {
        $('#cuisineTable').DataTable().clear().destroy();
    }

    $('#cuisineTable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: "/Cuisine/LoadCuisineData",
            type: "POST"
        },
        columns: [
            {
                targets: 0,
                data: "id",
                visible: false,
                searchable: false,
                orderable: false
            },
            {
                targets: 1,
                data: "name",
                visible: true,
                searchable: true
            },
            {
                targets: 2,
                data: null,
                searchable: false,
                orderable: false,
                defaultContent: '', 
                render: function (data, type, row, meta) {
                    return [`
                        <button type="button" class="btn btn-sm bg-success edit-cuisine" data-cuisine-id="${row.id}">
                            <i class="fa fa-pencil"></i>
                        </button>`,
                        `
                        <button type="button" class="btn btn-sm bg-danger delete-cuisine" data-cuisine-id="${row.id}">
                            <i class="fa fa-trash"></i>
                        </button>`].join('');
                }
            },
        ],
        columnDefs: [
            { "width": "100px", "targets": 2 }
        ]
    });


    $(document).on('click', '#btnCreate', (e) => {
        var actionUrl = getBaseUrl() + '/Cuisine/GetCreateCuisineModal';

        $.ajax({
            url: actionUrl,
            type: 'GET',
            dataType: 'html'
        }).done(function (content) {
            $('#CuisineModalPlaceholder div.modal-content').html(DOMPurify.sanitize(content));
            $('#CuisineModalPlaceholder').modal('show');
        }).fail(function (e) {
            console.error("Failed to load modal", e);
            uiCommon.notify.error("Get Create Cuisine Modal Failed");
        }) 
    })

    $(document).on('click', '.edit-cuisine', function (e) {

        let cuisineId = $(this).attr("data-cuisine-id");
        let actionUrl = getBaseUrl() + `/Cuisine/GetEditCuisineModal/${cuisineId}`;

        $.ajax({
            url: actionUrl,
            type: 'GET',
            success: function (content) {
                $('#CuisineModalPlaceholder div.modal-content').html(DOMPurify.sanitize(content));
                $('#CuisineModalPlaceholder').modal('show');
            },
            error: function (xhr) {
                console.error(xhr);
                uiCommon.notify.error("Get Edit Cuisine Modal Failed");
            }
        });
    })

    $(document).on('click', '.delete-cuisine', function (e) {
        let cuisineId = $(this).attr("data-cuisine-id");
        let actionUrl = getBaseUrl() + `/Cuisine/DeleteCuisine/${cuisineId}`;
        let _$cuisineTable = $('#cuisineTable');

        uiCommon.ui.block(_$cuisineTable);
        $.ajax({
            url: actionUrl,
            type: 'DELETE',
            success: function (content) {
                _$cuisineTable.DataTable().ajax.reload();
                uiCommon.notify.success("Cuisine deleted successfully!", "Deleted");
            },
            error: function (xhr) {
                uiCommon.notify.error(xhr.responseText);
            },
            complete: function () {
                uiCommon.ui.unblock(_$cuisineTable);
            }
        });
    })

    $(document).on('click', '#btnBulkUpload', (e) => {
        var actionUrl = getBaseUrl() + '/cuisine/CuisineBulkUpload';

        $.ajax({
            url: actionUrl,
            type: 'GET',
        }).done(function () {
            window.location.href = actionUrl;
        }).fail(function (xhr) {
            uiCommon.notify.error(xhr.responseText);
        })
    })

})(jQuery);

