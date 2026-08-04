(function ($) {
    if (DataTable.isDataTable('#categoryTable')) {
        $('#categoryTable').DataTable().clear().destroy();
    }

    $('#categoryTable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: "/Category/LoadCategoriesData",
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
                        <button type="button" class="btn btn-sm bg-success edit-category" data-category-id="${row.id}">
                            <i class="fa fa-pencil"></i>
                        </button>`,
                    `
                        <button type="button" class="btn btn-sm bg-danger delete-category" data-category-id="${row.id}">
                            <i class="fa fa-trash"></i>
                        </button>`].join('');
                }
            },
        ],
        columnDefs: [
            { "width": "100px", "targets": 2 }
        ]
    });

    $(document).on('click', "#btnCreate", function (e) {
        var actionUrl = getBaseUrl() + '/Category/GetCreateCategoryModal';

        $.ajax({
            url: actionUrl,
            type: 'GET',
            dataType: 'html'
        }).done(function (content) {
            $('#CategoryModalPlaceholder div.modal-content').html(DOMPurify.sanitize(content));
            $('#CategoryModalPlaceholder').modal('show');
        }).fail(function (e) {
            console.error("Failed to load modal", e);
            uiCommon.notify.error("Get Create Category Modal Failed");
        }) 
    })

    $(document).on('click', '.edit-category', function (e) {

        let categoryId = $(this).attr("data-category-id");
        let actionUrl = getBaseUrl() + `/Category/GetEditCategoryModal/${categoryId}`;

        $.ajax({
            url: actionUrl,
            type: 'GET',
            success: function (content) {
                $('#CategoryModalPlaceholder div.modal-content').html(DOMPurify.sanitize(content));
                $('#CategoryModalPlaceholder').modal('show');
            },
            error: function (xhr) {
                console.error(xhr);
                uiCommon.notify.error("Get Edit Category Modal Failed");
            }
        });
    })

    $(document).on('click', '.delete-category', function (e) {
        let categoryId = $(this).attr("data-category-id");
        let actionUrl = getBaseUrl() + `/Category/DeleteCategory/${categoryId}`;
        let _$categoryTable = $('#categoryTable');

        uiCommon.ui.block(_$categoryTable);
        $.ajax({
            url: actionUrl,
            type: 'DELETE',
            success: function (content) {
                _$categoryTable.DataTable().ajax.reload();
                uiCommon.notify.success("Category deleted successfully!", "Deleted");
            },
            error: function (xhr) {
                uiCommon.notify.error(xhr.responseText);
            },
            complete: function () {
                uiCommon.ui.unblock(_$categoryTable);
            }
        });
    })

})(jQuery)