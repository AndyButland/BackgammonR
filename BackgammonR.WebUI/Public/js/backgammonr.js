var backgammonr = window.backgammonr = function () {

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
        players: ko.observableArray([]),
        name: ko.observable(""),
        hasJoined: ko.observable(false),
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

        hub.client.joined = function (player, connectionId) {
            addPlayer(player);
            if ($.connection.hub.id != connectionId) {
                $("#player-joind-message").text(player.Name + " has joined.").show().delay(1000).fadeOut("slow");
            }
        };

        hub.client.callerJoined = function (name) {            
            viewModel.name(name);
            viewModel.hasJoined(true);
        };

        hub.client.left = function (name, connectionId) {
            removePlayer(name);
            if ($.connection.hub.id != connectionId) {
                $("#player-joind-message").text(name + " has left.").show().delay(1000).fadeOut("slow");
            }
        };

        hub.client.callerLeft = function () {
            viewModel.name("");
            viewModel.hasJoined(false);
        };

        hub.client.challengeMade = function (challengerName, challengedName) {
            updatePlayerStatus(challengerName, "Challenging");
            updatePlayerStatus(challengedName, "Challenged");
        };

        hub.client.displayError = function (text) {
            $("#error-display").text(text).show().delay(1000).fadeOut("slow");
        };

        $.connection.hub.start().done(function () {

            hub.server.getPlayers();

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

        });
    }

    //--------------------------------------
    // Game canvas
    //--------------------------------------
    function initCanvas() {
        var c = document.getElementById("canvas");
        var ctx = c.getContext("2d");
        var img = new Image();
        img.src = "/public/img/board.jpg";
        img.onload = function () {
            ctx.drawImage(img, 0, 0);
        }        
    }

    return {
        "init": function () {
            $(document).ready(function () {
                init();
            });
        }
    }
}();
