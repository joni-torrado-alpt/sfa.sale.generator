window.utils = {
    clipboard: {
        copyText: function (text) {
            navigator.clipboard.writeText(text).then(function () {
                alert("Copied!");
            })
                .catch(function (error) {
                    alert(error);
                });
        }
    }
};