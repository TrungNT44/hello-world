function YesNoDialog(ctl, action) {
    $("#" + ctl).dialog({
      resizable: false,
      height: "auto",
      width: 400,
      modal: true,
      buttons: {
          Ok: function () {
              $.ajax({
                  url: action,
                  method: 'POST',
                  success: window.location.href = window.location.href
              });
          $( this ).dialog( "close" );
        },
        Cancel: function() {
          $( this ).dialog( "close" );
        }
      }
    });
  }