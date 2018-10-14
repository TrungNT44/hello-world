/*
 * jQuery Barcode Scanner 
 *
 */
(function($){
    $.fn.BCScanner=function(options){

        // If string given, call onComplete callback
        if(typeof options==="string"){
            this.each(function(){
                this.BCScannerTest(options);
            });
            return this;
        }
		
	    // If false (boolean) given, deinitialize plugin
	    if(options === false){
	        this.each(function(){
	    	    this.BCScannerOff();
	        });
	        return this;
	    }

        var defaults={
            onComplete:false, // Callback after detection of a successfull scanning (scanned string in parameter)
            onError:false, // Callback after detection of a unsuccessfull scanning (scanned string in parameter)
            onReceive:false, // Callback after receiving and processing a char (scanned char in parameter)
            onKeyDetect:false, // Callback after detecting a keyDown (key char in parameter) - in contrast to onReceive, this fires for non-character keys like tab, arrows, etc. too!
            timeBeforeScanTest:100, // Wait duration (ms) after keypress event to check if scanning is finished
            avgTimeByChar:30, // Average time (ms) between 2 chars. Used to do difference between keyboard typing and scanning
            minLength:6, // Minimum length for a scanning
            endChar:[9,13], // Chars to remove and means end of scanning
	        startChar:[], // Chars to remove and means start of scanning
	        ignoreIfFocusOn:false, // do not handle scans if the currently focused element matches this selector
	        scanButtonKeyCode:false, // Key code of the scanner hardware button (if the scanner button a acts as a key itself) 
	        scanButtonLongPressThreshold:3, // How many times the hardware button should issue a pressed event before a barcode is read to detect a longpress
            onScanButtonLongPressed:false, // Callback after detection of a successfull scan while the scan button was pressed and held down
            stopPropagation:false, // Stop immediate propagation on keypress event
            preventDefault:false // Prevent default action on keypress event
        };
        if(typeof options==="function"){
            options={onComplete:options}
        }
        if(typeof options!=="object"){
            options=$.extend({},defaults);
        }else{
            options=$.extend({},defaults,options);
        }
        
        this.each(function(){
            var self=this, $self=$(self), firstCharTime=0, lastCharTime=0, stringWriting='', callIsScanner=false, testTimer=false, scanButtonCounter=0;
            var initBCScanner=function(){
                firstCharTime=0;
                stringWriting='';
		        scanButtonCounter=0;
            };
	        self.BCScannerOff=function(){
		    $self.unbind('keydown.BCScanner');
		    $self.unbind('keypress.BCScanner');
	    }
	    self.isFocusOnIgnoredElement=function(){
            if(!options.ignoreIfFocusOn) return false;
		    if(typeof options.ignoreIfFocusOn === 'string') return $(':focus').is(options.ignoreIfFocusOn);
	        if(typeof options.ignoreIfFocusOn === 'object' && options.ignoreIfFocusOn.length){
		        var focused=$(':focus');
		        for(var i=0; i<options.ignoreIfFocusOn.length; i++){
			        if(focused.is(options.ignoreIfFocusOn[i])){
			            return true;
			        }
		        }
		    }
		    return false;
	    }
        self.BCScannerTest=function(s){
            // If string is given, test it
            if(s){
                firstCharTime=lastCharTime=0;
                stringWriting=s;
            }

		    if (!scanButtonCounter){
		        scanButtonCounter = 1;
		    }

			// If all condition are good (length, time...), call the callback and re-initialize the plugin for next scanning
			// Else, just re-initialize
			if(stringWriting.length>=options.minLength && lastCharTime-firstCharTime<stringWriting.length*options.avgTimeByChar){
		        if(options.onScanButtonLongPressed && scanButtonCounter > options.scanButtonLongPressThreshold) options.onScanButtonLongPressed.call(self,stringWriting,scanButtonCounter);
                    else if(options.onComplete) options.onComplete.call(self,stringWriting,scanButtonCounter);
                    $self.trigger('BCScannerComplete',{string:stringWriting});
                    initBCScanner();
                    return true;
                }else{
                    if(options.onError) options.onError.call(self,stringWriting);
                    $self.trigger('BCScannerError',{string:stringWriting});
                    initBCScanner();
                    return false;
                }
            }
            $self.data('BCScanner',{options:options}).unbind('.BCScanner').bind('keydown.BCScanner',function(e){
			    // If it's just the button of the scanner, ignore it and wait for the real input
		        if(options.scanButtonKeyCode !== false && e.which==options.scanButtonKeyCode) {
                    scanButtonCounter++;
                    // Cancel default
                    e.preventDefault();
                    e.stopImmediatePropagation();
                }
		        // Add event on keydown because keypress is not triggered for non character keys (tab, up, down...)
                // So need that to check endChar and startChar (that is often tab or enter) and call keypress if necessary
                else if((firstCharTime && options.endChar.indexOf(e.which)!==-1) 
			    || (!firstCharTime && options.startChar.indexOf(e.which)!==-1)){
                    // Clone event, set type and trigger it
                    var e2=jQuery.Event('keypress',e);
                    e2.type='keypress.BCScanner';
                    $self.triggerHandler(e2);
                    // Cancel default
                    e.preventDefault();
                    e.stopImmediatePropagation();
                }
                // Fire keyDetect event in any case!
                if(options.onKeyDetect) options.onKeyDetect.call(self,e);
                $self.trigger('BCScannerKeyDetect',{evt:e});
				
            }).bind('keypress.BCScanner',function(e){
		        if (this.isFocusOnIgnoredElement()) return;
                if(options.stopPropagation) e.stopImmediatePropagation();
                if(options.preventDefault) e.preventDefault();

                if(firstCharTime && options.endChar.indexOf(e.which)!==-1){
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    callIsScanner=true;
                }else if(!firstCharTime && options.startChar.indexOf(e.which)!==-1){
                    e.preventDefault();
                    e.stopImmediatePropagation();
		            callIsScanner=false;
                } else {
                    // check the keys pressed are numbers
                    if (typeof (e.which) != 'undefined' && e.which >= 48 && e.which <= 57) {
                        stringWriting += String.fromCharCode(e.which);
                    }
                    //if (typeof(e.which) != 'undefined'){
                    //    stringWriting+=String.fromCharCode(e.which);
                    //}
                    callIsScanner=false;
                }

                if(!firstCharTime){
                    firstCharTime=Date.now();
                }
                lastCharTime=Date.now();

                if(testTimer) clearTimeout(testTimer);
                if(callIsScanner){
                    self.BCScannerTest();
                    testTimer=false;
                }else{
                    testTimer=setTimeout(self.BCScannerTest,options.timeBeforeScanTest);
                }

                if(options.onReceive) options.onReceive.call(self,e);
                $self.trigger('BCScannerReceive',{evt:e});
            });
        });
        return this;
    }
})(jQuery);
