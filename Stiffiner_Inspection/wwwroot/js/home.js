"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/homeHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

$(function () {
    var timeouts = [null, null, null, null];
    var deepcores = [null, null, null, null];
    var triggercams = [null, null, null, null];
    var clientConnects = [null, null, null, null];
    var previousTray = [];
    var resetPLC = 1;

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

    const SYSTEM_STATUS_CLIENT = Object.freeze({
        'RUNNING': 1,
        'PAUSE': 2,
        'ERROR': 3,
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

        previousTray.push(data);

        appendResultLog(data);
    });

    connection.on("ReceiveTimeLog", (time, type, message) => {
        const noDataTimeLogRow = $('.time-log-no-data');
        if (noDataTimeLogRow) {
            noDataTimeLogRow.remove();
        }

        $("#time-log table tbody").prepend(`
            <tr>
                <td class="max-w90">${convertDate(time)}</td>
                <td class="max-w105">${type}</td>
                <td>${message}</td>
            </tr>
        `);
    });

    //event check status camera pc
    connection.on("ChangeCAM", (client_id, status) => {
        clearTimeout(timeouts[client_id]);

        $(".dot-cam-" + client_id).removeClass('cam-is-active').css("background", status == 1 ? '#0ad90a' : '#b6b9b6');

        timeouts[client_id] = setTimeout(function () {
            $(".dot-cam-" + client_id).css("background", '#b6b9b6');
        }, 3500);
    });

    //event check status camera pc
    connection.on("deepcore", (client_id, status) => {
        clearTimeout(deepcores[client_id]);

        $(".dot-deep-core-" + client_id).css("background", status == 1 ? '#0ad90a' : '#b6b9b6');

        deepcores[client_id] = setTimeout(function () {
            $(".dot-deep-core-" + client_id).css("background", '#b6b9b6');
        }, 3500);
    });

    //event check status camera pc
    connection.on("ChangeClientConnect", (clientId) => {
        clearTimeout(clientConnects[clientId])
        $(".dot-connect-" + clientId).css("background", "#0ad90a")
        clientConnects[clientId] = setTimeout(function () {
            $(".dot-connect-" + clientId).css("background", '#b6b9b6')
        }, 3500);
    });

    //event change plc
    connection.on("ChangeStatusPLC", (status) => {
        console.log("status:" + status);
        let _status = $('#value-plc-status');
        let _message = $('#error-plc-status')
        
        status === STATUS_PLC.ALARM ? _message.removeClass('d-none') : _message.addClass('d-none');

        if (status === STATUS_PLC.DISCONNECTED) {
            _status.css("color", "#222222").css("background", "#E6E6E6").text("Disconnect");
            return;
        }

        if (status === STATUS_PLC.ALARM) {
            _status.css("color", "#3C3C3C").css("background", "#FFCA08").text("Alarm");
            return;
        }

        if (status === STATUS_PLC.EMG) {
            _status.css("color", "#E34440").css("background", "#FD53083D").text("EMG");
            return;
        }

        if (status === STATUS_PLC.START) {
            _status.css("color", "#ffffff").css("background", "#49A31D").text("Start");
            return;
        }

        if (status === STATUS_PLC.STOP) {
            _status.css("color", "#ffffff").css("background", "#E4491D").text("Stop");
            return;
        }
    });

    //event change system client
    connection.on("ChangeStatusSystemClient", (status, message) => {
        let _status = $('#value-system-status');
        let _message = $('#error-system-status');

        status === SYSTEM_STATUS_CLIENT.ERROR ? _message.removeClass('d-none') : _message.addClass('d-none');

        if (status === SYSTEM_STATUS_CLIENT.RUNNING) {
            _status.css("color", "#ffffff").css("background", "#49A31D").text("Running");
            _message.addClass('d-none');
        }

        if (status === SYSTEM_STATUS_CLIENT.PAUSE) {
            _status.css("color", "#344054").css("background", "#F2F4F7").text("Pause");
            _message.addClass('d-none');
        }

        if (status === SYSTEM_STATUS_CLIENT.ERROR) {
            _status.css("color", "#E34440").css("background", "#FD53083D").text("Error");
            _message.removeClass('d-none').text(message);
        }
    });

    //event change trigger cam
    connection.on("ChangeStatusTriggerCam", (clientId) => {
        clearTimeout(triggercams[clientId]);
        $('.dot-trigger-cam-' + clientId).css("color", '#0ad90a');
        triggercams[clientId] = setTimeout(() => {
            $('.dot-trigger-cam-' + clientId).css("color", '#b6b9b6');
        }, 3500)
    });

    //plc reset
    connection.on("PLCReset", async (value) => {
        if (value == 1 && resetPLC == 1) {
            let number = $('.lbl-number').html();
            number = parseInt(number) + 1;
            $('.lbl-number').html(number);

            resetPLC++;
            resetCurrentTray();
            appendPreviousTray();
        } else {
            resetPLC = 1;
        }
    });

    function appendPreviousTray() {

        let client1 = "";
        let client2 = "";
        let client3 = "";
        let client4 = "";

        if (previousTray.length == 80) {
            previousTray.forEach(item => {
                if (item.client_id == 1) {
                    client1 += `<span class="${(item.result == 1) ? 'ok' : (item.result == 2) ? 'ng' : (item.result == 3) ? 'wait' : ''}">
                ${(item.result == 1) ? 'OK' : (item.result == 2) ? 'NG' : (item.result == 3) ? 'Wait' : ''}
            </span>`;
                    return;
                }

                if (item.client_id == 2) {
                    client2 += `<span class="${(item.result == 1) ? 'ok' : (item.result == 2) ? 'ng' : (item.result == 3) ? 'wait' : ''}">
                ${(item.result == 1) ? 'OK' : (item.result == 2) ? 'NG' : (item.result == 3) ? 'Wait' : ''}
            </span>`;
                    return;
                }

                if (item.client_id == 3) {
                    client3 += `<span class="${(item.result == 1) ? 'ok' : (item.result == 2) ? 'ng' : (item.result == 3) ? 'wait' : ''}">
                ${(item.result == 1) ? 'OK' : (item.result == 2) ? 'NG' : (item.result == 3) ? 'Wait' : ''}
            </span>`;
                    return;
                }

                if (item.client_id == 4) {
                    client4 += `<span class="${(item.result == 1) ? 'ok' : (item.result == 2) ? 'ng' : (item.result == 3) ? 'wait' : ''}">
                ${(item.result == 1) ? 'OK' : (item.result == 2) ? 'NG' : (item.result == 3) ? 'Wait' : ''}
            </span>`;
                    return;
                }
            });
        } else {
            for (let i = 1; i <= 80; i++) {
                if (i <= 20) {
                    client1 += `<span class="wait">Wait</span>`;
                    return;
                }

                if (i > 20 && i <= 40) {
                    client2 += `<span class="wait">Wait</span>`;
                    return;
                }

                if (i > 40 && i <= 60) {
                    client3 += `<span class="wait">Wait</span>`;
                    return;
                }

                if (i > 60 && i <= 80) {
                    client4 += `<span class="wait">Wait</span>`;
                    return;
                }
            }
        }

        $('#result .previous-tray .checking-tray-left .ng-ok .left-tray').html(client1)
        $('#result .previous-tray .checking-tray-left .ng-ok .right-tray').html(client2)
        $('#result .previous-tray .checking-tray-right .ng-ok .left-tray').html(client3)
        $('#result .previous-tray .checking-tray-right .ng-ok .right-tray').html(client4)

        previousTray = [];
    }

    function resetCurrentTray() {
        let client1 = '';
        let client2 = '';
        let client3 = '';
        let client4 = '';

        for (let i = 1; i <= 20; i++) {
            client1 += `<span class="ok left-area-${i}">Wait</span>`;
            client2 += `<span class="ng left-line-${i}">Wait</span>`;
            client3 += `<span class="ok right-area-${i}">Wait</span>`;
            client4 += `<span class="ng right-line-${i}">Wait</span>`;
        }

        $('#result .current-tray .checking-tray-left .ng-ok .left-tray').empty().append(client1)
        $('#result .current-tray .checking-tray-left .ng-ok .right-tray').empty().append(client2)
        $('#result .current-tray .checking-tray-right .ng-ok .left-tray').empty().append(client3)
        $('#result .current-tray .checking-tray-right .ng-ok .right-tray').empty().append(client4)
    }

    function appendResultLog(data) {
        $("#result-log table tbody").prepend(`
            <tr>
                <td>${convertDate(data.time)}</td >
                <td class="text-capitalize">${data.model}</td>
                <td>${data.tray}</td>
                <td class="text-capitalize">${data.side}</td>
                <td>${data.index}</td>
                <td class="text-capitalize">${data.camera}</td>
                <td class="status-item ${data.result === 1 ? "text-success" : "text-danger"}">
                    ${data.result == 1 ? "OK" : (data.result == 2 ? "NG" : (data.result == 3 ? "EMPTY" : "")) }
                </td>
                <td class="detail-error">${data.errorDetection ?? "-"}</td>
            </tr>
        `);

        if (data.result === STATUS_RESULT.OK) {
            $(`.${data.side}-${data.camera}-${data.index}`).css("background", "#66b032").text("OK");
            return;
        }

        if (data.result === STATUS_RESULT.NG) {
            $(`.${data.side}-${data.camera}-${data.index}`).css("background", "#e4491d").text("NG");
            return;
        }

        if (data.result === STATUS_RESULT.EMPTY) {
            $(`.${data.side}-${data.camera}-${data.index}`).css("background", "#9F9F9F").text("Empty");
            return;
        }
    }

    function convertDate(date) {
        let hours = date.substr(11, 2);
        let minutes = date.substr(14, 2);
        let seconds = date.substr(17, 2);

        return hours + ":" + minutes + ":" + seconds;
    }
});