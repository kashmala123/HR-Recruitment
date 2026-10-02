namespace HRRecruitment.wwwroot.js
{
    public class contact_validation
    {
        $(document).ready(function() {
            $('#contactForm').on('submit', function (e) {
                e.preventDefault();

                // Clear previous errors
                $('.error-message').html('');

                let isValid = true;

                const name = $('#name').val().trim();
                const email = $('#email').val().trim();
                const subject = $('#subject').val().trim();
                const message = $('#message').val().trim();

                if (name === '') {
                    $('#nameError').html('Name is required');
                    isValid = false;
                }
                if (email === '') {
                    $('#emailError').html('Email is required');
                    isValid = false;
                } else if (!isValidEmail(email)) {
                    $('#emailError').html('Please enter a valid email address');
                    isValid = false;
                }
                if (subject === '') {
                    $('#subjectError').html('Subject is required');
                    isValid = false;
                }
                if (message === '') {
                    $('#messageError').html('Message is required');
                    isValid = false;
                }

                if (isValid) {
                    // Here you would normally send the data to the server
                    alert('Form submitted successfully!');
                    // Uncomment to actually submit:
                    // this.submit();
                }
            });

            function isValidEmail(email) {
                const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                return re.test(email);
            }
        });
    }
}
