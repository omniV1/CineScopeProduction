window.recaptchaFunctions = {
    getResponse: function () {
        try {
            return grecaptcha.getResponse();
        } catch (error) {
            console.error('Error getting reCAPTCHA response:', error);
            return '';
        }
    },

    reset: function () {
        try {
            grecaptcha.reset();
        } catch (error) {
            console.error('Error resetting reCAPTCHA:', error);
        }
    },

    render: function (elementId) {
        try {
            grecaptcha.ready(function () {
                grecaptcha.render(elementId, {
                    'sitekey': '6LezxwUrAAAAAAc9vWRlGmqOJ99Yn15gF5HZdG5f',
                    'theme': 'dark',
                    'callback': function (response) {
                        console.log('reCAPTCHA verified');
                    },
                    'expired-callback': function () {
                        console.log('reCAPTCHA expired');
                    },
                    'error-callback': function () {
                        console.error('reCAPTCHA error');
                    }
                });
            });
        } catch (error) {
            console.error('Error rendering reCAPTCHA:', error);
        }
    },

    executeRecaptcha: function (action) {
        return new Promise((resolve, reject) => {
            grecaptcha.ready(function () {
                grecaptcha.execute('6LezxwUrAAAAAAc9vWRlGmqOJ99Yn15gF5HZdG5f', { action: action }).then(function (token) {
                    resolve(token);
                }).catch(function (error) {
                    reject(error);
                });
            });
        });
    }
};
