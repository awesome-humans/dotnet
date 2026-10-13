// Opens the collapsed block a link points at, such as a source's notes on the roster pages,
// which build-site.cs collapses. Without it, a link to a closed block scrolls to its summary
// and leaves the notes hidden.
function openLinkedDetails() {
  const id = decodeURIComponent(location.hash.slice(1));
  const target = id && document.getElementById(id);
  if (!target) {
    return;
  }

  for (let details = target.closest("details"); details; details = details.parentElement?.closest("details")) {
    details.open = true;
  }

  target.scrollIntoView();
}

addEventListener("DOMContentLoaded", openLinkedDetails);
addEventListener("hashchange", openLinkedDetails);
