import { startSignalRConnection, sendPaddlePosition, onReceiveOpponentPaddlePosition } from './signalr.js';

document.addEventListener("DOMContentLoaded", () => {
    const canvas = document.getElementById("gameCanvas");
    const ctx = canvas.getContext("2d");

    const paddleWidth = 10;
    const paddleHeight = 100;
    const paddleSpeed = 7;

    let leftPaddleY = (canvas.height - paddleHeight) / 2;
    let rightPaddleY = (canvas.height - paddleHeight) / 2;

    let ballX = canvas.width / 2;
    let ballY = canvas.height / 2;
    let ballRadius = 10;
    let ballSpeedX = 1;
    let ballSpeedY = 1;

    let leftPlayerScore = 0;
    let rightPlayerScore = 0;

    let upPressed = false;
    let downPressed = false;
    let wPressed = false;
    let sPressed = false;

    let countdown = 5;
    let countdownActive = false;
    let ballReset = false;

    const urlParams = new URLSearchParams(window.location.search);
    const playerRole = urlParams.get('role');

    // Draw functions
    function drawPaddle(x, y) {
        ctx.fillStyle = "white";
        ctx.fillRect(x, y, paddleWidth, paddleHeight);
    }

    function drawDashedLine() {
        ctx.setLineDash([10, 10]);
        ctx.beginPath();
        ctx.moveTo(canvas.width / 2, 0);
        ctx.lineTo(canvas.width / 2, canvas.height);
        ctx.strokeStyle = "white";
        ctx.stroke();
    }

    function drawBall() {
        ctx.beginPath();
        ctx.arc(ballX, ballY, ballRadius, 0, Math.PI * 2);
        ctx.fillStyle = "white";
        ctx.fill();
        ctx.closePath();
    }

    function drawScore() {
        ctx.font = "32px Arial";
        ctx.fillText(leftPlayerScore, canvas.width / 4, 40);
        ctx.fillText(rightPlayerScore, (canvas.width * 3) / 4, 40);
    }

    function draw() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        drawDashedLine();
        drawPaddle(10, leftPaddleY);
        drawPaddle(canvas.width - paddleWidth - 10, rightPaddleY);
        drawBall();
        drawScore();
        drawCountdown();
    }


    function updateBall() {
        if (!countdownActive && !ballReset) {
            ballX += ballSpeedX;
            ballY += ballSpeedY;
        }

        if (ballY + ballRadius >= canvas.height || ballY - ballRadius <= 0) {
            ballSpeedY = -ballSpeedY;
        }

        if (ballX - ballRadius < paddleWidth + 10 && ballY > leftPaddleY && ballY < leftPaddleY + paddleHeight) {
            ballSpeedX = -ballSpeedX;
        }

        if (ballX + ballRadius > canvas.width - paddleWidth - 10 && ballY > rightPaddleY && ballY < rightPaddleY + paddleHeight) {
            ballSpeedX = -ballSpeedX;
        }

        if (ballX - ballRadius < 0 && !ballReset) {
            rightPlayerScore++;
            startCountdown();
        }

        if (ballX + ballRadius > canvas.width && !ballReset) {
            leftPlayerScore++;
            startCountdown();
        }
    }

    function resetBall() {
        ballSpeedX = 1;
        ballSpeedY = 1;
        ballReset = false;
    }

    function startCountdown() {
        countdown = 5;
        countdownActive = true;
        ballX = canvas.width / 2;
        ballY = canvas.height / 2;
        ballReset = true;

        let countdownInterval = setInterval(() => {
            countdown--;
            if (countdown === 0) {
                clearInterval(countdownInterval);
                resetBall();
                countdownActive = false;
            }
        }, 1000);
    }

    function drawCountdown() {
        if (countdownActive) {
            ctx.font = "60px Arial";
            ctx.fillStyle = "white";
            ctx.fillText(countdown, canvas.width / 2 - 5, canvas.height / 2 - 40);
        }
    }

    function updatePaddles() {
        if (playerRole === 'host') {
            if (wPressed && leftPaddleY > 0) {
                leftPaddleY -= paddleSpeed;
            }
            if (sPressed && leftPaddleY < canvas.height - paddleHeight) {
                leftPaddleY += paddleSpeed;
            }
        } else if (playerRole === 'player') {
            if (upPressed && rightPaddleY > 0) {
                rightPaddleY -= paddleSpeed;
            }
            if (downPressed && rightPaddleY < canvas.height - paddleHeight) {
                rightPaddleY += paddleSpeed;
            }
        }
    }

    function updateOpponentPaddle(paddleY) {
        if (playerRole === 'host') {
            rightPaddleY = paddleY;
        } else {
            leftPaddleY = paddleY;
        }
    }

    function getPaddlePosition() {
        if (playerRole === 'host') {
            return leftPaddleY;
        } else {
            return rightPaddleY;
        }
    }

    function startGameLoop() {
        startSignalRConnection().then(() => {
            onReceiveOpponentPaddlePosition((opponentPaddleY) => {
                updateOpponentPaddle(opponentPaddleY);
            });

            function gameLoop() {
                draw();
                updateBall();
                updatePaddles();

                const paddleY = getPaddlePosition();
                sendPaddlePosition(paddleY);

                requestAnimationFrame(gameLoop);
            }
            gameLoop();
        });
    }

    document.addEventListener("keydown", (e) => {
        if (playerRole === 'host') {
            if (e.key === 'w' || e.key === 'W') {
                wPressed = true;
            }
            if (e.key === 's' || e.key === 'S') {
                sPressed = true;
            }
        } else if (playerRole === 'player') {
            if (e.key === 'ArrowUp') {
                upPressed = true;
            }
            if (e.key === 'ArrowDown') {
                downPressed = true;
            }
        }
    }, false);

    document.addEventListener("keyup", (e) => {
        if (playerRole === 'host') {
            if (e.key === 'w' || e.key === 'W') {
                wPressed = false;
            }
            if (e.key === 's' || e.key === 'S') {
                sPressed = false;
            }
        } else if (playerRole === 'player') {
            if (e.key === 'ArrowUp') {
                upPressed = false;
            }
            if (e.key === 'ArrowDown') {
                downPressed = false;
            }
        }
    }, false);

    startGameLoop();
});
