//give it a new name each time you need to do this
var cookieName = 'medman-v1.99';
//get the cookie
var c = $.cookie(cookieName);
//if it doesn't exist this is their first time and they need the refresh
if (c == null) {
    //set cookie so this happens only once
    $.cookie(cookieName, 0, { expires: 7, path: '/' });
    //do a "hard refresh" of the page, clearing the cache
    location.reload(true);
}