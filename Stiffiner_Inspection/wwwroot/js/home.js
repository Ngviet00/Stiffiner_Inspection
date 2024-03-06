"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/homeHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

$(function () {

    //start connection
    connection.start().then(() => {
        console.log("Connection established");

        connection.invoke("GetCurrentStatusPLC")
            .catch(function (err) {
                console.error(err.toString());
            });
    }).catch(err => console.error(err.toString()));

    connection.on("ReceiveData", (data) => {
        const noDataTimeLogRow = $('.time-log-no-data');

        const noDataResultLogRow = $('.result-log-no-data');

        if (noDataResultLogRow) {
            noDataResultLogRow.remove();
        }

        appendResultLog(data);
    });

    connection.on("SetCurrentStatusPLC", (value) => {
        if (value == 1) {
            $('#value-plc-status').css("color", "#027A48").css("background", "#b3ffd2");
        }
    });

    connection.on("ChangeStatusPLC", (value) => {
        
    });

    function appendResultLog(data) {
        console.log();
        let result = `
            <tr>
                <td>${convertDate(data.time)}</td >
                <td>${data.model}</td>
                <td>${data.tray}</td>
                <td>${data.side}</td>
                <td>${data.no}</td>
                <td>${data.camera}</td>
                <td class="status-item ${data.result == 'OK' ? "text-success" : "text-danger"}">
                    ${data.result}
                </td>
                <td class="detail-error">${data.errorDetection ?? "-"}</td>
            </tr>
        `;

        $("#result-log table tbody").prepend(result);
    }

    function appendTimeLog(data) {
        let a = "13:49:12";
        let b = "Program";
        let c = "Sent>>LOATARESULT32,16,43";

        let result = `
            <tr>
                <td class="max-w90">${a}</td>
                <td class="max-w105">${b}</td>
                <td>${c}</td>
            </tr>
        `;

        $("#time-log table tbody").prepend(result);
    }

    function convertDate(date) {
        let hours = date.substr(11, 2);
        let minutes = date.substr(14, 2);
        let seconds = date.substr(17, 2);

        return hours + ":" + minutes + ":" + seconds;
    }
});