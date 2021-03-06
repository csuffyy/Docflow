/*
 * jQuery File Upload Plugin JS Example 8.9.1
 * https://github.com/blueimp/jQuery-File-Upload
 *
 * Copyright 2010, Sebastian Tschan
 * https://blueimp.net
 *
 * Licensed under the MIT license:
 * http://www.opensource.org/licenses/MIT
 */

/* global $, window */

$(function () {
    'use strict';
	
    $('#ajaxUploadForm').fileupload({
		autoUpload: true,
		dataType: 'json',
        pasteZone: null,
		limitMultiFileUploads: 1,
		fileInput: $("input[type='file'][name='files']"),
		url: urlFileUpload,
       
    });
	
	// Load existing files:
	$('#ajaxUploadForm').addClass('fileupload-processing');
	$.ajax({
		// Uncomment the following to send cross-domain cookies:
		//xhrFields: {withCredentials: true},
		url: urlFileDownload,
		dataType: 'json',
		context: $('#ajaxUploadForm')[0]
	}).always(function () {
		$(this).removeClass('fileupload-processing');
	}).done(function (result) {
		$(this).fileupload('option', 'done')
		    .call(this, $.Event('done'), { result: result });
	});
});
