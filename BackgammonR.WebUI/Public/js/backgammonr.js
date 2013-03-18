var backgammonr = window.backgammonr = function () {

    //--------------------------------------
    // Constants
    //--------------------------------------
    var BLACK = 1;
    var WHITE = 2;
    var NUMBER_OF_PLAYERS = 2;
    var NUMBER_OF_COUNTERS = 15;
    var NUMBER_OF_POINTS = 26;  // points + bar + off the board
    var COUNTER_WIDTH = 42;
    var COUNTER_SPACING_X = 8;
    var BAR_WIDTH = 56;
    var BOARD_WIDTH = 720;
    var BOARD_HEIGHT = 634;
    var BOARD_BORDER_X = 40;
    var BOARD_BORDER_Y = 30;

    //--------------------------------------
    // Globals
    //--------------------------------------
    var blackCounters;
    var whiteCounters;

    //--------------------------------------
    // Initialisation
    //--------------------------------------
    function init() {
        initPersistentConnection();
        initDataBinding();
        initDisplay();
    }

    //--------------------------------------
    // Data binding (knockout)
    //--------------------------------------
    var viewModel = {
        name: ko.observable(""),
        hasJoined: ko.observable(false),
        players: ko.observableArray([]),
        games: ko.observableArray([]),
        selectedGame: ko.observable(),
    }

    function initDataBinding() {
        ko.applyBindings(viewModel);
    }

    function getPlayer(name) {
        for (var i = 0; i < viewModel.players().length; i++) {
            if (viewModel.players()[i].name == name) {
                return viewModel.players()[i];
            }
        }
    }

    function addPlayer(playerDTO) {
        var player = {
            name: playerDTO.Name,
            status: ko.observable(playerDTO.StatusLabel)
        };

        player.canChallenge = ko.computed(function () {
            return viewModel.hasJoined() &&
                this.name != viewModel.name() &&
                this.status() == "Ready to play";
        }, player);

        viewModel.players.push(player);
    }

    function removePlayer(name) {
        viewModel.players.remove(function (player) { return player.name == name });
    }

    function updatePlayerStatus(name, status) {
        var player = getPlayer(name);
        player.status(status);
    }

    function addGame(gameDTO) {
        var game = {
            id: gameDTO.Id,
            black: gameDTO.Black.Name,
            white: gameDTO.White.Name,
            description: gameDTO.Black.Name + " v " + gameDTO.White.Name,
            board: gameDTO.Board,
        };

        viewModel.games.push(game);
    }

    function getGame(id) {
        for (var i = 0; i < viewModel.games().length; i++) {
            if (viewModel.games()[i].id == id) {
                return viewModel.games()[i];
            }
        }
    }

    function selectGame(game) {
        viewModel.selectedGame(game);
    }

    function updateGameBoard(board) {
        viewModel.selectedGame().board = board;
    }
    
    //--------------------------------------
    // Persistent connection (SignalR)
    //--------------------------------------
    function initPersistentConnection() {
  
        var hub = $.connection.gameNotificationHub;

        hub.client.loadPlayers = function (players) {
            for (var i = 0; i < players.length; i++) {
                addPlayer(players[i]);
            }
        };

        hub.client.loadGames = function (games) {
            for (var i = 0; i < games.length; i++) {
                addGame(games[i]);
            }
        };

        hub.client.joined = function (player, connectionId) {
            addPlayer(player);
            if ($.connection.hub.id != connectionId) {
                notify(player.Name + " has joined.", "message", true);
            }
        };

        hub.client.callerJoined = function (name) {            
            viewModel.name(name);
            viewModel.hasJoined(true);
        };

        hub.client.left = function (name, connectionId) {
            removePlayer(name);
            if ($.connection.hub.id != connectionId) {
                notify(player.Name + " has left.", "message", true);
            }
        };

        hub.client.callerLeft = function () {
            viewModel.name("");
            viewModel.hasJoined(false);
        };

        hub.client.challengeMade = function (challengerName, challengedName) {
            updatePlayerStatus(challengerName, "Challenging");
            updatePlayerStatus(challengedName, "Challenged");
            if (viewModel.name() == challengedName) {
                notify("<span>" + challengerName + " has challenged you to a game.  Accept? " + 
                    "<a href='' class='challenge-response' data-challenger-name='" + challengerName + "' data-response='accept'>Yes</a> | " +
                    "<a href='' class='challenge-response' data-challenger-name='" + challengerName + "' data-response='reject'>No</a></span>", "message", false);
            }
        };

        hub.client.challengeRespondedTo = function (challengerName, challengedName, accept, gameDTO) {
            if (accept) {
                updatePlayerStatus(challengerName, "Playing");
                updatePlayerStatus(challengedName, "Playing");
                addGame(gameDTO);

                var game = getGame(gameDTO.Id);
                selectGame(game);
                updateGameBoardDisplay(game);
            } else {
                updatePlayerStatus(challengerName, "Ready to play");
                updatePlayerStatus(challengedName, "Ready to play");
            }
            if (viewModel.name() == challengerName) {
                notify(challengedName + " has " + (accept ? "accepted" : "rejected" ) + " your challenge.", "message", true);
            }
        }

        hub.client.moved = function (gameDTO) {
            updateGameBoard(gameDTO.Board);
            updateGameBoardDisplay(viewModel.selectedGame());
            notify((playerNumber == 1 ? "Black" : "White") + " has moved.", "message", true);
        };

        hub.client.displayError = function (text) {
            notify(text, "error", true);
        };

        $.connection.hub.start().done(function () {

            hub.server.getPlayers();
            hub.server.getGames();

            $("#join-button").on("click", function () {
                hub.server.join($("#join-name").val());
            });

            $("#leave-button").on("click", function () {
                hub.server.leave(viewModel.name());
            });

            $(document).on("click", "#player-list a.challenge", function () {
                var challengedPlayer = $(this).prevAll("span.player-name").text();
                hub.server.challenge(challengedPlayer);
                return false;
            });

            $(document).on("click", "a.challenge-response", function () {                
                var challengingPlayer = $(this).attr("data-challenger-name");
                var accept = $(this).attr("data-response") == "accept";
                hub.server.respondToChallenge(challengingPlayer, accept);
                $(this).parent().delay(1000).fadeOut("slow");
                return false;
            });

            $(document).on("click", "#game-list a.view-game", function () {
                var gameId = $(this).attr("data-id");
                var game = getGame(gameId)
                selectGame(game);
                updateGameBoardDisplay(game);
                return false;
            });

            $(document).on("click", "#move", function () {
                var moveEntryValid,from1, to1, from2, to2;
                try {
                    from1 = parseInt($("#from1").val());
                    to1 = parseInt($("#to1").val());
                    from2 = parseInt($("#from2").val());
                    to2 = parseInt($("#to2").val());
                    moveEntryValid = true;
                } catch (e) {
                    notify("Invalid move entry", "error", true);
                    moveEntryValid = false;
                }

                if (moveEntryValid) {
                    hub.server.move(viewModel.selectedGame().id, from1, to1, from2, to2);
                }
            });

        });
    }

    //--------------------------------------
    // Notifications
    //--------------------------------------
    function notify(message, type, autoHide) {
        var e = $("#" + type + "-display");
        e.html(message).show();
        if (autoHide) {        
            e.delay(1000).fadeOut("slow");
        }
    }

    //--------------------------------------
    // Game canvas
    //--------------------------------------
    function initDisplay() {        
        blackCounters = getCounterArray("black");
        whiteCounters = getCounterArray("white")
    }

    function getContext() {
        var c = document.getElementById("canvas");
        return c.getContext("2d");
    }

    function drawBoardBackground(ctx) {
        var img = new Image();
        img.src = "/public/img/board.jpg";
        ctx.drawImage(img, 0, 0);
    }

    function updateGameBoardDisplay(game) {
        var c = document.getElementById("canvas");
        var ctx = getContext();
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        drawBoardBackground(ctx);
        drawCounters(ctx, game.board);
    }

    function drawCounters(ctx, board) {

        // Loop through players (i)
        for (var i = 0; i < NUMBER_OF_PLAYERS; i++) {
            var counterIndex = 0;
            var counters = i == 0 ? blackCounters : whiteCounters;
            // Loop through points (j)
            for (var j = 0; j < NUMBER_OF_POINTS; j++) {
                var x, y;
                if (j == 0) {
                    // Bar
                } else if (j == NUMBER_OF_POINTS - 1) {
                    // Off the board
                } else {
                    // Points
                    // Loop through counters on point
                    for (var k = 0; k < board[i][j]; k++) {                        
                        if (j <= NUMBER_OF_POINTS / 2) {
                            x = BOARD_WIDTH - BOARD_BORDER_X - ((COUNTER_WIDTH + COUNTER_SPACING_X) * (j - 1)) - COUNTER_WIDTH;
                            if (j >= NUMBER_OF_POINTS / 4) {
                                x -= BAR_WIDTH;
                            }
                            if (i + 1 == BLACK) {
                                y = BOARD_BORDER_Y + (k * COUNTER_WIDTH);
                            } else {
                                y = BOARD_HEIGHT - BOARD_BORDER_Y - (k * COUNTER_WIDTH) - COUNTER_WIDTH;
                            }
                        } else {
                            x = BOARD_BORDER_X + ((COUNTER_WIDTH + COUNTER_SPACING_X) * (j - (NUMBER_OF_POINTS / 2))) + COUNTER_WIDTH;
                            if (j < NUMBER_OF_POINTS / 4) {
                                x -= BAR_WIDTH;
                            }
                            if (i + 1 == BLACK) {
                                y = BOARD_HEIGHT - BOARD_BORDER_Y - (k * COUNTER_WIDTH) - COUNTER_WIDTH;
                            } else {
                                y = BOARD_BORDER_Y + (k * COUNTER_WIDTH);
                            }
                        }
                        ctx.drawImage(counters[counterIndex], x, y);
                        counterIndex++;                        
                    }
                }
            }
        }
    }

    function getCounterArray(color) {
        var counters = new Array();
        for (var i = 0; i < NUMBER_OF_COUNTERS; i++) {
            counters[i] = new Image();
            counters[i].src = "/public/img/" + color + ".png";
        }

        return counters;
    }

    return {
        "init": function () {
            $(document).ready(function () {
                init();
            });
        }
    }
}();
