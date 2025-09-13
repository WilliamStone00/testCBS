$(document).ready(function () {
    $("#applyTranslationFilterBtn").on("click", function (e) {
        e.preventDefault();
        loadUntranslatedMenus();
    });

    $("#resetTranslationFilterBtn").on("click", function () {
        $("#languageSelector").val("en");
        $("#updateTranslationTableBody").empty();

        if ($.fn.DataTable.isDataTable("#myDataTable")) {
            $('#myDataTable').DataTable().clear().destroy();
        }
    });
});
function loadUntranslatedMenus() {
    const lang = $("#languageSelector").val();
    if (!lang) return;

    $.ajax({
        url: `/MenuTranslation/UpdateMenuTranslation?lang=${lang}`,
        method: "GET",
        success: function (res) {
            console.log("Response from server:", res);

            if (Array.isArray(res) && res.length > 0) {
                renderTranslationTable(res, lang);
            } else {
                $("#updateTranslationTableBody").html(`
                        <tr>
                            <td colspan="7" class="text-center text-muted">
                                No untranslated menus found for "${lang}".
                            </td>
                        </tr>
                    `);

                if ($.fn.DataTable.isDataTable("#myDataTable")) {
                    $('#myDataTable').DataTable().clear().destroy();
                }
            }
        },
        error: function (err) {
            console.error("Error fetching untranslated menus", err);
            alert("Something went wrong while loading untranslated menus.");
        }
    });
}

function renderTranslationUpdateTable(translations) {
    const $table = $('#myDataTable');

    if ($.fn.DataTable.isDataTable($table)) {
        $table.DataTable().clear().destroy();
    }

    let rows = '';

    translations.forEach((item, index) => {
        rows += `
            <tr>
                <td>${index + 1}</td>
                <td>
                    ${item.menuText}
                    <input type="hidden" name="Id" value="${item.id}" />
                </td>
                <td>${item.originalDescription || ''}</td>
                <td>
                    <textarea class="form-control shadow wide-textarea"
                        name="TranslatedText"
                        rows="4"
                        required>${item.translatedText || ''}</textarea>
                </td>
                <td>
                    <textarea class="form-control shadow wide-textarea"
                        name="Description"
                        rows="4">${item.description || ''}</textarea>
                </td>
            </tr>
        `;
    });

    $('#updateTranslationTableBody').html(rows);

    $table.DataTable({
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        ordering: false,
        searching: true,
        info: true,
        language: {
            emptyTable: "No translated menus to update.",
            lengthMenu: "Show _MENU_ entries",
            info: "Showing _START_ to _END_ of _TOTAL_ entries"
        }
    });
}

function submitUpdatedMenuTranslations() {
    let updates = [];

    $('#updateTranslationTableBody tr').each(function () {
        const id = $(this).find('input[name="Id"]').val();
        const translatedText = $(this).find('textarea[name="TranslatedText"]').val().trim();
        const tooltip = $(this).find('textarea[name="Tooltip"]').val().trim();
        const description = $(this).find('textarea[name="Description"]').val().trim();

        if (translatedText !== '') {
            updates.push({
                id: id,
                translatedText: translatedText,
                tooltip: tooltip,
                description: description
            });
        }
    });

    if (updates.length === 0) {
        alertify.alert("No Updates", "Please fill at least one translated text before submitting.");
        return;
    }

    alertify.confirm(
        "Confirm Update",
        `You are about to update <strong>${updates.length}</strong> translation(s). Proceed?`,
        function () {
            $.ajax({
                url: '/MenuTranslation/UpdateTranslations',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(updates),
                success: function (res) {
                    if (res.success) {
                        alertify.success(res.message);
                        setTimeout(() => location.reload(), 1200);
                    } else {
                        alertify.error(res.message || "An error occurred during update.");
                    }
                },
                error: function (err) {
                    console.error("Update error:", err);
                    alertify.error("An error occurred while updating translations.");
                }
            });
        },
        function () {
            alertify.message("Update cancelled.");
        }
    );
}
