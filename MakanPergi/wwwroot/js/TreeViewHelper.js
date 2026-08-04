export function createTree(tree, hiddenTree, data, setIcon = false, closeAll = true, doneCallback = null) {
    return tree.jstree({
        'core': {
            'themes': {
                'name': 'default',
                'responsive': true,
                'variant': 'large',
                'icons': setIcon
            },
            'check_callback': true,
            // tell jsTree how to fetch its data.
            // node is the jsTree node requesting its children (on first load, node.id === "#", the “root”).
            // cb is the callback you call with whatever JSON array you want it to render under that node.
            'data': function (node, cb) { cb(data); }
        },
        'checkbox': {
            three_state: false,    // no tri-state behaviour - disabling tri-state prevents auto-toggling parents
            cascade: 'down',       // selecting parent only selects children, not vice-versa
            'keep_selected_style': true
        },
        'plugins': ['checkbox'],
    }).on('ready.jstree', () => {
        // ready.jstree, only fires once, when the tree is started.

        // HACK: Because the model binder does not bind arrays
        //       properly to the input element's val, I'm doing
        //       it this way.
        // Only input the data that is selected
        //console.log(JSON.stringify(data.filter(node => node.state.selected == true)));

        // tree is a jQuery object, dependent on what we passed in, for example, at _EditFoodModal.cshtml, in editFood.js we passed in
        // #categorylocationTree. 
        // calling js.tree basically means to retrieve the instance of the object of jsTree
        const inst = tree.jstree(true);
        // this inst, will have inbuilt methods such as get_checked(), get_node(), select_node() etc.
        // This line will retrieve all nodes which is checked. Also, this fetches the full node objects 
        // etc {id: ...., Parent: ....., Text: ....} because we passed true in the argument.
        const checkedNodes = inst.get_checked(true);

        // Exclude group/container nodes (nodes that have children) - only leaf nodes are
        // real selectable data. We can't filter by `parent !== "#"` here: that assumes every
        // top-level node is a container, which is true for the Shop tree (region shops sit at
        // the top) but false for the Category tree (categories are flat, so every real
        // selection has parent "#" and would be wrongly dropped).
        const payload = checkedNodes
            .filter(n => n.children.length === 0)
            .map(n => ({
                Id: n.id,
                Parent: n.parent,
                Text: n.text
            }));

        // Now, we just stringify the payload, and then set the value of hiddenTree to these selected nodes, and jsTree now
        // knows these nodes are pre-selected.
        // for example, at _EditFoodModal.cshtml, in editFood.js we passed in
        // #hiddenCategoryLocationTree.
        hiddenTree.val(JSON.stringify(payload));

        if (closeAll) tree.jstree("close_all");
        if (doneCallback) doneCallback();
    });
}

export function disableTree(treeNodeSelector, treeSelector, disable = true) {
    treeNodeSelector.each((index, element) => {
        treeSelector.jstree().disable_node(element);
    });
}