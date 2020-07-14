var RtlLanguage = false;

function makeNavbarFixedTop() {

    var sticky = $("#navbar");
    if (sticky.length) {

        var pos = sticky.offset().top;
        var win = $(window);

        win.on("scroll", function () {
            if (win.scrollTop() >= pos) {
                sticky.addClass("fixed-top");
                $("#navbar-brand").removeClass("d-none");
            } else {
                sticky.removeClass("fixed-top");
                if (window.innerWidth >= 992) $("#navbar-brand").addClass("d-none");
            }
        });

        win.on("resize", function () {
            if (window.innerWidth < 992) {
                $("#navbar-brand").removeClass("d-none");
            } else {
                if (win.scrollTop() < pos) {
                    $("#navbar-brand").addClass("d-none");
                }
            }
        });

        //set normal navbar on page loaded
        sticky.removeClass("fixed-top");
        if (window.innerWidth >= 992) $("#navbar-brand").addClass("d-none");
    }
}

function BackToTopBtn() {
    if ($('#back-to-top').length) {
        var scrollTrigger = 100, // px
            backToTop = function () {
                var scrollTop = $(window).scrollTop();
                if (scrollTop > scrollTrigger) {
                    $('#back-to-top').addClass('show');
                } else {
                    $('#back-to-top').removeClass('show');
                }
            };
        backToTop();
        $(window).on('scroll', function () {
            backToTop();
        });
        $('#back-to-top').on('click', function (e) {
            e.preventDefault();
            $('html,body').animate({
                scrollTop: 0
            }, 700);
        });
    }
}

function ErrorAlert(title, message) {
    iziToast.error({
        title: title,
        titleColor: "white",
        messageColor: "white",
        iconColor: "white",
        icon: "fa fa-times-circle-o",
        rtl: RtlLanguage,
        position: 'topCenter',
        message: message,
        titleSize: 17,
        color: '#af0011'
    });
}

function SuccessAlert(title, message) {
    iziToast.success({
        title: title,
        titleColor: "white",
        messageColor: "white",
        iconColor: "white",
        icon: "fa fa-check-circle-o",
        rtl: RtlLanguage,
        position: 'topCenter',
        message: message, 
        titleSize: 17,
        color: '#119830'
    });
}

function WarningAlert(title, message) {
    iziToast.warning({
        title: title,
        titleColor: "white",
        messageColor: "white",
        iconColor: "white",
        icon: "fa fa-exclamation-triangle",
        rtl: RtlLanguage,
        position: 'topCenter',
        message: message,
        titleSize: 17,
        color: '#d27d00'
    });
}

function PopupWindows(url, title, w, h, params, method) {
    var loadingHtml =
        '<html lang="en"><head><title>Loading - Please wait ...</title></head><body><img src = "/content/img/Loading_Large.gif" style = "margin: auto;display: block; max-width:100%"></body></html>';

    // Fixes dual-screen position Most browsers Firefox
    var dualScreenLeft = window.screenLeft != undefined ? window.screenLeft : screen.left;
    var dualScreenTop = window.screenTop != undefined ? window.screenTop : screen.top;

    var width = window.innerWidth ? window.innerWidth : document.documentElement.clientWidth ? document.documentElement.clientWidth : screen.width;
    var height = window.innerHeight ? window.innerHeight : document.documentElement.clientHeight ? document.documentElement.clientHeight : screen.height;

    var left = ((width / 2) - (w / 2)) + dualScreenLeft;
    var top = ((height / 2) - (h / 2)) + dualScreenTop;


    //Post Data
    if (params) {
        var newWindow1 = window.open(title, title, 'scrollbars=yes, width=' + w + ', height=' + h + ', top=' + top + ', left=' + left);
        newWindow1.document.open().write(loadingHtml);
        setTimeout(function() {
                var form = newWindow1.document.createElement("form");
                form.setAttribute("method", method);
                form.setAttribute("action", url);
                for (var i in params) {
                    if (params.hasOwnProperty(i)) {
                        var input = document.createElement('input');
                        input.type = 'hidden';
                        input.name = i;
                        input.value = params[i];
                        form.appendChild(input);
                    }
                }

                $(newWindow1.document.body).append(form);
                form.submit();
            },
            800);

        // Puts focus on the newWindow
        if (window.focus) {
            newWindow1.focus();
        }

        return newWindow1;

    } else {
        var newWindow2 = window.open(title, title, 'scrollbars=yes, width=' + w + ', height=' + h + ', top=' + top + ', left=' + left);
        newWindow2.document.open().write(loadingHtml);
        setTimeout(function() {
                newWindow2.location.href = url;
            },
            800);

        // Puts focus on the newWindow
        if (window.focus) {
            newWindow2.focus();
        }

        return newWindow2;
    }
}

function isBlank(obj) {
    return (!obj || $.trim(obj) === "");
};

function isValidEmailAddress(emailAddress) {
    var pattern = new RegExp(/^(("[\w-\s]+")|([\w-]+(?:\.[\w-]+)*)|("[\w-\s]+")([\w-]+(?:\.[\w-]+)*))(@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$)|(@\[?((25[0-5]\.|2[0-4][0-9]\.|1[0-9]{2}\.|[0-9]{1,2}\.))((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[0-9]{1,2})\.){2}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[0-9]{1,2})\]?$)/i);
    return pattern.test(emailAddress);
}

function randomPassword(length) {
    var chars = "abcdefghijklmnopqrstuvwxyz!@#$%^&*()-+<>ABCDEFGHIJKLMNOP1234567890";
    var pass = "";
    for (var x = 0; x < length; x++) {
        var i = Math.floor(Math.random() * chars.length);
        pass += chars.charAt(i);
    }
    return pass;
}


function ShowProductPurchaseWizard(prodId, invId) {
    var modal = $('#dlgProductPurchaseWizard');
    if (modal.length) {
        $('#LoadingPanel').show();
        $.ajax({
            type: "POST",
            url: '/Purchase/PurchaseProductWizard',
            data: {
                productId: prodId,
                invoiceId: invId
            },
            error: function (xhr, status, error) {
                $('#dlgProductPurchaseWizard').html('');
                $('#LoadingPanel').hide();
                ErrorAlert('Error', 'An error occurred while communicating with the server, please check your internet connection.');
            },
            success: function (response) {
                $('#dlgProductPurchaseWizard').html(response);
            },
            complete: function () {
                $('#LoadingPanel').hide();
            }
        });
    }
}

String.prototype.format = String.prototype.f = function () {
    var s = this,
        i = arguments.length;

    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
};


/* Custom Client Side Validations------------ */

$(function () {
    $.validator.setDefaults({
        ignore: []// any other default options and/or rules
    });

    jQuery.validator.addMethod('requiredlocalized', function (value, element, params) {
        if ($(element).attr("validation-element") !== undefined) {
            var objectValue = $("#" + $(element).attr("validation-element")).val();
            if (isBlank(objectValue)) {
                return false;
            }
            return true;
        } else {
            if (isBlank(value)) {
                return false;
            }
            return true;
        }
    }, '');

    jQuery.validator.addMethod('emailaddress', function (value, element, params) {
        if (!isBlank(value) && !isValidEmailAddress(value)) {
            return false;
        }
        if ($(element).attr("validation-element") !== undefined) {
            var idStart = $(element).attr("validation-element");
            for (var i = 0; i < $('*[id^="' + idStart + '"]').length; i++) {
                var currentElement = $('*[id^="' + idStart + '"]')[i];
                if (!isBlank($(currentElement).val()) && !isValidEmailAddress($(currentElement).val())) {
                    return false;
                }
            }
        }
        return true;
    }, '');

    jQuery.validator.addMethod('maxlength', function (value, element, params) {
        if (value.length > params) {
            return false;
        }
        if ($(element).attr("validation-element") !== undefined) {
            var idStart = $(element).attr("validation-element");
            for (var i = 0; i < $('*[id^="' + idStart + '"]').length; i++) {
                var currentElement = $('*[id^="' + idStart + '"]')[i];
                if ($(currentElement).val().length > params) {
                    return false;
                }
            }
        }
        return true;
    }, '');

    jQuery.validator.addMethod('allowedextensions', function (value, element, params) {
        var extensions = params.split(',');
        if (isBlank(value)) {
            return true;
        }
        var fileName = value;

        for (var i = 0; i < extensions.length; i++) {
            if (fileName.endsWith(extensions[i])) {
                return true;
            }
        }

        return false;
    }, '');

    jQuery.validator.addMethod('maxfilesize', function (value, element, params) {
        if (isBlank(value)) {
            return true;
        }
        var size = element.files[0].size;

        if (size <= params) {
            return true;
        }

        return false;
    }, '');

    jQuery.validator.addMethod('compare', function (value, element, params) {
        if (value === $("#"+params).val()) {
            return true;
        }
        return false;
    }, '');

    jQuery.validator.addMethod('shouldbetrue', function (value, element, params) {
        return $(element).is(':checked');
    }, '');

    
    jQuery.validator.unobtrusive.adapters.add('requiredlocalized', function (options) {
        options.rules['requiredlocalized'] = {};
        options.messages['requiredlocalized'] = options.message;
    });

    jQuery.validator.unobtrusive.adapters.add('emailaddress', function (options) {
        options.rules['emailaddress'] = {};
        options.messages['emailaddress'] = options.message;
    });

    jQuery.validator.unobtrusive.adapters.add('maxlength', ['maxallowedlength'], function (options) {
        options.rules['maxlength'] = options.params.maxallowedlength;
        options.messages['maxlength'] = options.message;
    });

    jQuery.validator.unobtrusive.adapters.add('allowedextensions', ['extensions'], function (options) {
        options.rules['allowedextensions'] = options.params.extensions;
        options.messages['allowedextensions'] = options.message;
    });

    jQuery.validator.unobtrusive.adapters.add('maxfilesize', ['maxsize'], function (options) {
        options.rules['maxfilesize'] = options.params.maxsize;
        options.messages['maxfilesize'] = options.message;
    });

    jQuery.validator.unobtrusive.adapters.add('compare', ['comparewith'], function (options) {
        options.rules['compare'] = options.params.comparewith;
        options.messages['compare'] = options.message;
    });

    jQuery.validator.unobtrusive.adapters.add('shouldbetrue', function (options) {
        options.rules['shouldbetrue'] = {};
        options.messages['shouldbetrue'] = options.message;
    });

}(jQuery));

/* ------------Custom Client Side Validations */


$(document).ready(function () {
    // NavBar Sticky
    makeNavbarFixedTop();

    BackToTopBtn();

    $('[data-toggle="tooltip"]').tooltip();

    /* Multi level bootstrap navbar------------ */
    $('.dropdown-menu a.dropdown-toggle').on('mouseover', function (e) {
        if (!$(this).next().hasClass('show')) {
            $(this).parents('.dropdown-menu').first().find('.show').removeClass("show");
        }
        var $subMenu = $(this).next(".dropdown-menu");
        $subMenu.toggleClass('show');


        $(this).parents('li.nav-item.dropdown.show').on('hidden.bs.dropdown', function (e) {
            $('.dropdown-submenu .show').removeClass("show");
        });


        return false;
    });
    /* ------------Multi level bootstrap navbar */
});

//Images Lazy Load
var myLazyLoad = new LazyLoad({
    elements_selector: ".lazy",
    load_delay: 500,
    threshold: 0,
    callback_error: function (element) {
        $(element).attr("src", "/Content/img/image-not-available.png");
    },
});