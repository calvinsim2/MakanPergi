(function ($) {
    if (DataTable.isDataTable('#shopTable')) {
        $('#shopTable').DataTable().clear().destroy();
    }

    var collapsedGroups = {};

    $('#shopTable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: "/Shop/LoadShopsData",
            type: "POST",
            dataSrc: function (json) {

                console.log (json)
                if (json == null) {
                    return;
                }

                json.data.forEach(function (row) {
                    if (row.regionName == null || row.regionName === "No Group") {
                        row.regionName = "Region";
                    } else {
                        row.regionName = row.regionName.trim().replace(/\s+/g, ' ');
                    }
                });
                return json.data;
            }
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
                data: "address",
                visible: true,
                searchable: false
            },
            {
                targets: 3,
                data: "regionName",
                visible: true,
                searchable: true
                
            },
            {
                targets: 4,
                data: null,
                searchable: false,
                orderable: false,
                defaultContent: '',
                render: function (data, type, row, meta) {
                    return [`
                        <button type="button" class="btn btn-sm bg-success edit-shop" data-shop-id="${row.id}">
                            <i class="fa fa-pencil"></i>
                        </button>`,
                    `
                        <button type="button" class="btn btn-sm bg-danger delete-shop" data-shop-id="${row.id}">
                            <i class="fa fa-trash"></i>
                        </button>`].join('');
                }
            },
        ],
        columnDefs: [
            { "width": "100px", "targets": 4 }
        ],
        // rowGroup is an optional extension and plugin of Datatables, which groups rows based on shared value in dataSrc
        rowGroup: {
            dataSrc: "regionName",

            // startRender is a callback function that executes ONCE per UNIQUE group.
            // for example, if our group has 10 records, but there are only 2 distinct records that match dataSrc
            // then this startRender will only execute twice.
            // rows → a DataTables API object containing all rows in this group
            // group → the value being grouped on (e.g., "regionName")
            startRender: function (rows, group) {
                console.log(group)

                // collapsedGroups is a JS object storing which groups are collapsed.
                // !! coerces the value to a boolean(true or false).

                // we'll get this type of structure for collapsedGroups object (a key value pair)
                // where the key is the rowGroup unique instance, and the value is a boolean.
                
                // var collapsedGroups = {
                //     "Bedok": true,   -> this group is collapsed
                //     "Tampines": false,   -> this group is expanded
                //     "Region": true   -> this group is collapsed
                // };
                var collapsed = !!collapsedGroups[group];

                // rows.nodes() returns a list (array-like) of the raw DOM nodes 
                // (the < tr > HTML elements) that represent each row in the group.
                // example, rows.nodes() => [<tr>Shop 1</tr>, <tr>Shop 2</tr>, ...]
                rows.nodes().each(function (r) {
                    // r.style.display -> for each of these rows, if collapsed is true, we append "none" to the display
                    r.style.display = collapsed ? "none" : "";
                });

                return $('<tr class="bg-info"/>')
                    .append('<td colspan="8" class="rowGroup">' + group + '</td>')
                    // this appends an attribute, called 'data-name' with the value of 'group' to the <td> row.
                    .attr("data-name", group)
                    .toggleClass("collapsed", collapsed);
            }
        }
    });

    $(document).on('click', 'tr.bg-info', function () {
        var name = $(this).data('name');
        collapsedGroups[name] = !collapsedGroups[name];
        $('#shopTable').DataTable().draw(false);
    });

    $(document).on('click', "#btnCreate", function (e) {
        var actionUrl = getBaseUrl() + '/Shop/GetCreateShopModal';

        $.ajax({
            url: actionUrl,
            type: 'GET',
            dataType: 'html'
        }).done(function (content) {
            $('#ShopModalPlaceholder div.modal-content').html(DOMPurify.sanitize(content));
            $('#ShopModalPlaceholder').modal('show');
        }).fail(function (e) {
            console.error("Failed to load modal", e);
            uiCommon.notify.error("Get Create Shop Modal Failed");
        })
    })

    $(document).on('click', '.edit-shop', function (e) {

        let shopId = $(this).attr("data-shop-id");
        let actionUrl = getBaseUrl() + `/Shop/GetEditShopModal/${shopId}`;

        $.ajax({
            url: actionUrl,
            type: 'GET',
            success: function (content) {
                $('#ShopModalPlaceholder div.modal-content').html(DOMPurify.sanitize(content));
                $('#ShopModalPlaceholder').modal('show');
            },
            error: function (xhr) {
                console.error(xhr);
                uiCommon.notify.error("Get Edit Shop Modal Failed");
            }
        });
    })

    $(document).on('click', '.delete-shop', function (e) {
        let shopId = $(this).attr("data-shop-id");
        let actionUrl = getBaseUrl() + `/Shop/DeleteShop/${shopId}`;
        let _$shopTable = $('#shopTable');

        uiCommon.ui.block(_$shopTable);
        $.ajax({
            url: actionUrl,
            type: 'DELETE',
            success: function (content) {
                _$shopTable.DataTable().ajax.reload();
                uiCommon.notify.success("Shop deleted successfully!", "Deleted");
            },
            error: function (xhr) {
                uiCommon.notify.error(xhr.responseText);
            },
            complete: function () {
                uiCommon.ui.unblock(_$shopTable);
            }
        });
    })

})(jQuery)