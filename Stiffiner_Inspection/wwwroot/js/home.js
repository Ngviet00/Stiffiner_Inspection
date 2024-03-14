"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/homeHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

$(function () {
    
    var timeouts = [null, null, null, null];
    var triggercams = [null, null, null, null];
    var clientConnects = [null, null, null, null];
    var currentTray = [];

    const STATUS_PLC = Object.freeze({
        'EMG': 0,
        'START': 1,
        'STOP': 2,
        'ALARM': 3,
        'DISCONNECTED': 4
    });

    const STATUS_RESULT = Object.freeze({
        'OK': 1,
        'NG': 2,
        'EMPTY': 3,
    });

    //connection start
    connection.start()
        .then(() => {
            console.log('Connection established!')
        })
        .catch((err) => {
            console.error(err.toString())
        });

    //event receive data
    connection.on("ReceiveData", (data) => {
        const noDataResultLogRow = $('.result-log-no-data');
        if (noDataResultLogRow) {
            noDataResultLogRow.remove();
        }

        currentTray.push(data);

        appendResultLog(data);
    });

    connection.on("ReceiveTimeLog", (time, type, message) => {
        const noDataTimeLogRow = $('.time-log-no-data');
        if (noDataTimeLogRow) {
            noDataTimeLogRow.remove();
        }

        let result = `
            <tr>
                <td class="max-w90">05:37:37</td>
                <td class="max-w105">${type}</td>
                <td>${message}</td>
            </tr>
        `;

        $("#time-log table tbody").prepend(result);
    });

    //event check status camera pc
    connection.on("ChangeStatusCamClient", (clientId, status) => {
        console.log('status-cam-client:', clientId, status);

        clearTimeout(timeouts[clientId]);

        $(".dot-cam-" + clientId).css("background", status == 1 ? '#0ad90a' : '#b6b9b6');

        timeouts[clientId] = setTimeout(function () {
            $(".dot-cam-" + clientId).css("background", '#b6b9b6');
        }, 3500);
    });

    //event check status camera pc
    connection.on("ChangeClientConnect", (clientId, status) => {
        console.log('client-connect:', clientId, status);

        clearTimeout(clientConnects[clientId]);

        $(".dot-connect-" + clientId).css("background", status == 1 ? '#0ad90a' : '#b6b9b6');

        clientConnects[clientId] = setTimeout(function () {
            $(".dot-connect-" + clientId).css("background", '#b6b9b6');
        }, 3500);
    });

    //event change plc
    connection.on("ChangeStatusPLC", (status) => {

        let _status = $('#value-plc-status');
        let _message = $('#error-plc-status')
        
        status === STATUS_PLC.ALARM ? _message.removeClass('d-none') : _message.addClass('d-none');

        if (status === STATUS_PLC.DISCONNECTED) {
            _status.css("color", "#222222").css("background", "#E6E6E6").text("Disconnect");
        }

        if (status === STATUS_PLC.ALARM) {
            _status.css("color", "#3C3C3C").css("background", "#FFCA08").text("Alarm");
        }

        if (status === STATUS_PLC.EMG) {
            _status.css("color", "#E34440").css("background", "#FD53083D").text("EMG");
        }

        if (status === STATUS_PLC.START) {
            _status.css("color", "#ffffff").css("background", "#49A31D").text("Start");
        }

        if (status === STATUS_PLC.STOP) {
            _status.css("color", "#ffffff").css("background", "#E4491D").text("Stop");
        }
    });

    //event change system client
    connection.on("ChangeStatusSystemClient", (status, message) => {
        console.log(status, message);
        let _status = $('#value-system-status');
        let _message = $('#error-system-status');

        status === 3 ? _message.removeClass('d-none') : _message.addClass('d-none');

        if (status === 1) {
            _status.css("color", "#ffffff").css("background", "#49A31D").text("Running");
            _message.addClass('d-none');
        }

        if (status === 2) {
            _status.css("color", "#344054").css("background", "#F2F4F7").text("Pause");
            _message.addClass('d-none');
        }

        if (status === 3) {
            _status.css("color", "#E34440").css("background", "#FD53083D").text("Error");
            _message.removeClass('d-none').text(message);
        }
    });

    connection.on("ChangeStatusTriggerCam", (clientId) => {
        clearTimeout(triggercams[clientId]);
        $('.dot-trigger-cam-' + clientId).css("color", '#0ad90a');
        triggercams[clientId] = setTimeout(() => {
            $('.dot-trigger-cam-' + clientId).css("color", '#b6b9b6');
        }, 3500)
    });

    connection.on("PLCReset", () => {
        console.log('reset PLC');
    });

    function appendResultLog(data) {
        let result = `
            <tr>
                <td>${convertDate(data.time)}</td >
                <td class="text-capitalize">${data.model}</td>
                <td>${data.tray}</td>
                <td class="text-capitalize">${data.side}</td>
                <td>${data.index}</td>
                <td class="text-capitalize">${data.camera}</td>
                <td class="status-item ${data.result === 1 ? "text-success" : "text-danger"}">
                    ${data.result === 1 ? "OK" : "NG"}
                </td>
                <td class="detail-error">${data.errorDetection ?? "-"}</td>
            </tr>
        `;

        $("#result-log table tbody").prepend(result);

        if (data.result === STATUS_RESULT.OK) {
            $(`.${data.side}-${data.camera}-${data.index}`).css("background", "#66b032").text("OK");
        }

        if (data.result === STATUS_RESULT.NG) {
            $(`.${data.side}-${data.camera}-${data.index}`).css("background", "#e4491d").text("NG");
        }

        if (data.result === STATUS_RESULT.EMPTY) {
            $(`.${data.side}-${data.camera}-${data.index}`).css("background", "#9F9F9F").text("Empty");
        }
    }

    function convertDate(date) {
        let hours = date.substr(11, 2);
        let minutes = date.substr(14, 2);
        let seconds = date.substr(17, 2);

        return hours + ":" + minutes + ":" + seconds;
    }
});