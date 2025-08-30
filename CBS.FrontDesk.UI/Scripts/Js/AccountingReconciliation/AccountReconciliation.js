
$(document).ready(function () {
    const $payload = $("#payloadInput");
    const $editor = $("#commandEditor");
    $("#btn-load-sample").on("click", function () {
        var requestId = $("#requestId").val();
        
        $.ajax({
            url: "/AccountingReconciliation/ReconciliationData/" + requestId, // API endpoint
            type: "GET",  // Or "POST" if needed
            contentType: "application/json",
            success: function (response) {
                console.log("Fetched reconciliation entries:"+ response.data);

                // Example: loop through entries and log

                $payload.val(safeStringify(response.data));

                // 👉 TODO: Bind response to your UI (table, list, etc.)
            },
            error: function (xhr, status, error) {
                console.error("Error fetching reconciliation entries:", error);
                console.error("Status:", status);
                console.error("Response:", xhr.responseText);
            }
        });

    });
    $("#btn-parse-payload").on("click", function () {
        var requestId = $("#requestId").val();
        try {
            $.ajax({
                url: "/AccountingReconciliation/ReconciliationData/" + requestId, // API endpoint
                type: "GET",
                contentType: "application/json",
                dataType: "json", // ✅ ensures response is parsed
                success: function (response) {
                    console.log("Fetched reconciliation entry:", response);

                    // If response contains data wrapper
                    const entry = response.data || response;

                    // Display the raw payload in your hidden/textarea
                    $payload.val(safeStringify(response.data));

                    // Populate destination URL
                    $('#destinationUrl').val(response.data.DestinationUrl || '');

                    // Parse inner JSON object if exists
                    let inner = {};
                    if (response.data.CommandJsonObject) {
                        try {
                            inner = JSON.parse(response.data.CommandJsonObject);
                        } catch (err) {
                            console.error("Invalid CommandJsonObject:", err);
                        }
                    }

                    // Pretty-print JSON into editor
                    $editor.val(JSON.stringify(inner, null, 2));
                },
                error: function (xhr, status, error) {
                    console.error("Error fetching reconciliation entries:", error);
                    console.error("Status:", status);
                    console.error("Response:", xhr.responseText);
                }
            });
        } catch (e) {
            alert('Invalid payload JSON: ' + e.message);
        }
    });

    $("#btn-add-header").on("click", function () {
        $("#headersList").append(`<input type='text' class='form-control' placeholder='Header-Name'/><input type='text' class='form-control' placeholder='Header-Value'/><button class='btn btn-outline-danger btn-sm btn-remove-h'>Remove</button>`);
    });
    $(document).on('click', '.btn-remove-h', function () {
        const grid = document.getElementById('headersList');
        const idx = Array.from(grid.children).indexOf(this);
        if (idx >= 2) { grid.children[idx].remove(); grid.children[idx - 1].remove(); grid.children[idx - 2].remove(); }
    });
    $("#btn-send").on("click", async function () {
        const url = '/AccountingReconciliation/RepostingData'; // <-- direct action URL

        let innerObj;
        try {
            // Parse what’s in the editor just to validate it’s valid JSON
            innerObj = JSON.parse($editor.val() || '{}');
        } catch {
            alert('Invalid commandJsonObject');
            return;
        }

        // Convert the validated JSON object back into a string
        const jsonString = JSON.stringify(innerObj);

        // Prepare headers (optional if your action expects form data instead of JSON)
        const headers = { "Content-Type": "application/json" };

        // Log request for debugging
        $('#requestLog').text(
            safeStringify({
                method: 'POST',
                url,
                headers,
                body: jsonString
            })
        );

        const started = performance.now();

        try {
            // Call your MVC action with the JSON string as body
            const xhr = await $.ajax({
                url,
                method: 'POST',
                data: JSON.stringify({ objectAsString: jsonString }), // <-- matches your controller param
                headers,
                contentType: "application/json",
                timeout: 30000
            });

            // Handle success
            $('#respStatus').removeClass().addClass('badge bg-success').text('SUCCESS');
            $('#respTime').text(`${Math.round(performance.now() - started)} ms`);
            $('#responseBody').val(safeStringify(xhr));
        } catch (jqXHR) {
            // Handle error
            $('#respStatus').removeClass().addClass('badge bg-danger').text(`ERROR ${jqXHR.status || ''}`);
            $('#respTime').text(`${Math.round(performance.now() - started)} ms`);
            $('#responseBody').val(
                safeStringify({
                    status: jqXHR.status,
                    statusText: jqXHR.statusText,
                    responseText: jqXHR.responseText
                })
            );
        }
    });

    /*$("#btn-send").on("click", async function () {
        //const url = $('#destinationUrl').val().trim();
        //let innerObj;
        //try { innerObj = JSON.parse($editor.val() || '{}'); } catch { alert('Invalid commandJsonObject'); return; }
        //const headers = collectExtraHeaders();
        //if (!Object.keys(headers).some(h => h.toLowerCase() === 'content-type')) headers['Content-Type'] = 'application/json';
        //$('#requestLog').text(safeStringify({ method: 'POST', url, headers, body: innerObj }));
        //const started = performance.now();
        //try {
        //    const xhr = await $.ajax({ url, method: 'POST', data: JSON.stringify(innerObj), headers, contentType: headers['Content-Type'], timeout: 30000 });
        //    $('#respStatus').removeClass().addClass('badge bg-success').text('SUCCESS');
        //    $('#respTime').text(`${Math.round(performance.now() - started)} ms`);
        //    $('#responseBody').val(safeStringify(xhr));
        //} catch (jqXHR) {
        //    $('#respStatus').removeClass().addClass('badge bg-danger').text(`ERROR ${jqXHR.status || ''}`);
        //    $('#respTime').text(`${Math.round(performance.now() - started)} ms`);
        //    $('#responseBody').val(safeStringify({ status: jqXHR.status, statusText: jqXHR.statusText, responseText: jqXHR.responseText }));
        //}
    });*/
});
function safeStringify(obj) {
    try { return JSON.stringify(obj, null, 2); } catch { return String(obj); }
}
function collectExtraHeaders() {
    const headers = {};
    const rows = Array.from(document.querySelectorAll('#headersList > *'));
    for (let i = 0; i < rows.length; i += 3) {
        const name = rows[i]?.value?.trim();
        const value = rows[i + 1]?.value;
        if (name) headers[name] = value;
    }
    return headers;
}