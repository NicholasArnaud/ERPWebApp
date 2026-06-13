$(document).ready(function () {
	var switchStatus = false;
	var siteFilter = "";

	var locationTable = $("#location-datatable").DataTable({
		"processing": true,
		"serverSide": true,
		"dom": "<'row'<'col-md-6 col-sm-6 col-xs-12'l><'col-md-6 col-sm-6 col-xs-12 custom-mt-xs'f>>"
			+ "<'row'<'col-sm-12'tr>>" + "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
		"ajax": {
			"url": "GetLocations",
			"type": "POST",
			"datatype": "json",
			"data": function (d) {
				d.showInactive = switchStatus;
				d.siteFilter = siteFilter;
			},
		},
		"columns": [  
			{ "data": "locationId", "name": "LocationId", "autoWidth": true },
			{ "data": "siteName", "name": "SiteName", "autoWidth": true },
			{ "data": "locationName", "name": "LocationName", "autoWidth": true },
			{ "data": "locationDescription", "name": "LocationDescription", "autoWidth": true },
			{ "data": "locationType", "name": "Type", "autoWidth": true },
			{
				"data": "isActive",
				"render": function (IsActive, type, full, meta) {
					if (IsActive) {
						return "Yes";
					} else {
						return "No";
					}
				},
				"name": "IsActive",
				"autoWidth": true
			},
			{
				"data": "isExternal",
				"render": function (IsExternal, type, full, meta) {
					if (IsExternal) {
						return "Yes";
					} else {
						return "No";
					}
				},
				"name": "IsExternal",
				"autoWidth": true
			},
			
			{
				title: "Actions",
				"data": "permission",
				"render": function (data, type, row) {
					if (data === "yes") {
						let linkBarcode = "<a  class=\"mdi mdi-24px mdi-barcode\" href=/Locations/DownloadBarcode/" + row.locationId + "></a>";
						let linkDetails = "<a  class=\"mdi mdi-24px mdi-book-information-variant\" href=/Locations/Details/" + row.locationId + "></a>";
						let linkEdit = "<a  class=\"mdi mdi-24px mdi-pencil\" href=/Locations/Edit/" + row.locationId + "></a>";
						let linkDelete = "<a  class=\"mdi mdi-24px mdi-delete\" href=/Locations/Delete/" + row.locationId + "></a>";
						return linkBarcode + " " + linkDetails + " " + linkEdit + " " + linkDelete;
					} else {
						let linkBarcode = "<a  class=\"mdi mdi-24px mdi-book-information-variant\" href=/Locations/GenerateBarcode/" + row.locationId + "></a>";
						let linkDetails = "<a  class=\"mdi mdi-24px mdi-book-information-variant\" href=/Locations/Details/" + row.locationId + "></a>";
						return linkBarcode + " " + linkDetails;
					}
				}
			},
		],
		"columnDefs": [
			{
				"targets": [0],
				"visible": false,
				"searchable": false
			},
		],
		pageLength: 100,
		"lengthMenu": [[50, 100, 200, 300, 400, 500], [50, 100, 200, 300, 400, 500]],
		//preset visuals for the table
		keys: !0,
		language: {
			paginate: {
				previous: "<i class='mdi mdi-chevron-left'>",
				next: "<i class='mdi mdi-chevron-right'>"
			},

		},
		drawCallback: function () {
			$(".dataTables_paginate > .pagination").addClass("pagination-rounded");
		},
	});

	$("#inactive-checkbox").change(function () {
		switchStatus = $(this).is(":checked");
		locationTable.ajax.reload();
	});
	
	$("#site-select-id").change(function () {
		siteFilter = $(this).val();
		locationTable.ajax.reload();
	});

});
