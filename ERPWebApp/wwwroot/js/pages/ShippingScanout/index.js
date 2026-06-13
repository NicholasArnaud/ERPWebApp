$(function () {
    const selectors = {
        warehouse: $('#warehouse_select'),
        carrier: $('#carrier_select'),
        closedShipments: $('#closedShipments'),
        search: $('#btnSearch'),
        shipDate: $('#shipDate')
    };

    function getAjaxData(data) {
        data.carrierId = selectors.carrier.val();
        data.warehouseId = selectors.warehouse.val();
        data.shipDate = selectors.shipDate.val();
    }

    function getExportButtons() {
        return ["copy", "excel", "csv", "pdf", "print"].map(format => ({
            extend: format,
            titleAttr: format.charAt(0).toUpperCase() + format.slice(1),
            action: newExportAction,
            exportOptions: { columns: ":not(.notexport)" }
        }));
    }

    function newExportAction(e, dt, button, config) {
        const self = this;
        const oldStart = dt.settings()[0]._iDisplayStart;

        dt.one("preXhr", function (e, s, data) {
            data.start = 0;
            data.length = 2147483647;

            dt.one("preDraw", function () {
                const buttonClass = button[0].className;

                switch (true) {
                    case buttonClass.includes("buttons-copy"):
                        $.fn.dataTable.ext.buttons.copyHtml5.action.call(self, e, dt, button, config);
                        break;
                    case buttonClass.includes("buttons-excel"):
                        exportFile(self, e, dt, button, config, "excelHtml5", "excelFlash");
                        break;
                    case buttonClass.includes("buttons-csv"):
                        exportFile(self, e, dt, button, config, "csvHtml5", "csvFlash");
                        break;
                    case buttonClass.includes("buttons-pdf"):
                        exportFile(self, e, dt, button, config, "pdfHtml5", "pdfFlash");
                        break;
                    case buttonClass.includes("buttons-print"):
                        $.fn.dataTable.ext.buttons.print.action.call(self, e, dt, button, config);
                        break;
                    default:
                        console.warn("Unhandled export action.");
                }
                
                dt.one("preXhr", (e, s, data) => {
                    s._iDisplayStart = oldStart;
                    data.start = oldStart;
                });
                
                setTimeout(() => dt.ajax.reload(), 0);
            });
        });
        
        dt.ajax.reload();
    }

    function exportFile(self, e, dt, button, config, html5, flash) {
        const exportMethod = $.fn.dataTable.ext.buttons[html5].available(dt, config)
            ? html5
            : flash;

        $.fn.dataTable.ext.buttons[exportMethod].action.call(self, e, dt, button, config);
    }

    function getTableConfig() {
        return {
            processing: true,
            serverSide: true,
            fixedHeader: true,
            lengthMenu: [[10, 25, 100], [10, 25, 100]],
            filter: false,
            responsive: { details: false },
            keys: true,
            language: {
                decimal: ".",
                paginate: {
                    previous: `<i class='mdi mdi-chevron-left'></i>`,
                    next: `<i class='mdi mdi-chevron-right'></i>`
                }
            },
            drawCallback: () => $(".dataTables_paginate > .pagination").addClass("pagination-rounded"),
            ajax: {
                url: "ClosedShipments",
                type: "GET",
                datatype: "json",
                data: getAjaxData,
                error: function (jqXHR, textStatus, errorThrown) {
                    console.error("Error fetching data:", textStatus, errorThrown);
                } 
            },
            columns: [
                { title: "Carrier", data: "carrier", name: "Carrier", autoWidth: true },
                { title: "Warehouse", data: "warehouse", name: "Warehouse", autoWidth: true },
                { title: "Number of Shipments", data: "shipments", name: "ShipmentCount", autoWidth: true },
                { title: "Closed Date", data: "created_at", name: "ClosedDate", autoWidth: true },
                { title: "Ship Date", data: "ship_date", name: "ShipDate", autoWidth: true },
                {
                    title: "Manifest File",
                    data: "manifestFile",
                    name: "ManifestFile",
                    autoWidth: true,
                    render: (data) => `<a class="mdi mdi-24px mdi-download-box-outline" href="${data}"onclick="this.blur();"></a>`
                }
            ],
            buttons: [
                {
                    extend: "collection",
                    text: "Export",
                    className: "btn btn-dark",
                    buttons: getExportButtons()
                }
            ]
        };
    }

    function init() {
        let activeTab = new URLSearchParams(window.location.search).get("activeTab");

        if (activeTab === "closedshipments") {
            const purchaseOrderTable = selectors.closedShipments.DataTable(getTableConfig());
            selectors.search.on("click", () => purchaseOrderTable.ajax.reload());
        }
    }

    init();
});