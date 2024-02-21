$(document).ready(function () {
 
    let row = 2;
    let col = 1;
    $(".add-entry").click(function (event) {
        row++; col++;
        event.preventDefault();
        $("#show-entry").prepend(
 
            '<div class="row">' +
            '<div class="mb-3 col-lg-6 col-xl-3 col-12 mb-0">' +
            '<div class="form-floating form-floating-outline">' +
            '<input type="text" id="form-repeater-'+row+'-'+col+'" class="form-control"  >' +
            '<label for="form-repeater' + row + '-' + col +'">Username</label>' +
            '</div>' +
            '</div>' +
            '<div class="mb-3 col-lg-6 col-xl-3 col-12 mb-0">' +
            '<div class="form-floating form-floating-outline">' +
        '<input type="text" id="form-repeater-'+row+'-'+col+'" class="form-control">' +
            '<label for="form-repeater' + row + '-' + col +'">Password</label>' +
            '</div>' +
            '</div>' +
            '<div class="mb-3 col-lg-6 col-xl-2 col-12 mb-0">' +
            '<div class="form-floating form-floating-outline">' +
            '<select id="form-repeater' + row + '-' + col +'" class="form-control select2">' +
            '<option value="DEBIT">DEBIT</option>' +
            '<option value="CREDIT">CREDIT</option>' +
            '</select>' +
            '<label for="entryType-' + row + '-' + col +'">EntryType</label>' +
            '</div>' +
            '</div>' +
            '<div class="mb-3 col-lg-6 col-xl-2 col-12 mb-0">' +
            '<div class="form-floating form-floating-outline">' +
            '<select id="form-repeater-' + row + '-' + col +'" class="form-control select2">' +
            '<option value="Designer">Designer</option>' +
            '<option value="Developer">Developer</option>' +
            '<option value="Tester">Tester</option>' +
            '<option value="Manager">Manager</option>' +
            '</select>' +
            '<label for="form-repeater-' + row + '-' + col +'">Profession</label>' +
            '</div>' +
            '</div>' +
            '<div class="mb-3 col-lg-12 col-xl-2 col-12 d-flex align-items-center mb-0">' +
            '<button class="btn btn-danger remove-entry">' +
            '<span class="align-middle">Remove Entry</span><i class="mdi mdi-minus-box-multiple"></i>' +
            '</button>' +
            '</div>' +
    
            '</div>'
        );
        $(document).on("click", ".remove-entry", function (event) {
            event.preventDefault();
            let row_item = $(this).parent().parent();
            row_item.remove();

            $(this).slideDown();
        });
       
    
    });
    $("#form-multple-manual-entries").submit(function (event) {
        event.preventDefault();
        console.log('Posting of entries in progress ...');
        $("#post-entries").val('Posting of entries in progress ...');
        $.ajax({
            URL: '/BankingOperations/CreateCashInfusion',
            type: 'POST',
            data: $(this).serialize(),
            success: function (data) {
                $("#post-entries").val('add');
              
            },

        });
    });
});  // ajax request to insert all form data to the server
    