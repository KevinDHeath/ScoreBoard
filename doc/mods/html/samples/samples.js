var hands;  // Number of hands completed (var hands = 0;)
var winner; // Game completed (var winner = false;)

function initializePage()
{
  if( hands === null || typeof(hands) === "undefined" ) hands = 0; // Default is 1st hand
  showScoreCards();

  if( winner === null || typeof(winner) === "undefined" ) winner = false; // Default is active
  showInPlayButton()
}

function showScoreCards()
{
  var scoreCards = document.getElementById("scoreCards");
  var scoreCardLink = document.getElementById("scoreCardsLink");
  if( hands === 0 ) // Hide score cards
  {
     if(!scoreCards.classList.contains("is-hidden")) { scoreCards.classList.add("is-hidden"); }
     if(!scoreCardLink.classList.contains("is-hidden")) { scoreCardLink.classList.add("is-hidden"); }
  }
  else // Show score cards
  {
     if(scoreCards.classList.contains("is-hidden")) { scoreCards.classList.remove("is-hidden"); }
     if(scoreCardLink.classList.contains("is-hidden")) { scoreCardLink.classList.remove("is-hidden"); }
  }
}

function showInPlayButton()
{
  var showInPlay = document.getElementById("showInPlay");
  if( showInPlay === null || typeof(showInPlay) === "undefined") return;
  if( winner === true ) // Hide in play button
  {
     if( !showInPlay.classList.contains("is-hidden")) { showInPlay.classList.add("is-hidden"); }
     $("#inHand").toggleClass("is-hidden");     // Show in-hand
     $("#inHandLink").toggleClass("is-hidden"); // Show quick link
  }
  else // Show in play button
  {
     if( showInPlay.classList.contains("is-hidden")) { showInPlay.classList.remove("is-hidden"); }
  }
}

function toggleInPlay()
{
  $("#inHand").toggleClass("is-hidden");
  $("#inHandLink").toggleClass("is-hidden");

  var inPlay = document.getElementById("inPlay"); // Set button text
  if( inPlay === null || typeof(inPlay) === "undefined") return;
  var inHand = document.getElementById("inHand");
  if( inHand.classList.contains("is-hidden") ) { inPlay.innerHTML = "Show in hand"; } else { inPlay.innerHTML = "Hide in hand"; }
}