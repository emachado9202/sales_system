(function ($, window, document, undefined) {
    // Telerik grid smart AJAX state preserving
    $('.t-grid.grid-preservestate').on('dataBound', function (e) {
        var grid = $(this).data("tGrid"),
            href = $(this).data("statepreserver-href"),
            gridId = $(this).data("statepreserver-key");

        if (href) {
            $.ajax({
                type: "POST",
                url: href,
                async: true,
                data: {
                    gridId: gridId,
                    path: location.pathname + location.search,
                    page: grid.currentPage,
                    size: grid.pageSize,
                    orderBy: grid.orderBy,
                    groupBy: grid.groupBy,
                    filter: grid.filterBy
                },
                global: false
            });
        }
    });
})(jQuery, this, document);