$(document).ready(function () {
    $('#Role').change(function () {
        var selection = $('#Role Option:Selected').text();
        if (selection != 'Company') {
            $('#CompanyId').hide();
        }
        else {
            $('#CompanyId').show();
        }
    })
})