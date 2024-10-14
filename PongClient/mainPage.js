import { startSignalRConnection, hostGame, joinGame, onPlayerJoined, onJoinedGame } from './signalr.js';

document.addEventListener("DOMContentLoaded", () => {
    const hostGameBtn = document.getElementById("hostGameBtn");
    const joinGameBtn = document.getElementById("joinGameBtn");
    const gameCodeDisplay = document.getElementById("gameCodeDisplay");
    const joinGameInputContainer = document.getElementById("joinGameInputContainer");
    const submitJoinGameBtn = document.getElementById("submitJoinGameBtn");
    const joinGameInput = document.getElementById("joinGameInput");

    startSignalRConnection().then((connectionId) => {

        // Host Game Logic
        hostGameBtn.addEventListener("click", function () {
            joinGameInputContainer.style.display = "none"; 
            gameCodeDisplay.style.display = "block";
            hostGame(connectionId).then((gameCode) => {
                gameCodeDisplay.innerText = `Game code: ${gameCode} \n Waiting for the joiner...`;
            }).catch(() => {
                gameCodeDisplay.innerText = "Failed to host game.";
            });
        });

        joinGameBtn.addEventListener("click", function () {
            joinGameInputContainer.style.display = "block";
            gameCodeDisplay.style.display = "none";
        });

        submitJoinGameBtn.addEventListener("click", function () {
            const gameCode = joinGameInput.value;
            if (gameCode) {
                joinGame(connectionId, gameCode).then(() => {
                    gameCodeDisplay.innerText = `Joined Game: ${gameCode}`;
                }).catch(() => {
                    gameCodeDisplay.innerText = "Failed to join game.";
                });
            } else {
                alert("Please enter a game code.");
            }
        });

        onPlayerJoined((opponentConnectionId) => {
            console.log("A player joined, connection ID: " + opponentConnectionId);
            window.location.href = `gamePage.html?role=host&connectionId=${connectionId}&opponentId=${opponentConnectionId}`;
        });

        onJoinedGame((hostConnectionId) => {
            console.log("Joined the game, host connection ID: " + hostConnectionId);
            window.location.href = `gamePage.html?role=player&connectionId=${connectionId}&hostId=${hostConnectionId}`;
        });

    });
});
