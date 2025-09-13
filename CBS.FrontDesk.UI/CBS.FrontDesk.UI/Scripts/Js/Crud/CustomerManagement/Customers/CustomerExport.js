(function () {
    // Build full filter object including nested Options from DataTables params
    function buildFilterFromDataTables(d) {
        return {
            BranchId: $("#branchId").val() || "",
            MembershipApprovalStatus: $("#membershipApprovalStatus").val() || "",
            Gender: $("#gender").val() || "",
            MaritalStatus: $("#maritalStatus").val() || "",
            WorkingStatus: $("#workingStatus").val() || "",
            CustomerType: $("#customerType").val() || "",
            AgeCategoryStatus: $("#ageCategoryStatus").val() || "",
            LegalForm: $("#legalForm").val() || "",
            CustomerId: $("#customerId").val() || "",
            FirstName: $("#firstName").val() || "",
            LastName: $("#lastName").val() || "",
            DateOfBirthFrom: $("#dobFrom").val() || "",
            DateOfBirthTo: $("#dobTo").val() || "",
            CreatedFrom: $("#createdFrom").val() || "",
            CreatedTo: $("#createdTo").val() || "",
            SearchTerm: $("#searchTerm").val() || "",
            Options: {
                start: d.start || 0,
                draw: d.draw ? String(d.draw) : "1",
                length: d.length || 10,
                sortColumnName: d.columns?.[d.order?.[0]?.column || 0]?.data || "CreatedDate",
                sortColumnDirection: d.order?.[0]?.dir || "desc",
                searchValue: d.search?.value || "",
                pageSize: d.length || 10,
                skip: d.start || 0,
                recordsTotal: 0,
                recordsFiltered: 0,
                search: d.search?.value || "",
                sortDirection: d.order?.[0]?.dir || "desc"
            }
        };
    }

    // Build filter for export using form values only (no DataTables paging unless you want it)
    function buildFilterForExport() {
        return {
            BranchId: $("#branchId").val() || "",
            MembershipApprovalStatus: $("#membershipApprovalStatus").val() || "",
            Gender: $("#gender").val() || "",
            MaritalStatus: $("#maritalStatus").val() || "",
            WorkingStatus: $("#workingStatus").val() || "",
            CustomerType: $("#customerType").val() || "",
            AgeCategoryStatus: $("#ageCategoryStatus").val() || "",
            LegalForm: $("#legalForm").val() || "",
            CustomerId: $("#customerId").val() || "",
            FirstName: $("#firstName").val() || "",
            LastName: $("#lastName").val() || "",
            DateOfBirthFrom: $("#dobFrom").val() || "",
            DateOfBirthTo: $("#dobTo").val() || "",
            CreatedFrom: $("#createdFrom").val() || "",
            CreatedTo: $("#createdTo").val() || "",
            SearchTerm: $("#searchTerm").val() || "",
            Options: {
                start: 0,
                draw: "export",
                length: 100000,
                sortColumnName: "CreatedDate",
                sortColumnDirection: "desc",
                searchValue: $("#searchTerm").val() || "",
                pageSize: 100000,
                skip: 0,
                recordsTotal: 0,
                recordsFiltered: 0,
                search: $("#searchTerm").val() || "",
                sortDirection: "desc"
            }
        };
    }

    // Convert nested object to query string with names like Options.start=...
    function toQueryString(obj, prefix) {
        const arr = [];
        for (const k in obj) {
            if (!Object.prototype.hasOwnProperty.call(obj, k)) continue;
            const v = obj[k];
            const key = prefix ? prefix + "." + k : k;
            if (v !== null && typeof v === "object" && !(v instanceof Date)) {
                arr.push(toQueryString(v, key));
            } else if (v !== undefined && v !== null) {
                arr.push(encodeURIComponent(key) + "=" + encodeURIComponent(v));
            }
        }
        return arr.join("&");
    }

    // ------- Lazy DataTable init (no load on page load) -------
    let table = null;

    function initTableOnce() {
        if (table || !$.fn.DataTable) return;

        table = $("#myDataTable").DataTable({
            serverSide: true,
            processing: true,
            searching: false,
            destroy: true,
            deferLoading: 0,                 // 🔸 prevents initial ajax call
            order: [[0, "desc"]],
            ajax: {
                url: "/Individual/LoadMembers",
                type: "POST",
                contentType: "application/json",
                data: function (d) {
                    const payload = buildFilterFromDataTables(d);
                    return JSON.stringify(payload);
                }
            },
            columns: [
                {
                    data: "CreatedDate",
                    render: function (data) {
                        return data ? moment(data).format("DD/MM/YYYY HH:mm") : "";
                    }
                },
                {
                    data: null,
                    render: function (data) {
                        return `${data.FirstName || ""} ${data.LastName || ""}`.trim();
                    }
                },
                { data: "CustomerId" },
                { data: "Telephone" },
                { data: "CustomerType" },
                {
                    data: null,
                    orderable: false,
                    render: function (data) {
                        return `
                            <button class="btn btn-sm btn-primary" onclick="showMemberDetails('${data.CustomerId}')">
                                <i class="mdi mdi-eye"></i> Details
                            </button>
                        `;
                    }
                }
            ]
        });
    }

    // Apply filter: initialize (if needed) then trigger first load / reload
    $("#applyFilterBtn").on("click", function (e) {
        e.preventDefault();
        initTableOnce();             // create table but still no call until reload/draw
        table.ajax.reload();         // 🔸 first actual server call happens here
    });

    // Reset: clear fields and re-arm the table to idle (no data, no call)
    $("#resetFilterBtn").on("click", function () {
        $("#memberFilterForm")[0].reset();
        $("#branchId").val("").trigger?.("change");

        if (table) {
            table.destroy();                     // tear down
            table = null;
            $("#myDataTable tbody").empty();     // clean slate
        }
        // do NOT auto-load anything
    });

    // Export to Excel
    $("#exportBtn").on("click", function () {
        const $btn = $(this);
        $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Preparing...');

        try {
            const filter = buildFilterForExport();
            const qs = toQueryString(filter);
            const url = "/Individual/DownloadMembersData?" + qs;
            window.location.href = url;  // browser download
        } finally {
            setTimeout(() => {
                $btn.prop("disabled", false).html('<i class="mdi mdi-file-excel"></i> Export to Excel');
            }, 2000);
        }
    });

    if (!$.fn.DataTable) {
        console.warn("DataTables not found. Ensure #myDataTable is present and plugin is loaded.");
    }
})();
