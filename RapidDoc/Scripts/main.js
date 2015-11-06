﻿ function calendar_init(url_json_calendar) {
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
        filterPlaceHolder: placeholder
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
        format: 'dd.mm.yyyy',
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

function qrcode_init() {
    var qrcode = new QRCode(document.getElementById("qrcode"), {
        width: 100,
        height: 100,
        colorDark: "#000000",
        colorLight: "#ffffff"
    });

    qrcode.makeCode(document.location.href+'?isAfterView=true');
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
            onPaste: function (e) {
                var thisNote = $(this);
                var updatePastedText = function (someNote) {
                    var original = someNote.code();
                    var regex = new RegExp('<table border="0"', 'gi');
                    var cleaned = original.replace(regex, '<table class="table table-bordered table-condensed"');
                    someNote.code(cleaned);
                };

                setTimeout(function () {
                    updatePastedText(thisNote);
                }, 10);
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
            defaultFontName: 'Arial',
            toolbar: [],
            onPaste: function (e) {
                var thisNote = $(this);
                var updatePastedText = function (someNote) {
                    var original = someNote.code();
                    var cleaned = cleanPastedHTML(original); //this is where to call whatever clean function you want. I have mine in a different file, called CleanPastedHTML.
                    someNote.code(cleaned);
                };

                setTimeout(function () {
                    updatePastedText(thisNote);
                }, 10);
            }
        });
    }
}

function cleanPastedHTML(input) {
    // 1. remove line breaks / Mso classes
    var stringStripper = /(\n|\r| class=(")?Mso[a-zA-Z]+(")?)/g;
    var output = input.replace(stringStripper, ' ');
    // 2. strip Word generated HTML comments
    var commentSripper = new RegExp('<!--(.*?)-->', 'g');
    var output = output.replace(commentSripper, '');
    var tagStripper = new RegExp('<(/)*(meta|link|span|strong|h1|h2|h3|h4|h5|h6|small|\\?xml:|st1:|o:|font)(.*?)>', 'gi');
    // 3. remove tags leave content if any
    output = output.replace(tagStripper, '');
    // 4. Remove everything in between and including tags '<style(.)style(.)>'
    var badTags = ['style', 'script', 'applet', 'embed', 'noframes', 'noscript', 'img'];

    for (var i = 0; i < badTags.length; i++) {
        tagStripper = new RegExp('<' + badTags[i] + '.*?' + badTags[i] + '(.*?)>', 'gi');
        output = output.replace(tagStripper, '');
    }
    // 5. remove attributes ' style="..."'
    var badAttributes = ['style', 'start'];
    for (var i = 0; i < badAttributes.length; i++) {
        var attributeStripper = new RegExp(' ' + badAttributes[i] + '="(.*?)"', 'gi');
        output = output.replace(attributeStripper, '');
    }
    return output;
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

function custom_tagsinputEmpl_init(url_json) {
    try {
        var element = document.getSelection('input[data-role=tagsinputEmpl]');
        if (typeof (element) != 'undefined' && element != null) {
            elt = $('input[data-role=tagsinputEmpl]');
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

            currentValue = $('input[data-role=tagsinputEmpl]').val();
            if (currentValue != null) {
                currentArrData = currentValue.split(",");
                $('input[data-role=tagsinputEmpl]').val('');

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputEmpl]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
    }
    catch (e) { 
    }
}

function custom_tagsinputEmpl2_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputEmpl2]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt2 = $('input[data-role=tagsinputEmpl2]');
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

            currentValue2 = $('input[data-role=tagsinputEmpl2]').val();
            if (currentValue2 != null) {
                currentArrData2 = currentValue2.split(",");
                $('input[data-role=tagsinputEmpl2]').val('');

                if (currentArrData2.length > 1) {
                    for (var i = 0; i < currentArrData2.length; i += 2) {
                        var key = currentArrData2[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData2[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputEmpl2]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
}

function custom_tagsinputEmpl3_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputEmpl3]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt2 = $('input[data-role=tagsinputEmpl3]');
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

            currentValue2 = $('input[data-role=tagsinputEmpl3]').val();
            if (currentValue2 != null) {
                currentArrData2 = currentValue2.split(",");
                $('input[data-role=tagsinputEmpl3]').val('');

                if (currentArrData2.length > 1) {
                    for (var i = 0; i < currentArrData2.length; i += 2) {
                        var key = currentArrData2[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData2[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputEmpl3]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
}

function custom_tagsinputEmpl4_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputEmpl4]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt2 = $('input[data-role=tagsinputEmpl4]');
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

            currentValue2 = $('input[data-role=tagsinputEmpl4]').val();
            if (currentValue2 != null) {
                currentArrData2 = currentValue2.split(",");
                $('input[data-role=tagsinputEmpl4]').val('');

                if (currentArrData2.length > 1) {
                    for (var i = 0; i < currentArrData2.length; i += 2) {
                        var key = currentArrData2[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData2[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputEmpl4]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
}

function custom_tagsinputEmpl5_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputEmpl5]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt2 = $('input[data-role=tagsinputEmpl5]');
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

            currentValue2 = $('input[data-role=tagsinputEmpl5]').val();
            if (currentValue2 != null) {
                currentArrData2 = currentValue2.split(",");
                $('input[data-role=tagsinputEmpl5]').val('');

                if (currentArrData2.length > 1) {
                    for (var i = 0; i < currentArrData2.length; i += 2) {
                        var key = currentArrData2[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData2[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputEmpl5]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
}

function custom_tagsinputGroup_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputGroup]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt2 = $('input[data-role=tagsinputGroup]');
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

            currentValue2 = $('input[data-role=tagsinputGroup]').val();
            if (currentValue2 != null) {
                currentArrData2 = currentValue2.split(",");
                $('input[data-role=tagsinputGroup]').val('');

                if (currentArrData2.length > 1) {
                    for (var i = 0; i < currentArrData2.length; i += 2) {
                        var key = currentArrData2[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData2[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputGroup]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
}

function custom_tagsinputGroup2_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputGroup2]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt2 = $('input[data-role=tagsinputGroup2]');
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

            currentValue2 = $('input[data-role=tagsinputGroup2]').val();
            if (currentValue2 != null) {
                currentArrData2 = currentValue2.split(",");
                $('input[data-role=tagsinputGroup2]').val('');

                if (currentArrData2.length > 1) {
                    for (var i = 0; i < currentArrData2.length; i += 2) {
                        var key = currentArrData2[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData2[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputGroup2]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
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

function custom_tagsinputEmplIntercompany_init(url_json) {
    try {
        var element = document.getSelection('input[data-role=tagsinputEmplIntercompany]');
        if (typeof (element) != 'undefined' && element != null) {
            elt = $('input[data-role=tagsinputEmplIntercompany]');
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

            currentValue = $('input[data-role=tagsinputEmplIntercompany]').val();
            if (currentValue != null) {
                currentArrData = currentValue.split(",");
                $('input[data-role=tagsinputEmplIntercompany]').val('');

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputEmplIntercompany]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
    }
    catch (e) {
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

function custom_tagsinputEmplOne_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputEmplOne]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt = $('input[data-role=tagsinputEmplOne]');
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

            currentValue = $('input[data-role=tagsinputEmplOne]').val();
            if (currentValue != null) {
                currentArrData = currentValue.split(",");
                $('input[data-role=tagsinputEmplOne]').val('');

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputEmplOne]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
}

function custom_tagsinputEmplOne1_init(url_json) {
    var element = document.getSelection('input[data-role=tagsinputEmplOne1]');
    if (typeof (element) != 'undefined' && element != null) {
        try {
            elt = $('input[data-role=tagsinputEmplOne1]');
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

            currentValue = $('input[data-role=tagsinputEmplOne1]').val();
            if (currentValue != null) {
                currentArrData = currentValue.split(",");
                $('input[data-role=tagsinputEmplOne1]').val('');

                if (currentArrData.length > 1) {
                    for (var i = 0; i < currentArrData.length; i += 2) {
                        var key = currentArrData[i];
                        var numValue = i;
                        numValue++;
                        var value = currentArrData[numValue];
                        if (value.length > 0)
                            $('input[data-role=tagsinputEmplOne1]').tagsinput('add', { "value": key + "," + value, "text": value });
                    }
                }
            }
        }
        catch (e) {

        }
    }
}

function custom_tagsinputEmplDynamic_init(url_json) {
    $('input[data-role=tagsinputEmplDynamic]').each(function (e) {
        elt = $(this);

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
    });
}
