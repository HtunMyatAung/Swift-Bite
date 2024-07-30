document.getElementById('UserNameVisible').addEventListener('input', function () {
    const nameError = document.getElementById('nameError');
    const visibleName = this.value;
    const hiddenName = document.getElementById('UserName');
    hiddenName.value = visibleName.replace(/ /g, '_');
    // Validate Full Name
    if (/[^a-zA-Z\s]/.test(visibleName)) {
        nameError.textContent = 'Full Name cannot include special characters and numbers.';
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

document.getElementById('profileForm').addEventListener('submit', function (event) {
    let valid = true;

    // Validate Full Name
    const visibleName = document.getElementById('UserNameVisible').value;
    const hiddenName = document.getElementById('UserName');
    hiddenName.value = visibleName.replace(/ /g, '_');
    const nameError = document.getElementById('nameError');
    if (/[^a-zA-Z\s]/.test(visibleName)) {
        nameError.textContent = 'Full Name cannot include special characters.';
        valid = false;
    }

    // Validate Phone Number
    const phone = document.getElementById('UserPhone').value;
    const phoneError = document.getElementById('phoneError');
    if (!/^\d{11}$/.test(phone)) {
        phoneError.textContent = 'Phone Number must be exactly 11 digits and contain only numbers.';
        valid = false;
    }

    // Validate Email
    const email = document.getElementById('UserEmail').value;
    const emailError = document.getElementById('emailError');
    const gmailPattern = /^[a-zA-Z0-9._%+-]+@gmail\.com$/;
    if (!gmailPattern.test(email)) {
        emailError.textContent = 'Email must be a valid Gmail address.';
        valid = false;
    }

    // If not valid, prevent form submission
    if (!valid) {
        event.preventDefault();
    }
});
