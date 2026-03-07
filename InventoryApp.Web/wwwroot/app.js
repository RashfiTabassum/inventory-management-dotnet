window.themeHelper = {
    getCookie: function (name) {
        const match = document.cookie.match(
            new RegExp('(^| )' + name + '=([^;]+)'));
        return match ? match[2] : null;
    },
    setCookie: function (name, value) {
        document.cookie = name + '=' + value
            + ';path=/;max-age=31536000';
    },
    applyTheme: function (isDark) {
        if (isDark) {
            document.body.classList.add('dark');
        } else {
            document.body.classList.remove('dark');
        }
    }
};