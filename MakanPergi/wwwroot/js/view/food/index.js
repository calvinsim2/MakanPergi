import { createTree, disableTree } from '../../TreeViewHelper.js';

(function ($) {
    if (DataTable.isDataTable('#foodTable')) {
        $('#foodTable').DataTable().clear().destroy();
    }

    $('#foodTable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: "/Food/LoadFoodsData",
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
                data: "cuisineName",
                visible: true,
                searchable: true
                //render: function (data, type, row, meta) {
                //    return data && data.name ? data.name : '';
                //}
            },
            {
                targets: 3,
                data: "categoryNames",
                visible: true,
                searchable: true,
                orderable: false
                //render: function (data, type, row, meta) {
                //    /*let mapped = data.map(category => `<p> ${category.name} <p>`);*/
                //    if (data.length === 0) {
                //        return ''
                //    }

                //    let mappedCategoryName = data.map(category => category.name);
                //    return mappedCategoryName.join(", ")
                //}
            },
            {
                targets: 4,
                data: "shopNames",
                visible: true,
                searchable: true,
                orderable: false
                //render: function (data, type, row, meta) {
                //    /*let mapped = data.map(category => `<p> ${category.name} <p>`);*/
                //    if (data.length === 0) {
                //        return ''
                //    }

                //    let mappedshopName = data.map(shop => shop.name);
                //    return mappedshopName.join(", ")
                //}
            },
            {
                targets: 5,
                data: null,
                searchable: false,
                orderable: false,
                defaultContent: '',
                render: function (data, type, row, meta) {
                    return [`
                        <button type="button" class="btn btn-sm bg-success edit-food" data-food-id="${row.id}">
                            <i class="fa fa-pencil"></i>
                        </button>`,
                    `
                        <button type="button" class="btn btn-sm bg-danger delete-food" data-food-id="${row.id}">
                            <i class="fa fa-trash"></i>
                        </button>`].join('');
                }
            },
        ],
        columnDefs: [
            { "width": "100px", "targets": 5 }
        ]
    });

    $(document).on('click', "#btnCreate", function (e) {
        var actionUrl = getBaseUrl() + '/Food/GetCreateFoodModal';

        $.ajax({
            url: actionUrl,
            type: 'GET',
            dataType: 'html'
        }).done(function (content) {
            $('#FoodModalPlaceholder div.modal-content').html(DOMPurify.sanitize(content));
            $('#FoodModalPlaceholder').modal('show');

            $(document).ready(function () {
                $('.multi-select').select2();
            });

            $.ajax({
                url: getBaseUrl() + '/Food/GetCategoryTreeView',
                method: 'GET',
                dataType: 'json'
            })
                .done((res) => {
                    $('.multi-select').select2();
                    createTree($("#categorylocationTree"), $("#hiddenCategoryLocationTree"), res)
                        .on('changed.jstree', (e, data) => {
                            if (!data || !data.node) {
                                console.error('No node data available');
                                return; // Exit if no node is available
                            }
                            var selectedItems = [];
                            var newlySelected = [];  // To hold the newly selected node and its children

                            /*console.log(`Tree change detected: ${JSON.stringify(data.node)}`);*/

                            // whenever we click on a node, the data input has an 'action' parameter, 
                            // which the value will be 'select_node'
                            if (data.action === 'select_node') {
                                // Newly selected node
                                // when a node is selected, there is a 'node' parameter, and in it, 
                                // it has an 'id' parameter.
                                var newId = data.node.id;

                                // basically, what we are doing is just pushing this newly selected node
                                // into the array 'newlySelected'
                                newlySelected.push({
                                    Id: newId,
                                    Text: data.node.text
                                });

                                // Get the children of the newly selected node and select them
                                // get_node, select_node, and children_d— are provided by jsTree

                                // get_node(newId) -> returns the internal “node object” for that ID (complete with all of its properties).
                                // children_d -> node.children_d is an array of all descendant IDs (i.e. the node’s entire subtree, flattened) 
                                const children = data.instance.get_node(newId).children_d;
                                children.forEach(childId => {

                                    // select_node(childId) -> programmatically checks (selects) that node, 
                                    //                         firing whatever selection logic / events you’ve wired up
                                    data.instance.select_node(childId); // Auto-select child nodes
                                    newlySelected.push({
                                        Id: childId,
                                        Parent: newId,
                                        Text: data.instance.get_node(childId).text
                                    });
                                });

                                /*console.log('Newly clicked node and its children:', newlySelected);*/
                            }

                            // data.selected -> its an array, provided by jsTree, which tells us
                            // the current options that are selected.

                            // For all previously selected nodes (except the newly selected one)
                            for (var i = 0; i < data.selected.length; i++) {
                                var id = data.selected[i];

                                // Skip the newly selected node (already processed above)
                                if (id === data.node.id) continue;

                                // Simply push the previously selected node without processing children
                                selectedItems.push({
                                    Id: id,
                                    Text: data.instance.get_node(id).text
                                });
                            }

                            // console.log('Previously selected nodes (not reprocessed):', selectedItems);

                            // Combine newly selected with previously selected
                            var allSelected = [...newlySelected, ...selectedItems];

                            console.log(`Selected Nodes Result: ${JSON.stringify(allSelected)}`);
                            $("#hiddenCategoryLocationTree").val(JSON.stringify(allSelected));

                        });
                })
                .fail((error) => {
                    uicommon.notify.error(error);
                });

            $.ajax({
                url: getBaseUrl() + '/Food/GetShopTreeView',
                method: 'GET',
                dataType: 'json'
            })
                .done((res) => {
                    $('.multi-select').select2();
                    createTree($("#shoplocationTree"), $("#hiddenShopLocationTree"), res, true)
                        .on('changed.jstree', (e, data) => {
                            if (!data || !data.node) {
                                console.error('No node data available');
                                return; // Exit if no node is available
                            }
                            var selectedItems = [];
                            var newlySelected = [];  // To hold the newly selected node and its children

                            /*console.log(`Tree change detected: ${JSON.stringify(data.node)}`);*/

                            // whenever we click on a node, the data input has an 'action' parameter, 
                            // which the value will be 'select_node'
                            if (data.action === 'select_node') {
                                // Newly selected node
                                // when a node is selected, there is a 'node' parameter, and in it, 
                                // it has an 'id' parameter.
                                var newId = data.node.id;

                                // basically, what we are doing is just pushing this newly selected node
                                // into the array 'newlySelected'
                                newlySelected.push({
                                    Id: newId,
                                    Parent: data.node.parent,
                                    Text: data.node.text
                                });

                                // Get the children of the newly selected node and select them
                                // get_node, select_node, and children_d— are provided by jsTree

                                // get_node(newId) -> returns the internal “node object” for that ID (complete with all of its properties).
                                // children_d -> node.children_d is an array of all descendant IDs (i.e. the node’s entire subtree, flattened) 
                                const children = data.instance.get_node(newId).children_d;
                                children.forEach(childId => {

                                    // select_node(childId) -> programmatically checks (selects) that node, 
                                    //                         firing whatever selection logic / events you’ve wired up
                                    data.instance.select_node(childId); // Auto-select child nodes
                                    newlySelected.push({
                                        Id: childId,
                                        Parent: newId,
                                        Text: data.instance.get_node(childId).text
                                    });
                                });

                                /*console.log('Newly clicked node and its children:', newlySelected);*/
                            }

                            // data.selected -> its an array, provided by jsTree, which tells us
                            // the current options that are selected.

                            // For all previously selected nodes (except the newly selected one)
                            for (var i = 0; i < data.selected.length; i++) {
                                var id = data.selected[i];

                                // Skip the newly selected node (already processed above)
                                if (id === data.node.id) continue;

                                // Simply push the previously selected node without processing children
                                selectedItems.push({
                                    Id: id,
                                    Parent: data.instance.get_node(id).parent,
                                    Text: data.instance.get_node(id).text
                                });
                            }

                            // console.log('Previously selected nodes (not reprocessed):', selectedItems);

                            // Combine newly selected with previously selected
                            var allSelected = [...newlySelected, ...selectedItems];

                            console.log(`Selected Nodes Result: ${JSON.stringify(allSelected)}`);
                            $("#hiddenShopLocationTree").val(JSON.stringify(allSelected));

                        });
                })
                .fail((error) => {
                    uicommon.notify.error(error);
                });

        }).fail(function (e) {
            console.error("Failed to load modal", e);
            uiCommon.notify.error("Get Create Food Modal Failed");
        })
    })


    $(document).on('click', '.edit-food', function (e) {

        let foodId = $(this).attr("data-food-id");
        let actionUrl = getBaseUrl() + `/Food/GetEditFoodModal/${foodId}`;

        $.ajax({
            url: actionUrl,
            type: 'GET',
            success: function (content) {
                $('#FoodModalPlaceholder div.modal-content').html(DOMPurify.sanitize(content));
                $('#FoodModalPlaceholder').modal('show');

                $(document).ready(function () {
                    $('.multi-select').select2();
                });


                $.ajax({
                    url: getBaseUrl() + `/Food/GetCategoryTreeView/${foodId}`,
                    method: 'GET',
                    dataType: 'json'
                })
                    .done((res) => {
                        $('.multi-select').select2();
                        createTree($("#categorylocationTree"), $("#hiddenCategoryLocationTree"), res)
                            .on('changed.jstree', (e, data) => {
                                if (!data || !data.node) {
                                    console.error('No node data available');
                                    return; // Exit if no node is available
                                }
                                var selectedItems = [];
                                var newlySelected = [];  // To hold the newly selected node and its children

                                /*console.log(`Tree change detected: ${JSON.stringify(data.node)}`);*/

                                // whenever we click on a node, the data input has an 'action' parameter, 
                                // which the value will be 'select_node'
                                if (data.action === 'select_node') {
                                    // Newly selected node
                                    // when a node is selected, there is a 'node' parameter, and in it, 
                                    // it has an 'id' parameter.
                                    var newId = data.node.id;

                                    // basically, what we are doing is just pushing this newly selected node
                                    // into the array 'newlySelected'
                                    newlySelected.push({
                                        Id: newId,
                                        Text: data.node.text
                                    });

                                    // Get the children of the newly selected node and select them
                                    // get_node, select_node, and children_d— are provided by jsTree

                                    // get_node(newId) -> returns the internal “node object” for that ID (complete with all of its properties).
                                    // children_d -> node.children_d is an array of all descendant IDs (i.e. the node’s entire subtree, flattened) 
                                    const children = data.instance.get_node(newId).children_d;
                                    children.forEach(childId => {

                                        // select_node(childId) -> programmatically checks (selects) that node, 
                                        //                         firing whatever selection logic / events you’ve wired up
                                        data.instance.select_node(childId); // Auto-select child nodes
                                        newlySelected.push({
                                            Id: childId,
                                            Parent: newId,
                                            Text: data.instance.get_node(childId).text
                                        });
                                    });

                                    /*console.log('Newly clicked node and its children:', newlySelected);*/
                                }

                                // data.selected -> its an array, provided by jsTree, which tells us
                                // the current options that are selected.

                                // For all previously selected nodes (except the newly selected one)
                                for (var i = 0; i < data.selected.length; i++) {
                                    var id = data.selected[i];

                                    // Skip the newly selected node (already processed above)
                                    if (id === data.node.id) continue;

                                    // Simply push the previously selected node without processing children
                                    selectedItems.push({
                                        Id: id,
                                        Text: data.instance.get_node(id).text
                                    });
                                }

                                // console.log('Previously selected nodes (not reprocessed):', selectedItems);

                                // Combine newly selected with previously selected
                                var allSelected = [...newlySelected, ...selectedItems];

                                console.log(`Selected Nodes Result: ${JSON.stringify(allSelected)}`);
                                $("#hiddenCategoryLocationTree").val(JSON.stringify(allSelected));

                            });
                    })
                    .fail((error) => {
                        uiCommon.notify.error(error);
                    });

                $.ajax({
                    url: getBaseUrl() + `/Food/GetShopTreeView/${foodId}`,
                    method: 'GET',
                    dataType: 'json'
                })
                    .done((res) => {
                        $('.multi-select').select2();
                        createTree($("#shoplocationTree"), $("#hiddenShopLocationTree"), res, true)
                            .on('changed.jstree', (e, data) => {
                                if (!data || !data.node) {
                                    console.error('No node data available');
                                    return; // Exit if no node is available
                                }
                                var selectedItems = [];
                                var newlySelected = [];  // To hold the newly selected node and its children

                                /*console.log(`Tree change detected: ${JSON.stringify(data.node)}`);*/

                                // whenever we click on a node, the data input has an 'action' parameter, 
                                // which the value will be 'select_node'
                                if (data.action === 'select_node') {
                                    // Newly selected node
                                    // when a node is selected, there is a 'node' parameter, and in it, 
                                    // it has an 'id' parameter.
                                    var newId = data.node.id;

                                    // basically, what we are doing is just pushing this newly selected node
                                    // into the array 'newlySelected'
                                    newlySelected.push({
                                        Id: newId,
                                        Parent: data.node.parent,
                                        Text: data.node.text
                                    });

                                    // Get the children of the newly selected node and select them
                                    // get_node, select_node, and children_d— are provided by jsTree

                                    // get_node(newId) -> returns the internal “node object” for that ID (complete with all of its properties).
                                    // children_d -> node.children_d is an array of all descendant IDs (i.e. the node’s entire subtree, flattened) 
                                    const children = data.instance.get_node(newId).children_d;
                                    children.forEach(childId => {

                                        // select_node(childId) -> programmatically checks (selects) that node, 
                                        //                         firing whatever selection logic / events you’ve wired up
                                        data.instance.select_node(childId); // Auto-select child nodes
                                        newlySelected.push({
                                            Id: childId,
                                            Parent: newId,
                                            Text: data.instance.get_node(childId).text
                                        });
                                    });

                                    /*console.log('Newly clicked node and its children:', newlySelected);*/
                                }

                                // data.selected -> its an array, provided by jsTree, which tells us
                                // the current options that are selected.

                                // For all previously selected nodes (except the newly selected one)
                                for (var i = 0; i < data.selected.length; i++) {
                                    var id = data.selected[i];

                                    // Skip the newly selected node (already processed above)
                                    if (id === data.node.id) continue;

                                    // Simply push the previously selected node without processing children
                                    selectedItems.push({
                                        Id: id,
                                        Parent: data.instance.get_node(id).parent,
                                        Text: data.instance.get_node(id).text
                                    });
                                }

                                // console.log('Previously selected nodes (not reprocessed):', selectedItems);

                                // Combine newly selected with previously selected
                                var allSelected = [...newlySelected, ...selectedItems];

                                console.log(`Selected Nodes Result: ${JSON.stringify(allSelected)}`);
                                $("#hiddenShopLocationTree").val(JSON.stringify(allSelected));

                            });
                    })
                    .fail((error) => {
                        uiCommon.notify.error(error);
                    });
            },
            error: function (xhr) {
                console.error(xhr);
                uiCommon.notify.error("Get Edit Food Modal Failed");
            }
        });
    })

    $(document).on('click', '.delete-food', function (e) {
        let categoryId = $(this).attr("data-food-id");
        let actionUrl = getBaseUrl() + `/Food/DeleteFood/${categoryId}`;
        let _$foodTable = $('#foodTable');

        uiCommon.ui.block(_$foodTable);
        $.ajax({
            url: actionUrl,
            type: 'DELETE',
            success: function (content) {
                _$foodTable.DataTable().ajax.reload();
                uiCommon.notify.success("Food deleted successfully!", "Deleted");
            },
            error: function (xhr) {
                uiCommon.notify.error(xhr.responseText);
            },
            complete: function () {
                uiCommon.ui.unblock(_$foodTable);
            }
        });
    })

})(jQuery)