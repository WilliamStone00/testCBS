const ConnectedUsers = {
    selectors: {
        tableId: "#myDataTable",
        statsId: "#connectedUserStats",
        filterSelectors: {
            UserName: '#userName',
            FirstName: '#firstName',
            LastName: '#lastName',
            BranchId: '#branchInput',
            Role: '#role',
            SessionCode: '#sessionCode',
            IsExpired: '#isExpired',
            StartDate: '#createdFrom',
            EndDate: '#createdTo'
        }
    },

    getFilterData: function () {
        const s = this.selectors.filterSelectors;
        return {
            UserName: $(s.UserName).val() || '',
            FullName: `${$(s.FirstName).val() || ''} ${$(s.LastName).val() || ''}`.trim(),
            BranchId: $(s.BranchId).val() || '',
            Role: $(s.Role).val() || '',
            SessionCode: $(s.SessionCode).val() || '',
            IsExpired: $(s.IsExpired).val() || '',
            StartDate: $(s.StartDate).val() || null,
            EndDate: $(s.EndDate).val() || null
        };
    },

    initDataTable: function () {
        this.table = $(this.selectors.tableId).DataTable({
            serverSide: true,
            destroy: true,
            processing: false, // ✅ Suppress loader
            searching: false,
            responsive: true,
            order: [[5, "desc"]],
            ajax: {
                url: '/UserManagement/LoadUserSessions',
                type: 'POST',
                contentType: 'application/json',
                data: d => {
                    const filters = ConnectedUsers.getFilterData();
                    filters.DataTableOptions = {
                        draw: d.draw,
                        start: d.start,
                        length: d.length,
                        skip: d.start,
                        pageSize: d.length,
                        sortColumnName: d.columns[d.order[0].column].data,
                        sortColumnDirection: d.order[0].dir
                    };
                    return JSON.stringify(filters);
                }
            },
            columns: [
                { data: 'SessionCode' },
                { data: 'FullName' },
                { data: 'Role' },
                { data: 'BranchName' },
                {
                    data: 'CreatedDate',
                    render: d => d ? moment(d).format('YYYY-MM-DD HH:mm') : 'N/A'
                },
                {
                    data: 'IsExpired',
                    render: d => d
                        ? '<span class="badge bg-danger">Expired</span>'
                        : '<span class="badge bg-success">Active</span>'
                },
                {
                    data: 'UserId',
                    orderable: false,
                    render: (data, type, row) => {
                        return `
                        <button class="btn btn-sm btn-danger" onclick="ConnectedUsers.terminateSession('${row.UserId}')">
                            <i class="mdi mdi-close-circle-outline"></i> End Session
                        </button>`;
                    }
                }

            ],
            language: {
                emptyTable: "No connected users found.",
                processing: "" // ✅ Hide loading spinner text
            }
        });
    },

    startCountdown: function (elementId, expirationTime) {
        const update = () => {
            const el = document.getElementById(elementId);
            if (!el) return;

            const now = moment();
            const exp = moment(expirationTime).add(1, 'hour'); // ⏱️ +1h
            const diff = exp.diff(now, 'seconds');

            if (diff <= 0) {
                el.innerText = "Expired";
                el.classList.remove("text-primary");
                el.classList.add("text-danger", "fw-bold");
                clearInterval(timer);
            } else {
                const mins = Math.floor(diff / 60);
                const secs = diff % 60;
                el.innerText = `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
                if (diff <= 300) el.classList.add("text-warning", "fw-bold");
            }
        };
        update();
        const timer = setInterval(update, 1000);
    },

    terminateSession: function (userid) {
        alertify.confirm(
            'Terminate Session',
            'Do you want to terminate or end the selected user session?',
            function () {
                // ✅ User clicked OK
                $.post('/UserManagement/TerminateSession', { userid }, function (response) {
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        ConnectedUsers.table.ajax.reload(null, false);

                    } else {
                        appalert(response.message, 3, 1);
                    }
                }).fail(function () {
                    appalert("❌ An error occurred while processing the request.", 3, 1);
                });
            },
            function () {
                // ❌ User clicked Cancel
                alertify.message();
                appalert("Termination cancelled.", 3, 1);
            }
        ).set('labels', { ok: 'Yes, Terminate', cancel: 'Cancel' });
    },


    init: function () {
        this.initDataTable();
        //this.bindUI();

        //// ⏱ Refresh stats every 30s
        //setInterval(() => ConnectedUsers.refreshStats(), 30000);

        // ⏱ Refresh table every 10s
        setInterval(() => {
            if (ConnectedUsers.table) {
                const table = ConnectedUsers.table;
                const pageInfo = table.page.info();

                $.ajax({
                    url: '/UserManagement/LoadUserSessions',
                    type: 'POST',
                    global: false, // 👈 disables triggering #loading or global spinner
                    contentType: 'application/json',
                    data: JSON.stringify({
                        ...ConnectedUsers.getFilterData(),
                        DataTableOptions: {
                            draw: 1,
                            start: pageInfo.start,
                            length: pageInfo.length,
                            skip: pageInfo.start,
                            pageSize: pageInfo.length,
                            sortColumnName: table.settings()[0].aoColumns[table.order()[0][0]].data,
                            sortColumnDirection: table.order()[0][1]
                        }
                    }),
                    beforeSend: function (xhr) {
                        // Optional: prevent global loader if you use a header-based bypass
                        xhr.setRequestHeader("X-Bypass-Loader", "true");
                    },
                    success: function (response) {
                        table.clear().rows.add(response.data).draw(false); // 🔇 Silent redraw without loader
                    },
                    error: function () {
                        console.warn("❌ Silent reload failed");
                    }
                });
            }
        }, 30000);

    }
};


$(document).ready(function () {
    // Hide filters on load
    $('#byBranch, #byUser, #byDate').prop('checked', false);
    $('#branchFilterSection, #userFilterSection, #dateRangeSection').hide();

    $('#byBranch, #byUser, #byDate').on('change', function () {
        $('#branchFilterSection').toggle($('#byBranch').is(':checked'));
        $('#userFilterSection').toggle($('#byUser').is(':checked'));
        $('#dateRangeSection').toggle($('#byDate').is(':checked'));
    });

    // Init dashboard
    ConnectedUsers.init();

    // ⏱ Session stats for header KPIs
    async function fetchSessionStats() {
        try {
            const response = await fetch('/UserManagement/GetLiveSessionDashboard');
            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
            const data = await response.json();
            if (!Array.isArray(data)) throw new Error("Invalid data format returned");

            const totalSessions = data.length;
            const activeSessions = data.filter(s => s.SessionStatus === "Active").length;
            const expiredSessions = data.filter(s => s.SessionStatus === "Expired").length;
            const uniqueUsers = new Set(data.map(s => s.UserId)).size;
            const uniqueBranches = new Set(data.map(s => s.BranchName)).size;

            const update = (id, value) => {
                const el = document.getElementById(id);
                if (el) el.textContent = value;
            };

            update("totalSessions", totalSessions);
            update("activeSessions", activeSessions);
            update("expiredSessions", expiredSessions);
            update("uniqueUsers", uniqueUsers);
            update("uniqueBranches", `From ${uniqueBranches} branch(es)`);

            document.getElementById("dashboardError")?.classList.add("d-none");

        } catch (error) {
            const errorBox = document.getElementById("dashboardError");
            if (errorBox) {
                errorBox.classList.remove("d-none");
                errorBox.textContent = `⚠️ Could not load dashboard: ${error.message}`;
            }
        }
    }

    fetchSessionStats();
    setInterval(fetchSessionStats, 30000);
});
