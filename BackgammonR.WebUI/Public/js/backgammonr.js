var backgammonr = window.backgammonr = function () {

    var BLACK = 1;
    var WHITE = 2;
    var NUMBER_OF_PLAYERS = 2;
    var NUMBER_OF_COUNTERS = 15;
    var NUMBER_OF_POINTS = 26;  // points + bar + off the board
    var COUNTER_WIDTH = 42;
    var COUNTER_SPACING_X = 13;
    var BOARD_WIDTH = 720;
    var BOARD_HEIGHT = 634;
    var BOARD_BORDER_X_LEFT = 18;
    var BOARD_BORDER_X_RIGHT = 40;
    var BOARD_BORDER_Y = 30;    

    //--------------------------------------
    // Initialisation
    //--------------------------------------
    function init() {
        initPersistentConnection();
        initDataBinding();
        initCanvas();
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

    function addPlayer(player) {
        var player = {
            name: player.Name,
            status: ko.observable(player.StatusLabel)
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

    function addGame(game) {
        var game = {
            id: game.Id,
            black: game.Black.Name,
            white: game.White.Name,
            description: game.Black.Name + " v " + game.White.Name,
            board: game.Board,
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

        hub.client.challengeRespondedTo = function (challengerName, challengedName, accept, game) {
            if (accept) {
                updatePlayerStatus(challengerName, "Playing");
                updatePlayerStatus(challengedName, "Playing");
                addGame(game);
                viewGame(game.Id);
            } else {
                updatePlayerStatus(challengerName, "Ready to play");
                updatePlayerStatus(challengedName, "Ready to play");
            }
            if (viewModel.name() == challengerName) {
                notify(challengedName + " has " + (accept ? "accepted" : "rejected" ) + " your challenge.", "message", true);
            }
        }

        hub.client.moved = function (playerNumber, from1, to1, from2, to2) {
            notify("Moved", message, true);
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
                viewGame(gameId);
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
                console.log(viewModel.selectedGame().Id);
                console.log(from1);
                console.log(to1);
                console.log(from2);
                console.log(to2);

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
    function initCanvas() {
        drawBoardBackground(getContext());
    }

    function getContext() {
        var c = document.getElementById("canvas");
        return c.getContext("2d");
    }

    function drawBoardBackground(ctx) {
        var img = new Image();
        img.src = "/public/img/board.jpg";
        img.onload = function () {
            ctx.drawImage(img, 0, 0);
        }            
    }

    function viewGame(id) {
        var game = getGame(id);

        viewModel.selectedGame(game);

        var c = document.getElementById("canvas");
        var ctx = c.getContext("2d");
        drawCounters(ctx, game.board);
    }

    function drawCounters(ctx, board) {

        var blackCounters = getCounterArray("black");
        var whiteCounters = getCounterArray("white");

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
                            x = BOARD_WIDTH - BOARD_BORDER_X_RIGHT - ((COUNTER_WIDTH + COUNTER_SPACING_X) * (j - 1)) - COUNTER_WIDTH;
                            if (i + 1 == BLACK) {
                                y = BOARD_BORDER_Y + (k * COUNTER_WIDTH);
                            } else {
                                y = BOARD_HEIGHT - BOARD_BORDER_Y - (k * COUNTER_WIDTH) - COUNTER_WIDTH;
                            }
                        } else {
                            x = BOARD_BORDER_X_LEFT + ((COUNTER_WIDTH + COUNTER_SPACING_X) * (j - (NUMBER_OF_POINTS / 2))) + COUNTER_WIDTH;
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
