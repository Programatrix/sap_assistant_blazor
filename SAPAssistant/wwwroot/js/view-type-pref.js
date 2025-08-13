window.viewTypePref = {
    set: function (value) {
        localStorage.setItem('preferred_view_type', value);
    },
    get: function () {
        return localStorage.getItem('preferred_view_type');
    }
};

