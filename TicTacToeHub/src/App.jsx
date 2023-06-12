import { useState, useEffect } from "react";
import "./App.css";
import * as signalR from "@microsoft/signalr";

const App = () => {
  const [board, setBoard] = useState(Array(9).fill(null));
  const [buttonStates, setButtonStates] = useState(Array(9).fill(true));
  const [player, setPlayer] = useState("");
  const [connection, setConnection] = useState(null);
  const [gameResult, setGameResult] = useState("");

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5101/gameHub")
      .build();

    newConnection
        .start()
        .then(() => {
            console.log("Connection started!", newConnection.connectionId);
            setConnection(newConnection);

            newConnection.on("ReceivePlayer", (player) => {
                setPlayer(player);
                console.log("ReceivePlayer", player);
            });
        })
        .catch((err) => console.log("Error while establishing connection: " + err));

  }, []);

  useEffect(() => {
    if (connection) {
      connection.on("ReceiveMove", (index, newPlayer) => {
        const newBoard = [...board];
        newBoard[index] = newPlayer;
        setBoard(newBoard);
        setButtonStates(Array(9).fill(player == newPlayer ? true : false));
      });

      connection.on("GameResult", result => {
        setGameResult(result);
        setButtonStates(Array(9).fill(true));
      });

      connection.on("GameStart", () => {
        setButtonStates(Array(9).fill(false));
      });
    }
  }, [connection ,board]);

  const handleClick = async (index) => {
    if (!board[index] && connection) {
      await connection.send("MakeMove", index, player);
    }
    console.log(`handleClick ${index}`, connection.connectionId);
  };

  const renderSquare = (index) => {
    return (
      <button
        key={index}
        className="square"
        onClick={() => handleClick(index)}
        disabled={buttonStates[index]}
      >
        {board[index]}
      </button>
    );
  };

  return (
    <div className="App">
      <h1>Tic-Tac-Toe with React and SignalR</h1>
      <h2>Player: {player}</h2>
      <div className="board">
        {board.map((_, index) => (renderSquare(index)))}
      </div>
      <p>{gameResult}</p>
    </div>
  );
};

export default App;
