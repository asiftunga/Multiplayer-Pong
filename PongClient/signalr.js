// signalr.js
let connection = null;

export function startSignalRConnection() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5292/ponggamehub")
        .build();

    return connection.start().then(() => {
        console.log("SignalR connection established.");
        return connection.connectionId;
    }).catch(err => console.error(err.toString()));
}

export function onPlayerJoined(callback) {
    connection.on("PlayerJoined", callback);
}

export function onJoinedGame(callback) {
    connection.on("JoinedGame", callback);
}

export function sendPaddlePosition(paddleY) {
    connection.invoke("UpdatePaddlePosition", paddleY).catch(err => console.error(err.toString()));
}

export function onReceiveOpponentPaddlePosition(callback) {
    connection.on("ReceiveOpponentPaddlePosition", callback);
}

export function hostGame(connectionId) {
    return axios.post('http://localhost:5292/api/Pong/HostGame', { connectionId })
        .then(response => response.data)
        .catch(error => {
            console.error("Error hosting game:", error);
            throw error;
        });
}

export function joinGame(connectionId, gameCode) {
    return axios.post("http://localhost:5292/api/Pong/JoinGame", { connectionId, gameCode })
        .then(response => response.data)
        .catch(error => {
            console.error("Error joining game:", error);
            throw error;
        });
}
