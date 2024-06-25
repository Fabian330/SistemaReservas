const items = document.querySelectorAll(".sideLinks li a");

items.forEach(function (item) {
    if (item.href === document.URL) {
        item.className = ("active");
    }
});