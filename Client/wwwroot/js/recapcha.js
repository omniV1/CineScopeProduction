// Helper functions for reCAPTCHA
window.recaptchaFunctions = {
    getResponse: function () {
        if (typeof grecaptcha !== 'undefined' && grecaptcha.getResponse) {
            return grecaptcha.getResponse();
        }
        return '';
    },

    reset: function () {
        if (typeof grecaptcha !== 'undefined' && grecaptcha.reset) {
            grecaptcha.reset();
        }
    }
};