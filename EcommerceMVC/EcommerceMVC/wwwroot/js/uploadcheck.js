

function ValidateInput() {
    if (document.getElementById("UploadBox").value == "") {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: 'Please upload an image!',
        });
        return false
    }
    return true
}
