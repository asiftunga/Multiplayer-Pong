import * as SignalR from "@microsoft/signalr";

const connection = new SignalR.HubConnectionBuilder()
    .withUrl("https://localhost:44365/pong")
    .build();


const receiveClientCount = "ReceiveClientCount";


connection.start().then(() => {
    console.log("Connected to SignalR hub.");
    console.log(`connection id : ${connection.connectionId}`)

// Game UI setup
const canvas = document.getElementById("pong-game") as HTMLCanvasElement;
const ctx = canvas.getContext("2d")!;

// Game parameters
const paddleHeight = 100; 
const paddleWidth = 20;

// Game state
let hostPaddle: Paddle = { x: 0, y: 0 };
let opponentPaddle: Paddle = { x: canvas.width - paddleWidth, y: 0 };
let ball: Ball = { x: canvas.width / 2, y: canvas.height / 2, speedX: 5, speedY: 5 }; 

// Start Screen UI
const startScreen = document.getElementById("start-screen")!;
const playerCount = document.getElementById("activePlayerCount")!;
const hostButton = document.getElementById("host")!;
const joinButton = document.getElementById("join")!;
const gameCodeInput = document.getElementById("game-code-input") as HTMLInputElement;

hostButton.addEventListener("click", () => {
    connection.invoke("CreateGame")
        .then((gameCode: string) => {
            showGameCode(gameCode);
        });
});

connection.on(receiveClientCount, (count) => {
    playerCount.textContent = count;
})


joinButton.addEventListener("click", () => {
    const gameCode = gameCodeInput.value;
    connection.invoke("JoinGame", gameCode)
        .then(() => {
            startScreen.style.display = "none";
            canvas.style.display = "block"; 
        })
        .catch((error: string) => {
            if (error === "invalidGameCode") {
                // Show error message
            }
        });
});

// Game code display logic
function showGameCode(gameCode: string) {
    // ... (Display the game code to the user)
}

// Event listeners for game controls
document.addEventListener("keydown", (event) => {
    if (event.key === "w") {
        // Move paddle up
    } else if (event.key === "s") {
        // Move paddle down
    }

    connection.invoke("SendPlayerInput", event.key); // Send paddle movement to host
});

connection.on("playerJoined", (player: Player) => {
    if (player.isHost) {
         // You are the opponent, wait for game state updates
    } else {
         // You are the host, start the game
         startGame(); 
    } 
});

connection.on("gameStateUpdate", (ball: Ball, hostPaddle: Paddle, opponentPaddle: Paddle) => {
    // Update ball and paddle positions
    ball = ball;
    hostPaddle = hostPaddle;
    opponentPaddle = opponentPaddle;


    render();
});

function startGame() { 
    // ... (Start the game loop and send game state updates)
} 

function render() { 
    // ... (Draw the game objects on the canvas using ctx)
} 

}); 


interface Player {
    connectionId: string; 
    isHost: boolean;
    gameCode?: string;
    hostConnectionId?: string;
}

interface GameState { 
    ball: Ball;
    hostPaddle: Paddle;
    opponentPaddle: Paddle;
    hostScore?: number;  
    opponentScore?: number;
}

interface Ball {
    x: number;
    y: number;
    speedX: number;
    speedY: number;
  }
  
  interface Paddle {
    x: number;
    y: number;
  }
  