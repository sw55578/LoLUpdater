(function() {
  var download, downloadAlt, screenshot;

  if (document.getElementsByTagName("body")[0].className.indexOf("xp") > -1) {
    download = document.getElementById("download");
    downloadAlt = document.getElementById("download-alt");
    screenshot = document.getElementById("screenshot");
    download.href = "/download-xp";
    downloadAlt.href = "/download";
    screenshot.src = "img/screenshot_xp.png";
  }

}).call(this);
