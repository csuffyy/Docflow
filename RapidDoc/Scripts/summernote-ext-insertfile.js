(function (factory) {
  /* global define */
  if (typeof define === 'function' && define.amd) {
    // AMD. Register as an anonymous module.
    define(['jquery'], factory);
  } else if (typeof module === 'object' && module.exports) {
    // Node/CommonJS
    module.exports = factory(require('jquery'));
  } else {
    // Browser globals
    factory(window.jQuery);
  }
}(function ($) {

  // Extends plugins for adding hello.
  //  - plugin is external module for customizing.
    $.extend($.summernote.plugins, {
        /**
         * @param {Object} context - context object has status of editor.
         */
        'insertfile': function (context) {
            var self = this;

            // ui has renders to build ui elements.
            //  - you can create a button with `ui.button`
            var ui = $.summernote.ui;

            // add hello button
            context.memo('button.insertfile', function () {
                // create button
                var button = ui.button({
                    contents: '<i class="fa fa-paperclip"/>',
                    tooltip: 'insertfile',
                    click: function () {
                        self.show();
                    }
                });

                // create jQuery object from button instance.
                var $insertfile = button.render();
                return $insertfile;
            });

            // This events will be attached when editor is initialized.
            this.events = {
                // This will be called after modules are initialized.
                'summernote.init': function (we, e) {

                },
                // This will be called when user releases a key on editable.
                'summernote.keyup': function (we, e) {

                }
            };

            // This method will be called when editor is initialized by $('..').summernote();
            // You can create elements for plugin
            this.initialize = function () {
                var self = this;
                var ui = $.summernote.ui;

                var $editor = context.layoutInfo.editor;
                var options = context.options;
                var lang = options.langInfo;

                var $container = options.dialogsInBody ? $(document.body) : $editor;

                var body = '<div class="form-group row note-group-select-from-files">' +
                                '<input class="note-insertfile-input form-control" type="file" name="fileinput" />' +
                            '</div>'
                var footer = '<button href="#" class="btn btn-primary note-insertfile-btn disabled" disabled>' + 'OK' + '</button>';

                this.$dialog = ui.dialog({
                    title: lang.image.selectFromFiles,
                    fade: options.dialogsFade,
                    body: body,
                    footer: footer
                }).render().appendTo($container);

                this.show = function () {
                    var text = context.invoke('editor.getSelectedText');
                    context.invoke('editor.saveRange');
                    this.showInsertFileDialog(text).then(function (url) {
                        // [workaround] hide dialog before restore range for IE range focus
                        ui.hideDialog(self.$dialog);
                        context.invoke('editor.restoreRange');
                    }).fail(function () {
                        context.invoke('editor.restoreRange');
                    });
                };

                var toggleBtn = function ($btn, isEnable) {
                    $btn.toggleClass('disabled', !isEnable);
                    $btn.attr('disabled', !isEnable);
                };

                this.showInsertFileDialog = function (text) {
                    return $.Deferred(function (deferred) {
                        var $insertFileDialog = self.$dialog;

                        var $insertFileUrl = $insertFileDialog.find('.note-insertfile-input'),
                        $insertFileBtn = $insertFileDialog.find('.note-insertfile-btn');
                        $insertFileDialog.one('shown.bs.modal', function () {
                            $insertFileUrl.on('change', function () {
                                toggleBtn($insertFileBtn, $insertFileUrl.val());
                            }).trigger('focus');

                            $insertFileBtn.click(function (event) {
                                event.preventDefault();
                                var files = $insertFileUrl[0].files[0];
                                if (window.FormData !== undefined) {
                                    var formData = new FormData();
                                    formData.append("fileInput", files);

                                    $.ajax({
                                        type: "POST",
                                        url: '/Document/UploadFile/',
                                        data: formData,
                                        contentType: false,
                                        processData: false,
                                        success: function (result) {
                                            var fileUrl = '<p><a href="' + result + '" target="_blank">' + files.name + '</a></p>';
                                            var $node = $('<span></span>').html(fileUrl)[0];
                                            if ($node) {
                                                context.invoke('editor.insertNode', $node);
                                            }
                                        },
                                        error: function (xhr, status, p3) {
                                            alert(xhr.responseText);
                                        }
                                    });
                                }

                                deferred.resolve($insertFileUrl.val());
                            });
                        }).one('hidden.bs.modal', function () {
                            $insertFileUrl.off('input');
                            $insertFileBtn.off('click');

                            if (deferred.state() === 'pending') {
                                deferred.reject();
                            }
                        }).modal('show');
                    });
                };

                // This methods will be called when editor is destroyed by $('..').summernote('destroy');
                // You should remove elements on `initialize`.
                this.destroy = function () {
                    ui.hideDialog(this.$dialog);
                    this.$dialog.remove();
                };
            }
        }
    });
}));
