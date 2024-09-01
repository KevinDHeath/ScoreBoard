PlayApp = {
  setName: function (name) {
    try {
      sessionStorage.setItem("Grass.Play", name);
      return true;
    } catch { return false; }
  },
  getName: function () {
    try {
      return sessionStorage.getItem("Grass.Play");
    } catch { return ''; }
  }
}
