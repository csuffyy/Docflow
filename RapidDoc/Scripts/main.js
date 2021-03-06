﻿function calendar_init(url_json_calendar) {
    var date = new Date();
    var workScheduleId = $('#WorkScheduleId').val();

    $('#calendar').fullCalendar({

        dayClick: function (date, allDay, jsEvent, view) {
            var status;
            $.ajax({
                type: 'POST',
                data: JSON.stringify({ date: date }),
                contentType: 'application/json',
                success: function (data) {
                    status = 'true';
                },
                error: function () {
                    status = 'false';
                }
            });
            if (status != 'false')
                $(this).toggleClass("calendarSelected");
        },

        firstDay: 1,

        dayRender: function (date, cell) {
            $.getJSON(url_json_calendar, { 'id': workScheduleId, 'day': date.getDate(), 'month': date.getMonth()+1, 'year': date.getFullYear() },
            function (data) {

                if (data.dayOff == 'true') {
                    cell.toggleClass("calendarSelected");
                }
            });
        }
    });
}

function selectpicker_init() {
    $('.selectpicker').selectpicker({
        width: '30%',
        style: 'btn btn-default btn-sm'
    });
}

function selectpickerfull_init() {
    $('.selectpickerfull').selectpicker({
        width: '100%',
        style: 'btn btn-default btn-sm'
    });
}

function selectpickermanual_init() {
    $('.selectpickermanual').selectpicker();
}

function popover_init() {
    $('.popover-link').popover({
        trigger: 'hover',
        placement: 'auto'
    });
}

function duallist_init(placeholder) {
    $('.duallist').bootstrapDualListbox({
        infoText: false,
        filterPlaceHolder: placeholder,
        moveOnSelect: false,
        preserveSelectionOnMove: 'moved'
    });

    $("#duallistform").submit(function (e) {
        var duallistdata = $('.duallist').val();

        e.preventDefault();
        $.ajax({
            type: 'POST',
            data: JSON.stringify({ listdata: duallistdata, isAjax: true }),
            contentType: 'application/json',
            success: function (data) {
                if (data.result == 'Redirect') {
                    window.location = data.url;
                }
                else {
                    $('.validation-summary-valid').append('<p class="field-validation-error">' + data.errorText + '</>').show();
                    setTimeout(function () {
                        $('input[type="submit"]').removeAttr('disabled');
                    }, 1);
                }
            }
        });
    });
}

function datepicker_init(lang) {
    var datepicker = $('.datepicker').datepicker({
        autoclose: true,
        language: lang,
        todayBtn: true,
        todayHighlight: true
    });
}

function timepicker_init() {
    $('.timepicker').timepicker({
        showMeridian: false,
        showInputs: false
    });
}

function datetimepicker_init(lang) {
    $('.datetimepicker').datetimepicker({
        language: lang,
        showToday: true
    });
}

function checkTextExists(text) { 
    //var prepare = text.replace(/<([^>]+?)([^>]*?)>(.*?)<\/\1>/ig, "");
    var prepare = text.replace(/(<([^>]+)>)/ig, "");
   // prepare = prepare.replace(/[^\w\s]/gi, '');
    prepare = prepare.trim();
    return prepare !== "";
}

function getClearText(text) {
    //var prepare = text.replace(/<([^>]+?)([^>]*s?)>(.*?)<\/\1>/ig, "");
    var prepare = text.replace(/(<([^>]+)>)/ig, "");
    var prepare = prepare.replace(/&nbsp;/gi, '');
    // prepare = prepare.replace(/[^\w\s]/gi, '');
    prepare = prepare.trim();
    return prepare;
}

function qrcode_init() {
    var qrcode = new QRCode(document.getElementById("qrcode"), {
        width: 100,
        height: 100,
        colorDark: "#000000",
        colorLight: "#ffffff"
    });

    qrcode.makeCode(document.location.href);
}

function checkbox_init(checked, unchecked) {
    $(':checkbox').each(function (e) {
        if ($(this).hasClass('labelauty') == false && $(this).hasClass('pseudo-checkbox') == false) {
            $(this).labelauty({
                label: true,
                checked_label: checked,
                unchecked_label: unchecked
            });
        }
    });
}

function favoritebutton(documentId, userId) {
    $('#favoriteButton').change(function () {
        if (window.FormData !== undefined) {
            var formData = new FormData();
            formData.append("documentId", documentId);
            formData.append("userId", userId);

            $.ajax({
                type: "POST",
                url: '/Document/FavoriteDocument/',
                data: formData,
                contentType: false,
                processData: false
            });
        }
    });
}

function summernote_init(lang) {
    if ((lang == "") || (lang == ""))
    {
        lang = 'en-US';
    }

    if ($(".summernote")[0]) {
        $('.summernote').summernote({
            height: 350,
            focus: false,
            disableDragAndDrop: true,
            lang: lang,
            defaultFontName: 'Arial',
            toolbar: [
                ['style', ['style']], // no style button
                ['style', ['bold', 'italic', 'clear']],
                //['fontsize', ['fontsize']],
                //['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                //['height', ['height']],
                ['insert', ['link']], // no insert buttons
                ['table', ['table']], // no table button
                ['misc', ['undo', 'redo']]
                //['help', ['help']] //no help button
            ],
            styleTags: ['p', 'h6'],
            callbacks: {
                onPaste: function (e) {
                    var startNote = $(this);
                    var before = startNote.summernote('code');
                    var plainText = ((e.originalEvent || e).clipboardData || window.clipboardData).getData('Text');
                    var bufferText = "";

                    // Opera 8.0+
                    var isOpera = (!!window.opr && !!opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;
                    // Firefox 1.0+
                    var isFirefox = typeof InstallTrigger !== 'undefined';
                    // At least Safari 3+: "[object HTMLElementConstructor]"
                    var isSafari = Object.prototype.toString.call(window.HTMLElement).indexOf('Constructor') > 0;
                    // Internet Explorer 6-11
                    var isIE = /*@cc_on!@*/false || !!document.documentMode;
                    // Edge 20+
                    var isEdge = !isIE && !!window.StyleMedia;
                    // Chrome 1+
                    var isChrome = !!window.chrome && !!window.chrome.webstore;
                    // Blink engine detection
                    var isBlink = (isChrome || isOpera) && !!window.CSS;

                    if (isChrome || isFirefox || isOpera || isSafari) {
                        bufferText = ((e.originalEvent || e).clipboardData || window.clipboardData).getData('text/html');
                    }

                    setTimeout(function () {
                        if (isChrome || isFirefox || isOpera || isSafari) {
                            if (bufferText != '' || (bufferText == '' && plainText != '')) {
                                if (bufferText == '')
                                    bufferText = plainText;
                                bufferText = cleanPastedHTML(bufferText);
                                bufferText = removeProperties(bufferText);

                                //console.log('bufferText: ' + bufferText);
                                var node = $('<span />').html(bufferText)[0];
                                startNote.summernote('insertNode', node);
                            }
                        }
                    }, 10);

                    if (isChrome || isFirefox || isOpera || isSafari) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                }
            }
        });
    }
}

function summernotelight_init(lang) {
    if ((lang == "") || (lang == "")) {
        lang = 'en-US';
    }

    if ($(".summernotelight")[0]) {
        $('.summernotelight').summernote({
            focus: false,
            lang: lang,
            disableDragAndDrop: true,
            defaultFontName: 'Arial',
            toolbar: [
                ['style', ['style']], // no style button
                ['style', ['bold', 'clear']],
                ['group', ['insertfile']],
                //['fontsize', ['fontsize']],
                //['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                //['height', ['height']],
                ['insert', ['link', 'insertfile']], // no insert buttons
                //['table', ['table']], // no table button
                ['misc', ['undo', 'redo']]
                //['help', ['help']] //no help button
            ],
            styleTags: ['p'],
            callbacks: {
                onPaste: function (e) {
                    var startNote = $(this);
                    var before = startNote.summernote('code');
                    var plainText = ((e.originalEvent || e).clipboardData || window.clipboardData).getData('Text');
                    var bufferText = "";

                    // Opera 8.0+
                    var isOpera = (!!window.opr && !!opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;
                    // Firefox 1.0+
                    var isFirefox = typeof InstallTrigger !== 'undefined';
                    // At least Safari 3+: "[object HTMLElementConstructor]"
                    var isSafari = Object.prototype.toString.call(window.HTMLElement).indexOf('Constructor') > 0;
                    // Internet Explorer 6-11
                    var isIE = /*@cc_on!@*/false || !!document.documentMode;
                    // Edge 20+
                    var isEdge = !isIE && !!window.StyleMedia;
                    // Chrome 1+
                    var isChrome = !!window.chrome && !!window.chrome.webstore;
                    // Blink engine detection
                    var isBlink = (isChrome || isOpera) && !!window.CSS;

                    if (isChrome || isFirefox || isOpera || isSafari) {
                        bufferText = ((e.originalEvent || e).clipboardData || window.clipboardData).getData('text/html');
                    }

                    setTimeout(function () {
                        if (isChrome || isFirefox || isOpera || isSafari) {
                            if (bufferText != '' || (bufferText == '' && plainText != '')) {
                                if (bufferText == '')
                                    bufferText = plainText;
                                bufferText = cleanPastedHTML(bufferText);
                                bufferText = removeProperties(bufferText);
                                console.log(bufferText);
                                var node = $('<span />').html(bufferText)[0];
                                startNote.summernote('insertNode', node);
                            }
                        }
                    }, 10);

                    if (isChrome || isFirefox || isOpera || isSafari) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                }
            }
        });
    }
}

function summernotemin_init(lang) {
    if ((lang == "") || (lang == "")) {
        lang = 'en-US';
    }

    if ($(".summernotemin")[0]) {
        $('.summernotemin').summernote({
            height: 100,
            focus: false,
            lang: lang,
            disableDragAndDrop: true,
            defaultFontName: 'Arial',
            toolbar: [
                ['style', ['bold', 'clear']],
                ['insert', ['link', 'insertfile']],
                ['misc', ['undo', 'redo']]
            ],
            callbacks: {
                onPaste: function (e) {
                    var startNote = $(this);
                    var before = startNote.summernote('code');
                    var plainText = ((e.originalEvent || e).clipboardData || window.clipboardData).getData('Text');
                    var bufferText = "";

                    // Opera 8.0+
                    var isOpera = (!!window.opr && !!opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;
                    // Firefox 1.0+
                    var isFirefox = typeof InstallTrigger !== 'undefined';
                    // At least Safari 3+: "[object HTMLElementConstructor]"
                    var isSafari = Object.prototype.toString.call(window.HTMLElement).indexOf('Constructor') > 0;
                    // Internet Explorer 6-11
                    var isIE = /*@cc_on!@*/false || !!document.documentMode;
                    // Edge 20+
                    var isEdge = !isIE && !!window.StyleMedia;
                    // Chrome 1+
                    var isChrome = !!window.chrome && !!window.chrome.webstore;
                    // Blink engine detection
                    var isBlink = (isChrome || isOpera) && !!window.CSS;

                    if (isChrome || isFirefox || isOpera || isSafari) {
                        bufferText = ((e.originalEvent || e).clipboardData || window.clipboardData).getData('text/html');
                    }

                    setTimeout(function () {

                        if (isChrome || isFirefox || isOpera || isSafari) {
                            if (bufferText != '' || (bufferText == '' && plainText != '')) {
                                if (checkSupportTags(bufferText)) {
                                    bufferText = plainText;
                                }
                                else {
                                    bufferText = cleanPastedHTML(bufferText);
                                    bufferText = removeProperties(bufferText);
                                }
                                var node = $('<span />').html(bufferText)[0];
                                startNote.summernote('insertNode', node);
                            }
                        }
                    }, 10);

                    if (isChrome || isFirefox || isOpera || isSafari) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                }
            }
        });
    }
}

function checkSupportTags(input) {
    if (input.indexOf('<table') == -1 && input.indexOf('<a href') == -1) {
        return true;
    }

    return false;
}

function cleanPastedHTML(input) {
    if (input.indexOf('Mso') != -1 && input.indexOf('<!--EndFragment-->') != -1) {
        input = input.substr(0, input.indexOf('<!--EndFragment-->')+38);
    }

    // 1. remove line breaks / Mso classes
    var stringStripper = /(\n|\r| class=(")?Mso[a-zA-Z]+(")?)/g;
    var output = input.replace(stringStripper, ' ');
    // 2. strip Word generated HTML comments
    var commentSripper = new RegExp('<!--(.*?)-->', 'g');
    var output = output.replace(commentSripper, '');
    var tagStripper = new RegExp('<(/)*(head|html|body|meta|link|span|strong|h1|h2|h3|h4|h5|h6|small|\\?xml:|st1:|o:|font)(.*?)>', 'gi');
    // 3. remove tags leave content if any
    output = output.replace(tagStripper, '');
    // 4. Remove everything in between and including tags '<style(.)style(.)>'
    var badTags = ['style', 'script', 'applet', 'embed', 'noframes', 'noscript', 'img'];
    for (var i = 0; i < badTags.length; i++) {
        tagStripper = new RegExp('<' + badTags[i] + '.*?' + badTags[i] + '(.*?)>', 'gi');
        output = output.replace(tagStripper, '');
    }
    // 5. remove attributes ' style="..."'
    if (output.indexOf('<table ') == -1) {
        var badAttributes = ['style', 'start'];
        for (var i = 0; i < badAttributes.length; i++) {
            var attributeStripper = new RegExp(' ' + badAttributes[i] + '="(.*?)"', 'gi');
            output = output.replace(attributeStripper, '');
        }
    }

    var regex = new RegExp('<table border="0"', 'gi');
    output = output.replace(regex, '<table class="table table-bordered table-condensed"');

    var regex = new RegExp('<table border=0', 'gi');
    output = output.replace(regex, '<table class="table table-bordered table-condensed"');

    return output.trim();
}

function removeProperties(markup) {
    var div = document.createElement('div');
    div.innerHTML = markup;
    var el, els = div.getElementsByTagName('*');

    for (var i = 0, iLen = els.length; i < iLen; i++) {
        el = els[i];
        el.id = '';
        el.style = '';
        el.className = '';

        if (el.tagName == 'TABLE') {
            el.className = 'table table-bordered table-condensed';
            el.style = 'border-collapse:collapse; border: 1px solid black;';
        }

        if (el.tagName == 'TH') {
            el.style = 'border: 1px solid black;';
        }
        if(el.tagName == 'TD') {
            el.style = 'border: 1px solid black; padding: 3px;';
        }
    }

    var commentSripper = new RegExp('<!--(.*?)-->', 'g');
    var output = div.innerHTML.replace(commentSripper, '');
    output = replaceAll(output, 'id=""', '');
    output = replaceAll(output, 'class=""', '');

    var tagStripper = new RegExp('width="(.*?)"', 'gi');
    output = output.replace(tagStripper, '');
    var tagStripper = new RegExp('height="(.*?)"', 'gi');
    output = output.replace(tagStripper, '');

    output = '<span>' + output + '</span>';
    output = replaceAll(output, '</table>', '</table></br>');
    return output;
}

function replaceAll(str, find, replace) {
    return str.replace(new RegExp(find, 'g'), replace);
}

function grid_init(url_json) {
    var self = this;
    this.currentPage = 1;

    var pageLink = $(".grid-ajax-pager");
    var gridTableBody = pageLink.closest(".grid-wrap").find("tbody");

    $(window).scroll(function () {
        if ($(window).scrollTop() <= $(document).height() - $(window).height() && $(window).scrollTop() >= $(document).height() - $(window).height() - 10) {
            $('div#loading').html('<img src="/Content/Custom/image-icon/autoload.gif"/>');
            self.loadNextPage();
        }
    });

    this.loadNextPage = function () {
        self.currentPage++;

        $.get(url_json + self.pad(location.search) + self.currentPage)
            .done(function (response) {

                gridTableBody.append(response.Html);
                if (!response.HasItems)
                    pageLink.hide();

                $('div#loading').empty();

                $('.popover-link').popover({
                    trigger: 'hover'
                });
            })
            .fail(function () {
            });
    };

    this.pad = function (query) {
        if (query.length == 0) return "?page=";
        return query + "&page=";
    };

    $('table.grid-table > tbody > tr > td.empty:has(.text-warning)').css("background", "#f39c12");
    $('table.grid-table > tbody > tr > td.empty:has(.text-danger)').css("background", "#e74c3c");
}

function custom_tagsinputRoles_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputRoles]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt2 = $('input[data-role=tagsinputRoles]');
            elt2.tagsinput({
                itemValue: 'value',
                itemText: 'text',
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt2.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt2));

            currentValue2 = $('input[data-role=tagsinputRoles]').val();
            if (currentValue2 != null) {
                currentArrData2 = currentValue2.split(",");
                $('input[data-role=tagsinputRoles]').val('');

                if (currentArrData2.length > 1) {
                    for (var i = 0; i < currentArrData2.length; i += 2) {
                        var key = currentArrData2[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData2[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputRoles]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
}

function custom_tagsinputOnlyGroup_init(url_json) {
    try {
        var element = document.getSelection('input[data-role=tagsinputOnlyGroup]');
        if (typeof (element) != 'undefined' && element != null) {
            elt = $('input[data-role=tagsinputOnlyGroup]');
            elt.tagsinput({
                itemValue: 'value',
                itemText: 'text',
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt));

            currentValue = $('input[data-role=tagsinputOnlyGroup]').val();
            if (currentValue != null) {
                currentArrData = currentValue.split(",");
                $('input[data-role=tagsinputOnlyGroup]').val('');

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputOnlyGroup]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
    }
    catch (e) {
    }
}

function custom_tagsinputEmplOneKZ_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputEmplOneKZ]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt = $('input[data-role=tagsinputEmplOneKZ]');
            elt.tagsinput({
                itemValue: 'value',
                itemText: 'text',
                maxTags: 1,
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt));

            currentValue = $('input[data-role=tagsinputEmplOneKZ]').val();
            if (currentValue != null) {
                currentArrData = currentValue.split(",");
                $('input[data-role=tagsinputEmplOneKZ]').val('');

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputEmplOneKZ]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
}

function custom_tagsinputEmplOneTrip_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputEmplOneTrip]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt = $('input[data-role=tagsinputEmplOneTrip]');
            elt.tagsinput({
                itemValue: 'value',
                itemText: 'text',
                maxTags: 1,
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt));

            currentValue = $('input[data-role=tagsinputEmplOneTrip]').val();
            if (currentValue != null) {
                currentArrData = currentValue.split(",");
                $('input[data-role=tagsinputEmplOneTrip]').val('');

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputEmplOneTrip]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
}

function custom_tagsinputEmployer_init(url_json) {
    try {
        $('input[data-role=tagsinputEmployer]').each(function (e) {
            var elt = $(this);

            elt.tagsinput({
                itemValue: 'value',
                itemText: 'text',
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt));

            var currentValue = elt.val();
            if (currentValue != null && currentValue != '') {
                var currentArrData = currentValue.split(",");

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $(this).tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        });
    }
    catch (e) {
    }
}

function custom_tagsinputEmployerIntercompany_init(url_json) {
    try {
        $('input[data-role=tagsinputEmployerIntercompany]').each(function (e) {
            var elt = $(this);

            elt.tagsinput({
                itemValue: 'value',
                itemText: 'text',
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt));

            var currentValue = elt.val();
            if (currentValue != null && currentValue != '') {
                var currentArrData = currentValue.split(",");

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $(this).tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        });
    }
    catch (e) {
    }
}

function custom_tagsinputEmployerIntercompanyManual_init(url_json) {
    try {
        $('input[data-role=tagsinputEmployerIntercompanyManual]').each(function (e) {
            var elt = $(this);

            elt.tagsinput({
                trimValue: true,
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum["text"]);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt)).blur(function () {
                $(this).val('')
            });
        });
    }
    catch (e) {
    }
}

function custom_tagsinputEmployerManual_init(url_json) {
    try {
        $('input[data-role=tagsinputEmployerManual]').each(function (e) {
            var elt = $(this);

            elt.tagsinput({
                trimValue: true,
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum["text"]);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt)).blur(function () {
                $(this).val('')
            });
        });
    }
    catch (e) {
    }
}

function custom_tagsinputGroup_init(url_json) {
    try {
        $('input[data-role=tagsinputGroup]').each(function (e) {
            var elt = $(this);

            elt.tagsinput({
                itemValue: 'value',
                itemText: 'text',
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt));

            var currentValue = elt.val();
            if (currentValue != null && currentValue != '') {
                var currentArrData = currentValue.split(",");

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $(this).tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        });
    }
    catch (e) {
    }
}

function custom_tagsinputEmployerOne_init(url_json) {
    try {
        $('input[data-role=tagsinputEmployerOne]').each(function (e) {
            var elt = $(this);

            elt.tagsinput({
                itemValue: 'value',
                itemText: 'text',
                maxTags: 1,
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt));

            currentValue = elt.val();
            if (currentValue != null && currentValue != '') {
                var currentArrData = currentValue.split(",");

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $(this).tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        });
    }
    catch (e) {
    }
}

function custom_tagsinputEmployerOneIntercompany_init(url_json) {
    try {
        $('input[data-role=tagsinputEmployerOneIntercompany]').each(function (e) {
            var elt = $(this);

            elt.tagsinput({
                itemValue: 'value',
                itemText: 'text',
                maxTags: 1,
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt));

            currentValue = elt.val();
            if (currentValue != null && currentValue != '') {
                var currentArrData = currentValue.split(",");

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $(this).tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        });
    }
    catch (e) {
    }
}

function custom_tagsinputDepartment_init(url_json) {
    try {
        var element = document.getSelection('input[data-role=tagsinputDepartment]');
        if (typeof (element) != 'undefined' && element != null) {
            elt = $('input[data-role=tagsinputDepartment]');
            elt.tagsinput({
                itemValue: 'value',
                itemText: 'text',
                tagClass: function (item) {
                    return 'label label-primary bts-tags';
                }
            });

            elt.tagsinput('input').typeahead({
                valueKey: 'text',
                prefetch: url_json,
                template: '<p>{{text}}</p>',
                engine: Hogan

            }).bind('typeahead:selected', $.proxy(function (obj, datum) {
                this.tagsinput('add', datum);
                this.tagsinput('input').typeahead('setQuery', '');
            }, elt));

            currentValue = $('input[data-role=tagsinputDepartment]').val();
            if (currentValue != null) {
                currentArrData = currentValue.split(",");
                $('input[data-role=tagsinputDepartment]').val('');

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputDepartment]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
    }
    catch (e) {
    }
}

function checkIncomeDoc() {
    var organizationId = $("#OrganizationTableId").val();
    var outgoingNumber = $("#OutgoingNumber").val();
    var outgoingDate = $("#OutgoingDate").val();

    if (organizationId.length > 0 && outgoingNumber.length > 0 && outgoingDate.length > 0) {
        var formData = new FormData();
        formData.append('OrganizationId', organizationId);
        formData.append('OutgoingNumber', outgoingNumber);
        formData.append('OutgoingDate', outgoingDate);

        $.ajax({
            type: 'POST',
            url: '/@ViewContext.RouteData.Values["company"]/Custom/CheckIncomeDocument/',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                $('#dublicateBlock').html(response);
            }
        });
    }
}

function actionForRowGrid(url) {
    pageGrids.ordersGrid.onRowSelect(function (e) {
        window.location.href = url + '/' + e.row.Id;
    });
}
