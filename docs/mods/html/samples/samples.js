var hands;  // Number of hands completed (var hands = 0;)
var winner; // Game completed (var winner = false;)
const cHidden = "is-hidden"; const cShowInHand = "score.showInHand";

function initializePage() {
  if (hands === null || typeof (hands) === "undefined") hands = 0; // Default is 1st hand
  showScoreCards();

  if (winner === null || typeof (winner) === "undefined") winner = false; // Default is active
  showInPlayButton()
}

function showScoreCards() {
  let scoreCards = document.getElementById("scoreCards");
  let scoreCardLink = document.getElementById("scoreCardsLink");
  if (hands === 0) { // Hide score cards
    if (!scoreCards.classList.contains(cHidden)) { scoreCards.classList.add(cHidden); }
    if (!scoreCardLink.classList.contains(cHidden)) { scoreCardLink.classList.add(cHidden); }
  } else { // Show score cards
    if (scoreCards.classList.contains(cHidden)) { scoreCards.classList.remove(cHidden); }
    if (scoreCardLink.classList.contains(cHidden)) { scoreCardLink.classList.remove(cHidden); }
  }
}

function showInPlayButton() {
  let showInPlay = document.getElementById("showInPlay");
  if (showInPlay === null || typeof (showInPlay) === "undefined") return;
  if (winner === true) { // Hide in play button
    if (!showInPlay.classList.contains(cHidden)) { showInPlay.classList.add(cHidden); }
    $("#inHand").toggleClass(cHidden);     // Show in-hand
    $("#inHandLink").toggleClass(cHidden); // Show quick link
  }
  else { // Show in play button
    if (showInPlay.classList.contains(cHidden)) { showInPlay.classList.remove(cHidden); }
    try {
      if (!sessionStorage.getItem(cShowInHand)) {
        sessionStorage.setItem(cShowInHand, false);
      } else {
        let showInHand = sessionStorage.getItem(cShowInHand);
        if (showInHand && showInHand === "true") { toggleInPlay(); }
      }
    } catch { }
  }
}

function toggleInPlay() {
  $("#inHand").toggleClass(cHidden);
  $("#inHandLink").toggleClass(cHidden);

  let inPlay = document.getElementById("inPlay");
  if (inPlay === null || typeof (inPlay) === "undefined") return;
  let inHand = document.getElementById("inHand"); // Set button text
  if (inHand.classList.contains(cHidden)) {
    inPlay.innerHTML = "Show in hand";
    try { sessionStorage.setItem(cShowInHand, false); } catch { }
  } else {
    inPlay.innerHTML = "Hide in hand";
    try { sessionStorage.setItem(cShowInHand, true); } catch { }
  }
}