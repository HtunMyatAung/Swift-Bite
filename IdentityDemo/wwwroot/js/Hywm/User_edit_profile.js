
document.getElementById('profileForm').addEventListener('submit', function (event) {
    let valid = true;

    // Validate Full Name
    const name = document.getElementById('UserName').value;
    const nameError = document.getElementById('nameError');
    if (/[^a-zA-Z\s]/.test(name)) {
        nameError.textContent = 'Full Name cannot include special characters.';
        valid = false;
    } else {
        nameError.textContent = '';
    }

    // Validate Phone Number
    const phone = document.getElementById('UserPhone').value;
    const phoneError = document.getElementById('phoneError');
    if (!/^\d{11}$/.test(phone)) {
        phoneError.textContent = 'Phone Number must be exactly 11 digits and contain only numbers.';
        valid = false;
    } else {
        phoneError.textContent = '';
    }

    // Validate Email
    const email = document.getElementById('UserEmail').value;
    const emailError = document.getElementById('emailError');
    const gmailPattern = /^[a-zA-Z0-9._%+-]+@gmail\.com$/;
    if (!gmailPattern.test(email)) {
        emailError.textContent = 'Email must be a valid Gmail address.';
        valid = false;
    } else {
        emailError.textContent = '';
    }

    // If not valid, prevent form submission
    if (!valid) {
        event.preventDefault();
    }
});

// Event listeners to clear error messages on input
document.getElementById('UserName').addEventListener('input', function () {
    const nameError = document.getElementById('nameError');
    if (/[^a-zA-Z\s]/.test(this.value)) {
        nameError.textContent = 'Full Name cannot include special characters.';
    } else {
        nameError.textContent = '';
    }
});

document.getElementById('UserPhone').addEventListener('input', function () {
    const phoneError = document.getElementById('phoneError');
    if (!/^\d{11}$/.test(this.value)) {
        phoneError.textContent = 'Phone Number must be exactly 11 digits and contain only numbers.';
    } else {
        phoneError.textContent = '';
    }
});

document.getElementById('UserEmail').addEventListener('input', function () {
    const emailError = document.getElementById('emailError');
    const gmailPattern = /^[a-zA-Z0-9._%+-]+@gmail\.com$/;
    if (!gmailPattern.test(this.value)) {
        emailError.textContent = 'Email must be a valid Gmail address.';
    } else {
        emailError.textContent = '';
    }
});

function previewImage(event) {
    var reader = new FileReader();
    reader.onload = function () {
        var output = document.getElementById('blah');
        output.src = reader.result;
    }
    reader.readAsDataURL(event.target.files[0]);
}
