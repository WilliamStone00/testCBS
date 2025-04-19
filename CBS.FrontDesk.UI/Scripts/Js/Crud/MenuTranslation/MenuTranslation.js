$(document).ready(function () {
    $("#applyTranslationFilterBtn").on("click", function (e) {
        e.preventDefault();
        loadUntranslatedMenus();
    });

    $("#resetTranslationFilterBtn").on("click", function () {
        $("#languageSelector").val("en");
        $("#translationTableBody").empty();

        if ($.fn.DataTable.isDataTable("#myDataTable")) {
            $('#myDataTable').DataTable().clear().destroy();
        }
    });
});
function loadUntranslatedMenus() {
    const lang = $("#languageSelector").val();
    if (!lang) return;

    $.ajax({
        url: `/MenuTranslation/Untranslated?lang=${lang}`,
        method: "GET",
        success: function (res) {
            console.log("Response from server:", res);

            if (Array.isArray(res) && res.length > 0) {
                renderTranslationTable(res, lang);
            } else {
                $("#translationTableBody").html(`
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

function renderTranslationTable(menus, lang) {
    const $table = $('#myDataTable');

    // ✅ Destroy DataTable before modifying DOM
    if ($.fn.DataTable.isDataTable($table)) {
        $table.DataTable().clear().destroy();
    }

    // ✅ Rebuild only the tbody
    let rows = "";

    menus.forEach((menu, index) => {
        rows += `
            <tr>
                <td>${index + 1}</td>
                <td>
                    ${menu.MenuText}
                    <input type="hidden" name="MenuMasterId" value="${menu.Id}" />
                </td>
                <td>${menu.Tooltip}</td>
                <td>${menu.Description}</td>
                <td>
                    <textarea class="form-control shadow wide-textarea"
                              name="TranslatedText"
                              placeholder="Enter menu translation to ${lang}"
                              data-menutext="${menu.MenuText}"
                              rows="4" required></textarea>
                </td>
                <td>
                    <textarea class="form-control shadow wide-textarea"
                              name="Tooltip"
                              placeholder="Enter tooltip translation to ${lang}"
                              rows="4"></textarea>
                </td>
                <td>
                    <textarea class="form-control shadow wide-textarea"
                              name="Description"
                              placeholder="Enter description translation to ${lang}"
                              rows="4"></textarea>
                </td>
            </tr>`;
    });

    // ✅ Inject rows into the existing tbody
    $('#translationTableBody').html(rows);

    // ✅ Reinitialize DataTable after DOM is updated
    $table.DataTable({
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100, 250, 500],
        ordering: false,
        searching: true,
        info: true,
        language: {
            emptyTable: "No untranslated menus found.",
            lengthMenu: "Show _MENU_ entries",
            info: "Showing _START_ to _END_ of _TOTAL_ entries"
        }
    });
}

function submitMenuTranslations() {
    const lang = $("#languageSelector").val();
    if (!lang) {
        appalert("Language Required", "Please select a language.", 2, 1);
        return;
    }

    // ✅ Check if table has any rows
    if ($("#translationTableBody tr").length === 0) {
        appalert("Empty Table", "There are no menus to translate for the selected language.", 2, 1);
        return;
    }

    let translations = [];
    let hasInvalid = false;

    $("#translationTableBody tr").each(function () {
        const translatedText = $(this).find('textarea[name="TranslatedText"]').val().trim();
        const menuMasterId = $(this).find('input[name="MenuMasterId"]').val();

        const tooltip = $(this).find('textarea[name="Tooltip"]').val().trim();
        const description = $(this).find('textarea[name="Description"]').val().trim();

        if (translatedText !== "") {
            translations.push({
                menuMasterId: parseInt(menuMasterId),
                languageCode: lang,
                translatedText: translatedText,
                tooltip: tooltip,
                description: description
            });
        } else if (tooltip !== "" || description !== "") {
            hasInvalid = true;
        }
    });

    if (hasInvalid) {
        appalert("Validation Error", "You cannot fill tooltip or description without providing the translated menu text.", 2, 1);
        return;
    }

    const entryCount = translations.length;

    if (entryCount === 0) {
        appalert("No Entries", "Please enter at least one translated menu text before submitting.", 2, 1);
        return;
    }

    // ✅ All validations passed — show confirmation
    alertify.confirm(
        "Submit Translations",
        `You are about to submit <strong>${entryCount}</strong> translation${entryCount > 1 ? "s" : ""} to <strong>${lang.toUpperCase()}</strong>. Do you want to continue?`,
        function () {
            $.ajax({
                url: '/MenuTranslation/SubmitTranslations',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(translations),
                success: function (res) {
                    if (res.success) {
                        appalert(res.message, 1, 1);
                        setTimeout(() => {
                            loadUntranslatedMenus();
                        }, 1500);
                    } else {
                        appalert(res.message || "An error occurred while saving.", 3, 1);
                        
                    }
                },
                error: function (err) {
                    console.error("Submission error:", err);
                    appalert("An error occurred while submitting translations.", 3, 1);
                }
            });
        },
        function () {
            appalert("Submission cancelled.", 2, 1);
        }
    );
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
